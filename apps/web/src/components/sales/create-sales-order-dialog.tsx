"use client";

import { useState } from "react";
import { useForm, useFieldArray, Controller } from "react-hook-form";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Plus, Trash2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { partnersApi } from "@/lib/partners-api";
import { productsApi } from "@/lib/catalog-api";
import { salesApi } from "@/lib/sales-api";
import { toastApiError } from "@/lib/api-errors";
import { PartnerType } from "@/lib/types";
import { formatMoney } from "@/lib/utils";

interface LineField {
  productId: string;
  quantity: number;
}

interface FormValues {
  customerId: string;
  orderDate: string;
  notes: string;
  lines: LineField[];
}

const today = () => new Date().toISOString().slice(0, 10);

export function CreateSalesOrderDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const customersQuery = useQuery({
    queryKey: ["partners", PartnerType.Customer],
    queryFn: () => partnersApi.list({ type: PartnerType.Customer, pageSize: 100 }),
    enabled: open,
  });
  const productsQuery = useQuery({
    queryKey: ["products", {}],
    queryFn: () => productsApi.list({ pageSize: 100 }),
    enabled: open,
  });

  const activeCustomers = customersQuery.data?.items.filter((c) => c.isActive) ?? [];
  const activeProducts = productsQuery.data?.items.filter((p) => p.isActive) ?? [];

  const {
    register,
    handleSubmit,
    control,
    reset,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    defaultValues: { customerId: "", orderDate: today(), notes: "", lines: [{ productId: "", quantity: 1 }] },
  });

  const { fields, append, remove } = useFieldArray({ control, name: "lines" });
  const watchedLines = watch("lines");

  const priceOf = (productId: string) => activeProducts.find((p) => p.id === productId)?.salePrice ?? 0;
  const total = watchedLines.reduce(
    (sum, line) => sum + priceOf(line.productId) * (Number(line.quantity) || 0),
    0,
  );

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      salesApi.create({
        customerId: values.customerId,
        orderDate: values.orderDate,
        notes: values.notes || null,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["sales-orders"] });
      toast.success("Pedido creado como borrador.");
      setOpen(false);
    },
    onError: (error) => {
      toastApiError(error, "No se pudo crear el pedido.");
    },
  });

  const onSubmit = (values: FormValues) => {
    if (!values.customerId) {
      toast.error("Selecciona un cliente.");
      return;
    }
    if (values.lines.some((l) => !l.productId)) {
      toast.error("Cada línea debe tener un producto.");
      return;
    }
    mutation.mutate(values);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ customerId: "", orderDate: today(), notes: "", lines: [{ productId: "", quantity: 1 }] });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nueva venta</Button>} />
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Nueva venta</DialogTitle>
          <DialogDescription>
            Se crea como borrador. El stock se descuenta al confirmar el pedido.
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-1.5">
              <Label>Cliente</Label>
              <Controller
                control={control}
                name="customerId"
                render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Selecciona un cliente" />
                    </SelectTrigger>
                    <SelectContent>
                      {activeCustomers.map((c) => (
                        <SelectItem key={c.id} value={c.id}>{c.name}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="orderDate">Fecha</Label>
              <Input id="orderDate" type="date" {...register("orderDate")} />
            </div>
          </div>

          <div className="grid gap-2">
            <Label>Productos</Label>
            {fields.map((field, index) => {
              const line = watchedLines[index];
              const subtotal = priceOf(line?.productId ?? "") * (Number(line?.quantity) || 0);
              return (
                <div key={field.id} className="flex items-end gap-2">
                  <div className="grid flex-1 gap-1">
                    <Controller
                      control={control}
                      name={`lines.${index}.productId`}
                      render={({ field: f }) => (
                        <Select value={f.value} onValueChange={f.onChange}>
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Producto" />
                          </SelectTrigger>
                          <SelectContent>
                            {activeProducts.map((p) => (
                              <SelectItem key={p.id} value={p.id}>
                                {p.name} · {formatMoney(p.salePrice)} · stock {p.quantityOnHand}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      )}
                    />
                  </div>
                  <div className="grid w-20 gap-1">
                    <Input
                      type="number"
                      min={1}
                      step="any"
                      aria-label="Cantidad"
                      {...register(`lines.${index}.quantity`, { valueAsNumber: true })}
                    />
                  </div>
                  <div className="text-muted-foreground w-24 pb-2 text-right text-sm tabular-nums">
                    {formatMoney(subtotal)}
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon-sm"
                    className="mb-1"
                    disabled={fields.length === 1}
                    onClick={() => remove(index)}
                  >
                    <Trash2 />
                  </Button>
                </div>
              );
            })}
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="justify-self-start"
              onClick={() => append({ productId: "", quantity: 1 })}
            >
              <Plus />Agregar producto
            </Button>
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="notes">Notas (opcional)</Label>
            <Input id="notes" {...register("notes")} />
          </div>

          <div className="flex items-center justify-between border-t pt-3">
            <span className="text-sm font-medium">Total</span>
            <span className="text-lg font-semibold tabular-nums">{formatMoney(total)}</span>
          </div>
          {errors.orderDate && <p className="text-destructive text-xs">La fecha es obligatoria.</p>}

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Creando…" : "Crear borrador"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

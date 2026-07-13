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
import { purchasingApi } from "@/lib/purchasing-api";
import { ApiError } from "@/lib/api-client";
import { PartnerType } from "@/lib/types";
import { formatMoney } from "@/lib/utils";

interface LineField {
  productId: string;
  quantity: number;
}

interface FormValues {
  supplierId: string;
  orderDate: string;
  notes: string;
  lines: LineField[];
}

const today = () => new Date().toISOString().slice(0, 10);

export function CreatePurchaseOrderDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const suppliersQuery = useQuery({
    queryKey: ["partners", PartnerType.Supplier],
    queryFn: () => partnersApi.list(PartnerType.Supplier),
    enabled: open,
  });
  const productsQuery = useQuery({
    queryKey: ["products", {}],
    queryFn: () => productsApi.list(),
    enabled: open,
  });

  const activeSuppliers = suppliersQuery.data?.filter((s) => s.isActive) ?? [];
  const activeProducts = productsQuery.data?.filter((p) => p.isActive) ?? [];

  const {
    register,
    handleSubmit,
    control,
    reset,
    watch,
  } = useForm<FormValues>({
    defaultValues: { supplierId: "", orderDate: today(), notes: "", lines: [{ productId: "", quantity: 1 }] },
  });

  const { fields, append, remove } = useFieldArray({ control, name: "lines" });
  const watchedLines = watch("lines");

  const costOf = (productId: string) => activeProducts.find((p) => p.id === productId)?.costPrice ?? 0;
  const total = watchedLines.reduce(
    (sum, line) => sum + costOf(line.productId) * (Number(line.quantity) || 0),
    0,
  );

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      purchasingApi.create({
        supplierId: values.supplierId,
        orderDate: values.orderDate,
        notes: values.notes || null,
        lines: values.lines.map((l) => ({ productId: l.productId, quantity: Number(l.quantity) })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["purchase-orders"] });
      toast.success("Orden creada como borrador.");
      setOpen(false);
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors?.length) {
        error.problem.errors.forEach((e) => toast.error(e.error));
        return;
      }
      const message = error instanceof ApiError ? error.problem.title : "No se pudo crear la orden.";
      toast.error(message);
    },
  });

  const onSubmit = (values: FormValues) => {
    if (!values.supplierId) {
      toast.error("Selecciona un proveedor.");
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
        if (next) reset({ supplierId: "", orderDate: today(), notes: "", lines: [{ productId: "", quantity: 1 }] });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nueva compra</Button>} />
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Nueva compra</DialogTitle>
          <DialogDescription>
            Se crea como borrador. El stock ingresa al inventario al confirmar la orden.
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-1.5">
              <Label>Proveedor</Label>
              <Controller
                control={control}
                name="supplierId"
                render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Selecciona un proveedor" />
                    </SelectTrigger>
                    <SelectContent>
                      {activeSuppliers.map((s) => (
                        <SelectItem key={s.id} value={s.id}>{s.name}</SelectItem>
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
              const subtotal = costOf(line?.productId ?? "") * (Number(line?.quantity) || 0);
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
                                {p.name} · costo {formatMoney(p.costPrice)} · stock {p.quantityOnHand}
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

"use client";

import { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Plus } from "lucide-react";
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
import { salesApi } from "@/lib/sales-api";
import { invoicingApi } from "@/lib/invoicing-api";
import { ApiError } from "@/lib/api-client";
import { formatMoney } from "@/lib/utils";

interface FormValues {
  salesOrderId: string;
  dueDate: string;
  notes: string;
}

export function CreateInvoiceDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const ordersQuery = useQuery({
    queryKey: ["sales-orders", "Confirmed"],
    queryFn: () => salesApi.list({ status: "Confirmed", pageSize: 100 }),
    enabled: open,
    select: (data) => data.items,
  });

  const { register, handleSubmit, control, reset } = useForm<FormValues>({
    defaultValues: { salesOrderId: "", dueDate: "", notes: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      invoicingApi.create({
        salesOrderId: values.salesOrderId,
        dueDate: values.dueDate || null,
        notes: values.notes || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invoices"] });
      toast.success("Factura emitida.");
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo emitir la factura.";
      toast.error(message);
    },
  });

  const onSubmit = (values: FormValues) => {
    if (!values.salesOrderId) {
      toast.error("Selecciona un pedido de venta.");
      return;
    }
    mutation.mutate(values);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ salesOrderId: "", dueDate: "", notes: "" });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nueva factura</Button>} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Nueva factura</DialogTitle>
          <DialogDescription>
            Emite una factura a partir de un pedido de venta confirmado.
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid gap-1.5">
            <Label>Pedido de venta</Label>
            <Controller
              control={control}
              name="salesOrderId"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Selecciona un pedido confirmado" />
                  </SelectTrigger>
                  <SelectContent>
                    {ordersQuery.data?.map((o) => (
                      <SelectItem key={o.id} value={o.id}>
                        {o.orderNumber} · {o.customerName} · {formatMoney(o.totalAmount)}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {ordersQuery.data?.length === 0 && (
              <p className="text-muted-foreground text-xs">No hay pedidos confirmados para facturar.</p>
            )}
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="dueDate">Vence (opcional)</Label>
            <Input id="dueDate" type="date" {...register("dueDate")} />
            <p className="text-muted-foreground text-xs">Si se deja vacío, vence a 30 días.</p>
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="notes">Notas (opcional)</Label>
            <Input id="notes" {...register("notes")} />
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Emitiendo…" : "Emitir factura"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

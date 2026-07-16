"use client";

import { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
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
import { invoicingApi } from "@/lib/invoicing-api";
import { toastApiError } from "@/lib/api-errors";
import { PaymentMethod, type InvoiceSummary } from "@/lib/types";
import { formatMoney } from "@/lib/utils";

interface FormValues {
  amount: number;
  paidAt: string;
  method: string;
  reference: string;
}

const METHOD_OPTIONS = [
  { value: PaymentMethod.Cash, label: "Efectivo" },
  { value: PaymentMethod.Transfer, label: "Transferencia" },
  { value: PaymentMethod.Card, label: "Tarjeta" },
  { value: PaymentMethod.Other, label: "Otro" },
];

const today = () => new Date().toISOString().slice(0, 10);

export function RegisterPaymentDialog({ invoice }: { invoice: InvoiceSummary }) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const { register, handleSubmit, control, reset } = useForm<FormValues>({
    defaultValues: {
      amount: invoice.outstandingBalance,
      paidAt: today(),
      method: String(PaymentMethod.Cash),
      reference: "",
    },
  });

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      invoicingApi.registerPayment(invoice.id, {
        amount: Number(values.amount),
        paidAt: values.paidAt,
        method: Number(values.method),
        reference: values.reference || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invoices"] });
      // El cobro genera un movimiento de caja en el backend: sin esto, una
      // vista de Caja abierta muestra el saldo viejo hasta que expire el cache.
      queryClient.invalidateQueries({ queryKey: ["cash"] });
      toast.success("Pago registrado.");
      setOpen(false);
    },
    onError: (error) => {
      toastApiError(error, "No se pudo registrar el pago.");
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) {
          reset({
            amount: invoice.outstandingBalance,
            paidAt: today(),
            method: String(PaymentMethod.Cash),
            reference: "",
          });
        }
      }}
    >
      <DialogTrigger render={<Button size="sm">Registrar pago</Button>} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Registrar pago · {invoice.invoiceNumber}</DialogTitle>
          <DialogDescription>
            Saldo pendiente: {formatMoney(invoice.outstandingBalance)}.
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-1.5">
              <Label htmlFor="amount">Monto</Label>
              <Input id="amount" type="number" min={0} step="any" {...register("amount", { valueAsNumber: true })} />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="paidAt">Fecha</Label>
              <Input id="paidAt" type="date" {...register("paidAt")} />
            </div>
          </div>

          <div className="grid gap-1.5">
            <Label>Método</Label>
            <Controller
              control={control}
              name="method"
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {METHOD_OPTIONS.map((m) => (
                      <SelectItem key={m.value} value={String(m.value)}>{m.label}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="reference">Referencia (opcional)</Label>
            <Input id="reference" {...register("reference")} />
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Registrando…" : "Registrar pago"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

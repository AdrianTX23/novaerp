"use client";

import { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { useMutation, useQueryClient } from "@tanstack/react-query";
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
import { cashApi } from "@/lib/cash-api";
import { toastApiError } from "@/lib/api-errors";
import { CashMovementType } from "@/lib/types";

interface FormValues {
  type: string;
  amount: number;
  date: string;
  concept: string;
  description: string;
}

const today = () => new Date().toISOString().slice(0, 10);

export function CreateCashMovementDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const { register, handleSubmit, control, reset } = useForm<FormValues>({
    defaultValues: { type: String(CashMovementType.Expense), amount: 0, date: today(), concept: "", description: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      cashApi.create({
        type: Number(values.type),
        amount: Number(values.amount),
        date: values.date,
        concept: values.concept,
        description: values.description || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cash"] });
      toast.success("Movimiento registrado.");
      setOpen(false);
    },
    onError: (error) => {
      toastApiError(error, "No se pudo registrar el movimiento.");
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ type: String(CashMovementType.Expense), amount: 0, date: today(), concept: "", description: "" });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nuevo movimiento</Button>} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Nuevo movimiento de caja</DialogTitle>
          <DialogDescription>
            Registra un ingreso o egreso manual (renta, aportes, gastos varios).
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="grid gap-1.5">
              <Label>Tipo</Label>
              <Controller
                control={control}
                name="type"
                render={({ field }) => (
                  <Select value={field.value} onValueChange={field.onChange}>
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={String(CashMovementType.Income)}>Ingreso</SelectItem>
                      <SelectItem value={String(CashMovementType.Expense)}>Egreso</SelectItem>
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="amount">Monto</Label>
              <Input id="amount" type="number" min={0} step="any" {...register("amount", { valueAsNumber: true })} />
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div className="grid gap-1.5">
              <Label htmlFor="date">Fecha</Label>
              <Input id="date" type="date" {...register("date")} />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="concept">Concepto</Label>
              <Input id="concept" placeholder="Renta, sueldos…" {...register("concept")} />
            </div>
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="description">Descripción (opcional)</Label>
            <Input id="description" {...register("description")} />
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Guardando…" : "Registrar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

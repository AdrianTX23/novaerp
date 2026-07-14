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
import { accountingApi } from "@/lib/accounting-api";
import { ApiError } from "@/lib/api-client";
import { formatMoney, cn } from "@/lib/utils";

interface LineField {
  accountId: string;
  debit: number;
  credit: number;
}

interface FormValues {
  date: string;
  description: string;
  reference: string;
  lines: LineField[];
}

const today = () => new Date().toISOString().slice(0, 10);
const emptyLines = (): LineField[] => [
  { accountId: "", debit: 0, credit: 0 },
  { accountId: "", debit: 0, credit: 0 },
];

export function CreateJournalEntryDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const accountsQuery = useQuery({
    queryKey: ["accounting", "accounts"],
    queryFn: accountingApi.accounts,
    enabled: open,
  });
  const accounts = accountsQuery.data ?? [];

  const { register, handleSubmit, control, reset, watch } = useForm<FormValues>({
    defaultValues: { date: today(), description: "", reference: "", lines: emptyLines() },
  });
  const { fields, append, remove } = useFieldArray({ control, name: "lines" });
  const watched = watch("lines");

  const totalDebit = watched.reduce((s, l) => s + (Number(l.debit) || 0), 0);
  const totalCredit = watched.reduce((s, l) => s + (Number(l.credit) || 0), 0);
  const balanced = totalDebit > 0 && Math.abs(totalDebit - totalCredit) < 0.005;

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      accountingApi.createJournalEntry({
        date: values.date,
        description: values.description,
        reference: values.reference || null,
        lines: values.lines
          .filter((l) => l.accountId)
          .map((l) => ({ accountId: l.accountId, debit: Number(l.debit) || 0, credit: Number(l.credit) || 0 })),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      toast.success("Asiento registrado.");
      setOpen(false);
    },
    onError: (error) => {
      if (error instanceof ApiError && error.problem.errors?.length) {
        error.problem.errors.forEach((e) => toast.error(e.error));
        return;
      }
      const message = error instanceof ApiError ? error.problem.title : "No se pudo registrar el asiento.";
      toast.error(message);
    },
  });

  const onSubmit = (values: FormValues) => {
    if (values.lines.some((l) => !l.accountId)) {
      toast.error("Cada línea debe tener una cuenta.");
      return;
    }
    if (!balanced) {
      toast.error("El asiento no cuadra: el debe debe igualar al haber.");
      return;
    }
    mutation.mutate(values);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ date: today(), description: "", reference: "", lines: emptyLines() });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nuevo asiento</Button>} />
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Nuevo asiento contable</DialogTitle>
          <DialogDescription>Partida doble: la suma del debe debe igualar la del haber.</DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-1.5">
              <Label htmlFor="date">Fecha</Label>
              <Input id="date" type="date" {...register("date")} />
            </div>
            <div className="grid gap-1.5">
              <Label htmlFor="reference">Referencia (opcional)</Label>
              <Input id="reference" {...register("reference")} />
            </div>
          </div>

          <div className="grid gap-1.5">
            <Label htmlFor="description">Descripción</Label>
            <Input id="description" placeholder="Venta al contado" {...register("description")} />
          </div>

          <div className="grid gap-2">
            <div className="text-muted-foreground grid grid-cols-[1fr_7rem_7rem_2rem] gap-2 text-xs font-medium">
              <span>Cuenta</span>
              <span className="text-right">Debe</span>
              <span className="text-right">Haber</span>
              <span />
            </div>
            {fields.map((field, index) => (
              <div key={field.id} className="grid grid-cols-[1fr_7rem_7rem_2rem] items-center gap-2">
                <Controller
                  control={control}
                  name={`lines.${index}.accountId`}
                  render={({ field: f }) => (
                    <Select value={f.value} onValueChange={f.onChange}>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Cuenta" />
                      </SelectTrigger>
                      <SelectContent>
                        {accounts.map((a) => (
                          <SelectItem key={a.id} value={a.id}>{a.code} · {a.name}</SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  )}
                />
                <Input type="number" min={0} step="any" className="text-right" aria-label="Debe"
                  {...register(`lines.${index}.debit`, { valueAsNumber: true })} />
                <Input type="number" min={0} step="any" className="text-right" aria-label="Haber"
                  {...register(`lines.${index}.credit`, { valueAsNumber: true })} />
                <Button type="button" variant="ghost" size="icon-sm" disabled={fields.length <= 2}
                  onClick={() => remove(index)} aria-label="Quitar línea">
                  <Trash2 />
                </Button>
              </div>
            ))}
            <Button type="button" variant="outline" size="sm" className="justify-self-start"
              onClick={() => append({ accountId: "", debit: 0, credit: 0 })}>
              <Plus />Agregar línea
            </Button>
          </div>

          <div className="grid grid-cols-[1fr_7rem_7rem_2rem] gap-2 border-t pt-3 text-sm font-medium">
            <span className={cn(balanced ? "text-emerald-600 dark:text-emerald-500" : "text-muted-foreground")}>
              {balanced ? "El asiento cuadra ✓" : "Debe = Haber para cuadrar"}
            </span>
            <span className="text-right tabular-nums">{formatMoney(totalDebit)}</span>
            <span className="text-right tabular-nums">{formatMoney(totalCredit)}</span>
            <span />
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending || !balanced}>
              {mutation.isPending ? "Registrando…" : "Registrar asiento"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

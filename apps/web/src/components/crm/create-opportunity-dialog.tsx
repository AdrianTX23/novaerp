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
import { FormField } from "@/components/auth/form-field";
import { partnersApi } from "@/lib/partners-api";
import { crmApi } from "@/lib/crm-api";
import { toastApiError } from "@/lib/api-errors";
import { PartnerType } from "@/lib/types";

interface FormValues {
  customerId: string;
  title: string;
  estimatedValue: number;
  expectedCloseDate: string;
  notes: string;
}

export function CreateOpportunityDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const customersQuery = useQuery({
    queryKey: ["partners", PartnerType.Customer],
    queryFn: () => partnersApi.list({ type: PartnerType.Customer, pageSize: 100 }),
    enabled: open,
  });
  const activeCustomers = customersQuery.data?.items.filter((c) => c.isActive) ?? [];

  const { register, handleSubmit, control, reset } = useForm<FormValues>({
    defaultValues: { customerId: "", title: "", estimatedValue: 0, expectedCloseDate: "", notes: "" },
  });

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      crmApi.create({
        customerId: values.customerId,
        title: values.title,
        estimatedValue: Number(values.estimatedValue),
        expectedCloseDate: values.expectedCloseDate || null,
        notes: values.notes || null,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["crm"] });
      toast.success("Oportunidad creada.");
      setOpen(false);
    },
    onError: (error) => {
      toastApiError(error, "No se pudo crear la oportunidad.");
    },
  });

  const onSubmit = (values: FormValues) => {
    if (!values.customerId) {
      toast.error("Selecciona un cliente.");
      return;
    }
    mutation.mutate(values);
  };

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ customerId: "", title: "", estimatedValue: 0, expectedCloseDate: "", notes: "" });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nueva oportunidad</Button>} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Nueva oportunidad</DialogTitle>
          <DialogDescription>Un negocio potencial con un cliente. Empieza en la etapa &quot;Nuevo&quot;.</DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit(onSubmit)}>
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

          <FormField id="title" label="Título" placeholder="Implementación ERP" {...register("title")} />

          <div className="grid gap-4 sm:grid-cols-2">
            <FormField
              id="estimatedValue"
              label="Valor estimado"
              type="number"
              {...register("estimatedValue", { valueAsNumber: true })}
            />
            <div className="grid gap-1.5">
              <Label htmlFor="expectedCloseDate">Cierre esperado</Label>
              <Input id="expectedCloseDate" type="date" {...register("expectedCloseDate")} />
            </div>
          </div>

          <FormField id="notes" label="Notas (opcional)" {...register("notes")} />

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Creando…" : "Crear oportunidad"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

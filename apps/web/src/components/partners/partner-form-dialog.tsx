"use client";

import { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
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
import { Checkbox } from "@/components/ui/checkbox";
import { FormField } from "@/components/auth/form-field";
import { partnerFormSchema, type PartnerFormValues } from "@/lib/partners-schemas";
import { partnersApi } from "@/lib/partners-api";
import { ApiError } from "@/lib/api-client";
import { PartnerType, type PartnerDto } from "@/lib/types";
import { Plus, Pencil } from "lucide-react";

export function PartnerFormDialog({
  partner,
  defaultType = PartnerType.Customer,
}: {
  partner?: PartnerDto;
  defaultType?: number;
}) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();
  const isEdit = !!partner;

  const defaultValues: PartnerFormValues = {
    name: partner?.name ?? "",
    isCustomer: partner ? (partner.type & PartnerType.Customer) !== 0 : (defaultType & PartnerType.Customer) !== 0,
    isSupplier: partner ? (partner.type & PartnerType.Supplier) !== 0 : (defaultType & PartnerType.Supplier) !== 0,
    documentNumber: partner?.documentNumber ?? "",
    email: partner?.email ?? "",
    phone: partner?.phone ?? "",
    address: partner?.address ?? "",
  };

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<PartnerFormValues>({ resolver: zodResolver(partnerFormSchema), defaultValues });

  const mutation = useMutation({
    mutationFn: (values: PartnerFormValues) => {
      const type = (values.isCustomer ? PartnerType.Customer : 0) | (values.isSupplier ? PartnerType.Supplier : 0);
      const payload = {
        name: values.name,
        type,
        documentNumber: values.documentNumber || null,
        email: values.email || null,
        phone: values.phone || null,
        address: values.address || null,
      };
      return isEdit ? partnersApi.update(partner.id, payload) : partnersApi.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["partners"] });
      toast.success(isEdit ? "Contacto actualizado." : "Contacto creado.");
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo guardar el contacto.";
      toast.error(message);
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset(defaultValues);
      }}
    >
      <DialogTrigger
        render={
          isEdit ? (
            <Button variant="ghost" size="icon-sm"><Pencil /></Button>
          ) : (
            <Button><Plus />Nuevo contacto</Button>
          )
        }
      />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? `Editar ${partner.name}` : "Nuevo contacto"}</DialogTitle>
          <DialogDescription>Un contacto puede ser cliente, proveedor o ambos.</DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit((values) => mutation.mutate(values))}>
          <FormField id="name" label="Nombre / Razón social" error={errors.name?.message} {...register("name")} />

          <div className="grid gap-1.5">
            <span className="text-sm font-medium">Tipo</span>
            <div className="flex gap-4">
              <label className="flex items-center gap-2 text-sm">
                <Controller
                  control={control}
                  name="isCustomer"
                  render={({ field }) => (
                    <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
                Cliente
              </label>
              <label className="flex items-center gap-2 text-sm">
                <Controller
                  control={control}
                  name="isSupplier"
                  render={({ field }) => (
                    <Checkbox checked={field.value} onCheckedChange={field.onChange} />
                  )}
                />
                Proveedor
              </label>
            </div>
            {errors.isCustomer && <p className="text-destructive text-xs">{errors.isCustomer.message}</p>}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField
              id="documentNumber"
              label="Documento fiscal (opcional)"
              error={errors.documentNumber?.message}
              {...register("documentNumber")}
            />
            <FormField
              id="email"
              label="Email (opcional)"
              type="email"
              error={errors.email?.message}
              {...register("email")}
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <FormField
              id="phone"
              label="Teléfono (opcional)"
              error={errors.phone?.message}
              {...register("phone")}
            />
            <FormField
              id="address"
              label="Dirección (opcional)"
              error={errors.address?.message}
              {...register("address")}
            />
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Guardando…" : "Guardar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

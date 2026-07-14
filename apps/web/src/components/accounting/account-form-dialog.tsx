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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { FormField } from "@/components/auth/form-field";
import { Label } from "@/components/ui/label";
import { accountingApi } from "@/lib/accounting-api";
import { ApiError } from "@/lib/api-client";
import { AccountType } from "@/lib/types";
import { ACCOUNT_TYPE_LABEL } from "@/components/accounting/account-type-label";

interface FormValues {
  code: string;
  name: string;
  type: string;
}

export function AccountFormDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const { register, handleSubmit, control, reset } = useForm<FormValues>({
    defaultValues: { code: "", name: "", type: String(AccountType.Asset) },
  });

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      accountingApi.createAccount({ code: values.code, name: values.name, type: Number(values.type) }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["accounting"] });
      toast.success("Cuenta creada.");
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo crear la cuenta.";
      toast.error(message);
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ code: "", name: "", type: String(AccountType.Asset) });
      }}
    >
      <DialogTrigger render={<Button><Plus />Nueva cuenta</Button>} />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Nueva cuenta</DialogTitle>
          <DialogDescription>Agrega una cuenta al plan contable.</DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <div className="grid grid-cols-3 gap-4">
            <FormField id="code" label="Código" placeholder="1500" {...register("code")} />
            <div className="col-span-2">
              <FormField id="name" label="Nombre" placeholder="Equipo de cómputo" {...register("name")} />
            </div>
          </div>

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
                    {Object.entries(AccountType).map(([key, value]) => (
                      <SelectItem key={key} value={String(value)}>
                        {ACCOUNT_TYPE_LABEL[key as keyof typeof AccountType]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Guardando…" : "Crear cuenta"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

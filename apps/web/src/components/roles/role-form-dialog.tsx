"use client";

import { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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
import { roleFormSchema, type RoleFormValues } from "@/lib/users-schemas";
import { rolesApi } from "@/lib/roles-api";
import { ApiError } from "@/lib/api-client";
import type { RoleDetail } from "@/lib/types";
import { Plus, Pencil } from "lucide-react";

interface RoleFormDialogProps {
  /** Si se pasa, el diálogo edita ese rol; si no, crea uno nuevo. */
  role?: RoleDetail;
}

export function RoleFormDialog({ role }: RoleFormDialogProps) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();
  const isEdit = !!role;

  const permissionsQuery = useQuery({
    queryKey: ["permissions"],
    queryFn: rolesApi.listPermissions,
    enabled: open,
  });

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<RoleFormValues>({
    resolver: zodResolver(roleFormSchema),
    defaultValues: {
      name: role?.name ?? "",
      description: role?.description ?? "",
      permissionCodes: role?.permissionCodes ?? [],
    },
  });

  const mutation = useMutation({
    mutationFn: (values: RoleFormValues) =>
      isEdit
        ? rolesApi.update(role.id, { ...values, description: values.description || null })
        : rolesApi.create({ ...values, description: values.description || null }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["roles"] });
      toast.success(isEdit ? "Rol actualizado." : "Rol creado.");
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo guardar el rol.";
      toast.error(message);
    },
  });

  const groups = permissionsQuery.data
    ? Object.groupBy(permissionsQuery.data, (p) => p.group)
    : {};

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) {
          reset({
            name: role?.name ?? "",
            description: role?.description ?? "",
            permissionCodes: role?.permissionCodes ?? [],
          });
        }
      }}
    >
      <DialogTrigger
        render={
          isEdit ? (
            <Button variant="ghost" size="icon-sm"><Pencil /></Button>
          ) : (
            <Button><Plus />Nuevo rol</Button>
          )
        }
      />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? `Editar ${role.name}` : "Nuevo rol"}</DialogTitle>
          <DialogDescription>Define el nombre y qué puede hacer este rol.</DialogDescription>
        </DialogHeader>

        <form
          className="grid gap-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField id="name" label="Nombre" error={errors.name?.message} {...register("name")} />
          <FormField
            id="description"
            label="Descripción (opcional)"
            error={errors.description?.message}
            {...register("description")}
          />

          <div className="grid gap-3 max-h-64 overflow-y-auto pr-1">
            <Controller
              control={control}
              name="permissionCodes"
              render={({ field }) => (
                <>
                  {Object.entries(groups).map(([group, permissions]) => (
                    <div key={group} className="grid gap-1.5">
                      <span className="text-muted-foreground text-xs font-medium uppercase tracking-wide">
                        {group}
                      </span>
                      {permissions?.map((permission) => (
                        <label key={permission.code} className="flex items-center gap-2 text-sm">
                          <Checkbox
                            checked={field.value.includes(permission.code)}
                            onCheckedChange={(checked) => {
                              field.onChange(
                                checked
                                  ? [...field.value, permission.code]
                                  : field.value.filter((code) => code !== permission.code),
                              );
                            }}
                          />
                          {permission.description}
                        </label>
                      ))}
                    </div>
                  ))}
                </>
              )}
            />
            {errors.permissionCodes && (
              <p className="text-destructive text-xs">{errors.permissionCodes.message}</p>
            )}
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

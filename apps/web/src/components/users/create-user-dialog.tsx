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
import { createUserSchema, type CreateUserFormValues } from "@/lib/users-schemas";
import { usersApi } from "@/lib/users-api";
import { rolesApi } from "@/lib/roles-api";
import { ApiError } from "@/lib/api-client";
import { UserPlus } from "lucide-react";

export function CreateUserDialog() {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const rolesQuery = useQuery({ queryKey: ["roles"], queryFn: rolesApi.list, enabled: open });

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { roleIds: [] },
  });

  const mutation = useMutation({
    mutationFn: usersApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      queryClient.invalidateQueries({ queryKey: ["roles"] });
      toast.success("Usuario creado.");
      reset({ fullName: "", email: "", password: "", roleIds: [] });
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo crear el usuario.";
      toast.error(message);
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (!next) reset({ fullName: "", email: "", password: "", roleIds: [] });
      }}
    >
      <DialogTrigger render={<Button><UserPlus />Nuevo usuario</Button>} />
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Nuevo usuario</DialogTitle>
          <DialogDescription>Crea una cuenta para un miembro de tu equipo.</DialogDescription>
        </DialogHeader>

        <form
          className="grid gap-4"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            id="fullName"
            label="Nombre completo"
            error={errors.fullName?.message}
            {...register("fullName")}
          />
          <FormField
            id="email"
            label="Email"
            type="email"
            error={errors.email?.message}
            {...register("email")}
          />
          <FormField
            id="password"
            label="Contraseña temporal"
            type="password"
            error={errors.password?.message}
            {...register("password")}
          />

          <div className="grid gap-1.5">
            <span className="text-sm font-medium">Roles</span>
            <Controller
              control={control}
              name="roleIds"
              render={({ field }) => (
                <div className="grid gap-2">
                  {rolesQuery.data?.map((role) => (
                    <label key={role.id} className="flex items-center gap-2 text-sm">
                      <Checkbox
                        checked={field.value.includes(role.id)}
                        onCheckedChange={(checked) => {
                          field.onChange(
                            checked
                              ? [...field.value, role.id]
                              : field.value.filter((id) => id !== role.id),
                          );
                        }}
                      />
                      {role.name}
                    </label>
                  ))}
                </div>
              )}
            />
            {errors.roleIds && (
              <p className="text-destructive text-xs">{errors.roleIds.message}</p>
            )}
          </div>

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Creando…" : "Crear usuario"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

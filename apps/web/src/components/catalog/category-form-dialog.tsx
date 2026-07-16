"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
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
import { FormField } from "@/components/auth/form-field";
import { categoryFormSchema, type CategoryFormValues } from "@/lib/catalog-schemas";
import { categoriesApi } from "@/lib/catalog-api";
import { toastApiError } from "@/lib/api-errors";
import type { CategoryDto } from "@/lib/types";
import { Plus, Pencil } from "lucide-react";

export function CategoryFormDialog({ category }: { category?: CategoryDto }) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();
  const isEdit = !!category;

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CategoryFormValues>({
    resolver: zodResolver(categoryFormSchema),
    defaultValues: { name: category?.name ?? "", description: category?.description ?? "" },
  });

  const mutation = useMutation({
    mutationFn: (values: CategoryFormValues) => {
      const payload = { name: values.name, description: values.description || null };
      return isEdit ? categoriesApi.update(category.id, payload) : categoriesApi.create(payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      toast.success(isEdit ? "Categoría actualizada." : "Categoría creada.");
      setOpen(false);
    },
    onError: (error) => {
      toastApiError(error, "No se pudo guardar la categoría.");
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) reset({ name: category?.name ?? "", description: category?.description ?? "" });
      }}
    >
      <DialogTrigger
        render={
          isEdit ? (
            <Button variant="ghost" size="icon-sm" aria-label="Editar categoría"><Pencil /></Button>
          ) : (
            <Button><Plus />Nueva categoría</Button>
          )
        }
      />
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? `Editar ${category.name}` : "Nueva categoría"}</DialogTitle>
          <DialogDescription>Agrupa productos para organizarlos y filtrarlos.</DialogDescription>
        </DialogHeader>

        <form className="grid gap-4" onSubmit={handleSubmit((values) => mutation.mutate(values))}>
          <FormField id="name" label="Nombre" error={errors.name?.message} {...register("name")} />
          <FormField
            id="description"
            label="Descripción (opcional)"
            error={errors.description?.message}
            {...register("description")}
          />
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

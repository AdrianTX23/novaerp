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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { FormField } from "@/components/auth/form-field";
import { productFormSchema, type ProductFormInput, type ProductFormValues } from "@/lib/catalog-schemas";
import { productsApi, categoriesApi } from "@/lib/catalog-api";
import { ApiError } from "@/lib/api-client";
import type { ProductSummary } from "@/lib/types";
import { Plus, Pencil } from "lucide-react";

const NO_CATEGORY = "__none__";

export function ProductFormDialog({ product }: { product?: ProductSummary }) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();
  const isEdit = !!product;

  const categoriesQuery = useQuery({ queryKey: ["categories"], queryFn: categoriesApi.list, enabled: open });

  const defaultValues: ProductFormInput = {
    sku: product?.sku ?? "",
    name: product?.name ?? "",
    unitOfMeasure: product?.unitOfMeasure ?? "unidad",
    costPrice: product?.costPrice ?? 0,
    salePrice: product?.salePrice ?? 0,
    categoryId: product?.categoryId ?? "",
    description: product?.description ?? "",
    reorderPoint: product?.reorderPoint ?? "",
    initialQuantity: 0,
  };

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<ProductFormInput, unknown, ProductFormValues>({
    resolver: zodResolver(productFormSchema),
    defaultValues,
  });

  const mutation = useMutation({
    mutationFn: (values: ProductFormValues) => {
      const categoryId = values.categoryId || null;
      const reorderPoint = values.reorderPoint ?? null;

      if (isEdit) {
        return productsApi.update(product.id, {
          name: values.name,
          unitOfMeasure: values.unitOfMeasure,
          costPrice: values.costPrice,
          salePrice: values.salePrice,
          categoryId,
          description: values.description || null,
          reorderPoint,
        });
      }

      return productsApi.create({
        sku: values.sku,
        name: values.name,
        unitOfMeasure: values.unitOfMeasure,
        costPrice: values.costPrice,
        salePrice: values.salePrice,
        categoryId,
        description: values.description || null,
        reorderPoint,
        initialQuantity: values.initialQuantity,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["products"] });
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      toast.success(isEdit ? "Producto actualizado." : "Producto creado.");
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo guardar el producto.";
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
            <Button><Plus />Nuevo producto</Button>
          )
        }
      />
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? `Editar ${product.name}` : "Nuevo producto"}</DialogTitle>
          <DialogDescription>Define el producto y su precio.</DialogDescription>
        </DialogHeader>

        <form
          className="grid max-h-[70vh] gap-4 overflow-y-auto pr-1"
          onSubmit={handleSubmit((values) => mutation.mutate(values))}
        >
          <FormField
            id="sku"
            label="SKU"
            disabled={isEdit}
            error={errors.sku?.message}
            {...register("sku")}
          />
          <FormField id="name" label="Nombre" error={errors.name?.message} {...register("name")} />

          <div className="grid grid-cols-2 gap-4">
            <FormField
              id="unitOfMeasure"
              label="Unidad de medida"
              placeholder="unidad, kg, caja…"
              error={errors.unitOfMeasure?.message}
              {...register("unitOfMeasure")}
            />
            <div className="grid gap-1.5">
              <span className="text-sm font-medium">Categoría</span>
              <Controller
                control={control}
                name="categoryId"
                render={({ field }) => (
                  <Select
                    value={field.value || NO_CATEGORY}
                    onValueChange={(value) => field.onChange(value === NO_CATEGORY ? "" : value)}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Sin categoría">
                        {(value: string) =>
                          categoriesQuery.data?.find((c) => c.id === value)?.name ?? "Sin categoría"
                        }
                      </SelectValue>
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={NO_CATEGORY}>Sin categoría</SelectItem>
                      {categoriesQuery.data?.map((category) => (
                        <SelectItem key={category.id} value={category.id}>{category.name}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField
              id="costPrice"
              label="Costo"
              type="number"
              step="0.01"
              error={errors.costPrice?.message}
              {...register("costPrice")}
            />
            <FormField
              id="salePrice"
              label="Precio de venta"
              type="number"
              step="0.01"
              error={errors.salePrice?.message}
              {...register("salePrice")}
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField
              id="reorderPoint"
              label="Punto de reorden (opcional)"
              type="number"
              step="0.001"
              error={errors.reorderPoint?.message as string | undefined}
              {...register("reorderPoint")}
            />
            {!isEdit && (
              <FormField
                id="initialQuantity"
                label="Stock inicial"
                type="number"
                step="0.001"
                error={errors.initialQuantity?.message}
                {...register("initialQuantity")}
              />
            )}
          </div>

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

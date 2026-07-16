"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { categoriesApi } from "@/lib/catalog-api";
import { toastApiError } from "@/lib/api-errors";
import { ConfirmButton } from "@/components/ui/confirm-button";
import { Trash2 } from "lucide-react";

export function DeleteCategoryButton({ categoryId }: { categoryId: string }) {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => categoriesApi.delete(categoryId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      toast.success("Categoría eliminada.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo eliminar la categoría.");
    },
  });

  return (
    <ConfirmButton
      variant="ghost"
      size="icon-sm"
      confirmLabel="Sí, eliminar"
      pendingLabel="Eliminando…"
      pending={mutation.isPending}
      onConfirm={() => mutation.mutate()}
      aria-label="Eliminar categoría"
    >
      <Trash2 />
    </ConfirmButton>
  );
}

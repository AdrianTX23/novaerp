"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { categoriesApi } from "@/lib/catalog-api";
import { toastApiError } from "@/lib/api-errors";
import { Trash2 } from "lucide-react";

export function DeleteCategoryButton({ categoryId }: { categoryId: string }) {
  const [confirming, setConfirming] = useState(false);
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => categoriesApi.delete(categoryId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["categories"] });
      toast.success("Categoría eliminada.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo eliminar la categoría.");
      setConfirming(false);
    },
  });

  if (confirming) {
    return (
      <div className="flex items-center gap-1">
        <Button variant="destructive" size="sm" disabled={mutation.isPending} onClick={() => mutation.mutate()}>
          {mutation.isPending ? "Eliminando…" : "Confirmar"}
        </Button>
        <Button variant="ghost" size="sm" onClick={() => setConfirming(false)}>
          Cancelar
        </Button>
      </div>
    );
  }

  return (
    <Button variant="ghost" size="icon-sm" onClick={() => setConfirming(true)}>
      <Trash2 />
    </Button>
  );
}

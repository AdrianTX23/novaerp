"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { rolesApi } from "@/lib/roles-api";
import { toastApiError } from "@/lib/api-errors";
import { ConfirmButton } from "@/components/ui/confirm-button";
import { Trash2 } from "lucide-react";

export function DeleteRoleButton({ roleId }: { roleId: string }) {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => rolesApi.delete(roleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["roles"] });
      toast.success("Rol eliminado.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo eliminar el rol.");
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
      aria-label="Eliminar rol"
    >
      <Trash2 />
    </ConfirmButton>
  );
}

"use client";

import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { rolesApi } from "@/lib/roles-api";
import { ApiError } from "@/lib/api-client";
import { Trash2 } from "lucide-react";

export function DeleteRoleButton({ roleId }: { roleId: string }) {
  const [confirming, setConfirming] = useState(false);
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => rolesApi.delete(roleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["roles"] });
      toast.success("Rol eliminado.");
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo eliminar el rol.";
      toast.error(message);
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

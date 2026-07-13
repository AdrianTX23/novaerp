"use client";

import { useState } from "react";
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
import { usersApi } from "@/lib/users-api";
import { rolesApi } from "@/lib/roles-api";
import { ApiError } from "@/lib/api-client";
import type { UserSummary } from "@/lib/types";
import { Pencil } from "lucide-react";

export function EditUserRolesDialog({ user }: { user: UserSummary }) {
  const [open, setOpen] = useState(false);
  const [selected, setSelected] = useState<string[]>(user.roles.map((r) => r.id));
  const queryClient = useQueryClient();

  const rolesQuery = useQuery({ queryKey: ["roles"], queryFn: rolesApi.list, enabled: open });

  const mutation = useMutation({
    mutationFn: (roleIds: string[]) => usersApi.updateRoles(user.id, roleIds),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      queryClient.invalidateQueries({ queryKey: ["roles"] });
      toast.success("Roles actualizados.");
      setOpen(false);
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudieron actualizar los roles.";
      toast.error(message);
    },
  });

  return (
    <Dialog
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) setSelected(user.roles.map((r) => r.id));
      }}
    >
      <DialogTrigger render={<Button variant="ghost" size="icon-sm"><Pencil /></Button>} />
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Roles de {user.fullName}</DialogTitle>
          <DialogDescription>Elige qué roles tiene esta persona en la empresa.</DialogDescription>
        </DialogHeader>

        <div className="grid gap-2">
          {rolesQuery.data?.map((role) => (
            <label key={role.id} className="flex items-center gap-2 text-sm">
              <Checkbox
                checked={selected.includes(role.id)}
                onCheckedChange={(checked) => {
                  setSelected((prev) =>
                    checked ? [...prev, role.id] : prev.filter((id) => id !== role.id));
                }}
              />
              {role.name}
            </label>
          ))}
        </div>

        <DialogFooter>
          <Button
            disabled={mutation.isPending || selected.length === 0}
            onClick={() => mutation.mutate(selected)}
          >
            {mutation.isPending ? "Guardando…" : "Guardar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

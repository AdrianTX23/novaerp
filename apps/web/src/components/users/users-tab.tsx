"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Skeleton } from "@/components/ui/skeleton";
import { usersApi } from "@/lib/users-api";
import { ApiError } from "@/lib/api-client";
import { useAuthStore } from "@/stores/auth-store";
import { CreateUserDialog } from "@/components/users/create-user-dialog";
import { EditUserRolesDialog } from "@/components/users/edit-user-roles-dialog";

export function UsersTab() {
  const currentUserId = useAuthStore((s) => s.user?.id);
  const queryClient = useQueryClient();
  const usersQuery = useQuery({ queryKey: ["users"], queryFn: usersApi.list });

  const toggleActive = useMutation({
    mutationFn: (vars: { userId: string; isActive: boolean }) =>
      vars.isActive ? usersApi.reactivate(vars.userId) : usersApi.deactivate(vars.userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["users"] }),
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo actualizar el estado.";
      toast.error(message);
    },
  });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">
          {usersQuery.data?.length ?? 0} personas con acceso a esta empresa.
        </p>
        <CreateUserDialog />
      </div>

      {usersQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nombre</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Roles</TableHead>
              <TableHead>Activo</TableHead>
              <TableHead className="w-10" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {usersQuery.data?.map((user) => (
              <TableRow key={user.id}>
                <TableCell className="font-medium">{user.fullName}</TableCell>
                <TableCell className="text-muted-foreground">{user.email}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {user.roles.map((role) => (
                      <Badge key={role.id} variant="outline">{role.name}</Badge>
                    ))}
                  </div>
                </TableCell>
                <TableCell>
                  <Switch
                    checked={user.isActive}
                    disabled={toggleActive.isPending || user.id === currentUserId}
                    onCheckedChange={(checked) =>
                      toggleActive.mutate({ userId: user.id, isActive: checked })
                    }
                  />
                </TableCell>
                <TableCell>
                  <EditUserRolesDialog user={user} />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

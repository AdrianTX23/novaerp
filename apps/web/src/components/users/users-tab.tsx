"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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
import { toastApiError } from "@/lib/api-errors";
import { useAuthStore } from "@/stores/auth-store";
import { CreateUserDialog } from "@/components/users/create-user-dialog";
import { EditUserRolesDialog } from "@/components/users/edit-user-roles-dialog";
import { QueryError } from "@/components/layout/query-error";

export function UsersTab() {
  const currentUserId = useAuthStore((s) => s.user?.id);
  const queryClient = useQueryClient();
  const usersQuery = useQuery({ queryKey: ["users"], queryFn: usersApi.list });

  const toggleActive = useMutation({
    mutationFn: (vars: { userId: string; isActive: boolean }) =>
      vars.isActive ? usersApi.reactivate(vars.userId) : usersApi.deactivate(vars.userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["users"] }),
    onError: (error) => {
      toastApiError(error, "No se pudo actualizar el estado.");
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

      {usersQuery.isError ? (
        <QueryError error={usersQuery.error} forbiddenMessage="Tu rol no tiene acceso a la gestión de usuarios." />
      ) : usersQuery.isLoading ? (
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

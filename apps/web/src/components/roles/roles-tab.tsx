"use client";

import { useQuery } from "@tanstack/react-query";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { rolesApi } from "@/lib/roles-api";
import { RoleFormDialog } from "@/components/roles/role-form-dialog";
import { DeleteRoleButton } from "@/components/roles/delete-role-button";

export function RolesTab() {
  const rolesQuery = useQuery({ queryKey: ["roles"], queryFn: rolesApi.list });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">
          Los roles Owner, Admin y Member son del sistema y no se pueden editar.
        </p>
        <RoleFormDialog />
      </div>

      {rolesQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Rol</TableHead>
              <TableHead>Permisos</TableHead>
              <TableHead>Usuarios</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {rolesQuery.data?.map((role) => (
              <TableRow key={role.id}>
                <TableCell className="font-medium">
                  <div className="flex items-center gap-2">
                    {role.name}
                    {role.isSystem && <Badge variant="secondary">Sistema</Badge>}
                  </div>
                  {role.description && (
                    <p className="text-muted-foreground text-xs">{role.description}</p>
                  )}
                </TableCell>
                <TableCell className="text-muted-foreground">
                  {role.permissionCodes.length} permisos
                </TableCell>
                <TableCell className="text-muted-foreground">{role.userCount}</TableCell>
                <TableCell>
                  {!role.isSystem && (
                    <div className="flex items-center gap-1">
                      <RoleFormDialog role={role} />
                      <DeleteRoleButton roleId={role.id} />
                    </div>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

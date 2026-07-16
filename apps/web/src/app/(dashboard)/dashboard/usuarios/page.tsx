"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { UsersTab } from "@/components/users/users-tab";
import { RolesTab } from "@/components/roles/roles-tab";
import { Forbidden } from "@/components/layout/forbidden";
import { useAuthStore } from "@/stores/auth-store";

export default function UsuariosPage() {
  const canViewUsers = useAuthStore((s) => s.user?.permissions.includes("users.read") ?? false);
  const canManageRoles = useAuthStore((s) => s.user?.permissions.includes("roles.manage") ?? false);

  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Usuarios</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Gestiona quién tiene acceso a tu empresa y qué puede hacer cada quien.
      </p>

      {!canViewUsers ? (
        <div className="mt-6">
          <Forbidden message="Tu rol no tiene acceso a la gestión de usuarios." />
        </div>
      ) : (
        <Tabs defaultValue="usuarios" className="mt-6">
          <TabsList>
            <TabsTrigger value="usuarios">Usuarios</TabsTrigger>
            {canManageRoles && <TabsTrigger value="roles">Roles</TabsTrigger>}
          </TabsList>
          <TabsContent value="usuarios" className="mt-4">
            <UsersTab />
          </TabsContent>
          {canManageRoles && (
            <TabsContent value="roles" className="mt-4">
              <RolesTab />
            </TabsContent>
          )}
        </Tabs>
      )}
    </div>
  );
}

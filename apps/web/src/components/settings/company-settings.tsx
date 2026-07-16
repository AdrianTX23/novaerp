"use client";

import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { FormField } from "@/components/auth/form-field";
import { settingsApi } from "@/lib/settings-api";
import { toastApiError } from "@/lib/api-errors";
import { useAuthStore } from "@/stores/auth-store";

export function CompanySettings() {
  const queryClient = useQueryClient();
  const canManage = useAuthStore((s) => s.user?.permissions.includes("users.manage") ?? false);

  const companyQuery = useQuery({
    queryKey: ["settings", "company"],
    queryFn: settingsApi.company,
  });

  // null = sin editar; el input muestra el nombre actual del servidor hasta
  // que el usuario escribe. Evita sincronizar estado con un useEffect.
  const [nameInput, setNameInput] = useState<string | null>(null);
  const name = nameInput ?? companyQuery.data?.name ?? "";

  const rename = useMutation({
    mutationFn: (newName: string) => settingsApi.renameCompany(newName),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["settings", "company"] });
      setNameInput(null);
      toast.success("Nombre de la empresa actualizado.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo actualizar la empresa.");
    },
  });

  if (companyQuery.isPending) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-40" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>
        <CardContent className="space-y-3">
          <Skeleton className="h-9 w-full max-w-sm" />
          <Skeleton className="h-4 w-48" />
        </CardContent>
      </Card>
    );
  }

  if (companyQuery.isError || !companyQuery.data) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Empresa</CardTitle>
          <CardDescription>No se pudo cargar la información de la empresa.</CardDescription>
        </CardHeader>
      </Card>
    );
  }

  const company = companyQuery.data;
  const createdAt = new Date(company.createdAt).toLocaleDateString("es", {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
  const dirty = name.trim() !== company.name;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Empresa</CardTitle>
        <CardDescription>Datos generales de tu empresa en NovaERP.</CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        <form
          className="max-w-sm space-y-3"
          onSubmit={(e) => {
            e.preventDefault();
            if (!name.trim() || !dirty) return;
            rename.mutate(name.trim());
          }}
        >
          <FormField
            id="company-name"
            label="Nombre"
            value={name}
            onChange={(e) => setNameInput(e.target.value)}
            disabled={!canManage || rename.isPending}
          />
          {canManage ? (
            <Button type="submit" disabled={!dirty || !name.trim() || rename.isPending}>
              {rename.isPending ? "Guardando…" : "Guardar cambios"}
            </Button>
          ) : (
            <p className="text-muted-foreground text-xs">
              Solo un administrador puede cambiar el nombre de la empresa.
            </p>
          )}
        </form>

        <dl className="grid gap-3 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted-foreground">Identificador</dt>
            <dd className="font-mono">{company.slug}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Registrada el</dt>
            <dd>{createdAt}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">Estado</dt>
            <dd>
              <Badge variant={company.isActive ? "secondary" : "destructive"}>
                {company.isActive ? "Activa" : "Inactiva"}
              </Badge>
            </dd>
          </div>
        </dl>
      </CardContent>
    </Card>
  );
}

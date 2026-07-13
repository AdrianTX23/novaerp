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
import { partnersApi } from "@/lib/partners-api";
import { ApiError } from "@/lib/api-client";
import { PartnerType } from "@/lib/types";
import { PartnerFormDialog } from "@/components/partners/partner-form-dialog";

export function PartnersTab({ type }: { type: number }) {
  const queryClient = useQueryClient();
  const partnersQuery = useQuery({
    queryKey: ["partners", type],
    queryFn: () => partnersApi.list(type),
  });

  const toggleActive = useMutation({
    mutationFn: (vars: { partnerId: string; isActive: boolean }) =>
      vars.isActive ? partnersApi.reactivate(vars.partnerId) : partnersApi.deactivate(vars.partnerId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["partners"] }),
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo actualizar el estado.";
      toast.error(message);
    },
  });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{partnersQuery.data?.length ?? 0} contactos.</p>
        <PartnerFormDialog defaultType={type} />
      </div>

      {partnersQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nombre</TableHead>
              <TableHead>Contacto</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Activo</TableHead>
              <TableHead className="w-10" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {partnersQuery.data?.map((partner) => (
              <TableRow key={partner.id}>
                <TableCell className="font-medium">
                  {partner.name}
                  {partner.documentNumber && (
                    <p className="text-muted-foreground text-xs">{partner.documentNumber}</p>
                  )}
                </TableCell>
                <TableCell className="text-muted-foreground">
                  {partner.email ?? partner.phone ?? "—"}
                </TableCell>
                <TableCell>
                  <div className="flex gap-1">
                    {(partner.type & PartnerType.Customer) !== 0 && <Badge variant="outline">Cliente</Badge>}
                    {(partner.type & PartnerType.Supplier) !== 0 && <Badge variant="outline">Proveedor</Badge>}
                  </div>
                </TableCell>
                <TableCell>
                  <Switch
                    checked={partner.isActive}
                    disabled={toggleActive.isPending}
                    onCheckedChange={(checked) =>
                      toggleActive.mutate({ partnerId: partner.id, isActive: checked })
                    }
                  />
                </TableCell>
                <TableCell>
                  <PartnerFormDialog partner={partner} />
                </TableCell>
              </TableRow>
            ))}
            {partnersQuery.data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} className="text-muted-foreground text-center">
                  Todavía no hay contactos de este tipo.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

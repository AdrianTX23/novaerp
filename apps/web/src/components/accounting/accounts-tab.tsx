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
import { accountingApi } from "@/lib/accounting-api";
import { ACCOUNT_TYPE_LABEL } from "@/components/accounting/account-type-label";
import { AccountFormDialog } from "@/components/accounting/account-form-dialog";
import { QueryError } from "@/components/layout/query-error";

export function AccountsTab() {
  const accountsQuery = useQuery({ queryKey: ["accounting", "accounts"], queryFn: accountingApi.accounts });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{accountsQuery.data?.length ?? 0} cuentas.</p>
        <AccountFormDialog />
      </div>

      {accountsQuery.isError ? (
        <QueryError error={accountsQuery.error} forbiddenMessage="Tu rol no tiene acceso a la contabilidad." />
      ) : accountsQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-24">Código</TableHead>
              <TableHead>Nombre</TableHead>
              <TableHead>Tipo</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {accountsQuery.data?.map((a) => (
              <TableRow key={a.id}>
                <TableCell className="font-medium tabular-nums">{a.code}</TableCell>
                <TableCell>{a.name}</TableCell>
                <TableCell>
                  <Badge variant="outline">{ACCOUNT_TYPE_LABEL[a.type]}</Badge>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

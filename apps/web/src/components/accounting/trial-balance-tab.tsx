"use client";

import { useQuery } from "@tanstack/react-query";
import { CheckCircle2, AlertTriangle } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { accountingApi } from "@/lib/accounting-api";
import { formatMoney } from "@/lib/utils";
import { QueryError } from "@/components/layout/query-error";

export function TrialBalanceTab() {
  const balanceQuery = useQuery({
    queryKey: ["accounting", "trial-balance"],
    queryFn: accountingApi.trialBalance,
  });
  const data = balanceQuery.data;

  if (balanceQuery.isError) {
    return <QueryError error={balanceQuery.error} forbiddenMessage="Tu rol no tiene acceso a la contabilidad." />;
  }
  if (balanceQuery.isLoading) {
    return <Skeleton className="h-40 w-full" />;
  }

  return (
    <div className="grid gap-4">
      {data && (
        <div
          className={`flex w-fit items-center gap-2 rounded-lg border px-3 py-1.5 text-sm ${
            data.isBalanced
              ? "border-emerald-500/30 text-emerald-600 dark:text-emerald-500"
              : "border-amber-500/30 text-amber-600 dark:text-amber-500"
          }`}
        >
          {data.isBalanced ? <CheckCircle2 className="size-4" /> : <AlertTriangle className="size-4" />}
          {data.isBalanced ? "Los libros cuadran" : "Los libros no cuadran"}
        </div>
      )}

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="w-24">Código</TableHead>
            <TableHead>Cuenta</TableHead>
            <TableHead className="text-right">Debe</TableHead>
            <TableHead className="text-right">Haber</TableHead>
            <TableHead className="text-right">Saldo</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data?.rows.map((r) => (
            <TableRow key={r.accountId}>
              <TableCell className="font-medium tabular-nums">{r.code}</TableCell>
              <TableCell>{r.name}</TableCell>
              <TableCell className="text-right tabular-nums">{formatMoney(r.totalDebit)}</TableCell>
              <TableCell className="text-right tabular-nums">{formatMoney(r.totalCredit)}</TableCell>
              <TableCell className="text-right font-medium tabular-nums">{formatMoney(r.balance)}</TableCell>
            </TableRow>
          ))}
          {data?.rows.length === 0 && (
            <TableRow>
              <TableCell colSpan={5} className="text-muted-foreground text-center">
                No hay movimientos contables todavía.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
        {data && data.rows.length > 0 && (
          <TableFooter>
            <TableRow>
              <TableCell colSpan={2} className="font-medium">Totales</TableCell>
              <TableCell className="text-right font-semibold tabular-nums">{formatMoney(data.totalDebit)}</TableCell>
              <TableCell className="text-right font-semibold tabular-nums">{formatMoney(data.totalCredit)}</TableCell>
              <TableCell />
            </TableRow>
          </TableFooter>
        )}
      </Table>
    </div>
  );
}

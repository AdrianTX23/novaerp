"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { Wallet, ArrowDownLeft, ArrowUpRight, Trash2 } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { cashApi } from "@/lib/cash-api";
import { ApiError } from "@/lib/api-client";
import { formatMoney, cn } from "@/lib/utils";
import { CreateCashMovementDialog } from "@/components/cash/create-cash-movement-dialog";

export function CashView() {
  const queryClient = useQueryClient();
  const summaryQuery = useQuery({ queryKey: ["cash", "summary"], queryFn: cashApi.summary, retry: false });
  const movementsQuery = useQuery({ queryKey: ["cash", "movements"], queryFn: cashApi.movements, retry: false });

  const forbidden =
    (summaryQuery.error instanceof ApiError && summaryQuery.error.status === 403) ||
    (movementsQuery.error instanceof ApiError && movementsQuery.error.status === 403);

  const deleteMovement = useMutation({
    mutationFn: (id: string) => cashApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["cash"] });
      toast.success("Movimiento eliminado.");
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo eliminar el movimiento.";
      toast.error(message);
    },
  });

  if (forbidden) {
    return (
      <div className="text-muted-foreground rounded-xl border border-dashed p-8 text-center text-sm">
        Tu rol no tiene acceso a la caja.
      </div>
    );
  }

  const summary = summaryQuery.data;

  return (
    <div className="grid gap-6">
      <div className="grid gap-4 sm:grid-cols-3">
        <div className="bg-card rounded-xl border p-5">
          <div className="flex items-start justify-between">
            <span className="text-muted-foreground text-xs font-medium tracking-wide uppercase">Saldo actual</span>
            <span className="bg-muted text-muted-foreground flex size-8 items-center justify-center rounded-lg">
              <Wallet className="size-4" />
            </span>
          </div>
          <div className="mt-3 text-2xl font-semibold tabular-nums">
            {summary ? formatMoney(summary.balance) : <Skeleton className="h-8 w-28" />}
          </div>
        </div>
        <div className="bg-card rounded-xl border p-5">
          <div className="flex items-start justify-between">
            <span className="text-muted-foreground text-xs font-medium tracking-wide uppercase">Ingresos del mes</span>
            <span className="flex size-8 items-center justify-center rounded-lg bg-emerald-500/10 text-emerald-600 dark:text-emerald-500">
              <ArrowDownLeft className="size-4" />
            </span>
          </div>
          <div className="mt-3 text-2xl font-semibold tabular-nums text-emerald-600 dark:text-emerald-500">
            {summary ? formatMoney(summary.incomeThisMonth) : <Skeleton className="h-8 w-28" />}
          </div>
        </div>
        <div className="bg-card rounded-xl border p-5">
          <div className="flex items-start justify-between">
            <span className="text-muted-foreground text-xs font-medium tracking-wide uppercase">Egresos del mes</span>
            <span className="flex size-8 items-center justify-center rounded-lg bg-rose-500/10 text-rose-600 dark:text-rose-500">
              <ArrowUpRight className="size-4" />
            </span>
          </div>
          <div className="mt-3 text-2xl font-semibold tabular-nums text-rose-600 dark:text-rose-500">
            {summary ? formatMoney(summary.expenseThisMonth) : <Skeleton className="h-8 w-28" />}
          </div>
        </div>
      </div>

      <div className="grid gap-4">
        <div className="flex items-center justify-between">
          <h2 className="font-medium">Movimientos</h2>
          <CreateCashMovementDialog />
        </div>

        {movementsQuery.isLoading ? (
          <Skeleton className="h-40 w-full" />
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha</TableHead>
                <TableHead>Concepto</TableHead>
                <TableHead>Origen</TableHead>
                <TableHead className="text-right">Monto</TableHead>
                <TableHead className="w-10" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {movementsQuery.data?.map((m) => {
                const isIncome = m.kind === "Income";
                return (
                  <TableRow key={`${m.source}-${m.id}`}>
                    <TableCell className="text-muted-foreground">{m.date}</TableCell>
                    <TableCell className="font-medium">
                      {m.concept}
                      {m.description && <p className="text-muted-foreground text-xs">{m.description}</p>}
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">{m.source === "Invoice" ? "Factura" : "Manual"}</Badge>
                    </TableCell>
                    <TableCell
                      className={cn(
                        "text-right font-medium tabular-nums",
                        isIncome ? "text-emerald-600 dark:text-emerald-500" : "text-rose-600 dark:text-rose-500",
                      )}
                    >
                      {isIncome ? "+" : "−"}
                      {formatMoney(m.amount)}
                    </TableCell>
                    <TableCell>
                      {m.canDelete && (
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          disabled={deleteMovement.isPending}
                          onClick={() => deleteMovement.mutate(m.id)}
                          aria-label="Eliminar movimiento"
                        >
                          <Trash2 />
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })}
              {movementsQuery.data?.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="text-muted-foreground text-center">
                    Todavía no hay movimientos. Los cobros de facturas aparecen aquí automáticamente.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        )}
      </div>
    </div>
  );
}

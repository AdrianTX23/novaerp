"use client";

import { useQuery } from "@tanstack/react-query";
import { Landmark } from "lucide-react";
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
import { reportsApi } from "@/lib/reports-api";
import { formatMoney, cn } from "@/lib/utils";
import { KpiCard } from "@/components/dashboard/kpi-card";

const BUCKET_VARIANT: Record<string, "secondary" | "outline" | "destructive"> = {
  "Al día": "secondary",
  "1-30 días": "outline",
  "31-60 días": "destructive",
  "60+ días": "destructive",
};

export function ReceivablesReportTab() {
  const reportQuery = useQuery({ queryKey: ["reports", "receivables"], queryFn: reportsApi.receivables });
  const data = reportQuery.data;

  if (reportQuery.isLoading) {
    return <Skeleton className="h-64 w-full" />;
  }
  if (!data) return null;

  return (
    <div className="grid gap-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <KpiCard label="Total por cobrar" value={formatMoney(data.totalOutstanding)} icon={Landmark} />
        {data.buckets.map((b) => (
          <div key={b.bucket} className="bg-card rounded-xl border p-5">
            <span className="text-muted-foreground text-xs font-medium tracking-wide uppercase">{b.bucket}</span>
            <div
              className={cn(
                "mt-2 text-xl font-semibold tabular-nums",
                b.bucket !== "Al día" && b.total > 0 && "text-rose-600 dark:text-rose-500",
              )}
            >
              {formatMoney(b.total)}
            </div>
            <span className="text-muted-foreground text-xs">{b.count} facturas</span>
          </div>
        ))}
      </div>

      <div className="bg-card rounded-xl border p-5">
        <h2 className="mb-3 font-medium">Facturas pendientes</h2>
        {data.invoices.length === 0 ? (
          <p className="text-muted-foreground text-sm">No hay facturas pendientes de cobro.</p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Factura</TableHead>
                <TableHead>Cliente</TableHead>
                <TableHead>Vence</TableHead>
                <TableHead>Antigüedad</TableHead>
                <TableHead className="text-right">Saldo</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.invoices.map((r) => (
                <TableRow key={r.invoiceId}>
                  <TableCell className="font-medium">{r.invoiceNumber}</TableCell>
                  <TableCell>{r.customerName}</TableCell>
                  <TableCell className="text-muted-foreground">{r.dueDate}</TableCell>
                  <TableCell>
                    <Badge variant={BUCKET_VARIANT[r.bucket] ?? "outline"}>
                      {r.bucket}
                      {r.daysOverdue > 0 && ` · ${r.daysOverdue}d`}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right font-medium tabular-nums">
                    {formatMoney(r.outstandingBalance)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>
    </div>
  );
}

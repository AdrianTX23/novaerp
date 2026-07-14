"use client";

import { useQuery } from "@tanstack/react-query";
import { Boxes, Package, AlertTriangle } from "lucide-react";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { reportsApi } from "@/lib/reports-api";
import { formatMoney } from "@/lib/utils";
import { KpiCard } from "@/components/dashboard/kpi-card";

export function InventoryReportTab() {
  const reportQuery = useQuery({ queryKey: ["reports", "inventory"], queryFn: reportsApi.inventory });
  const data = reportQuery.data;

  if (reportQuery.isLoading) {
    return <Skeleton className="h-64 w-full" />;
  }
  if (!data) return null;

  return (
    <div className="grid gap-6">
      <div className="grid gap-4 sm:grid-cols-3">
        <KpiCard label="Valor de inventario" value={formatMoney(data.totalValue)} icon={Boxes} />
        <KpiCard label="Productos activos" value={String(data.totalProducts)} icon={Package} />
        <KpiCard
          label="En bajo stock"
          value={String(data.lowStock.length)}
          icon={AlertTriangle}
          tone={data.lowStock.length > 0 ? "warning" : "default"}
        />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <div className="bg-card rounded-xl border p-5">
          <h2 className="mb-3 font-medium">Valorización por categoría</h2>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Categoría</TableHead>
                <TableHead className="text-right">Productos</TableHead>
                <TableHead className="text-right">Valor</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.byCategory.map((c) => (
                <TableRow key={c.categoryName}>
                  <TableCell>{c.categoryName}</TableCell>
                  <TableCell className="text-right tabular-nums">{c.productCount}</TableCell>
                  <TableCell className="text-right font-medium tabular-nums">{formatMoney(c.value)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        <div className="bg-card rounded-xl border p-5">
          <h2 className="mb-3 font-medium">Bajo stock</h2>
          {data.lowStock.length === 0 ? (
            <p className="text-muted-foreground text-sm">Todo el inventario está por encima de su punto de reorden.</p>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>SKU</TableHead>
                  <TableHead>Producto</TableHead>
                  <TableHead className="text-right">Stock</TableHead>
                  <TableHead className="text-right">Reorden</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.lowStock.map((l) => (
                  <TableRow key={l.sku}>
                    <TableCell className="font-medium">{l.sku}</TableCell>
                    <TableCell>{l.name}</TableCell>
                    <TableCell className="text-right text-amber-600 tabular-nums dark:text-amber-500">
                      {l.quantityOnHand}
                    </TableCell>
                    <TableCell className="text-muted-foreground text-right tabular-nums">{l.reorderPoint}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </div>
      </div>
    </div>
  );
}

"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { TrendingUp, ShoppingCart, Calculator } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { reportsApi } from "@/lib/reports-api";
import { formatMoney } from "@/lib/utils";
import { KpiCard } from "@/components/dashboard/kpi-card";

const today = () => new Date().toISOString().slice(0, 10);
const firstOfMonth = () => {
  const d = new Date();
  return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10);
};

export function SalesReportTab() {
  const [from, setFrom] = useState(firstOfMonth());
  const [to, setTo] = useState(today());

  const reportQuery = useQuery({
    queryKey: ["reports", "sales", from, to],
    queryFn: () => reportsApi.sales(from, to),
  });
  const data = reportQuery.data;
  const max = Math.max(...(data?.dailyBreakdown.map((d) => d.total) ?? [1]), 1);

  return (
    <div className="grid gap-6">
      <div className="flex flex-wrap items-end gap-4">
        <div className="grid gap-1.5">
          <Label htmlFor="from">Desde</Label>
          <Input id="from" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="to">Hasta</Label>
          <Input id="to" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
        </div>
      </div>

      {reportQuery.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : data ? (
        <>
          <div className="grid gap-4 sm:grid-cols-3">
            <KpiCard label="Ventas del período" value={formatMoney(data.totalSales)} icon={TrendingUp} />
            <KpiCard label="Pedidos" value={String(data.orderCount)} icon={ShoppingCart} />
            <KpiCard label="Ticket promedio" value={formatMoney(data.averageOrderValue)} icon={Calculator} />
          </div>

          <div className="bg-card rounded-xl border p-5">
            <h2 className="mb-4 font-medium">Ventas por día</h2>
            {data.dailyBreakdown.length === 0 ? (
              <p className="text-muted-foreground text-sm">Sin ventas confirmadas en el período.</p>
            ) : (
              <div className="grid gap-2">
                {data.dailyBreakdown.map((d) => (
                  <div key={d.date} className="grid grid-cols-[6rem_1fr_7rem] items-center gap-3 text-sm">
                    <span className="text-muted-foreground tabular-nums">{d.date}</span>
                    <div className="bg-muted h-2 overflow-hidden rounded-full">
                      <div className="bg-primary/80 h-full rounded-full" style={{ width: `${(d.total / max) * 100}%` }} />
                    </div>
                    <span className="text-right font-medium tabular-nums">{formatMoney(d.total)}</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      ) : null}
    </div>
  );
}

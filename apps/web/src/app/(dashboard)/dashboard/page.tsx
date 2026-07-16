"use client";

import { useQuery } from "@tanstack/react-query";
import { TrendingUp, ShoppingCart, ShoppingBag, PackageX, Boxes, Trophy } from "lucide-react";
import { dashboardApi } from "@/lib/dashboard-api";
import { useAuthStore } from "@/stores/auth-store";
import { ApiError } from "@/lib/api-client";
import { Skeleton } from "@/components/ui/skeleton";
import { KpiCard } from "@/components/dashboard/kpi-card";
import { SalesTrendChart } from "@/components/dashboard/sales-trend-chart";
import { Forbidden } from "@/components/layout/forbidden";
import { formatMoney } from "@/lib/utils";

export default function DashboardPage() {
  const firstName = useAuthStore((s) => s.user?.fullName.split(" ")[0]);
  const { data, isLoading, error } = useQuery({
    queryKey: ["dashboard"],
    queryFn: dashboardApi.get,
    retry: false,
  });

  const forbidden = error instanceof ApiError && error.status === 403;

  return (
    <div className="grid gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Hola, {firstName}</h1>
        <p className="text-muted-foreground mt-1 text-sm">
          Un vistazo al estado de tu empresa este mes.
        </p>
      </div>

      {forbidden ? (
        <Forbidden message="Tu rol no tiene acceso a los indicadores del negocio." />
      ) : isLoading ? (
        <DashboardSkeleton />
      ) : data ? (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
            <KpiCard
              label="Ventas del mes"
              value={formatMoney(data.salesThisMonth)}
              hint={`${data.salesOrdersThisMonth} ${data.salesOrdersThisMonth === 1 ? "pedido" : "pedidos"}`}
              icon={TrendingUp}
            />
            <KpiCard label="Pedidos del mes" value={String(data.salesOrdersThisMonth)} icon={ShoppingCart} />
            <KpiCard label="Compras del mes" value={formatMoney(data.purchasesThisMonth)} icon={ShoppingBag} />
            <KpiCard
              label="Bajo stock"
              value={String(data.lowStockCount)}
              hint={data.lowStockCount > 0 ? "productos por reordenar" : "todo en orden"}
              icon={PackageX}
              tone={data.lowStockCount > 0 ? "warning" : "default"}
            />
            <KpiCard label="Valor de inventario" value={formatMoney(data.inventoryValue)} icon={Boxes} />
          </div>

          <div className="grid gap-4 lg:grid-cols-3">
            <div className="bg-card rounded-xl border p-5 lg:col-span-2">
              <div className="mb-4 flex items-center justify-between">
                <h2 className="font-medium">Ventas de los últimos 6 meses</h2>
                <TrendingUp className="text-muted-foreground size-4" />
              </div>
              <SalesTrendChart data={data.salesTrend} />
            </div>

            <div className="grid gap-4">
              <div className="bg-card rounded-xl border p-5">
                <div className="mb-3 flex items-center gap-2">
                  <Trophy className="size-4 text-amber-500" />
                  <h2 className="font-medium">Mejor cliente</h2>
                </div>
                {data.topCustomerName ? (
                  <div>
                    <div className="text-lg font-semibold">{data.topCustomerName}</div>
                    <div className="text-muted-foreground text-sm tabular-nums">
                      {formatMoney(data.topCustomerRevenue)} en ventas
                    </div>
                  </div>
                ) : (
                  <p className="text-muted-foreground text-sm">Aún no hay ventas confirmadas.</p>
                )}
              </div>

              <div className="bg-card rounded-xl border p-5">
                <h2 className="mb-3 font-medium">Productos más vendidos</h2>
                {data.topProducts.length > 0 ? (
                  <TopProducts data={data.topProducts} />
                ) : (
                  <p className="text-muted-foreground text-sm">Aún no hay ventas confirmadas.</p>
                )}
              </div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
}

function TopProducts({ data }: { data: { productName: string; quantitySold: number; revenue: number }[] }) {
  const max = Math.max(...data.map((p) => p.quantitySold), 1);
  return (
    <ul className="grid gap-2.5">
      {data.map((p) => (
        <li key={p.productName} className="grid gap-1">
          <div className="flex items-baseline justify-between text-sm">
            <span className="font-medium">{p.productName}</span>
            <span className="text-muted-foreground tabular-nums">
              {p.quantitySold} · {formatMoney(p.revenue)}
            </span>
          </div>
          <div className="bg-muted h-1.5 overflow-hidden rounded-full">
            <div
              className="bg-primary/80 h-full rounded-full"
              style={{ width: `${(p.quantitySold / max) * 100}%` }}
            />
          </div>
        </li>
      ))}
    </ul>
  );
}

function DashboardSkeleton() {
  return (
    <div className="grid gap-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-28 w-full rounded-xl" />
        ))}
      </div>
      <div className="grid gap-4 lg:grid-cols-3">
        <Skeleton className="h-64 w-full rounded-xl lg:col-span-2" />
        <Skeleton className="h-64 w-full rounded-xl" />
      </div>
    </div>
  );
}

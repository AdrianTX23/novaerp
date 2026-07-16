"use client";

import { useState } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { salesApi } from "@/lib/sales-api";
import { toastApiError } from "@/lib/api-errors";
import type { SalesOrderStatus } from "@/lib/types";
import { formatMoney } from "@/lib/utils";
import { CreateSalesOrderDialog } from "@/components/sales/create-sales-order-dialog";
import { TablePagination } from "@/components/ui/table-pagination";
import { ConfirmButton } from "@/components/ui/confirm-button";
import { QueryError } from "@/components/layout/query-error";

const STATUS_META: Record<SalesOrderStatus, { label: string; variant: "secondary" | "default" | "destructive" }> = {
  Draft: { label: "Borrador", variant: "secondary" },
  Confirmed: { label: "Confirmado", variant: "default" },
  Cancelled: { label: "Cancelado", variant: "destructive" },
};

const PAGE_SIZE = 50;

export function SalesOrdersTab() {
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();
  const ordersQuery = useQuery({
    queryKey: ["sales-orders", page],
    queryFn: () => salesApi.list({ page, pageSize: PAGE_SIZE }),
    placeholderData: keepPreviousData,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["sales-orders"] });
    // El stock cambió: refresca también el inventario.
    queryClient.invalidateQueries({ queryKey: ["products"] });
  };

  const confirmOrder = useMutation({
    mutationFn: (orderId: string) => salesApi.confirm(orderId),
    onSuccess: () => {
      invalidate();
      toast.success("Pedido confirmado. Stock descontado.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo confirmar el pedido.");
    },
  });

  const cancelOrder = useMutation({
    mutationFn: (orderId: string) => salesApi.cancel(orderId),
    onSuccess: () => {
      invalidate();
      toast.success("Pedido cancelado.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo cancelar el pedido.");
    },
  });

  const busy = confirmOrder.isPending || cancelOrder.isPending;

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{ordersQuery.data?.totalCount ?? 0} pedidos.</p>
        <CreateSalesOrderDialog />
      </div>

      {ordersQuery.isError ? (
        <QueryError error={ordersQuery.error} forbiddenMessage="Tu rol no tiene acceso a las ventas." />
      ) : ordersQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Pedido</TableHead>
              <TableHead>Cliente</TableHead>
              <TableHead>Fecha</TableHead>
              <TableHead>Estado</TableHead>
              <TableHead className="text-right">Total</TableHead>
              <TableHead className="w-40" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {ordersQuery.data?.items.map((order) => {
              const meta = STATUS_META[order.status];
              return (
                <TableRow key={order.id}>
                  <TableCell className="font-medium">
                    {order.orderNumber}
                    <p className="text-muted-foreground text-xs">{order.lineCount} líneas</p>
                  </TableCell>
                  <TableCell>{order.customerName}</TableCell>
                  <TableCell className="text-muted-foreground">{order.orderDate}</TableCell>
                  <TableCell>
                    <Badge variant={meta.variant}>{meta.label}</Badge>
                  </TableCell>
                  <TableCell className="text-right font-medium tabular-nums">
                    {formatMoney(order.totalAmount)}
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      {order.status === "Draft" && (
                        <Button
                          size="sm"
                          disabled={busy}
                          onClick={() => confirmOrder.mutate(order.id)}
                        >
                          Confirmar
                        </Button>
                      )}
                      {order.status !== "Cancelled" && (
                        <ConfirmButton
                          confirmLabel="Sí, cancelar"
                          pendingLabel="Cancelando…"
                          pending={cancelOrder.isPending}
                          disabled={busy}
                          onConfirm={() => cancelOrder.mutate(order.id)}
                        >
                          Cancelar
                        </ConfirmButton>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
            {ordersQuery.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground text-center">
                  Todavía no hay pedidos de venta.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}

      {ordersQuery.data && (
        <TablePagination
          page={page}
          pageSize={PAGE_SIZE}
          totalCount={ordersQuery.data.totalCount}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}

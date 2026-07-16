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
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { purchasingApi } from "@/lib/purchasing-api";
import { toastApiError } from "@/lib/api-errors";
import type { PurchaseOrderStatus } from "@/lib/types";
import { formatMoney } from "@/lib/utils";
import { CreatePurchaseOrderDialog } from "@/components/purchasing/create-purchase-order-dialog";
import { QueryError } from "@/components/layout/query-error";
import { ConfirmButton } from "@/components/ui/confirm-button";

const STATUS_META: Record<PurchaseOrderStatus, { label: string; variant: "secondary" | "default" | "destructive" }> = {
  Draft: { label: "Borrador", variant: "secondary" },
  Confirmed: { label: "Recibida", variant: "default" },
  Cancelled: { label: "Cancelada", variant: "destructive" },
};

export function PurchaseOrdersTab() {
  const queryClient = useQueryClient();
  const ordersQuery = useQuery({
    queryKey: ["purchase-orders"],
    queryFn: () => purchasingApi.list(),
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["purchase-orders"] });
    // El stock cambió: refresca también el inventario.
    queryClient.invalidateQueries({ queryKey: ["products"] });
  };

  const confirmOrder = useMutation({
    mutationFn: (orderId: string) => purchasingApi.confirm(orderId),
    onSuccess: () => {
      invalidate();
      toast.success("Orden confirmada. Stock ingresado al inventario.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo confirmar la orden.");
    },
  });

  const cancelOrder = useMutation({
    mutationFn: (orderId: string) => purchasingApi.cancel(orderId),
    onSuccess: () => {
      invalidate();
      toast.success("Orden cancelada.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo cancelar la orden.");
    },
  });

  const busy = confirmOrder.isPending || cancelOrder.isPending;

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{ordersQuery.data?.length ?? 0} órdenes.</p>
        <CreatePurchaseOrderDialog />
      </div>

      {ordersQuery.isError ? (
        <QueryError error={ordersQuery.error} forbiddenMessage="Tu rol no tiene acceso a las compras." />
      ) : ordersQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Orden</TableHead>
              <TableHead>Proveedor</TableHead>
              <TableHead>Fecha</TableHead>
              <TableHead>Estado</TableHead>
              <TableHead className="text-right">Total</TableHead>
              <TableHead className="w-40" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {ordersQuery.data?.map((order) => {
              const meta = STATUS_META[order.status];
              return (
                <TableRow key={order.id}>
                  <TableCell className="font-medium">
                    {order.orderNumber}
                    <p className="text-muted-foreground text-xs">{order.lineCount} líneas</p>
                  </TableCell>
                  <TableCell>{order.supplierName}</TableCell>
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
            {ordersQuery.data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground text-center">
                  Todavía no hay órdenes de compra.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

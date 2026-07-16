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
import { Skeleton } from "@/components/ui/skeleton";
import { invoicingApi } from "@/lib/invoicing-api";
import { ApiError } from "@/lib/api-client";
import { toastApiError } from "@/lib/api-errors";
import type { InvoiceStatus } from "@/lib/types";
import { formatMoney } from "@/lib/utils";
import { CreateInvoiceDialog } from "@/components/invoicing/create-invoice-dialog";
import { RegisterPaymentDialog } from "@/components/invoicing/register-payment-dialog";
import { TablePagination } from "@/components/ui/table-pagination";
import { ConfirmButton } from "@/components/ui/confirm-button";

const STATUS_META: Record<InvoiceStatus, { label: string; variant: "secondary" | "default" | "outline" | "destructive" }> = {
  Issued: { label: "Emitida", variant: "outline" },
  PartiallyPaid: { label: "Pago parcial", variant: "secondary" },
  Paid: { label: "Pagada", variant: "default" },
  Void: { label: "Anulada", variant: "destructive" },
};

const PAGE_SIZE = 50;

export function InvoicesTab() {
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();
  const invoicesQuery = useQuery({
    queryKey: ["invoices", page],
    queryFn: () => invoicingApi.list({ page, pageSize: PAGE_SIZE }),
    retry: false,
    placeholderData: keepPreviousData,
  });

  const forbidden = invoicesQuery.error instanceof ApiError && invoicesQuery.error.status === 403;

  const voidInvoice = useMutation({
    mutationFn: (invoiceId: string) => invoicingApi.void(invoiceId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invoices"] });
      toast.success("Factura anulada.");
    },
    onError: (error) => {
      toastApiError(error, "No se pudo anular la factura.");
    },
  });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{invoicesQuery.data?.totalCount ?? 0} facturas.</p>
        <CreateInvoiceDialog />
      </div>

      {forbidden ? (
        <div className="text-muted-foreground rounded-xl border border-dashed p-8 text-center text-sm">
          Tu rol no tiene acceso a la facturación.
        </div>
      ) : invoicesQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Factura</TableHead>
              <TableHead>Cliente</TableHead>
              <TableHead>Emitida</TableHead>
              <TableHead>Vence</TableHead>
              <TableHead>Estado</TableHead>
              <TableHead className="text-right">Total</TableHead>
              <TableHead className="text-right">Saldo</TableHead>
              <TableHead className="w-52" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {invoicesQuery.data?.items.map((invoice) => {
              const meta = STATUS_META[invoice.status];
              const canPay = invoice.status === "Issued" || invoice.status === "PartiallyPaid";
              const canVoid = invoice.status === "Issued" && invoice.amountPaid === 0;
              return (
                <TableRow key={invoice.id}>
                  <TableCell className="font-medium">{invoice.invoiceNumber}</TableCell>
                  <TableCell>{invoice.customerName}</TableCell>
                  <TableCell className="text-muted-foreground">{invoice.issueDate}</TableCell>
                  <TableCell className="text-muted-foreground">{invoice.dueDate}</TableCell>
                  <TableCell>
                    <Badge variant={meta.variant}>{meta.label}</Badge>
                  </TableCell>
                  <TableCell className="text-right tabular-nums">{formatMoney(invoice.total)}</TableCell>
                  <TableCell className="text-right font-medium tabular-nums">
                    {formatMoney(invoice.outstandingBalance)}
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      {canPay && <RegisterPaymentDialog invoice={invoice} />}
                      {canVoid && (
                        <ConfirmButton
                          confirmLabel="Sí, anular"
                          pendingLabel="Anulando…"
                          pending={voidInvoice.isPending}
                          onConfirm={() => voidInvoice.mutate(invoice.id)}
                        >
                          Anular
                        </ConfirmButton>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
            {invoicesQuery.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} className="text-muted-foreground text-center">
                  Todavía no hay facturas. Emite una desde un pedido de venta confirmado.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}

      {!forbidden && invoicesQuery.data && (
        <TablePagination
          page={page}
          pageSize={PAGE_SIZE}
          totalCount={invoicesQuery.data.totalCount}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}

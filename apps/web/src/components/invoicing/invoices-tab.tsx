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
import { invoicingApi } from "@/lib/invoicing-api";
import { ApiError } from "@/lib/api-client";
import type { InvoiceStatus } from "@/lib/types";
import { formatMoney } from "@/lib/utils";
import { CreateInvoiceDialog } from "@/components/invoicing/create-invoice-dialog";
import { RegisterPaymentDialog } from "@/components/invoicing/register-payment-dialog";

const STATUS_META: Record<InvoiceStatus, { label: string; variant: "secondary" | "default" | "outline" | "destructive" }> = {
  Issued: { label: "Emitida", variant: "outline" },
  PartiallyPaid: { label: "Pago parcial", variant: "secondary" },
  Paid: { label: "Pagada", variant: "default" },
  Void: { label: "Anulada", variant: "destructive" },
};

export function InvoicesTab() {
  const queryClient = useQueryClient();
  const invoicesQuery = useQuery({
    queryKey: ["invoices"],
    queryFn: () => invoicingApi.list(),
    retry: false,
  });

  const forbidden = invoicesQuery.error instanceof ApiError && invoicesQuery.error.status === 403;

  const voidInvoice = useMutation({
    mutationFn: (invoiceId: string) => invoicingApi.void(invoiceId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["invoices"] });
      toast.success("Factura anulada.");
    },
    onError: (error) => {
      const message = error instanceof ApiError ? error.problem.title : "No se pudo anular la factura.";
      toast.error(message);
    },
  });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{invoicesQuery.data?.length ?? 0} facturas.</p>
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
            {invoicesQuery.data?.map((invoice) => {
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
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={voidInvoice.isPending}
                          onClick={() => voidInvoice.mutate(invoice.id)}
                        >
                          Anular
                        </Button>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              );
            })}
            {invoicesQuery.data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={8} className="text-muted-foreground text-center">
                  Todavía no hay facturas. Emite una desde un pedido de venta confirmado.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

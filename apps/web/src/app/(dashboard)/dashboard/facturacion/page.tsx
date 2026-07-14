import { InvoicesTab } from "@/components/invoicing/invoices-tab";

export default function FacturacionPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Facturación</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Emite facturas desde pedidos confirmados y registra sus pagos.
      </p>

      <div className="mt-6">
        <InvoicesTab />
      </div>
    </div>
  );
}

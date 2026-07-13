import { SalesOrdersTab } from "@/components/sales/sales-orders-tab";

export default function VentasPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Ventas</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Pedidos de venta. El stock se descuenta al confirmar cada pedido.
      </p>

      <div className="mt-6">
        <SalesOrdersTab />
      </div>
    </div>
  );
}

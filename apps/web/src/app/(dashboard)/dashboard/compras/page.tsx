import { PurchaseOrdersTab } from "@/components/purchasing/purchase-orders-tab";

export default function ComprasPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Compras</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Órdenes de compra a proveedores. El stock ingresa al inventario al confirmar cada orden.
      </p>

      <div className="mt-6">
        <PurchaseOrdersTab />
      </div>
    </div>
  );
}

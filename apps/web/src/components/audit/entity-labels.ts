/** Nombre de clase C# (EntityName en el backend) → etiqueta en español para la UI. */
export const ENTITY_LABELS: Record<string, string> = {
  Product: "Producto",
  ProductCategory: "Categoría de producto",
  Partner: "Contacto",
  SalesOrder: "Pedido de venta",
  PurchaseOrder: "Orden de compra",
  Invoice: "Factura",
  Payment: "Pago",
  CashMovement: "Movimiento de caja",
  Account: "Cuenta contable",
  JournalEntry: "Asiento contable",
  Opportunity: "Oportunidad",
  User: "Usuario",
  Role: "Rol",
  Tenant: "Empresa",
};

export function entityLabel(entityName: string): string {
  return ENTITY_LABELS[entityName] ?? entityName;
}

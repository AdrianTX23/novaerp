# NovaERP — Fase 9: Facturación

Facturas emitidas desde pedidos de venta confirmados, con registro de pagos y
ciclo de vida emitida → parcialmente pagada → pagada (o anulada).

## La cadena Ventas → Facturación

Una factura se genera **desde un pedido de venta confirmado** (flujo Odoo/SAP).
Al emitir, la factura hace *snapshot* del cliente, las líneas y el total: es un
documento legal congelado, independiente de cambios posteriores.

## Decisiones de diseño

1. **Solo se factura un pedido confirmado, y una sola vez** (índice único
   `TenantId + SalesOrderId`). Borrador → 409, ya facturado → 409.
2. **Ciclo con pagos:** `Issued → PartiallyPaid → Paid`, más `Void`. Se registran
   pagos (monto, fecha, método, referencia); la factura lleva `AmountPaid` y el
   estado se deriva. No se puede pagar de más ni pagar una factura pagada/anulada
   (→ 409). Solo se puede anular una factura **sin pagos** registrados.
3. **El `Payment` prepara la Caja (Fase 10):** vive dentro del agregado `Invoice`
   (frontera de consistencia del saldo), pero se expone como `DbSet<Payment>`
   indexado por fecha para que la Caja lea los ingresos directamente.
4. **Permiso propio `invoices.read` / `invoices.manage`** — la facturación es una
   preocupación contable, distinta de la operación de ventas.

## Bug corregido durante la verificación

Registrar un pago fallaba con `DbUpdateConcurrencyException` (EF generaba un
`UPDATE payments` en vez de un `INSERT`): agregar un hijo a un agregado ya
cargado confunde el fixup de EF. Solución (el patrón ya establecido en Fase 4):
`Invoice.RegisterPayment` crea el `Payment`, actualiza el saldo/estado y lo
**devuelve**; el Handler lo agrega vía `db.Payments.Add(payment)` para forzar el
INSERT. Cubierto por un test de handler que verifica la persistencia.

## Endpoints (`/api/invoices`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/` | `invoices.read` | Lista (filtros: status, customerId) |
| GET | `/{id}` | `invoices.read` | Detalle con líneas y pagos |
| POST | `/` | `invoices.manage` | Emite desde un pedido confirmado |
| POST | `/{id}/payments` | `invoices.manage` | Registra un pago |
| POST | `/{id}/void` | `invoices.manage` | Anula (solo sin pagos) |

## Nota sobre permisos y tenants existentes

Los permisos nuevos se siembran en el catálogo global (migración `AddInvoicing`),
así que **las empresas registradas después** los reciben en su rol Owner. Los
tenants creados antes no los tienen retroactivamente (se observó un 403 correcto
en una empresa vieja). En producción se resolvería con una migración que
"rellene" los roles Owner existentes; en desarrollo la BD se re-siembra.

## Verificación

**Backend (curl contra Postgres real):** emitir desde pedido confirmado
(INV-00001, snapshot, vence a 30 días) → refacturar 409 → facturar borrador 409
→ pago parcial (PartiallyPaid) → sobrepago 409 → pago del saldo (Paid, 2 pagos)
→ anular pagada 409 → pagar pagada 409 → anular factura sin pagos (Void). Listado
correcto.

**Frontend:** compila; la página `/dashboard/facturacion` monta, se integró al
sidebar, y maneja el 403 con un mensaje claro (verificado por captura). La
verificación interactiva de los diálogos (emitir/pagar) quedó pendiente por la
indisponibilidad de clics del panel de navegador del entorno; reutilizan el
patrón ya verificado de Ventas/Compras.

**Tests:** 12 nuevos (dominio: pagos, estados, anulación; handlers: emitir desde
pedido, unicidad, persistencia del pago, anulación bloqueada). Suite total: 88
unit + 6 integración, en verde.

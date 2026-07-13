# NovaERP — Fase 6: Ventas

Pedidos de venta con ciclo de vida (borrador → confirmado → cancelado),
descuento de stock al confirmar y snapshot de precios.

## Modelo

- **`SalesOrder`** (raíz del agregado) agrupa sus **`SalesOrderLine`**. Las líneas
  se acceden siempre a través del pedido, nunca por separado.
- **Estados:** `Draft → Confirmed → Cancelled`.

## Decisiones de diseño

1. **El stock se descuenta al confirmar, no al crear.** Un borrador es una
   cotización que aún no reserva inventario (modelo Odoo/SAP). Al cancelar un
   pedido confirmado, el stock se devuelve.
2. **Snapshot de precio, SKU y nombre en cada línea.** Si el producto cambia de
   precio después, el pedido histórico sigue reflejando lo que se cobró. La línea
   guarda además `ProductId` para trazabilidad y el movimiento de stock.
3. **Reutiliza `Product.AdjustStock(-cantidad)`.** Si una línea excede el stock,
   `AdjustStock` lanza `DomainException` (→ 409) y el `SaveChanges` nunca se
   ejecuta: no queda ningún descuento a medias (atomicidad).
4. **`TotalAmount` denormalizado** en `sales_orders` para que el Dashboard
   (Fase 8) haga `SUM(TotalAmount)` sin recalcular líneas.
5. **Numeración `SO-00001` por empresa.** Cuenta todos los pedidos del tenant
   (incluidos cancelados). Ventana de carrera mínima bajo alta concurrencia,
   documentada; en producción se cerraría con una secuencia de Postgres.

## Endpoints (`/api/sales-orders`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/` | `sales.read` | Lista (filtros: status, customerId) |
| GET | `/{id}` | `sales.read` | Detalle con líneas |
| POST | `/` | `sales.manage` | Crea un borrador |
| POST | `/{id}/confirm` | `sales.manage` | Confirma y descuenta stock |
| POST | `/{id}/cancel` | `sales.manage` | Cancela (devuelve stock si estaba confirmado) |

## Verificación

**Backend (curl contra Postgres real):** crear borrador (no toca stock) →
confirmar (10→7) → cancelar (7→10). Doble confirmación → 409. Stock insuficiente
→ 409 **sin descuento parcial** (stock intacto tras el fallo). Pedido vacío → 400.
Vender a un proveedor puro → 409. Numeración SO-00001/SO-00002.

**Frontend:** la página `/dashboard/ventas` monta, hace `GET /api/sales-orders`
(200) y renderiza la tabla con columnas y estado vacío, sin errores de consola.
Nota: la verificación interactiva del diálogo de creación (clics) quedó
bloqueada por una indisponibilidad del panel de navegador del entorno, no del
código; el diálogo usa el mismo patrón ya verificado de Contactos/Inventario.

**Tests:** 18 nuevos (dominio: máquina de estados e invariantes de línea;
handlers sobre EF InMemory: create con snapshot sin tocar stock, confirm
descuenta, cancel devuelve, stock insuficiente lanza). Total suite: 59 unit + 6
integración, en verde.

## Alcance deliberado

No se incluye edición línea-por-línea de un borrador existente (agregar/quitar
líneas después de crearlo): el pedido se crea con todas sus líneas de una vez.
Esto mantiene el agregado limpio y evita el bug de fixup de EF que se resolvió
en la Fase 4.

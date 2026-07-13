# NovaERP — Fase 7: Compras

Órdenes de compra a proveedores. Espejo de Ventas (Fase 6): mismo ciclo
`Draft → Confirmed → Cancelled`, pero suma stock en vez de restarlo.

## Diferencias con Ventas

1. **Referencia a un proveedor** (`PartnerType.Supplier`), no a un cliente.
2. **Al confirmar, el stock sube** (recepción de mercancía): `AdjustStock(+cantidad)`.
   Confirmar una compra nunca falla por inventario (sumar siempre es válido).
3. **Cancelar una compra confirmada revierte el stock** (`AdjustStock(-cantidad)`),
   y *esto sí puede fallar*: si el stock recibido ya se consumió (p. ej. se
   vendió), la reversión dejaría el saldo en negativo y `DomainException` la
   rechaza (→ 409). Esta regla sale "gratis" de la invariante del dominio y es
   justo el tipo de integración cruzada Compras↔Ventas que distingue un ERP real.
4. **Snapshot de costo** (`CostPrice`), lo que se paga al proveedor. No se
   recalcula el costo promedio ponderado del producto al recibir — eso es
   costeo avanzado (FIFO/promedio), anotado como refinamiento futuro.

## Decisión de arquitectura

Compras y Ventas se mantienen como **agregados separados** aunque hoy se
parezcan: van a divergir (Ventas → facturación/envío; Compras →
recepción/pagos a proveedor), y una base común "Order" haría dolorosa esa
divergencia. Es duplicación de estructura, no de lógica (sumar vs. restar).

## Endpoints (`/api/purchase-orders`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/` | `purchases.read` | Lista (filtros: status, supplierId) |
| GET | `/{id}` | `purchases.read` | Detalle con líneas |
| POST | `/` | `purchases.manage` | Crea un borrador |
| POST | `/{id}/confirm` | `purchases.manage` | Confirma y suma stock |
| POST | `/{id}/cancel` | `purchases.manage` | Cancela (revierte stock si estaba confirmada) |

## Verificación

**Backend (curl contra Postgres real):** crear borrador de 100 (no toca stock)
→ confirmar (0→100) → vender 60 (100→40) → **cancelar la compra falla con 409**
porque reversar 100 sobre 40 dejaría el stock en −60; el stock queda intacto en
40. Cancelación limpia con stock intacto: confirmar (40→60) → cancelar (60→40).
Comprar a un cliente puro → 409. Orden sin líneas → 400. Numeración PO-00001/2.

**Frontend:** la página `/dashboard/compras` monta, hace `GET /api/purchase-orders`
(200) y renderiza la tabla con estado vacío, sin errores de consola. La
verificación interactiva del diálogo (clics) quedó bloqueada por la
indisponibilidad del panel de navegador del entorno, no del código; el diálogo
reutiliza el patrón ya verificado de Ventas.

**Tests:** 15 nuevos (dominio + handlers sobre EF InMemory, incluido el caso de
cancelación bloqueada por stock consumido). Suite total: 74 unit + 6 integración,
en verde.

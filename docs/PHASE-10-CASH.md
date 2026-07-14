# NovaERP — Fase 10: Caja

Vista de tesorería: cuánto dinero hay, de dónde viene y a dónde va. Combina los
cobros de facturas (Fase 9) con movimientos manuales.

## Decisión de diseño: unificación en lectura

La Caja combina **dos fuentes sin acoplarse a ninguna**:

1. **Pagos de facturas** (`Payment`, de la Fase 9) → cuentan como **ingresos**, de
   solo lectura (los gestiona Facturación).
2. **Movimientos manuales** (`CashMovement`, entidad nueva) → ingresos o egresos
   sin factura: renta, sueldos, aportes, gastos varios.

**Por qué así y no un libro mayor único:** la alternativa sería que cada pago de
factura *también* escribiera una fila en Caja, lo que acopla Facturación a Caja
y crea un problema de sincronización. Unificar en la query deja los pagos como
única fuente de verdad (propiedad de Facturación) y a la Caja como *reporte +
ajustes manuales*. Más desacoplado.

- **Saldo** = todos los ingresos − todos los egresos.
- Los movimientos manuales se crean/eliminan; los ingresos por pago son de solo
  lectura aquí (sin botón de eliminar en la UI).

## Endpoints (`/api/cash`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/summary` | `cash.read` | Saldo, ingresos/egresos totales y del mes |
| GET | `/movements` | `cash.read` | Lista unificada (pagos + manuales) |
| POST | `/movements` | `cash.manage` | Registra un movimiento manual |
| DELETE | `/movements/{id}` | `cash.manage` | Elimina un movimiento manual |

Permisos nuevos `cash.read` / `cash.manage` (sembrados vía migración `AddCash`).

## Robustez: conexión a Redis tolerante a fallos

Durante la verificación, Docker se cayó y la API **no arrancaba**: registraba
`ConnectionMultiplexer.Connect(...)` de forma *eager* en el arranque, así que si
Redis estaba caído toda la API fallaba —siendo que Redis es solo para caché/
SignalR, no ruta crítica. Corregido: la conexión ahora es una **factory perezosa**
(`sp => Connect(...)`, solo conecta al primer uso) con `AbortOnConnectFail=false`.
La app arranca aunque Redis esté caído y reintenta en segundo plano.

## Verificación

**Backend (curl contra Postgres real):** con una factura cobrada (ingreso 1500) +
un aporte manual (1000) + una renta (400): el resumen dio saldo 2100
(ingresos 2500 − egresos 400); la lista unificada mostró los 3 movimientos con el
cobro etiquetado como "Cobro INV-00001 [Factura]"; borrar el aporte bajó el saldo
a 1100; monto 0 → 400.

**Frontend:** verificado en el navegador con datos reales (login funcionó) — la
página `/dashboard/caja` muestra saldo $3.400, ingresos/egresos del mes con color,
y la lista unificada con el cobro de factura como ingreso de solo lectura junto a
los movimientos manuales. Integrada al sidebar. El diálogo de alta se probó por
curl (clics del panel intermitentes en el entorno).

**Tests:** 6 nuevos (dominio: monto positivo, concepto; handlers: resumen unifica
pagos+manuales, lista unificada, borrar ajusta saldo). Suite total: 94 unit + 6
integración, en verde.

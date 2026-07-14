# NovaERP — Fase 13: Reportes

Tres reportes analíticos sobre datos ya existentes (Ventas, Inventario,
Facturación). **Sin entidades nuevas ni migración** — es una capa de consultas.

## Los 3 reportes

1. **Ventas**: total y desglose diario en un rango de fechas elegido por el
   usuario (a diferencia del Dashboard, que siempre muestra "este mes").
2. **Inventario**: valorización total, por categoría, y lista de bajo stock.
3. **Cuentas por cobrar**: el reporte contable clásico — facturas `Issued`/
   `PartiallyPaid` (excluye `Paid` y `Void`) con **antigüedad de saldo** (aging):
   Al día, 1-30, 31-60, 60+ días de mora, calculada contra `DueDate`.

Permiso: reutiliza `reports.read` (ya existía, lo usa también el Dashboard).

## Endpoints (`/api/reports`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/sales?from=&to=` | Ventas confirmadas en el rango + desglose diario |
| GET | `/inventory` | Valorización, por categoría, bajo stock |
| GET | `/receivables` | Cuentas por cobrar con cubetas de antigüedad |

## Frontend

`/dashboard/reportes` con 3 pestañas (Ventas / Inventario / Cuentas por cobrar),
reemplazando el placeholder. Ventas tiene selector de rango de fechas y un
gráfico de barras horizontal por día. Cuentas por cobrar muestra las 4 cubetas
como tarjetas (la vencida resaltada en rojo) y la tabla con badges de
antigüedad.

## Verificación

**Backend (curl contra Postgres real):** ventas del rango con total/promedio/
desglose correctos; inventario con valorización por categoría y bajo stock;
cuentas por cobrar excluyendo una factura pagada y clasificando una factura con
vencimiento forzado 45 días atrás en la cubeta "31-60 días".

**Frontend:** verificado en el navegador con datos reales — Ventas con gráfico
por día, Inventario con valorización, y Cuentas por cobrar mostrando $3.700 al
día + $900 en 31-60 días (factura INV-00004 con badge destructivo).

**Tests:** 3 nuevos (ventas solo confirmadas en rango; valorización + bajo
stock; cuentas por cobrar excluye pagadas y clasifica cubetas). Suite total:
115 unit + 6 integración, en verde.

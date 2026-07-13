# NovaERP — Fase 8: Dashboard Ejecutivo

Página de inicio con indicadores del negocio, calculados desde los datos reales
de Ventas, Compras e Inventario.

## Diseño

- **Una sola query** (`GetDashboardQuery` → `DashboardDto`): el dashboard se
  carga como una unidad, evitando múltiples round-trips. Endpoint `/api/dashboard`
  protegido con `reports.read` (permiso que todos los roles del sistema tienen).
- **Solo cuentan pedidos confirmados:** los borradores aún no son ventas/compras
  reales y los cancelados están anulados.
- Es una query de **solo lectura que cruza agregados** (ventas + compras +
  productos + partners): legítimo, porque un reporte lee, no muta.

## Indicadores

| KPI | Cálculo |
|-----|---------|
| Ventas del mes | Σ TotalAmount de ventas confirmadas del mes actual |
| Pedidos del mes | nº de ventas confirmadas del mes |
| Compras del mes | Σ TotalAmount de compras confirmadas del mes |
| Bajo stock | productos activos con `reorderPoint` y `stock ≤ reorderPoint` |
| Valor de inventario | Σ `stock × costo` de productos activos |
| Mejor cliente | cliente con mayor Σ ingresos confirmados (histórico) |
| Ventas 6 meses | serie mensual para el gráfico (rellena huecos con 0) |
| Top productos | 5 productos más vendidos por cantidad |

## Notas técnicas (Npgsql)

- Npgsql **no traduce** un `SelectMany(o => o.Lines).GroupBy(...)` sobre una
  navegación con el filtro de tenant, ni un `GroupBy` sobre un `Contains` de
  subquery. Solución: para "top productos" se materializan las líneas de pedidos
  confirmados (volumen acotado en un dashboard) y se agrupan en memoria.
- El `GroupBy` mensual sobre `OrderDate.Year/Month` (DateOnly) **sí** lo traduce
  Npgsql a `EXTRACT`.

## Frontend

- KPIs en tarjetas (`KpiCard`), con tono de alerta ámbar cuando hay bajo stock.
- **Gráfico de barras SVG hecho a mano** (`SalesTrendChart`), sin librería de
  gráficos: decisión KISS para evitar una dependencia pesada y fricción con
  React 19, con control total de la estética y los temas claro/oscuro vía
  variables CSS. Barras con hover y animación de entrada.
- Lista de productos más vendidos con barras de proporción; destacado del mejor
  cliente. Skeletons de carga. Manejo elegante del 403 (rol sin `reports.read`).

## Verificación

**Backend (curl contra Postgres real):** con datos sembrados (2 clientes, 3
productos, 4 ventas confirmadas + 1 borrador + 1 compra) el endpoint devolvió
exactamente lo esperado: ventas del mes, nº de pedidos, compras, bajo stock,
valor de inventario, mejor cliente y top productos — el borrador excluido, la
serie de 6 meses con huecos en 0.

**Tests:** 2 nuevos (agrega solo confirmados y calcula KPIs; devuelve ceros sin
actividad). Suite total: 76 unit + 6 integración, en verde.

**Frontend:** compila limpio. La verificación visual en el navegador quedó
pendiente por una indisponibilidad temporal del clasificador de seguridad del
entorno (no del código); reintentar navegando a `/dashboard` autenticado.

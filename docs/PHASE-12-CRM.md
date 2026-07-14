# NovaERP — Fase 12: CRM

Pipeline de oportunidades de venta sobre los clientes (Partners) existentes.

## Modelo

- **`Opportunity`**: un negocio potencial con un cliente — título, valor estimado,
  etapa, fecha de cierre esperada, notas.
- **Etapas** (`OpportunityStage`): `New → Qualified → Proposal → Won / Lost`.

## Decisiones de diseño

1. **Won y Lost son terminales.** Una oportunidad cerrada no se reabre ni se
   mueve: `Opportunity.MoveTo(...)` lanza `DomainException` (→ 409) si ya está
   cerrada. La invariante la protege el dominio.
2. **Fecha de cierre sin tiempo ambiental.** `MoveTo(stage, asOf)` recibe la
   fecha por parámetro (no usa `DateTime.Now` dentro del dominio, que quedaría
   no testeable) y registra `ClosedOn` al pasar a Won/Lost — para el KPI
   "ganado del mes".
3. **Resumen del pipeline**: valor abierto (etapas no cerradas), conteo, ganado
   del mes, y desglose por etapa para las columnas del tablero. Se materializa y
   agrupa en memoria (volumen acotado por tenant).

Permiso nuevo `crm.read` / `crm.manage`.

## Endpoints (`/api/crm`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/opportunities` | `crm.read` | Lista de oportunidades |
| GET | `/pipeline` | `crm.read` | Resumen (abierto, ganado del mes, por etapa) |
| POST | `/opportunities` | `crm.manage` | Crea una oportunidad (etapa Nuevo) |
| POST | `/opportunities/{id}/move` | `crm.manage` | Mueve de etapa |

## Frontend

- **Tablero Kanban** (`/dashboard/crm`): una columna por etapa (Nuevo, Calificado,
  Propuesta, Ganado, Perdido) con encabezado de conteo + valor, y tarjetas por
  oportunidad (título, cliente, valor). Las tarjetas abiertas tienen un menú
  "Mover" para cambiar de etapa; las cerradas no. Tarjetas de resumen arriba
  (pipeline abierto, oportunidades abiertas, ganado del mes). Sidebar integrado;
  manejo elegante del 403.
- El `stage` viaja como número en el request (consistente con PartnerType /
  PaymentMethod / AccountType).

## Verificación

**Backend (curl contra Postgres real):** crear oportunidades (empiezan en New);
mover New→Qualified→Proposal→Won; mover a Lost; **mover una ganada → 409**
(cerrada); resumen con abierto 5000 (1 op) y ganado del mes 25000; oportunidad
para un proveedor puro → 409.

**Frontend:** verificado en el navegador con datos reales — el tablero muestra
las 5 columnas (Nuevo, Calificado, Propuesta, Ganado, Perdido) con sus tarjetas,
totales por columna, y resumen (pipeline abierto $85.000, 4 abiertas, ganado del
mes $15.000). Las tarjetas en Ganado/Perdido no muestran el menú "Mover" —
confirma que la regla de etapa terminal se refleja también en la UI.

**Tests:** 8 nuevos (dominio: etapas, cierre terminal, valor no negativo;
handlers: rechazo de no-cliente, resumen abierto/ganado, mover cerrada).
Suite total: 112 unit + 6 integración, en verde.

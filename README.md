# NovaERP

ERP SaaS multiempresa construido desde cero como proyecto de portafolio: 13
módulos de negocio interconectados, arquitectura limpia por capas, RBAC
granular, y 121 tests automatizados que cubren las reglas de negocio más
delicadas (partida doble, stock compartido entre módulos, ciclo de vida de
facturas y pagos).

> Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para el diseño completo
> (stack, multi-tenancy, estructura del monorepo) y `docs/PHASE-*.md` para el
> registro de decisiones de cada módulo.

## Stack

**Backend** — ASP.NET Core 9 · Clean Architecture (Domain/Application/
Infrastructure/API) · CQRS con MediatR · Entity Framework Core 9 + PostgreSQL ·
FluentValidation · JWT + refresh tokens · xUnit + FluentAssertions

**Frontend** — Next.js 16 (App Router) · React 19 · TypeScript · Tailwind CSS 4 ·
shadcn/ui sobre Base UI · TanStack Query · React Hook Form + Zod · Zustand

**Infraestructura** — Docker Compose (Postgres + Redis) · GitHub Actions (CI)

## Módulos

| Módulo | Qué hace |
|---|---|
| Autenticación y RBAC | JWT + refresh en cookie httpOnly, roles y permisos granulares por tenant |
| Usuarios y Roles | Gestión de accesos con roles del sistema y personalizados |
| Inventario | Productos, categorías, control de stock con punto de reorden |
| Contactos | Clientes y proveedores (patrón Business Partner) |
| Ventas | Pedidos con ciclo borrador → confirmado → cancelado; descuenta stock |
| Compras | Espejo de Ventas: recepción de mercancía suma stock |
| Facturación | Facturas desde pedidos confirmados, pagos parciales, saldo pendiente |
| Caja | Tesorería: unifica cobros de facturas + movimientos manuales |
| Contabilidad | Partida doble real: plan de cuentas, asientos balanceados, balance de comprobación |
| CRM | Pipeline de oportunidades con tablero kanban |
| Reportes | Ventas por rango, valorización de inventario, cuentas por cobrar con aging |
| Dashboard | KPIs y gráficos con datos reales de todos los módulos anteriores |

## Decisiones de arquitectura que vale la pena mirar

- **Multi-tenancy con Global Query Filters de EF Core** — ningún módulo de
  negocio filtra por tenant manualmente; es imposible olvidarlo.
- **La invariante contable la protege el dominio, no el controller** —
  `JournalEntry.EnsureBalanced()` rechaza cualquier asiento donde
  Σdebe ≠ Σhaber antes de tocar la base de datos.
- **Compras y Ventas comparten el mismo stock** — cancelar una compra ya
  recibida y parcialmente vendida se rechaza automáticamente si dejaría el
  inventario en negativo. Esa regla emergió sola de la invariante
  `Product.AdjustStock`, sin código especial de integración entre módulos.
- **Caja no duplica datos**: unifica los pagos de Facturación (solo lectura)
  con movimientos manuales, en vez de escribir dos veces la misma transacción.

## Arrancar en local

```bash
# 1. Infraestructura (Postgres + Redis)
docker compose up -d

# 2. Backend
cd apps/api/src/NovaERP.API
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"
dotnet run

# 3. Frontend (desde la raíz del monorepo)
npm install
npm run dev --workspace=web
```

- Web: http://localhost:3000
- API + documentación interactiva (Scalar): http://localhost:5080/scalar
- Health check: http://localhost:5080/health

## Tests

```bash
cd apps/api && dotnet test
```

115 tests unitarios (dominio + handlers sobre EF Core InMemory) y 6 de
integración (`WebApplicationFactory` contra el pipeline HTTP real), todos en
verde.

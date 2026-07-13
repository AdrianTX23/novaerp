# NovaERP — Guía de Arquitectura (Fase 0)

Este documento explica las decisiones arquitectónicas tomadas antes de escribir la primera línea de código de negocio. Sirve como referencia para cualquier persona (incluido tú mismo en 6 meses, o un reclutador) que quiera entender **por qué** el proyecto está construido como está.

---

## 1. Visión del proyecto

NovaERP es un ERP SaaS multiempresa (multi-tenant) diseñado para portafolio profesional, con estándares de código y arquitectura equivalentes a un producto comercial real (nivel SAP / Dynamics 365 / NetSuite / Odoo), pero operando con **costo de infraestructura $0**.

---

## 2. Stack tecnológico final

| Capa | Tecnología | Hosting |
|---|---|---|
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS 4, shadcn/ui, TanStack Query, React Hook Form, Zod, Zustand, Motion | Vercel (free tier) |
| Backend | ASP.NET Core 9 Web API, Clean Architecture, CQRS + MediatR, EF Core, FluentValidation, AutoMapper, SignalR | Railway o Azure App Service F1 (free tier) |
| Base de datos | **PostgreSQL** (vía Npgsql) | Neon (serverless, con branching de DB por rama de Git) |
| Cache | Redis | Upstash (free tier) |
| Jobs en background | Hangfire, con storage en PostgreSQL | Mismo Postgres de Neon |
| Realtime | SignalR (self-hosted dentro de la API) | — |
| CI/CD | GitHub Actions | Gratis en repos públicos/privados personales |

### Por qué PostgreSQL en vez de SQL Server

El plan original pedía SQL Server, pero Azure SQL no tiene un free tier sostenible para un proyecto que debe estar **desplegado y accesible en vivo** (no solo en capturas de pantalla). PostgreSQL con EF Core (Npgsql) ofrece las mismas garantías: índices, constraints, transacciones, migraciones. Neon además da *branching de base de datos por Pull Request*, algo que refuerza el nivel técnico del proyecto en una entrevista.

### Por qué Hangfire sin Redis dedicado para jobs

Usar `Hangfire.PostgreSql` evita depender de dos servicios gratuitos distintos (cada uno con sus propios límites). Redis (Upstash) se reserva para caché de queries frecuentes y como backplane de SignalR si el proyecto escala a múltiples instancias.

---

## 3. Multi-tenancy: Shared Database + TenantId

**Decisión:** todas las empresas cliente comparten la misma base de datos física. Cada tabla de negocio tiene una columna `TenantId (Guid)`.

Es el mismo patrón que usan Shopify y Stripe internamente antes de justificar aislamiento físico por cliente. Es el más barato de operar y el más simple de migrar.

### Cómo se garantiza el aislamiento entre empresas

1. **Resolución del tenant:** un middleware lee el claim `tenant_id` del JWT en cada request y lo expone vía `ITenantProvider` (scoped).
2. **Global Query Filters de EF Core:** el `DbContext` aplica automáticamente `HasQueryFilter(e => e.TenantId == _currentTenant.Id)` a toda entidad que implemente `ITenantEntity`. Esto hace **imposible** que una query nueva olvide filtrar por tenant — el filtro se aplica a nivel de framework, no de cada desarrollador recordándolo.
3. **SaveChangesInterceptor:** al guardar cambios, un interceptor asigna automáticamente `TenantId`, `CreatedAt`, `CreatedBy`, y convierte los deletes en soft-delete (`IsDeleted`, `DeletedAt`) en vez de borrar filas físicamente.
4. **Índices:** todo índice compuesto lidera con `TenantId` (ej. `IX_Products_TenantId_Sku`), para que el planner de consultas de Postgres particione lógicamente por empresa de forma eficiente.

---

## 4. Estructura del Monorepo

```
novaerp/
├── apps/
│   ├── web/                        # Next.js 16
│   └── api/
│       ├── src/
│       │   ├── NovaERP.Domain/          # Entidades y reglas de negocio puras — sin dependencias externas
│       │   ├── NovaERP.Application/     # CQRS: Commands, Queries, Handlers, DTOs, Validators
│       │   ├── NovaERP.Infrastructure/  # EF Core, Hangfire, Redis, servicios externos
│       │   └── NovaERP.API/             # Controllers/endpoints, middleware, composition root
│       └── tests/
│           ├── NovaERP.UnitTests/
│           └── NovaERP.IntegrationTests/
├── packages/
│   ├── api-types/                  # Tipos TS generados desde el OpenAPI del backend (contrato único front-back)
│   └── ui/                         # Componentes shadcn compartidos
├── .github/workflows/               # CI: lint, test, build (separado por app)
└── docker-compose.yml               # Postgres + Redis para desarrollo local
```

### Por qué monorepo

Un solo repositorio con `/apps/web` y `/apps/api` simplifica el versionado conjunto y el CI/CD coordinado. Es el patrón que usan Vercel y Linear internamente. La carpeta `packages/api-types` evita que frontend y backend se desincronicen: los tipos TypeScript se generan desde el contrato OpenAPI del backend, no se escriben a mano dos veces.

### Por qué 4 proyectos separados en el backend (Clean Architecture estricta)

`NovaERP.Domain` no depende de EF Core, de MediatR, ni de nada externo. Esto permite testear las reglas de negocio sin necesidad de una base de datos real, y es lo primero que revisa un ingeniero senior al evaluar un repositorio.

No se usa un "Repository Pattern" genérico sobre EF Core: el `DbContext` ya funciona como Unit of Work, y `IQueryable` + specifications cubren la mayoría de los casos. Un repositorio explícito solo se introduce cuando el Handler necesita lógica de acceso a datos compleja y reutilizable (ej. `IInventoryRepository.GetReorderCandidatesAsync`).

---

## 5. Flujo de una request (ejemplo: crear producto)

```
Next.js (React Hook Form + Zod validan en cliente)
   → TanStack Query mutation
   → API Controller (delgado: solo mapea HTTP → Command)
   → Pipeline de MediatR:
        1. ValidationBehavior   (FluentValidation)
        2. AuthorizationBehavior (RBAC)
        3. Handler               (lógica de aplicación)
   → DbContext (con TenantId ya filtrado automáticamente) → PostgreSQL
   → Domain Event opcional (ej. "ProductCreated")
        → Hangfire job (ej. recalcular stock)
        → SignalR (notificar UI en tiempo real)
```

---

## 6. Próxima fase

**Fase 1 — Scaffolding del monorepo:** creación de la estructura de carpetas real, `docker-compose.yml` con Postgres + Redis local, proyectos .NET vacíos con las referencias correctas entre capas, y el proyecto Next.js inicial con Tailwind y shadcn/ui configurados. Todavía sin lógica de negocio — solo el esqueleto compilable y verificable.

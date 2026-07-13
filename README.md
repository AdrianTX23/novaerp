# NovaERP

ERP SaaS multiempresa. Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para el diseño completo (stack, multi-tenancy, estructura del monorepo).

## Requisitos

- Node.js 22+
- .NET SDK 9
- Docker (para Postgres y Redis locales)

## Arrancar en local

```bash
# 1. Infraestructura local (Postgres + Redis)
docker compose up -d

# 2. Backend
cd apps/api/src/NovaERP.API
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"
dotnet run

# 3. Frontend (desde la raíz del monorepo)
npm install
npm run dev --workspace=web
```

- API: http://localhost:5xxx/swagger (puerto asignado por `dotnet run`, ver consola)
- Web: http://localhost:3000
- Health check de la API: `/health`

## Estructura

```
apps/
  web/     # Next.js 16
  api/     # ASP.NET Core 9 — Clean Architecture (Domain/Application/Infrastructure/API)
packages/
  api-types/  # Tipos TS generados desde el OpenAPI del backend (Fase 2+)
  ui/         # Componentes shadcn compartidos (Fase 2+)
```

## Estado

**Fase 1 — Scaffolding completo.** Sin módulos de negocio todavía: el backend compila (6/6 proyectos) con multi-tenancy, auditoría y soft-delete ya cableados a nivel de framework; el frontend compila y corre con dark/light mode y TanStack Query listos.

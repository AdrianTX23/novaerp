# NovaERP — Fase 2: Autenticación + RBAC

Primer módulo de negocio. Implementa registro self-service de empresas, login,
refresh tokens con rotación, y autorización basada en permisos (RBAC).

## Endpoints

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/api/auth/register` | Anónimo | Crea empresa (tenant) + primer usuario (Owner) en una transacción |
| POST | `/api/auth/login` | Anónimo | Valida credenciales, emite access + refresh token |
| POST | `/api/auth/refresh` | Cookie | Rota el refresh token, emite nuevo access token |
| POST | `/api/auth/logout` | Cookie | Revoca el refresh token actual |
| GET | `/api/auth/me` | Bearer | Datos del usuario autenticado + roles + permisos |

## Modelo de datos

```
tenants ──1:N── users ──N:M── roles ──N:M── permissions (catálogo global)
                  │                └── role_permissions
                  └── user_roles
users ──1:N── refresh_tokens
```

- **Email globalmente único** — un email = una cuenta = un tenant. Permite que el
  login (sin tenant conocido aún) resuelva usuario por email.
- **Permisos** = catálogo global estático (10 permisos sembrados por migración con
  Guid deterministas). El código autoriza contra el `Code` (ej. `inventory.read`),
  nunca contra nombres de rol.
- **Roles del sistema** — al registrar un tenant se siembra el rol `Owner` con todos
  los permisos, marcado `IsSystem` (no editable/borrable desde UI).

## Seguridad

- **Contraseñas**: `PasswordHasher<T>` de ASP.NET Core (PBKDF2, salt por hash). Solo
  el hasher, sin arrastrar todo ASP.NET Identity.
- **Access token JWT**: 15 min. Claims `sub`, `email`, `tenant_id`, `name`, y un
  `permission` por cada permiso efectivo del usuario. `MapInboundClaims = false`
  para que los claims lleguen sin remapear a URIs largas.
- **Refresh token**: 7 días, guardado en DB **solo como hash SHA-256** (nunca en
  claro). Viaja al cliente en cookie `httpOnly` + `Secure` + `SameSite=Strict`,
  path acotado a `/api/auth`. En cada refresh se **rota** (el viejo se revoca y
  apunta al nuevo). Reutilizar un token revocado revoca toda la cadena del usuario
  (detección de robo).
- **Errores**: `ExceptionHandlingMiddleware` traduce excepciones de dominio a
  ProblemDetails con el código HTTP correcto (400/401/409/500) sin filtrar stack traces.

## Decisión técnica destacada: Global Query Filters y el cacheo de modelo de EF Core

Los filtros de tenant se aplican en `NovaErpDbContext` referenciando el **campo de
instancia** `_tenantId` dentro de un lambda de C# real (vía métodos genéricos
invocados por reflexión), **no** con `Expression.Constant`. Motivo: EF Core cachea
el modelo una sola vez; hornear el tenant como constante congelaría el valor de la
primera petición (típicamente `Guid.Empty` en el arranque de Hangfire), rompiendo
el aislamiento para siempre. Con el campo de instancia, EF lo trata como parámetro
re-evaluado por query. Este bug se detectó y corrigió probando `/me` end-to-end.

## Verificación realizada

- Migración `InitialIdentity` aplicada a Postgres (8 tablas, 10 permisos sembrados).
- Flujo completo probado con curl: register → me → login → refresh → logout, más
  casos de error (email duplicado 409, password incorrecto 401, password débil 400).
- Aislamiento multi-tenant confirmado: un segundo tenant (Globex) solo ve sus datos.
- Rotación de refresh tokens verificada en la propia base de datos.
- 10 tests unitarios en verde (PasswordHasher, TokenService, reglas de dominio).

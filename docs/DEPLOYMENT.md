# NovaERP — Guía de despliegue ($0)

Tres piezas independientes, conectadas por variables de entorno. Todo tiene
free tier suficiente para un portafolio. El código ya está listo (Dockerfile,
CORS, cookies cross-site, forwarded headers) — esto son los pasos que solo tú
puedes hacer porque implican crear cuentas y pegar secretos.

```
GitHub (código)
   ├─→ Neon        → Postgres administrado
   ├─→ Railway      → Backend (.NET, desde apps/api/Dockerfile)
   └─→ Vercel       → Frontend (Next.js, desde apps/web/)
```

Orden recomendado: **Neon → Railway → Vercel**. Cada paso siguiente necesita
una URL del paso anterior.

---

## 0. Subir a GitHub

```bash
# En github.com, crea un repo vacío (sin README/gitignore) y copia su URL.
git remote add origin https://github.com/TU-USUARIO/novaerp.git
git push -u origin main
```

## 1. Base de datos — Neon (neon.tech)

1. Crear cuenta → **New Project** → nombre `novaerp`, región cercana a donde
   despliegues el backend.
2. Copiar el **connection string** que te da Neon (modo "Pooled connection",
   formato `postgresql://usuario:password@host/novaerp?sslmode=require`).
3. Convertirlo al formato que espera Npgsql:
   `Host=<host>;Port=5432;Database=novaerp;Username=<usuario>;Password=<password>;Ssl Mode=Require;Trust Server Certificate=true`
4. Aplicar las migraciones **una vez**, desde tu máquina, apuntando a Neon:
   ```bash
   cd apps/api
   NOVAERP_MIGRATIONS_CONNECTION="<connection string de Neon>" \
     dotnet ef database update --project src/NovaERP.Infrastructure --startup-project src/NovaERP.API
   ```

## 2. Backend — Railway (railway.app)

1. **New Project → Deploy from GitHub repo** → selecciona tu repo.
2. En Settings del servicio: **Root Directory** = `apps/api` (ahí vive el
   `Dockerfile`; Railway lo detecta solo).
3. Variables de entorno (Settings → Variables):

   | Variable | Valor |
   |---|---|
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `ConnectionStrings__Postgres` | el connection string de Neon del paso 1 |
   | `Jwt__SigningKey` | genera uno con `openssl rand -base64 48` — **nunca reutilices el de tu máquina local** |
   | `Jwt__Issuer` | `NovaERP` |
   | `Jwt__Audience` | `NovaERP.Clients` |
   | `Cors__AllowedOrigins__0` | la URL de Vercel (la sabrás en el paso 3 — puedes volver a editar esta variable después) |

   `ConnectionStrings__Redis` se puede dejar sin definir: la conexión a Redis
   es perezosa y tolerante a fallos (ver `docs/PHASE-10-CASH.md`), así que la
   API arranca igual sin ella — solo pierdes caché, no funcionalidad.

4. Deploy. Railway te da una URL pública (`https://algo.up.railway.app`).
   Verifica con `https://esa-url/health` → debe responder `200`.

## 3. Frontend — Vercel (vercel.com)

1. **Add New → Project** → importa el mismo repo de GitHub.
2. **Root Directory** = `apps/web`.
3. Variable de entorno:

   | Variable | Valor |
   |---|---|
   | `NEXT_PUBLIC_API_URL` | la URL de Railway del paso 2 (sin `/` final) |

4. Deploy. Vercel te da la URL final (`https://novaerp-tuusuario.vercel.app`).
5. **Vuelve a Railway** y actualiza `Cors__AllowedOrigins__0` con esta URL de
   Vercel exacta (con `https://`, sin `/` final) — sin esto, el navegador
   bloquea las peticiones del frontend por CORS.

## Verificación final

1. Abre la URL de Vercel → `/register` → crea una empresa de prueba.
2. Si el login falla en el segundo intento (tras refrescar la página), revisa
   que `Cors__AllowedOrigins__0` en Railway sea exactamente la URL de Vercel.
3. Si ves un error 500 al registrar, probablemente falten las migraciones —
   repite el paso 1.4.

## Qué NO necesitas para esto

Redis (Upstash) queda pendiente para cuando construyas notificaciones en
tiempo real (SignalR) — no bloquea el despliegue actual.

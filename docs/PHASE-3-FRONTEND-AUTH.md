# NovaERP — Fase 3: Frontend de Autenticación

Pantallas de login/registro y el shell del dashboard (sidebar + topbar),
conectados a los endpoints de la Fase 2.

## Manejo de sesión

- El **access token vive solo en memoria** (`useAuthStore`, Zustand) — nunca en
  `localStorage`, para que un XSS no pueda robarlo.
- El **refresh token** vive en la cookie `httpOnly` que puso el backend; el
  navegador la envía solo en cada `fetch` con `credentials: "include"`.
- `AuthProvider` intenta un refresh silencioso al montar la app, para reponer
  la sesión tras un F5 sin pedir login de nuevo.
- `lib/api-client.ts` reintenta automáticamente una vez con refresh si una
  request devuelve 401, transparente para el resto de la app.
- `RequireAuth` (client-side) protege el grupo `(dashboard)`: mientras el
  refresh silencioso resuelve muestra un skeleton; si falla, redirige a `/login`.

## Decisiones y bugs corregidos durante la verificación

1. **`turbopack.root` mal apuntado.** En la Fase 1 lo configuré a `apps/web`
   para silenciar un warning de lockfile — pero eso le dice a Turbopack que el
   workspace termina ahí, y dejó de ver el `node_modules` hoisted por npm
   workspaces en la raíz del monorepo (`Module not found: Can't resolve 'react'`).
   Corregido apuntando dos niveles arriba.
2. **`proxy.ts` con cookie cross-origin.** Diseñé una barrera optimista en
   `proxy.ts` que leía la cookie de refresh para redirigir a `/login`. Pero esa
   cookie la pone el backend en `localhost:5080`, un origen distinto al del
   frontend (`localhost:3000`) donde corre `proxy.ts` — nunca la vería.
   Eliminado; la protección real vive en `RequireAuth` (cliente), que sí puede
   validar la sesión vía `fetch` cross-origin con `credentials: "include"`.
3. **`asChild` no existe en este setup.** Este proyecto de shadcn usa **Base UI**
   (no Radix): la composición de un trigger con un componente propio se hace
   con la prop `render={<Elemento />}`, no `asChild`.
4. **Toggle de tema comparaba contra `theme` en vez de `resolvedTheme`.**
   `next-themes` con `defaultTheme="system"` devuelve `"system"` en `theme`;
   comparar `theme === "dark"` nunca coincide cuando el usuario no ha elegido
   explícitamente, y el botón parecía no hacer nada. Corregido usando
   `resolvedTheme`, que siempre es `"dark"` o `"light"`.
5. **`next dev` con Turbopack en frío es demasiado lento para probar en este
   entorno** (minutos por compilación bajo carga). Para verificación se usa
   `npm run build && npm run start` (modo producción) — también es más fiel a
   cómo se comportará en Vercel.

## Verificación realizada

Flujo real en navegador contra la API corriendo sobre Postgres:
registro (Acme Browser Test) → redirección automática a `/dashboard` →
saludo personalizado, rol Owner, 10 permisos → sidebar con los 6 módulos y
"Configuración" → avatar con iniciales → toggle de tema (confirmado que
alterna `dark`/`light` en `<html>` y `localStorage`, usando `resolvedTheme`) →
logout (limpia la sesión, invalida el refresh y redirige a `/login`; la
preferencia de tema persiste porque es independiente de la sesión).

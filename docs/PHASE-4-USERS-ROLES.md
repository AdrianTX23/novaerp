# NovaERP — Fase 4: Usuarios y Roles

Gestión de quién tiene acceso a la empresa (usuarios) y qué puede hacer cada quien (roles + permisos), sobre la base de auth de la Fase 2.

## Backend

`RoleClaimType = "permission"` ya estaba configurado en el JWT desde la Fase 2, así que la autorización de estos endpoints no necesitó policies nuevas: `[Authorize(Roles = Permissions.UsersManage)]` funciona directo porque ASP.NET Core compara contra los claims `"permission"` del token.

**Roles del sistema.** Antes solo se sembraba `Owner` al registrar una empresa. Se añadieron `Admin` (todos los permisos salvo `roles.manage`) y `Member` (solo lectura), sembrados en el registro, para que el Owner tenga roles asignables desde el primer momento sin tener que crear uno a mano.

**Guardas de negocio:**
- Nadie puede desactivar su propia cuenta.
- No se puede quitar el rol Owner al único Owner activo restante.
- Los roles `IsSystem` (Owner, Admin, Member) no se pueden editar ni eliminar.
- No se puede eliminar un rol con usuarios asignados.

### Bug de EF Core: `Clear()` + `Add()` sobre una colección de navegación ya trackeada

Al reasignar los roles de un usuario o los permisos de un rol, la primera implementación mutaba la colección de navegación del agregado (`user.ReplaceRoles(...)`, `role.SetPermissions(...)`) con un `Clear()` seguido de reinserciones. Esto disparaba `DbUpdateConcurrencyException: expected to affect 1 row(s), but actually affected 0` — EF generaba un `UPDATE` sobre la fila de la tabla de unión en vez de un `INSERT`, tratando una entidad nueva como si ya existiera. El causante: mezclar en el mismo `DbContext` una entidad padre cargada por un `Include` (`db.Users.Include(u => u.Roles)`) con entidades relacionadas (`Role`) cargadas por una query separada confunde el fixup automático de navegación de EF.

**Solución:** en vez de mutar la colección de navegación del agregado, los Handlers (`UpdateUserRolesCommandHandler`, `UpdateRoleCommandHandler`) manipulan la tabla de unión directamente vía su propio `DbSet<UserRole>` / `DbSet<RolePermission>` — comparan el estado actual contra el deseado y solo hacen `AddRange`/`RemoveRange` de la diferencia real, sin tocar lo que no cambió. Es más explícito, evita el caso límite de EF, y de paso es más eficiente (no reescribe filas sin cambios).

### Endpoints

- `GET/POST /api/users`, `PUT /api/users/{id}/roles`, `POST /api/users/{id}/deactivate|reactivate`
- `GET/POST/PUT/DELETE /api/roles`, `GET /api/permissions`

## Frontend

`/dashboard/usuarios` con dos pestañas (Usuarios / Roles vía shadcn `Tabs`, base Base UI). La pestaña Roles solo se muestra si el usuario tiene `roles.manage`.

- **Usuarios:** tabla con nombre, email, badges de rol, `Switch` de activo/inactivo (deshabilitado para la propia cuenta), diálogo de creación con checkboxes de roles, diálogo de edición de roles.
- **Roles:** tabla con badge "Sistema" para los no editables, diálogo de creación/edición con permisos agrupados por módulo (Compras, Inventario, Ventas...), confirmación inline de dos pasos para eliminar (sin depender de `window.confirm` ni instalar un `AlertDialog` aparte).

Todo verificado en navegador contra la API real: crear usuario, reasignar roles (agregando y quitando, incluyendo el caso que antes daba 500), activar/desactivar, crear/editar/eliminar rol custom.

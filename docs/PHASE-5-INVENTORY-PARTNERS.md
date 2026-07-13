# NovaERP — Fase 5: Inventario y Contactos (Clientes/Proveedores)

Primer módulo de negocio real. Se reordenó el roadmap original (que ponía el
Dashboard Ejecutivo primero) porque un dashboard de KPIs sin ningún módulo
transaccional detrás solo puede mostrar ceros o datos inventados — mejor
construirlo cuando haya ventas/inventario reales que agregar.

## Decisión de modelado: `Partner` en vez de `Cliente`/`Proveedor`

Una sola entidad con `Type` como `[Flags] enum` (`Customer`, `Supplier`, o
ambos) en vez de dos tablas casi idénticas — patrón "Business Partner" de
SAP/Odoo. Evita duplicar Nombre/Email/Teléfono/Dirección, y una empresa que es
cliente y proveedor a la vez (frecuente) no necesita dos registros. El filtro
por tipo en `ListPartnersQueryHandler` usa AND bit a bit (`(p.Type & type) ==
type`), no igualdad, para que un partner "Ambos" aparezca en las dos pestañas.

## Alcance deliberado: stock sin ledger de movimientos

`Product.AdjustStock(delta)` es un ajuste manual directo sobre
`QuantityOnHand`. No existe todavía una tabla de movimientos con historial —
se construirá cuando Compras y Ventas existan, porque son esas transacciones
las que tienen el motivo de negocio real detrás de cada cambio de stock (no
tiene sentido diseñar el ledger antes de tener quién lo alimenta).

## Primera excepción de dominio

Hasta esta fase ninguna entidad de Domain rechazaba una operación (los
métodos como `Deactivate()` o `AssignRole()` eran o siempre válidos o
idempotentes). `Product.AdjustStock()` es el primer caso real de invariante de
negocio ("el stock no puede quedar negativo"), así que se creó
`NovaERP.Domain.Common.Exceptions.DomainException` — vive en Domain (no en
Application, para no invertir la dependencia) y `ExceptionHandlingMiddleware`
la traduce a 409, igual que `ConflictException`.

## Bug real encontrado en el navegador: `Select.Value` de Base UI

Al elegir una categoría en el formulario de producto, el combobox mostraba el
GUID crudo de la categoría en vez de su nombre ("Bebidas"). Causa: a
diferencia de Radix, `<Select.Value>` de Base UI renderiza el `value` tal cual
salvo que se le pase un render-prop que mapee el valor a una etiqueta — el
wrapper de shadcn no lo hace automáticamente. Corregido pasando
`children={(value) => categoriesQuery.data?.find(c => c.id === value)?.name}`.

## Bug de tipos: Zod 4 `coerce.number()` + react-hook-form

`useForm<ProductFormValues>` fallaba a compilar porque los campos con
`z.coerce.number()` tienen tipo de ENTRADA `unknown` (antes de coercionar) y
tipo de SALIDA `number` (después) — son distintos, y react-hook-form 7.81+
soporta exactamente este caso con un tercer genérico:
`useForm<Input, Context, Output>`. Se exportan `ProductFormInput` (`z.input`)
y `ProductFormValues` (`z.output`) por separado.

## Permisos nuevos

`partners.read` / `partners.manage` (grupo "Contactos"), migración
`AddCatalogAndPartners`. Los tenants registrados **antes** de esta fase no
tienen estos permisos en su rol Owner (los roles no ganan permisos
retroactivamente cuando el catálogo crece) — hay que registrar una empresa
nueva para probar Contactos con una cuenta vieja.

## Verificación

Backend: 41 unit tests (EF InMemory) + 6 integration tests (Postgres real) ya
existentes, más los nuevos handlers de Catálogo y Partners cubiertos con el
mismo patrón. Frontend: flujo completo en navegador contra la API real —
categoría → producto con stock inicial bajo el punto de reorden → badge "Bajo
stock" automático → ajuste de stock → sale del filtro de bajo stock →
contacto "Ambos" aparece en Clientes y Proveedores → contacto solo-Cliente
correctamente excluido de Proveedores.

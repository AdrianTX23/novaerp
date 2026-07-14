# NovaERP — Fase 11: Contabilidad básica

Contabilidad de **partida doble** simplificada: plan de cuentas, asientos
balanceados y balance de comprobación.

## Modelo

- **`Account`** (cuenta contable): código, nombre y tipo (Activo, Pasivo,
  Patrimonio, Ingreso, Gasto). Tenant-scoped.
- **`JournalEntry`** (asiento): raíz del agregado que agrupa sus líneas.
- **`JournalEntryLine`**: carga a una cuenta al debe **o** al haber (nunca ambos,
  ni ninguno).

## Decisiones de diseño

1. **La invariante sagrada la protege el dominio.** `JournalEntry.EnsureBalanced()`
   exige Σdebe = Σhaber (y > 0, y ≥ 2 líneas). Un asiento descuadrado lanza
   `DomainException` → 409 con el detalle ("debe X ≠ haber Y"). Cada línea valida
   además que cargue a un solo lado.
2. **Plan de cuentas base sembrado al registrar la empresa** (como los roles):
   10 cuentas estándar (Caja, Bancos, Clientes, Inventario, Proveedores, Capital,
   Ventas, Costo de ventas, Gastos…), para que la empresa "abra los libros" lista.
3. **Balance de comprobación**: por cada cuenta con movimiento, suma del debe y
   del haber, y saldo según su naturaleza (deudora = debe−haber; acreedora =
   haber−debe). Si los libros cuadran, el total del debe iguala al del haber.
   Se materializan las líneas y se agrupan en memoria (Npgsql no traduce un
   GroupBy sobre un Contains de subquery — mismo patrón que el dashboard).

Permiso nuevo `accounting.read` / `accounting.manage`.

## Endpoints (`/api/accounting`)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| GET | `/accounts` | `accounting.read` | Plan de cuentas |
| POST | `/accounts` | `accounting.manage` | Crea una cuenta |
| GET | `/journal-entries` | `accounting.read` | Lista de asientos |
| POST | `/journal-entries` | `accounting.manage` | Registra un asiento (balanceado) |
| GET | `/trial-balance` | `accounting.read` | Balance de comprobación |

## Verificación

**Backend (curl contra Postgres real):** plan de 10 cuentas sembrado al
registrar; asiento balanceado ASI-00001 creado; **descuadre (debe 1000 ≠ haber
900) → 409**; línea con debe y haber a la vez → 409; balance de comprobación con
debe total = haber total = 1500 y `isBalanced = true`.

**Frontend:** verificado en el navegador — la página `/dashboard/contabilidad`
(pestañas Asientos / Balance / Plan de cuentas) muestra los asientos y el balance
de comprobación con "Los libros cuadran" y totales $16.300 = $16.300. El diálogo
de asiento verifica el cuadre en vivo (debe/haber) y deshabilita el envío si no
cuadra. Sidebar integrado.

**Tests:** 10 nuevos (dominio: cuadre, línea debe-XOR-haber, mínimo de líneas;
handlers: asiento balanceado, rechazo de descuadre, balance de comprobación).
Suite total: 104 unit + 6 integración, en verde.

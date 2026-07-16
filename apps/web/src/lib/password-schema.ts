import { z } from "zod";

/**
 * Reglas de contraseña compartidas entre registro y creación de usuarios.
 * Espejo de las reglas de FluentValidation del backend (RegisterCommandValidator
 * y CreateUserCommandValidator) — si cambian allá, cambiar acá.
 */
export const passwordSchema = z
  .string()
  .min(8, "Debe tener al menos 8 caracteres")
  .regex(/[A-Z]/, "Debe incluir una mayúscula")
  .regex(/[a-z]/, "Debe incluir una minúscula")
  .regex(/[0-9]/, "Debe incluir un número");

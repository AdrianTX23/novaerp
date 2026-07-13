import { z } from "zod";

// Espejo de las reglas de FluentValidation en RegisterCommandValidator (backend).
const passwordSchema = z
  .string()
  .min(8, "Debe tener al menos 8 caracteres")
  .regex(/[A-Z]/, "Debe incluir una mayúscula")
  .regex(/[a-z]/, "Debe incluir una minúscula")
  .regex(/[0-9]/, "Debe incluir un número");

export const registerSchema = z.object({
  companyName: z.string().min(1, "Requerido").max(200),
  fullName: z.string().min(1, "Requerido").max(200),
  email: z.string().min(1, "Requerido").email("Email inválido"),
  password: passwordSchema,
});

export type RegisterFormValues = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.string().min(1, "Requerido").email("Email inválido"),
  password: z.string().min(1, "Requerido"),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

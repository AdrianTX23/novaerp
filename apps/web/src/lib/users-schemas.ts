import { z } from "zod";

export const createUserSchema = z.object({
  fullName: z.string().min(1, "Requerido").max(200),
  email: z.string().min(1, "Requerido").email("Email inválido"),
  password: z
    .string()
    .min(8, "Debe tener al menos 8 caracteres")
    .regex(/[A-Z]/, "Debe incluir una mayúscula")
    .regex(/[a-z]/, "Debe incluir una minúscula")
    .regex(/[0-9]/, "Debe incluir un número"),
  roleIds: z.array(z.string()).min(1, "Selecciona al menos un rol"),
});

export type CreateUserFormValues = z.infer<typeof createUserSchema>;

export const roleFormSchema = z.object({
  name: z.string().min(1, "Requerido").max(100),
  description: z.string().max(300).optional(),
  permissionCodes: z.array(z.string()).min(1, "Selecciona al menos un permiso"),
});

export type RoleFormValues = z.infer<typeof roleFormSchema>;

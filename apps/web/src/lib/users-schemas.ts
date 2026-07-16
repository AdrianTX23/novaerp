import { z } from "zod";
import { passwordSchema } from "@/lib/password-schema";

export const createUserSchema = z.object({
  fullName: z.string().min(1, "Requerido").max(200),
  email: z.string().min(1, "Requerido").email("Email inválido"),
  password: passwordSchema,
  roleIds: z.array(z.string()).min(1, "Selecciona al menos un rol"),
});

export type CreateUserFormValues = z.infer<typeof createUserSchema>;

export const roleFormSchema = z.object({
  name: z.string().min(1, "Requerido").max(100),
  description: z.string().max(300).optional(),
  permissionCodes: z.array(z.string()).min(1, "Selecciona al menos un permiso"),
});

export type RoleFormValues = z.infer<typeof roleFormSchema>;

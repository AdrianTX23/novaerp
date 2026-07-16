import { z } from "zod";
import { passwordSchema } from "@/lib/password-schema";

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

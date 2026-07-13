import { z } from "zod";

export const partnerFormSchema = z.object({
  name: z.string().min(1, "Requerido").max(200),
  isCustomer: z.boolean(),
  isSupplier: z.boolean(),
  documentNumber: z.string().max(50).optional(),
  email: z.string().email("Email inválido").optional().or(z.literal("")),
  phone: z.string().max(50).optional(),
  address: z.string().max(300).optional(),
}).refine((v) => v.isCustomer || v.isSupplier, {
  message: "Elige si es cliente, proveedor o ambos.",
  path: ["isCustomer"],
});

export type PartnerFormValues = z.infer<typeof partnerFormSchema>;

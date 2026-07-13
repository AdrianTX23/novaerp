import { z } from "zod";

export const categoryFormSchema = z.object({
  name: z.string().min(1, "Requerido").max(150),
  description: z.string().max(500).optional(),
});

export type CategoryFormValues = z.infer<typeof categoryFormSchema>;

export const productFormSchema = z.object({
  sku: z.string().min(1, "Requerido").max(50),
  name: z.string().min(1, "Requerido").max(200),
  unitOfMeasure: z.string().min(1, "Requerido").max(20),
  costPrice: z.coerce.number().min(0, "No puede ser negativo"),
  salePrice: z.coerce.number().min(0, "No puede ser negativo"),
  categoryId: z.string().optional(),
  description: z.string().max(1000).optional(),
  reorderPoint: z.preprocess(
    (v) => (v === "" || v === undefined ? undefined : v),
    z.coerce.number().min(0).optional(),
  ),
  initialQuantity: z.coerce.number().min(0, "No puede ser negativo"),
});

// react-hook-form necesita el tipo de ENTRADA (lo que hay en los <input>, antes
// de coerce) distinto del de SALIDA (lo que produce el resolver tras validar) —
// ver el triple genérico en useForm<Input, Context, Output> en product-form-dialog.
export type ProductFormInput = z.input<typeof productFormSchema>;
export type ProductFormValues = z.output<typeof productFormSchema>;

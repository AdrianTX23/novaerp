import { toast } from "sonner";
import { ApiError } from "@/lib/api-client";

/**
 * Muestra el error de una mutación en un toast, priorizando los errores de
 * validación por campo que devuelve el backend (FluentValidation) sobre el
 * título genérico. Antes ~19 mutaciones descartaban ese detalle y mostraban
 * solo "Ocurrió un error" — con esto el usuario ve QUÉ campo rechazó el API.
 */
export function toastApiError(error: unknown, fallback: string) {
  if (error instanceof ApiError) {
    if (error.problem.errors?.length) {
      error.problem.errors.forEach((e) => toast.error(e.error));
      return;
    }
    toast.error(error.problem.title || fallback);
    return;
  }
  toast.error(fallback);
}

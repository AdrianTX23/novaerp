"use client";

import { ApiError } from "@/lib/api-client";
import { Forbidden } from "@/components/layout/forbidden";

interface QueryErrorProps {
  error: unknown;
  forbiddenMessage: string;
}

/**
 * Estado de error estándar para listados: distingue "tu rol no puede ver esto"
 * (403) de un fallo real de red/servidor. Antes la mayoría de las tablas no
 * manejaba isError y quedaban en blanco (o mostraban "0 registros", que es
 * peor porque miente).
 */
export function QueryError({ error, forbiddenMessage }: QueryErrorProps) {
  if (error instanceof ApiError && error.status === 403) {
    return <Forbidden message={forbiddenMessage} />;
  }

  return (
    <div className="text-muted-foreground rounded-xl border border-dashed p-8 text-center text-sm">
      No se pudo cargar la información. Revisa tu conexión e intenta de nuevo.
    </div>
  );
}

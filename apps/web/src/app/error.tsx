"use client";

import { Button } from "@/components/ui/button";
import { AlertTriangle } from "lucide-react";

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-4 p-6 text-center">
      <AlertTriangle className="text-muted-foreground size-10" strokeWidth={1.5} />
      <div className="space-y-1">
        <h1 className="text-lg font-semibold">Algo salió mal</h1>
        <p className="text-muted-foreground text-sm">
          Ocurrió un error inesperado. Puedes reintentar o volver al inicio.
        </p>
        {error.digest && (
          <p className="text-muted-foreground/60 text-xs">Código: {error.digest}</p>
        )}
      </div>
      <div className="flex gap-2">
        <Button onClick={reset}>Reintentar</Button>
        <Button variant="outline" onClick={() => (window.location.href = "/dashboard")}>
          Ir al inicio
        </Button>
      </div>
    </div>
  );
}

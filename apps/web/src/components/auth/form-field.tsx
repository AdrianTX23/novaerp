import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import type { ComponentProps } from "react";

interface FormFieldProps extends ComponentProps<typeof Input> {
  label: string;
  error?: string;
}

export function FormField({ label, error, id, className, ...inputProps }: FormFieldProps) {
  // aria-describedby asocia el texto del error al campo: sin esto, un lector
  // de pantalla anuncia "inválido" pero no puede decir POR QUÉ.
  const errorId = error ? `${id}-error` : undefined;

  return (
    <div className="grid gap-1.5">
      <Label htmlFor={id}>{label}</Label>
      <Input
        id={id}
        aria-invalid={!!error}
        aria-describedby={errorId}
        className={cn(error && "border-destructive focus-visible:ring-destructive/30", className)}
        {...inputProps}
      />
      {error && (
        <p id={errorId} className="text-destructive text-xs">
          {error}
        </p>
      )}
    </div>
  );
}

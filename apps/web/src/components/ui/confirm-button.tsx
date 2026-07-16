"use client";

import { useState, type ReactNode } from "react";
import { Button } from "@/components/ui/button";

interface ConfirmButtonProps {
  /** Contenido del botón inicial (texto o ícono). */
  children: ReactNode;
  /** Texto del botón rojo de confirmación (ej. "Confirmar", "Sí, anular"). */
  confirmLabel: string;
  /** Texto mientras la acción corre (ej. "Anulando…"). */
  pendingLabel: string;
  pending: boolean;
  disabled?: boolean;
  onConfirm: () => void;
  variant?: "ghost" | "outline";
  size?: "sm" | "icon-sm";
  "aria-label"?: string;
}

/**
 * Acción irreversible en dos clics: el primero muestra Confirmar/Cancelar en
 * el mismo lugar, el segundo ejecuta. Mismo patrón que ya usaban
 * DeleteCategoryButton/DeleteRoleButton, extraído para anular facturas,
 * cancelar pedidos y eliminar movimientos — que antes eran de un solo clic.
 */
export function ConfirmButton({
  children,
  confirmLabel,
  pendingLabel,
  pending,
  disabled,
  onConfirm,
  variant = "outline",
  size = "sm",
  "aria-label": ariaLabel,
}: ConfirmButtonProps) {
  const [confirming, setConfirming] = useState(false);

  if (confirming) {
    return (
      <div className="flex items-center gap-1">
        <Button variant="destructive" size="sm" disabled={pending} onClick={onConfirm}>
          {pending ? pendingLabel : confirmLabel}
        </Button>
        <Button variant="ghost" size="sm" disabled={pending} onClick={() => setConfirming(false)}>
          Cancelar
        </Button>
      </div>
    );
  }

  return (
    <Button variant={variant} size={size} disabled={disabled} aria-label={ariaLabel} onClick={() => setConfirming(true)}>
      {children}
    </Button>
  );
}

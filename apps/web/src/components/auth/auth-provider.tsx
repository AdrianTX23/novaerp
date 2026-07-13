"use client";

import { useEffect } from "react";
import { useAuthStore } from "@/stores/auth-store";
import { refreshSession } from "@/lib/api-client";

/**
 * Al montar la app, intenta reponer la sesión desde la cookie httpOnly.
 * Si no hay cookie o expiró, queda "unauthenticated" sin ruido.
 */
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const status = useAuthStore((s) => s.status);
  const clearSession = useAuthStore((s) => s.clearSession);

  useEffect(() => {
    if (status !== "idle") return;

    refreshSession().then((ok) => {
      if (!ok) clearSession();
    });
  }, [status, clearSession]);

  return children;
}

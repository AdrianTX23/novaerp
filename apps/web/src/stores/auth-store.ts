import { create } from "zustand";
import type { AuthUser } from "@/lib/types";

interface AuthState {
  accessToken: string | null;
  user: AuthUser | null;
  /** "idle" hasta que el intento de refresh silencioso al cargar la app resuelve. */
  status: "idle" | "authenticated" | "unauthenticated";
  setSession: (accessToken: string, user: AuthUser) => void;
  clearSession: () => void;
}

/**
 * El access token vive solo en memoria (nunca localStorage/sessionStorage):
 * un XSS que ejecute JS arbitrario no puede robarlo. Sobrevive mientras dura
 * el tab; al recargar, AuthProvider lo repone vía refresh silencioso.
 */
export const useAuthStore = create<AuthState>((set) => ({
  accessToken: null,
  user: null,
  status: "idle",
  setSession: (accessToken, user) =>
    set({ accessToken, user, status: "authenticated" }),
  clearSession: () =>
    set({ accessToken: null, user: null, status: "unauthenticated" }),
}));

export function hasPermission(code: string): boolean {
  return useAuthStore.getState().user?.permissions.includes(code) ?? false;
}

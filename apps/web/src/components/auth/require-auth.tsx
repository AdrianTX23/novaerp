"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/stores/auth-store";
import { Skeleton } from "@/components/ui/skeleton";

/**
 * Gate del lado del cliente: mientras el refresh silencioso del AuthProvider
 * resuelve ("idle"), muestra un skeleton. Si termina "unauthenticated",
 * redirige a /login. La barrera dura (cookie ausente) ya la aplicó proxy.ts.
 */
export function RequireAuth({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const status = useAuthStore((s) => s.status);

  useEffect(() => {
    if (status === "unauthenticated") {
      router.replace("/login");
    }
  }, [status, router]);

  if (status !== "authenticated") {
    return (
      <div className="flex flex-1 items-center justify-center">
        <Skeleton className="h-8 w-40" />
      </div>
    );
  }

  return children;
}

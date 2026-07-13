"use client";

import { useAuthStore } from "@/stores/auth-store";

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);

  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">
        Hola, {user?.fullName.split(" ")[0]}
      </h1>
      <p className="text-muted-foreground mt-1 text-sm">
        {user?.roles.join(", ")} en tu empresa · {user?.permissions.length} permisos activos.
      </p>
      <p className="text-muted-foreground mt-6 text-sm">
        El dashboard ejecutivo (KPIs, gráficos) llega en su propia fase.
      </p>
    </div>
  );
}

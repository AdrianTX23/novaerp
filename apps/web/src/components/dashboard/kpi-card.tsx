import type { LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

interface KpiCardProps {
  label: string;
  value: string;
  hint?: string;
  icon: LucideIcon;
  /** Resalta la tarjeta cuando el valor pide atención (ej. bajo stock). */
  tone?: "default" | "warning";
}

export function KpiCard({ label, value, hint, icon: Icon, tone = "default" }: KpiCardProps) {
  return (
    <div className="bg-card flex flex-col gap-3 rounded-xl border p-5">
      <div className="flex items-start justify-between">
        <span className="text-muted-foreground text-xs font-medium tracking-wide uppercase">
          {label}
        </span>
        <span
          className={cn(
            "flex size-8 items-center justify-center rounded-lg",
            tone === "warning" ? "bg-amber-500/10 text-amber-600 dark:text-amber-500" : "bg-muted text-muted-foreground",
          )}
        >
          <Icon className="size-4" />
        </span>
      </div>
      <div>
        <div className="text-2xl font-semibold tracking-tight tabular-nums">{value}</div>
        {hint && <div className="text-muted-foreground mt-0.5 text-xs">{hint}</div>}
      </div>
    </div>
  );
}

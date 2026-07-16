"use client";

import { useState } from "react";
import type { MonthlyPoint } from "@/lib/types";
import { formatMoney } from "@/lib/utils";

const MONTHS_ES = ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

function monthLabel(month: string): string {
  const [, m] = month.split("-");
  return MONTHS_ES[Number(m) - 1] ?? month;
}

/**
 * Gráfico de barras de ventas por mes, dibujado a mano en SVG (sin librería).
 * viewBox + width 100% lo hacen responsive; los colores salen de variables CSS
 * para respetar los temas claro/oscuro.
 */
export function SalesTrendChart({ data }: { data: MonthlyPoint[] }) {
  const [hover, setHover] = useState<number | null>(null);

  const max = Math.max(...data.map((d) => d.total), 1);
  const width = 100;
  const height = 44;
  const gap = 3;
  const barWidth = (width - gap * (data.length - 1)) / data.length;

  return (
    <div className="w-full">
      <svg
        viewBox={`0 0 ${width} ${height}`}
        className="h-40 w-full overflow-visible"
        preserveAspectRatio="none"
        role="img"
        aria-label="Ventas por mes"
      >
        {data.map((point, i) => {
          const barHeight = point.total > 0 ? Math.max((point.total / max) * (height - 6), 1.5) : 0;
          const x = i * (barWidth + gap);
          const y = height - barHeight;
          const active = hover === i;
          return (
            <g key={point.month}>
              {/* pista de fondo */}
              <rect
                x={x}
                y={0}
                width={barWidth}
                height={height}
                fill="currentColor"
                className="text-muted/40"
                rx={1.2}
                onMouseEnter={() => setHover(i)}
                onMouseLeave={() => setHover(null)}
              />
              <rect
                x={x}
                y={y}
                width={barWidth}
                height={barHeight}
                rx={1.2}
                className={active ? "fill-primary" : "fill-primary/80"}
                style={{ transition: "y 0.5s ease, height 0.5s ease, fill 0.15s" }}
              />
            </g>
          );
        })}
      </svg>

      <div className="mt-2 flex justify-between">
        {data.map((point, i) => (
          <div key={point.month} className="flex-1 text-center">
            <div className="text-muted-foreground text-xs">{monthLabel(point.month)}</div>
            <div
              className={`text-xs tabular-nums transition-colors ${
                hover === i ? "text-foreground font-medium" : "text-muted-foreground"
              }`}
            >
              {point.total > 0 ? formatMoney(point.total) : "—"}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

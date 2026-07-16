"use client";

import { Button } from "@/components/ui/button";

interface TablePaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

/** Pie de paginación estándar para tablas: contador + Anterior/Siguiente. */
export function TablePagination({ page, pageSize, totalCount, onPageChange }: TablePaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  if (totalCount <= pageSize) {
    return null;
  }

  return (
    <div className="flex items-center justify-between gap-4 border-t px-4 py-3">
      <span className="text-muted-foreground text-xs">
        {totalCount} registro{totalCount === 1 ? "" : "s"} · página {page} de {totalPages}
      </span>
      <div className="flex gap-2">
        <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => onPageChange(Math.max(1, page - 1))}>
          Anterior
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        >
          Siguiente
        </Button>
      </div>
    </div>
  );
}

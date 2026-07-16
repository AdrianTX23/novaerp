"use client";

import { useState } from "react";
import { ChevronDown, ChevronRight } from "lucide-react";
import { TableCell, TableRow } from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import type { AuditAction, AuditLogDto, FieldChange } from "@/lib/audit-api";
import { entityLabel } from "@/components/audit/entity-labels";

const ACTION_LABEL: Record<AuditAction, string> = {
  Created: "Creado",
  Updated: "Actualizado",
  Deleted: "Eliminado",
};

const ACTION_VARIANT: Record<AuditAction, "secondary" | "outline" | "destructive"> = {
  Created: "secondary",
  Updated: "outline",
  Deleted: "destructive",
};

function parseChanges(changes: string | null): FieldChange[] {
  if (!changes) return [];
  try {
    return JSON.parse(changes) as FieldChange[];
  } catch {
    return [];
  }
}

export function AuditLogRow({ log }: { log: AuditLogDto }) {
  const [expanded, setExpanded] = useState(false);
  const changes = parseChanges(log.changes);
  const canExpand = log.action === "Updated" && changes.length > 0;

  return (
    <>
      <TableRow
        className={canExpand ? "cursor-pointer" : undefined}
        onClick={canExpand ? () => setExpanded((v) => !v) : undefined}
      >
        <TableCell className="w-8">
          {canExpand &&
            (expanded ? (
              <ChevronDown className="text-muted-foreground size-4" />
            ) : (
              <ChevronRight className="text-muted-foreground size-4" />
            ))}
        </TableCell>
        <TableCell className="text-muted-foreground whitespace-nowrap">
          {new Date(log.createdAt).toLocaleString("es", { dateStyle: "medium", timeStyle: "short" })}
        </TableCell>
        <TableCell>{log.userEmail ?? "Sistema"}</TableCell>
        <TableCell>
          <Badge variant={ACTION_VARIANT[log.action]}>{ACTION_LABEL[log.action]}</Badge>
        </TableCell>
        <TableCell>{entityLabel(log.entityName)}</TableCell>
        <TableCell className="text-muted-foreground font-mono text-xs">
          {log.entityId.slice(0, 8)}
        </TableCell>
      </TableRow>
      {expanded && canExpand && (
        <TableRow>
          <TableCell />
          <TableCell colSpan={5} className="bg-muted/30">
            <dl className="grid gap-1.5 py-1 text-xs">
              {changes.map((c) => (
                <div key={c.field} className="flex flex-wrap items-baseline gap-1.5">
                  <dt className="text-muted-foreground font-medium">{c.field}:</dt>
                  <dd>
                    <span className="text-muted-foreground line-through">{c.oldValue ?? "—"}</span>
                    {" → "}
                    <span className="font-medium">{c.newValue ?? "—"}</span>
                  </dd>
                </div>
              ))}
            </dl>
          </TableCell>
        </TableRow>
      )}
    </>
  );
}

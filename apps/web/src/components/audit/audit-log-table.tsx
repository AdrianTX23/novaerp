"use client";

import { useState } from "react";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { History } from "lucide-react";
import {
  Table,
  TableBody,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { TablePagination } from "@/components/ui/table-pagination";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { auditApi } from "@/lib/audit-api";
import { AuditLogRow } from "@/components/audit/audit-log-row";
import { ENTITY_LABELS } from "@/components/audit/entity-labels";
import { QueryError } from "@/components/layout/query-error";

const PAGE_SIZE = 25;
const ALL_ENTITIES = "all";

export function AuditLogTable() {
  const [entityName, setEntityName] = useState(ALL_ENTITIES);
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);

  const query = useQuery({
    queryKey: ["audit", entityName, from, to, page],
    queryFn: () =>
      auditApi.list({
        entityName: entityName === ALL_ENTITIES ? undefined : entityName,
        from: from || undefined,
        to: to || undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
    placeholderData: keepPreviousData,
  });

  const data = query.data;

  return (
    <div className="grid gap-4">
      <div className="flex flex-wrap items-end gap-4">
        <div className="grid gap-1.5">
          <Label>Entidad</Label>
          <Select
            value={entityName}
            onValueChange={(v) => {
              setEntityName(v ?? ALL_ENTITIES);
              setPage(1);
            }}
          >
            <SelectTrigger className="w-48">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value={ALL_ENTITIES}>Todas</SelectItem>
              {Object.entries(ENTITY_LABELS).map(([code, label]) => (
                <SelectItem key={code} value={code}>{label}</SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="audit-from">Desde</Label>
          <Input
            id="audit-from"
            type="date"
            value={from}
            onChange={(e) => {
              setFrom(e.target.value);
              setPage(1);
            }}
          />
        </div>
        <div className="grid gap-1.5">
          <Label htmlFor="audit-to">Hasta</Label>
          <Input
            id="audit-to"
            type="date"
            value={to}
            onChange={(e) => {
              setTo(e.target.value);
              setPage(1);
            }}
          />
        </div>
      </div>

      {query.isError ? (
        <QueryError error={query.error} forbiddenMessage="Tu rol no tiene acceso a la auditoría." />
      ) : query.isLoading ? (
        <Skeleton className="h-64 w-full" />
      ) : !data || data.items.length === 0 ? (
        <div className="text-muted-foreground flex flex-col items-center gap-2 rounded-xl border border-dashed py-16 text-center text-sm">
          <History className="size-8" strokeWidth={1.5} />
          No hay actividad registrada con estos filtros.
        </div>
      ) : (
        <div className="bg-card rounded-xl border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-8" />
                <TableHead>Fecha</TableHead>
                <TableHead>Usuario</TableHead>
                <TableHead>Acción</TableHead>
                <TableHead>Entidad</TableHead>
                <TableHead>ID</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.items.map((log) => (
                <AuditLogRow key={log.id} log={log} />
              ))}
            </TableBody>
          </Table>

          <TablePagination page={page} pageSize={PAGE_SIZE} totalCount={data.totalCount} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
}

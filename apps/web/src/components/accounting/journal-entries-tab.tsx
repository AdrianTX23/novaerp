"use client";

import { useQuery } from "@tanstack/react-query";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import { accountingApi } from "@/lib/accounting-api";
import { formatMoney } from "@/lib/utils";
import { CreateJournalEntryDialog } from "@/components/accounting/create-journal-entry-dialog";

export function JournalEntriesTab() {
  const entriesQuery = useQuery({
    queryKey: ["accounting", "journal-entries"],
    queryFn: accountingApi.journalEntries,
  });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">{entriesQuery.data?.length ?? 0} asientos.</p>
        <CreateJournalEntryDialog />
      </div>

      {entriesQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Asiento</TableHead>
              <TableHead>Fecha</TableHead>
              <TableHead>Descripción</TableHead>
              <TableHead>Referencia</TableHead>
              <TableHead className="text-right">Importe</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {entriesQuery.data?.map((e) => (
              <TableRow key={e.id}>
                <TableCell className="font-medium">{e.number}</TableCell>
                <TableCell className="text-muted-foreground">{e.date}</TableCell>
                <TableCell>{e.description}</TableCell>
                <TableCell className="text-muted-foreground">{e.reference ?? "—"}</TableCell>
                <TableCell className="text-right font-medium tabular-nums">{formatMoney(e.total)}</TableCell>
              </TableRow>
            ))}
            {entriesQuery.data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} className="text-muted-foreground text-center">
                  Todavía no hay asientos.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

import { AuditLogTable } from "@/components/audit/audit-log-table";

export default function AuditoriaPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Auditoría</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Quién creó, modificó o eliminó cada registro y cuándo.
      </p>

      <div className="mt-6">
        <AuditLogTable />
      </div>
    </div>
  );
}

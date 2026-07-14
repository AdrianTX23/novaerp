import { CrmBoard } from "@/components/crm/crm-board";

export default function CrmPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">CRM</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Pipeline de oportunidades de venta con tus clientes.
      </p>

      <div className="mt-6">
        <CrmBoard />
      </div>
    </div>
  );
}

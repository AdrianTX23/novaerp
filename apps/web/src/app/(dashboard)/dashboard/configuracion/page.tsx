import { CompanySettings } from "@/components/settings/company-settings";

export default function ConfiguracionPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Configuración</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Administra los datos de tu empresa.
      </p>

      <div className="mt-6">
        <CompanySettings />
      </div>
    </div>
  );
}

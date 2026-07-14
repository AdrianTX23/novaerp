import { CashView } from "@/components/cash/cash-view";

export default function CajaPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Caja</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Tesorería: los cobros de facturas y tus movimientos manuales, con el saldo actual.
      </p>

      <div className="mt-6">
        <CashView />
      </div>
    </div>
  );
}

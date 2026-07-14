"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { SalesReportTab } from "@/components/reports/sales-report-tab";
import { InventoryReportTab } from "@/components/reports/inventory-report-tab";
import { ReceivablesReportTab } from "@/components/reports/receivables-report-tab";

export default function ReportesPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Reportes</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Ventas por período, valorización de inventario y cuentas por cobrar.
      </p>

      <Tabs defaultValue="ventas" className="mt-6">
        <TabsList>
          <TabsTrigger value="ventas">Ventas</TabsTrigger>
          <TabsTrigger value="inventario">Inventario</TabsTrigger>
          <TabsTrigger value="cobrar">Cuentas por cobrar</TabsTrigger>
        </TabsList>
        <TabsContent value="ventas" className="mt-4">
          <SalesReportTab />
        </TabsContent>
        <TabsContent value="inventario" className="mt-4">
          <InventoryReportTab />
        </TabsContent>
        <TabsContent value="cobrar" className="mt-4">
          <ReceivablesReportTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}

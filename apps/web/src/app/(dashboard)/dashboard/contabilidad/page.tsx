"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { AccountsTab } from "@/components/accounting/accounts-tab";
import { JournalEntriesTab } from "@/components/accounting/journal-entries-tab";
import { TrialBalanceTab } from "@/components/accounting/trial-balance-tab";

export default function ContabilidadPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Contabilidad</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Plan de cuentas, asientos de partida doble y balance de comprobación.
      </p>

      <Tabs defaultValue="asientos" className="mt-6">
        <TabsList>
          <TabsTrigger value="asientos">Asientos</TabsTrigger>
          <TabsTrigger value="balance">Balance</TabsTrigger>
          <TabsTrigger value="cuentas">Plan de cuentas</TabsTrigger>
        </TabsList>
        <TabsContent value="asientos" className="mt-4">
          <JournalEntriesTab />
        </TabsContent>
        <TabsContent value="balance" className="mt-4">
          <TrialBalanceTab />
        </TabsContent>
        <TabsContent value="cuentas" className="mt-4">
          <AccountsTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}

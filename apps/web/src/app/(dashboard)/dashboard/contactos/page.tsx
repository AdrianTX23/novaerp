"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { PartnersTab } from "@/components/partners/partners-tab";
import { PartnerType } from "@/lib/types";

export default function ContactosPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Contactos</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Clientes y proveedores de tu empresa.
      </p>

      <Tabs defaultValue="clientes" className="mt-6">
        <TabsList>
          <TabsTrigger value="clientes">Clientes</TabsTrigger>
          <TabsTrigger value="proveedores">Proveedores</TabsTrigger>
        </TabsList>
        <TabsContent value="clientes" className="mt-4">
          <PartnersTab type={PartnerType.Customer} />
        </TabsContent>
        <TabsContent value="proveedores" className="mt-4">
          <PartnersTab type={PartnerType.Supplier} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

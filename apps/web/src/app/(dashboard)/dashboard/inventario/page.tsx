"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ProductsTab } from "@/components/catalog/products-tab";
import { CategoriesTab } from "@/components/catalog/categories-tab";

export default function InventarioPage() {
  return (
    <div>
      <h1 className="text-2xl font-semibold tracking-tight">Inventario</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Productos, stock y categorías.
      </p>

      <Tabs defaultValue="productos" className="mt-6">
        <TabsList>
          <TabsTrigger value="productos">Productos</TabsTrigger>
          <TabsTrigger value="categorias">Categorías</TabsTrigger>
        </TabsList>
        <TabsContent value="productos" className="mt-4">
          <ProductsTab />
        </TabsContent>
        <TabsContent value="categorias" className="mt-4">
          <CategoriesTab />
        </TabsContent>
      </Tabs>
    </div>
  );
}

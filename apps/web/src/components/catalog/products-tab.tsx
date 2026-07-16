"use client";

import { useState, useDeferredValue } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import { Input } from "@/components/ui/input";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { productsApi } from "@/lib/catalog-api";
import { toastApiError } from "@/lib/api-errors";
import { formatMoney } from "@/lib/utils";
import { ProductFormDialog } from "@/components/catalog/product-form-dialog";
import { AdjustStockDialog } from "@/components/catalog/adjust-stock-dialog";
import { TablePagination } from "@/components/ui/table-pagination";
import { AlertTriangle } from "lucide-react";
import { QueryError } from "@/components/layout/query-error";

const PAGE_SIZE = 50;

export function ProductsTab() {
  const [search, setSearch] = useState("");
  const [lowStockOnly, setLowStockOnly] = useState(false);
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();

  // Difiere el término de búsqueda: sin esto, cada tecla dispara una request
  // nueva al API mientras el usuario todavía está escribiendo.
  const deferredSearch = useDeferredValue(search);

  const productsQuery = useQuery({
    queryKey: ["products", { search: deferredSearch, lowStockOnly, page }],
    queryFn: () => productsApi.list({ search: deferredSearch || undefined, lowStockOnly, page, pageSize: PAGE_SIZE }),
    placeholderData: keepPreviousData,
  });

  const toggleActive = useMutation({
    mutationFn: (vars: { productId: string; isActive: boolean }) =>
      vars.isActive ? productsApi.reactivate(vars.productId) : productsApi.deactivate(vars.productId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["products"] }),
    onError: (error) => {
      toastApiError(error, "No se pudo actualizar el estado.");
    },
  });

  return (
    <div className="grid gap-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <Input
            placeholder="Buscar por nombre o SKU…"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="w-56"
          />
          <Button
            variant={lowStockOnly ? "default" : "outline"}
            size="sm"
            onClick={() => {
              setLowStockOnly((v) => !v);
              setPage(1);
            }}
          >
            <AlertTriangle />
            Bajo stock
          </Button>
        </div>
        <ProductFormDialog />
      </div>

      {productsQuery.isError ? (
        <QueryError error={productsQuery.error} forbiddenMessage="Tu rol no tiene acceso al inventario." />
      ) : productsQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Producto</TableHead>
              <TableHead>Categoría</TableHead>
              <TableHead>Stock</TableHead>
              <TableHead>Precio</TableHead>
              <TableHead>Activo</TableHead>
              <TableHead className="w-24" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {productsQuery.data?.items.map((product) => (
              <TableRow key={product.id}>
                <TableCell className="font-medium">
                  {product.name}
                  <p className="text-muted-foreground text-xs">{product.sku}</p>
                </TableCell>
                <TableCell className="text-muted-foreground">{product.categoryName ?? "—"}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-1.5">
                    <span className="tabular-nums">{product.quantityOnHand} {product.unitOfMeasure}</span>
                    {product.isLowStock && <Badge variant="destructive">Bajo stock</Badge>}
                  </div>
                </TableCell>
                <TableCell className="tabular-nums text-muted-foreground">
                  {formatMoney(product.salePrice)}
                </TableCell>
                <TableCell>
                  <Switch
                    checked={product.isActive}
                    disabled={toggleActive.isPending}
                    onCheckedChange={(checked) =>
                      toggleActive.mutate({ productId: product.id, isActive: checked })
                    }
                  />
                </TableCell>
                <TableCell>
                  <div className="flex items-center gap-1">
                    <AdjustStockDialog product={product} />
                    <ProductFormDialog product={product} />
                  </div>
                </TableCell>
              </TableRow>
            ))}
            {productsQuery.data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-muted-foreground text-center">
                  No hay productos que coincidan.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      )}

      {productsQuery.data && (
        <TablePagination
          page={page}
          pageSize={PAGE_SIZE}
          totalCount={productsQuery.data.totalCount}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}

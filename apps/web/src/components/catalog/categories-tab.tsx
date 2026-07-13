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
import { categoriesApi } from "@/lib/catalog-api";
import { CategoryFormDialog } from "@/components/catalog/category-form-dialog";
import { DeleteCategoryButton } from "@/components/catalog/delete-category-button";

export function CategoriesTab() {
  const categoriesQuery = useQuery({ queryKey: ["categories"], queryFn: categoriesApi.list });

  return (
    <div className="grid gap-4">
      <div className="flex items-center justify-between">
        <p className="text-muted-foreground text-sm">
          {categoriesQuery.data?.length ?? 0} categorías.
        </p>
        <CategoryFormDialog />
      </div>

      {categoriesQuery.isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nombre</TableHead>
              <TableHead>Productos</TableHead>
              <TableHead className="w-20" />
            </TableRow>
          </TableHeader>
          <TableBody>
            {categoriesQuery.data?.map((category) => (
              <TableRow key={category.id}>
                <TableCell className="font-medium">
                  {category.name}
                  {category.description && (
                    <p className="text-muted-foreground text-xs">{category.description}</p>
                  )}
                </TableCell>
                <TableCell className="text-muted-foreground">{category.productCount}</TableCell>
                <TableCell>
                  <div className="flex items-center gap-1">
                    <CategoryFormDialog category={category} />
                    <DeleteCategoryButton categoryId={category.id} />
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}

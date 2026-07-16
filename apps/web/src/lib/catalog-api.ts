import { apiClient } from "@/lib/api-client";
import type { CategoryDto, PagedResult, ProductSummary } from "@/lib/types";

export interface CategoryPayload {
  name: string;
  description: string | null;
}

export interface ProductFilters {
  search?: string;
  categoryId?: string;
  lowStockOnly?: boolean;
  page?: number;
  pageSize?: number;
}

export interface CreateProductPayload {
  sku: string;
  name: string;
  unitOfMeasure: string;
  costPrice: number;
  salePrice: number;
  categoryId: string | null;
  description: string | null;
  reorderPoint: number | null;
  initialQuantity: number;
}

export type UpdateProductPayload = Omit<CreateProductPayload, "sku" | "initialQuantity">;

function buildQuery(filters: ProductFilters): string {
  const params = new URLSearchParams();
  if (filters.search) params.set("search", filters.search);
  if (filters.categoryId) params.set("categoryId", filters.categoryId);
  if (filters.lowStockOnly) params.set("lowStockOnly", "true");
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const categoriesApi = {
  list: () => apiClient.get<CategoryDto[]>("/api/product-categories"),
  create: (payload: CategoryPayload) => apiClient.post<CategoryDto>("/api/product-categories", payload),
  update: (categoryId: string, payload: CategoryPayload) =>
    apiClient.put<CategoryDto>(`/api/product-categories/${categoryId}`, payload),
  delete: (categoryId: string) => apiClient.delete<void>(`/api/product-categories/${categoryId}`),
};

export const productsApi = {
  list: (filters: ProductFilters = {}) =>
    apiClient.get<PagedResult<ProductSummary>>(`/api/products${buildQuery(filters)}`),
  create: (payload: CreateProductPayload) => apiClient.post<ProductSummary>("/api/products", payload),
  update: (productId: string, payload: UpdateProductPayload) =>
    apiClient.put<ProductSummary>(`/api/products/${productId}`, payload),
  adjustStock: (productId: string, delta: number) =>
    apiClient.post<ProductSummary>(`/api/products/${productId}/adjust-stock`, { delta }),
  deactivate: (productId: string) => apiClient.post<ProductSummary>(`/api/products/${productId}/deactivate`),
  reactivate: (productId: string) => apiClient.post<ProductSummary>(`/api/products/${productId}/reactivate`),
};

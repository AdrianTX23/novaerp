import { apiClient } from "@/lib/api-client";
import type { PagedResult, SalesOrderDetail, SalesOrderSummary } from "@/lib/types";

export interface CreateSalesOrderLinePayload {
  productId: string;
  quantity: number;
}

export interface CreateSalesOrderPayload {
  customerId: string;
  orderDate: string;
  notes: string | null;
  lines: CreateSalesOrderLinePayload[];
}

export interface SalesOrderFilters {
  status?: string;
  customerId?: string;
  page?: number;
  pageSize?: number;
}

function buildQuery(filters: SalesOrderFilters): string {
  const params = new URLSearchParams();
  if (filters.status) params.set("status", filters.status);
  if (filters.customerId) params.set("customerId", filters.customerId);
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const salesApi = {
  list: (filters: SalesOrderFilters = {}) =>
    apiClient.get<PagedResult<SalesOrderSummary>>(`/api/sales-orders${buildQuery(filters)}`),
  get: (orderId: string) => apiClient.get<SalesOrderDetail>(`/api/sales-orders/${orderId}`),
  create: (payload: CreateSalesOrderPayload) =>
    apiClient.post<SalesOrderDetail>("/api/sales-orders", payload),
  confirm: (orderId: string) =>
    apiClient.post<SalesOrderDetail>(`/api/sales-orders/${orderId}/confirm`),
  cancel: (orderId: string) =>
    apiClient.post<SalesOrderDetail>(`/api/sales-orders/${orderId}/cancel`),
};

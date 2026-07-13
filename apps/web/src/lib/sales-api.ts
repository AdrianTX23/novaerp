import { apiClient } from "@/lib/api-client";
import type { SalesOrderDetail, SalesOrderSummary } from "@/lib/types";

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
}

function buildQuery(filters: SalesOrderFilters): string {
  const params = new URLSearchParams();
  if (filters.status) params.set("status", filters.status);
  if (filters.customerId) params.set("customerId", filters.customerId);
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const salesApi = {
  list: (filters: SalesOrderFilters = {}) =>
    apiClient.get<SalesOrderSummary[]>(`/api/sales-orders${buildQuery(filters)}`),
  get: (orderId: string) => apiClient.get<SalesOrderDetail>(`/api/sales-orders/${orderId}`),
  create: (payload: CreateSalesOrderPayload) =>
    apiClient.post<SalesOrderDetail>("/api/sales-orders", payload),
  confirm: (orderId: string) =>
    apiClient.post<SalesOrderDetail>(`/api/sales-orders/${orderId}/confirm`),
  cancel: (orderId: string) =>
    apiClient.post<SalesOrderDetail>(`/api/sales-orders/${orderId}/cancel`),
};

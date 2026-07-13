import { apiClient } from "@/lib/api-client";
import type { PurchaseOrderDetail, PurchaseOrderSummary } from "@/lib/types";

export interface CreatePurchaseOrderLinePayload {
  productId: string;
  quantity: number;
}

export interface CreatePurchaseOrderPayload {
  supplierId: string;
  orderDate: string;
  notes: string | null;
  lines: CreatePurchaseOrderLinePayload[];
}

export interface PurchaseOrderFilters {
  status?: string;
  supplierId?: string;
}

function buildQuery(filters: PurchaseOrderFilters): string {
  const params = new URLSearchParams();
  if (filters.status) params.set("status", filters.status);
  if (filters.supplierId) params.set("supplierId", filters.supplierId);
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const purchasingApi = {
  list: (filters: PurchaseOrderFilters = {}) =>
    apiClient.get<PurchaseOrderSummary[]>(`/api/purchase-orders${buildQuery(filters)}`),
  get: (orderId: string) => apiClient.get<PurchaseOrderDetail>(`/api/purchase-orders/${orderId}`),
  create: (payload: CreatePurchaseOrderPayload) =>
    apiClient.post<PurchaseOrderDetail>("/api/purchase-orders", payload),
  confirm: (orderId: string) =>
    apiClient.post<PurchaseOrderDetail>(`/api/purchase-orders/${orderId}/confirm`),
  cancel: (orderId: string) =>
    apiClient.post<PurchaseOrderDetail>(`/api/purchase-orders/${orderId}/cancel`),
};

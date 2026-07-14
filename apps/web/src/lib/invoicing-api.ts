import { apiClient } from "@/lib/api-client";
import type { InvoiceDetail, InvoiceSummary } from "@/lib/types";

export interface CreateInvoicePayload {
  salesOrderId: string;
  dueDate: string | null;
  notes: string | null;
}

export interface RegisterPaymentPayload {
  amount: number;
  paidAt: string;
  method: number;
  reference: string | null;
}

export interface InvoiceFilters {
  status?: string;
  customerId?: string;
}

function buildQuery(filters: InvoiceFilters): string {
  const params = new URLSearchParams();
  if (filters.status) params.set("status", filters.status);
  if (filters.customerId) params.set("customerId", filters.customerId);
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const invoicingApi = {
  list: (filters: InvoiceFilters = {}) =>
    apiClient.get<InvoiceSummary[]>(`/api/invoices${buildQuery(filters)}`),
  get: (invoiceId: string) => apiClient.get<InvoiceDetail>(`/api/invoices/${invoiceId}`),
  create: (payload: CreateInvoicePayload) => apiClient.post<InvoiceDetail>("/api/invoices", payload),
  registerPayment: (invoiceId: string, payload: RegisterPaymentPayload) =>
    apiClient.post<InvoiceDetail>(`/api/invoices/${invoiceId}/payments`, payload),
  void: (invoiceId: string) => apiClient.post<InvoiceDetail>(`/api/invoices/${invoiceId}/void`),
};

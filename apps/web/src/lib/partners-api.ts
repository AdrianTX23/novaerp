import { apiClient } from "@/lib/api-client";
import type { PagedResult, PartnerDto } from "@/lib/types";

export interface PartnerPayload {
  name: string;
  type: number;
  documentNumber: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
}

export interface PartnerFilters {
  type?: number;
  page?: number;
  pageSize?: number;
}

function buildQuery({ type, page, pageSize }: PartnerFilters): string {
  const params = new URLSearchParams();
  if (type) params.set("type", String(type));
  if (page) params.set("page", String(page));
  if (pageSize) params.set("pageSize", String(pageSize));
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const partnersApi = {
  list: (filters: PartnerFilters = {}) =>
    apiClient.get<PagedResult<PartnerDto>>(`/api/partners${buildQuery(filters)}`),
  create: (payload: PartnerPayload) => apiClient.post<PartnerDto>("/api/partners", payload),
  update: (partnerId: string, payload: PartnerPayload) =>
    apiClient.put<PartnerDto>(`/api/partners/${partnerId}`, payload),
  deactivate: (partnerId: string) => apiClient.post<PartnerDto>(`/api/partners/${partnerId}/deactivate`),
  reactivate: (partnerId: string) => apiClient.post<PartnerDto>(`/api/partners/${partnerId}/reactivate`),
};

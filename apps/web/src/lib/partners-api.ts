import { apiClient } from "@/lib/api-client";
import type { PartnerDto } from "@/lib/types";

export interface PartnerPayload {
  name: string;
  type: number;
  documentNumber: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
}

export const partnersApi = {
  list: (type?: number) =>
    apiClient.get<PartnerDto[]>(`/api/partners${type ? `?type=${type}` : ""}`),
  create: (payload: PartnerPayload) => apiClient.post<PartnerDto>("/api/partners", payload),
  update: (partnerId: string, payload: PartnerPayload) =>
    apiClient.put<PartnerDto>(`/api/partners/${partnerId}`, payload),
  deactivate: (partnerId: string) => apiClient.post<PartnerDto>(`/api/partners/${partnerId}/deactivate`),
  reactivate: (partnerId: string) => apiClient.post<PartnerDto>(`/api/partners/${partnerId}/reactivate`),
};

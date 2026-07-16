import { apiClient } from "@/lib/api-client";

export interface CompanyDto {
  id: string;
  name: string;
  slug: string;
  isActive: boolean;
  createdAt: string;
}

export const settingsApi = {
  company: () => apiClient.get<CompanyDto>("/api/settings/company"),
  renameCompany: (name: string) => apiClient.put<CompanyDto>("/api/settings/company", { name }),
};

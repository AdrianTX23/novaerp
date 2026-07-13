import { apiClient } from "@/lib/api-client";
import type { DashboardDto } from "@/lib/types";

export const dashboardApi = {
  get: () => apiClient.get<DashboardDto>("/api/dashboard"),
};

import { apiClient } from "@/lib/api-client";
import type { InventoryReportDto, ReceivablesReportDto, SalesReportDto } from "@/lib/types";

export const reportsApi = {
  sales: (from: string, to: string) =>
    apiClient.get<SalesReportDto>(`/api/reports/sales?from=${from}&to=${to}`),
  inventory: () => apiClient.get<InventoryReportDto>("/api/reports/inventory"),
  receivables: () => apiClient.get<ReceivablesReportDto>("/api/reports/receivables"),
};

import { apiClient } from "@/lib/api-client";
import type { CashMovementDto, CashSummaryDto } from "@/lib/types";

export interface CreateCashMovementPayload {
  type: number;
  amount: number;
  date: string;
  concept: string;
  description: string | null;
}

export const cashApi = {
  summary: () => apiClient.get<CashSummaryDto>("/api/cash/summary"),
  movements: () => apiClient.get<CashMovementDto[]>("/api/cash/movements"),
  create: (payload: CreateCashMovementPayload) =>
    apiClient.post<CashMovementDto>("/api/cash/movements", payload),
  delete: (movementId: string) => apiClient.delete<void>(`/api/cash/movements/${movementId}`),
};

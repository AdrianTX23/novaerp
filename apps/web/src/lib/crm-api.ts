import { apiClient } from "@/lib/api-client";
import type { OpportunityDto, PipelineSummaryDto } from "@/lib/types";

export interface CreateOpportunityPayload {
  customerId: string;
  title: string;
  estimatedValue: number;
  expectedCloseDate: string | null;
  notes: string | null;
}

export const crmApi = {
  opportunities: () => apiClient.get<OpportunityDto[]>("/api/crm/opportunities"),
  pipeline: () => apiClient.get<PipelineSummaryDto>("/api/crm/pipeline"),
  create: (payload: CreateOpportunityPayload) =>
    apiClient.post<OpportunityDto>("/api/crm/opportunities", payload),
  move: (opportunityId: string, stage: number) =>
    apiClient.post<OpportunityDto>(`/api/crm/opportunities/${opportunityId}/move`, { stage }),
};

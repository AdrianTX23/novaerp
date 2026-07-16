import { apiClient } from "@/lib/api-client";

export type AuditAction = "Created" | "Updated" | "Deleted";

export interface FieldChange {
  field: string;
  oldValue: string | null;
  newValue: string | null;
}

export interface AuditLogDto {
  id: string;
  entityName: string;
  entityId: string;
  action: AuditAction;
  userId: string | null;
  userEmail: string | null;
  changes: string | null;
  createdAt: string;
}

export interface AuditLogPageDto {
  items: AuditLogDto[];
  totalCount: number;
}

export interface ListAuditLogsParams {
  entityName?: string;
  from?: string;
  to?: string;
  page: number;
  pageSize: number;
}

export const auditApi = {
  list: ({ entityName, from, to, page, pageSize }: ListAuditLogsParams) => {
    const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
    if (entityName) params.set("entityName", entityName);
    if (from) params.set("from", from);
    if (to) params.set("to", to);
    return apiClient.get<AuditLogPageDto>(`/api/audit?${params.toString()}`);
  },
};

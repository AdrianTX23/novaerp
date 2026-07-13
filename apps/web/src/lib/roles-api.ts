import { apiClient } from "@/lib/api-client";
import type { PermissionDto, RoleDetail } from "@/lib/types";

export interface RolePayload {
  name: string;
  description: string | null;
  permissionCodes: string[];
}

export const rolesApi = {
  list: () => apiClient.get<RoleDetail[]>("/api/roles"),
  listPermissions: () => apiClient.get<PermissionDto[]>("/api/permissions"),
  create: (payload: RolePayload) => apiClient.post<RoleDetail>("/api/roles", payload),
  update: (roleId: string, payload: RolePayload) =>
    apiClient.put<RoleDetail>(`/api/roles/${roleId}`, payload),
  delete: (roleId: string) => apiClient.delete<void>(`/api/roles/${roleId}`),
};

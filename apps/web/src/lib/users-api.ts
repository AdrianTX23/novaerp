import { apiClient } from "@/lib/api-client";
import type { UserSummary } from "@/lib/types";

export interface CreateUserPayload {
  email: string;
  fullName: string;
  password: string;
  roleIds: string[];
}

export const usersApi = {
  list: () => apiClient.get<UserSummary[]>("/api/users"),
  create: (payload: CreateUserPayload) => apiClient.post<UserSummary>("/api/users", payload),
  updateRoles: (userId: string, roleIds: string[]) =>
    apiClient.put<UserSummary>(`/api/users/${userId}/roles`, { roleIds }),
  deactivate: (userId: string) =>
    apiClient.post<UserSummary>(`/api/users/${userId}/deactivate`),
  reactivate: (userId: string) =>
    apiClient.post<UserSummary>(`/api/users/${userId}/reactivate`),
};

import { apiClient } from "@/lib/api-client";
import type { AuthResponse } from "@/lib/types";

export interface RegisterPayload {
  companyName: string;
  fullName: string;
  email: string;
  password: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export const authApi = {
  register: (payload: RegisterPayload) =>
    apiClient.post<AuthResponse>("/api/auth/register", payload),
  login: (payload: LoginPayload) =>
    apiClient.post<AuthResponse>("/api/auth/login", payload),
  logout: () => apiClient.post<void>("/api/auth/logout"),
};

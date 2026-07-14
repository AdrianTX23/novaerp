import { apiClient } from "@/lib/api-client";
import type { AccountDto, JournalEntrySummary, TrialBalanceDto } from "@/lib/types";

export interface CreateAccountPayload {
  code: string;
  name: string;
  type: number;
}

export interface JournalLinePayload {
  accountId: string;
  debit: number;
  credit: number;
}

export interface CreateJournalEntryPayload {
  date: string;
  description: string;
  reference: string | null;
  lines: JournalLinePayload[];
}

export const accountingApi = {
  accounts: () => apiClient.get<AccountDto[]>("/api/accounting/accounts"),
  createAccount: (payload: CreateAccountPayload) =>
    apiClient.post<AccountDto>("/api/accounting/accounts", payload),
  journalEntries: () => apiClient.get<JournalEntrySummary[]>("/api/accounting/journal-entries"),
  createJournalEntry: (payload: CreateJournalEntryPayload) =>
    apiClient.post<unknown>("/api/accounting/journal-entries", payload),
  trialBalance: () => apiClient.get<TrialBalanceDto>("/api/accounting/trial-balance"),
};

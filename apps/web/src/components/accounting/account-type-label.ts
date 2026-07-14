import type { AccountTypeName } from "@/lib/types";

export const ACCOUNT_TYPE_LABEL: Record<AccountTypeName, string> = {
  Asset: "Activo",
  Liability: "Pasivo",
  Equity: "Patrimonio",
  Income: "Ingreso",
  Expense: "Gasto",
};

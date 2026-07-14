export interface AuthUser {
  id: string;
  tenantId: string;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
}

export interface MonthlyPoint {
  month: string;
  total: number;
}

export interface TopProductDto {
  productName: string;
  quantitySold: number;
  revenue: number;
}

export interface DashboardDto {
  salesThisMonth: number;
  salesOrdersThisMonth: number;
  purchasesThisMonth: number;
  lowStockCount: number;
  inventoryValue: number;
  topCustomerName: string | null;
  topCustomerRevenue: number;
  salesTrend: MonthlyPoint[];
  topProducts: TopProductDto[];
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  user: AuthUser;
}

export interface ProblemDetails {
  type?: string;
  title: string;
  status: number;
  errors?: { field: string; error: string }[];
}

export interface RoleRef {
  id: string;
  name: string;
}

export interface UserSummary {
  id: string;
  email: string;
  fullName: string;
  isActive: boolean;
  roles: RoleRef[];
}

export interface RoleDetail {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  userCount: number;
  permissionCodes: string[];
}

export interface PermissionDto {
  id: string;
  code: string;
  description: string;
  group: string;
}

export interface CategoryDto {
  id: string;
  name: string;
  description: string | null;
  productCount: number;
}

export interface ProductSummary {
  id: string;
  sku: string;
  name: string;
  description: string | null;
  categoryId: string | null;
  categoryName: string | null;
  unitOfMeasure: string;
  costPrice: number;
  salePrice: number;
  quantityOnHand: number;
  reorderPoint: number | null;
  isActive: boolean;
  isLowStock: boolean;
}

/** Bitmask: coincide con [Flags] PartnerType del backend (Customer=1, Supplier=2). */
export const PartnerType = {
  Customer: 1,
  Supplier: 2,
} as const;

export interface PartnerDto {
  id: string;
  name: string;
  type: number;
  documentNumber: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  isActive: boolean;
}

/** Coincide con CashMovementType del backend (enum por número). */
export const CashMovementType = {
  Income: 0,
  Expense: 1,
} as const;

export interface CashSummaryDto {
  balance: number;
  totalIncome: number;
  totalExpense: number;
  incomeThisMonth: number;
  expenseThisMonth: number;
}

export interface CashMovementDto {
  id: string;
  kind: "Income" | "Expense";
  amount: number;
  date: string;
  concept: string;
  description: string | null;
  source: "Invoice" | "Manual";
  canDelete: boolean;
}

/** Coincide con InvoiceStatus del backend (string). */
export type InvoiceStatus = "Issued" | "PartiallyPaid" | "Paid" | "Void";

/** Coincide con PaymentMethod del backend (enum por número). */
export const PaymentMethod = {
  Cash: 0,
  Transfer: 1,
  Card: 2,
  Other: 3,
} as const;

export interface InvoiceSummary {
  id: string;
  invoiceNumber: string;
  customerId: string;
  customerName: string;
  status: InvoiceStatus;
  issueDate: string;
  dueDate: string;
  total: number;
  amountPaid: number;
  outstandingBalance: number;
}

export interface InvoiceLineDto {
  productSku: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface PaymentDto {
  id: string;
  amount: number;
  paidAt: string;
  method: string;
  reference: string | null;
}

export interface InvoiceDetail {
  id: string;
  invoiceNumber: string;
  salesOrderId: string;
  customerId: string;
  customerName: string;
  status: InvoiceStatus;
  issueDate: string;
  dueDate: string;
  total: number;
  amountPaid: number;
  outstandingBalance: number;
  notes: string | null;
  lines: InvoiceLineDto[];
  payments: PaymentDto[];
}

/** Coincide con SalesOrderStatus del backend (string). */
export type SalesOrderStatus = "Draft" | "Confirmed" | "Cancelled";

export interface SalesOrderSummary {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  status: SalesOrderStatus;
  orderDate: string;
  totalAmount: number;
  lineCount: number;
}

export interface SalesOrderLineDto {
  productId: string;
  productSku: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface SalesOrderDetail {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  status: SalesOrderStatus;
  orderDate: string;
  notes: string | null;
  totalAmount: number;
  lines: SalesOrderLineDto[];
}

/** Coincide con PurchaseOrderStatus del backend (string). */
export type PurchaseOrderStatus = "Draft" | "Confirmed" | "Cancelled";

export interface PurchaseOrderSummary {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  status: PurchaseOrderStatus;
  orderDate: string;
  totalAmount: number;
  lineCount: number;
}

export interface PurchaseOrderLineDto {
  productId: string;
  productSku: string;
  productName: string;
  quantity: number;
  unitCost: number;
  lineTotal: number;
}

export interface PurchaseOrderDetail {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName: string;
  status: PurchaseOrderStatus;
  orderDate: string;
  notes: string | null;
  totalAmount: number;
  lines: PurchaseOrderLineDto[];
}

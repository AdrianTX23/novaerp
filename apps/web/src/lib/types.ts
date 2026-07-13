export interface AuthUser {
  id: string;
  tenantId: string;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
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

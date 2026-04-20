// Per-vendor product category (menu section). Vendor-owned, flat (no parent/sub).
export interface CategoryAPIResponse {
  id: number;
  vendorId: number;
  name: string;
  description: string | null;
  sortOrder: number | null;
  productsCount: number;
}

export interface CategoryType {
  id: number;
  vendorId: number;
  name: string;
  description: string | null;
  sortOrder: number | null;
  productsCount: number;
}

export function mapCategoryAPIToUI(api: CategoryAPIResponse): CategoryType {
  return {
    id: api.id,
    vendorId: api.vendorId,
    name: api.name,
    description: api.description ?? null,
    sortOrder: api.sortOrder ?? null,
    productsCount: api.productsCount ?? 0,
  };
}

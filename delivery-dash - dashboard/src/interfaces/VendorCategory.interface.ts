// API response shape returned by the backend
export interface VendorCategoryAPIResponse {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  vendorsCount: number;
  createdAt: string;
  updatedAt: string;
}

// UI shape used inside the dashboard
export interface VendorCategoryType {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  vendorsCount: number;
  createdAt: string;
  updatedAt: string;
}

export function mapVendorCategoryAPIToUI(
  api: VendorCategoryAPIResponse
): VendorCategoryType {
  return {
    id: api.id,
    name: api.name,
    description: api.description ?? null,
    isActive: api.isActive,
    vendorsCount: api.vendorsCount ?? 0,
    createdAt: api.createdAt,
    updatedAt: api.updatedAt,
  };
}

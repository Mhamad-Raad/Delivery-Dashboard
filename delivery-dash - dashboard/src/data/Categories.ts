import { axiosInstance } from '@/data/axiosInstance';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

const headers = { key: API_KEY, value: API_VALUE };

function toErrorPayload(error: any) {
  const errorData = error?.response?.data;
  return {
    error: errorData?.error || errorData?.message || error?.message,
    errors: errorData?.errors || [],
  };
}

// Admin-only — fetches a specific vendor's product categories with product counts.
// Backed by: GET /DeliveryDashApi/Category/by-vendor/{vendorId}
export const fetchCategoriesByVendor = async (vendorId: number | string) => {
  try {
    const response = await axiosInstance.get(`/Category/by-vendor/${vendorId}`, {
      headers,
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

// Public — used by the customer app / public browsing.
// Backed by: GET /DeliveryDashApi/Category/by-vendor/{vendorId}/public
export const fetchCategoriesByVendorPublic = async (
  vendorId: number | string
) => {
  try {
    const response = await axiosInstance.get(
      `/Category/by-vendor/${vendorId}/public`,
      { headers }
    );
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

// Paged list with optional vendor filter (admin-wide or staff-own).
export const fetchCategoriesPaged = async (params: {
  page?: number;
  limit?: number;
  vendorId?: number;
  searchName?: string;
}) => {
  try {
    const response = await axiosInstance.get('/Category/paged', {
      headers,
      params,
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

export const fetchCategoryById = async (id: number) => {
  try {
    const response = await axiosInstance.get(`/Category/${id}`, { headers });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

// Back-compat shim: autocomplete callers expected a paged-response shape
// ({ data, total, page, limit }). After the category refactor, categories are
// vendor-scoped — every row in `data` now carries a `vendorId`. Callers that
// need vendor-scoped results should pass `vendorId` explicitly.
export const fetchCategories = async (params?: {
  searchName?: string;
  limit?: number;
  vendorId?: number;
}) => {
  return fetchCategoriesPaged({
    page: 1,
    limit: params?.limit ?? 10,
    searchName: params?.searchName,
    vendorId: params?.vendorId,
  });
};

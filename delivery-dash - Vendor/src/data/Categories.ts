import { axiosInstance } from '@/data/axiosInstance';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

const headers = { key: API_KEY, value: API_VALUE };

export interface CategoryDTO {
  id: number;
  vendorId: number;
  name: string;
  description?: string | null;
  sortOrder?: number | null;
  productsCount: number;
}

function toErrorPayload(error: any) {
  const errorData = error?.response?.data;
  return {
    error: errorData?.error || errorData?.message || error?.message,
    errors: errorData?.errors || [],
  };
}

// Returns the calling vendor's (or vendor-staff's) own product categories.
// Backed by: GET /DeliveryDashApi/Category/mine
export const fetchMyCategories = async () => {
  try {
    const response = await axiosInstance.get('/Category/mine', { headers });
    return response.data as CategoryDTO[];
  } catch (error) {
    return toErrorPayload(error) as any;
  }
};

// Vendor owner only. POST /DeliveryDashApi/Category
export const createCategory = async (body: {
  name: string;
  description?: string | null;
  sortOrder?: number | null;
}) => {
  try {
    const response = await axiosInstance.post('/Category', body, {
      headers: { ...headers, 'Content-Type': 'application/json' },
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

// Vendor owner only. PUT /DeliveryDashApi/Category/{id}
export const updateCategory = async (
  id: number,
  body: {
    name?: string;
    description?: string | null;
    sortOrder?: number | null;
  }
) => {
  try {
    const response = await axiosInstance.put(`/Category/${id}`, body, {
      headers: { ...headers, 'Content-Type': 'application/json' },
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

// Vendor owner only. DELETE /DeliveryDashApi/Category/{id}
// Products under this category have their CategoryId nulled (uncategorised).
export const deleteCategory = async (id: number) => {
  try {
    const response = await axiosInstance.delete(`/Category/${id}`, { headers });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

// Back-compat alias for existing callers (e.g. CreateProduct picker).
// Now fetches the caller's own categories instead of the old global list.
export const fetchCategories = fetchMyCategories;

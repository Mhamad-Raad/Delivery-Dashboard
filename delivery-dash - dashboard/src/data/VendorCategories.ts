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

export const fetchVendorCategories = async (params?: {
  activeOnly?: boolean;
}) => {
  try {
    const response = await axiosInstance.get('/VendorCategory', {
      headers,
      params,
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

export const fetchVendorCategoriesPaged = async (params: {
  page?: number;
  limit?: number;
  searchName?: string;
  activeOnly?: boolean;
}) => {
  try {
    const response = await axiosInstance.get('/VendorCategory/paged', {
      headers,
      params,
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

export const fetchVendorCategoryById = async (id: number) => {
  try {
    const response = await axiosInstance.get(`/VendorCategory/${id}`, {
      headers,
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

export const createVendorCategory = async (body: {
  name: string;
  description?: string | null;
  isActive: boolean;
}) => {
  try {
    const response = await axiosInstance.post('/VendorCategory', body, {
      headers: { ...headers, 'Content-Type': 'application/json' },
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

export const updateVendorCategory = async (
  id: number,
  body: {
    name?: string;
    description?: string | null;
    isActive?: boolean;
  }
) => {
  try {
    const response = await axiosInstance.put(`/VendorCategory/${id}`, body, {
      headers: { ...headers, 'Content-Type': 'application/json' },
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

export const deleteVendorCategory = async (id: number) => {
  try {
    const response = await axiosInstance.delete(`/VendorCategory/${id}`, {
      headers,
    });
    return response.data;
  } catch (error) {
    return toErrorPayload(error);
  }
};

import { axiosInstance } from '@/data/axiosInstance';
import { compressImage } from '@/lib/imageCompression';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export const fetchProducts = async (params?: {
  page?: number;
  limit?: number;
  searchName?: string;
  inStock?: boolean;
}) => {
  try {
    const response = await axiosInstance.get('/Product/my-products', {
      headers: { key: API_KEY, value: API_VALUE },
      params: {
        page: params?.page,
        limit: params?.limit,
        searchName: params?.searchName ?? undefined,
        inStock: params?.inStock ?? undefined,
      },
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const fetchProductById = async (id: number) => {
  try {
    const response = await axiosInstance.get(`/Product/my-products/${id}`, {
      headers: { key: API_KEY, value: API_VALUE },
    });
    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const updateProduct = async (
  id: number,
  productData: {
    CategoryId: number;
    Name: string;
    Description: string;
    Price: number;
    DiscountPrice?: number;
    InStock: boolean;
    IsWeightable: boolean;
    ProductImageUrl?: File | string;
  }
) => {
  try {
    const formData = new FormData();
    for (const [key, value] of Object.entries(productData)) {
      if (value === undefined || value === null) continue;
      if (key === 'ProductImageUrl' && value instanceof File) {
        const compressedFile = await compressImage(value);
        formData.append(key, compressedFile);
      } else if (typeof value === 'boolean') {
        formData.append(key, value ? 'true' : 'false');
      } else {
        formData.append(key, value as any);
      }
    }

    const response = await axiosInstance.put(`/Product/${id}`, formData, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
      },
      transformRequest: [(data) => data],
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const deleteProduct = async (id: number) => {
  try {
    const response = await axiosInstance.delete(`/Product/${id}`, {
      headers: { key: API_KEY, value: API_VALUE },
    });
    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const createProduct = async (productData: {
  CategoryId: number;
  Name: string;
  Description: string;
  Price: number;
  DiscountPrice?: number;
  InStock: boolean;
  IsWeightable: boolean;
  ProductImageUrl?: File | string;
}) => {
  try {
    const formData = new FormData();
    for (const [key, value] of Object.entries(productData)) {
      if (value === undefined || value === null) continue;
      if (key === 'ProductImageUrl' && value instanceof File) {
        const compressedFile = await compressImage(value);
        formData.append(key, compressedFile);
      } else if (typeof value === 'boolean') {
        formData.append(key, value ? 'true' : 'false');
      } else {
        formData.append(key, value as any);
      }
    }

    const response = await axiosInstance.post('/Product', formData, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
      },
      transformRequest: [(data) => data],
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

import { axiosInstance } from '@/data/axiosInstance';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export interface CategoryDTO {
  id: number;
  name: string;
  description?: string;
  parentCategoryId?: number | null;
  parentCategoryName?: string | null;
  subCategoriesCount?: number;
  productsCount?: number;
}

export const fetchCategories = async () => {
  try {
    const response = await axiosInstance.get('/Category', {
      headers: { key: API_KEY, value: API_VALUE },
    });
    return response.data as CategoryDTO[];
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message } as any;
  }
};


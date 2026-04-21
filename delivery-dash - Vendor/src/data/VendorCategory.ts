import { axiosInstance } from '@/data/axiosInstance';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export interface VendorCategoryDTO {
  id: number;
  name: string;
  description?: string | null;
  isActive: boolean;
}

export const fetchActiveVendorCategories = async (): Promise<VendorCategoryDTO[]> => {
  try {
    const response = await axiosInstance.get('/VendorCategory', {
      params: { activeOnly: true },
      headers: { key: API_KEY, value: API_VALUE },
    });
    const data = response.data;
    // Accept either { data: [...] } or [...] shapes
    const arr = Array.isArray(data) ? data : data?.data ?? [];
    return arr.map((c: any) => ({
      id: c.id ?? c.Id,
      name: c.name ?? c.Name,
      description: c.description ?? c.Description ?? null,
      isActive: c.isActive ?? c.IsActive ?? true,
    }));
  } catch (error) {
    console.error('Failed to fetch vendor categories', error);
    return [];
  }
};

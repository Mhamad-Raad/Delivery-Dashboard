import { axiosInstance } from '@/data/axiosInstance';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export interface DashboardStats {
  totalOrders: number;
  pendingOrders: number;
  completedOrders: number;
  totalProducts: number;
  totalRevenue: number;
  todayRevenue: number;
  todayOrders: number;
  dailyStats: Array<{
    day: string;
    orders: number;
    profit: number;
  }>;
}

export const fetchDashboardStats = async (): Promise<DashboardStats | { error: string }> => {
  try {
    const response = await axiosInstance.get('/Vendor/dashboard', {
      headers: { key: API_KEY, value: API_VALUE },
    });
    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

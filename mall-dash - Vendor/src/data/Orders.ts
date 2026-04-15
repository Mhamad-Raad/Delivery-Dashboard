import { axiosInstance } from '@/data/axiosInstance';
import type { Order, OrderStatus } from '@/interfaces/Order.interface';

interface FetchOrdersParams {
  page?: number;
  limit?: number;
  status?: OrderStatus | 'All' | null;
}

interface OrdersResponse {
  data: Order[];
  total: number;
  page: number;
  limit: number;
}

export const fetchOrders = async (
  params: FetchOrdersParams = {},
  signal?: AbortSignal,
): Promise<OrdersResponse | { error: string }> => {
  try {
    const queryParams: Record<string, any> = {
      page: params.page || 1,
      limit: params.limit || 10,
    };

    if (params.status && params.status !== 'All') {
      queryParams.status = params.status;
    }

    const response = await axiosInstance.get<OrdersResponse>('/Order', {
      params: queryParams,
      signal,
    });

    return response.data;
  } catch (error: any) {
    console.error('Error fetching orders:', error);
    return {
      error:
        error.response?.data?.message ||
        error.message ||
        'Failed to fetch orders',
    };
  }
};

export const fetchOrderById = async (
  id: number,
): Promise<Order | { error: string }> => {
  try {
    const response = await axiosInstance.get<Order>(`/Order/${id}`);
    return response.data;
  } catch (error: any) {
    console.error(`Error fetching order ${id}:`, error);
    return {
      error:
        error.response?.data?.message ||
        error.message ||
        'Failed to fetch order',
    };
  }
};

export const updateOrderStatus = async (
  id: number,
  status: number,
): Promise<boolean | { error: string }> => {
  try {
    await axiosInstance.put(
      `/Order/${id}/status`,
      { status },
      {
        headers: {
          'Content-Type': 'application/json',
        },
      },
    );
    return true;
  } catch (error: any) {
    console.error(`Error updating order ${id} status:`, error);
    return {
      error:
        error.response?.data?.message ||
        error.message ||
        'Failed to update order status',
    };
  }
};


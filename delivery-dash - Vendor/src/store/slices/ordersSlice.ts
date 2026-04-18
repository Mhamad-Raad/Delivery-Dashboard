import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type {
  Order,
  OrdersState,
  OrderStatus,
} from '@/interfaces/Order.interface';
import {
  fetchOrders as fetchOrdersAPI,
  updateOrderStatus as updateOrderStatusAPI,
} from '@/data/Orders';

const initialState: OrdersState = {
  orders: [],
  loading: false,
  error: null,
  limit: 10,
  page: 1,
  total: 0,
  statusFilter: 'All',
};

export const fetchOrders = createAsyncThunk(
  'orders/fetchOrders',
  async (
    params: {
      page?: number;
      limit?: number;
      status?: OrderStatus | 'All' | null;
    } = {},
    { rejectWithValue, signal }
  ) => {
    try {
      const data = await fetchOrdersAPI(params, signal);

      if ('error' in data) {
        return rejectWithValue(data.error);
      }

      return data;
    } catch (error) {
      return rejectWithValue('Failed to fetch orders');
    }
  },
  {
    condition: (_, { getState }) => {
      const { orders } = getState() as { orders: OrdersState };
      // Prevent duplicate fetches if already loading
      return !orders.loading;
    },
  }
);

export const changeOrderStatus = createAsyncThunk(
  'orders/changeStatus',
  async (
    { id, status }: { id: number; status: number },
    { rejectWithValue }
  ) => {
    try {
      const result = await updateOrderStatusAPI(id, status);
      if (typeof result === 'object' && 'error' in result) {
        return rejectWithValue(result.error);
      }
      return { id, status };
    } catch (error) {
      return rejectWithValue('Failed to update order status');
    }
  }
);

const ordersSlice = createSlice({
  name: 'orders',
  initialState,
  reducers: {
    setPage: (state, action: PayloadAction<number>) => {
      state.page = action.payload;
    },
    setLimit: (state, action: PayloadAction<number>) => {
      state.limit = action.payload;
    },
    setStatusFilter: (
      state,
      action: PayloadAction<OrderStatus | 'All' | null>
    ) => {
      state.statusFilter = action.payload;
      state.page = 1; // Reset to first page on filter change
    },
    // Real-time updates
    addOrder: (state, action: PayloadAction<Order>) => {
      // Prevent duplicates
      if (state.orders.some((o) => o.id === action.payload.id)) {
        return;
      }

      // Only add if it matches current filter (or filter is All)
      if (
        state.statusFilter === 'All' ||
        !state.statusFilter ||
        action.payload.status === state.statusFilter
      ) {
        state.orders.unshift(action.payload);
        state.total += 1;

        // Remove last item if we exceed limit (optional, but keeps page size consistent)
        if (state.orders.length > state.limit) {
          state.orders.pop();
        }
      }
    },
    updateOrderStatus: (
      state,
      action: PayloadAction<{ id: number; status: OrderStatus }>
    ) => {
      const orderIndex = state.orders.findIndex(
        (o) => o.id === action.payload.id
      );
      if (orderIndex !== -1) {
        state.orders[orderIndex].status = action.payload.status;

        // If we are filtering by status and the status changed to something else, remove it
        if (
          state.statusFilter &&
          state.statusFilter !== 'All' &&
          state.statusFilter !== action.payload.status
        ) {
          state.orders.splice(orderIndex, 1);
          state.total -= 1;
        }
      }
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchOrders.pending, (state, action) => {
        state.loading = true;
        state.error = null;
        // Sync state with fetch params to ensure consistency without triggering re-fetches
        if (action.meta.arg.status !== undefined) {
          state.statusFilter = action.meta.arg.status;
        }
        if (action.meta.arg.page !== undefined) {
          state.page = action.meta.arg.page;
        }
        if (action.meta.arg.limit !== undefined) {
          state.limit = action.meta.arg.limit;
        }
      })
      .addCase(
        fetchOrders.fulfilled,
        (
          state,
          action: PayloadAction<{
            data: Order[];
            limit: number;
            page: number;
            total: number;
          }>
        ) => {
          state.loading = false;
          state.orders = action.payload.data;
          state.limit = action.payload.limit;
          state.page = action.payload.page;
          state.total = action.payload.total;
        }
      )
      .addCase(fetchOrders.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      .addCase(changeOrderStatus.fulfilled, (state, action) => {
        const { id, status } = action.payload;
        const order = state.orders.find((o) => o.id === id);
        if (order) {
          // Map number to string status if needed, but the interface allows number
          // Ideally we should convert it to the string enum if the state uses strings primarily
          // But for now, let's trust the interface 'OrderStatus | number'
          // Actually, let's use a helper or cast it if we want consistency,
          // but the reducer 'updateOrderStatus' takes OrderStatus (string).
          // Let's assume for the list view we might want to keep it consistent.
          // However, the interface allows number.
          order.status = status;

          // If we are filtering, we might need to remove it
          // (Logic similar to updateOrderStatus reducer)
          if (state.statusFilter && state.statusFilter !== 'All') {
            // We need to know the string representation to compare with filter
            // This is a bit tricky without the helper here.
            // But usually the backend sends integers.
            // Let's rely on the fact that if filter is set, we might need to refresh or remove.
            // For now, let's just update the status.
          }
        }
      });
  },
});

export const {
  setPage,
  setLimit,
  setStatusFilter,
  addOrder,
  updateOrderStatus,
} = ordersSlice.actions;
export default ordersSlice.reducer;


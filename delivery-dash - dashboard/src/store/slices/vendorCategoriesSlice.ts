import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';

import type {
  VendorCategoryAPIResponse,
  VendorCategoryType,
} from '@/interfaces/VendorCategory.interface';
import { mapVendorCategoryAPIToUI } from '@/interfaces/VendorCategory.interface';

import {
  fetchVendorCategoriesPaged as fetchVendorCategoriesPagedAPI,
  createVendorCategory as createVendorCategoryAPI,
  updateVendorCategory as updateVendorCategoryAPI,
  deleteVendorCategory as deleteVendorCategoryAPI,
} from '@/data/VendorCategories';

interface VendorCategoriesState {
  items: VendorCategoryType[];
  loading: boolean;
  error: string | null;
  errorDetails: string[];
  limit: number;
  page: number;
  total: number;
  mutating: boolean;
  mutationError: string | null;
  mutationErrors: string[];
}

const initialState: VendorCategoriesState = {
  items: [],
  loading: false,
  error: null,
  errorDetails: [],
  limit: 10,
  page: 1,
  total: 0,
  mutating: false,
  mutationError: null,
  mutationErrors: [],
};

export const fetchVendorCategories = createAsyncThunk(
  'vendorCategories/fetchPaged',
  async (
    params: {
      page?: number;
      limit?: number;
      searchName?: string;
      activeOnly?: boolean;
    } = {},
    { rejectWithValue }
  ) => {
    try {
      const data = await fetchVendorCategoriesPagedAPI(params);
      if (data.error) {
        return rejectWithValue({ error: data.error, errors: data.errors || [] });
      }
      return data;
    } catch {
      return rejectWithValue({
        error: 'Failed to fetch vendor categories',
        errors: [],
      });
    }
  }
);

export const createVendorCategory = createAsyncThunk(
  'vendorCategories/create',
  async (
    body: { name: string; description?: string | null; isActive: boolean },
    { rejectWithValue }
  ) => {
    const data = await createVendorCategoryAPI(body);
    if (data.error) {
      return rejectWithValue({ error: data.error, errors: data.errors || [] });
    }
    return data as VendorCategoryAPIResponse;
  }
);

export const updateVendorCategory = createAsyncThunk(
  'vendorCategories/update',
  async (
    args: {
      id: number;
      body: { name?: string; description?: string | null; isActive?: boolean };
    },
    { rejectWithValue }
  ) => {
    const data = await updateVendorCategoryAPI(args.id, args.body);
    if (data.error) {
      return rejectWithValue({ error: data.error, errors: data.errors || [] });
    }
    return data as VendorCategoryAPIResponse;
  }
);

export const deleteVendorCategory = createAsyncThunk(
  'vendorCategories/delete',
  async (id: number, { rejectWithValue }) => {
    const data = await deleteVendorCategoryAPI(id);
    if (data && data.error) {
      return rejectWithValue({ error: data.error, errors: data.errors || [] });
    }
    return id;
  }
);

const vendorCategoriesSlice = createSlice({
  name: 'vendorCategories',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
      state.errorDetails = [];
    },
    clearMutationError: (state) => {
      state.mutationError = null;
      state.mutationErrors = [];
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchVendorCategories.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.errorDetails = [];
      })
      .addCase(
        fetchVendorCategories.fulfilled,
        (
          state,
          action: PayloadAction<{
            data: VendorCategoryAPIResponse[];
            limit: number;
            page: number;
            total: number;
          }>
        ) => {
          state.loading = false;
          state.items = action.payload.data.map(mapVendorCategoryAPIToUI);
          state.limit = action.payload.limit;
          state.page = action.payload.page;
          state.total = action.payload.total;
        }
      )
      .addCase(fetchVendorCategories.rejected, (state, action) => {
        state.loading = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.error = payload?.error || 'An error occurred';
        state.errorDetails = payload?.errors || [];
      })

      .addCase(createVendorCategory.pending, (state) => {
        state.mutating = true;
        state.mutationError = null;
        state.mutationErrors = [];
      })
      .addCase(createVendorCategory.fulfilled, (state, action) => {
        state.mutating = false;
        state.items = [mapVendorCategoryAPIToUI(action.payload), ...state.items];
        state.total += 1;
      })
      .addCase(createVendorCategory.rejected, (state, action) => {
        state.mutating = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.mutationError = payload?.error || 'Failed to create';
        state.mutationErrors = payload?.errors || [];
      })

      .addCase(updateVendorCategory.pending, (state) => {
        state.mutating = true;
        state.mutationError = null;
        state.mutationErrors = [];
      })
      .addCase(updateVendorCategory.fulfilled, (state, action) => {
        state.mutating = false;
        const updated = mapVendorCategoryAPIToUI(action.payload);
        state.items = state.items.map((i) => (i.id === updated.id ? updated : i));
      })
      .addCase(updateVendorCategory.rejected, (state, action) => {
        state.mutating = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.mutationError = payload?.error || 'Failed to update';
        state.mutationErrors = payload?.errors || [];
      })

      .addCase(deleteVendorCategory.pending, (state) => {
        state.mutating = true;
        state.mutationError = null;
        state.mutationErrors = [];
      })
      .addCase(deleteVendorCategory.fulfilled, (state, action) => {
        state.mutating = false;
        state.items = state.items.filter((i) => i.id !== action.payload);
        state.total = Math.max(0, state.total - 1);
      })
      .addCase(deleteVendorCategory.rejected, (state, action) => {
        state.mutating = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.mutationError = payload?.error || 'Failed to delete';
        state.mutationErrors = payload?.errors || [];
      });
  },
});

export const { clearError, clearMutationError } = vendorCategoriesSlice.actions;
export default vendorCategoriesSlice.reducer;

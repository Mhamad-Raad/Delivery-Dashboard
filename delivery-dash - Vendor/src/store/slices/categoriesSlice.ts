import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';

import {
  fetchMyCategories as fetchMyCategoriesAPI,
  createCategory as createCategoryAPI,
  updateCategory as updateCategoryAPI,
  deleteCategory as deleteCategoryAPI,
  type CategoryDTO,
} from '@/data/Categories';

interface CategoriesState {
  items: CategoryDTO[];
  loading: boolean;
  error: string | null;
  errorDetails: string[];
  mutating: boolean;
  mutationError: string | null;
  mutationErrors: string[];
}

const initialState: CategoriesState = {
  items: [],
  loading: false,
  error: null,
  errorDetails: [],
  mutating: false,
  mutationError: null,
  mutationErrors: [],
};

export const fetchMyCategories = createAsyncThunk(
  'categories/fetchMine',
  async (_: void, { rejectWithValue }) => {
    const data = await fetchMyCategoriesAPI();
    if ((data as any)?.error) {
      return rejectWithValue({
        error: (data as any).error,
        errors: (data as any).errors || [],
      });
    }
    return data as CategoryDTO[];
  }
);

export const createCategory = createAsyncThunk(
  'categories/create',
  async (
    body: {
      name: string;
      description?: string | null;
      sortOrder?: number | null;
    },
    { rejectWithValue }
  ) => {
    const data = await createCategoryAPI(body);
    if (data?.error) {
      return rejectWithValue({ error: data.error, errors: data.errors || [] });
    }
    return data as CategoryDTO;
  }
);

export const updateCategory = createAsyncThunk(
  'categories/update',
  async (
    args: {
      id: number;
      body: {
        name?: string;
        description?: string | null;
        sortOrder?: number | null;
      };
    },
    { rejectWithValue }
  ) => {
    const data = await updateCategoryAPI(args.id, args.body);
    if (data?.error) {
      return rejectWithValue({ error: data.error, errors: data.errors || [] });
    }
    return data as CategoryDTO;
  }
);

export const deleteCategory = createAsyncThunk(
  'categories/delete',
  async (id: number, { rejectWithValue }) => {
    const data = await deleteCategoryAPI(id);
    if (data && data.error) {
      return rejectWithValue({ error: data.error, errors: data.errors || [] });
    }
    return id;
  }
);

const categoriesSlice = createSlice({
  name: 'categories',
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
      .addCase(fetchMyCategories.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.errorDetails = [];
      })
      .addCase(
        fetchMyCategories.fulfilled,
        (state, action: PayloadAction<CategoryDTO[]>) => {
          state.loading = false;
          state.items = action.payload;
        }
      )
      .addCase(fetchMyCategories.rejected, (state, action) => {
        state.loading = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.error = payload?.error || 'An error occurred';
        state.errorDetails = payload?.errors || [];
      })

      .addCase(createCategory.pending, (state) => {
        state.mutating = true;
        state.mutationError = null;
        state.mutationErrors = [];
      })
      .addCase(createCategory.fulfilled, (state, action) => {
        state.mutating = false;
        state.items = [action.payload, ...state.items];
      })
      .addCase(createCategory.rejected, (state, action) => {
        state.mutating = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.mutationError = payload?.error || 'Failed to create';
        state.mutationErrors = payload?.errors || [];
      })

      .addCase(updateCategory.pending, (state) => {
        state.mutating = true;
        state.mutationError = null;
        state.mutationErrors = [];
      })
      .addCase(updateCategory.fulfilled, (state, action) => {
        state.mutating = false;
        state.items = state.items.map((i) =>
          i.id === action.payload.id ? action.payload : i
        );
      })
      .addCase(updateCategory.rejected, (state, action) => {
        state.mutating = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.mutationError = payload?.error || 'Failed to update';
        state.mutationErrors = payload?.errors || [];
      })

      .addCase(deleteCategory.pending, (state) => {
        state.mutating = true;
        state.mutationError = null;
        state.mutationErrors = [];
      })
      .addCase(deleteCategory.fulfilled, (state, action) => {
        state.mutating = false;
        state.items = state.items.filter((i) => i.id !== action.payload);
      })
      .addCase(deleteCategory.rejected, (state, action) => {
        state.mutating = false;
        const payload = action.payload as
          | { error: string; errors: string[] }
          | undefined;
        state.mutationError = payload?.error || 'Failed to delete';
        state.mutationErrors = payload?.errors || [];
      });
  },
});

export const { clearError, clearMutationError } = categoriesSlice.actions;
export default categoriesSlice.reducer;

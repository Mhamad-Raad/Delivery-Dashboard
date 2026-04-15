import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { RequestsState } from '@/interfaces/Request.interface';
import { fetchMockRequests, fetchMockRequestById } from '@/data/Requests';

const initialState: RequestsState = {
  requests: [],
  selectedRequest: null,
  loading: false,
  error: null,
  total: 0,
  page: 1,
  limit: 10,
};

export const fetchRequests = createAsyncThunk(
  'requests/fetchRequests',
  async (
    params: {
      page?: number;
      limit?: number;
      search?: string;
      status?: string;
    } = {},
    { rejectWithValue }
  ) => {
    try {
      const { page = 1, limit = 10, search = '', status = 'All' } = params;
      const response = await fetchMockRequests(page, limit, search, status);
      return { ...response, page, limit };
    } catch (error: any) {
      return rejectWithValue(error.message || 'Failed to fetch requests');
    }
  }
);

export const fetchRequestDetails = createAsyncThunk(
  'requests/fetchRequestDetails',
  async (id: string, { rejectWithValue }) => {
    try {
      const response = await fetchMockRequestById(id);
      return response;
    } catch (error: any) {
      return rejectWithValue(
        error.message || 'Failed to fetch request details'
      );
    }
  }
);

const requestsSlice = createSlice({
  name: 'requests',
  initialState,
  reducers: {
    clearSelectedRequest: (state) => {
      state.selectedRequest = null;
    },
    setPage: (state, action: PayloadAction<number>) => {
      state.page = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch Requests
      .addCase(fetchRequests.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchRequests.fulfilled, (state, action) => {
        state.loading = false;
        state.requests = action.payload.data;
        state.total = action.payload.total;
        state.page = action.payload.page;
        state.limit = action.payload.limit;
      })
      .addCase(fetchRequests.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Fetch Request Details
      .addCase(fetchRequestDetails.pending, (state) => {
        state.loading = true;
        state.error = null;
        state.selectedRequest = null;
      })
      .addCase(fetchRequestDetails.fulfilled, (state, action) => {
        state.loading = false;
        state.selectedRequest = action.payload;
      })
      .addCase(fetchRequestDetails.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearSelectedRequest, setPage } = requestsSlice.actions;
export default requestsSlice.reducer;


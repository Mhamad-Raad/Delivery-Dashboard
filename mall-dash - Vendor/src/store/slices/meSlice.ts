import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { MeType } from '@/interfaces/Auth.interface';
import { updateMyProfile } from '@/data/Users';
import { fetchMe as fetchMeData } from '@/data/Authorization';

import type { PayloadAction } from '@reduxjs/toolkit';

interface MeState {
  user: MeType | null;
  loading: boolean;
  error: string | null;
}

// Try to load from localStorage on initialization
const loadUserFromStorage = (): MeType | null => {
  try {
    const storedUser = localStorage.getItem('me');
    return storedUser ? JSON.parse(storedUser) : null;
  } catch (error) {
    console.error('Failed to load user from storage:', error);
    return null;
  }
};

const initialState: MeState = {
  user: loadUserFromStorage(),
  loading: false,
  error: null,
};

export const fetchMe = createAsyncThunk(
  'me/fetch',
  async (_, { rejectWithValue }) => {
    const response = await fetchMeData();
    if (response.error) {
      return rejectWithValue(response.error);
    }
    return response.user;
  }
);

export const updateMe = createAsyncThunk(
  'me/update',
  async (
    userData: {
      FirstName: string;
      LastName: string;
      Email: string;
      PhoneNumber: string;
      ProfileImageUrl?: File;
    },
    { rejectWithValue }
  ) => {
    const response = await updateMyProfile(userData);
    if (response.error) {
      return rejectWithValue(response.error);
    }
    return response;
  }
);

const meSlice = createSlice({
  name: 'me',
  initialState,
  reducers: {
    setMe: (state, action: PayloadAction<MeType>) => {
      state.user = action.payload;
      localStorage.setItem('me', JSON.stringify(action.payload));
      state.error = null;
    },
    clearMe: (state) => {
      state.user = null;
      localStorage.removeItem('me');
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch Me
      .addCase(fetchMe.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchMe.fulfilled, (state, action) => {
        state.loading = false;
        state.user = action.payload ?? null;
        localStorage.setItem('me', JSON.stringify(action.payload));
      })
      .addCase(fetchMe.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Update Me
      .addCase(updateMe.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateMe.fulfilled, (state, action) => {
        state.loading = false;
        // Assuming the API returns the updated user object
        // We merge it with existing user to be safe, or replace it
        if (state.user) {
          state.user = { ...state.user, ...action.payload };
          localStorage.setItem('me', JSON.stringify(state.user));
        }
      })
      .addCase(updateMe.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { setMe, clearMe } = meSlice.actions;
export default meSlice.reducer;


import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { VendorProfileType } from '@/interfaces/Auth.interface';
import { fetchMyVendorProfile, updateMyVendorProfile } from '@/data/Vendor';

interface VendorState {
  profile: VendorProfileType | null;
  loading: boolean;
  error: string | null;
}

// Try to load from localStorage on initialization
const loadVendorFromStorage = (): VendorProfileType | null => {
  try {
    const storedVendor = localStorage.getItem('vendorProfile');
    return storedVendor ? JSON.parse(storedVendor) : null;
  } catch (error) {
    console.error('Failed to load vendor profile from storage:', error);
    return null;
  }
};

const initialState: VendorState = {
  profile: loadVendorFromStorage(),
  loading: false,
  error: null,
};

export const getVendorProfile = createAsyncThunk(
  'vendor/getProfile',
  async (_, { rejectWithValue }) => {
    const response = await fetchMyVendorProfile();
    if (response.error) {
      return rejectWithValue(response.error);
    }
    return response;
  }
);

export const updateVendor = createAsyncThunk(
  'vendor/update',
  async (
    vendorData: {
      Name: string;
      Description: string;
      OpeningTime: string;
      CloseTime: string;
      Type: string;
      ProfileImageUrl?: File;
    },
    { rejectWithValue }
  ) => {
    const response = await updateMyVendorProfile(vendorData);
    if (response.error) {
      return rejectWithValue(response.error);
    }
    return response;
  }
);

const vendorSlice = createSlice({
  name: 'vendor',
  initialState,
  reducers: {
    setVendorProfile: (state, action: PayloadAction<VendorProfileType>) => {
      state.profile = action.payload;
      localStorage.setItem('vendorProfile', JSON.stringify(action.payload));
      state.error = null;
    },
    clearVendorProfile: (state) => {
      state.profile = null;
      localStorage.removeItem('vendorProfile');
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Get Profile
      .addCase(getVendorProfile.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(getVendorProfile.fulfilled, (state, action) => {
        state.loading = false;
        state.profile = action.payload;
        localStorage.setItem('vendorProfile', JSON.stringify(action.payload));
      })
      .addCase(getVendorProfile.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      })
      // Update Profile
      .addCase(updateVendor.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateVendor.fulfilled, (state, action) => {
        state.loading = false;
        // Merge updated data
        if (state.profile) {
            // Note: API might return slightly different structure, adjust if needed
            // For now assuming it returns the updated object or we can merge known fields
             state.profile = { ...state.profile, ...action.payload };
             localStorage.setItem('vendorProfile', JSON.stringify(state.profile));
        }
      })
      .addCase(updateVendor.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export const { setVendorProfile, clearVendorProfile } = vendorSlice.actions;
export default vendorSlice.reducer;


import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { UserType } from '@/interfaces/Users.interface';
import {
  fetchUserById as fetchUserByIdAPI,
  updateVendorStaff as updateVendorStaffAPI,
  deleteVendorStaff as deleteVendorStaffAPI,
} from '@/data/Users';
import { initialUser } from '@/constants/Users';

interface UserState {
  user: UserType;
  luser: boolean;
  euser: string | null;
  updating: boolean;
  updatingError: string | null;
  deleting: boolean;
  deletingError: string | null;
}

const initialState: UserState = {
  user: initialUser,
  luser: false,
  euser: null,
  updating: false,
  updatingError: null,
  deleting: false,
  deletingError: null,
};

export const fetchUserById = createAsyncThunk(
  'user/fetchUserById',
  async (userId: string, { rejectWithValue }) => {
    const data = await fetchUserByIdAPI(userId);
    if (data.error) return rejectWithValue(data.error);
    return data;
  }
);

export const updateUser = createAsyncThunk(
  'user/updateUser',
  async (
    {
      id,
      update,
    }: {
      id: string | number;
      update: {
        firstName: string;
        lastName: string;
        email: string;
        phoneNumber: string;
        role: number | string;
        imageFile?: File;
        buildingName?: string;
        isActive?: boolean;
      };
    },
    { rejectWithValue }
  ) => {
    // Transform parameters to match API requirements
    const apiPayload = {
      FirstName: update.firstName,
      LastName: update.lastName,
      Email: update.email,
      PhoneNumber: update.phoneNumber,
      Role: update.role.toString(),
      IsActive: update.isActive ?? true, // Default to true if undefined
      ProfileImageUrl: update.imageFile,
    };

    const data = await updateVendorStaffAPI(id, apiPayload);
    if (data.error) return rejectWithValue(data.error);
    return data;
  }
);

export const deleteUser = createAsyncThunk(
  'user/deleteUser',
  async (id: string | number, { rejectWithValue }) => {
    const data = await deleteVendorStaffAPI(id);
    if (data.error) return rejectWithValue(data.error);
    return data;
  }
);

const userSlice = createSlice({
  name: 'user',
  initialState,
  reducers: {
    clearUser: (state) => {
      state.user = initialUser;
      state.euser = null;
      state.updating = false;
      state.updatingError = null;
      state.deleting = false;
      state.deletingError = null;
    },
    clearError: (state) => {
      state.euser = null;
      state.updatingError = null;
      state.deletingError = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch user
    builder
      .addCase(fetchUserById.pending, (state) => {
        state.luser = true;
        state.euser = null;
      })
      .addCase(
        fetchUserById.fulfilled,
        (state, action: PayloadAction<UserType>) => {
          state.luser = false;
          state.user = action.payload;
          state.euser = null;
        }
      )
      .addCase(fetchUserById.rejected, (state, action) => {
        state.luser = false;
        state.euser = (action.payload as string) || 'An error occurred';
      });

    // Update user
    builder
      .addCase(updateUser.pending, (state) => {
        state.updating = true;
        state.updatingError = null;
      })
      .addCase(
        updateUser.fulfilled,
        (state, action: PayloadAction<UserType>) => {
          state.updating = false;
          state.user = action.payload;
          state.updatingError = null;
        }
      )
      .addCase(updateUser.rejected, (state, action) => {
        state.updating = false;
        state.updatingError = (action.payload as string) || 'An error occurred';
      });

    // Delete user
    builder
      .addCase(deleteUser.pending, (state) => {
        state.deleting = true;
        state.deletingError = null;
      })
      .addCase(deleteUser.fulfilled, (state) => {
        state.deleting = false;
        state.user = initialUser; // Clear user after deletion
        state.deletingError = null;
      })
      .addCase(deleteUser.rejected, (state, action) => {
        state.deleting = false;
        state.deletingError = (action.payload as string) || 'An error occurred';
      });
  },
});

export const { clearUser, clearError } = userSlice.actions;
export default userSlice.reducer;

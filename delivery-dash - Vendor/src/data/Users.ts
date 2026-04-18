import { axiosInstance } from '@/data/axiosInstance';
import { compressImage } from '@/lib/imageCompression';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export const StaffRole = {
  Staff: 0,
  Driver: 1,
} as const;

export type StaffRole = (typeof StaffRole)[keyof typeof StaffRole];

export const fetchStaff = async (params?: {
  page?: number;
  limit?: number;
  isActive?: boolean;
  searchTerm?: string;
}) => {
  try {
    const response = await axiosInstance.get('/VendorStaff/my-staff', {
      headers: { key: API_KEY, value: API_VALUE },
      params,
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const fetchUsers = async (params?: {
  page?: number;
  limit?: number;
  searchTerm?: string;
  role?: number;
  buildingNameSearch?: string;
}) => {
  try {
    const response = await axiosInstance.get('/Account/users', {
      headers: { key: API_KEY, value: API_VALUE },
      params,
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const fetchUserById = async (id: string) => {
  try {
    const response = await axiosInstance.get(`/VendorStaff/${id}`, {
      headers: { key: API_KEY, value: API_VALUE },
    });
    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const updateVendorStaff = async (
  id: string | number,
  staffData: {
    FirstName: string;
    LastName: string;
    Email: string;
    PhoneNumber: string;
    Role: string; // Updated to match API (string)
    IsActive: boolean;
    ProfileImageUrl?: File | string;
  }
) => {
  try {
    const formData = new FormData();
    formData.append('FirstName', staffData.FirstName);
    formData.append('LastName', staffData.LastName);
    formData.append('Email', staffData.Email);
    formData.append('PhoneNumber', staffData.PhoneNumber);
    formData.append('Role', staffData.Role);
    formData.append('IsActive', String(staffData.IsActive));

    if (staffData.ProfileImageUrl instanceof File) {
      const compressedFile = await compressImage(staffData.ProfileImageUrl);
      formData.append('ProfileImageUrl', compressedFile);
    }

    const response = await axiosInstance.put(`/VendorStaff/${id}`, formData, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
      },
      transformRequest: [(data) => data],
    });

    return response.data;
  } catch (error: any) {
    console.error(
      'Update vendor staff failed:',
      error.response?.data || error.message
    );
    return { error: error.response?.data?.message || error.message };
  }
};

export const updateMyProfile = async (userData: {
  FirstName: string;
  LastName: string;
  Email: string;
  PhoneNumber: string;
  ProfileImageUrl?: File;
}) => {
  try {
    const formData = new FormData();
    formData.append('FirstName', userData.FirstName);
    formData.append('LastName', userData.LastName);
    formData.append('Email', userData.Email);
    formData.append('PhoneNumber', userData.PhoneNumber);

    if (userData.ProfileImageUrl) {
      const compressedFile = await compressImage(userData.ProfileImageUrl);
      formData.append('ProfileImageUrl', compressedFile);
    }

    const response = await axiosInstance.put('/Account/me', formData, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
      },
      transformRequest: [(data) => data],
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const updateUser = async (
  id: string,
  userData: {
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string;
    role: number;
    ProfileImageUrl?: File;
  }
) => {
  try {
    const formData = new FormData();
    for (const [key, value] of Object.entries(userData)) {
      if (value !== undefined) {
        if (key === 'ProfileImageUrl' && value instanceof File) {
          const compressedFile = await compressImage(value);
          formData.append(key, compressedFile);
        } else {
          formData.append(key, value as any);
        }
      }
    }

    // Create a custom config to override the default Content-Type
    const response = await axiosInstance.put(`/Account/${id}`, formData, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
      },
      transformRequest: [(data) => data],
    });

    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

// Delete vendor staff by ID (DELETE request)
export const deleteVendorStaff = async (id: string | number) => {
  try {
    const response = await axiosInstance.delete(`/VendorStaff/${id}`, {
      headers: { key: API_KEY, value: API_VALUE },
    });
    console.log(`Vendor staff ${id} deleted successfully`);
    return response.data;
  } catch (error: any) {
    console.error(
      `Delete vendor staff ${id} failed:`,
      error.response?.data || error.message
    );
    return { error: error.response?.data?.message || error.message };
  }
};

// Delete user by ID (DELETE request)
export const deleteUser = async (id: string) => {
  try {
    const response = await axiosInstance.delete(`/Account/${id}`, {
      headers: { key: API_KEY, value: API_VALUE },
    });
    return response.data;
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const createUser = async (userData: {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber: string;
  role: number;
  ProfileImageUrl?: File;
}) => {
  try {
    // Check if we have a file to upload
    const hasFile = userData.ProfileImageUrl instanceof File;

    if (hasFile) {
      // Use FormData for file upload
      const formData = new FormData();
      for (const [key, value] of Object.entries(userData)) {
        if (value !== undefined) {
          if (key === 'ProfileImageUrl' && value instanceof File) {
            const compressedFile = await compressImage(value);
            formData.append(key, compressedFile);
            console.log(
              `FormData appended: ${key} = File: ${compressedFile.name} (compressed)`
            );
          } else {
            formData.append(key, value as any);
            console.log(
              `FormData appended: ${key} =`,
              value instanceof File ? `File: ${value.name}` : value
            );
          }
        }
      }

      console.log('Creating user with image...');

      const response = await axiosInstance.post('/Account/register', formData, {
        headers: {
          key: API_KEY,
          value: API_VALUE,
          // Don't set Content-Type - let browser handle it for multipart/form-data
        },
        // This transformer prevents axios from setting the default Content-Type
        transformRequest: [(data) => data],
      });

      console.log('User created successfully with image:', response.data);
      return response.data;
    } else {
      // Use JSON for text-only data
      console.log('Creating user without image...');

      const response = await axiosInstance.post('/Account/register', userData, {
        headers: {
          key: API_KEY,
          value: API_VALUE,
          'Content-Type': 'application/json',
        },
      });

      console.log('User created successfully:', response.data);
      return response.data;
    }
  } catch (error: any) {
    console.error('Create user failed:', error.response?.data || error.message);
    return { error: error.response?.data?.message || error.message };
  }
};

export const createVendorStaff = async (staffData: {
  FirstName: string;
  LastName: string;
  Email: string;
  PhoneNumber: string;
  Password: string;
  Role: number;
  ProfileImageUrl?: File | string;
}) => {
  try {
    const formData = new FormData();
    formData.append('FirstName', staffData.FirstName);
    formData.append('LastName', staffData.LastName);
    formData.append('Email', staffData.Email);
    formData.append('PhoneNumber', staffData.PhoneNumber);
    formData.append('Password', staffData.Password);
    formData.append('Role', staffData.Role.toString());

    if (staffData.ProfileImageUrl) {
      if (staffData.ProfileImageUrl instanceof File) {
        const compressedFile = await compressImage(staffData.ProfileImageUrl);
        formData.append('ProfileImageUrl', compressedFile);
      } else {
        formData.append('ProfileImageUrl', staffData.ProfileImageUrl);
      }
    }

    const response = await axiosInstance.post('/VendorStaff', formData, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
      },
      transformRequest: [(data) => data],
    });

    return response.data;
  } catch (error: any) {
    console.error(
      'Create vendor staff failed:',
      error.response?.data || error.message
    );
    return { error: error.response?.data?.message || error.message };
  }
};

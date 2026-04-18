import { axiosInstance } from '@/data/axiosInstance';
import { compressImage } from '@/lib/imageCompression';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export const fetchMyVendorProfile = async () => {
  try {
    const response = await axiosInstance.get('/Vendor/me', {
      headers: { key: API_KEY, value: API_VALUE },
    });
    const data = response.data;

    // Map PascalCase from API to camelCase for internal use
    return {
      id: data.Id || data.id,
      name: data.Name || data.name,
      description: data.Description || data.description,
      openingTime: data.OpeningTime || data.openingTime,
      closeTime: data.CloseTime || data.closeTime,
      type: data.Type || data.type,
      profileImageUrl: data.ProfileImageUrl || data.profileImageUrl,
      // Preserve other fields if they exist
      ...data,
    };
  } catch (error: any) {
    return { error: error.response?.data?.message || error.message };
  }
};

export const updateMyVendorProfile = async (vendorData: {
  Name: string;
  Description: string;
  OpeningTime: string; // "HH:mm:ss"
  CloseTime: string; // "HH:mm:ss"
  Type: string;
  ProfileImageUrl?: File;
}) => {
  try {
    const formData = new FormData();
    formData.append('Name', vendorData.Name);
    formData.append('Description', vendorData.Description);
    formData.append('OpeningTime', vendorData.OpeningTime);
    formData.append('CloseTime', vendorData.CloseTime);
    formData.append('Type', vendorData.Type);

    if (vendorData.ProfileImageUrl) {
      const compressedFile = await compressImage(vendorData.ProfileImageUrl);
      formData.append('ProfileImageUrl', compressedFile);
    }

    const response = await axiosInstance.put('/Vendor/profile', formData, {
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


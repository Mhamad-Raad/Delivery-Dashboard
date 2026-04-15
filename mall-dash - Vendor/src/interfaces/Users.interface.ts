export interface UserType {
  id?: number | string;
  userId?: string | number;
  _id?: string; // Optional for backward compatibility
  firstName: string;
  lastName: string;
  role: string | number; // Support both string and number
  profileImageUrl: string;
  phone?: string;
  phoneNumber?: string; // Support both naming conventions
  email: string;
  buildingName?: string;
  isActive?: boolean;
  // Helper fields for UI
  src?: string; // Alias for profileImageUrl (for backward compatibility)
  fallback?: string; // Generated initials for avatar fallback
}

// Extended type for form data with optional image file
export interface UserFormData extends UserType {
  imageFile?: File;
}

export interface UsersType {
  users: UserType[];
}

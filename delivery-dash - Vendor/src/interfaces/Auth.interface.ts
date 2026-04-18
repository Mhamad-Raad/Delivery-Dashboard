export interface MeType {
  _id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  profileImageUrl: string | null;
  role: number;
}

export interface VendorProfileType {
  id: number;
  name: string;
  description: string;
  profileImageUrl: string;
  userProfileImageUrl: string | null;
  openingTime: string;
  closeTime: string;
  type: string;
  userId: string;
  phone: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
  user: MeType;
  vendorProfile: VendorProfileType;
}

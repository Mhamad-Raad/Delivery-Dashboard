export interface ProductType {
  id: number;
  name: string;
  price: number;
  discountPrice?: number | null;
  inStock: boolean;
  isWeightable: boolean;
  productImageUrl?: string | null;
  vendorId: number;
  vendorName: string;
  categoryId?: number | null;
  categoryName?: string | null;
  // Only available in ProductDetailResponse
  description?: string;
  // Legacy fields for backward compatibility
  _id?: string;
  imageUrl?: string;
  src?: string;
}

// Extended type for form data with optional image file
export interface ProductFormData extends ProductType {
  imageFile?: File;
}

export interface ProductsType {
  products: ProductType[];
}

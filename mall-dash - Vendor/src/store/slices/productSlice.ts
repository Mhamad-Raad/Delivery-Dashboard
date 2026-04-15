import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import type { ProductType } from '@/interfaces/Products.interface';
import {
  fetchProductById as fetchProductByIdAPI,
  updateProduct as updateProductAPI,
  deleteProduct as deleteProductAPI,
} from '@/data/Products';

const initialProduct: ProductType = {
  id: 0,
  name: '',
  price: 0,
  inStock: false,
  isWeightable: false,
  vendorId: 0,
  vendorName: '',
};

interface ProductState {
  product: ProductType;
  lproduct: boolean;
  eproduct: string | null;
  updating: boolean;
  updatingError: string | null;
  deleting: boolean;
  deletingError: string | null;
}

const initialState: ProductState = {
  product: initialProduct,
  lproduct: false,
  eproduct: null,
  updating: false,
  updatingError: null,
  deleting: false,
  deletingError: null,
};

export const fetchProductById = createAsyncThunk(
  'product/fetchProductById',
  async (productId: string, { rejectWithValue }) => {
    const id = parseInt(productId, 10);
    const data = await fetchProductByIdAPI(id);
    if ((data as any)?.error) return rejectWithValue((data as any).error);
    return data;
  }
);

export const updateProduct = createAsyncThunk(
  'product/updateProduct',
  async (
    {
      id,
      update,
    }: {
      id: string;
      update: {
        CategoryId: number;
        Name: string;
        Description: string;
        Price: number;
        DiscountPrice?: number;
        InStock: boolean;
        IsWeightable: boolean;
        imageFile?: File;
        ProductImageUrl?: string;
      };
    },
    { rejectWithValue }
  ) => {
    const apiPayload: any = {
      CategoryId: update.CategoryId,
      Name: update.Name,
      Description: update.Description,
      Price: update.Price,
      InStock: update.InStock,
      IsWeightable: update.IsWeightable,
    };
    if (typeof update.DiscountPrice === 'number')
      apiPayload.DiscountPrice = update.DiscountPrice;
    if (update.imageFile instanceof File) {
      apiPayload.ProductImageUrl = update.imageFile;
    } else if (update.ProductImageUrl) {
      apiPayload.ProductImageUrl = update.ProductImageUrl;
    }

    const data = await updateProductAPI(parseInt(id, 10), apiPayload);
    if ((data as any)?.error) return rejectWithValue((data as any).error);
    return data;
  }
);

export const deleteProduct = createAsyncThunk(
  'product/deleteProduct',
  async (id: string, { rejectWithValue }) => {
    const data = await deleteProductAPI(parseInt(id, 10));
    if ((data as any)?.error) return rejectWithValue((data as any).error);
    return data;
  }
);

const productSlice = createSlice({
  name: 'product',
  initialState,
  reducers: {
    clearProduct: (state) => {
      state.product = initialProduct;
      state.eproduct = null;
      state.updating = false;
      state.updatingError = null;
      state.deleting = false;
      state.deletingError = null;
    },
    clearError: (state) => {
      state.eproduct = null;
      state.updatingError = null;
      state.deletingError = null;
    },
  },
  extraReducers: (builder) => {
    // Fetch product
    builder
      .addCase(fetchProductById.pending, (state) => {
        state.lproduct = true;
        state.eproduct = null;
      })
      .addCase(
        fetchProductById.fulfilled,
        (state, action: PayloadAction<any>) => {
          state.lproduct = false;
          const p = action.payload;
          state.product = {
            id: p?.id ?? 0,
            name: p?.name ?? '',
            description: p?.description ?? '',
            price:
              typeof p?.price === 'number' ? p.price : Number(p?.price ?? 0),
            discountPrice:
              typeof p?.discountPrice === 'number' ? p.discountPrice : null,
            inStock: Boolean(p?.inStock),
            isWeightable: Boolean(p?.isWeightable),
            productImageUrl: p?.productImageUrl ?? null,
            vendorId: p?.vendorId ?? 0,
            vendorName: p?.vendorName ?? '',
            categoryId: p?.categoryId ?? null,
            categoryName: p?.categoryName ?? null,
          };
          state.eproduct = null;
        }
      )
      .addCase(fetchProductById.rejected, (state, action) => {
        state.lproduct = false;
        state.eproduct = (action.payload as string) || 'An error occurred';
      });

    // Update product
    builder
      .addCase(updateProduct.pending, (state) => {
        state.updating = true;
        state.updatingError = null;
      })
      .addCase(
        updateProduct.fulfilled,
        (state, action: PayloadAction<ProductType>) => {
          state.updating = false;
          state.product = action.payload;
          state.updatingError = null;
        }
      )
      .addCase(updateProduct.rejected, (state, action) => {
        state.updating = false;
        state.updatingError = (action.payload as string) || 'An error occurred';
      });

    // Delete product
    builder
      .addCase(deleteProduct.pending, (state) => {
        state.deleting = true;
        state.deletingError = null;
      })
      .addCase(deleteProduct.fulfilled, (state) => {
        state.deleting = false;
        state.product = initialProduct;
        state.deletingError = null;
      })
      .addCase(deleteProduct.rejected, (state, action) => {
        state.deleting = false;
        state.deletingError = (action.payload as string) || 'An error occurred';
      });
  },
});

export const { clearProduct, clearError } = productSlice.actions;
export default productSlice.reducer;

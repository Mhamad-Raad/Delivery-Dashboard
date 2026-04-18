import { useState, useEffect, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import ProductDetailHeader from '@/components/Products/ProductDetail/ProductDetailHeader';
import ProductDetailFooter from '@/components/Products/ProductDetail/ProductDetailFooter';
import ProductInfoCard from '@/components/Products/ProductDetail/ProductInfoCard';
import ProductDetailSkeleton from '@/components/Products/ProductDetail/ProductDetailSkeleton';
import ProductErrorCard from '@/components/Products/ProductDetail/ProductErrorCard';
import ConfirmModal, {
  type ChangeDetail,
} from '@/components/ui/Modals/ConfirmModal';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '@/store/store';
import {
  fetchProductById,
  clearProduct,
  updateProduct,
  deleteProduct,
} from '@/store/slices/productSlice';
import { toast } from 'sonner';
import { fetchCategories, type CategoryDTO } from '@/data/Categories';

import type { ProductFormData } from '@/interfaces/Products.interface';

const ProductDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();
   const { t } = useTranslation();

  const [showUpdateModal, setShowUpdateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const {
    product,
    lproduct: loading,
    eproduct: error,
    updatingError,
    deleting,
    deletingError,
  } = useSelector((state: RootState) => state.product);

  const [formData, setFormData] = useState<ProductFormData>({
    id: 0,
    name: '',
    description: '',
    price: 0,
    discountPrice: null,
    inStock: false,
    isWeightable: false,
    productImageUrl: '',
    vendorId: 0,
    vendorName: '',
    categoryId: undefined,
    categoryName: '',
    imageFile: undefined,
  });
  const [categories, setCategories] = useState<CategoryDTO[]>([]);

  // Track if any changes have been made
  const hasChanges = useMemo(() => {
    if (!product || !product.id) return false;

    // Check if image file was added
    if (formData.imageFile instanceof File) return true;

    // Check other fields for changes
    return (
      product.name !== formData.name ||
      product.description !== formData.description ||
      product.price !== formData.price ||
      (typeof product.discountPrice === 'number'
        ? product.discountPrice
        : null) !==
        (typeof formData.discountPrice === 'number'
          ? formData.discountPrice
          : null) ||
      Boolean(product.inStock) !== Boolean(formData.inStock) ||
      Boolean(product.isWeightable) !== Boolean(formData.isWeightable) ||
      (product.categoryId ?? undefined) !== formData.categoryId
    );
  }, [product, formData]);

  const changes = useMemo<ChangeDetail[]>(() => {
    if (!product) return [];
    const changesList: ChangeDetail[] = [];
    const fieldLabels: Record<string, string> = {
      name: t('products.detailNameLabel'),
      description: t('products.detailDescriptionLabel'),
      price: t('products.detailRegularPriceLabel'),
      discountPrice: t('products.detailDiscountPriceLabel'),
      inStock: t('products.detailInStockLabel'),
      isWeightable: t('products.detailWeightableLabel'),
      categoryId: t('products.detailCategoryLabel'),
      productImageUrl: t('products.detailProductImageTitle'),
    };

    // Check for image change
    if (formData.imageFile instanceof File) {
      changesList.push({
        field: t('products.detailProductImageTitle'),
        oldValue: product.productImageUrl
          ? t('products.detailImageCurrent')
          : t('products.detailImageNone'),
        newValue: formData.imageFile.name,
      });
    }

    // Check other fields
    (
      [
        'name',
        'description',
        'price',
        'discountPrice',
        'inStock',
        'isWeightable',
        'categoryId',
        'productImageUrl',
      ] as const
    ).forEach((key) => {
      if (product[key] !== formData[key]) {
        let oldVal = product[key];
        let newVal = formData[key];
        if (key === 'price') {
          const oldNum =
            typeof oldVal === 'number' ? oldVal : Number(oldVal ?? 0);
          const newNum =
            typeof newVal === 'number' ? newVal : Number(newVal ?? 0);
          oldVal = `$${oldNum.toFixed(2)}`;
          newVal = `$${newNum.toFixed(2)}`;
        } else if (key === 'discountPrice') {
          const oldNum =
            oldVal == null
              ? null
              : typeof oldVal === 'number'
              ? oldVal
              : Number(oldVal);
          const newNum =
            newVal == null
              ? null
              : typeof newVal === 'number'
              ? newVal
              : Number(newVal);
          oldVal = oldNum == null ? '—' : `$${oldNum.toFixed(2)}`;
          newVal = newNum == null ? '—' : `$${newNum.toFixed(2)}`;
        } else if (key === 'inStock' || key === 'isWeightable') {
          oldVal = Boolean(oldVal) ? t('common.yes') : t('common.no');
          newVal = Boolean(newVal) ? t('common.yes') : t('common.no');
        } else if (key === 'categoryId') {
          oldVal = product.categoryName ?? String(oldVal ?? '');
          newVal = String(newVal ?? '');
        }
        changesList.push({
          field: fieldLabels[key],
          oldValue: String(oldVal ?? ''),
          newValue: String(newVal ?? ''),
        });
      }
    });
    return changesList;
  }, [product, formData]);

  const handleInputChange = (
    field: keyof ProductFormData,
    value: string | number | File | boolean
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  useEffect(() => {
    if (id) {
      dispatch(fetchProductById(id));
    }
    return () => {
      dispatch(clearProduct());
    };
  }, [id, dispatch]);

  useEffect(() => {
    const run = async () => {
      const res = await fetchCategories();
      if ((res as any)?.error) {
        toast.error(t('products.detailCategoriesLoadFailed'));
      } else {
        setCategories(res as CategoryDTO[]);
      }
    };
    run();
  }, []);

  useEffect(() => {
    if (product && product.id) {
      setFormData({
        ...product,
        name: product.name ?? '',
        description: product.description ?? '',
        price: product.price ?? 0,
        discountPrice: product.discountPrice ?? null,
        inStock: Boolean(product.inStock),
        isWeightable: Boolean(product.isWeightable),
        productImageUrl: product.productImageUrl ?? '',
        vendorId: product.vendorId ?? 0,
        vendorName: product.vendorName ?? '',
        categoryId: product.categoryId,
        categoryName: product.categoryName ?? '',
        imageFile: undefined,
      });
    }
  }, [product]);

  useEffect(() => {
    if (updatingError) {
      toast.error(updatingError);
    }
  }, [updatingError]);

  useEffect(() => {
    if (deletingError) {
      toast.error(deletingError);
    }
    if (!deleting && showDeleteModal && !deletingError) {
      setShowDeleteModal(false);
      toast.success(t('products.detailDeleteSuccess'));
      navigate('/products');
    }
  }, [deleting, deletingError, navigate]);

  const handletoggleUpdateModal = () => {
    if (!hasChanges) return;
    setShowUpdateModal((v) => !v);
  };
  const handletoggleDeleteModal = () => setShowDeleteModal((v) => !v);

  const handleUpdateProduct = async () => {
    if (!hasChanges) return;
    const updatePayload = {
      CategoryId: formData.categoryId ?? 0,
      Name: formData.name,
      Description: formData.description ?? '',
      Price: Number(formData.price),
      DiscountPrice:
        typeof formData.discountPrice === 'number'
          ? formData.discountPrice
          : undefined,
      InStock: Boolean(formData.inStock),
      IsWeightable: Boolean(formData.isWeightable),
      imageFile: formData.imageFile,
      ProductImageUrl:
        !formData.imageFile && formData.productImageUrl
          ? formData.productImageUrl
          : undefined,
    };
    try {
      await dispatch(
        updateProduct({ id: id || String(product.id), update: updatePayload })
      ).unwrap();
      setShowUpdateModal(false);
      toast.success(t('products.detailUpdateSuccess'));
      navigate('/products');
    } catch (err: any) {
      toast.error(err?.message || t('products.detailUpdateFailed'));
    }
  };

  const handleDeleteProduct = async () => {
    await dispatch(deleteProduct(id || String(product.id)));
  };

  if (error) return <ProductErrorCard error={error} />;
  if (loading) return <ProductDetailSkeleton />;

  return (
    <div className='flex flex-col min-h-full gap-3'>
      <ProductDetailHeader
        onBack={() => navigate(-1)}
        productName={product?.name}
      />
      <ProductInfoCard
        formData={formData}
        categories={categories}
        onInputChange={handleInputChange}
      />
      <ProductDetailFooter
        onSave={handletoggleUpdateModal}
        onDelete={handletoggleDeleteModal}
        hasChanges={hasChanges}
      />
      <ConfirmModal
        open={showUpdateModal}
        title={t('products.detailUpdateTitle')}
        description={t('products.detailUpdateDescription')}
        userName={product.name}
        confirmType='warning'
        confirmLabel={t('common.saveChanges')}
        cancelLabel={t('common.cancel')}
        onCancel={handletoggleUpdateModal}
        onConfirm={handleUpdateProduct}
        changes={changes}
      />
      <ConfirmModal
        open={showDeleteModal}
        title={t('products.detailDeleteTitle')}
        description={t('products.detailDeleteDescription')}
        userName={product.name}
        warning={t('products.detailDeleteWarning')}
        confirmType='danger'
        confirmLabel={t('common.delete')}
        cancelLabel={t('common.cancel')}
        onCancel={handletoggleDeleteModal}
        onConfirm={handleDeleteProduct}
      />
    </div>
  );
};

export default ProductDetail;

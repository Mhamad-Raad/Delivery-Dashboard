import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

import {
  Package as PackageIcon,
  DollarSign,
  CheckCircle2,
  XCircle,
  Tag,
} from 'lucide-react';

import { ScrollArea } from '@/components/ui/scroll-area';

import ProductsCardsSkeleton from './ProductsCardsSkeleton';
import CustomTablePagination from '../CustomTablePagination';

import type { RootState } from '@/store/store';

const ProductsCards = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const {
    products,
    lproducts: loading,
    eproducts: error,
    total,
  } = useSelector((state: RootState) => state.products);

  const handleCardClick = (productId: string) => {
    navigate(`/products/${productId}`);
  };

  if (error) {
    return (
      <div className='rounded-2xl border bg-card shadow-sm p-8'>
        <div className='text-center text-destructive'>
          {t('common.errorTitle')}: {error}
        </div>
      </div>
    );
  }

  return (
    <div className='rounded-2xl border bg-card/50 backdrop-blur-sm shadow-sm flex flex-col overflow-hidden'>
      {/* Scrollable cards area */}
      <ScrollArea className='h-[calc(100vh-380px)] md:h-[calc(100vh-380px)]'>
        <div className='p-4 grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3'>
          {loading
            ? Array.from({ length: 12 }).map((_, index) => (
                <ProductsCardsSkeleton key={`skeleton-${index}`} />
              ))
            : products.map((product) => {
                const productImage = product.productImageUrl;
                const isInStock = product.inStock !== false;
                const hasDiscount =
                  product.discountPrice && product.discountPrice < product.price;

                return (
                  <div
                    key={product.id}
                    className='group relative bg-card border rounded-xl overflow-hidden shadow-sm hover:shadow-lg transition-all duration-200 cursor-pointer hover:border-primary/40'
                    onClick={() => handleCardClick(String(product.id))}
                  >
                    {/* Product Image */}
                    <div className='relative aspect-square w-full overflow-hidden bg-gradient-to-br from-muted to-muted/50'>
                      {productImage ? (
                        <img
                          src={productImage}
                          alt={product.name}
                          className='h-full w-full object-cover transition-transform duration-300 group-hover:scale-105'
                          onError={(e) => {
                            (e.target as HTMLImageElement).style.display =
                              'none';
                            const parent = (e.target as HTMLImageElement)
                              .parentElement;
                            if (parent) {
                              const fallback = document.createElement('div');
                              fallback.className =
                                'h-full w-full flex items-center justify-center bg-gradient-to-br from-primary/20 to-primary/5 text-primary';
                              fallback.innerHTML =
                                '<svg class="h-10 w-10 opacity-50" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" /></svg>';
                              parent.appendChild(fallback);
                            }
                          }}
                        />
                      ) : (
                        <div className='h-full w-full flex items-center justify-center bg-gradient-to-br from-primary/20 to-primary/5 text-primary'>
                          <PackageIcon className='h-10 w-10 opacity-50' />
                        </div>
                      )}

                      {/* Stock Badge - Top Left */}
                      <div className='absolute top-2 left-2'>
                        {isInStock ? (
                          <div className='flex items-center gap-1 bg-emerald-500/90 text-white text-[10px] font-medium px-1.5 py-0.5 rounded-md shadow-sm'>
                            <CheckCircle2 className='size-2.5' />
                            <span className='hidden sm:inline'>
                              {t('products.statusInStock')}
                            </span>
                          </div>
                        ) : (
                          <div className='flex items-center gap-1 bg-destructive/90 text-white text-[10px] font-medium px-1.5 py-0.5 rounded-md shadow-sm'>
                            <XCircle className='size-2.5' />
                            <span className='hidden sm:inline'>
                              {t('products.statusOutOfStock')}
                            </span>
                          </div>
                        )}
                      </div>

                      {/* Discount Badge - Top Right */}
                      {hasDiscount && (
                        <div className='absolute top-2 right-2'>
                          <div className='flex items-center gap-0.5 bg-amber-500 text-white text-[10px] font-bold px-1.5 py-0.5 rounded-md shadow-sm'>
                            <Tag className='size-2.5' />
                            {Math.round(
                              ((product.price - (product.discountPrice || 0)) /
                                product.price) *
                                100
                            )}
                            %
                          </div>
                        </div>
                      )}
                    </div>

                    {/* Product Info */}
                    <div className='p-2.5 space-y-1.5'>
                      {/* Product Name & Category */}
                      <div className='flex items-start justify-between gap-1.5'>
                        <h3 className='font-medium text-sm leading-tight line-clamp-2 group-hover:text-primary transition-colors flex-1'>
                          {product.name}
                        </h3>
                        {product.categoryName && (
                          <span className='text-[9px] font-medium bg-muted text-muted-foreground px-1.5 py-0.5 rounded shrink-0 mt-0.5'>
                            {product.categoryName}
                          </span>
                        )}
                      </div>

                      {/* Price */}
                      <div className='flex items-center gap-1.5 pt-1'>
                        <div className='flex items-center'>
                          <DollarSign className='h-3.5 w-3.5 text-emerald-600 dark:text-emerald-400' />
                          <span className='text-sm font-bold text-foreground'>
                            {hasDiscount
                              ? product.discountPrice?.toFixed(2)
                              : product.price.toFixed(2)}
                          </span>
                        </div>
                        {hasDiscount && (
                          <span className='text-[10px] text-muted-foreground line-through'>
                            ${product.price.toFixed(2)}
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
        </div>
      </ScrollArea>

      {/* Pagination */}
      <div className='border-t px-4 py-3 bg-muted/20'>
        <CustomTablePagination total={total} suggestions={[12, 24, 36, 48]} />
      </div>
    </div>
  );
};

export default ProductsCards;

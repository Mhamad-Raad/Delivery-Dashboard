import { useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

import {
  Package as PackageIcon,
  ChevronRight,
  DollarSign,
} from 'lucide-react';

import { ScrollArea, ScrollBar } from '@/components/ui/scroll-area';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';

import ProductsTableSkeleton from './ProductsTableSkeleton';
import CustomTablePagination from '../CustomTablePagination';

import type { RootState } from '@/store/store';

const ProductsTable = () => {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const {
    products,
    lproducts: loading,
    eproducts: error,
    total,
  } = useSelector((state: RootState) => state.products);

  const handleRowClick = (productId: string) => {
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
    <div className='rounded-xl border bg-card shadow-sm flex flex-col overflow-hidden'>
      {/* Scrollable table area - responsive height based on viewport */}
      <ScrollArea className='h-[calc(100vh-280px)] md:h-[calc(100vh-280px)]'>
        <Table className='w-full min-w-[700px]'>
          <TableHeader>
            <TableRow className='hover:bg-transparent border-b bg-muted/50'>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-12'>
                {t('products.tableProduct')}
              </TableHead>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-12'>
                {t('products.tableCategory')}
              </TableHead>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-12'>
                {t('products.tableStatus')}
              </TableHead>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-12'>
                {t('products.tablePrice')}
              </TableHead>
              <TableHead className='sticky top-0 z-10 w-12 bg-muted/50 backdrop-blur-sm border-b h-12'></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading
              ? Array.from({ length: 5 }).map((_, index) => (
                  <ProductsTableSkeleton key={`skeleton-${index}`} />
                ))
              : products.map((product, index) => {
                  const productImage = product.productImageUrl;
                  const isInStock = product.inStock !== false;
                  const hasDiscount =
                    product.discountPrice && product.discountPrice < product.price;

                  return (
                    <TableRow
                      key={`${product?.id}-${index}`}
                      className='group hover:bg-muted/50 transition-all cursor-pointer border-b last:border-0'
                      onClick={() => handleRowClick(String(product?.id))}
                    >
                      {/* Product Info with Image */}
                      <TableCell className='font-medium py-4'>
                        <div className='flex items-center gap-3'>
                          <div className='relative h-11 w-11 rounded-lg overflow-hidden border-2 border-border shadow-sm transition-all group-hover:shadow-md group-hover:border-primary/50 shrink-0'>
                            {productImage ? (
                              <img
                                src={productImage}
                                alt={product.name}
                                className='h-full w-full object-cover'
                                onError={(e) => {
                                  (e.target as HTMLImageElement).style.display =
                                    'none';
                                  const parent = (e.target as HTMLImageElement)
                                    .parentElement;
                                  if (parent) {
                                    const fallback =
                                      document.createElement('div');
                                    fallback.className =
                                      'h-full w-full flex items-center justify-center bg-gradient-to-br from-primary/20 to-primary/10 text-primary';
                                    fallback.innerHTML =
                                      '<svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" /></svg>';
                                    parent.appendChild(fallback);
                                  }
                                }}
                              />
                            ) : (
                              <div className='h-full w-full flex items-center justify-center bg-gradient-to-br from-primary/20 to-primary/10 text-primary'>
                                <PackageIcon className='h-5 w-5' />
                              </div>
                            )}
                          </div>
                          <div className='flex flex-col gap-0.5'>
                            <span className='font-semibold text-sm leading-tight group-hover:text-primary transition-colors'>
                              {product.name}
                            </span>
                            <span className='text-[11px] text-muted-foreground font-mono leading-tight'>
                              #{product?.id}
                            </span>
                          </div>
                        </div>
                      </TableCell>

                      {/* Category */}
                      <TableCell className='py-4'>
                        {product.categoryName ? (
                          <Badge
                            variant='outline'
                            className='bg-primary/10 text-primary border-primary/30 font-semibold text-xs px-3 py-1'
                          >
                            {product.categoryName}
                          </Badge>
                        ) : (
                          <span className='text-xs text-muted-foreground'>—</span>
                        )}
                      </TableCell>

                      {/* Stock Status */}
                      <TableCell className='py-4'>
                        <Badge
                          variant='outline'
                          className={`font-semibold text-xs px-3 py-1 ${
                            isInStock
                              ? 'bg-emerald-500/10 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-400 border-emerald-500/30 dark:border-emerald-500/40'
                              : 'bg-red-500/10 text-red-700 dark:bg-red-500/20 dark:text-red-400 border-red-500/30 dark:border-red-500/40'
                          }`}
                        >
                          {isInStock
                            ? t('products.statusInStock')
                            : t('products.statusOutOfStock')}
                        </Badge>
                      </TableCell>

                      {/* Price */}
                      <TableCell className='py-4'>
                        <div className='flex items-center gap-2.5 min-w-[120px]'>
                          <div className='flex items-center justify-center w-7 h-7 rounded-lg bg-muted group-hover:bg-primary/10 transition-colors'>
                            <DollarSign className='h-4 w-4 text-muted-foreground group-hover:text-primary transition-colors' />
                          </div>
                          <div className='flex flex-col gap-0.5'>
                            <span className='text-sm font-medium text-foreground/90'>
                              ${hasDiscount
                                ? product.discountPrice?.toFixed(2)
                                : product.price.toFixed(2)}
                            </span>
                            {hasDiscount && (
                              <span className='text-[11px] text-muted-foreground line-through leading-tight'>
                                ${product.price.toFixed(2)}
                              </span>
                            )}
                          </div>
                        </div>
                      </TableCell>

                      {/* Chevron indicator */}
                      <TableCell className='py-4 w-12'>
                        <ChevronRight className='h-4 w-4 text-muted-foreground group-hover:text-primary group-hover:translate-x-1 transition-all' />
                      </TableCell>
                    </TableRow>
                  );
                })}
          </TableBody>
        </Table>
        <ScrollBar orientation='horizontal' />
      </ScrollArea>

      {/* Pagination */}
      <div className='border-t px-4 py-3 bg-muted/20'>
        <CustomTablePagination
          total={total}
          suggestions={[10, 20, 40, 50, 100]}
        />
      </div>
    </div>
  );
};

export default ProductsTable;

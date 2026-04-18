import { useEffect, useState } from 'react';

import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

import type { AppDispatch, RootState } from '@/store/store';

import ProductsFilters from '@/components/Products/ProductsFilters';
import ProductsTable from '@/components/Products/ProductsTable';
import ProductsCards from '@/components/Products/ProductsCards';
import EmptyState from '@/components/Products/EmptyState';

import { fetchProducts } from '@/store/slices/productsSlice';

import { Card, CardContent } from '@/components/ui/card';
import { Package, PackageCheck, PackageX } from 'lucide-react';

const Products = () => {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const dispatch = useDispatch<AppDispatch>();
  const [viewMode, setViewMode] = useState<'table' | 'cards'>('table');

  const {
    products,
    lproducts: loading,
    eproducts: error,
    total,
  } = useSelector((state: RootState) => state.products);

  const limit = parseInt(searchParams.get('limit') || '40', 10);
  const page = parseInt(searchParams.get('page') || '1', 10);
  const searchName = searchParams.get('searchName') || '';
  const inStockParam = searchParams.get('inStock');
  const inStock = inStockParam !== null ? inStockParam === 'true' : null;

  useEffect(() => {
    const params: Record<string, any> = {
      limit,
      page,
    };
    if (searchName) params.searchName = searchName;
    if (typeof inStock === 'boolean') params.inStock = inStock;

    if (limit && page) {
      dispatch(fetchProducts(params));
    }
  }, [dispatch, limit, page, searchName, inStock]);

  const hasNoProducts = !loading && products.length === 0 && !error;

  // Calculate stats
  const inStockCount = products.filter((p) => p.inStock !== false).length;
  const outOfStockCount = products.filter((p) => p.inStock === false).length;

  const statsCards = [
    {
      title: t('products.statsTotalProducts'),
      value: total || products.length,
      icon: Package,
      iconColor: 'text-blue-600 dark:text-blue-400',
    },
    {
      title: t('products.statsInStock'),
      value: inStockCount,
      icon: PackageCheck,
      iconColor: 'text-emerald-600 dark:text-emerald-400',
    },
    {
      title: t('products.statsOutOfStock'),
      value: outOfStockCount,
      icon: PackageX,
      iconColor: 'text-rose-600 dark:text-rose-400',
    },
  ];

  return (
    <section className='w-full h-full flex flex-col gap-6 overflow-hidden'>
      {/* Filters Section */}
      <ProductsFilters viewMode={viewMode} onViewModeChange={setViewMode} />

      {/* Stats Cards */}
      {!loading && products.length > 0 && (
        <div className='grid grid-cols-1 sm:grid-cols-3 gap-4'>
          {statsCards.map((stat) => (
            <Card key={stat.title}>
              <CardContent className='p-4'>
                <div className='flex items-center justify-between'>
                  <div className='space-y-1'>
                    <p className='text-xs font-medium text-muted-foreground uppercase tracking-wider'>
                      {stat.title}
                    </p>
                    <p className='text-2xl font-bold tracking-tight'>
                      {stat.value}
                    </p>
                  </div>
                  <div className='p-2.5 rounded-xl bg-muted'>
                    <stat.icon className={`h-5 w-5 ${stat.iconColor}`} />
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Products View OR Empty State */}
      <div className='flex-1 min-h-0'>
        {hasNoProducts ? (
          <EmptyState />
        ) : viewMode === 'table' ? (
          <ProductsTable />
        ) : (
          <ProductsCards />
        )}
      </div>
    </section>
  );
};

export default Products;

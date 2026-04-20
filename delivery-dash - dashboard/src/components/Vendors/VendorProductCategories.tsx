import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Layers, Package, FolderOpen, Sparkles } from 'lucide-react';

import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';

import { fetchCategoriesByVendor } from '@/data/Categories';
import {
  mapCategoryAPIToUI,
  type CategoryAPIResponse,
  type CategoryType,
} from '@/interfaces/Category.interface';

interface Props {
  vendorId: string | number;
}

export default function VendorProductCategories({ vendorId }: Props) {
  const { t } = useTranslation('vendors');

  const [items, setItems] = useState<CategoryType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      setError(null);
      const res = await fetchCategoriesByVendor(vendorId);
      if (cancelled) return;

      if (res && res.error) {
        setError(res.error);
        setItems([]);
      } else if (Array.isArray(res)) {
        setItems((res as CategoryAPIResponse[]).map(mapCategoryAPIToUI));
      }
      setLoading(false);
    };

    load();
    return () => {
      cancelled = true;
    };
  }, [vendorId]);

  const totalProducts = items.reduce((sum, c) => sum + c.productsCount, 0);

  return (
    <Card className='border-2 overflow-hidden'>
      {/* Gradient accent strip */}
      <div className='h-1.5 bg-gradient-to-r from-primary via-primary/70 to-primary' />

      <CardContent className='p-5 md:p-6 space-y-5'>
        {/* Header */}
        <div className='flex flex-col md:flex-row md:items-center md:justify-between gap-3'>
          <div className='flex items-center gap-3'>
            <div className='relative flex-shrink-0'>
              <div className='absolute inset-0 bg-primary/20 rounded-xl blur-sm' />
              <div className='relative p-2.5 rounded-xl bg-gradient-to-br from-primary/20 to-primary/5 border border-primary/20 text-primary'>
                <Layers className='size-5' />
              </div>
            </div>
            <div>
              <div className='flex items-center gap-2'>
                <h3 className='text-lg font-bold tracking-tight'>
                  {t('productCategories.title')}
                </h3>
                <Sparkles className='size-4 text-amber-500' />
              </div>
              <p className='text-sm text-muted-foreground'>
                {t('productCategories.subtitle')}
              </p>
            </div>
          </div>

          {!loading && !error && items.length > 0 && (
            <div className='flex items-center gap-2'>
              <Badge
                variant='outline'
                className='bg-primary/10 text-primary border-primary/30 font-bold px-2.5 py-1 gap-1.5'
              >
                <FolderOpen className='size-3' />
                {t('productCategories.totalCategories', { count: items.length })}
              </Badge>
              <Badge
                variant='outline'
                className='bg-emerald-500/10 text-emerald-700 dark:text-emerald-300 border-emerald-500/30 font-bold px-2.5 py-1 gap-1.5'
              >
                <Package className='size-3' />
                {t('productCategories.totalProducts', { count: totalProducts })}
              </Badge>
            </div>
          )}
        </div>

        {/* Content */}
        {loading ? (
          <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3'>
            {Array.from({ length: 3 }).map((_, i) => (
              <Card key={`cat-skel-${i}`} className='border'>
                <CardContent className='p-4 space-y-2'>
                  <Skeleton className='h-5 w-32' />
                  <Skeleton className='h-3 w-full' />
                  <Skeleton className='h-4 w-20' />
                </CardContent>
              </Card>
            ))}
          </div>
        ) : error ? (
          <div className='rounded-xl border border-destructive/30 bg-destructive/5 text-destructive text-sm px-4 py-3'>
            {t('productCategories.errorLoad')}
          </div>
        ) : items.length === 0 ? (
          <div className='rounded-xl border-2 border-dashed bg-card/30 py-10 px-4 flex flex-col items-center justify-center text-center'>
            <div className='relative mb-3'>
              <div className='absolute inset-0 bg-primary/20 rounded-xl blur-lg opacity-60' />
              <div className='relative p-3.5 rounded-xl bg-gradient-to-br from-primary/15 to-primary/5 border border-primary/20 text-primary'>
                <FolderOpen className='size-6' />
              </div>
            </div>
            <h4 className='font-semibold text-base mb-1'>
              {t('productCategories.emptyTitle')}
            </h4>
            <p className='text-xs text-muted-foreground max-w-sm'>
              {t('productCategories.emptyDescription')}
            </p>
          </div>
        ) : (
          <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3'>
            {items.map((c) => (
              <Card
                key={c.id}
                className='border-2 hover:border-primary/50 hover:shadow-md transition-all duration-200 group bg-card/50'
              >
                <CardContent className='p-4 space-y-3'>
                  <div className='flex items-start justify-between gap-2'>
                    <div className='flex-1 min-w-0'>
                      <h4 className='font-bold text-sm leading-tight line-clamp-1 group-hover:text-primary transition-colors'>
                        {c.name}
                      </h4>
                      {c.description ? (
                        <p className='text-xs text-muted-foreground mt-1 line-clamp-2 leading-snug'>
                          {c.description}
                        </p>
                      ) : (
                        <p className='text-xs text-muted-foreground/60 italic mt-1'>
                          {t('productCategories.noDescription')}
                        </p>
                      )}
                    </div>
                    <div className='flex-shrink-0 p-1.5 rounded-lg bg-primary/10 border border-primary/20 text-primary'>
                      <FolderOpen className='size-3.5' />
                    </div>
                  </div>

                  <div className='flex items-center gap-2 p-2 rounded-lg bg-muted/30 border border-muted'>
                    <div className='p-1 rounded-md bg-background'>
                      <Package className='size-3.5 text-muted-foreground' />
                    </div>
                    <span className='text-xs font-semibold'>
                      {t('productCategories.productsCount', {
                        count: c.productsCount,
                      })}
                    </span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}

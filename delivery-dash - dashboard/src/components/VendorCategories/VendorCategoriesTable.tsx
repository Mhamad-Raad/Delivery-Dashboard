import { useTranslation } from 'react-i18next';
import {
  Store,
  Pencil,
  Trash2,
  CheckCircle2,
  XCircle,
  Tag,
} from 'lucide-react';

import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Skeleton } from '@/components/ui/skeleton';

import CustomTablePagination from '../CustomTablePagination';

import type { VendorCategoryType } from '@/interfaces/VendorCategory.interface';

interface Props {
  items: VendorCategoryType[];
  total: number;
  loading: boolean;
  onEdit: (item: VendorCategoryType) => void;
  onDelete: (item: VendorCategoryType) => void;
}

export default function VendorCategoriesTable({
  items,
  total,
  loading,
  onEdit,
  onDelete,
}: Props) {
  const { t } = useTranslation('vendorCategories');

  return (
    <div className='rounded-xl border bg-card shadow-sm flex flex-col overflow-hidden'>
      <ScrollArea className='h-[calc(100vh-280px)] md:h-[calc(100vh-280px)]'>
        <div className='p-3 md:p-6'>
          <div className='space-y-3 md:space-y-4'>
            {loading
              ? Array.from({ length: 6 }).map((_, i) => (
                  <Card key={`skeleton-${i}`} className='border'>
                    <CardContent className='p-4 md:p-6'>
                      <div className='flex items-start gap-4'>
                        <Skeleton className='h-14 w-14 rounded-xl flex-shrink-0' />
                        <div className='flex-1 space-y-3'>
                          <Skeleton className='h-6 w-48' />
                          <Skeleton className='h-4 w-64' />
                          <div className='flex gap-4'>
                            <Skeleton className='h-4 w-24' />
                            <Skeleton className='h-4 w-20' />
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))
              : items.map((item) => (
                  <Card
                    key={item.id}
                    className='border-2 hover:border-primary hover:shadow-xl transition-all duration-300 group overflow-hidden bg-card/50 backdrop-blur-sm hover:bg-card'
                  >
                    {/* Accent bar */}
                    <div
                      className={`h-1.5 ${
                        item.isActive
                          ? 'bg-gradient-to-r from-primary via-primary/70 to-primary'
                          : 'bg-gradient-to-r from-slate-400 via-slate-300 to-slate-400'
                      }`}
                    />

                    <CardContent className='p-4 md:p-5'>
                      <div className='flex flex-col lg:flex-row lg:items-center gap-4 lg:gap-6'>
                        {/* Left: Icon + Info */}
                        <div className='flex items-start gap-3 lg:flex-1 min-w-0'>
                          <div className='relative flex-shrink-0'>
                            <div className='absolute inset-0 bg-primary/20 rounded-xl blur-sm group-hover:bg-primary/30 transition-all duration-300' />
                            <div className='relative p-3.5 rounded-xl bg-gradient-to-br from-primary/20 to-primary/5 border border-primary/20 text-primary'>
                              <Tag className='size-6' />
                            </div>
                          </div>

                          <div className='min-w-0 space-y-2 flex-1'>
                            {/* Status badge */}
                            <Badge
                              variant='outline'
                              className={
                                item.isActive
                                  ? 'bg-emerald-500/15 text-emerald-700 dark:bg-emerald-500/25 dark:text-emerald-300 border-emerald-500/30 dark:border-emerald-500/40 font-bold text-xs px-2.5 py-1 w-fit shadow-sm gap-1'
                                  : 'bg-slate-500/15 text-slate-700 dark:bg-slate-500/25 dark:text-slate-300 border-slate-500/30 dark:border-slate-500/40 font-bold text-xs px-2.5 py-1 w-fit shadow-sm gap-1'
                              }
                            >
                              {item.isActive ? (
                                <CheckCircle2 className='size-3' />
                              ) : (
                                <XCircle className='size-3' />
                              )}
                              {item.isActive
                                ? t('card.active')
                                : t('card.inactive')}
                            </Badge>

                            <div>
                              <h3 className='font-bold text-lg md:text-xl leading-tight group-hover:text-primary transition-colors line-clamp-1 break-words'>
                                {item.name}
                              </h3>
                              <p className='text-xs md:text-sm text-muted-foreground mt-1 line-clamp-2 leading-snug'>
                                {item.description || t('card.noDescription')}
                              </p>
                            </div>

                            {/* Vendors count */}
                            <div className='flex items-center gap-2 p-2 rounded-lg bg-muted/30 border border-muted w-fit'>
                              <div className='p-1 rounded-md bg-background'>
                                <Store className='size-3.5 text-muted-foreground flex-shrink-0' />
                              </div>
                              <span className='text-xs md:text-sm font-medium'>
                                {t('card.vendorsUsing', { count: item.vendorsCount })}
                              </span>
                            </div>
                          </div>
                        </div>

                        <Separator className='lg:hidden' orientation='horizontal' />

                        {/* Actions */}
                        <div className='flex flex-row lg:flex-col gap-2 lg:flex-shrink-0'>
                          <Button
                            type='button'
                            variant='outline'
                            size='sm'
                            onClick={() => onEdit(item)}
                            className='gap-2 hover:border-primary hover:text-primary transition-all'
                          >
                            <Pencil className='size-4' />
                            {t('card.edit')}
                          </Button>
                          <Button
                            type='button'
                            variant='outline'
                            size='sm'
                            onClick={() => onDelete(item)}
                            className='gap-2 hover:border-destructive hover:text-destructive hover:bg-destructive/5 transition-all'
                          >
                            <Trash2 className='size-4' />
                            {t('card.delete')}
                          </Button>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
          </div>
        </div>
      </ScrollArea>

      {/* Pagination */}
      <div className='border-t bg-muted/30 px-4 py-3 mt-auto'>
        <CustomTablePagination total={total} suggestions={[10, 20, 40, 50, 100]} />
      </div>
    </div>
  );
}

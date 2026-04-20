import { useTranslation } from 'react-i18next';
import {
  FolderOpen,
  Pencil,
  Trash2,
  Package,
  Hash,
} from 'lucide-react';

import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Skeleton } from '@/components/ui/skeleton';

import type { CategoryDTO } from '@/data/Categories';

interface Props {
  items: CategoryDTO[];
  loading: boolean;
  onEdit: (item: CategoryDTO) => void;
  onDelete: (item: CategoryDTO) => void;
}

export default function CategoriesTable({
  items,
  loading,
  onEdit,
  onDelete,
}: Props) {
  const { t } = useTranslation('categories');

  return (
    <div className='rounded-xl border bg-card shadow-sm flex flex-col overflow-hidden'>
      <ScrollArea className='h-[calc(100vh-280px)]'>
        <div className='p-3 md:p-6'>
          <div className='space-y-3 md:space-y-4'>
            {loading
              ? Array.from({ length: 5 }).map((_, i) => (
                  <Card key={`skel-${i}`} className='border'>
                    <CardContent className='p-4 md:p-6'>
                      <div className='flex items-start gap-4'>
                        <Skeleton className='h-14 w-14 rounded-xl flex-shrink-0' />
                        <div className='flex-1 space-y-3'>
                          <Skeleton className='h-6 w-48' />
                          <Skeleton className='h-4 w-64' />
                          <div className='flex gap-4'>
                            <Skeleton className='h-4 w-24' />
                            <Skeleton className='h-4 w-16' />
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
                    <div className='h-1.5 bg-gradient-to-r from-primary via-primary/70 to-primary' />

                    <CardContent className='p-4 md:p-5'>
                      <div className='flex flex-col lg:flex-row lg:items-center gap-4 lg:gap-6'>
                        <div className='flex items-start gap-3 lg:flex-1 min-w-0'>
                          <div className='relative flex-shrink-0'>
                            <div className='absolute inset-0 bg-primary/20 rounded-xl blur-sm group-hover:bg-primary/30 transition-all duration-300' />
                            <div className='relative p-3.5 rounded-xl bg-gradient-to-br from-primary/20 to-primary/5 border border-primary/20 text-primary'>
                              <FolderOpen className='size-6' />
                            </div>
                          </div>

                          <div className='min-w-0 space-y-2 flex-1'>
                            {typeof item.sortOrder === 'number' && (
                              <Badge
                                variant='outline'
                                className='bg-muted/50 text-muted-foreground border-muted font-bold text-xs px-2.5 py-1 w-fit gap-1'
                              >
                                <Hash className='size-3' />
                                {t('card.sortOrder')}: {item.sortOrder}
                              </Badge>
                            )}

                            <div>
                              <h3 className='font-bold text-lg md:text-xl leading-tight group-hover:text-primary transition-colors line-clamp-1 break-words'>
                                {item.name}
                              </h3>
                              <p className='text-xs md:text-sm text-muted-foreground mt-1 line-clamp-2 leading-snug'>
                                {item.description || t('card.noDescription')}
                              </p>
                            </div>

                            <div className='flex items-center gap-2 p-2 rounded-lg bg-muted/30 border border-muted w-fit'>
                              <div className='p-1 rounded-md bg-background'>
                                <Package className='size-3.5 text-muted-foreground flex-shrink-0' />
                              </div>
                              <span className='text-xs md:text-sm font-medium'>
                                {t('card.productsCount', {
                                  count: item.productsCount,
                                })}
                              </span>
                            </div>
                          </div>
                        </div>

                        <Separator className='lg:hidden' orientation='horizontal' />

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
    </div>
  );
}

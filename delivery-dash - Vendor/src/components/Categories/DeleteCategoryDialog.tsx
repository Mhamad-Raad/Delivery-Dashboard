import { useTranslation } from 'react-i18next';
import { AlertTriangle, Trash2, Loader2, FolderOpen } from 'lucide-react';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

import type { CategoryDTO } from '@/data/Categories';

interface Props {
  open: boolean;
  target: CategoryDTO | null;
  submitting: boolean;
  serverError?: string | null;
  onClose: () => void;
  onConfirm: () => void;
}

export default function DeleteCategoryDialog({
  open,
  target,
  submitting,
  serverError,
  onClose,
  onConfirm,
}: Props) {
  const { t } = useTranslation('categories');

  if (!target) return null;

  return (
    <Dialog open={open} onOpenChange={(o) => !o && !submitting && onClose()}>
      <DialogContent className='sm:max-w-md'>
        <DialogHeader>
          <div className='flex items-center gap-3'>
            <div className='relative flex-shrink-0'>
              <div className='absolute inset-0 bg-destructive/20 rounded-xl blur-sm' />
              <div className='relative p-2.5 rounded-xl bg-gradient-to-br from-destructive/20 to-destructive/5 border border-destructive/20 text-destructive'>
                <AlertTriangle className='size-5' />
              </div>
            </div>
            <div>
              <DialogTitle>{t('confirmDelete.title')}</DialogTitle>
              <DialogDescription>
                {t('confirmDelete.description')}
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <div className='rounded-xl border bg-muted/30 p-3 flex items-center gap-3'>
          <div className='p-2 rounded-lg bg-background border'>
            <FolderOpen className='size-4 text-muted-foreground' />
          </div>
          <div className='min-w-0'>
            <p className='text-sm font-semibold truncate'>{target.name}</p>
            <p className='text-xs text-muted-foreground'>
              {target.productsCount === 1
                ? `${target.productsCount} product`
                : `${target.productsCount} products`}
            </p>
          </div>
        </div>

        <p className='text-sm text-destructive/80 font-medium'>
          {t('confirmDelete.warning')}
        </p>

        {serverError && (
          <div className='rounded-lg border border-destructive/40 bg-destructive/10 text-destructive text-sm px-3 py-2'>
            {serverError}
          </div>
        )}

        <DialogFooter className='gap-2 sm:gap-2'>
          <Button
            type='button'
            variant='outline'
            onClick={onClose}
            disabled={submitting}
          >
            {t('confirmDelete.cancel')}
          </Button>
          <Button
            type='button'
            variant='destructive'
            onClick={onConfirm}
            disabled={submitting}
            className='gap-2'
          >
            {submitting ? (
              <Loader2 className='size-4 animate-spin' />
            ) : (
              <Trash2 className='size-4' />
            )}
            {t('confirmDelete.confirm')}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

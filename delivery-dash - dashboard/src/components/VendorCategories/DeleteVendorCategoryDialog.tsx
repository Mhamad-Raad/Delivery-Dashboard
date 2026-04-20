import { useTranslation } from 'react-i18next';
import { AlertTriangle, Trash2, Loader2, Store } from 'lucide-react';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

import type { VendorCategoryType } from '@/interfaces/VendorCategory.interface';

interface Props {
  open: boolean;
  target: VendorCategoryType | null;
  submitting: boolean;
  serverError?: string | null;
  onClose: () => void;
  onConfirm: () => void;
}

export default function DeleteVendorCategoryDialog({
  open,
  target,
  submitting,
  serverError,
  onClose,
  onConfirm,
}: Props) {
  const { t } = useTranslation('vendorCategories');

  if (!target) return null;
  const inUse = target.vendorsCount > 0;

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
              <DialogTitle>
                {inUse
                  ? t('confirmDelete.blockedTitle')
                  : t('confirmDelete.title')}
              </DialogTitle>
              <DialogDescription>
                {inUse
                  ? t('confirmDelete.blockedDescription', {
                      count: target.vendorsCount,
                    })
                  : t('confirmDelete.description')}
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <div className='rounded-xl border bg-muted/30 p-3 flex items-center gap-3'>
          <div className='p-2 rounded-lg bg-background border'>
            <Store className='size-4 text-muted-foreground' />
          </div>
          <div className='min-w-0'>
            <p className='text-sm font-semibold truncate'>{target.name}</p>
            {target.description && (
              <p className='text-xs text-muted-foreground truncate'>
                {target.description}
              </p>
            )}
          </div>
        </div>

        {!inUse && (
          <p className='text-sm text-destructive/80 font-medium'>
            {t('confirmDelete.warning')}
          </p>
        )}

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
            disabled={submitting || inUse}
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

import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Tag, Save, Plus, Loader2 } from 'lucide-react';

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';

import type { VendorCategoryType } from '@/interfaces/VendorCategory.interface';

export type FormValues = {
  name: string;
  description: string;
  isActive: boolean;
};

interface Props {
  open: boolean;
  mode: 'create' | 'edit';
  initial?: VendorCategoryType | null;
  submitting: boolean;
  serverError?: string | null;
  onClose: () => void;
  onSubmit: (values: FormValues) => void;
}

export default function VendorCategoryFormModal({
  open,
  mode,
  initial,
  submitting,
  serverError,
  onClose,
  onSubmit,
}: Props) {
  const { t } = useTranslation('vendorCategories');

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [localErrors, setLocalErrors] = useState<{
    name?: string;
    description?: string;
  }>({});

  useEffect(() => {
    if (!open) return;
    if (mode === 'edit' && initial) {
      setName(initial.name);
      setDescription(initial.description || '');
      setIsActive(initial.isActive);
    } else {
      setName('');
      setDescription('');
      setIsActive(true);
    }
    setLocalErrors({});
  }, [open, mode, initial]);

  const validate = (): boolean => {
    const errors: { name?: string; description?: string } = {};
    if (!name.trim()) errors.name = t('form.validation.nameRequired');
    else if (name.trim().length > 100)
      errors.name = t('form.validation.nameTooLong');

    if (description && description.length > 500)
      errors.description = t('form.validation.descriptionTooLong');

    setLocalErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    onSubmit({
      name: name.trim(),
      description: description.trim(),
      isActive,
    });
  };

  return (
    <Dialog open={open} onOpenChange={(o) => !o && !submitting && onClose()}>
      <DialogContent className='sm:max-w-lg'>
        <form onSubmit={handleSubmit} className='space-y-5'>
          <DialogHeader>
            <div className='flex items-center gap-3'>
              <div className='relative flex-shrink-0'>
                <div className='absolute inset-0 bg-primary/20 rounded-xl blur-sm' />
                <div className='relative p-2.5 rounded-xl bg-gradient-to-br from-primary/20 to-primary/5 border border-primary/20 text-primary'>
                  <Tag className='size-5' />
                </div>
              </div>
              <div>
                <DialogTitle>
                  {mode === 'create' ? t('form.createTitle') : t('form.editTitle')}
                </DialogTitle>
                <DialogDescription>
                  {mode === 'create'
                    ? t('form.createSubtitle')
                    : t('form.editSubtitle')}
                </DialogDescription>
              </div>
            </div>
          </DialogHeader>

          {serverError && (
            <div className='rounded-lg border border-destructive/40 bg-destructive/10 text-destructive text-sm px-3 py-2'>
              {serverError}
            </div>
          )}

          <div className='space-y-4'>
            <div className='space-y-2'>
              <Label htmlFor='vc-name'>
                {t('form.name')} <span className='text-destructive'>*</span>
              </Label>
              <Input
                id='vc-name'
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder={t('form.namePlaceholder')}
                disabled={submitting}
                maxLength={100}
                autoFocus
                className='h-10 rounded-xl'
              />
              {localErrors.name && (
                <p className='text-xs text-destructive'>{localErrors.name}</p>
              )}
            </div>

            <div className='space-y-2'>
              <Label htmlFor='vc-description'>{t('form.description')}</Label>
              <Textarea
                id='vc-description'
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder={t('form.descriptionPlaceholder')}
                disabled={submitting}
                maxLength={500}
                rows={3}
                className='rounded-xl resize-none'
              />
              {localErrors.description && (
                <p className='text-xs text-destructive'>
                  {localErrors.description}
                </p>
              )}
            </div>

            <div className='flex items-start justify-between gap-4 rounded-xl border bg-muted/30 p-3'>
              <div className='flex-1 min-w-0'>
                <Label htmlFor='vc-active' className='cursor-pointer'>
                  {t('form.isActive')}
                </Label>
                <p className='text-xs text-muted-foreground mt-0.5'>
                  {t('form.isActiveHint')}
                </p>
              </div>
              <Switch
                id='vc-active'
                checked={isActive}
                onCheckedChange={setIsActive}
                disabled={submitting}
              />
            </div>
          </div>

          <DialogFooter className='gap-2 sm:gap-2'>
            <Button
              type='button'
              variant='outline'
              onClick={onClose}
              disabled={submitting}
            >
              {t('form.actions.cancel')}
            </Button>
            <Button type='submit' disabled={submitting} className='gap-2'>
              {submitting ? (
                <>
                  <Loader2 className='size-4 animate-spin' />
                  {mode === 'create'
                    ? t('form.actions.creating')
                    : t('form.actions.saving')}
                </>
              ) : mode === 'create' ? (
                <>
                  <Plus className='size-4' />
                  {t('form.actions.create')}
                </>
              ) : (
                <>
                  <Save className='size-4' />
                  {t('form.actions.save')}
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

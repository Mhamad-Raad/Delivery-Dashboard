import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FolderOpen, Save, Plus, Loader2 } from 'lucide-react';

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

import type { CategoryDTO } from '@/data/Categories';

export type FormValues = {
  name: string;
  description: string;
  sortOrder: number | null;
};

interface Props {
  open: boolean;
  mode: 'create' | 'edit';
  initial?: CategoryDTO | null;
  submitting: boolean;
  serverError?: string | null;
  onClose: () => void;
  onSubmit: (values: FormValues) => void;
}

export default function CategoryFormModal({
  open,
  mode,
  initial,
  submitting,
  serverError,
  onClose,
  onSubmit,
}: Props) {
  const { t } = useTranslation('categories');

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [sortOrder, setSortOrder] = useState<string>('');
  const [localErrors, setLocalErrors] = useState<{
    name?: string;
    description?: string;
  }>({});

  useEffect(() => {
    if (!open) return;
    if (mode === 'edit' && initial) {
      setName(initial.name);
      setDescription(initial.description || '');
      setSortOrder(
        typeof initial.sortOrder === 'number' ? String(initial.sortOrder) : ''
      );
    } else {
      setName('');
      setDescription('');
      setSortOrder('');
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
    const parsed = sortOrder.trim() === '' ? null : parseInt(sortOrder, 10);
    onSubmit({
      name: name.trim(),
      description: description.trim(),
      sortOrder: Number.isNaN(parsed as number) ? null : parsed,
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
                  <FolderOpen className='size-5' />
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
              <Label htmlFor='cat-name'>
                {t('form.name')} <span className='text-destructive'>*</span>
              </Label>
              <Input
                id='cat-name'
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
              <Label htmlFor='cat-description'>{t('form.description')}</Label>
              <Textarea
                id='cat-description'
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

            <div className='space-y-2'>
              <Label htmlFor='cat-sort'>{t('form.sortOrder')}</Label>
              <Input
                id='cat-sort'
                type='number'
                min={0}
                value={sortOrder}
                onChange={(e) => setSortOrder(e.target.value)}
                placeholder='0'
                disabled={submitting}
                className='h-10 rounded-xl'
              />
              <p className='text-xs text-muted-foreground'>
                {t('form.sortOrderHint')}
              </p>
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

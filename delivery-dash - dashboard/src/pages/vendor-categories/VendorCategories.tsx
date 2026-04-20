import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';

import type { AppDispatch, RootState } from '@/store/store';
import {
  fetchVendorCategories,
  createVendorCategory,
  updateVendorCategory,
  deleteVendorCategory,
  clearMutationError,
} from '@/store/slices/vendorCategoriesSlice';

import VendorCategoriesFilters from '@/components/VendorCategories/VendorCategoriesFilters';
import VendorCategoriesTable from '@/components/VendorCategories/VendorCategoriesTable';
import EmptyState from '@/components/VendorCategories/EmptyState';
import VendorCategoryFormModal, {
  type FormValues,
} from '@/components/VendorCategories/VendorCategoryFormModal';
import DeleteVendorCategoryDialog from '@/components/VendorCategories/DeleteVendorCategoryDialog';

import type { VendorCategoryType } from '@/interfaces/VendorCategory.interface';

const VendorCategories = () => {
  const [searchParams] = useSearchParams();
  const dispatch = useDispatch<AppDispatch>();
  const { t } = useTranslation('vendorCategories');

  const {
    items,
    loading,
    error,
    total,
    mutating,
    mutationError,
  } = useSelector((state: RootState) => state.vendorCategories);

  const limit = parseInt(searchParams.get('limit') || '10', 10);
  const page = parseInt(searchParams.get('page') || '1', 10);
  const search = searchParams.get('search') || '';
  const status = searchParams.get('status');
  const activeOnly =
    status === 'active' ? true : status === 'inactive' ? false : undefined;

  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'edit'>('create');
  const [editTarget, setEditTarget] = useState<VendorCategoryType | null>(null);

  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<VendorCategoryType | null>(
    null
  );

  useEffect(() => {
    const params: any = { page, limit };
    if (search) params.searchName = search;
    if (typeof activeOnly === 'boolean') params.activeOnly = activeOnly;
    dispatch(fetchVendorCategories(params));
  }, [dispatch, page, limit, search, activeOnly]);

  useEffect(() => {
    if (error) {
      toast.error(t('error.loadFailed'), { description: error });
    }
  }, [error, t]);

  const openCreate = () => {
    setFormMode('create');
    setEditTarget(null);
    dispatch(clearMutationError());
    setFormOpen(true);
  };

  const openEdit = (item: VendorCategoryType) => {
    setFormMode('edit');
    setEditTarget(item);
    dispatch(clearMutationError());
    setFormOpen(true);
  };

  const handleSubmit = async (values: FormValues) => {
    if (formMode === 'create') {
      const result = await dispatch(createVendorCategory(values));
      if (createVendorCategory.fulfilled.match(result)) {
        toast.success(t('success.created'), {
          description: t('success.createdDescription', { name: values.name }),
        });
        setFormOpen(false);
      } else {
        const payload = result.payload as { error?: string } | undefined;
        toast.error(t('error.createFailed'), {
          description: payload?.error || '',
        });
      }
    } else if (editTarget) {
      const result = await dispatch(
        updateVendorCategory({ id: editTarget.id, body: values })
      );
      if (updateVendorCategory.fulfilled.match(result)) {
        toast.success(t('success.updated'), {
          description: t('success.updatedDescription', { name: values.name }),
        });
        setFormOpen(false);
      } else {
        const payload = result.payload as { error?: string } | undefined;
        toast.error(t('error.updateFailed'), {
          description: payload?.error || '',
        });
      }
    }
  };

  const openDelete = (item: VendorCategoryType) => {
    setDeleteTarget(item);
    dispatch(clearMutationError());
    setDeleteOpen(true);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    const name = deleteTarget.name;
    const result = await dispatch(deleteVendorCategory(deleteTarget.id));
    if (deleteVendorCategory.fulfilled.match(result)) {
      toast.success(t('success.deleted'), {
        description: t('success.deletedDescription', { name }),
      });
      setDeleteOpen(false);
      setDeleteTarget(null);
    } else {
      const payload = result.payload as { error?: string } | undefined;
      toast.error(t('error.deleteFailed'), {
        description: payload?.error || '',
      });
    }
  };

  const hasNoItems = !loading && items.length === 0 && !error;

  return (
    <section className='w-full h-full flex flex-col gap-6 overflow-hidden'>
      <VendorCategoriesFilters onCreate={openCreate} />

      <div className='flex-1 min-h-0'>
        {hasNoItems ? (
          <EmptyState onCreate={openCreate} />
        ) : (
          <VendorCategoriesTable
            items={items}
            total={total}
            loading={loading}
            onEdit={openEdit}
            onDelete={openDelete}
          />
        )}
      </div>

      <VendorCategoryFormModal
        open={formOpen}
        mode={formMode}
        initial={editTarget}
        submitting={mutating}
        serverError={mutationError}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
      />

      <DeleteVendorCategoryDialog
        open={deleteOpen}
        target={deleteTarget}
        submitting={mutating}
        serverError={mutationError}
        onClose={() => setDeleteOpen(false)}
        onConfirm={handleDelete}
      />
    </section>
  );
};

export default VendorCategories;

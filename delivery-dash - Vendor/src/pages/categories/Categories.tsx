import { useEffect, useMemo, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';

import type { AppDispatch, RootState } from '@/store/store';
import {
  fetchMyCategories,
  createCategory,
  updateCategory,
  deleteCategory,
  clearMutationError,
} from '@/store/slices/categoriesSlice';

import CategoriesFilters from '@/components/Categories/CategoriesFilters';
import CategoriesTable from '@/components/Categories/CategoriesTable';
import EmptyState from '@/components/Categories/EmptyState';
import CategoryFormModal, {
  type FormValues,
} from '@/components/Categories/CategoryFormModal';
import DeleteCategoryDialog from '@/components/Categories/DeleteCategoryDialog';

import type { CategoryDTO } from '@/data/Categories';

const Categories = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { t } = useTranslation('categories');
  const { items, loading, error, mutating, mutationError } = useSelector(
    (state: RootState) => state.categories
  );

  const [search, setSearch] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState<'create' | 'edit'>('create');
  const [editTarget, setEditTarget] = useState<CategoryDTO | null>(null);

  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<CategoryDTO | null>(null);

  useEffect(() => {
    dispatch(fetchMyCategories());
  }, [dispatch]);

  useEffect(() => {
    if (error) {
      toast.error(t('error.loadFailed'), { description: error });
    }
  }, [error, t]);

  const filtered = useMemo(() => {
    if (!search.trim()) return items;
    const q = search.trim().toLowerCase();
    return items.filter((c) => c.name.toLowerCase().includes(q));
  }, [items, search]);

  const openCreate = () => {
    setFormMode('create');
    setEditTarget(null);
    dispatch(clearMutationError());
    setFormOpen(true);
  };

  const openEdit = (item: CategoryDTO) => {
    setFormMode('edit');
    setEditTarget(item);
    dispatch(clearMutationError());
    setFormOpen(true);
  };

  const handleSubmit = async (values: FormValues) => {
    if (formMode === 'create') {
      const result = await dispatch(createCategory(values));
      if (createCategory.fulfilled.match(result)) {
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
        updateCategory({ id: editTarget.id, body: values })
      );
      if (updateCategory.fulfilled.match(result)) {
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

  const openDelete = (item: CategoryDTO) => {
    setDeleteTarget(item);
    dispatch(clearMutationError());
    setDeleteOpen(true);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    const name = deleteTarget.name;
    const result = await dispatch(deleteCategory(deleteTarget.id));
    if (deleteCategory.fulfilled.match(result)) {
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
      <CategoriesFilters
        search={search}
        onSearchChange={setSearch}
        onCreate={openCreate}
      />

      <div className='flex-1 min-h-0'>
        {hasNoItems ? (
          <EmptyState onCreate={openCreate} />
        ) : (
          <CategoriesTable
            items={filtered}
            loading={loading}
            onEdit={openEdit}
            onDelete={openDelete}
          />
        )}
      </div>

      <CategoryFormModal
        open={formOpen}
        mode={formMode}
        initial={editTarget}
        submitting={mutating}
        serverError={mutationError}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
      />

      <DeleteCategoryDialog
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

export default Categories;

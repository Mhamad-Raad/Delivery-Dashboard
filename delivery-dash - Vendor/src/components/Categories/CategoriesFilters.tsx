import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import {
  Search,
  Plus,
  X,
  FolderTree,
  SlidersHorizontal,
  Sparkles,
} from 'lucide-react';

import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';

interface Props {
  search: string;
  onSearchChange: (value: string) => void;
  onCreate: () => void;
}

export default function CategoriesFilters({
  search,
  onSearchChange,
  onCreate,
}: Props) {
  const { t } = useTranslation('categories');
  const [typed, setTyped] = useState(search);
  const debounceRef = useRef<any>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      onSearchChange(typed);
    }, 250);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [typed]);

  const clear = () => {
    setTyped('');
    onSearchChange('');
  };

  const hasActive = !!search;

  return (
    <div className='flex flex-col gap-5'>
      {/* Header */}
      <div className='flex flex-col lg:flex-row lg:items-center justify-between gap-4'>
        <div className='flex items-center gap-4'>
          <div className='relative'>
            <div className='absolute inset-0 bg-gradient-to-br from-primary to-primary/50 rounded-2xl blur-lg opacity-40' />
            <div className='relative p-3.5 rounded-2xl bg-gradient-to-br from-primary to-primary/80 text-primary-foreground shadow-lg'>
              <FolderTree className='size-7' />
            </div>
          </div>
          <div>
            <div className='flex items-center gap-2'>
              <h1 className='text-3xl font-bold tracking-tight'>{t('title')}</h1>
              <Sparkles className='size-5 text-amber-500' />
            </div>
            <p className='text-muted-foreground mt-0.5'>{t('subtitle')}</p>
          </div>
        </div>

        <Button onClick={onCreate} className='gap-2' size='default'>
          <Plus className='size-4' />
          {t('addCategory')}
        </Button>
      </div>

      {/* Filter Bar */}
      <div className='flex flex-col sm:flex-row items-start sm:items-center gap-3 p-4 rounded-2xl border bg-card/50 backdrop-blur-sm shadow-sm hover:shadow-md transition-all duration-300'>
        <div className='flex items-center gap-2 text-muted-foreground'>
          <div className='p-1.5 rounded-lg bg-primary/10'>
            <SlidersHorizontal className='size-4 text-primary' />
          </div>
          <span className='text-sm font-medium'>{t('filterCategories')}</span>
        </div>

        <div className='relative flex-1 w-full group'>
          <Search className='absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-muted-foreground group-focus-within:text-primary transition-colors' />
          <Input
            type='text'
            placeholder={t('searchPlaceholder')}
            className='pl-10 pr-10 h-10 bg-background/80 border-border/50 focus-visible:border-primary/50 focus-visible:ring-primary/20 rounded-xl'
            value={typed}
            onChange={(e) => setTyped(e.target.value)}
          />
          {typed && (
            <button
              onClick={clear}
              className='absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-all rounded p-0.5'
              title='Clear search'
            >
              <X className='size-4' />
            </button>
          )}
        </div>

        <div
          className={`flex items-center gap-2 overflow-hidden transition-all duration-300 ${
            hasActive ? 'w-auto opacity-100' : 'w-0 opacity-0'
          }`}
        >
          {hasActive && (
            <Badge variant='secondary' className='gap-1.5 py-1.5 px-3 shadow-sm whitespace-nowrap'>
              <span className='size-1.5 rounded-full bg-primary animate-pulse' />
              <span className='font-medium'>Filtered</span>
            </Badge>
          )}
        </div>
      </div>
    </div>
  );
}

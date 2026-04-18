import { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import {
  Search,
  Plus,
  Package,
  LayoutGrid,
  Table,
  X,
  Sparkles,
  SlidersHorizontal,
} from 'lucide-react';

interface ProductsFiltersProps {
  viewMode: 'table' | 'cards';
  onViewModeChange: (mode: 'table' | 'cards') => void;
}

const ProductsFilters = ({
  viewMode,
  onViewModeChange,
}: ProductsFiltersProps) => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useTranslation();

  const [search, setSearch] = useState(
    () => searchParams.get('searchName') || ''
  );
  const [typedSearch, setTypedSearch] = useState(search);
  const [stockFilter, setStockFilter] = useState<string>(
    searchParams.get('inStock') || 'all'
  );

  const debounceRef = useRef<any>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      setSearch(typedSearch);
    }, 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [typedSearch]);

  useEffect(() => {
    const params = new URLSearchParams(searchParams);
    if (search) {
      params.set('searchName', search);
    } else {
      params.delete('searchName');
    }
    params.set('page', '1');
    navigate(`${window.location.pathname}?${params.toString()}`, {
      replace: true,
    });
    // eslint-disable-next-line
  }, [search]);

  useEffect(() => {
    const params = new URLSearchParams(searchParams);
    if (stockFilter && stockFilter !== 'all') {
      params.set('inStock', stockFilter);
    } else {
      params.delete('inStock');
    }
    params.set('page', '1');
    navigate(`${window.location.pathname}?${params.toString()}`, {
      replace: true,
    });
    // eslint-disable-next-line
  }, [stockFilter]);

  useEffect(() => {
    const urlSearch = searchParams.get('searchName') || '';
    setSearch(urlSearch);
    setTypedSearch(urlSearch);
    setStockFilter(searchParams.get('inStock') || 'all');
    // eslint-disable-next-line
  }, [searchParams]);

  const handleOnCreate = () => {
    navigate('/products/create');
  };

  const clearSearch = () => {
    setTypedSearch('');
    setSearch('');
  };

  const hasActiveFilters = search || stockFilter !== 'all';

  return (
    <div className='flex flex-col gap-5'>
      {/* Header Section */}
      <div className='flex flex-col lg:flex-row lg:items-center justify-between gap-4'>
        <div className='flex items-center gap-4'>
          <div className='relative'>
            <div className='absolute inset-0 bg-gradient-to-br from-primary to-primary/50 rounded-2xl blur-lg opacity-40' />
            <div className='relative p-3.5 rounded-2xl bg-gradient-to-br from-primary to-primary/80 text-primary-foreground shadow-lg'>
              <Package className='size-7' />
            </div>
          </div>
          <div>
            <div className='flex items-center gap-2'>
              <h1 className='text-3xl font-bold tracking-tight'>
                {t('products.pageTitle')}
              </h1>
              <Sparkles className='size-5 text-amber-500' />
            </div>
            <p className='text-muted-foreground mt-0.5'>
              {t('products.pageSubtitle')}
            </p>
          </div>
        </div>

        <Button
          type='button'
          onClick={handleOnCreate}
          className='gap-2'
          size='default'
        >
          <Plus className='size-4' />
          {t('products.addProduct')}
        </Button>
      </div>

      {/* Filters Bar */}
      <div className='flex flex-col sm:flex-row items-start sm:items-center gap-3 p-4 rounded-2xl border bg-card/50 backdrop-blur-sm shadow-sm'>
        <div className='flex items-center gap-2 text-muted-foreground'>
          <SlidersHorizontal className='size-4' />
          <span className='text-sm font-medium'>{t('products.filtersLabel')}</span>
        </div>

        <div className='flex flex-col sm:flex-row flex-1 items-start sm:items-center gap-3 w-full'>
          {/* Search Input */}
          <div className='relative flex-1 w-full max-w-md'>
            <Search className='absolute left-3.5 top-1/2 -translate-y-1/2 size-4 text-muted-foreground' />
            <Input
              type='text'
              placeholder={t('products.searchByNamePlaceholder')}
              className='pl-10 pr-10 h-10 bg-background/80 border-border/50 focus-visible:border-primary/50 focus-visible:ring-primary/20 transition-all rounded-xl'
              value={typedSearch}
              onChange={(e) => setTypedSearch(e.target.value)}
            />
            {typedSearch && (
              <button
                onClick={clearSearch}
                className='absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors'
              >
                <X className='size-4' />
              </button>
            )}
          </div>

          {/* Stock Filter */}
          <Select value={stockFilter} onValueChange={setStockFilter}>
            <SelectTrigger className='w-full sm:w-[160px] h-10 bg-background/80 border-border/50 rounded-xl'>
              <SelectValue placeholder={t('products.stockStatusPlaceholder')} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value='all'>{t('products.stockAll')}</SelectItem>
              <SelectItem value='true'>{t('products.stockIn')}</SelectItem>
              <SelectItem value='false'>{t('products.stockOut')}</SelectItem>
            </SelectContent>
          </Select>

          {/* View Mode Toggle */}
          <div className='flex items-center gap-1 p-1 bg-muted/80 rounded-xl'>
            <Button
              type='button'
              variant={viewMode === 'table' ? 'secondary' : 'ghost'}
              size='sm'
              className={`h-8 px-3 rounded-lg transition-all ${
                viewMode === 'table'
                  ? 'shadow-sm bg-background'
                  : 'hover:bg-transparent'
              }`}
              onClick={() => onViewModeChange('table')}
            >
              <Table className='size-4' />
              <span className='ml-1.5 hidden sm:inline text-xs font-medium'>
                {t('products.viewTable')}
              </span>
            </Button>
            <Button
              type='button'
              variant={viewMode === 'cards' ? 'secondary' : 'ghost'}
              size='sm'
              className={`h-8 px-3 rounded-lg transition-all ${
                viewMode === 'cards'
                  ? 'shadow-sm bg-background'
                  : 'hover:bg-transparent'
              }`}
              onClick={() => onViewModeChange('cards')}
            >
              <LayoutGrid className='size-4' />
              <span className='ml-1.5 hidden sm:inline text-xs font-medium'>
                {t('products.viewGrid')}
              </span>
            </Button>
          </div>
        </div>

        {/* Active Filters Badge */}
        {hasActiveFilters && (
          <Badge variant='secondary' className='gap-1.5 py-1'>
            <span className='size-1.5 rounded-full bg-primary animate-pulse' />
            {t('products.filteredBadge')}
          </Badge>
        )}
      </div>
    </div>
  );
};

export default ProductsFilters;

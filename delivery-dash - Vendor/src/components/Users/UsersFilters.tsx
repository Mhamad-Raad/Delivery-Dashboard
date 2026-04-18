import { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Search,
  Plus,
  Filter,
  Users as UsersIcon,
  CheckCircle2,
} from 'lucide-react';

const UsersFilters = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useTranslation();

  // Use index, -1 means "All"
  const [search, setSearch] = useState(
    () => searchParams.get('searchTerm') || ''
  );
  // typedSearch is what user is typing, search is debounced value synced to URL
  const [typedSearch, setTypedSearch] = useState(search);

  const [isActive, setIsActive] = useState<string>(() => {
    const param = searchParams.get('isActive');
    return param === null ? 'all' : param;
  });

  // Debounce for search input
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

  // Update search params in URL whenever any filter changes (debounced search)
  useEffect(() => {
    const params = new URLSearchParams(searchParams);
    if (search) {
      params.set('searchTerm', search);
    } else {
      params.delete('searchTerm');
    }

    if (isActive !== 'all') {
      params.set('isActive', isActive);
    } else {
      params.delete('isActive');
    }

    params.set('page', '1');
    navigate(`${window.location.pathname}?${params.toString()}`, {
      replace: true,
    });
    // eslint-disable-next-line
  }, [search, isActive]);

  // Sync state if URL changes externally (browser nav)
  useEffect(() => {
    const urlSearch = searchParams.get('searchTerm') || '';
    setSearch(urlSearch);
    setTypedSearch(urlSearch);

    const activeParam = searchParams.get('isActive');
    setIsActive(activeParam === null ? 'all' : activeParam);
    // eslint-disable-next-line
  }, [searchParams]);

  const handleOnCreate = () => {
    navigate('/users/create');
  };

  return (
    <div className='flex flex-col gap-6'>
      {/* Header with Title and Create Button */}
      <div className='flex flex-col sm:flex-row sm:items-center justify-between gap-4'>
        <div className='flex items-center gap-4 flex-1'>
          <div className='p-3 rounded-xl bg-gradient-to-br from-primary/20 to-primary/10 text-primary shadow-sm'>
            <UsersIcon className='size-6' />
          </div>
          <div>
            <h2 className='text-3xl font-bold tracking-tight bg-gradient-to-r from-foreground to-foreground/70 bg-clip-text'>
              {t('users.pageHeaderTitle')}
            </h2>
            <p className='text-sm text-muted-foreground mt-0.5'>
              {t('users.pageHeaderSubtitle')}
            </p>
          </div>
        </div>
        <Button
          type='button'
          className='gap-2 w-full sm:w-auto shadow-md hover:shadow-lg transition-shadow'
          size='lg'
          onClick={handleOnCreate}
        >
          <Plus className='size-4' />
          <span className='font-semibold'>{t('users.addStaff')}</span>
        </Button>
      </div>

      {/* Filters Section */}
      <div className='bg-gradient-to-br from-muted/50 to-muted/30 rounded-xl border shadow-sm p-5'>
        <div className='flex flex-col gap-4'>
          <div className='flex items-center gap-2.5 pb-3 border-b'>
            <div className='p-1.5 rounded-md bg-primary/10'>
              <Filter className='size-4 text-primary' />
            </div>
            <span className='text-sm font-semibold text-foreground'>
              {t('users.filtersTitle')}
            </span>
          </div>
          <div className='w-full grid gap-4 sm:grid-cols-2 lg:grid-cols-3'>
            {/* Search Input */}
            <div className='relative w-full'>
              <div className='text-muted-foreground pointer-events-none absolute inset-y-0 left-0 flex items-center justify-center pl-3.5'>
                <Search className='size-4' />
              </div>
              <Input
                type='text'
                placeholder={t('users.searchPlaceholder')}
                className='pl-10 bg-background w-full shadow-sm border-muted-foreground/20 focus-visible:border-primary/50 transition-colors h-11'
                value={typedSearch}
                onChange={(e) => setTypedSearch(e.target.value)}
              />
            </div>

            {/* Status Filter */}
            <div className='relative w-full'>
              <div className='text-muted-foreground pointer-events-none absolute inset-y-0 left-0 flex items-center justify-center pl-3.5 z-10'>
                <CheckCircle2 className='size-4' />
              </div>
              <Select
                value={isActive}
                onValueChange={(val) => setIsActive(val)}
              >
                <SelectTrigger className='w-full bg-background shadow-sm border-muted-foreground/20 focus:border-primary/50 transition-colors pl-10 [&>span]:pl-0 !h-11'>
                  <SelectValue placeholder={t('users.statusPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value='all'>{t('users.statusAll')}</SelectItem>
                  <SelectItem value='true'>{t('users.statusActive')}</SelectItem>
                  <SelectItem value='false'>{t('users.statusInactive')}</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default UsersFilters;

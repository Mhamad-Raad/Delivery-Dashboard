import { useState, useEffect, useRef } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Search, X, History, Filter } from 'lucide-react';

const HistoryFilters = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  // State
  const [entityName, setEntityName] = useState(
    () => searchParams.get('entityName') || ''
  );
  const [typedEntityName, setTypedEntityName] = useState(entityName);

  const [fromDate, setFromDate] = useState(
    () => searchParams.get('fromDate') || ''
  );
  const [toDate, setToDate] = useState(() => searchParams.get('toDate') || '');

  // Debounce for entity name search
  const debounceRef = useRef<any>(null);

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current);
    debounceRef.current = setTimeout(() => {
      setEntityName(typedEntityName);
    }, 300);
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    };
  }, [typedEntityName]);

  // Update URL
  useEffect(() => {
    const params = new URLSearchParams(searchParams);

    if (entityName) {
      params.set('entityName', entityName);
    } else {
      params.delete('entityName');
    }

    if (fromDate) {
      params.set('fromDate', fromDate);
    } else {
      params.delete('fromDate');
    }

    if (toDate) {
      params.set('toDate', toDate);
    } else {
      params.delete('toDate');
    }

    // Reset page to 1 when filters change
    params.set('page', '1');

    navigate(`${window.location.pathname}?${params.toString()}`, {
      replace: true,
    });
  }, [entityName, fromDate, toDate, navigate]); // Removed searchParams to avoid infinite loop

  const clearFilters = () => {
    setTypedEntityName('');
    setEntityName('');
    setFromDate('');
    setToDate('');
  };

  const hasActiveFilters = entityName || fromDate || toDate;

  return (
    <div className='flex flex-col gap-5'>
      {/* Header */}
      <div className='flex flex-col lg:flex-row lg:items-center justify-between gap-4'>
        <div className='flex items-center gap-4'>
          <div className='relative'>
            <div className='absolute inset-0 bg-gradient-to-br from-primary to-primary/50 rounded-2xl blur-lg opacity-40' />
            <div className='relative p-3.5 rounded-2xl bg-gradient-to-br from-primary to-primary/80 text-primary-foreground shadow-lg'>
              <History className='size-7' />
            </div>
          </div>
          <div>
            <div className='flex items-center gap-2'>
              <h1 className='text-3xl font-bold tracking-tight'>
                Audit History
              </h1>
            </div>
            <p className='text-muted-foreground mt-0.5'>
              View system audit logs and history
            </p>
          </div>
        </div>
      </div>

      {/* Filters Bar */}
      <div className='flex flex-col md:flex-row items-start md:items-center gap-4 p-4 rounded-2xl border bg-card/50 backdrop-blur-sm shadow-sm hover:shadow-md transition-all duration-300 relative z-30'>
        <div className='flex items-center gap-2 text-muted-foreground w-full md:w-auto'>
          <Filter className='size-4' />
          <span className='text-sm font-medium'>Filters</span>
          {hasActiveFilters && (
            <div className='h-4 w-px bg-border mx-2 hidden md:block' />
          )}
        </div>

        <div className='grid grid-cols-1 sm:grid-cols-2 lg:flex lg:items-center gap-3 w-full md:w-auto flex-1'>
          <div className='relative w-full lg:max-w-[240px]'>
            <Search className='absolute left-3 top-1/2 -translate-y-1/2 size-4 text-muted-foreground' />
            <Input
              placeholder='Search by Entity Name...'
              value={typedEntityName}
              onChange={(e) => setTypedEntityName(e.target.value)}
              className='pl-9 h-10 bg-background/80 border-border/50 focus-visible:border-primary/50 rounded-xl w-full'
            />
          </div>

          <div className='flex items-center gap-2 w-full lg:w-auto'>
            <span className='text-sm text-muted-foreground whitespace-nowrap min-w-[32px]'>
              From:
            </span>
            <Input
              type='datetime-local'
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              className='h-10 bg-background/80 border-border/50 focus-visible:border-primary/50 rounded-xl w-full'
            />
          </div>

          <div className='flex items-center gap-2 w-full lg:w-auto'>
            <span className='text-sm text-muted-foreground whitespace-nowrap min-w-[32px]'>
              To:
            </span>
            <Input
              type='datetime-local'
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              className='h-10 bg-background/80 border-border/50 focus-visible:border-primary/50 rounded-xl w-full'
            />
          </div>
        </div>

        {hasActiveFilters && (
          <Button
            variant='ghost'
            size='sm'
            onClick={clearFilters}
            className='text-muted-foreground hover:text-foreground hover:bg-muted/50 w-full md:w-auto justify-center'
          >
            <X className='mr-2 size-3.5' />
            Clear
          </Button>
        )}
      </div>
    </div>
  );
};

export default HistoryFilters;


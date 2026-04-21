import { useCallback, useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { AlertTriangle, RefreshCw } from 'lucide-react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

import { fetchAdminAnalytics } from '@/data/Reports';
import type { AdminAnalyticsResponse, Granularity } from '@/interfaces/Reports.interface';

import FinancialSection from '@/components/Reports/FinancialSection';
import OrdersSection from '@/components/Reports/OrdersSection';
import VendorsSection from '@/components/Reports/VendorsSection';
import DriversSection from '@/components/Reports/DriversSection';
import CustomersSection from '@/components/Reports/CustomersSection';
import SupportSection from '@/components/Reports/SupportSection';

const DAY_MS = 86_400_000;

const startOfDay = (d: Date) => {
  const c = new Date(d);
  c.setHours(0, 0, 0, 0);
  return c;
};

const endOfDay = (d: Date) => {
  const c = new Date(d);
  c.setHours(23, 59, 59, 999);
  return c;
};

const toInputDate = (d: Date) => {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
};

const parseInputDate = (s: string): Date | null => {
  if (!s) return null;
  const [y, m, d] = s.split('-').map(Number);
  if (!y || !m || !d) return null;
  return new Date(y, m - 1, d);
};

const defaultRangeFor = (g: Granularity): { from: Date; to: Date } => {
  const now = new Date();
  const to = endOfDay(now);
  let from: Date;
  if (g === 'week') {
    from = startOfDay(new Date(now.getTime() - 83 * DAY_MS));
  } else if (g === 'month') {
    from = startOfDay(new Date(now.getFullYear() - 1, now.getMonth(), now.getDate()));
  } else {
    from = startOfDay(new Date(now.getTime() - 29 * DAY_MS));
  }
  return { from, to };
};

const Reports = () => {
  const { t } = useTranslation('reports');

  const initial = defaultRangeFor('day');
  const [granularity, setGranularity] = useState<Granularity>('day');
  const [from, setFrom] = useState<Date>(initial.from);
  const [to, setTo] = useState<Date>(initial.to);

  const [data, setData] = useState<AdminAnalyticsResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const abortRef = useRef<AbortController | null>(null);

  const load = useCallback(
    async (showRefresh = false) => {
      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;

      if (showRefresh) setIsRefreshing(true);
      else setIsLoading(true);
      setError(null);

      const res = await fetchAdminAnalytics({
        from,
        to,
        granularity,
        signal: controller.signal,
      });

      if (controller.signal.aborted) return;

      if ('error' in res) {
        if (res.error !== 'canceled') {
          setError(res.error || t('error'));
          setData(null);
        }
      } else {
        setData(res);
      }

      setIsLoading(false);
      setIsRefreshing(false);
    },
    [from, to, granularity, t]
  );

  useEffect(() => {
    load();
    return () => abortRef.current?.abort();
  }, [load]);

  const handleGranularityChange = (g: Granularity) => {
    if (g === granularity) return;
    setGranularity(g);
    const range = defaultRangeFor(g);
    setFrom(range.from);
    setTo(range.to);
  };

  const handleFromChange = (value: string) => {
    const parsed = parseInputDate(value);
    if (parsed) setFrom(startOfDay(parsed));
  };

  const handleToChange = (value: string) => {
    const parsed = parseInputDate(value);
    if (parsed) setTo(endOfDay(parsed));
  };

  return (
    <div className='flex flex-col gap-6 p-6'>
      <div className='flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3'>
        <div>
          <h1 className='text-2xl font-bold tracking-tight'>{t('title')}</h1>
          <p className='text-sm text-muted-foreground'>{t('subtitle')}</p>
        </div>
        <Button
          variant='outline'
          size='sm'
          onClick={() => load(true)}
          disabled={isLoading || isRefreshing}
        >
          <RefreshCw className={`size-4 mr-2 ${isRefreshing ? 'animate-spin' : ''}`} />
          {t('filters.refresh')}
        </Button>
      </div>

      <Card>
        <CardContent className='p-4 flex flex-col md:flex-row md:items-end gap-4'>
          <div className='flex flex-col gap-1'>
            <label className='text-xs text-muted-foreground'>{t('filters.granularity')}</label>
            <Select value={granularity} onValueChange={(v) => handleGranularityChange(v as Granularity)}>
              <SelectTrigger className='w-[140px]'>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value='day'>{t('filters.day')}</SelectItem>
                <SelectItem value='week'>{t('filters.week')}</SelectItem>
                <SelectItem value='month'>{t('filters.month')}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className='flex flex-col gap-1'>
            <label className='text-xs text-muted-foreground'>{t('filters.from')}</label>
            <Input
              type='date'
              value={toInputDate(from)}
              onChange={(e) => handleFromChange(e.target.value)}
              max={toInputDate(to)}
              className='w-[170px]'
            />
          </div>
          <div className='flex flex-col gap-1'>
            <label className='text-xs text-muted-foreground'>{t('filters.to')}</label>
            <Input
              type='date'
              value={toInputDate(to)}
              onChange={(e) => handleToChange(e.target.value)}
              min={toInputDate(from)}
              className='w-[170px]'
            />
          </div>
        </CardContent>
      </Card>

      {error ? (
        <Card>
          <CardContent className='p-8 flex flex-col items-center text-center gap-3'>
            <AlertTriangle className='size-8 text-destructive' />
            <div>
              <p className='font-medium'>{t('errorTitle')}</p>
              <p className='text-sm text-muted-foreground mt-1'>{error}</p>
            </div>
            <Button size='sm' onClick={() => load(true)} disabled={isRefreshing}>
              <RefreshCw className={`size-4 mr-2 ${isRefreshing ? 'animate-spin' : ''}`} />
              {t('actions.retry')}
            </Button>
          </CardContent>
        </Card>
      ) : (
        <>
          <FinancialSection
            data={data?.financial}
            granularity={granularity}
            from={from}
            to={to}
            isLoading={isLoading}
          />
          <OrdersSection
            data={data?.orders}
            granularity={granularity}
            from={from}
            to={to}
            isLoading={isLoading}
          />
          <VendorsSection data={data?.vendors} from={from} to={to} isLoading={isLoading} />
          <DriversSection data={data?.drivers} from={from} to={to} isLoading={isLoading} />
          <CustomersSection
            data={data?.customers}
            granularity={granularity}
            from={from}
            to={to}
            isLoading={isLoading}
          />
          <SupportSection data={data?.support} from={from} to={to} isLoading={isLoading} />
        </>
      )}
    </div>
  );
};

export default Reports;

import { useTranslation } from 'react-i18next';
import { DollarSign } from 'lucide-react';
import { Bar, BarChart, CartesianGrid, Line, LineChart, XAxis, YAxis } from 'recharts';
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from '@/components/ui/chart';
import { ReportSection } from './ReportSection';
import { formatBucket, formatCurrency } from './formatters';
import type { FinancialSection as FinancialSectionData, Granularity } from '@/interfaces/Reports.interface';

interface Props {
  data?: FinancialSectionData;
  granularity: Granularity;
  from: Date;
  to: Date;
  isLoading?: boolean;
}

const chartConfig = {
  revenue: { label: 'Revenue', color: 'var(--chart-1)' },
} satisfies ChartConfig;

const FinancialSection = ({ data, granularity, from, to, isLoading }: Props) => {
  const { t } = useTranslation('reports');

  const isEmpty = !data || (data.revenueSeries.length === 0 && data.totalRevenue === 0);

  const seriesData = data?.revenueSeries.map((p) => ({
    label: formatBucket(p.bucket, granularity),
    revenue: p.value,
  })) ?? [];

  const categoryData = data?.revenueByCategory.map((c) => ({
    name: c.name,
    amount: c.amount,
  })) ?? [];

  return (
    <ReportSection
      title={t('sections.financial.title')}
      icon={<DollarSign className='size-5 text-primary' />}
      isLoading={isLoading}
      isEmpty={isEmpty}
      csvKind='orders'
      from={from}
      to={to}
    >
      {data && (
        <div className='flex flex-col gap-6'>
          <div className='grid grid-cols-1 md:grid-cols-3 gap-4'>
            <Kpi label={t('sections.financial.totalRevenue')} value={formatCurrency(data.totalRevenue)} />
            <Kpi label={t('sections.financial.gmv')} value={formatCurrency(data.gmv)} />
            <Kpi label={t('sections.financial.avgOrderValue')} value={formatCurrency(data.avgOrderValue)} />
          </div>

          {seriesData.length > 0 && (
            <div>
              <p className='text-sm font-medium mb-2'>{t('sections.financial.revenueOverTime')}</p>
              <ChartContainer config={chartConfig} className='h-[220px] w-full'>
                <LineChart data={seriesData}>
                  <CartesianGrid strokeDasharray='3 3' vertical={false} />
                  <XAxis dataKey='label' tickLine={false} axisLine={false} />
                  <YAxis tickLine={false} axisLine={false} width={60} />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Line type='monotone' dataKey='revenue' stroke='var(--chart-1)' strokeWidth={2} dot={false} />
                </LineChart>
              </ChartContainer>
            </div>
          )}

          {categoryData.length > 0 && (
            <div>
              <p className='text-sm font-medium mb-2'>{t('sections.financial.revenueByCategory')}</p>
              <ChartContainer config={{ amount: { label: 'Revenue', color: 'var(--chart-2)' } }} className='h-[220px] w-full'>
                <BarChart data={categoryData}>
                  <CartesianGrid strokeDasharray='3 3' vertical={false} />
                  <XAxis dataKey='name' tickLine={false} axisLine={false} />
                  <YAxis tickLine={false} axisLine={false} width={60} />
                  <ChartTooltip content={<ChartTooltipContent />} />
                  <Bar dataKey='amount' fill='var(--chart-2)' radius={4} />
                </BarChart>
              </ChartContainer>
            </div>
          )}
        </div>
      )}
    </ReportSection>
  );
};

const Kpi = ({ label, value }: { label: string; value: string }) => (
  <div className='p-4 rounded-lg bg-muted/30'>
    <p className='text-xs text-muted-foreground'>{label}</p>
    <p className='text-2xl font-bold tabular-nums mt-1'>{value}</p>
  </div>
);

export default FinancialSection;

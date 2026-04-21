import { useTranslation } from 'react-i18next';
import { Users } from 'lucide-react';
import { CartesianGrid, Line, LineChart, XAxis, YAxis } from 'recharts';
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from '@/components/ui/chart';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { ReportSection } from './ReportSection';
import { formatBucket, formatCurrency, formatNumber } from './formatters';
import type {
  CustomersSection as CustomersSectionData,
  Granularity,
} from '@/interfaces/Reports.interface';

interface Props {
  data?: CustomersSectionData;
  granularity: Granularity;
  from: Date;
  to: Date;
  isLoading?: boolean;
}

const chartConfig = {
  signups: { label: 'Signups', color: 'var(--chart-3)' },
} satisfies ChartConfig;

const CustomersSection = ({ data, granularity, from, to, isLoading }: Props) => {
  const { t } = useTranslation('reports');

  const isEmpty =
    !data ||
    (data.newSignups === 0 &&
      data.topSpenders.length === 0 &&
      data.returning === 0 &&
      data.oneTime === 0);

  const seriesData = data?.signupSeries.map((p) => ({
    label: formatBucket(p.bucket, granularity),
    signups: p.value,
  })) ?? [];

  return (
    <ReportSection
      title={t('sections.customers.title')}
      icon={<Users className='size-5 text-primary' />}
      isLoading={isLoading}
      isEmpty={isEmpty}
      csvKind='customers'
      from={from}
      to={to}
    >
      {data && (
        <div className='flex flex-col gap-6'>
          <div className='grid grid-cols-1 md:grid-cols-3 gap-4'>
            <Kpi label={t('sections.customers.newSignups')} value={formatNumber(data.newSignups)} />
            <Kpi label={t('sections.customers.returning')} value={formatNumber(data.returning)} />
            <Kpi label={t('sections.customers.oneTime')} value={formatNumber(data.oneTime)} />
          </div>

          <div className='grid grid-cols-1 lg:grid-cols-2 gap-6'>
            {seriesData.length > 0 && (
              <div>
                <p className='text-sm font-medium mb-2'>{t('sections.customers.signupsOverTime')}</p>
                <ChartContainer config={chartConfig} className='h-[220px] w-full'>
                  <LineChart data={seriesData}>
                    <CartesianGrid strokeDasharray='3 3' vertical={false} />
                    <XAxis dataKey='label' tickLine={false} axisLine={false} />
                    <YAxis tickLine={false} axisLine={false} width={40} />
                    <ChartTooltip content={<ChartTooltipContent />} />
                    <Line type='monotone' dataKey='signups' stroke='var(--chart-3)' strokeWidth={2} dot={false} />
                  </LineChart>
                </ChartContainer>
              </div>
            )}

            <div>
              <p className='text-sm font-medium mb-2'>{t('sections.customers.topSpenders')}</p>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>#</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead className='text-right'>{t('sections.customers.orderCount')}</TableHead>
                    <TableHead className='text-right'>{t('sections.customers.totalSpent')}</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.topSpenders.map((c, idx) => (
                    <TableRow key={c.customerId}>
                      <TableCell className='text-muted-foreground tabular-nums'>{idx + 1}</TableCell>
                      <TableCell className='font-medium'>{c.name}</TableCell>
                      <TableCell className='text-right tabular-nums'>{formatNumber(c.orderCount)}</TableCell>
                      <TableCell className='text-right tabular-nums'>{formatCurrency(c.totalSpent)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          </div>
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

export default CustomersSection;

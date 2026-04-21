import { useTranslation } from 'react-i18next';
import { ShoppingCart } from 'lucide-react';
import { CartesianGrid, Cell, Line, LineChart, Pie, PieChart, XAxis, YAxis } from 'recharts';
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from '@/components/ui/chart';
import { ReportSection } from './ReportSection';
import { formatBucket, formatNumber, formatPercent } from './formatters';
import type {
  OrdersSection as OrdersSectionData,
  Granularity,
} from '@/interfaces/Reports.interface';

interface Props {
  data?: OrdersSectionData;
  granularity: Granularity;
  from: Date;
  to: Date;
  isLoading?: boolean;
}

const chartConfig = {
  orders: { label: 'Orders', color: 'var(--chart-1)' },
} satisfies ChartConfig;

const OrdersSection = ({ data, granularity, from, to, isLoading }: Props) => {
  const { t } = useTranslation('reports');

  const isEmpty = !data || data.total === 0;

  const seriesData = data?.ordersSeries.map((p) => ({
    label: formatBucket(p.bucket, granularity),
    orders: p.value,
  })) ?? [];

  const donutData = data?.statusBreakdown.map((s, i) => ({
    name: t(`sections.orders.statuses.${s.status}`, s.status),
    value: s.count,
    fill: `var(--chart-${(i % 5) + 1})`,
  })) ?? [];

  return (
    <ReportSection
      title={t('sections.orders.title')}
      icon={<ShoppingCart className='size-5 text-primary' />}
      isLoading={isLoading}
      isEmpty={isEmpty}
      csvKind='orders'
      from={from}
      to={to}
    >
      {data && (
        <div className='flex flex-col gap-6'>
          <div className='grid grid-cols-1 md:grid-cols-2 gap-4'>
            <Kpi label={t('sections.orders.total')} value={formatNumber(data.total)} />
            <Kpi label={t('sections.orders.cancellationRate')} value={formatPercent(data.cancellationRate)} />
          </div>

          <div className='grid grid-cols-1 lg:grid-cols-2 gap-6'>
            {seriesData.length > 0 && (
              <div>
                <p className='text-sm font-medium mb-2'>{t('sections.orders.ordersOverTime')}</p>
                <ChartContainer config={chartConfig} className='h-[220px] w-full'>
                  <LineChart data={seriesData}>
                    <CartesianGrid strokeDasharray='3 3' vertical={false} />
                    <XAxis dataKey='label' tickLine={false} axisLine={false} />
                    <YAxis tickLine={false} axisLine={false} width={40} />
                    <ChartTooltip content={<ChartTooltipContent />} />
                    <Line type='monotone' dataKey='orders' stroke='var(--chart-1)' strokeWidth={2} dot={false} />
                  </LineChart>
                </ChartContainer>
              </div>
            )}

            {donutData.length > 0 && (
              <div>
                <p className='text-sm font-medium mb-2'>{t('sections.orders.statusBreakdown')}</p>
                <ChartContainer config={{}} className='mx-auto aspect-square max-h-[240px]'>
                  <PieChart>
                    <ChartTooltip content={<ChartTooltipContent />} />
                    <Pie data={donutData} dataKey='value' nameKey='name' innerRadius={50} outerRadius={90} strokeWidth={3}>
                      {donutData.map((entry, index) => (
                        <Cell key={index} fill={entry.fill} />
                      ))}
                    </Pie>
                  </PieChart>
                </ChartContainer>
                <div className='mt-2 grid grid-cols-2 gap-1 text-xs'>
                  {donutData.map((d, i) => (
                    <div key={i} className='flex items-center gap-2'>
                      <span className='size-2.5 rounded-full' style={{ backgroundColor: d.fill }} />
                      <span className='truncate'>{d.name}</span>
                      <span className='ml-auto tabular-nums'>{d.value}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
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

export default OrdersSection;

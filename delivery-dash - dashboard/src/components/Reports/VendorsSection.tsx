import { useTranslation } from 'react-i18next';
import { Store } from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { ReportSection } from './ReportSection';
import { formatCurrency, formatNumber } from './formatters';
import type { VendorsSection as VendorsSectionData } from '@/interfaces/Reports.interface';

interface Props {
  data?: VendorsSectionData;
  from: Date;
  to: Date;
  isLoading?: boolean;
}

const VendorsSection = ({ data, from, to, isLoading }: Props) => {
  const { t } = useTranslation('reports');

  const isEmpty =
    !data ||
    (data.topByRevenue.length === 0 &&
      data.topByOrders.length === 0 &&
      data.activeCount === 0 &&
      data.inactiveCount === 0);

  return (
    <ReportSection
      title={t('sections.vendors.title')}
      icon={<Store className='size-5 text-primary' />}
      isLoading={isLoading}
      isEmpty={isEmpty}
      csvKind='vendors'
      from={from}
      to={to}
    >
      {data && (
        <div className='flex flex-col gap-6'>
          <div className='grid grid-cols-1 md:grid-cols-3 gap-4'>
            <Kpi label={t('sections.vendors.newSignups')} value={formatNumber(data.newSignups)} />
            <Kpi label={t('sections.vendors.active')} value={formatNumber(data.activeCount)} />
            <Kpi label={t('sections.vendors.inactive')} value={formatNumber(data.inactiveCount)} />
          </div>

          <div className='grid grid-cols-1 lg:grid-cols-2 gap-6'>
            <RankTable
              title={t('sections.vendors.topByRevenue')}
              rows={data.topByRevenue.map((v) => ({
                id: v.vendorId,
                name: v.name,
                sub: v.categoryName,
                a: formatNumber(v.orderCount),
                b: formatCurrency(v.revenue),
              }))}
              aLabel={t('sections.vendors.orderCount')}
              bLabel={t('sections.vendors.revenue')}
            />
            <RankTable
              title={t('sections.vendors.topByOrders')}
              rows={data.topByOrders.map((v) => ({
                id: v.vendorId,
                name: v.name,
                sub: v.categoryName,
                a: formatNumber(v.orderCount),
                b: formatCurrency(v.revenue),
              }))}
              aLabel={t('sections.vendors.orderCount')}
              bLabel={t('sections.vendors.revenue')}
            />
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

interface RankRow {
  id: number | string;
  name: string;
  sub?: string;
  a: string;
  b: string;
}

const RankTable = ({
  title,
  rows,
  aLabel,
  bLabel,
}: {
  title: string;
  rows: RankRow[];
  aLabel: string;
  bLabel: string;
}) => (
  <div>
    <p className='text-sm font-medium mb-2'>{title}</p>
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>#</TableHead>
          <TableHead>Name</TableHead>
          <TableHead className='text-right'>{aLabel}</TableHead>
          <TableHead className='text-right'>{bLabel}</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((r, idx) => (
          <TableRow key={r.id}>
            <TableCell className='text-muted-foreground tabular-nums'>{idx + 1}</TableCell>
            <TableCell>
              <div className='font-medium'>{r.name}</div>
              {r.sub && <div className='text-xs text-muted-foreground'>{r.sub}</div>}
            </TableCell>
            <TableCell className='text-right tabular-nums'>{r.a}</TableCell>
            <TableCell className='text-right tabular-nums'>{r.b}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  </div>
);

export default VendorsSection;

import { useTranslation } from 'react-i18next';
import { Bike } from 'lucide-react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { ReportSection } from './ReportSection';
import { formatMinutes, formatNumber } from './formatters';
import type { DriversSection as DriversSectionData } from '@/interfaces/Reports.interface';

interface Props {
  data?: DriversSectionData;
  from: Date;
  to: Date;
  isLoading?: boolean;
}

const DriversSection = ({ data, from, to, isLoading }: Props) => {
  const { t } = useTranslation('reports');

  const isEmpty =
    !data || (data.topByDeliveries.length === 0 && data.activeCount === 0);

  return (
    <ReportSection
      title={t('sections.drivers.title')}
      icon={<Bike className='size-5 text-primary' />}
      isLoading={isLoading}
      isEmpty={isEmpty}
      csvKind='drivers'
      from={from}
      to={to}
    >
      {data && (
        <div className='flex flex-col gap-6'>
          <div className='grid grid-cols-1 md:grid-cols-2 gap-4'>
            <Kpi label={t('sections.drivers.activeDrivers')} value={formatNumber(data.activeCount)} />
            <Kpi
              label={t('sections.drivers.avgDeliveryTime')}
              value={data.avgDeliveryMinutes > 0 ? formatMinutes(data.avgDeliveryMinutes) : '—'}
            />
          </div>

          <div>
            <p className='text-sm font-medium mb-2'>{t('sections.drivers.topByDeliveries')}</p>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>#</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className='text-right'>{t('sections.drivers.deliveries')}</TableHead>
                  <TableHead className='text-right'>{t('sections.drivers.avgMinutes')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.topByDeliveries.map((d, idx) => (
                  <TableRow key={d.driverId}>
                    <TableCell className='text-muted-foreground tabular-nums'>{idx + 1}</TableCell>
                    <TableCell className='font-medium'>{d.name}</TableCell>
                    <TableCell className='text-right tabular-nums'>{formatNumber(d.deliveries)}</TableCell>
                    <TableCell className='text-right tabular-nums'>{formatMinutes(d.avgDeliveryMinutes)}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
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

export default DriversSection;

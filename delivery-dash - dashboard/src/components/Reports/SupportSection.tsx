import { useTranslation } from 'react-i18next';
import { LifeBuoy } from 'lucide-react';
import { ReportSection } from './ReportSection';
import { formatNumber } from './formatters';
import type { SupportSection as SupportSectionData } from '@/interfaces/Reports.interface';

interface Props {
  data?: SupportSectionData;
  from: Date;
  to: Date;
  isLoading?: boolean;
}

const SupportSection = ({ data, from, to, isLoading }: Props) => {
  const { t } = useTranslation('reports');

  const isEmpty =
    !data ||
    (data.opened === 0 &&
      data.resolved === 0 &&
      data.openBacklog === 0 &&
      data.avgResolutionHours === 0);

  return (
    <ReportSection
      title={t('sections.support.title')}
      icon={<LifeBuoy className='size-5 text-primary' />}
      isLoading={isLoading}
      isEmpty={isEmpty}
      csvKind='tickets'
      from={from}
      to={to}
    >
      {data && (
        <div className='grid grid-cols-2 md:grid-cols-4 gap-4'>
          <Kpi label={t('sections.support.opened')} value={formatNumber(data.opened)} />
          <Kpi label={t('sections.support.resolved')} value={formatNumber(data.resolved)} />
          <Kpi label={t('sections.support.openBacklog')} value={formatNumber(data.openBacklog)} />
          <Kpi
            label={t('sections.support.avgResolution')}
            value={data.avgResolutionHours > 0 ? data.avgResolutionHours.toFixed(1) : '—'}
          />
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

export default SupportSection;

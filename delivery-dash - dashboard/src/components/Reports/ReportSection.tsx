import type { ReactNode } from 'react';
import { useState } from 'react';
import { Download, Loader2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';
import { downloadCsv, type CsvKind } from '@/data/Reports';

interface ReportSectionProps {
  title: string;
  description?: string;
  icon?: ReactNode;
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyMessage?: string;
  csvKind?: CsvKind;
  from: Date;
  to: Date;
  children: ReactNode;
}

export const ReportSection = ({
  title,
  description,
  icon,
  isLoading,
  isEmpty,
  emptyMessage,
  csvKind,
  from,
  to,
  children,
}: ReportSectionProps) => {
  const { t } = useTranslation('reports');
  const [busy, setBusy] = useState(false);

  const handleExport = async () => {
    if (!csvKind) return;
    setBusy(true);
    try {
      const res = await downloadCsv(csvKind, { from, to });
      if (res && 'error' in res) toast.error(res.error);
    } finally {
      setBusy(false);
    }
  };

  return (
    <Card>
      <CardHeader className='flex flex-row items-start justify-between gap-4 space-y-0'>
        <div className='space-y-1'>
          <CardTitle className='flex items-center gap-2 text-lg'>
            {icon}
            {title}
          </CardTitle>
          {description && <CardDescription>{description}</CardDescription>}
        </div>
        {csvKind && (
          <Button variant='outline' size='sm' onClick={handleExport} disabled={busy || isLoading}>
            {busy ? (
              <Loader2 className='size-4 mr-2 animate-spin' />
            ) : (
              <Download className='size-4 mr-2' />
            )}
            {busy ? t('actions.downloading') : t('actions.exportCsv')}
          </Button>
        )}
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <SectionSkeleton />
        ) : isEmpty ? (
          <div className='flex items-center justify-center h-40 text-muted-foreground text-sm'>
            {emptyMessage ?? t('noData')}
          </div>
        ) : (
          children
        )}
      </CardContent>
    </Card>
  );
};

const SectionSkeleton = () => (
  <div className='flex flex-col gap-6'>
    <div className='grid grid-cols-1 md:grid-cols-3 gap-4'>
      {Array.from({ length: 3 }).map((_, i) => (
        <div key={i} className='p-4 rounded-lg bg-muted/30 space-y-2'>
          <Skeleton className='h-3 w-24' />
          <Skeleton className='h-7 w-32' />
        </div>
      ))}
    </div>
    <Skeleton className='h-[220px] w-full' />
  </div>
);

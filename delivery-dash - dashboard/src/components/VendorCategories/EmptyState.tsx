import { useTranslation } from 'react-i18next';
import { Tags, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface Props {
  onCreate: () => void;
}

export default function EmptyState({ onCreate }: Props) {
  const { t } = useTranslation('vendorCategories');

  return (
    <div className='flex flex-col items-center justify-center h-full py-16 px-6 rounded-2xl border-2 border-dashed bg-card/30'>
      <div className='relative mb-5'>
        <div className='absolute inset-0 bg-gradient-to-br from-primary to-primary/50 rounded-2xl blur-xl opacity-40' />
        <div className='relative p-5 rounded-2xl bg-gradient-to-br from-primary/20 to-primary/5 border border-primary/20 text-primary'>
          <Tags className='size-10' />
        </div>
      </div>
      <h3 className='text-xl font-bold tracking-tight mb-1.5 text-center'>
        {t('emptyState.title')}
      </h3>
      <p className='text-sm text-muted-foreground text-center max-w-md mb-6'>
        {t('emptyState.description')}
      </p>
      <Button onClick={onCreate} className='gap-2'>
        <Plus className='size-4' />
        {t('emptyState.action')}
      </Button>
    </div>
  );
}

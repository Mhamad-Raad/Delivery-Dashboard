import { User as UserIcon } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const EmptyState = () => {
  const { t } = useTranslation();

  return (
    <div className='flex flex-col items-center justify-center h-full text-center p-8'>
      <UserIcon className='w-12 h-12 text-muted-foreground mb-4' />
      <h2 className='text-xl font-semibold mb-2'>
        {t('users.emptyTitle')}
      </h2>
      <p className='text-muted-foreground mb-4'>
        {t('users.emptyDescription')}
      </p>
    </div>
  );
};

export default EmptyState;

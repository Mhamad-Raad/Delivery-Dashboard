import { Skeleton } from '@/components/ui/skeleton';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const UserDetailSkeleton = () => {
  const { t } = useTranslation();

  return (
    <div className='flex flex-col gap-6 p-4 md:p-6'>
      <div className='flex items-center justify-between mb-6'>
        <Button variant='ghost' disabled>
          <ArrowLeft className='mr-2 h-4 w-4' />
          {t('users.backToUsers')}
        </Button>
        <div className='flex gap-2'>
          <Skeleton className='h-10 w-32 rounded-md' />
          <Skeleton className='h-10 w-32 rounded-md' />
        </div>
      </div>

      <div className='mb-6'>
        <div className='flex flex-col md:flex-row items-start md:items-center gap-6'>
          <Skeleton className='h-24 w-24 rounded-full mb-2' />
          <div className='flex-1 w-full flex flex-col gap-3 mb-2'>
            <Skeleton className='h-6 w-40' />
            <Skeleton className='h-6 w-40' />
            <Skeleton className='h-4 w-32' />
            <Skeleton className='h-8 w-32 rounded-md' />
          </div>
        </div>
      </div>

      <div className='grid gap-6 lg:grid-cols-2'>
        <div>
          <Skeleton className='h-6 w-1/2 mb-4' />
          <Skeleton className='h-5 w-2/3 mb-2' />
          <Skeleton className='h-5 w-40 mb-2' />
          <Skeleton className='h-5 w-2/3 mb-2' />
          <Skeleton className='h-5 w-32 mb-2' />
        </div>
        <div>
          <Skeleton className='h-6 w-2/3 mb-4' />
          <Skeleton className='h-5 w-32 mb-2' />
          <Skeleton className='h-5 w-16 mb-2' />
          <Skeleton className='h-6 w-24 rounded-full' />
        </div>
        <div className='lg:col-span-2'>
          <Skeleton className='h-6 w-1/2 mb-4' />
          <div className='grid gap-6 md:grid-cols-3'>
            <Skeleton className='h-10 w-full' />
            <Skeleton className='h-10 w-full' />
            <Skeleton className='h-10 w-full' />
          </div>
        </div>
      </div>
    </div>
  );
};

export default UserDetailSkeleton;

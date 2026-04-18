import { Card, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';

export default function ProductDetailSkeleton() {
  return (
    <div className='flex flex-col gap-6 p-4 md:p-6'>
      {/* Header Skeleton */}
      <div className='flex flex-col sm:flex-row sm:items-center justify-between gap-4'>
        <div className='flex items-center gap-3'>
          <Skeleton className='size-9 rounded-md' />
          <div className='flex items-center gap-3'>
            <Skeleton className='size-10 rounded-lg' />
            <div className='space-y-2'>
              <Skeleton className='h-5 w-40' />
              <Skeleton className='h-4 w-52' />
            </div>
          </div>
        </div>
        <div className='flex items-center gap-2'>
          <Skeleton className='h-8 w-20' />
          <Skeleton className='h-8 w-28' />
        </div>
      </div>

      {/* Main Content Grid */}
      <div className='grid grid-cols-1 lg:grid-cols-3 gap-6'>
        {/* Left Column - Image Card */}
        <Card className='lg:col-span-1'>
          <CardContent className='p-4 space-y-4'>
            <Skeleton className='h-4 w-28' />
            <Skeleton className='aspect-square w-full rounded-lg' />
            <Skeleton className='h-9 w-full' />
            <div className='space-y-2'>
              <Skeleton className='h-3 w-24' />
              <Skeleton className='h-9 w-full' />
            </div>
            <div className='pt-4 border-t space-y-3'>
              <div className='flex justify-between'>
                <Skeleton className='h-4 w-16' />
                <Skeleton className='h-5 w-20 rounded-full' />
              </div>
              <div className='flex justify-between'>
                <Skeleton className='h-4 w-16' />
                <Skeleton className='h-4 w-24' />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Right Column - Form Card */}
        <Card className='lg:col-span-2'>
          <CardContent className='p-4 space-y-6'>
            {/* Basic Info Section */}
            <div className='space-y-4'>
              <Skeleton className='h-4 w-32' />
              <div className='space-y-4'>
                <div className='space-y-2'>
                  <Skeleton className='h-4 w-24' />
                  <Skeleton className='h-9 w-full' />
                </div>
                <div className='space-y-2'>
                  <Skeleton className='h-4 w-20' />
                  <Skeleton className='h-24 w-full' />
                </div>
                <div className='space-y-2'>
                  <Skeleton className='h-4 w-20' />
                  <Skeleton className='h-9 w-full' />
                </div>
              </div>
            </div>

            {/* Pricing Section */}
            <div className='space-y-4 pt-4 border-t'>
              <Skeleton className='h-4 w-20' />
              <div className='grid sm:grid-cols-2 gap-4'>
                <div className='space-y-2'>
                  <Skeleton className='h-4 w-24' />
                  <Skeleton className='h-9 w-full' />
                </div>
                <div className='space-y-2'>
                  <Skeleton className='h-4 w-20' />
                  <Skeleton className='h-9 w-full' />
                </div>
              </div>
            </div>

            {/* Options Section */}
            <div className='space-y-4 pt-4 border-t'>
              <Skeleton className='h-4 w-28' />
              <div className='grid sm:grid-cols-2 gap-4'>
                <Skeleton className='h-20 w-full rounded-lg' />
                <Skeleton className='h-20 w-full rounded-lg' />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

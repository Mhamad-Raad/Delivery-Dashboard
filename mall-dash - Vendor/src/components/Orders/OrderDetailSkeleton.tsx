import { Skeleton } from '@/components/ui/skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';

export const OrderDetailSkeleton = () => {
  return (
    <div className='flex flex-col gap-6 h-full overflow-hidden'>
      {/* Header Skeleton */}
      <div className='flex items-center justify-between'>
        <div className='flex items-center gap-4'>
          <Skeleton className='h-8 w-8 rounded-full' />
          <div>
            <Skeleton className='h-8 w-48 mb-2' />
            <div className='flex items-center gap-2'>
              <Skeleton className='h-4 w-24' />
              <Skeleton className='h-4 w-4 rounded-full' />
              <Skeleton className='h-4 w-24' />
            </div>
          </div>
        </div>

        <div className='flex items-center gap-3'>
          <Skeleton className='h-9 w-24' />
          <Skeleton className='h-9 w-24' />
          <Skeleton className='h-6 w-24 rounded-full' />
        </div>
      </div>

      <ScrollArea className='flex-1 -mx-6 px-6'>
        <div className='grid grid-cols-1 lg:grid-cols-3 gap-6 pb-6'>
          {/* Main Content - Left Column */}
          <div className='lg:col-span-2 space-y-6'>
            {/* Order Items Skeleton */}
            <Card>
              <CardHeader>
                <CardTitle className='flex items-center gap-2'>
                  <Skeleton className='h-5 w-5 rounded-full' />
                  <Skeleton className='h-6 w-32' />
                </CardTitle>
              </CardHeader>
              <CardContent className='p-0'>
                <div className='divide-y'>
                  {[1, 2, 3].map((i) => (
                    <div
                      key={i}
                      className='p-4 flex items-center justify-between'
                    >
                      <div className='flex items-center gap-4'>
                        <Skeleton className='h-12 w-12 rounded-lg' />
                        <div className='space-y-2'>
                          <Skeleton className='h-5 w-48' />
                          <Skeleton className='h-4 w-32' />
                        </div>
                      </div>
                      <Skeleton className='h-5 w-20' />
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Payment Summary Skeleton */}
            <Card>
              <CardHeader>
                <CardTitle className='flex items-center gap-2'>
                  <Skeleton className='h-5 w-5 rounded-full' />
                  <Skeleton className='h-6 w-40' />
                </CardTitle>
              </CardHeader>
              <CardContent className='space-y-3'>
                <div className='flex justify-between'>
                  <Skeleton className='h-4 w-20' />
                  <Skeleton className='h-4 w-16' />
                </div>
                <div className='flex justify-between'>
                  <Skeleton className='h-4 w-24' />
                  <Skeleton className='h-4 w-16' />
                </div>
                <Separator />
                <div className='flex justify-between'>
                  <Skeleton className='h-6 w-20' />
                  <Skeleton className='h-6 w-24' />
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Sidebar - Right Column */}
          <div className='space-y-6'>
            {/* Customer Info Skeleton */}
            <Card>
              <CardHeader>
                <CardTitle className='flex items-center gap-2'>
                  <Skeleton className='h-5 w-5 rounded-full' />
                  <Skeleton className='h-6 w-24' />
                </CardTitle>
              </CardHeader>
              <CardContent className='space-y-4'>
                <div className='flex items-center gap-3'>
                  <Skeleton className='h-10 w-10 rounded-full' />
                  <div className='space-y-2'>
                    <Skeleton className='h-5 w-32' />
                    <Skeleton className='h-3 w-20' />
                  </div>
                </div>

                <div className='space-y-2 pt-2'>
                  <div className='flex items-center gap-2'>
                    <Skeleton className='h-4 w-4 rounded-full' />
                    <Skeleton className='h-4 w-40' />
                  </div>
                  <div className='flex items-center gap-2'>
                    <Skeleton className='h-4 w-4 rounded-full' />
                    <Skeleton className='h-4 w-32' />
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Delivery Details Skeleton */}
            <Card>
              <CardHeader>
                <CardTitle className='flex items-center gap-2'>
                  <Skeleton className='h-5 w-5 rounded-full' />
                  <Skeleton className='h-6 w-36' />
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className='space-y-4'>
                  <div className='space-y-1'>
                    <Skeleton className='h-3 w-16 mb-2' />
                    <Skeleton className='h-4 w-32' />
                  </div>
                  <div className='space-y-1'>
                    <Skeleton className='h-3 w-16 mb-2' />
                    <div className='flex flex-col gap-1'>
                      <Skeleton className='h-4 w-48' />
                      <Skeleton className='h-4 w-32' />
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </ScrollArea>
    </div>
  );
};

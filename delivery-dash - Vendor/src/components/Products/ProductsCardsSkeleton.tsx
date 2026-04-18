import { Skeleton } from '@/components/ui/skeleton';

const ProductsCardsSkeleton = () => (
  <div className='bg-card border rounded-xl overflow-hidden shadow-sm'>
    {/* Image Skeleton - Square aspect ratio */}
    <Skeleton className='aspect-square w-full' />

    {/* Content */}
    <div className='p-2.5 space-y-1.5'>
      {/* Category */}
      <Skeleton className='h-4 w-14 rounded-md' />

      {/* Title - 2 lines */}
      <div className='space-y-1 min-h-[2.5rem]'>
        <Skeleton className='h-3.5 w-full' />
        <Skeleton className='h-3.5 w-3/4' />
      </div>

      {/* Price */}
      <div className='pt-1'>
        <Skeleton className='h-4 w-16' />
      </div>
    </div>
  </div>
);

export default ProductsCardsSkeleton;

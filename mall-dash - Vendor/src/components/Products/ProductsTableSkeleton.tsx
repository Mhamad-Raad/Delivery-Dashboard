import { Skeleton } from '@/components/ui/skeleton';
import { TableCell, TableRow } from '@/components/ui/table';

const ProductsTableSkeleton = () => (
  <TableRow className='border-b'>
    {/* Product Info Skeleton */}
    <TableCell className='font-medium py-4 pl-6'>
      <div className='flex items-center gap-4'>
        <Skeleton className='h-16 w-16 rounded-xl shrink-0' />
        <div className='flex flex-col gap-2'>
          <Skeleton className='h-4 w-36' />
          <Skeleton className='h-4 w-12 rounded' />
        </div>
      </div>
    </TableCell>

    {/* Category Skeleton */}
    <TableCell className='py-4'>
      <Skeleton className='h-6 w-20 rounded-full' />
    </TableCell>

    {/* Status Skeleton */}
    <TableCell className='py-4'>
      <Skeleton className='h-6 w-24 rounded-full' />
    </TableCell>

    {/* Price Skeleton */}
    <TableCell className='py-4'>
      <div className='flex flex-col gap-1'>
        <Skeleton className='h-5 w-20' />
        <Skeleton className='h-3 w-16' />
      </div>
    </TableCell>

    {/* ChevronRight Skeleton */}
    <TableCell className='py-4 w-16 pr-6'>
      <Skeleton className='h-8 w-8 rounded-lg' />
    </TableCell>
  </TableRow>
);

export default ProductsTableSkeleton;

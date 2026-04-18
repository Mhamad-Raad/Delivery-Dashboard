import { TrendingDownIcon, TrendingUpIcon, Package, PackageCheck, PackageX, DollarSign, Users, UserCheck, Truck } from 'lucide-react';
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
  CardFooter,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

import { formatCompact, formatPrice } from '@/lib/formatNumbers';

import type { HomeCardsProps } from '@/interfaces/Home.interface';

const cardIcons = {
  'Total Products': Package,
  'In Stock': PackageCheck,
  'Out of Stock': PackageX,
  'Order Revenue': DollarSign,
  'Total Orders': Package,
  'Employees': Users,
  'Staff': UserCheck,
  'Drivers': Truck,
};

const cardIconColors = {
  'Total Products': 'bg-primary/10 text-primary dark:bg-primary/20',
  'In Stock': 'bg-chart-2/10 text-chart-2 dark:bg-chart-2/20',
  'Out of Stock': 'bg-destructive/10 text-destructive dark:bg-destructive/20',
  'Order Revenue': 'bg-chart-1/10 text-chart-1 dark:bg-chart-1/20',
  'Total Orders': 'bg-chart-1/10 text-chart-1 dark:bg-chart-1/20',
  'Employees': 'bg-accent/10 text-accent-foreground dark:bg-accent/20',
  'Staff': 'bg-chart-2/10 text-chart-2 dark:bg-chart-2/20',
  'Drivers': 'bg-chart-1/10 text-chart-1 dark:bg-chart-1/20',
};

const HomeCards = ({ cards }: HomeCardsProps) => {
  return (
    <div className='grid grid-cols-1 sm:grid-cols-2 gap-6 w-full'>
      {cards.map((card, index: number) => {
        const Icon = cardIcons[card.title as keyof typeof cardIcons] || Package;
        const iconColorClass = cardIconColors[card.title as keyof typeof cardIconColors] || cardIconColors['Total Products'];
        const isRevenue = card.title === 'Order Revenue';
        
        return (
          <Card key={index} className='@container/card flex flex-col transition-all hover:shadow-lg'>
            <CardHeader className='pb-3'>
              <div className='flex items-start justify-between gap-4'>
                <div className='flex flex-col gap-1 flex-1'>
                  <CardDescription className='text-sm font-medium'>
                    {card.title}
                  </CardDescription>
                  <CardTitle className='text-3xl font-bold tabular-nums'>
                    {isRevenue ? formatPrice(card.value) : formatCompact(card.value)}
                  </CardTitle>
                </div>
                <div className={`p-3 rounded-full ${iconColorClass}`}>
                  <Icon className='size-5' />
                </div>
              </div>
              <Badge 
                variant='outline' 
                className={`gap-1 w-fit mt-2 ${
                  card.badge.trendingUp === true 
                    ? 'border-green-500/50 text-green-700 dark:text-green-400' 
                    : card.badge.trendingUp === false 
                    ? 'border-red-500/50 text-red-700 dark:text-red-400' 
                    : 'border-blue-500/50 text-blue-700 dark:text-blue-400'
                }`}
              >
                {card.badge.text}
                {card.badge.trendingUp === true && <TrendingUpIcon className='size-3' />}
                {card.badge.trendingUp === false && <TrendingDownIcon className='size-3' />}
              </Badge>
            </CardHeader>
            <CardFooter className='pt-0 pb-4'>
              <div className='flex items-center gap-1.5 text-xs text-muted-foreground'>
                {card.badge.trendingUp === true && (
                  <TrendingUpIcon className='size-3.5 text-green-600 dark:text-green-400' />
                )}
                {card.badge.trendingUp === false && (
                  <TrendingDownIcon className='size-3.5 text-red-600 dark:text-red-400' />
                )}
                <span className='line-clamp-1'>{card.footer}</span>
              </div>
            </CardFooter>
          </Card>
        );
      })}
    </div>
  );
};

export default HomeCards;

import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

import type { AppDispatch, RootState } from '@/store/store';
import { fetchOrders } from '@/store/slices/ordersSlice';
import type { OrderStatus } from '@/interfaces/Order.interface';

import OrdersFilters from '@/components/Orders/OrdersFilters';
import OrdersTable from '@/components/Orders/OrdersTable';
import EmptyState from '@/components/Orders/EmptyState';

import { Card, CardContent } from '@/components/ui/card';
import { ShoppingCart } from 'lucide-react';

const Orders = () => {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const dispatch = useDispatch<AppDispatch>();

  const { orders, loading, error, total } = useSelector(
    (state: RootState) => state.orders,
  );

  const limit = parseInt(searchParams.get('limit') || '10', 10);
  const page = parseInt(searchParams.get('page') || '1', 10);
  const statusParam = searchParams.get('status');
  const status = (statusParam === 'All' ? 'All' : statusParam) as
    | OrderStatus
    | 'All'
    | null;

  useEffect(() => {
    dispatch(fetchOrders({ limit, page, status: status || 'All' }));
  }, [dispatch, limit, page, status]);

  const hasNoOrders = !loading && orders.length === 0 && !error;

  return (
    <section className='w-full h-full flex flex-col gap-6 overflow-hidden'>
      {/* Filters Section */}
      <OrdersFilters />

      {/* Stats Cards - Only show if no filter is active or just show total found */}
      {!loading && orders.length > 0 && (
        <div className='grid grid-cols-1 sm:grid-cols-4 gap-4'>
          <Card>
            <CardContent className='p-4'>
              <div className='flex items-center justify-between'>
                <div className='space-y-1'>
                  <p className='text-xs font-medium text-muted-foreground uppercase tracking-wider'>
                    {status === 'All' || !status
                      ? t('orders.totalOrdersStats')
                      : t('orders.ordersStats', { status })}
                  </p>
                  <p className='text-2xl font-bold tracking-tight'>{total}</p>
                </div>
                <div className='p-2.5 rounded-xl bg-muted'>
                  <ShoppingCart className={`h-5 w-5 text-blue-600`} />
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Orders View OR Empty State */}
      <div className='flex-1 min-h-0'>
        {hasNoOrders ? <EmptyState /> : <OrdersTable />}
      </div>
    </section>
  );
};

export default Orders;

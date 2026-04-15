import { useState, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { useTranslation } from 'react-i18next';
import {
  Clock,
  MapPin,
  Package,
  Loader2,
  Phone,
  Mail,
  CheckCircle,
  Building2,
  Layers,
  Home,
  User,
} from 'lucide-react';

import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';

import { fetchOrderById } from '@/data/Orders';
import { changeOrderStatus } from '@/store/slices/ordersSlice';
import type { AppDispatch } from '@/store/store';
import type { Order, OrderStatus } from '@/interfaces/Order.interface';
import { getStatusText } from '@/utils/orderUtils';
import { toast } from 'sonner';
import { formatDate } from '@/utils/dateUtils';
import { cn } from '@/lib/utils';
import { OrderDisplaySkeleton } from './OrderDisplaySkeleton';

interface OrderDisplayProps {
  orderId: number;
}

const getStatusColor = (status: OrderStatus | number) => {
  const statusText = getStatusText(status);
  switch (statusText) {
    case 'Pending':
      return 'bg-yellow-500/15 text-yellow-700 dark:text-yellow-400 border-yellow-500/30';
    case 'Confirmed':
      return 'bg-blue-500/15 text-blue-700 dark:text-blue-400 border-blue-500/30';
    case 'Preparing':
      return 'bg-indigo-500/15 text-indigo-700 dark:text-indigo-400 border-indigo-500/30';
    case 'OutForDelivery':
      return 'bg-purple-500/15 text-purple-700 dark:text-purple-400 border-purple-500/30';
    case 'Delivered':
      return 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-400 border-emerald-500/30';
    case 'Cancelled':
      return 'bg-red-500/15 text-red-700 dark:text-red-400 border-red-500/30';
    default:
      return 'bg-gray-500/15 text-gray-700 dark:text-gray-400 border-gray-500/30';
  }
};

const statusTransitions: Record<string, number[]> = {
  Pending: [1, 5], // Can go to Confirmed or Cancelled
  Confirmed: [2, 5], // Can go to Preparing or Cancelled
  Preparing: [3, 5], // Can go to OutForDelivery or Cancelled
  OutForDelivery: [4, 5], // Can go to Delivered or Cancelled
  Delivered: [], // Final state
  Cancelled: [], // Final state
};

const numberToStatus: Record<number, string> = {
  0: 'Pending',
  1: 'Confirmed',
  2: 'Preparing',
  3: 'OutForDelivery',
  4: 'Delivered',
  5: 'Cancelled',
};

const OrderDisplay = ({ orderId }: OrderDisplayProps) => {
  const dispatch = useDispatch<AppDispatch>();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [updating, setUpdating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { t } = useTranslation();

  useEffect(() => {
    const loadOrder = async () => {
      setLoading(true);
      setError(null);

      try {
        const result = await fetchOrderById(orderId);
        if ('error' in result) {
          setError(result.error);
        } else {
          setOrder(result);
        }
      } catch (err) {
        setError(t('orders.detailErrorLoading'));
      } finally {
        setLoading(false);
      }
    };

    loadOrder();
  }, [orderId, t]);

  const handleStatusChange = async (newStatus: number) => {
    if (!order) return;
    setUpdating(true);
    try {
      await dispatch(
        changeOrderStatus({ id: order.id, status: newStatus })
      ).unwrap();

      setOrder((prev) => (prev ? { ...prev, status: newStatus } : null));
      toast.success(t('orders.detailStatusUpdated'));
    } catch (err) {
      toast.error(t('orders.detailStatusUpdateFailed'));
    } finally {
      setUpdating(false);
    }
  };

  if (loading) {
    return <OrderDisplaySkeleton />;
  }

  if (error || !order) {
    return (
      <div className="flex h-full items-center justify-center">
        <div className="text-center">
          <p className="text-sm text-muted-foreground">
            {error || t('orders.detailNotFound')}
          </p>
        </div>
      </div>
    );
  }

  const currentStatusText = getStatusText(order.status);
  const allowedTransitions = statusTransitions[currentStatusText] || [];

  return (
    <div className="h-full overflow-y-auto">
      <div className="flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-5 border-b bg-muted/30">
          <div>
            <h2 className="text-xl font-semibold tracking-tight">
              {t('orders.detailOrderLabel')} #{order.orderNumber}
            </h2>
            <p className="text-xs text-muted-foreground mt-1.5">
              {formatDate(order.createdAt, 'MMMM dd, yyyy · hh:mm a')}
            </p>
          </div>
          <Badge
            variant="secondary"
            className={cn(
              'border px-2.5 py-1 text-xs font-medium',
              getStatusColor(order.status)
            )}
          >
            {currentStatusText}
          </Badge>
        </div>

        {/* Status Actions */}
        {allowedTransitions.length > 0 && (
          <div className="px-6 py-5 border-b bg-muted/10">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-3">
              {t('orders.detailQuickActions')}
            </p>
            <div className="flex flex-wrap gap-2">
              {allowedTransitions.map((statusNum) => (
                <Button
                  key={statusNum}
                  onClick={() => handleStatusChange(statusNum)}
                  disabled={updating}
                  variant="outline"
                  size="sm"
                  className="gap-2 h-8"
                >
                  {updating && <Loader2 className="h-3 w-3 animate-spin" />}
                  {!updating && <CheckCircle className="h-3 w-3" />}
                  {t('orders.detailMarkAs', {
                    status: numberToStatus[statusNum],
                  })}
                </Button>
              ))}
            </div>
          </div>
        )}

        {/* Customer & Delivery Information */}
        <div className="px-6 py-5 border-b">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 divide-y md:divide-y-0 md:divide-x">
            {/* Customer Information */}
            <div className="pb-6 md:pb-0 md:pr-6">
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-3">
                {t('orders.detailCustomerTitle')}
              </p>
              <div className="flex gap-4">
                <Avatar className="h-16 w-16 flex-shrink-0">
                  <AvatarFallback className="bg-primary/10 text-primary font-semibold text-xl">
                    {(order.userName || order.customerName || 'G')[0].toUpperCase()}
                  </AvatarFallback>
                </Avatar>
                <div className="space-y-2.5 flex-1 min-w-0">
                  <div className="flex items-start gap-3">
                    <User className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                    <div className="flex-1 min-w-0">
                      <p className="text-xs text-muted-foreground">
                        {t('orders.detailNameLabel')}
                      </p>
                      <p className="font-medium text-base">
                        {order.userName ||
                          order.customerName ||
                          t('orders.guestCustomer')}
                      </p>
                    </div>
                  </div>
                  {order.userEmail && (
                    <div className="flex items-start gap-3">
                      <Mail className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-muted-foreground">
                          {t('orders.detailEmailLabel')}
                        </p>
                        <p className="font-medium text-base truncate">{order.userEmail}</p>
                      </div>
                    </div>
                  )}
                  {order.userPhone && (
                    <div className="flex items-start gap-3">
                      <Phone className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-muted-foreground">
                          {t('orders.detailPhoneLabel')}
                        </p>
                        <p className="font-medium text-base">{order.userPhone}</p>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Delivery Address */}
            {order.deliveryAddress && (
              <div className="pt-6 md:pt-0 md:pl-6">
                <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-3 flex items-center gap-2">
                  <MapPin className="h-3.5 w-3.5" />
                  {t('orders.detailDeliveryAddressTitle')}
                </p>
                <div className="space-y-2.5">
                  <div className="flex items-start gap-3">
                    <Building2 className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-xs text-muted-foreground">
                        {t('users.buildingLabel')}
                      </p>
                      <p className="font-medium text-base">
                        {order.deliveryAddress.buildingName}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <Layers className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-xs text-muted-foreground">
                        {t('orders.detailFloorLabel')}
                      </p>
                      <p className="font-medium text-base">
                        {t('orders.detailFloorLabel')}{' '}
                        {order.deliveryAddress.floorNumber}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <Home className="h-4 w-4 text-muted-foreground mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="text-xs text-muted-foreground">
                        {t('users.apartmentLabel')}
                      </p>
                      <p className="font-medium text-base">
                        {order.deliveryAddress.apartmentName}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Order Items */}
        {order.items && order.items.length > 0 && (
          <div className="px-6 py-5 border-b">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-4">
              {t('orders.detailItemsCount', { count: order.items.length })}
            </p>
            <div className="space-y-4">
              {order.items.map((item) => (
                <div
                  key={item.id}
                  className="flex items-center gap-4"
                >
                  {item.productImageUrl ? (
                    <img
                      src={item.productImageUrl}
                      alt={item.productName}
                      className="h-16 w-16 rounded-lg object-cover flex-shrink-0 border"
                    />
                  ) : (
                    <div className="flex h-16 w-16 flex-shrink-0 items-center justify-center rounded-lg bg-muted border">
                      <Package className="h-6 w-6 text-muted-foreground" />
                    </div>
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="font-medium text-sm mb-1 truncate">{item.productName}</p>
                    <p className="text-xs text-muted-foreground">
                      {t('orders.detailQuantityLabel')}{' '}
                      {item.quantity} × ${item.unitPrice.toFixed(2)}
                    </p>
                  </div>
                  <div className="text-right flex-shrink-0">
                    <p className="font-semibold text-base">${item.totalPrice.toFixed(2)}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Order Summary */}
        <div className="px-6 py-5 border-b bg-muted/10">
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-4">
            {t('orders.detailSummaryTitle')}
          </p>
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <span className="text-sm text-muted-foreground">
                {t('orders.detailSubtotal')}
              </span>
              <span className="font-medium text-sm">${order.subtotal.toFixed(2)}</span>
            </div>
            <div className="flex justify-between items-center">
              <span className="text-sm text-muted-foreground">
                {t('orders.detailDeliveryFee')}
              </span>
              <span className="font-medium text-sm">${order.deliveryFee.toFixed(2)}</span>
            </div>
            <Separator className="my-3" />
            <div className="flex justify-between items-center pt-1">
              <span className="font-semibold text-base">
                {t('orders.detailTotal')}
              </span>
              <span className="text-xl font-bold">${order.totalAmount.toFixed(2)}</span>
            </div>
          </div>
        </div>

        {/* Notes */}
        {order.notes && (
          <div className="px-6 py-5 border-b">
            <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-3">
              {t('orders.detailNotesTitle')}
            </p>
            <p className="text-sm leading-relaxed">{order.notes}</p>
          </div>
        )}

        {/* Timeline */}
        <div className="px-6 py-5">
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider mb-4">
            {t('orders.detailActivityTitle')}
          </p>
          <div className="space-y-4">
            <div className="flex items-start gap-3">
              <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-primary/10">
                <Clock className="h-4 w-4 text-primary" />
              </div>
              <div className="pt-1.5">
                <p className="font-medium text-sm">
                  {t('orders.detailOrderCreated')}
                </p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {formatDate(order.createdAt, 'MMMM dd, yyyy · hh:mm a')}
                </p>
              </div>
            </div>
            {order.completedAt && (
              <div className="flex items-start gap-3">
                <div className="flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-full bg-green-500/10">
                  <CheckCircle className="h-4 w-4 text-green-600" />
                </div>
                <div className="pt-1.5">
                  <p className="font-medium text-sm">
                    {t('orders.detailOrderCompleted')}
                  </p>
                  <p className="text-xs text-muted-foreground mt-0.5">
                    {formatDate(order.completedAt, 'MMMM dd, yyyy · hh:mm a')}
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrderDisplay;

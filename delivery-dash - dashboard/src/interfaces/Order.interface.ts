export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Preparing'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled';

export interface OrderItem {
  id: number;
  orderId?: number; // Optional in some contexts
  productId: number;
  productName: string;
  productImageUrl?: string; // Added field
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface DeliveryAddress {
  id: number;
  buildingName: string;
  floorNumber: number;
  apartmentName: string;
  latitude?: number;
  longitude?: number;
  street?: string;
  phoneNumber?: string;
  label?: string;
  additionalDirections?: string;
}

export interface Order {
  id: number;
  orderNumber: string;
  userId: string;
  userEmail?: string; // Added field
  userPhone?: string; // Added field
  vendorId: number;
  vendorName?: string; // Added field
  deliveryAddress?: DeliveryAddress | null; // Updated field
  subtotal: number;
  deliveryFee: number;
  totalAmount: number;
  status: OrderStatus | number;
  notes?: string;
  createdAt: string;
  confirmedAt?: string | null;
  preparingAt?: string | null;
  outForDeliveryAt?: string | null;
  deliveredAt?: string | null;
  cancelledAt?: string | null;
  completedAt?: string | null;
  customerName?: string;
  userName?: string;
  items?: OrderItem[];
  itemCount?: number;
}

export interface OrdersState {
  orders: Order[];
  loading: boolean;
  error: string | null;
  limit: number;
  page: number;
  total: number;
  statusFilter: OrderStatus | 'All' | null;
}

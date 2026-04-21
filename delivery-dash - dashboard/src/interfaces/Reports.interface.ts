export type Granularity = 'day' | 'week' | 'month';

export interface TimePoint {
  bucket: string;
  value: number;
}

export interface StatusCount {
  status: string;
  count: number;
}

export interface NamedAmount {
  name: string;
  amount: number;
}

export interface VendorRanking {
  vendorId: number;
  name: string;
  categoryName: string;
  orderCount: number;
  revenue: number;
}

export interface DriverRanking {
  driverId: string;
  name: string;
  deliveries: number;
  avgDeliveryMinutes: number;
}

export interface CustomerRanking {
  customerId: string;
  name: string;
  orderCount: number;
  totalSpent: number;
}

export interface FinancialSection {
  totalRevenue: number;
  gmv: number;
  avgOrderValue: number;
  revenueSeries: TimePoint[];
  revenueByCategory: NamedAmount[];
}

export interface OrdersSection {
  total: number;
  cancellationRate: number;
  ordersSeries: TimePoint[];
  statusBreakdown: StatusCount[];
}

export interface VendorsSection {
  newSignups: number;
  activeCount: number;
  inactiveCount: number;
  topByRevenue: VendorRanking[];
  topByOrders: VendorRanking[];
}

export interface DriversSection {
  activeCount: number;
  avgDeliveryMinutes: number;
  topByDeliveries: DriverRanking[];
}

export interface CustomersSection {
  newSignups: number;
  returning: number;
  oneTime: number;
  signupSeries: TimePoint[];
  topSpenders: CustomerRanking[];
}

export interface SupportSection {
  opened: number;
  resolved: number;
  openBacklog: number;
  avgResolutionHours: number;
}

export interface AdminAnalyticsResponse {
  fromUtc: string;
  toUtc: string;
  granularity: Granularity;
  financial: FinancialSection;
  orders: OrdersSection;
  vendors: VendorsSection;
  drivers: DriversSection;
  customers: CustomersSection;
  support: SupportSection;
}

export interface ReportsDateRange {
  from: Date;
  to: Date;
  granularity: Granularity;
}

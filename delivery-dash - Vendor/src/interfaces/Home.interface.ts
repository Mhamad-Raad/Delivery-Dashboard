export interface HomeCardsProps {
  cards: Array<{
    title: string;
    value: number;
    badge: {
      text: string;
      trendingUp?: boolean | null;
    };
    footer: string;
  }>;
}

export interface RecentOrder {
  id: string;
  src: string;
  name: string;
  fallback: string;
  vendor: string;
  location: string;
  status: string;
}

export interface RecentOrdersHomeProps {
  items: RecentOrder[];
}

export interface TopSellingItem {
  id: string;
  vendor: string;
  type: string;
  sold: number;
}

export interface TopSellingItemsHomeProps {
  items: TopSellingItem[];
}

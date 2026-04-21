export const formatNumber = (n: number) => n.toLocaleString();

export const formatCurrency = (n: number) =>
  new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 2,
  }).format(n ?? 0);

export const formatPercent = (ratio: number) =>
  `${(ratio * 100).toFixed(1)}%`;

export const formatBucket = (iso: string, granularity: 'day' | 'week' | 'month') => {
  const d = new Date(iso);
  if (granularity === 'month') {
    return d.toLocaleDateString(undefined, { month: 'short', year: 'numeric' });
  }
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
};

export const formatMinutes = (m: number) => `${m.toFixed(1)}m`;

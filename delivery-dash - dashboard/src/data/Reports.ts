import { axiosInstance } from '@/data/axiosInstance';
import type { AdminAnalyticsResponse, Granularity } from '@/interfaces/Reports.interface';

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export interface FetchAnalyticsParams {
  from: Date;
  to: Date;
  granularity: Granularity;
  signal?: AbortSignal;
}

const toIso = (d: Date) => d.toISOString();

export const fetchAdminAnalytics = async ({
  from,
  to,
  granularity,
  signal,
}: FetchAnalyticsParams): Promise<AdminAnalyticsResponse | { error: string }> => {
  try {
    const response = await axiosInstance.get<AdminAnalyticsResponse>(
      '/Dashboard/admin/analytics',
      {
        params: { from: toIso(from), to: toIso(to), granularity },
        headers: { key: API_KEY, value: API_VALUE },
        signal,
      }
    );
    return response.data;
  } catch (error: any) {
    if (error.name === 'CanceledError' || error.code === 'ERR_CANCELED') {
      return { error: 'canceled' };
    }
    return {
      error:
        error.response?.data?.message ||
        error.message ||
        'Failed to fetch analytics',
    };
  }
};

export type CsvKind = 'orders' | 'vendors' | 'drivers' | 'customers' | 'tickets';

export interface CsvExportParams {
  from: Date;
  to: Date;
  status?: string | null;
  vendorId?: number | null;
}

export const downloadCsv = async (
  kind: CsvKind,
  { from, to, status, vendorId }: CsvExportParams
): Promise<void | { error: string }> => {
  try {
    const params: Record<string, string | number> = {
      from: toIso(from),
      to: toIso(to),
    };
    if (kind === 'orders') {
      if (status && status !== 'All') params.status = status;
      if (vendorId) params.vendorId = vendorId;
    }

    const response = await axiosInstance.get(
      `/Dashboard/admin/export/${kind}.csv`,
      {
        params,
        headers: { key: API_KEY, value: API_VALUE },
        responseType: 'blob',
      }
    );

    const blob = new Blob([response.data], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    const fileName = extractFilename(response.headers['content-disposition']) ??
      `${kind}_${yyyymmdd(from)}_${yyyymmdd(to)}.csv`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  } catch (error: any) {
    return {
      error:
        error.response?.data?.message ||
        error.message ||
        `Failed to download ${kind} CSV`,
    };
  }
};

const yyyymmdd = (d: Date) =>
  `${d.getUTCFullYear()}${String(d.getUTCMonth() + 1).padStart(2, '0')}${String(
    d.getUTCDate()
  ).padStart(2, '0')}`;

const extractFilename = (contentDisposition?: string): string | null => {
  if (!contentDisposition) return null;
  const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(contentDisposition);
  return match ? decodeURIComponent(match[1]) : null;
};


import { axiosInstance } from '@/data/axiosInstance';
import type { Notification, NotificationType } from '@/interfaces/Notification.interface';

type AxiosLikeError = {
  response?: { data?: { error?: string; message?: string; errors?: unknown[] } };
  message?: string;
};

const extractError = (error: unknown): { error: string; errors: unknown[] } => {
  const e = (error ?? {}) as AxiosLikeError;
  return {
    error:
      e.response?.data?.error ||
      e.response?.data?.message ||
      e.message ||
      'Unknown error',
    errors: e.response?.data?.errors ?? [],
  };
};

const API_KEY = import.meta.env.VITE_API_KEY;
const API_VALUE = import.meta.env.VITE_API_VALUE;

export type BroadcastAudience = 'AllCustomers' | 'SpecificUsers';

export interface BroadcastRequest {
  title: string;
  body: string;
  audience: BroadcastAudience;
  customerIds?: string[];
  image?: File | null;
}

export interface BroadcastResponse {
  targeted: number;
  imageUrl: string | null;
}

export const broadcastNotification = async (
  req: BroadcastRequest
): Promise<BroadcastResponse | { error: string; errors?: unknown[] }> => {
  try {
    const form = new FormData();
    form.append('title', req.title);
    form.append('body', req.body);
    form.append('audience', req.audience);

    if (req.audience === 'SpecificUsers') {
      (req.customerIds ?? []).forEach((id) => form.append('customerIds', id));
    }
    if (req.image) {
      form.append('image', req.image);
    }

    const response = await axiosInstance.post('/Notification/broadcast', form, {
      headers: {
        key: API_KEY,
        value: API_VALUE,
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  } catch (error) {
    return extractError(error);
  }
};

interface NotificationResponse {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  actionUrl?: string;
  metadata?: Record<string, unknown>;
}

// Transform backend response to frontend Notification type
const transformNotification = (notification: NotificationResponse): Notification => ({
  id: notification.id,
  title: notification.title,
  message: notification.message,
  type: (notification.type?.toLowerCase() || 'info') as NotificationType,
  isRead: notification.isRead,
  createdAt: notification.createdAt,
  actionUrl: notification.actionUrl,
  metadata: notification.metadata,
});

// GET: Fetch user notifications
export const fetchNotifications = async (skip = 0, take = 20): Promise<Notification[]> => {
  try {
    const response = await axiosInstance.get<NotificationResponse[]>('/Notification', {
      params: { skip, take },
    });
    return response.data.map(transformNotification);
  } catch (error) {
    console.error('Error fetching notifications:', error);
    return [];
  }
};

// GET: Fetch unread count
export const fetchUnreadCount = async (): Promise<number> => {
  try {
    const response = await axiosInstance.get<{ count: number }>('/Notification/unread-count');
    return response.data.count;
  } catch (error) {
    console.error('Error fetching unread count:', error);
    return 0;
  }
};

// PUT: Mark notification as read
export const markNotificationAsRead = async (id: number): Promise<void> => {
  try {
    await axiosInstance.put(`/Notification/${id}/read`);
  } catch (error) {
    console.error('Error marking notification as read:', error);
  }
};

// PUT: Mark all notifications as read
export const markAllNotificationsAsRead = async (): Promise<void> => {
  try {
    await axiosInstance.put('/Notification/mark-all-read');
  } catch (error) {
    console.error('Error marking all notifications as read:', error);
  }
};

// DELETE: Delete a notification
export const deleteNotificationApi = async (id: number): Promise<void> => {
  try {
    await axiosInstance.delete(`/Notification/${id}`);
  } catch (error) {
    console.error('Error deleting notification:', error);
  }
};

// ----- Admin broadcast history -----

export interface BroadcastSummary {
  key: number;
  title: string;
  message: string;
  imageUrl: string | null;
  createdAt: string;
  recipients: number;
}

export const fetchBroadcasts = async (
  skip = 0,
  take = 20
): Promise<BroadcastSummary[]> => {
  try {
    const response = await axiosInstance.get<BroadcastSummary[]>(
      '/Notification/admin/broadcasts',
      { params: { skip, take } }
    );
    return response.data;
  } catch (error) {
    console.error('Error fetching broadcasts:', error);
    return [];
  }
};

export const fetchBroadcast = async (
  key: number
): Promise<BroadcastSummary | null> => {
  try {
    const response = await axiosInstance.get<BroadcastSummary>(
      `/Notification/admin/broadcasts/${key}`
    );
    return response.data;
  } catch (error) {
    console.error('Error fetching broadcast:', error);
    return null;
  }
};

export const deleteBroadcast = async (
  key: number
): Promise<{ removed: number } | { error: string }> => {
  try {
    const response = await axiosInstance.delete<{ removed: number }>(
      `/Notification/admin/broadcasts/${key}`
    );
    return response.data;
  } catch (error) {
    const { error: msg } = extractError(error);
    return { error: msg };
  }
};

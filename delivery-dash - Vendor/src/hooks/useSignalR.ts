import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';

import {
  addNotification,
  setNotifications,
  setConnectionStatus,
} from '@/store/slices/notificationsSlice';
import { addOrder, updateOrderStatus } from '@/store/slices/ordersSlice';
import { fetchNotifications } from '@/data/Notifications';
import { fetchOrderById } from '@/data/Orders';

import type { Notification } from '@/interfaces/Notification.interface';
import type { OrderStatus } from '@/interfaces/Order.interface';

const API_URL = import.meta.env.VITE_API_URL;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private notificationCallbacks: ((notification: Notification) => void)[] = [];
  private connectionStateCallbacks: ((
    state: signalR.HubConnectionState
  ) => void)[] = [];
  private connectionPromise: Promise<void> | null = null;

  public async startConnection(): Promise<void> {
    // If we have an existing connection, check its state
    if (this.connection) {
      // If already connected, we're good
      if (this.connection.state === signalR.HubConnectionState.Connected) {
        console.log('SignalR already connected');
        return;
      }

      // If in any other state (Connecting, Reconnecting, Disconnected),
      // stop it cleanly before creating a new one to prevent orphans.
      // This handles Strict Mode where cleanup (stop) might race with next start.
      try {
        await this.stopConnection();
      } catch (e) {
        console.error('Error stopping existing connection:', e);
      }
    }

    if (this.connectionPromise) {
      return this.connectionPromise;
    }

    this.connectionPromise = (async () => {
      // The hub URL - adjust this based on your backend configuration
      // Common patterns: /hubs/notifications, /notificationHub, /signalr/notifications
      const baseUrl = API_URL.replace('/MalDashApi', '');
      const hubUrl = `${baseUrl}/hubs/notifications`;
      console.log('SignalR connecting to:', hubUrl);

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          // Use cookies for authentication (HTTP-only cookies sent automatically)
          withCredentials: true,
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up connection state handlers
      this.connection.onreconnecting((error) => {
        console.log('SignalR reconnecting...', error);
        this.notifyConnectionState(signalR.HubConnectionState.Reconnecting);
      });

      this.connection.onreconnected((connectionId) => {
        console.log('SignalR reconnected. Connection ID:', connectionId);
        this.notifyConnectionState(signalR.HubConnectionState.Connected);
      });

      this.connection.onclose((error) => {
        console.log('SignalR connection closed', error);
        this.notifyConnectionState(signalR.HubConnectionState.Disconnected);
      });

      // Listen for notifications from backend
      this.connection.on('ReceiveNotification', (notification: any) => {
        console.log('Received notification:', notification);

        const processedNotification: Notification = {
          id: notification.Id ?? notification.id,
          title: notification.Title ?? notification.title,
          message: notification.Message ?? notification.message,
          type: (
            notification.Type ??
            notification.type ??
            'info'
          ).toLowerCase(),
          isRead: false,
          createdAt: new Date(notification.CreatedAt ?? notification.createdAt),
          actionUrl: notification.ActionUrl ?? notification.actionUrl,
          metadata: notification.Metadata ?? notification.metadata,
        };

        this.notificationCallbacks.forEach((callback) =>
          callback(processedNotification)
        );
      });

      try {
        await this.connection.start();
        console.log('✅ SignalR Connected successfully');
        this.notifyConnectionState(signalR.HubConnectionState.Connected);
      } catch (error: any) {
        console.error('❌ SignalR Connection Error:', error?.message || error);
        this.notifyConnectionState(signalR.HubConnectionState.Disconnected);

        // Don't retry on 404 - the hub URL is wrong
        if (error?.message?.includes('404')) {
          console.error(
            'Hub not found. Please check the SignalR hub URL configuration.'
          );
          return;
        }

        // Retry for other errors
        // Note: AutomaticReconnect handles most retries, but initial connection needs manual retry
        setTimeout(() => {
          this.connectionPromise = null;
          this.startConnection();
        }, 5000);
        throw error; // Re-throw to clear promise
      }
    })();

    try {
      await this.connectionPromise;
    } catch (e) {
      // Error logged inside promise
      this.connectionPromise = null;
    } finally {
      // Don't clear promise on success, to prevent concurrent re-connections if called again
      // Only clear on error (handled in catch/retry)
      // Actually, if we succeed, we want subsequent calls to see Connected state and return early.
      // But if state check fails (e.g. disconnected later), we need a new promise.
      // So we should probably clear promise after completion?
      // If we clear it, a concurrent call MIGHT start a new one if state hasn't updated yet?
      // But we await it.
      this.connectionPromise = null;
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR Disconnected');
    }
  }

  public onNotificationReceived(
    callback: (notification: Notification) => void
  ): () => void {
    this.notificationCallbacks.push(callback);
    return () => {
      this.notificationCallbacks = this.notificationCallbacks.filter(
        (cb) => cb !== callback
      );
    };
  }

  public onConnectionStateChanged(
    callback: (state: signalR.HubConnectionState) => void
  ): () => void {
    this.connectionStateCallbacks.push(callback);
    return () => {
      this.connectionStateCallbacks = this.connectionStateCallbacks.filter(
        (cb) => cb !== callback
      );
    };
  }

  public isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }

  private notifyConnectionState(state: signalR.HubConnectionState): void {
    this.connectionStateCallbacks.forEach((callback) => callback(state));
  }
}

export const signalRService = new SignalRService();

export const useSignalR = () => {
  const dispatch = useDispatch();
  const isConnected = useRef(false);

  useEffect(() => {
    let isMounted = true;

    // Set up listeners immediately
    const unsubscribeConnection = signalRService.onConnectionStateChanged(
      (state) => {
        const connected = state === signalR.HubConnectionState.Connected;
        isConnected.current = connected;
        dispatch(setConnectionStatus(connected));
      }
    );

    const unsubscribeNotification = signalRService.onNotificationReceived(
      async (notification) => {
        dispatch(addNotification(notification));

        // Handle Order Updates via SignalR
        try {
          if (notification.metadata) {
            const metadata =
              typeof notification.metadata === 'string'
                ? JSON.parse(notification.metadata)
                : notification.metadata;

            if (metadata?.orderId) {
              // New Order or Status Update
              if (
                notification.title.includes('New Order') ||
                notification.title.includes('Order Created')
              ) {
                const newOrder = await fetchOrderById(metadata.orderId);
                if (!('error' in newOrder)) {
                  dispatch(addOrder(newOrder));
                }
              } else if (metadata.status) {
                dispatch(
                  updateOrderStatus({
                    id: metadata.orderId,
                    status: metadata.status as OrderStatus,
                  })
                );
              } else if (notification.title.includes('Cancelled')) {
                dispatch(
                  updateOrderStatus({
                    id: metadata.orderId,
                    status: 'Cancelled',
                  })
                );
              }
            }
          }
        } catch (error) {
          console.error('Error processing real-time order update:', error);
        }

        // Show toast for new notification
        toast(notification.title, {
          description: notification.message,
          action: notification.actionUrl
            ? {
                label: 'View',
                onClick: () => (window.location.href = notification.actionUrl!),
              }
            : undefined,
        });
      }
    );

    const initSignalR = async () => {
      // 1. Load initial notifications
      try {
        const notifications = await fetchNotifications();
        if (isMounted) {
          dispatch(setNotifications(notifications));
        }
      } catch (error) {
        console.error('Failed to load notifications:', error);
      }

      // 2. Start SignalR connection
      if (isMounted) {
        await signalRService.startConnection();
      }
    };

    initSignalR();

    return () => {
      isMounted = false;
      unsubscribeNotification();
      unsubscribeConnection();
      signalRService.stopConnection();
    };
  }, [dispatch]);

  return { signalRService };
};


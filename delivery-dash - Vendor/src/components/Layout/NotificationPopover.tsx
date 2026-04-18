import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { Bell, Check, X } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';

import type {
  Notification,
  NotificationType,
} from '@/interfaces/Notification.interface';
import type { RootState } from '@/store/store';
import {
  markAsRead,
  markAllAsRead,
  deleteNotification,
} from '@/store/slices/notificationsSlice';
import {
  markNotificationAsRead,
  markAllNotificationsAsRead,
  deleteNotificationApi,
} from '@/data/Notifications';

const getNotificationColor = (type: NotificationType) => {
  switch (type) {
    case 'success':
      return 'text-green-600 dark:text-green-400';
    case 'error':
      return 'text-red-600 dark:text-red-400';
    case 'warning':
      return 'text-yellow-600 dark:text-yellow-400';
    case 'info':
      return 'text-blue-600 dark:text-blue-400';
    case 'system':
      return 'text-purple-600 dark:text-purple-400';
    default:
      return 'text-foreground';
  }
};

const formatTimestamp = (date: Date | string) => {
  const d = new Date(date);
  const now = new Date();
  const diff = now.getTime() - d.getTime();
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (minutes < 1) return 'Just now';
  if (minutes < 60) return `${minutes}m ago`;
  if (hours < 24) return `${hours}h ago`;
  if (days < 7) return `${days}d ago`;
  return d.toLocaleDateString();
};

type NotificationTab = 'all' | 'unread' | 'mentions' | 'system';

export default function NotificationPopover() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const notifications = useSelector(
    (state: RootState) => state.notifications.notifications
  );
  const [open, setOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<NotificationTab>('all');

  const unreadCount = notifications.filter((n) => !n.isRead).length;
  const allCount = notifications.length;
  const mentionsCount = notifications.filter(
    (n) => n.type === 'warning' || n.type === 'error'
  ).length;
  const systemCount = notifications.filter((n) => n.type === 'system').length;

  const getFilteredNotifications = () => {
    switch (activeTab) {
      case 'unread':
        return notifications.filter((n) => !n.isRead);
      case 'mentions':
        return notifications.filter(
          (n) => n.type === 'warning' || n.type === 'error'
        );
      case 'system':
        return notifications.filter((n) => n.type === 'system');
      default:
        return notifications;
    }
  };

  const filteredNotifications = getFilteredNotifications();

  const handleMarkAllRead = () => {
    dispatch(markAllAsRead());
    markAllNotificationsAsRead();
  };

  const handleMarkAsRead = (id: number) => {
    dispatch(markAsRead(id));
    markNotificationAsRead(id);
  };

  const handleDelete = (id: number, e: React.MouseEvent) => {
    e.stopPropagation();
    dispatch(deleteNotification(id));
    deleteNotificationApi(id);
  };

  const handleNotificationClick = (notification: Notification) => {
    if (!notification.isRead) {
      handleMarkAsRead(notification.id);
    }
    if (notification.actionUrl) {
      navigate(notification.actionUrl);
      setOpen(false);
    }
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant='ghost'
          size='icon'
          className='relative hover:bg-accent transition-colors'
          aria-label='Notifications'
        >
          <Bell className='size-4' />
          {unreadCount > 0 && (
            <span className='absolute top-1 right-1 size-2 bg-blue-500 rounded-full' />
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent
        className='w-[380px] p-0 border shadow-lg'
        align='end'
        sideOffset={8}
      >
        {/* Header */}
        <div className='px-4 pt-3 pb-0 border-b'>
          <div className='flex items-center justify-between mb-3'>
            <h3 className='font-semibold text-sm'>{t('Notifications')}</h3>
            {notifications.length > 0 && unreadCount > 0 && (
              <Button
                variant='ghost'
                size='sm'
                onClick={handleMarkAllRead}
                className='h-7 px-2 text-xs hover:bg-accent'
              >
                <Check className='size-3 mr-1' />
                Mark all as read
              </Button>
            )}
          </div>

          {/* Tabs */}
          <div className='flex gap-1 -mb-px'>
            <button
              onClick={() => setActiveTab('all')}
              className={`px-3 py-2 text-xs font-medium border-b-2 transition-colors ${
                activeTab === 'all'
                  ? 'border-foreground text-foreground'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              All
              {allCount > 0 && (
                <span className='ml-1.5 text-[10px] px-1.5 py-0.5 rounded-full bg-muted'>
                  {allCount}
                </span>
              )}
            </button>
            <button
              onClick={() => setActiveTab('unread')}
              className={`px-3 py-2 text-xs font-medium border-b-2 transition-colors ${
                activeTab === 'unread'
                  ? 'border-foreground text-foreground'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              Unread
              {unreadCount > 0 && (
                <span className='ml-1.5 text-[10px] px-1.5 py-0.5 rounded-full bg-blue-500 text-white'>
                  {unreadCount}
                </span>
              )}
            </button>
            <button
              onClick={() => setActiveTab('mentions')}
              className={`px-3 py-2 text-xs font-medium border-b-2 transition-colors ${
                activeTab === 'mentions'
                  ? 'border-foreground text-foreground'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              Important
              {mentionsCount > 0 && (
                <span className='ml-1.5 text-[10px] px-1.5 py-0.5 rounded-full bg-muted'>
                  {mentionsCount}
                </span>
              )}
            </button>
            <button
              onClick={() => setActiveTab('system')}
              className={`px-3 py-2 text-xs font-medium border-b-2 transition-colors ${
                activeTab === 'system'
                  ? 'border-foreground text-foreground'
                  : 'border-transparent text-muted-foreground hover:text-foreground'
              }`}
            >
              System
              {systemCount > 0 && (
                <span className='ml-1.5 text-[10px] px-1.5 py-0.5 rounded-full bg-muted'>
                  {systemCount}
                </span>
              )}
            </button>
          </div>
        </div>

        {/* Notifications List */}
        {filteredNotifications.length === 0 ? (
          <div className='flex flex-col items-center justify-center py-12 px-4'>
            <Bell className='size-8 text-muted-foreground/40 mb-3' />
            <p className='text-sm text-muted-foreground'>
              {activeTab === 'unread'
                ? 'No unread notifications'
                : activeTab === 'mentions'
                ? 'No important notifications'
                : activeTab === 'system'
                ? 'No system notifications'
                : 'No notifications'}
            </p>
          </div>
        ) : (
          <>
            <ScrollArea className='h-[420px]'>
              {filteredNotifications.map((notification) => (
                <div
                  key={notification.id}
                  className={`group relative px-4 py-3 border-b last:border-b-0 transition-colors cursor-pointer ${
                    !notification.isRead
                      ? 'bg-accent/30 hover:bg-accent/50'
                      : 'hover:bg-accent/30'
                  }`}
                  onClick={() => handleNotificationClick(notification)}
                >
                  <div className='flex gap-3'>
                    {/* Unread Indicator */}
                    <div className='flex-shrink-0 pt-1.5'>
                      {!notification.isRead ? (
                        <div className='size-2 bg-blue-500 rounded-full' />
                      ) : (
                        <div className='size-2' />
                      )}
                    </div>

                    {/* Content */}
                    <div className='flex-1 min-w-0'>
                      <div className='flex items-start justify-between gap-2 mb-1'>
                        <p
                          className={`text-sm leading-tight ${getNotificationColor(
                            notification.type
                          )} font-medium`}
                        >
                          {notification.title}
                        </p>
                        <span className='text-[10px] text-muted-foreground whitespace-nowrap'>
                          {formatTimestamp(notification.createdAt)}
                        </span>
                      </div>
                      <p
                        className={`text-xs text-muted-foreground line-clamp-2 ${
                          !notification.isRead
                            ? 'font-medium text-foreground/80'
                            : ''
                        }`}
                      >
                        {notification.message}
                      </p>
                    </div>
                  </div>

                  {/* Hover Actions */}
                  <div className='absolute right-2 top-2 opacity-0 group-hover:opacity-100 transition-opacity flex gap-1 bg-background/80 backdrop-blur-sm rounded p-0.5'>
                    <Button
                      variant='ghost'
                      size='icon'
                      className='size-6 h-6 w-6'
                      onClick={(e) => handleDelete(notification.id, e)}
                      title='Delete'
                    >
                      <X className='size-3' />
                    </Button>
                  </div>
                </div>
              ))}
            </ScrollArea>

            {/* Footer */}
            <div className='p-2 border-t bg-muted/20'>
              <Button
                variant='ghost'
                className='w-full text-xs h-8'
                onClick={() => {
                  setOpen(false);
                  navigate('/notifications');
                }}
              >
                View all notifications
              </Button>
            </div>
          </>
        )}
      </PopoverContent>
    </Popover>
  );
}


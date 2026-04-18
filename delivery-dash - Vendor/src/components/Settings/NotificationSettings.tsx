import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Separator } from '@/components/ui/separator';

const NotificationSettings = () => {
  const notificationCategories = [
    {
      title: 'Orders',
      description: 'Notifications about order status and updates',
      settings: [
        { id: 'new-orders', label: 'New orders', defaultChecked: true },
        { id: 'order-status', label: 'Order status changes', defaultChecked: true },
        { id: 'order-cancellations', label: 'Order cancellations', defaultChecked: true },
      ],
    },
    {
      title: 'Users',
      description: 'Notifications about user activity and registrations',
      settings: [
        { id: 'new-users', label: 'New user registrations', defaultChecked: true },
        { id: 'user-reports', label: 'User reports and complaints', defaultChecked: true },
        { id: 'user-inactive', label: 'Inactive user alerts', defaultChecked: false },
      ],
    },
    {
      title: 'Vendors',
      description: 'Notifications about vendor activities',
      settings: [
        { id: 'vendor-requests', label: 'New vendor requests', defaultChecked: true },
        { id: 'vendor-updates', label: 'Vendor profile updates', defaultChecked: false },
        { id: 'vendor-performance', label: 'Performance alerts', defaultChecked: true },
      ],
    },
    {
      title: 'Buildings & Apartments',
      description: 'Notifications about property management',
      settings: [
        { id: 'building-updates', label: 'Building updates', defaultChecked: true },
        { id: 'apartment-changes', label: 'Apartment status changes', defaultChecked: true },
        { id: 'maintenance-alerts', label: 'Maintenance alerts', defaultChecked: true },
      ],
    },
    {
      title: 'System',
      description: 'System-wide notifications and updates',
      settings: [
        { id: 'security-alerts', label: 'Security alerts', defaultChecked: true },
        { id: 'system-updates', label: 'System updates', defaultChecked: false },
        { id: 'backup-reports', label: 'Backup reports', defaultChecked: false },
      ],
    },
  ];

  return (
    <div className='space-y-6'>
      {/* Email Notifications */}
      <Card>
        <CardHeader>
          <CardTitle>Email Notifications</CardTitle>
          <CardDescription>
            Manage which notifications you receive via email
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-6'>
          {notificationCategories.map((category, index) => (
            <div key={category.title}>
              <div className='space-y-4'>
                <div>
                  <h3 className='font-medium'>{category.title}</h3>
                  <p className='text-sm text-muted-foreground'>
                    {category.description}
                  </p>
                </div>
                <div className='space-y-3'>
                  {category.settings.map((setting) => (
                    <div
                      key={setting.id}
                      className='flex items-center justify-between'
                    >
                      <Label htmlFor={setting.id} className='cursor-pointer'>
                        {setting.label}
                      </Label>
                      <Switch
                        id={setting.id}
                        defaultChecked={setting.defaultChecked}
                      />
                    </div>
                  ))}
                </div>
              </div>
              {index < notificationCategories.length - 1 && (
                <Separator className='mt-6' />
              )}
            </div>
          ))}
        </CardContent>
      </Card>

      {/* Push Notifications */}
      <Card>
        <CardHeader>
          <CardTitle>Push Notifications</CardTitle>
          <CardDescription>
            Manage browser push notifications
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label htmlFor='push-enabled'>Enable Push Notifications</Label>
              <p className='text-sm text-muted-foreground'>
                Receive real-time notifications in your browser
              </p>
            </div>
            <Switch id='push-enabled' />
          </div>

          <Separator />

          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label htmlFor='sound-enabled'>Notification Sound</Label>
              <p className='text-sm text-muted-foreground'>
                Play sound when receiving notifications
              </p>
            </div>
            <Switch id='sound-enabled' defaultChecked />
          </div>
        </CardContent>
      </Card>

      {/* Digest Settings */}
      <Card>
        <CardHeader>
          <CardTitle>Notification Digest</CardTitle>
          <CardDescription>
            Receive a summary of notifications at specific intervals
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label htmlFor='daily-digest'>Daily Digest</Label>
              <p className='text-sm text-muted-foreground'>
                Receive a daily summary at 8:00 AM
              </p>
            </div>
            <Switch id='daily-digest' defaultChecked />
          </div>

          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label htmlFor='weekly-digest'>Weekly Digest</Label>
              <p className='text-sm text-muted-foreground'>
                Receive a weekly summary every Monday
              </p>
            </div>
            <Switch id='weekly-digest' />
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default NotificationSettings;

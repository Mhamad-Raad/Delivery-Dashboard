import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Shield, Smartphone, Key, Clock } from 'lucide-react';

const SecuritySettings = () => {
  const activeSessions = [
    {
      id: 1,
      device: 'Windows PC',
      location: 'New York, USA',
      lastActive: '2 minutes ago',
      current: true,
    },
    {
      id: 2,
      device: 'iPhone 15 Pro',
      location: 'New York, USA',
      lastActive: '2 hours ago',
      current: false,
    },
    {
      id: 3,
      device: 'Chrome on MacBook',
      location: 'San Francisco, USA',
      lastActive: '1 day ago',
      current: false,
    },
  ];

  return (
    <div className='space-y-6'>
      {/* Change Password */}
      <Card>
        <CardHeader>
          <CardTitle className='flex items-center gap-2'>
            <Key className='h-5 w-5' />
            Change Password
          </CardTitle>
          <CardDescription>
            Update your password to keep your account secure
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='space-y-2'>
            <Label htmlFor='current-password'>Current Password</Label>
            <Input id='current-password' type='password' />
          </div>
          <div className='space-y-2'>
            <Label htmlFor='new-password'>New Password</Label>
            <Input id='new-password' type='password' />
          </div>
          <div className='space-y-2'>
            <Label htmlFor='confirm-password'>Confirm New Password</Label>
            <Input id='confirm-password' type='password' />
          </div>

          <div className='bg-muted p-3 rounded-md'>
            <p className='text-sm text-muted-foreground'>
              Password must be at least 8 characters long and contain:
            </p>
            <ul className='text-sm text-muted-foreground mt-2 space-y-1 ml-4 list-disc'>
              <li>At least one uppercase letter</li>
              <li>At least one lowercase letter</li>
              <li>At least one number</li>
              <li>At least one special character</li>
            </ul>
          </div>

          <div className='flex justify-end gap-3'>
            <Button variant='outline'>Cancel</Button>
            <Button>Update Password</Button>
          </div>
        </CardContent>
      </Card>

      {/* Two-Factor Authentication */}
      <Card>
        <CardHeader>
          <CardTitle className='flex items-center gap-2'>
            <Shield className='h-5 w-5' />
            Two-Factor Authentication
          </CardTitle>
          <CardDescription>
            Add an extra layer of security to your account
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          <div className='flex items-center justify-between'>
            <div className='space-y-1'>
              <Label>Enable 2FA</Label>
              <p className='text-sm text-muted-foreground'>
                Require a verification code in addition to your password
              </p>
            </div>
            <Switch id='2fa-enabled' />
          </div>

          <Separator />

          <div className='space-y-3'>
            <h4 className='font-medium text-sm'>Authentication Methods</h4>
            <div className='space-y-2'>
              <div className='flex items-center justify-between p-3 border rounded-lg'>
                <div className='flex items-center gap-3'>
                  <Smartphone className='h-5 w-5 text-muted-foreground' />
                  <div>
                    <p className='font-medium text-sm'>Authenticator App</p>
                    <p className='text-xs text-muted-foreground'>
                      Use an app like Google Authenticator
                    </p>
                  </div>
                </div>
                <Button variant='outline' size='sm'>
                  Setup
                </Button>
              </div>

              <div className='flex items-center justify-between p-3 border rounded-lg'>
                <div className='flex items-center gap-3'>
                  <Smartphone className='h-5 w-5 text-muted-foreground' />
                  <div>
                    <p className='font-medium text-sm'>SMS Verification</p>
                    <p className='text-xs text-muted-foreground'>
                      Receive codes via text message
                    </p>
                  </div>
                </div>
                <Button variant='outline' size='sm'>
                  Setup
                </Button>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Active Sessions */}
      <Card>
        <CardHeader>
          <CardTitle className='flex items-center gap-2'>
            <Clock className='h-5 w-5' />
            Active Sessions
          </CardTitle>
          <CardDescription>
            Manage your active sessions across different devices
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-4'>
          {activeSessions.map((session, index) => (
            <div key={session.id}>
              <div className='flex items-center justify-between'>
                <div className='space-y-1'>
                  <div className='flex items-center gap-2'>
                    <p className='font-medium text-sm'>{session.device}</p>
                    {session.current && (
                      <Badge variant='secondary' className='text-xs'>
                        Current
                      </Badge>
                    )}
                  </div>
                  <p className='text-sm text-muted-foreground'>
                    {session.location} • {session.lastActive}
                  </p>
                </div>
                {!session.current && (
                  <Button variant='outline' size='sm'>
                    Revoke
                  </Button>
                )}
              </div>
              {index < activeSessions.length - 1 && <Separator className='mt-4' />}
            </div>
          ))}

          <Separator />

          <Button variant='destructive' size='sm' className='w-full'>
            Sign Out All Other Sessions
          </Button>
        </CardContent>
      </Card>

      {/* Login History */}
      <Card>
        <CardHeader>
          <CardTitle>Login Activity</CardTitle>
          <CardDescription>
            Recent login attempts and activity on your account
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className='space-y-3'>
            {[
              { date: 'Today at 2:45 PM', location: 'New York, USA', status: 'success' },
              { date: 'Yesterday at 9:30 AM', location: 'New York, USA', status: 'success' },
              { date: '2 days ago at 3:15 PM', location: 'San Francisco, USA', status: 'success' },
              { date: '3 days ago at 11:20 AM', location: 'Unknown Location', status: 'failed' },
            ].map((activity, index) => (
              <div
                key={index}
                className='flex items-center justify-between p-3 border rounded-lg'
              >
                <div>
                  <p className='font-medium text-sm'>{activity.date}</p>
                  <p className='text-sm text-muted-foreground'>{activity.location}</p>
                </div>
                <Badge variant={activity.status === 'success' ? 'default' : 'destructive'}>
                  {activity.status === 'success' ? 'Success' : 'Failed'}
                </Badge>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default SecuritySettings;

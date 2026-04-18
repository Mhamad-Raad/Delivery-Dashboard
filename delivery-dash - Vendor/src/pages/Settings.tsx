import { useState } from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { User, Bell, Shield, Palette } from 'lucide-react';
import AccountSettings from '@/components/Settings/AccountSettings';
import AppearanceSettings from '@/components/Settings/AppearanceSettings';
import NotificationSettings from '@/components/Settings/NotificationSettings';
import SecuritySettings from '@/components/Settings/SecuritySettings';

const Settings = () => {
  const [activeTab, setActiveTab] = useState('account');

  const tabs = [
    {
      id: 'account',
      label: 'Account',
      icon: User,
      component: AccountSettings,
    },
    {
      id: 'appearance',
      label: 'Appearance',
      icon: Palette,
      component: AppearanceSettings,
    },
    {
      id: 'notifications',
      label: 'Notifications',
      icon: Bell,
      component: NotificationSettings,
    },
    {
      id: 'security',
      label: 'Security',
      icon: Shield,
      component: SecuritySettings,
    },
  ];

  const ActiveComponent = tabs.find((tab) => tab.id === activeTab)?.component;

  return (
    <div className='w-full h-full flex flex-col gap-6 p-6 overflow-y-auto'>

      <div className='grid grid-cols-1 lg:grid-cols-12 gap-6'>
        {/* Sidebar Navigation */}
        <Card className='lg:col-span-3'>
          <CardContent className='p-4'>
            <nav className='flex flex-col space-y-1'>
              {tabs.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium transition-colors ${
                      activeTab === tab.id
                        ? 'bg-primary text-primary-foreground'
                        : 'hover:bg-muted text-muted-foreground hover:text-foreground'
                    }`}
                  >
                    <Icon className='h-5 w-5' />
                    {tab.label}
                  </button>
                );
              })}
            </nav>
          </CardContent>
        </Card>

        {/* Main Content Area */}
        <div className='lg:col-span-9'>
          {ActiveComponent && <ActiveComponent />}
        </div>
      </div>
    </div>
  );
};

export default Settings;

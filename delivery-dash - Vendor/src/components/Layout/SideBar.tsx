import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import {
  Home,
  Users,
  Package,
  Settings,
  ShoppingCart,
  ChevronRight,
  Palette,
} from 'lucide-react';

import { useSelector } from 'react-redux';
import type { RootState } from '@/store/store';

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubItem,
  SidebarMenuSubButton,
  SidebarRail,
  SidebarHeader,
  SidebarFooter,
  SidebarSeparator,
} from '@/components/ui/sidebar';

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';

import { NavUser } from '@/components/ui/nav-user';

import { logoutUser } from '@/data/Authorization';

import Logo from '@/assets/Logo.jpg';

// Main navigation items
const mainNavItems = [
  {
    titleKey: 'dashboard',
    url: '/',
    icon: Home,
  },
  {
    titleKey: 'users',
    url: '/users',
    icon: Users,
  },
  {
    titleKey: 'products',
    url: '/products',
    icon: Package,
  },
  {
    titleKey: 'orders',
    url: '/orders',
    icon: ShoppingCart,
  },
];

// Settings sub-items
const settingsSubItems = [
  {
    titleKey: 'themes',
    url: '/settings/themes',
    icon: Palette,
  },
];

export function AppSidebar() {
  const { t } = useTranslation('sidebar');
  const location = useLocation();
  const navigate = useNavigate();
  const { user: me } = useSelector((state: RootState) => state.me);
  const { profile: vendorProfile } = useSelector(
    (state: RootState) => state.vendor,
  );
  const [settingsOpen, setSettingsOpen] = useState(() => {
    return location.pathname.startsWith('/settings');
  });

  const firstName = me?.firstName?.trim() || '';
  const lastName = me?.lastName?.trim() || '';
  const fullName = `${firstName} ${lastName}`.trim();

  const user = {
    name: vendorProfile?.name || fullName || me?.email || 'Guest User',
    email: me?.email || '',
    avatar: vendorProfile?.profileImageUrl || me?.profileImageUrl || '',
    initials: fullName
      ? `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase() || 'GU'
      : 'GU',
  };

  const isActive = (url: string) => {
    if (url === '/') {
      return location.pathname === '/';
    }
    return location.pathname.startsWith(url);
  };

  const handleUserLogout = async () => {
    await logoutUser();
  };

  return (
    <Sidebar collapsible='icon'>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              asChild
              size='lg'
              className='my-2 hover:bg-primary/10 transition-all duration-300 group/logo'
              onClick={() => navigate('/')}
            >
              <a href='/' className='cursor-pointer'>
                <div className='relative shrink-0'>
                  <div className='absolute inset-0 bg-primary/20 rounded-lg blur-sm group-hover/logo:bg-primary/30 transition-all duration-300' />
                  <img
                    src={vendorProfile?.profileImageUrl || Logo}
                    title={vendorProfile?.name || 'Akkooo Logo'}
                    className='size-8 rounded-lg relative z-10 shadow-md group-hover/logo:scale-110 transition-transform duration-300 object-cover'
                  />
                </div>
                <div className='flex flex-col min-w-0'>
                  <span className='text-base font-bold bg-gradient-to-r from-primary via-primary to-primary/70 bg-clip-text text-transparent group-hover/logo:from-primary group-hover/logo:to-primary transition-all duration-300 truncate'>
                    {vendorProfile?.name || t('appName')}
                  </span>
                  <span className='text-[10px] text-muted-foreground font-medium tracking-wider uppercase truncate'>
                    {t('appSubtitle')}
                  </span>
                </div>
              </a>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        {/* Main Navigation */}
        <SidebarGroup>
          <SidebarGroupLabel className='text-xs font-semibold text-muted-foreground/70 uppercase tracking-wider'>
            {t('mainMenu')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu className='mt-2'>
              {mainNavItems.map((item) => {
                return (
                  <SidebarMenuItem key={item.titleKey}>
                    <SidebarMenuButton
                      asChild
                      tooltip={t(item.titleKey)}
                      isActive={isActive(item.url)}
                      className={`
                      transition-all duration-200
                      ${
                        isActive(item.url)
                          ? 'bg-primary text-primary-foreground hover:bg-primary/90 font-semibold shadow-sm'
                          : 'hover:bg-muted/50'
                      }
                    `}
                    >
                      <a
                        href={item.url}
                        onClick={(e) => {
                          if (item.url !== '#') {
                            e.preventDefault();
                            navigate(item.url);
                          }
                        }}
                      >
                        <item.icon className='h-4 w-4' />
                        <span>{t(item.titleKey)}</span>
                      </a>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        <SidebarSeparator className='my-2' />

        {/* Settings Section */}
        <SidebarGroup>
          <SidebarGroupLabel className='text-xs font-semibold text-muted-foreground/70 uppercase tracking-wider'>
            {t('system')}
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu className='mt-2'>
              <SidebarMenuItem>
                <Collapsible
                  open={settingsOpen}
                  onOpenChange={setSettingsOpen}
                  className='group/collapsible [&[data-state=open]>button>svg.chevron]:rotate-90'
                >
                  <CollapsibleTrigger asChild>
                    <SidebarMenuButton tooltip={t('settings')}>
                      <Settings className='size-5 shrink-0' />
                      <span>{t('settings')}</span>
                      <ChevronRight className='chevron ml-auto size-4 shrink-0 transition-transform duration-200' />
                    </SidebarMenuButton>
                  </CollapsibleTrigger>
                  <CollapsibleContent>
                    <SidebarMenuSub>
                      {settingsSubItems.map((item) => (
                        <SidebarMenuSubItem key={item.titleKey}>
                          <SidebarMenuSubButton
                            asChild
                            isActive={isActive(item.url)}
                          >
                            <a
                              href={item.url}
                              onClick={(e) => {
                                e.preventDefault();
                                navigate(item.url);
                              }}
                            >
                              <item.icon className='size-4 shrink-0' />
                              <span>{t(item.titleKey)}</span>
                            </a>
                          </SidebarMenuSubButton>
                        </SidebarMenuSubItem>
                      ))}
                    </SidebarMenuSub>
                  </CollapsibleContent>
                </Collapsible>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarRail />
      <SidebarFooter>
        <NavUser
          user={user}
          onLogOut={handleUserLogout}
          onProfileClick={() => navigate('/profile')}
          onVendorProfileClick={() => navigate('/vendor-profile')}
          currentPath={location.pathname}
        />
      </SidebarFooter>
    </Sidebar>
  );
}


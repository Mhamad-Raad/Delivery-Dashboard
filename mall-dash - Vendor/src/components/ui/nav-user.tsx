import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { User, LogOut, ChevronDown, Store } from 'lucide-react';
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '@/components/ui/collapsible';
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from '@/components/ui/sidebar';
import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';

export function NavUser({
  user,
  onLogOut,
  onProfileClick,
  onVendorProfileClick,
  currentPath,
}: {
  user: {
    name: string;
    email: string;
    avatar: string;
    initials?: string;
  };
  onLogOut: () => void;
  onProfileClick?: () => void;
  onVendorProfileClick?: () => void;
  currentPath?: string;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const { t } = useTranslation('sidebar');
  const initials = user.initials || user.name.slice(0, 2).toUpperCase();
  const containerRef = useRef<HTMLDivElement>(null);
  const { setOpen, state } = useSidebar();

  const isProfileActive = currentPath && currentPath.startsWith('/profile');
  const isVendorProfileActive =
    currentPath && currentPath.startsWith('/vendor-profile');

  // Close accordion when sidebar collapses
  useEffect(() => {
    if (state === 'collapsed') {
      setIsOpen(false);
    }
  }, [state]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        containerRef.current &&
        !containerRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  const handleOpenChange = (open: boolean) => {
    if (open) {
      // Open the sidebar first if it's collapsed
      setOpen(true);
    }
    setIsOpen(open);
  };

  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <div ref={containerRef}>
          <Collapsible open={isOpen} onOpenChange={handleOpenChange}>
            <CollapsibleTrigger asChild>
              <SidebarMenuButton
                size='lg'
                className='w-full transition-all duration-200 hover:bg-sidebar-accent/50 data-[state=open]:bg-sidebar-accent'
              >
                <Avatar className='h-8 w-8 shrink-0 rounded-full transition-all duration-200'>
                  <AvatarImage
                    src={user.avatar}
                    alt={user.name}
                    className='object-cover'
                  />
                  <AvatarFallback className='rounded-full bg-gradient-to-br from-primary/80 to-primary text-primary-foreground font-semibold text-xs'>
                    {initials}
                  </AvatarFallback>
                </Avatar>
                <div className='grid flex-1 text-left text-sm leading-tight'>
                  <span className='truncate font-semibold'>{user.name}</span>
                  <span className='text-muted-foreground truncate text-xs'>
                    {user.email}
                  </span>
                </div>
                <ChevronDown
                  className={`ml-auto size-4 text-muted-foreground transition-transform duration-300 ${isOpen ? '' : 'rotate-180'}`}
                />
              </SidebarMenuButton>
            </CollapsibleTrigger>

            <CollapsibleContent className='overflow-hidden data-[state=closed]:animate-collapsible-up data-[state=open]:animate-collapsible-down'>
              <div className='space-y-0.5 py-1 px-1'>
                {onProfileClick && (
                  <button
                    onClick={onProfileClick}
                    className={`flex w-full items-center gap-2.5 rounded-md px-2 py-2 text-left transition-colors duration-150 hover:bg-accent ${
                      isProfileActive ? 'bg-accent' : ''
                    }`}
                  >
                    <div className='flex h-7 w-7 items-center justify-center rounded-md bg-primary/10'>
                      <User className='h-3.5 w-3.5 text-primary' />
                    </div>
                    <div className='flex flex-col'>
                      <span className='font-medium text-sm'>
                        {t('profile')}
                      </span>
                      <span className='text-xs text-muted-foreground'>
                        {t('profileDescription')}
                      </span>
                    </div>
                  </button>
                )}

                {onVendorProfileClick && (
                  <button
                    onClick={onVendorProfileClick}
                    className={`flex w-full items-center gap-2.5 rounded-md px-2 py-2 text-left transition-colors duration-150 hover:bg-accent ${
                      isVendorProfileActive ? 'bg-accent' : ''
                    }`}
                  >
                    <div className='flex h-7 w-7 items-center justify-center rounded-md bg-primary/10'>
                      <Store className='h-3.5 w-3.5 text-primary' />
                    </div>
                    <div className='flex flex-col'>
                      <span className='font-medium text-sm'>
                        {t('vendorProfile')}
                      </span>
                      <span className='text-xs text-muted-foreground'>
                        {t('vendorProfileDescription')}
                      </span>
                    </div>
                  </button>
                )}

                <div className='my-1 h-px bg-border/50' />

                <button
                  onClick={onLogOut}
                  className='flex w-full items-center gap-2.5 rounded-md px-2 py-2 text-left transition-colors duration-150 hover:bg-destructive/10 group'
                >
                  <div className='flex h-7 w-7 items-center justify-center rounded-md bg-destructive/10 group-hover:bg-destructive/20 transition-colors'>
                    <LogOut className='h-3.5 w-3.5 text-destructive' />
                  </div>
                  <span className='font-medium text-sm text-destructive'>
                    {t('logout')}
                  </span>
                </button>
              </div>
            </CollapsibleContent>
          </Collapsible>
        </div>
      </SidebarMenuItem>
    </SidebarMenu>
  );
}


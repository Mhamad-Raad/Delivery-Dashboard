import { useNavigate } from 'react-router-dom';
import { Clock, ExternalLink } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import { getActionConfig, getEntityIcon, getEntityRoute } from './auditUtils';
import CopyButton from './CopyButton';
import type { AuditLog } from '@/interfaces/Audit.interface';

interface AuditDetailsSidebarProps {
  log: AuditLog;
  isDeleted: boolean;
}

const AuditDetailsSidebar = ({ log, isDeleted }: AuditDetailsSidebarProps) => {
  const navigate = useNavigate();
  const actionConfig = getActionConfig(log.action);
  const ActionIcon = actionConfig.icon;
  const EntityIcon = getEntityIcon(log.entityName);
  const entityRoute = getEntityRoute(log.entityName, log.entityId);

  return (
    <Card className='flex flex-col overflow-hidden'>
      <ScrollArea className='flex-1'>
        <div className='p-5 space-y-6'>
          {/* User Section */}
          <div className='space-y-3'>
            <h3 className='text-sm font-semibold text-muted-foreground uppercase tracking-wider'>
              Performed By
            </h3>
            <div className='flex items-center gap-4 group'>
              <Avatar className='h-14 w-14'>
                <AvatarImage src={log.profileImageUrl} alt={log.userEmail} />
                <AvatarFallback className='bg-primary/10 text-primary font-medium text-lg'>
                  {log.userEmail?.slice(0, 2).toUpperCase() || 'U'}
                </AvatarFallback>
              </Avatar>
              <div className='flex-1 min-w-0'>
                <p className='font-medium text-lg truncate'>{log.userEmail}</p>
                <p className='text-sm text-muted-foreground font-mono truncate'>
                  {log.userId}
                </p>
              </div>
              <CopyButton text={log.userEmail || ''} />
            </div>
          </div>

          <Separator />

          {/* Action & Target */}
          <div className='space-y-4'>
            <h3 className='text-sm font-semibold text-muted-foreground uppercase tracking-wider'>
              Action Details
            </h3>
            <div className='space-y-4'>
              <div className='flex items-center justify-between'>
                <span className='text-base text-muted-foreground'>Action</span>
                <Badge variant='outline' className={`${actionConfig.color} gap-1.5 text-sm py-1 px-2.5`}>
                  <ActionIcon className='h-4 w-4' />
                  {actionConfig.label}
                </Badge>
              </div>
              <div className='flex items-center justify-between'>
                <span className='text-base text-muted-foreground'>Entity</span>
                <div className='flex items-center gap-2'>
                  <div className='flex h-7 w-7 items-center justify-center rounded bg-muted/60'>
                    <EntityIcon className='h-4 w-4 text-muted-foreground' />
                  </div>
                  <span className='font-medium text-base'>{log.entityName}</span>
                </div>
              </div>
              <div className='flex items-center justify-between group'>
                <span className='text-base text-muted-foreground'>Entity ID</span>
                <div className='flex items-center gap-1'>
                  <code className='text-sm bg-muted/50 px-2.5 py-1 rounded font-mono'>
                    {log.entityId}
                  </code>
                  <CopyButton text={log.entityId || ''} />
                </div>
              </div>
            </div>
          </div>

          <Separator />

          {/* Timestamp */}
          <div className='space-y-3'>
            <h3 className='text-sm font-semibold text-muted-foreground uppercase tracking-wider'>
              Timestamp
            </h3>
            <div className='flex items-center gap-4'>
              <div className='flex h-12 w-12 items-center justify-center rounded-lg bg-muted/50'>
                <Clock className='h-6 w-6 text-muted-foreground' />
              </div>
              <div>
                <p className='font-medium text-lg'>
                  {new Date(log.timestamp).toLocaleDateString('en-US', {
                    weekday: 'long',
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                  })}
                </p>
                <p className='text-base text-muted-foreground'>
                  {new Date(log.timestamp).toLocaleTimeString('en-US', {
                    hour: 'numeric',
                    minute: '2-digit',
                    second: '2-digit',
                    hour12: true,
                  })}
                </p>
              </div>
            </div>
          </div>

          {/* Technical Details */}
          {(log.ipAddress || log.userAgent) && (
            <>
              <Separator />
              <div className='space-y-4'>
                <h3 className='text-sm font-semibold text-muted-foreground uppercase tracking-wider'>
                  Technical Details
                </h3>
                {log.ipAddress && (
                  <div className='flex items-center justify-between'>
                    <span className='text-base text-muted-foreground'>IP Address</span>
                    <code className='text-sm bg-muted/50 px-2.5 py-1 rounded font-mono'>
                      {log.ipAddress}
                    </code>
                  </div>
                )}
                {log.userAgent && (
                  <div className='space-y-1.5'>
                    <span className='text-base text-muted-foreground'>User Agent</span>
                    <code className='text-xs bg-muted/50 px-2.5 py-1.5 rounded font-mono block break-all text-muted-foreground'>
                      {log.userAgent}
                    </code>
                  </div>
                )}
              </div>
            </>
          )}

          {/* View Entity Link - Only show if not deleted */}
          {!isDeleted && entityRoute && (
            <>
              <Separator />
              <Button
                variant='outline'
                className='w-full gap-2'
                onClick={() => navigate(entityRoute)}
              >
                <ExternalLink className='h-4 w-4' />
                View {log.entityName}
              </Button>
            </>
          )}
        </div>
      </ScrollArea>
    </Card>
  );
};

export default AuditDetailsSidebar;

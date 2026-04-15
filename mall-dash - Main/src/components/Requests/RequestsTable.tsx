import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { ScrollArea } from '@/components/ui/scroll-area';
import {
  Clock,
  Hourglass,
  Loader2,
  CheckCircle2,
  XCircle,
  HelpCircle,
  FileText,
  ChevronRight,
} from 'lucide-react';
import CustomTablePagination from '../CustomTablePagination';
import type { RootState } from '@/store/store';
import type { Request } from '@/interfaces/Request.interface';

const getStatusConfig = (status: string) => {
  switch (status) {
    case 'Pending':
      return {
        icon: Hourglass,
        color: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/30',
        label: 'Pending',
      };
    case 'In Progress':
      return {
        icon: Loader2,
        color: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/30',
        label: 'In Progress',
      };
    case 'Resolved':
      return {
        icon: CheckCircle2,
        color: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/30',
        label: 'Resolved',
      };
    case 'Rejected':
      return {
        icon: XCircle,
        color: 'bg-red-500/10 text-red-600 dark:text-red-400 border-red-500/30',
        label: 'Rejected',
      };
    default:
      return {
        icon: HelpCircle,
        color: 'bg-gray-500/10 text-gray-600 dark:text-gray-400 border-gray-500/30',
        label: status || 'Unknown',
      };
  }
};

const formatRelativeTime = (timestamp: string) => {
  const now = new Date();
  const date = new Date(timestamp);
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'Just now';
  if (diffMins < 60) return `${diffMins}m ago`;
  if (diffHours < 24) return `${diffHours}h ago`;
  if (diffDays < 7) return `${diffDays}d ago`;
  return date.toLocaleDateString();
};

const RequestsTable = () => {
  const navigate = useNavigate();
  const { requests, loading, error, total } = useSelector(
    (state: RootState) => state.requests
  );

  const handleRowClick = (id: string) => {
    navigate(`/requests/${id}`);
  };

  if (error) {
    return (
      <div className='rounded-lg border bg-card shadow-sm p-8'>
        <div className='text-center text-destructive'>Error: {error}</div>
      </div>
    );
  }

  return (
    <div className='rounded-xl border bg-card shadow-sm flex flex-col overflow-hidden'>
      <ScrollArea className='h-[calc(100vh-280px)]'>
        <Table className='w-full'>
          <TableHeader>
            <TableRow className='hover:bg-transparent border-b bg-muted/50'>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-11 pl-4'>
                Tenant
              </TableHead>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-11'>
                Request
              </TableHead>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-11 w-[130px]'>
                Status
              </TableHead>
              <TableHead className='sticky top-0 z-10 font-semibold text-foreground/80 bg-muted/50 backdrop-blur-sm border-b h-11 w-[150px] text-right pr-4'>
                Submitted
              </TableHead>
              <TableHead className='sticky top-0 z-10 w-8 bg-muted/50 backdrop-blur-sm border-b h-11 pr-4'></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {loading ? (
              Array.from({ length: 10 }).map((_, index) => (
                <TableRow key={`skeleton-${index}`} className='border-b'>
                  <TableCell className='py-4 pl-4'>
                    <div className='flex items-center gap-3'>
                      <Skeleton className='h-8 w-8 rounded-full shrink-0' />
                      <Skeleton className='h-4 w-32' />
                    </div>
                  </TableCell>
                  <TableCell className='py-4'>
                    <div className='flex flex-col gap-1'>
                      <Skeleton className='h-4 w-48' />
                      <Skeleton className='h-3 w-24' />
                    </div>
                  </TableCell>
                  <TableCell className='py-4'>
                    <Skeleton className='h-6 w-[100px] rounded-full' />
                  </TableCell>
                  <TableCell className='py-4 pr-4'>
                    <div className='flex flex-col items-end gap-1'>
                      <Skeleton className='h-4 w-14' />
                      <Skeleton className='h-3 w-20' />
                    </div>
                  </TableCell>
                  <TableCell className='py-4 pr-4'>
                    <Skeleton className='h-4 w-4 rounded' />
                  </TableCell>
                </TableRow>
              ))
            ) : requests.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} className='h-40 text-center'>
                  <div className='flex flex-col items-center gap-3 text-muted-foreground'>
                    <div className='rounded-full bg-muted/50 p-4'>
                      <FileText className='h-8 w-8 opacity-50' />
                    </div>
                    <div>
                      <p className='font-medium'>No requests found</p>
                      <p className='text-sm text-muted-foreground/70'>
                        Tenant requests will appear here
                      </p>
                    </div>
                  </div>
                </TableCell>
              </TableRow>
            ) : (
              requests.map((request: Request) => {
                const statusConfig = getStatusConfig(request.status);
                const StatusIcon = statusConfig.icon;

                return (
                  <TableRow
                    key={request.id}
                    className='hover:bg-muted/50 transition-colors border-b last:border-0 cursor-pointer group'
                    onClick={() => handleRowClick(request.id)}
                  >
                    {/* Tenant Column */}
                    <TableCell className='py-4 pl-4'>
                      <div className='flex items-center gap-3'>
                        <Avatar className='h-8 w-8 shrink-0'>
                          <AvatarFallback className='bg-primary/10 text-primary text-xs font-medium'>
                            {request.tenantName?.slice(0, 2).toUpperCase() || 'T'}
                          </AvatarFallback>
                        </Avatar>
                        <span className='text-sm font-medium truncate'>
                          {request.tenantName || '-'}
                        </span>
                      </div>
                    </TableCell>

                    {/* Request Column */}
                    <TableCell className='py-4'>
                      <div className='flex flex-col'>
                        <span className='font-medium text-sm line-clamp-1'>
                          {request.title}
                        </span>
                        <code className='text-[11px] font-mono text-muted-foreground'>
                          #{request.id}
                        </code>
                      </div>
                    </TableCell>

                    {/* Status Column */}
                    <TableCell className='py-4'>
                      <Badge
                        variant='outline'
                        className={`${statusConfig.color} gap-1.5 font-medium text-xs`}
                      >
                        <StatusIcon className='h-3 w-3' />
                        {statusConfig.label}
                      </Badge>
                    </TableCell>

                    {/* Submitted Column */}
                    <TableCell className='py-4 text-right pr-4'>
                      <div className='flex flex-col items-end'>
                        <span className='text-sm font-medium flex items-center gap-1'>
                          <Clock className='h-3 w-3 text-muted-foreground' />
                          {request.createdAt ? formatRelativeTime(request.createdAt) : '-'}
                        </span>
                        <span className='text-[11px] text-muted-foreground'>
                          {request.createdAt
                            ? new Date(request.createdAt).toLocaleString('en-US', {
                                month: 'short',
                                day: 'numeric',
                                hour: 'numeric',
                                minute: '2-digit',
                              })
                            : ''}
                        </span>
                      </div>
                    </TableCell>

                    {/* Chevron */}
                    <TableCell className='py-4 pr-4'>
                      <ChevronRight className='h-4 w-4 text-muted-foreground opacity-0 group-hover:opacity-100 transition-opacity' />
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </ScrollArea>
      <div className='p-4 border-t bg-muted/20 shrink-0'>
        <CustomTablePagination
          total={total}
          suggestions={[10, 20, 50, 100]}
        />
      </div>
    </div>
  );
};

export default RequestsTable;


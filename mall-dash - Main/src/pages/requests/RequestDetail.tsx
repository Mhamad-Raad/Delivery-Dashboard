import { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  Clock,
  FileText,
  Image as ImageIcon,
  Hourglass,
  Loader2,
  CheckCircle2,
  XCircle,
  HelpCircle,
  Calendar,
  Hash,
  X,
  ChevronLeft,
  ChevronRight,
  ZoomIn,
  UserCircle,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Separator } from '@/components/ui/separator';
import {
  fetchRequestDetails,
  clearSelectedRequest,
} from '@/store/slices/requestsSlice';
import type { RootState, AppDispatch } from '@/store/store';

const getStatusConfig = (status: string) => {
  switch (status) {
    case 'Pending':
      return {
        icon: Hourglass,
        color: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/30',
        bgColor: 'bg-amber-500',
        label: 'Pending',
      };
    case 'In Progress':
      return {
        icon: Loader2,
        color: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/30',
        bgColor: 'bg-blue-500',
        label: 'In Progress',
      };
    case 'Resolved':
      return {
        icon: CheckCircle2,
        color: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/30',
        bgColor: 'bg-emerald-500',
        label: 'Resolved',
      };
    case 'Rejected':
      return {
        icon: XCircle,
        color: 'bg-red-500/10 text-red-600 dark:text-red-400 border-red-500/30',
        bgColor: 'bg-red-500',
        label: 'Rejected',
      };
    default:
      return {
        icon: HelpCircle,
        color: 'bg-gray-500/10 text-gray-600 dark:text-gray-400 border-gray-500/30',
        bgColor: 'bg-gray-500',
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

const RequestDetailPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();
  const { selectedRequest, loading, error } = useSelector(
    (state: RootState) => state.requests
  );
  const [lightboxOpen, setLightboxOpen] = useState(false);
  const [lightboxIndex, setLightboxIndex] = useState(0);

  useEffect(() => {
    if (id) {
      dispatch(fetchRequestDetails(id));
    }
    return () => {
      dispatch(clearSelectedRequest());
    };
  }, [id, dispatch]);

  const openLightbox = (index: number) => {
    setLightboxIndex(index);
    setLightboxOpen(true);
  };

  const closeLightbox = () => {
    setLightboxOpen(false);
  };

  const nextImage = () => {
    if (selectedRequest?.images) {
      setLightboxIndex((prev) => (prev + 1) % selectedRequest.images.length);
    }
  };

  const prevImage = () => {
    if (selectedRequest?.images) {
      setLightboxIndex((prev) => (prev - 1 + selectedRequest.images.length) % selectedRequest.images.length);
    }
  };

  if (error) {
    return (
      <section className='w-full h-full flex flex-col gap-4 overflow-hidden'>
        <div className='flex items-center gap-3'>
          <Button
            variant='ghost'
            size='icon'
            onClick={() => navigate('/requests')}
            className='h-9 w-9 shrink-0'
          >
            <ArrowLeft className='h-4 w-4' />
          </Button>
          <div>
            <h1 className='text-2xl font-bold tracking-tight'>Request Details</h1>
          </div>
        </div>
        <Card className='border-destructive/50'>
          <CardContent className='pt-6'>
            <div className='flex flex-col items-center gap-4 py-8'>
              <div className='rounded-full bg-destructive/10 p-4'>
                <XCircle className='h-8 w-8 text-destructive' />
              </div>
              <div className='text-center'>
                <p className='font-medium text-destructive'>Error loading request</p>
                <p className='text-sm text-muted-foreground mt-1'>{error}</p>
              </div>
              <Button variant='outline' onClick={() => navigate('/requests')}>
                Back to Requests
              </Button>
            </div>
          </CardContent>
        </Card>
      </section>
    );
  }

  if (loading) {
    return (
      <section className='w-full h-full flex flex-col gap-4 overflow-hidden'>
        <div className='flex items-center gap-3'>
          <Skeleton className='h-9 w-9 rounded-md' />
          <div className='space-y-2'>
            <Skeleton className='h-7 w-48' />
            <Skeleton className='h-4 w-32' />
          </div>
        </div>
        <div className='grid gap-4 lg:grid-cols-3 flex-1 min-h-0'>
          <Skeleton className='h-full rounded-xl' />
          <div className='lg:col-span-2'>
            <Skeleton className='h-full rounded-xl' />
          </div>
        </div>
      </section>
    );
  }

  if (!selectedRequest) {
    return (
      <section className='w-full h-full flex flex-col gap-4 overflow-hidden'>
        <div className='flex items-center gap-3'>
          <Button
            variant='ghost'
            size='icon'
            onClick={() => navigate('/requests')}
            className='h-9 w-9 shrink-0'
          >
            <ArrowLeft className='h-4 w-4' />
          </Button>
          <div>
            <h1 className='text-2xl font-bold tracking-tight'>Request Details</h1>
          </div>
        </div>
        <Card>
          <CardContent className='pt-6'>
            <div className='flex flex-col items-center gap-4 py-8'>
              <div className='rounded-full bg-muted p-4'>
                <FileText className='h-8 w-8 text-muted-foreground' />
              </div>
              <div className='text-center'>
                <p className='font-medium'>Request not found</p>
                <p className='text-sm text-muted-foreground mt-1'>
                  The request you're looking for doesn't exist or has been removed.
                </p>
              </div>
              <Button variant='outline' onClick={() => navigate('/requests')}>
                Back to Requests
              </Button>
            </div>
          </CardContent>
        </Card>
      </section>
    );
  }

  const statusConfig = getStatusConfig(selectedRequest.status);
  const StatusIcon = statusConfig.icon;

  return (
    <section className='w-full h-full flex flex-col gap-4 overflow-hidden'>
      {/* Header */}
      <div className='flex items-center gap-3 shrink-0'>
        <Button
          variant='ghost'
          size='icon'
          onClick={() => navigate('/requests')}
          className='h-9 w-9 shrink-0'
        >
          <ArrowLeft className='h-4 w-4' />
        </Button>
        <div className='min-w-0 flex-1'>
          <div className='flex items-center gap-3'>
            <h1 className='text-2xl font-bold tracking-tight truncate'>
              {selectedRequest.title}
            </h1>
            <Badge
              variant='outline'
              className={`${statusConfig.color} gap-1.5 font-medium shrink-0`}
            >
              <StatusIcon className='h-3 w-3' />
              {statusConfig.label}
            </Badge>
          </div>
          <div className='flex items-center gap-2 text-sm text-muted-foreground mt-0.5'>
            <Hash className='h-3 w-3' />
            <span className='font-mono'>{selectedRequest.id}</span>
            <span className='text-muted-foreground/50'>•</span>
            <Clock className='h-3 w-3' />
            <span>{formatRelativeTime(selectedRequest.createdAt)}</span>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className='grid gap-4 lg:grid-cols-3 flex-1 min-h-0 overflow-hidden'>
        {/* Left Column - Request Info */}
        <Card className='flex flex-col overflow-hidden'>
          <CardContent className='p-0 flex-1 overflow-y-auto'>
            {/* Tenant Info */}
            <div className='p-4'>
              <div className='flex items-center gap-2 mb-3'>
                <div className='flex h-6 w-6 items-center justify-center rounded bg-muted'>
                  <UserCircle className='h-3.5 w-3.5 text-muted-foreground' />
                </div>
                <h3 className='font-semibold text-sm'>Tenant</h3>
              </div>
              <div className='flex items-center gap-3'>
                <Avatar className='h-10 w-10 shrink-0'>
                  <AvatarFallback className='bg-primary/10 text-primary text-lg font-semibold'>
                    {selectedRequest.tenantName?.slice(0, 2).toUpperCase() || 'T'}
                  </AvatarFallback>
                </Avatar>
                <div className='min-w-0 flex-1'>
                  <p className='font-semibold truncate'>{selectedRequest.tenantName}</p>
                  <p className='text-sm text-muted-foreground font-mono truncate'>
                    {selectedRequest.tenantId}
                  </p>
                </div>
              </div>
            </div>

            <Separator />

            {/* Meta Info */}
            <div className='p-4 space-y-4'>
              <div className='flex items-start gap-3'>
                <div className='flex h-8 w-8 items-center justify-center rounded-md bg-muted shrink-0'>
                  <Calendar className='h-4 w-4 text-muted-foreground' />
                </div>
                <div className='min-w-0 flex-1'>
                  <p className='text-xs text-muted-foreground uppercase tracking-wide font-medium'>
                    Submitted
                  </p>
                  <p className='text-sm font-medium'>
                    {new Date(selectedRequest.createdAt).toLocaleString('en-US', {
                      weekday: 'short',
                      month: 'short',
                      day: 'numeric',
                      year: 'numeric',
                      hour: 'numeric',
                      minute: '2-digit',
                    })}
                  </p>
                </div>
              </div>

              <div className='flex items-start gap-3'>
                <div className='flex h-8 w-8 items-center justify-center rounded-md bg-muted shrink-0'>
                  <Clock className='h-4 w-4 text-muted-foreground' />
                </div>
                <div className='min-w-0 flex-1'>
                  <p className='text-xs text-muted-foreground uppercase tracking-wide font-medium'>
                    Last Updated
                  </p>
                  <p className='text-sm font-medium'>
                    {new Date(selectedRequest.updatedAt).toLocaleString('en-US', {
                      weekday: 'short',
                      month: 'short',
                      day: 'numeric',
                      year: 'numeric',
                      hour: 'numeric',
                      minute: '2-digit',
                    })}
                  </p>
                </div>
              </div>

              <div className='flex items-start gap-3'>
                <div className={`flex h-8 w-8 items-center justify-center rounded-md shrink-0 ${statusConfig.color.split(' ')[0]}`}>
                  <StatusIcon className={`h-4 w-4 ${statusConfig.color.split(' ')[1]}`} />
                </div>
                <div className='min-w-0 flex-1'>
                  <p className='text-xs text-muted-foreground uppercase tracking-wide font-medium'>
                    Status
                  </p>
                  <p className='text-sm font-medium'>{statusConfig.label}</p>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Right Column - Description & Attachments */}
        <Card className='lg:col-span-2 flex flex-col overflow-hidden'>
          <CardContent className='p-0 flex-1 overflow-y-auto'>
            {/* Description */}
            <div className='p-4'>
              <div className='flex items-center gap-2 mb-3'>
                <div className='flex h-6 w-6 items-center justify-center rounded bg-muted'>
                  <FileText className='h-3.5 w-3.5 text-muted-foreground' />
                </div>
                <h3 className='font-semibold text-sm'>Description</h3>
              </div>
              <div className='rounded-lg bg-muted/50 p-4'>
                <p className='text-sm leading-relaxed whitespace-pre-wrap'>
                  {selectedRequest.description || 'No description provided.'}
                </p>
              </div>
            </div>

            {/* Attachments */}
            {selectedRequest.images && selectedRequest.images.length > 0 && (
              <>
                <Separator />
                <div className='p-4'>
                  <div className='flex items-center gap-2 mb-3'>
                    <div className='flex h-6 w-6 items-center justify-center rounded bg-muted'>
                      <ImageIcon className='h-3.5 w-3.5 text-muted-foreground' />
                    </div>
                    <h3 className='font-semibold text-sm'>
                      Attachments
                      <span className='ml-2 text-xs text-muted-foreground font-normal'>
                        ({selectedRequest.images.length} {selectedRequest.images.length === 1 ? 'image' : 'images'})
                      </span>
                    </h3>
                  </div>
                  <div className='grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3'>
                    {selectedRequest.images.map((img, index) => (
                      <button
                        key={index}
                        onClick={() => openLightbox(index)}
                        className='group relative aspect-square rounded-lg overflow-hidden border bg-muted hover:ring-2 hover:ring-primary/50 transition-all focus:outline-none focus:ring-2 focus:ring-primary'
                      >
                        <img
                          src={img}
                          alt={`Attachment ${index + 1}`}
                          className='object-cover w-full h-full group-hover:scale-105 transition-transform duration-300'
                        />
                        <div className='absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center'>
                          <ZoomIn className='h-6 w-6 text-white opacity-0 group-hover:opacity-100 transition-opacity drop-shadow-lg' />
                        </div>
                      </button>
                    ))}
                  </div>
                </div>
              </>
            )}

            {/* Empty attachments state */}
            {(!selectedRequest.images || selectedRequest.images.length === 0) && (
              <>
                <Separator />
                <div className='p-4'>
                  <div className='flex items-center gap-2 mb-3'>
                    <div className='flex h-6 w-6 items-center justify-center rounded bg-muted'>
                      <ImageIcon className='h-3.5 w-3.5 text-muted-foreground' />
                    </div>
                    <h3 className='font-semibold text-sm'>Attachments</h3>
                  </div>
                  <div className='rounded-lg border border-dashed p-8 text-center'>
                    <ImageIcon className='h-8 w-8 mx-auto text-muted-foreground/50 mb-2' />
                    <p className='text-sm text-muted-foreground'>No attachments</p>
                  </div>
                </div>
              </>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Lightbox */}
      {lightboxOpen && selectedRequest.images && (
        <div
          className='fixed inset-0 z-50 bg-black/90 flex items-center justify-center'
          onClick={closeLightbox}
        >
          <Button
            variant='ghost'
            size='icon'
            className='absolute top-4 right-4 text-white hover:bg-white/20 h-10 w-10'
            onClick={closeLightbox}
          >
            <X className='h-6 w-6' />
          </Button>

          {selectedRequest.images.length > 1 && (
            <>
              <Button
                variant='ghost'
                size='icon'
                className='absolute left-4 text-white hover:bg-white/20 h-12 w-12'
                onClick={(e) => {
                  e.stopPropagation();
                  prevImage();
                }}
              >
                <ChevronLeft className='h-8 w-8' />
              </Button>
              <Button
                variant='ghost'
                size='icon'
                className='absolute right-4 text-white hover:bg-white/20 h-12 w-12'
                onClick={(e) => {
                  e.stopPropagation();
                  nextImage();
                }}
              >
                <ChevronRight className='h-8 w-8' />
              </Button>
            </>
          )}

          <div
            className='max-w-[90vw] max-h-[90vh] flex items-center justify-center'
            onClick={(e) => e.stopPropagation()}
          >
            <img
              src={selectedRequest.images[lightboxIndex]}
              alt={`Attachment ${lightboxIndex + 1}`}
              className='max-w-full max-h-[90vh] object-contain rounded-lg'
            />
          </div>

          {selectedRequest.images.length > 1 && (
            <div className='absolute bottom-4 left-1/2 -translate-x-1/2 flex items-center gap-2'>
              {selectedRequest.images.map((_, index) => (
                <button
                  key={index}
                  onClick={(e) => {
                    e.stopPropagation();
                    setLightboxIndex(index);
                  }}
                  className={`w-2 h-2 rounded-full transition-colors ${
                    index === lightboxIndex ? 'bg-white' : 'bg-white/40 hover:bg-white/60'
                  }`}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </section>
  );
};

export default RequestDetailPage;


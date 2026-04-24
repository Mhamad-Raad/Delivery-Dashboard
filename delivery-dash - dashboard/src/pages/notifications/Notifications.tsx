import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Bell, Loader2, Plus, Trash2, Users } from 'lucide-react';
import { toast } from 'sonner';

import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';

import {
  type BroadcastSummary,
  deleteBroadcast,
  fetchBroadcasts,
} from '@/data/Notifications';

const PAGE_SIZE = 20;

const formatDate = (iso: string) => {
  try {
    return new Date(iso).toLocaleString();
  } catch {
    return iso;
  }
};

const Notifications = () => {
  const navigate = useNavigate();
  const [items, setItems] = useState<BroadcastSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<BroadcastSummary | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const data = await fetchBroadcasts(0, PAGE_SIZE);
      setItems(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const onDelete = async () => {
    if (!selected) return;
    setDeleting(true);
    try {
      const result = await deleteBroadcast(selected.key);
      if ('error' in result) {
        toast.error(result.error || 'Failed to delete.');
        return;
      }
      toast.success(
        `Removed ${result.removed} notification${result.removed === 1 ? '' : 's'}.`
      );
      setItems((prev) => prev.filter((b) => b.key !== selected.key));
      setSelected(null);
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className='mx-auto w-full max-w-5xl space-y-6 p-6'>
      <div className='flex items-center justify-between gap-3'>
        <div className='flex items-center gap-3'>
          <div className='bg-primary/10 flex h-10 w-10 items-center justify-center rounded-lg'>
            <Bell className='text-primary h-5 w-5' />
          </div>
          <div>
            <h1 className='text-xl font-semibold'>Notifications</h1>
            <p className='text-muted-foreground text-sm'>
              Past broadcasts to the customer app.
            </p>
          </div>
        </div>
        <Button onClick={() => navigate('/notifications/new')}>
          <Plus className='mr-2 h-4 w-4' />
          Create
        </Button>
      </div>

      <Card className='overflow-hidden p-0'>
        {loading ? (
          <div className='text-muted-foreground flex h-48 items-center justify-center text-sm'>
            <Loader2 className='mr-2 h-4 w-4 animate-spin' /> Loading…
          </div>
        ) : items.length === 0 ? (
          <div className='text-muted-foreground flex h-48 flex-col items-center justify-center gap-2 text-sm'>
            <Bell className='h-6 w-6 opacity-40' />
            <p>No notifications sent yet.</p>
            <Button
              variant='outline'
              size='sm'
              onClick={() => navigate('/notifications/new')}
            >
              <Plus className='mr-2 h-4 w-4' />
              Send the first one
            </Button>
          </div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Title</TableHead>
                <TableHead>Message</TableHead>
                <TableHead className='w-32'>Recipients</TableHead>
                <TableHead className='w-48'>Sent</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.map((b) => (
                <TableRow
                  key={b.key}
                  className='hover:bg-accent/40 cursor-pointer'
                  onClick={() => setSelected(b)}
                >
                  <TableCell className='font-medium'>{b.title}</TableCell>
                  <TableCell className='text-muted-foreground max-w-md truncate'>
                    {b.message}
                  </TableCell>
                  <TableCell>
                    <span className='inline-flex items-center gap-1 text-sm'>
                      <Users className='h-3.5 w-3.5' />
                      {b.recipients}
                    </span>
                  </TableCell>
                  <TableCell className='text-muted-foreground text-sm'>
                    {formatDate(b.createdAt)}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Card>

      <Dialog
        open={selected !== null}
        onOpenChange={(open) => {
          if (!open) setSelected(null);
        }}
      >
        <DialogContent className='max-w-lg'>
          {selected && (
            <>
              <DialogHeader>
                <DialogTitle>{selected.title}</DialogTitle>
                <DialogDescription>
                  Sent {formatDate(selected.createdAt)} ·{' '}
                  {selected.recipients} recipient
                  {selected.recipients === 1 ? '' : 's'}
                </DialogDescription>
              </DialogHeader>

              <div className='space-y-4'>
                {selected.imageUrl && (
                  <img
                    src={selected.imageUrl}
                    alt={selected.title}
                    className='max-h-48 w-full rounded-md object-cover'
                  />
                )}
                <p className='whitespace-pre-wrap text-sm'>
                  {selected.message}
                </p>
              </div>

              <DialogFooter>
                <Button
                  variant='outline'
                  onClick={() => setSelected(null)}
                  disabled={deleting}
                >
                  Close
                </Button>
                <Button
                  variant='destructive'
                  onClick={onDelete}
                  disabled={deleting}
                >
                  {deleting ? (
                    <Loader2 className='mr-2 h-4 w-4 animate-spin' />
                  ) : (
                    <Trash2 className='mr-2 h-4 w-4' />
                  )}
                  Delete
                </Button>
              </DialogFooter>
            </>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default Notifications;

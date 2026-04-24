import { useCallback, useEffect, useRef, useState } from 'react';
import { Bell, Image as ImageIcon, Loader2, Search, Send, Users, X } from 'lucide-react';
import { toast } from 'sonner';

import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { ScrollArea } from '@/components/ui/scroll-area';

import {
  broadcastNotification,
  type BroadcastAudience,
} from '@/data/Notifications';
import { fetchUsers } from '@/data/Users';

interface CustomerRow {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
}

const Notifications = () => {
  const [title, setTitle] = useState('');
  const [body, setBody] = useState('');
  const [audience, setAudience] = useState<BroadcastAudience>('AllCustomers');
  const [selected, setSelected] = useState<CustomerRow[]>([]);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [sending, setSending] = useState(false);
  const fileRef = useRef<HTMLInputElement | null>(null);

  const onPickImage = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] ?? null;
    setImageFile(file);
    if (imagePreview) URL.revokeObjectURL(imagePreview);
    setImagePreview(file ? URL.createObjectURL(file) : null);
  };

  useEffect(() => {
    return () => {
      if (imagePreview) URL.revokeObjectURL(imagePreview);
    };
  }, [imagePreview]);

  const reset = () => {
    setTitle('');
    setBody('');
    setAudience('AllCustomers');
    setSelected([]);
    setImageFile(null);
    if (imagePreview) URL.revokeObjectURL(imagePreview);
    setImagePreview(null);
    if (fileRef.current) fileRef.current.value = '';
  };

  const onSend = async () => {
    if (!title.trim() || !body.trim()) {
      toast.error('Title and body are required.');
      return;
    }
    if (audience === 'SpecificUsers' && selected.length === 0) {
      toast.error('Pick at least one customer, or switch to "All customers".');
      return;
    }
    setSending(true);
    try {
      const result = await broadcastNotification({
        title: title.trim(),
        body: body.trim(),
        audience,
        customerIds: audience === 'SpecificUsers' ? selected.map((s) => s.id) : undefined,
        image: imageFile,
      });
      if ('error' in result) {
        toast.error(result.error || 'Failed to send notification.');
        return;
      }
      toast.success(`Notification sent to ${result.targeted} customer${result.targeted === 1 ? '' : 's'}.`);
      reset();
    } finally {
      setSending(false);
    }
  };

  return (
    <div className='mx-auto w-full max-w-4xl space-y-6 p-6'>
      <div className='flex items-center gap-3'>
        <div className='bg-primary/10 flex h-10 w-10 items-center justify-center rounded-lg'>
          <Bell className='text-primary h-5 w-5' />
        </div>
        <div>
          <h1 className='text-xl font-semibold'>Send Notification</h1>
          <p className='text-muted-foreground text-sm'>
            Push a notification to the customer app.
          </p>
        </div>
      </div>

      <Card className='space-y-5 p-6'>
        <div className='space-y-2'>
          <Label htmlFor='notification-title'>Title</Label>
          <Input
            id='notification-title'
            placeholder='e.g. 20% off tonight!'
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            maxLength={120}
          />
        </div>

        <div className='space-y-2'>
          <Label htmlFor='notification-body'>Body</Label>
          <Textarea
            id='notification-body'
            placeholder='Short message shown under the title…'
            rows={4}
            value={body}
            onChange={(e) => setBody(e.target.value)}
            maxLength={500}
          />
        </div>

        <div className='grid gap-4 md:grid-cols-2'>
          <div className='space-y-2'>
            <Label>Audience</Label>
            <Select
              value={audience}
              onValueChange={(v: BroadcastAudience) => setAudience(v)}
            >
              <SelectTrigger className='w-full'>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value='AllCustomers'>All customers</SelectItem>
                <SelectItem value='SpecificUsers'>Specific customers</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className='space-y-2'>
            <Label htmlFor='notification-image'>Image (optional)</Label>
            <div className='flex items-center gap-3'>
              <Input
                id='notification-image'
                ref={fileRef}
                type='file'
                accept='image/*'
                onChange={onPickImage}
                className='file:text-foreground file:mr-3 file:rounded-md file:border-0 file:bg-secondary file:px-3 file:py-1.5'
              />
              {imagePreview && (
                <button
                  type='button'
                  onClick={() => {
                    setImageFile(null);
                    if (imagePreview) URL.revokeObjectURL(imagePreview);
                    setImagePreview(null);
                    if (fileRef.current) fileRef.current.value = '';
                  }}
                  className='text-muted-foreground hover:text-foreground'
                  aria-label='Remove image'
                >
                  <X className='h-4 w-4' />
                </button>
              )}
            </div>
          </div>
        </div>

        {audience === 'SpecificUsers' && (
          <CustomerPicker selected={selected} onChange={setSelected} />
        )}

        {imagePreview && (
          <div className='rounded-lg border p-3'>
            <div className='text-muted-foreground mb-2 flex items-center gap-2 text-xs'>
              <ImageIcon className='h-3.5 w-3.5' /> Preview
            </div>
            <img
              src={imagePreview}
              alt='preview'
              className='max-h-48 rounded-md object-cover'
            />
          </div>
        )}

        <div className='flex items-center justify-end gap-2 pt-2'>
          <Button
            variant='outline'
            onClick={reset}
            disabled={sending}
          >
            Clear
          </Button>
          <Button onClick={onSend} disabled={sending}>
            {sending ? (
              <Loader2 className='mr-2 h-4 w-4 animate-spin' />
            ) : (
              <Send className='mr-2 h-4 w-4' />
            )}
            Send notification
          </Button>
        </div>
      </Card>
    </div>
  );
};

function CustomerPicker({
  selected,
  onChange,
}: {
  selected: CustomerRow[];
  onChange: (rows: CustomerRow[]) => void;
}) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [results, setResults] = useState<CustomerRow[]>([]);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async (term: string) => {
    setLoading(true);
    try {
      const data = await fetchUsers({ role: 3, searchTerm: term, limit: 30, page: 1 });
      const raw = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);
      setResults(
        raw.map((u: any) => ({
          id: u.id ?? u.userId,
          firstName: u.firstName ?? '',
          lastName: u.lastName ?? '',
          email: u.email ?? '',
          phoneNumber: u.phoneNumber,
        }))
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!open) return;
    const t = setTimeout(() => load(search), 250);
    return () => clearTimeout(t);
  }, [open, search, load]);

  const toggle = (row: CustomerRow) => {
    const has = selected.some((s) => s.id === row.id);
    onChange(has ? selected.filter((s) => s.id !== row.id) : [...selected, row]);
  };

  const remove = (id: string) => onChange(selected.filter((s) => s.id !== id));

  return (
    <div className='space-y-2'>
      <Label>Customers</Label>
      <div className='rounded-md border p-3'>
        <div className='mb-3 flex flex-wrap gap-2'>
          {selected.length === 0 ? (
            <span className='text-muted-foreground text-sm'>No customers selected</span>
          ) : (
            selected.map((s) => (
              <span
                key={s.id}
                className='bg-secondary text-secondary-foreground flex items-center gap-1 rounded-full px-3 py-1 text-xs'
              >
                {s.firstName} {s.lastName}
                <button
                  type='button'
                  onClick={() => remove(s.id)}
                  className='hover:text-foreground'
                  aria-label='Remove'
                >
                  <X className='h-3 w-3' />
                </button>
              </span>
            ))
          )}
        </div>

        <Dialog open={open} onOpenChange={setOpen}>
          <DialogTrigger asChild>
            <Button type='button' variant='outline' size='sm'>
              <Users className='mr-2 h-4 w-4' />
              Pick customers
            </Button>
          </DialogTrigger>
          <DialogContent className='max-w-lg'>
            <DialogHeader>
              <DialogTitle>Select customers</DialogTitle>
            </DialogHeader>
            <div className='relative mb-3'>
              <Search className='text-muted-foreground absolute top-2.5 left-3 h-4 w-4' />
              <Input
                placeholder='Search by name, email, or phone…'
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className='pl-9'
              />
            </div>
            <ScrollArea className='h-72 rounded-md border'>
              {loading ? (
                <div className='flex h-full items-center justify-center p-8 text-sm text-muted-foreground'>
                  <Loader2 className='mr-2 h-4 w-4 animate-spin' /> Loading…
                </div>
              ) : results.length === 0 ? (
                <div className='text-muted-foreground p-8 text-center text-sm'>
                  No customers match.
                </div>
              ) : (
                <ul className='divide-y'>
                  {results.map((row) => {
                    const isSelected = selected.some((s) => s.id === row.id);
                    return (
                      <li key={row.id}>
                        <button
                          type='button'
                          onClick={() => toggle(row)}
                          className={`flex w-full items-center justify-between px-3 py-2 text-left transition hover:bg-accent/50 ${isSelected ? 'bg-primary/5' : ''}`}
                        >
                          <div>
                            <div className='text-sm font-medium'>
                              {row.firstName} {row.lastName}
                            </div>
                            <div className='text-muted-foreground text-xs'>
                              {row.email}
                              {row.phoneNumber ? ` · ${row.phoneNumber}` : ''}
                            </div>
                          </div>
                          <input
                            type='checkbox'
                            checked={isSelected}
                            readOnly
                            className='h-4 w-4'
                          />
                        </button>
                      </li>
                    );
                  })}
                </ul>
              )}
            </ScrollArea>
          </DialogContent>
        </Dialog>
      </div>
    </div>
  );
}

export default Notifications;

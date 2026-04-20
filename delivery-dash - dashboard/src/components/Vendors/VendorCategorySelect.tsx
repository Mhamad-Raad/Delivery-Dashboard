import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Tags, Loader2 } from 'lucide-react';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

import { fetchVendorCategories } from '@/data/VendorCategories';
import type {
  VendorCategoryAPIResponse,
  VendorCategoryType,
} from '@/interfaces/VendorCategory.interface';
import { mapVendorCategoryAPIToUI } from '@/interfaces/VendorCategory.interface';

interface Props {
  value: string; // stringified id, or "all" when allowAll
  onValueChange: (value: string, item?: VendorCategoryType) => void;
  disabled?: boolean;
  allowAll?: boolean;
  placeholder?: string;
  className?: string;
  /**
   * Icon inside the trigger (left-aligned, absolute positioned).
   * Defaults to <Tags />. Pass null for no icon.
   */
  showIcon?: boolean;
}

export default function VendorCategorySelect({
  value,
  onValueChange,
  disabled,
  allowAll = false,
  placeholder,
  className,
  showIcon = true,
}: Props) {
  const { t } = useTranslation('vendors');
  const [items, setItems] = useState<VendorCategoryType[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setLoading(true);
      const res = await fetchVendorCategories({ activeOnly: true });
      if (cancelled) return;
      if (Array.isArray(res)) {
        setItems(
          (res as VendorCategoryAPIResponse[]).map(mapVendorCategoryAPIToUI)
        );
      }
      setLoading(false);
    };

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className={`relative ${className || ''}`}>
      {showIcon && (
        <div className='absolute left-3.5 top-1/2 -translate-y-1/2 z-10 pointer-events-none text-muted-foreground'>
          {loading ? (
            <Loader2 className='size-4 animate-spin' />
          ) : (
            <Tags className='size-4' />
          )}
        </div>
      )}
      <Select
        value={value}
        onValueChange={(v) => {
          const item = items.find((c) => String(c.id) === v);
          onValueChange(v, item);
        }}
        disabled={disabled || loading}
      >
        <SelectTrigger
          className={`w-full h-10 bg-background/80 border-border/50 rounded-xl transition-all duration-200 ${
            showIcon ? 'pl-10' : ''
          }`}
        >
          <SelectValue placeholder={placeholder || t('categorySelect.placeholder')} />
        </SelectTrigger>
        <SelectContent>
          {allowAll && (
            <SelectItem value='all'>{t('categorySelect.all')}</SelectItem>
          )}
          {items.map((c) => (
            <SelectItem key={c.id} value={String(c.id)}>
              {c.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

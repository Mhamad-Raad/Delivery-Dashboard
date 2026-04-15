import { Button } from '@/components/ui/button';
import { ArrowLeft, Package } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface ProductDetailHeaderProps {
  onBack: () => void;
  productName?: string;
}

export default function ProductDetailHeader({
  onBack,
  productName,
}: ProductDetailHeaderProps) {
  const { t } = useTranslation();

  return (
    <div className='flex items-center gap-3'>
      <Button
        variant='ghost'
        size='icon'
        onClick={onBack}
        className='size-9 shrink-0'
      >
        <ArrowLeft className='size-4' />
      </Button>
      <div className='flex items-center gap-3'>
        <div className='flex size-10 items-center justify-center rounded-lg bg-primary/10'>
          <Package className='size-5 text-primary' />
        </div>
        <div>
          <h1 className='text-xl font-semibold'>
            {productName || t('products.detailTitle')}
          </h1>
          <p className='text-sm text-muted-foreground'>
            {t('products.detailSubtitle')}
          </p>
        </div>
      </div>
    </div>
  );
}

import { Card, CardContent } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { useTranslation } from 'react-i18next';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Package,
  Upload,
  X,
  ImageIcon,
  DollarSign,
  Tag,
  FolderOpen,
  Store,
  Scale,
  PackageCheck,
} from 'lucide-react';
import type { ProductFormData } from '@/interfaces/Products.interface';
import type { CategoryDTO } from '@/data/Categories';

interface ProductInfoCardProps {
  formData: ProductFormData;
  categories?: CategoryDTO[];
  onInputChange: (
    field: keyof ProductFormData,
    value: string | number | File | boolean
  ) => void;
}

export default function ProductInfoCard({
  formData,
  categories = [],
  onInputChange,
}: ProductInfoCardProps) {
  const { t } = useTranslation();
  const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onInputChange('imageFile', file);
    }
  };

  const handleRemovePhoto = () => {
    onInputChange('imageFile', null as any);
    onInputChange('productImageUrl', '');
  };

  const previewUrl = formData.imageFile
    ? URL.createObjectURL(formData.imageFile)
    : formData.productImageUrl;

  const hasDiscount =
    formData.discountPrice != null &&
    formData.discountPrice > 0 &&
    formData.discountPrice < formData.price;

  return (
    <div className='grid grid-cols-1 lg:grid-cols-3 gap-6'>
      {/* Left Column - Image */}
      <Card className='lg:col-span-1'>
        <CardContent className='p-4'>
          <div className='space-y-4'>
            <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
              <ImageIcon className='size-4' />
              {t('products.detailProductImageTitle')}
            </div>

            {/* Image Preview */}
            <div className='relative aspect-square w-full overflow-hidden rounded-lg border bg-muted'>
              {previewUrl ? (
                <>
                  <img
                    src={previewUrl}
                    alt={formData.name || t('products.detailProductImageAlt')}
                    className='size-full object-cover'
                  />
                  <Button
                    type='button'
                    variant='secondary'
                    size='icon'
                    className='absolute top-2 right-2 size-8'
                    onClick={handleRemovePhoto}
                  >
                    <X className='size-4' />
                  </Button>
                </>
              ) : (
                <div className='flex size-full flex-col items-center justify-center gap-2 text-muted-foreground'>
                  <Package className='size-12' />
                  <span className='text-sm'>
                    {t('products.detailImageNone')}
                  </span>
                </div>
              )}
            </div>

            {/* Upload Button */}
            <Button
              type='button'
              variant='outline'
              className='w-full'
              onClick={() =>
                document.getElementById('photo-upload-edit')?.click()
              }
            >
              <Upload className='size-4 mr-2' />
              {t('products.detailUploadImage')}
            </Button>
            <input
              id='photo-upload-edit'
              type='file'
              accept='image/*'
              className='hidden'
              onChange={handlePhotoChange}
            />

            {/* Image URL */}
            <div className='space-y-2'>
              <Label htmlFor='productImageUrl' className='text-xs'>
                {t('products.detailOrPasteImageUrl')}
              </Label>
              <Input
                id='productImageUrl'
                placeholder='https://...'
                value={formData.productImageUrl || ''}
                onChange={(e) =>
                  onInputChange('productImageUrl', e.target.value)
                }
                className='text-sm'
              />
            </div>

            {/* Quick Stats */}
            <div className='pt-4 border-t space-y-3'>
              <div className='flex items-center justify-between text-sm'>
                <span className='text-muted-foreground'>
                  {t('products.detailStatusLabel')}
                </span>
                <Badge
                  variant={formData.inStock ? 'default' : 'secondary'}
                  className={
                    formData.inStock
                      ? 'bg-emerald-500/10 text-emerald-600 hover:bg-emerald-500/20'
                      : ''
                  }
                >
                  {formData.inStock
                    ? t('products.statusInStock')
                    : t('products.statusOutOfStock')}
                </Badge>
              </div>
              <div className='flex items-center justify-between text-sm'>
                <span className='text-muted-foreground'>
                  {t('products.detailWeightableLabel')}
                </span>
                <Badge
                  variant={formData.isWeightable ? 'default' : 'secondary'}
                  className={
                    formData.isWeightable
                      ? 'bg-blue-500/10 text-blue-600 hover:bg-blue-500/20'
                      : ''
                  }
                >
                  {formData.isWeightable
                    ? t('common.yes')
                    : t('common.no')}
                </Badge>
              </div>
              <div className='flex items-center justify-between text-sm'>
                <span className='text-muted-foreground'>
                  {t('products.detailDiscountLabel')}
                </span>
                <Badge
                  className={
                    hasDiscount
                      ? 'bg-amber-500/10 text-amber-600 hover:bg-amber-500/20'
                      : 'bg-muted text-muted-foreground'
                  }
                >
                  {hasDiscount
                    ? t('products.detailDiscountBadge', {
                        percent: Math.round(
                          ((formData.price - (formData.discountPrice || 0)) /
                            formData.price) *
                            100,
                        ),
                      })
                    : t('products.detailNoDiscount')}
                </Badge>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Right Column - Form Fields */}
      <Card className='lg:col-span-2'>
        <CardContent className='p-4 space-y-6'>
          {/* Basic Info Section */}
          <div className='space-y-4'>
            <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
              <Tag className='size-4' />
              {t('products.detailBasicInformation')}
            </div>

            <div className='grid gap-4'>
              {/* Product Name */}
              <div className='space-y-2'>
                <Label htmlFor='name'>
                  {t('products.detailNameLabel')}
                </Label>
                <Input
                  id='name'
                  value={formData.name}
                  onChange={(e) => onInputChange('name', e.target.value)}
                  placeholder={t('products.detailNamePlaceholder')}
                />
              </div>

              {/* Description */}
              <div className='space-y-2'>
                <Label htmlFor='description'>
                  {t('products.detailDescriptionLabel')}
                </Label>
                <Textarea
                  id='description'
                  value={formData.description || ''}
                  onChange={(e) => onInputChange('description', e.target.value)}
                  placeholder={t('products.detailDescriptionPlaceholder')}
                  rows={4}
                  className='resize-none'
                />
              </div>

              {/* Category */}
              <div className='space-y-2'>
                <Label className='flex items-center gap-2'>
                  <FolderOpen className='size-3.5' />
                  {t('products.detailCategoryLabel')}
                </Label>
                <Select
                  value={String(formData.categoryId ?? '')}
                  onValueChange={(value) =>
                    onInputChange('categoryId', parseInt(value, 10))
                  }
                >
                  <SelectTrigger className='w-full'>
                    <SelectValue
                      placeholder={t('products.detailCategoryPlaceholder')}
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map((c) => (
                      <SelectItem key={c.id} value={String(c.id)}>
                        {c.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
          </div>

          {/* Pricing Section */}
          <div className='space-y-4 pt-4 border-t'>
            <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
              <DollarSign className='size-4' />
              {t('products.detailPricingTitle')}
            </div>

            <div className='grid sm:grid-cols-2 gap-4'>
              {/* Price */}
              <div className='space-y-2'>
                <Label htmlFor='price'>
                  {t('products.detailRegularPriceLabel')}
                </Label>
                <div className='relative'>
                  <span className='absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground'>
                    {t('products.detailCurrencyPrefix')}
                  </span>
                  <Input
                    id='price'
                    type='number'
                    step='0.01'
                    min='0'
                    value={formData.price}
                    onChange={(e) =>
                      onInputChange('price', parseFloat(e.target.value) || 0)
                    }
                    placeholder={t('products.detailPricePlaceholder')}
                    className='pl-7'
                  />
                </div>
                <p className='text-xs text-muted-foreground'>
                  {t('products.detailRegularPriceHelp')}
                </p>
              </div>

              {/* Discount Price */}
              <div className='space-y-2'>
                <Label htmlFor='discountPrice'>
                  {t('products.detailDiscountPriceLabel')}
                </Label>
                <div className='relative'>
                  <span className='absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground'>
                    $
                  </span>
                  <Input
                    id='discountPrice'
                    type='number'
                    step='0.01'
                    min='0'
                    value={formData.discountPrice ?? ''}
                    onChange={(e) =>
                      onInputChange(
                        'discountPrice',
                        e.target.value ? parseFloat(e.target.value) : 0
                      )
                    }
                    placeholder={t('products.detailPricePlaceholder')}
                    className='pl-7'
                  />
                </div>
                <p className='text-xs text-muted-foreground'>
                  {t('products.detailDiscountPriceHelp')}
                </p>
              </div>
            </div>
          </div>

          {/* Product Options Section */}
          <div className='space-y-4 pt-4 border-t'>
            <div className='flex items-center gap-2 text-sm font-medium text-muted-foreground'>
              <Store className='size-4' />
              {t('products.detailOptionsTitle')}
            </div>

            <div className='grid sm:grid-cols-2 gap-4'>
              {/* In Stock Toggle */}
              <div className='flex items-center justify-between rounded-lg border p-4'>
                <div className='flex items-center gap-3'>
                  <div className='flex size-9 items-center justify-center rounded-md bg-emerald-500/10'>
                    <PackageCheck className='size-4 text-emerald-600' />
                  </div>
                  <div>
                    <Label className='text-sm font-medium'>
                      {t('products.detailInStockLabel')}
                    </Label>
                    <p className='text-xs text-muted-foreground'>
                      {t('products.detailInStockHelp')}
                    </p>
                  </div>
                </div>
                <Switch
                  checked={Boolean(formData.inStock)}
                  onCheckedChange={(checked) =>
                    onInputChange('inStock', checked)
                  }
                />
              </div>

              {/* Weightable Toggle */}
              <div className='flex items-center justify-between rounded-lg border p-4'>
                <div className='flex items-center gap-3'>
                  <div className='flex size-9 items-center justify-center rounded-md bg-blue-500/10'>
                    <Scale className='size-4 text-blue-600' />
                  </div>
                  <div>
                    <Label className='text-sm font-medium'>
                      {t('products.detailWeightableLabel')}
                    </Label>
                    <p className='text-xs text-muted-foreground'>
                      {t('products.detailWeightableHelp')}
                    </p>
                  </div>
                </div>
                <Switch
                  checked={Boolean(formData.isWeightable)}
                  onCheckedChange={(checked) =>
                    onInputChange('isWeightable', checked)
                  }
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}


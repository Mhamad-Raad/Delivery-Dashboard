import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { useTranslation } from 'react-i18next';

import VendorCategorySelect from '@/components/Vendors/VendorCategorySelect';
import type { VendorCategoryType } from '@/interfaces/VendorCategory.interface';

interface VendorBasicInfoProps {
  name: string;
  description: string;
  vendorCategoryId: string;
  vendorId: string;
  onInputChange: (field: string, value: string) => void;
  onVendorCategoryChange?: (id: string, item?: VendorCategoryType) => void;
  disabled?: boolean;
}

const VendorBasicInfo = ({
  name,
  description,
  vendorCategoryId,
  vendorId,
  onInputChange,
  onVendorCategoryChange,
  disabled,
}: VendorBasicInfoProps) => {
  const { t } = useTranslation('vendors');

  return (
    <div className='flex-1 space-y-4'>
      <div className='space-y-2'>
        <Label htmlFor='name'>
          {t('createVendor.basicInfo.businessName')}{' '}
          <span className='text-destructive'>*</span>
        </Label>
        <Input
          id='name'
          value={name}
          onChange={(e) => onInputChange('name', e.target.value)}
          placeholder={t('createVendor.basicInfo.businessNamePlaceholder')}
          disabled={disabled}
          className='text-lg font-semibold'
        />
      </div>

      <div className='space-y-2'>
        <Label htmlFor='vendorCategoryId'>
          {t('createVendor.basicInfo.businessType')}{' '}
          <span className='text-destructive'>*</span>
        </Label>
        <VendorCategorySelect
          value={vendorCategoryId}
          onValueChange={(v, item) => {
            onInputChange('vendorCategoryId', v);
            if (onVendorCategoryChange) onVendorCategoryChange(v, item);
          }}
          disabled={disabled}
          placeholder={t('createVendor.basicInfo.selectType')}
          showIcon={false}
        />
      </div>

      <div className='space-y-2'>
        <Label htmlFor='description'>
          {t('createVendor.basicInfo.description')}{' '}
          <span className='text-destructive'>*</span>
        </Label>
        <Textarea
          id='description'
          value={description}
          onChange={(e) => onInputChange('description', e.target.value)}
          rows={6}
          placeholder={t('createVendor.basicInfo.descriptionPlaceholder')}
          disabled={disabled}
        />
      </div>

      {vendorId && (
        <div className='pt-2'>
          <span className='text-xs text-muted-foreground'>
            {t('createVendor.basicInfo.id')}: {vendorId}
          </span>
        </div>
      )}
    </div>
  );
};

export default VendorBasicInfo;

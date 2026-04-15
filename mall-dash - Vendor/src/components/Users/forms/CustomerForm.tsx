import { useState, useEffect } from 'react';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import {
  Select,
  SelectTrigger,
  SelectContent,
  SelectItem,
  SelectValue,
} from '@/components/ui/select';
import { Mail, Lock, Building2, Image as ImageIcon, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';

const buildings = ['Sky Tower', 'Rose Heights', 'Emerald Plaza'];
const floors = ['1', '2', '3', '4', '5'];
const apartments = ['101', '202', '303', '404', '505'];

type CustomerFormProps = {
  formData: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    confirmPassword: string;
    phoneNumber: string;
    buildingId: string;
    floorId: string;
    apartmentId: string;
    photo?: File | null;
  };
  onInputChange: (field: string, value: unknown) => void;
};

export default function CustomerForm({
  formData,
  onInputChange,
}: CustomerFormProps) {
  const [preview, setPreview] = useState<string>('');
  const { t } = useTranslation();

  useEffect(() => {
    if (formData.photo instanceof File) {
      const url = URL.createObjectURL(formData.photo);
      setPreview(url);
      return () => URL.revokeObjectURL(url);
    } else {
      setPreview('');
    }
  }, [formData.photo]);

  return (
    <div className='space-y-8'>
      {/* Profile Picture & Name Section */}
      <div className='flex flex-col md:flex-row items-start gap-6 p-6 bg-muted/30 rounded-lg border'>
        <div className='w-32 h-32 md:w-36 md:h-36 rounded-full shrink-0 self-center md:self-start relative'>
          <input
            id='customer-photo'
            type='file'
            accept='image/*'
            className='hidden'
            onChange={(e) =>
              onInputChange('photo', e.target.files?.[0] || null)
            }
          />
          <label
            htmlFor='customer-photo'
            className='w-full h-full rounded-full bg-background flex items-center justify-center border-2 border-dashed border-muted-foreground/25 overflow-hidden cursor-pointer hover:border-primary/50 hover:bg-muted/50 transition-all group'
          >
            {preview ? (
              <img
                src={preview}
                alt='Preview'
                className='w-full h-full object-cover'
              />
            ) : (
              <div className='flex flex-col items-center gap-2'>
                <ImageIcon className='size-12 text-muted-foreground/50 group-hover:text-primary/70 transition-colors' />
                <span className='text-xs text-muted-foreground'>
                  {t('users.uploadPhoto')}
                </span>
              </div>
            )}
          </label>
          {preview && (
            <button
              type='button'
              onClick={(e) => {
                e.preventDefault();
                onInputChange('photo', null);
              }}
              className='absolute top-1 right-1 p-1.5 rounded-full bg-destructive text-destructive-foreground hover:bg-destructive/90 transition-colors shadow-lg z-10'
            >
              <X className='size-4' />
            </button>
          )}
        </div>
        <div className='flex-1 space-y-4 w-full'>
          <div className='space-y-2'>
            <Label htmlFor='customer-firstname' className='text-sm font-medium'>
              {t('profile.firstName')} <span className='text-destructive'>*</span>
            </Label>
            <Input
              id='customer-firstname'
              placeholder='John'
              value={formData.firstName}
              onChange={(e) => onInputChange('firstName', e.target.value)}
              className='h-11'
            />
          </div>
          <div className='space-y-2'>
            <Label htmlFor='customer-lastname' className='text-sm font-medium'>
              {t('profile.lastName')} <span className='text-destructive'>*</span>
            </Label>
            <Input
              id='customer-lastname'
              placeholder='Doe'
              value={formData.lastName}
              onChange={(e) => onInputChange('lastName', e.target.value)}
              className='h-11'
            />
          </div>
          {formData.photo && (
            <p className='text-xs text-muted-foreground'>
              ✓ {formData.photo.name}
            </p>
          )}
        </div>
      </div>

      <Separator />

      {/* Contact Information */}
      <div className='space-y-4'>
        <div className='flex items-center gap-2 pb-2'>
          <Mail className='size-5 text-primary' />
          <h3 className='text-base font-semibold'>{t('users.contactTitle')}</h3>
        </div>
        <div className='grid grid-cols-1 md:grid-cols-2 gap-4'>
          <div className='space-y-2'>
            <Label htmlFor='customer-phone' className='text-sm font-medium'>
              {t('users.phoneLabel')} <span className='text-destructive'>*</span>
            </Label>
            <Input
              id='customer-phone'
              type='tel'
              placeholder={t('users.phonePlaceholder')}
              value={formData.phoneNumber}
              onChange={(e) => onInputChange('phoneNumber', e.target.value)}
              className='h-11'
            />
          </div>
          <div className='space-y-2'>
            <Label htmlFor='customer-email' className='text-sm font-medium'>
              {t('users.emailLabel')} <span className='text-destructive'>*</span>
            </Label>
            <Input
              id='customer-email'
              type='email'
              placeholder={t('users.emailPlaceholder')}
              value={formData.email}
              onChange={(e) => onInputChange('email', e.target.value)}
              className='h-11'
            />
          </div>
        </div>
      </div>

      <Separator />

      {/* Security */}
      <div className='space-y-4'>
        <div className='flex items-center gap-2 pb-2'>
          <Lock className='size-5 text-primary' />
          <h3 className='text-base font-semibold'>{t('users.securityTitle')}</h3>
        </div>
        <div className='grid grid-cols-1 md:grid-cols-2 gap-4'>
          <div className='space-y-2'>
            <Label htmlFor='customer-password' className='text-sm font-medium'>
              {t('login.passwordLabel')} <span className='text-destructive'>*</span>
            </Label>
            <Input
              id='customer-password'
              type='password'
              placeholder={t('login.passwordPlaceholder')}
              value={formData.password}
              onChange={(e) => onInputChange('password', e.target.value)}
              className='h-11'
            />
          </div>
          <div className='space-y-2'>
            <Label htmlFor='customer-confirm' className='text-sm font-medium'>
              {t('users.confirmPasswordLabel')} <span className='text-destructive'>*</span>
            </Label>
            <Input
              id='customer-confirm'
              type='password'
              placeholder={t('login.passwordPlaceholder')}
              value={formData.confirmPassword}
              onChange={(e) => onInputChange('confirmPassword', e.target.value)}
              className='h-11'
            />
          </div>
        </div>
        <p className='text-xs text-muted-foreground'>
          {t('users.passwordRequirements')}
        </p>
      </div>

      <Separator />

      {/* Address */}
      <div className='space-y-4'>
        <div className='flex items-center gap-2 pb-2'>
          <Building2 className='size-5 text-primary' />
          <h3 className='text-base font-semibold'>{t('users.addressTitle')}</h3>
          <span className='text-xs text-muted-foreground font-normal ml-auto'>
            {t('users.optionalLabel')}
          </span>
        </div>
        <div className='grid grid-cols-1 md:grid-cols-3 gap-4'>
          <div className='space-y-2'>
            <Label htmlFor='customer-building' className='text-sm font-medium'>
              {t('users.buildingLabel')}
            </Label>
            <Select
              value={formData.buildingId}
              onValueChange={(value) => onInputChange('buildingId', value)}
            >
              <SelectTrigger id='customer-building' className='h-11'>
                <SelectValue placeholder={t('users.selectBuildingPlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {buildings.map((b, index) => (
                  <SelectItem key={index} value={index.toString()}>
                    {b}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className='space-y-2'>
            <Label htmlFor='customer-floor' className='text-sm font-medium'>
              {t('users.floorLabel')}
            </Label>
            <Select
              value={formData.floorId}
              onValueChange={(value) => onInputChange('floorId', value)}
            >
              <SelectTrigger id='customer-floor' className='h-11'>
                <SelectValue placeholder={t('users.selectFloorPlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {floors.map((f, index) => (
                  <SelectItem key={index} value={index.toString()}>
                    {t('users.floorOptionLabel', { number: f })}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
          <div className='space-y-2'>
            <Label htmlFor='customer-apartment' className='text-sm font-medium'>
              {t('users.apartmentLabel')}
            </Label>
            <Select
              value={formData.apartmentId}
              onValueChange={(value) => onInputChange('apartmentId', value)}
            >
              <SelectTrigger id='customer-apartment' className='h-11'>
                <SelectValue placeholder={t('users.selectApartmentPlaceholder')} />
              </SelectTrigger>
              <SelectContent>
                {apartments.map((a, index) => (
                  <SelectItem key={index} value={index.toString()}>
                    #{a}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>
    </div>
  );
}

import { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Image as ImageIcon, X } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import type { UserFormData } from '@/interfaces/Users.interface';

interface UserProfileCardProps {
  formData: UserFormData;
  onInputChange: (
    field: keyof UserFormData,
    value: string | number | File
  ) => void;
}

const UserProfileCard = ({ formData, onInputChange }: UserProfileCardProps) => {
  const [preview, setPreview] = useState<string>('');
  const { t } = useTranslation();

  useEffect(() => {
    if (formData.imageFile instanceof File) {
      const url = URL.createObjectURL(formData.imageFile);
      setPreview(url);
      return () => URL.revokeObjectURL(url);
    } else {
      setPreview(formData.profileImageUrl || '');
    }
  }, [formData.imageFile, formData.profileImageUrl]);

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) onInputChange('imageFile', file);
  };

  const handleRemoveImage = () => {
    onInputChange('imageFile', null as any);
    onInputChange('profileImageUrl', '');
    // Reset the file input
    const fileInput = document.getElementById('user-photo') as HTMLInputElement;
    if (fileInput) fileInput.value = '';
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className='text-lg'>
          {t('users.profileCardTitle')}
        </CardTitle>
        <CardDescription>
          {t('users.profileCardDescription')}
        </CardDescription>
      </CardHeader>
      <CardContent className='space-y-8'>
        {/* Profile Picture & Name Section */}
        <div className='flex flex-col md:flex-row items-start gap-6 p-6 bg-muted/30 rounded-lg border'>
          <div className='w-32 h-32 md:w-36 md:h-36 rounded-full shrink-0 self-center md:self-start relative'>
            <input
              id='user-photo'
              type='file'
              accept='image/*'
              className='hidden'
              onChange={handleImageChange}
            />
            <label
              htmlFor='user-photo'
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
                  handleRemoveImage();
                }}
                className='absolute top-1 right-1 p-1.5 rounded-full bg-destructive text-destructive-foreground hover:bg-destructive/90 transition-colors shadow-lg z-10'
              >
                <X className='size-4' />
              </button>
            )}
          </div>
          <div className='flex-1 space-y-4 w-full'>
            <div className='space-y-2'>
              <Label htmlFor='firstName' className='text-sm font-medium'>
                {t('profile.firstName')} <span className='text-destructive'>*</span>
              </Label>
              <Input
                id='firstName'
                placeholder='John'
                value={formData.firstName}
                onChange={(e) => onInputChange('firstName', e.target.value)}
                className='h-11'
              />
            </div>
            <div className='space-y-2'>
              <Label htmlFor='lastName' className='text-sm font-medium'>
                {t('profile.lastName')} <span className='text-destructive'>*</span>
              </Label>
              <Input
                id='lastName'
                placeholder='Doe'
                value={formData.lastName}
                onChange={(e) => onInputChange('lastName', e.target.value)}
                className='h-11'
              />
            </div>
            <div className='space-y-2'>
              <Label htmlFor='userRole' className='text-sm font-medium'>
                {t('users.userRoleLabel')}
              </Label>
              <Input
                id='userRole'
                value='Staff'
                readOnly
                className='h-11 bg-muted'
              />
            </div>
            {(formData.userId || formData.id || formData._id) && (
              <p className='text-xs text-muted-foreground'>
                {t('profile.userIdLabel')}: {formData.userId || formData.id || formData._id}
              </p>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default UserProfileCard;

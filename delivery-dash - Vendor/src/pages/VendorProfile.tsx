import { useState, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import type { RootState, AppDispatch } from '@/store/store';
import { updateVendor, getVendorProfile } from '@/store/slices/vendorSlice';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Loader2, Camera, Save } from 'lucide-react';

const VendorProfile = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { t } = useTranslation();
  const { profile, loading: vendorLoading } = useSelector(
    (state: RootState) => state.vendor,
  );

  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    Name: '',
    Description: '',
    OpeningTime: '',
    CloseTime: '',
    Type: '',
  });
  const [profileImage, setProfileImage] = useState<File | null>(null);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    // Fetch latest vendor profile on mount
    dispatch(getVendorProfile());
  }, [dispatch]);

  useEffect(() => {
    if (profile) {
      setFormData({
        Name: profile.name || '',
        Description: profile.description || '',
        OpeningTime: profile.openingTime || '',
        CloseTime: profile.closeTime || '',
        Type: profile.type || '',
      });
      setPreviewImage(profile.profileImageUrl);
    }
  }, [profile]);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setProfileImage(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setPreviewImage(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!profile?.id) return;

    setLoading(true);
    try {
      const updateData = {
        ...formData,
        ProfileImageUrl: profileImage || undefined,
      };

      const resultAction = await dispatch(updateVendor(updateData));

      if (updateVendor.fulfilled.match(resultAction)) {
        toast.success(t('vendorProfile.updateSuccess'));
      } else {
        if (resultAction.payload) {
          toast.error(resultAction.payload as string);
        } else {
          toast.error(t('vendorProfile.updateFailed'));
        }
      }
    } catch (error) {
      toast.error(t('vendorProfile.updateFailed'));
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  if (!profile && vendorLoading) {
    return (
      <div className='flex justify-center items-center h-full'>
        <Loader2 className='h-8 w-8 animate-spin' />
      </div>
    );
  }

  return (
    <div className='container mx-auto py-8 max-w-3xl'>
      <Card className='mb-8'>
        <CardHeader>
          <CardTitle className='text-2xl font-bold'>
            {t('vendorProfile.pageTitle')}
          </CardTitle>
          <CardDescription>{t('vendorProfile.pageSubtitle')}</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className='space-y-8'>
            {/* Profile Image Section */}
            <div className='flex flex-col items-center gap-4'>
              <div className='relative group'>
                <Avatar className='w-32 h-32 cursor-pointer border-4 border-background shadow-xl'>
                  <AvatarImage
                    src={previewImage || ''}
                    className='object-cover'
                  />
                  <AvatarFallback className='text-4xl bg-muted'>
                    {formData.Name?.[0]?.toUpperCase()}
                  </AvatarFallback>
                </Avatar>
                <div
                  className='absolute inset-0 flex items-center justify-center bg-black/40 rounded-full opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer'
                  onClick={() => fileInputRef.current?.click()}
                >
                  <Camera className='w-8 h-8 text-white' />
                </div>
                <input
                  type='file'
                  ref={fileInputRef}
                  className='hidden'
                  accept='image/*'
                  onChange={handleImageChange}
                />
              </div>
              <p className='text-sm text-muted-foreground'>
                {t('vendorProfile.uploadHint')}
              </p>
            </div>

            <div className='grid grid-cols-1 md:grid-cols-2 gap-6'>
              {/* Name */}
              <div className='space-y-2'>
                <Label htmlFor='Name'>{t('vendorProfile.shopName')}</Label>
                <Input
                  id='Name'
                  name='Name'
                  value={formData.Name}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Type */}
              <div className='space-y-2'>
                <Label htmlFor='Type'>{t('vendorProfile.shopType')}</Label>
                <Input
                  id='Type'
                  name='Type'
                  value={formData.Type}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Opening Time */}
              <div className='space-y-2'>
                <Label htmlFor='OpeningTime'>
                  {t('vendorProfile.openingTime')}
                </Label>
                <Input
                  id='OpeningTime'
                  name='OpeningTime'
                  type='time'
                  step='1'
                  value={formData.OpeningTime}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Close Time */}
              <div className='space-y-2'>
                <Label htmlFor='CloseTime'>
                  {t('vendorProfile.closingTime')}
                </Label>
                <Input
                  id='CloseTime'
                  name='CloseTime'
                  type='time'
                  step='1'
                  value={formData.CloseTime}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Description */}
              <div className='space-y-2 md:col-span-2'>
                <Label htmlFor='Description'>
                  {t('vendorProfile.description')}
                </Label>
                <Textarea
                  id='Description'
                  name='Description'
                  value={formData.Description}
                  onChange={handleChange}
                  required
                  className='min-h-[100px]'
                />
              </div>
            </div>

            <div className='flex justify-end pt-4'>
              <Button type='submit' disabled={loading}>
                {loading ? (
                  <>
                    <Loader2 className='mr-2 h-4 w-4 animate-spin' />
                    {t('vendorProfile.saving')}
                  </>
                ) : (
                  <>
                    <Save className='mr-2 h-4 w-4' />
                    {t('vendorProfile.saveChanges')}
                  </>
                )}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
};

export default VendorProfile;


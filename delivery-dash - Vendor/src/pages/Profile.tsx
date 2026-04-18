import { useState, useEffect, useRef } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import { toast } from 'sonner';
import type { RootState, AppDispatch } from '@/store/store';
import { updateMe } from '@/store/slices/meSlice';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  CardDescription,
} from '@/components/ui/card';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Loader2, Camera, Save } from 'lucide-react';

const Profile = () => {
  const dispatch = useDispatch<AppDispatch>();
  const { t } = useTranslation();
  const { user } = useSelector((state: RootState) => state.me);

  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    role: 0,
  });
  const [profileImage, setProfileImage] = useState<File | null>(null);
  const [previewImage, setPreviewImage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Removed ensureUserData per user request
  /*
  useEffect(() => {
    const ensureUserData = async () => {
      if (!user) {
        try {
          const response = await fetchMe();
          if (response.error || !response.user) {
            toast.error('Session expired. Please log in again.');
            dispatch(clearMe());
            dispatch(clearVendorProfile());
            await logoutUser();
            navigate('/login');
            return;
          }

          dispatch(setMe(response.user));
          if (response.vendorProfile) {
            dispatch(setVendorProfile(response.vendorProfile));
          } else {
            dispatch(clearVendorProfile());
          }
        } catch (error) {
          console.error('Error fetching user data:', error);
          toast.error('An error occurred. Please log in again.');
          await logoutUser();
          navigate('/login');
        }
      }
    };

    ensureUserData();
  }, [user, dispatch, navigate]);
  */

  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        email: user.email || '',
        phoneNumber: user.phoneNumber || '',
        role: user.role || 0,
      });
      setPreviewImage(user.profileImageUrl);
    }
  }, [user]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
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
    if (!user?._id) return;

    setLoading(true);
    try {
      const updateData = {
        FirstName: formData.firstName,
        LastName: formData.lastName,
        Email: formData.email,
        PhoneNumber: formData.phoneNumber,
        ProfileImageUrl: profileImage || undefined,
      };

      const resultAction = await dispatch(updateMe(updateData));

      if (updateMe.fulfilled.match(resultAction)) {
        toast.success(t('profile.updateSuccess'));
      } else {
        if (resultAction.payload) {
          toast.error(resultAction.payload as string);
        } else {
          toast.error(t('profile.updateFailed'));
        }
      }
    } catch (error) {
      toast.error(t('profile.updateFailed'));
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  if (!user) {
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
            {t('profile.pageTitle')}
          </CardTitle>
          <CardDescription>{t('profile.pageSubtitle')}</CardDescription>
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
                    {formData.firstName?.[0]?.toUpperCase()}
                    {formData.lastName?.[0]?.toUpperCase()}
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
                {t('profile.uploadHint')}
              </p>
            </div>

            <div className='grid grid-cols-1 md:grid-cols-2 gap-6'>
              {/* First Name */}
              <div className='space-y-2'>
                <Label htmlFor='firstName'>{t('profile.firstName')}</Label>
                <Input
                  id='firstName'
                  name='firstName'
                  value={formData.firstName}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Last Name */}
              <div className='space-y-2'>
                <Label htmlFor='lastName'>{t('profile.lastName')}</Label>
                <Input
                  id='lastName'
                  name='lastName'
                  value={formData.lastName}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Email */}
              <div className='space-y-2'>
                <Label htmlFor='email'>{t('profile.email')}</Label>
                <Input
                  id='email'
                  name='email'
                  type='email'
                  value={formData.email}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Phone Number */}
              <div className='space-y-2'>
                <Label htmlFor='phoneNumber'>{t('profile.phoneNumber')}</Label>
                <Input
                  id='phoneNumber'
                  name='phoneNumber'
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Role (Read Only) */}
              <div className='space-y-2'>
                <Label htmlFor='role'>{t('profile.role')}</Label>
                <div className='flex h-10 w-full rounded-md border border-input bg-muted px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 text-muted-foreground'>
                  {formData.role === 0
                    ? t('profile.roleAdmin')
                    : formData.role === 1
                      ? t('profile.roleVendor')
                      : t('profile.roleUser')}
                </div>
              </div>

              {/* ID (Hidden/Read Only) */}
              <div className='space-y-2 md:col-span-2'>
                <Label htmlFor='id' className='text-xs text-muted-foreground'>
                  {t('profile.userIdLabel')}
                </Label>
                <div className='text-xs font-mono text-muted-foreground'>
                  {user._id}
                </div>
              </div>
            </div>

            <div className='flex justify-end pt-4'>
              <Button
                type='submit'
                disabled={loading}
                className='min-w-[120px]'
              >
                {loading ? (
                  <>
                    <Loader2 className='mr-2 h-4 w-4 animate-spin' />
                    {t('profile.saving')}
                  </>
                ) : (
                  <>
                    <Save className='mr-2 h-4 w-4' />
                    {t('profile.saveChanges')}
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

export default Profile;

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Camera } from 'lucide-react';

const AccountSettings = () => {
  return (
    <div className='space-y-6'>
      {/* Profile Information */}
      <Card>
        <CardHeader>
          <CardTitle>Profile Information</CardTitle>
          <CardDescription>
            Update your account profile information and email address
          </CardDescription>
        </CardHeader>
        <CardContent className='space-y-6'>
          {/* Profile Picture */}
          <div className='flex items-center gap-6'>
            <Avatar className='h-24 w-24'>
              <AvatarImage src='https://github.com/shadcn.png' alt='Profile' />
              <AvatarFallback>JD</AvatarFallback>
            </Avatar>
            <div className='space-y-2'>
              <Button variant='outline' size='sm'>
                <Camera className='h-4 w-4 mr-2' />
                Change Photo
              </Button>
              <p className='text-sm text-muted-foreground'>
                JPG, PNG or GIF. Max size 2MB
              </p>
            </div>
          </div>

          {/* Name Fields */}
          <div className='grid grid-cols-1 md:grid-cols-2 gap-4'>
            <div className='space-y-2'>
              <Label htmlFor='firstName'>First Name</Label>
              <Input id='firstName' placeholder='John' defaultValue='John' />
            </div>
            <div className='space-y-2'>
              <Label htmlFor='lastName'>Last Name</Label>
              <Input id='lastName' placeholder='Doe' defaultValue='Doe' />
            </div>
          </div>

          {/* Email */}
          <div className='space-y-2'>
            <Label htmlFor='email'>Email</Label>
            <Input
              id='email'
              type='email'
              placeholder='john.doe@example.com'
              defaultValue='john.doe@example.com'
            />
          </div>

          {/* Phone */}
          <div className='space-y-2'>
            <Label htmlFor='phone'>Phone Number</Label>
            <Input
              id='phone'
              type='tel'
              placeholder='+1 (555) 000-0000'
              defaultValue='+1 (555) 000-0000'
            />
          </div>

          {/* Bio */}
          <div className='space-y-2'>
            <Label htmlFor='bio'>Bio</Label>
            <Textarea
              id='bio'
              placeholder='Tell us about yourself'
              className='resize-none h-24'
              defaultValue='I am a mall manager with 5 years of experience.'
            />
          </div>

          {/* Save Button */}
          <div className='flex justify-end gap-3'>
            <Button variant='outline'>Cancel</Button>
            <Button>Save Changes</Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default AccountSettings;

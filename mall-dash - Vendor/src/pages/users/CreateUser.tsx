import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
  CardDescription,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { UserPlus, ArrowLeft, Save } from 'lucide-react';
import StaffForm from '@/components/Users/forms/StaffForm';

import { createVendorStaff } from '@/data/Users';

export default function CreateUser() {
  const navigate = useNavigate();

  const [staffFormData, setStaffFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phoneNumber: '',
    role: 0, // 0  for staff role always
    photo: null,
  });

  const [loading, setLoading] = useState(false);

  // Handler to update staff form data
  const handleStaffInputChange = (field: string, value: unknown) => {
    setStaffFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleBack = () => {
    navigate('/users');
  };

  const handleCreateUser = async () => {
    setLoading(true);

    let res: any = {};
    let userName = '';

    const { confirmPassword, photo, ...userData } = staffFormData;

    if (
      !userData.firstName ||
      !userData.lastName ||
      !userData.email ||
      !userData.password ||
      !userData.phoneNumber
    ) {
      toast.error('Missing Required Fields', {
        description: 'Please fill in all required fields.',
      });
      setLoading(false);
      return;
    }

    if (userData.password.length < 6) {
      toast.error('Password too short', {
        description: 'Password must be at least 6 characters long.',
      });
      setLoading(false);
      return;
    }

    if (staffFormData.password !== staffFormData.confirmPassword) {
      toast.error('Passwords do not match', {
        description: 'Please make sure both password fields are identical.',
      });
      setLoading(false);
      return;
    }

    userName = `${staffFormData.firstName} ${staffFormData.lastName}`;

    const apiData = {
      FirstName: userData.firstName,
      LastName: userData.lastName,
      Email: userData.email,
      PhoneNumber: userData.phoneNumber,
      Password: userData.password,
      Role: userData.role,
      ...(photo ? { ProfileImageUrl: photo } : {}),
    };

    res = await createVendorStaff(apiData);

    setLoading(false);

    if (res.error) {
      toast.error('Failed to Create User', {
        description: res.error || 'An error occurred while creating the user.',
      });
    } else {
      toast.success('User Created Successfully!', {
        description: `${userName} has been added to the system.`,
      });
      navigate('/users');
    }
  };

  return (
    <div className='w-[calc(100%+2rem)] md:w-[calc(100%+3rem)] h-full flex flex-col -m-4 md:-m-6'>
      {/* Scrollable Content */}
      <div className='flex-1 overflow-y-auto p-4 md:p-6 space-y-6 pb-24'>
        {/* Header */}
        <div className='flex items-center gap-3 sm:gap-4'>
          <Button
            variant='outline'
            size='icon'
            onClick={handleBack}
            className='h-10 w-10 shrink-0'
          >
            <ArrowLeft className='size-4' />
          </Button>
          <div className='flex items-center gap-2 sm:gap-3'>
            <div className='p-2 rounded-lg bg-primary/10 text-primary shrink-0'>
              <UserPlus className='size-5' />
            </div>
            <div className='min-w-0'>
              <h1 className='text-xl sm:text-2xl font-bold tracking-tight'>
                Create New Staff
              </h1>
              <p className='text-xs sm:text-sm text-muted-foreground'>
                Add a new staff member to the system
              </p>
            </div>
          </div>
        </div>

        {/* Main Content */}
        <div className='space-y-6'>
          {/* Form Fields */}
          <Card>
            <CardHeader>
              <CardTitle className='text-lg'>Staff Information</CardTitle>
              <CardDescription>
                Fill in the details for the new staff member
              </CardDescription>
            </CardHeader>
            <CardContent>
              <StaffForm
                formData={staffFormData}
                onInputChange={handleStaffInputChange}
              />
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Footer Actions */}
      <div className='absolute bottom-0 left-0 right-0 p-4 md:p-6 bg-background/80 backdrop-blur-sm border-t flex items-center justify-end gap-3 md:gap-4 z-10'>
        <Button
          variant='outline'
          onClick={handleBack}
          className='min-w-[100px]'
        >
          Cancel
        </Button>
        <Button
          onClick={handleCreateUser}
          className='min-w-[140px] shadow-lg shadow-primary/20'
          disabled={loading}
        >
          {loading ? (
            'Creating...'
          ) : (
            <>
              <Save className='size-4 mr-2' />
              Create User
            </>
          )}
        </Button>
      </div>
    </div>
  );
}

import { useState, useEffect, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import UserDetailHeader from '@/components/Users/UserDetail/UserDetailHeader';
import UserDetailFooter from '@/components/Users/UserDetail/UserDetailFooter';
import UserProfileCard from '@/components/Users/UserDetail/UserProfileCard';
import ContactInfoCard from '@/components/Users/UserDetail/ContactInfoCard';
import LocationRoleCard from '@/components/Users/UserDetail/LocationRoleCard';
import UserDetailSkeleton from '@/components/Users/UserDetail/UserDetailSkeloton';
import UserErrorCard from '@/components/Users/UserDetail/UserErrorCard';
import ConfirmModal, {
  type ChangeDetail,
} from '@/components/ui/Modals/ConfirmModal';
import { useDispatch, useSelector } from 'react-redux';
import type { AppDispatch, RootState } from '@/store/store';
import {
  fetchUserById,
  clearUser,
  updateUser,
  deleteUser,
} from '@/store/slices/userSlice';
import { toast } from 'sonner';
import { initialUser } from '@/constants/Users';

import type { UserFormData } from '@/interfaces/Users.interface';

const UserDetail = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();

  console.log(id);

  const [showUpdateModal, setShowUpdateModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const {
    user,
    luser: loading,
    euser: error,
    updating,
    updatingError,
    deleting,
    deletingError,
  } = useSelector((state: RootState) => state.user);

  const [formData, setFormData] = useState<UserFormData>({
    ...initialUser,
    imageFile: undefined,
  });

  // Track if any changes have been made
  const hasChanges = useMemo(() => {
    if (!user || (!user.userId && !user.id)) return false;

    // Check if image file was added
    if (formData.imageFile instanceof File) return true;

    // Check other fields for changes
    return (
      user.firstName !== formData.firstName ||
      user.lastName !== formData.lastName ||
      user.email !== formData.email ||
      (user.phone || user.phoneNumber) !== formData.phoneNumber
    );
  }, [user, formData]);

  const changes = useMemo((): ChangeDetail[] => {
    if (!user || (!user.userId && !user.id)) return [];
    const changesList: ChangeDetail[] = [];
    const fieldLabels: Record<string, string> = {
      firstName: 'First Name',
      lastName: 'Last Name',
      email: 'Email',
      phoneNumber: 'Phone Number',
    };

    // Check for image change
    if (formData.imageFile instanceof File) {
      changesList.push({
        field: 'Profile Image',
        oldValue: user.profileImageUrl ? 'Current image' : 'No image',
        newValue: formData.imageFile.name,
      });
    }

    // Check other fields
    (['firstName', 'lastName', 'email', 'phoneNumber'] as const).forEach(
      (key) => {
        let userValue = user[key];
        if (key === 'phoneNumber') userValue = user.phone || user.phoneNumber;

        if (userValue !== formData[key]) {
          const oldVal = userValue;
          const newVal = formData[key];

          changesList.push({
            field: fieldLabels[key],
            oldValue: String(oldVal ?? ''),
            newValue: String(newVal ?? ''),
          });
        }
      }
    );
    return changesList;
  }, [user, formData]);

  const handleInputChange = (
    field: keyof UserFormData,
    value: string | number | File
  ) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  useEffect(() => {
    if (id) {
      dispatch(fetchUserById(id));
    }
    return () => {
      dispatch(clearUser());
    };
  }, [id, dispatch]);

  useEffect(() => {
    if (user) {
      setFormData({
        ...user,
        firstName: user.firstName ?? '',
        lastName: user.lastName ?? '',
        email: user.email ?? '',
        phoneNumber: user.phone || user.phoneNumber || '',
        buildingName: user.buildingName ?? '',
        profileImageUrl: user.profileImageUrl ?? '',
        role: user.role,
        imageFile: undefined, // Always clear file on user load
      });
    }
  }, [user]);

  useEffect(() => {
    if (updatingError) {
      toast.error(updatingError);
      setShowUpdateModal(false);
    }
    if (!updating && showUpdateModal && !updatingError) {
      setShowUpdateModal(false);
      toast.success('User updated successfully!');
      setTimeout(() => {
        navigate('/users');
      }, 3000);
    }
  }, [updating, updatingError]);

  useEffect(() => {
    if (deletingError) {
      toast.error(deletingError);
    }
    if (!deleting && showDeleteModal && !deletingError) {
      setShowDeleteModal(false);
      toast.success('User deleted!');
      navigate('/users');
    }
  }, [deleting, deletingError, navigate]);

  const handletoggleUpdateModal = () => {
    if (!hasChanges) return;
    setShowUpdateModal((v) => !v);
  };
  const handletoggleDeleteModal = () => setShowDeleteModal((v) => !v);

  const handleUpdateUser = async () => {
    // Determine the ID to use - prioritize URL param 'id', then user.id/user.userId
    const userIdToUpdate = id || user.id || user.userId;

    if (!userIdToUpdate) {
      toast.error('User ID not found');
      return;
    }

    await dispatch(
      updateUser({
        id: userIdToUpdate,
        update: {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber || '',
          role: 0,
          imageFile: formData.imageFile,
          buildingName: formData.buildingName,
          isActive: formData.isActive,
        },
      })
    );
  };

  const handleDeleteUser = async () => {
    const deleteId = id || user.id || user.userId || user._id;
    if (!deleteId) {
      toast.error('User ID not found');
      return;
    }
    await dispatch(deleteUser(String(deleteId)));
  };

  if (error) return <UserErrorCard error={error} />;
  if (loading) return <UserDetailSkeleton />;

  return (
    <div className='flex flex-col gap-6 p-4 md:p-6'>
      <UserDetailHeader onBack={() => navigate(-1)} hasChanges={hasChanges} />
      <UserProfileCard formData={formData} onInputChange={handleInputChange} />
      <div className='grid gap-6 lg:grid-cols-2'>
        <ContactInfoCard
          formData={formData}
          onInputChange={handleInputChange}
        />
        <LocationRoleCard
          formData={formData}
          onInputChange={handleInputChange}
        />
      </div>
      <UserDetailFooter
        onSave={handletoggleUpdateModal}
        onDelete={handletoggleDeleteModal}
        hasChanges={hasChanges}
      />
      <ConfirmModal
        open={showUpdateModal}
        title='Update User'
        description='Are you sure you want to update this user?'
        userName={`${user.firstName} ${user.lastName}`}
        confirmType='warning'
        confirmLabel='Update'
        cancelLabel='Cancel'
        onCancel={handletoggleUpdateModal}
        onConfirm={handleUpdateUser}
        changes={changes}
      />
      <ConfirmModal
        open={showDeleteModal}
        title='Delete User'
        description='Are you sure you want to delete this user?'
        userName={`${user.firstName} ${user.lastName}`}
        warning='WARNING! This action cannot be undone.'
        confirmType='danger'
        confirmLabel='Delete'
        cancelLabel='Cancel'
        onCancel={handletoggleDeleteModal}
        onConfirm={handleDeleteUser}
      />
    </div>
  );
};

export default UserDetail;

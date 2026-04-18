import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import Layout from '@/components/Layout/DashboardLayout';
import {
  validateRefreshToken,
} from '@/utils/authUtils';
import Logo from '@/assets/Logo.jpg';
import { Loader2 } from 'lucide-react';
import { clearMe } from '@/store/slices/meSlice';
import {
  clearVendorProfile,
  getVendorProfile,
} from '@/store/slices/vendorSlice';
import type { AppDispatch } from '@/store/store';

const LoadingPage = () => {
  const [isAuthorized, setIsAuthorized] = useState<boolean | null>(null);
  const navigate = useNavigate();
  const dispatch = useDispatch<AppDispatch>();

  useEffect(() => {
    const handler = () => {
      dispatch(clearMe());
      dispatch(clearVendorProfile());
      navigate('/login', { replace: true });
    };
    window.addEventListener('force-logout', handler);
    return () => window.removeEventListener('force-logout', handler);
  }, [navigate, dispatch]);

  useEffect(() => {
    const checkAuth = async () => {
      // Check session validity using HTTP-only cookie
      const isValid = await validateRefreshToken();

      if (!isValid) {
        dispatch(clearMe());
        dispatch(clearVendorProfile());
        setIsAuthorized(false);
        return;
      }
      
      // Fetch fresh data
      try {
        await Promise.all([
           // Re-fetch user data if needed, or if validateRefreshToken doesn't return user
           // Assuming validateRefreshToken handles token logic, we still need store data
           // We'll dispatch actions to populate store
           dispatch(getVendorProfile()),
           // We might need to fetch 'me' as well if it's not persisted or needs refresh
           // dispatch(fetchMe()) // Assuming such action exists or similar
        ]);
      } catch (e) {
         console.error("Failed to fetch initial data", e);
      }

      setIsAuthorized(true);
    };

    // Give time for cookie to be set/checked
    const timer = setTimeout(() => {
      checkAuth();
    }, 50);

    return () => clearTimeout(timer);
  }, [dispatch]);

  useEffect(() => {
    if (isAuthorized === false) {
      navigate('/login', { replace: true });
    }
  }, [isAuthorized, navigate]);

  if (isAuthorized === null) {
    return (
      <div className='flex flex-col justify-center items-center h-screen'>
        <img
          src={Logo}
          alt='Company Logo'
          className='w-32 h-32 object-contain'
          style={{ animation: 'bw-pulse 2.5s infinite' }}
        />
        <Loader2 className='mt-8 h-8 w-8 animate-spin text-gray-800' />
      </div>
    );
  }

  return (
    <>
      <Layout />
    </>
  );
};

export default LoadingPage;

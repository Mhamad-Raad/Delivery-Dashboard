import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import { toast } from 'sonner';
import { showValidationErrors } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import { Eye, EyeOff, Mail, Lock, Loader2 } from 'lucide-react';
import { loginUser } from '@/data/Authorization';
import { validateRefreshToken } from '@/utils/authUtils';
import { setAccessToken } from '@/store/slices/notificationsSlice';
import Logo from '@/assets/Logo.jpg';

import { setMe } from '@/store/slices/meSlice';


const Login = () => {
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isLocked, setIsLocked] = useState(false);
  const [isCheckingSession, setIsCheckingSession] = useState(true);

  useEffect(() => {
    const checkSession = async () => {
      // Try to validate session using HTTP-only cookie
      const isValid = await validateRefreshToken();
      if (isValid) {
        navigate('/');
        return;
      }
      setIsCheckingSession(false);
    };
    checkSession();
  }, [navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isLocked) return;
    setIsLoading(true);

    try {
      const response = await loginUser({ email, password });
      if (response.error || response.errors?.length > 0) {
        showValidationErrors(
          'Login Failed',
          response.errors?.length > 0 ? response.errors : response.error,
          'Email or password is incorrect'
        );
        setIsLocked(true);
      } else {
        // Store the access token for SignalR connection
        if (response.accessToken || response.token) {
          dispatch(setAccessToken(response.accessToken || response.token));
        }
        dispatch(setMe(response));
        toast.success('Login successful!');
        navigate('/');
      }
    } catch (error: any) {
      toast.error('Server is down or unreachable!');
      setIsLocked(true);
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange =
    (setter: React.Dispatch<React.SetStateAction<string>>) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setter(e.target.value);
      if (isLocked) setIsLocked(false);
    };

  if (isCheckingSession) {
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
    <div className='w-full h-[100vh] flex items-center justify-center'>
      <div className='w-[90%] max-w-[1000px] flex flex-col gap-6'>
        <Card className='overflow-hidden p-0'>
          <CardContent className='grid p-0 md:grid-cols-2'>
            <form className='p-6 md:p-8' onSubmit={handleSubmit}>
              <FieldGroup>
                <div className='flex flex-col items-center gap-2 text-center mb-4'>
                  <h1 className='text-2xl font-bold'>Welcome back!</h1>
                  <p className='text-muted-foreground text-balance'>
                    Login to your account
                  </p>
                </div>
                {/* Email Field */}
                <Field className='gap-2'>
                  <FieldLabel htmlFor='email' className='text-muted-foreground'>
                    Email
                  </FieldLabel>
                  <div className='relative'>
                    <Input
                      id='email'
                      type='email'
                      placeholder='m@example.com'
                      className='h-11 pl-10'
                      required
                      value={email}
                      onChange={handleChange(setEmail)}
                    />
                    <span className='absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground'>
                      <Mail size={20} />
                    </span>
                  </div>
                </Field>
                {/* Password Field */}
                <Field className='gap-2'>
                  <div className='flex items-center'>
                    <FieldLabel
                      htmlFor='password'
                      className='text-muted-foreground'
                    >
                      Password
                    </FieldLabel>
                    <a
                      href='#'
                      className='ml-auto text-sm underline-offset-2 hover:underline'
                    >
                      Forgot your password?
                    </a>
                  </div>
                  <div className='relative'>
                    <Input
                      id='password'
                      type={showPassword ? 'text' : 'password'}
                      className='h-11 pl-10 pr-10'
                      placeholder='******'
                      required
                      value={password}
                      onChange={handleChange(setPassword)}
                    />
                    <span className='absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground'>
                      <Lock size={20} />
                    </span>
                    <button
                      type='button'
                      tabIndex={-1}
                      onClick={() => setShowPassword((v) => !v)}
                      className='absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground'
                    >
                      {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                    </button>
                  </div>
                </Field>
                {/* Submit Button */}
                <Field className='mt-4'>
                  <Button
                    type='submit'
                    disabled={isLoading || isLocked}
                    className='w-full'
                  >
                    {isLoading ? (
                      <>
                        <Loader2 className='mr-2 h-4 w-4 animate-spin' />{' '}
                        Logging in...
                      </>
                    ) : isLocked ? (
                      'Please edit your credentials'
                    ) : (
                      'Login'
                    )}
                  </Button>
                </Field>
              </FieldGroup>
            </form>
            <div className='bg-muted relative hidden md:block'>
              <img
                src={Logo}
                alt='Image'
                className='absolute inset-0 h-full w-full object-cover dark:brightness-[0.2] dark:grayscale'
              />
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
};

export default Login;

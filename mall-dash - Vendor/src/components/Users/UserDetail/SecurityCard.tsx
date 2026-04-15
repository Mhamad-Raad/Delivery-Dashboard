import { Lock } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { useTranslation } from 'react-i18next';

interface SecurityCardProps {
  password: string;
  confirmPassword: string;
  onPasswordChange: (password: string) => void;
  onConfirmPasswordChange: (confirmPassword: string) => void;
}

const SecurityCard = ({
  password,
  confirmPassword,
  onPasswordChange,
  onConfirmPasswordChange,
}: SecurityCardProps) => {
  const passwordsMatch = !password || !confirmPassword || password === confirmPassword;
  const { t } = useTranslation();

  return (
    <Card>
      <CardHeader>
        <CardTitle className='text-lg'>{t('users.securityTitle')}</CardTitle>
        <CardDescription>{t('users.securityDescription')}</CardDescription>
      </CardHeader>
      <CardContent className='space-y-6'>
        <div className='space-y-2'>
          <Label htmlFor='password' className='text-sm font-medium flex items-center gap-2'>
            <Lock className='size-4 text-primary' />
            {t('users.newPasswordLabel')}
          </Label>
          <Input
            id='password'
            type='password'
            placeholder='••••••••'
            value={password}
            onChange={(e) => onPasswordChange(e.target.value)}
            className='h-11'
          />
        </div>
        <Separator />
        <div className='space-y-2'>
          <Label htmlFor='confirmPassword' className='text-sm font-medium flex items-center gap-2'>
            <Lock className='size-4 text-primary' />
            {t('users.confirmNewPasswordLabel')}
          </Label>
          <Input
            id='confirmPassword'
            type='password'
            placeholder='••••••••'
            value={confirmPassword}
            onChange={(e) => onConfirmPasswordChange(e.target.value)}
            className='h-11'
          />
          {!passwordsMatch && (
            <p className='text-xs text-destructive'>
              {t('users.passwordsDoNotMatch')}
            </p>
          )}
        </div>
        <p className='text-xs text-muted-foreground'>
          {t('users.securityHelp')}
        </p>
      </CardContent>
    </Card>
  );
};

export default SecurityCard;

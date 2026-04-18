import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardTitle, CardDescription } from '@/components/ui/card';

interface UserErrorCardProps {
  error: string;
}

const UserErrorCard: React.FC<UserErrorCardProps> = ({ error }) => {
  const navigate = useNavigate();
  const { t } = useTranslation();

  return (
    <div className='flex flex-col gap-6 p-4 md:p-6'>
      <Button
        variant='ghost'
        className='mb-6'
        onClick={() => navigate('/users')}
      >
        <ArrowLeft className='mr-2 h-4 w-4' />
        {t('users.backToUsers')}
      </Button>
      <Card className='p-12 text-center'>
        <CardTitle className='text-2xl mb-2'>
          {t('common.errorTitle')}
        </CardTitle>
        <CardDescription>{error}</CardDescription>
      </Card>
    </div>
  );
};

export default UserErrorCard;

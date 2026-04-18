import { Card, CardHeader, CardTitle, CardContent, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useTranslation } from 'react-i18next';

const userTypeSections = [
  {
    titleKey: 'users.userTypeStaffTitle',
    descriptionKey: 'users.userTypeStaffDescription',
    value: 'Staff',
    icon: '👔',
  },
  {
    titleKey: 'users.userTypeCustomerTitle',
    descriptionKey: 'users.userTypeCustomerDescription',
    value: 'Customer',
    icon: '👤',
  },
];

interface UserTypeSelectorProps {
  selectedType: string;
  onTypeChange: (type: string) => void;
}

export default function UserTypeSelector({ selectedType, onTypeChange }: UserTypeSelectorProps) {
  const { t } = useTranslation();

  return (
    <Card>
      <CardHeader>
        <CardTitle className='text-lg'>{t('users.userTypeTitle')}</CardTitle>
        <CardDescription>
          {t('users.userTypeDescription')}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className='grid grid-cols-1 md:grid-cols-2 gap-4'>
          {userTypeSections.map((section) => (
            <button
              key={section.value}
              onClick={() => onTypeChange(section.value)}
              className={`p-6 rounded-lg border-2 transition-all text-left hover:shadow-md ${
                selectedType === section.value
                  ? 'border-primary bg-primary/5'
                  : 'border-border hover:border-primary/50'
              }`}
            >
              <div className='flex flex-col items-center text-center gap-3'>
                <span className='text-5xl'>{section.icon}</span>
                <div className='space-y-1'>
                  <p className='font-semibold text-lg'>
                    {t(section.titleKey)}
                  </p>
                  <p className='text-xs text-muted-foreground'>
                    {t(section.descriptionKey)}
                  </p>
                </div>
                {selectedType === section.value && (
                  <Badge variant='default'>
                    {t('users.userTypeSelectedBadge')}
                  </Badge>
                )}
              </div>
            </button>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

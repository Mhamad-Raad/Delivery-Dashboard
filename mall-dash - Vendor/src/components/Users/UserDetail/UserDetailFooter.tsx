import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Save, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface UserDetailFooterProps {
  onSave: () => void;
  onDelete: () => void;
  hasChanges: boolean;
}

export default function UserDetailFooter({
  onSave,
  onDelete,
  hasChanges,
}: UserDetailFooterProps) {
  const { t } = useTranslation();

  return (
    <div className='sticky bottom-0 z-10 -mx-4 md:-mx-6 px-4 md:px-6 py-3 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 border-t shrink-0'>
      <div className='flex items-center justify-between gap-3 h-8'>
        <div className='flex items-center gap-2'>
          {hasChanges && (
            <Badge variant='secondary' className='text-xs'>
              {t('common.unsavedChanges')}
            </Badge>
          )}
        </div>
        <div className='flex items-center gap-2'>
          <Button
            variant='outline'
            size='sm'
            className='text-destructive hover:text-destructive hover:bg-destructive/10'
            onClick={onDelete}
          >
            <Trash2 className='size-4 mr-1.5' />
            {t('common.delete')}
          </Button>
          <Button size='sm' onClick={onSave} disabled={!hasChanges}>
            <Save className='size-4 mr-1.5' />
            {t('common.saveChanges')}
          </Button>
        </div>
      </div>
    </div>
  );
}

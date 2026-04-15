import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { AlertCircle, ArrowLeft, RefreshCw } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

interface ProductErrorCardProps {
  error: string;
}

export default function ProductErrorCard({ error }: ProductErrorCardProps) {
  const navigate = useNavigate();

  return (
    <div className='flex items-center justify-center min-h-[60vh] p-4 md:p-6'>
      <Card className='w-full max-w-sm'>
        <CardContent className='flex flex-col items-center justify-center py-8 text-center'>
          <div className='flex size-12 items-center justify-center rounded-full bg-destructive/10 mb-4'>
            <AlertCircle className='size-6 text-destructive' />
          </div>
          <h2 className='text-lg font-semibold mb-1'>
            Failed to load product
          </h2>
          <p className='text-sm text-muted-foreground mb-6'>{error}</p>
          <div className='flex items-center gap-2'>
            <Button variant='outline' size='sm' onClick={() => navigate(-1)}>
              <ArrowLeft className='size-4 mr-1.5' />
              Go Back
            </Button>
            <Button size='sm' onClick={() => window.location.reload()}>
              <RefreshCw className='size-4 mr-1.5' />
              Retry
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

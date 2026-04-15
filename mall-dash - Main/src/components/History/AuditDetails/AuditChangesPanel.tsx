import { FileText } from 'lucide-react';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { ScrollArea } from '@/components/ui/scroll-area';
import { formatFieldName } from './auditUtils';
import ChangeRow from './ChangeRow';

interface Change {
  field: string;
  oldValue: string;
  newValue: string;
}

interface AuditChangesPanelProps {
  changes: Change[];
  isCreated: boolean;
  isDeleted: boolean;
}

const AuditChangesPanel = ({ changes, isCreated, isDeleted }: AuditChangesPanelProps) => {
  return (
    <Card className='lg:col-span-2 flex flex-col overflow-hidden'>
      <div className='p-4 border-b bg-muted/30 flex items-center justify-between'>
        <div className='flex items-center gap-3'>
          <h3 className='font-semibold text-lg'>
            {isCreated ? 'Created Values' : isDeleted ? 'Deleted Values' : 'Data Changes'}
          </h3>
          <Badge variant='outline' className='text-sm font-normal'>
            {changes.length} field{changes.length !== 1 ? 's' : ''}
          </Badge>
        </div>
      </div>

      {changes.length > 0 ? (
        <ScrollArea className='flex-1'>
          {/* Header Row */}
          {!isCreated && !isDeleted && (
            <div className='grid grid-cols-[160px_1fr_28px_1fr] gap-3 py-3 px-5 border-b bg-muted/20 text-sm font-medium text-muted-foreground'>
              <div>Field</div>
              <div>Previous Value</div>
              <div></div>
              <div>New Value</div>
            </div>
          )}

          {/* For Created - show only new values */}
          {isCreated ? (
            <div className='divide-y'>
              {changes.map((change, index) => (
                <div
                  key={index}
                  className='flex items-start justify-between py-4 px-5 hover:bg-muted/30 transition-colors'
                >
                  <span className='text-base font-medium text-foreground'>
                    {formatFieldName(change.field)}
                  </span>
                  <code className='text-sm bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20 px-3 py-1.5 rounded max-w-[60%] break-all text-right'>
                    {change.newValue}
                  </code>
                </div>
              ))}
            </div>
          ) : isDeleted ? (
            /* For Deleted - show only old values */
            <div className='divide-y'>
              {changes.map((change, index) => (
                <div
                  key={index}
                  className='flex items-start justify-between py-4 px-5 hover:bg-muted/30 transition-colors'
                >
                  <span className='text-base font-medium text-foreground'>
                    {formatFieldName(change.field)}
                  </span>
                  <code className='text-sm bg-red-500/10 text-red-600 dark:text-red-400 border border-red-500/20 px-3 py-1.5 rounded max-w-[60%] break-all text-right line-through'>
                    {change.oldValue}
                  </code>
                </div>
              ))}
            </div>
          ) : (
            /* For Updated - show comparison */
            <div className='divide-y'>
              {changes.map((change, index) => (
                <ChangeRow
                  key={index}
                  field={change.field}
                  oldValue={change.oldValue}
                  newValue={change.newValue}
                  isNew={change.oldValue === 'null'}
                />
              ))}
            </div>
          )}
        </ScrollArea>
      ) : (
        <div className='flex-1 flex flex-col items-center justify-center text-muted-foreground p-8'>
          <div className='rounded-full bg-muted/50 p-5 mb-4'>
            <FileText className='h-10 w-10 opacity-50' />
          </div>
          <p className='font-medium text-lg'>No data changes recorded</p>
          <p className='text-base text-muted-foreground/70'>
            This action did not modify any fields
          </p>
        </div>
      )}
    </Card>
  );
};

export default AuditChangesPanel;

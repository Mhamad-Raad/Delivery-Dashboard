import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';
import RequestsFilters from '@/components/Requests/RequestsFilters';
import RequestsTable from '@/components/Requests/RequestsTable';
import { fetchRequests } from '@/store/slices/requestsSlice';
import type { AppDispatch, RootState } from '@/store/store';

const RequestsPage = () => {
  const dispatch = useDispatch<AppDispatch>();
  const [searchParams] = useSearchParams();
  const { limit } = useSelector((state: RootState) => state.requests);

  useEffect(() => {
    const page = Number(searchParams.get('page')) || 1;
    const search = searchParams.get('search') || '';
    const status = searchParams.get('status') || 'All';

    dispatch(
      fetchRequests({
        page,
        limit,
        search,
        status,
      })
    );
  }, [dispatch, searchParams, limit]);

  return (
    <section className='w-full h-full flex flex-col gap-4 overflow-hidden'>
      <div className='flex items-center justify-between'>
        <div>
          <h1 className='text-3xl font-bold tracking-tight'>Tenant Requests</h1>
          <p className='text-muted-foreground mt-2'>
            Manage maintenance and service requests from tenants.
          </p>
        </div>
      </div>
      
      <RequestsFilters />
      <div className='flex-1 min-h-0 flex flex-col'>
        <RequestsTable />
      </div>
    </section>
  );
};

export default RequestsPage;

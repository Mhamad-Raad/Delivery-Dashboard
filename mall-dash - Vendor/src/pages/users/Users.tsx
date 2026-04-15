import { useEffect } from 'react';

import { useDispatch, useSelector } from 'react-redux';
import { useSearchParams } from 'react-router-dom';

import type { AppDispatch, RootState } from '@/store/store';

import UsersFilters from '@/components/Users/UsersFilters';
import UsersTable from '@/components/Users/UsersTable';
import EmptyState from '@/components/Users/EmptyState';

import { fetchUsers } from '@/store/slices/usersSlice';

const Users = () => {
  const [searchParams] = useSearchParams();
  const dispatch = useDispatch<AppDispatch>();
  const {
    users,
    lusers: loading,
    eusers: error,
  } = useSelector((state: RootState) => state.users);

  console.log(users);

  const limit = parseInt(searchParams.get('limit') || '40', 10);
  const page = parseInt(searchParams.get('page') || '1', 10);

  const search = searchParams.get('searchTerm') || '';
  const isActiveParam = searchParams.get('isActive');

  useEffect(() => {
    const params: Record<string, any> = {
      limit,
      page,
      searchTerm: search,
    };

    if (isActiveParam === 'true') {
      params.isActive = true;
    } else if (isActiveParam === 'false') {
      params.isActive = false;
    }

    if (limit && page) {
      dispatch(fetchUsers(params));
    }
  }, [dispatch, limit, page, search, isActiveParam]);

  const hasNoUsers = !loading && users.length === 0 && !error;
  return (
    <section className='w-full h-full flex flex-col gap-6 overflow-hidden'>
      {/* Filters Section */}
      <UsersFilters />

      {/* Users Table OR Empty State */}
      <div className='flex-1 min-h-0'>
        {hasNoUsers ? <EmptyState /> : <UsersTable />}
      </div>
    </section>
  );
};

export default Users;

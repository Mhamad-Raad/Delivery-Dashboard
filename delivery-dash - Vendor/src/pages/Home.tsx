import { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import type { RootState } from '@/store/store';
import CardsHome from '@/components/Home/CardsHome';
import OrdersProfitChart from '@/components/Home/OrdersProfitChart';
import { fetchDashboardStats, type DashboardStats } from '@/data/Dashboard';

const Home = () => {
  const { t } = useTranslation();
  const { profile } = useSelector((state: RootState) => state.vendor);
  const [dashboardData, setDashboardData] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadDashboardData = async () => {
      setLoading(true);
      const result = await fetchDashboardStats();
      if ('error' in result) {
        console.error('Failed to fetch dashboard stats:', result.error);
      } else {
        setDashboardData(result);
      }
      setLoading(false);
    };

    loadDashboardData();
  }, []);

  // Cards info based on API data
  const cardsInfo = dashboardData ? [
    {
      title: t('home.cardTotalOrdersTitle'),
      value: dashboardData.totalOrders,
      badge: { 
        text: `${dashboardData.todayOrders} ${t('home.cardTodayOrdersBadge')}`, 
        trendingUp: dashboardData.todayOrders > 0 ? true : null 
      },
      footer: t('home.cardTotalOrdersFooter'),
    },
    {
      title: t('home.cardTotalProductsTitle'),
      value: dashboardData.totalProducts,
      badge: { text: t('home.cardTotalProductsBadge'), trendingUp: null },
      footer: t('home.cardTotalProductsFooter'),
    },
  ] : [];

  if (loading) {
    return (
      <div className='flex flex-col gap-4 w-full p-2'>
        <div className='flex items-center justify-center h-64'>
          <p className='text-muted-foreground'>{t('common.loading')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className='flex flex-col gap-4 w-full p-2'>
      {/* Welcome Message */}
      {profile && (
        <div className='flex flex-col gap-1 mb-4'>
          <h1 className='text-3xl md:text-4xl font-bold'>
            {t('home.welcomeTitle', { name: profile.name })}
          </h1>
          <p className='text-muted-foreground text-lg'>
            {t('home.welcomeSubtitle')}
          </p>
        </div>
      )}

      {/* Stats Cards */}
      {dashboardData && <CardsHome cards={cardsInfo} />}

      {/* Orders & Profit Chart */}
      {dashboardData && dashboardData.dailyStats.length > 0 && (
        <OrdersProfitChart data={dashboardData.dailyStats} />
      )}
    </div>
  );
};

export default Home;

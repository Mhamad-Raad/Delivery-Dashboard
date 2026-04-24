import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import LoadingPage from './pages/LoadingPage';

import Home from '@/pages/Home';
import Login from '@/pages/Login';
import ResetPassword from '@/pages/ResetPassword';
import Users from './pages/users/Users';
import CreateUser from './pages/users/CreateUser';
import UserDetail from './pages/users/UserDetail';
import Vendors from './pages/vendors/Vendors';
import VendorDetail from './pages/vendors/VendorDetail';
import CreateVendor from './pages/vendors/CreateVendor';
import VendorCategories from './pages/vendor-categories/VendorCategories';
import Profile from './pages/Profile';
import Reports from './pages/Reports';
import Notifications from './pages/notifications/Notifications';
import CreateNotification from './pages/notifications/CreateNotification';
import Themes from './pages/settings/Themes';
import Products from './pages/products/Products';
import CreateProduct from './pages/products/CreateProduct';
import ProductDetail from './pages/products/ProductDetail';
import HistoryPage from './pages/HistoryPage';
import AuditDetailsPage from './pages/AuditDetailsPage';
import Orders from './pages/orders/Orders';
import SupportTickets from './pages/support-tickets/SupportTickets';
import SupportTicketDetail from '@/pages/support-tickets/SupportTicketDetail';

import NotFound from './pages/NotFound';
import ErrorPage from './pages/ErrorPage';

const router = createBrowserRouter([
  {
    path: '/login',
    element: <Login />,
  },
  {
    path: '/reset-password',
    element: <ResetPassword />,
  },
  {
    element: <LoadingPage />,
    errorElement: <ErrorPage />,
    children: [
      {
        path: '/',
        element: <Home />,
      },
      {
        path: '/users',
        element: <Users />,
      },
      {
        path: '/users/create',
        element: <CreateUser />,
      },
      {
        path: '/users/:id',
        element: <UserDetail />,
      },
      {
        path: '/vendors',
        element: <Vendors />,
      },
      {
        path: '/vendors/create',
        element: <CreateVendor />,
      },
      {
        path: '/vendors/:id',
        element: <VendorDetail />,
      },
      {
        path: '/vendor-categories',
        element: <VendorCategories />,
      },
      {
        path: '/profile',
        element: <Profile />,
      },
      {
        path: '/reports',
        element: <Reports />,
      },
      {
        path: '/notifications',
        element: <Notifications />,
      },
      {
        path: '/notifications/new',
        element: <CreateNotification />,
      },
      {
        path: '/history',
        element: <HistoryPage />,
      },
      {
        path: '/history/:id',
        element: <AuditDetailsPage />,
      },
      {
        path: '/orders',
        element: <Orders />,
      },
      {
        path: '/orders/:id',
        element: <Orders />,
      },
      {
        path: '/support-tickets',
        element: <SupportTickets />,
      },
      {
        path: '/support-tickets/:id',
        element: <SupportTicketDetail />,
      },
      {
        path: '/settings/themes',
        element: <Themes />,
      },
      {
        path: '/products',
        element: <Products />,
      },
      {
        path: '/products/create',
        element: <CreateProduct />,
      },
      {
        path: '/products/:id',
        element: <ProductDetail />,
      },
      {
        path: '*',
        element: <NotFound />,
      },
    ],
  },
]);

function App() {
  return <RouterProvider router={router} />;
}

export default App;

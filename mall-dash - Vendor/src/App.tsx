import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import LoadingPage from './pages/LoadingPage';

import Home from '@/pages/Home';
import Login from '@/pages/Login';
import Users from './pages/users/Users';
import CreateUser from './pages/users/CreateUser';
import UserDetail from './pages/users/UserDetail';
import Products from './pages/products/Products';
import CreateProduct from './pages/products/CreateProduct';
import ProductDetail from './pages/products/ProductDetail';

import Settings from './pages/Settings';
import Themes from './pages/settings/Themes';
import Profile from './pages/Profile';
import VendorProfile from './pages/VendorProfile';
import Orders from './pages/orders/Orders';
import OrderDetail from './pages/orders/OrderDetail';

import NotFound from './pages/NotFound';
import ErrorPage from './pages/ErrorPage';

const router = createBrowserRouter([
  {
    path: '/login',
    element: <Login />,
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
        path: '/orders',
        element: <Orders />,
      },
      {
        path: '/orders/:id',
        element: <OrderDetail />,
      },
      {
        path: '/settings',
        element: <Settings />,
      },
      {
        path: '/settings/themes',
        element: <Themes />,
      },
      {
        path: '/profile',
        element: <Profile />,
      },
      {
        path: '/vendor-profile',
        element: <VendorProfile />,
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

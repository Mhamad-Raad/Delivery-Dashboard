import axios, { AxiosError } from 'axios';

import type { AxiosInstance } from 'axios';

const API_URL = import.meta.env.VITE_API_URL;

let isRefreshing = false;
let failedQueue: {
  resolve: () => void;
  reject: (error: any) => void;
}[] = [];

const processQueue = (error: any) => {
  failedQueue.forEach((prom) => {
    if (error) prom.reject(error);
    else prom.resolve();
  });
  failedQueue = [];
};

export const axiosInstance: AxiosInstance = axios.create({
  baseURL: API_URL,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
});

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest: any = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        try {
          await new Promise<void>((resolve, reject) => {
            failedQueue.push({ resolve, reject });
          });

          return axiosInstance(originalRequest);
        } catch (err) {
          return Promise.reject(err);
        }
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        await axios.post(
          `${API_URL}/Account/NewRefreshToken`,
          {},
          { withCredentials: true }
        );

        processQueue(null);

        return axiosInstance(originalRequest);
      } catch (err: any) {
        processQueue(err);
        window.dispatchEvent(new Event('force-logout'));
        throw new Error(err?.message);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

// api/client.ts
import axios, { AxiosError, type AxiosInstance, type AxiosResponse } from 'axios'
import type { ApiError } from '../types/Auth'

const API_BASE_URL = 'https://localhost:7070'
const FRONTEND_BASE_URL = 'https://localhost:5173'

// Axios instance (cookie-based auth ONLY)
export const api: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Helper: detect OAuth callback phase
const isOAuthCallback = () =>
  window.location.search.includes('code=') || 
  window.location.search.includes('state=') ||
  window.location.pathname.includes('ExternalLoginCallback') || window.location.pathname.includes('google-callback')
 || window.location.pathname.includes('google-signin')

// Response interceptor
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError<ApiError>) => {
    const status = error.response?.status;
    const path = window.location.pathname;
    const requestUrl = error.config?.url || '';

    // 🚨 DO NOT interfere during OAuth callback
    if (isOAuthCallback()) {
      return Promise.reject(error);
    }

    // Handle 401 ONLY outside login + OAuth flow
    if (
      status === 401 &&
      !path.includes('/login') &&
      !requestUrl.includes('/api/auth/me') // ⬅️ ADD THIS LINE!
    ) {
      const returnUrl = window.location.href;
      localStorage.setItem('returnUrl', returnUrl);
      window.location.href = `${FRONTEND_BASE_URL}/login?returnUrl=${encodeURIComponent(returnUrl)}`;
    }

    return Promise.reject(error);
  }
);
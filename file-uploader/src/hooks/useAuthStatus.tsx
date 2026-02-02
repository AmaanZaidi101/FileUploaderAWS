// hooks/useAuthStatus.ts
import { useEffect, useState } from 'react';
import { api } from '../api/client';
import type { User } from '../types/Auth';

export const useAuthStatus = (): {
  user: User | null;
  loading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
} => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const checkStatus = async (): Promise<void> => {
    try {
      setLoading(true);
      const response = await api.get<User>('/api/auth/me');
      setUser(response.data);
      setError(null);
    } catch (err) {
      const axiosError = err as { response?: { status: number } };
      setUser(null);
      
      if (axiosError.response?.status !== 401) {
        setError('Unable to verify authentication');
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    checkStatus();
  }, []);

  return {
    user,
    loading,
    error,
    refresh: checkStatus,
  };
};
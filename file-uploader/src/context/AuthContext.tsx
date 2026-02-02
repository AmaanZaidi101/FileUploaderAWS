import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from 'react'
import { api } from '../api/client'
import type { AuthContextType, User } from '../types/Auth'

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = (): AuthContextType => {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}

const FRONTEND_BASE_URL = 'https://localhost:5173'
const BACKEND_BASE_URL = 'https://localhost:7070'

// Update the isOAuthCallback function in AuthContext
const isOAuthCallback = () =>
  window.location.search.includes('code=') ||
  window.location.search.includes('state=') ||
  window.location.pathname.includes('ExternalLoginCallback') || window.location.pathname.includes('google-callback')
  || window.location.pathname.includes('google-signin')

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // ---------------- AUTH CHECK ----------------
  const checkAuth = useCallback(async () => {
  if (isOAuthCallback()) {
    // Don't check auth during OAuth callback
    setLoading(false)
    return
  }

  try {
    const res = await api.get<User>('/api/auth/me')
    setUser(res.data)
    localStorage.setItem('user', JSON.stringify(res.data))
    setError(null)
  } catch (err: any) {
    // Check if it's a network error vs 401
    if (err.code === 'ERR_NETWORK') {
      console.warn('Network error during auth check')
      // Keep cached user if available
      return
    }
    
    if (err?.response?.status === 401) {
      // This is expected for anonymous users
      setUser(null)
      setError(null)
      // DON'T redirect here - this is a valid state
    } else {
      console.error('Auth check failed:', err)
      setError('Failed to check authentication')
      // Only set error, don't redirect
    }
  } finally {
    setLoading(false)
  }
}, [])


  // ---------------- INITIAL BOOTSTRAP ----------------
  useEffect(() => {
    // Restore cached user for instant UI
    const cached = localStorage.getItem('user')
    if (cached) {
      try {
        setUser(JSON.parse(cached))
      } catch {
        localStorage.removeItem('user')
      }
    }

    // Delay avoids cookie commit race after OAuth
    const t = setTimeout(() => {
      checkAuth()
    }, 500)

    return () => clearTimeout(t)
  }, [checkAuth])

  // ---------------- MULTI-TAB SYNC ----------------
  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === 'user') {
        setUser(e.newValue ? JSON.parse(e.newValue) : null)
      }
    }
    window.addEventListener('storage', onStorage)
    return () => window.removeEventListener('storage', onStorage)
  }, [])

  // ---------------- ACTIONS ----------------
  const login = () => {
    const returnUrl =
      localStorage.getItem('returnUrl') ||
      `${FRONTEND_BASE_URL}/home`;
    console.log(returnUrl);
    window.location.href =
      `${BACKEND_BASE_URL}/api/auth/google-login?returnUrl=${encodeURIComponent(
        returnUrl
      )}`
  }

  const logout = async () => {
    try {
      await api.get('/api/auth/logout')
    } finally {
      setUser(null)
      localStorage.removeItem('user')
      localStorage.removeItem('returnUrl')

      window.location.href = `${FRONTEND_BASE_URL}/`
    }
  }

  const refreshAuth = async () => {
    await checkAuth()
  }

  const value: AuthContextType = {
    user,
    loading,
    error,
    login,
    logout,
    refreshAuth,
    isAuthenticated: !!user,
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}

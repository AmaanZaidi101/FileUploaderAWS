export interface User {
  id: string;
  email: string;
  userName: string;
  firstName?: string;
  lastName?: string;
  profilePictureUrl?: string;
  roles: string[];
  createdAt?: Date;
}

export interface AuthContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  login: () => void;
  logout: () => Promise<void>;
  refreshAuth: () => Promise<void>;
  isAuthenticated: boolean;
}

export interface ProtectedRouteProps {
  children: React.ReactNode;
  requireRoles?: string[];
}

export interface LoginResponse {
  user: User;
  accessToken?: string;
}

export interface ApiError {
  message: string;
  statusCode: number;
  errors?: Record<string, string[]>;
}
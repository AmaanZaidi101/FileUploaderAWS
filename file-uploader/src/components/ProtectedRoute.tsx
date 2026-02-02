import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface Props {
  children: React.ReactNode;
  requireRoles?: string[];
}

export const ProtectedRoute: React.FC<Props> = ({
  children,
  requireRoles = [],
}) => {
  const { isAuthenticated, loading, user } = useAuth();
  const location = useLocation();

  // ⛔ WAIT until auth is resolved
  if (loading) {
    return null; // or spinner
  }

  // ❌ Not logged in → login
  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  // ❌ Role mismatch
//   if (
//     requireRoles.length > 0 &&
//     !requireRoles.some(r => user?.roles?.includes(r))
//   ) {
//     return <Navigate to="/unauthorized" replace />;
//   }

  return <>{children}</>;
};

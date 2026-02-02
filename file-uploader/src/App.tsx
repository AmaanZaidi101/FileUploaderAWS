import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import Layout from './components/Layout';

import LoginPage from './pages/LoginPage';
import UnauthorisedPage from './pages/UnauthorisedPage';
import HomePage from './pages/HomePage';
import AdminPage from './pages/AdminPage';
import Uploader from './pages/Uploader';

const App: React.FC = () => {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          {/* Routes WITH navbar */}
          <Route element={<Layout />}>
            <Route path="/" element={<HomePage />} />

            <Route
              path="/dashboard"
              element={
                <ProtectedRoute>
                  <Uploader />
                </ProtectedRoute>
              }
            />

            <Route
              path="/admin"
              element={
                <ProtectedRoute requireRoles={['Admin']}>
                  <AdminPage />
                </ProtectedRoute>
              }
            />
          </Route>

          {/* Routes WITHOUT navbar */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/unauthorized" element={<UnauthorisedPage />} />

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </Router>
  );
};

export default App;

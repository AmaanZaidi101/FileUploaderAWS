// pages/Dashboard.tsx
import React from 'react';
import { useAuth } from '../context/AuthContext';

const Dashboard: React.FC = () => {
  const { user, logout, refreshAuth } = useAuth();

  if (!user) {
    return null; // Will be redirected by ProtectedRoute
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100">
      <nav className="bg-white shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-4">
              <h1 className="text-xl font-bold text-gray-800">Dashboard</h1>
              <span className="bg-blue-100 text-blue-800 text-xs font-medium px-2.5 py-0.5 rounded">
                {user.roles.join(', ')}
              </span>
            </div>
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-3">
                {user.profilePictureUrl && (
                  <img
                    src={user.profilePictureUrl}
                    alt="Profile"
                    className="h-8 w-8 rounded-full ring-2 ring-gray-300"
                  />
                )}
                <div className="text-right">
                  <p className="text-sm font-medium text-gray-700">
                    {user.firstName && user.lastName 
                      ? `${user.firstName} ${user.lastName}`
                      : user.email}
                  </p>
                  <p className="text-xs text-gray-500">{user.email}</p>
                </div>
              </div>
              <div className="flex space-x-2">
                <button
                  onClick={refreshAuth}
                  className="px-3 py-1.5 text-sm bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
                >
                  Refresh
                </button>
                <button
                  onClick={logout}
                  className="px-4 py-1.5 text-sm bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors shadow-sm"
                >
                  Logout
                </button>
              </div>
            </div>
          </div>
        </div>
      </nav>
      
      <main className="max-w-7xl mx-auto py-8 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
            <h2 className="text-2xl font-bold text-gray-800 mb-6">
              Welcome back, {user.firstName || user.userName}!
            </h2>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
              <div className="bg-gradient-to-r from-blue-50 to-blue-100 p-5 rounded-xl">
                <h3 className="font-semibold text-blue-800 mb-2">Account Information</h3>
                <div className="space-y-2">
                  <p className="text-sm">
                    <span className="font-medium text-gray-600">Email:</span>{' '}
                    {user.email}
                  </p>
                  <p className="text-sm">
                    <span className="font-medium text-gray-600">User ID:</span>{' '}
                    <code className="text-xs bg-gray-100 px-1.5 py-0.5 rounded">
                      {user.id}
                    </code>
                  </p>
                  <p className="text-sm">
                    <span className="font-medium text-gray-600">Roles:</span>{' '}
                    {user.roles.map(role => (
                      <span key={role} className="ml-1 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                        {role}
                      </span>
                    ))}
                  </p>
                </div>
              </div>
              
              <div className="bg-gradient-to-r from-green-50 to-green-100 p-5 rounded-xl">
                <h3 className="font-semibold text-green-800 mb-2">Session Status</h3>
                <div className="space-y-2">
                  <p className="text-sm">
                    <span className="font-medium text-gray-600">Status:</span>{' '}
                    <span className="inline-flex items-center">
                      <span className="h-2 w-2 rounded-full bg-green-500 mr-1.5"></span>
                      Authenticated
                    </span>
                  </p>
                  {user.createdAt && (
                    <p className="text-sm">
                      <span className="font-medium text-gray-600">Member since:</span>{' '}
                      {new Date(user.createdAt).toLocaleDateString()}
                    </p>
                  )}
                </div>
              </div>
            </div>
            
            <div className="border-t border-gray-200 pt-6">
              <h3 className="text-lg font-semibold text-gray-700 mb-4">Raw User Data</h3>
              <div className="bg-gray-900 text-gray-100 p-4 rounded-lg overflow-auto">
                <pre className="text-sm">
                  {JSON.stringify(user, null, 2)}
                </pre>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;
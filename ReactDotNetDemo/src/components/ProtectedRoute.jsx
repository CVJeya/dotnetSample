import { Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { selectIsAuthenticated } from '../features/auth/authSlice';

export default function ProtectedRoute({ children }) {
  console.log('[ProtectedRoute] Component rendered');
  const isAuthenticated = useSelector(selectIsAuthenticated);
  console.log('[ProtectedRoute] isAuthenticated:', isAuthenticated);

  if (!isAuthenticated) {
    console.log('[ProtectedRoute] ✗ User NOT authenticated - redirecting to /login');
    return <Navigate to="/login" replace />;
  }

  console.log('[ProtectedRoute] ✓ User authenticated - allowing access');
  return children;
}

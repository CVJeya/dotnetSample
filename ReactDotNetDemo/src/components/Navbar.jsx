import { Link, useLocation } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { selectIsAuthenticated, selectAuthUser, logout } from '../features/auth/authSlice';

export default function Navbar() {
  const location = useLocation();
  const dispatch = useDispatch();
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const user = useSelector(selectAuthUser);

  const linkClass = (path) =>
    `nav-link${location.pathname === path ? ' active' : ''}`;

  const handleLogout = () => {
    dispatch(logout());
  };

  return (
    <nav className="navbar">
      <Link to="/" className="navbar-brand">
        React &middot; .NET Demo
      </Link>
      <div className="navbar-links">
        <Link to="/users" className={linkClass('/users')}>Users</Link>
        <Link to="/products" className={linkClass('/products')}>Products</Link>
        {isAuthenticated ? (
          <>
            <span className="nav-user">{user?.name}</span>
            <Link to="/" onClick={handleLogout} className="nav-link">Logout</Link>
          </>
        ) : (
          <>
            <Link to="/login" className={linkClass('/login')}>Login</Link>
            <Link to="/register" className={linkClass('/register')}>Register</Link>
          </>
        )}
      </div>
    </nav>
  );
}

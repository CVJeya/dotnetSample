import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { login, clearAuthError, selectAuth } from '../features/auth/authSlice';

export default function Login() {
  console.log('[Login] Component rendered');
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { status, error } = useSelector(selectAuth);

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    console.log('[Login] handleSubmit() called - email:', email);
    dispatch(clearAuthError());
    console.log('[Login] Dispatching login thunk...');
    
    const result = await dispatch(login({ email, password }));
    console.log('[Login] login() result status:', result.meta.requestStatus);
    
    if (result.meta.requestStatus === 'fulfilled') {
      console.log('[Login] ✓ Login succeeded - navigating to /products');
      navigate('/products');
    } else {
      console.log('[Login] ✗ Login failed');
    }
  };

  return (
    <div className="auth-container">
      <h1>Login</h1>

      {error && <div className="alert alert-error">{error}</div>}

      <form onSubmit={handleSubmit} className="auth-form">
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder="Enter email" />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required placeholder="Enter password" />
        </div>
        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={status === 'loading'}>
            {status === 'loading' ? 'Logging in...' : 'Login'}
          </button>
        </div>
        <p className="auth-link">Don't have an account? <Link to="/register">Register</Link></p>
      </form>
    </div>
  );
}

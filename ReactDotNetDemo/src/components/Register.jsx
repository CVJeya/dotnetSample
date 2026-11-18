import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link, useNavigate } from 'react-router-dom';
import { register, clearAuthError, selectAuth } from '../features/auth/authSlice';

export default function Register() {
  console.log('[Register] Component rendered');
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { status, error } = useSelector(selectAuth);

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    console.log('[Register] handleSubmit() called - name:', name, 'email:', email);
    dispatch(clearAuthError());
    console.log('[Register] Dispatching register thunk...');
    
    const result = await dispatch(register({ name, email, password }));
    console.log('[Register] register() result status:', result.meta.requestStatus);
    
    if (result.meta.requestStatus === 'fulfilled') {
      console.log('[Register] ✓ Registration succeeded - navigating to /products');
      navigate('/products');
    } else {
      console.log('[Register] ✗ Registration failed');
    }
  };

  return (
    <div className="auth-container">
      <h1>Register</h1>

      {error && <div className="alert alert-error">{error}</div>}

      <form onSubmit={handleSubmit} className="auth-form">
        <div className="form-group">
          <label htmlFor="name">Name</label>
          <input id="name" value={name} onChange={(e) => setName(e.target.value)} required placeholder="Enter name" />
        </div>
        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required placeholder="Enter email" />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password</label>
          <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={6} placeholder="Min 6 characters" />
        </div>
        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={status === 'loading'}>
            {status === 'loading' ? 'Registering...' : 'Register'}
          </button>
        </div>
        <p className="auth-link">Already have an account? <Link to="/login">Login</Link></p>
      </form>
    </div>
  );
}

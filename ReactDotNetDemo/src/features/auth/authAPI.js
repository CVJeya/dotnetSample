const BASE_URL = '/api/auth';

function getAuthHeaders() {
  const token = localStorage.getItem('jwt_token');
  const headers = { 'Content-Type': 'application/json' };
  console.log('[authAPI] getAuthHeaders() called → token exists:', !!token);
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
    console.log('[authAPI] Authorization header added');
  }
  return headers;
}

export const authAPI = {
  async login(email, password) {
    console.log('[authAPI] login() called with email:', email);
    try {
      const response = await fetch(`${BASE_URL}/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      });
      
      if (!response.ok) {
        console.log('[authAPI] ✗ login() failed - status:', response.status);
        const err = await response.json().catch(() => ({ message: 'Login failed' }));
        throw new Error(err.message || 'Login failed');
      }
      
      const data = await response.json();
      console.log('[authAPI] ✓ login() succeeded - token length:', data.token.length, 'user:', data.user.name);
      return data;
    } catch (error) {
      console.log('[authAPI] ✗ login() error:', error.message);
      throw error;
    }
  },

  async register(name, email, password) {
    console.log('[authAPI] register() called with name:', name, 'email:', email);
    try {
      const response = await fetch(`${BASE_URL}/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, email, password }),
      });
      
      if (!response.ok) {
        console.log('[authAPI] ✗ register() failed - status:', response.status);
        const err = await response.json().catch(() => ({ message: 'Registration failed' }));
        throw new Error(err.message || 'Registration failed');
      }
      
      const data = await response.json();
      console.log('[authAPI] ✓ register() succeeded - token length:', data.token.length, 'user:', data.user.name);
      return data;
    } catch (error) {
      console.log('[authAPI] ✗ register() error:', error.message);
      throw error;
    }
  },
};

export { getAuthHeaders };

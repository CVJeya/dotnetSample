import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { authAPI } from './authAPI';

export const login = createAsyncThunk('auth/login', async ({ email, password }) => {
  console.log('[authSlice] login thunk called with email:', email);
  const data = await authAPI.login(email, password);
  console.log('[authSlice] login thunk response received');
  return data;
});

export const register = createAsyncThunk('auth/register', async ({ name, email, password }) => {
  console.log('[authSlice] register thunk called with name:', name, 'email:', email);
  const data = await authAPI.register(name, email, password);
  console.log('[authSlice] register thunk response received');
  return data;
});

function loadInitialState() {
  console.log('[authSlice] loadInitialState() called');
  const token = localStorage.getItem('jwt_token');
  const raw = localStorage.getItem('auth_user');
  if (token && raw) {
    console.log('[authSlice] ✓ Existing session found in localStorage');
    return {
      isAuthenticated: true,
      user: JSON.parse(raw),
      token,
      status: 'idle',
      error: null,
    };
  }
  console.log('[authSlice] ✗ No existing session - initializing empty state');
  return {
    isAuthenticated: false,
    user: null,
    token: null,
    status: 'idle',
    error: null,
  };
}

const authSlice = createSlice({
  name: 'auth',
  initialState: loadInitialState(),
  reducers: {
    logout(state) {
      state.isAuthenticated = false;
      state.user = null;
      state.token = null;
      state.status = 'idle';
      state.error = null;
      localStorage.removeItem('jwt_token');
      localStorage.removeItem('auth_user');
    },
    clearAuthError(state) {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        console.log('[authSlice] login.pending - setting status to loading');
        state.status = 'loading';
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        console.log('[authSlice] login.fulfilled - updating state with user and token');
        state.status = 'succeeded';
        state.isAuthenticated = true;
        state.user = action.payload.user;
        state.token = action.payload.token;
        localStorage.setItem('jwt_token', action.payload.token);
        localStorage.setItem('auth_user', JSON.stringify(action.payload.user));
        console.log('[authSlice] ✓ login.fulfilled - user authenticated, state updated');
      })
      .addCase(login.rejected, (state, action) => {
        console.log('[authSlice] login.rejected - error:', action.error.message);
        state.status = 'failed';
        state.error = action.error.message;
      })
      .addCase(register.pending, (state) => {
        console.log('[authSlice] register.pending - setting status to loading');
        state.status = 'loading';
        state.error = null;
      })
      .addCase(register.fulfilled, (state, action) => {
        console.log('[authSlice] register.fulfilled - updating state with user and token');
        state.status = 'succeeded';
        state.isAuthenticated = true;
        state.user = action.payload.user;
        state.token = action.payload.token;
        localStorage.setItem('jwt_token', action.payload.token);
        localStorage.setItem('auth_user', JSON.stringify(action.payload.user));
        console.log('[authSlice] ✓ register.fulfilled - user registered and authenticated, state updated');
      })
      .addCase(register.rejected, (state, action) => {
        console.log('[authSlice] register.rejected - error:', action.error.message);
        state.status = 'failed';
        state.error = action.error.message;
      });
  },
});

export const { logout, clearAuthError } = authSlice.actions;

export const selectAuth = (state) => state.auth;
export const selectIsAuthenticated = (state) => state.auth.isAuthenticated;
export const selectAuthUser = (state) => state.auth.user;

export default authSlice.reducer;

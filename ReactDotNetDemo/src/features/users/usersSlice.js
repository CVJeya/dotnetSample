import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { login, logout, register } from '../auth/authSlice';
import { usersAPI } from './usersAPI';

export const fetchUsers = createAsyncThunk('users/fetchUsers', async () => {
  const data = await usersAPI.getAll();
  return data;
});

export const fetchCurrentUser = createAsyncThunk('users/fetchCurrentUser', async (_userId) => {
  const data = await usersAPI.getCurrentUser();
  return data;
});

export const fetchUserById = createAsyncThunk('users/fetchUserById', async (id) => {
  const data = await usersAPI.getById(id);
  return data;
});

export const createUser = createAsyncThunk('users/createUser', async (user) => {
  const data = await usersAPI.create(user);
  return data;
});

export const updateUser = createAsyncThunk('users/updateUser', async ({ id, user }) => {
  await usersAPI.update(id, user);
  return { id, ...user };
});

export const deleteUser = createAsyncThunk('users/deleteUser', async (id) => {
  await usersAPI.delete(id);
  return id;
});

const initialState = {
  items: [],
  selectedUser: null,
  currentUser: null,
  currentUserStatus: 'idle',
  currentUserError: null,
  currentUserOwnerId: null,
  currentUserRequestId: null,
  status: 'idle', // 'idle' | 'loading' | 'succeeded' | 'failed'
  error: null,
};

const usersSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    clearSelectedUser(state) {
      state.selectedUser = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.fulfilled, (state, action) => {
        state.currentUser = null;
        state.currentUserStatus = 'idle';
        state.currentUserError = null;
        state.currentUserOwnerId = action.payload.user.id;
        state.currentUserRequestId = null;
      })
      .addCase(register.fulfilled, (state, action) => {
        state.currentUser = null;
        state.currentUserStatus = 'idle';
        state.currentUserError = null;
        state.currentUserOwnerId = action.payload.user.id;
        state.currentUserRequestId = null;
      })
      .addCase(logout, (state) => {
        state.currentUser = null;
        state.currentUserStatus = 'idle';
        state.currentUserError = null;
        state.currentUserOwnerId = null;
        state.currentUserRequestId = null;
      })
      .addCase(fetchCurrentUser.pending, (state, action) => {
        if (state.currentUserOwnerId === null) {
          state.currentUserOwnerId = action.meta.arg;
        }
        if (state.currentUserOwnerId !== action.meta.arg) return;
        state.currentUserRequestId = action.meta.requestId;
        state.currentUserStatus = 'loading';
        state.currentUserError = null;
      })
      .addCase(fetchCurrentUser.fulfilled, (state, action) => {
        if (
          action.meta.arg !== state.currentUserOwnerId ||
          action.meta.requestId !== state.currentUserRequestId
        ) return;
        state.currentUserStatus = 'succeeded';
        state.currentUser = action.payload;
      })
      .addCase(fetchCurrentUser.rejected, (state, action) => {
        if (
          action.meta.arg !== state.currentUserOwnerId ||
          action.meta.requestId !== state.currentUserRequestId
        ) return;
        state.currentUserStatus = 'failed';
        state.currentUserError = action.error.message;
      })
      // Fetch all users
      .addCase(fetchUsers.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(fetchUsers.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
      })
      .addCase(fetchUsers.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message;
      })
      // Fetch single user
      .addCase(fetchUserById.fulfilled, (state, action) => {
        state.selectedUser = action.payload;
      })
      // Create user
      .addCase(createUser.fulfilled, (state, action) => {
        state.items.push(action.payload);
      })
      // Update user
      .addCase(updateUser.fulfilled, (state, action) => {
        const index = state.items.findIndex((u) => u.id === action.payload.id);
        if (index !== -1) {
          state.items[index] = action.payload;
        }
      })
      // Delete user
      .addCase(deleteUser.fulfilled, (state, action) => {
        state.items = state.items.filter((u) => u.id !== action.payload);
      });
  },
});

export const { clearSelectedUser } = usersSlice.actions;

export const selectAllUsers = (state) => state.users.items;
export const selectUserById = (state) => state.users.selectedUser;
export const selectUsersStatus = (state) => state.users.status;
export const selectUsersError = (state) => state.users.error;
export const selectCurrentUser = (state) => state.users.currentUser;
export const selectCurrentUserStatus = (state) => state.users.currentUserStatus;
export const selectCurrentUserError = (state) => state.users.currentUserError;

export default usersSlice.reducer;

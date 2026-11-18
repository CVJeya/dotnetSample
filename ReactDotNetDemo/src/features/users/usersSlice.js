import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { usersAPI } from './usersAPI';

export const fetchUsers = createAsyncThunk('users/fetchUsers', async () => {
  const data = await usersAPI.getAll();
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

export default usersSlice.reducer;

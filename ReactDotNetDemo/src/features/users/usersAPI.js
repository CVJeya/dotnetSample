import { getAuthHeaders } from '../auth/authAPI';

const BASE_URL = '/api/users';

export const usersAPI = {
  async getCurrentUser() {
    const response = await fetch(`${BASE_URL}/me`, { headers: getAuthHeaders() });
    if (!response.ok) throw new Error('Failed to fetch current user');
    return response.json();
  },

  async getAll() {
    const response = await fetch(BASE_URL, { headers: getAuthHeaders() });
    if (!response.ok) throw new Error('Failed to fetch users');
    return response.json();
  },

  async getById(id) {
    const response = await fetch(`${BASE_URL}/${id}`, { headers: getAuthHeaders() });
    if (!response.ok) throw new Error('Failed to fetch user');
    return response.json();
  },

  async create(user) {
    const response = await fetch(BASE_URL, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(user),
    });
    if (!response.ok) throw new Error('Failed to create user');
    return response.json();
  },

  async update(id, user) {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(user),
    });
    if (!response.ok) throw new Error('Failed to update user');
  },

  async delete(id) {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete user');
  },
};
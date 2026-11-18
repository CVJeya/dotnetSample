import { getAuthHeaders } from '../auth/authAPI';

const BASE_URL = '/api/products';

export const productsAPI = {
  async getAll() {
    const response = await fetch(BASE_URL, { headers: getAuthHeaders() });
    if (!response.ok) throw new Error('Failed to fetch products');
    return response.json();
  },

  async getById(id) {
    const response = await fetch(`${BASE_URL}/${id}`, { headers: getAuthHeaders() });
    if (!response.ok) throw new Error('Failed to fetch product');
    return response.json();
  },

  async create(product) {
    const response = await fetch(BASE_URL, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(product),
    });
    if (!response.ok) throw new Error('Failed to create product');
    return response.json();
  },

  async update(id, product) {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(product),
    });
    if (!response.ok) throw new Error('Failed to update product');
  },

  async delete(id) {
    const response = await fetch(`${BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete product');
  },
};

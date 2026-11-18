import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { productsAPI } from './productsAPI';

export const fetchProducts = createAsyncThunk('products/fetchProducts', async () => {
  console.log('[productsSlice] fetchProducts thunk called');
  const data = await productsAPI.getAll();
  console.log('[productsSlice] fetchProducts thunk response received - count:', data.length);
  return data;
});

export const fetchProductById = createAsyncThunk('products/fetchProductById', async (id) => {
  console.log('[productsSlice] fetchProductById thunk called with id:', id);
  const data = await productsAPI.getById(id);
  console.log('[productsSlice] fetchProductById thunk response received');
  return data;
});

export const createProduct = createAsyncThunk('products/createProduct', async (product) => {
  console.log('[productsSlice] createProduct thunk called with:', product);
  const data = await productsAPI.create(product);
  console.log('[productsSlice] createProduct thunk response received');
  return data;
});

export const updateProduct = createAsyncThunk('products/updateProduct', async ({ id, product }) => {
  console.log('[productsSlice] updateProduct thunk called with id:', id, 'product:', product);
  await productsAPI.update(id, product);
  console.log('[productsSlice] updateProduct thunk response received');
  return { id, ...product };
});

export const deleteProduct = createAsyncThunk('products/deleteProduct', async (id) => {
  console.log('[productsSlice] deleteProduct thunk called with id:', id);
  await productsAPI.delete(id);
  console.log('[productsSlice] deleteProduct thunk response received');
  return id;
});

const initialState = {
  items: [],
  selectedProduct: null,
  status: 'idle',
  error: null,
};

const productsSlice = createSlice({
  name: 'products',
  initialState,
  reducers: {
    clearSelectedProduct(state) {
      state.selectedProduct = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchProducts.pending, (state) => {
        state.status = 'loading';
      })
      .addCase(fetchProducts.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.items = action.payload;
      })
      .addCase(fetchProducts.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.error.message;
      })
      .addCase(fetchProductById.fulfilled, (state, action) => {
        state.selectedProduct = action.payload;
      })
      .addCase(createProduct.fulfilled, (state, action) => {
        state.items.push(action.payload);
      })
      .addCase(updateProduct.fulfilled, (state, action) => {
        const idx = state.items.findIndex((p) => p.id === action.payload.id);
        if (idx !== -1) state.items[idx] = action.payload;
      })
      .addCase(deleteProduct.fulfilled, (state, action) => {
        state.items = state.items.filter((p) => p.id !== action.payload);
      });
  },
});

export const { clearSelectedProduct } = productsSlice.actions;

export const selectAllProducts = (state) => state.products.items;
export const selectProductById = (state) => state.products.selectedProduct;
export const selectProductsStatus = (state) => state.products.status;
export const selectProductsError = (state) => state.products.error;

export default productsSlice.reducer;

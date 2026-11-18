import { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { createProduct, updateProduct, fetchProductById, clearSelectedProduct, selectProductById } from '../features/products/productsSlice';

export default function ProductForm() {
  console.log('[ProductForm] Component rendered');
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditMode = Boolean(id);
  const selectedProduct = useSelector(selectProductById);

  console.log('[ProductForm] isEditMode:', isEditMode, 'id:', id);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    console.log('[ProductForm] useEffect1 - loading product if edit mode');
    if (isEditMode) {
      console.log('[ProductForm] Loading product with id:', id);
      dispatch(fetchProductById(Number(id)));
    }
    return () => {
      console.log('[ProductForm] useEffect1 cleanup - clearing selected product');
      dispatch(clearSelectedProduct());
    };
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    console.log('[ProductForm] useEffect2 - populating form if edit mode');
    if (isEditMode && selectedProduct) {
      console.log('[ProductForm] ✓ Product loaded - populating form:', selectedProduct.name);
      setName(selectedProduct.name);
      setDescription(selectedProduct.description);
      setPrice(String(selectedProduct.price));
    }
  }, [isEditMode, selectedProduct]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    console.log('[ProductForm] handleSubmit() called - isEditMode:', isEditMode);
    setSaving(true);
    const productData = { name, description, price: parseFloat(price) };
    console.log('[ProductForm] Submitting data:', productData);

    let result;
    if (isEditMode) {
      console.log('[ProductForm] Updating product with id:', id);
      result = await dispatch(updateProduct({ id: Number(id), product: productData }));
    } else {
      console.log('[ProductForm] Creating new product');
      result = await dispatch(createProduct(productData));
    }

    setSaving(false);
    console.log('[ProductForm] Submit result status:', result.meta.requestStatus);
    
    if (result.meta.requestStatus === 'fulfilled') {
      console.log('[ProductForm] ✓ Submit succeeded - navigating to /products');
      navigate('/products');
    } else {
      console.log('[ProductForm] ✗ Submit failed');
    }
  };

  return (
    <div className="auth-container">
      <h1>{isEditMode ? 'Edit Product' : 'Add Product'}</h1>

      <form onSubmit={handleSubmit} className="auth-form">
        <div className="form-group">
          <label htmlFor="name">Name</label>
          <input id="name" value={name} onChange={(e) => setName(e.target.value)} required placeholder="Product name" />
        </div>
        <div className="form-group">
          <label htmlFor="description">Description</label>
          <textarea id="description" value={description} onChange={(e) => setDescription(e.target.value)} required placeholder="Product description" rows={3} className="form-control" />
        </div>
        <div className="form-group">
          <label htmlFor="price">Price ($)</label>
          <input id="price" type="number" step="0.01" min="0" value={price} onChange={(e) => setPrice(e.target.value)} required placeholder="0.00" />
        </div>
        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving...' : (isEditMode ? 'Update' : 'Create')}
          </button>
          <Link to="/products" className="btn btn-secondary">Cancel</Link>
        </div>
      </form>
    </div>
  );
}

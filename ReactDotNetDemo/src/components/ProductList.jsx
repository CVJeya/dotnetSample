import { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Link } from 'react-router-dom';
import { fetchProducts, deleteProduct, selectAllProducts, selectProductsStatus, selectProductsError } from '../features/products/productsSlice';

export default function ProductList() {
  console.log('[ProductList] Component rendered');
  const dispatch = useDispatch();
  const products = useSelector(selectAllProducts);
  const status = useSelector(selectProductsStatus);
  const error = useSelector(selectProductsError);

  console.log('[ProductList] Products loaded:', products.length, 'Status:', status);

  useEffect(() => {
    console.log('[ProductList] useEffect() called - fetching products');
    dispatch(fetchProducts());
  }, [dispatch]);

  const handleDelete = (id) => {
    console.log('[ProductList] handleDelete() called with id:', id);
    if (!confirm('Delete this product?')) {
      console.log('[ProductList] Delete cancelled by user');
      return;
    }
    console.log('[ProductList] User confirmed delete - dispatching deleteProduct()');
    dispatch(deleteProduct(id));
  };

  if (status === 'loading') return <div className="loading">Loading products...</div>;
  if (status === 'failed') return <div className="error">Error: {error}</div>;

  return (
    <div>
      <div className="page-header">
        <h1>Products</h1>
        <Link to="/products/new" className="btn btn-primary">Add Product</Link>
      </div>

      {products.length === 0 ? (
        <div className="empty-state">No products found.</div>
      ) : (
        <div className="table-wrapper">
          <table className="user-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Description</th>
                <th>Price</th>
                <th>Created By</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.id}>
                  <td>{product.id}</td>
                  <td className="user-link">{product.name}</td>
                  <td>{product.description}</td>
                  <td>${Number(product.price).toFixed(2)}</td>
                  <td>{product.createdBy}</td>
                  <td>
                    <div className="actions">
                      <button onClick={() => handleDelete(product.id)} className="btn btn-sm btn-danger">Delete</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

import { Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import Login from './components/Login';
import Register from './components/Register';
import ProductList from './components/ProductList';
import ProductForm from './components/ProductForm';
import UserList from './features/users/UserList';
import UserForm from './features/users/UserForm';
import UserDetail from './features/users/UserDetail';
import './App.css';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />

        <Route path="/" element={<ProtectedRoute><ProductList /></ProtectedRoute>} />
        <Route path="/products" element={<ProtectedRoute><ProductList /></ProtectedRoute>} />
        <Route path="/products/new" element={<ProtectedRoute><ProductForm /></ProtectedRoute>} />
        <Route path="/products/:id" element={<ProtectedRoute><ProductForm /></ProtectedRoute>} />

        <Route path="/users" element={<UserList />} />
        <Route path="/users/new" element={<UserForm />} />
        <Route path="/users/:id" element={<UserDetail />} />
        <Route path="/users/:id/edit" element={<UserForm />} />
      </Route>
    </Routes>
  );
}

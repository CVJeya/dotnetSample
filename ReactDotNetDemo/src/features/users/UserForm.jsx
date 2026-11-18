import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../app/hooks';
import {
  createUser,
  updateUser,
  fetchUserById,
  selectUserById,
  clearSelectedUser,
} from './usersSlice';

export default function UserForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const selectedUser = useAppSelector(selectUserById);

  const isEditMode = Boolean(id);

  const [formData, setFormData] = useState({ name: '', email: '' });
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (isEditMode) {
      dispatch(fetchUserById(Number(id)));
    }
    return () => {
      dispatch(clearSelectedUser());
    };
  }, [id, isEditMode, dispatch]);

  useEffect(() => {
    if (isEditMode && selectedUser) {
      setFormData({ name: selectedUser.name, email: selectedUser.email });
    }
  }, [isEditMode, selectedUser]);

  const handleChange = (e) => {
    setFormData((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);

    try {
      if (isEditMode) {
        await dispatch(updateUser({ id: Number(id), user: formData })).unwrap();
      } else {
        await dispatch(createUser(formData)).unwrap();
      }
      navigate('/');
    } catch {
      setSubmitting(false);
    }
  };

  return (
    <div className="user-form-container">
      <h1>{isEditMode ? 'Edit User' : 'Add User'}</h1>

      <form onSubmit={handleSubmit} className="user-form">
        <div className="form-group">
          <label htmlFor="name">Name</label>
          <input
            id="name"
            name="name"
            type="text"
            required
            value={formData.name}
            onChange={handleChange}
            placeholder="Enter full name"
          />
        </div>

        <div className="form-group">
          <label htmlFor="email">Email</label>
          <input
            id="email"
            name="email"
            type="email"
            required
            value={formData.email}
            onChange={handleChange}
            placeholder="Enter email address"
          />
        </div>

        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={submitting}>
            {submitting ? 'Saving...' : isEditMode ? 'Update User' : 'Create User'}
          </button>
          <button type="button" className="btn btn-secondary" onClick={() => navigate('/')}>
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}

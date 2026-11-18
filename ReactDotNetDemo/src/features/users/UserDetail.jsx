import { useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../app/hooks';
import {
  fetchUserById,
  deleteUser,
  selectUserById,
  clearSelectedUser,
} from './usersSlice';

export default function UserDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const user = useAppSelector(selectUserById);

  useEffect(() => {
    dispatch(fetchUserById(Number(id)));
    return () => {
      dispatch(clearSelectedUser());
    };
  }, [id, dispatch]);

  const handleDelete = () => {
    if (window.confirm(`Are you sure you want to delete "${user.name}"?`)) {
      dispatch(deleteUser(Number(id)));
      navigate('/');
    }
  };

  if (!user) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="user-detail">
      <div className="page-header">
        <h1>{user.name}</h1>
        <div className="detail-actions">
          <Link to={`/users/${user.id}/edit`} className="btn btn-edit">Edit</Link>
          <button className="btn btn-danger" onClick={handleDelete}>Delete</button>
          <Link to="/" className="btn btn-secondary">Back</Link>
        </div>
      </div>

      <div className="detail-card">
        <div className="detail-row">
          <span className="detail-label">ID</span>
          <span className="detail-value">{user.id}</span>
        </div>
        <div className="detail-row">
          <span className="detail-label">Name</span>
          <span className="detail-value">{user.name}</span>
        </div>
        <div className="detail-row">
          <span className="detail-label">Email</span>
          <span className="detail-value">{user.email}</span>
        </div>
        <div className="detail-row">
          <span className="detail-label">Created At</span>
          <span className="detail-value">
            {new Date(user.createdAt).toLocaleString()}
          </span>
        </div>
      </div>
    </div>
  );
}

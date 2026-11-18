import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../app/hooks';
import {
  fetchUsers,
  deleteUser,
  selectAllUsers,
  selectUsersStatus,
  selectUsersError,
} from './usersSlice';

export default function UserList() {
  const dispatch = useAppDispatch();
  const users = useAppSelector(selectAllUsers);
  const status = useAppSelector(selectUsersStatus);
  const error = useAppSelector(selectUsersError);

  useEffect(() => {
    if (status === 'idle') {
      dispatch(fetchUsers());
    }
  }, [status, dispatch]);

  const handleDelete = (id, name) => {
    if (window.confirm(`Are you sure you want to delete "${name}"?`)) {
      dispatch(deleteUser(id));
    }
  };

  if (status === 'loading') {
    return <div className="loading">Loading users...</div>;
  }

  if (status === 'failed') {
    return <div className="error">Error: {error}</div>;
  }

  return (
    <div className="user-list">
      <div className="page-header">
        <h1>Users</h1>
        <Link to="/users/new" className="btn btn-primary">+ Add User</Link>
      </div>

      {users.length === 0 ? (
        <p className="empty-state">No users found. Create one to get started.</p>
      ) : (
        <div className="table-wrapper">
          <table className="user-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Email</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.id}</td>
                  <td>
                    <Link to={`/users/${user.id}`} className="user-link">
                      {user.name}
                    </Link>
                  </td>
                  <td>{user.email}</td>
                  <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                  <td className="actions">
                    <Link to={`/users/${user.id}/edit`} className="btn btn-sm btn-edit">
                      Edit
                    </Link>
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => handleDelete(user.id, user.name)}
                    >
                      Delete
                    </button>
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

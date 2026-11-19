import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../../app/hooks';
import { selectAuthUser } from '../auth/authSlice';
import {
  fetchCurrentUser,
  selectCurrentUser,
  selectCurrentUserError,
  selectCurrentUserStatus,
} from './usersSlice';

export default function Profile() {
  const dispatch = useAppDispatch();
  const authUser = useAppSelector(selectAuthUser);
  const user = useAppSelector(selectCurrentUser);
  const status = useAppSelector(selectCurrentUserStatus);
  const error = useAppSelector(selectCurrentUserError);

  useEffect(() => {
    if (authUser?.id != null) {
      dispatch(fetchCurrentUser(authUser.id));
    }
  }, [dispatch, authUser?.id]);

  if (authUser?.id == null) {
    return <div className="error">Unable to identify the signed-in user.</div>;
  }

  if (status === 'idle' || status === 'loading') {
    return <div className="loading">Loading profile...</div>;
  }

  if (status === 'failed') {
    return <div className="error">Error: {error}</div>;
  }

  return (
    <div className="user-detail">
      <div className="page-header">
        <h1>My Profile</h1>
      </div>

      <div className="detail-card">
        <div className="detail-row">
          <span className="detail-label">Name</span>
          <span className="detail-value">{user.name}</span>
        </div>
        <div className="detail-row">
          <span className="detail-label">Email</span>
          <span className="detail-value">{user.email}</span>
        </div>
        <div className="detail-row">
          <span className="detail-label">Account ID</span>
          <span className="detail-value">{user.id}</span>
        </div>
        <div className="detail-row">
          <span className="detail-label">Member since</span>
          <span className="detail-value">
            {new Date(user.createdAt).toLocaleDateString()}
          </span>
        </div>
      </div>
    </div>
  );
}

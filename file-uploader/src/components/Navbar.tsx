import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const NavbarComp: React.FC = () => {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout(); // if async; safe even if not
    navigate('/');
  };

  return (
    <nav style={styles.nav}>
      <div style={styles.left}>
        <Link to="/" style={styles.brand}>App</Link>

        {isAuthenticated ? (
          <Link to="/dashboard" style={styles.link}>
            VideoUploader
          </Link>
        ) : (
          <span style={styles.muted}>
            Sign in to access File Uploader
          </span>
        )}
      </div>

      <div style={styles.right}>
        {!isAuthenticated ? (
          <Link to="/login" style={styles.link}>
            Login
          </Link>
        ) : (
          <button onClick={handleLogout} style={styles.logoutBtn}>
            Logout
          </button>
        )}
      </div>
    </nav>
  );
};

export default NavbarComp;

const styles: Record<string, React.CSSProperties> = {
  nav: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '12px 20px',
    borderBottom: '1px solid #ddd',
  },
  left: {
    display: 'flex',
    alignItems: 'center',
    gap: '16px',
  },
  right: {
    display: 'flex',
    alignItems: 'center',
  },
  brand: {
    fontWeight: 600,
    textDecoration: 'none',
    color: '#000',
  },
  link: {
    textDecoration: 'none',
    color: '#007bff',
    fontWeight: 500,
  },
  muted: {
    color: '#777',
    fontStyle: 'italic',
  },
  logoutBtn: {
    background: 'none',
    border: 'none',
    color: '#d00',
    cursor: 'pointer',
    fontWeight: 500,
  },
  
};

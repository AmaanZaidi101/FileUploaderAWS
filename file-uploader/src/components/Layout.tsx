import React from 'react';
import { Outlet } from 'react-router-dom';
import NavbarComp from './Navbar';

const Layout: React.FC = () => {
  return (
    <>
      <NavbarComp />
      <main style={{
  maxWidth: '1200px',
  margin: '0 auto',
  padding: '32px 24px'
}}>
  <Outlet />
</main>
    </>
  );
};

export default Layout;

const styles: Record<string, React.CSSProperties> = {
  main: {
    maxWidth: '1200px',
    margin: '0 auto',
    padding: '24px',
  },
};

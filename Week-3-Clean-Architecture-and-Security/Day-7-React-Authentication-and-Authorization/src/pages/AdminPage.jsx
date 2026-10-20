function AdminPage() {
  return (
    <main className="lesson-card">
      <p className="eyebrow">Admin area</p>
      <h1>Admin page</h1>
      <p>This page is intended for users with the Admin role.</p>
      <section aria-labelledby="security-note-heading">
        <h2 id="security-note-heading">Security note</h2>
        <p>
          React route guards improve the user experience. Browser-decoded JWT
          claims can be changed or become stale. The backend must validate each
          JWT and enforce role permissions on every protected API endpoint.
        </p>
      </section>
    </main>
  );
}

export default AdminPage;

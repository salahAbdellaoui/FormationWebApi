import { Link } from "react-router-dom";

function Forbidden() {
  return (
    <main className="lesson-card">
      <p className="eyebrow">Access denied</p>
      <h1>Forbidden</h1>
      <p>You do not have permission to view this page.</p>
      <Link to="/">Return to the start page</Link>
    </main>
  );
}

export default Forbidden;

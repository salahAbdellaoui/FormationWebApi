import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./auth/useAuth.js";
import ProtectedRoute from "./auth/ProtectedRoute.jsx";
import AdminPage from "./pages/AdminPage.jsx";
import Dashboard from "./pages/Dashboard.jsx";
import Forbidden from "./pages/Forbidden.jsx";
import Login from "./pages/Login.jsx";

function App() {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      <Route
        path="/"
        element={
          <Navigate
            to={isAuthenticated ? "/dashboard" : "/login"}
            replace
          />
        }
      />
      <Route
        path="/login"
        element={
          isAuthenticated ? <Navigate to="/dashboard" replace /> : <Login />
        }
      />
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/admin"
        element={
          <ProtectedRoute requiredRoles={["Admin"]}>
            <AdminPage />
          </ProtectedRoute>
        }
      />
      <Route path="/forbidden" element={<Forbidden />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default App;

import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./useAuth.js";

function ProtectedRoute({ children, requiredRoles = [] }) {
  const { isAuthenticated, roles } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return (
      <Navigate
        to="/login"
        replace
        state={{ from: location }}
      />
    );
  }

  const rolesToRequire = Array.isArray(requiredRoles)
    ? requiredRoles
    : [requiredRoles];
  const hasRequiredRole =
    rolesToRequire.length === 0 ||
    rolesToRequire.some((role) => roles.includes(role));

  if (!hasRequiredRole) {
    return <Navigate to="/forbidden" replace />;
  }

  // This guard controls the UI only; the API validates tokens on requests.
  return children;
}

export default ProtectedRoute;

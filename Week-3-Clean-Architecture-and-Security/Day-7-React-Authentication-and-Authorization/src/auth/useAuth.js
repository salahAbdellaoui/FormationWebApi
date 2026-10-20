import { useContext } from "react";
import { AuthContext } from "./AuthContext.jsx";
import { readTokenUiClaims } from "./tokenClaims.js";

export function useAuth() {
  const authState = useContext(AuthContext);

  if (authState === null) {
    throw new Error("useAuth must be used inside an AuthProvider.");
  }

  // These unverified claims guide the UI only; the backend validates the token.
  const claims = readTokenUiClaims(authState.token);
  const isAuthenticated = Boolean(claims && !claims.isExpired);

  return {
    ...authState,
    isAuthenticated,
    roles: isAuthenticated ? claims.roles : [],
  };
}

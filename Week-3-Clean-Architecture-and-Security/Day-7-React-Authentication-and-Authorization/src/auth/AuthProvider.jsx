import { useCallback, useEffect, useState } from "react";
import {
  apiClient,
  registerAuthExpiryHandler,
} from "../api/apiClient.js";
import { AuthContext } from "./AuthContext.jsx";
import { AUTH_TOKEN_STORAGE_KEY } from "./authStorage.js";

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() =>
    sessionStorage.getItem(AUTH_TOKEN_STORAGE_KEY),
  );

  async function login(username, password) {
    const response = await apiClient.post("/auth/login", {
      username,
      password,
    });
    const accessToken = response.data?.token;

    if (typeof accessToken !== "string" || accessToken.length === 0) {
      throw new Error("The login response did not include an access token.");
    }

    // Keep the token in this tab; JavaScript can still read sessionStorage.
    sessionStorage.setItem(AUTH_TOKEN_STORAGE_KEY, accessToken);
    setToken(accessToken);
  }

  const logout = useCallback(() => {
    sessionStorage.removeItem(AUTH_TOKEN_STORAGE_KEY);
    setToken(null);
  }, []);

  useEffect(
    () => registerAuthExpiryHandler(logout),
    [logout],
  );

  const authState = {
    token,
    login,
    logout,
  };

  return <AuthContext.Provider value={authState}>{children}</AuthContext.Provider>;
}

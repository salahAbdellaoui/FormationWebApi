import axios from "axios";
import { AUTH_TOKEN_STORAGE_KEY } from "../auth/authStorage.js";

// Keep the API address in local environment configuration, not in request code.
export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
});

let authExpiryHandler = null;

export function registerAuthExpiryHandler(handler) {
  authExpiryHandler = handler;

  return () => {
    if (authExpiryHandler === handler) {
      authExpiryHandler = null;
    }
  };
}

function isLoginRequest(config) {
  const path = (config?.url ?? "").split("?")[0].replace(/\/+$/, "");
  return config?.method?.toLowerCase() === "post" && path.endsWith("/auth/login");
}

// Install one interceptor pair when this module loads, not on React renders.
apiClient.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(AUTH_TOKEN_STORAGE_KEY);

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !isLoginRequest(error.config)) {
      authExpiryHandler?.();
    }

    // Keep the original error so callers retain its status and response data.
    return Promise.reject(error);
  },
);

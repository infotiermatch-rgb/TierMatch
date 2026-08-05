import axios, {
  type AxiosError,
  type InternalAxiosRequestConfig,
} from "axios";

import { authTokenStore } from "../features/authentication/authTokenStore";
import type {
  AuthenticationResponse,
  RefreshRequest,
} from "../types/auth";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL;

if (!apiBaseUrl) {
  throw new Error(
    "Die Umgebungsvariable VITE_API_BASE_URL wurde nicht konfiguriert.",
  );
}

export const httpClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10_000,
  headers: {
    Accept: "application/json",
  },
});

/*
 * Dieser Client besitzt keine Interceptors.
 * Dadurch vermeiden wir eine Endlosschleife, wenn auch der
 * Refresh-Endpunkt mit 401 antwortet.
 */
const refreshHttpClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10_000,
  headers: {
    Accept: "application/json",
  },
});

type RetryableRequestConfig =
  InternalAxiosRequestConfig & {
    _retry?: boolean;
  };

let activeRefreshRequest: Promise<string> | null = null;

function isRefreshExcludedEndpoint(
  url: string | undefined,
): boolean {
  if (!url) {
    return false;
  }

  const excludedEndpoints = [
    "/api/v1/auth/login",
    "/api/v1/auth/register",
    "/api/v1/auth/refresh",
    "/api/v1/auth/forgot-password",
    "/api/v1/auth/reset-password",
  ];

  return excludedEndpoints.some((endpoint) =>
    url.includes(endpoint),
  );
}

async function performTokenRefresh(): Promise<string> {
  const refreshToken =
    authTokenStore.getRefreshToken();

  if (!refreshToken) {
    throw new Error("Es ist kein Refresh Token vorhanden.");
  }

  const request: RefreshRequest = {
    refreshToken,
  };

  const response =
    await refreshHttpClient.post<AuthenticationResponse>(
      "/api/v1/auth/refresh",
      request,
    );

  authTokenStore.setSession(response.data);

  return response.data.accessToken;
}

export function refreshAccessToken(): Promise<string> {
  /*
   * Mehrere gleichzeitige 401-Antworten verwenden dieselbe
   * Refresh-Anfrage. Das verhindert eine mehrfache Rotation
   * desselben Refresh Tokens.
   */
  if (!activeRefreshRequest) {
    activeRefreshRequest = performTokenRefresh().finally(
      () => {
        activeRefreshRequest = null;
      },
    );
  }

  return activeRefreshRequest;
}

httpClient.interceptors.request.use(
  (config) => {
    const accessToken =
      authTokenStore.getAccessToken();

    if (accessToken) {
      config.headers.set(
        "Authorization",
        `Bearer ${accessToken}`,
      );
    }

    return config;
  },
  (error: unknown) => Promise.reject(error),
);

httpClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest =
      error.config as RetryableRequestConfig | undefined;

    const shouldRefresh =
      error.response?.status === 401 &&
      originalRequest !== undefined &&
      originalRequest._retry !== true &&
      !isRefreshExcludedEndpoint(
        originalRequest.url,
      );

    if (!shouldRefresh || !originalRequest) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      const newAccessToken =
        await refreshAccessToken();

      originalRequest.headers.set(
        "Authorization",
        `Bearer ${newAccessToken}`,
      );

      return await httpClient(originalRequest);
    } catch {
      authTokenStore.clearSession();

      window.dispatchEvent(
        new Event("tiermatch:session-expired"),
      );

      return Promise.reject(error);
    }
  },
);
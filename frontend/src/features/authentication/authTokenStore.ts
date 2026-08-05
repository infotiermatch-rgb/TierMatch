import type { AuthenticationResponse } from "../../types/auth";

const refreshTokenStorageKey = "tiermatch.refreshToken";

let accessToken: string | null = null;

function getRefreshToken(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return window.sessionStorage.getItem(
    refreshTokenStorageKey,
  );
}

function setSession(
  authentication: AuthenticationResponse,
): void {
  accessToken = authentication.accessToken;

  window.sessionStorage.setItem(
    refreshTokenStorageKey,
    authentication.refreshToken,
  );
}

function clearSession(): void {
  accessToken = null;

  if (typeof window !== "undefined") {
    window.sessionStorage.removeItem(
      refreshTokenStorageKey,
    );
  }
}

export const authTokenStore = {
  getAccessToken(): string | null {
    return accessToken;
  },

  getRefreshToken,

  setSession,

  clearSession,
};
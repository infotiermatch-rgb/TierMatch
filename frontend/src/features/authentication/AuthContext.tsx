import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";

import { refreshAccessToken } from "../../api/httpClient";

import type {
  ChangePasswordRequest,
  CurrentUserResponse,
  LoginRequest,
  RegisterRequest,
  UpdateCurrentUserProfileRequest,
} from "../../types/auth";

import {
  changePasswordRequest,
  currentUserRequest,
  loginRequest,
  logoutRequest,
  registerRequest,
  updateCurrentUserProfileRequest,
} from "./authApi";

import { authTokenStore } from "./authTokenStore";

export type AuthenticationStatus =
  | "loading"
  | "authenticated"
  | "anonymous";

export type UserRole =
  | "Admin"
  | "ShelterAdmin"
  | "User";

type AuthContextValue = {
  status: AuthenticationStatus;
  user: CurrentUserResponse | null;

  /*
   * Rollen- und Tierheimdaten
   */
  roles: string[];
  shelterId: string | null;

  isAdmin: boolean;
  isShelterAdmin: boolean;
  isUser: boolean;

  /*
   * true für:
   * - globale Administratoren
   * - ShelterAdmins mit gültiger ShelterId
   */
  canManageShelter: boolean;

  hasRole: (
    role: UserRole,
  ) => boolean;

  login: (
    request: LoginRequest,
  ) => Promise<CurrentUserResponse>;

  register: (
    request: RegisterRequest,
  ) => Promise<void>;

  updateProfile: (
    request: UpdateCurrentUserProfileRequest,
  ) => Promise<CurrentUserResponse>;

  changePassword: (
    request: ChangePasswordRequest,
  ) => Promise<void>;

  logout: () => Promise<void>;

  reloadCurrentUser:
    () => Promise<CurrentUserResponse>;
};

type AuthProviderProps = {
  children: ReactNode;
};

const AuthContext =
  createContext<AuthContextValue | undefined>(
    undefined,
  );

export function AuthProvider({
  children,
}: AuthProviderProps) {
  const [status, setStatus] =
    useState<AuthenticationStatus>("loading");

  const [user, setUser] =
    useState<CurrentUserResponse | null>(null);

  const setAnonymousSession =
    useCallback(() => {
      authTokenStore.clearSession();
      setUser(null);
      setStatus("anonymous");
    }, []);

  const reloadCurrentUser =
    useCallback(
      async (): Promise<CurrentUserResponse> => {
        const currentUser =
          await currentUserRequest();

        setUser(currentUser);
        setStatus("authenticated");

        return currentUser;
      },
      [],
    );

  const login = useCallback(
    async (
      request: LoginRequest,
    ): Promise<CurrentUserResponse> => {
      const authentication =
        await loginRequest(request);

      authTokenStore.setSession(authentication);

      try {
        return await reloadCurrentUser();
      } catch (error: unknown) {
        setAnonymousSession();
        throw error;
      }
    },
    [
      reloadCurrentUser,
      setAnonymousSession,
    ],
  );

  const register = useCallback(
    async (
      request: RegisterRequest,
    ): Promise<void> => {
      const authentication =
        await registerRequest(request);

      authTokenStore.setSession(authentication);

      try {
        await reloadCurrentUser();
      } catch (error: unknown) {
        setAnonymousSession();
        throw error;
      }
    },
    [
      reloadCurrentUser,
      setAnonymousSession,
    ],
  );

  const updateProfile = useCallback(
    async (
      request: UpdateCurrentUserProfileRequest,
    ): Promise<CurrentUserResponse> => {
      const updatedUser =
        await updateCurrentUserProfileRequest(
          request,
        );

      setUser(updatedUser);
      setStatus("authenticated");

      return updatedUser;
    },
    [],
  );

  const changePassword = useCallback(
    async (
      request: ChangePasswordRequest,
    ): Promise<void> => {
      await changePasswordRequest(request);

      /*
       * Nach einer Passwortänderung wird die lokale
       * Sitzung sicherheitshalber beendet.
       */
      setAnonymousSession();
    },
    [setAnonymousSession],
  );

  const logout =
    useCallback(async (): Promise<void> => {
      const refreshToken =
        authTokenStore.getRefreshToken();

      try {
        if (refreshToken) {
          await logoutRequest({
            refreshToken,
          });
        }
      } finally {
        setAnonymousSession();
      }
    }, [setAnonymousSession]);

  useEffect(() => {
    let isCancelled = false;

    async function initializeSession(): Promise<void> {
      const refreshToken =
        authTokenStore.getRefreshToken();

      if (!refreshToken) {
        if (!isCancelled) {
          setStatus("anonymous");
        }

        return;
      }

      try {
        await refreshAccessToken();

        const currentUser =
          await currentUserRequest();

        if (!isCancelled) {
          setUser(currentUser);
          setStatus("authenticated");
        }
      } catch {
        if (!isCancelled) {
          setAnonymousSession();
        }
      }
    }

    function handleExpiredSession(): void {
      setAnonymousSession();
    }

    window.addEventListener(
      "tiermatch:session-expired",
      handleExpiredSession,
    );

    void initializeSession();

    return () => {
      isCancelled = true;

      window.removeEventListener(
        "tiermatch:session-expired",
        handleExpiredSession,
      );
    };
  }, [setAnonymousSession]);

  const roles = useMemo(
    () => user?.roles ?? [],
    [user],
  );

  const hasRole = useCallback(
    (role: UserRole): boolean => {
      return roles.includes(role);
    },
    [roles],
  );

  const isAdmin = useMemo(
    () => hasRole("Admin"),
    [hasRole],
  );

  const isShelterAdmin = useMemo(
    () => hasRole("ShelterAdmin"),
    [hasRole],
  );

  const isUser = useMemo(
    () => hasRole("User"),
    [hasRole],
  );

  const shelterId =
    user?.shelterId ?? null;

  const canManageShelter =
    isAdmin ||
    (
      isShelterAdmin &&
      shelterId !== null
    );

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      user,
      roles,
      shelterId,
      isAdmin,
      isShelterAdmin,
      isUser,
      canManageShelter,
      hasRole,
      login,
      register,
      updateProfile,
      changePassword,
      logout,
      reloadCurrentUser,
    }),
    [
      status,
      user,
      roles,
      shelterId,
      isAdmin,
      isShelterAdmin,
      isUser,
      canManageShelter,
      hasRole,
      login,
      register,
      updateProfile,
      changePassword,
      logout,
      reloadCurrentUser,
    ],
  );

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error(
      "useAuth muss innerhalb eines AuthProvider verwendet werden.",
    );
  }

  return context;
}
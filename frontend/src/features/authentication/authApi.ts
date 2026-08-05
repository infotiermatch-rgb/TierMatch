import { httpClient } from "../../api/httpClient";

import type {
  AuthenticationResponse,
  ChangePasswordRequest,
  CurrentUserResponse,
  ForgotPasswordRequest,
  LoginRequest,
  LogoutRequest,
  RegisterRequest,
  ResetPasswordRequest,
  UpdateCurrentUserProfileRequest,
} from "../../types/auth";

export async function registerRequest(
  request: RegisterRequest,
): Promise<AuthenticationResponse> {
  const response =
    await httpClient.post<AuthenticationResponse>(
      "/api/v1/auth/register",
      request,
    );

  return response.data;
}

export async function loginRequest(
  request: LoginRequest,
): Promise<AuthenticationResponse> {
  const response =
    await httpClient.post<AuthenticationResponse>(
      "/api/v1/auth/login",
      request,
    );

  return response.data;
}

export async function forgotPasswordRequest(
  request: ForgotPasswordRequest,
): Promise<void> {
  await httpClient.post(
    "/api/v1/auth/forgot-password",
    request,
  );
}

export async function resetPasswordRequest(
  request: ResetPasswordRequest,
): Promise<void> {
  await httpClient.post(
    "/api/v1/auth/reset-password",
    request,
  );
}

export async function currentUserRequest(): Promise<CurrentUserResponse> {
  const response =
    await httpClient.get<CurrentUserResponse>(
      "/api/v1/auth/me",
    );

  return response.data;
}

export async function updateCurrentUserProfileRequest(
  request: UpdateCurrentUserProfileRequest,
): Promise<CurrentUserResponse> {
  const response =
    await httpClient.patch<CurrentUserResponse>(
      "/api/v1/auth/me",
      request,
    );

  return response.data;
}

export async function changePasswordRequest(
  request: ChangePasswordRequest,
): Promise<void> {
  await httpClient.post(
    "/api/v1/auth/change-password",
    request,
  );
}

export async function logoutRequest(
  request: LogoutRequest,
): Promise<void> {
  await httpClient.post(
    "/api/v1/auth/logout",
    request,
  );
}
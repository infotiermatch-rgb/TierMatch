export type LoginRequest = {
  email: string;
  password: string;
};

export type RegisterRequest = {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
};

export type ForgotPasswordRequest = {
  email: string;
};

export type ResetPasswordRequest = {
  email: string;
  token: string;
  newPassword: string;
};

export type UpdateCurrentUserProfileRequest = {
  firstName: string;
  lastName: string;
};

export type ChangePasswordRequest = {
  currentPassword: string;
  newPassword: string;
};

export type RefreshRequest = {
  refreshToken: string;
};

export type LogoutRequest = {
  refreshToken: string;
};

export type AuthenticationResponse = {
  accessToken: string;
  refreshToken: string;

  accessTokenExpiresAt?: string;
  expiresAt?: string;

  userId: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  roles: string[];
};

export type CurrentUserResponse = {
  userId: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  roles: string[];
  shelterId: string | null;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
};
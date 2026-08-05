import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  Navigate,
  useLocation,
  useNavigate,
} from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { useAuth } from "../features/authentication/AuthContext";

const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib deine E-Mail-Adresse ein.",
    )
    .email(
      "Bitte gib eine gültige E-Mail-Adresse ein.",
    ),

  password: z
    .string()
    .min(
      1,
      "Bitte gib dein Passwort ein.",
    ),
});

type LoginFormValues = z.infer<
  typeof loginSchema
>;

type LoginLocationState = {
  from?: {
    pathname?: string;
  };

  passwordReset?: boolean;
  passwordChanged?: boolean;
  email?: string;
};

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const { login, status } = useAuth();

  const locationState =
    location.state as LoginLocationState | null;

  const destination =
    locationState?.from?.pathname ??
    "/profile";

  const passwordWasReset =
    locationState?.passwordReset === true;

  const passwordWasChanged =
    locationState?.passwordChanged === true;

  const previousEmail =
    locationState?.email ?? "";

  const [serverError, setServerError] =
    useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: previousEmail,
      password: "",
    },
  });

  if (status === "authenticated") {
    return (
      <Navigate
        to={destination}
        replace
      />
    );
  }

  async function onSubmit(
    values: LoginFormValues,
  ): Promise<void> {
    setServerError(null);

    try {
      await login({
        email: values.email.trim(),
        password: values.password,
      });

      navigate(destination, {
        replace: true,
      });
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Die Anmeldung ist fehlgeschlagen. Bitte überprüfe deine Zugangsdaten.",
        ),
      );
    }
  }

  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">
          TierMatch-Konto
        </p>

        <h1>Anmelden</h1>

        <p className="form-card__description">
          Melde dich an, um dein Profil und deine
          TierMatch-Funktionen zu verwenden.
        </p>

        {passwordWasReset && (
          <div
            className="form-alert form-alert--success"
            role="status"
          >
            Dein Passwort wurde erfolgreich
            zurückgesetzt. Du kannst dich jetzt
            mit dem neuen Passwort anmelden.
          </div>
        )}

        {passwordWasChanged && (
          <div
            className="form-alert form-alert--success"
            role="status"
          >
            Dein Passwort wurde erfolgreich
            geändert. Bitte melde dich erneut an.
          </div>
        )}

        {serverError && (
          <div
            className="form-alert form-alert--error"
            role="alert"
          >
            {serverError}
          </div>
        )}

        <form
          className="form"
          onSubmit={handleSubmit(onSubmit)}
          noValidate
        >
          <div className="form-field">
            <label htmlFor="email">
              E-Mail-Adresse
            </label>

            <input
              id="email"
              type="email"
              autoComplete="email"
              aria-invalid={
                errors.email
                  ? "true"
                  : "false"
              }
              {...register("email")}
            />

            {errors.email && (
              <p className="form-field__error">
                {errors.email.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="password">
              Passwort
            </label>

            <input
              id="password"
              type="password"
              autoComplete="current-password"
              autoFocus={
                passwordWasReset ||
                passwordWasChanged
              }
              aria-invalid={
                errors.password
                  ? "true"
                  : "false"
              }
              {...register("password")}
            />

            {errors.password && (
              <p className="form-field__error">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="form-links">
            <Link
              className="text-link"
              to="/forgot-password"
            >
              Passwort vergessen?
            </Link>
          </div>

          <button
            className="button button--primary button--full"
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting
              ? "Anmeldung läuft …"
              : "Anmelden"}
          </button>
        </form>

        <p className="form-card__footer">
          Noch kein Konto?{" "}
          <Link
            className="text-link"
            to="/register"
          >
            Jetzt registrieren
          </Link>
        </p>
      </div>
    </section>
  );
}
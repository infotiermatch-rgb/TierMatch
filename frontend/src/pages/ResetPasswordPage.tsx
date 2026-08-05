import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  useNavigate,
  useSearchParams,
} from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { resetPasswordRequest } from "../features/authentication/authApi";
import { useAuth } from "../features/authentication/AuthContext";

const resetPasswordSchema = z
  .object({
    newPassword: z
      .string()
      .min(
        8,
        "Das Passwort muss mindestens 8 Zeichen enthalten.",
      )
      .regex(
        /[a-z]/,
        "Das Passwort muss mindestens einen Kleinbuchstaben enthalten.",
      )
      .regex(
        /[A-Z]/,
        "Das Passwort muss mindestens einen Großbuchstaben enthalten.",
      )
      .regex(
        /[0-9]/,
        "Das Passwort muss mindestens eine Zahl enthalten.",
      ),

    confirmPassword: z
      .string()
      .min(
        1,
        "Bitte wiederhole dein neues Passwort.",
      ),
  })
  .refine(
    (values) =>
      values.newPassword ===
      values.confirmPassword,
    {
      message:
        "Die eingegebenen Passwörter stimmen nicht überein.",
      path: ["confirmPassword"],
    },
  );

type ResetPasswordFormValues = z.infer<
  typeof resetPasswordSchema
>;

type ResetPasswordLoginState = {
  passwordReset: true;
  email: string;
};

export function ResetPasswordPage() {
  const navigate = useNavigate();

  const [searchParams] = useSearchParams();

  const { logout } = useAuth();

  const email =
    searchParams.get("email")?.trim() ?? "";

  const token =
    searchParams.get("token") ?? "";

  const emailIsValid = z
    .string()
    .email()
    .safeParse(email)
    .success;

  const resetLinkIsValid =
    emailIsValid &&
    token.trim().length > 0;

  const [serverError, setServerError] =
    useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(
      resetPasswordSchema,
    ),
    defaultValues: {
      newPassword: "",
      confirmPassword: "",
    },
  });

  async function onSubmit(
    values: ResetPasswordFormValues,
  ): Promise<void> {
    if (!resetLinkIsValid) {
      return;
    }

    setServerError(null);

    try {
      await resetPasswordRequest({
        email,
        token,
        newPassword: values.newPassword,
      });

      /*
       * Das Backend widerruft beim Passwort-Reset
       * vorhandene Refresh Tokens. Dadurch wird auch
       * eine möglicherweise noch bestehende lokale
       * Anmeldung beendet.
       */
      try {
        await logout();
      } catch {
        /*
         * logout() entfernt die lokale Sitzung durch
         * seinen finally-Block auch dann, wenn die
         * Logout-Anfrage fehlschlägt.
         */
      }

      const state: ResetPasswordLoginState = {
        passwordReset: true,
        email,
      };

      navigate("/login", {
        replace: true,
        state,
      });
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Das Passwort konnte nicht zurückgesetzt werden. Der Link ist möglicherweise ungültig oder abgelaufen.",
        ),
      );
    }
  }

  if (!resetLinkIsValid) {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            Ungültiger Reset-Link
          </p>

          <h1>Der Link ist unvollständig</h1>

          <div
            className="form-alert form-alert--error"
            role="alert"
          >
            Der Passwort-Reset-Link enthält keine
            gültige E-Mail-Adresse oder keinen
            Reset-Token.
          </div>

          <p className="form-card__description">
            Fordere einen neuen Link an und öffne
            anschließend den vollständigen Link aus
            der E-Mail.
          </p>

          <Link
            className="button button--primary button--full"
            to="/forgot-password"
          >
            Neuen Reset-Link anfordern
          </Link>
        </div>
      </section>
    );
  }

  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">
          Kontowiederherstellung
        </p>

        <h1>Neues Passwort festlegen</h1>

        <p className="form-card__description">
          Lege ein neues Passwort für{" "}
          <strong>{email}</strong> fest.
        </p>

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
            <label htmlFor="newPassword">
              Neues Passwort
            </label>

            <input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              autoFocus
              aria-invalid={
                errors.newPassword
                  ? "true"
                  : "false"
              }
              {...register("newPassword")}
            />

            <p className="form-field__hint">
              Mindestens 8 Zeichen sowie jeweils
              ein Großbuchstabe, ein Kleinbuchstabe
              und eine Zahl.
            </p>

            {errors.newPassword && (
              <p className="form-field__error">
                {errors.newPassword.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="confirmPassword">
              Neues Passwort wiederholen
            </label>

            <input
              id="confirmPassword"
              type="password"
              autoComplete="new-password"
              aria-invalid={
                errors.confirmPassword
                  ? "true"
                  : "false"
              }
              {...register(
                "confirmPassword",
              )}
            />

            {errors.confirmPassword && (
              <p className="form-field__error">
                {
                  errors.confirmPassword
                    .message
                }
              </p>
            )}
          </div>

          <button
            className="button button--primary button--full"
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting
              ? "Passwort wird gespeichert …"
              : "Passwort speichern"}
          </button>
        </form>

        <p className="form-card__footer">
          <Link
            className="text-link"
            to="/login"
          >
            Zur Anmeldung
          </Link>
        </p>
      </div>
    </section>
  );
}
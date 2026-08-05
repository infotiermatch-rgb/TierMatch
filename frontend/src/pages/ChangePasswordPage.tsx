import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  useNavigate,
} from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { useAuth } from "../features/authentication/AuthContext";

const changePasswordSchema = z
  .object({
    currentPassword: z
      .string()
      .min(
        1,
        "Bitte gib dein aktuelles Passwort ein.",
      ),

    newPassword: z
      .string()
      .min(
        8,
        "Das neue Passwort muss mindestens 8 Zeichen enthalten.",
      )
      .regex(
        /[a-z]/,
        "Das neue Passwort muss mindestens einen Kleinbuchstaben enthalten.",
      )
      .regex(
        /[A-Z]/,
        "Das neue Passwort muss mindestens einen Großbuchstaben enthalten.",
      )
      .regex(
        /[0-9]/,
        "Das neue Passwort muss mindestens eine Zahl enthalten.",
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
      values.newPassword === values.confirmPassword,
    {
      message:
        "Die eingegebenen Passwörter stimmen nicht überein.",
      path: ["confirmPassword"],
    },
  )
  .refine(
    (values) =>
      values.currentPassword !== values.newPassword,
    {
      message:
        "Das neue Passwort muss sich vom aktuellen Passwort unterscheiden.",
      path: ["newPassword"],
    },
  );

type ChangePasswordFormValues = z.infer<
  typeof changePasswordSchema
>;

export function ChangePasswordPage() {
  const navigate = useNavigate();

  const {
    user,
    changePassword,
  } = useAuth();

  const [serverError, setServerError] =
    useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(
      changePasswordSchema,
    ),
    defaultValues: {
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  if (!user) {
    return null;
  }

  /*
   * Die E-Mail-Adresse wird als eigenständiger String
   * gespeichert. Dadurch weiß TypeScript auch innerhalb
   * der asynchronen onSubmit-Funktion, dass sie nicht null ist.
   */
  const userEmail = user.email;

  async function onSubmit(
    values: ChangePasswordFormValues,
  ): Promise<void> {
    setServerError(null);

    try {
      await changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });

      navigate("/login", {
        replace: true,
        state: {
          passwordChanged: true,
          email: userEmail,
        },
      });
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Das Passwort konnte nicht geändert werden. Bitte überprüfe dein aktuelles Passwort.",
        ),
      );
    }
  }

  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">
          Kontosicherheit
        </p>

        <h1>Passwort ändern</h1>

        <p className="form-card__description">
          Nach der Änderung wirst du abgemeldet und
          musst dich mit dem neuen Passwort erneut
          anmelden.
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
            <label htmlFor="currentPassword">
              Aktuelles Passwort
            </label>

            <input
              id="currentPassword"
              type="password"
              autoComplete="current-password"
              autoFocus
              aria-invalid={
                errors.currentPassword
                  ? "true"
                  : "false"
              }
              {...register("currentPassword")}
            />

            {errors.currentPassword && (
              <p className="form-field__error">
                {errors.currentPassword.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="newPassword">
              Neues Passwort
            </label>

            <input
              id="newPassword"
              type="password"
              autoComplete="new-password"
              aria-invalid={
                errors.newPassword
                  ? "true"
                  : "false"
              }
              {...register("newPassword")}
            />

            <p className="form-field__hint">
              Mindestens 8 Zeichen sowie jeweils ein
              Großbuchstabe, ein Kleinbuchstabe und
              eine Zahl.
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
              {...register("confirmPassword")}
            />

            {errors.confirmPassword && (
              <p className="form-field__error">
                {errors.confirmPassword.message}
              </p>
            )}
          </div>

          <div className="form-actions">
            <Link
              className="button button--outline"
              to="/profile"
            >
              Abbrechen
            </Link>

            <button
              className="button button--primary"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting
                ? "Passwort wird geändert …"
                : "Passwort ändern"}
            </button>
          </div>
        </form>
      </div>
    </section>
  );
}
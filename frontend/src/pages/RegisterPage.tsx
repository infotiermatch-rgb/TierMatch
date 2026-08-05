import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  Navigate,
  useNavigate,
} from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { useAuth } from "../features/authentication/AuthContext";

const registerSchema = z
  .object({
    firstName: z
      .string()
      .trim()
      .min(1, "Bitte gib deinen Vornamen ein.")
      .max(
        100,
        "Der Vorname darf höchstens 100 Zeichen enthalten.",
      ),

    lastName: z
      .string()
      .trim()
      .min(1, "Bitte gib deinen Nachnamen ein.")
      .max(
        100,
        "Der Nachname darf höchstens 100 Zeichen enthalten.",
      ),

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
        8,
        "Das Passwort muss mindestens 8 Zeichen enthalten.",
      ),

    confirmPassword: z
      .string()
      .min(
        1,
        "Bitte wiederhole dein Passwort.",
      ),
  })
  .refine(
    (values) =>
      values.password ===
      values.confirmPassword,
    {
      message:
        "Die eingegebenen Passwörter stimmen nicht überein.",
      path: ["confirmPassword"],
    },
  );

type RegisterFormValues = z.infer<
  typeof registerSchema
>;

export function RegisterPage() {
  const navigate = useNavigate();

  const {
    register: registerUser,
    status,
  } = useAuth();

  const [serverError, setServerError] =
    useState<string | null>(null);

  const {
    register: registerField,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
    },
  });

  if (status === "authenticated") {
    return (
      <Navigate
        to="/profile"
        replace
      />
    );
  }

  async function onSubmit(
    values: RegisterFormValues,
  ): Promise<void> {
    setServerError(null);

    try {
      await registerUser({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        email: values.email.trim(),
        password: values.password,
      });

      navigate("/profile", {
        replace: true,
      });
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Die Registrierung ist fehlgeschlagen. Bitte überprüfe deine Angaben.",
        ),
      );
    }
  }

  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">
          Neues TierMatch-Konto
        </p>

        <h1>Registrieren</h1>

        <p className="form-card__description">
          Erstelle dein Konto und entdecke Tiere
          aus Tierheimen in deiner Nähe.
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
          <div className="form-grid">
            <div className="form-field">
              <label htmlFor="firstName">
                Vorname
              </label>

              <input
                id="firstName"
                type="text"
                autoComplete="given-name"
                aria-invalid={
                  errors.firstName
                    ? "true"
                    : "false"
                }
                {...registerField("firstName")}
              />

              {errors.firstName && (
                <p className="form-field__error">
                  {errors.firstName.message}
                </p>
              )}
            </div>

            <div className="form-field">
              <label htmlFor="lastName">
                Nachname
              </label>

              <input
                id="lastName"
                type="text"
                autoComplete="family-name"
                aria-invalid={
                  errors.lastName
                    ? "true"
                    : "false"
                }
                {...registerField("lastName")}
              />

              {errors.lastName && (
                <p className="form-field__error">
                  {errors.lastName.message}
                </p>
              )}
            </div>
          </div>

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
              {...registerField("email")}
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
              autoComplete="new-password"
              aria-invalid={
                errors.password
                  ? "true"
                  : "false"
              }
              {...registerField("password")}
            />

            <p className="form-field__hint">
              Verwende mindestens 8 Zeichen.
            </p>

            {errors.password && (
              <p className="form-field__error">
                {errors.password.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="confirmPassword">
              Passwort wiederholen
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
              {...registerField(
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
              ? "Konto wird erstellt …"
              : "Konto erstellen"}
          </button>
        </form>

        <p className="form-card__footer">
          Bereits registriert?{" "}
          <Link
            className="text-link"
            to="/login"
          >
            Jetzt anmelden
          </Link>
        </p>
      </div>
    </section>
  );
}
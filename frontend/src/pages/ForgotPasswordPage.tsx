import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { forgotPasswordRequest } from "../features/authentication/authApi";

const forgotPasswordSchema = z.object({
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
});

type ForgotPasswordFormValues = z.infer<
  typeof forgotPasswordSchema
>;

export function ForgotPasswordPage() {
  const [requestSucceeded, setRequestSucceeded] =
    useState(false);

  const [serverError, setServerError] =
    useState<string | null>(null);

  const {
    register,
    handleSubmit,
    getValues,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(
      forgotPasswordSchema,
    ),
    defaultValues: {
      email: "",
    },
  });

  async function onSubmit(
    values: ForgotPasswordFormValues,
  ): Promise<void> {
    setServerError(null);

    try {
      await forgotPasswordRequest({
        email: values.email.trim(),
      });

      setRequestSucceeded(true);
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Die Passwort-Reset-Anfrage konnte nicht verarbeitet werden.",
        ),
      );
    }
  }

  if (requestSucceeded) {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            E-Mail wurde angefordert
          </p>

          <h1>Überprüfe dein Postfach</h1>

          <div
            className="form-alert form-alert--success"
            role="status"
          >
            Falls ein aktives TierMatch-Konto für{" "}
            <strong>{getValues("email")}</strong>{" "}
            existiert, wurde eine E-Mail mit einem
            Link zum Zurücksetzen des Passworts
            versendet.
          </div>

          <p className="form-card__description">
            Überprüfe auch deinen Spam-Ordner. Der
            Link in der E-Mail öffnet anschließend
            die TierMatch-Seite zum Festlegen eines
            neuen Passworts.
          </p>

          <Link
            className="button button--primary button--full"
            to="/login"
          >
            Zur Anmeldung
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

        <h1>Passwort vergessen</h1>

        <p className="form-card__description">
          Gib die E-Mail-Adresse deines
          TierMatch-Kontos ein. Du erhältst
          anschließend einen Link zum Festlegen
          eines neuen Passworts.
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
            <label htmlFor="email">
              E-Mail-Adresse
            </label>

            <input
              id="email"
              type="email"
              autoComplete="email"
              autoFocus
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

          <button
            className="button button--primary button--full"
            type="submit"
            disabled={isSubmitting}
          >
            {isSubmitting
              ? "E-Mail wird angefordert …"
              : "Reset-Link anfordern"}
          </button>
        </form>

        <p className="form-card__footer">
          Passwort wieder eingefallen?{" "}
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
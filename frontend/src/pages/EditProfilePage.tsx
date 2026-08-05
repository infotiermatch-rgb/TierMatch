import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { useAuth } from "../features/authentication/AuthContext";

const editProfileSchema = z.object({
  firstName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib deinen Vornamen ein.",
    )
    .max(
      100,
      "Der Vorname darf höchstens 100 Zeichen enthalten.",
    ),

  lastName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib deinen Nachnamen ein.",
    )
    .max(
      100,
      "Der Nachname darf höchstens 100 Zeichen enthalten.",
    ),
});

type EditProfileFormValues = z.infer<
  typeof editProfileSchema
>;

export function EditProfilePage() {
  const {
    user,
    updateProfile,
  } = useAuth();

  const [serverError, setServerError] =
    useState<string | null>(null);

  const [updateSucceeded, setUpdateSucceeded] =
    useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: {
      errors,
      isSubmitting,
      isDirty,
    },
  } = useForm<EditProfileFormValues>({
    resolver: zodResolver(
      editProfileSchema,
    ),
    defaultValues: {
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
    },
  });

  useEffect(() => {
    if (!user) {
      return;
    }

    reset({
      firstName: user.firstName ?? "",
      lastName: user.lastName ?? "",
    });
  }, [user, reset]);

  if (!user) {
    return null;
  }

  async function onSubmit(
    values: EditProfileFormValues,
  ): Promise<void> {
    setServerError(null);
    setUpdateSucceeded(false);

    try {
      const updatedUser =
        await updateProfile({
          firstName: values.firstName.trim(),
          lastName: values.lastName.trim(),
        });

      reset({
        firstName:
          updatedUser.firstName ?? "",
        lastName:
          updatedUser.lastName ?? "",
      });

      setUpdateSucceeded(true);
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Das Profil konnte nicht aktualisiert werden.",
        ),
      );
    }
  }

  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">
          Persönliche Daten
        </p>

        <h1>Profil bearbeiten</h1>

        <p className="form-card__description">
          Aktualisiere deinen Vor- und Nachnamen.
          Deine E-Mail-Adresse bleibt unverändert.
        </p>

        {updateSucceeded && (
          <div
            className="form-alert form-alert--success"
            role="status"
          >
            Dein Profil wurde erfolgreich
            aktualisiert.
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
                {...register("firstName")}
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
                {...register("lastName")}
              />

              {errors.lastName && (
                <p className="form-field__error">
                  {errors.lastName.message}
                </p>
              )}
            </div>
          </div>

          <div className="form-field">
            <label htmlFor="profileEmail">
              E-Mail-Adresse
            </label>

            <input
              id="profileEmail"
              type="email"
              value={user.email}
              disabled
              readOnly
            />

            <p className="form-field__hint">
              Die E-Mail-Adresse kann aktuell nicht
              über das Profil geändert werden.
            </p>
          </div>

          <div className="form-actions">
            <Link
              className="button button--outline"
              to="/profile"
            >
              Zurück
            </Link>

            <button
              className="button button--primary"
              type="submit"
              disabled={
                isSubmitting || !isDirty
              }
            >
              {isSubmitting
                ? "Profil wird gespeichert …"
                : "Änderungen speichern"}
            </button>
          </div>
        </form>
      </div>
    </section>
  );
}
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

import { getApiErrorMessage } from "../../api/apiError";

import { useAuth } from "../../features/authentication/AuthContext";

const shelterLoginSchema = z.object({
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

type ShelterLoginFormValues = z.infer<
  typeof shelterLoginSchema
>;

type ShelterLoginLocationState = {
  from?: {
    pathname?: string;
  };

  email?: string;
};

function getShelterDestination(
  pathname?: string,
): string {
  if (
    pathname &&
    pathname.startsWith("/shelter") &&
    pathname !== "/shelter/login"
  ) {
    return pathname;
  }

  return "/shelter";
}

export function ShelterLoginPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const {
    login,
    logout,
    status,
    canManageShelter,
  } = useAuth();

  const locationState =
    location.state as
      | ShelterLoginLocationState
      | null;

  const destination =
    getShelterDestination(
      locationState?.from?.pathname,
    );

  const previousEmail =
    locationState?.email ?? "";

  const [
    serverError,
    setServerError,
  ] = useState<string | null>(null);

  const [
    isSwitchingAccount,
    setIsSwitchingAccount,
  ] = useState(false);

  const {
    register,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<ShelterLoginFormValues>({
    resolver: zodResolver(
      shelterLoginSchema,
    ),
    defaultValues: {
      email: previousEmail,
      password: "",
    },
  });

  if (status === "loading") {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>Sitzung wird geprüft</h1>

          <p className="form-card__description">
            Deine Anmeldedaten und
            Tierheim-Berechtigungen werden
            überprüft …
          </p>
        </div>
      </section>
    );
  }

  if (
    status === "authenticated" &&
    canManageShelter
  ) {
    return (
      <Navigate
        to={destination}
        replace
      />
    );
  }

  async function handleSwitchAccount(): Promise<void> {
    setServerError(null);
    setIsSwitchingAccount(true);

    try {
      await logout();
    } catch {
      /*
       * Der AuthContext entfernt die lokale
       * Sitzung auch dann, wenn der Server beim
       * Abmelden nicht erreichbar ist.
       */
    } finally {
      setIsSwitchingAccount(false);
    }
  }

  if (
    status === "authenticated" &&
    !canManageShelter
  ) {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>
            Normales Benutzerkonto angemeldet
          </h1>

          <p className="form-card__description">
            Das aktuell angemeldete Konto besitzt
            keine Berechtigung für die
            Tierheim-Verwaltung.
          </p>

          <div
            className="form-alert form-alert--error"
            role="alert"
          >
            Für diesen Bereich wird ein
            freigeschaltetes Tierheimkonto
            benötigt.
          </div>

          <button
            className="button button--primary button--full"
            type="button"
            disabled={isSwitchingAccount}
            onClick={() =>
              void handleSwitchAccount()
            }
          >
            {isSwitchingAccount
              ? "Konto wird gewechselt …"
              : "Mit Tierheimkonto anmelden"}
          </button>

          <p className="form-card__footer">
            <Link
              className="text-link"
              to="/profile"
            >
              Zum aktuellen Benutzerkonto
            </Link>
          </p>
        </div>
      </section>
    );
  }

  async function onSubmit(
    values: ShelterLoginFormValues,
  ): Promise<void> {
    setServerError(null);

    try {
      const currentUser = await login({
        email: values.email.trim(),
        password: values.password,
      });

      const isAdmin =
        currentUser.roles.includes("Admin");

      const isShelterAdmin =
        currentUser.roles.includes(
          "ShelterAdmin",
        );

      const hasShelterAccess =
        isAdmin ||
        (
          isShelterAdmin &&
          currentUser.shelterId !== null
        );

      if (!hasShelterAccess) {
        try {
          await logout();
        } catch {
          /*
           * Die lokale Sitzung wurde bereits
           * durch den AuthContext entfernt.
           */
        }

        if (
          isShelterAdmin &&
          currentUser.shelterId === null
        ) {
          setServerError(
            "Dein Tierheimkonto wurde noch keinem Tierheim zugeordnet. Bitte wende dich an einen TierMatch-Administrator.",
          );

          return;
        }

        setServerError(
          "Dieses Konto besitzt keine Berechtigung für die Tierheim-Verwaltung. Bitte verwende ein freigeschaltetes Tierheimkonto.",
        );

        return;
      }

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
          TierMatch Shelter
        </p>

        <h1>Tierheim anmelden</h1>

        <p className="form-card__description">
          Melde dich mit deinem freigeschalteten
          Tierheimkonto an, um Tiere und
          Adoptionsanfragen zu verwalten.
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
            <label htmlFor="shelter-email">
              E-Mail-Adresse
            </label>

            <input
              id="shelter-email"
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
            <label htmlFor="shelter-password">
              Passwort
            </label>

            <input
              id="shelter-password"
              type="password"
              autoComplete="current-password"
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
              ? "Tierheim wird angemeldet …"
              : "Tierheim anmelden"}
          </button>
        </form>

        <p className="form-card__footer">
          Du suchst den normalen Login?{" "}
          <Link
            className="text-link"
            to="/login"
          >
            Zum Benutzer-Login
          </Link>
        </p>
      </div>
    </section>
  );
}
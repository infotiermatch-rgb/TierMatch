import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../../api/apiError";
import {
  submitShelterRegistration,
} from "../../features/shelterRegistrations/shelterRegistrationApi";

const shelterRegistrationSchema = z.object({
  shelterName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib den Namen des Tierheims ein.",
    )
    .max(
      150,
      "Der Name darf höchstens 150 Zeichen enthalten.",
    ),

  street: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib die Straße des Tierheims ein.",
    )
    .max(
      150,
      "Die Straße darf höchstens 150 Zeichen enthalten.",
    ),

  houseNumber: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib die Hausnummer ein.",
    )
    .max(
      20,
      "Die Hausnummer darf höchstens 20 Zeichen enthalten.",
    ),

  postalCode: z
    .string()
    .trim()
    .regex(
      /^\d{5}$/,
      "Bitte gib eine gültige fünfstellige Postleitzahl ein.",
    ),

  city: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib den Ort des Tierheims ein.",
    )
    .max(
      100,
      "Der Ort darf höchstens 100 Zeichen enthalten.",
    ),

  country: z
    .string()
    .trim()
    .length(
      2,
      "Bitte gib ein zweistelliges Länderkürzel ein.",
    ),

  shelterPhoneNumber: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib die Telefonnummer des Tierheims ein.",
    )
    .max(
      30,
      "Die Telefonnummer darf höchstens 30 Zeichen enthalten.",
    ),

  shelterEmail: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib die E-Mail-Adresse des Tierheims ein.",
    )
    .email(
      "Bitte gib eine gültige E-Mail-Adresse ein.",
    )
    .max(
      255,
      "Die E-Mail-Adresse darf höchstens 255 Zeichen enthalten.",
    ),

  website: z
    .string()
    .trim()
    .max(
      250,
      "Die Internetadresse darf höchstens 250 Zeichen enthalten.",
    ),

  description: z
    .string()
    .trim()
    .max(
      2000,
      "Die Beschreibung darf höchstens 2000 Zeichen enthalten.",
    ),

  contactFirstName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib den Vornamen der Ansprechperson ein.",
    )
    .max(
      100,
      "Der Vorname darf höchstens 100 Zeichen enthalten.",
    ),

  contactLastName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib den Nachnamen der Ansprechperson ein.",
    )
    .max(
      100,
      "Der Nachname darf höchstens 100 Zeichen enthalten.",
    ),

  contactEmail: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib die E-Mail-Adresse der Ansprechperson ein.",
    )
    .email(
      "Bitte gib eine gültige E-Mail-Adresse ein.",
    )
    .max(
      255,
      "Die E-Mail-Adresse darf höchstens 255 Zeichen enthalten.",
    ),

  contactPhoneNumber: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib die Telefonnummer der Ansprechperson ein.",
    )
    .max(
      30,
      "Die Telefonnummer darf höchstens 30 Zeichen enthalten.",
    ),

  message: z
    .string()
    .trim()
    .max(
      2000,
      "Die Nachricht darf höchstens 2000 Zeichen enthalten.",
    ),
});

type ShelterRegistrationFormValues = z.infer<
  typeof shelterRegistrationSchema
>;

export function ShelterRegisterPage() {
  const [serverError, setServerError] =
    useState<string | null>(null);

  const [
    createdRegistrationId,
    setCreatedRegistrationId,
  ] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<ShelterRegistrationFormValues>({
    resolver: zodResolver(
      shelterRegistrationSchema,
    ),
    defaultValues: {
      shelterName: "",
      street: "",
      houseNumber: "",
      postalCode: "",
      city: "",
      country: "DE",
      shelterPhoneNumber: "",
      shelterEmail: "",
      website: "",
      description: "",
      contactFirstName: "",
      contactLastName: "",
      contactEmail: "",
      contactPhoneNumber: "",
      message: "",
    },
  });

  async function onSubmit(
    values: ShelterRegistrationFormValues,
  ): Promise<void> {
    setServerError(null);

    try {
      const registrationId =
        await submitShelterRegistration({
          shelterName:
            values.shelterName.trim(),

          street:
            values.street.trim(),

          houseNumber:
            values.houseNumber.trim(),

          postalCode:
            values.postalCode.trim(),

          city:
            values.city.trim(),

          country:
            values.country
              .trim()
              .toUpperCase(),

          shelterPhoneNumber:
            values.shelterPhoneNumber.trim(),

          shelterEmail:
            values.shelterEmail.trim(),

          website:
            values.website.trim(),

          description:
            values.description.trim(),

          contactFirstName:
            values.contactFirstName.trim(),

          contactLastName:
            values.contactLastName.trim(),

          contactEmail:
            values.contactEmail.trim(),

          contactPhoneNumber:
            values.contactPhoneNumber.trim(),

          message:
            values.message.trim(),
        });

      setCreatedRegistrationId(
        registrationId,
      );
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Die Tierheimregistrierung konnte nicht übermittelt werden.",
        ),
      );
    }
  }

  if (createdRegistrationId) {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            Antrag übermittelt
          </p>

          <h1>
            Vielen Dank für die Registrierung
          </h1>

          <div
            className="form-alert form-alert--success"
            role="status"
          >
            Der Registrierungsantrag wurde
            erfolgreich an TierMatch übermittelt.
          </div>

          <p className="form-card__description">
            Der Antrag wird nun durch einen
            Administrator geprüft. Nach erfolgreicher
            Freigabe erhält die angegebene
            Ansprechperson eine E-Mail mit einem Link
            zur Einrichtung des Tierheimkontos.
          </p>

          <div className="adoption-request-reference">
            <span>Registrierungsnummer</span>

            <strong>
              {createdRegistrationId}
            </strong>
          </div>

          <div className="form-actions">
            <Link
              className="button button--outline"
              to="/"
            >
              Zur Startseite
            </Link>

            <Link
              className="button button--primary"
              to="/shelter/login"
            >
              Zum Tierheim-Login
            </Link>
          </div>
        </div>
      </section>
    );
  }

  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">
          Für Tierheime
        </p>

        <h1>
          Tierheim bei TierMatch registrieren
        </h1>

        <p className="form-card__description">
          Reiche die Daten deines Tierheims ein.
          Nach der Prüfung wird ein separates
          Verwaltungskonto erstellt. Über dieses
          Konto können später Tiere und
          Adoptionsanfragen verwaltet werden.
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
          <h2>Angaben zum Tierheim</h2>

          <div className="form-field">
            <label htmlFor="shelterName">
              Name des Tierheims
            </label>

            <input
              id="shelterName"
              type="text"
              autoComplete="organization"
              maxLength={150}
              aria-invalid={
                errors.shelterName
                  ? "true"
                  : "false"
              }
              {...register("shelterName")}
            />

            {errors.shelterName && (
              <p className="form-field__error">
                {errors.shelterName.message}
              </p>
            )}
          </div>

          <div className="form-grid">
            <div className="form-field">
              <label htmlFor="street">
                Straße
              </label>

              <input
                id="street"
                type="text"
                autoComplete="street-address"
                maxLength={150}
                aria-invalid={
                  errors.street
                    ? "true"
                    : "false"
                }
                {...register("street")}
              />

              {errors.street && (
                <p className="form-field__error">
                  {errors.street.message}
                </p>
              )}
            </div>

            <div className="form-field">
              <label htmlFor="houseNumber">
                Hausnummer
              </label>

              <input
                id="houseNumber"
                type="text"
                maxLength={20}
                aria-invalid={
                  errors.houseNumber
                    ? "true"
                    : "false"
                }
                {...register("houseNumber")}
              />

              {errors.houseNumber && (
                <p className="form-field__error">
                  {errors.houseNumber.message}
                </p>
              )}
            </div>
          </div>

          <div className="form-grid">
            <div className="form-field">
              <label htmlFor="postalCode">
                Postleitzahl
              </label>

              <input
                id="postalCode"
                type="text"
                inputMode="numeric"
                autoComplete="postal-code"
                maxLength={5}
                aria-invalid={
                  errors.postalCode
                    ? "true"
                    : "false"
                }
                {...register("postalCode")}
              />

              {errors.postalCode && (
                <p className="form-field__error">
                  {errors.postalCode.message}
                </p>
              )}
            </div>

            <div className="form-field">
              <label htmlFor="city">
                Ort
              </label>

              <input
                id="city"
                type="text"
                autoComplete="address-level2"
                maxLength={100}
                aria-invalid={
                  errors.city
                    ? "true"
                    : "false"
                }
                {...register("city")}
              />

              {errors.city && (
                <p className="form-field__error">
                  {errors.city.message}
                </p>
              )}
            </div>
          </div>

          <div className="form-grid">
            <div className="form-field">
              <label htmlFor="country">
                Länderkürzel
              </label>

              <input
                id="country"
                type="text"
                autoComplete="country"
                maxLength={2}
                aria-invalid={
                  errors.country
                    ? "true"
                    : "false"
                }
                {...register("country")}
              />

              {errors.country && (
                <p className="form-field__error">
                  {errors.country.message}
                </p>
              )}
            </div>

            <div className="form-field">
              <label htmlFor="shelterPhoneNumber">
                Telefonnummer
              </label>

              <input
                id="shelterPhoneNumber"
                type="tel"
                autoComplete="organization-tel"
                maxLength={30}
                aria-invalid={
                  errors.shelterPhoneNumber
                    ? "true"
                    : "false"
                }
                {...register(
                  "shelterPhoneNumber",
                )}
              />

              {errors.shelterPhoneNumber && (
                <p className="form-field__error">
                  {
                    errors.shelterPhoneNumber
                      .message
                  }
                </p>
              )}
            </div>
          </div>

          <div className="form-field">
            <label htmlFor="shelterEmail">
              E-Mail-Adresse des Tierheims
            </label>

            <input
              id="shelterEmail"
              type="email"
              autoComplete="organization-email"
              maxLength={255}
              aria-invalid={
                errors.shelterEmail
                  ? "true"
                  : "false"
              }
              {...register("shelterEmail")}
            />

            {errors.shelterEmail && (
              <p className="form-field__error">
                {errors.shelterEmail.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="website">
              Internetseite
            </label>

            <input
              id="website"
              type="url"
              placeholder="https://www.beispiel-tierheim.de"
              maxLength={250}
              aria-invalid={
                errors.website
                  ? "true"
                  : "false"
              }
              {...register("website")}
            />

            {errors.website && (
              <p className="form-field__error">
                {errors.website.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="description">
              Beschreibung des Tierheims
            </label>

            <textarea
              id="description"
              rows={6}
              maxLength={2000}
              placeholder="Beschreibe kurz das Tierheim, seine Arbeit und die betreuten Tiere."
              aria-invalid={
                errors.description
                  ? "true"
                  : "false"
              }
              {...register("description")}
            />

            {errors.description && (
              <p className="form-field__error">
                {errors.description.message}
              </p>
            )}
          </div>

          <h2>Ansprechperson</h2>

          <p className="form-card__description">
            An diese Person wird nach erfolgreicher
            Prüfung der Link zur Einrichtung des
            Verwaltungskontos gesendet.
          </p>

          <div className="form-grid">
            <div className="form-field">
              <label htmlFor="contactFirstName">
                Vorname
              </label>

              <input
                id="contactFirstName"
                type="text"
                autoComplete="given-name"
                maxLength={100}
                aria-invalid={
                  errors.contactFirstName
                    ? "true"
                    : "false"
                }
                {...register(
                  "contactFirstName",
                )}
              />

              {errors.contactFirstName && (
                <p className="form-field__error">
                  {
                    errors.contactFirstName
                      .message
                  }
                </p>
              )}
            </div>

            <div className="form-field">
              <label htmlFor="contactLastName">
                Nachname
              </label>

              <input
                id="contactLastName"
                type="text"
                autoComplete="family-name"
                maxLength={100}
                aria-invalid={
                  errors.contactLastName
                    ? "true"
                    : "false"
                }
                {...register(
                  "contactLastName",
                )}
              />

              {errors.contactLastName && (
                <p className="form-field__error">
                  {
                    errors.contactLastName
                      .message
                  }
                </p>
              )}
            </div>
          </div>

          <div className="form-field">
            <label htmlFor="contactEmail">
              E-Mail-Adresse
            </label>

            <input
              id="contactEmail"
              type="email"
              autoComplete="email"
              maxLength={255}
              aria-invalid={
                errors.contactEmail
                  ? "true"
                  : "false"
              }
              {...register("contactEmail")}
            />

            {errors.contactEmail && (
              <p className="form-field__error">
                {errors.contactEmail.message}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="contactPhoneNumber">
              Telefonnummer
            </label>

            <input
              id="contactPhoneNumber"
              type="tel"
              autoComplete="tel"
              maxLength={30}
              aria-invalid={
                errors.contactPhoneNumber
                  ? "true"
                  : "false"
              }
              {...register(
                "contactPhoneNumber",
              )}
            />

            {errors.contactPhoneNumber && (
              <p className="form-field__error">
                {
                  errors.contactPhoneNumber
                    .message
                }
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="message">
              Zusätzliche Nachricht
            </label>

            <textarea
              id="message"
              rows={6}
              maxLength={2000}
              placeholder="Hier können zusätzliche Informationen für die Prüfung eingetragen werden."
              aria-invalid={
                errors.message
                  ? "true"
                  : "false"
              }
              {...register("message")}
            />

            {errors.message && (
              <p className="form-field__error">
                {errors.message.message}
              </p>
            )}
          </div>

          <div className="form-actions">
            <Link
              className="button button--outline"
              to="/"
            >
              Abbrechen
            </Link>

            <button
              className="button button--primary"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting
                ? "Antrag wird übermittelt …"
                : "Registrierungsantrag senden"}
            </button>
          </div>
        </form>
      </div>
    </section>
  );
}
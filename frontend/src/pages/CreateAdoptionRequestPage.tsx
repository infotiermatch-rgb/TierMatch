import { zodResolver } from "@hookform/resolvers/zod";
import axios from "axios";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import {
  Link,
  useParams,
} from "react-router";
import { z } from "zod";

import { getApiErrorMessage } from "../api/apiError";
import { submitAdoptionRequest } from "../features/adoptionRequests/adoptionRequestApi";
import { getAnimalByIdRequest } from "../features/animals/animalApi";
import {
  formatAnimalAge,
  formatAnimalValue,
} from "../features/animals/animalFormatters";
import { useAuth } from "../features/authentication/AuthContext";

import type { AnimalDto } from "../types/animal";

const adoptionRequestSchema = z.object({
  firstName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib deinen Vornamen ein.",
    ),

  lastName: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib deinen Nachnamen ein.",
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

  phoneNumber: z
    .string()
    .trim()
    .min(
      1,
      "Bitte gib deine Telefonnummer ein.",
    ),

  message: z
    .string()
    .trim()
    .min(
      1,
      "Bitte schreibe eine kurze Nachricht an das Tierheim.",
    ),
});

type AdoptionRequestFormValues = z.infer<
  typeof adoptionRequestSchema
>;

type AnimalState =
  | {
      status: "loading";
    }
  | {
      status: "success";
      animal: AnimalDto;
    }
  | {
      status: "error";
      message: string;
    };

export function CreateAdoptionRequestPage() {
  const { id } = useParams<{
    id: string;
  }>();

  const { user } = useAuth();

  const [animalState, setAnimalState] =
    useState<AnimalState>({
      status: "loading",
    });

  const [serverError, setServerError] =
    useState<string | null>(null);

  const [createdRequestId, setCreatedRequestId] =
    useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: {
      errors,
      isSubmitting,
    },
  } = useForm<AdoptionRequestFormValues>({
    resolver: zodResolver(
      adoptionRequestSchema,
    ),
    defaultValues: {
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      email: user?.email ?? "",
      phoneNumber: "",
      message: "",
    },
  });

  useEffect(() => {
    if (!user) {
      return;
    }

    reset({
      firstName: user.firstName ?? "",
      lastName: user.lastName ?? "",
      email: user.email,
      phoneNumber: "",
      message: "",
    });
  }, [user, reset]);

  useEffect(() => {
    if (!id) {
      setAnimalState({
        status: "error",
        message:
          "Es wurde keine gültige Tier-ID angegeben.",
      });

      return;
    }

    const abortController =
      new AbortController();

    async function loadAnimal(
      animalId: string,
    ): Promise<void> {
      try {
        const animal =
          await getAnimalByIdRequest(
            animalId,
            abortController.signal,
          );

        if (abortController.signal.aborted) {
          return;
        }

        setAnimalState({
          status: "success",
          animal,
        });
      } catch (error: unknown) {
        if (
          axios.isCancel(error) ||
          abortController.signal.aborted
        ) {
          return;
        }

        setAnimalState({
          status: "error",
          message: getApiErrorMessage(
            error,
            "Das Tier konnte nicht geladen werden.",
          ),
        });
      }
    }

    void loadAnimal(id);

    return () => {
      abortController.abort();
    };
  }, [id]);

  if (!user) {
    return null;
  }

  if (animalState.status === "loading") {
    return (
      <section className="page page--narrow">
        <div
          className="animals-message"
          role="status"
        >
          <span className="animals-loader" />

          <div>
            <strong>
              Tierdaten werden geladen
            </strong>

            <p>
              Die Adoptionsanfrage wird
              vorbereitet …
            </p>
          </div>
        </div>
      </section>
    );
  }

  if (animalState.status === "error") {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            Adoptionsanfrage
          </p>

          <h1>
            Das Tier konnte nicht geladen werden
          </h1>

          <div
            className="form-alert form-alert--error"
            role="alert"
          >
            {animalState.message}
          </div>

          <Link
            className="button button--primary button--full"
            to="/animals"
          >
            Zur Tierübersicht
          </Link>
        </div>
      </section>
    );
  }

  const animal = animalState.animal;

  const animalIsAvailable =
    animal.status.toLowerCase() ===
    "available";

  async function onSubmit(
    values: AdoptionRequestFormValues,
  ): Promise<void> {
    const animalId = animal.id;

    setServerError(null);

    try {
      const requestId =
        await submitAdoptionRequest({
          animalId,
          firstName:
            values.firstName.trim(),
          lastName:
            values.lastName.trim(),
          email: values.email.trim(),
          phoneNumber:
            values.phoneNumber.trim(),
          message: values.message.trim(),
        });

      setCreatedRequestId(requestId);
    } catch (error: unknown) {
      setServerError(
        getApiErrorMessage(
          error,
          "Die Adoptionsanfrage konnte nicht versendet werden.",
        ),
      );
    }
  }

  if (createdRequestId) {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            Anfrage versendet
          </p>

          <h1>
            Vielen Dank für dein Interesse
          </h1>

          <div
            className="form-alert form-alert--success"
            role="status"
          >
            Deine Adoptionsanfrage für{" "}
            <strong>{animal.name}</strong>{" "}
            wurde erfolgreich übermittelt.
          </div>

          <p className="form-card__description">
            Das zuständige Tierheim kann deine
            Kontaktdaten und deine Nachricht nun
            prüfen.
          </p>

          <div className="adoption-request-reference">
            <span>Anfragenummer</span>

            <strong>{createdRequestId}</strong>
          </div>

          <div className="form-actions">
            <Link
              className="button button--outline"
              to="/animals"
            >
              Weitere Tiere ansehen
            </Link>

            <Link
              className="button button--primary"
              to={`/animals/${animal.id}`}
            >
              Zurück zu {animal.name}
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
          Adoptionsanfrage
        </p>

        <h1>
          Interesse an {animal.name}
        </h1>

        <div className="adoption-animal-summary">
          <div>
            <strong>{animal.name}</strong>

            <span>
              {formatAnimalValue(
                animal.species,
              )}{" "}
              · {animal.breed ||
                "Rasse unbekannt"}{" "}
              · {formatAnimalAge(
                animal.birthDate,
              )}
            </span>
          </div>

          <span className="adoption-animal-summary__status">
            {formatAnimalValue(
              animal.status,
            )}
          </span>
        </div>

        {!animalIsAvailable ? (
          <>
            <div
              className="form-alert form-alert--error"
              role="alert"
            >
              Dieses Tier steht derzeit nicht mehr
              zur Adoption.
            </div>

            <Link
              className="button button--primary button--full"
              to={`/animals/${animal.id}`}
            >
              Zurück zum Tierprofil
            </Link>
          </>
        ) : (
          <>
            <p className="form-card__description">
              Sende dem zuständigen Tierheim deine
              Kontaktdaten und eine kurze Nachricht.
              Das Absenden der Anfrage ist noch
              keine verbindliche Adoption.
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
                    {...register("firstName")}
                  />

                  {errors.firstName && (
                    <p className="form-field__error">
                      {
                        errors.firstName
                          .message
                      }
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
                      {
                        errors.lastName
                          .message
                      }
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
                  {...register("email")}
                />

                {errors.email && (
                  <p className="form-field__error">
                    {errors.email.message}
                  </p>
                )}
              </div>

              <div className="form-field">
                <label htmlFor="phoneNumber">
                  Telefonnummer
                </label>

                <input
                  id="phoneNumber"
                  type="tel"
                  autoComplete="tel"
                  aria-invalid={
                    errors.phoneNumber
                      ? "true"
                      : "false"
                  }
                  {...register(
                    "phoneNumber",
                  )}
                />

                {errors.phoneNumber && (
                  <p className="form-field__error">
                    {
                      errors.phoneNumber
                        .message
                    }
                  </p>
                )}
              </div>

              <div className="form-field">
                <label htmlFor="message">
                  Nachricht an das Tierheim
                </label>

                <textarea
                  id="message"
                  rows={7}
                  placeholder={`Erzähle dem Tierheim kurz, warum du dich für ${animal.name} interessierst und wie das zukünftige Zuhause aussehen würde.`}
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
                  to={`/animals/${animal.id}`}
                >
                  Abbrechen
                </Link>

                <button
                  className="button button--primary"
                  type="submit"
                  disabled={isSubmitting}
                >
                  {isSubmitting
                    ? "Anfrage wird gesendet …"
                    : "Adoptionsanfrage senden"}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </section>
  );
}
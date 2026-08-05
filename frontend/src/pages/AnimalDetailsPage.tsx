import axios from "axios";
import {
  useEffect,
  useMemo,
  useState,
} from "react";
import {
  Link,
  useParams,
} from "react-router";

import { getApiErrorMessage } from "../api/apiError";

import {
  getAnimalByIdRequest,
  getAnimalImagesRequest,
  resolveAnimalImageUrl,
  sortAnimalImages,
} from "../features/animals/animalApi";

import {
  formatAnimalAge,
  formatAnimalBirthDate,
  formatAnimalValue,
} from "../features/animals/animalFormatters";

import type {
  AnimalDto,
  AnimalImageDto,
} from "../types/animal";

type AnimalDetailsState =
  | {
      status: "loading";
    }
  | {
      status: "success";
      animal: AnimalDto;
      images: AnimalImageDto[];
      galleryError: string | null;
    }
  | {
      status: "error";
      message: string;
    };

function formatBoolean(
  value: boolean,
): string {
  return value ? "Ja" : "Nein";
}

export function AnimalDetailsPage() {
  const { id } = useParams<{
    id: string;
  }>();

  const [detailsState, setDetailsState] =
    useState<AnimalDetailsState>({
      status: "loading",
    });

  const [
    selectedImageId,
    setSelectedImageId,
  ] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      setDetailsState({
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

        let images: AnimalImageDto[] = [];
        let galleryError: string | null =
          null;

        try {
          images =
            await getAnimalImagesRequest(
              animalId,
              abortController.signal,
            );
        } catch (error: unknown) {
          if (
            axios.isCancel(error) ||
            abortController.signal.aborted
          ) {
            return;
          }

          galleryError =
            getApiErrorMessage(
              error,
              "Die Bilder konnten nicht geladen werden.",
            );
        }

        if (abortController.signal.aborted) {
          return;
        }

        setDetailsState({
          status: "success",
          animal,
          images,
          galleryError,
        });
      } catch (error: unknown) {
        if (
          axios.isCancel(error) ||
          abortController.signal.aborted
        ) {
          return;
        }

        setDetailsState({
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

  const orderedImages = useMemo(() => {
    if (
      detailsState.status !== "success"
    ) {
      return [];
    }

    return sortAnimalImages(
      detailsState.images,
    );
  }, [detailsState]);

  useEffect(() => {
    setSelectedImageId(
      orderedImages[0]?.id ?? null,
    );
  }, [orderedImages]);

  if (detailsState.status === "loading") {
    return (
      <section className="page">
        <div
          className="animals-message"
          role="status"
        >
          <span className="animals-loader" />

          <div>
            <strong>
              Tierprofil wird geladen
            </strong>

            <p>
              Die Informationen werden
              vorbereitet …
            </p>
          </div>
        </div>
      </section>
    );
  }

  if (detailsState.status === "error") {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            Tier nicht verfügbar
          </p>

          <h1>
            Das Tier konnte nicht geladen werden
          </h1>

          <div
            className="form-alert form-alert--error"
            role="alert"
          >
            {detailsState.message}
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

  const {
    animal,
    galleryError,
  } = detailsState;

  const selectedImage =
    orderedImages.find(
      (image) =>
        image.id === selectedImageId,
    ) ??
    orderedImages[0] ??
    null;

  const selectedImageUrl =
    resolveAnimalImageUrl(
      selectedImage?.url,
    );

  const animalIsAvailable =
    animal.status.toLowerCase() ===
    "available";

  return (
    <section className="page">
      <Link
        className="animal-details__back"
        to="/animals"
      >
        ← Zurück zur Tierübersicht
      </Link>

      <div className="animal-details">
        <div className="animal-gallery">
          <div className="animal-gallery__main">
            {selectedImageUrl ? (
              <img
                src={selectedImageUrl}
                alt={`${animal.name} – ${
                  selectedImage?.fileName ??
                  "Tierbild"
                }`}
              />
            ) : (
              <div className="animal-gallery__placeholder">
                <span aria-hidden="true">
                  🐾
                </span>

                <p>
                  Für {animal.name} ist noch kein
                  Bild verfügbar.
                </p>
              </div>
            )}

            <span className="animal-details__status">
              {formatAnimalValue(
                animal.status,
              )}
            </span>
          </div>

          {galleryError && (
            <div className="animal-gallery__warning">
              {galleryError}
            </div>
          )}

          {orderedImages.length > 1 && (
            <div
              className="animal-gallery__thumbnails"
              aria-label="Weitere Tierbilder"
            >
              {orderedImages.map((image) => {
                const thumbnailUrl =
                  resolveAnimalImageUrl(
                    image.url,
                  );

                if (!thumbnailUrl) {
                  return null;
                }

                const isSelected =
                  image.id ===
                  selectedImage?.id;

                return (
                  <button
                    key={image.id}
                    className={
                      isSelected
                        ? "animal-gallery__thumbnail animal-gallery__thumbnail--active"
                        : "animal-gallery__thumbnail"
                    }
                    type="button"
                    onClick={() =>
                      setSelectedImageId(
                        image.id,
                      )
                    }
                    aria-label={`${image.fileName} anzeigen`}
                    aria-pressed={isSelected}
                  >
                    <img
                      src={thumbnailUrl}
                      alt=""
                      loading="lazy"
                    />
                  </button>
                );
              })}
            </div>
          )}
        </div>

        <div className="animal-details__content">
          <p className="eyebrow">
            {formatAnimalValue(
              animal.species,
            )}
          </p>

          <h1>{animal.name}</h1>

          <p className="animal-details__subtitle">
            {animal.breed ||
              "Rasse unbekannt"}{" "}
            ·{" "}
            {formatAnimalAge(
              animal.birthDate,
            )}
          </p>

          {animal.shelterName && (
            <div className="animal-details__shelter">
              <span>Untergebracht bei</span>

              <strong>
                {animal.shelterName}
              </strong>
            </div>
          )}

          <dl className="animal-details__facts">
            <div>
              <dt>Tierart</dt>
              <dd>
                {formatAnimalValue(
                  animal.species,
                )}
              </dd>
            </div>

            <div>
              <dt>Geschlecht</dt>
              <dd>
                {formatAnimalValue(
                  animal.gender,
                )}
              </dd>
            </div>

            <div>
              <dt>Größe</dt>
              <dd>
                {formatAnimalValue(
                  animal.size,
                )}
              </dd>
            </div>

            <div>
              <dt>Geburtsdatum</dt>
              <dd>
                {formatAnimalBirthDate(
                  animal.birthDate,
                )}
              </dd>
            </div>

            <div>
              <dt>Geimpft</dt>
              <dd>
                {formatBoolean(
                  animal.isVaccinated,
                )}
              </dd>
            </div>

            <div>
              <dt>Kastriert</dt>
              <dd>
                {formatBoolean(
                  animal.isCastrated,
                )}
              </dd>
            </div>
          </dl>

          <section className="animal-details__description">
            <h2>Über {animal.name}</h2>

            <p>
              {animal.description ||
                "Für dieses Tier wurde noch keine ausführliche Beschreibung hinterlegt."}
            </p>
          </section>

          <section className="animal-details__adoption">
            <h2>
              Interesse an {animal.name}?
            </h2>

            {animalIsAvailable ? (
              <>
                <p>
                  Sende dem zuständigen Tierheim
                  deine Kontaktdaten und eine kurze
                  Nachricht.
                </p>

                <Link
                  className="button button--primary button--full"
                  to={`/animals/${animal.id}/adoption-request`}
                >
                  Adoptionsanfrage stellen
                </Link>
              </>
            ) : (
              <div className="form-alert form-alert--error">
                Dieses Tier steht derzeit nicht
                mehr zur Adoption.
              </div>
            )}
          </section>
        </div>
      </div>
    </section>
  );
}
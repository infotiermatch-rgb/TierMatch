import axios from "axios";
import {
  useEffect,
  useMemo,
  useState,
} from "react";

import { getApiErrorMessage } from "../api/apiError";
import { AnimalCard } from "../components/animals/AnimalCard";

import {
  getAnimalImagesRequest,
  getAnimalsRequest,
  getPrimaryAnimalImage,
  resolveAnimalImageUrl,
} from "../features/animals/animalApi";

import { formatAnimalValue } from "../features/animals/animalFormatters";

import type { AnimalDto } from "../types/animal";

type AnimalsState =
  | {
      status: "loading";
    }
  | {
      status: "success";
      animals: AnimalDto[];
      imageUrls: Record<string, string | null>;
    }
  | {
      status: "error";
      message: string;
    };

function getUniqueValues(
  animals: AnimalDto[],
  selector: (animal: AnimalDto) => string,
): string[] {
  return [
    ...new Set(
      animals
        .map(selector)
        .filter((value) => value.trim().length > 0),
    ),
  ].sort((first, second) =>
    formatAnimalValue(first).localeCompare(
      formatAnimalValue(second),
      "de",
    ),
  );
}

export function AnimalsPage() {
  const [animalsState, setAnimalsState] =
    useState<AnimalsState>({
      status: "loading",
    });

  const [searchTerm, setSearchTerm] =
    useState("");

  const [selectedSpecies, setSelectedSpecies] =
    useState("");

  const [selectedGender, setSelectedGender] =
    useState("");

  const [selectedSize, setSelectedSize] =
    useState("");

  const [selectedStatus, setSelectedStatus] =
    useState("");

  useEffect(() => {
    const abortController =
      new AbortController();

    async function loadAnimals(): Promise<void> {
      try {
        const animals =
          await getAnimalsRequest(
            abortController.signal,
          );

        const imageEntries = await Promise.all(
          animals.map(async (animal) => {
            try {
              const images =
                await getAnimalImagesRequest(
                  animal.id,
                  abortController.signal,
                );

              const primaryImage =
                getPrimaryAnimalImage(images);

              return [
                animal.id,
                resolveAnimalImageUrl(
                  primaryImage?.url,
                ),
              ] as const;
            } catch {
              return [
                animal.id,
                null,
              ] as const;
            }
          }),
        );

        if (abortController.signal.aborted) {
          return;
        }

        setAnimalsState({
          status: "success",
          animals,
          imageUrls:
            Object.fromEntries(imageEntries),
        });
      } catch (error: unknown) {
        if (
          axios.isCancel(error) ||
          abortController.signal.aborted
        ) {
          return;
        }

        setAnimalsState({
          status: "error",
          message: getApiErrorMessage(
            error,
            "Die Tiere konnten nicht geladen werden.",
          ),
        });
      }
    }

    void loadAnimals();

    return () => {
      abortController.abort();
    };
  }, []);

  const animals =
    animalsState.status === "success"
      ? animalsState.animals
      : [];

  const speciesOptions = useMemo(
    () =>
      getUniqueValues(
        animals,
        (animal) => animal.species,
      ),
    [animals],
  );

  const genderOptions = useMemo(
    () =>
      getUniqueValues(
        animals,
        (animal) => animal.gender,
      ),
    [animals],
  );

  const sizeOptions = useMemo(
    () =>
      getUniqueValues(
        animals,
        (animal) => animal.size,
      ),
    [animals],
  );

  const statusOptions = useMemo(
    () =>
      getUniqueValues(
        animals,
        (animal) => animal.status,
      ),
    [animals],
  );

  const filteredAnimals = useMemo(() => {
    const normalizedSearch =
      searchTerm.trim().toLocaleLowerCase("de");

    return animals
      .filter((animal) => {
        const matchesSearch =
          normalizedSearch.length === 0 ||
          [
            animal.name,
            animal.breed,
            animal.species,
            animal.shelterName ?? "",
          ].some((value) =>
            value
              .toLocaleLowerCase("de")
              .includes(normalizedSearch),
          );

        const matchesSpecies =
          !selectedSpecies ||
          animal.species === selectedSpecies;

        const matchesGender =
          !selectedGender ||
          animal.gender === selectedGender;

        const matchesSize =
          !selectedSize ||
          animal.size === selectedSize;

        const matchesStatus =
          !selectedStatus ||
          animal.status === selectedStatus;

        return (
          matchesSearch &&
          matchesSpecies &&
          matchesGender &&
          matchesSize &&
          matchesStatus
        );
      })
      .sort((first, second) =>
        first.name.localeCompare(
          second.name,
          "de",
        ),
      );
  }, [
    animals,
    searchTerm,
    selectedSpecies,
    selectedGender,
    selectedSize,
    selectedStatus,
  ]);

  const filtersAreActive =
    searchTerm.trim().length > 0 ||
    selectedSpecies.length > 0 ||
    selectedGender.length > 0 ||
    selectedSize.length > 0 ||
    selectedStatus.length > 0;

  function resetFilters(): void {
    setSearchTerm("");
    setSelectedSpecies("");
    setSelectedGender("");
    setSelectedSize("");
    setSelectedStatus("");
  }

  return (
    <section className="page">
      <div className="animals-page-header">
        <div>
          <p className="eyebrow">
            Tiervermittlung
          </p>

          <h1>Finde deinen neuen Begleiter</h1>

          <p className="page-description">
            Entdecke Tiere aus Tierheimen und
            erfahre mehr über ihre Eigenschaften,
            Bedürfnisse und Vorgeschichte.
          </p>
        </div>
      </div>

      {animalsState.status === "loading" && (
        <div
          className="animals-message"
          role="status"
        >
          <span className="animals-loader" />
          <div>
            <strong>Tiere werden geladen</strong>
            <p>
              Die TierMatch-Datenbank wird
              durchsucht …
            </p>
          </div>
        </div>
      )}

      {animalsState.status === "error" && (
        <div
          className="animals-message animals-message--error"
          role="alert"
        >
          <strong>
            Tiere konnten nicht geladen werden
          </strong>

          <p>{animalsState.message}</p>
        </div>
      )}

      {animalsState.status === "success" && (
        <>
          <div className="animal-filters">
            <div className="animal-filter animal-filter--search">
              <label htmlFor="animalSearch">
                Suche
              </label>

              <input
                id="animalSearch"
                type="search"
                placeholder="Name, Rasse oder Tierheim"
                value={searchTerm}
                onChange={(event) =>
                  setSearchTerm(
                    event.target.value,
                  )
                }
              />
            </div>

            <div className="animal-filter">
              <label htmlFor="speciesFilter">
                Tierart
              </label>

              <select
                id="speciesFilter"
                value={selectedSpecies}
                onChange={(event) =>
                  setSelectedSpecies(
                    event.target.value,
                  )
                }
              >
                <option value="">
                  Alle Tierarten
                </option>

                {speciesOptions.map((species) => (
                  <option
                    key={species}
                    value={species}
                  >
                    {formatAnimalValue(species)}
                  </option>
                ))}
              </select>
            </div>

            <div className="animal-filter">
              <label htmlFor="genderFilter">
                Geschlecht
              </label>

              <select
                id="genderFilter"
                value={selectedGender}
                onChange={(event) =>
                  setSelectedGender(
                    event.target.value,
                  )
                }
              >
                <option value="">
                  Alle Geschlechter
                </option>

                {genderOptions.map((gender) => (
                  <option
                    key={gender}
                    value={gender}
                  >
                    {formatAnimalValue(gender)}
                  </option>
                ))}
              </select>
            </div>

            <div className="animal-filter">
              <label htmlFor="sizeFilter">
                Größe
              </label>

              <select
                id="sizeFilter"
                value={selectedSize}
                onChange={(event) =>
                  setSelectedSize(
                    event.target.value,
                  )
                }
              >
                <option value="">
                  Alle Größen
                </option>

                {sizeOptions.map((size) => (
                  <option
                    key={size}
                    value={size}
                  >
                    {formatAnimalValue(size)}
                  </option>
                ))}
              </select>
            </div>

            <div className="animal-filter">
              <label htmlFor="statusFilter">
                Status
              </label>

              <select
                id="statusFilter"
                value={selectedStatus}
                onChange={(event) =>
                  setSelectedStatus(
                    event.target.value,
                  )
                }
              >
                <option value="">
                  Alle Status
                </option>

                {statusOptions.map((status) => (
                  <option
                    key={status}
                    value={status}
                  >
                    {formatAnimalValue(status)}
                  </option>
                ))}
              </select>
            </div>

            <button
              className="button button--outline animal-filters__reset"
              type="button"
              onClick={resetFilters}
              disabled={!filtersAreActive}
            >
              Filter zurücksetzen
            </button>
          </div>

          <div className="animals-results-header">
            <p>
              <strong>
                {filteredAnimals.length}
              </strong>{" "}
              {filteredAnimals.length === 1
                ? "Tier gefunden"
                : "Tiere gefunden"}
            </p>
          </div>

          {filteredAnimals.length > 0 ? (
            <div className="animal-grid">
              {filteredAnimals.map((animal) => (
                <AnimalCard
                  key={animal.id}
                  animal={animal}
                  imageUrl={
                    animalsState.imageUrls[
                      animal.id
                    ] ?? null
                  }
                />
              ))}
            </div>
          ) : (
            <div className="animals-empty-state">
              <span aria-hidden="true">🐾</span>

              <h2>
                Keine passenden Tiere gefunden
              </h2>

              <p>
                Ändere deine Suche oder setze die
                Filter zurück.
              </p>

              <button
                className="button button--primary"
                type="button"
                onClick={resetFilters}
              >
                Filter zurücksetzen
              </button>
            </div>
          )}
        </>
      )}
    </section>
  );
}
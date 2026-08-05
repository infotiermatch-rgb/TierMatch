import { useEffect, useState } from "react";
import { Link } from "react-router";

import {
  formatAnimalAge,
  formatAnimalValue,
} from "../../features/animals/animalFormatters";

import type { AnimalDto } from "../../types/animal";

type AnimalCardProps = {
  animal: AnimalDto;
  imageUrl: string | null;
};

export function AnimalCard({
  animal,
  imageUrl,
}: AnimalCardProps) {
  const [imageFailed, setImageFailed] =
    useState(false);

  useEffect(() => {
    setImageFailed(false);
  }, [imageUrl]);

  return (
    <article className="animal-card">
      <Link
        className="animal-card__image-link"
        to={`/animals/${animal.id}`}
        aria-label={`Details zu ${animal.name}`}
      >
        {imageUrl && !imageFailed ? (
          <img
            className="animal-card__image"
            src={imageUrl}
            alt={animal.name}
            loading="lazy"
            onError={() => setImageFailed(true)}
          />
        ) : (
          <div className="animal-card__placeholder">
            <span aria-hidden="true">🐾</span>
            <span>Kein Bild vorhanden</span>
          </div>
        )}

        <span className="animal-card__status">
          {formatAnimalValue(animal.status)}
        </span>
      </Link>

      <div className="animal-card__content">
        <div>
          <p className="animal-card__species">
            {formatAnimalValue(animal.species)}
          </p>

          <h2 className="animal-card__title">
            <Link to={`/animals/${animal.id}`}>
              {animal.name}
            </Link>
          </h2>
        </div>

        <p className="animal-card__breed">
          {animal.breed || "Rasse unbekannt"}
        </p>

        <div className="animal-card__facts">
          <span>
            {formatAnimalValue(animal.gender)}
          </span>

          <span>
            {formatAnimalValue(animal.size)}
          </span>

          <span>
            {formatAnimalAge(animal.birthDate)}
          </span>
        </div>

        {animal.shelterName && (
          <p className="animal-card__shelter">
            Tierheim: {animal.shelterName}
          </p>
        )}

        <p className="animal-card__description">
          {animal.description ||
            "Für dieses Tier wurde noch keine Beschreibung hinterlegt."}
        </p>

        <Link
          className="button button--primary button--full"
          to={`/animals/${animal.id}`}
        >
          Tier kennenlernen
        </Link>
      </div>
    </article>
  );
}
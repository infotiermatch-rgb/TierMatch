import { httpClient } from "../../api/httpClient";

import type {
  AnimalDto,
  AnimalImageDto,
} from "../../types/animal";

export async function getAnimalsRequest(
  signal?: AbortSignal,
): Promise<AnimalDto[]> {
  const response = await httpClient.get<AnimalDto[]>(
    "/api/v1/animals",
    {
      signal,
    },
  );

  return response.data;
}

export async function getAnimalByIdRequest(
  animalId: string,
  signal?: AbortSignal,
): Promise<AnimalDto> {
  const response = await httpClient.get<AnimalDto>(
    `/api/v1/animals/${animalId}`,
    {
      signal,
    },
  );

  return response.data;
}

export async function getAnimalImagesRequest(
  animalId: string,
  signal?: AbortSignal,
): Promise<AnimalImageDto[]> {
  const response = await httpClient.get<
    AnimalImageDto[]
  >(`/api/v1/animals/${animalId}/images`, {
    signal,
  });

  return response.data;
}

export function sortAnimalImages(
  images: AnimalImageDto[],
): AnimalImageDto[] {
  return [...images].sort((first, second) => {
    if (first.isPrimary !== second.isPrimary) {
      return first.isPrimary ? -1 : 1;
    }

    return first.sortOrder - second.sortOrder;
  });
}

export function getPrimaryAnimalImage(
  images: AnimalImageDto[],
): AnimalImageDto | null {
  return sortAnimalImages(images)[0] ?? null;
}

export function resolveAnimalImageUrl(
  imageUrl: string | null | undefined,
): string | null {
  if (!imageUrl?.trim()) {
    return null;
  }

  try {
    return new URL(
      imageUrl,
      import.meta.env.VITE_API_BASE_URL,
    ).toString();
  } catch {
    return null;
  }
}
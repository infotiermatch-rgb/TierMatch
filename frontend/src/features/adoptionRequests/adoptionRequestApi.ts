import { httpClient } from "../../api/httpClient";

import type {
  AdoptionRequestDto,
  CreateAdoptionRequestRequest,
} from "../../types/adoptionRequest";

export async function submitAdoptionRequest(
  request: CreateAdoptionRequestRequest,
): Promise<string> {
  const response = await httpClient.post<string>(
    "/api/v1/adoption-requests",
    request,
  );

  return response.data;
}

export async function getMyAdoptionRequests(
  signal?: AbortSignal,
): Promise<AdoptionRequestDto[]> {
  const response =
    await httpClient.get<AdoptionRequestDto[]>(
      "/api/v1/adoption-requests/me",
      {
        signal,
      },
    );

  return response.data;
}

/*
 * Gibt die für das angemeldete Verwaltungskonto
 * sichtbaren Adoptionsanfragen zurück.
 *
 * Admin:
 * - alle Anfragen
 *
 * ShelterAdmin:
 * - nur Anfragen des eigenen Tierheims
 */
export async function getShelterAdoptionRequests(
  signal?: AbortSignal,
): Promise<AdoptionRequestDto[]> {
  const response =
    await httpClient.get<AdoptionRequestDto[]>(
      "/api/v1/adoption-requests",
      {
        signal,
      },
    );

  return response.data;
}

/*
 * Gibt eine einzelne verwaltbare Adoptionsanfrage
 * zurück.
 */
export async function getShelterAdoptionRequestById(
  id: string,
  signal?: AbortSignal,
): Promise<AdoptionRequestDto> {
  const response =
    await httpClient.get<AdoptionRequestDto>(
      `/api/v1/adoption-requests/${encodeURIComponent(id)}`,
      {
        signal,
      },
    );

  return response.data;
}

/*
 * Genehmigt eine offene Adoptionsanfrage.
 *
 * Das Backend reserviert dabei das Tier und lehnt
 * weitere offene Anfragen für dasselbe Tier ab.
 */
export async function approveAdoptionRequest(
  id: string,
): Promise<void> {
  await httpClient.patch<void>(
    `/api/v1/adoption-requests/${encodeURIComponent(id)}/approve`,
  );
}

/*
 * Lehnt eine offene Adoptionsanfrage ab.
 */
export async function rejectAdoptionRequest(
  id: string,
): Promise<void> {
  await httpClient.patch<void>(
    `/api/v1/adoption-requests/${encodeURIComponent(id)}/reject`,
  );
}
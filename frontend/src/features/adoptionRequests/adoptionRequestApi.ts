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
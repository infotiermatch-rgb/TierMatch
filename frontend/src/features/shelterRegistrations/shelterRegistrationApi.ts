import { httpClient } from "../../api/httpClient";

import type {
  CreateShelterRegistrationRequest,
} from "../../types/shelterRegistration";

/*
 * Reicht einen öffentlichen Registrierungsantrag
 * für ein Tierheim ein.
 *
 * Das Backend speichert den Antrag zunächst mit
 * dem Status Pending. Ein Benutzerkonto entsteht
 * erst nach der Freigabe durch einen Administrator.
 */
export async function submitShelterRegistration(
  request: CreateShelterRegistrationRequest,
): Promise<string> {
  const response = await httpClient.post<string>(
    "/api/v1/shelter-registrations",
    request,
  );

  return response.data;
}
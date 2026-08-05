export type CreateAdoptionRequestRequest = {
  animalId: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  message: string;
};

/*
 * ASP.NET Core kann Enums abhängig von der
 * JSON-Konfiguration als Zahl oder Text ausgeben.
 */
export type AdoptionRequestStatus =
  | number
  | string;

export type AdoptionRequestDto = {
  id: string;
  animalId: string;
  animalName: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  message: string;
  status: AdoptionRequestStatus;
  requestedAt: string;
};
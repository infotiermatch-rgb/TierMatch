import type { AdoptionRequestStatus } from "../../types/adoptionRequest";

export type AdoptionRequestStatusKey =
  | "pending"
  | "approved"
  | "rejected"
  | "unknown";

export type AdoptionRequestStatusInfo = {
  key: AdoptionRequestStatusKey;
  label: string;
  className: string;
};

export function getAdoptionRequestStatusKey(
  status: AdoptionRequestStatus,
): AdoptionRequestStatusKey {
  const normalizedStatus = String(status)
    .trim()
    .toLowerCase();

  if (
    status === 0 ||
    normalizedStatus === "0" ||
    normalizedStatus === "pending"
  ) {
    return "pending";
  }

  if (
    status === 1 ||
    normalizedStatus === "1" ||
    normalizedStatus === "approved"
  ) {
    return "approved";
  }

  if (
    status === 2 ||
    normalizedStatus === "2" ||
    normalizedStatus === "rejected"
  ) {
    return "rejected";
  }

  return "unknown";
}

export function getAdoptionRequestStatusInfo(
  status: AdoptionRequestStatus,
): AdoptionRequestStatusInfo {
  const key =
    getAdoptionRequestStatusKey(status);

  switch (key) {
    case "pending":
      return {
        key,
        label: "Offen",
        className:
          "adoption-status adoption-status--pending",
      };

    case "approved":
      return {
        key,
        label: "Genehmigt",
        className:
          "adoption-status adoption-status--approved",
      };

    case "rejected":
      return {
        key,
        label: "Abgelehnt",
        className:
          "adoption-status adoption-status--rejected",
      };

    default:
      return {
        key: "unknown",
        label: "Unbekannter Status",
        className:
          "adoption-status adoption-status--unknown",
      };
  }
}

export function formatAdoptionRequestDate(
  value: string,
): string {
  const parsedDate = new Date(value);

  if (Number.isNaN(parsedDate.getTime())) {
    return "Datum unbekannt";
  }

  return new Intl.DateTimeFormat("de-DE", {
    dateStyle: "long",
    timeStyle: "short",
  }).format(parsedDate);
}
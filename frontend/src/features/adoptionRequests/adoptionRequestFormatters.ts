import type { AdoptionRequestStatus } from "../../types/adoptionRequest";

export type AdoptionRequestStatusInfo = {
  label: string;
  className: string;
};

export function getAdoptionRequestStatusInfo(
  status: AdoptionRequestStatus,
): AdoptionRequestStatusInfo {
  if (
    status === 0 ||
    String(status).toLowerCase() === "pending"
  ) {
    return {
      label: "Offen",
      className:
        "adoption-status adoption-status--pending",
    };
  }

  if (
    status === 1 ||
    String(status).toLowerCase() === "approved"
  ) {
    return {
      label: "Genehmigt",
      className:
        "adoption-status adoption-status--approved",
    };
  }

  if (
    status === 2 ||
    String(status).toLowerCase() === "rejected"
  ) {
    return {
      label: "Abgelehnt",
      className:
        "adoption-status adoption-status--rejected",
    };
  }

  return {
    label: "Unbekannter Status",
    className:
      "adoption-status adoption-status--unknown",
  };
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
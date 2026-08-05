import axios from "axios";

export type ApiProblemDetails = {
  status?: number;
  title?: string;
  detail?: string;
  message?: string;
  errors?: Record<string, string[]> | string[] | null;
  timestamp?: string;
  traceId?: string;
};

function extractValidationMessages(
  errors: ApiProblemDetails["errors"],
): string[] {
  if (!errors) {
    return [];
  }

  if (Array.isArray(errors)) {
    return errors.filter(
      (message): message is string =>
        typeof message === "string",
    );
  }

  return Object.values(errors).flatMap((messages) =>
    Array.isArray(messages)
      ? messages.filter(
          (message): message is string =>
            typeof message === "string",
        )
      : [],
  );
}

export function getApiErrorMessage(
  error: unknown,
  fallbackMessage =
    "Die Anfrage konnte nicht verarbeitet werden.",
): string {
  if (!axios.isAxiosError(error)) {
    return fallbackMessage;
  }

  if (!error.response) {
    return "Die TierMatch-API konnte nicht erreicht werden.";
  }

  const responseData: unknown = error.response.data;

  if (typeof responseData === "string") {
    return responseData.trim() || fallbackMessage;
  }

  if (
    !responseData ||
    typeof responseData !== "object"
  ) {
    return fallbackMessage;
  }

  const problemDetails =
    responseData as ApiProblemDetails;

  const validationMessages =
    extractValidationMessages(
      problemDetails.errors,
    );

  if (validationMessages.length > 0) {
    return validationMessages.join(" ");
  }

  return (
    problemDetails.detail ??
    problemDetails.message ??
    problemDetails.title ??
    fallbackMessage
  );
}
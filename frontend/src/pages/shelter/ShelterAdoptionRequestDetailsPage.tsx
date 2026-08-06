import axios from "axios";
import {
  useEffect,
  useState,
} from "react";
import {
  Link,
  useParams,
} from "react-router";

import { getApiErrorMessage } from "../../api/apiError";

import {
  approveAdoptionRequest,
  getShelterAdoptionRequestById,
  rejectAdoptionRequest,
} from "../../features/adoptionRequests/adoptionRequestApi";

import {
  formatAdoptionRequestDate,
  getAdoptionRequestStatusInfo,
} from "../../features/adoptionRequests/adoptionRequestFormatters";

import type { AdoptionRequestDto } from "../../types/adoptionRequest";

type RequestDetailsState =
  | {
      status: "loading";
    }
  | {
      status: "success";
      request: AdoptionRequestDto;
    }
  | {
      status: "error";
      message: string;
    };

type RequestAction =
  | "approve"
  | "reject"
  | null;

export function ShelterAdoptionRequestDetailsPage() {
  const { id } = useParams<{
    id: string;
  }>();

  const [
    detailsState,
    setDetailsState,
  ] = useState<RequestDetailsState>({
    status: "loading",
  });

  const [
    activeAction,
    setActiveAction,
  ] = useState<RequestAction>(null);

  const [
    actionError,
    setActionError,
  ] = useState<string | null>(null);

  useEffect(() => {
    if (!id) {
      setDetailsState({
        status: "error",
        message:
          "Es wurde keine gültige Anfrage-ID angegeben.",
      });

      return;
    }

    const abortController =
      new AbortController();

    async function loadRequest(
      requestId: string,
    ): Promise<void> {
      try {
        const request =
          await getShelterAdoptionRequestById(
            requestId,
            abortController.signal,
          );

        if (abortController.signal.aborted) {
          return;
        }

        setDetailsState({
          status: "success",
          request,
        });
      } catch (error: unknown) {
        if (
          axios.isCancel(error) ||
          abortController.signal.aborted
        ) {
          return;
        }

        setDetailsState({
          status: "error",
          message: getApiErrorMessage(
            error,
            "Die Adoptionsanfrage konnte nicht geladen werden.",
          ),
        });
      }
    }

    void loadRequest(id);

    return () => {
      abortController.abort();
    };
  }, [id]);

  async function reloadRequest(): Promise<void> {
    if (!id) {
      return;
    }

    const request =
      await getShelterAdoptionRequestById(id);

    setDetailsState({
      status: "success",
      request,
    });
  }

  async function handleApprove(): Promise<void> {
    if (
      detailsState.status !== "success"
    ) {
      return;
    }

    const request = detailsState.request;

    const confirmed = window.confirm(
      `Möchtest du die Adoptionsanfrage für ${request.animalName} wirklich genehmigen?\n\nDas Tier wird reserviert und weitere offene Anfragen für dieses Tier werden abgelehnt.`,
    );

    if (!confirmed) {
      return;
    }

    setActionError(null);
    setActiveAction("approve");

    try {
      await approveAdoptionRequest(request.id);
      await reloadRequest();
    } catch (error: unknown) {
      setActionError(
        getApiErrorMessage(
          error,
          "Die Adoptionsanfrage konnte nicht genehmigt werden.",
        ),
      );
    } finally {
      setActiveAction(null);
    }
  }

  async function handleReject(): Promise<void> {
    if (
      detailsState.status !== "success"
    ) {
      return;
    }

    const request = detailsState.request;

    const confirmed = window.confirm(
      `Möchtest du die Adoptionsanfrage von ${request.firstName} ${request.lastName} wirklich ablehnen?`,
    );

    if (!confirmed) {
      return;
    }

    setActionError(null);
    setActiveAction("reject");

    try {
      await rejectAdoptionRequest(request.id);
      await reloadRequest();
    } catch (error: unknown) {
      setActionError(
        getApiErrorMessage(
          error,
          "Die Adoptionsanfrage konnte nicht abgelehnt werden.",
        ),
      );
    } finally {
      setActiveAction(null);
    }
  }

  if (detailsState.status === "loading") {
    return (
      <section className="page">
        <div
          className="animals-message"
          role="status"
        >
          <span className="animals-loader" />

          <div>
            <strong>
              Adoptionsanfrage wird geladen
            </strong>

            <p>
              Die Anfrage und ihre Kontaktdaten
              werden abgerufen …
            </p>
          </div>
        </div>
      </section>
    );
  }

  if (detailsState.status === "error") {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>
            Anfrage nicht verfügbar
          </h1>

          <div
            className="form-alert form-alert--error"
            role="alert"
          >
            {detailsState.message}
          </div>

          <Link
            className="button button--primary button--full"
            to="/shelter/adoption-requests"
          >
            Zur Anfragenübersicht
          </Link>
        </div>
      </section>
    );
  }

  const request = detailsState.request;

  const statusInfo =
    getAdoptionRequestStatusInfo(
      request.status,
    );

  const isPending =
    statusInfo.key === "pending";

  return (
    <section className="page">
      <Link
        className="shelter-request-details__back"
        to="/shelter/adoption-requests"
      >
        ← Zurück zu den Adoptionsanfragen
      </Link>

      <div className="shelter-request-details__header">
        <div>
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>
            Adoptionsanfrage für{" "}
            {request.animalName ||
              "unbekanntes Tier"}
          </h1>

          <p className="page-description">
            Eingegangen am{" "}
            {formatAdoptionRequestDate(
              request.requestedAt,
            )}
          </p>
        </div>

        <span className={statusInfo.className}>
          {statusInfo.label}
        </span>
      </div>

      {actionError && (
        <div
          className="animals-message animals-message--error"
          role="alert"
        >
          <div>
            <strong>
              Aktion konnte nicht ausgeführt
              werden
            </strong>

            <p>{actionError}</p>
          </div>
        </div>
      )}

      <div className="shelter-request-details__layout">
        <div className="shelter-request-details__main">
          <section className="shelter-request-details__card">
            <p className="eyebrow">
              Interessent
            </p>

            <h2>
              {request.firstName}{" "}
              {request.lastName}
            </h2>

            <dl className="adoption-request-contact">
              <div>
                <dt>E-Mail</dt>

                <dd>
                  <a
                    href={`mailto:${request.email}`}
                  >
                    {request.email}
                  </a>
                </dd>
              </div>

              <div>
                <dt>Telefon</dt>

                <dd>
                  {request.phoneNumber ? (
                    <a
                      href={`tel:${request.phoneNumber}`}
                    >
                      {request.phoneNumber}
                    </a>
                  ) : (
                    "Nicht angegeben"
                  )}
                </dd>
              </div>
            </dl>

            <div className="shelter-request-details__contact-actions">
              <a
                className="button button--outline"
                href={`mailto:${request.email}`}
              >
                E-Mail schreiben
              </a>

              {request.phoneNumber && (
                <a
                  className="button button--outline"
                  href={`tel:${request.phoneNumber}`}
                >
                  Anrufen
                </a>
              )}
            </div>
          </section>

          <section className="shelter-request-details__card">
            <p className="eyebrow">
              Nachricht
            </p>

            <h2>
              Nachricht des Interessenten
            </h2>

            <p className="shelter-request-details__message">
              {request.message ||
                "Es wurde keine Nachricht hinterlegt."}
            </p>
          </section>
        </div>

        <aside className="shelter-request-details__sidebar">
          <section className="shelter-request-details__card">
            <p className="eyebrow">
              Tier
            </p>

            <h2>
              {request.animalName ||
                "Unbekanntes Tier"}
            </h2>

            <p>
              Öffne das öffentliche Tierprofil,
              um die vollständigen Angaben und
              Bilder anzusehen.
            </p>

            <Link
              className="button button--outline button--full"
              to={`/animals/${request.animalId}`}
            >
              Tierprofil ansehen
            </Link>
          </section>

          <section className="shelter-request-details__card">
            <p className="eyebrow">
              Anfrage
            </p>

            <h2>Informationen</h2>

            <dl className="shelter-request-details__facts">
              <div>
                <dt>Status</dt>
                <dd>{statusInfo.label}</dd>
              </div>

              <div>
                <dt>Eingegangen</dt>
                <dd>
                  {formatAdoptionRequestDate(
                    request.requestedAt,
                  )}
                </dd>
              </div>

              <div>
                <dt>Anfrage-ID</dt>
                <dd>
                  <code>{request.id}</code>
                </dd>
              </div>

              <div>
                <dt>Tier-ID</dt>
                <dd>
                  <code>{request.animalId}</code>
                </dd>
              </div>
            </dl>
          </section>

          {isPending && (
            <section className="shelter-request-details__card shelter-request-details__decision">
              <p className="eyebrow">
                Entscheidung
              </p>

              <h2>Anfrage bearbeiten</h2>

              <p>
                Die Entscheidung kann nach der
                Bearbeitung derzeit nicht wieder
                zurückgesetzt werden.
              </p>

              <button
                className="button button--primary button--full"
                type="button"
                disabled={activeAction !== null}
                onClick={() =>
                  void handleApprove()
                }
              >
                {activeAction === "approve"
                  ? "Wird genehmigt …"
                  : "Anfrage genehmigen"}
              </button>

              <button
                className="button button--danger button--full"
                type="button"
                disabled={activeAction !== null}
                onClick={() =>
                  void handleReject()
                }
              >
                {activeAction === "reject"
                  ? "Wird abgelehnt …"
                  : "Anfrage ablehnen"}
              </button>
            </section>
          )}
        </aside>
      </div>
    </section>
  );
}
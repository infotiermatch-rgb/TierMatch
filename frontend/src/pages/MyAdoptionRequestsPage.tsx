import axios from "axios";
import {
  useEffect,
  useMemo,
  useState,
} from "react";
import { Link } from "react-router";

import { getApiErrorMessage } from "../api/apiError";

import {
  getMyAdoptionRequests,
} from "../features/adoptionRequests/adoptionRequestApi";

import {
  formatAdoptionRequestDate,
  getAdoptionRequestStatusInfo,
} from "../features/adoptionRequests/adoptionRequestFormatters";

import type { AdoptionRequestDto } from "../types/adoptionRequest";

type AdoptionRequestsState =
  | {
      status: "loading";
    }
  | {
      status: "success";
      requests: AdoptionRequestDto[];
    }
  | {
      status: "error";
      message: string;
    };

export function MyAdoptionRequestsPage() {
  const [requestsState, setRequestsState] =
    useState<AdoptionRequestsState>({
      status: "loading",
    });

  useEffect(() => {
    const abortController =
      new AbortController();

    async function loadRequests(): Promise<void> {
      try {
        const requests =
          await getMyAdoptionRequests(
            abortController.signal,
          );

        if (abortController.signal.aborted) {
          return;
        }

        setRequestsState({
          status: "success",
          requests,
        });
      } catch (error: unknown) {
        if (
          axios.isCancel(error) ||
          abortController.signal.aborted
        ) {
          return;
        }

        setRequestsState({
          status: "error",
          message: getApiErrorMessage(
            error,
            "Deine Adoptionsanfragen konnten nicht geladen werden.",
          ),
        });
      }
    }

    void loadRequests();

    return () => {
      abortController.abort();
    };
  }, []);

  const sortedRequests = useMemo(() => {
    if (requestsState.status !== "success") {
      return [];
    }

    return [...requestsState.requests].sort(
      (first, second) => {
        const firstDate = new Date(
          first.requestedAt,
        ).getTime();

        const secondDate = new Date(
          second.requestedAt,
        ).getTime();

        return secondDate - firstDate;
      },
    );
  }, [requestsState]);

  return (
    <section className="page">
      <div className="my-requests-header">
        <div>
          <p className="eyebrow">
            Dein TierMatch-Konto
          </p>

          <h1>Meine Adoptionsanfragen</h1>

          <p className="page-description">
            Hier findest du deine bisherigen
            Anfragen und ihren aktuellen
            Bearbeitungsstatus.
          </p>
        </div>

        <Link
          className="button button--primary"
          to="/animals"
        >
          Weitere Tiere entdecken
        </Link>
      </div>

      {requestsState.status === "loading" && (
        <div
          className="animals-message"
          role="status"
        >
          <span className="animals-loader" />

          <div>
            <strong>
              Adoptionsanfragen werden geladen
            </strong>

            <p>
              Deine bisherigen Anfragen werden
              abgerufen …
            </p>
          </div>
        </div>
      )}

      {requestsState.status === "error" && (
        <div
          className="animals-message animals-message--error"
          role="alert"
        >
          <div>
            <strong>
              Adoptionsanfragen konnten nicht
              geladen werden
            </strong>

            <p>{requestsState.message}</p>
          </div>
        </div>
      )}

      {requestsState.status === "success" &&
        sortedRequests.length === 0 && (
          <div className="my-requests-empty">
            <span aria-hidden="true">🐾</span>

            <h2>
              Noch keine Adoptionsanfragen
            </h2>

            <p>
              Du hast bislang noch keine Anfrage
              für ein Tier gestellt.
            </p>

            <Link
              className="button button--primary"
              to="/animals"
            >
              Tiere entdecken
            </Link>
          </div>
        )}

      {requestsState.status === "success" &&
        sortedRequests.length > 0 && (
          <>
            <div className="my-requests-summary">
              <p>
                <strong>
                  {sortedRequests.length}
                </strong>{" "}
                {sortedRequests.length === 1
                  ? "Adoptionsanfrage"
                  : "Adoptionsanfragen"}
              </p>
            </div>

            <div className="my-requests-list">
              {sortedRequests.map((request) => {
                const statusInfo =
                  getAdoptionRequestStatusInfo(
                    request.status,
                  );

                return (
                  <article
                    key={request.id}
                    className="adoption-request-card"
                  >
                    <div className="adoption-request-card__header">
                      <div>
                        <p className="adoption-request-card__label">
                          Adoptionsanfrage
                        </p>

                        <h2>
                          {request.animalName ||
                            "Unbekanntes Tier"}
                        </h2>

                        <p className="adoption-request-card__date">
                          Eingereicht am{" "}
                          {formatAdoptionRequestDate(
                            request.requestedAt,
                          )}
                        </p>
                      </div>

                      <span
                        className={
                          statusInfo.className
                        }
                      >
                        {statusInfo.label}
                      </span>
                    </div>

                    <div className="adoption-request-card__content">
                      <section>
                        <h3>Deine Nachricht</h3>

                        <p className="adoption-request-card__message">
                          {request.message ||
                            "Keine Nachricht hinterlegt."}
                        </p>
                      </section>

                      <section>
                        <h3>Kontaktdaten</h3>

                        <dl className="adoption-request-contact">
                          <div>
                            <dt>Name</dt>

                            <dd>
                              {request.firstName}{" "}
                              {request.lastName}
                            </dd>
                          </div>

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
                                  {
                                    request.phoneNumber
                                  }
                                </a>
                              ) : (
                                "Nicht angegeben"
                              )}
                            </dd>
                          </div>
                        </dl>
                      </section>
                    </div>

                    <div className="adoption-request-card__footer">
                      <span className="adoption-request-card__reference">
                        Anfrage-ID: {request.id}
                      </span>

                      <Link
                        className="button button--outline"
                        to={`/animals/${request.animalId}`}
                      >
                        Tierprofil ansehen
                      </Link>
                    </div>
                  </article>
                );
              })}
            </div>
          </>
        )}
    </section>
  );
}
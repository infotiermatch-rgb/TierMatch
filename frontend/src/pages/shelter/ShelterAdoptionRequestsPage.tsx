import axios from "axios";
import {
  useEffect,
  useMemo,
  useState,
} from "react";
import { Link } from "react-router";

import { getApiErrorMessage } from "../../api/apiError";

import {
  approveAdoptionRequest,
  getShelterAdoptionRequests,
  rejectAdoptionRequest,
} from "../../features/adoptionRequests/adoptionRequestApi";

import {
  formatAdoptionRequestDate,
  getAdoptionRequestStatusInfo,
  getAdoptionRequestStatusKey,
} from "../../features/adoptionRequests/adoptionRequestFormatters";

import type { AdoptionRequestDto } from "../../types/adoptionRequest";

type RequestFilter =
  | "all"
  | "pending"
  | "approved"
  | "rejected";

type RequestsState =
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

type RequestAction =
  | {
      requestId: string;
      type: "approve" | "reject";
    }
  | null;

type RequestCounts = Record<
  RequestFilter,
  number
>;

const FILTER_OPTIONS: ReadonlyArray<{
  value: RequestFilter;
  label: string;
}> = [
  {
    value: "all",
    label: "Alle",
  },
  {
    value: "pending",
    label: "Offen",
  },
  {
    value: "approved",
    label: "Genehmigt",
  },
  {
    value: "rejected",
    label: "Abgelehnt",
  },
];

function getRequestTimestamp(
  value: string,
): number {
  const timestamp = new Date(value).getTime();

  return Number.isNaN(timestamp)
    ? 0
    : timestamp;
}

export function ShelterAdoptionRequestsPage() {
  const [
    requestsState,
    setRequestsState,
  ] = useState<RequestsState>({
    status: "loading",
  });

  const [
    selectedFilter,
    setSelectedFilter,
  ] = useState<RequestFilter>("all");

  const [
    searchTerm,
    setSearchTerm,
  ] = useState("");

  const [
    activeAction,
    setActiveAction,
  ] = useState<RequestAction>(null);

  const [
    actionError,
    setActionError,
  ] = useState<string | null>(null);

  useEffect(() => {
    const abortController =
      new AbortController();

    async function loadRequests(): Promise<void> {
      try {
        const requests =
          await getShelterAdoptionRequests(
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
            "Die Adoptionsanfragen konnten nicht geladen werden.",
          ),
        });
      }
    }

    void loadRequests();

    return () => {
      abortController.abort();
    };
  }, []);

  const requestCounts =
    useMemo<RequestCounts>(() => {
      const counts: RequestCounts = {
        all: 0,
        pending: 0,
        approved: 0,
        rejected: 0,
      };

      if (
        requestsState.status !== "success"
      ) {
        return counts;
      }

      for (
        const request of
        requestsState.requests
      ) {
        const status =
          getAdoptionRequestStatusKey(
            request.status,
          );

        counts.all += 1;

        if (
          status === "pending" ||
          status === "approved" ||
          status === "rejected"
        ) {
          counts[status] += 1;
        }
      }

      return counts;
    }, [requestsState]);

  const visibleRequests = useMemo(() => {
    if (
      requestsState.status !== "success"
    ) {
      return [];
    }

    const normalizedSearchTerm =
      searchTerm.trim().toLowerCase();

    return [...requestsState.requests]
      .filter((request) => {
        const requestStatus =
          getAdoptionRequestStatusKey(
            request.status,
          );

        if (
          selectedFilter !== "all" &&
          requestStatus !== selectedFilter
        ) {
          return false;
        }

        if (!normalizedSearchTerm) {
          return true;
        }

        const searchableContent = [
          request.animalName,
          request.firstName,
          request.lastName,
          request.email,
          request.phoneNumber,
          request.message,
          request.id,
          request.animalId,
        ]
          .join(" ")
          .toLowerCase();

        return searchableContent.includes(
          normalizedSearchTerm,
        );
      })
      .sort(
        (first, second) =>
          getRequestTimestamp(
            second.requestedAt,
          ) -
          getRequestTimestamp(
            first.requestedAt,
          ),
      );
  }, [
    requestsState,
    searchTerm,
    selectedFilter,
  ]);

  async function reloadRequests(): Promise<void> {
    const requests =
      await getShelterAdoptionRequests();

    setRequestsState({
      status: "success",
      requests,
    });
  }

  async function handleApprove(
    request: AdoptionRequestDto,
  ): Promise<void> {
    const confirmed = window.confirm(
      `Möchtest du die Adoptionsanfrage für ${
        request.animalName ||
        "dieses Tier"
      } wirklich genehmigen?\n\nDas Tier wird reserviert und weitere offene Anfragen für dieses Tier werden abgelehnt.`,
    );

    if (!confirmed) {
      return;
    }

    setActionError(null);

    setActiveAction({
      requestId: request.id,
      type: "approve",
    });

    try {
      await approveAdoptionRequest(
        request.id,
      );
    } catch (error: unknown) {
      setActionError(
        getApiErrorMessage(
          error,
          "Die Adoptionsanfrage konnte nicht genehmigt werden.",
        ),
      );

      setActiveAction(null);

      return;
    }

    try {
      await reloadRequests();
    } catch (error: unknown) {
      setActionError(
        getApiErrorMessage(
          error,
          "Die Anfrage wurde genehmigt, die Liste konnte aber nicht aktualisiert werden. Bitte lade die Seite neu.",
        ),
      );
    } finally {
      setActiveAction(null);
    }
  }

  async function handleReject(
    request: AdoptionRequestDto,
  ): Promise<void> {
    const applicantName = [
      request.firstName,
      request.lastName,
    ]
      .filter(Boolean)
      .join(" ");

    const confirmed = window.confirm(
      `Möchtest du die Adoptionsanfrage${
        applicantName
          ? ` von ${applicantName}`
          : ""
      } wirklich ablehnen?`,
    );

    if (!confirmed) {
      return;
    }

    setActionError(null);

    setActiveAction({
      requestId: request.id,
      type: "reject",
    });

    try {
      await rejectAdoptionRequest(
        request.id,
      );
    } catch (error: unknown) {
      setActionError(
        getApiErrorMessage(
          error,
          "Die Adoptionsanfrage konnte nicht abgelehnt werden.",
        ),
      );

      setActiveAction(null);

      return;
    }

    try {
      await reloadRequests();
    } catch (error: unknown) {
      setActionError(
        getApiErrorMessage(
          error,
          "Die Anfrage wurde abgelehnt, die Liste konnte aber nicht aktualisiert werden. Bitte lade die Seite neu.",
        ),
      );
    } finally {
      setActiveAction(null);
    }
  }

  return (
    <section className="page">
      <div className="shelter-requests-header">
        <div>
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>Adoptionsanfragen</h1>

          <p className="page-description">
            Prüfe eingegangene Anfragen,
            kontaktiere Interessenten und
            entscheide über die Vermittlung.
          </p>
        </div>

        <Link
          className="button button--outline"
          to="/shelter"
        >
          Zurück zum Dashboard
        </Link>
      </div>

      {requestsState.status ===
        "loading" && (
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
              Die eingegangenen Anfragen werden
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

      {requestsState.status ===
        "success" && (
        <>
          <div className="shelter-request-statistics">
            <div>
              <strong>
                {requestCounts.all}
              </strong>

              <span>Gesamt</span>
            </div>

            <div>
              <strong>
                {requestCounts.pending}
              </strong>

              <span>Offen</span>
            </div>

            <div>
              <strong>
                {requestCounts.approved}
              </strong>

              <span>Genehmigt</span>
            </div>

            <div>
              <strong>
                {requestCounts.rejected}
              </strong>

              <span>Abgelehnt</span>
            </div>
          </div>

          <div className="shelter-request-controls">
            <label className="shelter-request-search">
              <span>
                Anfragen durchsuchen
              </span>

              <input
                type="search"
                value={searchTerm}
                onChange={(event) =>
                  setSearchTerm(
                    event.target.value,
                  )
                }
                placeholder="Tier, Name, E-Mail oder Telefonnummer"
              />
            </label>

            <div
              className="shelter-request-filters"
              aria-label="Statusfilter"
            >
              {FILTER_OPTIONS.map(
                (option) => (
                  <button
                    key={option.value}
                    className={
                      selectedFilter ===
                      option.value
                        ? "shelter-filter-button shelter-filter-button--active"
                        : "shelter-filter-button"
                    }
                    type="button"
                    aria-pressed={
                      selectedFilter ===
                      option.value
                    }
                    onClick={() =>
                      setSelectedFilter(
                        option.value,
                      )
                    }
                  >
                    {option.label}

                    <span>
                      {
                        requestCounts[
                          option.value
                        ]
                      }
                    </span>
                  </button>
                ),
              )}
            </div>
          </div>

          {actionError && (
            <div
              className="animals-message animals-message--error"
              role="alert"
            >
              <div>
                <strong>
                  Aktion konnte nicht
                  vollständig ausgeführt werden
                </strong>

                <p>{actionError}</p>
              </div>
            </div>
          )}

          <div className="shelter-request-result-summary">
            <strong>
              {visibleRequests.length}
            </strong>{" "}
            {visibleRequests.length === 1
              ? "Anfrage gefunden"
              : "Anfragen gefunden"}
          </div>

          {visibleRequests.length === 0 ? (
            <div className="my-requests-empty">
              <span aria-hidden="true">
                📭
              </span>

              <h2>
                Keine Anfragen gefunden
              </h2>

              <p>
                Für die gewählte Suche oder
                den Statusfilter sind keine
                Adoptionsanfragen vorhanden.
              </p>

              <button
                className="button button--outline"
                type="button"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedFilter("all");
                }}
              >
                Filter zurücksetzen
              </button>
            </div>
          ) : (
            <div className="shelter-request-list">
              {visibleRequests.map(
                (request) => {
                  const statusInfo =
                    getAdoptionRequestStatusInfo(
                      request.status,
                    );

                  const isPending =
                    statusInfo.key ===
                    "pending";

                  const isCurrentAction =
                    activeAction?.requestId ===
                    request.id;

                  return (
                    <article
                      key={request.id}
                      className="shelter-request-card"
                    >
                      <div className="shelter-request-card__header">
                        <div>
                          <p className="shelter-request-card__label">
                            Anfrage für
                          </p>

                          <h2>
                            {request.animalName ||
                              "Unbekanntes Tier"}
                          </h2>

                          <p>
                            Eingegangen am{" "}
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

                      <div className="shelter-request-card__grid">
                        <section>
                          <h3>Interessent</h3>

                          <dl className="adoption-request-contact">
                            <div>
                              <dt>Name</dt>

                              <dd>
                                {
                                  request.firstName
                                }{" "}
                                {request.lastName}
                              </dd>
                            </div>

                            <div>
                              <dt>E-Mail</dt>

                              <dd>
                                <a
                                  href={`mailto:${request.email}`}
                                >
                                  {
                                    request.email
                                  }
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

                        <section>
                          <h3>Nachricht</h3>

                          <p className="shelter-request-card__message">
                            {request.message ||
                              "Keine Nachricht hinterlegt."}
                          </p>
                        </section>
                      </div>

                      <div className="shelter-request-card__footer">
                        <div>
                          <span>
                            Anfrage-ID:
                          </span>

                          <code>
                            {request.id}
                          </code>
                        </div>

                        <div className="shelter-request-card__actions">
                          <Link
                            className="button button--outline"
                            to={`/shelter/adoption-requests/${request.id}`}
                          >
                            Anfrage öffnen
                          </Link>

                          <Link
                            className="button button--outline"
                            to={`/animals/${request.animalId}`}
                          >
                            Tierprofil ansehen
                          </Link>

                          {isPending && (
                            <>
                              <button
                                className="button button--danger"
                                type="button"
                                disabled={
                                  activeAction !==
                                  null
                                }
                                onClick={() =>
                                  void handleReject(
                                    request,
                                  )
                                }
                              >
                                {isCurrentAction &&
                                activeAction.type ===
                                  "reject"
                                  ? "Wird abgelehnt …"
                                  : "Ablehnen"}
                              </button>

                              <button
                                className="button button--primary"
                                type="button"
                                disabled={
                                  activeAction !==
                                  null
                                }
                                onClick={() =>
                                  void handleApprove(
                                    request,
                                  )
                                }
                              >
                                {isCurrentAction &&
                                activeAction.type ===
                                  "approve"
                                  ? "Wird genehmigt …"
                                  : "Genehmigen"}
                              </button>
                            </>
                          )}
                        </div>
                      </div>
                    </article>
                  );
                },
              )}
            </div>
          )}
        </>
      )}
    </section>
  );
}
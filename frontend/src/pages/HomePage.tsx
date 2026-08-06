import axios from "axios";
import { useEffect, useState } from "react";
import { Link } from "react-router";

import { httpClient } from "../api/httpClient";

type HealthResponse = {
  success?: boolean;
  service?: string;
  version?: string;
  status?: string;
  timestamp?: string;
};

type ApiErrorResponse = {
  title?: string;
  detail?: string;
  error?: string;
};

type ConnectionState =
  | {
      status: "loading";
    }
  | {
      status: "success";
      data: HealthResponse;
    }
  | {
      status: "error";
      message: string;
    };

export function HomePage() {
  const [connectionState, setConnectionState] =
    useState<ConnectionState>({
      status: "loading",
    });

  useEffect(() => {
    const abortController = new AbortController();

    async function checkBackendConnection(): Promise<void> {
      try {
        const response =
          await httpClient.get<HealthResponse>(
            "/api/v1/health",
            {
              signal: abortController.signal,
            },
          );

        setConnectionState({
          status: "success",
          data: response.data,
        });
      } catch (error: unknown) {
        if (
          axios.isCancel(error) ||
          abortController.signal.aborted
        ) {
          return;
        }

        if (
          axios.isAxiosError<ApiErrorResponse>(
            error,
          )
        ) {
          const message =
            error.response?.data?.detail ??
            error.response?.data?.error ??
            error.response?.data?.title ??
            error.message;

          setConnectionState({
            status: "error",
            message,
          });

          return;
        }

        setConnectionState({
          status: "error",
          message:
            "Es ist ein unbekannter Fehler aufgetreten.",
        });
      }
    }

    void checkBackendConnection();

    return () => {
      abortController.abort();
    };
  }, []);

  return (
    <section className="page">
      <div className="hero">
        <div className="hero__content">
          <p className="eyebrow">
            Willkommen bei TierMatch
          </p>

          <h1>
            Finde ein Tier, das wirklich zu dir
            passt.
          </h1>

          <p className="hero__description">
            Entdecke Tiere aus Tierheimen in deiner
            Nähe und finde deinen neuen tierischen
            Begleiter.
          </p>

          <div className="hero__actions">
            <Link
              className="button button--primary"
              to="/animals"
            >
              Tiere entdecken
            </Link>

            <Link
              className="button button--secondary"
              to="/register"
            >
              Konto erstellen
            </Link>
          </div>
        </div>
      </div>

      <section className="status-card">
        <div>
          <p className="eyebrow">
            Für Tierheime
          </p>

          <h2>
            Gebt euren Tieren eine neue Bühne
          </h2>

          <p>
            Tierheime können sich bei TierMatch
            registrieren und nach erfolgreicher
            Prüfung ein eigenes Verwaltungskonto
            erhalten.
          </p>

          <p>
            Über das Tierheimkonto können später
            Tiere eingestellt und eingehende
            Adoptionsanfragen bearbeitet werden.
          </p>
        </div>

        <div className="hero__actions">
          <Link
            className="button button--primary"
            to="/shelter/register"
          >
            Tierheim registrieren
          </Link>

          <Link
            className="button button--outline"
            to="/shelter/login"
          >
            Tierheim-Login
          </Link>
        </div>
      </section>

      <section className="status-card">
        <div>
          <p className="eyebrow">
            Systemstatus
          </p>

          <h2>
            Verbindung zur TierMatch-API
          </h2>
        </div>

        {connectionState.status === "loading" && (
          <div className="status-message">
            <span className="status-indicator status-indicator--loading" />

            <span>
              Verbindung wird geprüft …
            </span>
          </div>
        )}

        {connectionState.status === "success" && (
          <div className="status-message status-message--success">
            <div className="status-message__headline">
              <span className="status-indicator status-indicator--success" />

              <strong>
                Backend erreichbar
              </strong>
            </div>

            <dl className="health-details">
              <div>
                <dt>Dienst</dt>

                <dd>
                  {connectionState.data.service ??
                    "TierMatch API"}
                </dd>
              </div>

              <div>
                <dt>Version</dt>

                <dd>
                  {connectionState.data.version ??
                    "Unbekannt"}
                </dd>
              </div>

              <div>
                <dt>Status</dt>

                <dd>
                  {connectionState.data.status ??
                    "Online"}
                </dd>
              </div>
            </dl>
          </div>
        )}

        {connectionState.status === "error" && (
          <div className="status-message status-message--error">
            <div className="status-message__headline">
              <span className="status-indicator status-indicator--error" />

              <strong>
                Backend nicht erreichbar
              </strong>
            </div>

            <p>
              {connectionState.message}
            </p>
          </div>
        )}
      </section>
    </section>
  );
}
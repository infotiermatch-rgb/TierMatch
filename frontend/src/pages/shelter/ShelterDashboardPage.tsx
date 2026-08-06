import { Link } from "react-router";

import { useAuth } from "../../features/authentication/AuthContext";

export function ShelterDashboardPage() {
  const {
    user,
    isAdmin,
    shelterId,
  } = useAuth();

  const displayName = [
    user?.firstName,
    user?.lastName,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <section className="page">
      <div className="shelter-dashboard__header">
        <div>
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>Tierheim-Verwaltung</h1>

          <p className="page-description">
            Willkommen
            {displayName
              ? `, ${displayName}`
              : ""}
            . Hier verwaltest du Tiere und
            eingegangene Adoptionsanfragen.
          </p>
        </div>

        <Link
          className="button button--outline"
          to="/"
        >
          Zur öffentlichen Website
        </Link>
      </div>

      <div className="shelter-account-notice">
        <div>
          <strong>
            {isAdmin
              ? "Globale Administration"
              : "Tierheimkonto"}
          </strong>

          <p>
            {isAdmin
              ? "Du hast Zugriff auf die Daten aller Tierheime."
              : "Du kannst ausschließlich die Daten deines zugeordneten Tierheims verwalten."}
          </p>
        </div>

        {!isAdmin && shelterId && (
          <span className="shelter-account-notice__id">
            Tierheim-ID: {shelterId}
          </span>
        )}
      </div>

      <div className="shelter-dashboard__grid">
        <article className="shelter-dashboard-card">
          <div
            className="shelter-dashboard-card__icon"
            aria-hidden="true"
          >
            📬
          </div>

          <div>
            <p className="shelter-dashboard-card__label">
              Interessenten
            </p>

            <h2>Adoptionsanfragen</h2>

            <p>
              Prüfe eingegangene Anfragen,
              öffne Kontaktdaten und genehmige
              oder lehne Anfragen ab.
            </p>
          </div>

          <Link
            className="button button--primary"
            to="/shelter/adoption-requests"
          >
            Anfragen verwalten
          </Link>
        </article>

        <article className="shelter-dashboard-card">
          <div
            className="shelter-dashboard-card__icon"
            aria-hidden="true"
          >
            🐾
          </div>

          <div>
            <p className="shelter-dashboard-card__label">
              Bestand
            </p>

            <h2>Tierverwaltung</h2>

            <p>
              Erstelle Tierprofile, bearbeite
              Angaben, verwalte Bilder und ändere
              den Vermittlungsstatus.
            </p>
          </div>

          <span className="shelter-dashboard-card__status">
            Wird anschließend eingerichtet
          </span>
        </article>

        <article className="shelter-dashboard-card">
          <div
            className="shelter-dashboard-card__icon"
            aria-hidden="true"
          >
            🏠
          </div>

          <div>
            <p className="shelter-dashboard-card__label">
              Organisation
            </p>

            <h2>Tierheimprofil</h2>

            <p>
              Später können hier Name, Anschrift,
              Kontaktdaten, Beschreibung und
              Öffnungszeiten gepflegt werden.
            </p>
          </div>

          <span className="shelter-dashboard-card__status">
            Für eine spätere Ausbaustufe
          </span>
        </article>
      </div>

      <section className="shelter-dashboard__next">
        <div>
          <p className="eyebrow">
            Aktueller Verwaltungsbereich
          </p>

          <h2>Adoptionsanfragen verwalten</h2>

          <p>
            Eingegangene Adoptionsanfragen können
            nach Status gefiltert, durchsucht,
            genehmigt oder abgelehnt werden.
          </p>

          <Link
            className="button button--primary"
            to="/shelter/adoption-requests"
          >
            Zur Anfragenverwaltung
          </Link>
        </div>
      </section>
    </section>
  );
}
import { Link } from "react-router";

import { useAuth } from "../features/authentication/AuthContext";

function formatDate(
  dateValue: string | null,
): string {
  if (!dateValue) {
    return "Noch keine Angabe";
  }

  return new Intl.DateTimeFormat("de-DE", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(dateValue));
}

export function ProfilePage() {
  const { user } = useAuth();

  if (!user) {
    return null;
  }

  const displayName =
    [user.firstName, user.lastName]
      .filter(Boolean)
      .join(" ") || user.email;

  return (
    <section className="page">
      <div className="profile-header">
        <div>
          <p className="eyebrow">
            Dein TierMatch-Konto
          </p>

          <h1>{displayName}</h1>

          <p className="page-description">
            Verwalte deine persönlichen Daten und
            die Sicherheit deines Kontos.
          </p>
        </div>

        <div className="profile-actions">
          <Link
            className="button button--primary"
            to="/profile/edit"
          >
            Profil bearbeiten
          </Link>

          <Link
            className="button button--outline"
            to="/profile/change-password"
          >
            Passwort ändern
          </Link>
        </div>
      </div>
       <Link
    className="button button--outline"
    to="/my-adoption-requests"
  >
    Meine Adoptionsanfragen
  </Link>

      <div className="profile-card">
        <dl className="profile-details">
          <div>
            <dt>E-Mail-Adresse</dt>
            <dd>{user.email}</dd>
          </div>

          <div>
            <dt>Vorname</dt>
            <dd>
              {user.firstName ??
                "Nicht angegeben"}
            </dd>
          </div>

          <div>
            <dt>Nachname</dt>
            <dd>
              {user.lastName ??
                "Nicht angegeben"}
            </dd>
          </div>

          <div>
            <dt>Rollen</dt>
            <dd>
              {user.roles.length > 0
                ? user.roles.join(", ")
                : "Keine Rolle"}
            </dd>
          </div>

          <div>
            <dt>Kontostatus</dt>
            <dd>
              {user.isActive
                ? "Aktiv"
                : "Deaktiviert"}
            </dd>
          </div>

          <div>
            <dt>Tierheim</dt>
            <dd>
              {user.shelterId ??
                "Keinem Tierheim zugeordnet"}
            </dd>
          </div>

          <div>
            <dt>Erstellt am</dt>
            <dd>
              {formatDate(user.createdAt)}
            </dd>
          </div>

          <div>
            <dt>Letzte Anmeldung</dt>
            <dd>
              {formatDate(user.lastLoginAt)}
            </dd>
          </div>
        </dl>
      </div>
    </section>
  );
}
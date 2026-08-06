import {
  Link,
  Navigate,
  Outlet,
  useLocation,
} from "react-router";

import { useAuth } from "../features/authentication/AuthContext";

export function ShelterRoute() {
  const location = useLocation();

  const {
    status,
    canManageShelter,
    isShelterAdmin,
    shelterId,
  } = useAuth();

  if (status === "loading") {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>Zugriff wird geprüft</h1>

          <p className="form-card__description">
            Deine Berechtigungen für den
            Tierheim-Bereich werden überprüft …
          </p>
        </div>
      </section>
    );
  }

  if (status === "anonymous") {
    return (
      <Navigate
        to="/shelter/login"
        replace
        state={{
          from: location,
        }}
      />
    );
  }

  if (!canManageShelter) {
    const hasMissingShelterAssignment =
      isShelterAdmin &&
      shelterId === null;

    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">
            TierMatch Shelter
          </p>

          <h1>
            {hasMissingShelterAssignment
              ? "Kein Tierheim zugeordnet"
              : "Zugriff nicht erlaubt"}
          </h1>

          <p className="form-card__description">
            {hasMissingShelterAssignment
              ? "Dein Konto besitzt zwar die Rolle eines Tierheimadministrators, ist aber noch keinem Tierheim zugeordnet. Bitte wende dich an einen TierMatch-Administrator."
              : "Dieser Bereich ist ausschließlich für Tierheimmitarbeiter und TierMatch-Administratoren verfügbar."}
          </p>

          <div className="profile-actions">
            {!hasMissingShelterAssignment && (
              <Link
                className="button button--primary"
                to="/shelter/login"
                state={{
                  from: location,
                }}
              >
                Mit Tierheimkonto anmelden
              </Link>
            )}

            <Link
              className="button button--outline"
              to="/"
            >
              Zur Startseite
            </Link>

            <Link
              className="button button--outline"
              to="/profile"
            >
              Zum Profil
            </Link>
          </div>
        </div>
      </section>
    );
  }

  return <Outlet />;
}
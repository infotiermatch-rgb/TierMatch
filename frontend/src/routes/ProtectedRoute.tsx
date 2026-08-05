import {
  Navigate,
  Outlet,
  useLocation,
} from "react-router";

import { useAuth } from "../features/authentication/AuthContext";

export function ProtectedRoute() {
  const location = useLocation();
  const { status } = useAuth();

  if (status === "loading") {
    return (
      <section className="page page--narrow">
        <div className="form-card">
          <p className="eyebrow">TierMatch-Konto</p>
          <h1>Sitzung wird geprüft</h1>

          <p className="form-card__description">
            Deine Anmeldedaten werden überprüft …
          </p>
        </div>
      </section>
    );
  }

  if (status === "anonymous") {
    return (
      <Navigate
        to="/login"
        replace
        state={{
          from: location,
        }}
      />
    );
  }

  return <Outlet />;
}
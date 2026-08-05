import { useState } from "react";
import {
  NavLink,
  Outlet,
  useNavigate,
} from "react-router";

import { useAuth } from "../features/authentication/AuthContext";

function getNavigationClassName(
  isActive: boolean,
): string {
  return isActive
    ? "navigation-link navigation-link--active"
    : "navigation-link";
}

export function MainLayout() {
  const navigate = useNavigate();

  const {
    status,
    user,
    logout,
  } = useAuth();

  const [isLoggingOut, setIsLoggingOut] =
    useState(false);

  async function handleLogout(): Promise<void> {
    setIsLoggingOut(true);

    try {
      await logout();

      navigate("/", {
        replace: true,
      });
    } finally {
      setIsLoggingOut(false);
    }
  }

  return (
    <div className="app-shell">
      <header className="main-header">
        <div className="main-header__content">
          <NavLink
            className="brand"
            to="/"
          >
            TierMatch
          </NavLink>

          <nav
            className="main-navigation"
            aria-label="Hauptnavigation"
          >
            <NavLink
              className={({ isActive }) =>
                getNavigationClassName(
                  isActive,
                )
              }
              to="/"
              end
            >
              Startseite
            </NavLink>

            <NavLink
              className={({ isActive }) =>
                getNavigationClassName(
                  isActive,
                )
              }
              to="/animals"
            >
              Tiere
            </NavLink>

            {status === "authenticated" &&
              user && (
                <>
                  <NavLink
                    className={({
                      isActive,
                    }) =>
                      getNavigationClassName(
                        isActive,
                      )
                    }
                    to="/my-adoption-requests"
                  >
                    Meine Anfragen
                  </NavLink>

                  <NavLink
                    className={({
                      isActive,
                    }) =>
                      getNavigationClassName(
                        isActive,
                      )
                    }
                    to="/profile"
                  >
                    Profil
                  </NavLink>

                  <button
                    className="navigation-button"
                    type="button"
                    onClick={() =>
                      void handleLogout()
                    }
                    disabled={isLoggingOut}
                  >
                    {isLoggingOut
                      ? "Abmeldung …"
                      : "Abmelden"}
                  </button>
                </>
              )}

            {status === "anonymous" && (
              <>
                <NavLink
                  className={({
                    isActive,
                  }) =>
                    getNavigationClassName(
                      isActive,
                    )
                  }
                  to="/login"
                >
                  Anmelden
                </NavLink>

                <NavLink
                  className={({
                    isActive,
                  }) =>
                    getNavigationClassName(
                      isActive,
                    )
                  }
                  to="/register"
                >
                  Registrieren
                </NavLink>
              </>
            )}
          </nav>
        </div>
      </header>

      <main className="main-content">
        <Outlet />
      </main>

      <footer className="main-footer">
        <p>
          TierMatch – Tiere und Menschen
          zusammenbringen
        </p>
      </footer>
    </div>
  );
}
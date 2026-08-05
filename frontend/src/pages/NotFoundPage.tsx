import { Link } from "react-router";

export function NotFoundPage() {
  return (
    <section className="page page--narrow">
      <div className="form-card">
        <p className="eyebrow">Fehler 404</p>
        <h1>Seite nicht gefunden</h1>

        <p className="form-card__description">
          Die angeforderte Seite existiert nicht.
        </p>

        <Link className="button button--primary" to="/">
          Zur Startseite
        </Link>
      </div>
    </section>
  );
}
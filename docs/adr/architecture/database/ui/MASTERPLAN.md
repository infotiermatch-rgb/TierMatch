TierMatch – MASTERPLAN.md
1. Vision
TierMatch verbindet Tierheime und Menschen.

Tierheime können Tiere verwalten, Interessenten können passende Tiere
finden und den gesamten Adoptionsprozess digital durchführen.
2. Projektziele
Backend
Clean Architecture
Domain Driven Design (light)
CQRS + MediatR
ASP.NET Core 9
PostgreSQL
Entity Framework Core
ASP.NET Identity
JWT + Refresh Tokens
REST API
Swagger
Serilog
Unit Tests
Integration Tests
Frontend

Später:

React
TypeScript
Material UI
TanStack Query
React Router
Axios
3. Architektur
TierMatch
│
├── backend
│
│   ├── src
│   │
│   ├── TierMatch.Api
│   ├── TierMatch.Application
│   ├── TierMatch.Domain
│   ├── TierMatch.Infrastructure
│   └── TierMatch.Contracts
│
│
├── frontend
│
├── docs
│
├── tests
│
└── .github
4. Clean Architecture
          API
           │
           ▼
     Application
           │
           ▼
        Domain
           ▲
           │
    Infrastructure

Regeln:

Domain kennt nichts.
Application kennt nur Domain.
Infrastructure kennt alles.
API kennt Application.
5. Module
Authentication

Status

🟡 In Entwicklung

Enthält:

Login
Register
Refresh
Logout
Session Management
Current User
JWT
Refresh Tokens
Roles
Policies
Shelter

Status

⚪ Geplant

Enthält

CRUD
Ansprechpartner
Adresse
Öffnungszeiten
Bilder
Animals

Status

⚪ Geplant

Enthält

CRUD
Eigenschaften
Bilder
Vermittlungsstatus
Favorites

Status

⚪ Geplant

Adoption

Status

⚪ Geplant

Workflow

Interessent

↓

Anfrage

↓

Tierheim

↓

Bearbeitung

↓

Annahme / Ablehnung

↓

Historie
Search

Status

⚪ Geplant

Filter

Tierart
Alter
Größe
Entfernung
Tierheim
Geschlecht
Notifications

Status

⚪ Geplant

Email
Push
InApp
6. Rollen
Admin

↓

ShelterAdmin

↓

User
7. Coding Standards
nullable aktiviert
async/await überall
keine Businesslogik im Controller
CQRS
FluentValidation
Result Pattern
Repository Pattern
UnitOfWork
Logging
XML Kommentare bei Public APIs
8. API Standards

Alle Endpunkte

/api/v1/

Antworten

{
  "data": {},
  "errors": [],
  "status": 200
}
9. Security
JWT
Refresh Token Rotation
SHA256 Hashing
ASP.NET Identity
HTTPS
Roles
Policies
Security Logging
10. Datenbank

PostgreSQL

Tabellen

Animals

Shelters

AnimalImages

Users

RefreshTokens

AdoptionRequests

Favorites

Notifications
11. Releases
Release 0.1

✅ Foundation

Release 0.2

🟡 Authentication

Release 0.3

Shelters

Release 0.4

Animals

Release 0.5

Search

Release 0.6

Favorites

Release 0.7

Adoption

Release 0.8

Notifications

Release 0.9

Administration

Release 1.0

Production Ready

12. Definition of Done

Ein Modul gilt nur als fertig wenn:

✅ Build erfolgreich
✅ Keine Compiler-Warnungen
✅ Tests vorhanden
✅ Swagger getestet
✅ Logging vorhanden
✅ Validation vorhanden
✅ Dokumentation aktualisiert
13. Langfristige Ziele (nach Version 1.0)
Mobile App (Android/iOS)
Google Login
Microsoft Login
Apple Login
Discord Login
Kartenansicht
Chat zwischen Interessenten und Tierheimen
KI-gestützte Tierempfehlungen
Mehrsprachigkeit (DE/EN)
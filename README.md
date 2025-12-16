# Salahly — Full-Stack Home Services Marketplace

Salahly is a two-sided marketplace that connects homeowners with vetted local craftsmen. It combines an Angular 20 SPA for customers, craftsmen, and admins with an ASP.NET Core Web API backend that handles authentication, service-request orchestration, bookings, notifications, and payments.

---

## Table of Contents

1. [Platform Overview](#platform-overview)
2. [User Roles & Capabilities](#user-roles--capabilities)
   - [Customers](#customers)
   - [Craftsmen / Technicians](#craftsmen--technicians)
   - [Admins](#admins)
3. [Frontend Application](#frontend-application)
4. [Backend Application](#backend-application)
5. [End-to-End Service Request Flow](#end-to-end-service-request-flow)
6. [Technology Stack](#technology-stack)
7. [Local Development Setup](#local-development-setup)
8. [Folder Structure](#folder-structure)
9. [Testing](#testing)
10. [Contributing](#contributing)

---

## Platform Overview

- **Marketplace model:** Customers post service requests (plumbing, electrical, etc.). Craftsmen browse opportunities, submit offers, and are booked when their offer is accepted.
- **Real-time experience:** SignalR-based notification and chat hubs keep both sides updated on offers, bookings, and job progress.
- **End-to-end workflow:** Covers user onboarding, request management, offer comparison, booking, payment capture, portfolio approvals, and admin oversight.
- **International-ready UI:** Angular standalone components with `@ngx-translate/core`, responsive layouts, and modular feature sections.

---

## User Roles & Capabilities

### Customers

- **Discover technicians:** Public landing and browse pages showcase featured crafts, highlight benefits, and list technicians with ratings and crafts.
- **Account management:** Sign up / log in, complete profile, and manage saved addresses and preferences.
- **Create service requests:** Dedicated form collects craft, area, schedule, budget, payment method, and optional images.
- **Track requests:** “Show Services Requested” dashboard with tabs (Active/History), filters (status, area, search), budget summaries, image galleries, inline edits, and delete/complete actions.
- **Review opportunities:** Inspect offers, chat with craftsmen after accepting an offer, and mark work as completed to trigger payouts.
- **Notifications:** Receive real-time updates for offers, booking confirmations, and payment outcomes.

### Craftsmen / Technicians

- **Onboarding:** Complete profile flow gathers craft specialization, service areas, pricing, availability, and required documents.
- **Opportunity feed:** “Browse Opportunities” page surfaces geo-filtered open requests with status chips, offer counters, and quick actions to respond.
- **Offer management:** Submit, update, and track offers; view history of accepted / rejected offers.
- **Portfolio tools:** Build and edit showcased work (images, descriptions) with admin approval pipeline.
- **Wallet & payouts:** Balance updates after completed bookings; view job history and payment records.
- **Notifications:** Immediate alerts when new requests land in their service radius or when offers are accepted.

### Admins

- **Dashboard:** High-level KPIs on service requests, bookings, revenue, and active users.
- **Craft & area management:** CRUD interfaces for maintaining taxonomy used by the marketplace.
- **Portfolio moderation:** Review and approve craftsman portfolio submissions.
- **User oversight:** Monitor craftsmen availability, flagged requests, and handle escalations.

---

## Frontend Application

- **Framework:** Angular 20 standalone components with SSR-ready configuration (`@angular/ssr`).
- **Routing (`Client/src/app/app.routes.ts`):**
  - Public shell: landing, home, login, signup, browse technicians, technician profile.
  - Customer shell (guarded): profile, service-request list, request form, service-request details.
  - Technician shell (guarded): complete profile, browse opportunities, history, portfolio (list + detail).
  - Admin shell (guarded): dashboard, craft management, area management, portfolio approvals.
- **Shared UI:** Feature cards, hero banners, hover directives, localized copy, Bootstrap 5 styling with custom tokens, skeleton loaders, toasts, and spinners.
- **State & services:** Angular signals for reactive UI, `ServicesRequestsService` for CRUD, token-aware interceptors, ngx-translate for i18n, and SignalR client for notifications/chat.
- **Quality-of-life features:** Inline request editing, modal confirmations, progress indicators (e.g., offers progress), budget formatting, and responsive grid layouts.

---

## Backend Application

- **Solution layout:**
  - `Salahly.DAL` — EF Core entities (ServiceRequest, CraftsmanOffer, Booking, Payment, etc.), repositories, migrations.
  - `Salahly.DSL` — Domain services (auth, service requests, booking orchestrator, payments, notifications) plus DTOs and Mapster mapping profiles.
  - `SalahlyProject` — ASP.NET Core Web API host exposing Customer/Craftsman/Admin controllers, SignalR hubs, and background jobs.
- **Key services:**
  - `AuthService` — Identity + JWT auth, refresh tokens, role management.
  - `ServiceRequestService` — CRUD, filtering, image support, status transitions, craftsman notifications, booking hooks.
  - `NotificationService` — Sends real-time alerts via SignalR and persists notifications.
  - `OfferAcceptanceOrchestrator` (DSL) — Atomic workflow for accepting offers, booking creation, payment initialization, and rollback on failure.
  - Payment strategies — Paymob, wallet, cash implementations injected via DI.
  - Hosted jobs — e.g., `BookingCleanupHostedService` to expire unpaid bookings.
- **APIs & hubs (examples):**
  - `POST /api/auth/login` — JWT issuance.
  - `POST /api/customer/servicerequests` — create request with optional image uploads.
  - `GET /api/craftsman/service-requests/opportunities` — location-aware feed.
  - `POST /api/customer/servicerequests/{id}/complete` — trigger completion + payout.
  - `NotificationHub` & `BookingChatHub` — SignalR hubs for pushes and messaging.
- **Security:** ASP.NET Core Identity, JWT bearer auth with role-based policies (Customer, Craftsman, Admin).
- **Data integrity:** UnitOfWork + repository pattern, transaction scopes in orchestrators, background reconciliation jobs.

---

## End-to-End Service Request Flow

1. **Customer submits** a request with craft, area, times, budget, payment method, and optional photos.
2. **Backend stores** the request (`Status = Open`, expiry + max offers) and notifies matching craftsmen in the area.
3. **Craftsmen review & offer**: those with matching craft/service area see the opportunity feed and submit offers with pricing and notes.
4. **Customer compares** offers via the “Show Services Requested” UI (status chips, offer counts, history).
5. **Offer accepted:** orchestrator locks the offer, creates a booking, and kicks off payment (Paymob / wallet / cash).
6. **Chat & execution:** real-time chat + notifications keep both parties aligned; technicians update progress.
7. **Completion & payout:** customer marks request finished, backend updates booking status, and technician wallet balance is credited.
8. **History & analytics:** both sides review past jobs; admins track KPIs and moderate new content.

---

## Technology Stack

| Layer    | Technology                                                                   |
| -------- | ---------------------------------------------------------------------------- |
| Frontend | Angular 20, RxJS, `@ngx-translate`, Bootstrap 5, Bootstrap Icons, SignalR JS |
| Backend  | ASP.NET Core 9 (Web API), Entity Framework Core, SignalR, Mapster            |
| Database | SQL Server                                                                   |
| Auth     | ASP.NET Core Identity + JWT Bearer                                           |
| Tooling  | .NET CLI, Angular CLI, npm, TypeScript, ESLint, Prettier                     |

---

## Local Development Setup

### Prerequisites

- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- Node.js LTS + npm
- Angular CLI (`npm install -g @angular/cli`)

### Backend

1. `cd Backend/SalahlyApp`
2. Update `appsettings.Development.json` with:
   - `ConnectionStrings:DefaultConnection`
   - `JwtSettings` (Secret, Issuer, Audience, ExpiresMinutes)
   - Payment provider keys & callback URLs
3. Apply EF migrations:
   ```bash
   dotnet ef database update --project Salahly.DAL --startup-project SalahlyProject
   ```
4. Run the API:
   ```bash
   dotnet run --project SalahlyProject
   ```

### Frontend

1. `cd Client`
2. Install dependencies: `npm install`
3. Configure API base URL in `src/environments` if needed.
4. Start dev server:
   ```bash
   ng serve
   ```
5. Visit `http://localhost:4200` (Angular dev server proxies API calls when configured).

---

## Folder Structure

```
Salahly/
├─ Backend/
│  └─ SalahlyApp/
│     ├─ Salahly.DAL/      # Entities, repositories, migrations
│     ├─ Salahly.DSL/      # Business logic, DTOs, orchestrators, notifications
│     └─ SalahlyProject/   # ASP.NET Core Web API host + hubs + DI setup
├─ Client/
│  ├─ src/app/             # Angular standalone components, layouts, guards
│  ├─ package.json         # Frontend dependencies
│  └─ README.md            # Angular CLI usage notes
└─ README.md               # (This document)
```

---

## Testing

- **Frontend:**
  - Unit tests with Karma/Jasmine (`ng test`).
  - Optional e2e testing via preferred framework (e.g., Cypress) — scaffolding available via Angular CLI.
- **Backend:**
  - Integration/unit tests can be added with xUnit/NUnit.
  - Use Postman or REST Client files to verify endpoints and SignalR hubs.

---

## Contributing

1. Fork and clone the repository.
2. Create a feature branch (`feat/<name>`).
3. Follow existing code style (Angular standalone components, UnitOfWork + service pattern in backend).
4. Add or update tests when introducing new behavior.
5. Submit a PR describing the change, affected roles (customer/craftsman/admin), and testing notes.

For questions or walkthroughs, open an issue detailing your environment and the feature/problem you’re tackling.

---

**Happy building with Salahly!**

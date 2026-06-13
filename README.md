# Sports Club Management System

**Stack:** ASP.NET Core 9 Web API (C#) + React 18 (TypeScript, Vite) + SQL Server
**Auth:** JWT bearer tokens

A role-based web app for managing a sports club's members, coaches, training
classes, schedules, and membership packages. Migrated from the original Java
Servlet/JSP/JDBC version — same SQL Server schema and the same three roles.

---

## 1. Architecture

```
React SPA (5173) ──fetch + JWT──> ASP.NET Core API (5097) ──> EF Core ──> SQL Server
```

| Role | API prefix | Responsibilities |
|------|-----------|------------------|
| **ADMIN**  | `/api/admin/*`  | Manage members, coaches, classes, schedules, packages |
| **COACH**  | `/api/coach/*`  | View own assigned classes + enrolled members + schedule |
| **MEMBER** | `/api/member/*` | Enroll/cancel classes, view schedule, manage profile |
| public     | `/api/auth/*`   | Login, register, logout, current user |

---

## 2. Prerequisites

| Software | Version |
|----------|---------|
| .NET SDK | 9.0+ |
| Node.js  | 18+ (npm 9+) |
| SQL Server | 2019+ |

---

## 3. Database Setup

Run the schema script once in SQL Server Management Studio or `sqlcmd`:

```
backend/SportsClub.Api/database.sql
```

It creates `SportsClubDB` with all tables and sample data.

**Connection settings** come from environment variables (with local-dev
fallbacks) via the `DatabaseConfig` singleton — no hardcoded secrets:

| Variable | Default | Notes |
|----------|---------|-------|
| `DB_HOST` | `localhost` | SQL Server host |
| `DB_PORT` | `1433` | Port |
| `DB_NAME` | `SportsClubDB` | Database name |
| `DB_USER` | `sa` | User |
| `DB_PASSWORD` | `P@ssw0rd` | Password |
| `DB_TRUST_CERT` | `true` | **Set `false` in production** to validate the TLS cert |

You can also override everything with a `ConnectionStrings:Default` value in
`appsettings.json`.

---

## 4. Running

```bash
# Terminal 1 — backend API
cd backend
dotnet run --project SportsClub.Api
#  → http://localhost:5097

# Terminal 2 — frontend SPA
cd frontend
npm install          # first time only
npm run dev
#  → http://localhost:5173  (Vite proxies /api to the backend)
```

Open <http://localhost:5173> and log in.

### Sample accounts (password: `Password123`)
| Username | Role |
|----------|------|
| `admin` | ADMIN |
| `coach1` | COACH |
| `member1` | MEMBER |

---

## 5. Feature Guide

### Admin
- **Dashboard** — totals for members, coaches, active classes.
- **Members** — list (filter by status), add, change ACTIVE/INACTIVE/SUSPENDED.
- **Coaches** — list, add, edit.
- **Classes** — list, add, edit, **clone** (Prototype: duplicates the template).
- **Schedules** — list, add, **clone** (next-week copy), delete.
- **Packages** — list, add, edit, **clone at +20% price** (Prototype).

### Coach
- **Dashboard** — assigned classes + weekly schedule.
- **My Classes** — pick a class to see its enrolled members. Requesting a class
  that isn't yours returns **403** (IDOR prevention, enforced server-side).

### Member
- **Dashboard** — membership status, enrolled classes, weekly schedule.
- **Classes** — browse active classes, enroll (blocked when full or already
  enrolled), cancel.
- **Profile** — update name/phone/address; change password (requires current
  password + strength policy).

### Registration
Self-service at the Register page creates a **MEMBER**. Admin/Coach accounts are
created by an admin. Duplicate username/email returns one generic message
(account-enumeration prevention).

---

## 6. Security Features

| Feature | Behavior |
|---------|----------|
| **Password hashing** | BCrypt cost 12 — never plaintext |
| **JWT auth** | Signed HS256 tokens in the `Authorization` header; stateless |
| **Role-based access** | `[Authorize(Roles = ...)]` on every protected endpoint |
| **Object-level access (IDOR)** | A coach can only read classes they own |
| **Brute-force lockout** | 5 failed logins in 15 min locks the account |
| **Password strength policy** | ≥ 8 chars, ≥ 1 letter and ≥ 1 digit |
| **Password-change re-auth** | Must re-enter the current password |
| **Account-enumeration prevention** | Generic error on login/register conflicts |
| **Security headers** | `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy`, `Strict-Transport-Security` on every response |
| **SQL-injection safe** | All data access is parameterised EF Core LINQ |
| **No hardcoded secrets** | DB credentials from env vars; JWT key from config/env |

> **Note on CSRF:** because auth travels in the `Authorization` header (not a
> cookie), the browser never auto-attaches it to forged cross-site requests, so
> classic CSRF does not apply. The original app's session + `_csrf` token model
> was replaced by stateless JWT during the migration.

---

## 7. Troubleshooting

- **"Could not connect to database"** — confirm SQL Server is on `localhost:1433`,
  the `DB_*` vars match, and `database.sql` has been run.
- **Account locked** — wait 15 min, or clear rows:
  `DELETE FROM login_attempts WHERE username = '...';`
- **401 in the SPA** — the token expired (30 min default); log in again.
- **CORS error** — the API allows `http://localhost:5173` by default; add other
  origins under `Cors:Origins` in `appsettings.json`.
- **Password reset** — no self-service reset; an admin updates the `users` row
  with a BCrypt (cost 12) hash.

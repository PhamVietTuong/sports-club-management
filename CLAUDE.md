# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Layout

This is a sports-club management app split into two projects:

- **`backend/`** — ASP.NET Core 9 Web API (C#), EF Core + SQL Server, JWT auth.
  Solution: `backend/SportsClub.sln`; project: `backend/SportsClub.Api/`.
- **`frontend/`** — React 18 + TypeScript SPA (Vite), talks to the API over JSON.

The app was migrated from a Java Servlet/JSP/JDBC monolith; the SQL Server schema
and the three roles (ADMIN / COACH / MEMBER) are unchanged.

## Build & Run

```bash
# Backend (http://localhost:5097, OpenAPI at /openapi/v1.json in Development)
cd backend
dotnet run --project SportsClub.Api

# Frontend (http://localhost:5173, proxies /api → backend)
cd frontend
npm install      # first time only
npm run dev
```

Database: run `backend/SportsClub.Api/database.sql` against SQL Server 2019+ once
before first launch. Sample accounts (password `Password123`): `admin` (ADMIN),
`coach1` (COACH), `member1` (MEMBER).

There is no automated test suite. Verification is manual: start both servers,
log in as each role, and exercise the dashboards. Requires .NET 9 SDK, Node 18+,
SQL Server 2019+.

## Critical Constraints

- **SQL Server dialect only.** `database.sql` uses `IDENTITY(1,1)`, `NVARCHAR`,
  `DATETIME2`/`GETDATE()`, `GO` batches — never MySQL syntax. EF Core entities map
  to the existing snake_case tables/columns in `Data/AppDbContext.cs`; preserve
  those column names when changing the model.
- **EF Core, not raw ADO.NET.** Data access goes through the repository classes in
  `Repositories/` (the DAO layer). EF parameterises every LINQ query — never build
  SQL by string concatenation.
- **JWT bearer auth, stateless.** The SPA stores the token and sends it in the
  `Authorization` header. There are no server sessions and no cookies, so classic
  CSRF does not apply. Authorize endpoints with `[Authorize(Roles = UserRole.X)]`.

## Architecture

```
React SPA  ──fetch(JWT)──>  ASP.NET Core controllers  ──>  repositories (DAO)  ──>  EF Core  ──>  SQL Server
```

### Role-based routing
| Prefix | Required role | Controllers |
|--------|--------------|-------------|
| `/api/admin/*`  | ADMIN  | `Controllers/Admin/` |
| `/api/coach/*`  | COACH  | `CoachController` |
| `/api/member/*` | MEMBER | `MemberController` |
| `/api/auth/*`   | public | `AuthController` |

### Design patterns (assignment requirement — keep them)
- **Singleton** — `Patterns/Singleton/DatabaseConfig.cs` builds the connection
  string once (double-checked locking); `Program.cs` uses it for EF Core.
- **Prototype** — `Patterns/Prototype/ISportClubPrototype<T>`; entities implement
  `Clone()` (via `MemberwiseClone`). Used by the admin clone endpoints
  (classes, packages at +20% price, schedules, member templates).
- **Iterator** — `Patterns/Iterator/` (`ClubCollection<T>` / `ClubIterator<T>`).
  List endpoints (members, classes, schedules) traverse via the iterator instead
  of returning the raw list, mirroring the original Java servlets.
- **DAO** — `Repositories/*Repository.cs`, one per table.

### Security invariants (Web Security course rubric)
- **Password hashing** — BCrypt cost 12 (`Security/PasswordHasher.cs`); existing
  `$2a$` hashes from the Java app still verify.
- **Brute-force lockout** — 5 failed logins in 15 min locks the account
  (`login_attempts` table, checked in `AuthController.Login`).
- **Password policy** — min 8 chars, ≥1 letter and ≥1 digit (`Security/PasswordPolicy.cs`).
- **Account-enumeration prevention** — register/login return one generic message.
- **IDOR prevention** — `CoachController.ClassDetail` returns 403 unless the class
  belongs to the calling coach.
- **Password change** — requires the current password to be re-verified.
- **Security headers** — `Security/SecurityHeadersMiddleware.cs` (X-Frame-Options,
  X-Content-Type-Options, HSTS, etc.) runs on every response.

### Adding a new feature
1. Add the table/columns to `database.sql` and the matching EF entity in
   `Models/Entities/` + mapping in `Data/AppDbContext.cs`.
2. Add a repository in `Repositories/` (parameterised LINQ only).
3. Add a controller (or action) under the right role route; annotate with
   `[Authorize(Roles = ...)]` and define request/response DTOs in `Models/Dtos/`.
4. Add the TypeScript type in `frontend/src/api/types.ts` and a page/call in
   `frontend/src/pages/`.

## Reference Docs
- `README.md` — setup + end-user feature walkthrough (all three roles).
- `SECURITY.md` — security-control rationale.
- `backend/SportsClub.Api/database.sql` — authoritative schema + seed data.

# Security Controls — Sports Club Management

This document maps the nine web-security categories required by the assignment to the exact
places they are enforced in the **.NET backend**. Paths are relative to `backend/SportsClub.Api/`.

> **Architecture note.** The app is now a React SPA + ASP.NET Core JSON API with **stateless
> JWT** authentication (replacing the original Java server-side sessions). Every request runs
> through `SecurityHeadersMiddleware`, then JWT authentication, then `[Authorize]` /
> `[Authorize(Roles = ...)]` authorization on the controller/action.

## Summary

| # | Category | Status | Primary location |
|---|----------|--------|------------------|
| 1 | HTML Injection | ✅ Mitigated | React auto-escaping (JSX), security headers |
| 2 | XSS | ✅ Mitigated | React text binding (no `dangerouslySetInnerHTML`), `SecurityHeadersMiddleware` |
| 3 | SQL Injection | ✅ Mitigated | `Repositories/*` — 100% parameterised EF Core LINQ |
| 4 | Authentication Vulnerabilities | ✅ Mitigated | `Security/PasswordHasher`, `PasswordPolicy`, `JwtTokenService`, `AuthController`, RBAC + IDOR |
| 5 | Session & Token Security | ✅ Mitigated | `JwtTokenService` (short-lived signed JWT), `Program.cs` validation params |
| 6 | CSRF | ✅ Mitigated by design | Bearer token in `Authorization` header — not a cookie, so not auto-sent cross-site |
| 7 | File Upload Security | ⚪ N/A (no upload feature) | — (guidance below) |
| 8 | Path Traversal & Command Injection | ⚪ N/A (no file/command surface) | — (guidance below) |
| 9 | API & JWT | ✅ Mitigated | `Program.cs` JWT bearer config, `JwtTokenService`, `[Authorize]` |

---

## 1. HTML Injection & 2. XSS

**Threat:** stored markup or `<script>` rendered into the page.

**Mitigation — output encoding by default.** The frontend is React: all dynamic values are
rendered as text nodes via JSX `{value}`, which HTML-escapes automatically. There is **no**
use of `dangerouslySetInnerHTML` anywhere in `frontend/src/`. The API returns JSON only (never
HTML), so there is no server-side template injection surface. `SecurityHeadersMiddleware` adds
`X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, and `X-XSS-Protection: 1; mode=block`
as defense-in-depth.

## 3. SQL Injection

**Threat:** user input concatenated into SQL.

**Mitigation — parameterised queries everywhere.** All data access goes through the repository
classes in `Repositories/` using EF Core LINQ (`UserRepository`, `MemberRepository`,
`CoachRepository`, `ClassRepository`, `ScheduleRepository`, `EnrollmentRepository`,
`PackageRepository`). EF Core compiles every LINQ predicate to a **parameterised** SQL command —
user values are bound, never interpolated. No raw SQL string concatenation exists. Numeric route
ids are typed (`int`) and model-bound, so they cannot carry SQL.

## 4. Authentication Vulnerabilities

- **Password storage** — `Security/PasswordHasher.cs` uses BCrypt **cost 12**
  (`BCrypt.Net-Next`); the existing `$2a$` hashes from the Java app verify unchanged.
- **Password strength policy** — `Security/PasswordPolicy.cs`: ≥ 8 chars incl. ≥ 1 letter and
  ≥ 1 digit. Enforced in `AuthController.Register`, `AdminMembersController`,
  `AdminCoachesController`, and the password change in `MemberController.UpdateProfile`.
- **Brute-force lockout** — every attempt is logged to `login_attempts`
  (`UserRepository.LogLoginAttemptAsync`); `CountRecentFailedAttemptsAsync` counts failures in
  the last 15 minutes and `AuthController.Login` rejects after **5** failures.
- **Password change re-authentication** — `MemberController.UpdateProfile` requires the
  **current password** (BCrypt-verified) before accepting a new one.
- **Account-enumeration prevention** — register/login return a single generic message for a
  taken username *or* email and for any bad credential.

**Access control:**
- **RBAC** — every protected endpoint carries `[Authorize(Roles = UserRole.X)]`; the JWT's role
  claim is validated on each request. Wrong role → HTTP 403.
- **Object-level / IDOR** — `CoachController.ClassDetail` returns **403** unless the requested
  class's `CoachId` equals the calling coach's id; member endpoints resolve data from the JWT's
  identity, never from a client-supplied owner id.

## 5. Session & Token Security

- **Stateless JWT** — `JwtTokenService` issues an HS256-signed token at login carrying the user
  id, username, role, and profile id. No server session state, so there is no session-fixation
  surface.
- **Short lifetime** — 30-minute expiry (`Jwt:ExpiryMinutes`); the SPA is bounced to login on a
  401 (`api/client.ts` response interceptor).
- **Validation** — `Program.cs` validates issuer, audience, lifetime, and signing key with a
  1-minute clock skew.
- **Logout** — the client discards the token; `AuthController.Logout` exists for symmetry.
- **HSTS** — `SecurityHeadersMiddleware` sends `Strict-Transport-Security`.

## 6. CSRF

**Mitigated by design.** Authentication travels as a bearer token in the `Authorization`
header (stored in `localStorage`), not in a cookie. Browsers do not automatically attach the
`Authorization` header to cross-site requests, so a forged cross-site request carries no
credentials — the classic cookie-based CSRF vector does not exist. (The original app's
synchronizer `_csrf` token + `SameSite` cookie model is therefore no longer needed.)

> If cookie-based auth is ever reintroduced, add anti-forgery tokens
> (`AddAntiforgery` / `[ValidateAntiForgeryToken]`) and `SameSite=Strict` cookies.

## 7. File Upload Security — ⚪ Not Applicable

There is no file-upload functionality (no multipart endpoints, no `IFormFile` usage).
**If added** (e.g. member avatars): allow-list extensions/MIME types, validate magic bytes,
randomize server-side filenames, cap size, store outside the web root, and serve via a
controlled endpoint — never trust the client filename.

## 8. Path Traversal & Command Injection — ⚪ Not Applicable

No file is opened from user input (no `File.Open`/`Path.Combine` on request data) and no OS
process is spawned (no `Process.Start`). **If added:** canonicalize and confine paths to a fixed
base directory (reject `..`/absolute paths) and never pass user input to a shell — use
parameterised process arguments.

## 9. API & JWT

The app **is** a JWT-secured JSON API, configured in `Program.cs`:
- Tokens are HS256-signed with a server-held key (`Jwt:Key`, overridable via the `Jwt__Key`
  env var — never commit a real key).
- The bearer handler verifies the **signature and algorithm** (rejecting unsigned/`none`
  tokens) plus `exp`, `iss`, and `aud`.
- Tokens are short-lived (30 min) and intended for HTTPS transport.
- CORS is locked to the configured SPA origin(s) (`Cors:Origins`), not `*`.

---

## Verification (manual)

Start both servers (`README.md` §4), then:
- §3 — log in with username `' OR '1'='1` → rejected, no bypass.
- §1/§2 — set a member full name to `<script>alert(1)</script>`, save, reload → rendered as
  inert text, no alert.
- §4 — 5 bad logins → lockout message; change password with a wrong current password → rejected;
  register an existing username/email → single generic message.
- §4 (IDOR) — as `coach1`, `GET /api/coach/classes/{id}` for another coach's class → 403.
- §5/§9 — call any `/api/admin/*` endpoint without a token, or with a tampered/expired token →
  401; with a MEMBER token → 403.

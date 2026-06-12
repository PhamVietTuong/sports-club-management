# Security Controls — Sports Club Management

This document maps the nine web-security categories required by the assignment to the exact
places they are enforced in the codebase. All paths are relative to
`sports-club-management/src/main/`.

> **Architecture note.** Requests pass through a filter chain before reaching any servlet:
> `SecurityHeadersFilter` → `CsrfFilter` → `AuthenticationFilter` → `AuthorizationFilter`
> (all in `java/com/sportsclub/filter/`). All JSP views live under `WEB-INF/views/` and are
> never directly reachable — servlets forward to them via `RequestDispatcher`.

## Summary

| # | Category | Status | Primary location |
|---|----------|--------|------------------|
| 1 | HTML Injection | ✅ Mitigated | `util/HtmlUtils.java`, JSP `<c:out>`, CSP header |
| 2 | XSS (Cross-Site Scripting) | ✅ Mitigated | JSP `<c:out>`, `SecurityHeadersFilter` (CSP), `HtmlUtils` |
| 3 | SQL Injection | ✅ Mitigated | `dao/*.java` — 100% `PreparedStatement` |
| 4 | Authentication Vulnerabilities | ✅ Mitigated | `BCryptUtil`, `PasswordPolicy`, `LoginServlet`, `UserDAO`, RBAC + IDOR filters/servlets |
| 5 | Session & Cookie Security | ✅ Mitigated | `LoginServlet`, `LogoutServlet`, `web.xml`, `META-INF/context.xml` |
| 6 | CSRF | ✅ Mitigated | `CsrfFilter`, `CsrfUtils`, `SameSite=Strict`, form tokens |
| 7 | File Upload Security | ⚪ N/A (no upload feature) | — (guidance below) |
| 8 | Path Traversal & Command Injection | ⚪ N/A (no file/command surface) | — (guidance below) |
| 9 | API & JWT | ⚪ N/A (session-based, no API/JWT) | — (guidance below) |

---

## 1. HTML Injection

**Threat:** attacker stores markup (e.g. `<b>`, `<iframe>`, `<img>`) in a field such as
full name or address, which is then rendered into the page and alters its structure.

**Mitigation — output encoding on every dynamic value.**
- All JSP views render dynamic data through JSTL `<c:out value="${...}"/>`, which HTML-escapes
  by default. Example: `WEB-INF/views/member/profile.jsp` outputs `member.fullName`,
  `member.email`, `member.address`, etc. exclusively via `<c:out>`. There is **no** raw
  `${...}` interpolated into HTML body and **no** `<%= %>` scriptlet output anywhere in
  `WEB-INF/views/`.
- For values escaped in Java (rather than JSP), `util/HtmlUtils.escapeHtml()` encodes
  `& < > " ' /`.

Because the data is neutralized at the point of output, stored markup is displayed as inert
text rather than being parsed as HTML.

## 2. XSS (Cross-Site Scripting)

**Threat:** injected `<script>` or event-handler attributes execute in a victim's browser.

**Mitigation — layered:**
1. **Output encoding** (same as §1): `<c:out>` turns `<script>` into `&lt;script&gt;`, so it
   never executes. This is the primary defense.
2. **Content-Security-Policy** — `filter/SecurityHeadersFilter.java` sets a CSP that restricts
   scripts/styles to `'self'` plus the explicit CDN origins, blocking inline and third-party
   script injection even if encoding were bypassed.
3. **Supporting headers** set on every response by the same filter:
   `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `X-XSS-Protection: 1; mode=block`.
4. **Cookie theft minimized** — the session cookie is `HttpOnly` (see §5), so script cannot
   read it even if XSS occurred.

## 3. SQL Injection

**Threat:** user input concatenated into SQL alters the query.

**Mitigation — parameterized queries everywhere.** Every database call in
`java/com/sportsclub/dao/` (`UserDAO`, `MemberDAO`, `CoachDAO`, `ClassDAO`, `ScheduleDAO`,
`EnrollmentDAO`, `PackageDAO`) uses `PreparedStatement` with bound `?` parameters. There is
**no** string concatenation of user input into SQL — the `+` operators in the DAOs only join
static SQL literal fragments across source lines.

Representative example — `UserDAO.findByUsername`:
```java
String sql = "SELECT * FROM users WHERE username = ?";
try (Connection conn = getConn();
     PreparedStatement ps = conn.prepareStatement(sql)) {
    ps.setString(1, username);   // value bound, never interpolated
    ...
}
```
Numeric inputs (IDs) are additionally parsed with `Integer.parseInt(...)` in the servlets
before use, so they cannot carry SQL.

## 4. Authentication Vulnerabilities

**Mitigations:**
- **Password storage** — `util/BCryptUtil.java` hashes with BCrypt **cost factor 12**; plaintext
  passwords are never stored. Verification uses `BCrypt.checkpw` (timing-safe).
- **Password strength policy** — `util/PasswordPolicy.java` requires ≥ 8 characters including
  at least one letter and one digit. Enforced in `RegisterServlet`, admin
  `MemberManagementServlet`, and the member password change in `MemberProfileServlet`.
- **Brute-force lockout** — every attempt is logged to `login_attempts`
  (`UserDAO.logLoginAttempt`); `UserDAO.countRecentFailedAttempts` counts failures in the last
  15 minutes and `LoginServlet` rejects login after **5** failures.
- **Password change re-authentication** — `MemberProfileServlet` requires the **current
  password** (verified with BCrypt) before accepting a new one, so an unattended/hijacked
  session cannot silently reset the password.
- **Account-enumeration prevention** — `RegisterServlet` returns a single generic message for a
  taken username *or* email, so an attacker cannot probe which accounts exist.
- **Generic error messages** — login failures and server errors never reveal internals
  (`LoginServlet` catch block returns a generic message).

**Access control (closely related):**
- **RBAC** — `filter/AuthenticationFilter` requires a valid session for `/admin/*`, `/coach/*`,
  `/member/*`; `filter/AuthorizationFilter` enforces that the URL prefix matches the user's role
  (else HTTP 403).
- **Object-level / IDOR** — beyond URL RBAC, `servlet/coach/CoachClassServlet` verifies the
  requested `classId` actually belongs to the logged-in coach
  (`selected.getCoachId() == coach.getId()`) before exposing the class and its enrolled members.
  Member-scoped servlets resolve data from the session user, not from client-supplied IDs.

## 5. Session & Cookie Security

**Mitigations:**
- **Session fixation** — on successful login `LoginServlet` invalidates the pre-auth session and
  creates a fresh one before storing the user, so a fixed session id cannot be reused.
- **Secure logout** — `LogoutServlet` calls `session.invalidate()` (full destruction, not
  attribute removal).
- **Cookie flags** (`WEB-INF/web.xml` `<cookie-config>`): `HttpOnly=true` (JS cannot read the
  session cookie); `Secure` = `false` for local HTTP dev — **set to `true` in production (HTTPS)**.
- **SameSite=Strict** — `webapp/META-INF/context.xml` configures Tomcat's
  `Rfc6265CookieProcessor` with `sameSiteCookies="strict"`, so `JSESSIONID` is never sent on
  cross-site requests (a strong second CSRF layer).
- **Inactivity timeout** — 30 minutes, set both in `web.xml` and on the new session in
  `LoginServlet` (`setMaxInactiveInterval(30 * 60)`).
- **HSTS** — `SecurityHeadersFilter` sends `Strict-Transport-Security` to enforce HTTPS.

## 6. CSRF (Cross-Site Request Forgery)

**Mitigations — synchronizer token + SameSite:**
- **Per-session token** — `util/CsrfUtils.generateToken()` stores a UUID in the session;
  servlets expose it as request attribute `csrfToken` in their `doGet`.
- **Token in every form** — each POST form includes
  `<input type="hidden" name="_csrf" value="${csrfToken}">`.
- **Server-side validation** — `filter/CsrfFilter` validates `_csrf` on **every POST** for
  `/admin/*`, `/coach/*`, `/member/*`, `/login`, `/register`; a mismatch returns HTTP 403.
- **Constant-time comparison** — `CsrfUtils.isValidToken` compares tokens with
  `MessageDigest.isEqual(...)` to avoid a timing side channel.
- **SameSite=Strict cookie** (see §5) blocks the forged request from carrying the session at all.

---

## 7. File Upload Security — ⚪ Not Applicable

The application has **no file-upload functionality**: there are no `@MultipartConfig` servlets,
no `request.getPart(...)`/`Part` usage, and no upload forms. There is therefore no attack
surface for malicious file upload (web-shell, content-type spoofing, oversized files).

**If an upload feature is added** (e.g. member avatars), apply: allow-list of extensions and
MIME types, magic-byte validation, randomized server-side filenames, a size cap, storage
**outside** the web root (or in the DB), and serving via a controlled download endpoint — never
trust the client-supplied filename.

## 8. Path Traversal & Command Injection — ⚪ Not Applicable

There is **no surface** for either:
- **Path traversal** — the app never opens files from user input. There is no `new File(...)`,
  `FileInputStream`, or `getResourceAsStream` driven by request parameters. JSPs are fixed
  server-side paths under `WEB-INF/views/` reached only via `RequestDispatcher`, so a client
  cannot request an arbitrary view, and `../` sequences have nothing to act on.
- **Command injection** — the app never executes OS commands: there is no `Runtime.exec`,
  `ProcessBuilder`, or any shell invocation. (The only `RuntimeException` references are
  exception handling in model `clone()` methods, unrelated to OS commands.)

**If file access or external processes are ever added:** canonicalize and confine paths to a
fixed base directory (reject `..` / absolute paths), and never pass user input to a shell —
use parameterized process arguments instead of string-built command lines.

## 9. API & JWT — ⚪ Not Applicable

The application is a **server-rendered MVC app using session-based authentication**, not a
token API. There is **no REST/JSON API, no JWT library, and no bearer-token handling** — auth
state lives in the server-side `HttpSession`, and every protected route is gated by the servlet
filter chain (§4–§6).

**If a JWT-based API is added later:** sign with a strong server-held secret/asymmetric key,
always verify the signature and `alg` (reject `none`), validate `exp`/`iss`/`aud`, keep tokens
short-lived with refresh rotation, transmit only over HTTPS, and apply the same CSRF
consideration (prefer the `Authorization` header over cookies for API tokens).

---

## Verification

- **Build:** `mvn clean package` in `sports-club-management/` → `target/sports-club-management.war`.
- **Manual checks (deploy to Tomcat 10):**
  - §3 — submit `' OR '1'='1` as username/password → rejected, no auth bypass.
  - §1/§2 — set full name to `<script>alert(1)</script>`, save, reload profile → rendered as
    text, no alert; confirm CSP header is present in the response.
  - §4 — 5 bad logins → lockout message; try changing password with a wrong current password →
    rejected; register with an existing username/email → single generic message.
  - §4 (IDOR) — as `coach1`, request `/coach/classes?classId=<another coach's class>` → access
    denied, no member data shown.
  - §5 — confirm `JSESSIONID` cookie has `HttpOnly` and `SameSite=Strict`; verify the session id
    changes after login.
  - §6 — submit any POST with a missing/altered `_csrf` value → HTTP 403.

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Layout

This repo root (`DoAn/`) is a wrapper. The actual application — a Maven Java web project — lives in the **`sports-club-management/`** subdirectory. All build commands, source, and the SQL schema are under there.

**`sports-club-management/CLAUDE.md` contains the full architecture, security invariants, design-pattern usage, and "adding a new feature" workflow. Read it before making changes** — it is the authoritative guide and this root file only summarizes.

## Build & Run

```bash
cd sports-club-management
mvn clean package          # → target/sports-club-management.war
# Deploy WAR to Tomcat 10 webapps/; app at http://localhost:8080/sports-club-management/
```

There is no test suite. Verification is manual: build the WAR, deploy to Tomcat 10, and exercise the role dashboards. Requires JDK 11+, Tomcat **10.x** (Jakarta EE — not Tomcat 9), SQL Server 2019+, Maven 3.6+.

## Critical Constraints

- **Jakarta, not javax**: all servlet/filter/JSP imports must be `jakarta.servlet.*`. Tomcat 10 will not load `javax.servlet.*` code.
- **SQL Server dialect only** in `src/main/resources/database.sql` and DAOs: `IDENTITY(1,1)`, `NVARCHAR`, `DATETIME2`/`GETDATE()`, `OFFSET … FETCH NEXT`, `GO` batch separator — never MySQL syntax.
- **No Spring / no framework** — plain Servlet + JSP + JDBC. New routes are wired with `@WebServlet` annotations, not `web.xml`.
- Run `database.sql` against SQL Server before first launch; connection settings are hardcoded in `util/DatabaseConnection.java`.

## Reference Docs

- `sports-club-management/CLAUDE.md` — architecture & conventions (primary reference)
- `USER_MANUAL.md` — end-user feature walkthrough and troubleshooting

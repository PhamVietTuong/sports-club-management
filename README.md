# Sports Club Management System — User Manual

**Version:** 1.0  
**Platform:** Java Servlet + JSP + SQL Server (Tomcat 10)  
**URL:** `http://localhost:8080/sports-club-management/`

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Prerequisites & Installation](#2-prerequisites--installation)
3. [Database Setup](#3-database-setup)
4. [Deploying the Application](#4-deploying-the-application)
5. [Logging In](#5-logging-in)
6. [Admin Guide](#6-admin-guide)
7. [Coach Guide](#7-coach-guide)
8. [Member Guide](#8-member-guide)
9. [Account Registration](#9-account-registration)
10. [Security Features](#10-security-features)
11. [Troubleshooting](#11-troubleshooting)

---

## 1. System Overview

The Sports Club Management System is a role-based web application for managing a sports club's members, coaches, training classes, schedules, and membership packages.

### Roles

| Role | Access Prefix | Responsibilities |
|------|--------------|-----------------|
| **ADMIN** | `/admin/*` | Full system control — users, classes, schedules, packages |
| **COACH** | `/coach/*` | View assigned classes and training schedules |
| **MEMBER** | `/member/*` | Enroll in classes, view schedule, manage profile |

---

## 2. Prerequisites & Installation

### Required Software

| Software | Minimum Version |
|----------|----------------|
| Java JDK | 11 or higher |
| Apache Tomcat | 10.x (Jakarta EE — **not** Tomcat 9) |
| SQL Server | 2019 or higher |
| Maven | 3.6+ |

### Build the WAR File

```bash
cd sports-club-management
mvn clean package
```

The output WAR is located at:

```
sports-club-management/target/sports-club-management.war
```

---

## 3. Database Setup

### Step 1 — Configure Connection

Edit `src/main/java/com/sportsclub/util/DatabaseConnection.java` and update the credentials before building:

```
Host:     localhost:1433
Database: SportsClubDB
User:     sa
Password: YourPassword123
```

### Step 2 — Run the Schema Script

Open SQL Server Management Studio (or `sqlcmd`) and run:

```
src/main/resources/database.sql
```

This creates the `SportsClubDB` database with all tables, stored procedures, views, and sample data.

### Tables Created

| Table | Description |
|-------|-------------|
| `users` | Authentication and role for all accounts |
| `members` | Member profile and membership status |
| `coaches` | Coach profile and specialization |
| `training_packages` | Membership package plans |
| `training_classes` | Classes with assigned coach and capacity |
| `schedules` | Weekly schedule for each class |
| `enrollments` | Member-to-class enrollment records |
| `login_attempts` | Brute-force protection log |

---

## 4. Deploying the Application

1. Copy the WAR to Tomcat's `webapps/` directory:

   ```
   cp target/sports-club-management.war $TOMCAT_HOME/webapps/
   ```

2. Start (or restart) Tomcat.

3. Open a browser and navigate to:

   ```
   http://localhost:8080/sports-club-management/
   ```

   The root page automatically redirects to the login screen.

---

## 5. Logging In

### Login Page — `/login`

1. Enter your **Username** and **Password**.
2. Click **Login**.
3. On success you are redirected to your role's dashboard:
   - ADMIN ? `/admin/dashboard`
   - COACH ? `/coach/dashboard`
   - MEMBER ? `/member/dashboard`

### Sample Accounts (password: `Password123`)

| Username | Role |
|----------|------|
| `admin` | ADMIN |
| `coach1` | COACH |
| `member1` | MEMBER |

### Login Errors

| Message | Cause |
|---------|-------|
| "Invalid username or password" | Wrong credentials |
| "Account locked. Try again later." | 5 failed attempts within 15 minutes |

### Logging Out

Click **Logout** in the navigation bar. Your session is fully invalidated immediately.

---

## 6. Admin Guide

After login the admin is taken to the **Admin Dashboard** at `/admin/dashboard`.

### 6.1 Dashboard

The dashboard provides a summary overview:

- Total active members
- Total coaches
- Total active training classes
- Class enrollment statistics (from the `vw_ClassEnrollmentStats` view)

---

### 6.2 Member Management — `/admin/members`

**View Members**

The members list shows: full name, username, email, gender, join date, expiry date, and status (ACTIVE / INACTIVE / SUSPENDED).

**Add a Member**

1. Click **Add Member**.
2. Fill in the form:
   - Username, Email, Password (min 8 characters)
   - Full Name, Gender, Date of Birth, Address
   - Package (optional) and Expiry Date
3. Click **Save**.

**Edit a Member**

1. Click the **Edit** button next to a member.
2. Update the desired fields.
3. Click **Update**.

**Delete a Member**

1. Click the **Delete** button next to a member.
2. Confirm the deletion prompt.

**Filter by Status**

Use the status filter dropdown to view ACTIVE, INACTIVE, or SUSPENDED members.

---

### 6.3 Coach Management — `/admin/coaches`

**View Coaches**

The list shows: full name, email, specialization, years of experience, and salary.

**Add a Coach**

1. Click **Add Coach**.
2. Fill in:
   - Username, Email, Password
   - Full Name, Specialization, Years of Experience, Bio, Salary
3. Click **Save**.

**Edit / Delete a Coach**

Same procedure as member management — click **Edit** or **Delete** next to the coach row.

---

### 6.4 Class Management — `/admin/classes`

**View Classes**

The list shows: class name, assigned coach, capacity, current enrollment, level, and status.

**Add a Class**

1. Click **Add Class**.
2. Fill in:
   - Name, Coach (dropdown), Capacity
   - Level: BEGINNER / INTERMEDIATE / ADVANCED
   - Description
3. Click **Save**.

**Duplicate a Class (Prototype Pattern)**

1. Click **Clone** next to an existing class.
2. A copy is created with the same settings — edit the name and coach before saving.

**Activate / Deactivate a Class**

Toggle the **Active** checkbox in the edit form. Inactive classes do not appear in the member enrollment view.

---

### 6.5 Schedule Management — `/admin/schedules`

**View Schedules**

Each schedule entry shows: class name, day of week, start/end time, and room.

**Add a Schedule**

1. Click **Add Schedule**.
2. Choose:
   - Class (dropdown)
   - Day of Week (MONDAY – SUNDAY)
   - Start Time and End Time (HH:MM)
   - Room (e.g., Studio A, Gym Floor)
3. Check **Repeat Weekly** to make it a recurring session.
4. Click **Save**.

**Clone Weekly Schedule (Prototype Pattern)**

Click **Clone Week** on an existing schedule to duplicate all sessions for the same class shifted by one week.

**Edit / Delete**

Click **Edit** or **Delete** next to a schedule row.

---

### 6.6 Package Management — `/admin/packages`

**View Packages**

The list shows: name, duration (months), price, max classes included, and active status.

**Add a Package**

1. Click **Add Package**.
2. Fill in:
   - Name (e.g., Basic, Standard, Premium)
   - Duration in Months
   - Price
   - Max Classes allowed under the package
   - Description
3. Click **Save**.

**Duplicate a Package at 120% Price (Prototype Pattern)**

Click **Clone** next to a package to create a copy with the price automatically increased by 20%.

**Deactivate a Package**

Uncheck **Active** in the edit form. Inactive packages are hidden from member views.

---

## 7. Coach Guide

### 7.1 Dashboard — `/coach/dashboard`

The coach dashboard shows:

- Number of active classes assigned to this coach
- Upcoming schedule for the week

### 7.2 My Classes — `/coach/classes`

Displays all training classes assigned to the logged-in coach, including:

- Class name and level
- Capacity and current enrollment count
- Weekly schedule (days, times, rooms)

Coaches have **read-only** access to class and schedule information. To make changes, contact an admin.

---

## 8. Member Guide

### 8.1 Dashboard — `/member/dashboard`

The member dashboard shows:

- Current membership package and expiry date
- Membership status (ACTIVE / INACTIVE / SUSPENDED)
- List of currently enrolled classes

### 8.2 Browse & Enroll in Classes — `/member/classes`

**Browse Classes**

The class list shows all active classes:

- Class name, level, coach name
- Remaining slots (capacity - enrolled)
- Weekly schedule

**Enroll in a Class**

1. Find a class with available slots.
2. Click **Enroll**.
3. Enrollment is confirmed immediately. The class now appears on your dashboard.

**Cancel Enrollment**

1. Find the class in the list (it will show "Enrolled" status).
2. Click **Cancel Enrollment**.
3. Your slot is released.

> **Note:** You cannot enroll in the same class twice. Duplicate enrollment is blocked automatically.

### 8.3 My Profile — `/member/profile`

View and update your personal details:

- Full Name, Gender, Date of Birth, Address
- Phone Number
- Email (read-only — contact admin to change)

Click **Update Profile** to save changes.

---

## 9. Account Registration

### Self-Registration — `/register`

New members can create their own account:

1. Go to `/register` from the login page.
2. Fill in:
   - Username (unique, no spaces)
   - Email (unique)
   - Password (minimum 8 characters)
   - Full Name, Gender, Date of Birth, Address, Phone
3. Click **Register**.
4. On success you are redirected to the login page.

> Registered users are assigned the **MEMBER** role by default. Admin and Coach accounts must be created by an existing admin.

---

## 10. Security Features

The following security controls are enforced automatically — no user action required.

| Feature | Behavior |
|---------|----------|
| **Password hashing** | All passwords stored as BCrypt (cost 12) — never in plain text |
| **CSRF protection** | Every form includes a hidden token; mismatched tokens are rejected with HTTP 403 |
| **XSS prevention** | All output in JSP views is escaped — malicious scripts cannot be injected |
| **Brute-force lockout** | After 5 failed login attempts within 15 minutes, the account is locked |
| **Session fixation protection** | A new session ID is issued on every successful login |
| **Role-based access control** | Attempting to access `/admin/*` as a MEMBER returns HTTP 403 |
| **Security headers** | `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection` added to every response |
| **Secure logout** | Full session invalidation — no residual session data |

---

## 11. Troubleshooting

### Application shows a blank page or 404

- Verify Tomcat 10 is running (not Tomcat 9).
- Confirm the WAR was copied to `$TOMCAT_HOME/webapps/`.
- Check `$TOMCAT_HOME/logs/catalina.out` for startup errors.

### "Could not connect to database"

- Confirm SQL Server is running on `localhost:1433`.
- Verify credentials in `DatabaseConnection.java` match your SQL Server setup.
- Ensure the `SportsClubDB` database was created by running `database.sql`.

### "Account locked. Try again later."

Wait 15 minutes, then try again. If you need immediate access, an admin can clear the `login_attempts` table:

```sql
DELETE FROM login_attempts WHERE username = 'your_username';
```

### "403 Forbidden" after login

Your account's role does not match the URL you are trying to access. Log out and log back in with the correct account.

### Class shows 0 available slots

The class has reached its capacity. Contact an admin to increase the capacity or wait for another member to cancel.

### Password reset

There is no self-service password reset in this version. An admin must update the password hash directly in the `users` table using a BCrypt-hashed value (cost 12).

-- ============================================================
-- Sports Club Management Database — SQL Server 2019+
-- Uses SQL Server syntax: IDENTITY, NVARCHAR, DATETIME2,
-- GETDATE(), GO batch separator, OFFSET/FETCH for pagination.
-- Run this once before starting the .NET API.
-- ============================================================

CREATE DATABASE SportsClubDB;
GO
USE SportsClubDB;
GO

-- Users table — shared authentication for all roles
CREATE TABLE users (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    username      NVARCHAR(50)  NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    email         NVARCHAR(100) NOT NULL UNIQUE,
    phone         NVARCHAR(20),
    role          NVARCHAR(10)  NOT NULL CHECK (role IN ('ADMIN','COACH','MEMBER')),
    created_at    DATETIME2     DEFAULT GETDATE()
);
GO

CREATE TABLE members (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    user_id       INT           NOT NULL FOREIGN KEY REFERENCES users(id),
    full_name     NVARCHAR(100) NOT NULL,
    gender        NVARCHAR(10)  CHECK (gender IN ('MALE','FEMALE','OTHER')),
    date_of_birth DATE,
    address       NVARCHAR(255),
    package_id    INT           DEFAULT 0,
    join_date     DATE          DEFAULT CAST(GETDATE() AS DATE),
    expiry_date   DATE,
    status        NVARCHAR(15)  DEFAULT 'ACTIVE'
                                CHECK (status IN ('ACTIVE','INACTIVE','SUSPENDED'))
);
GO

CREATE TABLE coaches (
    id             INT IDENTITY(1,1) PRIMARY KEY,
    user_id        INT           NOT NULL FOREIGN KEY REFERENCES users(id),
    full_name      NVARCHAR(100) NOT NULL,
    specialization NVARCHAR(100),
    experience     INT           DEFAULT 0,
    bio            NVARCHAR(MAX),
    salary         DECIMAL(12,2) DEFAULT 0,
    status         NVARCHAR(15)  DEFAULT 'ACTIVE'
                                 CHECK (status IN ('ACTIVE','UNDER_REVIEW','TERMINATED'))
);
GO

CREATE TABLE training_packages (
    id              INT IDENTITY(1,1) PRIMARY KEY,
    name            NVARCHAR(100)  NOT NULL,
    duration_months INT            NOT NULL,
    price           DECIMAL(12,2)  NOT NULL,
    max_classes     INT            DEFAULT 0,
    description     NVARCHAR(MAX),
    is_active       BIT            DEFAULT 1
);
GO

CREATE TABLE training_classes (
    id               INT IDENTITY(1,1) PRIMARY KEY,
    name             NVARCHAR(100) NOT NULL,
    coach_id         INT           FOREIGN KEY REFERENCES coaches(id),
    capacity         INT           DEFAULT 20,
    current_enrolled INT           DEFAULT 0,
    level            NVARCHAR(20)  CHECK (level IN ('BEGINNER','INTERMEDIATE','ADVANCED')),
    description      NVARCHAR(MAX),
    is_active        BIT           DEFAULT 1
);
GO

CREATE TABLE schedules (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    class_id      INT          NOT NULL FOREIGN KEY REFERENCES training_classes(id),
    day_of_week   NVARCHAR(15) NOT NULL
                               CHECK (day_of_week IN
                               ('MONDAY','TUESDAY','WEDNESDAY',
                                'THURSDAY','FRIDAY','SATURDAY','SUNDAY')),
    start_time    TIME         NOT NULL,
    end_time      TIME         NOT NULL,
    room          NVARCHAR(50),
    repeat_weekly BIT          DEFAULT 1
);
GO

CREATE TABLE enrollments (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    member_id   INT          NOT NULL FOREIGN KEY REFERENCES members(id),
    class_id    INT          NOT NULL FOREIGN KEY REFERENCES training_classes(id),
    enroll_date DATE         DEFAULT CAST(GETDATE() AS DATE),
    status      NVARCHAR(15) DEFAULT 'ACTIVE'
                             CHECK (status IN ('ACTIVE','CANCELLED')),
    CONSTRAINT uq_enrollment UNIQUE (member_id, class_id)
);
GO

-- Brute-force protection
CREATE TABLE login_attempts (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    username     NVARCHAR(50)  NOT NULL,
    ip_address   NVARCHAR(50),
    attempt_time DATETIME2     DEFAULT GETDATE(),
    is_success   BIT           DEFAULT 0
);
GO

-- ============================================================
-- Upgrade modules
-- ============================================================

-- Module 1 — Equipment management (admin CRUD)
CREATE TABLE equipment (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    name          NVARCHAR(100) NOT NULL,
    category      NVARCHAR(50),
    quantity      INT           DEFAULT 1,
    status        NVARCHAR(15)  DEFAULT 'AVAILABLE'
                                CHECK (status IN ('AVAILABLE','IN_USE','MAINTENANCE','RETIRED')),
    purchase_date DATE,
    notes         NVARCHAR(MAX)
);
GO

-- Module 2 — Payments (membership purchases, fees). Revenue is aggregated from this table.
CREATE TABLE payments (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    member_id   INT           NOT NULL FOREIGN KEY REFERENCES members(id),
    package_id  INT,
    amount      DECIMAL(12,2) NOT NULL,
    method      NVARCHAR(20)  DEFAULT 'CASH'
                              CHECK (method IN ('CASH','CARD','TRANSFER')),
    status      NVARCHAR(15)  DEFAULT 'COMPLETED'
                              CHECK (status IN ('PENDING','COMPLETED','REFUNDED')),
    description NVARCHAR(255),
    paid_at     DATETIME2     DEFAULT GETDATE()
);
GO

-- Module 3 — Attendance (coach marks per session; member self check-in).
-- One row per (class, member, day); status PRESENT/ABSENT/LATE.
CREATE TABLE attendance (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    class_id      INT           NOT NULL FOREIGN KEY REFERENCES training_classes(id),
    member_id     INT           NOT NULL FOREIGN KEY REFERENCES members(id),
    session_date  DATE          NOT NULL,
    status        NVARCHAR(15)  DEFAULT 'PRESENT'
                                CHECK (status IN ('PRESENT','ABSENT','LATE')),
    checked_in_at DATETIME2,
    CONSTRAINT uq_attendance UNIQUE (class_id, member_id, session_date)
);
GO

-- Module 4 — Lesson plans (coach -> class) and progress notes (coach -> member).
CREATE TABLE lesson_plans (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    class_id   INT           NOT NULL FOREIGN KEY REFERENCES training_classes(id),
    coach_id   INT           NOT NULL FOREIGN KEY REFERENCES coaches(id),
    title      NVARCHAR(150) NOT NULL,
    content    NVARCHAR(MAX),
    created_at DATETIME2     DEFAULT GETDATE()
);
GO

CREATE TABLE progress_notes (
    id          INT IDENTITY(1,1) PRIMARY KEY,
    member_id   INT           NOT NULL FOREIGN KEY REFERENCES members(id),
    coach_id    INT           NOT NULL FOREIGN KEY REFERENCES coaches(id),
    class_id    INT           FOREIGN KEY REFERENCES training_classes(id),
    note        NVARCHAR(MAX) NOT NULL,
    rating      INT           CHECK (rating BETWEEN 1 AND 5),
    recorded_at DATETIME2     DEFAULT GETDATE()
);
GO

-- Module 5 — Coach ratings (member -> coach). One rating per (member, coach).
CREATE TABLE coach_ratings (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    member_id  INT           NOT NULL FOREIGN KEY REFERENCES members(id),
    coach_id   INT           NOT NULL FOREIGN KEY REFERENCES coaches(id),
    rating     INT           NOT NULL CHECK (rating BETWEEN 1 AND 5),
    comment    NVARCHAR(500),
    created_at DATETIME2     DEFAULT GETDATE(),
    CONSTRAINT uq_coach_rating UNIQUE (member_id, coach_id)
);
GO

-- Module 6 — Health metrics (member self-tracking).
CREATE TABLE health_metrics (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    member_id     INT           NOT NULL FOREIGN KEY REFERENCES members(id),
    recorded_date DATE          NOT NULL,
    weight_kg     DECIMAL(5,2),
    height_cm     DECIMAL(5,2),
    body_fat_pct  DECIMAL(5,2),
    notes         NVARCHAR(255),
    created_at    DATETIME2     DEFAULT GETDATE()
);
GO

-- Module 7 — Personal-training sessions (member books a coach).
CREATE TABLE pt_sessions (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    member_id    INT          NOT NULL FOREIGN KEY REFERENCES members(id),
    coach_id     INT          NOT NULL FOREIGN KEY REFERENCES coaches(id),
    session_date DATE         NOT NULL,
    start_time   TIME         NOT NULL,
    end_time     TIME         NOT NULL,
    status       NVARCHAR(15) DEFAULT 'PENDING'
                              CHECK (status IN ('PENDING','CONFIRMED','CANCELLED','COMPLETED')),
    notes        NVARCHAR(255),
    created_at   DATETIME2    DEFAULT GETDATE()
);
GO

-- Module 9 — Direct messages (coach <-> member chat).
CREATE TABLE messages (
    id                INT IDENTITY(1,1) PRIMARY KEY,
    sender_user_id    INT            NOT NULL FOREIGN KEY REFERENCES users(id),
    recipient_user_id INT            NOT NULL FOREIGN KEY REFERENCES users(id),
    body              NVARCHAR(2000) NOT NULL,
    sent_at           DATETIME2      DEFAULT GETDATE(),
    is_read           BIT            DEFAULT 0
);
GO

-- Module 10 — Membership requests (member registers a package → admin approves).
-- Lifecycle: PENDING → APPROVED → ACTIVE (or REJECTED/CANCELLED). After approval
-- the member may still cancel/change within a 24h grace window, until the
-- membership is activated (first class registered/checked-in or explicit activate).
CREATE TABLE membership_requests (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    member_id    INT           NOT NULL FOREIGN KEY REFERENCES members(id),
    package_id   INT           NOT NULL FOREIGN KEY REFERENCES training_packages(id),
    amount       DECIMAL(12,2) NOT NULL,
    method       NVARCHAR(20)  DEFAULT 'CASH'
                               CHECK (method IN ('CASH','CARD','TRANSFER')),
    status       NVARCHAR(15)  DEFAULT 'PENDING'
                               CHECK (status IN ('PENDING','APPROVED','ACTIVE','REJECTED','CANCELLED')),
    requested_at DATETIME2     DEFAULT GETDATE(),
    approved_at  DATETIME2,
    start_date   DATE,
    activated_at DATETIME2,
    note         NVARCHAR(255)
);
GO

-- Module 10 — Package ⇄ class links. A member who holds a package may only
-- register for the classes attached to it.
CREATE TABLE package_classes (
    id         INT IDENTITY(1,1) PRIMARY KEY,
    package_id INT NOT NULL FOREIGN KEY REFERENCES training_packages(id),
    class_id   INT NOT NULL FOREIGN KEY REFERENCES training_classes(id),
    CONSTRAINT uq_package_class UNIQUE (package_id, class_id)
);
GO

-- Module 10 — Coach class-change requests (accept/claim or give-up/release a
-- class). The assignment only changes after an admin approves the request.
CREATE TABLE class_change_requests (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    coach_id     INT          NOT NULL FOREIGN KEY REFERENCES coaches(id),
    class_id     INT          NOT NULL FOREIGN KEY REFERENCES training_classes(id),
    action       NVARCHAR(10) NOT NULL CHECK (action IN ('CLAIM','RELEASE')),
    status       NVARCHAR(15) DEFAULT 'PENDING'
                              CHECK (status IN ('PENDING','APPROVED','REJECTED')),
    requested_at DATETIME2    DEFAULT GETDATE(),
    decided_at   DATETIME2,
    note         NVARCHAR(255)
);
GO

-- ============================================================
-- Sample data (passwords are BCrypt hash of "Password123").
-- BCrypt.Net-Next verifies these $2a$ hashes natively.
--   admin / Password123   → ADMIN
--   coach1 / Password123  → COACH
--   member1 / Password123 → MEMBER
-- ============================================================
INSERT INTO users (username, password_hash, email, role) VALUES
('admin',   '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'admin@sportsclub.com',  'ADMIN'),
('coach1',  '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'coach1@sportsclub.com', 'COACH'),
('member1', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'member1@gmail.com',     'MEMBER');
GO

INSERT INTO coaches (user_id, full_name, specialization, experience, salary) VALUES
(2, N'John Smith', N'Strength & Conditioning', 5, 3500.00);
GO

INSERT INTO members (user_id, full_name, gender, join_date, expiry_date, status) VALUES
(3, N'Alice Johnson', 'FEMALE', CAST(GETDATE() AS DATE),
 DATEADD(MONTH, 3, CAST(GETDATE() AS DATE)), 'ACTIVE');
GO

INSERT INTO training_packages (name, duration_months, price, max_classes, description) VALUES
(N'Basic',    1, 50.00,   10, N'1-month access to 10 classes.'),
(N'Standard', 3, 130.00,  30, N'3-month access to 30 classes.'),
(N'Premium',  6, 230.00,  99, N'6-month unlimited access.');
GO

INSERT INTO training_classes (name, coach_id, capacity, level, description, is_active) VALUES
(N'Morning Yoga',   1, 20, 'BEGINNER',     N'Gentle yoga for all levels.', 1),
(N'CrossFit Blast', 1, 15, 'ADVANCED',     N'High-intensity functional fitness.', 1),
(N'Pilates Core',   1, 20, 'INTERMEDIATE', N'Core strengthening pilates.', 1);
GO

INSERT INTO schedules (class_id, day_of_week, start_time, end_time, room) VALUES
(1, 'MONDAY',    '07:00', '08:00', 'Studio A'),
(1, 'WEDNESDAY', '07:00', '08:00', 'Studio A'),
(2, 'TUESDAY',   '18:00', '19:00', 'Gym Floor'),
(2, 'THURSDAY',  '18:00', '19:00', 'Gym Floor'),
(3, 'FRIDAY',    '10:00', '11:00', 'Studio B');
GO

-- Link packages to the classes they grant access to.
--   Basic    → Morning Yoga
--   Standard → Morning Yoga, CrossFit Blast, Pilates Core
--   Premium  → all classes
INSERT INTO package_classes (package_id, class_id) VALUES
(1, 1),
(2, 1), (2, 2), (2, 3),
(3, 1), (3, 2), (3, 3);
GO

-- Give the sample member (Alice, member id 1) an already-active Standard
-- membership so the demo shows classes immediately.
UPDATE members SET package_id = 2 WHERE id = 1;
GO

INSERT INTO membership_requests
    (member_id, package_id, amount, method, status, requested_at, approved_at, start_date, activated_at)
VALUES
(1, 2, 130.00, 'CASH', 'ACTIVE', GETDATE(), GETDATE(), CAST(GETDATE() AS DATE), GETDATE());
GO

INSERT INTO equipment (name, category, quantity, status, purchase_date, notes) VALUES
(N'Treadmill',        N'Cardio',    8, 'AVAILABLE',   '2024-01-15', N'Life Fitness T5.'),
(N'Olympic Barbell',  N'Strength', 12, 'AVAILABLE',   '2024-02-01', N'20kg bars.'),
(N'Spin Bike',        N'Cardio',    6, 'MAINTENANCE', '2023-11-20', N'2 units under repair.'),
(N'Yoga Mat',         N'Studio',   30, 'AVAILABLE',   '2024-03-10', NULL);
GO

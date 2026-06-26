-- ============================================================
-- Sports Club — Incremental upgrade script (SQL Server 2019+)
--
-- Use this to upgrade an EXISTING SportsClubDB to the latest schema
-- WITHOUT dropping it or losing data. database.sql is for a fresh
-- install; this file only adds what the upgrade modules introduced.
--
-- It is fully idempotent: every change is guarded by an existence
-- check, so re-running it is safe and partial upgrades are handled.
--
-- Run once against your existing database, e.g.:
--   sqlcmd -S .\SQLEXPRESS -E -d SportsClubDB -i database-upgrade.sql
-- ============================================================

USE SportsClubDB;
GO

-- ── Coach employment status (ACTIVE / UNDER_REVIEW / TERMINATED) ────────────
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.coaches') AND name = 'status')
    ALTER TABLE coaches ADD status NVARCHAR(15) NOT NULL
        CONSTRAINT DF_coaches_status DEFAULT 'ACTIVE'
        CONSTRAINT CK_coaches_status CHECK (status IN ('ACTIVE','UNDER_REVIEW','TERMINATED'));
GO

-- ── Module 1: Equipment ─────────────────────────────────────────────────────
IF OBJECT_ID('dbo.equipment', 'U') IS NULL
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

-- ── Module 2: Payments ──────────────────────────────────────────────────────
IF OBJECT_ID('dbo.payments', 'U') IS NULL
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

-- ── Module 3: Attendance ────────────────────────────────────────────────────
IF OBJECT_ID('dbo.attendance', 'U') IS NULL
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

-- ── Module 4: Lesson plans + progress notes ────────────────────────────────
IF OBJECT_ID('dbo.lesson_plans', 'U') IS NULL
    CREATE TABLE lesson_plans (
        id         INT IDENTITY(1,1) PRIMARY KEY,
        class_id   INT           NOT NULL FOREIGN KEY REFERENCES training_classes(id),
        coach_id   INT           NOT NULL FOREIGN KEY REFERENCES coaches(id),
        title      NVARCHAR(150) NOT NULL,
        content    NVARCHAR(MAX),
        created_at DATETIME2     DEFAULT GETDATE()
    );
GO

IF OBJECT_ID('dbo.progress_notes', 'U') IS NULL
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

-- ── Module 5: Coach ratings (member -> coach) ───────────────────────────────
IF OBJECT_ID('dbo.coach_ratings', 'U') IS NULL
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

-- ── Module 6: Health metrics (member self-tracking) ─────────────────────────
IF OBJECT_ID('dbo.health_metrics', 'U') IS NULL
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

-- ── Module 7: Personal-training sessions ────────────────────────────────────
IF OBJECT_ID('dbo.pt_sessions', 'U') IS NULL
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

-- ── Module 9: Direct messages (coach <-> member chat) ───────────────────────
IF OBJECT_ID('dbo.messages', 'U') IS NULL
    CREATE TABLE messages (
        id                INT IDENTITY(1,1) PRIMARY KEY,
        sender_user_id    INT            NOT NULL FOREIGN KEY REFERENCES users(id),
        recipient_user_id INT            NOT NULL FOREIGN KEY REFERENCES users(id),
        body              NVARCHAR(2000) NOT NULL,
        sent_at           DATETIME2      DEFAULT GETDATE(),
        is_read           BIT            DEFAULT 0
    );
GO

-- ── Optional sample equipment (only if the table is empty) ──────────────────
IF NOT EXISTS (SELECT 1 FROM equipment)
    INSERT INTO equipment (name, category, quantity, status, purchase_date, notes) VALUES
    (N'Treadmill',       N'Cardio',    8, 'AVAILABLE',   '2024-01-15', N'Life Fitness T5.'),
    (N'Olympic Barbell', N'Strength', 12, 'AVAILABLE',   '2024-02-01', N'20kg bars.'),
    (N'Spin Bike',       N'Cardio',    6, 'MAINTENANCE', '2023-11-20', N'2 units under repair.'),
    (N'Yoga Mat',        N'Studio',   30, 'AVAILABLE',   '2024-03-10', NULL);
GO

PRINT 'Upgrade complete.';
GO

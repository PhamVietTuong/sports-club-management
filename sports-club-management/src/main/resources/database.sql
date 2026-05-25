-- ============================================================
-- Sports Club Management Database — SQL Server 2019+
-- Uses SQL Server syntax: IDENTITY, NVARCHAR, DATETIME2,
-- GETDATE(), GO batch separator, OFFSET/FETCH for pagination.
-- ============================================================

-- Create database
CREATE DATABASE SportsClubDB;
GO
USE SportsClubDB;
GO

-- ============================================================
-- Users table — shared authentication for all roles
-- ============================================================
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

-- ============================================================
-- Members table
-- ============================================================
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

-- ============================================================
-- Coaches table
-- ============================================================
CREATE TABLE coaches (
    id             INT IDENTITY(1,1) PRIMARY KEY,
    user_id        INT           NOT NULL FOREIGN KEY REFERENCES users(id),
    full_name      NVARCHAR(100) NOT NULL,
    specialization NVARCHAR(100),
    experience     INT           DEFAULT 0,
    bio            NVARCHAR(MAX),
    salary         DECIMAL(12,2) DEFAULT 0
);
GO

-- ============================================================
-- Training packages table
-- ============================================================
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

-- ============================================================
-- Training classes table
-- ============================================================
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

-- ============================================================
-- Schedules table
-- ============================================================
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

-- ============================================================
-- Enrollments table
-- ============================================================
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

-- ============================================================
-- Login attempts table — brute-force protection
-- ============================================================
CREATE TABLE login_attempts (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    username     NVARCHAR(50)  NOT NULL,
    ip_address   NVARCHAR(50),
    attempt_time DATETIME2     DEFAULT GETDATE(),
    is_success   BIT           DEFAULT 0
);
GO

-- ============================================================
-- Sample data (passwords are BCrypt hash of "Password123")
-- Run BCryptUtil.hashPassword("Password123") to regenerate
-- ============================================================
INSERT INTO users (username, password_hash, email, role) VALUES
(
    'admin',
    '$2a$12$Hw3rJEcpB9AuClOEpRD/ceZY4kOIBViBqdNKf3v/VBDOQ9bM6Ug1G',
    'admin@sportsclub.com',
    'ADMIN'
),
(
    'coach1',
    '$2a$12$Hw3rJEcpB9AuClOEpRD/ceZY4kOIBViBqdNKf3v/VBDOQ9bM6Ug1G',
    'coach1@sportsclub.com',
    'COACH'
),
(
    'member1',
    '$2a$12$Hw3rJEcpB9AuClOEpRD/ceZY4kOIBViBqdNKf3v/VBDOQ9bM6Ug1G',
    'member1@gmail.com',
    'MEMBER'
);
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
(N'Morning Yoga',       1, 20, 'BEGINNER',     N'Gentle yoga for all levels.', 1),
(N'CrossFit Blast',     1, 15, 'ADVANCED',     N'High-intensity functional fitness.', 1),
(N'Pilates Core',       1, 20, 'INTERMEDIATE', N'Core strengthening pilates.', 1);
GO

INSERT INTO schedules (class_id, day_of_week, start_time, end_time, room) VALUES
(1, 'MONDAY',    '07:00', '08:00', 'Studio A'),
(1, 'WEDNESDAY', '07:00', '08:00', 'Studio A'),
(2, 'TUESDAY',   '18:00', '19:00', 'Gym Floor'),
(2, 'THURSDAY',  '18:00', '19:00', 'Gym Floor'),
(3, 'FRIDAY',    '10:00', '11:00', 'Studio B');
GO

-- ============================================================
-- Stored Procedure: get members by status
-- ============================================================
CREATE PROCEDURE sp_GetMembersByStatus
    @status NVARCHAR(15)
AS
BEGIN
    SELECT m.*, u.username, u.email
    FROM members m
    INNER JOIN users u ON m.user_id = u.id
    WHERE m.status = @status
    ORDER BY m.join_date DESC;
END;
GO

-- ============================================================
-- View: class enrollment statistics
-- ============================================================
CREATE VIEW vw_ClassEnrollmentStats AS
SELECT
    tc.id,
    tc.name             AS class_name,
    tc.capacity,
    tc.current_enrolled,
    (tc.capacity - tc.current_enrolled) AS available_slots,
    c.full_name         AS coach_name
FROM training_classes tc
LEFT JOIN coaches c ON tc.coach_id = c.id
WHERE tc.is_active = 1;
GO

-- ============================================================
-- Example pagination using SQL Server OFFSET/FETCH
-- (replaces MySQL LIMIT/OFFSET)
-- ============================================================
-- SELECT * FROM members
-- ORDER BY id
-- OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;

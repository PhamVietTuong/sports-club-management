-- ============================================================
-- Sample data for the ENTIRE system (SQL Server).
--
-- Run order:
--   1. database.sql          (schema + base seed: admin/coach1/member1,
--                             packages Basic/Standard/Premium, 3 classes,
--                             schedules, equipment)
--   2. database-upgrade.sql  (if upgrading an existing DB to Module 10 tables)
--   3. THIS FILE             (adds coaches, 15 members, more packages/classes/
--                             schedules, and activity across every table)
--
--   sqlcmd -S .\SQLEXPRESS -E -d SportsClubDB -i sample-data.sql
--
-- This file is comprehensive and already includes the 15 sample members, so it
-- SUPERSEDES sample-members.sql — run one or the other, not both.
-- Login password for every account below is "Password123".
-- All inserts link foreign keys by name/username, so they do not depend on ids.
-- ============================================================

USE SportsClubDB;
GO

DECLARE @PW NVARCHAR(255) = '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a';

-- ── Extra coaches (logins) ──────────────────────────────────────────────────
INSERT INTO users (username, password_hash, email, phone, role) VALUES
('coach2', @PW, 'bao.tran@sportsclub.com',   '0911000002', 'COACH'),
('coach3', @PW, 'long.le@sportsclub.com',    '0911000003', 'COACH'),
('coach4', @PW, 'ha.pham@sportsclub.com',    '0911000004', 'COACH'),
('coach5', @PW, 'linh.nguyen@sportsclub.com','0911000005', 'COACH'),
('coach6', @PW, 'khang.do@sportsclub.com',   '0911000006', 'COACH');
GO

INSERT INTO coaches (user_id, full_name, specialization, experience, bio, salary, status)
SELECT u.id, x.full_name, x.spec, x.exp, x.bio, x.salary, x.status
FROM (VALUES
    ('coach2', N'Trần Quốc Bảo',   N'Strength & Boxing',      7, N'Cựu VĐV quyền anh.',            4200.00, 'ACTIVE'),
    ('coach3', N'Lê Thành Long',   N'Body Combat',            4, N'HLV nhóm năng lượng cao.',      3300.00, 'ACTIVE'),
    ('coach4', N'Phạm Thu Hà',     N'Yoga & Pilates',         9, N'Chứng chỉ yoga quốc tế.',       4500.00, 'ACTIVE'),
    ('coach5', N'Nguyễn Mỹ Linh',  N'Cardio & HIIT',          3, N'Chuyên đốt mỡ, HIIT.',          3100.00, 'ACTIVE'),
    ('coach6', N'Đỗ Gia Khang',    N'General Fitness',        2, N'Đang trong thời gian xem xét.', 2800.00, 'UNDER_REVIEW')
) AS x(username, full_name, spec, exp, bio, salary, status)
JOIN users u ON u.username = x.username;
GO

-- ── Extra training packages ─────────────────────────────────────────────────
INSERT INTO training_packages (name, duration_months, price, max_classes, description) VALUES
(N'Student',    1,  35.00,   8, N'Gói sinh viên 1 tháng, 8 lớp.'),
(N'VIP Annual', 12, 420.00, 99, N'Gói VIP 12 tháng, không giới hạn lớp.');
GO

-- ── Extra classes (coach assigned by name; two left unassigned for claims) ──
INSERT INTO training_classes (name, coach_id, capacity, current_enrolled, level, description, is_active)
SELECT x.name, c.id, x.capacity, 0, x.level, x.descr, 1
FROM (VALUES
    (N'Zumba Dance',        N'Trần Quốc Bảo',  25, 'INTERMEDIATE', N'Nhảy Zumba sôi động.'),
    (N'Boxing Basics',      N'Lê Thành Long',  18, 'BEGINNER',     N'Nhập môn quyền anh.'),
    (N'Spin Cycling',       N'Phạm Thu Hà',    20, 'INTERMEDIATE', N'Đạp xe trong nhà.'),
    (N'Power Lifting',      N'Trần Quốc Bảo',  12, 'ADVANCED',     N'Cử tạ nâng cao.'),
    (N'Body Combat',        N'Lê Thành Long',  22, 'ADVANCED',     N'Võ thuật kết hợp cardio.'),
    (N'Stretch & Mobility', N'Phạm Thu Hà',    20, 'BEGINNER',     N'Giãn cơ và linh hoạt.'),
    (N'HIIT Express',       N'Nguyễn Mỹ Linh', 16, 'ADVANCED',     N'Đốt mỡ cường độ cao 30 phút.')
) AS x(name, coach_name, capacity, level, descr)
JOIN coaches c ON c.full_name = x.coach_name;
GO

-- Two unassigned active classes (no coach) — for the coach "claim class" flow.
INSERT INTO training_classes (name, coach_id, capacity, current_enrolled, level, description, is_active) VALUES
(N'Aqua Fitness',   NULL, 20, 0, 'BEGINNER', N'Thể dục dưới nước.', 1),
(N'Senior Fitness', NULL, 15, 0, 'BEGINNER', N'Thể dục cho người lớn tuổi.', 1);
GO

-- ── Schedules for the new classes (room/time chosen to avoid clashes) ───────
INSERT INTO schedules (class_id, day_of_week, start_time, end_time, room)
SELECT c.id, x.day, x.st, x.et, x.room
FROM (VALUES
    (N'Zumba Dance',        'MONDAY',    '18:00', '19:00', N'Studio C'),
    (N'Zumba Dance',        'THURSDAY',  '18:00', '19:00', N'Studio C'),
    (N'Boxing Basics',      'TUESDAY',   '17:00', '18:00', N'Ring Room'),
    (N'Spin Cycling',       'WEDNESDAY', '06:00', '07:00', N'Spin Studio'),
    (N'Spin Cycling',       'FRIDAY',    '06:00', '07:00', N'Spin Studio'),
    (N'Power Lifting',      'SATURDAY',  '09:00', '10:30', N'Weight Room'),
    (N'Body Combat',        'TUESDAY',   '19:00', '20:00', N'Studio C'),
    (N'Stretch & Mobility', 'FRIDAY',    '08:00', '09:00', N'Studio B2'),
    (N'HIIT Express',       'MONDAY',    '12:00', '12:30', N'Gym Floor 2')
) AS x(class_name, day, st, et, room)
JOIN training_classes c ON c.name = x.class_name;
GO

-- ── 15 members (logins) ─────────────────────────────────────────────────────
INSERT INTO users (username, password_hash, email, phone, role)
SELECT x.username, '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', x.email, x.phone, 'MEMBER'
FROM (VALUES
    ('mem001', 'an.nguyen@example.com',  '0900000001'),
    ('mem002', 'binh.tran@example.com',  '0900000002'),
    ('mem003', 'cuong.le@example.com',   '0900000003'),
    ('mem004', 'dung.pham@example.com',  '0900000004'),
    ('mem005', 'duc.hoang@example.com',  '0900000005'),
    ('mem006', 'hoa.vu@example.com',     '0900000006'),
    ('mem007', 'hung.dang@example.com',  '0900000007'),
    ('mem008', 'lan.bui@example.com',    '0900000008'),
    ('mem009', 'huy.do@example.com',     '0900000009'),
    ('mem010', 'mai.ngo@example.com',    '0900000010'),
    ('mem011', 'nam.duong@example.com',  '0900000011'),
    ('mem012', 'oanh.ly@example.com',    '0900000012'),
    ('mem013', 'phuc.phan@example.com',  '0900000013'),
    ('mem014', 'quynh.vo@example.com',   '0900000014'),
    ('mem015', 'son.truong@example.com', '0900000015')
) AS x(username, email, phone);
GO

INSERT INTO members (user_id, full_name, gender, date_of_birth, address, package_id, join_date, expiry_date, status)
SELECT u.id, x.full_name, x.gender, x.dob, x.address,
       ISNULL((SELECT id FROM training_packages WHERE name = x.package_name), 0),
       x.join_date, x.expiry_date, x.status
FROM (VALUES
    ('mem001', N'Nguyễn Văn An',    'MALE',   CAST('1995-03-12' AS DATE), N'12 Lê Lợi, Q1',          N'Standard', CAST('2025-01-10' AS DATE), CAST('2025-04-10' AS DATE), 'ACTIVE'),
    ('mem002', N'Trần Thị Bình',    'FEMALE', CAST('1998-07-25' AS DATE), N'45 Nguyễn Huệ, Q1',      N'Basic',    CAST('2025-02-01' AS DATE), CAST('2025-03-01' AS DATE), 'ACTIVE'),
    ('mem003', N'Lê Hoàng Cường',   'MALE',   CAST('1992-11-03' AS DATE), N'78 Trần Hưng Đạo, Q5',   N'Premium',  CAST('2024-12-15' AS DATE), CAST('2025-06-15' AS DATE), 'ACTIVE'),
    ('mem004', N'Phạm Thị Dung',    'FEMALE', CAST('2000-05-18' AS DATE), N'23 Hai Bà Trưng, Q3',    N'Standard', CAST('2025-01-20' AS DATE), CAST('2025-04-20' AS DATE), 'INACTIVE'),
    ('mem005', N'Hoàng Minh Đức',   'MALE',   CAST('1990-09-09' AS DATE), N'5 CMT8, Q10',            N'Student',  CAST('2025-03-05' AS DATE), CAST('2025-04-05' AS DATE), 'ACTIVE'),
    ('mem006', N'Vũ Thị Hoa',       'FEMALE', CAST('1997-01-30' AS DATE), N'90 Lý Thường Kiệt, Q11', N'Premium',  CAST('2024-11-01' AS DATE), CAST('2025-05-01' AS DATE), 'SUSPENDED'),
    ('mem007', N'Đặng Văn Hùng',    'MALE',   CAST('1993-06-14' AS DATE), N'33 ĐBP, Bình Thạnh',     N'Standard', CAST('2025-02-10' AS DATE), CAST('2025-05-10' AS DATE), 'ACTIVE'),
    ('mem008', N'Bùi Thị Lan',      'FEMALE', CAST('1999-08-22' AS DATE), N'17 Phan Xích Long, PN',  N'Basic',    CAST('2025-03-12' AS DATE), CAST('2025-04-12' AS DATE), 'ACTIVE'),
    ('mem009', N'Đỗ Quang Huy',     'MALE',   CAST('1996-04-07' AS DATE), N'8 Nguyễn Trãi, Q5',      N'VIP Annual',CAST('2024-10-20' AS DATE),CAST('2025-10-20' AS DATE), 'ACTIVE'),
    ('mem010', N'Ngô Thị Mai',      'FEMALE', CAST('2001-12-01' AS DATE), N'62 Võ Văn Tần, Q3',      N'Standard', CAST('2025-01-25' AS DATE), CAST('2025-04-25' AS DATE), 'INACTIVE'),
    ('mem011', N'Dương Văn Nam',    'MALE',   CAST('1994-02-19' AS DATE), N'100 Lê Văn Sỹ, TB',      N'Basic',    CAST('2025-03-01' AS DATE), CAST('2025-04-01' AS DATE), 'ACTIVE'),
    ('mem012', N'Lý Thị Oanh',      'FEMALE', CAST('1998-10-11' AS DATE), N'29 Cộng Hòa, TB',        N'Premium',  CAST('2024-09-15' AS DATE), CAST('2025-03-15' AS DATE), 'ACTIVE'),
    ('mem013', N'Phan Văn Phúc',    'MALE',   CAST('1991-07-08' AS DATE), N'14 Hoàng Văn Thụ, PN',   N'Standard', CAST('2025-02-18' AS DATE), CAST('2025-05-18' AS DATE), 'ACTIVE'),
    ('mem014', N'Võ Thị Quỳnh',     'FEMALE', CAST('2000-03-27' AS DATE), N'56 NTMK, Q1',            N'Student',  CAST('2025-03-20' AS DATE), CAST('2025-04-20' AS DATE), 'SUSPENDED'),
    ('mem015', N'Trương Văn Sơn',   'MALE',   CAST('1995-11-16' AS DATE), N'71 Pasteur, Q3',         N'Premium',  CAST('2024-12-01' AS DATE), CAST('2025-06-01' AS DATE), 'ACTIVE')
) AS x(username, full_name, gender, dob, address, package_name, join_date, expiry_date, status)
JOIN users u ON u.username = x.username;
GO

-- ── Package ⇄ class links (which classes each package grants) ────────────────
-- Basic → 3 entry classes; Standard → 6; Premium / VIP Annual → all; Student → 4.
INSERT INTO package_classes (package_id, class_id)
SELECT p.id, c.id
FROM training_packages p
JOIN training_classes c ON 1 = 1
WHERE NOT EXISTS (SELECT 1 FROM package_classes pc WHERE pc.package_id = p.id AND pc.class_id = c.id)
  AND (
        (p.name = N'Basic'      AND c.name IN (N'Morning Yoga', N'Stretch & Mobility', N'Boxing Basics'))
     OR (p.name = N'Standard'   AND c.name IN (N'Morning Yoga', N'Pilates Core', N'Zumba Dance', N'Spin Cycling', N'Boxing Basics', N'Stretch & Mobility'))
     OR (p.name = N'Premium')
     OR (p.name = N'VIP Annual')
     OR (p.name = N'Student'    AND c.name IN (N'Morning Yoga', N'Zumba Dance', N'HIIT Express', N'Stretch & Mobility'))
      );
GO

-- ── Enrollments (member ⇄ class) ────────────────────────────────────────────
INSERT INTO enrollments (member_id, class_id, enroll_date, status)
SELECT m.id, c.id, x.enroll_date, x.status
FROM (VALUES
    ('mem001', N'Morning Yoga',       CAST('2025-01-11' AS DATE), 'ACTIVE'),
    ('mem001', N'Pilates Core',       CAST('2025-01-15' AS DATE), 'ACTIVE'),
    ('mem002', N'Morning Yoga',       CAST('2025-02-02' AS DATE), 'ACTIVE'),
    ('mem003', N'Power Lifting',      CAST('2024-12-16' AS DATE), 'ACTIVE'),
    ('mem003', N'Body Combat',        CAST('2024-12-20' AS DATE), 'ACTIVE'),
    ('mem005', N'HIIT Express',       CAST('2025-03-06' AS DATE), 'ACTIVE'),
    ('mem007', N'Zumba Dance',        CAST('2025-02-11' AS DATE), 'ACTIVE'),
    ('mem007', N'Spin Cycling',       CAST('2025-02-12' AS DATE), 'ACTIVE'),
    ('mem008', N'Boxing Basics',      CAST('2025-03-13' AS DATE), 'ACTIVE'),
    ('mem009', N'CrossFit Blast',     CAST('2024-10-21' AS DATE), 'ACTIVE'),
    ('mem009', N'Power Lifting',      CAST('2024-10-25' AS DATE), 'ACTIVE'),
    ('mem011', N'Stretch & Mobility', CAST('2025-03-02' AS DATE), 'ACTIVE'),
    ('mem012', N'Zumba Dance',        CAST('2024-09-16' AS DATE), 'ACTIVE'),
    ('mem013', N'Spin Cycling',       CAST('2025-02-19' AS DATE), 'ACTIVE'),
    ('mem006', N'Morning Yoga',       CAST('2024-11-02' AS DATE), 'CANCELLED')
) AS x(username, class_name, enroll_date, status)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id
JOIN training_classes c ON c.name = x.class_name;
GO

-- Keep current_enrolled in sync with the ACTIVE enrollments just added.
UPDATE c
SET current_enrolled = (SELECT COUNT(*) FROM enrollments e WHERE e.class_id = c.id AND e.status = 'ACTIVE')
FROM training_classes c;
GO

-- ── Payments ────────────────────────────────────────────────────────────────
INSERT INTO payments (member_id, package_id, amount, method, status, description, paid_at)
SELECT m.id, (SELECT id FROM training_packages WHERE name = x.package_name), x.amount, x.method, x.status, x.descr, x.paid_at
FROM (VALUES
    ('mem001', N'Standard',  130.00, 'CARD',     'COMPLETED', N'Kích hoạt gói Standard',  '2025-01-10T09:05:00'),
    ('mem002', N'Basic',      50.00, 'CASH',     'COMPLETED', N'Kích hoạt gói Basic',     '2025-02-01T10:20:00'),
    ('mem003', N'Premium',   230.00, 'TRANSFER', 'COMPLETED', N'Kích hoạt gói Premium',   '2024-12-15T14:00:00'),
    ('mem004', N'Standard',  130.00, 'CARD',     'COMPLETED', N'Kích hoạt gói Standard',  '2025-01-20T11:30:00'),
    ('mem005', N'Student',    35.00, 'CASH',     'COMPLETED', N'Kích hoạt gói Student',   '2025-03-05T08:45:00'),
    ('mem006', N'Premium',   230.00, 'TRANSFER', 'REFUNDED',  N'Hoàn tiền (tạm ngưng)',   '2024-11-01T09:00:00'),
    ('mem007', N'Standard',  130.00, 'CARD',     'COMPLETED', N'Kích hoạt gói Standard',  '2025-02-10T16:10:00'),
    ('mem008', N'Basic',      50.00, 'CASH',     'COMPLETED', N'Kích hoạt gói Basic',     '2025-03-12T13:25:00'),
    ('mem009', N'VIP Annual',420.00, 'TRANSFER', 'COMPLETED', N'Kích hoạt gói VIP Annual','2024-10-20T15:40:00'),
    ('mem010', N'Standard',  130.00, 'CARD',     'PENDING',   N'Chờ xác nhận thanh toán', '2025-01-25T10:00:00'),
    ('mem011', N'Basic',      50.00, 'CASH',     'COMPLETED', N'Kích hoạt gói Basic',     '2025-03-01T09:15:00'),
    ('mem012', N'Premium',   230.00, 'TRANSFER', 'COMPLETED', N'Kích hoạt gói Premium',   '2024-09-15T11:00:00'),
    ('mem013', N'Standard',  130.00, 'CARD',     'COMPLETED', N'Kích hoạt gói Standard',  '2025-02-18T17:30:00'),
    ('mem014', N'Student',    35.00, 'CASH',     'COMPLETED', N'Kích hoạt gói Student',   '2025-03-20T08:00:00'),
    ('mem015', N'Premium',   230.00, 'TRANSFER', 'COMPLETED', N'Kích hoạt gói Premium',   '2024-12-01T10:45:00')
) AS x(username, package_name, amount, method, status, descr, paid_at)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id;
GO

-- ── Membership requests (lifecycle states, for the admin review screen) ─────
INSERT INTO membership_requests (member_id, package_id, amount, method, status, requested_at, approved_at, start_date, activated_at, note)
SELECT m.id, p.id, p.price, x.method, x.status, x.requested_at, x.approved_at, x.start_date, x.activated_at, x.note
FROM (VALUES
    ('mem001', N'Standard',  'CARD',     'ACTIVE',    '2025-01-09T08:00:00', '2025-01-09T09:00:00', CAST('2025-01-10' AS DATE), '2025-01-10T09:05:00', NULL),
    ('mem003', N'Premium',   'TRANSFER', 'ACTIVE',    '2024-12-14T08:00:00', '2024-12-14T10:00:00', CAST('2024-12-15' AS DATE), '2024-12-15T14:00:00', NULL),
    ('mem010', N'Standard',  'CARD',     'PENDING',   '2025-03-22T09:00:00', NULL,                  NULL,                       NULL,                  NULL),
    ('mem011', N'Premium',   'CASH',     'PENDING',   '2025-03-23T10:30:00', NULL,                  NULL,                       NULL,                  NULL),
    ('mem008', N'Standard',  'CARD',     'APPROVED',  '2025-03-21T11:00:00', '2025-03-21T15:00:00', NULL,                       NULL,                  NULL),
    ('mem014', N'VIP Annual','TRANSFER', 'REJECTED',  '2025-03-19T14:00:00', NULL,                  NULL,                       NULL,                  N'Thông tin thanh toán không hợp lệ.'),
    ('mem004', N'Premium',   'CARD',     'CANCELLED', '2025-03-18T09:00:00', '2025-03-18T10:00:00', NULL,                       NULL,                  N'Thành viên đổi ý.')
) AS x(username, package_name, method, status, requested_at, approved_at, start_date, activated_at, note)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id
JOIN training_packages p ON p.name = x.package_name;
GO

-- ── Attendance (per class/member/day) ───────────────────────────────────────
INSERT INTO attendance (class_id, member_id, session_date, status, checked_in_at)
SELECT c.id, m.id, x.session_date, x.status, x.checked_in_at
FROM (VALUES
    ('mem001', N'Morning Yoga',   CAST('2025-03-03' AS DATE), 'PRESENT', '2025-03-03T07:01:00'),
    ('mem001', N'Morning Yoga',   CAST('2025-03-05' AS DATE), 'LATE',    '2025-03-05T07:12:00'),
    ('mem002', N'Morning Yoga',   CAST('2025-03-03' AS DATE), 'PRESENT', '2025-03-03T06:58:00'),
    ('mem003', N'Power Lifting',  CAST('2025-03-08' AS DATE), 'PRESENT', '2025-03-08T09:00:00'),
    ('mem005', N'HIIT Express',   CAST('2025-03-10' AS DATE), 'PRESENT', '2025-03-10T12:00:00'),
    ('mem007', N'Zumba Dance',    CAST('2025-03-06' AS DATE), 'ABSENT',  NULL),
    ('mem008', N'Boxing Basics',  CAST('2025-03-18' AS DATE), 'PRESENT', '2025-03-18T17:02:00'),
    ('mem009', N'CrossFit Blast', CAST('2025-03-11' AS DATE), 'PRESENT', '2025-03-11T18:00:00')
) AS x(username, class_name, session_date, status, checked_in_at)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id
JOIN training_classes c ON c.name = x.class_name;
GO

-- ── Lesson plans (coach → class) ────────────────────────────────────────────
INSERT INTO lesson_plans (class_id, coach_id, title, content, created_at)
SELECT c.id, c.coach_id, x.title, x.content, x.created_at
FROM (VALUES
    (N'Power Lifting',  N'Tuần 1: Squat nền tảng',   N'Khởi động + kỹ thuật squat, 5x5.', '2025-03-01T08:00:00'),
    (N'Power Lifting',  N'Tuần 2: Deadlift',         N'Deadlift kỹ thuật, tăng tải dần.', '2025-03-08T08:00:00'),
    (N'Zumba Dance',    N'Choreography số 1',        N'Bài nhảy Latin cơ bản.',           '2025-03-02T09:00:00'),
    (N'HIIT Express',   N'Vòng tabata 4 phút',       N'8 hiệp 20/10, 4 động tác.',        '2025-03-05T10:00:00'),
    (N'Boxing Basics',  N'Đòn jab & cross',          N'Tư thế thủ, jab-cross combo.',     '2025-03-11T11:00:00'),
    (N'Spin Cycling',   N'Interval leo dốc',         N'5 đoạn leo dốc 3 phút.',           '2025-03-06T06:30:00')
) AS x(class_name, title, content, created_at)
JOIN training_classes c ON c.name = x.class_name
WHERE c.coach_id IS NOT NULL;
GO

-- ── Progress notes (coach → member) ─────────────────────────────────────────
INSERT INTO progress_notes (member_id, coach_id, class_id, note, rating, recorded_at)
SELECT m.id, c.coach_id, c.id, x.note, x.rating, x.recorded_at
FROM (VALUES
    ('mem003', N'Power Lifting',  N'Tiến bộ tốt, squat tăng 10kg.',     5, '2025-03-09T10:00:00'),
    ('mem009', N'Power Lifting',  N'Cần cải thiện tư thế lưng.',        3, '2025-03-09T10:10:00'),
    ('mem005', N'HIIT Express',   N'Thể lực cải thiện rõ rệt.',         4, '2025-03-11T12:40:00'),
    ('mem007', N'Zumba Dance',    N'Bắt nhịp tốt, cần tự tin hơn.',     4, '2025-03-07T19:05:00'),
    ('mem008', N'Boxing Basics',  N'Đòn jab chuẩn, lực khá.',           4, '2025-03-19T18:05:00')
) AS x(username, class_name, note, rating, recorded_at)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id
JOIN training_classes c ON c.name = x.class_name;
GO

-- ── Coach ratings (member → coach, unique per pair) ─────────────────────────
INSERT INTO coach_ratings (member_id, coach_id, rating, comment, created_at)
SELECT m.id, co.id, x.rating, x.comment, x.created_at
FROM (VALUES
    ('mem003', N'Trần Quốc Bảo',  5, N'HLV tận tâm, chuyên môn cao.',   '2025-03-10T20:00:00'),
    ('mem009', N'Trần Quốc Bảo',  4, N'Hướng dẫn kỹ, dễ hiểu.',          '2025-03-10T21:00:00'),
    ('mem005', N'Nguyễn Mỹ Linh', 5, N'Lớp HIIT rất hiệu quả!',          '2025-03-12T08:00:00'),
    ('mem007', N'Trần Quốc Bảo',  4, N'Năng lượng tốt.',                 '2025-03-08T09:00:00'),
    ('mem008', N'Lê Thành Long',  5, N'Học được nhiều kỹ thuật.',        '2025-03-20T10:00:00')
) AS x(username, coach_name, rating, comment, created_at)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id
JOIN coaches co ON co.full_name = x.coach_name;
GO

-- ── Health metrics (member self-tracking) ───────────────────────────────────
INSERT INTO health_metrics (member_id, recorded_date, weight_kg, height_cm, body_fat_pct, notes, created_at)
SELECT m.id, x.recorded_date, x.weight, x.height, x.fat, x.notes, x.created_at
FROM (VALUES
    ('mem001', CAST('2025-02-01' AS DATE), 68.5, 172.0, 18.2, N'Bắt đầu chương trình.', '2025-02-01T08:00:00'),
    ('mem001', CAST('2025-03-01' AS DATE), 67.0, 172.0, 16.8, N'Giảm 1.5kg.',           '2025-03-01T08:00:00'),
    ('mem003', CAST('2025-03-01' AS DATE), 80.0, 178.0, 20.0, N'Tăng cơ.',              '2025-03-01T09:00:00'),
    ('mem005', CAST('2025-03-06' AS DATE), 60.0, 165.0, 22.5, NULL,                     '2025-03-06T08:30:00'),
    ('mem009', CAST('2025-03-05' AS DATE), 85.0, 180.0, 19.0, N'Mục tiêu 82kg.',        '2025-03-05T07:00:00')
) AS x(username, recorded_date, weight, height, fat, notes, created_at)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id;
GO

-- ── PT sessions (member books a coach) ──────────────────────────────────────
INSERT INTO pt_sessions (member_id, coach_id, session_date, start_time, end_time, status, notes, created_at)
SELECT m.id, co.id, x.session_date, x.st, x.et, x.status, x.notes, x.created_at
FROM (VALUES
    ('mem001', N'Phạm Thu Hà',     CAST('2025-04-02' AS DATE), '10:00', '11:00', 'CONFIRMED', N'Tập yoga riêng.',        '2025-03-25T09:00:00'),
    ('mem003', N'Trần Quốc Bảo',   CAST('2025-04-03' AS DATE), '15:00', '16:00', 'PENDING',   N'Tư vấn giáo án tạ.',      '2025-03-26T10:00:00'),
    ('mem005', N'Nguyễn Mỹ Linh',  CAST('2025-04-01' AS DATE), '13:00', '13:45', 'COMPLETED', N'Buổi HIIT 1-1.',          '2025-03-20T08:00:00'),
    ('mem009', N'Trần Quốc Bảo',   CAST('2025-04-05' AS DATE), '09:00', '10:00', 'CONFIRMED', N'Kiểm tra tư thế.',        '2025-03-27T11:00:00'),
    ('mem007', N'Lê Thành Long',   CAST('2025-04-04' AS DATE), '18:00', '19:00', 'CANCELLED', N'Bận đột xuất.',           '2025-03-28T12:00:00')
) AS x(username, coach_name, session_date, st, et, status, notes, created_at)
JOIN users u ON u.username = x.username
JOIN members m ON m.user_id = u.id
JOIN coaches co ON co.full_name = x.coach_name;
GO

-- ── Coach class-change requests (claim/release awaiting admin) ──────────────
INSERT INTO class_change_requests (coach_id, class_id, action, status, requested_at, decided_at, note)
SELECT co.id, c.id, x.action, x.status, x.requested_at, x.decided_at, x.note
FROM (VALUES
    (N'Nguyễn Mỹ Linh', N'Aqua Fitness',   'CLAIM',   'PENDING',  '2025-03-24T09:00:00', NULL,                  NULL),
    (N'Đỗ Gia Khang',   N'Senior Fitness', 'CLAIM',   'PENDING',  '2025-03-24T10:00:00', NULL,                  NULL),
    (N'Lê Thành Long',  N'Body Combat',    'RELEASE', 'APPROVED', '2025-03-10T08:00:00', '2025-03-10T12:00:00', NULL)
) AS x(coach_name, class_name, action, status, requested_at, decided_at, note)
JOIN coaches co ON co.full_name = x.coach_name
JOIN training_classes c ON c.name = x.class_name;
GO

-- ── Direct messages (coach <-> member chat) ─────────────────────────────────
INSERT INTO messages (sender_user_id, recipient_user_id, body, sent_at, is_read)
SELECT s.id, r.id, x.body, x.sent_at, x.is_read
FROM (VALUES
    ('mem001', 'coach4', N'Chào HLV, em muốn hỏi về buổi yoga PT ạ.', '2025-03-25T08:50:00', 1),
    ('coach4', 'mem001', N'Chào em, 10h sáng thứ 4 nhé.',             '2025-03-25T09:10:00', 1),
    ('mem003', 'coach2', N'Thầy ơi giáo án tuần này thế nào ạ?',       '2025-03-26T09:00:00', 0),
    ('mem005', 'coach5', N'Buổi HIIT hôm qua tuyệt lắm ạ!',           '2025-03-12T07:30:00', 1)
) AS x(sender, recipient, body, sent_at, is_read)
JOIN users s ON s.username = x.sender
JOIN users r ON r.username = x.recipient;
GO

PRINT 'Sample data for the entire system inserted.';
GO

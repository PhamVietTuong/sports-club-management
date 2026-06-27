-- ============================================================
-- Sample data — 15 members (for testing the admin Members
-- pagination/filtering: 15 rows = 2 pages at the default 10/page).
--
-- Each member needs a matching users row. Login password for ALL
-- of these accounts is "Password123" (same BCrypt $2a$ hash as the
-- seed accounts in database.sql).
--
-- Run after database.sql, against the existing database:
--   sqlcmd -S .\SQLEXPRESS -E -d SportsClubDB -i sample-members.sql
-- ============================================================

USE SportsClubDB;
GO

-- 1) Create the 15 login accounts (role MEMBER).
INSERT INTO users (username, password_hash, email, phone, role) VALUES
('mem001', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'an.nguyen@example.com',     '0900000001', 'MEMBER'),
('mem002', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'binh.tran@example.com',     '0900000002', 'MEMBER'),
('mem003', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'cuong.le@example.com',      '0900000003', 'MEMBER'),
('mem004', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'dung.pham@example.com',     '0900000004', 'MEMBER'),
('mem005', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'duc.hoang@example.com',     '0900000005', 'MEMBER'),
('mem006', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'hoa.vu@example.com',        '0900000006', 'MEMBER'),
('mem007', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'hung.dang@example.com',     '0900000007', 'MEMBER'),
('mem008', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'lan.bui@example.com',       '0900000008', 'MEMBER'),
('mem009', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'huy.do@example.com',        '0900000009', 'MEMBER'),
('mem010', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'mai.ngo@example.com',       '0900000010', 'MEMBER'),
('mem011', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'nam.duong@example.com',     '0900000011', 'MEMBER'),
('mem012', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'oanh.ly@example.com',       '0900000012', 'MEMBER'),
('mem013', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'phuc.phan@example.com',     '0900000013', 'MEMBER'),
('mem014', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'quynh.vo@example.com',      '0900000014', 'MEMBER'),
('mem015', '$2a$12$j1j8IrGCOAQbJyYEuM7.VOzGUplD3oNw6.cgBvT0FqS0ANffTvz3a', 'son.truong@example.com',    '0900000015', 'MEMBER');
GO

-- 2) Create the member profiles, linking to the accounts by username so this
--    works regardless of the auto-generated user ids.
INSERT INTO members (user_id, full_name, gender, date_of_birth, address, package_id, join_date, expiry_date, status)
SELECT u.id, x.full_name, x.gender, x.dob, x.address, x.package_id, x.join_date, x.expiry_date, x.status
FROM (VALUES
    ('mem001', N'Nguyễn Văn An',    'MALE',   CAST('1995-03-12' AS DATE), N'12 Lê Lợi, Q1, TP.HCM',        2, CAST('2025-01-10' AS DATE), CAST('2025-04-10' AS DATE), 'ACTIVE'),
    ('mem002', N'Trần Thị Bình',    'FEMALE', CAST('1998-07-25' AS DATE), N'45 Nguyễn Huệ, Q1, TP.HCM',     1, CAST('2025-02-01' AS DATE), CAST('2025-03-01' AS DATE), 'ACTIVE'),
    ('mem003', N'Lê Hoàng Cường',   'MALE',   CAST('1992-11-03' AS DATE), N'78 Trần Hưng Đạo, Q5, TP.HCM',  3, CAST('2024-12-15' AS DATE), CAST('2025-06-15' AS DATE), 'ACTIVE'),
    ('mem004', N'Phạm Thị Dung',    'FEMALE', CAST('2000-05-18' AS DATE), N'23 Hai Bà Trưng, Q3, TP.HCM',   2, CAST('2025-01-20' AS DATE), CAST('2025-04-20' AS DATE), 'INACTIVE'),
    ('mem005', N'Hoàng Minh Đức',   'MALE',   CAST('1990-09-09' AS DATE), N'5 Cách Mạng Tháng 8, Q10',      1, CAST('2025-03-05' AS DATE), CAST('2025-04-05' AS DATE), 'ACTIVE'),
    ('mem006', N'Vũ Thị Hoa',       'FEMALE', CAST('1997-01-30' AS DATE), N'90 Lý Thường Kiệt, Q11',        3, CAST('2024-11-01' AS DATE), CAST('2025-05-01' AS DATE), 'SUSPENDED'),
    ('mem007', N'Đặng Văn Hùng',    'MALE',   CAST('1993-06-14' AS DATE), N'33 Điện Biên Phủ, Bình Thạnh',  2, CAST('2025-02-10' AS DATE), CAST('2025-05-10' AS DATE), 'ACTIVE'),
    ('mem008', N'Bùi Thị Lan',      'FEMALE', CAST('1999-08-22' AS DATE), N'17 Phan Xích Long, Phú Nhuận',  1, CAST('2025-03-12' AS DATE), CAST('2025-04-12' AS DATE), 'ACTIVE'),
    ('mem009', N'Đỗ Quang Huy',     'MALE',   CAST('1996-04-07' AS DATE), N'8 Nguyễn Trãi, Q5, TP.HCM',     3, CAST('2024-10-20' AS DATE), CAST('2025-04-20' AS DATE), 'ACTIVE'),
    ('mem010', N'Ngô Thị Mai',      'FEMALE', CAST('2001-12-01' AS DATE), N'62 Võ Văn Tần, Q3, TP.HCM',     2, CAST('2025-01-25' AS DATE), CAST('2025-04-25' AS DATE), 'INACTIVE'),
    ('mem011', N'Dương Văn Nam',    'MALE',   CAST('1994-02-19' AS DATE), N'100 Lê Văn Sỹ, Tân Bình',       1, CAST('2025-03-01' AS DATE), CAST('2025-04-01' AS DATE), 'ACTIVE'),
    ('mem012', N'Lý Thị Oanh',      'FEMALE', CAST('1998-10-11' AS DATE), N'29 Cộng Hòa, Tân Bình',         3, CAST('2024-09-15' AS DATE), CAST('2025-03-15' AS DATE), 'ACTIVE'),
    ('mem013', N'Phan Văn Phúc',    'MALE',   CAST('1991-07-08' AS DATE), N'14 Hoàng Văn Thụ, Phú Nhuận',   2, CAST('2025-02-18' AS DATE), CAST('2025-05-18' AS DATE), 'ACTIVE'),
    ('mem014', N'Võ Thị Quỳnh',     'FEMALE', CAST('2000-03-27' AS DATE), N'56 Nguyễn Thị Minh Khai, Q1',  1, CAST('2025-03-20' AS DATE), CAST('2025-04-20' AS DATE), 'SUSPENDED'),
    ('mem015', N'Trương Văn Sơn',   'MALE',   CAST('1995-11-16' AS DATE), N'71 Pasteur, Q3, TP.HCM',        3, CAST('2024-12-01' AS DATE), CAST('2025-06-01' AS DATE), 'ACTIVE')
) AS x(username, full_name, gender, dob, address, package_id, join_date, expiry_date, status)
JOIN users u ON u.username = x.username;
GO

PRINT '15 sample members inserted.';
GO

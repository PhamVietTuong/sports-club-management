# Business Logic — Hệ thống Quản lý Câu lạc bộ Thể thao

Tài liệu này mô tả **toàn bộ nghiệp vụ (business logic)** của hệ thống: vai trò,
quy trình, ràng buộc và các quy tắc kiểm tra. Thuật ngữ kỹ thuật (tên bảng, cột,
endpoint, trạng thái) giữ nguyên tiếng Anh để khớp với mã nguồn.

- **Backend:** ASP.NET Core 9 (C#), EF Core + SQL Server, JWT.
- **Frontend:** React 18 + TypeScript (Vite).
- Tầng truy cập dữ liệu theo **DAO** (`Repositories/`), truy vấn LINQ tham số hoá.

---

## 1. Vai trò & phân quyền (Roles & Authorization)

Có 3 vai trò, xác thực bằng **JWT bearer** (không session, không cookie):

| Tiền tố route | Vai trò bắt buộc | Controller |
|---|---|---|
| `/api/admin/*`  | `ADMIN`  | `Controllers/Admin/*` |
| `/api/coach/*`  | `COACH`  | `CoachController` |
| `/api/member/*` | `MEMBER` | `MemberController` |
| `/api/auth/*`   | công khai | `AuthController` |

Mỗi endpoint gắn `[Authorize(Roles = UserRole.X)]`. SPA lưu token và gửi trong
header `Authorization: Bearer`.

---

## 2. Bảo mật (Security invariants)

- **Băm mật khẩu:** BCrypt cost 12 (`Security/PasswordHasher.cs`); tương thích hash
  `$2a$` cũ.
- **Chống dò mật khẩu (brute-force):** 5 lần đăng nhập sai trong 15 phút → khoá
  tài khoản (bảng `login_attempts`, kiểm tra trong `AuthController.Login`).
- **Chính sách mật khẩu:** tối thiểu 8 ký tự, có ≥1 chữ cái và ≥1 chữ số
  (`Security/PasswordPolicy.cs`).
- **Chống dò tài khoản (account enumeration):** đăng ký/đăng nhập trả về một
  thông báo chung chung.
- **Chống IDOR:** ví dụ `CoachController.ClassDetail` trả 403 nếu lớp không thuộc
  về HLV đang đăng nhập; thành viên chỉ thao tác trên dữ liệu của chính mình.
- **Đổi mật khẩu:** bắt buộc xác minh lại mật khẩu hiện tại.
- **Security headers:** `Security/SecurityHeadersMiddleware.cs` (X-Frame-Options,
  X-Content-Type-Options, HSTS…) áp dụng cho mọi response.
- **Rate limiting:** giới hạn theo IP cho endpoint đăng nhập (HTTP 429).
- **Validate dữ liệu:** email, số điện thoại (regex `0xxxxxxxxx` / `+84xxxxxxxxx`).

---

## 3. Quy trình đăng ký gói tập (Membership request lifecycle)

Bảng nguồn sự thật: **`membership_requests`**. Vòng đời trạng thái:

```
PENDING ──(admin duyệt)──> APPROVED ──(kích hoạt / đăng ký lớp đầu tiên)──> ACTIVE
   │                          │
   │(admin từ chối)           │(hủy/đổi trong 24h)
   ▼                          ▼
REJECTED                   CANCELLED
```

### 3.1. Gửi yêu cầu — `POST /api/member/membership/request`
- Tạo bản ghi **`PENDING`**, **chưa thu tiền**. Lưu `amount` (giá tại thời điểm
  yêu cầu) và `method` (CASH/CARD/TRANSFER).
- **Chỉ một yêu cầu đang xử lý:** chặn nếu đã có yêu cầu `PENDING` hoặc `APPROVED`.
- **RULE 6 — không trùng kỳ hạn gói:** chặn nếu thành viên đang có gói còn hiệu
  lực (`Status = ACTIVE` và `ExpiryDate >= hôm nay`). Chỉ một gói hoạt động tại
  một thời điểm.

### 3.2. Admin duyệt / từ chối
- `POST /api/admin/membership-requests/{id}/approve`: `PENDING → APPROVED`, ghi
  `approved_at`. **Việc duyệt KHÔNG thu tiền và KHÔNG kích hoạt** — chỉ mở cửa sổ
  ân hạn.
- `POST .../reject`: `PENDING → REJECTED`, kèm `note` (lý do).

### 3.3. Cửa sổ ân hạn 24 giờ (`IsModifiable()`)
Sau khi được duyệt, thành viên còn quyền **hủy hoặc đổi gói** khi và chỉ khi:
- Trạng thái là `PENDING`, **hoặc**
- Trạng thái là `APPROVED` **và** `DateTime.Now < approved_at + 24 giờ`.

Khi đã `ACTIVE` (đã kích hoạt hoặc đã đăng ký lớp đầu tiên) thì **khoá**, không
hủy/đổi được nữa.

- **Hủy** — `POST .../{id}/cancel`: chuyển `CANCELLED`.
- **Đổi gói** — `POST .../{id}/change`: hủy yêu cầu hiện tại (`CANCELLED`) và tạo
  yêu cầu `PENDING` mới cho gói mới (cần duyệt lại).
- **Kích hoạt** — `POST .../{id}/activate`: chỉ khi `APPROVED`.

### 3.4. Kích hoạt (`ActivateMembershipAsync`)
Khi kích hoạt (chủ động hoặc tự động khi đăng ký lớp đầu tiên):
1. Tạo `payments` trạng thái `COMPLETED` (số tiền = `amount` đã chốt).
2. Cập nhật member: `PackageId`, `Status = ACTIVE`, `ExpiryDate` cộng dồn
   (`max(hôm nay, hạn cũ) + DurationMonths`).
3. Cập nhật request: `ACTIVE`, `start_date = hôm nay`, `activated_at = now`.

---

## 4. Quan hệ Gói tập ⇄ Lớp học (Package ⇄ Class)

Bảng liên kết: **`package_classes`** (unique `(package_id, class_id)`).
- Admin cấu hình gói được học những lớp nào (`PUT /api/admin/packages/{id}/classes`).
- **Gói "hiệu lực" của thành viên** (`FindEffectiveAsync`): gói của yêu cầu `ACTIVE`
  gần nhất; nếu chưa có thì gói của yêu cầu `APPROVED` gần nhất.
- Thành viên **chỉ thấy và chỉ đăng ký được** các lớp gắn với gói hiệu lực
  (`GET /api/member/classes` đã lọc theo `package_classes`).
- **Hạn mức lớp:** nếu `package.MaxClasses > 0`, số lớp `ACTIVE` của thành viên
  không vượt quá `MaxClasses`.

---

## 5. Đăng ký lớp (Enrollment) — `POST /api/member/classes/{id}/enroll`

Kiểm tra theo thứ tự:
1. Lớp tồn tại và `IsActive`.
2. Có gói hiệu lực (ACTIVE hoặc APPROVED); nếu không → từ chối.
3. Lớp thuộc gói (`package_classes`); nếu không → từ chối.
4. Chưa đăng ký lớp này (enrollment `ACTIVE`).
5. Chưa vượt `MaxClasses`.
6. **RULE 3 — không trùng giờ:** lịch lớp mới không được trùng (cùng thứ + giao
   thời gian) với các lớp thành viên đang theo học.
7. Nếu gói đang `APPROVED` → **tự động kích hoạt** (mục 3.4) rồi mới ghi danh.
8. **Giữ chỗ nguyên tử:** `ClassRepository.TryIncrementEnrolledAsync` (một câu
   UPDATE có điều kiện) chống đăng ký vượt sức chứa khi truy cập đồng thời.

- **Hủy đăng ký** — `POST .../{id}/cancel`: chuyển enrollment `CANCELLED` và trả
  lại 1 chỗ (`DecrementEnrolledAsync`).
- **Tự check-in** — `POST .../{id}/checkin`: chỉ khi đang ghi danh `ACTIVE`; ghi
  `attendance` trạng thái `PRESENT` cho ngày hôm nay.

---

## 6. HLV nhận / trả lớp (Coach claim / release → admin duyệt)

Bảng: **`class_change_requests`** (`action` = CLAIM/RELEASE; status PENDING/
APPROVED/REJECTED). HLV **không** tự gán lớp trực tiếp nữa.

### 6.1. HLV gửi yêu cầu
- **CLAIM** — `POST /api/coach/classes/{id}/claim`: lớp phải `IsActive`, **chưa có
  HLV**, không có yêu cầu `PENDING` nào cho lớp đó, và **RULE 1 — không trùng lịch
  dạy** (lịch lớp không đụng thời khoá biểu hiện tại của HLV).
- **RELEASE** — `POST .../release`: HLV phải đang phụ trách lớp, không có yêu cầu
  `PENDING` trùng.
- `GET /api/coach/class-requests`: xem yêu cầu của mình.

### 6.2. Admin duyệt — `POST /api/admin/class-requests/{id}/approve`
- Với **CLAIM**: **kiểm tra lại RULE 1** (thời khoá biểu có thể đã đổi), rồi gán
  nguyên tử `TryClaimAsync` (chỉ thành công nếu lớp vẫn trống HLV).
- Với **RELEASE**: `ReleaseAsync` (chỉ khi HLV vẫn sở hữu lớp).
- Nếu thao tác nguyên tử thất bại → trả lỗi, không đổi trạng thái.
- `POST .../reject`: từ chối kèm `note`.

---

## 7. Lịch tập (Schedules) — admin CRUD, có thể chỉnh sửa

Bảng `schedules` (theo tuần: `day_of_week`, `start_time`, `end_time`, `room`).
Tạo/sửa (`POST`/`PUT /api/admin/schedules`) kiểm tra (`ValidateSlotAsync`):
1. `end_time > start_time`.
2. Không trùng giờ với **buổi khác của cùng lớp** trong cùng ngày.
3. **RULE 2 — một phòng một lớp:** không có lớp khác dùng cùng `room`, cùng thứ,
   giao thời gian.
4. **RULE 1 — HLV không dạy 2 lớp cùng lúc:** nếu lớp đã có HLV, slot mới không
   được trùng các lớp khác của HLV đó.

Khi admin **gán HLV** cho lớp đã có lịch (`PUT /api/admin/classes/{id}`) cũng áp
dụng RULE 1.

---

## 8. Đặt lịch PT (Personal Training)

Bảng `pt_sessions` (status PENDING/CONFIRMED/CANCELLED/COMPLETED).

### 8.1. Thành viên đặt — `POST /api/member/pt-sessions`
- HLV phải `ACTIVE`; giờ hợp lệ, `end > start`.
- **RULE — không trùng lớp của HLV:** slot PT không đụng thời khoá biểu dạy của
  HLV trong ngày đó.
- **RULE 4 — không trùng lớp của chính thành viên:** slot PT không đụng lớp thành
  viên đã đăng ký.
- Tạo ở trạng thái `PENDING`.

### 8.2. HLV xử lý — `POST /api/coach/pt-sessions/{id}/status`
- Khi **CONFIRMED**: kiểm tra lại trùng lớp dạy, và **RULE 5 — HLV không nhận 2
  buổi PT trùng giờ** (không có buổi `CONFIRMED` khác cùng ngày giao thời gian).
- `COMPLETED` / `CANCELLED` theo luồng.
- Thành viên có thể tự hủy buổi `PENDING`/`CONFIRMED`.

---

## 9. Điểm danh (Attendance)

Bảng `attendance` (unique `(class_id, member_id, session_date)`; status
PRESENT/ABSENT/LATE).
- HLV điểm danh từng buổi (`POST /api/coach/classes/{id}/attendance`): thành viên
  phải đang ghi danh lớp đó (IDOR-guarded: chỉ lớp của HLV).
- Hệ thống hiển thị **lịch cố định của lớp** và **cảnh báo** nếu ngày điểm danh
  không nằm trong lịch (cho phép buổi bù).
- Thành viên tự check-in (mục 5).

---

## 10. Giáo án & Tiến độ (Lesson plans / Progress notes)

- **Lesson plans** (`lesson_plans`): HLV tạo cho lớp **của mình**; thành viên xem
  giáo án của các lớp **đang ghi danh** (`GET /api/member/lesson-plans`).
- **Progress notes** (`progress_notes`): HLV ghi nhận xét cho thành viên — thành
  viên phải thuộc một lớp **đang ACTIVE** của HLV; nếu chỉ định lớp thì lớp đó
  phải của HLV. Điểm `rating` 1–5.

---

## 11. Đánh giá HLV (Coach ratings)

Bảng `coach_ratings` (unique `(member_id, coach_id)` — mỗi cặp một đánh giá).
- Thành viên chỉ đánh giá HLV của lớp **mình đã/đang tham gia** (`canRate`).
- `rating` 1–5; có điểm trung bình và số lượt cho mỗi HLV.

---

## 12. Theo dõi sức khỏe (Health metrics)

Bảng `health_metrics`. Thành viên tự ghi (cân nặng, chiều cao, % mỡ, ghi chú) và
**chỉ thao tác bản ghi của chính mình** (xóa/sửa bị chặn nếu không phải chủ sở hữu).

---

## 13. Thanh toán & Doanh thu (Payments & Revenue)

Bảng `payments` (method CASH/CARD/TRANSFER; status PENDING/COMPLETED/REFUNDED).
- Bản ghi `COMPLETED` được tạo khi **kích hoạt gói** (mục 3.4).
- Báo cáo doanh thu (`/api/admin/payments/revenue`) tổng hợp các khoản
  `COMPLETED`, có bóc tách theo tháng.

---

## 14. Quản trị danh mục (Admin master data)

- **Thành viên** (`members`, status ACTIVE/INACTIVE/SUSPENDED): admin tạo (kèm tài
  khoản đăng nhập), đổi trạng thái, **clone** từ mẫu (Prototype).
- **HLV** (`coaches`, status ACTIVE/UNDER_REVIEW/TERMINATED): tạo/sửa, đổi trạng
  thái (xem xét / cho nghỉ).
- **Lớp học** (`training_classes`, level BEGINNER/INTERMEDIATE/ADVANCED): CRUD,
  clone; gán HLV (có RULE 1).
- **Gói tập** (`training_packages`): CRUD, clone **+20% giá** (Prototype), gán lớp.
- **Thiết bị** (`equipment`, status AVAILABLE/IN_USE/MAINTENANCE/RETIRED): CRUD.

---

## 15. Nhắn tin (Chat)

Bảng `messages`. Trò chuyện trực tiếp **HLV ↔ thành viên** theo thời gian thực
qua **SignalR** (hub `/hubs/chat`), không polling.

---

## 16. Phân trang & lọc (Pagination & Filtering)

- **Mặc định 10 dòng/trang**, điều hướng "Trước / Sau" (xem 10 dòng kế tiếp).
- **Server-side** (`PagedResult<T>` = `{ items, total, page, pageSize }`,
  `?page&pageSize&search&status`) cho các trang danh sách chính:
  - Admin: members, coaches, classes, schedules, packages, equipment, payments,
    membership-requests, class-requests.
  - Coach: lesson-plans, progress, pt-sessions.
  - Member: payments, lesson-plans, progress, health, coaches, pt-sessions.
- **Client-side** (`useClientPaged`) cho bảng theo ngữ cảnh/đã tải đủ: danh sách
  lớp của thành viên, yêu cầu gói, lịch sử điểm danh; lớp của HLV + danh sách học
  viên, roster điểm danh, trang "Nhận lớp" (3 bảng), đánh giá của HLV.
- Endpoint dùng chung cho dropdown được gọi với `?pageSize=1000` để lấy đủ.

---

## 17. Tổng hợp 6 quy tắc chống trùng lịch (Conflict rules)

| # | Quy tắc | Điểm kiểm tra |
|---|---|---|
| 1 | HLV không dạy 2 lớp cùng lúc | claim, admin duyệt claim, admin gán HLV, tạo/sửa lịch |
| 2 | Một phòng không 2 lớp cùng lúc | tạo/sửa lịch (room + thứ + giờ) |
| 3 | Thành viên không học 2 lớp cùng lúc | đăng ký lớp |
| 4 | PT không trùng lớp đã đăng ký của thành viên | đặt PT |
| 5 | HLV không nhận 2 buổi PT trùng giờ | HLV xác nhận PT |
| 6 | Mỗi thành viên một gói còn hiệu lực | gửi yêu cầu gói |

Logic so trùng dùng chung ở `Services/ScheduleClash.cs`
(`FindConflict`, `FindClassClash`, `FindRoomClash`, `TimesOverlap`): hai khoảng
thời gian trùng khi `start1 < end2 && start2 < end1` (cùng thứ/ngày).

---

## 18. Mẫu thiết kế (Design patterns — yêu cầu đồ án)

- **Singleton** — `Patterns/Singleton/DatabaseConfig.cs` (double-checked locking)
  dựng connection string một lần.
- **Prototype** — `ISportClubPrototype<T>` + `Clone()`; dùng cho clone lớp/gói
  (+20% giá)/lịch/mẫu thành viên.
- **Iterator** — `Patterns/Iterator/ClubCollection<T>` + `ClubIterator<T>`; mọi
  endpoint danh sách duyệt qua iterator (kể cả khi phân trang, qua
  `PagedResult.MapIterating`).
- **DAO** — `Repositories/*Repository.cs`, mỗi bảng một lớp, LINQ tham số hoá.

---

## 19. Bảng trạng thái (Status reference)

| Thực thể | Trường | Giá trị hợp lệ |
|---|---|---|
| User | role | ADMIN, COACH, MEMBER |
| Member | status | ACTIVE, INACTIVE, SUSPENDED |
| Coach | status | ACTIVE, UNDER_REVIEW, TERMINATED |
| TrainingClass | level | BEGINNER, INTERMEDIATE, ADVANCED |
| Schedule | day_of_week | MONDAY … SUNDAY |
| Enrollment | status | ACTIVE, CANCELLED |
| Equipment | status | AVAILABLE, IN_USE, MAINTENANCE, RETIRED |
| Payment | method / status | CASH/CARD/TRANSFER · PENDING/COMPLETED/REFUNDED |
| Attendance | status | PRESENT, ABSENT, LATE |
| PtSession | status | PENDING, CONFIRMED, CANCELLED, COMPLETED |
| MembershipRequest | status | PENDING, APPROVED, ACTIVE, REJECTED, CANCELLED |
| ClassChangeRequest | action / status | CLAIM/RELEASE · PENDING/APPROVED/REJECTED |

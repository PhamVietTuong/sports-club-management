<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Bảng điều khiển thành viên — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>

<nav class="member-nav">
    <a href="${pageContext.request.contextPath}/member/dashboard" class="member-nav-logo">
        <div class="member-nav-logo-badge">SC</div>
        Sports Club
    </a>
    <div class="member-nav-links">
        <a href="${pageContext.request.contextPath}/member/dashboard" class="active">Trang chủ</a>
        <a href="${pageContext.request.contextPath}/member/classes">Lớp học</a>
        <a href="${pageContext.request.contextPath}/member/profile">Hồ sơ của tôi</a>
    </div>
    <div class="member-nav-actions">
        <span class="nav-username"><c:out value="${member.fullName}"/></span>
        <form method="post" action="${pageContext.request.contextPath}/logout" class="d-inline">
            <input type="hidden" name="_csrf" value="${csrfToken}">
            <button type="submit" class="btn btn-ghost btn-sm">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
                    <polyline points="16 17 21 12 16 7"/>
                    <line x1="21" y1="12" x2="9" y2="12"/>
                </svg>
                Đăng xuất
            </button>
        </form>
    </div>
</nav>

<div class="page-body">

    <c:if test="${not empty sessionScope.flash}">
        <div class="alert alert-info"><c:out value="${sessionScope.flash}"/></div>
        <c:remove var="flash" scope="session"/>
    </c:if>

    <h2 class="fs-22 mb-4px">
        Chào mừng trở lại, <c:out value="${member.fullName}"/>!
    </h2>
    <p class="text-muted fs-13 mb-4">
        Đây là tổng quan về hội viên của bạn.
    </p>

    <div class="grid-2">
        <!-- Membership Status Card -->
        <div class="stat-card">
            <div class="stat-label mb-2">Trạng thái hội viên</div>
            <div class="mb-8px">
                <c:choose>
                    <c:when test="${member.status eq 'ACTIVE'}">
                        <span class="badge badge-success fs-13">
                            HOẠT ĐỘNG
                        </span>
                    </c:when>
                    <c:when test="${member.status eq 'SUSPENDED'}">
                        <span class="badge badge-danger fs-13">
                            TẠM KHÓA
                        </span>
                    </c:when>
                    <c:otherwise>
                        <span class="badge badge-warning fs-13">
                            NGỪNG HOẠT ĐỘNG
                        </span>
                    </c:otherwise>
                </c:choose>
            </div>
            <p class="fs-13 text-muted m-0">
                Hết hạn:
                <strong class="text-default">
                    <c:out value="${member.expiryDate != null ? member.expiryDate : 'Không có'}"/>
                </strong>
            </p>
        </div>

        <!-- Enrollments Card -->
        <div class="stat-card">
            <div class="stat-icon blue mb-2">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                    <line x1="16" y1="2" x2="16" y2="6"/>
                    <line x1="8" y1="2" x2="8" y2="6"/>
                    <line x1="3" y1="10" x2="21" y2="10"/>
                </svg>
            </div>
            <div class="stat-number">${enrollments.size()}</div>
            <div class="stat-label">Lớp học đã đăng ký</div>
            <a href="${pageContext.request.contextPath}/member/classes"
               class="btn btn-primary btn-sm mt-12px">
                Xem các lớp học
            </a>
        </div>
    </div>

    <div class="section-title mt-4">Lịch tập hàng tuần của tôi</div>
    <div class="table-wrap">
        <table class="data-table">
            <thead>
                <tr>
                    <th>Lớp học</th>
                    <th>Ngày</th>
                    <th>Bắt đầu</th>
                    <th>Kết thúc</th>
                    <th>Phòng</th>
                </tr>
            </thead>
            <tbody>
                <c:forEach var="s" items="${schedules}">
                <tr>
                    <td><c:out value="${s.className}"/></td>
                    <td>
                        <c:choose>
                            <c:when test="${s.dayOfWeek eq 'MONDAY'}">Thứ Hai</c:when>
                            <c:when test="${s.dayOfWeek eq 'TUESDAY'}">Thứ Ba</c:when>
                            <c:when test="${s.dayOfWeek eq 'WEDNESDAY'}">Thứ Tư</c:when>
                            <c:when test="${s.dayOfWeek eq 'THURSDAY'}">Thứ Năm</c:when>
                            <c:when test="${s.dayOfWeek eq 'FRIDAY'}">Thứ Sáu</c:when>
                            <c:when test="${s.dayOfWeek eq 'SATURDAY'}">Thứ Bảy</c:when>
                            <c:when test="${s.dayOfWeek eq 'SUNDAY'}">Chủ Nhật</c:when>
                            <c:otherwise><c:out value="${s.dayOfWeek}"/></c:otherwise>
                        </c:choose>
                    </td>
                    <td><c:out value="${s.startTime}"/></td>
                    <td><c:out value="${s.endTime}"/></td>
                    <td><c:out value="${s.room}"/></td>
                </tr>
                </c:forEach>
                <c:if test="${empty schedules}">
                    <tr>
                        <td colspan="5" class="text-center text-muted empty-pad-32">Chưa có lớp học nào trong lịch.</td>
                    </tr>
                </c:if>
            </tbody>
        </table>
    </div>

</div>
</body>
</html>

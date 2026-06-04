<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Xem lớp học — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>

<nav class="member-nav">
    <a href="${pageContext.request.contextPath}/member/dashboard" class="member-nav-logo">
        <div class="member-nav-logo-badge">SC</div>
        Sports Club
    </a>
    <div class="member-nav-links">
        <a href="${pageContext.request.contextPath}/member/dashboard">Trang chủ</a>
        <a href="${pageContext.request.contextPath}/member/classes" class="active">Lớp học</a>
        <a href="${pageContext.request.contextPath}/member/profile">Hồ sơ của tôi</a>
    </div>
    <div class="member-nav-actions">
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

    <div class="page-header">
        <div>
            <div class="page-title">Xem lớp học</div>
            <div class="page-subtitle">Tìm và đăng ký buổi tập tiếp theo của bạn</div>
        </div>
    </div>

    <div class="grid-3">
        <c:forEach var="tc" items="${classes}">
        <div class="class-card">
            <c:choose>
                <c:when test="${tc.level eq 'BEGINNER'}">
                    <div class="class-card-stripe stripe-beginner"></div>
                </c:when>
                <c:when test="${tc.level eq 'INTERMEDIATE'}">
                    <div class="class-card-stripe stripe-intermediate"></div>
                </c:when>
                <c:when test="${tc.level eq 'ADVANCED'}">
                    <div class="class-card-stripe stripe-advanced"></div>
                </c:when>
                <c:otherwise>
                    <div class="class-card-stripe stripe-beginner"></div>
                </c:otherwise>
            </c:choose>
            <div class="class-card-body">
                <h5 class="fw-bold"><c:out value="${tc.name}"/></h5>
                <div class="mb-1">
                    <c:choose>
                        <c:when test="${tc.level eq 'BEGINNER'}">
                            <span class="badge badge-success">CƠ BẢN</span>
                        </c:when>
                        <c:when test="${tc.level eq 'INTERMEDIATE'}">
                            <span class="badge badge-warning">TRUNG CẤP</span>
                        </c:when>
                        <c:when test="${tc.level eq 'ADVANCED'}">
                            <span class="badge badge-danger">NÂNG CAO</span>
                        </c:when>
                        <c:otherwise>
                            <span class="badge badge-muted"><c:out value="${tc.level}"/></span>
                        </c:otherwise>
                    </c:choose>
                </div>
                <p class="by-coach">bởi <c:out value="${tc.coachName}"/></p>
                <p class="description"><c:out value="${tc.description}"/></p>
                <div class="progress-bar-wrap">
                    <div class="progress-bar-fill ${(tc.capacity - tc.availableSlots) >= tc.capacity ? 'full' : (tc.capacity - tc.availableSlots) < tc.capacity * 0.5 ? 'low' : ''}"
                         data-width="${(tc.capacity - tc.availableSlots) * 100 / (tc.capacity > 0 ? tc.capacity : 1)}"></div>
                </div>
                <p class="capacity-text">
                    <span class="text-default fw-600">${tc.capacity - tc.availableSlots}</span>
                    / <c:out value="${tc.capacity}"/> chỗ đã đầy
                </p>
            </div>
            <div class="class-card-footer">
                <c:choose>
                    <c:when test="${tc.availableSlots > 0}">
                        <form method="post" action="${pageContext.request.contextPath}/member/classes">
                            <input type="hidden" name="_csrf"   value="${csrfToken}">
                            <input type="hidden" name="action"  value="enroll">
                            <input type="hidden" name="classId" value="${tc.id}">
                            <button type="submit" class="btn btn-primary w-100">Đăng ký</button>
                        </form>
                    </c:when>
                    <c:otherwise>
                        <button class="btn btn-ghost w-100" disabled>Lớp đã đầy</button>
                    </c:otherwise>
                </c:choose>
            </div>
        </div>
        </c:forEach>
        <c:if test="${empty classes}">
            <p class="text-muted col-span-full pad-24-0">
                Hiện tại không có lớp học nào.
            </p>
        </c:if>
    </div>

</div>
<script src="${pageContext.request.contextPath}/assets/js/app.js"></script>
</body>
</html>

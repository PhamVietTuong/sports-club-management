<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Lớp học của tôi — Huấn luyện viên</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<div class="app-shell">
    <!-- Inline Coach Sidebar — My Classes active -->
    <aside class="sidebar">
        <div class="sidebar-logo">
            <div class="sidebar-monogram">SC</div>
            <span class="sidebar-name">Sports Club</span>
        </div>
        <nav class="sidebar-nav">
            <a href="${pageContext.request.contextPath}/coach/dashboard">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="3" width="7" height="7" rx="1"/>
                    <rect x="14" y="3" width="7" height="7" rx="1"/>
                    <rect x="14" y="14" width="7" height="7" rx="1"/>
                    <rect x="3" y="14" width="7" height="7" rx="1"/>
                </svg>
                Bảng điều khiển
            </a>
            <a href="${pageContext.request.contextPath}/coach/classes" class="active">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                    <line x1="16" y1="2" x2="16" y2="6"/>
                    <line x1="8" y1="2" x2="8" y2="6"/>
                    <line x1="3" y1="10" x2="21" y2="10"/>
                </svg>
                Lớp học của tôi
            </a>
        </nav>
        <div class="sidebar-footer">
            <div class="sidebar-user-info">
                <div class="sidebar-user-name">
                    <c:out value="${sessionScope.loggedInUser.username}"/>
                </div>
                Huấn luyện viên
            </div>
            <form method="post" action="${pageContext.request.contextPath}/logout">
                <input type="hidden" name="_csrf" value="${csrfToken}">
                <button type="submit">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                         stroke="currentColor" stroke-width="1.5">
                        <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
                        <polyline points="16 17 21 12 16 7"/>
                        <line x1="21" y1="12" x2="9" y2="12"/>
                    </svg>
                    Đăng xuất
                </button>
            </form>
        </div>
    </aside>

    <div class="main-content">
        <header class="top-bar">
            <span class="top-bar-title">Lớp học của tôi</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Huấn luyện viên)</span>
                <div class="user-avatar">
                    <c:out value="${not empty sessionScope.loggedInUser.username ? sessionScope.loggedInUser.username.substring(0,1).toUpperCase() : '?'}"/>
                </div>
            </div>
        </header>

        <div class="page-body">
            <div class="page-header">
                <div class="page-title">Lớp học của tôi</div>
            </div>

            <div class="grid-3">
                <c:forEach var="tc" items="${myClasses}">
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
                        <div class="mb-2">
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
                        <p class="fs-13 text-muted mb-8px">
                            Đã đăng ký:
                            <strong class="text-default">
                                <c:out value="${tc.currentEnrolled}"/>
                            </strong>
                            / <c:out value="${tc.capacity}"/>
                        </p>
                        <div class="progress-bar-wrap">
                            <div class="progress-bar-fill ${tc.currentEnrolled >= tc.capacity ? 'full' : tc.currentEnrolled < tc.capacity * 0.5 ? 'low' : ''}"
                                 data-width="${tc.currentEnrolled * 100 / (tc.capacity > 0 ? tc.capacity : 1)}"></div>
                        </div>
                    </div>
                    <div class="class-card-footer">
                        <a href="?classId=${tc.id}" class="btn btn-ghost btn-sm w-100">Xem thành viên</a>
                    </div>
                </div>
                </c:forEach>
                <c:if test="${empty myClasses}">
                    <p class="text-muted col-span-full pad-24-0">
                        Chưa có lớp học nào được phân công.
                    </p>
                </c:if>
            </div>

            <c:if test="${not empty selectedClass}">
                <div class="section-title mt-4">
                    Thành viên đã đăng ký &mdash; <c:out value="${selectedClass.name}"/>
                </div>
                <div class="table-wrap">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Thành viên</th>
                                <th>Ngày đăng ký</th>
                                <th>Trạng thái</th>
                            </tr>
                        </thead>
                        <tbody>
                            <c:forEach var="e" items="${enrolledMembers}">
                            <tr>
                                <td><c:out value="${e.memberName}"/></td>
                                <td><c:out value="${e.enrollDate}"/></td>
                                <td>
                                    <c:choose>
                                        <c:when test="${e.status eq 'ACTIVE'}">
                                            <span class="badge badge-success">
                                                HOẠT ĐỘNG
                                            </span>
                                        </c:when>
                                        <c:when test="${e.status eq 'CANCELLED'}">
                                            <span class="badge badge-danger">
                                                ĐÃ HỦY
                                            </span>
                                        </c:when>
                                        <c:otherwise>
                                            <span class="badge badge-muted">
                                                <c:out value="${e.status}"/>
                                            </span>
                                        </c:otherwise>
                                    </c:choose>
                                </td>
                            </tr>
                            </c:forEach>
                            <c:if test="${empty enrolledMembers}">
                                <tr>
                                    <td colspan="3" class="text-center text-muted empty-pad-32">Chưa có thành viên đăng ký.</td>
                                </tr>
                            </c:if>
                        </tbody>
                    </table>
                </div>
            </c:if>

        </div>
    </div>
</div>
<script src="${pageContext.request.contextPath}/assets/js/app.js"></script>
</body>
</html>

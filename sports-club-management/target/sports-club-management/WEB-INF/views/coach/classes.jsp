<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>My Classes — Coach</title>
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
                Dashboard
            </a>
            <a href="${pageContext.request.contextPath}/coach/classes" class="active">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                    <line x1="16" y1="2" x2="16" y2="6"/>
                    <line x1="8" y1="2" x2="8" y2="6"/>
                    <line x1="3" y1="10" x2="21" y2="10"/>
                </svg>
                My Classes
            </a>
        </nav>
        <div class="sidebar-footer">
            <div style="padding:0 2px 10px;font-size:12px;color:var(--muted);">
                <div style="font-weight:600;color:var(--text);margin-bottom:2px;">
                    <c:out value="${sessionScope.loggedInUser.username}"/>
                </div>
                Coach
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
                    Logout
                </button>
            </form>
        </div>
    </aside>

    <div class="main-content">
        <header class="top-bar">
            <span class="top-bar-title">My Classes</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Coach)</span>
                <div class="user-avatar">
                    ${sessionScope.loggedInUser.username.substring(0,1).toUpperCase()}
                </div>
            </div>
        </header>

        <div class="page-body">
            <div class="page-header">
                <div class="page-title">My Classes</div>
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
                        <div style="margin-bottom:10px;">
                            <c:choose>
                                <c:when test="${tc.level eq 'BEGINNER'}">
                                    <span class="badge badge-success">BEGINNER</span>
                                </c:when>
                                <c:when test="${tc.level eq 'INTERMEDIATE'}">
                                    <span class="badge badge-warning">INTERMEDIATE</span>
                                </c:when>
                                <c:when test="${tc.level eq 'ADVANCED'}">
                                    <span class="badge badge-danger">ADVANCED</span>
                                </c:when>
                                <c:otherwise>
                                    <span class="badge badge-muted"><c:out value="${tc.level}"/></span>
                                </c:otherwise>
                            </c:choose>
                        </div>
                        <p style="font-size:13px;color:var(--muted);margin-bottom:8px;">
                            Enrolled:
                            <strong style="color:var(--text);">
                                <c:out value="${tc.currentEnrolled}"/>
                            </strong>
                            / <c:out value="${tc.capacity}"/>
                        </p>
                        <div class="progress-bar-wrap">
                            <div class="progress-bar-fill ${tc.currentEnrolled >= tc.capacity ? 'full' : tc.currentEnrolled < tc.capacity * 0.5 ? 'low' : ''}"
                                 style="width: ${tc.currentEnrolled * 100 / (tc.capacity > 0 ? tc.capacity : 1)}%"></div>
                        </div>
                    </div>
                    <div class="class-card-footer">
                        <a href="?classId=${tc.id}" class="btn btn-ghost btn-sm w-100">View Members</a>
                    </div>
                </div>
                </c:forEach>
                <c:if test="${empty myClasses}">
                    <p class="text-muted" style="grid-column:1/-1;padding:24px 0;">
                        No classes assigned yet.
                    </p>
                </c:if>
            </div>

            <c:if test="${not empty selectedClass}">
                <div class="section-title mt-4">
                    Enrolled Members &mdash; <c:out value="${selectedClass.name}"/>
                </div>
                <div class="table-wrap">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Member</th>
                                <th>Enroll Date</th>
                                <th>Status</th>
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
                                                <c:out value="${e.status}"/>
                                            </span>
                                        </c:when>
                                        <c:when test="${e.status eq 'CANCELLED'}">
                                            <span class="badge badge-danger">
                                                <c:out value="${e.status}"/>
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
                                    <td colspan="3" class="text-center text-muted"
                                        style="padding:32px 14px;">No members enrolled.</td>
                                </tr>
                            </c:if>
                        </tbody>
                    </table>
                </div>
            </c:if>

        </div>
    </div>
</div>
</body>
</html>

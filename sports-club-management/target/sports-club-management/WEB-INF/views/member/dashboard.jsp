<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Member Dashboard — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>

<nav class="member-nav">
    <a href="${pageContext.request.contextPath}/member/dashboard" class="member-nav-logo">
        <div class="member-nav-logo-badge">SC</div>
        Sports Club
    </a>
    <div class="member-nav-links">
        <a href="${pageContext.request.contextPath}/member/dashboard" class="active">Dashboard</a>
        <a href="${pageContext.request.contextPath}/member/classes">Classes</a>
        <a href="${pageContext.request.contextPath}/member/profile">My Profile</a>
    </div>
    <div class="member-nav-actions">
        <span class="nav-username"><c:out value="${member.fullName}"/></span>
        <form method="post" action="${pageContext.request.contextPath}/logout" style="display:inline;">
            <input type="hidden" name="_csrf" value="${csrfToken}">
            <button type="submit" class="btn btn-ghost btn-sm">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
                    <polyline points="16 17 21 12 16 7"/>
                    <line x1="21" y1="12" x2="9" y2="12"/>
                </svg>
                Logout
            </button>
        </form>
    </div>
</nav>

<div class="page-body">

    <c:if test="${not empty sessionScope.flash}">
        <div class="alert alert-info"><c:out value="${sessionScope.flash}"/></div>
        <c:remove var="flash" scope="session"/>
    </c:if>

    <h2 style="font-size:22px;margin-bottom:4px;">
        Welcome back, <c:out value="${member.fullName}"/>!
    </h2>
    <p class="text-muted" style="font-size:13px;margin-bottom:24px;">
        Here's your membership overview.
    </p>

    <div class="grid-2">
        <!-- Membership Status Card -->
        <div class="stat-card">
            <div class="stat-label" style="margin-bottom:10px;">Membership Status</div>
            <div style="margin-bottom:8px;">
                <c:choose>
                    <c:when test="${member.status eq 'ACTIVE'}">
                        <span class="badge badge-success" style="font-size:13px;">
                            <c:out value="${member.status}"/>
                        </span>
                    </c:when>
                    <c:when test="${member.status eq 'SUSPENDED'}">
                        <span class="badge badge-danger" style="font-size:13px;">
                            <c:out value="${member.status}"/>
                        </span>
                    </c:when>
                    <c:otherwise>
                        <span class="badge badge-warning" style="font-size:13px;">
                            <c:out value="${member.status}"/>
                        </span>
                    </c:otherwise>
                </c:choose>
            </div>
            <p style="font-size:13px;color:var(--muted);margin:0;">
                Expires:
                <strong style="color:var(--text);">
                    <c:out value="${member.expiryDate != null ? member.expiryDate : 'N/A'}"/>
                </strong>
            </p>
        </div>

        <!-- Enrollments Card -->
        <div class="stat-card">
            <div class="stat-icon blue" style="margin-bottom:10px;">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                    <line x1="16" y1="2" x2="16" y2="6"/>
                    <line x1="8" y1="2" x2="8" y2="6"/>
                    <line x1="3" y1="10" x2="21" y2="10"/>
                </svg>
            </div>
            <div class="stat-number">${enrollments.size()}</div>
            <div class="stat-label">Enrolled Classes</div>
            <a href="${pageContext.request.contextPath}/member/classes"
               class="btn btn-primary btn-sm" style="margin-top:12px;">
                Browse Classes
            </a>
        </div>
    </div>

    <div class="section-title mt-4">My Weekly Schedule</div>
    <div class="table-wrap">
        <table class="data-table">
            <thead>
                <tr>
                    <th>Class</th>
                    <th>Day</th>
                    <th>Start</th>
                    <th>End</th>
                    <th>Room</th>
                </tr>
            </thead>
            <tbody>
                <c:forEach var="s" items="${schedules}">
                <tr>
                    <td><c:out value="${s.className}"/></td>
                    <td><c:out value="${s.dayOfWeek}"/></td>
                    <td><c:out value="${s.startTime}"/></td>
                    <td><c:out value="${s.endTime}"/></td>
                    <td><c:out value="${s.room}"/></td>
                </tr>
                </c:forEach>
                <c:if test="${empty schedules}">
                    <tr>
                        <td colspan="5" class="text-center text-muted"
                            style="padding:32px 14px;">No scheduled classes yet.</td>
                    </tr>
                </c:if>
            </tbody>
        </table>
    </div>

</div>
</body>
</html>

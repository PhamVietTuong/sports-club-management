<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Coach Dashboard — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<div class="app-shell">
    <!-- Inline Coach Sidebar -->
    <aside class="sidebar">
        <div class="sidebar-logo">
            <div class="sidebar-monogram">SC</div>
            <span class="sidebar-name">Sports Club</span>
        </div>
        <nav class="sidebar-nav">
            <a href="${pageContext.request.contextPath}/coach/dashboard" class="active">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                     stroke="currentColor" stroke-width="1.5">
                    <rect x="3" y="3" width="7" height="7" rx="1"/>
                    <rect x="14" y="3" width="7" height="7" rx="1"/>
                    <rect x="14" y="14" width="7" height="7" rx="1"/>
                    <rect x="3" y="14" width="7" height="7" rx="1"/>
                </svg>
                Dashboard
            </a>
            <a href="${pageContext.request.contextPath}/coach/classes">
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
            <span class="top-bar-title">Dashboard</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Coach)</span>
                <div class="user-avatar">
                    ${sessionScope.loggedInUser.username.substring(0,1).toUpperCase()}
                </div>
            </div>
        </header>

        <div class="page-body">

            <c:if test="${not empty sessionScope.flash}">
                <div class="alert alert-info"><c:out value="${sessionScope.flash}"/></div>
                <c:remove var="flash" scope="session"/>
            </c:if>

            <div class="grid-2">
                <!-- My Classes stat -->
                <div class="stat-card">
                    <div class="stat-icon blue">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                             stroke="currentColor" stroke-width="1.5">
                            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                            <line x1="16" y1="2" x2="16" y2="6"/>
                            <line x1="8" y1="2" x2="8" y2="6"/>
                            <line x1="3" y1="10" x2="21" y2="10"/>
                        </svg>
                    </div>
                    <div class="stat-number">${myClasses.size()}</div>
                    <div class="stat-label">My Classes</div>
                    <a href="${pageContext.request.contextPath}/coach/classes" class="stat-link">
                        View Classes →
                    </a>
                </div>

                <!-- Specialization -->
                <div class="stat-card">
                    <div class="stat-icon green">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none"
                             stroke="currentColor" stroke-width="1.5">
                            <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                        </svg>
                    </div>
                    <div class="stat-spec"><c:out value="${coach.specialization}"/></div>
                    <div class="stat-spec-sub">
                        <c:out value="${coach.experience}"/> year(s) experience
                    </div>
                    <div class="stat-label" style="margin-top:6px;">Specialization</div>
                </div>
            </div>

            <div class="section-title mt-4">This Week's Schedule</div>
            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>Class</th>
                            <th>Day</th>
                            <th>Time</th>
                            <th>Room</th>
                        </tr>
                    </thead>
                    <tbody>
                        <c:forEach var="s" items="${schedules}">
                        <tr>
                            <td><c:out value="${s.className}"/></td>
                            <td><c:out value="${s.dayOfWeek}"/></td>
                            <td>
                                <c:out value="${s.startTime}"/> &ndash; <c:out value="${s.endTime}"/>
                            </td>
                            <td><c:out value="${s.room}"/></td>
                        </tr>
                        </c:forEach>
                        <c:if test="${empty schedules}">
                            <tr>
                                <td colspan="4" class="text-center text-muted"
                                    style="padding:32px 14px;">No schedule assigned.</td>
                            </tr>
                        </c:if>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
</body>
</html>

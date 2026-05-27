<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Admin Dashboard — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<c:set var="activeNav" value="dashboard"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">
        <header class="top-bar">
            <span class="top-bar-title">Dashboard</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Admin)</span>
                <div class="user-avatar">
                    ${sessionScope.loggedInUser.username.substring(0,1).toUpperCase()}
                </div>
            </div>
        </header>

        <div class="page-body">
            <c:if test="${not empty error}">
                <div class="alert alert-warning"><c:out value="${error}"/></div>
            </c:if>

            <div class="grid-3">
                <!-- Total Members -->
                <div class="stat-card">
                    <div class="stat-icon blue">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                            <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                            <circle cx="9" cy="7" r="4"/>
                            <path d="M23 21v-2a4 4 0 0 0-3-3.87"/>
                            <path d="M16 3.13a4 4 0 0 1 0 7.75"/>
                        </svg>
                    </div>
                    <div class="stat-number"><c:out value="${totalMembers}"/></div>
                    <div class="stat-label">Total Members</div>
                    <a href="${pageContext.request.contextPath}/admin/members" class="stat-link">Manage →</a>
                </div>

                <!-- Total Coaches -->
                <div class="stat-card">
                    <div class="stat-icon green">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                            <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                        </svg>
                    </div>
                    <div class="stat-number"><c:out value="${totalCoaches}"/></div>
                    <div class="stat-label">Total Coaches</div>
                    <a href="${pageContext.request.contextPath}/admin/coaches" class="stat-link">Manage →</a>
                </div>

                <!-- Active Classes -->
                <div class="stat-card">
                    <div class="stat-icon orange">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                            <line x1="16" y1="2" x2="16" y2="6"/>
                            <line x1="8" y1="2" x2="8" y2="6"/>
                            <line x1="3" y1="10" x2="21" y2="10"/>
                        </svg>
                    </div>
                    <div class="stat-number"><c:out value="${totalClasses}"/></div>
                    <div class="stat-label">Active Classes</div>
                    <a href="${pageContext.request.contextPath}/admin/classes" class="stat-link">Manage →</a>
                </div>
            </div>

            <div class="grid-2 mt-4">
                <div class="card">
                    <div class="card-title">Quick Actions</div>
                    <div style="display:flex;flex-direction:column;gap:10px;">
                        <a href="${pageContext.request.contextPath}/admin/schedules"
                           class="btn btn-ghost">
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                <circle cx="12" cy="12" r="10"/>
                                <polyline points="12 6 12 12 16 14"/>
                            </svg>
                            Manage Schedules
                        </a>
                        <a href="${pageContext.request.contextPath}/admin/packages"
                           class="btn btn-ghost">
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                <line x1="16.5" y1="9.4" x2="7.5" y2="4.21"/>
                                <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/>
                                <polyline points="3.27 6.96 12 12.01 20.73 6.96"/>
                                <line x1="12" y1="22.08" x2="12" y2="12"/>
                            </svg>
                            Manage Packages
                        </a>
                    </div>
                </div>

                <div class="card">
                    <div class="card-title">Overview</div>
                    <p style="font-size:13px;color:var(--muted);line-height:1.8;">
                        Manage your sports club members, coaches, and training classes from this dashboard.
                        Use the sidebar to navigate between sections.
                    </p>
                    <p style="font-size:12px;color:var(--muted);margin-top:10px;">
                        Logged in as <strong style="color:var(--text);">
                            <c:out value="${sessionScope.loggedInUser.username}"/>
                        </strong>
                    </p>
                </div>
            </div>
        </div>
    </div>
</div>
</body>
</html>

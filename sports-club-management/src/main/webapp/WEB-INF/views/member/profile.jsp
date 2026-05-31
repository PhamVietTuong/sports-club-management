<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>My Profile — Sports Club</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>

<nav class="member-nav">
    <a href="${pageContext.request.contextPath}/member/dashboard" class="member-nav-logo">
        <div class="member-nav-logo-badge">SC</div>
        Sports Club
    </a>
    <div class="member-nav-links">
        <a href="${pageContext.request.contextPath}/member/dashboard">Dashboard</a>
        <a href="${pageContext.request.contextPath}/member/classes">Classes</a>
        <a href="${pageContext.request.contextPath}/member/profile" class="active">My Profile</a>
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
                Logout
            </button>
        </form>
    </div>
</nav>

<div class="page-body">

    <c:if test="${not empty sessionScope.flash}">
        <div class="alert alert-success"><c:out value="${sessionScope.flash}"/></div>
        <c:remove var="flash" scope="session"/>
    </c:if>

    <div class="page-header mb-20px">
        <div class="page-title">My Profile</div>
    </div>

    <div class="grid-2">
        <!-- Left: Identity Card -->
        <div class="card">
            <div class="profile-avatar">
                <c:out value="${not empty member.fullName ? member.fullName.substring(0,1).toUpperCase() : '?'}"/>
            </div>
            <h4 class="text-center mb-4px">
                <c:out value="${member.fullName}"/>
            </h4>
            <p class="text-center mb-14px">
                <span class="badge badge-primary">MEMBER</span>
            </p>

            <hr class="divider my-14px">

            <div class="info-row">
                <span class="info-row-label">Status</span>
                <span class="info-row-value">
                    <c:choose>
                        <c:when test="${member.status eq 'ACTIVE'}">
                            <span class="badge badge-success"><c:out value="${member.status}"/></span>
                        </c:when>
                        <c:when test="${member.status eq 'SUSPENDED'}">
                            <span class="badge badge-danger"><c:out value="${member.status}"/></span>
                        </c:when>
                        <c:otherwise>
                            <span class="badge badge-warning"><c:out value="${member.status}"/></span>
                        </c:otherwise>
                    </c:choose>
                </span>
            </div>
            <div class="info-row">
                <span class="info-row-label">Join Date</span>
                <span class="info-row-value"><c:out value="${member.joinDate}"/></span>
            </div>
            <div class="info-row">
                <span class="info-row-label">Expiry</span>
                <span class="info-row-value"><c:out value="${member.expiryDate}"/></span>
            </div>
            <div class="info-row">
                <span class="info-row-label">Gender</span>
                <span class="info-row-value"><c:out value="${member.gender}"/></span>
            </div>
            <div class="info-row">
                <span class="info-row-label">Email</span>
                <span class="info-row-value small"><c:out value="${member.email}"/></span>
            </div>
        </div>

        <!-- Right: Edit Form -->
        <div class="card">
            <div class="section-header">Personal Information</div>

            <form method="post" action="${pageContext.request.contextPath}/member/profile">
                <input type="hidden" name="_csrf" value="${csrfToken}">

                <div class="form-grid-2">
                    <div class="form-group">
                        <label class="form-label">Full Name</label>
                        <input type="text" name="fullName" class="form-control"
                               value="<c:out value='${member.fullName}'/>" required>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Phone</label>
                        <input type="text" name="phone" class="form-control"
                               value="<c:out value='${member.phone}'/>">
                    </div>
                </div>

                <div class="form-group">
                    <label class="form-label">Address</label>
                    <input type="text" name="address" class="form-control"
                           value="<c:out value='${member.address}'/>"
                           placeholder="Your address">
                </div>

                <hr class="divider">

                <p class="fw-bold fs-13 mb-4px">Change Password</p>
                <p class="text-muted small mb-12px">
                    Leave blank to keep current password
                </p>

                <div class="form-group">
                    <label class="form-label">New Password</label>
                    <input type="password" name="newPassword" class="form-control"
                           minlength="8" placeholder="Minimum 8 characters">
                </div>

                <button type="submit" class="btn btn-primary w-100 mt-8px">
                    Save Changes
                </button>
            </form>
        </div>
    </div>

</div>
</body>
</html>

<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Member Dashboard — Sports Club</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <div class="container">
        <a class="navbar-brand fw-bold" href="${pageContext.request.contextPath}/member/dashboard">
            Sports Club
        </a>
        <div class="collapse navbar-collapse">
            <ul class="navbar-nav me-auto">
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/member/classes">Classes</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/member/profile">My Profile</a>
                </li>
            </ul>
            <ul class="navbar-nav">
                <li class="nav-item">
                    <span class="nav-link text-light">
                        <c:out value="${member.fullName}"/>
                    </span>
                </li>
                <li class="nav-item">
                    <form method="post" action="${pageContext.request.contextPath}/logout" class="d-inline">
                        <input type="hidden" name="_csrf" value="${csrfToken}">
                        <button type="submit" class="btn btn-sm btn-outline-light ms-2">Logout</button>
                    </form>
                </li>
            </ul>
        </div>
    </div>
</nav>

<div class="container py-4">
    <h2>Welcome, <c:out value="${member.fullName}"/>!</h2>

    <c:if test="${not empty sessionScope.flash}">
        <div class="alert alert-info alert-dismissible fade show">
            <c:out value="${sessionScope.flash}"/>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
        <c:remove var="flash" scope="session"/>
    </c:if>

    <div class="row g-4 mt-2">
        <div class="col-md-4">
            <div class="card bg-light">
                <div class="card-body">
                    <h5>Membership Status</h5>
                    <p><span class="badge bg-success fs-6"><c:out value="${member.status}"/></span></p>
                    <p class="mb-0">Expires: <strong><c:out value="${member.expiryDate != null ? member.expiryDate : 'N/A'}"/></strong></p>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card bg-light">
                <div class="card-body">
                    <h5>My Enrollments</h5>
                    <p class="display-6">${enrollments.size()}</p>
                    <a href="${pageContext.request.contextPath}/member/classes" class="btn btn-primary btn-sm">Browse Classes</a>
                </div>
            </div>
        </div>
    </div>

    <h4 class="mt-4">My Weekly Schedule</h4>
    <div class="table-responsive">
        <table class="table table-bordered align-middle">
            <thead class="table-primary">
                <tr><th>Class</th><th>Day</th><th>Start</th><th>End</th><th>Room</th></tr>
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
                    <tr><td colspan="5" class="text-center text-muted">You have no scheduled classes yet.</td></tr>
                </c:if>
            </tbody>
        </table>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

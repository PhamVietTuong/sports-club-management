<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Coach Dashboard — Sports Club</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<nav class="navbar navbar-expand-lg navbar-dark bg-success">
    <div class="container">
        <a class="navbar-brand fw-bold" href="${pageContext.request.contextPath}/coach/dashboard">
            Sports Club — Coach
        </a>
        <div class="collapse navbar-collapse">
            <ul class="navbar-nav me-auto">
                <li class="nav-item">
                    <a class="nav-link" href="${pageContext.request.contextPath}/coach/classes">My Classes</a>
                </li>
            </ul>
            <ul class="navbar-nav">
                <li class="nav-item">
                    <span class="nav-link text-light">
                        <c:out value="${sessionScope.loggedInUser.username}"/> (Coach)
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
    <h2>Welcome, <c:out value="${coach.fullName}"/></h2>
    <p class="text-muted"><c:out value="${coach.specialization}"/> &bull;
        <c:out value="${coach.experience}"/> year(s) experience</p>

    <div class="row g-4 mt-2">
        <div class="col-md-4">
            <div class="card text-white bg-success">
                <div class="card-body text-center">
                    <h2>${myClasses.size()}</h2>
                    <p>My Classes</p>
                    <a href="${pageContext.request.contextPath}/coach/classes" class="btn btn-light btn-sm">View</a>
                </div>
            </div>
        </div>
    </div>

    <h4 class="mt-4">My Schedule</h4>
    <div class="table-responsive">
        <table class="table table-bordered align-middle">
            <thead class="table-success">
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
                    <tr><td colspan="5" class="text-center text-muted">No schedule assigned.</td></tr>
                </c:if>
            </tbody>
        </table>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Browse Classes — Sports Club</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<nav class="navbar navbar-dark bg-primary">
    <div class="container">
        <a class="navbar-brand" href="${pageContext.request.contextPath}/member/dashboard">Sports Club</a>
        <a class="nav-link text-white" href="${pageContext.request.contextPath}/member/profile">Profile</a>
        <form method="post" action="${pageContext.request.contextPath}/logout" class="d-inline">
            <input type="hidden" name="_csrf" value="${csrfToken}">
            <button class="btn btn-sm btn-outline-light">Logout</button>
        </form>
    </div>
</nav>
<div class="container py-4">
    <h2>Available Classes</h2>

    <c:if test="${not empty sessionScope.flash}">
        <div class="alert alert-info alert-dismissible fade show">
            <c:out value="${sessionScope.flash}"/>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
        <c:remove var="flash" scope="session"/>
    </c:if>

    <div class="row g-4">
        <c:forEach var="tc" items="${classes}">
        <div class="col-md-4">
            <div class="card h-100 shadow-sm">
                <div class="card-body">
                    <h5><c:out value="${tc.name}"/></h5>
                    <p class="text-muted"><c:out value="${tc.description}"/></p>
                    <ul class="list-unstyled small">
                        <li>Coach: <c:out value="${tc.coachName}"/></li>
                        <li>Level: <span class="badge bg-info"><c:out value="${tc.level}"/></span></li>
                        <li>Slots: <c:out value="${tc.availableSlots}"/> / <c:out value="${tc.capacity}"/></li>
                    </ul>
                </div>
                <div class="card-footer">
                    <c:choose>
                        <c:when test="${tc.availableSlots > 0}">
                            <form method="post" action="${pageContext.request.contextPath}/member/classes">
                                <input type="hidden" name="_csrf"   value="${csrfToken}">
                                <input type="hidden" name="action"  value="enroll">
                                <input type="hidden" name="classId" value="${tc.id}">
                                <button class="btn btn-primary btn-sm w-100">Enroll</button>
                            </form>
                        </c:when>
                        <c:otherwise>
                            <button class="btn btn-secondary btn-sm w-100" disabled>Class Full</button>
                        </c:otherwise>
                    </c:choose>
                </div>
            </div>
        </div>
        </c:forEach>
        <c:if test="${empty classes}">
            <p class="text-muted">No classes available at this time.</p>
        </c:if>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

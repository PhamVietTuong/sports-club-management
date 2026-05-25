<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>My Classes — Coach</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<nav class="navbar navbar-dark bg-success">
    <div class="container">
        <a class="navbar-brand" href="${pageContext.request.contextPath}/coach/dashboard">Coach Home</a>
        <form method="post" action="${pageContext.request.contextPath}/logout" class="d-inline">
            <input type="hidden" name="_csrf" value="${csrfToken}">
            <button class="btn btn-sm btn-outline-light">Logout</button>
        </form>
    </div>
</nav>
<div class="container py-4">
    <h2>My Classes</h2>
    <div class="row g-3 mb-4">
        <c:forEach var="tc" items="${myClasses}">
        <div class="col-md-4">
            <div class="card">
                <div class="card-body">
                    <h5><c:out value="${tc.name}"/></h5>
                    <p class="text-muted"><c:out value="${tc.level}"/></p>
                    <p>Enrolled: <c:out value="${tc.currentEnrolled}"/> / <c:out value="${tc.capacity}"/></p>
                    <a href="?classId=${tc.id}" class="btn btn-sm btn-success">View Members</a>
                </div>
            </div>
        </div>
        </c:forEach>
    </div>

    <c:if test="${not empty selectedClass}">
        <h4>Enrolled Members — <c:out value="${selectedClass.name}"/></h4>
        <table class="table table-bordered">
            <thead class="table-success">
                <tr><th>Member</th><th>Enroll Date</th><th>Status</th></tr>
            </thead>
            <tbody>
                <c:forEach var="e" items="${enrolledMembers}">
                <tr>
                    <td><c:out value="${e.memberName}"/></td>
                    <td><c:out value="${e.enrollDate}"/></td>
                    <td><c:out value="${e.status}"/></td>
                </tr>
                </c:forEach>
            </tbody>
        </table>
    </c:if>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

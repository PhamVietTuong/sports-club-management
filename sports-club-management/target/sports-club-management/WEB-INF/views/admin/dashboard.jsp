<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Admin Dashboard — Sports Club</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<%@ include file="navbar.jsp" %>
<div class="container py-4">
    <h2 class="mb-4">Admin Dashboard</h2>

    <c:if test="${not empty error}">
        <div class="alert alert-warning"><c:out value="${error}"/></div>
    </c:if>

    <div class="row g-4">
        <div class="col-md-4">
            <div class="card text-white bg-primary">
                <div class="card-body text-center">
                    <h1 class="display-4"><c:out value="${totalMembers}"/></h1>
                    <p class="card-text fs-5">Total Members</p>
                    <a href="${pageContext.request.contextPath}/admin/members"
                       class="btn btn-light btn-sm">Manage</a>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card text-white bg-success">
                <div class="card-body text-center">
                    <h1 class="display-4"><c:out value="${totalCoaches}"/></h1>
                    <p class="card-text fs-5">Total Coaches</p>
                    <a href="${pageContext.request.contextPath}/admin/coaches"
                       class="btn btn-light btn-sm">Manage</a>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card text-white bg-info">
                <div class="card-body text-center">
                    <h1 class="display-4"><c:out value="${totalClasses}"/></h1>
                    <p class="card-text fs-5">Active Classes</p>
                    <a href="${pageContext.request.contextPath}/admin/classes"
                       class="btn btn-light btn-sm">Manage</a>
                </div>
            </div>
        </div>
    </div>

    <div class="row mt-4 g-3">
        <div class="col-md-3">
            <a href="${pageContext.request.contextPath}/admin/schedules"
               class="btn btn-outline-secondary w-100">Schedules</a>
        </div>
        <div class="col-md-3">
            <a href="${pageContext.request.contextPath}/admin/packages"
               class="btn btn-outline-secondary w-100">Packages</a>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

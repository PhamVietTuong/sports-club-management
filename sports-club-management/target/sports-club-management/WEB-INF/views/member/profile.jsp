<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>My Profile — Sports Club</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<nav class="navbar navbar-dark bg-primary">
    <div class="container">
        <a class="navbar-brand" href="${pageContext.request.contextPath}/member/dashboard">Sports Club</a>
        <a class="nav-link text-white" href="${pageContext.request.contextPath}/member/classes">Classes</a>
        <form method="post" action="${pageContext.request.contextPath}/logout" class="d-inline">
            <input type="hidden" name="_csrf" value="${csrfToken}">
            <button class="btn btn-sm btn-outline-light">Logout</button>
        </form>
    </div>
</nav>
<div class="container py-4">
    <h2>My Profile</h2>

    <c:if test="${not empty sessionScope.flash}">
        <div class="alert alert-success alert-dismissible fade show">
            <c:out value="${sessionScope.flash}"/>
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
        <c:remove var="flash" scope="session"/>
    </c:if>

    <div class="row">
        <div class="col-md-6">
            <div class="card shadow-sm">
                <div class="card-body">
                    <form method="post" action="${pageContext.request.contextPath}/member/profile">
                        <input type="hidden" name="_csrf" value="${csrfToken}">

                        <div class="mb-3">
                            <label class="form-label">Full Name</label>
                            <input type="text" name="fullName" class="form-control"
                                   value="<c:out value='${member.fullName}'/>" required>
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Phone</label>
                            <input type="text" name="phone" class="form-control"
                                   value="<c:out value='${member.phone}'/>">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Address</label>
                            <input type="text" name="address" class="form-control"
                                   value="<c:out value='${member.address}'/>">
                        </div>
                        <hr>
                        <h6>Change Password (leave blank to keep current)</h6>
                        <div class="mb-3">
                            <label class="form-label">New Password</label>
                            <input type="password" name="newPassword" class="form-control" minlength="8">
                        </div>
                        <button type="submit" class="btn btn-primary">Save Changes</button>
                    </form>
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card shadow-sm">
                <div class="card-body">
                    <h5>Membership Info</h5>
                    <table class="table table-sm">
                        <tr><th>Status</th><td><c:out value="${member.status}"/></td></tr>
                        <tr><th>Join Date</th><td><c:out value="${member.joinDate}"/></td></tr>
                        <tr><th>Expiry</th><td><c:out value="${member.expiryDate}"/></td></tr>
                        <tr><th>Gender</th><td><c:out value="${member.gender}"/></td></tr>
                        <tr><th>Email</th><td><c:out value="${member.email}"/></td></tr>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

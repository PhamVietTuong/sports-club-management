<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<%@ taglib prefix="fmt" uri="http://java.sun.com/jsp/jstl/fmt" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Members — Admin</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<%@ include file="navbar.jsp" %>
<div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Member Management</h2>
        <%-- Status filter --%>
        <form class="d-flex gap-2" method="get">
            <select name="status" class="form-select form-select-sm">
                <option value="">All Statuses</option>
                <option value="ACTIVE"    ${statusFilter eq 'ACTIVE'    ? 'selected' : ''}>Active</option>
                <option value="INACTIVE"  ${statusFilter eq 'INACTIVE'  ? 'selected' : ''}>Inactive</option>
                <option value="SUSPENDED" ${statusFilter eq 'SUSPENDED' ? 'selected' : ''}>Suspended</option>
            </select>
            <button class="btn btn-sm btn-secondary">Filter</button>
        </form>
    </div>

    <c:if test="${not empty error}">
        <div class="alert alert-danger"><c:out value="${error}"/></div>
    </c:if>

    <div class="table-responsive">
        <table class="table table-striped table-hover align-middle">
            <thead class="table-dark">
                <tr>
                    <th>ID</th><th>Full Name</th><th>Username</th><th>Email</th>
                    <th>Gender</th><th>Join Date</th><th>Expiry</th><th>Status</th><th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <c:forEach var="m" items="${members}">
                <tr>
                    <td><c:out value="${m.id}"/></td>
                    <%-- XSS PREVENTION — c:out escapes HTML entities --%>
                    <td><c:out value="${m.fullName}"/></td>
                    <td><c:out value="${m.username}"/></td>
                    <td><c:out value="${m.email}"/></td>
                    <td><c:out value="${m.gender}"/></td>
                    <td><c:out value="${m.joinDate}"/></td>
                    <td><c:out value="${m.expiryDate}"/></td>
                    <td>
                        <c:choose>
                            <c:when test="${m.status eq 'ACTIVE'}">
                                <span class="badge bg-success">ACTIVE</span>
                            </c:when>
                            <c:when test="${m.status eq 'SUSPENDED'}">
                                <span class="badge bg-danger">SUSPENDED</span>
                            </c:when>
                            <c:otherwise>
                                <span class="badge bg-secondary">INACTIVE</span>
                            </c:otherwise>
                        </c:choose>
                    </td>
                    <td>
                        <form method="post" action="${pageContext.request.contextPath}/admin/members"
                              class="d-inline">
                            <input type="hidden" name="_csrf"     value="${csrfToken}">
                            <input type="hidden" name="action"    value="updateStatus">
                            <input type="hidden" name="memberId"  value="${m.id}">
                            <select name="status" class="form-select form-select-sm d-inline w-auto">
                                <option value="ACTIVE">Active</option>
                                <option value="INACTIVE">Inactive</option>
                                <option value="SUSPENDED">Suspended</option>
                            </select>
                            <button class="btn btn-sm btn-primary ms-1">Update</button>
                        </form>
                    </td>
                </tr>
                </c:forEach>
                <c:if test="${empty members}">
                    <tr><td colspan="9" class="text-center text-muted">No members found.</td></tr>
                </c:if>
            </tbody>
        </table>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

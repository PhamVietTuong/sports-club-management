<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Coaches — Admin</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<%@ include file="navbar.jsp" %>
<div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Coach Management</h2>
        <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addCoachModal">
            + Add Coach
        </button>
    </div>

    <c:if test="${not empty error}">
        <div class="alert alert-danger"><c:out value="${error}"/></div>
    </c:if>

    <div class="table-responsive">
        <table class="table table-striped align-middle">
            <thead class="table-dark">
                <tr>
                    <th>ID</th><th>Name</th><th>Email</th>
                    <th>Specialization</th><th>Experience</th><th>Salary</th>
                </tr>
            </thead>
            <tbody>
                <c:forEach var="c" items="${coaches}">
                <tr>
                    <td><c:out value="${c.id}"/></td>
                    <td><c:out value="${c.fullName}"/></td>
                    <td><c:out value="${c.email}"/></td>
                    <td><c:out value="${c.specialization}"/></td>
                    <td><c:out value="${c.experience}"/> yr(s)</td>
                    <td>$<c:out value="${c.salary}"/></td>
                </tr>
                </c:forEach>
                <c:if test="${empty coaches}">
                    <tr><td colspan="6" class="text-center text-muted">No coaches found.</td></tr>
                </c:if>
            </tbody>
        </table>
    </div>
</div>

<!-- Add Coach Modal -->
<div class="modal fade" id="addCoachModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Add New Coach</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/coaches">
                <input type="hidden" name="_csrf"   value="${csrfToken}">
                <input type="hidden" name="action"  value="add">
                <div class="modal-body row g-3">
                    <div class="col-md-6">
                        <label class="form-label">Username</label>
                        <input type="text" name="username" class="form-control" required maxlength="50">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Full Name</label>
                        <input type="text" name="fullName" class="form-control" required maxlength="100">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Email</label>
                        <input type="email" name="email" class="form-control" required>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Phone</label>
                        <input type="text" name="phone" class="form-control" maxlength="20">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Password</label>
                        <input type="password" name="password" class="form-control" required minlength="8">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Specialization</label>
                        <input type="text" name="specialization" class="form-control" maxlength="100">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Experience (years)</label>
                        <input type="number" name="experience" class="form-control" min="0" value="0">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Salary</label>
                        <input type="number" name="salary" class="form-control" step="0.01" min="0" value="0">
                    </div>
                    <div class="col-12">
                        <label class="form-label">Bio</label>
                        <textarea name="bio" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save Coach</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

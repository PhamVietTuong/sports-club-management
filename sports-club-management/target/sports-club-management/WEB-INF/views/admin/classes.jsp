<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Classes — Admin</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<%@ include file="navbar.jsp" %>
<div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Class Management</h2>
        <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addClassModal">
            + Add Class
        </button>
    </div>

    <c:if test="${not empty error}">
        <div class="alert alert-danger"><c:out value="${error}"/></div>
    </c:if>

    <div class="table-responsive">
        <table class="table table-striped align-middle">
            <thead class="table-dark">
                <tr>
                    <th>ID</th><th>Name</th><th>Coach</th><th>Level</th>
                    <th>Capacity</th><th>Enrolled</th><th>Status</th><th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <c:forEach var="tc" items="${classes}">
                <tr>
                    <td><c:out value="${tc.id}"/></td>
                    <td><c:out value="${tc.name}"/></td>
                    <td><c:out value="${tc.coachName}"/></td>
                    <td><span class="badge bg-secondary"><c:out value="${tc.level}"/></span></td>
                    <td><c:out value="${tc.capacity}"/></td>
                    <td><c:out value="${tc.currentEnrolled}"/></td>
                    <td>
                        <c:choose>
                            <c:when test="${tc.active}">
                                <span class="badge bg-success">Active</span>
                            </c:when>
                            <c:otherwise>
                                <span class="badge bg-secondary">Inactive</span>
                            </c:otherwise>
                        </c:choose>
                    </td>
                    <td>
                        <%-- PROTOTYPE — clone this class --%>
                        <form method="post" action="${pageContext.request.contextPath}/admin/classes"
                              class="d-inline">
                            <input type="hidden" name="_csrf"       value="${csrfToken}">
                            <input type="hidden" name="action"      value="clone">
                            <input type="hidden" name="templateId"  value="${tc.id}">
                            <button class="btn btn-sm btn-outline-info" title="Clone (Prototype)">Clone</button>
                        </form>
                    </td>
                </tr>
                </c:forEach>
                <c:if test="${empty classes}">
                    <tr><td colspan="8" class="text-center text-muted">No classes found.</td></tr>
                </c:if>
            </tbody>
        </table>
    </div>
</div>

<!-- Add Class Modal -->
<div class="modal fade" id="addClassModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Add New Class</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/classes">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body row g-3">
                    <div class="col-12">
                        <label class="form-label">Class Name</label>
                        <input type="text" name="name" class="form-control" required maxlength="100">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Coach</label>
                        <select name="coachId" class="form-select" required>
                            <c:forEach var="coach" items="${coaches}">
                                <option value="${coach.id}"><c:out value="${coach.fullName}"/></option>
                            </c:forEach>
                        </select>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Level</label>
                        <select name="level" class="form-select">
                            <option value="BEGINNER">Beginner</option>
                            <option value="INTERMEDIATE">Intermediate</option>
                            <option value="ADVANCED">Advanced</option>
                        </select>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Capacity</label>
                        <input type="number" name="capacity" class="form-control" min="1" value="20">
                    </div>
                    <div class="col-12">
                        <label class="form-label">Description</label>
                        <textarea name="description" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save Class</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

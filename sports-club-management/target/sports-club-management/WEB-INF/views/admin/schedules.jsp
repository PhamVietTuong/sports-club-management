<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Schedules — Admin</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<%@ include file="navbar.jsp" %>
<div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Schedule Management</h2>
        <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addScheduleModal">
            + Add Schedule
        </button>
    </div>

    <div class="table-responsive">
        <table class="table table-striped align-middle">
            <thead class="table-dark">
                <tr>
                    <th>ID</th><th>Class</th><th>Day</th>
                    <th>Start</th><th>End</th><th>Room</th><th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <c:forEach var="s" items="${schedules}">
                <tr>
                    <td><c:out value="${s.id}"/></td>
                    <td><c:out value="${s.className}"/></td>
                    <td><c:out value="${s.dayOfWeek}"/></td>
                    <td><c:out value="${s.startTime}"/></td>
                    <td><c:out value="${s.endTime}"/></td>
                    <td><c:out value="${s.room}"/></td>
                    <td class="d-flex gap-1">
                        <%-- PROTOTYPE — clone schedule for next week --%>
                        <form method="post" action="${pageContext.request.contextPath}/admin/schedules">
                            <input type="hidden" name="_csrf"    value="${csrfToken}">
                            <input type="hidden" name="action"   value="clone">
                            <input type="hidden" name="sourceId" value="${s.id}">
                            <button class="btn btn-sm btn-outline-info" title="Clone (Prototype)">Clone</button>
                        </form>
                        <form method="post" action="${pageContext.request.contextPath}/admin/schedules">
                            <input type="hidden" name="_csrf"  value="${csrfToken}">
                            <input type="hidden" name="action" value="delete">
                            <input type="hidden" name="id"     value="${s.id}">
                            <button class="btn btn-sm btn-outline-danger"
                                    onclick="return confirm('Delete this schedule?')">Delete</button>
                        </form>
                    </td>
                </tr>
                </c:forEach>
                <c:if test="${empty schedules}">
                    <tr><td colspan="7" class="text-center text-muted">No schedules found.</td></tr>
                </c:if>
            </tbody>
        </table>
    </div>
</div>

<!-- Add Schedule Modal -->
<div class="modal fade" id="addScheduleModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Add New Schedule</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/schedules">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body row g-3">
                    <div class="col-12">
                        <label class="form-label">Class</label>
                        <select name="classId" class="form-select" required>
                            <c:forEach var="tc" items="${classes}">
                                <option value="${tc.id}"><c:out value="${tc.name}"/></option>
                            </c:forEach>
                        </select>
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Day of Week</label>
                        <select name="dayOfWeek" class="form-select">
                            <option>MONDAY</option><option>TUESDAY</option>
                            <option>WEDNESDAY</option><option>THURSDAY</option>
                            <option>FRIDAY</option><option>SATURDAY</option><option>SUNDAY</option>
                        </select>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">Start Time</label>
                        <input type="time" name="startTime" class="form-control" required>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">End Time</label>
                        <input type="time" name="endTime" class="form-control" required>
                    </div>
                    <div class="col-12">
                        <label class="form-label">Room</label>
                        <input type="text" name="room" class="form-control" maxlength="50">
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save Schedule</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

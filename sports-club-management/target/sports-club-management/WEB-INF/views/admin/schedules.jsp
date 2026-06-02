<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Schedules — Admin</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<c:set var="activeNav" value="schedules"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">

        <header class="top-bar">
            <span class="top-bar-title">Schedules</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Admin)</span>
                <div class="user-avatar">
                    <c:out value="${not empty sessionScope.loggedInUser.username ? sessionScope.loggedInUser.username.substring(0,1).toUpperCase() : '?'}"/>
                </div>
            </div>
        </header>

        <div class="page-body">

            <div class="page-header">
                <div>
                    <div class="page-title">Schedules</div>
                    <div class="page-subtitle">Manage weekly class timetables</div>
                </div>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addScheduleModal">
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    Add Schedule
                </button>
            </div>

            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Class</th>
                            <th>Day</th>
                            <th>Start</th>
                            <th>End</th>
                            <th>Room</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <c:forEach var="s" items="${schedules}">
                        <tr>
                            <td class="text-muted small"><c:out value="${s.id}"/></td>
                            <td class="fw-bold"><c:out value="${s.className}"/></td>
                            <td>
                                <span class="day-chip day-${s.dayOfWeek}">
                                    <c:out value="${s.dayOfWeek}"/>
                                </span>
                            </td>
                            <td>
                                <span class="font-display-600">
                                    <c:out value="${s.startTime}"/>
                                </span>
                            </td>
                            <td>
                                <span class="font-display-600">
                                    <c:out value="${s.endTime}"/>
                                </span>
                            </td>
                            <td>
                                <span class="text-muted">
                                    <c:out value="${s.room}"/>
                                </span>
                            </td>
                            <td>
                                <div class="action-cell">
                                    <%-- PROTOTYPE — clone this schedule --%>
                                    <form method="post" action="${pageContext.request.contextPath}/admin/schedules" class="d-inline">
                                        <input type="hidden" name="_csrf"    value="${csrfToken}">
                                        <input type="hidden" name="action"   value="clone">
                                        <input type="hidden" name="sourceId" value="${s.id}">
                                        <button type="submit" class="btn btn-ghost btn-sm" title="Clone (Prototype)">
                                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                                <rect x="9" y="9" width="13" height="13" rx="2"/>
                                                <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
                                            </svg>
                                            Clone
                                        </button>
                                    </form>
                                    <form method="post" action="${pageContext.request.contextPath}/admin/schedules" class="d-inline"
                                          data-confirm="Delete this schedule?">
                                        <input type="hidden" name="_csrf"  value="${csrfToken}">
                                        <input type="hidden" name="action" value="delete">
                                        <input type="hidden" name="id"     value="${s.id}">
                                        <button type="submit" class="btn btn-danger btn-sm">
                                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                                <polyline points="3 6 5 6 21 6"/>
                                                <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
                                                <path d="M10 11v6"/><path d="M14 11v6"/>
                                                <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/>
                                            </svg>
                                            Delete
                                        </button>
                                    </form>
                                </div>
                            </td>
                        </tr>
                        </c:forEach>
                        <c:if test="${empty schedules}">
                            <tr>
                                <td colspan="7" class="text-center text-muted empty-pad-36">
                                    No schedules found. Add one to get started.
                                </td>
                            </tr>
                        </c:if>
                    </tbody>
                </table>
            </div>

        </div>
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
                <div class="modal-body">
                    <div class="form-group">
                        <label class="form-label">Class <span class="req">*</span></label>
                        <select name="classId" class="form-select" required>
                            <c:forEach var="tc" items="${classes}">
                                <option value="${tc.id}"><c:out value="${tc.name}"/></option>
                            </c:forEach>
                        </select>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Day of Week <span class="req">*</span></label>
                        <select name="dayOfWeek" class="form-select">
                            <option value="MONDAY">Monday</option>
                            <option value="TUESDAY">Tuesday</option>
                            <option value="WEDNESDAY">Wednesday</option>
                            <option value="THURSDAY">Thursday</option>
                            <option value="FRIDAY">Friday</option>
                            <option value="SATURDAY">Saturday</option>
                            <option value="SUNDAY">Sunday</option>
                        </select>
                    </div>
                    <div class="form-grid-2">
                        <div class="form-group">
                            <label class="form-label">Start Time <span class="req">*</span></label>
                            <input type="time" name="startTime" class="form-control" required>
                        </div>
                        <div class="form-group">
                            <label class="form-label">End Time <span class="req">*</span></label>
                            <input type="time" name="endTime" class="form-control" required>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Room</label>
                        <input type="text" name="room" class="form-control" maxlength="50" placeholder="e.g. Studio A">
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-ghost" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-primary">Save Schedule</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
<script src="${pageContext.request.contextPath}/assets/js/app.js"></script>
</body>
</html>

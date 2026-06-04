<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Lớp học — Quản trị</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css?v=2">
</head>
<body>
<c:set var="activeNav" value="classes"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">
        <header class="top-bar">
            <span class="top-bar-title">Lớp học</span>
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
                    <div class="page-title">Lớp học</div>
                    <div class="page-subtitle">Quản lý các lớp tập luyện và đăng ký</div>
                </div>
                <button class="btn btn-primary"
                        data-bs-toggle="modal" data-bs-target="#addClassModal">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none"
                         stroke="currentColor" stroke-width="1.5">
                        <line x1="12" y1="5" x2="12" y2="19"/>
                        <line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    Thêm lớp học
                </button>
            </div>

            <c:if test="${not empty error}">
                <div class="alert alert-danger"><c:out value="${error}"/></div>
            </c:if>

            <div class="table-wrap">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Tên</th>
                            <th>Huấn luyện viên</th>
                            <th>Cấp độ</th>
                            <th>Sức chứa</th>
                            <th>Đã đăng ký</th>
                            <th>Trạng thái</th>
                            <th>Thao tác</th>
                        </tr>
                    </thead>
                    <tbody>
                        <c:forEach var="tc" items="${classes}">
                        <tr>
                            <td><c:out value="${tc.id}"/></td>
                            <td><c:out value="${tc.name}"/></td>
                            <td><c:out value="${tc.coachName}"/></td>
                            <td>
                                <c:choose>
                                    <c:when test="${tc.level eq 'BEGINNER'}">
                                        <span class="badge badge-success">CƠ BẢN</span>
                                    </c:when>
                                    <c:when test="${tc.level eq 'INTERMEDIATE'}">
                                        <span class="badge badge-warning">TRUNG CẤP</span>
                                    </c:when>
                                    <c:when test="${tc.level eq 'ADVANCED'}">
                                        <span class="badge badge-danger">NÂNG CAO</span>
                                    </c:when>
                                    <c:otherwise>
                                        <span class="badge badge-muted"><c:out value="${tc.level}"/></span>
                                    </c:otherwise>
                                </c:choose>
                            </td>
                            <td><c:out value="${tc.capacity}"/></td>
                            <td>
                                <c:out value="${tc.currentEnrolled}"/>
                                <div class="progress-bar-wrap">
                                    <div class="progress-bar-fill ${tc.currentEnrolled >= tc.capacity ? 'full' : tc.currentEnrolled < tc.capacity * 0.5 ? 'low' : ''}"
                                         data-width="${tc.currentEnrolled * 100 / (tc.capacity > 0 ? tc.capacity : 1)}"></div>
                                </div>
                            </td>
                            <td>
                                <c:choose>
                                    <c:when test="${tc.active}">
                                        <span class="badge badge-success">Hoạt động</span>
                                    </c:when>
                                    <c:otherwise>
                                        <span class="badge badge-muted">Ngừng hoạt động</span>
                                    </c:otherwise>
                                </c:choose>
                            </td>
                            <td>
                                <%-- PROTOTYPE — clone this class --%>
                                <form method="post"
                                      action="${pageContext.request.contextPath}/admin/classes"
                                      class="d-inline">
                                    <input type="hidden" name="_csrf"      value="${csrfToken}">
                                    <input type="hidden" name="action"     value="clone">
                                    <input type="hidden" name="templateId" value="${tc.id}">
                                    <button type="submit" class="btn btn-ghost btn-sm"
                                            title="Nhân bản (Prototype)">
                                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none"
                                             stroke="currentColor" stroke-width="1.5">
                                            <rect x="9" y="9" width="13" height="13" rx="2" ry="2"/>
                                            <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
                                        </svg>
                                        Nhân bản
                                    </button>
                                </form>
                            </td>
                        </tr>
                        </c:forEach>
                        <c:if test="${empty classes}">
                            <tr>
                                <td colspan="8" class="text-center text-muted empty-pad-40">Không tìm thấy lớp học nào.</td>
                            </tr>
                        </c:if>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<!-- Add Class Modal -->
<div class="modal fade" id="addClassModal" tabindex="-1" aria-labelledby="addClassModalLabel" aria-hidden="true">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="addClassModalLabel">Thêm lớp học mới</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/classes">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body form-stack">
                    <div class="form-group mb-0">
                        <label class="form-label">Tên lớp học</label>
                        <input type="text" name="name" class="form-control"
                               required maxlength="100" placeholder="vd: Yoga buổi sáng">
                    </div>
                    <div class="form-grid-2">
                        <div class="form-group mb-0">
                            <label class="form-label">Huấn luyện viên</label>
                            <select name="coachId" class="form-select" required>
                                <option value="">Chọn huấn luyện viên...</option>
                                <c:forEach var="coach" items="${coaches}">
                                    <option value="${coach.id}"><c:out value="${coach.fullName}"/></option>
                                </c:forEach>
                            </select>
                        </div>
                        <div class="form-group mb-0">
                            <label class="form-label">Cấp độ</label>
                            <select name="level" class="form-select">
                                <option value="BEGINNER">Cơ bản</option>
                                <option value="INTERMEDIATE">Trung cấp</option>
                                <option value="ADVANCED">Nâng cao</option>
                            </select>
                        </div>
                    </div>
                    <div class="form-group mb-0">
                        <label class="form-label">Sức chứa</label>
                        <input type="number" name="capacity" class="form-control" min="1" value="20">
                    </div>
                    <div class="form-group mb-0">
                        <label class="form-label">Mô tả</label>
                        <textarea name="description" class="form-control" rows="3"
                                  placeholder="Mô tả ngắn gọn về lớp học..."></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Lưu lớp học</button>
                    <button type="button" class="btn btn-ghost" data-bs-dismiss="modal">Hủy</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
<script src="${pageContext.request.contextPath}/assets/js/app.js"></script>
</body>
</html>

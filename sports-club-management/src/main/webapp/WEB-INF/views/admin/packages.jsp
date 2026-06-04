<%@ page contentType="text/html;charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Gói tập — Quản trị</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
</head>
<body>
<c:set var="activeNav" value="packages"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">

        <header class="top-bar">
            <span class="top-bar-title">Gói tập</span>
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
                    <div class="page-title">Gói tập luyện</div>
                    <div class="page-subtitle">Quản lý các gói hội viên và giá</div>
                </div>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addPackageModal">
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    Thêm gói tập
                </button>
            </div>

            <div class="grid-3">
                <c:forEach var="pkg" items="${packages}">
                <div class="pkg-card ${pkg.active ? '' : 'inactive'}">
                    <div class="pkg-card-accent"></div>
                    <div class="pkg-card-body">
                        <div class="flex-between-start">
                            <h5 class="fs-16 mb-4px">
                                <c:out value="${pkg.name}"/>
                            </h5>
                            <c:choose>
                                <c:when test="${pkg.active}">
                                    <span class="badge badge-success">Hoạt động</span>
                                </c:when>
                                <c:otherwise>
                                    <span class="badge badge-muted">Ngừng hoạt động</span>
                                </c:otherwise>
                            </c:choose>
                        </div>
                        <p class="fs-12-5 text-muted mb-12px min-h-36">
                            <c:out value="${pkg.description}"/>
                        </p>

                        <div class="pkg-price">$<c:out value="${pkg.price}"/></div>
                        <p class="pkg-price-sub">cho <c:out value="${pkg.durationMonths}"/> tháng</p>

                        <div class="pkg-stat-row">
                            <span class="pkg-stat-label">Thời hạn</span>
                            <span class="pkg-stat-value"><c:out value="${pkg.durationMonths}"/> tháng</span>
                        </div>
                        <div class="pkg-stat-row">
                            <span class="pkg-stat-label">Số lớp tối đa</span>
                            <span class="pkg-stat-value">
                                <c:choose>
                                    <c:when test="${pkg.maxClasses >= 99}">Không giới hạn</c:when>
                                    <c:otherwise><c:out value="${pkg.maxClasses}"/></c:otherwise>
                                </c:choose>
                            </span>
                        </div>
                    </div>
                    <div class="pkg-card-footer">
                        <%-- PROTOTYPE — clone this package at 120% price --%>
                        <form method="post" action="${pageContext.request.contextPath}/admin/packages" class="flex-1">
                            <input type="hidden" name="_csrf"      value="${csrfToken}">
                            <input type="hidden" name="action"     value="clone">
                            <input type="hidden" name="templateId" value="${pkg.id}">
                            <button type="submit" class="btn btn-ghost btn-sm w-100" title="Nhân bản với giá 120% (Prototype)">
                                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                    <rect x="9" y="9" width="13" height="13" rx="2"/>
                                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
                                </svg>
                                Nhân bản (×1.2)
                            </button>
                        </form>
                    </div>
                </div>
                </c:forEach>
                <c:if test="${empty packages}">
                    <p class="text-muted col-span-full pad-24-0">
                        Không tìm thấy gói tập nào. Thêm mới để bắt đầu.
                    </p>
                </c:if>
            </div>

        </div>
    </div>
</div>

<!-- Add Package Modal -->
<div class="modal fade" id="addPackageModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Thêm gói tập luyện</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/packages">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body">
                    <div class="form-group">
                        <label class="form-label">Tên gói <span class="req">*</span></label>
                        <input type="text" name="name" class="form-control" required maxlength="100"
                               placeholder="vd: Premium, Tiêu chuẩn, Cơ bản">
                    </div>
                    <div class="form-grid-2">
                        <div class="form-group">
                            <label class="form-label">Thời hạn (tháng) <span class="req">*</span></label>
                            <input type="number" name="durationMonths" class="form-control" min="1" value="1">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Giá ($) <span class="req">*</span></label>
                            <input type="number" name="price" class="form-control" step="0.01" min="0" placeholder="0.00">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Số lớp tối đa</label>
                            <input type="number" name="maxClasses" class="form-control" min="0" value="0">
                            <p class="form-hint">Dùng 99 để truy cập không giới hạn.</p>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Mô tả</label>
                        <textarea name="description" class="form-control" rows="3"
                                  placeholder="Mô tả ngắn gọn những gì bao gồm..."></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-ghost" data-bs-dismiss="modal">Hủy</button>
                    <button type="submit" class="btn btn-primary">Lưu gói tập</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

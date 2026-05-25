<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Packages — Admin</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css">
</head>
<body>
<%@ include file="navbar.jsp" %>
<div class="container py-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Training Package Management</h2>
        <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addPackageModal">
            + Add Package
        </button>
    </div>

    <div class="row g-4">
        <c:forEach var="pkg" items="${packages}">
        <div class="col-md-4">
            <div class="card h-100 shadow-sm ${pkg.active ? '' : 'border-secondary opacity-75'}">
                <div class="card-body">
                    <h5 class="card-title"><c:out value="${pkg.name}"/></h5>
                    <p class="text-muted"><c:out value="${pkg.description}"/></p>
                    <ul class="list-unstyled">
                        <li><strong>Duration:</strong> <c:out value="${pkg.durationMonths}"/> month(s)</li>
                        <li><strong>Price:</strong> $<c:out value="${pkg.price}"/></li>
                        <li><strong>Max Classes:</strong> <c:out value="${pkg.maxClasses}"/></li>
                        <li><strong>Status:</strong>
                            <c:choose>
                                <c:when test="${pkg.active}">
                                    <span class="badge bg-success">Active</span>
                                </c:when>
                                <c:otherwise>
                                    <span class="badge bg-secondary">Inactive</span>
                                </c:otherwise>
                            </c:choose>
                        </li>
                    </ul>
                </div>
                <div class="card-footer d-flex gap-2">
                    <%-- PROTOTYPE — clone package template --%>
                    <form method="post" action="${pageContext.request.contextPath}/admin/packages">
                        <input type="hidden" name="_csrf"       value="${csrfToken}">
                        <input type="hidden" name="action"      value="clone">
                        <input type="hidden" name="templateId"  value="${pkg.id}">
                        <button class="btn btn-sm btn-outline-info" title="Clone (Prototype)">Clone</button>
                    </form>
                </div>
            </div>
        </div>
        </c:forEach>
        <c:if test="${empty packages}">
            <div class="col"><p class="text-muted">No packages found.</p></div>
        </c:if>
    </div>
</div>

<!-- Add Package Modal -->
<div class="modal fade" id="addPackageModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Add Training Package</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/packages">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body row g-3">
                    <div class="col-12">
                        <label class="form-label">Package Name</label>
                        <input type="text" name="name" class="form-control" required maxlength="100">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Duration (months)</label>
                        <input type="number" name="durationMonths" class="form-control" min="1" value="1">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Price ($)</label>
                        <input type="number" name="price" class="form-control" step="0.01" min="0">
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Max Classes</label>
                        <input type="number" name="maxClasses" class="form-control" min="0" value="0">
                    </div>
                    <div class="col-12">
                        <label class="form-label">Description</label>
                        <textarea name="description" class="form-control" rows="3"></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save Package</button>
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

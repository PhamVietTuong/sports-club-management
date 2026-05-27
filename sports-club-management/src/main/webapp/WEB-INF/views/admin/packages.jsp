<%@ page contentType="text/html;charset=UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Packages — Admin</title>
    <link rel="stylesheet" href="${pageContext.request.contextPath}/assets/css/style.css">
    <style>
        .pkg-card {
            background: var(--surface);
            border: 1px solid var(--border);
            border-radius: 10px;
            display: flex;
            flex-direction: column;
            transition: transform 0.15s, box-shadow 0.15s;
            overflow: hidden;
        }
        .pkg-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 24px rgba(0,0,0,0.3);
        }
        .pkg-card.inactive {
            opacity: 0.55;
        }
        .pkg-card-accent {
            height: 3px;
            background: var(--primary);
        }
        .pkg-card.inactive .pkg-card-accent {
            background: var(--muted);
        }
        .pkg-card-body {
            padding: 20px;
            flex: 1;
        }
        .pkg-price {
            font-family: 'Syne', sans-serif;
            font-weight: 800;
            font-size: 32px;
            color: var(--text);
            line-height: 1;
            margin: 12px 0 4px;
        }
        .pkg-price-sub {
            font-size: 12px;
            color: var(--muted);
            margin-bottom: 16px;
        }
        .pkg-stat-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 0;
            border-bottom: 1px solid var(--border);
            font-size: 13px;
        }
        .pkg-stat-row:last-of-type {
            border-bottom: none;
        }
        .pkg-stat-label { color: var(--muted); }
        .pkg-stat-value { color: var(--text); font-weight: 600; }
        .pkg-card-footer {
            padding: 14px 20px;
            border-top: 1px solid var(--border);
            display: flex;
            gap: 8px;
        }
    </style>
</head>
<body>
<c:set var="activeNav" value="packages"/>
<div class="app-shell">
    <%@ include file="navbar.jsp" %>
    <div class="main-content">

        <header class="top-bar">
            <span class="top-bar-title">Packages</span>
            <div class="top-bar-user">
                <span><c:out value="${sessionScope.loggedInUser.username}"/> (Admin)</span>
                <div class="user-avatar">
                    ${sessionScope.loggedInUser.username.substring(0,1).toUpperCase()}
                </div>
            </div>
        </header>

        <div class="page-body">

            <div class="page-header">
                <div>
                    <div class="page-title">Training Packages</div>
                    <div class="page-subtitle">Manage membership plans and pricing</div>
                </div>
                <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addPackageModal">
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    Add Package
                </button>
            </div>

            <div class="grid-3">
                <c:forEach var="pkg" items="${packages}">
                <div class="pkg-card ${pkg.active ? '' : 'inactive'}">
                    <div class="pkg-card-accent"></div>
                    <div class="pkg-card-body">
                        <div style="display:flex;justify-content:space-between;align-items:flex-start;">
                            <h5 style="font-size:16px;margin-bottom:4px;">
                                <c:out value="${pkg.name}"/>
                            </h5>
                            <c:choose>
                                <c:when test="${pkg.active}">
                                    <span class="badge badge-success">Active</span>
                                </c:when>
                                <c:otherwise>
                                    <span class="badge badge-muted">Inactive</span>
                                </c:otherwise>
                            </c:choose>
                        </div>
                        <p style="font-size:12.5px;color:var(--muted);margin-bottom:12px;min-height:36px;">
                            <c:out value="${pkg.description}"/>
                        </p>

                        <div class="pkg-price">$<c:out value="${pkg.price}"/></div>
                        <p class="pkg-price-sub">per <c:out value="${pkg.durationMonths}"/> month(s)</p>

                        <div class="pkg-stat-row">
                            <span class="pkg-stat-label">Duration</span>
                            <span class="pkg-stat-value"><c:out value="${pkg.durationMonths}"/> month(s)</span>
                        </div>
                        <div class="pkg-stat-row">
                            <span class="pkg-stat-label">Max Classes</span>
                            <span class="pkg-stat-value">
                                <c:choose>
                                    <c:when test="${pkg.maxClasses >= 99}">Unlimited</c:when>
                                    <c:otherwise><c:out value="${pkg.maxClasses}"/></c:otherwise>
                                </c:choose>
                            </span>
                        </div>
                    </div>
                    <div class="pkg-card-footer">
                        <%-- PROTOTYPE — clone this package at 120% price --%>
                        <form method="post" action="${pageContext.request.contextPath}/admin/packages" style="flex:1;">
                            <input type="hidden" name="_csrf"      value="${csrfToken}">
                            <input type="hidden" name="action"     value="clone">
                            <input type="hidden" name="templateId" value="${pkg.id}">
                            <button type="submit" class="btn btn-ghost btn-sm w-100" title="Clone at 120% price (Prototype)">
                                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                                    <rect x="9" y="9" width="13" height="13" rx="2"/>
                                    <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/>
                                </svg>
                                Clone (×1.2)
                            </button>
                        </form>
                    </div>
                </div>
                </c:forEach>
                <c:if test="${empty packages}">
                    <p class="text-muted" style="grid-column:1/-1;padding:24px 0;">
                        No packages found. Add one to get started.
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
                <h5 class="modal-title">Add Training Package</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form method="post" action="${pageContext.request.contextPath}/admin/packages">
                <input type="hidden" name="_csrf"  value="${csrfToken}">
                <input type="hidden" name="action" value="add">
                <div class="modal-body">
                    <div class="form-group">
                        <label class="form-label">Package Name <span class="req">*</span></label>
                        <input type="text" name="name" class="form-control" required maxlength="100"
                               placeholder="e.g. Premium, Standard, Basic">
                    </div>
                    <div class="form-grid-2">
                        <div class="form-group">
                            <label class="form-label">Duration (months) <span class="req">*</span></label>
                            <input type="number" name="durationMonths" class="form-control" min="1" value="1">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Price ($) <span class="req">*</span></label>
                            <input type="number" name="price" class="form-control" step="0.01" min="0" placeholder="0.00">
                        </div>
                        <div class="form-group">
                            <label class="form-label">Max Classes</label>
                            <input type="number" name="maxClasses" class="form-control" min="0" value="0">
                            <p class="form-hint">Use 99 for unlimited access.</p>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Description</label>
                        <textarea name="description" class="form-control" rows="3"
                                  placeholder="Briefly describe what's included..."></textarea>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-ghost" data-bs-dismiss="modal">Cancel</button>
                    <button type="submit" class="btn btn-primary">Save Package</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

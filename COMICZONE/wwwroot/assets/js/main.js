/**
 * main.js
 * Template core logic (Sidebar, Mobile Menu, Tooltips)
 * Optimized version: Removed bundled Bootstrap, Popper, and ApexCharts.
 */

document.addEventListener("DOMContentLoaded", () => {
    // 1. Sidebar and Topbar Elements
    const sidebar = document.getElementById("sidebar");
    const content = document.getElementById("content");
    const topbar = document.getElementById("topbar");
    const toggleBtn = document.getElementById("toggleBtn");
    const mobileBtn = document.getElementById("mobileBtn");
    const overlay = document.getElementById("overlay");

    // 2. Sidebar Toggle (Desktop)
    if (toggleBtn) {
        toggleBtn.addEventListener("click", () => {
            if (sidebar) {
                sidebar.classList.toggle("collapsed");

                // Đóng tất cả các menu con khi thu nhỏ sidebar
                if (sidebar.classList.contains("collapsed")) {
                    const openMenus = sidebar.querySelectorAll(".collapse.show");
                    openMenus.forEach(menu => {
                        // Thử dùng API của Bootstrap
                        const collapseInstance = bootstrap.Collapse.getInstance(menu);
                        if (collapseInstance) {
                            collapseInstance.hide();
                        } else {
                            // Fallback nếu chưa khởi tạo instance
                            menu.classList.remove("show");
                        }
                    });
                }
            }
            if (content) content.classList.toggle("full");
            if (topbar) topbar.classList.toggle("full");
        });
    }

    // 3. Mobile View Sidebar Toggle
    if (mobileBtn) {
        mobileBtn.addEventListener("click", () => {
            if (sidebar) {
                sidebar.classList.remove("collapsed"); // Đảm bảo hiện đầy đủ chữ trên mobile
                sidebar.classList.add("mobile-show");
            }
            if (overlay) overlay.classList.add("show");
        });
    }

    // 4. Overlay Click (Close Mobile Sidebar)
    if (overlay) {
        overlay.addEventListener("click", () => {
            if (sidebar) sidebar.classList.remove("mobile-show");
            if (overlay) overlay.classList.remove("show");
        });
    }

    // 5. Active Link handling (Simple template logic)
    const currentPath = window.location.pathname.split("/").pop() || "index.html";
    const navLinks = document.querySelectorAll(".sidebar .nav-link");
    if (navLinks.length > 0) {
        navLinks.forEach(link => {
            if (link.getAttribute("href") === currentPath) {
                link.classList.add("active");
            }
        });
    }

    // 6. Initialize Bootstrap Tooltips/Popovers (Standard BS5 boilerplate)
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // 7. Auto-close mobile sidebar on navigation
    const mobileNavLinks = document.querySelectorAll(".sidebar .nav-link:not([data-bs-toggle])");
    mobileNavLinks.forEach(link => {
        link.addEventListener("click", () => {
            if (window.innerWidth < 992) {
                if (sidebar) sidebar.classList.remove("mobile-show");
                if (overlay) overlay.classList.remove("show");
            }
        });
    });
});

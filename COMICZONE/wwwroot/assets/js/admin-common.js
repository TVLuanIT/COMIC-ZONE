/**
 * admin-common.js
 * Core helpers, global notifications, and general layout logic.
 */

// ===============================
// Alert Helper (SweetAlert2 wrapper)
// ===============================
const AlertHelper = (() => {
    const confirm = (title, text) => {
        return Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Huỷ',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33'
        });
    };

    const success = (message) => {
        return Swal.fire({
            title: 'Thành công',
            text: message,
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        });
    };

    const error = (message = 'Đã xảy ra lỗi') => {
        return Swal.fire({
            title: 'Lỗi',
            text: message,
            icon: 'error'
        });
    };

    return {
        confirm,
        success,
        error
    };
})();

// ===============================
// Global Notifications handler
// ===============================
const GlobalNotifications = () => {
    const init = () => {
        if (typeof Swal === "undefined" || !window.notifications) return;

        const { success, error, warning } = window.notifications;

        if (success) {
            Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: success,
                timer: 3000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        }

        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: error,
                toast: true,
                position: 'top-end',
                showConfirmButton: true
            });
        }

        if (warning) {
            Swal.fire({
                icon: 'warning',
                title: 'Cảnh báo!',
                text: warning,
                toast: true,
                position: 'top-end',
                showConfirmButton: true
            });
        }
    };
    return { init };
};

// ===============================
// General Layout logic
// ===============================
const AdminLayout = () => {
    const init = () => {
        handleCollapseIcon();
    };

    const handleCollapseIcon = () => {
        // Handle collapse icon for system menu (example)
        const menus = document.querySelectorAll('.collapse');
        menus.forEach(menu => {
            const parent = menu.closest(".nav-item");
            if (!parent) return;

            const plusIcon = parent.querySelector(".icon-plus");
            const minusIcon = parent.querySelector(".icon-minus");

            if (!plusIcon || !minusIcon) return;

            function updateIcon() {
                if (menu.classList.contains("show")) {
                    plusIcon.style.display = "none";
                    minusIcon.style.display = "inline-block";
                } else {
                    plusIcon.style.display = "inline-block";
                    minusIcon.style.display = "none";
                }
            }

            menu.addEventListener("shown.bs.collapse", updateIcon);
            menu.addEventListener("hidden.bs.collapse", updateIcon);
            updateIcon(); // Initial state
        });
    };

    return { init };
};

// ===============================
// DOM Initialization
// ===============================
const safeInit = (module) => {
    if (typeof module === "function") {
        module().init();
    }
};

const initAdmin = () => {
    if (typeof window.jQuery === "undefined") {
        console.error("jQuery chưa load");
        return;
    }

    console.log("admin modules initializing...");

    safeInit(GlobalNotifications);
    safeInit(AdminLayout);

    // Feature modules (initialized if they exist)
    if (typeof ProductIndex === 'function') safeInit(ProductIndex);
    if (typeof BlogIndex === 'function') safeInit(BlogIndex);
    if (typeof BadgeSelect === 'function') safeInit(BadgeSelect);
    if (typeof ImagesUpload === 'function') safeInit(ImagesUpload);
    if (typeof ProductReviews === 'function') safeInit(ProductReviews);
    if (typeof ViolationReportIndex === 'function') safeInit(ViolationReportIndex);
    if (typeof OrderIndex === 'function') safeInit(OrderIndex);
    if (typeof UserIndex === 'function') safeInit(UserIndex);
    if (typeof CustomerIndex === 'function') safeInit(CustomerIndex);
    if (typeof ProductReviewIndex === 'function') safeInit(ProductReviewIndex);
    if (typeof ProductReviewReplyIndex === 'function') safeInit(ProductReviewReplyIndex);
    if (typeof TagIndex === 'function') safeInit(TagIndex);
    if (typeof ArtistIndex === 'function') safeInit(ArtistIndex);
    if (typeof InvoiceIndex === 'function') safeInit(InvoiceIndex);
    if (typeof NotificationIndex === 'function') safeInit(NotificationIndex);
    if (typeof RefundIndex === 'function') safeInit(RefundIndex);
    if (typeof BlogCommentIndex === 'function') safeInit(BlogCommentIndex);
    if (typeof BlogCommentReplyIndex === 'function') safeInit(BlogCommentReplyIndex);

    // Reports Dashboard entry point
    const reportsConfig = document.querySelector('[data-reports-config]');
    if (reportsConfig) {
        try {
            const urls = JSON.parse(reportsConfig.getAttribute('data-reports-config'));
            if (typeof ReportsDashboard === 'function') {
                ReportsDashboard().init(urls);
            }
        } catch (e) {
            console.error("Failed to parse reports config", e);
        }
    } else if (typeof window.initReports === 'function') {
        window.initReports();
    }
}

// Initial entry
if (document.readyState !== "loading") {
    initAdmin();
} else {
    document.addEventListener("DOMContentLoaded", initAdmin);
}

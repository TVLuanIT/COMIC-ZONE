// wwwroot/js/site.js - Global helpers and Main Initialization
function openAvatarUpload() {
    document.getElementById("avatarUpload").click();
}

function uploadAvatar(input) {
    if (input.files.length === 0) return;
    const file = input.files[0];
    const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
    const maxSize = 2 * 1024 * 1024;

    if (!allowedTypes.includes(file.type)) {
        if (PremiumSwal) PremiumSwal.fire({ icon: 'warning', title: 'File không hợp lệ', text: 'Chỉ cho phép JPG, PNG hoặc WEBP' });
        return;
    }

    if (file.size > maxSize) {
        if (PremiumSwal) PremiumSwal.fire({ icon: 'warning', title: 'Ảnh quá lớn', text: 'Avatar phải nhỏ hơn 2MB' });
        return;
    }

    const reader = new FileReader();
    reader.onload = function (e) {
        let avatar = document.querySelector(".profile-avatar img");
        if (!avatar) {
            const defaultAvatar = document.querySelector(".default-avatar");
            if (defaultAvatar) {
                avatar = document.createElement("img");
                avatar.className = "avatar-img";
                defaultAvatar.replaceWith(avatar);
            }
        }
        if (avatar) avatar.src = e.target.result;
    };
    reader.readAsDataURL(file);

    let formData = new FormData();
    formData.append("avatar", file);

    fetch("/UserProfiles/UploadAvatar", { method: "POST", body: formData })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                if (PremiumSwal) PremiumSwal.fire({
                    icon: 'success', title: 'Thành công', timer: 1500, showConfirmButton: false
                }).then(() => location.reload());
            } else {
                if (PremiumSwal) PremiumSwal.fire({ icon: 'error', title: 'Lỗi', text: data.message });
            }
        })
        .catch(() => {
            if (PremiumSwal) PremiumSwal.fire({ icon: 'error', title: 'Lỗi', text: 'Không thể tải avatar.' });
        });
}

// Global Premium Swal Mixin
window.PremiumSwal = typeof Swal !== 'undefined' ? Swal.mixin({
    customClass: {
        popup: 'premium-swal-popup',
        confirmButton: 'premium-swal-confirm',
        cancelButton: 'premium-swal-cancel'
    },
    buttonsStyling: false,
    reverseButtons: true,
    showClass: { popup: 'animate__animated animate__fadeInDown animate__faster' },
    hideClass: { popup: 'animate__animated animate__fadeOutUp animate__faster' }
}) : null;

const PremiumSwal = window.PremiumSwal;

window.showLoginRequired = function(message) {
    if (window.isShowingLoginAlert) return;
    window.isShowingLoginAlert = true;

    PremiumSwal.fire({
        icon: 'warning',
        title: 'Yêu cầu đăng nhập',
        text: message,
        confirmButtonText: 'Đăng nhập',
        cancelButtonText: 'Hủy',
        showCancelButton: true,
        reverseButtons: true,
        allowOutsideClick: false,
        allowEscapeKey: false
    }).then((result) => {
        window.isShowingLoginAlert = false;
        console.log("Alert result:", result);
        if (result.isConfirmed) {
            const currentUrl = window.location.pathname + window.location.search;
            window.location.href = `/Account/Authentication/Login?returnUrl=${encodeURIComponent(currentUrl)}`;
        }
    });
}

function checkLoginStatus(selector) {
    let container = (selector instanceof jQuery ? selector : $(selector)).first();
    return container.length && container.attr('data-loggedin') === "true";
}

// Unified Notification System
const AutoAlert = (() => {
    const showToast = (message) => {
        const toast = document.createElement("div");
        toast.className = "toast-notification";
        toast.innerText = message;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add("toast-show"), 100);
        setTimeout(() => {
            toast.classList.remove("toast-show");
            setTimeout(() => toast.remove(), 500);
        }, 3500);
    };

    const init = () => {
        if (window.successMessage) {
            showToast(window.successMessage);
            window.successMessage = null;
        }
        if (window.errorMessage) {
            if (PremiumSwal) {
                PremiumSwal.fire({ icon: 'error', title: 'Thông báo', text: window.errorMessage });
            }
            window.errorMessage = null;
        }
        if (window.loginRequiredMessage) {
            showLoginRequired(window.loginRequiredMessage);
            window.loginRequiredMessage = null;
        }
    };

    return { init, showToast };
})();

// MAIN DOM READY INITIALIZATION
document.addEventListener('DOMContentLoaded', () => {
    // UI-UX Common
    if (typeof ProductSummary !== 'undefined') ProductSummary.init();
    if (typeof ProductSort !== 'undefined') ProductSort.init();
    if (typeof UrlTabs !== 'undefined') UrlTabs.init();
    if (typeof AutoAlert !== 'undefined') AutoAlert.init();
    if (typeof ReviewMenu_UserProfile !== 'undefined') ReviewMenu_UserProfile.init();
    if (typeof ReplyMenu_UserProfile !== 'undefined') ReplyMenu_UserProfile.init();

    // Checkout
    if (typeof CartQuantity !== 'undefined') CartQuantity.init();
    if (typeof CheckoutPayment !== 'undefined') CheckoutPayment.init();

    // Chatbot
    if (typeof ChatbotAI !== 'undefined') ChatbotAI.init();

    // Product Reviews (Details Page Only)
    const productIdElement = document.getElementById('product-id');
    if (productIdElement && typeof ProductReviews !== 'undefined') {
        const id = parseInt(productIdElement.value);
        if (!isNaN(id)) ProductReviews.init(id);
    }

    // Global Report Init
    if (typeof ProductReport !== 'undefined') ProductReport.init();

    // Refresh AOS on tab change
    var tabEls = document.querySelectorAll('button[data-bs-toggle="tab"], a[data-bs-toggle="tab"]');
    tabEls.forEach(function (tabEl) {
        tabEl.addEventListener('shown.bs.tab', function (event) {
            if (typeof AOS !== 'undefined') {
                AOS.refresh();
            }
        });
    });
});
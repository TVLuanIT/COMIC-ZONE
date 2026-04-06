// ui-ux.js - Xử lý các hiệu ứng giao diện, sắp xếp, và các tiện ích nhỏ
// TÓM TẮT SẢN PHẨM (XEM THÊM / ẨN BỚT)
const ProductSummary = (() => {
    const maxLength = 250;
    const init = () => {
        document.querySelectorAll('.summary-text').forEach(p => {
            const fullText = p.getAttribute('data-full');
            const btn = p.nextElementSibling;
            if (fullText && fullText.length > maxLength) {
                p.innerText = fullText.substring(0, maxLength) + '...';
                btn.innerText = 'Xem thêm';
            } else {
                p.innerText = fullText || '';
                if (btn) btn.style.display = 'none';
            }
        });
    };
    const toggle = (id) => {
        const p = document.getElementById(`desc-${id}`);
        const btn = p.nextElementSibling;
        const fullText = p.getAttribute('data-full');
        if (btn.innerText === 'Xem thêm') {
            p.innerText = fullText;
            btn.innerText = 'Ẩn bớt';
        } else {
            p.innerText = fullText.substring(0, maxLength) + '...';
            btn.innerText = 'Xem thêm';
        }
    };
    return { init, toggle };
})();

// XỬ LÝ SẮP XẾP SẢN PHẨM (CUSTOM DROPDOWN)
const ProductSort = (() => {
    const init = () => {
        const sortWrapper = document.getElementById('sortWrapper');
        const sortTrigger = document.getElementById('sortTrigger');
        const sortOptions = document.querySelectorAll('.sort-option-item');
        const sortInput = document.getElementById('currentSortOrder');
        const sortForm = document.getElementById('sortForm');
        if (!sortTrigger || !sortWrapper) return;
        sortTrigger.addEventListener('click', (e) => { e.stopPropagation(); sortWrapper.classList.toggle('active'); });
        sortOptions.forEach(option => {
            option.addEventListener('click', () => {
                const val = option.getAttribute('data-value');
                if (sortInput) sortInput.value = val;
                if (sortForm) sortForm.submit();
            });
        });
        document.addEventListener('click', (e) => { if (sortWrapper && !sortWrapper.contains(e.target)) sortWrapper.classList.remove('active'); });
    };
    return { init };
})();

// XỬ LÝ TAB TRÊN URL
const UrlTabs = {
    init() {
        const params = new URLSearchParams(window.location.search);
        const tab = params.get("tab");
        if (!tab) return;
        const trigger = document.querySelector(`[data-bs-target="#${tab}"]`);
        if (trigger) new bootstrap.Tab(trigger).show();
    }
};

// THÔNG BÁO TỰ ĐỘNG
// User Profile Menus
const ReviewMenu_UserProfile = (() => {
    const init = () => {
        $(document).off('click', '.menu-btn');
        $(document).on('click', '.menu-btn', function (e) {
            e.stopPropagation();
            const wrapper = $(this).closest('.menu-wrapper');
            $('.menu-wrapper').not(wrapper).removeClass('active');
            wrapper.toggleClass('active');
        });
        $(document).on('click', function () { $('.menu-wrapper').removeClass('active'); });
        $(document).off("click", ".delete-review");
        $(document).on("click", ".delete-review", function (e) { e.preventDefault(); const id = $(this).data("id"); if (id) { window.reviewIdToDelete = id; $("#deleteConfirmModal").addClass("active"); } });
        $(document).on("click", "#deleteConfirmModal .btn-cancel", function () { $("#deleteConfirmModal").removeClass("active"); });
        $(document).on("click", "#deleteConfirmModal .btn-delete", function () {
            const token = $('input[name="__RequestVerificationToken"]').val();
            $.ajax({ url: "/ProductReviews/DeleteReview", type: "POST", data: { id: window.reviewIdToDelete, __RequestVerificationToken: token }, success: function (res) { if (res.success) location.reload(); } });
        });
    };
    return { init };
})();

const ReplyMenu_UserProfile = (() => {
    const init = () => {
        $(document).on("click", ".delete-reply", function (e) { e.preventDefault(); const id = $(this).data("id"); if (id) { window.replyIdToDelete = id; $("#deleteReplyConfirmModal").addClass("active"); } });
        $(document).on("click", "#deleteReplyConfirmModal .btn-cancel", function () { $("#deleteReplyConfirmModal").removeClass("active"); });
        $(document).on("click", "#deleteReplyConfirmModal .btn-delete", function () {
            const token = $('input[name="__RequestVerificationToken"]').val();
            $.ajax({ url: "/ProductReviews/DeleteReply", type: "POST", data: { replyId: window.replyIdToDelete, __RequestVerificationToken: token }, success: function (res) { if (res.success) location.href = window.location.pathname + "?tab=replies"; } });
        });
    };
    return { init };
})();

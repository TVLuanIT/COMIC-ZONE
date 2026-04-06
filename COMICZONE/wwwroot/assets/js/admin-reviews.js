/**
 * admin-reviews.js
 * Product reviews and replies management logic.
 */

// Star rating range slider logic
const ProductReviews = () => {
    const init = () => {
        const range = document.getElementById("ratingRange");
        if (!range) return;

        let currentValue = parseInt(range.value) || 5;

        renderStars(currentValue);
        updateText(currentValue);

        range.addEventListener("input", function () {
            currentValue = parseInt(this.value);
            renderStars(currentValue);
            updateText(currentValue);
        });
    };

    const renderStars = (val) => {
        const container = document.getElementById("ratingStars");
        if (!container) return;

        let html = "";
        for (let i = 1; i <= 5; i++) {
            html += `<i class="ti ${i <= val ? "ti-star-filled" : "ti-star"}"></i>`;
        }
        container.innerHTML = html;
    };

    const updateText = (val) => {
        const text = document.getElementById("ratingValue");
        if (text) text.innerText = val;
    };

    return { init };
};

// Review Index Module
const ProductReviewIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-review-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hiển thị/Ẩn" của đánh giá này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#review-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện đánh giá');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hiển thị</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn đánh giá');
                        }

                        AlertHelper.success("Thay đổi trạng thái thành công");
                    },
                    error: function () {
                        AlertHelper.error('Không thể kết nối server');
                    }
                });
            });
        });
    };

    return { init };
};

// Review Reply Index Module
const ProductReviewReplyIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-reply-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hiển thị/Ẩn" của phản hồi này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#reply-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện phản hồi');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hiển thị</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn phản hồi');
                        }

                        AlertHelper.success("Thay đổi trạng thái thành công");
                    },
                    error: function () {
                        AlertHelper.error('Không thể kết nối server');
                    }
                });
            });
        });
    };

    return { init };
};

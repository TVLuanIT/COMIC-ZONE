/**
 * admin-blog-comments.js
 * Blog comments and replies management logic.
 */

// Blog Comment Index Module
const BlogCommentIndex = () => {
    const init = () => {
        toggleCommentStatus();
    };

    const toggleCommentStatus = () => {
        jQuery(document).on('click', '.ajax-toggle-blog-comment-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hiển thị/Ẩn" của bình luận này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#comment-row-' + id);
                        const statusCell = jQuery('#comment-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger rounded-pill px-3 py-2">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện bình luận');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success rounded-pill px-3 py-2">Hiển thị</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn bình luận');
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

// Blog Comment Reply Index Module
const BlogCommentReplyIndex = () => {
    const init = () => {
        toggleReplyStatus();
    };

    const toggleReplyStatus = () => {
        jQuery(document).on('click', '.ajax-toggle-blog-reply-status', function () {
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
                        const row = jQuery('#blog-reply-row-' + id);
                        const statusCell = jQuery('#blog-reply-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger rounded-pill px-3 py-2">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện phản hồi');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success rounded-pill px-3 py-2">Hiển thị</span>');
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

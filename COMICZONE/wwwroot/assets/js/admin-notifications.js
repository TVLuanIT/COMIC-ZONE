/**
 * admin-notifications.js
 * Notifications management logic.
 */

// Notification Index Module
const NotificationIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-notification-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn Thay đổi trạng thái Ẩn/Hiện của thông báo này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#notification-row-' + id);
                        const statusCell = jQuery('#soft-delete-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger-subtle text-danger border border-danger-subtle px-2"><i class="ti ti-eye-off me-1 small"></i> Ẩn</span>');
                            btn.removeClass('text-danger').addClass('text-success');
                            btn.html('<i class="ti ti-refresh fs-5"></i>');
                            btn.attr('title', 'Khôi phục');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success-subtle text-success border border-success-subtle px-2"><i class="ti ti-eye me-1 small"></i> Hiện</span>');
                            btn.removeClass('text-success').addClass('text-danger');
                            btn.html('<i class="ti ti-ban fs-5"></i>');
                            btn.attr('title', 'Xóa mềm');
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

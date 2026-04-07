/**
 * admin-refunds.js
 * Handles AJAX toggles and interactions for the Refund Management module.
 */

const RefundIndex = () => {
    const init = () => {
        handleStatusToggle();
    };

    const handleStatusToggle = () => {
        $(document).off('click', '.ajax-toggle-refund-status').on('click', '.ajax-toggle-refund-status', function (e) {
            e.preventDefault();
            const $btn = $(this);
            const id = $btn.data('id');
            const url = $btn.data('url');
            const $icon = $btn.find('i');
            const $row = $(`#row-${id}`);
            const $cell = $(`#visibility-cell-${id}`);

            if (!url) return;

            // Use AlertHelper from admin-common.js if available
            const confirmTitle = 'Bạn có chắc chắn?';
            const confirmText = 'Trạng thái hiển thị của hồ sơ hoàn tiền này sẽ thay đổi.';

            if (typeof AlertHelper !== 'undefined') {
                AlertHelper.confirm(confirmTitle, confirmText).then((result) => {
                    if (result.isConfirmed) {
                        performToggle(id, url, $btn, $icon, $row, $cell);
                    }
                });
            } else {
                if (confirm(confirmText)) {
                    performToggle(id, url, $btn, $icon, $row, $cell);
                }
            }
        });
    };

    const performToggle = (id, url, $btn, $icon, $row, $cell) => {
        // Disable button during request
        $btn.prop('disabled', true).addClass('opacity-50');

        $.ajax({
            url: url,
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    const isDeleted = response.isDeleted;

                    // Update Row appearance
                    if (isDeleted) {
                        $row.addClass('table-light text-muted');
                        $cell.html(`
                            <span class="badge bg-secondary text-white px-3 py-2 rounded-pill fw-bold shadow-sm">
                                <i class="ti ti-eye-off me-1"></i> Tạm ẩn
                            </span>
                        `);
                        $icon.removeClass('ti-eye-off text-warning').addClass('ti-refresh text-success');
                        $btn.attr('title', 'Khôi phục');
                    } else {
                        $row.removeClass('table-light text-muted');
                        $cell.html(`
                            <span class="badge bg-info text-white px-3 py-2 rounded-pill fw-bold shadow-sm">
                                <i class="ti ti-eye me-1"></i> Hiển thị
                            </span>
                        `);
                        $icon.removeClass('ti-refresh text-success').addClass('ti-eye-off text-warning');
                        $btn.attr('title', 'Xóa mềm (Tạm ẩn)');
                    }

                    if (typeof AlertHelper !== 'undefined') {
                        AlertHelper.success('Đã cập nhật trạng thái hiển thị');
                    }
                } else {
                    if (typeof AlertHelper !== 'undefined') {
                        AlertHelper.error(response.message || 'Đã có lỗi xảy ra');
                    } else {
                        alert(response.message || 'Đã có lỗi xảy ra');
                    }
                }
            },
            error: function () {
                if (typeof AlertHelper !== 'undefined') {
                    AlertHelper.error('Lỗi kết nối máy chủ');
                } else {
                    alert('Lỗi kết nối máy chủ');
                }
            },
            complete: function () {
                $btn.prop('disabled', false).removeClass('opacity-50');
            }
        });
    };

    return { init };
};

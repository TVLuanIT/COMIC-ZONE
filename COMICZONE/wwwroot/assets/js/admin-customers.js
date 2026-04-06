/**
 * admin-customers.js
 * Customers management logic.
 */

const CustomerIndex = () => {
    const init = () => {
        handleToggleStatus();
    };

    const handleToggleStatus = () => {
        jQuery(document).on('click', '.ajax-toggle-customer-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi?',
                'Bạn có chắc chắn muốn thay đổi trạng thái vô hiệu hóa của hồ sơ này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#customer-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã vô hiệu hóa</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Khôi phục hồ sơ');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hoạt động</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Vô hiệu hóa hồ sơ');
                        }

                        AlertHelper.success("Cập nhật trạng thái thành công");
                    },
                    error: function () {
                        AlertHelper.error('Không thể kết nối đến máy chủ');
                    }
                });
            });
        });
    };

    return { init };
};

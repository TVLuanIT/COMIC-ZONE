/**
 * admin-tags.js
 * Tags management logic.
 */

// Tag Index Module
const TagIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-tag-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Ẩn/Hiện" của thẻ này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#tag-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện thẻ');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hiển thị</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn thẻ');
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

/**
 * admin-blogs.js
 * Logic for Blogs and BlogCategories index pages.
 */

const BlogIndex = () => {
    const init = () => {
        handleToggleStatus();
    };

    const handleToggleStatus = () => {
        jQuery(document).on('click', '.ajax-toggle-blog-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');
            
            // Determine if it's a Blog or Category for the message
            const isCategory = url.toLowerCase().includes('category');
            const typeName = isCategory ? 'danh mục' : 'bài viết';

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                `Bạn có chắc chắn muốn thay đổi trạng thái của ${typeName} này?` + 
                (isCategory ? ' (Tất cả bài viết thuộc danh mục này cũng sẽ bị ẩn/hiện tương ứng)' : '')
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) {
                            AlertHelper.error(response.message || 'Thao tác không thành công');
                            return;
                        }

                        const isDeleted = response.isDeleted;
                        const rowId = isCategory ? '#category-row-' + id : '#blog-row-' + id;
                        const row = jQuery(rowId);
                        const editBtn = row.find('a[title="Chỉnh sửa"]');
                        
                        // Select the correct cell to update
                        const cellToUpdate = isCategory 
                            ? jQuery('#status-cell-' + id) 
                            : jQuery('#visibility-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted');
                            cellToUpdate.html(`
                                <span class="badge bg-danger-subtle text-danger px-3 py-2 rounded-pill fw-bold">
                                    <i class="ti ti-eye-off me-1"></i> Tạm ẩn
                                </span>
                            `);
                            btn.removeClass('btn-outline-warning').addClass('btn-outline-success');
                            btn.attr('title', 'Khôi phục');
                            btn.html('<i class="ti ti-refresh fs-5"></i>');
                            editBtn.addClass('disabled');
                        } else {
                            row.removeClass('bg-light text-muted');
                            
                            const label = isCategory ? 'Hoạt động' : 'Hiển thị';
                            const icon = isCategory ? 'ti-check' : 'ti-eye';

                            cellToUpdate.html(`
                                <span class="badge bg-success-subtle text-success px-3 py-2 rounded-pill fw-bold">
                                    <i class="ti ${icon} me-1"></i> ${label}
                                </span>
                            `);
                            
                            btn.removeClass('btn-outline-success').addClass('btn-outline-warning');
                            btn.attr('title', 'Tạm ẩn');
                            btn.html('<i class="ti ti-ban fs-5"></i>');
                            editBtn.removeClass('disabled');
                        }

                        AlertHelper.success("Cập nhật trạng thái thành công");
                        
                        // If it's a category toggle, we might want to reload to show changed counts or just for consistency
                        if (isCategory) {
                             setTimeout(() => location.reload(), 1000);
                        }
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

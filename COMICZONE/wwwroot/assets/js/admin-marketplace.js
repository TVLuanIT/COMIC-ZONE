// =============================================
// MARKETPLACE ADMIN MODULE
// =============================================

const MarketplacePostIndex = {
    init() {
        this.bindApproveButtons();
        this.bindRejectButtons();
        this.bindToggleDeleteButtons();
    },

    bindApproveButtons() {
        document.querySelectorAll('.btn-approve-post').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = btn.dataset.id;
                const result = await Swal.fire({
                    title: 'Duyệt bài đăng?',
                    text: `Bạn muốn duyệt bài đăng #${id}?`,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonColor: '#198754',
                    cancelButtonText: 'Hủy',
                    confirmButtonText: 'Duyệt'
                });

                if (result.isConfirmed) {
                    try {
                        const response = await fetch(`/Admin/MarketplacePosts/Approve/${id}`, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' }
                        });
                        const data = await response.json();
                        if (data.success) {
                            Swal.fire('Đã duyệt!', 'Bài đăng đã được phê duyệt.', 'success')
                                .then(() => location.reload());
                        }
                    } catch (err) {
                        Swal.fire('Lỗi', 'Không thể duyệt bài đăng.', 'error');
                    }
                }
            });
        });
    },

    bindRejectButtons() {
        document.querySelectorAll('.btn-reject-post').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = btn.dataset.id;
                const result = await Swal.fire({
                    title: 'Từ chối bài đăng?',
                    text: `Bạn muốn từ chối bài đăng #${id}?`,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#dc3545',
                    cancelButtonText: 'Hủy',
                    confirmButtonText: 'Từ chối'
                });

                if (result.isConfirmed) {
                    try {
                        const response = await fetch(`/Admin/MarketplacePosts/Reject/${id}`, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' }
                        });
                        const data = await response.json();
                        if (data.success) {
                            Swal.fire('Đã từ chối!', 'Bài đăng đã bị từ chối.', 'info')
                                .then(() => location.reload());
                        }
                    } catch (err) {
                        Swal.fire('Lỗi', 'Không thể từ chối bài đăng.', 'error');
                    }
                }
            });
        });
    },

    bindToggleDeleteButtons() {
        document.querySelectorAll('.btn-toggle-delete-post').forEach(btn => {
            btn.addEventListener('click', async () => {
                const id = btn.dataset.id;
                const result = await Swal.fire({
                    title: 'Thay đổi trạng thái?',
                    text: `Ẩn/Hiện bài đăng #${id}?`,
                    icon: 'question',
                    showCancelButton: true,
                    cancelButtonText: 'Hủy',
                    confirmButtonText: 'Xác nhận'
                });

                if (result.isConfirmed) {
                    try {
                        const response = await fetch(`/Admin/MarketplacePosts/ToggleDelete/${id}`, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' }
                        });
                        const data = await response.json();
                        if (data.success) {
                            const row = document.getElementById(`mkt-post-row-${id}`);
                            const icon = btn.querySelector('i');
                            const badge = row ? row.querySelector('.visibility-badge') : null;

                            if (data.isDeleted) {
                                if (row) row.classList.add('bg-light', 'text-muted', 'opacity-75');
                                if (icon) icon.className = 'ti ti-refresh';
                                btn.title = 'Hiển thị lại';
                                if (badge) {
                                    badge.className = 'badge bg-danger-subtle text-danger visibility-badge';
                                    badge.innerText = 'Đã ẩn';
                                }
                            } else {
                                if (row) row.classList.remove('bg-light', 'text-muted', 'opacity-75');
                                if (icon) icon.className = 'ti ti-ban';
                                btn.title = 'Tạm ẩn';
                                if (badge) {
                                    badge.className = 'badge bg-success-subtle text-success visibility-badge';
                                    badge.innerText = 'Công khai';
                                }
                            }
                            Swal.fire({
                                toast: true, position: 'top-end', icon: 'success',
                                title: data.isDeleted ? 'Đã ẩn bài đăng' : 'Đã hiện bài đăng',
                                showConfirmButton: false, timer: 1500
                            });
                        }
                    } catch (err) {
                        console.error(err);
                        Swal.fire('Lỗi', 'Không thể thay đổi trạng thái.', 'error');
                    }
                }
            });
        });
    }
};

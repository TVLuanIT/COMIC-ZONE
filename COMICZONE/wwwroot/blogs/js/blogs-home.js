/* JS for Blogs Index Page */
document.addEventListener('DOMContentLoaded', () => {
    const blogBtn = document.querySelector('.auth-check-link');
    if (blogBtn) {
        blogBtn.addEventListener('click', function(e) {
            const isLoggedIn = this.getAttribute('data-loggedin') === 'true';
            if (!isLoggedIn) {
                e.preventDefault();
                
                // Use PremiumSwal from window to ensure synchronization
                const swal = window.PremiumSwal || (typeof PremiumSwal !== 'undefined' ? PremiumSwal : null);
                
                if (swal) {
                    swal.fire({
                        icon: 'warning',
                        title: 'Yêu cầu đăng nhập',
                        text: 'Vui lòng đăng nhập để chia sẻ bài viết của bạn với cộng đồng.',
                        confirmButtonText: 'Đăng nhập',
                        cancelButtonText: 'Hủy',
                        showCancelButton: true,
                        reverseButtons: true
                    }).then((result) => {
                        if (result.isConfirmed) {
                            const currentUrl = window.location.pathname + window.location.search;
                            window.location.href = `/Account/Authentication/Login?returnUrl=${encodeURIComponent(currentUrl)}`;
                        }
                    });
                } else if (typeof window.showLoginRequired === 'function') {
                    window.showLoginRequired("Vui lòng đăng nhập để viết bài.");
                } else {
                    // Final fallback
                    console.log("PremiumSwal not found, using fallback");
                    window.location.href = `/Account/Authentication/Login?returnUrl=${encodeURIComponent(window.location.pathname)}`;
                }
            }
        });
    }
});

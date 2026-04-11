/* JS for Blog Creation Page */
document.addEventListener('DOMContentLoaded', function() {
    const thumbnailInput = document.getElementById('thumbnail-input');
    if (thumbnailInput) {
        thumbnailInput.addEventListener('change', function(e) {
            if (e.target.files && e.target.files[0]) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const previewImg = document.querySelector('#thumbnail-preview img');
                    if (previewImg) {
                        previewImg.src = e.target.result;
                    }
                }
                reader.readAsDataURL(e.target.files[0]);
            }
        });
    }

    // Animation for form submission
    const form = document.querySelector('.premium-form');
    if (form) {
        form.addEventListener('submit', function() {
            // Using jQuery validation if available
            const isJQueryValid = typeof $ !== 'undefined' && typeof $.fn.valid === 'function' && $(this).valid();
            const isNativeValid = this.checkValidity();
            
            if (isJQueryValid || isNativeValid) {
                const btn = this.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = true;
                    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Đang gửi bài...';
                }
            }
        });
    }
});

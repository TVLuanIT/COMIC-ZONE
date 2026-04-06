// order.js - Quản lý thanh toán và các bước đặt hàng
const CheckoutPayment = (() => {
    const init = () => {
        const cards = document.querySelectorAll('.payment-card');
        const hiddenSelect = document.getElementById('payment-method');
        const defaultButton = document.getElementById('default-submit-button');
        const paypalWrapper = document.getElementById('paypal-button-wrapper');

        if (!cards.length || !hiddenSelect) return;

        function toggleVisuals(val) {
            if (val === '3') {
                if (defaultButton) defaultButton.style.setProperty('display', 'none', 'important');
                if (paypalWrapper) {
                    paypalWrapper.style.display = 'block';
                    paypalWrapper.classList.add('premium-fade-in');
                }
            } else {
                if (defaultButton) defaultButton.style.setProperty('display', 'flex', 'important');
                if (paypalWrapper) {
                    paypalWrapper.style.display = 'none';
                    paypalWrapper.classList.remove('premium-fade-in');
                }
            }
        }

        cards.forEach(card => {
            card.addEventListener('click', function() {
                cards.forEach(c => c.classList.remove('active'));
                this.classList.add('active');
                
                const val = this.getAttribute('data-value');
                hiddenSelect.value = val;
                
                toggleVisuals(val);
                
                // Gửi sự kiện thay đổi cho các script khác (vd: paypal.js)
                hiddenSelect.dispatchEvent(new Event('change'));
            });
        });

        // Khởi tạo trạng thái ban đầu
        toggleVisuals(hiddenSelect.value || 'COD');
    };

    return { init };
})();

// Khởi tạo khi DOM sẵn sàng
document.addEventListener('DOMContentLoaded', () => {
    CheckoutPayment.init();
});

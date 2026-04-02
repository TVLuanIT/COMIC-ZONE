// checkout.js - Xử lý giỏ hàng và thanh toán (PayPal, Update Quantity)
const CartQuantity = (() => {
    const init = () => {
        document.querySelectorAll(".quantity-input").forEach(input => {
            input.addEventListener("change", function () {
                const cartItemId = this.dataset.cartitemId;
                const quantity = this.value;

                fetch("/Carts/UpdateQuantity", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({
                        cartItemId: parseInt(cartItemId),
                        quantity: parseInt(quantity)
                    })
                })
                    .then(res => {
                        if (!res.ok) throw new Error("Server error");
                        return res.json();
                    })
                    .then(data => {
                        if (data.success) {
                            // cập nhật tổng tiền item
                            const itemContainer = input.closest(".cart-item-modern") || input.closest("tr");
                            
                            if (itemContainer) {
                                const totalCell = itemContainer.querySelector(".item-total");
                                if (totalCell) {
                                    totalCell.innerText = data.itemTotal + " ₫";
                                }
                            }

                            // cập nhật tổng tiền giỏ hàng
                            const cartTotal = document.getElementById("cart-total");

                            if (cartTotal) {
                                cartTotal.innerText = data.cartTotal + " ₫";
                            }
                        }
                    })
                    .catch(err => {
                        console.error(err);
                    });
            });
        });
    };

    return { init };
})();

// CHECKOUT PAYMENT TOGGLE
const CheckoutPayment = (() => {
    const init = () => {
        const cards = document.querySelectorAll('.payment-card');
        const hiddenSelect = document.getElementById('payment-method');
        const defaultButton = document.getElementById('default-submit-button');
        const paypalContainer = document.getElementById('paypal-button-container');

        if (!cards.length || !hiddenSelect) return;

        function toggleVisuals(val) {
            if (val === 'Paypal') {
                if (defaultButton) defaultButton.style.setProperty('display', 'none', 'important');
                if (paypalContainer) {
                    paypalContainer.style.display = 'block';
                    paypalContainer.classList.add('premium-fade-in');
                }
            } else {
                if (defaultButton) defaultButton.style.setProperty('display', 'flex', 'important');
                if (paypalContainer) {
                    paypalContainer.style.display = 'none';
                    paypalContainer.classList.remove('premium-fade-in');
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
                
                // Notify other scripts (like paypal.js initialization)
                hiddenSelect.dispatchEvent(new Event('change'));
            });
        });

        // Set initial state
        toggleVisuals(hiddenSelect.value || 'COD');
    };

    return { init };
})();

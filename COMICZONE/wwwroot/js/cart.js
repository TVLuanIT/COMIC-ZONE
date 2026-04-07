// cart.js - Xử lý các tương tác trong giỏ hàng (Số lượng, AJAX)
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

                            // cập nhật tổng tiền giỏ hàng (trong sidebar summary nếu có)
                            const cartTotal = document.getElementById("cart-total");

                            if (cartTotal) {
                                cartTotal.innerText = data.cartTotal + " ₫";
                            }

                            // Cập nhật badge giỏ hàng
                            const cartBadge = document.querySelector(".cart-badge");
                            if (cartBadge) {
                                if (data.cartCount > 0) {
                                    cartBadge.innerText = data.cartCount > 99 ? "99+" : data.cartCount;
                                    cartBadge.classList.remove("d-none");
                                } else {
                                    cartBadge.classList.add("d-none");
                                }
                            }
                        }
                    })
                    .catch(err => {
                        console.error("Lỗi cập nhật số lượng:", err);
                    });
            });
        });

        // Hỗ trợ nút tăng/giảm số lượng
        document.querySelectorAll('.increase-qty').forEach(btn => {
            btn.addEventListener('click', function () {
                const input = this.parentElement.querySelector('.quantity-input');
                if (input) {
                    input.value = parseInt(input.value) + 1;
                    input.dispatchEvent(new Event('change'));
                }
            });
        });

        document.querySelectorAll('.decrease-qty').forEach(btn => {
            btn.addEventListener('click', function () {
                const input = this.parentElement.querySelector('.quantity-input');
                if (input && parseInt(input.value) > 1) {
                    input.value = parseInt(input.value) - 1;
                    input.dispatchEvent(new Event('change'));
                }
            });
        });
    };

    return { init };
})();

// Khởi tạo khi DOM sẵn sàng
document.addEventListener('DOMContentLoaded', () => {
    CartQuantity.init();
});

const paypalButtons = () => {
    const init = () => {
        // Kiểm tra thư viện PayPal SDK đã tải xong chưa
        if (typeof paypal === 'undefined') {
            console.error("PayPal SDK chưa được tải. Vui lòng kiểm tra Client ID hoặc kết nối mạng.");
            return;
        }

        const container = document.getElementById('paypal-button-container');
        if (!container) {
            console.warn("Không tìm thấy #paypal-button-container. Bỏ qua khởi tạo PayPal.");
            return;
        }

        button();
    };

    const button = () => {
        paypal.Buttons({
            style: {
                layout: 'vertical',
                color: 'gold',
                shape: 'rect',
                label: 'paypal',
                tagline: false
            },

            onClick: function (data, actions) {
                let name = document.querySelector('input[name="fullname"]')?.value.trim();
                let phone = document.querySelector('input[name="phone"]')?.value.trim();
                let address = document.querySelector('textarea[name="address"]')?.value.trim();

                if (!name || !phone || !address) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Thiếu thông tin',
                        text: 'Vui lòng nhập đầy đủ Họ tên, SĐT và Địa chỉ nhận hàng.',
                        confirmButtonText: 'Đã hiểu'
                    });
                    return actions.reject();
                }

                return actions.resolve();
            },

            createOrder: (data, actions) => {
                let name = document.querySelector('input[name="fullname"]')?.value.trim();
                let phone = document.querySelector('input[name="phone"]')?.value.trim();
                let address = document.querySelector('textarea[name="address"]')?.value.trim();
                let note = document.querySelector('textarea[name="note"]')?.value.trim();

                return fetch("/Orders/create-paypal-order", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        Fullname: name,
                        Phone: phone,
                        Address: address,
                        Note: note,
                        PaymentMethod: 3
                    })
                })
                .then((response) => {
                    if (!response.ok) {
                        return response.json().then(err => { throw new Error(err.message || "Lỗi tạo đơn hàng"); });
                    }
                    return response.json();
                })
                .then((order) => order.id)
                .catch(err => {
                    console.error("Create Paypal Order Failed:", err);
                    Swal.fire('Lỗi', err.message, 'error');
                });
            },

            onApprove: (data, actions) => {
                return fetch(`/Orders/capture-paypal-order?orderId=${data.orderID}`, {
                    method: "POST",
                })
                .then((response) => {
                    if (!response.ok) {
                        return response.json().then(err => { throw new Error(err.message || "Lỗi xác nhận thanh toán"); });
                    }
                    window.location.href = "/Orders/Success";
                })
                .catch(error => {
                    console.error("Capture Paypal Order Failed:", error);
                    Swal.fire('Lỗi', error.message, 'error');
                });
            },

            onError: function (err) {
                console.error("PayPal SDK Error:", err);
                // Đôi khi SDK báo lỗi nếu container bị ẩn trong lúc render
                // Chúng ta sẽ log chi tiết để kỹ thuật kiểm tra
            }
        }).render('#paypal-button-container');
    };

    return { init };
};

document.addEventListener("DOMContentLoaded", () => {
    paypalButtons().init();
});
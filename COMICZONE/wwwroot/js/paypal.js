const paypalButtons = () => {
    const init = () => {
        displayButton();
        button();
    };

    const displayButton = () => {
        const paymentSelect = document.getElementById("payment-method");
        const paypalContainer = document.getElementById("paypal-button-container");
        const defaultButton = document.getElementById("default-submit-button");

        function togglePaymentButton() {
            if (paymentSelect.value === "Paypal") {
                paypalContainer.style.display = "block";
                defaultButton.style.display = "none";
            } else {
                paypalContainer.style.display = "none";
                defaultButton.style.display = "block";
            }
        }

        togglePaymentButton();

        paymentSelect.addEventListener("change", togglePaymentButton);
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
                    alert("Vui lòng nhập đầy đủ Họ tên, SĐT và Địa chỉ ở phần Thông tin thanh toán trước khi quẹt thẻ.");
                    return actions.reject(); // chặn popup PayPal mở
                }

                return actions.resolve(); // cho phép mở popup
            },

            createOrder: (data, actions) => {
                let name = document.querySelector('input[name="fullname"]')?.value.trim();
                let phone = document.querySelector('input[name="phone"]')?.value.trim();
                let address = document.querySelector('textarea[name="address"]')?.value.trim();
                let note = document.querySelector('textarea[name="note"]')?.value.trim();

                return fetch("/Orders/create-paypal-order", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
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
                            return response.text().then((err) => {
                                throw new Error(err);
                            });
                        }
                        return response.json();
                    })
                    .then((order) => order.id)
                    .catch(err => {
                        alert("Lỗi tạo PayPal Order: " + err.message);
                        console.error(err);
                    });
            },

            onApprove: (data, actions) => {
                return fetch(`/Orders/capture-paypal-order?orderId=${data.orderID}`, {
                    method: "POST",
                })
                    .then((response) => {
                        if (!response.ok) {
                            return response.json().then(err => { throw error; });
                        }
                        window.location.href = "/Orders/Success";
                    })
                    .catch(error => { alert(error.message); });
            },

            onError: function (err) {
                console.error("Lỗi PayPal:", err);

                alert("Đã xảy ra lỗi khi kết nối PayPal.");
            }
        }).render('#paypal-button-container');
    };

    return { init };
};

document.addEventListener("DOMContentLoaded", () => {
    paypalButtons().init();
});
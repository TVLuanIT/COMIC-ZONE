// ===============================
// helper
// ===============================
const AlertHelper = (() => {
    const confirm = (title, text) => {
        return Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Huỷ',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33'
        });
    };

    const success = (message) => {
        return Swal.fire({
            title: 'Thành công',
            text: message,
            icon: 'success',
            timer: 1500,
            showConfirmButton: false
        });
    };

    const error = (message = 'Đã xảy ra lỗi') => {
        return Swal.fire({
            title: 'Lỗi',
            text: message,
            icon: 'error'
        });
    };

    return {
        confirm,
        success,
        error
    };

})();

// ===============================
// Areas/Admin/Views/Products/Edit.cshtml
// ===============================
// Badge module (Artist + Tag)
const BadgeSelect = () => {
    const init = () => {
        initSelect2();
        initBadge(".selectedartist", "artistContainer", "artist-badge", "remove-artist", "SelectedArtists");
        initBadge(".selectedtag", "tagContainer", "tag-badge", "remove-tag", "SelectedTags");
        initRemove();
    };

    // ===============================
    // Select2
    // ===============================
    const initSelect2 = () => {
        if (typeof window.jQuery === "undefined") return;
        if (!window.jQuery.fn.select2) return;

        const selects = jQuery(".selectedartist, .selectedtag");

        if (!selects || !selects.length) return;

        selects.select2({
            width: "100%",
            placeholder: "Chọn",
            allowClear: true
        });
    };

    // ===============================
    // Tạo badge
    // ===============================
    const initBadge = (selectClass, containerId, badgeClass, removeClass, inputName) => {
        const select = document.querySelector(selectClass);

        if (!select) return;

        select.addEventListener("change", function () {
            const container = document.getElementById(containerId);
            if (!container) return;

            const selectedOptions = this.selectedOptions;

            for (let option of selectedOptions) {
                const id = option.value;
                const name = option.text;

                const exists = container.querySelector(`.${badgeClass}[data-id="${id}"]`);

                if (exists) continue;

                const badge = document.createElement("span");

                badge.className =
                    `badge bg-secondary ${badgeClass} d-flex align-items-center gap-1`;

                badge.setAttribute("data-id", id);

                badge.innerHTML =
                    name +
                    ` <button type="button" class="btn-close btn-close-white ${removeClass}"></button>` +
                    `<input type="hidden" name="${inputName}" value="${id}" />`;

                container.appendChild(badge);
            }
        });
    };

    // ===============================
    // Xoá badge
    // ===============================
    const initRemove = () => {
        document.addEventListener("click", function (e) {
            if (e.target.classList.contains("remove-artist") ||
                e.target.classList.contains("remove-tag")) {

                const badge = e.target.closest(".badge");

                if (badge) badge.remove();
            }
        });
    };

    return { init };
};

const ImagesUpload = () => {
    const init = () => {
        initRemovePicture();
        initCustomFileInput();
    };

    // ===============================
    // Xóa ảnh cũ (Edit page)
    // ===============================
    const initRemovePicture = () => {
        document.addEventListener("click", function (e) {
            const btn = e.target.closest(".remove-picture");

            if (!btn) return;

            const box = btn.closest(".picture-item");

            if (!box) return;

            const input = box.querySelector(".deleted-picture");

            if (input) {
                input.disabled = false;
            }

            box.style.display = "none";
        });
    };


    // ===============================
    // Upload + preview ảnh (Create + Edit)
    // ===============================
    const initCustomFileInput = () => {
        const inputNew = document.getElementById("NewPictures");
        const inputCreate = document.getElementById("Pictures");

        const input = inputNew ? inputNew : inputCreate;

        const button = document.getElementById("btnUpload");
        const fileName = document.getElementById("fileName");
        const preview = document.getElementById("previewImages");

        if (!input || !button || !fileName || !preview) return;

        const dataTransfer = new DataTransfer();

        button.addEventListener("click", () => {
            input.click();
        });

        input.addEventListener("change", () => {
            for (let file of input.files) {
                dataTransfer.items.add(file);

                const item = document.createElement("div");
                item.className = "col-6 col-md-2 text-center new-picture";

                const box = document.createElement("div");
                box.className = "border rounded p-2";

                const imgBox = document.createElement("div");
                imgBox.className = "img-box";

                const img = document.createElement("img");
                img.src = URL.createObjectURL(file);

                imgBox.appendChild(img);

                const removeDiv = document.createElement("div");
                removeDiv.className = "text-center";

                const removeLabel = document.createElement("label");
                removeLabel.className = "text-danger remove-new-picture";
                removeLabel.style.cursor = "pointer";
                removeLabel.textContent = "Xóa";

                removeDiv.appendChild(removeLabel);

                box.appendChild(imgBox);
                box.appendChild(removeDiv);
                item.appendChild(box);

                preview.appendChild(item);
            }

            input.files = dataTransfer.files;

            fileName.textContent = dataTransfer.files.length + " ảnh đã chọn";
        });


        // ===============================
        // Xóa ảnh mới
        // ===============================
        document.addEventListener("click", function (e) {
            if (!e.target.classList.contains("remove-new-picture")) return;

            const item = e.target.closest(".new-picture");

            if (!item) return;

            const index = [...preview.children].indexOf(item);

            item.remove();

            dataTransfer.items.remove(index);

            input.files = dataTransfer.files;

            fileName.textContent = dataTransfer.files.length + " ảnh đã chọn";
        });
    };

    return { init };
};

// ===============================
// Areas/Admin/Views/ProductReviews/Edit.cshtml
// ===============================
const ProductReviews = () => {
    const init = () => {
        const range = document.getElementById("ratingRange");
        if (!range) return;

        let currentValue = parseInt(range.value) || 5;

        renderStars(currentValue);
        updateText(currentValue);

        range.addEventListener("input", function () {
            currentValue = parseInt(this.value);
            renderStars(currentValue);
            updateText(currentValue);
        });
    };

    const renderStars = (val) => {
        const container = document.getElementById("ratingStars");
        if (!container) return;

        let html = "";
        for (let i = 1; i <= 5; i++) {
            html += `<i class="ti ${i <= val ? "ti-star-filled" : "ti-star"}"></i>`;
        }
        container.innerHTML = html;
    };

    const updateText = (val) => {
        const text = document.getElementById("ratingValue");
        if (text) text.innerText = val;
    };

    return { init };
};

// ===============================
// Areas/Admin/Views/Products/Index.cshtml
// ===============================
const ProductIndex = () => {

    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {

        jQuery(document).on('click', '.ajax-toggle-status', function () {

            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái kinh doanh của sản phẩm này?'
            ).then((result) => {

                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },

                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#product-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted');
                            statusCell.html(
                                '<span class="badge bg-danger">Ngừng kinh doanh</span>'
                            );
                            btn.removeClass('btn-light text-danger').addClass('btn-outline-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                        } else {
                            row.removeClass('bg-light text-muted');
                            statusCell.html(
                                '<span class="badge bg-success">Đang bán</span>'
                            );
                            btn.removeClass('btn-outline-success').addClass('btn-light text-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                        }

                        AlertHelper.success("Thay đổi thành công")
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

const ViolationReportIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-report-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Đã xóa" của báo cáo này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#report-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted');
                            statusCell.html('<span class="badge bg-danger">Đã xóa</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                        } else {
                            row.removeClass('bg-light text-muted');
                            statusCell.html('<span class="badge bg-success">Còn</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
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

const OrderIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-order-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Đã xóa" của đơn hàng này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#order-row-' + id);
                        const statusCell = jQuery('#soft-delete-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                        } else {
                            row.removeClass('bg-light text-muted');
                            statusCell.html('<span class="badge bg-success">Hiện</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
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

const UserIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-user-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Xóa mềm" của người dùng này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#user-row-' + id);
                        const statusCell = jQuery('#soft-delete-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã xóa mềm</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Khôi phục tài khoản');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hoạt động</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Xóa mềm (Vô hiệu hóa)');
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

const ProductReviewReplyIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-reply-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hiển thị/Ẩn" của phản hồi này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#reply-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện phản hồi');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hiển thị</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn phản hồi');
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

const ProductReviewIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-review-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hiển thị/Ẩn" của đánh giá này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#review-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Hiện đánh giá');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hiển thị</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn đánh giá');
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

const ArtistIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-artist-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hoạt động/Ẩn" của họa sĩ này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#artist-row-' + id);
                        const statusCell = jQuery('#status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-light text-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Khôi phục');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Hoạt động</span>');
                            btn.removeClass('btn-success').addClass('btn-light text-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Xóa mềm');
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

const InvoiceIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-invoice-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn thay đổi trạng thái "Hiển thị/Ẩn" của hóa đơn này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#invoice-row-' + id);
                        const statusCell = jQuery('#soft-delete-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger">Đã ẩn</span>');
                            btn.removeClass('btn-outline-danger').addClass('btn-success');
                            btn.html('<i class="ti ti-refresh"></i>');
                            btn.attr('title', 'Khôi phục hóa đơn');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success">Đang hiện</span>');
                            btn.removeClass('btn-success').addClass('btn-outline-danger');
                            btn.html('<i class="ti ti-ban"></i>');
                            btn.attr('title', 'Ẩn hóa đơn');
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

const NotificationIndex = () => {
    const init = () => {
        isDeleteButton();
    };

    const isDeleteButton = () => {
        jQuery(document).on('click', '.ajax-toggle-notification-status', function () {
            const btn = jQuery(this);
            const id = btn.data('id');
            const url = btn.data('url');

            AlertHelper.confirm(
                'Xác nhận thay đổi trạng thái?',
                'Bạn có chắc chắn muốn Thay đổi trạng thái Ẩn/Hiện của thông báo này?'
            ).then((result) => {
                if (!result.isConfirmed) return;

                jQuery.ajax({
                    url: url,
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (!response.success) return;

                        const isDeleted = response.isDeleted;
                        const row = jQuery('#notification-row-' + id);
                        const statusCell = jQuery('#soft-delete-status-cell-' + id);

                        if (isDeleted) {
                            row.addClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-danger-subtle text-danger border border-danger-subtle px-2"><i class="ti ti-eye-off me-1 small"></i> Ẩn</span>');
                            btn.removeClass('text-danger').addClass('text-success');
                            btn.html('<i class="ti ti-refresh fs-5"></i>');
                            btn.attr('title', 'Khôi phục');
                        } else {
                            row.removeClass('bg-light text-muted opacity-75');
                            statusCell.html('<span class="badge bg-success-subtle text-success border border-success-subtle px-2"><i class="ti ti-eye me-1 small"></i> Hiện</span>');
                            btn.removeClass('text-success').addClass('text-danger');
                            btn.html('<i class="ti ti-ban fs-5"></i>');
                            btn.attr('title', 'Xóa mềm');
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

const ReportsDashboard = () => {
    let salesChart = null;
    let statusChart = null;
    let fp = null;
    let currentStartDate = null;
    let currentEndDate = null;
    let apiUrls = null;

    const init = (urls) => {
        apiUrls = urls;

        console.info("Reports Dashboard Init", apiUrls);

        if (typeof flatpickr === 'undefined') return;

        // Khởi tạo Flatpickr
        const pickerEle = document.getElementById("dateRangePicker");
        if (!pickerEle) return;

        fp = flatpickr(pickerEle, {
            mode: "range",
            dateFormat: "Y-m-d",
            altInput: true,
            altFormat: "d/m/Y",
            locale: "vn",
            maxDate: "today",
            defaultDate: [
                new Date(new Date().setDate(new Date().getDate() - 6)),
                new Date()
            ],
            onClose: function (selectedDates) {
                if (selectedDates.length === 2) {
                    updateReports(selectedDates[0], selectedDates[1]);
                    const btn = document.getElementById('btnQuickFilter');
                    if (btn) btn.innerHTML = '<i class="ti ti-clock-hour-4 me-1"></i> Tùy chọn';
                }
            }
        });

        // Xử lý Quick Filters
        document.querySelectorAll('.quick-filter').forEach(item => {
            item.addEventListener('click', function (e) {
                e.preventDefault();
                const days = parseInt(this.getAttribute('data-days'));
                const filterText = this.innerText;

                const btn = document.getElementById('btnQuickFilter');
                if (btn) btn.innerHTML = '<i class="ti ti-clock-hour-4 me-1"></i> ' + filterText;

                const end = new Date();
                const start = new Date();
                start.setDate(end.getDate() - (days - 1));

                fp.setDate([start, end]);
                updateReports(start, end);
            });
        });

        // Load ban đầu mặc định (7 ngày qua)
        const end = new Date();
        const start = new Date();
        start.setDate(end.getDate() - 6); // 7 ngày bao gồm hôm nay

        // Cập nhật giao diện chọn nhanh
        const btnF = document.getElementById('btnQuickFilter');
        if (btnF) btnF.innerHTML = '<i class="ti ti-clock-hour-4 me-1"></i> 7 ngày qua';

        // Cập nhật bộ chọn ngày
        if (fp) fp.setDate([start, end]);

        // Tải dữ liệu
        updateReports(start, end);

        // Xử lý Xuất Excel
        const btnExport = document.getElementById('btnExport');
        if (btnExport) {
            btnExport.addEventListener('click', function () {
                if (currentStartDate && currentEndDate) {
                    window.location.href = `${apiUrls.export}?startDate=${currentStartDate}&endDate=${currentEndDate}`;
                } else {
                    if (typeof Swal !== 'undefined') {
                        Swal.fire('Chú ý', 'Vui lòng chọn khoảng thời gian để xuất báo cáo', 'warning');
                    } else {
                        alert('Vui lòng chọn khoảng thời gian để xuất báo cáo');
                    }
                }
            });
        }
    };

    async function updateReports(dStart, dEnd) {
        currentStartDate = dStart.toISOString().split('T')[0];
        currentEndDate = dEnd.toISOString().split('T')[0];

        const sStr = currentStartDate;
        const eStr = currentEndDate;

        const titleEle = document.getElementById('salesChartTitle');
        if (titleEle) {
            const sFormatted = sStr.split('-').reverse().join('/');
            const eFormatted = eStr.split('-').reverse().join('/');
            titleEle.innerText = `${sFormatted} - ${eFormatted}`;
        }

        toggleLoading(true);

        const query = `?startDate=${sStr}&endDate=${eStr}`;

        try {
            const [resSales, resStatus] = await Promise.all([
                fetch(apiUrls.sales + query).then(r => r.json()),
                fetch(apiUrls.status + query).then(r => r.json())
            ]);

            renderSalesChart(resSales);
            renderStatusChart(resStatus);
        } catch (error) {
            console.error("API Error:", error);
        } finally {
            toggleLoading(false);
        }
    }

    function toggleLoading(isLoading) {
        const sLoad = document.getElementById('salesLoading');
        const tLoad = document.getElementById('statusLoading');
        const display = isLoading ? 'flex' : 'none';
        if (sLoad) sLoad.style.display = display;
        if (tLoad) tLoad.style.display = display;
    }

    function renderSalesChart(data) {
        const ctx = document.getElementById('salesChart');
        if (!ctx || typeof Chart === 'undefined') return;

        // Tạo Gradient (Đổ màu bóng)
        const chartCtx = ctx.getContext('2d');
        const gradient = chartCtx.createLinearGradient(0, 0, 0, 350);
        gradient.addColorStop(0, 'rgba(78, 115, 223, 0.2)');
        gradient.addColorStop(1, 'rgba(78, 115, 223, 0)');

        // Luôn tìm và hủy biểu đồ cũ một cách triệt để
        const existing = Chart.getChart(ctx);
        if (existing) existing.destroy();

        salesChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.labels,
                datasets: [{
                    label: 'Doanh thu (₫)',
                    data: data.values,
                    fill: true,
                    backgroundColor: gradient, // Sử dụng gradient đã tạo
                    borderColor: '#4e73df',
                    borderWidth: 3,
                    pointBackgroundColor: '#fff',
                    pointBorderColor: '#4e73df',
                    pointBorderWidth: 2,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    pointHoverBackgroundColor: '#4e73df',
                    pointHoverBorderColor: '#fff',
                    pointHoverBorderWidth: 2,
                    tension: 0.4 // Đường cong mềm mại hơn
                }]
            },
            options: {
                maintainAspectRatio: false, // Rất quan trọng để biểu đồ thu nhỏ theo khung hình
                responsive: true,
                layout: {
                    padding: {
                        top: 5,
                        bottom: 15,
                        left: 10,
                        right: 0
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        min: 0,
                        suggestedMin: 0,
                        ticks: {
                            stepSize: 50000,
                            maxTicksLimit: 15,
                            font: {
                                size: 10
                            },
                            callback: function (value) {
                                if (value >= 1000000) return (value / 1000000).toFixed(1) + 'M';
                                return value.toLocaleString() + ' ₫';
                            }
                        }
                    },
                    x: {
                        ticks: {
                            maxRotation: 0,
                            minRotation: 0,
                            font: {
                                size: 11
                            },
                            padding: 3
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'index',
                    axis: 'x'
                },
                hover: {
                    animationDuration: 400
                },
                animations: {
                    radius: {
                        duration: 400,
                        easing: 'linear',
                        loop: (context) => context.active
                    }
                },
                plugins: {
                    tooltip: {
                        enabled: true,
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        padding: 12,
                        cornerRadius: 8,
                        titleFont: { size: 14, weight: 'bold' },
                        bodyFont: { size: 13 },
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                if (context.parsed.y !== null) {
                                    label += new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(context.parsed.y);
                                }
                                return label;
                            }
                        }
                    }
                }
            }
        });
    }

    function renderStatusChart(data) {
        const ctx = document.getElementById('orderStatusChart');
        const noData = document.getElementById('noDataAlert');
        if (!ctx || !noData || typeof Chart === 'undefined') return;

        // Luôn tìm và hủy biểu đồ cũ một cách triệt để
        const existing = Chart.getChart(ctx);
        if (existing) existing.destroy();

        if (!data || data.length === 0) {
            noData.classList.remove('d-none');
            ctx.style.display = 'none';
            return;
        }

        noData.classList.add('d-none');
        ctx.style.display = 'block';

        statusChart = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(x => x.status),
                datasets: [{
                    data: data.map(x => x.count),
                    backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796'],
                    borderWidth: 2
                }]
            },
            options: {
                maintainAspectRatio: false,
                responsive: true,
                plugins: {
                    legend: { position: 'bottom', labels: { boxWidth: 12, padding: 15 } }
                },
                cutout: '75%'
            }
        });
    }

    return { init };
};

const GlobalNotifications = () => {
    const init = () => {
        if (typeof Swal === "undefined" || !window.notifications) return;

        const { success, error, warning } = window.notifications;

        if (success) {
            Swal.fire({
                icon: 'success',
                title: 'Thành công!',
                text: success,
                timer: 3000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
        }

        if (error) {
            Swal.fire({
                icon: 'error',
                title: 'Lỗi!',
                text: error,
                toast: true,
                position: 'top-end',
                showConfirmButton: true
            });
        }

        if (warning) {
            Swal.fire({
                icon: 'warning',
                title: 'Cảnh báo!',
                text: warning,
                toast: true,
                position: 'top-end',
                showConfirmButton: true
            });
        }
    };
    return { init };
};

const AdminLayout = () => {
    const init = () => {
        handleCollapseIcon();
    };

    const handleCollapseIcon = () => {
        const menu = document.getElementById("systemMenu");
        if (!menu) return;

        const parent = menu.closest(".nav-item");
        const plusIcon = parent.querySelector(".icon-plus");
        const minusIcon = parent.querySelector(".icon-minus");

        function updateIcon() {
            if (plusIcon && minusIcon) {
                if (menu.classList.contains("show")) {
                    plusIcon.style.display = "none";
                    minusIcon.style.display = "inline-block";
                } else {
                    plusIcon.style.display = "inline-block";
                    minusIcon.style.display = "none";
                }
            }
        }

        menu.addEventListener("shown.bs.collapse", updateIcon);
        menu.addEventListener("hidden.bs.collapse", updateIcon);

        updateIcon();
    };

    return { init };
};

// ===============================
// DOM READY
// ===============================
const safeInit = (module) => {
    if (typeof module === "function") {
        module().init();
    }
};

const initAdmin = () => {

    if (typeof window.jQuery === "undefined") {
        console.error("jQuery chưa load");
        return;
    }

    console.log("admin.js loaded");

    safeInit(GlobalNotifications);
    safeInit(ProductIndex);
    safeInit(BadgeSelect);
    safeInit(ImagesUpload);
    safeInit(ProductReviews);
    safeInit(ViolationReportIndex);
    safeInit(OrderIndex);
    safeInit(UserIndex);
    safeInit(ProductReviewIndex);
    safeInit(ProductReviewReplyIndex);
    safeInit(TagIndex);
    safeInit(ArtistIndex);
    safeInit(InvoiceIndex);
    safeInit(NotificationIndex);
    safeInit(AdminLayout);

    // Reports Dashboard is manually initialized in View to pass URLs
    if (typeof window.initReports === 'function') {
        window.initReports();
    }
};

document.readyState !== "loading"
    ? initAdmin()
    : document.addEventListener("DOMContentLoaded", initAdmin);
// ===============================
// Areas/Admin/Views/Products/Edit.cshtml
// ===============================

// ===============================
// Badge module (Artist + Tag)
// ===============================
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

        const selects = $(".selectedartist, .selectedtag");

        if (!selects.length) return;

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

const images_edit = () => {
    const init = () => {
        initRemovePicture();
        initCustomFileInput();
    };

    // ===============================
    // Xóa ảnh cũ
    // ===============================
    const initRemovePicture = () => {
        document.addEventListener("click", function (e) {
            const btn = e.target.closest(".remove-picture");

            if (!btn) return;

            const box = btn.closest(".picture-item");

            if (!box) return;

            const input = box.querySelector(".deleted-picture");

            if (input) {
                input.disabled = false;   // bật để submit
            }

            box.style.display = "none";   // ẨN thay vì REMOVE
        });
    };

    // ===============================
    // Upload + preview ảnh mới
    // ===============================
    const initCustomFileInput = () => {
        const input = document.getElementById("NewPictures");
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
    };

    return { init };
};

// ===============================
// DOM READY
// ===============================
document.addEventListener("DOMContentLoaded", () => {

    console.log("admin.js loaded");

    BadgeSelect().init();
    images_edit().init();
});
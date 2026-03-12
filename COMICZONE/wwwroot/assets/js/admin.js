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


// ===============================
// DOM READY
// ===============================
document.addEventListener("DOMContentLoaded", () => {

    console.log("admin.js loaded");

    BadgeSelect().init();

});
// wwwroot/js/site.js

function openAvatarUpload() {
    document.getElementById("avatarUpload").click();
}

function uploadAvatar(input) {
    if (input.files.length === 0) return;

    const file = input.files[0];

    const allowedTypes = ["image/jpeg", "image/png", "image/webp"];
    const maxSize = 2 * 1024 * 1024;

    if (!allowedTypes.includes(file.type)) {
        Swal.fire({
            icon: 'warning',
            title: 'File không hợp lệ',
            text: 'Chỉ cho phép JPG, PNG hoặc WEBP'
        });

        return;
    }

    if (file.size > maxSize) {
        Swal.fire({
            icon: 'warning',
            title: 'Ảnh quá lớn',
            text: 'Avatar phải nhỏ hơn 2MB'
        });

        return;
    }

    // preview avatar
    const reader = new FileReader();

    reader.onload = function (e) {
        let avatar = document.querySelector(".profile-avatar img");

        if (!avatar) {
            const defaultAvatar = document.querySelector(".default-avatar");

            if (defaultAvatar) {
                avatar = document.createElement("img");
                avatar.className = "avatar-img";

                defaultAvatar.replaceWith(avatar);
            }
        }

        if (avatar) avatar.src = e.target.result;
    };

    reader.readAsDataURL(file);

    let formData = new FormData();
    formData.append("avatar", file);

    fetch("/UserProfiles/UploadAvatar", {
        method: "POST",
        body: formData
    })
        .then(res => {
            if (!res.ok) throw new Error("Server error");
            return res.json();
        })
        .then(data => {

            if (data.success) {
                Swal.fire({
                    icon: 'success',
                    title: 'Thành công',
                    text: 'Avatar đã được cập nhật',
                    timer: 1500,
                    showConfirmButton: false,
                    customClass: {
                        popup: 'premium-swal-popup',
                        title: 'premium-swal-title',
                        htmlContainer: 'premium-swal-html-container'
                    }
                }).then(() => location.reload());

            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: data.message,
                    customClass: {
                        popup: 'premium-swal-popup',
                        title: 'premium-swal-title',
                        htmlContainer: 'premium-swal-html-container',
                        confirmButton: 'premium-swal-confirm'
                    },
                    buttonsStyling: false
                });
            }
        })
        .catch(err => {
            console.error(err);

            Swal.fire({
                icon: 'error',
                title: 'Upload thất bại',
                text: 'Không thể tải avatar.'
            });
        });
}

// HIỂN THỊ POPUP YÊU CẦU ĐĂNG NHẬP VỚI SWEETALERT2
function showLoginRequired(message) {
    Swal.fire({
        icon: 'warning',
        title: 'Yêu cầu đăng nhập',
        text: message,
        confirmButtonText: 'Đăng nhập',
        cancelButtonText: 'Hủy',
        showCancelButton: true,
        reverseButtons: true,
        customClass: {
            popup: 'premium-swal-popup',
            title: 'premium-swal-title',
            htmlContainer: 'premium-swal-html-container',
            confirmButton: 'premium-swal-confirm',
            cancelButton: 'premium-swal-cancel'
        },
        buttonsStyling: false
    }).then((result) => {
        if (result.isConfirmed) {
            const currentUrl = window.location.pathname + window.location.search;
            window.location.href =
                `/Authentication/Login?returnUrl=${encodeURIComponent(currentUrl)}`;
        }
    });
}

///**
// * Kiểm tra trạng thái đăng nhập của một container bất kỳ
// * @param {string|jQuery} selector - selector CSS hoặc jQuery object
// * @returns {boolean} true nếu đăng nhập, false nếu không
// */
// CÁI NÀY DÙNG CHUNG CHO TOÀN BỘ REVIEW/REPLY (BẤM NÚT → CHECK LOGIN TRƯỚC → MỞ FORM) NÊN ĐỂ RA NGOÀI CHO DỄ QUẢN LÝ
function checkLoginStatus(selector) {
    let container = (selector instanceof jQuery ? selector : $(selector)).first();
    if (!container.length) return false;

    const loggedInAttr = container.attr('data-loggedin');
    return loggedInAttr === "true";
}

//TÓM TẮT SẢN PHẨM (XEM THÊM / ẨN BỚT)
const ProductSummary = (() => {
    const maxLength = 250; // số ký tự hiển thị ban đầu

    const init = () => {
        document.querySelectorAll('.summary-text').forEach(p => {
            const fullText = p.getAttribute('data-full');
            const btn = p.nextElementSibling;
            if (fullText.length > maxLength) {
                p.innerText = fullText.substring(0, maxLength) + '...';
                btn.innerText = 'Xem thêm';
            } else {
                p.innerText = fullText;
                btn.style.display = 'none'; // nếu nội dung ngắn thì không hiện nút
            }
        });
    };

    const toggle = (id) => {
        const p = document.getElementById(`desc-${id}`);
        const btn = p.nextElementSibling;
        const fullText = p.getAttribute('data-full');

        if (btn.innerText === 'Xem thêm') {
            p.innerText = fullText;
            btn.innerText = 'Ẩn bớt';
        } else {
            const shortText = fullText.length > maxLength ? fullText.substring(0, maxLength) + '...' : fullText;
            p.innerText = shortText;
            btn.innerText = 'Xem thêm';
        }
    };

    return { init, toggle };
})();

// QUẢN LÝ REVIEW SẢN PHẨM (LOAD, PAGINATION, LIKE/DISLIKE, EDIT, DELETE)
const ProductReviews = (() => {
    let productId;

    const init = (id) => {
        productId = id;
        loadReviewsFirstTime();
        initPagination();
        initReactionButtons();
        initEditReview();
        initDeleteReview();
        initDeleteReply();

        ProductReplies.initReplyButtons(); // khởi tạo reply form
        ProductReplies.initReplyReactionButtons(); // khởi tạo nút like/dislike cho reply
        ProductReplies.initReplyToReplyButtons(); // khởi tạo nút reply-to-reply
        ProductReplies.initEditReply(); // khởi tạo edit reply
    };

    const loadReviewsFirstTime = () => {
        fetch('/ProductReviews/Reviews?productId=' + productId)
            .then(response => response.text())
            .then(html => {
                const container = document.getElementById("review-container");
                container.innerHTML = html;

                initReactionButtons(); // gắn lại nút like
                // KHỞI TẠO hiển thị reply theo batch sau khi load xong
                ProductReplies.initReplyList();
            });
    };

    const initPagination = () => {
        $(document).on('click', '.review-page-link', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            $('#review-list-container')
                .load(`/ProductReviews/Reviews?productId=${productId}&page=${page}`, () => {
                    initReactionButtons();
                    initDeleteReview();
                    initDeleteReply();

                    ProductReplies.initReplyButtons();
                    ProductReplies.initReplyReactionButtons();
                    ProductReplies.initReplyList();
                    ProductReplies.initReplyToReplyButtons();
                    ProductReplies.initEditReply();
                });
        });
    };

    const initReactionButtons = () => {
        // Tránh bind trùng
        $(document).off('click', '.toggle-like');
        $(document).off('click', '.toggle-dislike');

        // =====================
        // LIKE
        // =====================
        $(document).on('click', '.toggle-like', function (e) {
            e.preventDefault();

            const likeBtn = $(this);
            const reviewId = likeBtn.data('review-id');
            const container = likeBtn.closest('.review-actions');
            const dislikeBtn = container.find('.toggle-dislike'); // nút dislike cùng review

            $.post('/ProductReviews/ToggleLike', { reviewId: reviewId }, function (res) {
                if (!res.success) return;

                // Cập nhật số lượng like
                likeBtn.find('.like-count').text(res.likeCount);

                if (res.isLiked) {
                    likeBtn.addClass('liked');          // bật màu like
                    dislikeBtn.removeClass('disliked'); // tắt màu dislike nếu đang bật
                } else {
                    likeBtn.removeClass('liked');       // tắt màu like
                }
            });
        });


        // =====================
        // DISLIKE
        // =====================
        $(document).on('click', '.toggle-dislike', function (e) {
            e.preventDefault();

            const dislikeBtn = $(this);
            const reviewId = dislikeBtn.data('review-id');
            const container = dislikeBtn.closest('.review-actions');
            const likeBtn = container.find('.toggle-like'); // nút like cùng review

            $.post('/ProductReviews/ToggleDislike', { reviewId: reviewId }, function (res) {
                if (!res.success) return;

                if (res.isDisliked) {
                    dislikeBtn.addClass('disliked'); // bật màu dislike
                    likeBtn.removeClass('liked');   // tắt màu like nếu đang bật
                } else {
                    dislikeBtn.removeClass('disliked'); // tắt màu dislike
                }

                // Cập nhật số like nếu server trả về
                if (res.likeCount !== undefined) {
                    likeBtn.find('.like-count').text(res.likeCount);
                }
            });
        });
    };

    const initEditReview = () => {
        $(document).on('submit', '#add-review-form', function (e) {
            const container_login = $(this).closest('[data-loggedin]'); // container chứa data-loggedin
            if (!checkLoginStatus(container_login)) {
                showLoginRequired('Bạn cần đăng nhập để gửi báo cáo.');
                return false; // Dừng xử lý nếu chưa đăng nhập
            }
        });

        // Mở form edit
        $(document).off('click', '.edit-review');
        $(document).on('click', '.edit-review', function (e) {
            e.preventDefault();
            const reviewId = $(this).data('id');

            $('#review-text-' + reviewId).hide();
            $('.edit-review-form[data-review-id="' + reviewId + '"]').removeClass('d-none');
        });

        // Hủy edit
        $(document).off('click', '.cancel-edit');
        $(document).on('click', '.cancel-edit', function () {
            const form = $(this).closest('.edit-review-form');
            form.addClass('d-none');
            form.siblings('.review-text').show();
        });

        // Submit edit
        $(document).off('submit', '.edit-review-form');
        $(document).on('submit', '.edit-review-form', function (e) {
            e.preventDefault();
            const form = $(this);
            const reviewId = form.find('input[name="Reviewid"]').val();
            const productId = form.find('input[name="Productid"]').val();
            const content = form.find('textarea[name="Reviewcontent"]').val().trim();

            if (!content) return;

            // serialize form sẽ tự gửi token + tất cả input
            const formData = form.serialize();

            $.ajax({
                url: '/ProductReviews/EditReview',
                type: 'POST',
                data: formData,
                success: function (res) {
                    if (res.success) {
                        $('#review-text-' + reviewId)
                            .text(content)
                            .show();

                        form.addClass('d-none');

                        // hiện label đã chỉnh sửa
                        $('#review-text-' + reviewId)
                            .closest('.review-card')
                            .find('.edit-label')
                            .removeClass('d-none');

                        // cập nhật thời gian nếu có
                        $('#review-text-' + reviewId)
                            .closest('.review-card')
                            .find('.review-date')
                            .text(res.updatedAt);

                    } else {
                        alert(res.message);
                    }
                },
                error: function () {
                    alert('Có lỗi xảy ra khi cập nhật đánh giá!');
                }
            });
        });
    };

    const initDeleteReview = () => {
        // tránh bind trùng
        $(document).off('click', '.delete-review');
        $(document).on('click', '.delete-review', function (e) {
            e.preventDefault();

            const reviewId = $(this).data('id');
            if (!reviewId) return;

            Swal.fire({
                title: 'Xác nhận xóa',
                text: "Bạn có chắc muốn xóa đánh giá này không?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy',
                reverseButtons: true,
                customClass: {
                    popup: 'premium-swal-popup',
                    title: 'premium-swal-title',
                    htmlContainer: 'premium-swal-html-container',
                    confirmButton: 'premium-swal-confirm',
                    cancelButton: 'premium-swal-cancel'
                },
                buttonsStyling: false
            }).then((result) => {
                if (!result.isConfirmed) return;

                const token = $('input[name="__RequestVerificationToken"]').first().val();

                $.ajax({
                    url: '/ProductReviews/DeleteReview',
                    type: 'POST',
                    data: {
                        __RequestVerificationToken: token,
                        id: reviewId
                    },
                    success: function (res) {
                        if (res.success) {
                            // Xóa khỏi DOM
                            const reviewCard = $('.review-card')
                                .find(`.delete-review[data-id="${reviewId}"]`)
                                .closest('.review-card');

                            reviewCard.fadeOut(300, function () {
                                $(this).remove();

                                // nếu không còn review
                                if ($('.review-card').length === 0) {
                                    $('.review-list-section')
                                        .append('<div class="no-review">Chưa có đánh giá nào cho sản phẩm này.</div>');
                                }
                            });

                            Swal.fire({
                                icon: 'success',
                                title: 'Đã xóa!',
                                text: 'Đánh giá đã được xóa.',
                                timer: 1500,
                                showConfirmButton: false,
                                customClass: {
                                    popup: 'premium-swal-popup',
                                    title: 'premium-swal-title',
                                    htmlContainer: 'premium-swal-html-container'
                                }
                            });

                        } else {
                            Swal.fire('Lỗi', res.message || 'Xóa thất bại', 'error');
                        }
                    },
                    error: function () {
                        Swal.fire('Lỗi', 'Có lỗi xảy ra.', 'error');
                    }
                });
            });
        });
    };

    // XÓA REPLY
    const initDeleteReply = () => {
        // Xóa các sự kiện cũ để tránh bind trùng
        $(document).off('click', '.delete-reply');

        // Bắt sự kiện click xóa reply
        $(document).on('click', '.delete-reply', function (e) {
            e.preventDefault();

            const replyId = $(this).data('id');
            if (!replyId) return;

            Swal.fire({
                title: 'Xác nhận xóa',
                text: "Bạn có chắc muốn xóa phản hồi này không?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy',
                reverseButtons: true,
                customClass: {
                    popup: 'premium-swal-popup',
                    title: 'premium-swal-title',
                    htmlContainer: 'premium-swal-html-container',
                    confirmButton: 'premium-swal-confirm',
                    cancelButton: 'premium-swal-cancel'
                },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    // Lấy token CSRF từ form (AntiForgeryToken)
                    const token = $('input[name="__RequestVerificationToken"]').first().val();

                    $.ajax({
                        url: '/ProductReviews/DeleteReply',
                        type: 'POST',
                        data: {
                            __RequestVerificationToken: token,
                            replyId: replyId
                        },
                        success: function (response) {
                            if (response.success) {
                                // Xóa reply khỏi DOM
                                $('.reply-item[data-reply-id="' + replyId + '"]').fadeOut(300, function () {
                                    $(this).remove();

                                    // Re-init lại toàn bộ reply list
                                    ProductReplies.initReplyList();
                                });

                                Swal.fire({
                                    icon: 'success',
                                    title: 'Đã xóa!',
                                    text: 'Phản hồi đã được xóa.',
                                    timer: 1500,
                                    showConfirmButton: false,
                                    customClass: {
                                        popup: 'premium-swal-popup',
                                        title: 'premium-swal-title',
                                        htmlContainer: 'premium-swal-html-container'
                                    }
                                });
                            } else {
                                Swal.fire('Lỗi', response.message || 'Xóa thất bại', 'error');
                            }
                        },
                        error: function () {
                            Swal.fire('Lỗi', 'Xóa thất bại do lỗi server', 'error');
                        }
                    });
                }
            });
        });
    };

    return { init };
})();

// XỬ LÝ PHẢN HỒI (REPLY) CHO REVIEW
const ProductReplies = (() => {
    const batchSize = 5; // số reply hiển thị mỗi lần, có thể thay đổi

    const initReplyButtons = () => {
        // Mở / ẩn khung reply
        $(document).off('click', '.toggle-reply');
        $(document).on('click', '.toggle-reply', function (e) {
            e.preventDefault();
            const reviewId = $(this).data('review-id');
            const container = $(`#reply-form-${reviewId}`);

            // Lấy container reply parent hoặc toàn bộ review container
            const container_login = $(this).closest('[data-loggedin]');
            if (!checkLoginStatus(container_login)) {
                showLoginRequired('Bạn cần đăng nhập nếu muốn gửi phản hồi.');
                return;
            }

            // Nếu có data-user (người được reply), set vào form
            const replyToUserId = $(this).data('reply-to-user-id');      // thêm attribute vào nút reply
            const replyToUsername = $(this).data('reply-to-username');    // thêm attribute vào nút reply

            const textarea = container.find('textarea');

            if (replyToUsername) {
                textarea.val(`@${replyToUsername} `); // tự động điền @username
            } else {
                textarea.val(''); // mặc định rỗng
            }

            container.find('form')
                .attr('data-reply-to-user-id', replyToUserId || '')
                .attr('data-reply-to-username', replyToUsername || '')
                .attr('data-parent-reply-id', '');

            // Đóng tất cả form khác trước
            //$('.reply-form-container').not(container).slideUp();

            // Chỉ mở form nếu đang bị ẩn
            if (!container.is(':visible')) {
                container.stop(true, true).slideDown();
            }
        });

        // Hủy reply
        $(document).off('click', '.cancel-reply');
        $(document).on('click', '.cancel-reply', function (e) {
            e.preventDefault();

            const formContainer = $(this).closest('.reply-form-container');

            // Nếu là form clone (reply-to-reply)
            if (formContainer.hasClass('clone-form')) {
                formContainer.slideUp(function () {
                    $(this).remove(); // xóa luôn khỏi DOM
                });
                return;
            }

            // Nếu là form gốc của review
            const reviewId = formContainer.data('review-id');

            formContainer.slideUp();

            const anchor = $(`#review-reply-anchor-${reviewId}`);
            anchor.after(formContainer);
        });

        // Submit reply
        $(document).off('submit', '.reply-form');
        $(document).on('submit', '.reply-form', function (e) {
            e.preventDefault();
            const form = $(this);
            const reviewId = form.data('review-id');
            const replyToUserId = form.data('reply-to-user-id');
            const parentReplyId = form.data('parent-reply-id');
            const content = form.find('textarea').val().trim();
            if (!content) return;

            $.ajax({
                url: '/ProductReviews/AddReply',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    ReviewId: reviewId,
                    Content: content,
                    ReplyToUserId: replyToUserId ? parseInt(replyToUserId) : null,
                    ParentReplyId: parentReplyId ? parseInt(parentReplyId) : null
                }),
                success: function (res) {
                    if (res.success) {
                        // Xóa nội dung và ẩn form
                        form.find('textarea').val('');
                        form.closest('.reply-form-container').slideUp();
                    } else {
                        alert('Gửi phản hồi thất bại!');
                    }
                },
                error: function () {
                    alert('Có lỗi xảy ra, vui lòng thử lại.');
                }
            });
        });
    };

    const initReplyReactionButtons = () => {
        // Xóa các handler cũ tránh bind trùng
        $(document).off('click', '.reply-toggle-like');
        $(document).off('click', '.reply-toggle-dislike');

        // LIKE
        $(document).on('click', '.reply-toggle-like', function (e) {
            e.preventDefault();

            const likeBtn = $(this);
            const replyId = likeBtn.data('reply-id');
            const container = likeBtn.closest('.reply-actions');
            const dislikeBtn = container.find('.reply-toggle-dislike');

            $.post('/ProductReviews/ToggleReplyLike', { replyId: replyId }, function (res) {
                if (!res.success) return;

                // Cập nhật số lượng like
                likeBtn.find('.like-count').text(res.likeCount);

                if (res.isLiked) {
                    likeBtn.addClass('liked');
                    dislikeBtn.removeClass('disliked');
                } else {
                    likeBtn.removeClass('liked');
                }
            });
        });

        // DISLIKE
        $(document).on('click', '.reply-toggle-dislike', function (e) {
            e.preventDefault();

            const dislikeBtn = $(this);
            const replyId = dislikeBtn.data('reply-id');
            const container = dislikeBtn.closest('.reply-actions');
            const likeBtn = container.find('.reply-toggle-like');

            $.post('/ProductReviews/ToggleReplyDislike', { replyId: replyId }, function (res) {
                if (!res.success) return;

                likeBtn.find('.like-count').text(res.likeCount);

                if (res.isDisliked) {
                    dislikeBtn.addClass('disliked');
                    likeBtn.removeClass('liked');
                } else {
                    dislikeBtn.removeClass('disliked');
                }
            });
        });
    };

    const initReplyList = () => {
        document.querySelectorAll('.reply-list-container')
            .forEach(container => {
                const replies = Array.from(container.querySelectorAll('.reply-item'));
                const showBtn = container.querySelector('.show-replies-btn');

                // Nếu không còn reply → xóa luôn nút
                if (replies.length === 0) {
                    if (showBtn) showBtn.remove();
                    return;
                }

                if (!showBtn) return;

                const total = replies.length;
                const batch = parseInt(showBtn.dataset.batchSize) || batchSize;

                let shownCount = 0;
                let expanded = false;

                // reset display
                replies.forEach(r => r.style.display = 'none');
                showBtn.innerText = `${total} phản hồi`;

                // clone button để xóa event cũ
                const newBtn = showBtn.cloneNode(true);
                showBtn.parentNode.replaceChild(newBtn, showBtn);

                newBtn.addEventListener('click', () => {
                    const currentReplies = Array.from(container.querySelectorAll('.reply-item'));
                    const currentTotal = currentReplies.length;

                    // Nếu tất cả reply đã bị xóa
                    if (currentTotal === 0) {
                        newBtn.remove();
                        return;
                    }

                    if (expanded) {
                        currentReplies.forEach(r => r.style.display = 'none');
                        shownCount = 0;
                        expanded = false;
                        newBtn.innerText = `${currentTotal} phản hồi`;
                        // Đưa nút xuống cuối danh sách
                        container.appendChild(newBtn);
                        return;
                    }

                    const remaining = currentTotal - shownCount;
                    const toShow = Math.min(batch, remaining);

                    for (let i = shownCount; i < shownCount + toShow; i++) {
                        if (currentReplies[i])
                            currentReplies[i].style.display = 'block';
                    }

                    // luôn đưa nút xuống cuối
                    container.appendChild(newBtn);

                    shownCount += toShow;

                    const left = currentTotal - shownCount;

                    if (left > 0) {
                        newBtn.innerText = `${left} phản hồi còn lại`;
                    } else {
                        newBtn.innerText = 'Ẩn phản hồi';
                        expanded = true;
                    }
                });
            });
    };

    const initReplyToReplyButtons = () => {
        $(document).off('click', '.reply-to-reply');

        $(document).on('click', '.reply-to-reply', function (e) {
            e.preventDefault();

            const reviewId = $(this).data('review-id');
            const replyId = $(this).data('reply-id');
            const replyToUserId = $(this).data('reply-to-user-id');
            const replyToUsername = $(this).data('reply-to-username');

            const originalForm = $(`#reply-form-${reviewId}`);

            // Lấy container reply parent hoặc toàn bộ review container
            const container_login = $(this).closest('[data-loggedin]');
            if (!checkLoginStatus(container_login)) {
                showLoginRequired('Bạn cần đăng nhập nếu muốn gửi phản hồi.');
                return;
            }

            const currentReplyItem = $(this).closest('.reply-item');

            // KIỂM TRA đã có form trong reply này chưa
            if (currentReplyItem.find('.reply-form-container.clone-form').length > 0) {
                return; // đã có form rồi → không tạo thêm
            }

            // Clone form
            const clonedForm = originalForm.clone(true);

            clonedForm
                .removeAttr('id') // tránh trùng id
                .addClass('clone-form'); // đánh dấu là form clone

            // Reset nội dung
            clonedForm.find('textarea')
                .val(replyToUsername ? `@${replyToUsername} ` : '');

            // Set lại data
            clonedForm.find('form')
                .attr('data-reply-to-user-id', replyToUserId || '')
                .attr('data-parent-reply-id', replyId || '')
                .attr('data-reply-to-username', replyToUsername || '');

            // Thêm xuống reply
            currentReplyItem.append(clonedForm);

            clonedForm.hide().slideDown();

            const textareaEl = clonedForm.find('textarea').get(0);
            if (textareaEl) {
                textareaEl.focus();
                textareaEl.setSelectionRange(
                    textareaEl.value.length,
                    textareaEl.value.length
                );
            }
        });
    };

    const initEditReply = () => {
        // click vào nút edit reply
        $(document).on("click", ".edit-reply", function (e) {
            e.preventDefault();

            const replyItem = $(this).closest(".reply-item");
            const contentDiv = replyItem.find(".reply-content");
            const editBox = replyItem.find(".reply-edit-container");
            const textarea = replyItem.find(".edit-reply-text");

            textarea.val(contentDiv.text().trim());

            contentDiv.hide();
            editBox.show();
        });

        //Nút HỦY
        $(document).on("click", ".cancel-edit", function () {
            const replyItem = $(this).closest(".reply-item");

            replyItem.find(".reply-edit-container").hide();
            replyItem.find(".reply-content").show();
        });

        //Nút LƯU (AJAX)
        $(document).on("click", ".save-edit", function () {
            const replyItem = $(this).closest(".reply-item");
            const replyId = replyItem.data("reply-id");
            const newContent = replyItem.find(".edit-reply-text").val();

            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: '/ProductReviews/EditReply',
                type: 'POST',
                data: {
                    __RequestVerificationToken: token, // 🔥 gửi đúng cách
                    replyId: replyId,
                    content: newContent
                },
                success: function (res) {
                    if (res.success) {
                        // cập nhật nội dung
                        replyItem.find(".reply-content")
                            .text(newContent)
                            .show();

                        replyItem.find(".reply-edit-container").hide();

                        // thêm "(đã chỉnh sửa)" nếu chưa có
                        if (replyItem.find(".edited-label").length === 0) {
                            replyItem.find(".reply-username strong")
                                .append(' <span class="edited-label text-muted ms-1">(đã chỉnh sửa)</span>');
                        }

                        // cập nhật thời gian
                        replyItem.find(".reply-date").text(res.updatedAt);
                    } else {
                        if (res.message === "Bạn cần đăng nhập.") {
                            showLoginRequired(res.message);
                            return;
                        }

                        alert(res.message);
                    }
                },
                error: function () {
                    alert("Có lỗi xảy ra.");
                }
            });
        });
    };

    return {
        initReplyButtons,
        initReplyReactionButtons,
        initReplyList,
        initReplyToReplyButtons,
        initEditReply
    };
})();

//BÁO CÁO REVIEW / REPLY (BẤM NÚT → HIỆN MODAL ĐIỀN LÝ DO → GỬI AJAX) - CÓ CHECK LOGIN TRƯỚC KHI MỞ MODAL
const ProductReport = (() => {
    const init = () => {
        // ================= CLICK REPORT =================
        $(document).off('click', '.report-review');
        $(document).on('click', '.report-review', function (e) {
            e.preventDefault();

            // CHECK LOGIN TRƯỚC
            // Tìm container chứa thông tin login (có thể là card review hoặc container list)
            const containerLogin = $(this).closest('[data-loggedin]');
            const isLoggedIn = containerLogin.length ? (containerLogin.attr('data-loggedin') === 'true') : false;

            if (!isLoggedIn) {
                showLoginRequired('Bạn cần đăng nhập để báo cáo vi phạm.');
                return;
            }

            const reviewIdRaw = $(this).attr('data-review-id');
            const replyIdRaw = $(this).attr('data-reply-id');

            const reviewId = reviewIdRaw ? parseInt(reviewIdRaw) : null;
            const replyId = replyIdRaw ? parseInt(replyIdRaw) : null;

            const modalElement = document.getElementById('reportModal');

            if (modalElement) {
                const form = modalElement.querySelector('#reportForm');
                if (form) form.reset();

                // Gán ID vào các input ẩn
                const reviewInput = modalElement.querySelector('input[name="ReviewId"]');
                const replyInput = modalElement.querySelector('input[name="ReplyId"]');
                
                if (reviewInput) reviewInput.value = reviewId ?? '';
                if (replyInput) replyInput.value = replyId ?? '';

                // Hiển thị Modal theo chuẩn Bootstrap 5
                let bsModal = bootstrap.Modal.getInstance(modalElement);
                if (!bsModal) {
                    bsModal = new bootstrap.Modal(modalElement);
                }
                bsModal.show();
            } else {
                console.error("Không tìm thấy #reportModal trong DOM.");
            }
        });

        // ================= SUBMIT REPORT =================
        $(document).off('submit', '#reportForm');
        $(document).on('submit', '#reportForm', function (e) {
            e.preventDefault();

            const form = $(this);
            const submitBtn = form.find('button[type="submit"]');
            const originalText = submitBtn.text();

            submitBtn.prop('disabled', true).text('Đang gửi...');

            const formData = form.serialize();

            $.ajax({
                url: '/ProductReviews/Report',
                type: 'POST',
                data: formData,
                success: function (res) {
                    submitBtn.prop('disabled', false).text(originalText);

                    if (res.success) {
                        const reviewId = form.find('input[name="ReviewId"]').val();
                        const replyId = form.find('input[name="ReplyId"]').val();

                        const modalElement = document.getElementById('reportModal');
                        const bsModal = bootstrap.Modal.getInstance(modalElement);
                        if (bsModal) {
                            bsModal.hide();
                        }

                        // UPDATE UI NGAY LẬP TỨC
                        const selector = replyId
                            ? `.report-review[data-reply-id="${replyId}"]`
                            : `.report-review[data-review-id="${reviewId}"]`;

                        const reportItem = $(selector);

                        if (reportItem.length) {
                            reportItem.replaceWith(`
                                <span class="dropdown-item text-muted small">
                                    Bạn đã gửi báo cáo (đang chờ xử lý)
                                </span>
                            `);
                        }

                        Swal.fire({
                            icon: 'success',
                            title: 'Thành công',
                            text: 'Báo cáo đã được gửi.',
                            timer: 1500,
                            showConfirmButton: false,
                            customClass: {
                                popup: 'premium-swal-popup',
                                title: 'premium-swal-title',
                                htmlContainer: 'premium-swal-html-container'
                            }
                        });
                    }
                    else {
                        // Nếu chưa login → hiện popup login
                        if (res.message === "Bạn cần đăng nhập.") {
                            showLoginRequired(res.message);
                            return;
                        }

                        Swal.fire({
                            icon: 'warning',
                            title: 'Thông báo',
                            text: res.message
                        });
                    }
                },
                error: function (err) {
                    submitBtn.prop('disabled', false).text(originalText);
                    console.log(err);
                    Swal.fire({
                        icon: 'error',
                        title: 'Lỗi',
                        text: 'Có lỗi xảy ra khi gửi báo cáo.'
                    });
                }
            });
        });
    };

    return { init };
})();

// MENU BA CHẤM CHO REVIEW CARD - USER PROFILE
const ReviewMenu_UserProfile = (() => {
    const init = () => {
        // MENU BA CHẤM
        $(document).off('click', '.menu-btn');

        $(document).on('click', '.menu-btn', function (e) {

            e.stopPropagation();

            const wrapper = $(this).closest('.menu-wrapper');

            $('.menu-wrapper').not(wrapper).removeClass('active');

            wrapper.toggleClass('active');
        });

        // click ngoài → đóng menu
        $(document).on('click', function () {
            $('.menu-wrapper').removeClass('active');
        });

        // DELETE REVIEW
        let reviewIdToDelete = null;

        $(document).off("click", ".delete-review");
        // click nút xóa
        $(document).on("click", ".delete-review", function (e) {
            e.preventDefault();

            reviewIdToDelete = $(this).data("id");

            $("#deleteConfirmModal").addClass("active");

        });

        // hủy
        $(document).on("click", "#deleteConfirmModal .btn-cancel", function () {
            $("#deleteConfirmModal").removeClass("active");
        });

        // xác nhận xóa
        $(document).on("click", "#deleteConfirmModal .btn-delete", function () {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: "/ProductReviews/DeleteReview",
                type: "POST",
                data: {
                    id: reviewIdToDelete,
                    __RequestVerificationToken: token
                },
                success: function (res) {
                    if (res.success) {
                        location.reload();
                    } else {
                        alert(res.message || "Không thể xóa đánh giá");
                    }
                }
            });
        });
    };

    return { init };
})();

const ReplyMenu_UserProfile = (() => {
    let replyIdToDelete = null;

    const init = () => {
        // CLICK DELETE
        $(document).on("click", ".delete-reply", function (e) {
            e.preventDefault();

            replyIdToDelete = $(this).data("id");

            $("#deleteReplyConfirmModal").addClass("active");

        });

        // CANCEL
        $(document).on("click", "#deleteReplyConfirmModal .btn-cancel", function () {
            $("#deleteReplyConfirmModal").removeClass("active");
            replyIdToDelete = null;
        });

        // CONFIRM DELETE
        $(document).on("click", "#deleteReplyConfirmModal .btn-delete", function () {
            const token = $('input[name="__RequestVerificationToken"]').val();

            $.ajax({
                url: "/ProductReviews/DeleteReply",
                type: "POST",
                data: {
                    replyId: replyIdToDelete,
                    __RequestVerificationToken: token
                },
                success: function (res) {
                    if (res.success) {
                        location.href = window.location.pathname + "?tab=replies";
                    } else {
                        alert(res.message || "Không thể xóa phản hồi");
                    }
                },
                error: function () {
                    alert("Lỗi server");
                }
            });
        });
    };

    return { init };
})();

const UrlTabs = {
    init() {
        const params = new URLSearchParams(window.location.search);
        const tab = params.get("tab");

        if (!tab) return;

        const trigger = document.querySelector(`[data-bs-target="#${tab}"]`);

        if (trigger) {
            new bootstrap.Tab(trigger).show();
        }
    }
};

const AutoAlert = (() => {
    const init = () => {
        if (window.successMessage) {
            const toast = document.createElement("div");
            toast.className = "toast-notification";
            toast.innerText = window.successMessage;

            document.body.appendChild(toast);

            setTimeout(() => {
                toast.classList.add("toast-show");
            }, 100);

            setTimeout(() => {
                toast.style.opacity = "0";
                setTimeout(() => toast.remove(), 400);
            }, 3000);
        }
    };

    return { init };
})();

// =============================
// CART QUANTITY UPDATE (AJAX)
// =============================
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

// =============================
// _Layout.cshtml(chatbot ai)
// =============================
const ChatbotAI = (() => {

    // Thêm message vào UI (Modularized based on user request)
    const addMessage = (text, sender) => {
        const messagesContainer = document.getElementById('chatbot-messages');
        if (!messagesContainer) return;

        const line = document.createElement('div');
        line.className = `message ${sender}-message`;

        const senderName = sender === 'bot' ? 'Trợ lý' : 'Bạn';
        let avatarHtml = '';
        let senderNameHtml = '';

        if (sender === 'bot') {
            avatarHtml = `<div class="bot-avatar-small"><i class="bi bi-robot"></i></div>`;
            senderNameHtml = `<span class="sender-name">Trợ lý</span>`;
        }

        // Convert basic Markdown (**bold**, *italic*) to HTML
        let htmlText = text
            .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
            .replace(/\*(.*?)\*/g, '<em>$1</em>')
            .replace(/\n/g, '<br/>');

        line.innerHTML = `
            ${avatarHtml}
            <div class="msg-wrapper">
                ${sender === 'bot' ? senderNameHtml : ''}
                <div class="bubble">
                    ${htmlText}
                </div>
            </div>
        `;

        messagesContainer.appendChild(line);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    };

    const restoreHistory = () => {
        const history = JSON.parse(localStorage.getItem('chatHistory') || '[]');
        history.forEach(msg => {
            addMessage(msg.text, msg.sender);
        });
    };

    const chatbot = () => {
        const icon = document.getElementById("chatbot-icon");
        const windowChat = document.getElementById("chatbot-window");
        const closeBtn = document.getElementById("close-chat");
        const input = document.getElementById("chatbot-input");
        const sendBtn = document.getElementById("chatbot-send");
        const messages = document.getElementById("chatbot-messages");

        if (!icon || !windowChat || !input || !messages) return;

        const saveMessage = (text, sender) => {
            const history = JSON.parse(localStorage.getItem('chatHistory') || '[]');
            history.push({ text, sender });
            if (history.length > 50) history.shift();
            localStorage.setItem('chatHistory', JSON.stringify(history));
        };

        const toggleChat = (e) => {
            if (e) e.stopPropagation();
            const isVisible = window.getComputedStyle(windowChat).display === "flex";
            if (isVisible) {
                windowChat.style.display = "none";
                icon.innerHTML = '<i class="bi bi-chat-dots-fill"></i>';
            } else {
                windowChat.style.display = "flex";
                icon.innerHTML = '<i class="bi bi-x-lg"></i>';
                input.focus();
                messages.scrollTop = messages.scrollHeight;
            }
        };

        icon.onclick = toggleChat;

        if (closeBtn) {
            closeBtn.onclick = (e) => {
                e.stopPropagation();
                windowChat.style.display = "none";
                icon.innerHTML = '<i class="bi bi-chat-dots-fill"></i>';
            };
        }

        const sendMessage = async () => {
            const text = input.value.trim();
            if (!text) return;

            addMessage(text, 'user');
            saveMessage(text, 'user');
            input.value = "";

            // Modern Typing Indicator (3 Dots)
            const typing = document.createElement("div");
            typing.className = "message bot-message typing-wrapper";
            typing.innerHTML = `
                <div class="bot-avatar-small"><i class="bi bi-robot"></i></div>
                <div class="msg-wrapper">
                    <span class="sender-name">Trợ lý</span>
                    <div class="bubble typing-bubble">
                        <div class="typing-dots">
                            <span></span>
                            <span></span>
                            <span></span>
                        </div>
                    </div>
                </div>
            `;
            messages.appendChild(typing);
            messages.scrollTop = messages.scrollHeight;

            try {
                const res = await fetch("/Chatbot/SendMessage", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ message: text })
                });
                const data = await res.json();
                typing.remove();
                addMessage(data.reply, 'bot');
                saveMessage(data.reply, 'bot');
            } catch {
                typing.remove();
                addMessage('Hệ thống bận, thử lại sau.', 'bot');
            }
        };

        if (sendBtn) sendBtn.onclick = sendMessage;
        if (input) {
            input.onkeypress = (e) => {
                if (e.key === 'Enter') sendMessage();
            };
        }
    };

    const init = () => {
        chatbot();
        restoreHistory();
    };

    return { init };
})();

// XỬ LÝ SẮP XẾP SẢN PHẨM (CUSTOM DROPDOWN)
const ProductSort = (() => {
    const init = () => {
        const sortWrapper = document.getElementById('sortWrapper');
        const sortTrigger = document.getElementById('sortTrigger');
        const sortOptions = document.querySelectorAll('.sort-option-item');
        const sortInput = document.getElementById('currentSortOrder');
        const sortForm = document.getElementById('sortForm');

        if (!sortTrigger || !sortWrapper) return;

        sortTrigger.addEventListener('click', (e) => {
            e.stopPropagation();
            sortWrapper.classList.toggle('active');
        });

        sortOptions.forEach(option => {
            option.addEventListener('click', () => {
                const val = option.getAttribute('data-value');
                if (sortInput) sortInput.value = val;
                if (sortForm) sortForm.submit();
            });
        });

        document.addEventListener('click', (e) => {
            if (!sortWrapper.contains(e.target)) {
                sortWrapper.classList.remove('active');
            }
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

document.addEventListener('DOMContentLoaded', () => {
    ProductReport.init();
    ProductSummary.init();
    ReviewMenu_UserProfile.init();
    ReplyMenu_UserProfile.init();
    UrlTabs.init();
    AutoAlert.init();
    CartQuantity.init();
    ChatbotAI.init();
    ProductSort.init();

    // Truyền productId từ Razor view
    const productIdElement = document.getElementById('product-id');

    if (productIdElement) {
        const id = parseInt(productIdElement.value);

        if (!isNaN(id)) {
            ProductReviews.init(id);
        }
    }

    CheckoutPayment.init();

    // ===== LOGIN REQUIRED POPUP =====
    if (window.loginRequiredMessage) {
        showLoginRequired(window.loginRequiredMessage);
    }
});
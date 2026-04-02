// product-reviews.js - Quản lý đánh giá, phản hồi và báo cáo vi phạm
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
                if (container) {
                    container.innerHTML = html;
                    initReactionButtons(); // gắn lại nút like
                    ProductReplies.initReplyList();
                }
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
        $(document).off('click', '.toggle-like');
        $(document).off('click', '.toggle-dislike');

        $(document).on('click', '.toggle-like', function (e) {
            e.preventDefault();
            const likeBtn = $(this);
            const reviewId = likeBtn.data('review-id');
            const container = likeBtn.closest('.review-actions');
            const dislikeBtn = container.find('.toggle-dislike');

            $.post('/ProductReviews/ToggleLike', { reviewId: reviewId }, function (res) {
                if (!res.success) return;
                likeBtn.find('.like-count').text(res.likeCount);
                if (res.isLiked) {
                    likeBtn.addClass('liked');
                    dislikeBtn.removeClass('disliked');
                } else {
                    likeBtn.removeClass('liked');
                }
            });
        });

        $(document).on('click', '.toggle-dislike', function (e) {
            e.preventDefault();
            const dislikeBtn = $(this);
            const reviewId = dislikeBtn.data('review-id');
            const container = dislikeBtn.closest('.review-actions');
            const likeBtn = container.find('.toggle-like');

            $.post('/ProductReviews/ToggleDislike', { reviewId: reviewId }, function (res) {
                if (!res.success) return;
                if (res.isDisliked) {
                    dislikeBtn.addClass('disliked');
                    likeBtn.removeClass('liked');
                } else {
                    dislikeBtn.removeClass('disliked');
                }
                if (res.likeCount !== undefined) {
                    likeBtn.find('.like-count').text(res.likeCount);
                }
            });
        });
    };

    const initEditReview = () => {
        $(document).on('submit', '#add-review-form', function (e) {
            const container_login = $(this).closest('[data-loggedin]');
            if (!checkLoginStatus(container_login)) {
                showLoginRequired('Bạn cần đăng nhập để gửi báo cáo.');
                return false;
            }
        });

        $(document).off('click', '.edit-review');
        $(document).on('click', '.edit-review', function (e) {
            e.preventDefault();
            const reviewId = $(this).data('id');
            $('#review-text-' + reviewId).hide();
            $('.edit-review-form[data-review-id="' + reviewId + '"]').removeClass('d-none');
        });

        $(document).off('click', '.cancel-edit');
        $(document).on('click', '.cancel-edit', function () {
            const form = $(this).closest('.edit-review-form');
            form.addClass('d-none');
            form.siblings('.review-text').show();
        });

        $(document).off('submit', '.edit-review-form');
        $(document).on('submit', '.edit-review-form', function (e) {
            e.preventDefault();
            const form = $(this);
            const reviewId = form.find('input[name="Reviewid"]').val();
            const content = form.find('textarea[name="Reviewcontent"]').val().trim();
            if (!content) return;

            const formData = form.serialize();
            $.ajax({
                url: '/ProductReviews/EditReview',
                type: 'POST',
                data: formData,
                success: function (res) {
                    if (res.success) {
                        $('#review-text-' + reviewId).text(content).show();
                        form.addClass('d-none');
                        $('#review-text-' + reviewId).closest('.review-card').find('.edit-label').removeClass('d-none');
                        $('#review-text-' + reviewId).closest('.review-card').find('.review-date').text(res.updatedAt);
                    } else {
                        alert(res.message);
                    }
                }
            });
        });
    };

    const initDeleteReview = () => {
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
                    data: { __RequestVerificationToken: token, id: reviewId },
                    success: function (res) {
                        if (res.success) {
                            const reviewCard = $('.review-card').find(`.delete-review[data-id="${reviewId}"]`).closest('.review-card');
                            reviewCard.fadeOut(300, function () {
                                $(this).remove();
                                if ($('.review-card').length === 0) {
                                    $('.review-list-section').append('<div class="no-review">Chưa có đánh giá nào cho sản phẩm này.</div>');
                                }
                            });
                            Swal.fire({ icon: 'success', title: 'Đã xóa!', timer: 1500, showConfirmButton: false });
                        }
                    }
                });
            });
        });
    };

    const initDeleteReply = () => {
        $(document).off('click', '.delete-reply');
        $(document).on('click', '.delete-reply', function (e) {
            e.preventDefault();
            const replyId = $(this).data('id');
            if (!replyId) return;

            Swal.fire({
                title: 'Xác nhận xóa', text: "Bạn có chắc muốn xóa phản hồi này không?", icon: 'warning',
                showCancelButton: true, confirmButtonText: 'Xóa', cancelButtonText: 'Hủy', reverseButtons: true,
                customClass: { popup: 'premium-swal-popup', confirmButton: 'premium-swal-confirm', cancelButton: 'premium-swal-cancel' },
                buttonsStyling: false
            }).then((result) => {
                if (result.isConfirmed) {
                    const token = $('input[name="__RequestVerificationToken"]').first().val();
                    $.ajax({
                        url: '/ProductReviews/DeleteReply', type: 'POST',
                        data: { __RequestVerificationToken: token, replyId: replyId },
                        success: function (response) {
                            if (response.success) {
                                $('.reply-item[data-reply-id="' + replyId + '"]').fadeOut(300, function () {
                                    $(this).remove();
                                    ProductReplies.initReplyList();
                                });
                                Swal.fire({ icon: 'success', title: 'Đã xóa!', timer: 1500, showConfirmButton: false });
                            }
                        }
                    });
                }
            });
        });
    };

    return { init };
})();

// XỬ LÝ PHẢN HỒI (REPLY)
const ProductReplies = (() => {
    const batchSize = 5;

    const initReplyButtons = () => {
        $(document).off('click', '.toggle-reply');
        $(document).on('click', '.toggle-reply', function (e) {
            e.preventDefault();
            const reviewId = $(this).data('review-id');
            const container = $(`#reply-form-${reviewId}`);
            const container_login = $(this).closest('[data-loggedin]');
            if (!checkLoginStatus(container_login)) {
                showLoginRequired('Bạn cần đăng nhập nếu muốn gửi phản hồi.');
                return;
            }

            const replyToUserId = $(this).data('reply-to-user-id');
            const replyToUsername = $(this).data('reply-to-username');
            const textarea = container.find('textarea');

            if (replyToUsername) textarea.val(`@${replyToUsername} `);
            else textarea.val('');

            container.find('form').attr('data-reply-to-user-id', replyToUserId || '').attr('data-reply-to-username', replyToUsername || '').attr('data-parent-reply-id', '');
            if (!container.is(':visible')) container.stop(true, true).slideDown();
        });

        $(document).off('click', '.cancel-reply');
        $(document).on('click', '.cancel-reply', function (e) {
            e.preventDefault();
            const formContainer = $(this).closest('.reply-form-container');
            if (formContainer.hasClass('clone-form')) {
                formContainer.slideUp(function () { $(this).remove(); });
                return;
            }
            formContainer.slideUp();
        });

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
                url: '/ProductReviews/AddReply', method: 'POST', contentType: 'application/json',
                data: JSON.stringify({ ReviewId: reviewId, Content: content, ReplyToUserId: replyToUserId ? parseInt(replyToUserId) : null, ParentReplyId: parentReplyId ? parseInt(parentReplyId) : null }),
                success: function (res) {
                    if (res.success) { form.find('textarea').val(''); form.closest('.reply-form-container').slideUp(); }
                }
            });
        });
    };

    const initReplyReactionButtons = () => {
        $(document).off('click', '.reply-toggle-like');
        $(document).off('click', '.reply-toggle-dislike');

        $(document).on('click', '.reply-toggle-like', function (e) {
            e.preventDefault();
            const likeBtn = $(this);
            const replyId = likeBtn.data('reply-id');
            const container = likeBtn.closest('.reply-actions');
            const dislikeBtn = container.find('.reply-toggle-dislike');
            $.post('/ProductReviews/ToggleReplyLike', { replyId: replyId }, function (res) {
                if (res.success) { likeBtn.find('.like-count').text(res.likeCount); if (res.isLiked) { likeBtn.addClass('liked'); dislikeBtn.removeClass('disliked'); } else likeBtn.removeClass('liked'); }
            });
        });

        $(document).on('click', '.reply-toggle-dislike', function (e) {
            e.preventDefault();
            const dislikeBtn = $(this);
            const replyId = dislikeBtn.data('reply-id');
            const container = dislikeBtn.closest('.reply-actions');
            const likeBtn = container.find('.reply-toggle-like');
            $.post('/ProductReviews/ToggleReplyDislike', { replyId: replyId }, function (res) {
                if (res.success) { likeBtn.find('.like-count').text(res.likeCount); if (res.isDisliked) { dislikeBtn.addClass('disliked'); likeBtn.removeClass('liked'); } else dislikeBtn.removeClass('disliked'); }
            });
        });
    };

    const initReplyList = () => {
        document.querySelectorAll('.reply-list-container').forEach(container => {
            const replies = Array.from(container.querySelectorAll('.reply-item'));
            const showBtn = container.querySelector('.show-replies-btn');
            if (replies.length === 0) { if (showBtn) showBtn.remove(); return; }
            if (!showBtn) return;
            const total = replies.length;
            const batch = parseInt(showBtn.dataset.batchSize) || batchSize;
            let shownCount = 0; let expanded = false;
            replies.forEach(r => r.style.display = 'none');
            showBtn.innerText = `${total} phản hồi`;
            const newBtn = showBtn.cloneNode(true);
            showBtn.parentNode.replaceChild(newBtn, showBtn);
            newBtn.addEventListener('click', () => {
                const currentReplies = Array.from(container.querySelectorAll('.reply-item'));
                const currentTotal = currentReplies.length;
                if (currentTotal === 0) { newBtn.remove(); return; }
                if (expanded) { currentReplies.forEach(r => r.style.display = 'none'); shownCount = 0; expanded = false; newBtn.innerText = `${currentTotal} phản hồi`; container.appendChild(newBtn); return; }
                const remaining = currentTotal - shownCount; const toShow = Math.min(batch, remaining);
                for (let i = shownCount; i < shownCount + toShow; i++) if (currentReplies[i]) currentReplies[i].style.display = 'block';
                container.appendChild(newBtn); shownCount += toShow; const left = currentTotal - shownCount;
                if (left > 0) newBtn.innerText = `${left} phản hồi còn lại`;
                else { newBtn.innerText = 'Ẩn phản hồi'; expanded = true; }
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
            const container_login = $(this).closest('[data-loggedin]');
            if (!checkLoginStatus(container_login)) { showLoginRequired('Bạn cần đăng nhập nếu muốn gửi phản hồi.'); return; }
            const currentReplyItem = $(this).closest('.reply-item');
            if (currentReplyItem.find('.reply-form-container.clone-form').length > 0) return;
            const clonedForm = originalForm.clone(true);
            clonedForm.removeAttr('id').addClass('clone-form');
            clonedForm.find('textarea').val(replyToUsername ? `@${replyToUsername} ` : '');
            clonedForm.find('form').attr('data-reply-to-user-id', replyToUserId || '').attr('data-parent-reply-id', replyId || '').attr('data-reply-to-username', replyToUsername || '');
            currentReplyItem.append(clonedForm); clonedForm.hide().slideDown();
            const textareaEl = clonedForm.find('textarea').get(0);
            if (textareaEl) { textareaEl.focus(); textareaEl.setSelectionRange(textareaEl.value.length, textareaEl.value.length); }
        });
    };

    const initEditReply = () => {
        $(document).on("click", ".edit-reply", function (e) {
            e.preventDefault();
            const replyItem = $(this).closest(".reply-item");
            const contentDiv = replyItem.find(".reply-content");
            const editBox = replyItem.find(".reply-edit-container");
            const textarea = replyItem.find(".edit-reply-text");
            textarea.val(contentDiv.text().trim());
            contentDiv.hide(); editBox.show();
        });

        $(document).on("click", ".cancel-edit", function () {
            const replyItem = $(this).closest(".reply-item");
            replyItem.find(".reply-edit-container").hide(); replyItem.find(".reply-content").show();
        });

        $(document).on("click", ".save-edit", function () {
            const replyItem = $(this).closest(".reply-item");
            const replyId = replyItem.data("reply-id");
            const newContent = replyItem.find(".edit-reply-text").val();
            const token = $('input[name="__RequestVerificationToken"]').val();
            $.ajax({
                url: '/ProductReviews/EditReply', type: 'POST',
                data: { __RequestVerificationToken: token, replyId: replyId, content: newContent },
                success: function (res) {
                    if (res.success) {
                        replyItem.find(".reply-content").text(newContent).show();
                        replyItem.find(".reply-edit-container").hide();
                        if (replyItem.find(".edited-label").length === 0) replyItem.find(".reply-username strong").append(' <span class="edited-label text-muted ms-1">(đã chỉnh sửa)</span>');
                        replyItem.find(".reply-date").text(res.updatedAt);
                    } else alert(res.message);
                }
            });
        });
    };

    return { initReplyButtons, initReplyReactionButtons, initReplyList, initReplyToReplyButtons, initEditReply };
})();

// BÁO CÁO REVIEW / REPLY
const ProductReport = (() => {
    const init = () => {
        $(document).off('click', '.report-review');
        $(document).on('click', '.report-review', function (e) {
            e.preventDefault();
            const containerLogin = $(this).closest('[data-loggedin]');
            const isLoggedIn = containerLogin.length ? (containerLogin.attr('data-loggedin') === 'true') : false;
            if (!isLoggedIn) { showLoginRequired('Bạn cần đăng nhập để báo cáo vi phạm.'); return; }
            const reviewId = $(this).attr('data-review-id');
            const replyId = $(this).attr('data-reply-id');
            const modalElement = document.getElementById('reportModal');
            if (modalElement) {
                const form = modalElement.querySelector('#reportForm');
                if (form) form.reset();
                modalElement.querySelector('input[name="ReviewId"]').value = reviewId ?? '';
                modalElement.querySelector('input[name="ReplyId"]').value = replyId ?? '';
                let bsModal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
                bsModal.show();
            }
        });

        $(document).off('submit', '#reportForm');
        $(document).on('submit', '#reportForm', function (e) {
            e.preventDefault();
            const form = $(this);
            const submitBtn = form.find('button[type="submit"]');
            submitBtn.prop('disabled', true).text('Đang gửi...');
            $.ajax({
                url: '/ProductReviews/Report', type: 'POST', data: form.serialize(),
                success: function (res) {
                    submitBtn.prop('disabled', false).text('Gửi báo cáo');
                    if (res.success) {
                        const modalElement = document.getElementById('reportModal');
                        bootstrap.Modal.getInstance(modalElement).hide();
                        const reviewId = form.find('input[name="ReviewId"]').val();
                        const replyId = form.find('input[name="ReplyId"]').val();
                        const selector = replyId ? `.report-review[data-reply-id="${replyId}"]` : `.report-review[data-review-id="${reviewId}"]`;
                        $(selector).replaceWith('<span class="dropdown-item text-muted small">Bạn đã gửi báo cáo (đang chờ xử lý)</span>');
                        Swal.fire({ icon: 'success', title: 'Thành công', text: 'Báo cáo đã được gửi.', timer: 1500, showConfirmButton: false });
                    } else alert(res.message);
                }
            });
        });
    };
    return { init };
})();

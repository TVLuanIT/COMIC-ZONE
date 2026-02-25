// wwwroot/js/product-summary.js

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

const ProductReviews = (() => {
    let productId;

    const init = (id) => {
        productId = id;
        loadReviewsFirstTime();
        initPagination();
        initReactionButtons();
        ProductReplies.initReplyButtons(); // khởi tạo reply form
        ProductReplies.initReplyReactionButtons(); // khởi tạo nút like/dislike cho reply
    };

    const loadReviewsFirstTime = () => {
        fetch('/ProductReviews/Reviews?productId=' + productId)
            .then(response => response.text())
            .then(html => {
                document.getElementById("review-container").innerHTML = html;
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
                    initReactionButtons(); // gọi ở đây
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

    return { init };
})();

const ProductReplies = (() => {

    const batchSize = 5; // số reply hiển thị mỗi lần, có thể thay đổi

    const initReplyButtons = () => {
        // Mở / ẩn khung reply
        $(document).off('click', '.toggle-reply');
        $(document).on('click', '.toggle-reply', function (e) {
            e.preventDefault();
            const reviewId = $(this).data('review-id');
            const container = $(`#reply-form-${reviewId}`);

            if (container.length === 0) {
                // Chưa login → redirect login
                // Lấy URL hiện tại
                const currentUrl = window.location.pathname + window.location.search;
                window.location.href = `/Authentication/Login?returnUrl=${encodeURIComponent(currentUrl)}`;
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
                .attr('data-reply-to-username', replyToUsername || '');

            // Toggle hiển thị form
            container.slideToggle();
        });

        // Hủy reply
        $(document).off('click', '.cancel-reply');
        $(document).on('click', '.cancel-reply', function (e) {
            e.preventDefault();
            $(this).closest('.reply-form-container').slideUp();
        });

        // Submit reply
        $(document).off('submit', '.reply-form');
        $(document).on('submit', '.reply-form', function (e) {
            e.preventDefault();
            const form = $(this);
            const reviewId = form.data('review-id');
            const replyToUserId = form.data('reply-to-user-id');
            const content = form.find('textarea').val().trim();
            if (!content) return;

            $.ajax({
                url: '/ProductReviews/AddReply',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    ReviewId: reviewId,
                    Content: content,
                    ReplyToUserId: replyToUserId ? parseInt(replyToUserId) : null
                }),
                success: function (res) {
                    if (res.success) {
                        // Xóa nội dung và ẩn form
                        form.find('textarea').val('');
                        form.closest('.reply-form-container').slideUp();

                        let mention = res.replytouserUsername ? `@${res.replytouserUsername} ` : '';
                        // Thêm reply mới vào danh sách hiển thị
                        let replyHtml = `
                            <div class="reply-item mt-2 ps-5">
                                <strong>${res.username}</strong>: ${res.content}
                            </div>
                        `;
                        $(`#reply-list-${reviewId}`).append(replyHtml);
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
        const replyContainers = document.querySelectorAll('.reply-list-container');

        replyContainers.forEach(container => {
            const showBtn = container.querySelector('.show-replies-btn');
            const replies = Array.from(container.querySelectorAll('.reply-item'));
            let shownCount = 0;
            const batchSize = parseInt(showBtn.dataset.batchSize) || 5;

            // ĐƯA NÚT XUỐNG CUỐI
            container.appendChild(showBtn);

            // Ẩn tất cả reply ban đầu
            replies.forEach(r => r.style.display = 'none');

            showBtn.addEventListener('click', () => {
                const remaining = replies.length - shownCount;
                const toShow = Math.min(batchSize, remaining);

                for (let i = shownCount; i < shownCount + toShow; i++) {
                    replies[i].style.display = 'block';
                }

                shownCount += toShow;

                const left = replies.length - shownCount;
                if (left > 0) {
                    showBtn.innerText = `${left} phản hồi còn lại`;
                } else {
                    showBtn.style.display = 'none';
                }
            });
        });
    };

    return {
        initReplyButtons,
        initReplyReactionButtons,
        initReplyList
    };

})();

// Khi trang load xong
document.addEventListener('DOMContentLoaded', () => {
    ProductSummary.init();

    // Truyền productId từ Razor view
    const productIdElement = document.getElementById('product-id');
    if (productIdElement) {
        const id = parseInt(productIdElement.value);
        if (!isNaN(id)) {
            ProductReviews.init(id);

            // KHỞI TẠO HIỂN THỊ REPLY THEO BATCH
            // Ở đây gọi sau khi loadReviewFirstTime xong (HTML đã render)
            setTimeout(() => {
                ProductReplies.initReplyList();
            }, 500); // delay 0.5s để DOM load xong
        }
    }
});
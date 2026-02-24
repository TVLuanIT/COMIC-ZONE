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
    };

    const loadReviewsFirstTime = () => {
        fetch('/ProductReviews/Reviews?productId=' + productId)
            .then(response => response.text())
            .then(html => {
                document.getElementById("review-container").innerHTML = html;
                initReactionButtons(); // gắn lại nút like
            });
    };

    const initPagination = () => {
        $(document).on('click', '.review-page-link', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            $('#review-list-container')
                .load(`/ProductReviews/Reviews?productId=${productId}&page=${page}`, () => {
                    initReactionButtons(); // ✅ gọi ở đây
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

// Khi trang load xong
document.addEventListener('DOMContentLoaded', () => {
    ProductSummary.init();

    // Truyền productId từ Razor view
    const productIdElement = document.getElementById('product-id');
    if (productIdElement) {
        const id = parseInt(productIdElement.value);
        if (!isNaN(id)) {
            ProductReviews.init(id);
        }
    }
});
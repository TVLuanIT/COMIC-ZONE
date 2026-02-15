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
    };

    const loadReviewsFirstTime = () => {
        $('#review-list-container').load(`/ProductReviews/Reviews?productId=${productId}`);
    };

    const initPagination = () => {
        $(document).on('click', '.review-page-link', function (e) {
            e.preventDefault();
            const page = $(this).data('page');
            $('#review-list-container').load(`/ProductReviews/Reviews?productId=${productId}&page=${page}`);
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
        const id = productIdElement.value;
        ProductReviews.init(id);
    }
});
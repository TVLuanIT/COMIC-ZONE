/* JS for Blog Comments Section */
document.addEventListener('DOMContentLoaded', function () {
    const commentsArea = document.querySelector('.comments-area');
    const isLoggedIn = commentsArea ? commentsArea.getAttribute('data-is-logged-in') === 'true' : false;
    const loginUrl = commentsArea ? commentsArea.getAttribute('data-login-url') : '';

    function checkAuth() {
        if (!isLoggedIn) {
            Swal.fire({
                title: 'Yêu cầu đăng nhập',
                text: 'Vui lòng đăng nhập để thực hiện chức năng này.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Đăng nhập',
                cancelButtonText: 'Hủy',
                reverseButtons: true,
                customClass: {
                    popup: 'premium-swal-popup',
                    confirmButton: 'premium-swal-confirm',
                    cancelButton: 'premium-swal-cancel'
                },
                buttonsStyling: false,
                showClass: {
                    popup: 'animate__animated animate__fadeInDown animate__faster'
                },
                hideClass: {
                    popup: 'animate__animated animate__fadeOutUp animate__faster'
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = loginUrl;
                }
            });
            return false;
        }
        return true;
    }

    const btnSubmit = document.getElementById('btnSubmitComment');
    if (btnSubmit) {
        btnSubmit.addEventListener('click', async function () {
            const contentInput = document.getElementById('commentContent');
            const content = contentInput.value.trim();
            const blogId = document.getElementById('blogId').value;
            // Get URL from data attribute
            const commentForm = document.getElementById('commentForm');
            const actionUrl = commentForm ? commentForm.getAttribute('data-url') : '';

            if (!content) {
                Swal.fire({
                    text: 'Vui lòng nhập nội dung bình luận!',
                    icon: 'info',
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 3000
                });
                return;
            }

            // Check auth
            if (!checkAuth()) return;

            if (!actionUrl) {
                console.error('Comment action URL not found');
                return;
            }

            const btn = this;
            const textSpan = btn.querySelector('.submit-text');
            const iconSpan = btn.querySelector('.submit-icon');
            const spinnerSpan = btn.querySelector('.submit-spinner');

            // Loading state
            btn.disabled = true;
            spinnerSpan.classList.remove('d-none');
            iconSpan.classList.add('d-none');

            try {
                const formData = new FormData();
                formData.append('blogId', blogId);
                formData.append('content', content);

                const response = await fetch(actionUrl, {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                if (result.success) {
                    contentInput.value = '';

                    // Create new comment HTML
                    const html = `
                        <div class="comment-node mb-4 animate__animated animate__fadeInDown" id="comment-${result.comment.id}">
                            <div class="d-flex gap-3">
                                <img src="${result.comment.avatar}" class="comment-avatar" alt="Avatar">
                                <div class="comment-body w-100">
                                    <div class="d-flex justify-content-between align-items-center mb-1">
                                        <h6 class="mb-0 fw-bold">${result.comment.username}</h6>
                                        <span class="x-small text-muted">${result.comment.createdAt}</span>
                                    </div>
                                    <p class="text-secondary mb-2">${result.comment.content}</p>
                                    
                                    <div class="comment-actions d-flex gap-3 align-items-center">
                                        <button class="btn-like d-flex align-items-center gap-1" 
                                                data-comment-id="${result.comment.id}" data-is-like="true">
                                            <i class="bi bi-hand-thumbs-up"></i>
                                            <span class="like-count count">0</span>
                                        </button>
                                        
                                        <button class="btn-dislike d-flex align-items-center gap-1" 
                                                data-comment-id="${result.comment.id}" data-is-like="false">
                                            <i class="bi bi-hand-thumbs-down"></i>
                                        </button>

                                        <button class="btn-toggle-reply d-flex align-items-center gap-1" 
                                                data-comment-id="${result.comment.id}"
                                                data-form-id="reply-form-c-${result.comment.id}"
                                                data-reply-to-user="${result.comment.username}">
                                            <i class="bi bi-reply-fill"></i>
                                            <span>Phản hồi</span>
                                        </button>
                                    </div>

                                    <div class="reply-form-container mt-3 d-none" id="reply-form-c-${result.comment.id}">
                                         <div class="d-flex gap-2 align-items-center">
                                             <textarea class="form-control-premium x-small reply-content w-100" rows="2" placeholder="Viết phản hồi..."></textarea>
                                             <button class="btn btn-primary btn-sm rounded-pill px-3 btn-submit-reply" 
                                                     data-comment-id="${result.comment.id}"
                                                     data-form-id="reply-form-c-${result.comment.id}">
                                                 <i class="bi bi-send-fill"></i>
                                             </button>
                                         </div>
                                    </div>

                                    <div class="replies-list mt-3 ms-2 ps-3 border-start" id="replies-list-${result.comment.id}">
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;

                    const commentsList = document.querySelector('.comments-list');
                    if (commentsList) {
                        commentsList.insertAdjacentHTML('afterbegin', html);
                    }

                    // Update comment count
                    const countHeader = document.querySelector('.comments-area h3');
                    if (countHeader) {
                        const currentText = countHeader.innerText;
                        const countMatch = currentText.match(/\d+/);
                        if (countMatch) {
                            const newCount = parseInt(countMatch[0]) + 1;
                            countHeader.innerText = currentText.replace(/\d+/, newCount);
                        }
                    }
                } else if (!result.message || !result.message.includes("đăng nhập")) {
                    Swal.fire({
                        text: result.message || 'Có lỗi xảy ra!',
                        icon: 'error',
                        customClass: { popup: 'premium-swal-popup' }
                    });
                }
            } catch (err) {
                console.error(err);
                Swal.fire({
                    text: 'Gửi bình luận thất bại!',
                    icon: 'error',
                    customClass: { popup: 'premium-swal-popup' }
                });
            } finally {
                // Restore button state
                btn.disabled = false;
                spinnerSpan.classList.add('d-none');
                iconSpan.classList.remove('d-none');
            }
        });
    }

    // Like/Dislike Functionality
    const commentsList = document.querySelector('.comments-list');
    if (commentsList) {
        const toggleLikeUrl = commentsList.getAttribute('data-toggle-like-url');
        const toggleReplyUrl = commentsList.getAttribute('data-toggle-reply-url');
        const toggleReplyLikeUrl = commentsList.getAttribute('data-toggle-reply-like-url');

        // Toggle Reply Form
        commentsList.addEventListener('click', function (e) {
            const btn = e.target.closest('.btn-toggle-reply');
            if (btn) {
                const formId = btn.getAttribute('data-form-id');
                const parentReplyId = btn.getAttribute('data-parent-reply-id');
                const replyToUser = btn.getAttribute('data-reply-to-user');

                const form = document.getElementById(formId);
                if (form) {
                    const isOpening = form.classList.contains('d-none');
                    form.classList.toggle('d-none');

                    const submitBtn = form.querySelector('.btn-submit-reply');
                    const textarea = form.querySelector('textarea');

                    if (!form.classList.contains('d-none')) {
                        form.classList.add('animate__animated', 'animate__fadeIn');

                        // Prep tag if reply target exists
                        if (replyToUser && isOpening) {
                            textarea.value = `@${replyToUser} `;
                        } else if (!replyToUser && isOpening) {
                            textarea.value = '';
                        }

                        textarea.focus();
                    }
                }
            }
        });

        // Submit Reply
        commentsList.addEventListener('click', async function (e) {
            const btn = e.target.closest('.btn-submit-reply');
            if (btn) {
                const commentId = btn.getAttribute('data-comment-id');
                const formId = btn.getAttribute('data-form-id');
                const formContainer = document.getElementById(formId);
                const textarea = formContainer.querySelector('.reply-content');
                const content = textarea.value.trim();

                if (!content) {
                    Swal.fire({
                        text: 'Vui lòng nhập nội dung phản hồi!',
                        icon: 'info',
                        toast: true,
                        position: 'top-end',
                        showConfirmButton: false,
                        timer: 3000
                    });
                    return;
                }

                // Check auth before sending
                if (!checkAuth()) return;

                btn.disabled = true;
                const originalHtml = btn.innerHTML;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';

                try {
                    const formData = new FormData();
                    formData.append('commentId', commentId);
                    formData.append('content', content);

                    const parentReplyId = btn.getAttribute('data-parent-reply-id');
                    if (parentReplyId) {
                        formData.append('parentReplyId', parentReplyId);
                    }

                    const response = await fetch(toggleReplyUrl, {
                        method: 'POST',
                        body: formData
                    });

                    const result = await response.json();
                    if (result.success) {
                        textarea.value = '';
                        formContainer.classList.add('d-none');

                        // Create new reply HTML
                        const replyToHtml = result.reply.replyToUsername
                            ? `<span class="fw-normal text-muted ms-1"><i class="bi bi-caret-right-fill x-small"></i> ${result.reply.replyToUsername}</span>`
                            : '';

                        const replyHtml = `
                            <div class="reply-node mb-3 animate__animated animate__fadeIn" id="reply-${result.reply.id}">
                                <div class="d-flex gap-2">
                                    <img src="${result.reply.avatar}" class="reply-avatar">
                                    <div class="reply-body w-100">
                                        <div class="d-flex justify-content-between align-items-center mb-0">
                                            <h6 class="text-sm fw-bold mb-0">
                                                ${result.reply.username}
                                                ${replyToHtml}
                                            </h6>
                                            <span class="xx-small text-muted">${result.reply.createdAt}</span>
                                        </div>
                                        <p class="small text-secondary mb-1">${result.reply.content}</p>
                                        
                                        <div class="reply-actions d-flex gap-2 align-items-center">
                                            <button class="btn-reply-like xx-small" 
                                                    data-reply-id="${result.reply.id}" data-is-like="true">
                                                <i class="bi bi-hand-thumbs-up"></i>
                                                <span class="count">0</span>
                                            </button>
                                            <button class="btn-reply-dislike xx-small" 
                                                    data-reply-id="${result.reply.id}" data-is-like="false">
                                                <i class="bi bi-hand-thumbs-down"></i>
                                            </button>
                                            <button class="btn-toggle-reply xx-small border-0 bg-transparent text-primary fw-medium" 
                                                    data-comment-id="${commentId}" 
                                                    data-parent-reply-id="${result.reply.id}"
                                                    data-reply-to-user="${result.reply.username}"
                                                    data-form-id="reply-form-r-${result.reply.id}">
                                                Phản hồi
                                            </button>
                                        </div>

                                        <div class="reply-form-container mt-2 d-none" id="reply-form-r-${result.reply.id}">
                                            <div class="d-flex gap-2 align-items-center ps-3 border-start">
                                                <textarea class="form-control-premium xx-small reply-content w-100" rows="1" placeholder="Viết phản hồi..."></textarea>
                                                <button class="btn btn-primary btn-sm rounded-pill px-2 btn-submit-reply" 
                                                        data-comment-id="${commentId}"
                                                        data-parent-reply-id="${result.reply.id}"
                                                        data-form-id="reply-form-r-${result.reply.id}">
                                                    <i class="bi bi-send-fill x-small"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        `;

                        const repliesList = document.getElementById(`replies-list-${commentId}`);
                        if (repliesList) {
                            repliesList.insertAdjacentHTML('beforeend', replyHtml);
                            repliesList.classList.remove('d-none');
                            
                            // Hiển thị tất cả các phản hồi đang có
                            repliesList.querySelectorAll('.reply-node').forEach(node => node.classList.remove('d-none'));

                            // Cập nhật nút điều khiển
                            let ctrlBtn = document.querySelector(`.btn-view-replies[data-comment-id="${commentId}"]`);
                            if (!ctrlBtn) {
                                // Tạo nút mới nếu chưa có
                                const ctrlHtml = `
                                    <div class="replies-control mt-2" id="replies-control-${commentId}">
                                        <button class="btn-view-replies btn btn-link btn-sm p-0 text-decoration-none fw-bold small" 
                                                data-comment-id="${commentId}" 
                                                data-total="1"
                                                data-current="1">
                                            <i class="bi bi-chat-dots-fill me-1"></i>
                                            Ẩn phản hồi
                                        </button>
                                    </div>
                                `;
                                repliesList.insertAdjacentHTML('afterend', ctrlHtml);
                            } else {
                                let total = parseInt(ctrlBtn.getAttribute('data-total')) + 1;
                                // Tự động mở rộng toàn bộ để người dùng thấy câu trả lời vừa gửi
                                let current = total; 
                                ctrlBtn.setAttribute('data-total', total);
                                ctrlBtn.setAttribute('data-current', current);
                                updateRepliesButtonText(ctrlBtn, total, current);
                            }
                        }
                    } else if (!result.message || !result.message.includes("đăng nhập")) {
                        Swal.fire({
                            text: result.message || 'Có lỗi xảy ra!',
                            icon: 'error',
                            customClass: { popup: 'premium-swal-popup' }
                        });
                    }
                } catch (err) {
                    console.error(err);
                    Swal.fire({
                        text: 'Gửi phản hồi thất bại!',
                        icon: 'error',
                        customClass: { popup: 'premium-swal-popup' }
                    });
                } finally {
                    btn.disabled = false;
                    btn.innerHTML = originalHtml;
                }
            }
        });

        // Toggle Reply Like/Dislike
        commentsList.addEventListener('click', async function (e) {
            const btn = e.target.closest('.btn-reply-like, .btn-reply-dislike');
            if (btn) {
                const replyId = btn.getAttribute('data-reply-id');
                const isLike = btn.getAttribute('data-is-like') === 'true';

                try {
                    const formData = new FormData();
                    formData.append('replyId', replyId);
                    formData.append('isLike', isLike);

                    const response = await fetch(toggleReplyLikeUrl, {
                        method: 'POST',
                        body: formData
                    });

                    const result = await response.json();
                    if (result.success) {
                        const replyNode = document.getElementById(`reply-${replyId}`);
                        if (replyNode) {
                            const likeBtn = replyNode.querySelector('.btn-reply-like');
                            const dislikeBtn = replyNode.querySelector('.btn-reply-dislike');
                            const likeCount = likeBtn.querySelector('.count');

                            // Update count
                            if (likeCount) likeCount.innerText = result.likeCount;

                            // Update active states
                            likeBtn.classList.toggle('active', result.currentUserReaction === true);
                            if (dislikeBtn) dislikeBtn.classList.toggle('active', result.currentUserReaction === false);

                            // Update icons
                            const likeIcon = likeBtn.querySelector('i');
                            const dislikeIcon = dislikeBtn ? dislikeBtn.querySelector('i') : null;

                            if (result.currentUserReaction === true) {
                                likeIcon.className = 'bi bi-hand-thumbs-up-fill';
                                if (dislikeIcon) dislikeIcon.className = 'bi bi-hand-thumbs-down';
                            } else if (result.currentUserReaction === false) {
                                likeIcon.className = 'bi bi-hand-thumbs-up';
                                if (dislikeIcon) dislikeIcon.className = 'bi bi-hand-thumbs-down-fill';
                            } else {
                                likeIcon.className = 'bi bi-hand-thumbs-up';
                                if (dislikeIcon) dislikeIcon.className = 'bi bi-hand-thumbs-down';
                            }
                        }
                    } else if (!result.message || !result.message.includes("đăng nhập")) {
                        Swal.fire({
                            text: result.message || 'Có lỗi xảy ra!',
                            icon: 'error',
                            customClass: { popup: 'premium-swal-popup' }
                        });
                    }
                } catch (err) {
                    console.error(err);
                    Swal.fire({
                        text: 'Thao tác thất bại!',
                        icon: 'error',
                        customClass: { popup: 'premium-swal-popup' }
                    });
                }
            }
        });

        // Toggle Comment Like
        commentsList.addEventListener('click', async function (e) {
            const btn = e.target.closest('.btn-like, .btn-dislike');
            if (!btn) return;
            if (btn.classList.contains('btn-reply-like')) return; // handled separately

            const commentId = btn.getAttribute('data-comment-id');
            const isLike = btn.getAttribute('data-is-like') === 'true';

            try {
                const formData = new FormData();
                formData.append('commentId', commentId);
                formData.append('isLike', isLike);

                const response = await fetch(toggleLikeUrl, {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();
                if (result.success) {
                    const commentNode = document.getElementById(`comment-${commentId}`);
                    if (commentNode) {
                        const likeBtn = commentNode.querySelector('.btn-like');
                        const dislikeBtn = commentNode.querySelector('.btn-dislike');
                        const likeCount = commentNode.querySelector('.like-count');

                        // Update counts
                        if (likeCount) likeCount.innerText = result.likeCount;

                        // Update button states
                        likeBtn.classList.toggle('active', result.currentUserReaction === true);
                        dislikeBtn.classList.toggle('active', result.currentUserReaction === false);

                        // Update icons
                        const likeIcon = likeBtn.querySelector('i');
                        const dislikeIcon = dislikeBtn.querySelector('i');

                        if (result.currentUserReaction === true) {
                            likeIcon.className = 'bi bi-hand-thumbs-up-fill';
                            dislikeIcon.className = 'bi bi-hand-thumbs-down';
                        } else if (result.currentUserReaction === false) {
                            likeIcon.className = 'bi bi-hand-thumbs-up';
                            dislikeIcon.className = 'bi bi-hand-thumbs-down-fill';
                        } else {
                            likeIcon.className = 'bi bi-hand-thumbs-up';
                            dislikeIcon.className = 'bi bi-hand-thumbs-down';
                        }
                    }
                } else if (!result.message || !result.message.includes("đăng nhập")) {
                    Swal.fire({
                        text: result.message || 'Có lỗi xảy ra!',
                        icon: 'error',
                        customClass: { popup: 'premium-swal-popup' }
                    });
                }
            } catch (err) {
                console.error(err);
                Swal.fire({
                    text: 'Thao tác thất bại!',
                    icon: 'error',
                    customClass: { popup: 'premium-swal-popup' }
                });
            }
        });

        // Toggle/Expand Replies
        commentsList.addEventListener('click', function (e) {
            const btn = e.target.closest('.btn-view-replies');
            if (btn) {
                const commentId = btn.getAttribute('data-comment-id');
                const list = document.getElementById(`replies-list-${commentId}`);
                if (!list) return;

                const total = parseInt(btn.getAttribute('data-total'));
                let current = parseInt(btn.getAttribute('data-current'));

                if (current === total && current > 0) {
                    // Hide all
                    list.classList.add('d-none');
                    current = 0;
                } else {
                    // Show next 5
                    list.classList.remove('d-none');
                    current = Math.min(current + 5, total);

                    const nodes = list.querySelectorAll('.reply-node');
                    nodes.forEach((node, index) => {
                        if (index < current) {
                            node.classList.remove('d-none');
                        } else {
                            node.classList.add('d-none');
                        }
                    });
                }

                btn.setAttribute('data-current', current);
                updateRepliesButtonText(btn, total, current);
            }
        });

        function updateRepliesButtonText(btn, total, current) {
            const icon = '<i class="bi bi-chat-dots-fill me-1"></i> ';
            if (current === 0) {
                btn.innerHTML = `${icon}${total} phản hồi`;
            } else if (current < total) {
                btn.innerHTML = `${icon}${total - current} phản hồi`;
            } else {
                btn.innerHTML = `${icon}Ẩn phản hồi`;
            }
        }
    }
});

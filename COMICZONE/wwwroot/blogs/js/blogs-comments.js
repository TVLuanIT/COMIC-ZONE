/* JS for Blog Comments Section */
document.addEventListener('DOMContentLoaded', function() {
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
                alert('Vui lòng nhập nội dung bình luận!');
                return;
            }
            
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
                        <div class="comment-node mb-4 animate__animated animate__fadeInDown">
                            <div class="d-flex gap-3">
                                <img src="${result.comment.avatar}" class="comment-avatar" alt="Avatar">
                                <div class="comment-body">
                                    <div class="d-flex justify-content-between align-items-center mb-1">
                                        <h6 class="mb-0 fw-bold">${result.comment.username}</h6>
                                        <span class="x-small text-muted">${result.comment.createdAt}</span>
                                    </div>
                                    <p class="text-secondary mb-0">${result.comment.content}</p>
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
                } else {
                    alert(result.message || 'Có lỗi xảy ra!');
                }
            } catch (err) {
                console.error(err);
                alert('Gửi bình luận thất bại!');
            } finally {
                // Restore button state
                btn.disabled = false;
                spinnerSpan.classList.add('d-none');
                iconSpan.classList.remove('d-none');
            }
        });
    }
});

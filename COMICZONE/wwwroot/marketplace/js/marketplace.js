/**
 * Marketplace Module Logic
 * Handles Post Creation checks, Favorites, Chat Drawer, and Image Previews.
 */

const Marketplace = {
    config: {},
    chat: {
        interval: null,
        isOpen: false
    },

    // Initialization for Index page
    initIndex: function(config) {
        this.config = config;
        
        // Handle redirect message if present
        if (config.incompleteProfile) {
            Swal.fire({
                title: 'Hồ sơ chưa hoàn thiện',
                text: config.incompleteProfile,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Cập nhật ngay',
                cancelButtonText: 'Để sau',
                confirmButtonColor: '#6366f1',
                cancelButtonColor: '#aaa',
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = config.editProfileUrl;
                }
            });
        }

        // Initialize NiceSelect2 for filter inputs
        const selects = document.querySelectorAll('.mp-filter-bar select');
        selects.forEach(select => {
            NiceSelect.bind(select, {
                searchable: false
            });
        });
    },

    handleCreatePostClick: async function() {
        const config = this.config;
        try {
            const response = await fetch(config.checkProfileUrl);
            const data = await response.json();

            if (!data.success) {
                if (data.message === "login_required") {
                    this.requireLoginPrompt('Bạn cần đăng nhập để đăng bài bán.', config.loginUrl);
                } else {
                    Swal.fire('Lỗi', 'Đã có lỗi xảy ra, vui lòng thử lại.', 'error');
                }
                return;
            }

            if (!data.isComplete) {
                Swal.fire({
                    title: 'Hồ sơ chưa hoàn thiện',
                    text: 'Bạn cần bổ sung số điện thoại và địa chỉ để người mua có thể liên lạc trước khi đăng bán.',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Cập nhật ngay',
                    cancelButtonText: 'Để sau',
                    confirmButtonColor: '#6366f1',
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = config.editProfileUrl;
                    }
                });
            } else {
                window.location.href = config.createPostUrl;
            }
        } catch (error) {
            console.error('Error checking profile:', error);
            window.location.href = config.createPostUrl;
        }
    },

    // Initialization for Details page
    initDetails: function(config) {
        this.config = config;
        
        const btnContact = document.getElementById('btnContactSeller');
        const btnCloseChat = document.getElementById('btnCloseChat');
        const drawerBackdrop = document.getElementById('chatDrawerBackdrop');
        const chatForm = document.getElementById('chatForm');

        if (btnContact) {
            btnContact.addEventListener('click', () => {
                if (!config.currentUserId) {
                    this.requireLoginPrompt('Bạn cần đăng nhập để liên hệ người bán.', config.loginUrl);
                    return;
                }
                this.openChatDrawer();
            });
        }

        if (btnCloseChat) btnCloseChat.addEventListener('click', () => this.closeChatDrawer());
        if (drawerBackdrop) drawerBackdrop.addEventListener('click', () => this.closeChatDrawer());

        if (chatForm) {
            chatForm.addEventListener('submit', (e) => this.handleChatSubmit(e));
        }

        // Unread badge polling
        if (config.currentUserId && config.currentUserId !== config.sellerId) {
            this.checkUnread();
            setInterval(() => this.checkUnread(), 10000);
        }
    },

    toggleFavorite: async function(postId) {
        try {
            const res = await fetch(`${this.config.toggleFavoriteUrl}?postId=${postId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });
            const data = await res.json();

            if (!data.success && data.message === 'login_required') {
                this.requireLoginPrompt('Bạn cần đăng nhập để lưu bài đăng.', this.config.loginUrl);
                return;
            }

            const btn = document.getElementById('btnFavorite');
            const icon = btn.querySelector('i');
            const text = btn.querySelector('span');

            if (data.isFavorited) {
                icon.className = 'bi bi-heart-fill me-2 text-danger';
                text.textContent = 'Đã lưu vào danh sách';
                btn.classList.add('btn-dark');
                btn.classList.remove('btn-outline-dark');
            } else {
                icon.className = 'bi bi-heart me-2';
                text.textContent = 'Lưu bài đăng này';
                btn.classList.remove('btn-dark');
                btn.classList.add('btn-outline-dark');
            }

            const Toast = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 2000,
                timerProgressBar: true,
            });
            Toast.fire({
                icon: 'success',
                title: data.isFavorited ? 'Đã lưu bài đăng' : 'Đã bỏ lưu bài đăng'
            });
        } catch (err) {
            console.error(err);
            Swal.fire('Lỗi', 'Có lỗi xảy ra, vui lòng thử lại.', 'error');
        }
    },

    requireLoginPrompt: function(textMsg, loginUrl) {
        Swal.fire({
            icon: 'warning',
            title: 'Chưa đăng nhập',
            text: textMsg,
            confirmButtonText: 'Đăng nhập',
            showCancelButton: true,
            cancelButtonText: 'Để sau',
            confirmButtonColor: '#6366f1',
            reverseButtons: true
        }).then((result) => {
            if (result.isConfirmed) {
                const returnUrl = encodeURIComponent(window.location.pathname);
                window.location.href = `${loginUrl}?returnUrl=${returnUrl}`;
            }
        });
    },

    openChatDrawer: function() {
        const postId = this.config.postId;
        const sellerId = this.config.sellerId;
        window.location.href = `/Chat/Conversations/Index?otherUserId=${sellerId}&postId=${postId}`;
    },

    closeChatDrawer: function() {
        // Obsolete
    },

    loadMessages: async function() { /* Obsolete */ },
    renderMessages: function(messages) { /* Obsolete */ },
    handleChatSubmit: async function(e) { /* Obsolete */ },
    checkUnread: async function() { /* Obsolete */ },
    scrollToBottom: function() { /* Obsolete */ },
    escapeHtml: function(unsafe) { /* Obsolete */ },

    // Initialization for Create page
    initCreate: function() {
        // Initialize NiceSelect2 for selects
        const selects = document.querySelectorAll('select.mp-form-control');
        selects.forEach(select => {
            NiceSelect.bind(select, { searchable: false });
        });

        const imageInput = document.getElementById('imageInput');
        if (imageInput) {
            imageInput.addEventListener('change', (event) => {
                const container = document.getElementById('imagePreviewContainer');
                container.innerHTML = '';
                let files = event.target.files;
                
                if (files && files.length > 5) {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Giới hạn hình ảnh',
                        text: 'Hệ thống chỉ chấp nhận tối đa 5 ảnh. Chúng tôi sẽ chỉ giữ lại 5 ảnh đầu tiên bạn đã chọn.',
                        confirmButtonColor: '#6366f1'
                    });

                    // Update the input files to only include the first 5
                    const dataTransfer = new DataTransfer();
                    for (let i = 0; i < 5; i++) {
                        dataTransfer.items.add(files[i]);
                    }
                    imageInput.files = dataTransfer.files;
                    files = imageInput.files;
                }

                if (files) {
                    Array.from(files).forEach(file => {
                        if (file.type.startsWith('image/')) {
                            const reader = new FileReader();
                            reader.onload = (e) => {
                                const imgWrapper = document.createElement('div');
                                imgWrapper.className = 'rounded-3 border overflow-hidden shadow-sm animate__animated animate__zoomIn';
                                imgWrapper.style.width = '120px';
                                imgWrapper.style.height = '120px';
                                
                                const img = document.createElement('img');
                                img.src = e.target.result;
                                img.className = 'w-100 h-100 object-fit-cover';
                                
                                imgWrapper.appendChild(img);
                                container.appendChild(imgWrapper);
                            };
                            reader.readAsDataURL(file);
                        }
                    });
                }
            });
        }
    }
};

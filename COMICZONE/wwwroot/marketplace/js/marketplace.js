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
        this.chat.isOpen = true;
        document.getElementById('chatDrawerBackdrop').classList.add('show');
        document.getElementById('chatDrawer').classList.add('show');
        document.body.style.overflow = 'hidden';
        
        const unreadBadge = document.getElementById('unreadBadge');
        if (unreadBadge) {
            unreadBadge.style.display = 'none';
            unreadBadge.innerText = '0';
        }
        
        this.loadMessages();
        this.chat.interval = setInterval(() => this.loadMessages(), 5000);
        setTimeout(() => document.getElementById('chatInput').focus(), 300);
    },

    closeChatDrawer: function() {
        this.chat.isOpen = false;
        document.getElementById('chatDrawerBackdrop').classList.remove('show');
        document.getElementById('chatDrawer').classList.remove('show');
        document.body.style.overflow = '';
        
        if (this.chat.interval) {
            clearInterval(this.chat.interval);
            this.chat.interval = null;
        }
    },

    loadMessages: async function() {
        if (!this.config.currentUserId) return;
        try {
            const res = await fetch(`${this.config.getMessagesUrl}?postId=${this.config.postId}&otherUserId=${this.config.sellerId}`);
            const data = await res.json();
            if (data.success) {
                this.renderMessages(data.messages);
            }
        } catch (e) {
            console.error('Error loading messages:', e);
        }
    },

    renderMessages: function(messages) {
        const chatMessageList = document.getElementById('chatMessageList');
        if (!messages || messages.length === 0) {
            chatMessageList.innerHTML = '<div class="text-center text-muted small mt-4">Chưa có tin nhắn nào. Bắt đầu trò chuyện ngay!</div>';
            return;
        }
        
        const isScrolledToBottom = chatMessageList.scrollHeight - chatMessageList.clientHeight <= chatMessageList.scrollTop + 50;
        
        let html = '';
        messages.forEach(msg => {
            const isMe = msg.senderId === this.config.currentUserId;
            const time = new Date(msg.createdAt).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
            
            if (isMe) {
                html += `
                    <div class="chat-sender-container">
                        <div class="chat-bubble chat-sender">${this.escapeHtml(msg.text)}</div>
                        <span class="chat-time text-end">${time}</span>
                    </div>
                `;
            } else {
                html += `
                    <div class="chat-receiver-container">
                        <div class="chat-bubble chat-receiver">${this.escapeHtml(msg.text)}</div>
                        <span class="chat-time text-start">${time}</span>
                    </div>
                `;
            }
        });
        
        if (chatMessageList.innerHTML.length !== html.length) {
            chatMessageList.innerHTML = html;
            if (isScrolledToBottom) {
                this.scrollToBottom();
            }
        }
    },

    handleChatSubmit: async function(e) {
        e.preventDefault();
        const chatInput = document.getElementById('chatInput');
        const text = chatInput.value.trim();
        if (!text) return;
        
        const currentText = text;
        chatInput.value = '';
        
        const payload = {
            PostId: this.config.postId,
            ReceiverId: this.config.sellerId,
            Message: currentText
        };
        
        try {
            const res = await fetch(this.config.sendMessageUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            const data = await res.json();
            
            if (data.success) {
                this.loadMessages();
                this.scrollToBottom();
            } else {
                Swal.fire('Lỗi', data.message || 'Không thể gửi tin nhắn.', 'error');
            }
        } catch (err) {
            console.error('Send message error:', err);
            Swal.fire('Lỗi', 'Có lỗi xảy ra, vui lòng thử lại.', 'error');
        }
    },

    checkUnread: async function() {
        if (!this.config.currentUserId || this.chat.isOpen) return;
        try {
            const res = await fetch(`${this.config.getUnreadCountUrl}?postId=${this.config.postId}`);
            const data = await res.json();
            const unreadBadge = document.getElementById('unreadBadge');
            
            if (data && data.count > 0 && unreadBadge) {
                unreadBadge.innerText = data.count > 99 ? '99+' : data.count;
                unreadBadge.style.display = 'block';
            } else if (unreadBadge) {
                unreadBadge.style.display = 'none';
            }
        } catch (e) { }
    },

    scrollToBottom: function() {
        const chatMessageList = document.getElementById('chatMessageList');
        if (chatMessageList) {
            setTimeout(() => {
                chatMessageList.scrollTop = chatMessageList.scrollHeight;
            }, 50);
        }
    },

    escapeHtml: function(unsafe) {
        return unsafe
             .replace(/&/g, "&amp;")
             .replace(/</g, "&lt;")
             .replace(/>/g, "&gt;")
             .replace(/"/g, "&quot;")
             .replace(/'/g, "&#039;");
    },

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

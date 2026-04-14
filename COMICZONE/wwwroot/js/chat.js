/**
 * Chat System Logic (SignalR Client)
 */
const ChatApp = {
    connection: null,
    config: {},
    activeUserId: null,

    init: function (config) {
        this.config = config;
        this.activeUserId = config.initialOtherUserId;

        // Initialize SignalR Connection
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .build();

        this.setupSignalRHandlers();
        this.startConnection();
        this.setupFormHandlers();

        if (this.activeUserId) {
            this.openConversation(this.activeUserId);
        }
    },

    setupSignalRHandlers: function () {
        // Lắng nghe tin nhắn mới từ người khác
        this.connection.on("ReceiveMessage", (data) => {
            console.log("New message received:", data);

            // Nếu đang mở đúng cuộc hội thoại với người gửi này
            if (this.activeUserId === data.senderId) {
                this.appendMessage({
                    senderId: data.senderId,
                    text: data.text,
                    createdAt: data.createdAt,
                    senderAvatar: data.senderAvatar,
                    postId: data.postId,
                    postTitle: data.postTitle,
                    postPrice: data.postPrice,
                    postImage: data.postImage,
                    id: data.id
                });
                this.scrollToBottom();
            } else {
                // Nếu đang ở cuộc hội thoại khác, cập nhật badge thông báo ở sidebar
                this.updateSidebarUnread(data.senderId, data.text);
            }
        });

        // Xác nhận tin nhắn đã gửi thành công (từ chính mình trên tab khác)
        this.connection.on("MessageSent", (data) => {
            if (this.activeUserId === data.receiverId) {
                this.appendMessage({
                    senderId: this.config.currentUserId,
                    text: data.text,
                    createdAt: data.createdAt,
                    postId: data.postId,
                    postTitle: data.postTitle,
                    postPrice: data.postPrice,
                    postImage: data.postImage,
                    id: data.id
                });
                this.scrollToBottom();
            }
        });

        // Lắng nghe thu hồi tin nhắn
        this.connection.on("MessageRecalled", (id) => {
            const el = document.querySelector(`.message-wrapper[data-id="${id}"]`);
            if (el) {
                const bubble = el.querySelector('.message-bubble'); // Updated from .chat-bubble
                if (bubble) {
                    bubble.classList.add('recalled');
                    bubble.innerHTML = '<div class="recalled-wrapper"><i class="bi bi-arrow-counterclockwise me-2"></i><span>Tin nhắn đã được thu hồi</span></div>';
                }
                const btn = el.querySelector('.recall-btn');
                if (btn) btn.remove();
            }
        });
    },

    startConnection: async function () {
        try {
            await this.connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.error("SignalR Connection Error: ", err);
            setTimeout(() => this.startConnection(), 5000);
        }
    },

    setupFormHandlers: function () {
        const form = document.getElementById('chatFormMain');
        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const input = document.getElementById('chatInputMain');
                const text = input.value.trim();
                const receiverId = document.getElementById('targetUserId').value;
                const postId = document.getElementById('currentPostId').value;

                if (!text || !receiverId) return;

                input.value = '';

                try {
                    // 1. Lưu vào Database trước
                    const res = await fetch(this.config.sendMessageUrl, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            ReceiverId: parseInt(receiverId),
                            Message: text,
                            PostId: postId ? parseInt(postId) : null
                        })
                    });
                    const data = await res.json();

                    if (data.success) {
                        // Get post info from active context header if it exists
                        let postTitle = null, postPrice = null, postImage = null;
                        const context = document.getElementById('activePostContext');
                        if (context && postId) {
                            postTitle = context.querySelector('.fw-800').innerText.replace('Ngữ cảnh: ', '');
                            const priceText = context.querySelector('.text-primary').innerText.replace(' ₫', '').replace(/\./g, '');
                            postPrice = parseFloat(priceText);
                            postImage = context.querySelector('img').src.split('/').pop();
                        }

                        // 2. Gửi qua SignalR để người kia nhận được ngay
                        await this.connection.invoke("SendMessageToUser", parseInt(receiverId), text,
                            postId ? parseInt(postId) : null,
                            postTitle, postPrice, postImage, data.id);

                        // Update sidebar last msg
                        this.updateSidebarLastMsg(receiverId, text, true);

                        // Render tin nhắn của mình ngay lập tức (hoặc đợi MessageSent)
                        // Do đã có MessageSent handler nên không cần render tay ở đây.
                        // Tuy nhiên, MessageSent cần có ID để thu hồi.
                        // Ta có thể thêm ID vào dữ liệu gửi qua Hub hoặc đợi Hub trả về.

                        // QUAN TRỌNG: Tự động đóng ngữ cảnh sau tin nhắn đầu tiên
                        if (postId) {
                            this.closePostContext();
                        }
                    }
                } catch (err) {
                    console.error("Send error:", err)
                }
            });
        }
    },

    openConversation: async function (userId, username, avatar) {
        this.activeUserId = userId;
        document.getElementById('targetUserId').value = userId;

        // Update Active Class in Sidebar
        document.querySelectorAll('.chat-item').forEach(el => el.classList.remove('active'));
        const activeItem = document.querySelector(`.chat-item[data-user-id="${userId}"]`);
        if (activeItem) activeItem.classList.add('active');

        // Chỉ đóng ngữ cảnh sản phẩm nếu người dùng chủ động chuyển sang cuộc hội thoại khác từ sidebar
        // (Khi load lần đầu từ URL, username và avatar sẽ không được truyền vào)
        if (username || avatar) {
            this.closePostContext();
        }

        // Show chat area
        document.getElementById('welcomeScreen').classList.add('d-none');
        document.getElementById('chatContent').classList.remove('d-none');
        document.getElementById('chatContent').classList.add('d-flex');

        if (username) document.getElementById('activeUserName').innerText = username;
        if (avatar) {
            document.getElementById('activeUserAvatar').innerHTML = `<img src="${avatar}" class="rounded-circle" width="48" height="48" style="object-fit: cover; border: 2px solid #fff; box-shadow: 0 2px 10px rgba(0,0,0,0.1);" />`;
        }

        // Load History
        const messagesList = document.getElementById('messagesList');
        messagesList.innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary spinner-border-sm"></div></div>';

        try {
            const res = await fetch(`${this.config.getMessagesUrl}?otherUserId=${userId}`);
            const data = await res.json();
            if (data.success) {
                this.renderMessages(data.messages);
            }
        } catch (e) {
            console.error("Load history error:", e);
        }
    },

    renderMessages: function (messages) {
        const list = document.getElementById('messagesList');
        list.innerHTML = '';
        if (!messages || messages.length === 0) {
            list.innerHTML = '<div class="text-center text-muted my-5 small">Bắt đầu cuộc trò chuyện mới</div>';
            return;
        }

        messages.forEach(m => {
            this.appendMessage({
                senderId: m.senderId,
                text: m.text,
                createdAt: m.createdAt,
                postId: m.postId,
                postTitle: m.postTitle,
                postPrice: m.postPrice,
                postImage: m.postImage,
                id: m.id // Pass ID for recall
            });
        });
        this.scrollToBottom();
    },

    appendMessage: function (msg) {
        const list = document.getElementById('messagesList');
        const isMe = msg.senderId === this.config.currentUserId;
        const isRecalled = msg.text === "[MESSAGE_RECALLED]";
        const time = new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        const div = document.createElement('div');
        div.className = `${isMe ? 'chat-sender-container' : 'chat-receiver-container'} message-wrapper animate__animated animate__fadeInUp`;
        if (msg.id) div.setAttribute('data-id', msg.id);

        let postHtml = '';
        if (msg.postId && msg.postTitle && !isRecalled) {
            const displayPrice = msg.postPrice ? `${new Intl.NumberFormat('vi-VN').format(msg.postPrice)} ₫` : '';
            const displayImage = msg.postImage ? `/uploads/marketplace/${msg.postImage}` : '/uploads/marketplace/default.png';

            postHtml = `
                <a href="/Marketplace/MarketplacePosts/Details/${msg.postId}" class="bubble-post-card-link" target="_blank">
                    <div class="bubble-post-card">
                        <img src="${displayImage}" alt="${this.escapeHtml(msg.postTitle)}" />
                        <div class="bubble-post-info">
                            <div class="title">${this.escapeHtml(msg.postTitle)}</div>
                            <div class="price">${displayPrice}</div>
                        </div>
                    </div>
                </a>
            `;
        }

        let bubbleContent = isRecalled
            ? '<div class="recalled-wrapper"><i class="bi bi-arrow-counterclockwise me-2"></i><span>Tin nhắn đã được thu hồi</span></div>'
            : `<div class="message-text">${this.escapeHtml(msg.text)}</div>`;

        let recallBtn = (isMe && !isRecalled && msg.id)
            ? `<button class="recall-btn" onclick="ChatApp.recallMessage(${msg.id})" title="Thu hồi"><i class="bi bi-arrow-counterclockwise"></i></button>`
            : '';

        div.innerHTML = `
            <div class="message-bubble ${isMe ? 'chat-sender' : 'chat-receiver'} ${isRecalled ? 'recalled' : ''}">
                ${postHtml}
                ${bubbleContent}
            </div>
            ${recallBtn}
            <span class="chat-time ${isMe ? 'text-end' : 'text-start'}">${time}</span>
        `;

        list.appendChild(div);
    },

    recallMessage: async function (messageId) {
        Swal.fire({
            title: 'Thu hồi tin nhắn?',
            text: "Bạn có chắc chắn muốn thu hồi tin nhắn này không?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Thu hồi',
            cancelButtonText: 'Hủy',
            reverseButtons: true,
            customClass: {
                popup: 'premium-swal-popup animate__animated animate__fadeInDown animate__faster',
                confirmButton: 'premium-swal-confirm',
                cancelButton: 'premium-swal-cancel'
            },
            showClass: {
                popup: 'animate__animated animate__fadeInDown animate__faster'
            },
            hideClass: {
                popup: 'animate__animated animate__fadeOutUp animate__faster'
            }
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    const res = await fetch(`${this.config.recallMessageUrl}?messageId=${messageId}`, { method: 'POST' });
                    const data = await res.json();
                    if (data.success) {
                        // Invoke SignalR to notify other party
                        const receiverId = document.getElementById('targetUserId').value;
                        await this.connection.invoke("RecallMessage", parseInt(messageId), parseInt(receiverId));
                    }
                } catch (e) {
                    console.error("Recall error:", e);
                }
            }
        });
    },

    updateSidebarUnread: function (senderId, text) {
        const item = document.querySelector(`.chat-item[data-user-id="${senderId}"]`);
        if (item) {
            // Hiển thị chấm đỏ
            let dot = item.querySelector('.unread-dot');
            if (!dot) {
                dot = document.createElement('div');
                dot.className = 'unread-dot';
                item.querySelector('.position-relative').appendChild(dot);
            }
            dot.style.display = 'block';
            this.updateSidebarLastMsg(senderId, text, false);
        } else {
            // Nếu người gửi chưa có trong danh sách (hội thoại mới hoàn toàn)
            // Có thể reload danh sách hội thoại hoặc thêm động
            location.reload();
        }
    },

    updateSidebarLastMsg: function (userId, text, isMe) {
        const item = document.querySelector(`.chat-item[data-user-id="${userId}"]`);
        if (item) {
            const lastMsgEl = item.querySelector('.chat-item-last-msg');
            if (lastMsgEl) {
                const displayMsg = text === "[MESSAGE_RECALLED]" ? "Tin nhắn đã bị thu hồi" : text;
                lastMsgEl.innerText = (isMe ? 'Bạn: ' : '') + displayMsg;
            }

            // Đưa hội thoại lên đầu danh sách
            const list = document.getElementById('conversationList');
            list.prepend(item);
        }
    },

    scrollToBottom: function () {
        const list = document.getElementById('messagesList');
        if (list) {
            setTimeout(() => {
                list.scrollTop = list.scrollHeight;
            }, 50);
        }
    },

    closePostContext: function () {
        const context = document.getElementById('activePostContext');
        if (context) {
            context.classList.remove('animate__fadeInDown');
            context.classList.add('animate__fadeOutUp');
            setTimeout(() => {
                context.remove();
                // Xóa cả trong input hidden để các tin nhắn sau không bị đính kèm postId này
                const input = document.getElementById('currentPostId');
                if (input) input.value = '';
            }, 500);
        }
    },

    escapeHtml: function (unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
};

// chatbot.js - Logic xử lý Chatbot AI (Pink-Purple Messenger UI)
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

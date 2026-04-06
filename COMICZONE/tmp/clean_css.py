import sys
path = r'd:\University\IT\AnGiang_University\Nam_4\Nam_4_HK2\thuc_tap\tai_lieu_tren_lop\website\COMICZONE\COMICZONE\wwwroot\css\site.css'
try:
    with open(path, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Chúng ta muốn giữ từ dòng 1 (index 0) đến 2688 (index 2687)
    # Và bỏ từ dòng 2689 (index 2688) đến 2785 (index 2784)
    # Sau đó giữ tiếp từ dòng 2786 (index 2785) trở đi.
    new_lines = lines[:2688] + ["/* ================= AI CHATBOT STYLES MOVED ================= */\n"] + lines[2785:]
    
    with open(path, 'w', encoding='utf-8') as f:
        f.writelines(new_lines)
    print("SUCCESS: site.css has been cleaned.")
except Exception as e:
    print(f"ERROR: {str(e)}")

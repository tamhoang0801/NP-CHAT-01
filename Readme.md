# NP-CHAT-01 - Chat Application

## Thông tin Nhóm 11

- Thành viên 1:
- Thành viên 2:
- Thành viên 3:
- Thành viên 4:

## Mô tả

Ứng dụng chat TCP/IP sử dụng C# Windows Forms (.NET). Hỗ trợ:
- Public chat (phòng chat chung)
- Private chat (nhắn tin riêng giữa hai người dùng)
- Multi-session (nhiều phòng chat / nhiều cuộc trò chuyện riêng)
- Gửi ảnh và video qua kết nối TCP
- Chọn emoji trong tin nhắn
- Danh sách người dùng online

## Cấu trúc project

```
NP-CHAT-01-main/
├── Code/
│   ├── ChatClient/               # Client - Ứng dụng WinForms
│   │   ├── ChatManager.cs        # Quản lý danh sách session chat
│   │   ├── ChatSession.cs        # Một phiên chat (lịch sử, RTF)
│   │   ├── EmojiPickerForm.cs    # Bảng chọn emoji
│   │   ├── FrmChat.cs            # Form chat chính
│   │   ├── FrmChat.Designer.cs   # Designer cho FrmChat
│   │   ├── FrmLogin.cs           # Form đăng nhập
│   │   ├── FrmLogin.Designer.cs  # Designer cho FrmLogin
│   │   ├── Program.cs            # Entry point
│   │   └── ThanhVien3-GUI.csproj # Project file
│   └── ChatServer/               # Server - Ứng dụng Console/WinForms
│       └── ChatServerApp/
│           ├── Form1.cs          # Form server chính + logic TCP
│           ├── Form1.Designer.cs # Designer cho Form1
│           ├── Program.cs        # Entry point
│           └── ChatServerApp.csproj
├── DOCX/                         # Tài liệu báo cáo
└── Readme.md                     # File này
```

## Công nghệ

- C# Windows Forms
- .NET 10 (Client) / .NET Framework 4.7.2 (Server)
- TCP/IP Sockets

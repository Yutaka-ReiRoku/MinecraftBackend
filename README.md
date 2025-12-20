# 🎮 Voxel Game Fullstack

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![Unity Version](https://img.shields.io/badge/Unity-6000.0.63f1-000000.svg?style=for-the-badge&logo=unity)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)

**Voxel Game Fullstack** là một dự án kết hợp gồm hệ thống máy chủ cung cấp API (Backend) và một game client 3D. Dự án mang đến nền tảng xử lý dữ liệu mạnh mẽ, quy trình xác thực người dùng an toàn và sự kết nối đồng bộ trực tiếp với Unity Game.

---

## ✨ Tính Năng Nổi Bật (Features)

### 🛡️ Hệ Thống Máy Chủ (Core Backend API)
*   **Xác Thực & Bảo Mật:** Hệ thống xác thực bằng kiến trúc JSON Web Tokens (JWT Bearer) kết hợp quy chuẩn mã hóa mật khẩu an toàn BCrypt.
*   **Quản Lý Cơ Sở Dữ Liệu:** Xử lý luồng lưu trữ qua cơ sở dữ liệu cục bộ SQLite, tích hợp kiến trúc Entity Framework Core 8.0 (Code-First & Migrations tự động).
*   **Mô hình Kiến Trúc:** Triển khai theo chuẩn cấu trúc ASP.NET Core Web API / MVC, tách biệt logic rõ ràng và mạnh mẽ để có thể mở rộng endpoints một cách dễ dàng.

### 🕹️ Môi Trường Giao Diện (Game Client)
*   **Nền Tảng Đồ Họa:** Client quản lý game trên Unity 6 mang lại hiệu suất tốt và quy trình dựng hình mạnh mẽ.
*   **Tương Tác API:** Có thiết kế giao thức kết nối nhằm gửi dữ liệu lên HTTP Server từ Unity một cách mượt mà.

---

## 📂 Kiến Trúc & Cấu Trúc Mã Nguồn

Dự án được phân chia độc lập, giúp việc quản lý mã nguồn (Source Control) cực kỳ linh hoạt:

*   `WEB/`: Môi trường ứng dụng máy chủ (.NET 8). Bao gồm các hệ thống xử lý Request (`Controllers`), Cấu trúc Database (`Models`, `Data`) và tập tin dữ liệu gốc (`minecraft.db`).
*   `GAME/`: Môi trường lập trình của Unity. Chứa hệ thống cấu hình Assets, đồ họa, và các Component/Scripts kết nối người chơi tới máy chủ.

---

## 🛠️ Yêu Cầu Cài Đặt (Requirements)

1.  **.NET SDK:** Phiên bản `8.0` cho môi trường máy chủ.
2.  **Unity Editor:** Phiên bản chính xác `6000.0.63f1` (Unity 6).
3.  **Công cụ EF (Tùy chọn):** `dotnet-ef` để tương tác trực diện vào thiết kế Database thông qua giao diện Terminal.

---

## 📌 Hướng Dẫn Nhanh (Quick Start)

### 💻 1. Khởi Chạy Backend (Server)

1. Mở Terminal / PowerShell và đi tới đúng thư mục gốc của Back-end API:
```bash
cd WEB/MinecraftBackend/MinecraftBackend
```
2. Phục hồi cấu hình package và tiến hành kích hoạt/cập nhật cơ sở dữ liệu:
```bash
dotnet restore
dotnet ef database update
```
3. Chạy hệ thống Backend:
```bash
dotnet run
```
*Server sẽ bắt đầu chạy và lắng nghe ở các cổng mặc định. Bạn có quyền được khai thác endpoints hoặc kiểm thử trên giao diện.*

### 🎮 2. Khởi Chạy Game Client (Unity)

1. Mở Unity Hub, chọn **Add Project** (hoặc Open) và trỏ thẳng vào thư mục: `GAME/MinecraftBackend`.
2. Mở trực tiếp bằng Editor `6000.0.63f1`.
3. Tùy chỉnh (nếu cần) URL trỏ về Back-end (dạng `https://localhost:<port>`). Khởi chạy Scene chính và nhấn **Play**.

---

*Dự án thuộc bản quyền phát triển bởi [Yutaka-ReiRoku](https://github.com/Yutaka-ReiRoku) & [Reider25](https://github.com/Reider25).*

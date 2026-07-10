# Cấu Trúc Thư Mục Dự Án GunShooter (Assets Folder Structure)

Tài liệu này lưu lại sơ đồ cấu trúc thư mục của dự án sau khi đã được tối ưu hóa và dọn dẹp. Toàn bộ tài nguyên bên thứ ba (Third-Party Assets) đã được gom gọn vào thư mục `Assets/ThirdParty/` để giữ cho thư mục gốc của dự án luôn sạch sẽ.

---

## 📂 Sơ Đồ Cấu Trúc Thư Mục Gốc (`Assets/`)

* **`Project Data/`** (Thư mục cốt lõi của game):
  * Chứa toàn bộ source code (C# scripts), cấu hình hệ thống, dữ liệu nhân vật, súng, màn chơi, UI và prefabs cốt lõi của dự án.
* **`ThirdParty/`** (Tất cả tài nguyên bên thứ ba - Xem chi tiết bên dưới):
  * Gom gọn các asset gói mua/tải về từ Unity Asset Store.
* **`Plugins/`** (Các thư viện bổ trợ):
  * Chứa các plugin bổ trợ biên dịch trước cho Unity.
* **`Scenes/`** (Danh sách cảnh game):
  * Chứa các file màn chơi chính (.unity) để mở chạy thử hoặc chỉnh sửa màn chơi.
* **`Settings/`** (Thiết lập đồ họa & Render):
  * Chứa cấu hình đồ họa Universal Render Pipeline (URP).
* **`URPDefaultResources/`** (Tài nguyên mặc định URP):
  * Các shader và material mặc định đi kèm với hệ thống render URP.

---

## 📦 Danh Sách Chi Tiết Thư Mục Trong `Assets/ThirdParty/`

Dưới đây là vị trí mới của các thư mục tài nguyên bên thứ ba để anh tiện tìm kiếm:

| Tên Thư Mục Gốc | Vị Trí Mới | Công Dụng / Mô Tả |
| :--- | :--- | :--- |
| `ATART` | `Assets/ThirdParty/ATART/` | Mô hình và trang phục của Nhân vật chính. |
| `Farland Skies` | `Assets/ThirdParty/Farland Skies/` | Hiệu ứng bầu trời (Skybox), vũ trụ, mây trời trong game. |
| `Hovl Studio` | `Assets/ThirdParty/Hovl Studio/` | Hiệu ứng đạn bắn, tia laser, hiệu ứng nổ phép thuật. |
| `Magic Pig Games (Infinity PBR)` | `Assets/ThirdParty/Magic Pig Games (Infinity PBR)/` | Dữ liệu âm thanh, tiếng súng, tiếng động vật và ngoại trang nhân vật. |
| `NamuFX` | `Assets/ThirdParty/NamuFX/` | Thư viện hiệu ứng hạt VFX (Stylized VFX Doodles). |
| `NamuFX_Slash` | `Assets/ThirdParty/NamuFX_Slash/` | Thư viện hiệu ứng chém kiếm (Simple Stylized Slash vol2). |
| `PolygonCyberCity` | `Assets/ThirdParty/PolygonCyberCity/` | Gói mô hình 3D thành phố tương lai của hãng Synty. |
| `PolygonMech` | `Assets/ThirdParty/PolygonMech/` | Gói mô hình robot và chiến giáp cơ khí của hãng Synty. |
| `PolygonMilitary` | `Assets/ThirdParty/PolygonMilitary/` | Gói mô hình quân đội, vũ khí quân sự của hãng Synty. |
| `Space_Exploration_GUI_Kit` | `Assets/ThirdParty/Space_Exploration_GUI_Kit/` | Bộ giao diện người dùng (GUI) phong cách không gian/viễn tưởng. |
| `TutorialInfo` | `Assets/ThirdParty/TutorialInfo/` | Thư mục thông tin hướng dẫn mặc định của Unity. |
| `Vefects` | `Assets/ThirdParty/Vefects/` | Thư viện các hiệu ứng kỹ năng vật lý/phép thuật. |
| `vFavorites` | `Assets/ThirdParty/vFavorites/` | Plugin hỗ trợ quản lý thư mục yêu thích trong Editor. |
| `_Recovery` | `Assets/ThirdParty/_Recovery/` | Các cảnh game tự động sao lưu dự phòng của Unity. |

---

> [!TIP]
> **Lưu ý quan trọng:** Toàn bộ liên kết vật liệu (materials), prefab và cảnh (scenes) trong game đã được tự động bảo toàn thông qua việc di chuyển file `.meta` tương ứng. Khi cần tìm hiệu ứng hay mô hình, anh cứ vào thư mục **`Assets/ThirdParty/`** là thấy đầy đủ!

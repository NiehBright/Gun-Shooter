# 🔫 GunShooter (Squad Shooter)

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg?style=for-the-badge&logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/Language-C%23-green.svg?style=for-the-badge&logo=c-sharp)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-purple.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)
[![Developer](https://img.shields.io/badge/Developer-NiehBright-orange.svg?style=for-the-badge&logo=github)](https://github.com/NiehBright)

Một tựa game **Squad Shooter** phiêu lưu bắn súng góc nhìn từ trên xuống (Top-down Shooter) được phát triển trên engine **Unity**. Đây là một dự án cá nhân (Solo Dev) được đầu tư công phu về cả lập trình hệ thống, tối ưu hóa hiệu năng, và thiết kế trải nghiệm người dùng (UX/UI).

---

## 🌟 Tính Năng Nổi Bật (Key Features)

### 🔮 Hệ Thống Kỹ Năng Kích Hoạt Độc Quyền (Active Skills)
* **Kỹ năng Hố đen (Black Hole Skill) của Levi:** Triệu hồi một hố đen trọng lực tại vị trí chỉ định. Hố đen sẽ tự động hút toàn bộ kẻ địch xung quanh vào tâm, đồng thời gây sát thương liên tục trước khi phát nổ.
* **Hệ thống cấu hình linh hoạt (Modular Skill Data):** Cho phép dễ dàng thiết lập phạm vi ảnh hưởng, lực hút, sát thương và thời gian hồi chiêu (cooldown) cho từng nhân vật thông qua `ScriptableObject`.

### 🛡️ Hệ Thống Trang Bị Tùy Biến Cao (Advanced Equipment System)
* **Phân tách Khung Độ Hiếm (Rarity Frames):** Tách biệt trực quan giữa Khung Trang Bị ở Sảnh (Slot Frame) và Khung phẩm chất vật phẩm (Common, Rare, Epic) giúp nâng cao trải nghiệm thị giác.
* **Công cụ Editor Tự Động Hóa (Active Item UI Builder):** Bộ công cụ trong Editor cho phép tự động sinh cấu trúc Prefab vật phẩm hoạt động, đồng bộ hóa sang TextMeshPro (TMP) sắc nét và tự động liên kết dữ liệu vào hệ thống chỉ bằng 1 click chuột.

### 🛠️ Bộ Công Cụ Thiết Kế Màn Chơi Tùy Biến (Custom Level Editor Tool)
* **Trình thiết kế màn chơi trực quan (Custom Level Editor Window):** Giao diện chỉnh sửa tích hợp sẵn trong Unity Editor giúp thiết kế và cấu hình toàn bộ các phòng chơi (Rooms), bố trí cổng dịch chuyển (Gates) và chướng ngại vật một cách nhanh chóng.
* **Bản đồ nhiệt mật độ quái (Enemy Heatmap Overlay):** Tính năng phân tích và hiển thị trực quan mật độ phân bổ của kẻ địch dưới dạng bản đồ nhiệt (Heatmap) ngay trong Editor. Giúp nhà phát triển cân bằng độ khó của màn chơi, điều chỉnh phân bổ nhịp độ chiến đấu một cách khoa học.
* **Cấu hình đường tuần tra (Patrolling Paths Setup):** Dễ dàng thiết lập các điểm di chuyển tuần tra (Waypoints) cho quái vật trực tiếp trên giao diện Editor kéo thả, tạo hành vi di chuyển chân thực cho kẻ địch ở Runtime.

### 🎮 Chế Độ Thử Sát Thương Tại Lobby (Lobby Combat Test Mode)
* **Bia Tập Bắn Bất Tử (Immortal Training Dummy):** Bật chế độ thử sát thương ngay tại sảnh chờ, cho phép người chơi bắn thử lên các Bia tập bắn (Dummy). Bia tập sẽ tự động hồi phục đầy máu khi lượng máu về 0 (`Regen!`) và đứng yên tại chỗ.
* **Tích hợp UI Chiến đấu mượt mà:** Tự động ẩn giao diện sảnh chính (`UIMainMenu`) khi kích hoạt chế độ thử để tránh nhấn nhầm và hiển thị đầy đủ HUD chiến đấu (`UIGame`) gồm Joystick, Dash và các nút dùng chiêu.

### ⚡ Tối Ưu Hóa & Trải Nghiệm Người Dùng (UX/UI & Optimization)
* **Nâng cấp TextMeshPro (TMP):** Toàn bộ nhãn chữ của trang bị, tên vật phẩm và cấp độ được chuyển đổi sang TextMeshPro giúp hiển thị sắc nét ở mọi độ phân giải.
* **Vùng Tương Tác VFX Mở UI (UI Interaction Zone):** Tự động phát hiện khi người chơi bước vào vùng chỉ định ở Sảnh (Lobby) để mở các bảng nâng cấp vũ khí/trang bị đi kèm hiệu ứng VFX vòng tròn phát sáng dưới chân cực kỳ mượt mà.
* **Sạch mã nguồn (Clean Code & Warning-free):** Đã sửa toàn bộ các cảnh báo lỗi thời (`Obsolete Warning CS0618`) của Unity, tối ưu hóa các hàm tìm kiếm đối tượng (`FindAnyObjectByType`), đảm bảo dự án biên dịch sạch sẽ.

---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

* **Engine:** Unity (Hỗ trợ tốt nhất từ phiên bản 2022.3 LTS trở lên)
* **Ngôn ngữ:** C# (Cập nhật các tính năng và tiêu chuẩn tối ưu hóa mới nhất)
* **UI System:** Unity UI (uGUI) kết hợp **TextMeshPro** để hiển thị văn bản chất lượng cao.
* **Architecture:** Mô hình Singleton kết hợp cấu trúc hướng dữ liệu (Data-driven) với ScriptableObject giúp dễ dàng mở rộng nội dung game mà không cần can thiệp sâu vào code core.

---

## 🚀 Hướng Dẫn Cài Đặt (Quick Setup)

1. Tải dự án về máy hoặc clone repository:
   ```bash
   git clone https://github.com/NiehBright/Gun-Shooter.git
   ```
2. Mở dự án bằng **Unity Hub** với phiên bản Unity tương ứng.
3. Tìm và mở Scene chính tại:
   `Assets/Project Data/Game/Scenes/Game.unity`
4. Ấn nút **Play** trong Unity Editor để trải nghiệm trực tiếp!

### ⚙️ Các Công Cụ Tiện Ích Trong Editor (Editor Tools)
Dự án tích hợp sẵn các công cụ tự động hóa trong menu **Tools**:
* **Tools $\rightarrow$ Equipment $\rightarrow$ Create and Link Active Item Prefab:** Tự động dựng cấu trúc mẫu và liên kết Prefab Vật phẩm hoạt động vào kho đồ.
* **Tools $\rightarrow$ Equipment $\rightarrow$ Integrate Lobby Combat UI:** Tự động gắn các nút bật/tắt chế độ Thử Sát Thương vào các prefab UI hệ thống chính xác.

---

## 👤 Thông Tin Tác Giả (About the Author)

Dự án được phát triển và hoàn thiện độc lập (Solo Dev) bởi:

* **Họ và tên:** Nguyễn Thanh Hiền (Nieh)
* **GitHub:** [NiehBright](https://github.com/NiehBright)
* **Facebook:** [Nguyễn Thanh Hiền (Nieh)](https://www.facebook.com/Nieh.1608/)
* **Email:** thanhhiengamedev@gmail.com *(Hoặc email công việc của bạn)*

---

## 📄 Bản Quyền (License)

Dự án này được phân phối dưới giấy phép **MIT License** - xem tệp `LICENSE` để biết thêm chi tiết.

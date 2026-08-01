<div align="center">

<img src="https://readme-typing-svg.herokuapp.com?font=JetBrains+Mono&size=28&pause=1000&color=00D4FF&center=true&vCenter=true&width=600&lines=🎮+GunShooter+(Squad+Shooter);Unity+3D+Top-Down+Action;Dự+án+Solo+Dev+bởi+Nieh" alt="Typing SVG" />

<br/>

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-Language-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Windows](https://img.shields.io/badge/Windows-Platform-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Made with ❤️](https://img.shields.io/badge/Made%20with-%E2%9D%A4%EF%B8%8F-red?style=for-the-badge)](https://github.com/NiehBright)

<br/>

> **Game bắn súng góc nhìn từ trên xuống (Top-down Shooter) mượt mà phát triển bằng Unity.**
> Tích hợp hệ thống Kỹ năng chủ động (Active Skills), Bia tập bắn bất tử (Dummy) tại Lobby và Bộ thiết kế màn chơi Custom Level Editor.

<br/>

![Demo Preview](https://via.placeholder.com/900x450/0d1117/00d4ff?text=🎮+GunShooter+Gameplay+Demo)

</div>

---

## 📖 Mục lục

- [✨ Tính năng nổi bật](#-tính-năng-nổi-bật)
- [🏗️ Kiến trúc hệ thống](#️-kiến-trúc-hệ-thống)
- [⚙️ Yêu cầu hệ thống](#️-yêu-cầu-hệ-thống)
- [🚀 Hướng dẫn cài đặt](#-hướng-dẫn-cài-đặt)
- [🛠️ Công cụ hỗ trợ Editor](#️-công-cụ-hỗ-trợ-editor)
- [❓ FAQ & Xử lý lỗi](#-faq--xử-lý-lỗi)
- [👨‍💻 Tác giả](#-tác-giả)
- [📄 License](#-license)

---

## ✨ Tính năng nổi bật

| Tính năng | Mô tả |
|-----------|-------|
| 🔮 **Hố Đen Trọng Lực (Levi)** | Triệu hồi hố đen trọng lực hút toàn bộ kẻ địch vào tâm và phát nổ gây sát thương diện rộng. |
| 🛠️ **Custom Level Editor** | Trình thiết kế màn chơi trực quan dạng kéo thả, tích hợp sẵn trong Unity Editor. |
| 🔥 **Bản Đồ Nhiệt (Heatmap)** | Visual hóa mật độ phân bổ quái vật dạng Heatmap màu ngay trong Editor để cân bằng độ khó. |
| 🎯 **Bia Tập Bắn Bất Tử** | Đặt quái ở Lobby và tích chọn `Is Dummy` để biến nó thành quái đứng yên, tự hồi đầy máu (`Regen!`) để test damage. |
| 🛡️ **Advanced Equipment UI** | Neo giữ khung ô sảnh trống và hiển thị động 3 khung Rarity (Common, Rare, Epic) của vật phẩm tùy biến. |
| 🖥️ **TextMeshPro (TMP)** | Nâng cấp toàn bộ hệ thống chữ chỉ số, cấp độ sang TMP sắc nét, dễ dàng thay đổi Font Asset. |
| ⚡ **Lobby Combat Mode** | Bấm nút ở sảnh để lập tức ẩn UI sảnh chính (tránh click nhầm) và bật UI HUD Game (Joystick, Dash, bắn). |
| 🚶 **UI Interaction Zone** | Vòng tương tác VFX dưới chân, bước vào tự động kích hoạt chuyển cảnh mở bảng nâng cấp. |
| 🧼 **Mã nguồn sạch sẽ** | Sửa sạch 100% cảnh báo biên dịch cũ (`Obsolete Warning CS0618`), tối ưu hóa hiệu năng Runtime. |

---

## 🏗️ Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────────────┐
│                      Lobby Combat System                        │
│                Architecture & Event Flow Diagram                │
└─────────────────────────────────────────────────────────────────┘

   ┌──────────────────┐    Click Button    ┌───────────────────────┐
   │   UIMainMenu     │ ─────────────────► │LobbyCombatController  │
   │ (Lobby UI Page)  │   (Deactivate)     │  (Runtime Controller) │
   └──────────────────┘                    └───────────┬───────────┘
                                                       │
                           ┌───────────────────────────┼───────────────────────────┐
                           │ (Activate Mode)           │ (Spawns / Manually Placed)│ (Deactivate Mode)
                           ▼                           ▼                           ▼
                   ┌──────────────┐            ┌──────────────┐            ┌──────────────┐
                   │   UIGame     │            │ BaseEnemy    │            │  UIMainMenu  │
                   │ (Gameplay HUD│            │ (IsDummy=true│            │ (Show Again) │
                   │  Joystick,   │            │  Stand Still,│            │              │
                   │  Dash, Skill)│            │  Immortal)   │            │  UIGame      │
                   └──────┬───────┘            └──────┬───────┘            │  (Hide)      │
                          │                           │                    └──────────────┘
                          ▼                           ▼
                   ┌──────────────────────────────────────────┐
                   │    Player Target & Auto Shoot Logic      │
                   │ CharacterBehaviour.IsLobbyModeActive=false│
                   │ EnemyDetector.TryAddClosestEnemy(dummy)  │
                   └──────────────────────────────────────────┘
```

**Tại sao kiến trúc này tối ưu?**
- **Tránh trùng lặp UI:** Khi vào chế độ Test, sảnh chính `UIMainMenu` tắt hoàn toàn, loại bỏ 100% xung đột click đè nút với các thanh nâng cấp.
- **Tận dụng tối đa Codebase:** Bằng cách tắt cờ `CharacterBehaviour.IsLobbyModeActive`, nhân vật tự rút súng, tự nhắm bắn và xả skill y hệt như đang đi phụ bản thật mà không cần viết lại logic AI điều khiển.
- **Bảo toàn dữ liệu màn chơi:** Bia tập bắn có cơ chế tự phục hồi máu mà không bao giờ trigger sự kiện `OnDeath()` để tránh gây lỗi cho hệ thống ghi nhận kết thúc màn chơi của Level Controller.

---

## ⚙️ Yêu cầu hệ thống

| Yêu cầu | Chi tiết |
|---------|-----------|
| **OS** | Windows 10 / 11 hoặc macOS |
| **Unity Version** | Unity 2022.3 LTS hoặc mới hơn |
| **Render Pipeline** | Universal Render Pipeline (URP) |
| **Input System** | Keyboard & Mouse / Virtual Joystick |

---

## 🚀 Hướng dẫn cài đặt

### Bước 1: Clone repository

```bash
git clone https://github.com/NiehBright/Gun-Shooter.git
cd Gun-Shooter
```

### Bước 2: Nhập dự án vào Unity

1. Mở **Unity Hub** lên.
2. Nhấn **Add** $\rightarrow$ chọn thư mục dự án vừa tải về.
3. Chọn phiên bản **Unity 2022.3 LTS** (hoặc mới hơn) để mở dự án.
4. Chờ Unity nhập tài nguyên và biên dịch các package ban đầu (mất khoảng 2-5 phút).

### Bước 3: Chạy Scene chính

1. Trong ô tìm kiếm Project, mở thư mục scene:
   `Assets/Project Data/Game/Scenes/`
2. Double-click mở scene **`Game.unity`**.
3. Bấm nút **Play** (hình tam giác phát) để chạy thử game.

---

## 🛠️ Công cụ hỗ trợ Editor (Editor Tools)

Dự án có xây dựng sẵn các công cụ trong Editor giúp tự động hóa thiết lập UI và màn chơi. Anh/chị có thể tìm thấy chúng trên thanh công cụ menu phía trên:

| Menu Path | Chức năng |
|-----------|-----------|
| **Tools $\rightarrow$ Equipment $\rightarrow$ Create and Link Active Item Prefab** | Tự động dựng khung prefab mẫu cho vật phẩm mới và tự liên kết nó vào các hệ thống quản lý. |
| **Tools $\rightarrow$ Equipment $\rightarrow$ Integrate Lobby Combat UI** | Tự động gắn các nút bật/tắt chế độ Thử Sát Thương trực tiếp vào cấu trúc của Prefab UI Main Menu và UI Game. |

---

## ❓ FAQ & Xử lý lỗi

<details>
<summary><b>❌ Không nhìn thấy nút "Thử Sát Thương" ở Lobby sảnh chờ</b></summary>

- Đảm bảo anh/chị đã nhấn chạy công cụ tích hợp nút trong Editor trước khi Play game: **Tools** $\rightarrow$ **Equipment** $\rightarrow$ **Integrate Lobby Combat UI**.
- Nút này chỉ xuất hiện khi sảnh chính hoạt động và sẽ tự động ẩn đi khi anh/chị mở các bảng nâng cấp con để tránh đè giao diện.

</details>

<details>
<summary><b>❌ Làm thế nào để tự tạo một Bia tập bắn ở Lobby?</b></summary>

1. Kéo thả bất kỳ quái vật nào từ thư mục Prefab vào vị trí mong muốn trong khu vực Lobby của scene.
2. Click chọn con quái đó, nhìn sang bảng **Inspector** $\rightarrow$ tìm component quái.
3. Tích chọn vào ô **`Is Dummy`** (Biến cờ này đã được serialize).
4. Khi chạy game, con quái đó sẽ tự động đứng yên và có máu bất tử để bắn thử.

</details>

<details>
<summary><b>❌ Lỗi font chữ hiển thị (ô vuông hoặc mất ký tự) trên ô hiển thị Sao</b></summary>

- Khác với cấp độ và chỉ số đã được nâng cấp sang **TextMeshPro**, phần hiển thị sao của nhân vật (`CharStarsText`) bắt buộc phải dùng **UnityEngine.UI.Text thường** để hiển thị chuẩn xác ký tự ngôi sao đặc biệt Unicode (`★`). Công cụ dựng prefab đã được cập nhật để tự động bảo toàn Text thường này.

</details>

<details>
<summary><b>❌ Level Editor Window bị treo hoặc báo lỗi chỉ số mảng</b></summary>

- Lỗi tràn chỉ số mảng cũ khi load quái trong Level Editor đã được khắc phục hoàn chỉnh trong script `LevelEditorWindow.cs`. Hãy đảm bảo anh/chị chọn đúng data level hợp lệ trên cửa sổ Editor.

</details>

---

## 👨‍💻 Tác giả

<div align="center">

<img src="https://avatars.githubusercontent.com/NiehBright" width="120" style="border-radius: 50%;" alt="Nguyễn Thanh Hiền"/>

### Nguyễn Thanh Hiền — *Nieh*

**Solo Developer** · Indie Game Enthusiast · Full-Stack Hobbyist

*"Làm vì đam mê, code để giải quyết vấn đề thực tế."*

<br/>

[![GitHub](https://img.shields.io/badge/GitHub-NiehBright-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/NiehBright)
[![Facebook](https://img.shields.io/badge/Facebook-Nguyễn%20Thanh%20Hiền-1877F2?style=for-the-badge&logo=facebook&logoColor=white)](https://www.facebook.com/Nieh.1608/)

</div>

<br/>

> Dự án game bắn súng này được tôi thiết kế và phát triển hoàn toàn **một mình** (*solo dev*) với mục tiêu học hỏi, cải tiến các bộ công cụ Editor của Unity và tự tay xây dựng các cơ chế gameplay mượt mà nhất.
>
> Nếu bạn thấy dự án hữu ích hoặc học hỏi được điều gì đó từ source code, hãy ⭐ **Star** repo để ủng hộ mình nhé!

---

## 📄 License

Phân phối dưới [MIT License](LICENSE). Bạn được phép sử dụng, sao chép, sửa đổi và phân phối tự do.

---

<div align="center">

**Made with ❤️ by [Nieh](https://github.com/NiehBright) · Vietnam 🇻🇳**

*Nếu gặp vấn đề trong quá trình chạy source code, hãy mở Issue — tôi sẽ hỗ trợ sớm nhất có thể!*

</div>

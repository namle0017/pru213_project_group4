# 🗺️ BẢN ĐỒ CÔNG VIỆC: GAME FLOW

## ✅ 1. Xây dựng Scene Manager (Chuyển & Load Scene)
- **Hiện trạng:** Đã mở rộng file `StartGame.cs`.
- **Nhiệm vụ:** Cung cấp hàm `LoadMainMenu()` để về menu, và `QuitGame()` để thoát game.
- **Tình trạng:** Đã hoàn thành.

## ✅ 2. Xây dựng Tính năng Restart (Chơi lại)
- **Nhiệm vụ:** Cho phép chơi lại màn hiện tại sau khi Game Over.
- **Tình trạng:** Đã hoàn thành (đã thêm hàm `RestartCurrentScene()` vào `StartGame.cs`).

## ✅ 3. Xây dựng Pause System (Tạm dừng Game)
- **Nhiệm vụ:** Đóng băng và tiếp tục thời gian trong game (`Time.timeScale`), kết hợp ẩn/hiện giao diện tạm dừng.
- **Tình trạng:** Đã hoàn thành (đã tạo `PauseManager.cs`).

## ✅ 4. Nâng cấp Save/Load (Lưu và tải dữ liệu)
- **Nhiệm vụ:** Mở rộng hệ thống để lưu **Tổng số Tiền Vàng (Total Coins)** mà người chơi cày cuốc được qua nhiều ván chơi.
- **Logic:**
  - Cộng số Coin ván này vào Tổng Coin cũ.
  - Lưu xuống máy bằng `PlayerPrefs`.
  - Đọc Tổng Coin cũ nạp vào hệ thống mỗi khi Load.
- **Tình trạng:** Đã hoàn thành.

---
**🎉 CHÚC MỪNG! MODULE GAME FLOW ĐÃ HOÀN THÀNH 100% 🎉**

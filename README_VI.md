# BÍ MẬT KHU DI TÍCH — Ruins of Aster

Bản nâng cấp mỹ thuật trên project gốc • 23/10/2026 • Unity 6000.3.26f1 • Built-in Render Pipeline.

Đây là project Unity riêng cho đề tài game khám phá di tích, không phải SchoolEscape. Giữ nguyên thư mục này để chỉnh dần sau mỗi lần test; không cần tạo lại project.

## Nâng cấp lần này

Đã tích hợp model CC0 KayKit/Kenney thật, phối lại cảnh và UI xanh ngọc–vàng, thêm ảnh vật phẩm, camera menu, safe area và ánh sáng trại. Đọc **Documentation/ART_UPDATE.md** để cập nhật vào project đang giữ.

## Mở project

1. Giải nén ZIP. Trong Unity Hub chọn **Add → Add project from disk**, chọn thư mục **RuinsOfAster** chứa `Assets`, `Packages`, `ProjectSettings`.
2. Mở bằng **Unity 6000.3.26f1**. Lần đầu cần Internet để Unity Package Manager tải Input System, AR Foundation, ARCore và XR Management. Cài Android Build Support + SDK/NDK/OpenJDK nếu muốn build Android.
3. Đợi import hoàn tất. Script Editor tạo **Assets/Scenes/Main.unity** một lần, đặt model, vật liệu, ánh sáng và cấu hình thẻ AR. Scene được tạo thành các đối tượng chỉnh sửa được.
4. Nếu Main chưa xuất hiện, xem Console; sau khi không còn lỗi compile, chọn **Aster → Prepare project (preserves Main scene)**. Mở Main, nhấn Play.
5. Chọn **Bắt đầu hành trình**. Dùng màn hình Game tỷ lệ 16:9 ngang, ví dụ 1600 × 900. Click màn hình Game để nhận chuột/bàn phím.

**Giới hạn kiểm chứng:** môi trường tạo file không có Unity Editor. Đã kiểm tra cú pháp và tài sản của bản mới; luật chơi giữ nguyên từ bản đã kiểm tra độc lập, nhưng chưa chạy Play Mode, chưa build APK và chưa thử AR trên máy thật. Đọc `Documentation/VALIDATION.md`. Đây là bản nguồn để mở test và tinh chỉnh, chưa phải bản phát hành đã QA.

## Nội dung có trong mã

- Góc nhìn thứ nhất, xoay tự do 360°, WASD/chuột và điều khiển cảm ứng.
- Bản đồ compact khoảng 54 × 65 m với trại, cổng, sân trung tâm, khu gương, kênh nước, xưởng và thánh đường.
- Chuỗi 4 câu đố có khóa tiến trình; vật phẩm, túi đồ, nhật ký, 3 ghi chép phụ, gợi ý và đèn pin.
- Quan sát đĩa đá bằng trình xem 3D trên PC hoặc nhận diện một thẻ AR trên Android. Hai chế độ được ghi nhận riêng, không tính xem 3D là AR.
- Cảnh kết thúc khi hồ nước được khôi phục; thống kê thời gian, câu đố, ghi chép, gợi ý và sử dụng AR.
- Tự lưu JSON sau tương tác và mỗi 30 giây chơi, có bản sao `.bak`, tiếp tục từ checkpoint.
- Nhạc nền nguyên bản tổng hợp, môi trường rừng, tiếng bước chân, đá, bánh răng, van, cửa, thành công/thất bại và âm thanh không gian ở máy/guồng nước.

## Điều khiển

| PC | Chức năng |
|---|---|
| WASD / chuột | Di chuyển / nhìn |
| Shift | Đi nhanh |
| E | Tương tác trong tầm 3,1 m |
| I hoặc Tab | Túi đồ |
| F | Bật/tắt đèn sau khi nhặt |
| Esc | Tạm dừng |

Android: cần trái để di chuyển, vuốt vùng phải để nhìn; nút Tương tác, Túi đồ, Nhật ký, Đèn và Gợi ý trên HUD. Không có nhảy nhằm giữ các chốt câu đố.

## AR

1. In `Documentation/AR_Card.png` **rộng 15 cm, cao 15 cm**, không cắt và không co tự động vừa trang. Có thể xem trên màn hình thứ hai để thử, nhưng in mờ ít phản chiếu sẽ ổn định hơn.
2. Unity: chạy menu Prepare; kiểm tra Project Settings → XR Plug-in Management → Android → ARCore. Mã Editor tạo Image Library với kích thước 0,15 × 0,15 m.
3. Build Android ARM64, IL2CPP, OpenGLES3, min API 24; yêu cầu thiết bị tương thích ARCore. Resolve các cảnh báo trong XR Project Validation trước khi build. Target API/SDK cài thực tế cần kiểm tra trong Unity Hub khi build.
4. Nhặt đĩa đá trong game → Túi đồ → Quan sát → Quét thẻ AR. Cấp quyền camera, đưa toàn bộ thẻ vào hình, xoay hiện vật 180°, nhấn Ghi manh mối.
5. Nếu thiết bị không hỗ trợ hoặc từ chối camera, quay lại xem hiện vật 3D. Không có nhận diện vật thể, plane placement hay lắp ráp AR ngoài phạm vi này.

## Chỉnh sửa tiếp trên project này

- **Bố cục:** mở Main.unity; di chuyển/đổi vật liệu đồ vật dưới World. Không đổi tên các gốc `I_`, `A_`, `P_` vì gameplay tìm chúng bằng tên. Menu Aster / Validate scene anchors rà các tên cần thiết.
- **Model:** `SourceArt/World.blend` có hình học gốc; FBX đã đóng gói nên không cần cài Blender để chạy project. Các cạnh đá và phụ kiện được bevel; lá, bình, gương, bánh răng, cửa và đĩa có mesh riêng.
- **Code:** `Assets/Scripts/Expedition.cs` điều phối; `ExpeditionState.cs` luật; `Explorer.cs` điều khiển; `ExpeditionUI.cs` giao diện; `ArtifactViewer.cs` AR/3D; `ExpeditionAudio.cs` âm thanh.
- **Âm thanh:** thay WAV cùng tên trong `Assets/Resources/Audio`. Chỉnh âm lượng trong menu.
- **Art generation:** `SourceArt/upgrade_art.py` dựng art mới bằng Blender 4.0; `build_world.py` là nền gốc; `build_audio.py` dùng Python/numpy/scipy; `build_marker.py` dùng Python/Pillow. Chỉ chạy khi muốn tái tạo asset gốc: chúng ghi đè asset tương ứng. Không chạy lại để sửa lặt vặt trong scene.
- Menu Prepare **không ghi đè Main.unity đã tồn tại**. Giữ cả `.meta` Unity tạo khi commit Git. Dùng **Aster / Update artwork in current scene** để nạp FBX mới và giữ World cũ tắt làm backup; xem ART_UPDATE.md.
- Tất cả code/art/audio nguồn được kèm theo. Xem `Documentation/ASSET_LICENSES.md` cho font và gói Unity.

## Tài liệu đi kèm

- `Documentation/WALKTHROUGH.md`: lời giải và các chặng chơi.
- `Documentation/VALIDATION.md`: bằng chứng kiểm tra và các việc phải test trong Unity.
- `Documentation/Previews/`: 5 ảnh render trực tiếp model Blender (không phải screenshot Unity). Hình có thể hiển thị nước, bánh răng hoặc các ký hiệu cùng lúc; khi chơi mã sẽ ẩn/hiện theo tiến trình.
- `Tests/RulesTest.cs`: kiểm tra thuần C# có thể chạy bằng Mono ngoài Unity.

Hướng hình ảnh hiện tại là di tích low-poly màu cát, xanh lá và đồng, các cạnh mềm. Đây là bản dựng 3D từ mô tả để chỉnh tiếp, không cam kết khớp 100% ảnh concept. Ánh sáng Unity và Blender sẽ khác nhau; cần đối chiếu trên máy chạy thực tế.

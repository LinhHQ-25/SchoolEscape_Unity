# Kiểm tra bản nâng cấp 23/10/2026

## Đã thực hiện

| Kiểm tra | Kết quả và giới hạn |
|---|---|
| Luật chơi độc lập | Kết quả bản gốc: Mono mcs + mono chạy Tests/RulesTest.cs PASS 17 điều kiện; mã luật và bài test giữ nguyên; duyệt đủ 64 tổ hợp cổng, chỉ một lời giải khi đã ghi manh mối |
| Cú pháp C# | Tree-sitter kiểm tra 12 tệp C# mới: không có syntax error |
| Compile Unity | Bản art/UI mới chưa compile trong Unity. Kết quả reference/stub của bản trước không chứng minh bản này compile được |
| API gói XR | Đối chiếu source chính thức AR Foundation 6.3.1 và XR Management 4.5.3 cho Image Library editor extensions, XRGeneralSettings.Manager/InitManagerOnStart và cấu hình per build target |
| Hình học | Blender 4.0.2 tạo và export World.fbx thành công; giữ nguồn World.blend; 5 ảnh render model được kiểm tra bố cục |
| FBX sau nâng cấp | Import lại bằng Blender thành công: đủ 53 anchor (44 gốc + 9 menu/đèn), 3021 mesh |
| Nguồn art | 24 mẫu CC0, 514 instance; có nguồn/license cho mọi mẫu đã đặt; 8 ảnh vật phẩm 256×256 và atlas đọc hợp lệ |
| Đối chiếu bản trước | 44 vị trí anchor, mã luật ExpeditionState.cs và RulesTest.cs giữ nguyên so với ZIP trước |
| Điểm gameplay | Tests/audit_assets.py xác nhận đủ 44 tên gốc tương tác/động/vị trí cần thiết |
| Tệp âm thanh | 15 WAV stereo 22.050 Hz đọc hợp lệ; có nhạc nền, ambient, kết thúc, SFX, loop nước/máy |
| Tệp cần thiết | FBX, Blend, font, marker, shader có dữ liệu; ZIP kiểm CRC không có lỗi |

## Chưa xác minh — cần chạy trong Unity

- **Chưa chạy Unity Editor/Play Mode**, chưa xác nhận compile/import toàn bộ package Editor. Môi trường tạo file không cài Unity Editor.
- **Chưa build APK hoặc bản PC**, chưa đo FPS, bộ nhớ, nhiệt độ máy, chất lượng bóng hoặc khả năng tracking AR trên thiết bị thật.
- **Chưa xác nhận marker score bằng ARCore**. Card đã thiết kế có chi tiết bất đối xứng, nhưng phải test ánh sáng, khoảng cách, bề mặt phản chiếu và kích thước in.
- Chưa chơi xuyên suốt bằng input thực tế. Luật đã test độc lập; collider, tầm tương tác, tỷ lệ FBX, góc camera, chuyển scene AR và UI vẫn cần kiểm trên Unity.
- Chưa kiểm layout màn hình tai thỏ / tỷ lệ dài. Đã thêm co theo safe area nhưng chưa xác nhận trên thiết bị thật.
- Ảnh Blender không phải screenshot Unity và không xác nhận giống 100% concept. Mesh có bevel nhưng vẫn là phong cách low-poly; ánh sáng hai engine khác nhau.
- Chưa có bake occlusion/lightmap hoặc benchmark mobile. Các mesh tĩnh được gắn BatchingStatic trong scene tạo lần đầu; cần kiểm Batches/SetPass trên máy thật trước khi tối ưu tiếp.
- Nước/ánh sáng câu đố là mô phỏng trực quan theo trạng thái rời rạc; không phải fluid simulation/ray-traced optics.

## Checklist test bắt buộc sau khi mở

- [ ] Package Manager tải xong, Console không có lỗi đỏ; Main.unity đã tạo.
- [ ] Aster / Validate scene anchors báo 0 missing.
- [ ] Play: player đứng trên nền đúng tỷ lệ; WASD, chuột, E, pause hoạt động.
- [ ] Mở/đóng túi đồ và quay lại không mất điều khiển; chạm giữ joystick rồi mở menu không gây trôi.
- [ ] Cổng không mở khi chưa ghi manh mối hoặc sai ký hiệu; đúng lời giải mở và đi qua được.
- [ ] Mái và gương có phản hồi, cửa bên chỉ mở khi đủ điều kiện.
- [ ] Tay van, ba kênh, guồng nước đúng thứ tự; không bị kẹt trên bệ/kênh.
- [ ] Chọn từng bánh răng trong túi, lắp sai bị từ chối, lắp đúng đủ 3 mới chạy máy.
- [ ] Cửa cuối mở, van cuối có kết thúc và thống kê đúng.
- [ ] Thoát Play rồi vào lại → Tiếp tục khôi phục tiến trình và đồ vật; test từng checkpoint.
- [ ] Âm lượng nhạc/SFX có hiệu lực; loop nước/máy dừng khi bắt đầu mới.
- [ ] Android build: XR Project Validation, permission camera, Landscape, ARM64, IL2CPP, OpenGLES3.
- [ ] AR thẻ in 15 cm: nhận diện, mất thẻ, nhận lại, xoay mặt sau, lưu arUsed=true; viewer PC vẫn arUsed=false.
- [ ] Từ chối camera/thiết bị không hỗ trợ: quay lại chơi 3D được.
- [ ] Android: UI không bị cắt, cảm ứng hai tay hoạt động; đo FPS ở cả 4 khu trước khi chốt chất lượng.

## Cách báo lỗi để sửa tiếp đúng project
Gửi ảnh Console đầy đủ thông báo đầu tiên, phiên bản Unity, hệ điều hành/thiết bị, thao tác tái hiện và nếu có scene đã sửa thì gửi nguyên thư mục Assets liên quan cùng .meta. Không cần dựng lại project từ đầu.

# Nâng cấp mỹ thuật trên project đang giữ

## Đã thay đổi
- Giữ chuỗi trại → cổng → gương → nước → bánh răng → thánh đường/kết thúc, cùng các gốc tương tác.
- Tích hợp model CC0 KayKit/Kenney: tường và nền có chi tiết, đồ gỗ, cây và bụi cỏ; thêm cổng vòm, dây leo, mái bạt cong, màu đá ấm, xanh olive, đồng và xanh ngọc. Tán cây được làm cong; mặt trang trí bánh răng hướng lên, đĩa đá có họa tiết mặt trời.
- UI xanh ngọc/vàng: menu gọn trên nền cảnh thật; HUD nhỏ hơn và tiến độ 4 chặng; icon vector; túi đồ có ảnh vật phẩm; nhật ký dạng thẻ; cài đặt có trị số âm lượng/thẻ AR; thống kê kết thúc.
- Cần cảm ứng chỉ hiện trên thiết bị di động. Giao diện co theo safe area; nút có phản hồi hover/nhấn, hộp thoại fade, trạng thái nhận thẻ AR rõ hơn.
- Giữ nhạc/SFX; thêm ánh sáng đèn ấm dao động nhẹ và bụi không khí.

## Mở bản mới lần đầu
Theo README_VI.md. Không cần Blender hay importer GLB, vì FBX/atlas đã xuất sẵn. Dùng Unity 6000.3.26f1, Built-in Render Pipeline như project cũ.

## Cập nhật vào project đang làm trên máy
1. Sao lưu/commit project hiện tại. Dừng Play Mode.
2. Chép Assets, SourceArt, Documentation, Tests từ bản này vào project đang giữ. Giữ .meta và scene hiện có. Nếu nhóm đã sửa cùng file code, so sánh/merge thay vì chép đè mù quáng.
3. Đợi import/compile xong, mở Main.unity hiện tại.
4. Chọn **Aster → Update artwork in current scene**. Game tham chiếu World mới; World cũ được tắt và đổi thành **World_ArtworkBackup**. Lệnh hỗ trợ Undo các liên kết/đối tượng scene.
5. Kiểm tra rồi Save. Chạy **Aster → Validate scene anchors**, phải báo 0 missing.
6. Test theo VALIDATION.md. Từ đây chỉnh trực tiếp trên cùng project. Không chạy lại lệnh cập nhật art sau khi đã sửa World mới, trừ khi muốn nạp lại FBX.

World_ArtworkBackup/Undo không khôi phục file FBX/code đã chép đè ở bước 2. Muốn quay về toàn bộ bản trước phải dùng bản sao lưu/Git ở bước 1. Menu Prepare vẫn không ghi đè Main đã tồn tại.

## Nguồn chỉnh sửa
- `SourceArt/World.blend`: cảnh 3D để chỉnh tiếp.
- `SourceArt/upgrade_art.py`: tái dựng art mới từ các model kèm theo; ghi đè FBX/Blend/preview, không ghi scene Unity. Chạy bằng Blender 4.0.
- `SourceArt/render_items.py`: xuất 8 ảnh vật phẩm từ các mesh thật.
- `SourceArt/build_world.py`: nền ban đầu được upgrade_art dùng; chạy riêng sẽ quay về art cũ.
- `Assets/Scripts/AsterInterfaceArt.cs`: icon vector, safe area và phản hồi nút.
- `Documentation/Previews/`: 5 render Blender của model thật, không phải screenshot Unity. Cơ cấu có thể đang cùng hiện ở trạng thái authoring; khi chơi sẽ ẩn/hiện theo tiến trình.

## Giới hạn thực tế
Đã thay và bổ sung tài sản thật nhưng vẫn là phong cách stylized/low-poly, chưa khớp 100% concept. Một phần cơ cấu, địa hình còn đơn giản hơn ảnh minh họa. Ánh sáng Blender khác Unity. Chưa chạy Unity Editor/Play Mode, chưa build PC/APK, chưa đo hiệu năng điện thoại và chưa test AR thật. Cần hoàn thành các bước đó trước khi coi đây là bản phát hành đã QA.

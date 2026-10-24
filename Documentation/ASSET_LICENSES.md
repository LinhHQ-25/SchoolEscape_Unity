# Nguồn tài sản — bản nâng cấp mỹ thuật 23/10/2026

## Model tải sẵn đã tích hợp

### KayKit — Dungeon Remastered 1.0
- Tác giả: Kay Lousberg / KayKit Game Assets.
- Nguồn chính thức: https://github.com/KayKit-Game-Assets/KayKit-Dungeon-Remastered-1.0
- Giấy phép: CC0 1.0; giữ nguyên `SourceArt/ThirdParty/KayKit/LICENSE.txt`.
- Đã đưa vào World.fbx: tường, tường nứt, cột đá, nền đá thường/nứt/có cây, bàn, ghế, rương, thùng gỗ, giường trại, chai và nến.
- Chỉnh sửa: kích thước, bố trí, kết hợp mesh và tint vật liệu ấm. Atlas gốc xuất thành `Assets/Resources/Art/Textures/KayKit_dungeon_texture.png`, Editor gắn atlas cho vật liệu Unity.
- GLB gốc giữ trong SourceArt; chạy game không cần cài GLTF importer. Danh sách mẫu thực dùng và số instance nằm ở `geometry.json → source_asset_instances`.

### Kenney — Nature Kit
- Tác giả: Kenney; nguồn https://kenney.nl/assets/nature-kit
- Giấy phép: CC0 1.0; giữ `SourceArt/ThirdParty/KenneyNature/License.txt` cùng OBJ/MTL.
- Đã dùng cây, đá, bụi lá và cỏ. Chỉnh bảng màu olive/nâu, tỷ lệ, bố trí; tách tán cây để subdivision thành bề mặt cong, bevel đá.

CC0 cho phép sử dụng và sửa mà không bắt buộc ghi công. Vẫn giữ nguồn/license để nhóm bàn giao rõ ràng. Không dùng model Sketchfab từ đề tài trường học cũ. Không có asset Quaternius trong bản này vì nguồn tải không khả dụng.

## Tài sản riêng và phụ thuộc
- Cổng vòm, cửa chạm ký hiệu, dây leo, mái bạt cong, các cơ cấu câu đố, đĩa đá, máy và kênh nước: source Blender/Python kèm project. Giữ một phần hình học nền gốc để bảo toàn gameplay.
- UI: Unity uGUI, icon vector bằng code và ảnh vật phẩm render trực tiếp từ mesh thật. Không dùng ảnh concept giả làm cảnh chơi.
- 15 tệp nhạc/SFX/ambient WAV tổng hợp riêng bằng `SourceArt/build_audio.py`; không dùng bản ghi thương mại.
- Thẻ AR từ `SourceArt/build_marker.py`, không phải QR.
- Body.ttf = DejaVu Sans, Title.ttf = DejaVu Serif; giữ `FONT_LICENSE.txt` khi phân phối.
- Unity Engine/packages: tải riêng qua Package Manager, theo giấy phép của Unity và từng gói.
- Code gameplay/UI/công cụ kèm theo dành cho nhóm tiếp tục sửa và phát triển đồ án.

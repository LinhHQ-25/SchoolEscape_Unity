# Hành trình và lời giải

## 1. Trại khảo sát → Cổng
Nhặt nhật ký và đèn pin trên bàn. Nhặt đĩa đá trên trụ bên trái cổng. Mở Túi đồ, Quan sát: chọn xem 3D trên PC hoặc AR trên Android. Xoay bốn lần 45° tới mặt sau; nhấn Ghi manh mối.

Ba vòng bên phải cổng lần lượt cần **lá, nước, mặt trời**. Các trạng thái trong mã: lá=0, nước=1, mặt trời=2, núi=3; ban đầu cả ba là núi. Từ trạng thái ban đầu xoay vòng trái 1 lần, giữa 2 lần, phải 3 lần; kéo cần bên phải. Phải ghi nhận manh mối trước khi cần mở cổng. Cổng mở, checkpoint chuyển vào sân.

## 2. Sân → Khu gương
Khu có mái bên trái sân chứa hai gương và bia nhận sáng. Mở cần mái ở góc phía trong bên phải. Từ trạng thái ban đầu, gương thứ nhất xoay 1 lần, gương thứ hai xoay 3 lần. Có tia sáng minh họa thay đổi theo từng nấc. Khi đúng, cổng ở vách phải sân nâng lên. Đây là câu đố căn hướng rời rạc; không phải mô phỏng quang học phản xạ liên tục.

## 3. Kênh nước
Qua cửa bên phải, nhặt tay van trên kệ thấp gần lối vào. Tương tác van ở mặt trước bồn nước để lắp tay và mở nước. Ba đoạn kênh từ trái qua phải cần trạng thái **1,0,1**; từ ban đầu xoay đoạn 1 và 3 một lần, giữ đoạn 2. Khi đúng guồng quay hoạt động. Luồng nước được minh họa bằng mesh/shader; không mô phỏng chất lỏng vật lý.

## 4. Xưởng
Đi tới xưởng có mái phía sau guồng. Nhặt bánh lớn và vừa trên bàn; bánh nhỏ trên kệ bên phải. Trong Túi đồ chọn bánh muốn lắp, đóng túi và tương tác ổ đúng trên bảng máy: trái lớn, giữa vừa, phải nhỏ. Nếu chọn sai, game thông báo loại cần. Sau khi đủ ba bánh, kéo cần bên phải bảng máy. Bộ truyền quay và cửa thánh đường mở.

## 5. Kết thúc
Trở lại sân, đi vào thánh đường đối diện cổng ban đầu. Tương tác van ở bên phải mô hình nhỏ. Hồ trung tâm có nước; camera chuyển cảnh 7 giây và nhạc kết thúc phát. Bảng kết quả thể hiện 4 câu đố, 3 ghi chép tùy chọn, thời gian, số gợi ý, có dùng AR thực hay không. Có thể tiếp tục khám phá hoặc về menu.

## Ghi chép tùy chọn
1. Gần trại khảo sát.
2. Trong khu gương, sát tường phía trong.
3. Kệ bên phải xưởng.

## Lưu và tiếp tục
Mỗi tương tác cập nhật save JSON. Load đưa người chơi tới checkpoint gần nhất (trại / sân / lối kênh / trước thánh đường), không lưu tọa độ chính xác để tránh sinh ra trong cửa đang mở. Nếu file chính lỗi đọc hoặc sai kích thước mảng, thử bản `.bak`. Bắt đầu mới cần xác nhận khi có save.

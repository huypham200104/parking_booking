# Đặt Vấn Đề Dự Án: Hệ Thống Tìm Kiếm Và Đặt Chỗ Đỗ Xe Ô Tô

## 1. Bối cảnh hình thành dự án

Tại TP.HCM, đặc biệt ở các khu vực trung tâm như Quận 1, Quận 3 và Quận 5, số lượng ô tô và nhu cầu di chuyển ngày càng tăng trong khi quỹ đất dành cho đỗ xe có giới hạn. Các bãi đỗ xe tại đây thường xuyên rơi vào tình trạng quá tải, "hết chỗ". Trong khi đó, các bãi xe lại nằm rải rác, thuộc nhiều đơn vị quản lý khác nhau và chưa có một nguồn dữ liệu tập trung để tài xế biết chính xác bãi xe ở đâu, còn chỗ hay không, mức giá bao nhiêu và có phù hợp với phương tiện của mình hay không.

Trong cách vận hành hiện tại, tài xế thường tìm bãi xe bằng kinh nghiệm cá nhân, hỏi người xung quanh hoặc di chuyển lần lượt qua nhiều địa điểm (mang tính cầu may). Thông tin trên bản đồ phổ thông chủ yếu phản ánh vị trí, nhưng không bảo đảm có dữ liệu về số chỗ trống, giờ hoạt động, giới hạn chiều cao, biểu phí hoặc khả năng đặt chỗ. Vì vậy, việc tìm được một bãi xe trên bản đồ không đồng nghĩa với việc tài xế có thể đỗ xe khi đến nơi. 

Ở phía chủ bãi, nhiều hoạt động vẫn được thực hiện thủ công: ghi nhận xe vào/ra bằng thẻ giấy/thẻ từ, kiểm đếm chỗ trống, tính phí, đối soát và quản lý đặt chỗ. Điều này làm giảm khả năng khai thác bãi, dễ gây ùn ứ tại cửa ra vào, khó kiểm soát dữ liệu và không thể cung cấp thông tin tức thời cho khách hàng.

Dự án xuất hiện từ nhu cầu xây dựng một nền tảng công nghệ chung giúp kết nối tài xế, chủ bãi xe và quản trị viên. Hệ thống không chỉ đơn thuần hiển thị vị trí, mà trọng tâm là **"bán sự an tâm và tiện lợi"** thông qua việc tìm kiếm bãi xe, theo dõi chỗ trống thời gian thực, đặt giữ chỗ trước, check-in/check-out siêu tốc và hỗ trợ thanh toán không tiền mặt trên cùng một hệ thống.

## 2. Vấn đề trung tâm cần giải quyết

Vấn đề cốt lõi của dự án là **sự bất cân xứng và thiếu liên thông thông tin giữa nhu cầu đỗ xe của tài xế với khả năng cung cấp chỗ của các bãi xe**.

Vấn đề này thể hiện qua bốn điểm chính:
1. Tài xế không biết bãi xe phù hợp gần mình ở đâu.
2. Tài xế không biết trạng thái còn chỗ có chính xác tại thời điểm đến hay không.
3. Tài xế không thể chắc chắn giữ được chỗ trong thời gian di chuyển tới bãi.
4. Chủ bãi không có công cụ thống nhất để công bố thông tin, quản lý công suất và số hóa quá trình phục vụ.

Nếu chỉ giải quyết việc hiển thị vị trí bãi xe, hệ thống chưa giải quyết được bài toán thực tế. Giá trị của dự án phụ thuộc vào độ tin cậy của dữ liệu, khả năng giữ chỗ đúng, đồng bộ trạng thái kịp thời và xử lý trọn vẹn vòng đời của một lượt đỗ xe.

## 3. Các vấn đề và nhu cầu của từng nhóm người dùng

### 3.1. Đối với tài xế
* **Rủi ro hết chỗ cao:** Mất nhiều thời gian và nhiên liệu để chạy vòng quanh tìm chỗ đỗ. Có thể đến bãi nhưng bãi đã đầy do thông tin cũ, buộc phải quay đầu xe, gây trễ giờ hẹn/họp hoặc dẫn đến việc dừng/đỗ sai quy định.
* **Thiếu cơ chế bảo đảm:** Không có cơ chế đặt trước để chắc chắn nắm giữ một "slot" đỗ xe an toàn ngay khi đang di chuyển trên đường.
* **Thông tin mập mờ, thiếu minh bạch:** Khó so sánh khoảng cách, giá cả (dễ bị "chặt chém" tại các bãi xe truyền thống), giờ mở cửa, mái che và giới hạn chiều cao giữa các bãi. Không biết bãi có nhận xe qua đêm hoặc có phù hợp với các dòng xe kích thước lớn (SUV, bán tải) hay không.
* **Bất tiện khi Check-in/Check-out truyền thống:** Việc sử dụng thẻ giấy hoặc thẻ từ dễ bị rơi rớt, rách nát, mất thẻ dẫn đến bị phạt tiền oan. Khi trời mưa, hạ kính xuống lấy thẻ từ bảo vệ gây bất tiện lớn.
* **Ùn ứ khi thanh toán:** Việc móc ví tìm tiền mặt/tiền lẻ để trả cho bảo vệ khi ra khỏi bãi mất thời gian, dễ gây kẹt xe ùn ứ tại cổng ra.
* **Quản lý phương tiện:** Một người có thể sử dụng nhiều xe nhưng hệ thống phải nhận diện đúng phương tiện cho từng lượt đặt.

### 3.2. Đối với chủ bãi xe
* **Hạn chế tiếp cận khách hàng:** Khó tiếp cận tài xế đang có nhu cầu ở khu vực lân cận; không khai thác hết công suất bãi vì khách hàng không biết bãi còn chỗ trống.
* **Vận hành thủ công, chậm trễ:** Việc cập nhật chỗ trống thủ công dễ chậm hoặc sai lệch. Khó quản lý đồng thời cùng lúc khách đặt trước qua app và khách vãng lai đến trực tiếp.
* **Tính toán biểu phí phức tạp:** Biểu phí thay đổi theo loại xe, khung giờ, ngày/đêm và block thời gian (ví dụ: block 30 phút hoặc 1 giờ), dễ gây nhầm lẫn nếu tính toán bằng tay.
* **Rủi ro thất thoát doanh thu:** Có nguy cơ thất thoát doanh thu nếu dữ liệu check-in, check-out và thanh toán không nhất quán.
* **Sự cố kỹ thuật & nhân sự:** Nhân viên cần xử lý được trường hợp tài xế không có ứng dụng, không quét được QR, điện thoại hết pin hoặc cung cấp sai biển số.
* **Xác thực pháp lý:** Hồ sơ pháp lý và thông tin bãi cần được kiểm duyệt trước khi công khai để tránh bãi xe "lậu" hoặc sai thông tin.

### 3.3. Đối với quản trị viên hệ thống (Admin)
* **Kiểm duyệt nguồn dữ liệu:** Phải xác minh bãi xe và ngăn thông tin không hợp lệ xuất hiện trên hệ thống.
* **Quản lý phân quyền:** Phải quản lý tài khoản, phân quyền và lịch sử thao tác của nhiều nhóm người dùng (tài xế, chủ bãi, nhân viên bãi xe).
* **Chống gian lận & phá hoại:** Phải phát hiện báo cáo giả từ cộng đồng, dữ liệu sai và hành vi lợi dụng cơ chế đặt chỗ (đặt rồi bỏ bom).
* **Xử lý khiếu nại:** Phải giải quyết các tranh chấp liên quan đến đặt chỗ, giá tiền, trạng thái hủy phòng và giao dịch thanh toán.
* **Thống kê & tối ưu:** Cần số liệu đủ tin cậy để thống kê công suất, nhu cầu và tình hình vận hành theo khu vực, thời gian.

## 4. Các bài toán nghiệp vụ phải xử lý

### 4.1. Tìm bãi xe phù hợp
Hệ thống không chỉ tìm bãi gần nhất theo đường thẳng. Kết quả còn phải xét bán kính tìm kiếm, số chỗ trống thực tế, loại phương tiện, giới hạn chiều cao, giờ hoạt động, mức giá và các tiện ích (có mái che, review...). Vị trí GPS của người dùng có thể không chính xác hoặc bị từ chối quyền truy cập, nên hệ thống cần có cách xử lý thay thế.

### 4.2. Xác định số chỗ thực sự có thể cung cấp
Tổng sức chứa, số xe đang đỗ, số chỗ đã được giữ và số chỗ ngừng khai thác là các khái niệm khác nhau. Nếu tính sai một trong các thành phần này, hệ thống có thể báo còn chỗ dù bãi đã đầy hoặc từ chối khách dù bãi còn khả năng phục vụ.

### 4.3. Tranh chấp chỗ đỗ và đặt vượt sức chứa (Overbooking)
Khi chỉ còn một chỗ nhưng nhiều người đặt cùng lúc, chỉ một yêu cầu được phép thành công. Đây là vấn đề đồng thời ở phía máy chủ; chỉ kiểm tra số chỗ ở giao diện hoặc kiểm tra rồi cập nhật bằng hai thao tác tách rời sẽ không đủ an toàn.

### 4.4. Vòng đời của lượt đặt chỗ
Một lượt đặt cần có trạng thái và quy tắc chuyển trạng thái rõ ràng: *chờ xác nhận, đã giữ chỗ, đã check-in, đã hoàn thành, đã hủy hoặc đã hết hạn*. Hệ thống phải ngăn các chuyển đổi không hợp lệ như check-in một booking đã hủy, check-out hai lần hoặc thanh toán cho lượt đỗ chưa kết thúc.

### 4.5. Đặt chỗ nhưng không đến (No-show)
Nếu tài xế đặt chỗ nhưng không đến, chỗ đỗ bị giữ vô ích và bãi mất cơ hội phục vụ khách khác. Hệ thống cần giới hạn thời gian giữ chỗ, tự động hết hạn và có chính sách phù hợp (phạt/khóa tài khoản) đối với hành vi lặp lại, đồng thời tránh phạt nhầm người dùng do tắc đường hoặc lỗi hệ thống.

### 4.6. Check-in và check-out siêu tốc bằng QR
Mã QR phải xác định đúng bãi và không được dùng để giả mạo check-in từ xa. Khi tài xế đến bãi, hệ thống cần hỗ trợ quét mã QR để đối chiếu booking, phương tiện, thời gian và mở barie nhanh chóng trong vài giây. Ngoài luồng tự phục vụ, vẫn phải có luồng xử lý thủ công (nhân viên nhập biển số) cho trường hợp mất mạng, camera/QR lỗi, hoặc điện thoại tài xế hết pin.

### 4.7. Tính phí tự động
Mỗi bãi có thể áp dụng quy tắc giá khác nhau: theo lượt, theo block thời gian, làm tròn thời gian, giá ngày và đêm, phí qua đêm, ngày lễ hoặc loại xe. Công thức phải xác định được, kiểm thử được và lưu lại phiên bản áp dụng tại thời điểm phát sinh giao dịch để tránh việc chủ bãi thay đổi biểu phí làm sai lệch hóa đơn cũ.

### 4.8. Thanh toán không tiền mặt và đối soát
Hệ thống tích hợp thanh toán online trực tiếp (qua Ngân hàng/VietQR/Ví điện tử) giúp tài xế ra cổng nhanh chóng. Hệ thống phải giải quyết bài toán: thanh toán thành công ở ngân hàng nhưng phản hồi về hệ thống bị chậm/thất bại, hoặc người dùng gửi trùng yêu cầu. Phải chống ghi nhận thanh toán trùng, xác minh trạng thái giao dịch thông qua webhook/IPN thực tế thay vì chỉ dựa vào ảnh chụp màn hình của người dùng.

### 4.9. Hủy chỗ, hoàn tiền và tranh chấp
Cần xác định rõ ràng ai được hủy, thời hạn hủy, phí hủy, điều kiện hoàn tiền và cách xử lý khi bãi không thể cung cấp chỗ dù booking còn hiệu lực. Nếu quy tắc không minh bạch, tranh chấp giữa tài xế, chủ bãi và nền tảng sẽ phát sinh.

### 4.10. Báo cáo trạng thái từ cộng đồng
Thông tin cộng đồng (crowdsourcing) có thể giúp bổ sung dữ liệu nhưng cũng dễ bị báo sai hoặc phá hoại. Báo cáo cần gắn với vị trí, thời gian, độ uy tín của người dùng và phải giảm trọng số tin cậy khi dữ liệu đã cũ. Dữ liệu cộng đồng không nên mặc nhiên thay thế dữ liệu vận hành chính thức của bãi.

## 5. Các thách thức về dữ liệu

### 5.1. Thiếu nguồn dữ liệu thống nhất
Dữ liệu bãi xe đến từ nhiều nguồn: tòa nhà, bãi tư nhân, bãi lộ thiên và điểm đỗ dưới lòng đường. Mỗi nguồn có cách mô tả địa chỉ, giá, công suất và thời gian hoạt động khác nhau. Việc chuẩn hóa dữ liệu là điều kiện cần để tìm kiếm và so sánh chính xác.

### 5.2. Thiếu dữ liệu chỗ trống thời gian thực
Nhiều bãi chưa có API, cảm biến hoặc camera kết nối với hệ thống. Trong giai đoạn thử nghiệm, trạng thái chỗ trống có thể phải cập nhật thủ công hoặc giả lập. Vì vậy, hệ thống cần phân biệt rõ dữ liệu thật, dữ liệu do chủ bãi cập nhật, dữ liệu cộng đồng và dữ liệu mô phỏng; không được trình bày dữ liệu mô phỏng như dữ liệu vận hành thực tế.

### 5.3. Dữ liệu nhanh lỗi thời
Số chỗ trống thay đổi liên tục. Một giá trị đúng ở thời điểm truy vấn có thể không còn đúng khi tài xế đến nơi. Mỗi trạng thái cần có thời điểm cập nhật, nguồn dữ liệu và mức độ tin cậy rõ ràng.

### 5.4. Chất lượng tọa độ và dữ liệu không gian
Sai tọa độ có thể dẫn người dùng đến nhầm lối vào hoặc sai phía đường (đường chia dải phân cách). Khoảng cách địa lý cũng không hoàn toàn phản ánh quãng đường lái xe do đường một chiều, cấm rẽ và điều kiện giao thông. Dữ liệu nên xác định được cả vị trí bãi lẫn lối xe vào khi có thể.

### 5.5. Nhất quán dữ liệu
Số chỗ hiển thị, booking, lượt xe đang đỗ và giao dịch thanh toán phải thống nhất giữa cơ sở dữ liệu, bộ nhớ đệm (Cache) và các máy khách (Client app). Nếu cập nhật một phần thất bại, hệ thống cần có khả năng khôi phục (rollback) hoặc đối soát.

## 6. Các thách thức kỹ thuật

### 6.1. Cập nhật thời gian thực (Real-time updates)
Khi có xe vào/ra, booking mới hoặc booking hết hạn, trạng thái trên bản đồ và trang quản trị phải được cập nhật ngay lập tức (sử dụng WebSocket/SSE). Hệ thống cũng phải xử lý mất kết nối và đồng bộ lại dữ liệu sau khi máy khách kết nối trở lại; thông báo thời gian thực không thể là nguồn dữ liệu duy nhất.

### 6.2. Tính đồng thời và toàn vẹn giao dịch (Concurrency control)
Các thao tác giữ chỗ, check-in, check-out và thanh toán có thể xảy ra đồng thời. Cần có ràng buộc cơ sở dữ liệu, transaction và cơ chế khóa (pessimistic/optimistic locking) phù hợp để tránh số chỗ âm, booking trùng hoặc cập nhật mất dữ liệu (lost update).

### 6.3. Tác vụ nền (Background Jobs)
Booking hết hạn, giải phóng chỗ và giảm độ tin cậy của báo cáo cần được xử lý tự động. Tác vụ nền phải chạy lặp an toàn (idempotent), không xử lý hai lần và có khả năng chạy bù (retry) sau khi dịch vụ bị gián đoạn.

### 6.4. Bộ nhớ đệm (Caching)
Cache giúp giảm tải truy vấn cơ sở dữ liệu nhưng có thể làm người dùng thấy dữ liệu cũ. Dữ liệu tĩnh như tên, địa chỉ phù hợp để cache lâu hơn; dữ liệu chỗ trống cần thời gian sống (TTL) ngắn và chiến lược vô hiệu hóa (eviction) rõ ràng.

### 6.5. Bản đồ và định vị
Ứng dụng phụ thuộc vào quyền GPS, chất lượng tín hiệu, dịch vụ bản đồ (Google Maps/Mapbox) và hạn mức API bên thứ ba. Cần xử lý các trường hợp người dùng tắt định vị, GPS sai lệch, mất mạng hoặc dịch vụ bản đồ không phản hồi.

### 6.6. Bảo mật và phân quyền
Hệ thống lưu tài khoản, biển số xe, vị trí, lịch sử đỗ và giao dịch tài chính. Các API phải xác thực (Authentication) và phân quyền (Authorization) chặt chẽ. Đặc biệt phải ngăn chủ bãi truy cập dữ liệu của bãi khác, giả mạo QR, sửa giá sau giao dịch hoặc gọi lén API thanh toán.

### 6.7. Khả năng quan sát và xử lý sự cố (Observability)
Cần có log, chỉ số giám sát (metrics) và dấu vết giao dịch (tracing) đủ để điều tra khi số chỗ bị sai, booking biến mất hoặc thanh toán không khớp. Log phải hỗ trợ truy vết nhanh nhưng không làm lộ thông tin nhạy cảm của người dùng.

### 6.8. Kiểm thử (Testing)
Ngoài kiểm thử chức năng thông thường, dự án cần kiểm thử cạnh tranh (concurrency testing) khi nhiều người cùng đặt chỗ, kiểm thử vòng đời booking, công thức tính giá, tác vụ hết hạn, thanh toán lặp, mất kết nối và phân quyền. Đây là các khu vực có rủi ro cao hơn giao diện hiển thị đơn thuần.

## 7. Các vấn đề vận hành và triển khai thực tế

* Cần thuyết phục các chủ bãi xe truyền thống tham gia hệ thống và duy trì thói quen cập nhật dữ liệu đúng.
* Nhân viên bãi xe (bảo vệ, quản lý) cần được đào tạo hướng dẫn sử dụng quy trình công nghệ mới song song hoặc thay thế quy trình thủ công hiện tại.
* Hạ tầng thiết bị không đồng đều: Không phải bãi nào cũng có mạng ổn định, thiết bị thông minh để quét QR hoặc hệ thống kiểm soát xe tự động (Barie thông minh).
* Dữ liệu ban đầu (vị trí bãi, sức chứa) cần được đội ngũ dự án kiểm tra thực địa (survey) và cập nhật định kỳ.
* Pháp lý và trách nhiệm: Cần quy định rõ ràng trách nhiệm khi thông tin trên ứng dụng khác biệt với tình trạng thực địa (ví dụ: app báo còn chỗ nhưng thực tế bãi bị đóng cửa đột xuất).
* Cần có quy trình chăm sóc khách hàng (CSKH) để hỗ trợ tài xế và chủ bãi xử lý khiếu nại, hoàn tiền kịp thời.
* Chi phí vận hành: Chi phí dịch vụ bản đồ, máy chủ, lưu trữ, SMS OTP và phí cổng thanh toán tăng lũy tiến theo số lượng người dùng.
* Khi mở rộng ra nhiều quận hoặc tỉnh thành khác, biểu phí, quy định pháp lý về đỗ xe lòng đường và đơn vị vận hành có thể khác nhau hoàn toàn.

## 8. Giới hạn và giả định của giai đoạn đầu (MVP)

* **Khu vực thử nghiệm:** Tập trung giới hạn tại khu vực lõi Quận 1, TP.HCM.
* **Nguồn dữ liệu:** Tên, địa chỉ, tọa độ và thông tin cơ bản của bãi xe sử dụng dữ liệu thực tế đã được kiểm tra trong phạm vi có thể.
* **Mô phỏng phần cứng:** Do chưa kết nối trực tiếp với phần cứng/camera của bãi xe, biến động chỗ trống tạm thời được cập nhật thủ công bởi chủ bãi hoặc thông qua hệ thống mô phỏng có kiểm soát dựa trên lượt đặt qua app.
* **Giới hạn trách nhiệm:** Hệ thống đóng vai trò hỗ trợ quyết định tìm và đặt chỗ nhưng không thể bảo đảm tuyệt đối tình trạng thực địa nếu nguồn dữ liệu đầu vào từ chủ bãi không được cập nhật đúng.
* **Cơ chế thanh toán:** Tích hợp VietQR hỗ trợ tạo thông tin thanh toán động; việc xác nhận tiền đã nhận cần tích hợp phản hồi tự động (Webhook/API) từ phía ngân hàng hoặc có quy trình đối soát bán tự động phù hợp.
* **Tính năng cốt lõi:** Phiên bản đầu ưu tiên hoàn thiện luồng đi cốt lõi: *Tìm bãi xe phù hợp -> Đặt giữ chỗ -> Check-in -> Check-out -> Tính phí & Thanh toán online* trước khi mở rộng sang các tính năng nâng cao (như dẫn đường chi tiết, đăng ký vé tháng, social review...).

## 9. Hậu quả nếu không giải quyết tốt

* **Mất niềm tin từ tài xế:** Tài xế sẽ gỡ ứng dụng nếu nhiều lần đặt chỗ thành công nhưng đến nơi bãi đã đầy hoặc bị từ chối phục vụ.
* **Sự từ chối từ chủ bãi:** Chủ bãi sẽ rời bỏ nền tảng nếu hệ thống gây ra lỗi đặt vượt chỗ (overbooking), tính sai giá tiền hoặc làm phức tạp thêm khối lượng vận hành của nhân viên bãi.
* **Sai lệch dữ liệu hệ thống:** Dữ liệu sai lệch làm mất hoàn toàn giá trị của thuật toán tìm kiếm, gợi ý bãi xe và bản đồ thời gian thực.
* **Tranh chấp tài chính:** Lỗi đồng thời (concurrency) hoặc lỗi hệ thống thanh toán có thể gây thiệt hại tài chính cho các bên, dẫn đến khiếu nại pháp lý.
* **Rủi ro bảo mật:** Rò rỉ vị trí, biển số xe hoặc lịch sử di chuyển, giao dịch của tài xế gây rủi ro lớn về quyền riêng tư và uy tín của nền tảng.
* **Thất bại khi triển khai thực tế:** Hệ thống có thể chạy hoàn hảo khi làm demo (luồng lý tưởng với dữ liệu giả lập) nhưng sẽ đổ vỡ hoàn toàn khi chịu tải thật hoặc gặp các kịch bản bất thường ngoài thực tế.

## 10. Mục tiêu và tiêu chí đánh giá thành công

Dự án được xem là giải quyết đúng vấn đề khi đạt được các kết quả sau:
* **Tối ưu tìm kiếm:** Tài xế dễ dàng tìm được bãi đỗ xe phù hợp nhất dựa trên bộ lọc vị trí, giá cả, kích thước xe và nhu cầu thực tế.
* **Minh bạch dữ liệu:** Trạng thái chỗ trống hiển thị trên ứng dụng có nguồn gốc, thời điểm cập nhật và mức độ tin cậy rõ ràng; tài xế biết trước chính xác biểu phí và thông tin bãi đỗ.
* **Toàn vẹn giao dịch:** Tuyệt đối không xảy ra hiện tượng đặt vượt sức chứa (overbooking) khi nhiều yêu cầu giữ chỗ đến đồng thời tại một thời điểm bãi sắp đầy.
* **Tự động hóa tác vụ:** Booking hết hạn (quá giờ hẹn không đến) được giải phóng tự động, chính xác và hoàn trả đúng số chỗ trống thực cho hệ thống.
* **Truy vết vòng đời:** Toàn bộ vòng đời lượt đỗ (từ đặt chỗ, check-in bằng QR, check-out đến thanh toán không tiền mặt) được ghi nhận đầy đủ, mượt mà và có thể kiểm chứng, truy vết (audit trail).
* **Tính phí chính xác:** Chi phí được tính toán tự động và nhất quán theo đúng biểu phí mà bãi xe đã thiết lập, không xảy ra sai lệch hóa đơn.
* **Thanh toán an toàn:** Giao dịch thanh toán online được xử lý bất tuần hoàn (idempotent), không bị ghi nhận trùng và hỗ trợ đối soát chính
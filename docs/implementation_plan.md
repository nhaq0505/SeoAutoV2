# Kế hoạch Triển khai SEO-Auto-V2 (Lộ trình 8 tháng)

Với quỹ thời gian **8 tháng**, bạn hoàn toàn có thể xây dựng toàn bộ **12 Microservices** và biến đây thành một trong những Đồ án tốt nghiệp xuất sắc và hoàn thiện nhất, tiệm cận với một sản phẩm SaaS (Software-as-a-Service) thực tế mang ra thị trường kinh doanh.

Chúng ta vẫn sẽ áp dụng chiến lược **Chia để trị (Phased Approach)**, nhưng mục tiêu bây giờ không chỉ là làm cho xong, mà là làm chuẩn chỉnh kiến trúc, có Unit Test đầy đủ, áp dụng CI/CD, và thiết kế UI/UX thật sự "WOW" đúng chất Enterprise.

---

## Open Questions

> [!IMPORTANT]
> Để chuẩn bị scaffold mã nguồn ngay bây giờ, bạn vui lòng xác nhận:
> 1. **Frontend**: Bạn đồng ý sử dụng **Next.js (React) + TailwindCSS** để làm Web Dashboard chứ?
> 2. **Kiến trúc mã nguồn Backend**: Bạn muốn cấu trúc thư mục dạng **Clean Architecture** (phân chia theo Domain, Application, Infrastructure) hay **Vertical Slice Architecture** (gom theo Features: ví dụ Feature CreateAudit chứa chung API, Handler, DB config)? 
> *(Với Microservices, Vertical Slice Architecture thường được ưu tiên vì giúp code base gọn nhẹ, độc lập và dễ maintain hơn).*
> 3. Chúng ta sẽ bắt đầu khởi tạo Solution từ **YARP API Gateway** hay **Identity Service**?

---

## Lộ trình Triển khai Tổng thể (8 Tháng)

### Tháng 1: Hạ tầng (Infrastructure) & Identity Core (HOÀN THÀNH 100%)
- [x] Thiết lập Docker Compose (PostgreSQL, Redis, RabbitMQ).
- [x] Thiết lập Shared Library (`BuildingBlocks/SeoAuto.BuildingBlocks` với Global Exception Handler).
- [x] Khởi tạo **YARP API Gateway** (Cấu hình định tuyến cơ bản qua Port 8000, bật CORS).
- [x] Xây dựng **Identity Service** (Đăng ký/Đăng nhập với JWT, Refresh Token Flow, Profile & Change Password).
- [x] Khởi tạo bộ khung Frontend Dashboard (**Next.js 15 + TypeScript + TailwindCSS**) tại `Frontend/seo-auto-web`.

### Tháng 2: Core Audit Flow
- [x] Xây dựng **Audit Service** (Tích hợp Google PageSpeed Insights API, HTML & On-page SEO, CQRS/Vertical Slice, Query APIs).
- [x] Thiết lập Message Bus (RabbitMQ) để giao tiếp bất đồng bộ giữa các services (AuditRequestedEvent).
- [x] Xây dựng **Report Service** (Lưu kết quả, quản lý Projects/Websites, lưu trữ JSONB, sinh mã chia sẻ & lịch sử điểm số).
- [x] Ghép nối Frontend: Tạo Dashboard cơ bản, form nhập URL Audit, polling trạng thái và hiển thị Core Web Vitals & On-page SEO checklist.

### Tháng 3: Trí tuệ Nhân tạo & Trải nghiệm Real-time
- [x] Xây dựng **AI Service** (Tích hợp Gemini API để đọc raw data và trả về giải pháp tối ưu mã nguồn, Polly Retry & Smart Fallback).
- [ ] Xây dựng **Notification Service** (Dùng SignalR để push thông báo real-time xuống trình duyệt khi AI phân tích xong).
- [ ] Hoàn thiện luồng hiển thị kết quả chi tiết trên UI.

### Tháng 4: Deep Analysis (Crawler & Keyword)
- [ ] Xây dựng **Site Crawler Service** (Dùng thuật toán BFS/DFS cào toàn site, tìm link hỏng, phân tích sitemap.xml).
- [ ] Xây dựng **Keyword Tracking Service** (Theo dõi thứ hạng từ khóa qua SERP API, biểu đồ xu hướng lịch sử).
- [ ] Trực quan hóa dữ liệu trên Frontend (Sử dụng thư viện biểu đồ như Recharts hoặc Chart.js).

### Tháng 5: Lập lịch, Tự động hóa & Chrome Extension
- [ ] Xây dựng **Scheduler Service** (Tích hợp Hangfire để hẹn giờ chạy audit định kỳ, audit hàng loạt/batch).
- [ ] Chạy ngầm việc so sánh báo cáo cũ/mới để tìm ra điểm cải thiện.
- [ ] Lập trình **Chrome Extension** (Dùng Manifest V3) gọi trực tiếp về API Gateway.

### Tháng 6: Phân hệ Doanh nghiệp (Subscription & Team)
- [ ] Xây dựng **Team Service** (Tạo Organization, Invite members qua Email, phân quyền Editor/Viewer).
- [ ] Xây dựng **Subscription Service** (Tích hợp cổng thanh toán Stripe/VNPay, xử lý webhook thanh toán, hóa đơn).
- [ ] Áp dụng giới hạn Quota trên API Gateway (VD: Free chỉ được 10 lần audit/ngày).

### Tháng 7: Tích hợp bên ngoài (Integrations) & Tối ưu hóa
- [ ] Xây dựng **Integration Service** (Tích hợp Slack/Telegram Bot, Google Search Console, Google Analytics).
- [ ] Viết Unit Tests & Integration Tests (đạt độ phủ > 70%).
- [ ] Tối ưu hóa truy vấn Database (thêm Indexes), Redis Caching.

### Tháng 8: Triển khai & Viết Báo Cáo
- [ ] Thiết lập luồng CI/CD (GitHub Actions).
- [ ] Triển khai lên Cloud (AWS / Azure / DigitalOcean / Vercel).
- [ ] Hoàn thiện tài liệu Báo cáo Đồ án tốt nghiệp (lấy dữ liệu từ SRS này sang).
- [ ] Quay video demo và chuẩn bị slide bảo vệ.

---

## Verification Plan & Tiêu chuẩn chất lượng
Vì đây là dự án có thời gian dài, chúng ta sẽ áp dụng các tiêu chuẩn chất lượng cao:
- **Code Quality:** Sử dụng SonarQube hoặc `.editorconfig` nghiêm ngặt để check code smell.
- **Testing:** Yêu cầu có Unit Test cho tất cả các Core Business Logic.
- **UI/UX:** Giao diện phải mang cảm giác Premium, sử dụng Dark mode/Light mode, hiệu ứng mượt mà, layout Dashboard chuyên nghiệp (như Vercel Dashboard).

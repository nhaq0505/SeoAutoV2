using SeoAuto.AuditService.Domain.Entities;
using SeoAuto.AuditService.Infrastructure.Database;
using System.Text.Json;

namespace SeoAuto.AuditService.Infrastructure.ExternalService
{
    public class PageSpeedService : IPageSpeedService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PageSpeedService> _logger;
        private readonly AuditDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PageSpeedService(HttpClient httpClient, ILogger<PageSpeedService> logger, AuditDbContext dbContext, IConfiguration configuration)
        {   
            _httpClient = httpClient;
            _logger = logger;
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<RawMetrics> GetPageSpeedMetricsAsync(Guid auditRequestId, string strategy, string url, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching API Google");

            var apiKey = _configuration["GoogleApiKey"];
            var requestStrategy = strategy.ToLower() == "mobile" ? "mobile" : "desktop";
            // Bắt buộc yêu cầu Google chấm đủ 4 danh mục: Performance, SEO, Accessibility, Best Practices
            var requestUrl = $"https://www.googleapis.com/pagespeedonline/v5/runPagespeed?url={Uri.EscapeDataString(url)}&strategy={requestStrategy}&category=PERFORMANCE&category=SEO&category=ACCESSIBILITY&category=BEST_PRACTICES";
            // Chỉ thêm param key nếu có cấu hình ApiKey
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                requestUrl += $"&key={apiKey}";
            }

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);

            // 3. Dùng JsonDocument để bóc tách các chỉ số cần thiết
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;
            var lighthouse = root.GetProperty("lighthouseResult");
            var categories = lighthouse.GetProperty("categories");
            var audits = lighthouse.GetProperty("audits");

            // 4. Bóc tách điểm số Categories (Google trả về 0.0 - 1.0 nên ta nhân 100)
            var perfScore = (int)((categories.GetProperty("performance").GetProperty("score").GetDouble()) * 100);
            var seoScore = (int)((categories.GetProperty("seo").GetProperty("score").GetDouble()) * 100);
            var accessScore = (int)((categories.GetProperty("accessibility").GetProperty("score").GetDouble()) * 100);
            var bestPracticesScore = (int)((categories.GetProperty("best-practices").GetProperty("score").GetDouble()) * 100);
            // 5. Bóc tách các chỉ số Core Web Vitals
            var lcp = (int)audits.GetProperty("largest-contentful-paint").GetProperty("numericValue").GetDouble();
            var fcp = (int)audits.GetProperty("first-contentful-paint").GetProperty("numericValue").GetDouble();
            var cls = (float)audits.GetProperty("cumulative-layout-shift").GetProperty("numericValue").GetDouble();
            var speedIndex = (int)audits.GetProperty("speed-index").GetProperty("numericValue").GetDouble();
            var ttfb = (int)audits.GetProperty("server-response-time").GetProperty("numericValue").GetDouble();

            // Google dùng Total Blocking Time (TBT) đại diện cho khả năng phản hồi tương tác (INP) trong lab data
            var inp = (int)audits.GetProperty("total-blocking-time").GetProperty("numericValue").GetDouble();
            _logger.LogInformation("✅ Đã lấy điểm Google thành công: Performance={Perf}, SEO={Seo}, LCP={Lcp}ms", perfScore, seoScore, lcp);
            // 6. Đóng gói vào Entity RawMetrics
            return new RawMetrics
            {
                Id = Guid.NewGuid(),
                AuditRequestId = auditRequestId,
                PerformanceScore = perfScore,
                AccessibilityScore = accessScore,
                BestPracticesScore = bestPracticesScore,
                SeoScore = seoScore,
                LCP_ms = lcp,
                INP_ms = inp,
                CLS = cls,
                TTFB_ms = ttfb,
                FCP_ms = fcp,
                SpeedIndex_ms = speedIndex
            };

            
        }
    }
}

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SeoAuto.AiService.Features.Gemini;

public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiAnalysisResult> AnalyzeAuditAsync(AuditAnalysisInput input, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"] ?? string.Empty;
        var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";

        // FR-405: Kiểm tra API Key, nếu rỗng hoặc placeholder thì tự động kích hoạt Rule-based Smart Fallback
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("YOUR_"))
        {
            _logger.LogWarning("Gemini API Key chưa được cấu hình. Kích hoạt Rule-based Expert Fallback cho AuditId {AuditId}.", input.AuditId);
            return GenerateFallbackAnalysis(input, "Rule-based Engine (Fallback)");
        }

        try
        {
            var prompt = BuildExpertPrompt(input);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.4,
                    maxOutputTokens = 2500
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            
            _logger.LogInformation("Gửi yêu cầu phân tích tới Gemini API ({Model}) cho AuditId {AuditId}...", model, input.AuditId);
            var response = await _httpClient.PostAsync(endpoint, jsonContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Gemini API trả về lỗi HTTP {StatusCode}: {ErrorBody}. Kích hoạt Smart Fallback.", response.StatusCode, errorBody);
                return GenerateFallbackAnalysis(input, $"Fallback (API Error {response.StatusCode})");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var parsedText = ExtractGeneratedText(responseJson);

            if (string.IsNullOrWhiteSpace(parsedText))
            {
                _logger.LogWarning("Không thể bóc tách nội dung phản hồi từ Gemini API. Chuyển sang Fallback.");
                return GenerateFallbackAnalysis(input, "Fallback (Empty AI Response)");
            }

            var summaryAdvice = ExtractSummaryAdvice(parsedText);

            return new AiAnalysisResult(
                MarkdownContent: parsedText,
                SummaryAdvice: summaryAdvice,
                ModelUsed: model,
                IsFallback: false
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi gọi Gemini API cho AuditId {AuditId}. Chuyển sang Smart Fallback.", input.AuditId);
            return GenerateFallbackAnalysis(input, "Fallback (Exception Handled)");
        }
    }

    private static string BuildExpertPrompt(AuditAnalysisInput input)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là một Kỹ sư Cấp cao chuyên về Tối ưu hóa SEO On-page và Google Core Web Vitals (Web Performance Engineer).");
        sb.AppendLine("Hãy phân tích dữ liệu kỹ thuật từ đợt kiểm tra website dưới đây và cung cấp bản đánh giá chuyên sâu, chi tiết, kèm các đoạn mã code khắc phục lỗi cụ thể.");
        sb.AppendLine();
        sb.AppendLine($"### THÔNG TIN KIỂM TRA:");
        sb.AppendLine($"- Website URL: {input.Url}");
        sb.AppendLine($"- Thiết bị đánh giá: {input.Strategy}");
        sb.AppendLine($"- Điểm Tổng Hợp: {input.OverallScore}/100");
        sb.AppendLine($"- Điểm Performance: {input.PerformanceScore}/100");
        sb.AppendLine($"- Điểm SEO On-page: {input.SeoScore}/100");
        sb.AppendLine($"- Điểm Accessibility: {input.AccessibilityScore}/100");
        sb.AppendLine($"- Điểm Best Practices: {input.BestPracticesScore}/100");
        sb.AppendLine();
        sb.AppendLine("### DỮ LIỆU HIỆU NĂNG & CORE WEB VITALS (RAW METRICS):");
        sb.AppendLine(input.PerformanceDataJson);
        sb.AppendLine();
        sb.AppendLine("### DỮ LIỆU CẤU TRÚC HTML & SEO ON-PAGE:");
        sb.AppendLine(input.SeoDataJson);
        sb.AppendLine();
        sb.AppendLine("### YÊU CẦU ĐẦU RA (ĐỊNH DẠNG MARKDOWN CHUẨN):");
        sb.AppendLine("1. **Tóm Tắt Chẩn Đoán (Executive Summary)**: Đánh giá tổng quan sức khỏe website trong 2-3 câu ngắn gọn.");
        sb.AppendLine("2. **Phân Tích Chỉ Số Core Web Vitals & Tốc Độ Tải**: Nêu rõ LCP, CLS, INP, TTFB đạt chuẩn hay chưa đạt và nguyên nhân gốc rễ (Root Cause).");
        sb.AppendLine("3. **Khắc Phục Lỗi SEO On-page**: Đánh giá Title, Meta Description, Thẻ Heading H1, Thuộc tính Alt ảnh, Robots.txt, Sitemap.");
        sb.AppendLine("4. **Đoạn Mã Tối Ưu Cụ Thể (Actionable Code Snippets)**: Cung cấp code mẫu sẵn sàng copy (VD: thẻ HTML meta chuẩn, cấu hình caching Nginx, hoặc cú pháp Next.js Image component tối ưu LCP).");
        sb.AppendLine("5. **Danh Sách Đầu Việc Ưu Tiên (Priority Action Checklist)**: Chia thành 3 cấp: [Ưu tiên Cao - Làm ngay], [Ưu tiên Trung bình], [Cải thiện dài hạn].");
        sb.AppendLine();
        sb.AppendLine("Yêu cầu viết bằng tiếng Việt tự nhiên, chuyên nghiệp và súc tích.");
        return sb.ToString();
    }

    private static string ExtractGeneratedText(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString();
                    return text ?? string.Empty;
                }
            }
        }
        catch
        {
            // fallback
        }
        return string.Empty;
    }

    private static string ExtractSummaryAdvice(string markdown)
    {
        var lines = markdown.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('#') && trimmed.Length > 20)
            {
                return trimmed.Length > 200 ? trimmed[..197] + "..." : trimmed;
            }
        }
        return "Website đã hoàn thành phân tích hiệu năng và SEO on-page với các khuyến nghị tối ưu chi tiết.";
    }

    private static AiAnalysisResult GenerateFallbackAnalysis(AuditAnalysisInput input, string modelName)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Báo Cáo Phân Tích & Khuyến Nghị Tối Ưu Hóa");
        sb.AppendLine();
        sb.AppendLine("> [!NOTE]");
        sb.AppendLine($"> Báo cáo được khởi tạo tự động bởi Hệ thống Phân tích Quy chuẩn SEO-Auto Engine dành cho `{input.Url}` ({input.Strategy}).");
        sb.AppendLine();

        sb.AppendLine("## 1. Tóm Tắt Chẩn Đoán (Executive Summary)");
        sb.AppendLine($"Website đạt **{input.OverallScore}/100** điểm tổng hợp (Performance: **{input.PerformanceScore}**, SEO: **{input.SeoScore}**). ");
        if (input.PerformanceScore < 70)
        {
            sb.AppendLine("Hiệu năng trang hiện tại cần được ưu tiên khắc phục khẩn cấp để đảm bảo trải nghiệm người dùng và thứ hạng trên Google.");
        }
        else
        {
            sb.AppendLine("Website duy trì tốc độ tải trang khá tốt, tuy nhiên vẫn còn một số điểm nghẽn kỹ thuật cần tinh chỉnh.");
        }
        sb.AppendLine();

        sb.AppendLine("## 2. Phân Tích Kỹ Thuật Core Web Vitals");
        sb.AppendLine("- **Largest Contentful Paint (LCP)**: Cần đảm bảo hình ảnh hero hoặc banner lớn nhất được tải bằng định dạng WebP/AVIF và bổ sung thuộc tính `fetchpriority=\"high\"`.");
        sb.AppendLine("- **Cumulative Layout Shift (CLS)**: Cần gán cứng `width` và `height` cho toàn bộ ảnh và khối quảng cáo/iframe để tránh giật trang.");
        sb.AppendLine("- **Time to First Byte (TTFB)**: Cân nhắc kích hoạt bộ đệm CDN (Cloudflare) hoặc bật OPcache / Redis Cache ở backend.");
        sb.AppendLine();

        sb.AppendLine("## 3. Khắc Phục Lỗi SEO On-page");
        sb.AppendLine("- **Thẻ Heading H1**: Đảm bảo trên trang chỉ có duy nhất một thẻ `<h1>` mang từ khóa mục tiêu chính.");
        sb.AppendLine("- **Thẻ Mô tả (Meta Description)**: Giữ độ dài tối ưu từ 140 - 160 ký tự để hiển thị đầy đủ trên kết quả Google SERP.");
        sb.AppendLine("- **Thuộc tính Alt ảnh**: Bổ sung `alt` mô tả ngữ cảnh cho mọi thẻ `<img>` nhằm tối ưu Google Image Search và hỗ trợ Accessibility.");
        sb.AppendLine();

        sb.AppendLine("## 4. Đoạn Mã Tối Ưu Sẵn Sàng Áp Dụng (Code Snippets)");
        sb.AppendLine("```html");
        sb.AppendLine("<!-- 1. Tối ưu nạp trước ảnh quan trọng (LCP Hero Image) -->");
        sb.AppendLine("<link rel=\"preload\" fetchpriority=\"high\" as=\"image\" href=\"/images/hero-banner.webp\" type=\"image/webp\">");
        sb.AppendLine();
        sb.AppendLine("<!-- 2. Thẻ Meta SEO Tiêu chuẩn -->");
        sb.AppendLine("<title>Tiêu đề Trang Web Chuẩn SEO (50-60 Ký tự)</title>");
        sb.AppendLine("<meta name=\"description\" content=\"Mô tả hấp dẫn chứa từ khóa cốt lõi giúp tăng tỷ lệ nhấp chuột CTR (140-160 ký tự).\">");
        sb.AppendLine("<link rel=\"canonical\" href=\"" + input.Url + "\">");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("```nginx");
        sb.AppendLine("# Cấu hình Nginx Caching cho Tài nguyên Tĩnh");
        sb.AppendLine("location ~* \\.(jpg|jpeg|png|gif|ico|webp|css|js)$ {");
        sb.AppendLine("    expires 365d;");
        sb.AppendLine("    add_header Cache-Control \"public, no-transform\";");
        sb.AppendLine("}");
        sb.AppendLine("```");
        sb.AppendLine();

        sb.AppendLine("## 5. Danh Sách Đầu Việc Ưu Tiên (Action Checklist)");
        sb.AppendLine("- [ ] **[Cao]** Chuyển đổi toàn bộ ảnh trên trang sang chuẩn WebP/AVIF và nén dung lượng dưới 150KB.");
        sb.AppendLine("- [ ] **[Cao]** Khai báo đầy đủ thẻ `<link rel=\"canonical\">` và bổ sung thẻ `<h1>` duy nhất.");
        sb.AppendLine("- [ ] **[Trung bình]** Thiết lập bộ nhớ đệm trình duyệt (Browser Cache Headers) cho CSS/JS.");
        sb.AppendLine("- [ ] **[Thấp]** Bổ sung dữ liệu có cấu trúc Schema Markup (JSON-LD) cho tổ chức hoặc bài viết.");

        var summaryAdvice = $"Điểm tổng hợp: {input.OverallScore}/100. Cần ưu tiên tối ưu hóa tài nguyên ảnh và bổ sung cấu trúc heading chuẩn SEO.";

        return new AiAnalysisResult(
            MarkdownContent: sb.ToString(),
            SummaryAdvice: summaryAdvice,
            ModelUsed: modelName,
            IsFallback: true
        );
    }
}

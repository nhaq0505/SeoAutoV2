using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SeoAuto.AuditService.Domain.Entities;

namespace SeoAuto.AuditService.Infrastructure.ExternalService
{
    public class HtmlService : IHtmlService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HtmlService> _logger;

        public HtmlService(HttpClient httpClient, ILogger<HtmlService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SeoAnalysis> SeoAnalysisAsync(Guid auditRequestId, string url, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting HTML & SEO analysis for URL: {Url}", url);

            // 1. Tải nội dung HTML bất đồng bộ thông qua HttpClient
            var html = await _httpClient.GetStringAsync(url, cancellationToken);

            // 2. Nạp nội dung vào HtmlAgilityPack để bóc tách DOM
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Title
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            var title = titleNode != null ? WebUtility.HtmlDecode(titleNode.InnerText.Trim()) : string.Empty;

            // Meta Description
            var metaDescNode = doc.DocumentNode.SelectSingleNode("//meta[translate(@name, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='description']");
            var metaDescription = metaDescNode != null ? WebUtility.HtmlDecode(metaDescNode.GetAttributeValue("content", string.Empty).Trim()) : string.Empty;

            // Canonical URL
            var canonicalNode = doc.DocumentNode.SelectSingleNode("//link[translate(@rel, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='canonical']");
            var canonicalUrl = canonicalNode?.GetAttributeValue("href", string.Empty)?.Trim() ?? string.Empty;

            // H1 Count
            var h1Nodes = doc.DocumentNode.SelectNodes("//h1");
            var h1Count = h1Nodes?.Count ?? 0;

            // Images Without Alt
            var imagesWithoutAltNodes = doc.DocumentNode.SelectNodes("//img[not(@alt) or normalize-space(@alt)='']");
            var imagesWithoutAlt = imagesWithoutAltNodes?.Count ?? 0;

            // OpenGraph (og:*) & Twitter Card (twitter:*) meta tags
            var openGraphData = ExtractSocialMetaTags(doc);

            // Structured Data (JSON-LD)
            var jsonLdNode = doc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
            var structuredData = jsonLdNode != null ? jsonLdNode.InnerText.Trim() : null;

            // Kiểm tra robots.txt & Tìm Sitemap thông minh
            var uri = new Uri(url);
            var baseHost = $"{uri.Scheme}://{uri.Authority}";
            var (hasRobotsTxt, hasSitemap) = await CheckRobotsAndSitemapAsync(baseHost, cancellationToken);

            return new SeoAnalysis
            {
                Id = Guid.NewGuid(),
                AuditRequestId = auditRequestId,
                Title = title,
                MetaDescription = metaDescription,
                CanonicalUrl = canonicalUrl,
                HasRobotsTxt = hasRobotsTxt,
                HasSitemap = hasSitemap,
                H1Count = h1Count,
                ImagesWithoutAlt = imagesWithoutAlt,
                OpenGraphData = openGraphData,
                StructuredData = structuredData
            };
        }

        /// <summary>
        /// Trích xuất tất cả thẻ Social Meta Tags (OpenGraph og:* và Twitter Card twitter:*).
        /// Gộp vào chung 1 Dictionary và serialize thành chuỗi JSON.
        /// </summary>
        private static string? ExtractSocialMetaTags(HtmlDocument doc)
        {
            var socialDict = new Dictionary<string, string>();
            var metaNodes = doc.DocumentNode.SelectNodes("//meta[@property or @name]");

            if (metaNodes != null)
            {
                foreach (var node in metaNodes)
                {
                    var key = (node.GetAttributeValue("property", null) ?? node.GetAttributeValue("name", null))?.Trim().ToLowerInvariant();
                    if (key != null && (key.StartsWith("og:") || key.StartsWith("twitter:")))
                    {
                        var content = node.GetAttributeValue("content", string.Empty);
                        socialDict.TryAdd(key, WebUtility.HtmlDecode(content));
                    }
                }
            }

            return socialDict.Count > 0 ? JsonSerializer.Serialize(socialDict) : null;
        }

        /// <summary>
        /// Kiểm tra sự tồn tại của robots.txt và tìm kiếm Sitemap thông minh:
        /// 1. Tải robots.txt và quét tìm chỉ thị "Sitemap: <url>"
        /// 2. Nếu tìm thấy, kiểm tra URL đó; nếu không, fallback về /sitemap.xml mặc định
        /// </summary>
        private async Task<(bool HasRobotsTxt, bool HasSitemap)> CheckRobotsAndSitemapAsync(string baseHost, CancellationToken cancellationToken)
        {
            var robotsUrl = $"{baseHost}/robots.txt";
            var hasRobotsTxt = false;
            string? sitemapUrl = null;

            try
            {
                var robotsResponse = await _httpClient.GetAsync(robotsUrl, cancellationToken);
                if (robotsResponse.IsSuccessStatusCode)
                {
                    hasRobotsTxt = true;
                    var robotsContent = await robotsResponse.Content.ReadAsStringAsync(cancellationToken);
                    var match = Regex.Match(robotsContent, @"^Sitemap:\s*(https?://\S+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (match.Success)
                    {
                        sitemapUrl = match.Groups[1].Value.Trim();
                        _logger.LogInformation("Found Sitemap directive in robots.txt: {SitemapUrl}", sitemapUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch or parse robots.txt for: {BaseHost}", baseHost);
            }

            // Kiểm tra sitemap tìm thấy trong robots.txt hoặc fallback đường dẫn mặc định
            var targetSitemap = sitemapUrl ?? $"{baseHost}/sitemap.xml";
            var hasSitemap = await CheckUrlExistsAsync(targetSitemap, cancellationToken);

            return (hasRobotsTxt, hasSitemap);
        }

        /// <summary>
        /// Kiểm tra một URL có tồn tại hay không (chỉ đọc Headers để tiết kiệm băng thông).
        /// </summary>
        private async Task<bool> CheckUrlExistsAsync(string checkUrl, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.GetAsync(checkUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check existence for: {CheckUrl}", checkUrl);
                return false;
            }
        }
    }
}

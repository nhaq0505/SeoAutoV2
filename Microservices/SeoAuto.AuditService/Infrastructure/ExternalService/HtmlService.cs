using System.Net;
using System.Text.Json;
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

            // OpenGraph tags (og:*)
            var ogNodes = doc.DocumentNode.SelectNodes("//meta[starts-with(translate(@property, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'og:')]");
            string? openGraphData = null;
            if (ogNodes != null && ogNodes.Count > 0)
            {
                var ogDict = new Dictionary<string, string>();
                foreach (var node in ogNodes)
                {
                    var property = node.GetAttributeValue("property", string.Empty);
                    var content = node.GetAttributeValue("content", string.Empty);
                    if (!string.IsNullOrWhiteSpace(property) && !ogDict.ContainsKey(property))
                    {
                        ogDict[property] = WebUtility.HtmlDecode(content);
                    }
                }
                openGraphData = JsonSerializer.Serialize(ogDict);
            }

            // Structured Data (JSON-LD)
            var jsonLdNode = doc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
            var structuredData = jsonLdNode != null ? jsonLdNode.InnerText.Trim() : null;

            // Check robots.txt & sitemap.xml
            var uri = new Uri(url);
            var baseHost = $"{uri.Scheme}://{uri.Authority}";
            var hasRobotsTxt = await CheckUrlExistsAsync($"{baseHost}/robots.txt", cancellationToken);
            var hasSitemap = await CheckUrlExistsAsync($"{baseHost}/sitemap.xml", cancellationToken);

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


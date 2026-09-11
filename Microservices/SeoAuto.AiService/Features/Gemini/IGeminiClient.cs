using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeoAuto.AiService.Features.Gemini;

public record AuditAnalysisInput(
    Guid AuditId,
    Guid UserId,
    string Url,
    string Strategy,
    int OverallScore,
    int PerformanceScore,
    int SeoScore,
    int AccessibilityScore,
    int BestPracticesScore,
    string PerformanceDataJson,
    string SeoDataJson
);

public record AiAnalysisResult(
    string MarkdownContent,
    string SummaryAdvice,
    string ModelUsed,
    bool IsFallback
);

public interface IGeminiClient
{
    Task<AiAnalysisResult> AnalyzeAuditAsync(AuditAnalysisInput input, CancellationToken cancellationToken = default);
}

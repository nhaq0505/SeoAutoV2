using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SeoAuto.AiService.Features.Gemini;

namespace SeoAuto.AiService.Features;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .WithTags("AI Service");

        group.MapGet("/ping", () => "AiService is running on port 5004!")
            .WithName("AiPing");

        group.MapPost("/analyze-preview", async (AuditAnalysisInput input, IGeminiClient client) =>
        {
            var result = await client.AnalyzeAuditAsync(input);
            return Results.Ok(result);
        })
        .WithName("AiAnalyzePreview");
    }
}

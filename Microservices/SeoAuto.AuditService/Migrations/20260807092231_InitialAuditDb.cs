using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SeoAuto.AuditService.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Strategy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RawMetrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformanceScore = table.Column<int>(type: "integer", nullable: false),
                    AccessibilityScore = table.Column<int>(type: "integer", nullable: false),
                    BestPracticesScore = table.Column<int>(type: "integer", nullable: false),
                    SeoScore = table.Column<int>(type: "integer", nullable: false),
                    LCP_ms = table.Column<int>(type: "integer", nullable: false),
                    INP_ms = table.Column<int>(type: "integer", nullable: false),
                    CLS = table.Column<float>(type: "real", nullable: false),
                    TTFB_ms = table.Column<int>(type: "integer", nullable: false),
                    FCP_ms = table.Column<int>(type: "integer", nullable: false),
                    SpeedIndex_ms = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawMetrics_AuditRequests_AuditRequestId",
                        column: x => x.AuditRequestId,
                        principalTable: "AuditRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeoAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    MetaDescription = table.Column<string>(type: "text", nullable: false),
                    CanonicalUrl = table.Column<string>(type: "text", nullable: false),
                    HasRobotsTxt = table.Column<bool>(type: "boolean", nullable: false),
                    HasSitemap = table.Column<bool>(type: "boolean", nullable: false),
                    H1Count = table.Column<int>(type: "integer", nullable: false),
                    ImagesWithoutAlt = table.Column<int>(type: "integer", nullable: false),
                    OpenGraphData = table.Column<string>(type: "jsonb", nullable: true),
                    StructuredData = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeoAnalyses_AuditRequests_AuditRequestId",
                        column: x => x.AuditRequestId,
                        principalTable: "AuditRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawMetrics_AuditRequestId",
                table: "RawMetrics",
                column: "AuditRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeoAnalyses_AuditRequestId",
                table: "SeoAnalyses",
                column: "AuditRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawMetrics");

            migrationBuilder.DropTable(
                name: "SeoAnalyses");

            migrationBuilder.DropTable(
                name: "AuditRequests");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WikiLive.Api.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnchorJson = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedBy = table.Column<string>(type: "text", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageComments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkText = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    ContentJson = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageRevisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpaceId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    ContentJson = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageComments_PageId",
                table: "PageComments",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_PageComments_ParentCommentId",
                table: "PageComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_PageLinks_FromPageId_ToPageId",
                table: "PageLinks",
                columns: new[] { "FromPageId", "ToPageId" });

            migrationBuilder.CreateIndex(
                name: "IX_PageRevisions_PageId_CreatedAtUtc",
                table: "PageRevisions",
                columns: new[] { "PageId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_SpaceId_Slug",
                table: "Pages",
                columns: new[] { "SpaceId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageComments");

            migrationBuilder.DropTable(
                name: "PageLinks");

            migrationBuilder.DropTable(
                name: "PageRevisions");

            migrationBuilder.DropTable(
                name: "Pages");
        }
    }
}

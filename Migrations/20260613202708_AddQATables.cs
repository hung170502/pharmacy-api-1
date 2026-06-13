using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pharmacy_API.Migrations
{
    /// <inheritdoc />
    public partial class AddQATables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductQuestions",
                columns: table => new
                {
                    QuestionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HelpfulCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductQuestions", x => x.QuestionId);
                });

            migrationBuilder.CreateTable(
                name: "ProductAnswers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionId = table.Column<int>(type: "integer", nullable: false),
                    RespondentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RespondentRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pharmacist"),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAnswers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_ProductAnswers_ProductQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "ProductQuestions",
                        principalColumn: "QuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_CreatedAt",
                table: "ProductAnswers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAnswers_QuestionId",
                table: "ProductAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_CreatedAt",
                table: "ProductQuestions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_IsActive",
                table: "ProductQuestions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProductQuestions_ProductId",
                table: "ProductQuestions",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductAnswers");

            migrationBuilder.DropTable(
                name: "ProductQuestions");
        }
    }
}

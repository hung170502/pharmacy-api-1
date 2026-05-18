using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOtpTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_UnitId1",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitId1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "Products");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActiveFrom",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StockStatus",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserOtps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOtps", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOtps");

            migrationBuilder.DropColumn(
                name: "ActiveFrom",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StockStatus",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "UnitId1",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId1",
                table: "Products",
                column: "UnitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_UnitId1",
                table: "Products",
                column: "UnitId1",
                principalTable: "Units",
                principalColumn: "UnitId");
        }
    }
}

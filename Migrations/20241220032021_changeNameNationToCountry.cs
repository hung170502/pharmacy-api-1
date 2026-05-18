using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_API.Migrations
{
    /// <inheritdoc />
    public partial class changeNameNationToCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Nations_BrandOriginId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Nations");

            migrationBuilder.AddColumn<int>(
                name: "CountryId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId1",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    CountryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Sort = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.CountryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CountryId",
                table: "Products",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId1",
                table: "Products",
                column: "UnitId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Countries_BrandOriginId",
                table: "Products",
                column: "BrandOriginId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Countries_CountryId",
                table: "Products",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Units_UnitId1",
                table: "Products",
                column: "UnitId1",
                principalTable: "Units",
                principalColumn: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Countries_BrandOriginId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Countries_CountryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Units_UnitId1",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropIndex(
                name: "IX_Products_CountryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitId1",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CountryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitId1",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "Nations",
                columns: table => new
                {
                    NationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NationName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Sort = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nations", x => x.NationId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Nations_BrandOriginId",
                table: "Products",
                column: "BrandOriginId",
                principalTable: "Nations",
                principalColumn: "NationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

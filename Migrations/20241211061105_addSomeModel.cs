using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_API.Migrations
{
    /// <inheritdoc />
    public partial class addSomeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    BrandImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Sort = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CategoryName = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    CategoryAlias = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    CategoryImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sort = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Sort = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    NameAlias = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sale = table.Column<double>(type: "float", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    BrandId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    SortDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DosageForm = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    Packaging = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    BrandOriginId = table.Column<int>(type: "int", nullable: false),
                    ManufacturerId = table.Column<int>(type: "int", nullable: false),
                    Ingredients = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    Usage = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    DosageAndAdministration = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    SideEffects = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    Precautions = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    Storage = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: false),
                    Sort = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "BrandId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Nations_BrandOriginId",
                        column: x => x.BrandOriginId,
                        principalTable: "Nations",
                        principalColumn: "NationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandOriginId",
                table: "Products",
                column: "BrandOriginId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitId",
                table: "Products",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Nations");

            migrationBuilder.DropTable(
                name: "Units");
        }
    }
}

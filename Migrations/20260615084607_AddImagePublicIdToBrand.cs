using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pharmacy_API.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePublicIdToBrand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Brands",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Brands");
        }
    }
}

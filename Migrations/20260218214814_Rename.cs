using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CostumeRentalSystem.Migrations
{
    /// <inheritdoc />
    public partial class Rename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Costumes",
                newName: "ImageFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageFile",
                table: "Costumes",
                newName: "ImageUrl");
        }
    }
}

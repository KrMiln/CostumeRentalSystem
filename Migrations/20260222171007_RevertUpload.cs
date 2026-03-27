using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CostumeRentalSystem.Migrations
{
    /// <inheritdoc />
    public partial class RevertUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Costumes",
                newName: "ImageFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageFile",
                table: "Costumes",
                newName: "ImagePath");
        }
    }
}

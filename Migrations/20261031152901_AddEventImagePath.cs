using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLSK.Migrations
{
    /// <inheritdoc />
    public partial class AddEventImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder boDungMigration)
        {
            boDungMigration.AddColumn<string>(
                name: "HinhAnh",
                table: "SuKien",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder boDungMigration)
        {
            boDungMigration.DropColumn(
                name: "HinhAnh",
                table: "SuKien");
        }
    }
}

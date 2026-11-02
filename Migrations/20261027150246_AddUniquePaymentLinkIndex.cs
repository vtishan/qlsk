using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLSK.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePaymentLinkIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder boDungMigration)
        {
            boDungMigration.CreateIndex(
                name: "IX_ThanhToan_MaLienKet",
                table: "ThanhToan",
                column: "MaLienKet",
                unique: true,
                filter: "[MaLienKet] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder boDungMigration)
        {
            boDungMigration.DropIndex(
                name: "IX_ThanhToan_MaLienKet",
                table: "ThanhToan");
        }
    }
}

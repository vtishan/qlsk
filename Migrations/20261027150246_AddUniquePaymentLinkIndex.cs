using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLSK.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePaymentLinkIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_MaLienKet",
                table: "ThanhToan",
                column: "MaLienKet",
                unique: true,
                filter: "[MaLienKet] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThanhToan_MaLienKet",
                table: "ThanhToan");
        }
    }
}

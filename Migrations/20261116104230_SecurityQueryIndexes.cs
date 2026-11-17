using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLSK.Migrations
{
    /// <inheritdoc />
    public partial class SecurityQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "DonVe",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                collation: "Latin1_General_100_BIN2",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuKien_TrangThai_KetThuc",
                table: "SuKien",
                columns: new[] { "TrangThai", "KetThuc" });

            migrationBuilder.CreateIndex(
                name: "IX_DonVe_TaiKhoanId_Id",
                table: "DonVe",
                columns: new[] { "TaiKhoanId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SuKien_TrangThai_KetThuc",
                table: "SuKien");

            migrationBuilder.DropIndex(
                name: "IX_DonVe_TaiKhoanId_Id",
                table: "DonVe");

            migrationBuilder.AlterColumn<string>(
                name: "IdempotencyKey",
                table: "DonVe",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldCollation: "Latin1_General_100_BIN2");
        }
    }
}

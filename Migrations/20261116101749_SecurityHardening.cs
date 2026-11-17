using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLSK.Migrations
{
    /// <inheritdoc />
    public partial class SecurityHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "ThanhToan",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LinkCreationStartedAt",
                table: "ThanhToan",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TokenVersion",
                table: "TaiKhoan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "DonVe",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HoanTien",
                columns: table => new
                {
                    HoanTienId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThanhToanId = table.Column<int>(type: "int", nullable: false),
                    MaHoanTien = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoanTien", x => x.HoanTienId);
                    table.CheckConstraint("CK_HoanTien_SoTien", "[SoTien] > 0");
                    table.ForeignKey(
                        name: "FK_HoanTien_ThanhToan_ThanhToanId",
                        column: x => x.ThanhToanId,
                        principalTable: "ThanhToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonVe_TaiKhoanId_IdempotencyKey",
                table: "DonVe",
                columns: new[] { "TaiKhoanId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DonVe_TaiKhoanId_TrangThai_HetHan",
                table: "DonVe",
                columns: new[] { "TaiKhoanId", "TrangThai", "HetHan" });

            migrationBuilder.CreateIndex(
                name: "IX_HoanTien_MaHoanTien",
                table: "HoanTien",
                column: "MaHoanTien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoanTien_ThanhToanId",
                table: "HoanTien",
                column: "ThanhToanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoanTien");

            migrationBuilder.DropIndex(
                name: "IX_DonVe_TaiKhoanId_IdempotencyKey",
                table: "DonVe");

            migrationBuilder.DropIndex(
                name: "IX_DonVe_TaiKhoanId_TrangThai_HetHan",
                table: "DonVe");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "LinkCreationStartedAt",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "TokenVersion",
                table: "TaiKhoan");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "DonVe");
        }
    }
}

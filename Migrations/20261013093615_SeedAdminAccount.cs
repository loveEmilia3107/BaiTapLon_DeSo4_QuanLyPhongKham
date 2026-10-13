using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyPhongKham.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TaiKhoan",
                columns: new[] { "maTK", "TrangThaiTK", "email", "password", "role", "sdt", "username" },
                values: new object[] { 99, 0, "admin@clinic.cln", "from1to8", 0, "0933457162", "headAdmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaiKhoan",
                keyColumn: "maTK",
                keyValue: 99);
        }
    }
}

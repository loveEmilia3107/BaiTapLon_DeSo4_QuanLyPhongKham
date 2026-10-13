using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyPhongKham.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChuyenKhoa",
                columns: table => new
                {
                    maCK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenCK = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    motaCK = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThaiCK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenKhoa", x => x.maCK);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    maTK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    password = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    role = table.Column<int>(type: "int", nullable: false),
                    TrangThaiTK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.maTK);
                });

            migrationBuilder.CreateTable(
                name: "BacSi",
                columns: table => new
                {
                    maBS = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tenBS = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    mck = table.Column<int>(type: "int", nullable: false),
                    nsBS = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gt = table.Column<bool>(type: "bit", nullable: false),
                    sdtBS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    emailBS = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TrD = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    namKN = table.Column<int>(type: "int", nullable: false),
                    giaKham = table.Column<double>(type: "float", nullable: false),
                    TrangThaiBS = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BacSi", x => x.maBS);
                    table.ForeignKey(
                        name: "FK_BacSi_ChuyenKhoa_mck",
                        column: x => x.mck,
                        principalTable: "ChuyenKhoa",
                        principalColumn: "maCK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BenhNhan",
                columns: table => new
                {
                    maBN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    mtk = table.Column<int>(type: "int", nullable: false),
                    tenBN = table.Column<string>(type: "nvarchar(75)", maxLength: 75, nullable: false),
                    nsBN = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sdtBN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    diachi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenhNhan", x => x.maBN);
                    table.ForeignKey(
                        name: "FK_BenhNhan_TaiKhoan_mtk",
                        column: x => x.mtk,
                        principalTable: "TaiKhoan",
                        principalColumn: "maTK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichKham",
                columns: table => new
                {
                    maLK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    mmbs = table.Column<int>(type: "int", nullable: false),
                    ngayKham = table.Column<DateTime>(type: "datetime2", nullable: false),
                    gioBatdau = table.Column<TimeOnly>(type: "time", nullable: false),
                    gioKetthuc = table.Column<TimeOnly>(type: "time", nullable: false),
                    maxSL = table.Column<int>(type: "int", nullable: false),
                    currentSL = table.Column<int>(type: "int", nullable: false),
                    TrangThaiLK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichKham", x => x.maLK);
                    table.ForeignKey(
                        name: "FK_LichKham_BacSi_mmbs",
                        column: x => x.mmbs,
                        principalTable: "BacSi",
                        principalColumn: "maBS",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DangKyKham",
                columns: table => new
                {
                    maPhieu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ooBN = table.Column<int>(type: "int", nullable: false),
                    mlk = table.Column<int>(type: "int", nullable: false),
                    ngayDK = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LiDo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TrangThaiDK = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyKham", x => x.maPhieu);
                    table.ForeignKey(
                        name: "FK_DangKyKham_BenhNhan_ooBN",
                        column: x => x.ooBN,
                        principalTable: "BenhNhan",
                        principalColumn: "maBN",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyKham_LichKham_mlk",
                        column: x => x.mlk,
                        principalTable: "LichKham",
                        principalColumn: "maLK",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BacSi_mck",
                table: "BacSi",
                column: "mck");

            migrationBuilder.CreateIndex(
                name: "IX_BenhNhan_mtk",
                table: "BenhNhan",
                column: "mtk");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKham_mlk",
                table: "DangKyKham",
                column: "mlk");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKham_ooBN",
                table: "DangKyKham",
                column: "ooBN");

            migrationBuilder.CreateIndex(
                name: "IX_LichKham_mmbs",
                table: "LichKham",
                column: "mmbs");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_username",
                table: "TaiKhoan",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangKyKham");

            migrationBuilder.DropTable(
                name: "BenhNhan");

            migrationBuilder.DropTable(
                name: "LichKham");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "BacSi");

            migrationBuilder.DropTable(
                name: "ChuyenKhoa");
        }
    }
}

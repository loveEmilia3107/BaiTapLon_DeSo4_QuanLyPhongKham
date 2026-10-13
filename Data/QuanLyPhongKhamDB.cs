using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham.Models;

public class QuanLyPhongKhamDB : DbContext
{
    public QuanLyPhongKhamDB(DbContextOptions<QuanLyPhongKhamDB> options)
        : base(options)
    {
    }

    public DbSet<QuanLyPhongKham.Models.BenhNhan> BenhNhan { get; set; } = default!;

    public DbSet<QuanLyPhongKham.Models.BacSi> BacSi { get; set; } = default!;

    public DbSet<QuanLyPhongKham.Models.ChuyenKhoa> ChuyenKhoa { get; set; } = default!;

    public DbSet<QuanLyPhongKham.Models.DangKyKham> DangKyKham { get; set; } = default!;

    public DbSet<QuanLyPhongKham.Models.LichKham> LichKham { get; set; } = default!;

    public DbSet<QuanLyPhongKham.Models.TaiKhoan> TaiKhoan { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🛑 PRE-CREATE (SEED DATA): TỰ ĐỘNG TẠO TÀI KHOẢN ADMIN MẶC ĐỊNH
        modelBuilder.Entity<TaiKhoan>().HasData(
            new TaiKhoan
            {
                maTK = 99,
                username = "headAdmin",
                password = "from1to8",
                sdt = "0933457162",
                email = "admin@clinic.cln",
                role = 0,
                TrangThaiTK = QuanLyPhongKham.Models.TaiKhoan.TrangThaiTaiKhoan.DangHoatDong
            }
        );
    }
}

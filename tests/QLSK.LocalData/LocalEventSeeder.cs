using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;
using QLSK.Domain.Enums;

namespace QLSK.LocalData;

public static partial class LocalEventSeeder
{
    public const int EventCount = 100;

    public static async Task<int> TaoDuLieuSuKienMauAsync(ApplicationDbContext nguCanhDuLieu, IPasswordHasher<TaiKhoan> boBamMatKhau,
        string thuMucNoiDungUngDung, string matKhauNhapLieu, DateTime? thoiDiemUtcChiDinh = null)
    {
        if (string.IsNullOrWhiteSpace(matKhauNhapLieu))
            throw new InvalidOperationException("A password is required to create local organizer accounts.");

        var duongDanTep = Path.Combine(thuMucNoiDungUngDung, "tests", "QLSK.LocalData", "catalog", "events.json");
        await using var luongDuLieuTep = File.OpenRead(duongDanTep);
        var danhMucSuKienMau = await JsonSerializer.DeserializeAsync<Catalog>(luongDuLieuTep,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var danhSachSuKienMau = danhMucSuKienMau?.DanhSachSuKienMau.OrderBy(suKienMau => suKienMau.SoThuTuSuKien).ToArray() ?? [];
        if (danhSachSuKienMau.Length != EventCount ||
            !danhSachSuKienMau.Select(suKienMau => suKienMau.SoThuTuSuKien).SequenceEqual(Enumerable.Range(1, EventCount)) ||
            danhSachSuKienMau.Select(suKienMau => suKienMau.TenSuKienMau).Distinct().Count() != EventCount)
            throw new InvalidOperationException("The local catalog must contain 100 uniquely numbered and named events.");

        foreach (var suKienMauTrongDanhMuc in danhSachSuKienMau)
        {
            var duongDanAnhMongDoi = $"/assets/images/events/event-{suKienMauTrongDanhMuc.SoThuTuSuKien:000}.jpg";
            if (string.IsNullOrWhiteSpace(suKienMauTrongDanhMuc.TenSuKienMau) || suKienMauTrongDanhMuc.TenSuKienMau.Length > 200 ||
                string.IsNullOrWhiteSpace(suKienMauTrongDanhMuc.TenLoaiSuKienMau) || suKienMauTrongDanhMuc.TenLoaiSuKienMau.Length > 120 ||
                suKienMauTrongDanhMuc.MoTaSuKienMau.Length > 4000 || suKienMauTrongDanhMuc.DuongDanAnhSuKienMau != duongDanAnhMongDoi)
                throw new InvalidOperationException($"Invalid local event {suKienMauTrongDanhMuc.SoThuTuSuKien}.");
            if (!File.Exists(Path.Combine(thuMucNoiDungUngDung, "wwwroot", duongDanAnhMongDoi.TrimStart('/'))))
                throw new FileNotFoundException($"Missing local image: {duongDanAnhMongDoi}");
        }

        await ChuanHoaDuLieuMauHienCoAsync(nguCanhDuLieu);
        var thoiDiemHienTai = thoiDiemUtcChiDinh ?? DateTime.UtcNow;
        await using var giaoDichCoSoDuLieu = nguCanhDuLieu.Database.IsRelational()
            ? await nguCanhDuLieu.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)
            : null;

        var danhSachBanToChucHienCo = new List<BanToChuc>();
        for (var chiSo = 1; chiSo <= 5; chiSo++)
        {
            var emailTaiKhoan = $"organizer{chiSo:00}@qlsk.local";
            var taiKhoanNguoiDung = await nguCanhDuLieu.DanhSachTaiKhoan.Include(taiKhoanDb => taiKhoanDb.BanToChuc).SingleOrDefaultAsync(taiKhoanDb => taiKhoanDb.Email == emailTaiKhoan);
            if (taiKhoanNguoiDung is null)
            {
                taiKhoanNguoiDung = new TaiKhoan
                {
                    Email = emailTaiKhoan, HoTen = OrganizerNames[chiSo - 1], VaiTroId = 2,
                    HoatDong = true, NgayTao = thoiDiemHienTai
                };
                taiKhoanNguoiDung.MatKhauBam = boBamMatKhau.HashPassword(taiKhoanNguoiDung, matKhauNhapLieu);
                nguCanhDuLieu.DanhSachTaiKhoan.Add(taiKhoanNguoiDung);
            }
            else if (taiKhoanNguoiDung.VaiTroId != 2)
            {
                throw new InvalidOperationException($"Local organizer email is already used by another role: {emailTaiKhoan}");
            }

            var banToChucHienCo = taiKhoanNguoiDung.BanToChuc;
            if (banToChucHienCo is null)
            {
                banToChucHienCo = new BanToChuc
                {
                    TaiKhoan = taiKhoanNguoiDung, TenBanToChuc = OrganizerNames[chiSo - 1],
                    MoTaBanToChuc = OrganizerDescriptions[chiSo - 1], NgayXacThuc = thoiDiemHienTai
                };
                nguCanhDuLieu.DanhSachBanToChuc.Add(banToChucHienCo);
            }
            danhSachBanToChucHienCo.Add(banToChucHienCo);
        }

        var danhSachLoaiSuKien = await nguCanhDuLieu.DanhSachLoaiSuKien.ToDictionaryAsync(loaiSuKienDb => loaiSuKienDb.TenLoaiSuKien, StringComparer.OrdinalIgnoreCase);
        foreach (var tenDoiTuong in danhSachSuKienMau.Select(suKienMau => suKienMau.TenLoaiSuKienMau).Distinct())
        {
            if (danhSachLoaiSuKien.ContainsKey(tenDoiTuong)) continue;
            var loaiSuKienKiemThu = new LoaiSuKien { TenLoaiSuKien = tenDoiTuong, MoTaLoaiSuKien = $"Các sự kiện về {tenDoiTuong.ToLowerInvariant()}.", HoatDong = true };
            nguCanhDuLieu.DanhSachLoaiSuKien.Add(loaiSuKienKiemThu);
            danhSachLoaiSuKien.Add(tenDoiTuong, loaiSuKienKiemThu);
        }

        var danhSachDiaDiemHienCo = new List<DiaDiem>();
        for (var chiSo = 0; chiSo < Cities.Length; chiSo++)
        {
            var tenDoiTuong = VenueNames[chiSo];
            var diaChiEmail = $"Khu trung tâm, {Cities[chiSo]}";
            var diaDiemSuKien = await nguCanhDuLieu.DanhSachDiaDiem.SingleOrDefaultAsync(diaDiemDb => diaDiemDb.TenDiaDiem == tenDoiTuong && diaDiemDb.DiaChi == diaChiEmail);
            if (diaDiemSuKien is null)
            {
                diaDiemSuKien = new DiaDiem
                {
                    TenDiaDiem = tenDoiTuong, DiaChi = diaChiEmail, SucChua = 600 + chiSo * 100,
                    GiaThue = 2_000_000 + chiSo * 500_000, HoatDong = true
                };
                nguCanhDuLieu.DanhSachDiaDiem.Add(diaDiemSuKien);
            }
            danhSachDiaDiemHienCo.Add(diaDiemSuKien);
        }

        var danhSachBanToChucId = danhSachBanToChucHienCo.Where(banToChucDb => banToChucDb.BanToChucId != 0).Select(banToChucDb => banToChucDb.BanToChucId).ToArray();
        var danhSachSuKienDaTonTai = await nguCanhDuLieu.DanhSachSuKien.Where(suKienDb => danhSachBanToChucId.Contains(suKienDb.BanToChucId)).ToListAsync();
        var danhSachDuongDanAnh = danhSachSuKienMau.Select(suKienMau => suKienMau.DuongDanAnhSuKienMau).ToHashSet();
        var tapSuKienDaNhanDien = danhSachSuKienDaTonTai.Where(suKienDb => suKienDb.HinhAnh != null && danhSachDuongDanAnh.Contains(suKienDb.HinhAnh)).ToList();
        var ngayDauLichDat = thoiDiemHienTai.Date.AddDays(7).AddHours(11); // 18:00 in Vietnam; persisted dates are UTC.
        if (tapSuKienDaNhanDien.Count > 0)
        {
            var mocThoiGianSuKien = tapSuKienDaNhanDien.OrderBy(suKienDb => suKienDb.NgayTao).First();
            var suKienMauLamMoc = danhSachSuKienMau.Single(suKienMau => suKienMau.DuongDanAnhSuKienMau == mocThoiGianSuKien.HinhAnh);
            ngayDauLichDat = mocThoiGianSuKien.ThoiGianBatDau.AddDays(1 - suKienMauLamMoc.SoThuTuSuKien);
        }

        var danhSachDiaDiemId = danhSachDiaDiemHienCo.Where(diaDiemDb => diaDiemDb.DiaDiemId != 0).Select(diaDiemDb => diaDiemDb.DiaDiemId).ToArray();
        var danhSachPhieuDatDiaDiem = await nguCanhDuLieu.DanhSachDatDiaDiem.Where(datDiaDiemDb => danhSachDiaDiemId.Contains(datDiaDiemDb.DiaDiemId) &&
            (datDiaDiemDb.TrangThai == TrangThaiDatCho.ChoXacNhan || datDiaDiemDb.TrangThai == TrangThaiDatCho.DaXacNhan)).ToListAsync();
        var soBanGhiDaThem = 0;
        foreach (var suKienMauTrongDanhMuc in danhSachSuKienMau)
        {
            // Keep existing event edits and associations intact on repeated explicit runs.
            if (danhSachSuKienDaTonTai.Any(suKienDb => suKienDb.HinhAnh == suKienMauTrongDanhMuc.DuongDanAnhSuKienMau || suKienDb.TenSuKien == suKienMauTrongDanhMuc.TenSuKienMau)) continue;
            var chiSoBanGhi = suKienMauTrongDanhMuc.SoThuTuSuKien - 1;
            var diaDiemSuKien = danhSachDiaDiemHienCo[chiSoBanGhi % danhSachDiaDiemHienCo.Count];
            var thoiDiemBatDau = ngayDauLichDat.AddDays(chiSoBanGhi);
            if (thoiDiemBatDau <= thoiDiemHienTai.AddDays(1)) thoiDiemBatDau = thoiDiemHienTai.Date.AddDays(7 + chiSoBanGhi).AddHours(11);
            while (danhSachPhieuDatDiaDiem.Any(datDiaDiemDb => (datDiaDiemDb.DiaDiem == diaDiemSuKien || (diaDiemSuKien.DiaDiemId != 0 && datDiaDiemDb.DiaDiemId == diaDiemSuKien.DiaDiemId)) &&
                datDiaDiemDb.ThoiGianBatDau < thoiDiemBatDau.AddHours(3) && datDiaDiemDb.ThoiGianKetThuc > thoiDiemBatDau)) thoiDiemBatDau = thoiDiemBatDau.AddDays(1);
            var sucChuaSuKien = Math.Min(120 + chiSoBanGhi % 8 * 40, diaDiemSuKien.SucChua);
            var soTaiKhoanThuong = Math.Max(1, sucChuaSuKien * 4 / 5);
            var giaVe = suKienMauTrongDanhMuc.TenLoaiSuKienMau == "Cộng đồng" ? 0 : 80_000 + chiSoBanGhi % 6 * 40_000;
            var suKienCanThem = new SuKien
            {
                BanToChuc = danhSachBanToChucHienCo[chiSoBanGhi / 10 % danhSachBanToChucHienCo.Count], LoaiSuKien = danhSachLoaiSuKien[suKienMauTrongDanhMuc.TenLoaiSuKienMau],
                TenSuKien = suKienMauTrongDanhMuc.TenSuKienMau, MoTaSuKien = suKienMauTrongDanhMuc.MoTaSuKienMau, HinhAnh = suKienMauTrongDanhMuc.DuongDanAnhSuKienMau,
                ThoiGianBatDau = thoiDiemBatDau, ThoiGianKetThuc = thoiDiemBatDau.AddHours(3), SoNguoi = sucChuaSuKien,
                TrangThai = TrangThaiSuKien.DaDuyet, NgayTao = thoiDiemHienTai
            };
            suKienCanThem.DanhSachLoaiVe.Add(new LoaiVe
            {
                TenLoaiVe = "Vé tiêu chuẩn", GiaVe = giaVe, SoLuong = soTaiKhoanThuong,
                ThoiGianMoBan = thoiDiemHienTai.AddDays(-1), ThoiGianDongBan = thoiDiemBatDau
            });
            if (sucChuaSuKien > soTaiKhoanThuong)
                suKienCanThem.DanhSachLoaiVe.Add(new LoaiVe
                {
                    TenLoaiVe = "Vé đồng hành", GiaVe = giaVe == 0 ? 0 : giaVe + 100_000,
                    SoLuong = sucChuaSuKien - soTaiKhoanThuong, ThoiGianMoBan = thoiDiemHienTai.AddDays(-1), ThoiGianDongBan = thoiDiemBatDau
                });
            var phieuDatDiaDiem = new DatDiaDiem
            {
                SuKien = suKienCanThem, DiaDiem = diaDiemSuKien, ThoiGianBatDau = thoiDiemBatDau, ThoiGianKetThuc = suKienCanThem.ThoiGianKetThuc,
                TongTien = diaDiemSuKien.GiaThue, TienCoc = diaDiemSuKien.GiaThue * 0.3m,
                TrangThai = TrangThaiDatCho.DaXacNhan
            };
            nguCanhDuLieu.DanhSachSuKien.Add(suKienCanThem);
            nguCanhDuLieu.DanhSachDatDiaDiem.Add(phieuDatDiaDiem);
            danhSachPhieuDatDiaDiem.Add(phieuDatDiaDiem);
            soBanGhiDaThem++;
        }

        await nguCanhDuLieu.SaveChangesAsync();
        if (giaoDichCoSoDuLieu is not null) await giaoDichCoSoDuLieu.CommitAsync();
        return soBanGhiDaThem;
    }

    private sealed class Catalog
    {
        [System.Text.Json.Serialization.JsonPropertyName("events")] public List<SampleEvent> DanhSachSuKienMau { get; set; } = [];
    }

    private sealed class SampleEvent
    {
        [System.Text.Json.Serialization.JsonPropertyName("number")] public int SoThuTuSuKien { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("title")] public string TenSuKienMau { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("category")] public string TenLoaiSuKienMau { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("description")] public string MoTaSuKienMau { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("image")] public string DuongDanAnhSuKienMau { get; set; } = "";
    }
}

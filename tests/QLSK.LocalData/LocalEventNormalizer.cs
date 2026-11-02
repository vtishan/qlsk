using Microsoft.EntityFrameworkCore;
using QLSK.Data;
using QLSK.Domain.Entities;

namespace QLSK.LocalData;

public sealed record NormalizationResult([property: System.Text.Json.Serialization.JsonPropertyName("accounts")] int SoTaiKhoanDaChuanHoa, [property: System.Text.Json.Serialization.JsonPropertyName("organizers")] int SoBanToChucDaChuanHoa, [property: System.Text.Json.Serialization.JsonPropertyName("venues")] int SoDiaDiemDaChuanHoa, [property: System.Text.Json.Serialization.JsonPropertyName("events")] int SoSuKienDaChuanHoa);

public static partial class LocalEventSeeder
{
    private const string LegacyDescriptionMarker = " Đây là sự kiện mẫu phục vụ trải nghiệm hệ thống QLSK.";
    private static readonly string[] Cities = ["TP. Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Huế", "Hải Phòng",
        "Cần Thơ", "Đà Lạt", "Nha Trang", "Quy Nhơn", "Vũng Tàu"];
    private static readonly string[] OrganizerNames = ["Kết Nối Văn Hóa", "Sáng Tạo Số", "Nhịp Sống Năng Động",
        "Hành Trình Tri Thức", "Cộng Đồng Xanh"];
    private static readonly string[] OrganizerDescriptions = [
        "Tổ chức các chương trình âm nhạc, nghệ thuật và giao lưu văn hóa.",
        "Kết nối cộng đồng công nghệ và phát triển các hoạt động sáng tạo.",
        "Tổ chức hoạt động thể thao, ẩm thực và trải nghiệm cuộc sống.",
        "Phát triển hội thảo, chương trình học tập và kết nối doanh nghiệp.",
        "Kết nối các hoạt động cộng đồng, du lịch có trách nhiệm và gia đình."
    ];
    private static readonly string[] VenueNames = ["Trung tâm sự kiện Bến Sông", "Hội trường Thăng Long",
        "Trung tâm hội nghị Sông Hàn", "Sảnh tiệc Hương Giang", "Trung tâm sự kiện Hải Âu",
        "Không gian sự kiện Cửu Long", "Khu sự kiện Đồi Thông", "Sảnh tiệc Biển Xanh",
        "Hội trường An Nhiên", "Khu sự kiện Gió Biển"];

    // This explicit local maintenance command updates recognized records in place.
    // Reading the plan is detached so --dry-run never changes tracked entities or the database.
    public static async Task<NormalizationResult> ChuanHoaDuLieuMauHienCoAsync(ApplicationDbContext nguCanhDuLieu, bool chiXemTruocThayDoi = false)
    {
        var cacThayDoiDuLieu = new List<(object Entity, Dictionary<string, object?> Values)>();
        var danhSachTaiKhoanHienCo = await nguCanhDuLieu.DanhSachTaiKhoan.AsNoTracking().ToListAsync();
        var danhSachBanToChucHienCo = await nguCanhDuLieu.DanhSachBanToChuc.AsNoTracking().ToListAsync();
        var tapBanToChucMauId = new HashSet<int>();
        var soTaiKhoanCanChuanHoa = 0;
        var soBanToChucCanChuanHoa = 0;
        for (var chiSo = 1; chiSo <= OrganizerNames.Length; chiSo++)
        {
            var emailTruocChuanHoa = $"demo.btc{chiSo:00}@qlsk.local";
            var emailSauChuanHoa = $"organizer{chiSo:00}@qlsk.local";
            var taiKhoanNguoiDung = danhSachTaiKhoanHienCo.SingleOrDefault(taiKhoanDb => taiKhoanDb.Email == emailTruocChuanHoa || taiKhoanDb.Email == emailSauChuanHoa);
            if (taiKhoanNguoiDung is null || taiKhoanNguoiDung.VaiTroId != 2) continue;
            var giaTriThuocTinhCanCapNhat = new Dictionary<string, object?>();
            if (taiKhoanNguoiDung.Email == emailTruocChuanHoa)
            {
                if (danhSachTaiKhoanHienCo.Any(taiKhoanDb => taiKhoanDb.TaiKhoanId != taiKhoanNguoiDung.TaiKhoanId && taiKhoanDb.Email.Equals(emailSauChuanHoa, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("The local organizer address is already assigned to another account.");
                giaTriThuocTinhCanCapNhat[nameof(TaiKhoan.Email)] = emailSauChuanHoa;
            }
            if (taiKhoanNguoiDung.HoTen == $"Ban tổ chức mẫu {chiSo:00}") giaTriThuocTinhCanCapNhat[nameof(TaiKhoan.HoTen)] = OrganizerNames[chiSo - 1];
            if (giaTriThuocTinhCanCapNhat.Count > 0) { cacThayDoiDuLieu.Add((taiKhoanNguoiDung, giaTriThuocTinhCanCapNhat)); soTaiKhoanCanChuanHoa++; }

            var banToChucHienCo = danhSachBanToChucHienCo.SingleOrDefault(banToChucDb => banToChucDb.TaiKhoanId == taiKhoanNguoiDung.TaiKhoanId);
            if (banToChucHienCo is null) continue;
            tapBanToChucMauId.Add(banToChucHienCo.BanToChucId);
            giaTriThuocTinhCanCapNhat = new Dictionary<string, object?>();
            if (banToChucHienCo.TenBanToChuc == $"SựKiện Hub Demo {chiSo:00}") giaTriThuocTinhCanCapNhat[nameof(BanToChuc.TenBanToChuc)] = OrganizerNames[chiSo - 1];
            if (banToChucHienCo.MoTaBanToChuc == "Ban tổ chức mẫu phục vụ trải nghiệm hệ thống.")
                giaTriThuocTinhCanCapNhat[nameof(BanToChuc.MoTaBanToChuc)] = OrganizerDescriptions[chiSo - 1];
            if (giaTriThuocTinhCanCapNhat.Count > 0) { cacThayDoiDuLieu.Add((banToChucHienCo, giaTriThuocTinhCanCapNhat)); soBanToChucCanChuanHoa++; }
        }

        var danhSachDiaDiemHienCo = await nguCanhDuLieu.DanhSachDiaDiem.AsNoTracking().ToListAsync();
        var soDiaDiemCanChuanHoa = 0;
        for (var chiSo = 0; chiSo < Cities.Length; chiSo++)
        {
            var tenTruocChuanHoa = $"Không gian sự kiện Demo {chiSo + 1:00} - {Cities[chiSo]}";
            var diaChiTruocChuanHoa = $"Khu trải nghiệm mẫu {chiSo + 1:00}, {Cities[chiSo]} (địa điểm giả lập)";
            var diaDiemSuKien = danhSachDiaDiemHienCo.SingleOrDefault(diaDiemDb => diaDiemDb.TenDiaDiem == tenTruocChuanHoa && diaDiemDb.DiaChi == diaChiTruocChuanHoa);
            if (diaDiemSuKien is null) continue;
            var diaChiSauChuanHoa = $"Khu trung tâm, {Cities[chiSo]}";
            if (danhSachDiaDiemHienCo.Any(diaDiemDb => diaDiemDb.DiaDiemId != diaDiemSuKien.DiaDiemId && diaDiemDb.TenDiaDiem == VenueNames[chiSo] && diaDiemDb.DiaChi == diaChiSauChuanHoa))
                throw new InvalidOperationException("The local venue name and address are already assigned to another venue.");
            cacThayDoiDuLieu.Add((diaDiemSuKien, new Dictionary<string, object?>
            {
                [nameof(DiaDiem.TenDiaDiem)] = VenueNames[chiSo], [nameof(DiaDiem.DiaChi)] = diaChiSauChuanHoa
            }));
            soDiaDiemCanChuanHoa++;
        }

        var soSuKienCanChuanHoa = 0;
        var tapDuongDanAnh = Enumerable.Range(1, EventCount).Select(chiSo => $"/assets/images/events/event-{chiSo:000}.jpg").ToHashSet();
        var danhSachSuKienHienCo = await nguCanhDuLieu.DanhSachSuKien.AsNoTracking().Where(suKienDb => tapBanToChucMauId.Contains(suKienDb.BanToChucId)).ToListAsync();
        foreach (var suKienDangXuLy in danhSachSuKienHienCo)
        {
            if (suKienDangXuLy.HinhAnh is null || !tapDuongDanAnh.Contains(suKienDangXuLy.HinhAnh) || !suKienDangXuLy.MoTaSuKien.Contains(LegacyDescriptionMarker)) continue;
            cacThayDoiDuLieu.Add((suKienDangXuLy, new Dictionary<string, object?>
            {
                [nameof(SuKien.MoTaSuKien)] = suKienDangXuLy.MoTaSuKien.Replace(LegacyDescriptionMarker, "", StringComparison.Ordinal)
            }));
            soSuKienCanChuanHoa++;
        }

        if (!chiXemTruocThayDoi)
        {
            foreach (var (entityCanXuLy, giaTriThuocTinhCanCapNhat) in cacThayDoiDuLieu)
            {
                var banGhiId = (int)nguCanhDuLieu.Entry(entityCanXuLy).Property(nguCanhDuLieu.Entry(entityCanXuLy).Metadata.FindPrimaryKey()!.Properties.Single().Name).CurrentValue!;
                var entityDangDuocTheoDoi = nguCanhDuLieu.ChangeTracker.Entries().SingleOrDefault(banGhiTheoDoi =>
                    banGhiTheoDoi.Entity.GetType() == entityCanXuLy.GetType() && (int)banGhiTheoDoi.Property(banGhiTheoDoi.Metadata.FindPrimaryKey()!.Properties.Single().Name).CurrentValue! == banGhiId);
                var banGhiTheoDoiThayDoi = entityDangDuocTheoDoi ?? nguCanhDuLieu.Attach(entityCanXuLy);
                foreach (var (tenThuocTinh, giaTriCanXuLy) in giaTriThuocTinhCanCapNhat) banGhiTheoDoiThayDoi.Property(tenThuocTinh).CurrentValue = giaTriCanXuLy;
            }
            if (cacThayDoiDuLieu.Count > 0) await nguCanhDuLieu.SaveChangesAsync();
        }
        return new NormalizationResult(soTaiKhoanCanChuanHoa, soBanToChucCanChuanHoa, soDiaDiemCanChuanHoa, soSuKienCanChuanHoa);
    }
}

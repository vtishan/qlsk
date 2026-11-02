using QLSK.Domain.Entities;

namespace QLSK.Common.Inventory;
internal static class QuanLyTonVe
{
    internal static void GiuSoLuongVeChoDon(LoaiVe loaiVe, int soLuong)
        => loaiVe.SoLuongDangGiu += soLuong;

    internal static void GiaiPhongVeDangGiu(LoaiVe loaiVe, int soLuong)
        => loaiVe.SoLuongDangGiu = Math.Max(0, loaiVe.SoLuongDangGiu - soLuong);

    internal static void ChuyenVeGiuSangDaBan(LoaiVe loaiVe, int soLuong)
    {
        loaiVe.SoLuongDangGiu -= soLuong;
        loaiVe.SoLuongDaBan += soLuong;
    }

    internal static void TraLaiSoLuongVeDaBan(LoaiVe loaiVe, int soLuong)
        => loaiVe.SoLuongDaBan = Math.Max(0, loaiVe.SoLuongDaBan - soLuong);
}

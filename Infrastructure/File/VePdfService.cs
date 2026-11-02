using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QLSK.Domain.Entities;

namespace QLSK.Infrastructure.File;

public class VePdfService
{
    public byte[] TaoPdfVe(Ve veDaPhatHanh, byte[] anhQrPng)
    {
        return Document.Create(taiLieuPdf =>
        {
            taiLieuPdf.Page(trangPdf =>
            {
                trangPdf.Size(PageSizes.A5);
                trangPdf.Margin(30);

                TaoTieuDePdfVe(trangPdf);
                TaoNoiDungPdfVe(trangPdf, veDaPhatHanh, anhQrPng);
                TaoChanTrangPdfVe(trangPdf);
            });
        }).GeneratePdf();
    }

    private void TaoTieuDePdfVe(PageDescriptor trangPdf)
    {
        trangPdf.Header()
            .Text("VÉ SỰ KIỆN")
            .FontSize(22)
            .Bold();
    }

    private void TaoNoiDungPdfVe(
        PageDescriptor trangPdf,
        Ve veDaPhatHanh,
        byte[] anhQrPng)
    {
        var suKien = veDaPhatHanh.LoaiVe.SuKien;

        trangPdf.Content().Column(cotNoiDung =>
        {
            cotNoiDung.Spacing(8);

            cotNoiDung.Item().Text($"Sự kiện: {suKien.TenSuKien}");
            cotNoiDung.Item().Text($"Loại vé: {veDaPhatHanh.LoaiVe.TenLoaiVe}");
            cotNoiDung.Item().Text($"Mã vé: {veDaPhatHanh.MaVe}");
            cotNoiDung.Item().Text($"Thời gian: {suKien.ThoiGianBatDau:dd/MM/yyyy HH:mm}");

            cotNoiDung.Item()
                .Width(180)
                .Image(anhQrPng);
        });
    }

    private void TaoChanTrangPdfVe(PageDescriptor trangPdf)
    {
        trangPdf.Footer()
            .AlignCenter()
            .Text("Xuất từ hệ thống QLSK");
    }
}
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QLSK.Domain.Entities;

namespace QLSK.Infrastructure.File;

public class VePdfService
{
    public byte[] Tao(Ve ve, byte[] qr)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);

                TaoTieuDe(page);
                TaoNoiDung(page, ve, qr);
                TaoChanTrang(page);
            });
        }).GeneratePdf();
    }

    private void TaoTieuDe(PageDescriptor page)
    {
        page.Header()
            .Text("VÉ SỰ KIỆN")
            .FontSize(22)
            .Bold();
    }

    private void TaoNoiDung(
        PageDescriptor page,
        Ve ve,
        byte[] qr)
    {
        var suKien = ve.LoaiVe.SuKien;

        page.Content().Column(c =>
        {
            c.Spacing(8);

            c.Item().Text($"Sự kiện: {suKien.Ten}");
            c.Item().Text($"Loại vé: {ve.LoaiVe.Ten}");
            c.Item().Text($"Mã vé: {ve.MaVe}");
            c.Item().Text($"Thời gian: {suKien.BatDau:dd/MM/yyyy HH:mm}");

            c.Item()
                .Width(180)
                .Image(qr);
        });
    }

    private void TaoChanTrang(PageDescriptor page)
    {
        page.Footer()
            .AlignCenter()
            .Text("Xuất từ hệ thống QLSK");
    }
}
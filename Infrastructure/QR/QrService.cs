using QRCoder;

namespace QLSK.Infrastructure.QR;

public class QrService
{
    public byte[] TaoAnhQrPng(string noiDungMaQr)
    {
        using var boTaoMaQr = new QRCodeGenerator();

        using var duLieuQr = boTaoMaQr.CreateQrCode(
            noiDungMaQr,
            QRCodeGenerator.ECCLevel.Q
        );

        var anhMaQr = new PngByteQRCode(duLieuQr);

        return anhMaQr.GetGraphic(12);
    }
}
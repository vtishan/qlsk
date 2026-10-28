using QRCoder;

namespace QLSK.Infrastructure.QR;

public class QrService
{
    public byte[] Tao(string text)
    {
        using var boTao = new QRCodeGenerator();

        using var duLieu = boTao.CreateQrCode(
            text,
            QRCodeGenerator.ECCLevel.Q
        );

        var maQr = new PngByteQRCode(duLieu);

        return maQr.GetGraphic(12);
    }
}
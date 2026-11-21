using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace QLSK.Features.Reports;

internal static class ReportDocuments
{
    private const string OpenXml = "application/vnd.openxmlformats-officedocument.";
    private const string RelationType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/";
    private static readonly XNamespace PackageRels = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace DocumentRels = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace Types = "http://schemas.openxmlformats.org/package/2006/content-types";
    private static readonly XNamespace Spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Document = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    public static ReportFile Excel(ReportDto report)
    {
        var data = Package(zip =>
        {
            Write(zip, "[Content_Types].xml", ContentTypes(
                ("/xl/workbook.xml", OpenXml + "spreadsheetml.sheet.main+xml"),
                ("/xl/worksheets/sheet1.xml", OpenXml + "spreadsheetml.worksheet+xml")));
            Write(zip, "_rels/.rels", Relation("xl/workbook.xml", "officeDocument"));
            Write(zip, "xl/_rels/workbook.xml.rels", Relation("worksheets/sheet1.xml", "worksheet"));
            Write(zip, "xl/workbook.xml", new XElement(Spreadsheet + "workbook",
                new XAttribute(XNamespace.Xmlns + "r", DocumentRels),
                new XElement(Spreadsheet + "sheets", new XElement(Spreadsheet + "sheet",
                    new XAttribute("name", "QLSK"), new XAttribute("sheetId", "1"), new XAttribute(DocumentRels + "id", "rId1")))));
            var dataRows = new[] { report.Columns.ToArray() }.Concat(report.Rows).Select((row, index) =>
                new XElement(Spreadsheet + "row", new XAttribute("r", index + 1), row.Select((cell, column) =>
                {
                    var numeric = index > 0 && report.Columns[column].Contains('₫') &&
                        decimal.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
                    return numeric
                        ? new XElement(Spreadsheet + "c", new XElement(Spreadsheet + "v", cell))
                        : new XElement(Spreadsheet + "c", new XAttribute("t", "inlineStr"),
                            new XElement(Spreadsheet + "is", new XElement(Spreadsheet + "t", Clean(cell))));
                })));
            Write(zip, "xl/worksheets/sheet1.xml", new XElement(Spreadsheet + "worksheet",
                new XElement(Spreadsheet + "sheetData", dataRows)));
        });
        return new(data, OpenXml + "spreadsheetml.sheet", "qlsk-bao-cao.xlsx");
    }

    public static ReportFile Word(ReportDto report)
    {
        XElement Paragraph(string value) => new(Document + "p", new XElement(Document + "r",
            new XElement(Document + "t", new XAttribute(XNamespace.Xml + "space", "preserve"), Clean(value))));
        var data = Package(zip =>
        {
            Write(zip, "[Content_Types].xml", ContentTypes(("/word/document.xml", OpenXml + "wordprocessingml.document.main+xml")));
            Write(zip, "_rels/.rels", Relation("word/document.xml", "officeDocument"));
            var dataRows = new[] { report.Columns.ToArray() }.Concat(report.Rows)
                .Select(row => new XElement(Document + "tr", row.Select(value => new XElement(Document + "tc", Paragraph(value)))));
            var table = new XElement(Document + "tbl",
                new XElement(Document + "tblPr", new XElement(Document + "tblW",
                    new XAttribute(Document + "w", "0"), new XAttribute(Document + "type", "auto"))),
                new XElement(Document + "tblGrid", report.Columns.Select(_ => new XElement(Document + "gridCol", new XAttribute(Document + "w", "1600")))),
                dataRows);
            Write(zip, "word/document.xml", new XElement(Document + "document",
                new XElement(Document + "body", Paragraph(report.Title), table)));
        });
        return new(data, OpenXml + "wordprocessingml.document", "qlsk-bao-cao.docx");
    }

    private static byte[] Package(Action<ZipArchive> fill)
    {
        using var bytes = new MemoryStream();
        using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true)) fill(zip);
        return bytes.ToArray();
    }

    private static XElement ContentTypes(params (string Path, string Type)[] entries) => new(Types + "Types",
        new XElement(Types + "Default", new XAttribute("Extension", "rels"),
            new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
        new XElement(Types + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
        entries.Select(entry => new XElement(Types + "Override", new XAttribute("PartName", entry.Path), new XAttribute("ContentType", entry.Type))));

    private static XElement Relation(string target, string type) => new(PackageRels + "Relationships",
        new XElement(PackageRels + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", RelationType + type), new XAttribute("Target", target)));

    private static void Write(ZipArchive zip, string path, XElement xml)
    {
        using var stream = zip.CreateEntry(path).Open();
        new XDocument(xml).Save(stream, SaveOptions.DisableFormatting);
    }

    private static string Clean(string value) => string.Concat(value.EnumerateRunes()
        .Where(rune => rune.Value is 9 or 10 or 13 or >= 32 and <= 0xd7ff or >= 0xe000 and <= 0xfffd or >= 0x10000)
        .Select(rune => rune.ToString()));
}

namespace Iccu.Application.Common.Export;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

public static class ExcelWriter
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public const double MinColumnWidth = 8;
    public const double MaxColumnWidth = 80;

    private const uint FirstSheetId = 1;
    private const uint A4PaperSize = 9;
    private const uint OnePageWide = 1;
    private const uint AsManyPagesTallAsNeeded = 0;
    private const int SheetNameMaxLength = 31;
    private const string FallbackSheetName = "Sheet1";
    private const double ColumnPadding = 4;
    private const double BoldWidthFactor = 1.1;

    private const uint BodyStyleIndex = 1;
    private const uint HeaderStyleIndex = 2;
    private const uint RegularFontId = 0;
    private const uint BoldFontId = 1;

    private static readonly char[] CharactersForbiddenInSheetName = ['[', ']', ':', '*', '?', '/', '\\'];

    public static byte[] ToWorkbook(
        string sheetName,
        IReadOnlyList<string> header,
        IEnumerable<IReadOnlyList<string>> rows)
    {
        var body = rows.ToList();

        using var stream = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(
                new SheetProperties(Children(new PageSetupProperties { FitToPage = true })),
                ToColumns(header, body),
                ToSheetData(header, body),
                LandscapePageSetup());

            workbookPart.Workbook
                .AppendChild(new Sheets())
                .AppendChild(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = FirstSheetId,
                    Name = SanitizeSheetName(sheetName)
                });

            workbookPart.Workbook.Save();
        }

        return stream.ToArray();
    }

    public static double ColumnWidth(string header, IEnumerable<string> values)
    {
        var headerWidth = header.Length * BoldWidthFactor;
        var widest = values.Select(value => (double)value.Length).Append(headerWidth).Max();

        return Math.Clamp(Math.Ceiling(widest + ColumnPadding), MinColumnWidth, MaxColumnWidth);
    }

    private static Columns ToColumns(IReadOnlyList<string> header, IReadOnlyList<IReadOnlyList<string>> body)
    {
        var columns = new Columns();

        for (var index = 0; index < header.Count; index++)
        {
            var position = (uint)(index + 1);
            var values = body.Select(row => index < row.Count ? row[index] : string.Empty);

            columns.AppendChild(new Column
            {
                Min = position,
                Max = position,
                Width = ColumnWidth(header[index], values),
                CustomWidth = true
            });
        }

        return columns;
    }

    private static SheetData ToSheetData(IReadOnlyList<string> header, IReadOnlyList<IReadOnlyList<string>> body)
    {
        var sheetData = new SheetData();

        sheetData.AppendChild(ToRow(header, HeaderStyleIndex));

        foreach (var row in body)
        {
            sheetData.AppendChild(ToRow(row, BodyStyleIndex));
        }

        return sheetData;
    }

    private static Row ToRow(IReadOnlyList<string> values, uint styleIndex)
    {
        var row = new Row();

        foreach (var value in values)
        {
            var cell = row.AppendChild(new Cell { DataType = CellValues.InlineString, StyleIndex = styleIndex });

            cell.AppendChild(new InlineString()).AppendChild(new Text(value));
        }

        return row;
    }

    private static PageSetup LandscapePageSetup()
    {
        return new PageSetup
        {
            PaperSize = A4PaperSize,
            Orientation = OrientationValues.Landscape,
            FitToWidth = OnePageWide,
            FitToHeight = AsManyPagesTallAsNeeded
        };
    }

    private static Stylesheet CreateStylesheet()
    {
        return new Stylesheet(
            new Fonts(new Font(), new Font(Children(new Bold()))) { Count = 2 },
            new Fills(
                new Fill(Children(new PatternFill { PatternType = PatternValues.None })),
                new Fill(Children(new PatternFill { PatternType = PatternValues.Gray125 }))) { Count = 2 },
            new Borders(Children(new Border())) { Count = 1 },
            new CellFormats(
                new CellFormat(),
                CenteredFormat(RegularFontId),
                CenteredFormat(BoldFontId)) { Count = 3 });
    }

    private static CellFormat CenteredFormat(uint fontId)
    {
        return new CellFormat(Children(new Alignment
        {
            Horizontal = HorizontalAlignmentValues.Center,
            Vertical = VerticalAlignmentValues.Center
        }))
        {
            FontId = fontId,
            ApplyFont = true,
            ApplyAlignment = true
        };
    }

    private static OpenXmlElement[] Children(params OpenXmlElement[] children) => children;

    private static string SanitizeSheetName(string sheetName)
    {
        var allowed = sheetName
            .Where(character => !CharactersForbiddenInSheetName.Contains(character))
            .Take(SheetNameMaxLength)
            .ToArray();

        return allowed.Length == 0 ? FallbackSheetName : new string(allowed);
    }
}

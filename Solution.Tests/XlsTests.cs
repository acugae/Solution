using System.Data;
using NPOI.SS.UserModel;
using Solution.IO;

namespace Solution.Tests;

public class XlsTests
{
    [Fact]
    public void XlsxWorkbook_CanBeWrittenToFileAndReadBack()
    {
        using var directory = new TemporaryDirectory();
        var xls = new XLS();
        var workbook = xls.NewFileXLSX();
        var sheet = workbook.GetSheetAt(0);
        var header = sheet.CreateRow(0);
        header.CreateCell(0).SetCellValue("Name");
        header.CreateCell(1).SetCellValue("Amount");
        var data = sheet.CreateRow(1);
        data.CreateCell(0).SetCellValue("Ada");
        data.CreateCell(1).SetCellValue(42);
        var path = directory.GetFilePath("report.xlsx");

        xls.WriteToFile(path, workbook);
        var loaded = xls.ReadFromFileXLSX(path);
        var loadedSheet = loaded.GetSheetAt(0);

        Assert.True(File.Exists(path));
        Assert.Equal(new List<int> { 0, 1 }, xls.GetColumns(loadedSheet, ["Name", "Amount"]));
        Assert.Equal("Ada", xls.GetValue(loadedSheet.GetRow(1).GetCell(0)));
        Assert.Equal("42", xls.GetValue(loadedSheet.GetRow(1).GetCell(1)));
    }

    [Fact]
    public void GetXlsFromDatatable_ExportsHeadersAndValues()
    {
        var table = new DataTable();
        table.Columns.Add("Name");
        table.Columns.Add("Age");
        table.Rows.Add("Ada", "36");
        var xls = new XLS();

        var bytes = xls.GetXLSFromDatatable(table, numCol: 2, showColumnName: true);
        using var stream = new MemoryStream(bytes!);
        var workbook = xls.ReadFromStream(stream);
        var sheet = workbook.GetSheetAt(0);
        var result = xls.GetDataTable(sheet);

        Assert.NotNull(bytes);
        Assert.Equal("Name", result.Columns[0].ColumnName);
        Assert.Equal("Age", result.Columns[1].ColumnName);
        Assert.Equal("Ada", result.Rows[0]["Name"]);
        Assert.Equal("36", result.Rows[0]["Age"]);
    }
}

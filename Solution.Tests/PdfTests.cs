using iTextSharp.text;
using iTextSharp.text.pdf;
using Solution.IO;

namespace Solution.Tests;

public class PdfTests
{
    [Fact]
    public void Pdf_CanOpenAndCopyASpecificPage()
    {
        using var directory = new TemporaryDirectory();
        var sourcePath = directory.GetFilePath("source.pdf");
        var outputPath = directory.GetFilePath("page-two.pdf");
        CreatePdf(sourcePath);
        var pdf = new PDF();

        using var source = pdf.Open(sourcePath);
        pdf.CopyPage(sourcePath, pageNumber: 2, outputPath);
        using var copied = new PdfReader(outputPath);

        Assert.Equal(2, source.NumberOfPages);
        Assert.True(File.Exists(outputPath));
        Assert.Equal(1, copied.NumberOfPages);
        Assert.Empty(pdf.GetFields(File.ReadAllBytes(sourcePath)));
    }

    private static void CreatePdf(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var document = new Document(PageSize.A4);
        PdfWriter.GetInstance(document, stream);
        document.Open();
        document.Add(new Paragraph("Prima pagina"));
        document.NewPage();
        document.Add(new Paragraph("Seconda pagina"));
    }
}

using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Solution.IO;

namespace Solution.Tests;

public class ZipTests
{
    [Fact]
    public void ZipAndUnZip_RoundTripsAllEntries()
    {
        var zip = new ZIP();
        var files = new Dictionary<string, byte[]>
        {
            ["documents/first.txt"] = Encoding.UTF8.GetBytes("contenuto uno"),
            ["second.txt"] = Encoding.UTF8.GetBytes("contenuto due")
        };

        var archive = zip.Zip(files);
        var extracted = zip.UnZip(archive);

        Assert.NotEmpty(archive);
        Assert.Equal(files.Keys.Order(), extracted.Keys.Order());
        Assert.Equal(files["documents/first.txt"], extracted["documents/first.txt"]);
        Assert.Equal(files["second.txt"], extracted["second.txt"]);
    }

    [Fact]
    public void UnZip_RejectsPathTraversalEntries()
    {
        using var archive = new MemoryStream();
        using (var writer = new ZipOutputStream(archive))
        {
            writer.PutNextEntry(new ZipEntry("../unsafe.txt"));
            var content = Encoding.UTF8.GetBytes("unsafe");
            writer.Write(content, 0, content.Length);
            writer.Finish();
        }

        Assert.Throws<InvalidOperationException>(() => new ZIP().UnZip(archive.ToArray()));
    }
}

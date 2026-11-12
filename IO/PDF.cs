namespace Solution.IO;
public class PDF
{
    public PDF() { }

    public PdfReader Open(string sFileInput)
    {
        return new PdfReader(FileManager.ValidatePath(sFileInput));
    }


    //public MemoryStream GetPages(string sourcePdfPath, IList<int> neededPages)
    //{
    //    var sourceDocumentStream = new FileStream(sourcePdfPath, FileMode.Open, FileAccess.Read);
    //    //var sourceDocumentStream = new MemoryStream();//(sourcePdfPath, FileMode.Open);
    //    var destinationDocumentStream = new MemoryStream();//(outputPdfPath, FileMode.Create);
    //    //var destinationDocumentStream = new FileStream(outputPdfPath, FileMode.Create);
    //    var pdfConcat = new PdfConcatenate(destinationDocumentStream);

    //    var pdfReader = new PdfReader(sourceDocumentStream);
    //    pdfReader.SelectPages(neededPages);
    //    pdfConcat.AddPages(pdfReader);

    //    pdfReader.Close();
    //    pdfConcat.Close();
    //    return destinationDocumentStream;
    //}

    //public MemoryStream GetPages(byte[] inputByteArray, IList<int> neededPages)
    //{
    //    MemoryStream sourceDocumentStream = new MemoryStream(inputByteArray);
    //    var destinationDocumentStream = new MemoryStream();//(outputPdfPath, FileMode.Create);
    //    var pdfConcat = new PdfConcatenate(destinationDocumentStream);

    //    var pdfReader = new PdfReader(sourceDocumentStream);
    //    pdfReader.SelectPages(neededPages);
    //    pdfConcat.AddPages(pdfReader);

    //    pdfReader.Close();
    //    pdfConcat.Close();
    //    return destinationDocumentStream;
    //}

    //public string ReadTextFromPage(PdfReader oFile, int idPage)
    //{
    //    string strText = string.Empty;
    //    if (idPage <= oFile.NumberOfPages)
    //    {
    //        ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
    //        String s = PdfTextExtractor.GetTextFromPage(oFile, idPage, its);
    //        s = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
    //        strText = strText + s;
    //    }
    //    return strText;
    //}

    //public string ReadPdfFile(string sFileInput)
    //{
    //    PdfReader reader2 = new PdfReader(sFileInput);
    //    string strText = string.Empty;

    //    for (int page = 1; page <= reader2.NumberOfPages; page++)
    //    {
    //        ITextExtractionStrategy its = new iTextSharp.text.pdf.parser.SimpleTextExtractionStrategy();
    //        PdfReader reader = new PdfReader(sFileInput);
    //        String s = PdfTextExtractor.GetTextFromPage(reader, page, its);

    //        s = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(s)));
    //        strText = strText + s;
    //        reader.Close();
    //    }
    //    return strText;
    //}

    public void CopyPage(string sFileInput, int pageNumber, string sFileOutput)
    {
        string validInput = FileManager.ValidatePath(sFileInput);
        string validOutput = FileManager.ValidatePath(sFileOutput);
        PdfReader reader = new PdfReader(validInput);
        try
        {
            iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(pageNumber);
            Document document = new Document(size);
            using FileStream output = new FileStream(validOutput, FileMode.Create, FileAccess.Write);
            PdfWriter writer = PdfWriter.GetInstance(document, output);
            document.AddTitle("Cedolino");
            document.AddSubject("Generatore Automatico");
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            document.NewPage();
            PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
            cb.AddTemplate(page, 0, 0);
            document.Close();
        }
        finally
        {
            reader.Close();
        }
    }

    /// <summary>
    /// Create a new pdf file getting only a list of pages specified form another pdf file.
    /// </summary>
    /// <param name="sFileInput">Path of the pdf file input.(Where pages are taken).</param>
    /// <param name="sFileOutput">Path of the new pdf file to be saved.</param>
    /// <param name="neededPages">List of the pages to be copied from input pdf file to the output pdf file.</param>
    public void CreatePdfFromDifferentPages(string sFileInput, string sFileOutput, IList<int> neededPages)
    {
        string validInput = FileManager.ValidatePath(sFileInput);
        string validOutput = FileManager.ValidatePath(sFileOutput);
        using FileStream outStream = new FileStream(validOutput, FileMode.Create, FileAccess.Write);
        using Document document = new Document();
        PdfSmartCopy copy = new PdfSmartCopy(document, outStream);
        document.Open();
        using PdfReader reader = new PdfReader(validInput);
        try
        {
            foreach (int page in neededPages)
            {
                copy.AddPage(copy.GetImportedPage(reader, page));
            }
        }
        finally
        {
            document.Close();
            copy.Close();
            reader.Close();
        }
    }

    public List<string> GetFields(string sFilename)
    {
        List<string> oList = new List<string>();
        string validPath = FileManager.ValidatePath(sFilename);
        if (!File.Exists(validPath))
            return null;
        PdfReader reader = new PdfReader(validPath);
        try
        {
            AcroFields oFieldsPDf = reader.AcroFields;
            foreach (KeyValuePair<string, AcroFields.Item> oItem in oFieldsPDf.Fields)
            {
                oList.Add(oItem.Key);
            }
            return oList;
        }
        finally
        {
            reader.Close();
        }
    }

    public List<string> GetFields(byte[] oFilePDF)
    {
        List<string> oList = new List<string>();
        PdfReader reader = new PdfReader(oFilePDF);
        try
        {
            AcroFields oFieldsPDf = reader.AcroFields;
            foreach (KeyValuePair<string, AcroFields.Item> oItem in oFieldsPDf.Fields)
            {
                oList.Add(oItem.Key);
            }
            return oList;
        }
        finally
        {
            reader.Close();
        }
    }


    public void Write(string sFileInput, string sFileOutput, GCollection<string, string> oFileds)
    {
        string validInput = FileManager.ValidatePath(sFileInput);
        string validOutput = FileManager.ValidatePath(sFileOutput);
        if (!File.Exists(validInput))
            return;
        PdfReader reader = new PdfReader(validInput);
        try
        {
            using FileStream outStream = new FileStream(validOutput, FileMode.Create, FileAccess.Write);
            PdfStamper pdfStamper = new PdfStamper(reader, outStream);
            try
            {
                AcroFields oFieldsPDf = pdfStamper.AcroFields;
                oFieldsPDf.GenerateAppearances = true;
                foreach (KeyValuePair<string, AcroFields.Item> oItem in oFieldsPDf.Fields)
                {
                    if (oFileds.ContainsKey(oItem.Key))
                    {
                        oFieldsPDf.SetField(oItem.Key, oFileds[oItem.Key]);
                    }
                }
                pdfStamper.FormFlattening = true;
            }
            finally
            {
                pdfStamper.Close();
            }
        }
        finally
        {
            reader.Close();
        }
    }

    public void WriteImage(string sFileInput, string sFileOutput, SkiaSharp.SKBitmap oImage, float x, float y)
    {
        using (Stream inputPdfStream = new FileStream(sFileInput, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (Stream outputPdfStream = new FileStream(sFileOutput, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var reader = new PdfReader(inputPdfStream);
            var stamper = new PdfStamper(reader, outputPdfStream);
            var pdfContentByte = stamper.GetOverContent(1);
            iTextSharp.text.Image oI = iTextSharp.text.Image.GetInstance(oImage, iTextSharp.text.BaseColor.White);
            oI.SetAbsolutePosition(x, y);
            pdfContentByte.AddImage(oI);
            stamper.Close();
        }
    }

    public void MergeFiles(string destinationFile, string[] sourceFiles)
    {
        string validDest = FileManager.ValidatePath(destinationFile);
        var readers = new List<PdfReader>();
        try
        {
            int f = 0;
            PdfReader reader = new PdfReader(FileManager.ValidatePath(sourceFiles[f]));
            readers.Add(reader);

            int n = reader.NumberOfPages;
            using FileStream destStream = new FileStream(validDest, FileMode.Create, FileAccess.Write);
            using Document document = new Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = PdfWriter.GetInstance(document, destStream);
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            while (f < sourceFiles.Length)
            {
                int i = 0;
                while (i < n)
                {
                    i++;
                    document.SetPageSize(reader.GetPageSizeWithRotation(i));
                    document.NewPage();
                    page = writer.GetImportedPage(reader, i);
                    rotation = reader.GetPageRotation(i);
                    if (rotation == 90 || rotation == 270)
                    {
                        cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                    }
                    else
                    {
                        cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                    }
                }
                f++;
                if (f < sourceFiles.Length)
                {
                    reader = new PdfReader(FileManager.ValidatePath(sourceFiles[f]));
                    readers.Add(reader);
                    n = reader.NumberOfPages;
                }
            }
            document.Close();
        }
        finally
        {
            foreach (var r in readers)
            {
                try { r.Close(); } catch { }
            }
        }
    }

    public byte[] MergeFiles(List<byte[]> sourceFiles)
    {
        using MemoryStream mergeStream = new MemoryStream();
        var readers = new List<PdfReader>();
        try
        {
            int f = 0;
            PdfReader reader = new PdfReader(sourceFiles[f]);
            readers.Add(reader);

            int n = reader.NumberOfPages;
            using Document document = new Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = PdfWriter.GetInstance(document, mergeStream);
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            while (f < sourceFiles.Count)
            {
                int i = 0;
                while (i < n)
                {
                    i++;
                    document.SetPageSize(reader.GetPageSizeWithRotation(i));
                    document.NewPage();
                    page = writer.GetImportedPage(reader, i);
                    rotation = reader.GetPageRotation(i);
                    if (rotation == 90 || rotation == 270)
                    {
                        cb.AddTemplate(page, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                    }
                    else
                    {
                        cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                    }
                }
                f++;
                if (f < sourceFiles.Count)
                {
                    reader = new PdfReader(sourceFiles[f]);
                    readers.Add(reader);
                    n = reader.NumberOfPages;
                }
            }
            document.Close();
        }
        finally
        {
            foreach (var r in readers)
            {
                try { r.Close(); } catch { }
            }
        }

        return mergeStream.ToArray();
    }
}

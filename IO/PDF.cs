namespace Solution.IO;
public class PDF
{
    public PDF() { }

    public PdfReader Open(string sFileInput)
    {
        return new PdfReader(sFileInput);
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
        PdfReader reader = new PdfReader(sFileInput);
        iTextSharp.text.Rectangle size = reader.GetPageSizeWithRotation(pageNumber);
        Document document = new Document(size);
        PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(sFileOutput, FileMode.Create, FileAccess.Write));
        document.AddTitle("Cedolino");
        document.AddSubject("Generatore Automatico");
        document.Open();
        PdfContentByte cb = writer.DirectContent;
        document.NewPage();
        PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
        cb.AddTemplate(page, 0, 0);
        document.Close();
    }

    /// <summary>
    /// Create a new pdf file getting only a list of pages specified form another pdf file.
    /// </summary>
    /// <param name="sFileInput">Path of the pdf file input.(Where pages are taken).</param>
    /// <param name="sFileOutput">Path of the new pdf file to be saved.</param>
    /// <param name="neededPages">List of the pages to be copied from input pdf file to the output pdf file.</param>
    public void CreatePdfFromDifferentPages(string sFileInput, string sFileOutput, IList<int> neededPages)
    {
        Document document = new Document();
        PdfSmartCopy copy = new PdfSmartCopy(document, new FileStream(sFileOutput, FileMode.Create));
        document.Open();
        foreach (int page in neededPages)
        {
            PdfReader reader = new PdfReader(sFileInput);
            copy.AddPage(copy.GetImportedPage(reader, page));
        }
    }

    public List<string> GetFields(string sFilename)
    {
        List<string> oList = new List<string>();
        if (!File.Exists(sFilename))
            return null;
        PdfReader reader = new PdfReader(sFilename);

        //StreamWriter oWriter = new StreamWriter(sFilename + ".pdf");
        //MemoryStream oWriter = new MemoryStream();
        //PdfStamper pdfStamper = new PdfStamper(reader, oWriter.BaseStream);
        //

        AcroFields oFieldsPDf = reader.AcroFields;
        //oFieldsPDf.GenerateAppearances = false;
        foreach (KeyValuePair<string, AcroFields.Item> oItem in oFieldsPDf.Fields)
        {
            oList.Add(oItem.Key);
        }
        //pdfStamper.Close();
        reader.Close();
        return oList;
    }

    public List<string> GetFields(byte[] oFilePDF)
    {
        List<string> oList = new List<string>();
        PdfReader reader = new PdfReader(oFilePDF);
        AcroFields oFieldsPDf = reader.AcroFields;
        //oFieldsPDf.GenerateAppearances = false;
        foreach (KeyValuePair<string, AcroFields.Item> oItem in oFieldsPDf.Fields)
        {
            oList.Add(oItem.Key);
        }
        //pdfStamper.Close();
        reader.Close();
        return oList;
    }


    public void Write(string sFileInput, string sFileOutput, GCollection<string, string> oFileds)
    {
        if (!File.Exists(sFileInput))
            return;
        PdfReader reader = new PdfReader(sFileInput);
        StreamWriter oWriter = new StreamWriter(sFileOutput);
        PdfStamper pdfStamper = new PdfStamper(reader, oWriter.BaseStream);
        //
        AcroFields oFieldsPDf = pdfStamper.AcroFields;
        oFieldsPDf.GenerateAppearances = true;
        foreach (KeyValuePair<string, AcroFields.Item> oItem in oFieldsPDf.Fields)
        {
            if (oFileds.ContainsKey(oItem.Key))
            {
                oFieldsPDf.SetField(oItem.Key, oFileds[oItem.Key]);
                //oFieldsPDf.SetFieldProperty(oItem.Key, "setfflags", PdfFormField.FF_READ_ONLY, null);

            }
        }
        pdfStamper.FormFlattening = true;
        pdfStamper.Close();
        reader.Close();
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

        try
        {
            int f = 0;
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(sourceFiles[f]);

            // we retrieve the total number of pages
            int n = reader.NumberOfPages;

            //Console.WriteLine("There are " + n + " pages in the original file.");
            // step 1: creation of a document-object
            Document document = new Document(reader.GetPageSizeWithRotation(1));
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(destinationFile, FileMode.Create));
            // step 3: we open the document
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            // step 4: we add content
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
                    //Console.WriteLine("Processed page " + i);
                }
                f++;
                if (f < sourceFiles.Length)
                {
                    reader = new PdfReader(sourceFiles[f]);
                    // we retrieve the total number of pages
                    n = reader.NumberOfPages;
                    //Console.WriteLine("There are " + n + " pages in the original file.");
                }
            }
            // step 5: we close the document
            document.Close();
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    public byte[] MergeFiles(List<byte[]> sourceFiles)
    {
        MemoryStream mergeStream = new MemoryStream();
        try
        {
            int f = 0;
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(sourceFiles[f]);

            // we retrieve the total number of pages
            int n = reader.NumberOfPages;

            //Console.WriteLine("There are " + n + " pages in the original file.");
            // step 1: creation of a document-object
            Document document = new Document(reader.GetPageSizeWithRotation(1));
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, mergeStream);
            // step 3: we open the document
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;
            int rotation;
            // step 4: we add content
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
                    //Console.WriteLine("Processed page " + i);
                }
                f++;
                if (f < sourceFiles.Count)
                {
                    reader = new PdfReader(sourceFiles[f]);
                    // we retrieve the total number of pages
                    n = reader.NumberOfPages;
                    //Console.WriteLine("There are " + n + " pages in the original file.");
                }
            }
            // step 5: we close the document
            document.Close();
        }
        catch (Exception e)
        {
            throw e;
        }

        return mergeStream.ToArray();
    }
}

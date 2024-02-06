namespace Solution.IO;
public class cXLS
{
    public void WriteToFile(string sFileXLS, IWorkbook hssfworkbook)
    {
        FileStream file = new FileStream(sFileXLS, FileMode.Create);
        hssfworkbook.Write(file, false);
        file.Close();
    }

    public byte[] Write(IWorkbook hssfworkbook)
    {
        MemoryStream oM = new MemoryStream();
        hssfworkbook.Write(oM, false);
        return oM.ToArray();
    }

    public List<int> GetColumns(ISheet sheet, string[] oColumnName)
    {
        IRow row = sheet.GetRow(0);
        List<int> oList = new List<int>();
        for (int j = 0; j < oColumnName.Length; j++)
        {
            for (int i = 0; i < row.LastCellNum; i++)
            {
                if (oColumnName[j].ToLower().Trim().Equals(row.GetCell(i).ToString().ToLower().Trim()))
                    oList.Add(i);
            }
        }
        if (oList.Count == oColumnName.Length)
            return oList;
        return null;
    }

    public HSSFWorkbook NewFile()
    {
        HSSFWorkbook workbook = new HSSFWorkbook();
        // Create two sheet by calling createSheet of workbook.
        workbook.CreateSheet("Foglio 1");
        return workbook;
    }

    public XSSFWorkbook NewFileXLSX()
    {
        XSSFWorkbook workbook = new XSSFWorkbook();
        // Create two sheet by calling createSheet of workbook.
        workbook.CreateSheet("Foglio 1");
        return workbook;
    }

    public HSSFWorkbook ReadFromFile(string sFileXLS)
    {
        try
        {
            HSSFWorkbook oH = new HSSFWorkbook(new FileStream(sFileXLS, FileMode.Open));
            return oH;
        }
        catch (IOException)
        {
            throw;
        }
    }

    public HSSFWorkbook ReadFromStream(Stream oFile)
    {
        try
        {
            HSSFWorkbook oH = new HSSFWorkbook(oFile);
            return oH;
        }
        catch (IOException)
        {
            throw;
        }
    }

    public XSSFWorkbook ReadFromFileXLSX(string sFileXLSX)
    {
        try
        {
            XSSFWorkbook oH = new XSSFWorkbook(new FileStream(sFileXLSX, FileMode.Open));
            return oH;
        }
        catch (IOException)
        {
            throw;
        }
    }

    public XSSFWorkbook ReadFromStreamXLSX(Stream oFile)
    {
        try
        {
            XSSFWorkbook oH = new XSSFWorkbook(oFile);
            return oH;
        }
        catch (IOException)
        {
            throw;
        }
    }

    public string GetValue(ICell oCell)
    {
        if (oCell == null)
            return null;
        if (oCell.CellType == CellType.Numeric)
            return oCell.NumericCellValue.ToString();
        return oCell.StringCellValue;
    }

    public DateTime GetDate(ICell oCell)
    {
        try
        {
            return oCell.DateCellValue;
        }
        catch
        {
            return new DateTime(Convert.ToInt32(oCell.StringCellValue.Substring(6, 4)), Convert.ToInt32(oCell.StringCellValue.Substring(3, 2)), Convert.ToInt32(oCell.StringCellValue.Substring(0, 2)));
        }
    }

    public void esportaExcel(DataTable dt, ref ISheet sheet)
    {
        int iRiga = 1;
        int iMaxLCols = dt.Columns.Count;
        try
        {
            IRow row = sheet.CreateRow(0);
            for (int j = 0; j < iMaxLCols; j++)
            {
                ICell cell = row.CreateCell(j);
                cell.SetCellValue(dt.Columns[j].ColumnName);
            }
            ICellStyle style1 = sheet.Workbook.CreateCellStyle();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Create a row and put some cells in it. Rows are 0 based.
                row = sheet.CreateRow(iRiga + i);
                object[] oValues = dt.Rows[i].ItemArray;
                //bool bIsNew = false;
                for (int j = 0; j < iMaxLCols; j++)
                {
                    // Create a cell and put a date value in it.  The first cell is not styled as a date.
                    ICell cell = row.CreateCell(j);
                    if (j < oValues.Length)
                    {
                        cell.SetCellValue(oValues[j].ToString());
                    }
                    //if (bIsNew)
                    //    cell.CellStyle = style1;
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public DataTable GetDataTable(ISheet sheet)
    {
        DataTable dt = new DataTable();

        //FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        //HSSFWorkbook workbook = new HSSFWorkbook(fs, true);
        //ISheet sheet = workbook.GetSheet(workbook.Workbook.GetSheetName(0));



        int columnsTotal = sheet.GetRow(0).Cells.Count;
        int ir = 0;

        for (IEnumerator ie = sheet.GetRowEnumerator(); ie.MoveNext();)
        {
            IRow dataRow = (IRow)ie.Current;
            DataRow tableRow = dt.NewRow();

            for (int i = 0; i < columnsTotal; i++)
            {
                ICell cell = dataRow.GetCell(i);
                string value = null;
                try
                {
                    switch (cell.CellType)
                    {
                        case CellType.Numeric: // Numeric type
                            if (DateUtil.IsCellDateFormatted(cell))
                                value = cell.DateCellValue.ToString();
                            else
                                value = cell.NumericCellValue.ToString();
                            break;

                        default: // String type
                            value = cell.StringCellValue;
                            break;
                    }
                }
                catch
                {
                }

                if (ir == 0)
                {
                    dt.Columns.Add(value);
                }
                else
                {
                    tableRow[i] = value;
                }
            }
            if (ir > 0)
                dt.Rows.Add(tableRow);
            ir++;
        }
        return dt;
    }

    public byte[] GetXLSFromDatatable(DataTable dt, int numCol, bool showColumnName = false)
    {
        byte[] fileExcel = null;
        try
        {
            int iRiga = 0;
            int iMaxLCols = numCol;
            HSSFWorkbook workbook = this.NewFile();
            ISheet sheet = workbook.GetSheet("Foglio 1");
            ICellStyle style1 = workbook.CreateCellStyle();


            if (showColumnName)
            {
                ICellStyle style0 = workbook.CreateCellStyle();

                IFont font0 = workbook.CreateFont();
                font0.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
                style0.SetFont(font0);
                style0.FillForegroundColor = IndexedColors.CornflowerBlue.Index;
                style0.FillPattern = FillPattern.SolidForeground;

                IRow row = sheet.CreateRow(0);
                int iCol = 0;
                foreach (DataColumn c in dt.Columns)
                {
                    ICell cell = row.CreateCell(iCol);
                    cell.SetCellValue(c.ColumnName);
                    cell.CellStyle = style0;

                    iCol++;
                }

                iRiga = 1;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet.CreateRow(iRiga + i);
                object[] oValues = dt.Rows[i].ItemArray;
                bool bIsNew = false; //DateTime.Parse(oDT.Rows[i]["createdon"].ToString()) >= DateTime.Parse(Params) ? true : false;
                for (int j = 0; j < iMaxLCols; j++)
                {
                    ICell cell = row.CreateCell(j);
                    if (j < oValues.Length)
                    {
                        cell.SetCellValue(oValues[j].ToString());
                    }
                    if (bIsNew)
                        cell.CellStyle = style1;
                }
            }
            fileExcel = this.Write(workbook);
        }
        catch (Exception ex)
        {
        }
        return fileExcel;
    }

    public byte[] returnCSVfromDatatable(DataTable dt)
    {

        int iRows = 0;
        int nRows = dt.Rows.Count;

        int iCols = 0;
        int nCols = dt.Columns.Count;

        string sFile = "";
        while (iRows < nRows)
        {
            iCols = 0;
            while (iCols < nCols)
            {
                sFile += dt.Rows[iRows][iCols].ToString() + ";";

                iCols++;
            }
            sFile = sFile.Substring(0, sFile.Length - 1);
            sFile += "\n";

            iRows++;
        }

        return Encoding.GetEncoding("iso-8859-1").GetBytes(sFile);

    }

    public byte[] GetXLSXFromDatatable(DataTable dt, int numCol, bool showColumnName = false)
    {
        byte[] fileExcel = null;
        try
        {
            int iRiga = 0;
            int iMaxLCols = numCol;
            XSSFWorkbook workbook = this.NewFileXLSX();
            ISheet sheet = workbook.GetSheet("Foglio 1");
            ICellStyle style1 = workbook.CreateCellStyle();

            if (showColumnName)
            {
                ICellStyle style0 = workbook.CreateCellStyle();

                IFont font0 = workbook.CreateFont();
                font0.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
                style0.SetFont(font0);
                style0.FillForegroundColor = IndexedColors.CornflowerBlue.Index;
                style0.FillPattern = FillPattern.SolidForeground;

                IRow row = sheet.CreateRow(0);
                int iCol = 0;
                for (int j = 0; j < iMaxLCols; j++)
                {
                    ICell cell = row.CreateCell(iCol);
                    cell.SetCellValue(dt.Columns[j].ColumnName);
                    cell.CellStyle = style0;

                    iCol++;
                }

                iRiga = 1;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                IRow row = sheet.CreateRow(iRiga + i);
                object[] oValues = dt.Rows[i].ItemArray;
                bool bIsNew = false; //DateTime.Parse(oDT.Rows[i]["createdon"].ToString()) >= DateTime.Parse(Params) ? true : false;
                for (int j = 0; j < iMaxLCols; j++)
                {
                    ICell cell = row.CreateCell(j);
                    if (j < oValues.Length)
                    {
                        cell.SetCellValue(oValues[j].ToString());
                    }
                    if (bIsNew)
                        cell.CellStyle = style1;
                }
            }
            fileExcel = this.Write(workbook);
        }
        catch (Exception ex)
        {
        }
        return fileExcel;
    }

    /// <summary>
    /// Deletes empty rows from the end of the given worksheet
    /// </summary>
    public ISheet DeleteEmptyRows(ISheet worksheet)
    {
        ISheet _sheet = worksheet;

        for (int i = 0; i <= worksheet.LastRowNum; i++)
        {
            var row = worksheet.GetRow(i);

            if (this.IsEmptyRow(row))
                _sheet.RemoveRow(row);

        }
        //Console.WriteLine("FINE");

        return _sheet;
    }

    public bool IsEmptyRow(IRow row)
    {
        return (row.Cells.Where(x => String.IsNullOrEmpty(x.ToString())).Count() == row.Cells.Count);
    }

    public DataTable ConvertCSVtoDataTable(string strFilePath, char sSeparator = ';', bool bAutoGenerateProgressiveColumn = false)
    {
        DataTable dt = new DataTable();
        StreamReader sr = new StreamReader(strFilePath);
        if (!bAutoGenerateProgressiveColumn)
        {
            string[] headers = sr.ReadLine().Split(sSeparator);
            foreach (string header in headers)
            {
                dt.Columns.Add(header);
            }
        }
        while (!sr.EndOfStream)
        {
            string[] row = sr.ReadLine().Split(sSeparator);
            if (dt.Columns == null || dt.Columns.Count == 0)
            {
                for (int indexColumn = 0; indexColumn < row.Length; indexColumn++)
                {
                    dt.Columns.Add("Colonna" + (indexColumn + 1).ToString());
                }
            }
            DataRow dr = dt.NewRow();
            for (int i = 0; i < row.Length; i++)
            {
                dr[i] = row[i];
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }

    public string ConvertDataTableToCSV(DataTable oDT, string sSeparator = ";", bool bAutoGenerateColumn = true)
    {
        StringBuilder sb = new StringBuilder();
        List<string> columnNames = new List<string>();
        if (bAutoGenerateColumn)
        {
            foreach (DataColumn col in oDT.Columns)
            {
                columnNames.Add(col.ColumnName);
            }
            sb.AppendLine(string.Join(sSeparator, columnNames));
        }
        foreach (DataRow row in oDT.Rows)
        {
            //List<string> sList = row.ItemArray.Select(y => string.Concat("", "\"", y.ToString().Replace("\"", "\"\""), "\"")).ToList();
            List<string> sList = row.ItemArray.Select(y => y.ToString()).ToList();
            string newLine = string.Join(sSeparator, sList);
            sb.AppendLine(newLine);
        }
        return sb.ToString();
    }
}

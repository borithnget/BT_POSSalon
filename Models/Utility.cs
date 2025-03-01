using OfficeOpenXml.Drawing;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System;
using System.Drawing;
using System.Linq;

public class Utility
{
    public static (DataTable, List<T>) ProcessExcelWithImages<T>(Stream fileStream, string saveDirectory) where T : class, new()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        DataTable dataTable = new DataTable();
        var dataList = new List<T>();
        var imagePaths = new Dictionary<int, string>(); 

        using (var package = new ExcelPackage(fileStream))
        {
            var worksheet = package.Workbook.Worksheets[0];
            dataTable.Clear();
            dataTable.Columns.Clear();
            dataTable.Rows.Clear();
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                dataTable.Columns.Add(worksheet.Cells[1, col].Text.Trim());
            }
            dataTable.Columns.Add("Image");
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                DataRow dataRow = dataTable.NewRow();
                var dataItem = new T();

                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var cell = worksheet.Cells[row, col];
                    string value = cell.Text.Trim();

                
                    string mergedRangeAddress = null;
                    foreach (var range in worksheet.MergedCells)
                    {
                        var mergedRange = new ExcelAddress(range);
                        if (row >= mergedRange.Start.Row && row <= mergedRange.End.Row &&
                            col >= mergedRange.Start.Column && col <= mergedRange.End.Column)
                        {
                            mergedRangeAddress = range;
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(mergedRangeAddress))
                    {
                        var mergedRange = new ExcelAddress(mergedRangeAddress);
                        value = worksheet.Cells[mergedRange.Start.Row, mergedRange.Start.Column].Text.Trim();
                    }

                    dataRow[col - 1] = value;
                    var columnName = dataTable.Columns[col - 1].ColumnName;
                    var property = typeof(T).GetProperty(columnName);
                    if (property == null)
                    {
                        property = typeof(T).GetProperties().ElementAtOrDefault(col - 1);
                    }

                    if (property != null && !string.IsNullOrEmpty(value))
                    {
                            property.SetValue(dataItem, Convert.ChangeType(value, property.PropertyType));
                    }
                }

                dataTable.Rows.Add(dataRow);
                dataList.Add(dataItem);
            }
            var drawings = worksheet.Drawings;
            int imageCounter = 1;

            foreach (var drawing in drawings)
            {
                if (drawing is ExcelPicture excelPicture)
                {
                    var rowIndex = excelPicture.From.Row + 1; 
                    string uniqueFileName = $"image_{imageCounter}_{Guid.NewGuid()}.png";
                    string fileName = Path.Combine(saveDirectory, uniqueFileName);
                    using (var memoryStream = new MemoryStream(excelPicture.Image.ImageBytes))
                    using (var image = Image.FromStream(memoryStream))
                    {
                        image.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    if (rowIndex >= 1 && rowIndex - 1 < dataTable.Rows.Count)
                    {
                        int dataTableRowIndex = rowIndex - 1;
                        string imagePath = "/Images/" + uniqueFileName;
                        dataTable.Rows[dataTableRowIndex]["Image"] = imagePath;

                        if (dataList.Count > dataTableRowIndex)
                        {
                            var imageProperty = typeof(T).GetProperty("ImagePath");
                            if (imageProperty != null)
                            {
                                imageProperty.SetValue(dataList[dataTableRowIndex], imagePath);
                            }
                        }
                    }

                    imageCounter++;
                }
            }

        }

        return (dataTable, dataList);
    }

}

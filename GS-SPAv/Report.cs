using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Service;
using Excel = Microsoft.Office.Interop.Excel;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Text.RegularExpressions;
using System.Globalization;

namespace GS_SPAv {
  public class Report {
    private ObservableCollection<DataParam> dataParams;
    private ObservableCollection<Stage> stages;

    public void Do(WorkInfo wi, ObservableCollection<DataParam> dataParams, ObservableCollection<Stage> stages) {
      bool nullChecked = true;
      foreach (Stage stage in stages) {
        if (stage.IsChecked)
          nullChecked = false;
      }
      if (nullChecked) {
        MessageBox.Show("Не выбраны ни одна стадия", "Внимание", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }
      this.dataParams = dataParams;
      this.stages = stages;

      /*Excel.Application appXL = new Excel.Application();
      Excel._Workbook workbook = (Excel._Workbook)appXL.Workbooks.Add(Missing.Value);
      Excel._Worksheet worksheet = (Excel._Worksheet)workbook.ActiveSheet;
      worksheet.Name = "Все данные";
      ((Excel.Range)worksheet.Columns["C:C", Missing.Value]).ColumnWidth = 10;
      worksheet.Cells[1,1] = "Подрядчик:";
      worksheet.Cells[1, 3].Value = wi.Builder;
      worksheet.Cells[2, 1].Value = "Скважина:";
      worksheet.Cells[2, 3].Value = wi.Well;
      worksheet.Cells[3, 1].Value = "Месторождение:";
      worksheet.Cells[3, 3].Value = wi.Field;
      worksheet.Cells[4, 1].Value = "Ответственный:";
      worksheet.Cells[4, 3].Value = wi.FIO;
      worksheet.Cells[5, 1].Value = "Начало:";
      worksheet.Cells[5, 3].Value = wi.Start.Substring(0, 10);
      worksheet.Cells[5, 4].Value = wi.Start.Substring(10);

      worksheet.Cells[6, 1].Value = "Время";
      Excel.Range rngExcel = null;
      string[,] clnDataDateTime = new string[dataParams[0].Points.Count, 1];
      rngExcel = worksheet.get_Range("A7", Missing.Value);
      rngExcel = rngExcel.get_Resize(dataParams[0].Points.Count, 1);
      for (int i = 0; i < dataParams[0].Points.Count; i++) {
        clnDataDateTime[i, 0] = dataParams[0].Points[i].DateTime.ToString("HH:mm:ss");
      }
      rngExcel.set_Value(Missing.Value, clnDataDateTime);

      Stopwatch sw = new Stopwatch();
      sw.Start();
      int column = 1;

      string columnLetter = char.ConvertFromUtf32("A".ToCharArray()[0] + column);
      float[,] clnDataString = new float[dataParams[0].Points.Count, 18];
      rngExcel = worksheet.get_Range(columnLetter + "7", "S" + dataParams[0].Points.Count + 7);
      //rngExcel = rngExcel.get_Resize(dataParams[0].Points.Count, 18);
      foreach (DataParam param in dataParams) {
        worksheet.Cells[6, column+1].Value = param.Title;        
        for (int i = 0; i < param.Points.Count; i++) {
          clnDataString[i, column-1] = param.Points[i].Value;
        }
        column++;
      }
      rngExcel.set_Value(Missing.Value, clnDataString);
      sw.Stop();
      MessageBox.Show(sw.ElapsedMilliseconds.ToString());
      //WriteStages(workbook);

      workbook.SaveAs(Directory.GetCurrentDirectory() + "\\"  + DateTime.Now.ToString("dd_MM_yyyy_hh_mm") + ".xlsx");
      appXL.UserControl = true;
      appXL.Visible = true;*/

      string fileName = Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".xlsm";
      File.Copy(Directory.GetCurrentDirectory() + "\\template.xlsm", fileName);


      using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, true, new OpenSettings())) {

        WorkbookPart workbookPart = document.WorkbookPart;
        workbookPart.Workbook = new Workbook();
        var stylePart = document.WorkbookPart.WorkbookStylesPart;
        stylePart.Stylesheet = new Stylesheet {
          Fonts = new Fonts(new Font()),
          Fills = new Fills(new Fill()),
          Borders = new Borders(new Border()),
          CellStyleFormats = new CellStyleFormats(new CellFormat()),
          CellFormats = new CellFormats(
            new CellFormat(),
            new CellFormat { NumberFormatId = 21, ApplyNumberFormat = true })
        };

        WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        worksheetPart.Worksheet = new Worksheet(new SheetData());
        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
        Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Общие данные" };
        sheets.Append(sheet);
        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        Columns lstColumns = new Columns();
        for (int i = 0; i < 20; i++)
          lstColumns.Append(new Column() { Min = 1, Max = 10, Width = 11, CustomWidth = true });
        {
          Row row = new Row() { RowIndex = 1 };
          InsertCell(row, 1, "Подрядчик:", CellValues.String);
          InsertCell(row, 2, "", CellValues.String);
          InsertCell(row, 3, wi.Builder, CellValues.String);
          sheetData.Append(row);
        }
        {
          Row row = new Row() { RowIndex = 2 };
          InsertCell(row, 1, "Скважина:", CellValues.String);
          InsertCell(row, 2, "", CellValues.String);
          InsertCell(row, 3, wi.Well, CellValues.String);
          sheetData.Append(row);
        }
        {
          Row row = new Row() { RowIndex = 3 };
          InsertCell(row, 1, "Месторождение:", CellValues.String);
          InsertCell(row, 2, "", CellValues.String);
          InsertCell(row, 3, wi.Field, CellValues.String);
          sheetData.Append(row);
        }
        {
          Row row = new Row() { RowIndex = 4 };
          InsertCell(row, 1, "Ответственный:", CellValues.String);
          InsertCell(row, 2, "", CellValues.String);
          InsertCell(row, 3, wi.FIO, CellValues.String);
          sheetData.Append(row);
        }
        {
          Row row = new Row() { RowIndex = 5 };
          InsertCell(row, 1, "Начало:", CellValues.String);
          InsertCell(row, 2, "", CellValues.String);
          InsertCell(row, 3, wi.Start.Substring(0, 10), CellValues.String);
          InsertCell(row, 4, wi.Start.Substring(10), CellValues.String);
          sheetData.Append(row);
        }
        {
          Row row = new Row() { RowIndex = 6 };
          InsertCell(row, 1, "Время", CellValues.String);
          for (int i = 0; i < dataParams.Count; i++) {
            InsertCell(row, i + 2, dataParams[i].Title, CellValues.String);
          }
          sheetData.Append(row);
        }
        for (int i = 0; i < dataParams[0].Points.Count; i++) {
          Row row = new Row() { RowIndex = (uint)(i + 7) };
          sheetData.Append(row);
          InsertCell(row, 1, dataParams[0].Points[i].DateTime.ToOADate().ToString("0.0000000").Replace(',', '.'), CellValues.Number, true);
          foreach (DataParam param in dataParams) {
            InsertCell(row, i + 2, param.Points[i].Value.ToString("0.000").Replace(',', '.'), CellValues.Number);
          }
        }
        worksheetPart.Worksheet.InsertAt(lstColumns, 0);

        uint sheetIndex = 2;
        foreach (Stage stage in stages) {
          if (stage.IsSecond || !stage.IsChecked) continue;
          DateTime start = stage.DateTime;
          Stage second = GetSecondStage(stage.ID);
          DateTime end;
          if (second == null)
            end = dataParams[0].Points[dataParams[0].Points.Count - 1].DateTime;
          else
            end = second.DateTime;

          int pointStart = -1;
          int pointEnd = -1;
          for (int point = 0; point < dataParams[0].Points.Count; point++) {
            if (pointStart == -1 && (dataParams[0].Points[point].DateTime >= stage.DateTime))
              pointStart = point;
            if (dataParams[0].Points[point].DateTime <= end)
              pointEnd = point + 1;
          }

          WorksheetPart worksheetPart2 = workbookPart.AddNewPart<WorksheetPart>();
          worksheetPart2.Worksheet = new Worksheet(new SheetData());

          Sheet sheet2 = new Sheet() {
            Id = workbookPart.GetIdOfPart(worksheetPart2),
            SheetId = sheetIndex,
            Name = stage.Text + " (c." + (sheetIndex - 1).ToString() + ")"
          };
          sheetIndex++;
          sheets.Append(sheet2);
          SheetData sheetData2 = worksheetPart2.Worksheet.GetFirstChild<SheetData>();
          Columns lstColumns2 = new Columns(); for (int i = 0; i < 20; i++)
            lstColumns2.Append(new Column() { Min = 1, Max = 10, Width = 11, CustomWidth = true });
          {
            Row row = new Row() { RowIndex = 1 };
            InsertCell(row, 1, "Начало:", CellValues.String);
            InsertCell(row, 2, start.ToString("dd.MM.yyyy"), CellValues.String);
            InsertCell(row, 3, start.ToString("HH:mm:ss"), CellValues.String);
            sheetData2.Append(row);
          }
          {
            Row row = new Row() { RowIndex = 2 };
            InsertCell(row, 1, "Конец:", CellValues.String);
            InsertCell(row, 2, end.ToString("dd.MM.yyyy"), CellValues.String);
            InsertCell(row, 3, end.ToString("HH:mm:ss"), CellValues.String);
            sheetData2.Append(row);
          }
          {
            Row row = new Row() { RowIndex = 3 };
            InsertCell(row, 1, "Время", CellValues.String);
            for (int i = 0; i < dataParams.Count; i++) {
              InsertCell(row, i + 2, dataParams[i].Title, CellValues.String);
            }
            sheetData2.Append(row);
          }
          int indexPoint = pointStart;
          for (int i = 0; i < pointEnd - pointStart; i++, indexPoint++) {
            Row row = new Row() { RowIndex = (uint)(i + 4) };
            sheetData2.Append(row);
            InsertCell(row, 1, dataParams[0].Points[indexPoint].DateTime.ToOADate().ToString("0.0000000").Replace(',', '.'), CellValues.Number, true);
            foreach (DataParam param in dataParams) {
              InsertCell(row, i + 2, param.Points[indexPoint].Value.ToString("0.000").Replace(',', '.'), CellValues.Number);
            }
          }
          worksheetPart2.Worksheet.InsertAt(lstColumns2, 0);

        }
        workbookPart.Workbook.Save();
        document.Close();
      }
      Excel.Application excel = new Excel.Application();
      Excel.Workbook wb = excel.Workbooks.Open(fileName);
      excel.ScreenUpdating = true;
      excel.Run("newGraphics");
      excel.Visible = true;
      wb.Save();
      //EndProcessExcel();
    }

    private void EndProcessExcel() {
      // Получаем все процессы
      Process[] procList = Process.GetProcesses();
      foreach (Process p in procList) {
        if (p.ProcessName.ToString().Trim().ToUpper() == "EXCEL") {
          // Завершаем процесс
          p.Kill();
        }
      }
    }

    static void InsertCell(Row row, int cell_num, string val, CellValues type, bool date = false) {
      Cell refCell = null;
      Cell newCell = new Cell() { CellReference = cell_num.ToString() + ":" + row.RowIndex.ToString() };
      newCell.DataType = new EnumValue<CellValues>(type);
      newCell.CellValue = new CellValue(val);
      if (date) {
        
        newCell.StyleIndex = 1;
      }
      row.InsertBefore(newCell, refCell);
    }

    private Stage GetSecondStage(int id) {
      foreach (Stage stage in stages)
        if (stage.IsSecond && stage.ID == id)
          return stage;
      return null;
    }
  }
}

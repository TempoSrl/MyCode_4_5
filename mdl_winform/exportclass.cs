 using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
//using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Reflection;
using System.Threading;
//using Microsoft.Office.Interop.Excel;
using System.Globalization;
using mdl;
// Utilizzato per EPPlus
using OfficeOpenXml;
using OfficeOpenXml.Style;
using LM=mdl_language.LanguageManager;
using static mdl_utils.tagUtils;
using mdl_utils;

#pragma warning disable IDE1006 // Naming Styles



namespace mdl_winform {
     /// <summary>
     /// Classe per l'esportazione di Dati.
     /// Reference: Microsoft Excel 9.0 Object Library
     /// Metodo da utilizzare per l'esportazione in Excel:
     /// public static void DataTableToExcel(System.Data.DataTable DT,bool Header)
     /// Codice di chiamata: exportdata.exportclass.DataTableToExcel(MyDS.Tables[0],true);
     /// Max 08 ottobre 2002
     /// </summary>
     public class exportclass {
         private static object Value(DataRow r) {
             if (r == null) return DBNull.Value;
             if (r.Table.Columns.Contains("Descrizione")) return r["Descrizione"];
             if (r.Table.Columns.Contains("value")) return r["value"];
             return DBNull.Value;
         }

         private static object Kind(DataRow r) {
             if (r == null) return DBNull.Value;
             if (r.Table.Columns.Contains("Formato")) return r["Formato"];
             if (r.Table.Columns.Contains("kind")) return r["kind"];
             return DBNull.Value;
         }

         private static object Amount(DataRow r) {
             if (r == null) return DBNull.Value;
             if (r.Table.Columns.Contains("Importo")) return r["Importo"];
             if (r.Table.Columns.Contains("amount")) return r["amount"];
             return DBNull.Value;
         }


         /// <summary>
         /// Visualizza una situazione (memorizzata in un DataTable) in un DataGrid
         /// </summary>
         /// <param name="dt">DataTable con la situazione</param>
         /// <param name="dg">Grid in cui visualizzare la situazione</param>
         public static void SituazioneToDataGrid(System.Data.DataTable dt, System.Windows.Forms.DataGrid dg) {
             DataSet myDs = new DataSet();
             System.Data.DataTable myDt = new System.Data.DataTable(LM.translate("result", true));
             myDs.Tables.Add(myDt);
             string descrColName = LM.translate("description", true);
             string amountColName = LM.translate("amount", true);
             myDt.Columns.Add(new DataColumn(descrColName, typeof(string)));
             myDt.Columns.Add(new DataColumn(LM.translate("amount", true), typeof(string)));
             foreach (DataRow dr in dt.Rows) {
                 DataRow myDr = myDt.NewRow();
                 string myFormat = Kind(dr).ToString();
                 switch (myFormat) {
                     case "H":
                     case " ":
                     case "S":
                         object Desc = Value(dr);
                         if (Desc.ToString() == "NULL") myDr[descrColName] = "";
                         else myDr[descrColName] = Desc;

                         myDr[amountColName] = Amount(dr);
                         //object amount = Amount(DR);
                         //if (amount.ToString() == "") MyDR["Importo"] = 0;
                         //else MyDR["Importo"] = amount;
                         myDt.Rows.Add(myDr);
                         continue;
                     case "N":
                         //LASCIA LA RIGA VUOTA
                         myDt.Rows.Add(myDr);
                         continue;
                 }
             }

             dg.SetDataBinding(myDt.DataSet, myDt.TableName);
             dg.DataMember = (myDt.TableName);


         }


//		/// <summary>
//		/// Get the string value to put in Excel in corrispondence of
//		///  an object value
//		/// </summary>
//		/// <param name="O"></param>
//		/// <returns></returns>
//		public static string ExcelValue(Object O){
//			if ((O==null)||(O==DBNull.Value)) return "";
//			string typename=O.GetType().Name;
//			if (typename=="DateTime"){
//				if (O.ToString()==QueryCreator.EmptyDate().ToString()) {
//					return "";                        
//				}
//				else {
//					return ((DateTime)O).ToShortDateString();
//				}
//			}  
//			else {
//				if (O.GetType().Name=="Decimal"){		
//					Decimal D = (Decimal) O;
//					return D.ToString("n");	
//					}
//
//				if (O.GetType().Name=="Double"){
//					Double D2 = (Double) O;
//					return D2.ToString("n");
//				}
//
//				if (O.GetType().Name=="Single"){
//					Single D3 = (Single) O;
//					return D3.ToString("n");
//				}
//
//				if (O.GetType().Name=="Int32"){
//					Int32 II = (Int32) O;
//					return II.ToString();
//				}
//				if (O.GetType().Name=="Int16"){
//					Int16 I2 = (Int16) O;
//					return I2.ToString();
//				}
//
//				return "'"+O.ToString();
//			}
//		}

         /// <summary>
         /// Save a data table to a file, asking user for  the file name
         /// </summary>
         /// <param name="dt"></param>
         /// <param name="header">when true, an header rows is inserted</param>
         public static void SaveTableToCSV(System.Data.DataTable dt, bool header) {
             SaveTableToCSV(dt, header, null);
         }

         /// <summary>
         /// Save a data table to a file, asking user for  the file name
         /// </summary>
         /// <param name="dt"></param>
         /// <param name="header"></param>
         /// <param name="filename"></param>
         public static void SaveTableToCSV(System.Data.DataTable dt, bool header, string filename) {
             if (filename == null) {
                var FD = new OpenFileDialog {
                    Title = LM.selectFile,
                    AddExtension = true,
                    DefaultExt = "CSV",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    Multiselect = false
                };
                var DR = FD.ShowDialog();
                 if (DR != System.Windows.Forms.DialogResult.OK) return;
                 filename = FD.FileName;
             }



             
                 string S = DataTableToCSV(dt, true);
                 var SWR = new StreamWriter(filename, false, Encoding.Default);
                 SWR.Write(S);
                 SWR.Close();
                 //SWR.Dispose();
             
             Process.Start(filename);
         }

         #region Salvataggio delle DataTable in formato Excel

         /// <summary>
         /// Save a datatable to an xlsx file
         /// </summary>
         /// <param name="DT">Input table</param>
         /// <param name="Header">true if headers have to be shown in Excel</param>
         public static void DataTableToExcel(System.Data.DataTable DT, bool Header) {
             DataTableToExcel(DT, Header, null, null);
         }

         /// <summary>
         /// Save a datatable to an xlsx file
         /// </summary>
         /// <param name="DT">Input table</param>
         /// <param name="Header">true if headers have to be shown in Excel</param>
         /// <param name="groupby">column position of column onto which create a group</param>
         /// <param name="totals">columns numbeposition of columns to be totalized</param>
         public static void DataTableToExcel(System.Data.DataTable DT, bool Header, int[] groupby, object[] totals) {
             iDataTableToExcel(DT, Header, groupby, totals);
         }

         /// <summary>
         /// Save a datatable to an xlsx file
         /// </summary>
         /// <param name="DT">Input table</param>
         /// <param name="Header">true if headers have to be shown in Excel</param>
         /// <param name="groupby">column position of column onto which create a group</param>
         /// <param name="totals">columns numbeposition of columns to be totalized</param>
         /// <param name="filename">filename of file to create</param>
         /// <returns></returns>
         public static string DataTableToExcel(System.Data.DataTable DT, bool Header, int[] groupby, object[] totals,
             string filename) {
            return iDataTableToExcel(DT, Header, groupby, totals, filename);
        }

         /// <summary>
         /// Export all data in a datatable into an Excel sheet
         /// </summary>
         /// <param name="DT">DataTable to export</param>
         /// <param name="Header">true if headers have to be shown in Excel</param>
         /// <param name="groupby">columns number to be grouped</param>
         /// <param name="totals">columns number to be totalized</param>
         private static void iDataTableToExcel(System.Data.DataTable DT, bool Header, int[] groupby, object[] totals) {

             // Codice inserito per debug di iDataTableToOfficeXML
             // -------------------------------------------------------------------------
             if (DT.Select().Length == 0) return;
             string filename = Path.GetTempFileName() + ".xlsx";
             string retCode = iDataTableToOfficeXML(DT, Header, groupby, totals, filename);

             if (retCode != null) return;
             Process.Start(filename);


             // -------------------------------------------------------------------------
         }

         private static string iDataTableToExcel(System.Data.DataTable DT, bool Header, int[] groupby, object[] totals,
             string filename) {

             // Codice inserito per debug di iDataTableToOfficeXML
             // -------------------------------------------------------------------------
             if (DT.Select().Length == 0) return null;
             string retCode = iDataTableToOfficeXML(DT, Header, groupby, totals, filename);
             // -------------------------------------------------------------------------
             return retCode;

             #region CodiceDaCancellare

             /*
             if (DT.Select().Length == 0) return "Esportazione vuota";
             Microsoft.Office.Interop.Excel.Application m_objExcel;
             try {
                 m_objExcel = new Microsoft.Office.Interop.Excel.Application();
                 m_objExcel.Visible = false;
                 m_objExcel.DisplayAlerts = false;
                 m_objExcel.Interactive = false;
                 m_objExcel.SheetsInNewWorkbook = 1;
 
             }
             catch {
                 SaveTableToCSV(DT, Header, filename);
                 return null;
                 //return "Non è possibile eseguire l'esportazione in Excel. " +
                 //    "Excel non è installato su questo computer o è presente una versione " +
                 //    "non compatibile con l'oggetto COM: Microsoft Excel 9.0 Object Library";
             }
 
             Workbook MyWorkbook; // = m_objExcel.Workbooks.Add(-4167);	//Numero magico by Nino
 
             try {
                 MyWorkbook = m_objExcel.Workbooks.Add(); //Numero magico by Nino
             }
             catch (Exception E) {
 
                 m_objExcel.Quit();
                 releaseObject(m_objExcel);
                 QueryCreator.ShowException(
                     "A causa di un bug di Office, documentato qui: http://support.microsoft.com/kb/320369 " +
                     " è necessario installare Microsoft Office XP Multilingual User Interface Pack.", E);
                 SaveTableToCSV(DT, Header, filename);
                 return null;
             }
 
             try {
                 Worksheet Myworksheet = (Worksheet)MyWorkbook.Worksheets.get_Item(1);
                 int RowCount = DT.Select().Length; //Numero Righe del datatable
                 int ColumnCount = DT.Columns.Count; //Numero Colonne del datatable
                 int Step = 0;
                 //Attento:	Il Worksheet ha come base per le righe e colonne 1	(prima cella [1,1])
                 //			Il Datatable ha come base 0.	(prima cella [0,0])
                 //Aggiungo i titoli delle colonne
                 if (Header) {
                     waitReady(m_objExcel);
                     Range Head = (Range)Myworksheet.Range[
                         Myworksheet.Cells[1, 1],
                         Myworksheet.Cells[1, ColumnCount + 1]];
                     waitReady(m_objExcel);
                     Head.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                     waitReady(m_objExcel);
                     Head.Font.Bold = true;
                     Step = 1;
 
                 }
                 int mincol = 99999;
                 for (int Colonna = 0; Colonna < ColumnCount; Colonna++) {
                     DataColumn Col = DT.Columns[Colonna];
                     int ColonnaExcel = Colonna + 1;
                     if (Col.ExtendedProperties["ListColPos"] != null)
                         ColonnaExcel = Convert.ToInt32(Col.ExtendedProperties["ListColPos"]);
                     if (ColonnaExcel == -1) continue;
                     if (mincol > ColonnaExcel) mincol = ColonnaExcel;
 
                 }
                 int disp = 0;
                 if (mincol == 0) disp = 1;
 
                 DataRow[] Rows = DT.Select(null,
                     (string)DT.ExtendedProperties["ExcelSort"]);
                 //per ogni colonna del datatable:
                 int Excel_Col_Index = 0;
                 for (int Colonna = 0; Colonna < ColumnCount; Colonna++) {
                     DataColumn Col = DT.Columns[Colonna];
                     string caption = (string)Col.ExtendedProperties["ExcelTitle"];
                     if (caption == null) caption = DT.Columns[Colonna].Caption;
                     if (caption == "") continue;
                     if ((Col.ExtendedProperties["ListColPos"] == null) &&(caption.StartsWith("."))) continue;
 
                     int ColonnaExcel = Excel_Col_Index + 1;
                     if (Col.ExtendedProperties["ListColPos"] != null) ColonnaExcel = Convert.ToInt32(Col.ExtendedProperties["ListColPos"]);
 
                     if (ColonnaExcel == -1) continue;
 
                     if (caption.StartsWith(".")) caption = caption.Remove(0, 1);
                     Excel_Col_Index++;
 
                     if (Col.ExtendedProperties["ExcelFormat"] != null) {
                         try {
                             waitReady(m_objExcel);
                             Range ExcCol = (Range)Myworksheet.Range[
                                 Myworksheet.Cells[1 + Step, ColonnaExcel + disp],
                                 Myworksheet.Cells[RowCount + Step, ColonnaExcel + disp]];
                             waitReady(m_objExcel);
                             ExcCol.NumberFormat = Col.ExtendedProperties["ExcelFormat"].ToString();
                         }
                         catch (Exception E) {
                             m_objExcel.Quit();
                             releaseObject(m_objExcel);
                             return E.Message;
                         }
                     }
                     Object[,] arr;
                     if (Header) {
                         arr = new Object[RowCount + 1, 1];
                         arr[0, 0] = caption;
                     }
                     else
                         arr = new Object[RowCount, 1];
                     string Tag = "x.y";
                     Tag = HelpForm.CompleteTag(Tag, Col);
                     for (int Riga = 0; Riga < RowCount; Riga++) {
                         if (Rows[Riga][Colonna] == DBNull.Value) {
                             arr[Riga + Step, 0] = "";
                             continue;
                         }
                         if (DT.Columns[Colonna].DataType == typeof(String)) {
                             arr[Riga + Step, 0] = "'" + HelpForm.StringValue(Rows[Riga][Colonna], Tag);
                         }
                         else {
                             arr[Riga + Step, 0] = HelpForm.StringValue(Rows[Riga][Colonna], Tag);
                         }
                     }
 
                     //Formatta le colonne del Worksheet e le giustifica:
                     try {
                         waitReady(m_objExcel);
                         Range X = (Range)Myworksheet.Range[
                             Myworksheet.Cells[1, ColonnaExcel + disp],
                             Myworksheet.Cells[RowCount + Step, ColonnaExcel + disp]];
                         waitReady(m_objExcel);
                         X.Value2 = arr;
                         waitReady(m_objExcel);
                         X.EntireColumn.AutoFit(); //Giustifica la colonna
                     }
                     catch (Exception E) {
                         m_objExcel.Quit();
                         releaseObject(m_objExcel);
                         return E.Message;
                     }
                     //X.EntireColumn.Style =  				
                 }
 
                 //m_objExcel.Visible = true;
                 try {
                     if ((groupby != null) && (totals != null)) {
                         for (int i = 0; i < groupby.Length; i++) {
                             waitReady(m_objExcel);
                             Myworksheet.Cells.Subtotal(groupby[i],
                                 XlConsolidationFunction.xlSum,
                                 totals,
                                 false,
                                 false,
                                 XlSummaryRow.xlSummaryBelow
                                 );
                         }
                     }
                 }
                 catch (Exception E) {
                     QueryCreator.ShowException(E);
                 }
                 waitReady(m_objExcel);
                 Myworksheet.SaveAs(filename, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                     Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                 m_objExcel.DisplayAlerts = false;
                 m_objExcel.Quit();
                 releaseObject(m_objExcel);
             }
             catch (Exception E) {
                 m_objExcel.Quit();
                 releaseObject(m_objExcel);
                 QueryCreator.ShowException(E);
                 return E.Message;
             }
             return null;
                 */

             #endregion
         }

         delegate object ColumnParser(object value);

         /// <summary>
         /// Import an xlsx file
         /// </summary>
         /// <param name="DT"></param>
         /// <param name="fileName"></param>
         /// <param name="headerRows"></param>
         /// <returns></returns>
         public static bool importXlsx(System.Data.DataTable DT, string fileName, int headerRows = 0) {
             var package = new ExcelPackage(new FileInfo(fileName));

             string decSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyDecimalSeparator;
             string GroupsSeparator = CultureInfo.CurrentUICulture.NumberFormat.CurrencyGroupSeparator;
             string numSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
             string numGroupsSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberGroupSeparator;

            var MyNFI = new NumberFormatInfo {
                NegativeSign = "-",
                CurrencyDecimalSeparator = decSeparator,
                CurrencyGroupSeparator = GroupsSeparator,
                NumberDecimalSeparator = numSeparator,
                NumberGroupSeparator = numGroupsSeparator
            };

            var workSheet = package.Workbook.Worksheets._First();
             var totalRows = workSheet.Dimension.End.Row;
             var totalColumns = workSheet.Dimension.End.Column;
             for (int i = headerRows + 1; i <= totalRows; i++) {
                 var r = DT.NewRow();
                 for (int j = 1; j <= DT.Columns.Count; j++) {
                     try {
                         var c = DT.Columns[j - 1];
                         object val = workSheet.Cells[i, j].Value;
                         if (val == null) {
                             r[c] = DBNull.Value;
                             continue;
                         }

                         if (c.ExtendedProperties["converter"] != null) {
                             r[c] = ((ColumnParser) c.ExtendedProperties["converter"])(val);
                             continue;
                         }

                         if (c.DataType == typeof(decimal) & val.GetType() == typeof(string)) {
                             var str = (string) val;
                             if ((str.IndexOf(GroupsSeparator) >= str.Length - 3) &&
                                 (str.IndexOf(decSeparator) == -1)) {
                                 str = str.Replace(GroupsSeparator, decSeparator);
                             }

                             r[c] = decimal.Parse(str, NumberStyles.Currency, MyNFI);
                             continue;
                         }

                         if (c.DataType == typeof(Double) & val.GetType() == typeof(string)) {
                             var str = (string) val;
                             if ((str.IndexOf(numGroupsSeparator) >= str.Length - 3) &&
                                 (str.IndexOf(numSeparator) == -1)) {
                                 str = str.Replace(numGroupsSeparator, numSeparator);
                             }

                             r[c] = double.Parse(str, NumberStyles.Number, MyNFI);
                             continue;
                         }

                         if (c.DataType == typeof(int)) {
                             r[c] = Convert.ToInt32(val);
                             continue;
                         }

                         if (c.DataType == typeof(decimal)) {
                             r[c] = Convert.ToDecimal(val);
                             continue;
                         }

                         if (c.DataType == typeof(double)) {
                             r[c] = Convert.ToDouble(val);
                             continue;
                         }

                         if (c.DataType == typeof(DateTime)) {
                             r[c] = Convert.ToDateTime(val);
                             continue;
                         }

                         r[c] = val;
                     }
                     catch(Exception) {
                         return false;

                     }


                 }


                 DT.Rows.Add(r);
             }


             return true;
         }

         /// <summary>
         /// Utilizzata per elaborare il file XLSX se non presente il nome del file di output
         /// </summary>
         /// <param name="DT">DataTable di imput</param>
         /// <param name="Header">Flag per la visualizzazione dell'header</param>
         /// <param name="groupby">group by</param>
         /// <param name="totals">colonne su cui effettuare i totali</param>
         private static void iDataTableToOfficeXML(System.Data.DataTable DT, bool Header, int[] groupby,
             object[] totals) {

             // Controlli formali sulla presenza dei dati in input
             if (DT == null) return;
             if (DT.Select().Length == 0) return;
             if (DT.Columns.Count == 0) return;

             string fileName = Path.GetTempFileName() + ".xlsx";
             string retCode = iDataTableToOfficeXML(DT, Header, groupby, totals, fileName);

             if (retCode != "") return;
             Process.Start(fileName);

             //Microsoft.Office.Interop.Excel.Application m_objExcel;

             //try {
             //    m_objExcel = new Microsoft.Office.Interop.Excel.Application();

             //    m_objExcel.Workbooks.Open(fileName);
             //    m_objExcel.Visible = true;
             //    m_objExcel.Interactive = true;

             //}
             //catch (Exception E) {
             //    QueryCreator.ShowException(E);
             //    return;
             //}
         }

         private static string iDataTableToOfficeXML(System.Data.DataTable DT, bool Header, int[] groupby,
             object[] totals, string filename) {

             // Controlli formali sulla presenza dei dati in input
             if (DT == null) return LM.emptyExport;
             if (DT.Columns.Count == 0) return LM.emptyExport;

             ExcelPackage xlPackage = new ExcelPackage();

             if (filename.Length != 0) {
                 var fi = new FileInfo(filename);
                 xlPackage = new ExcelPackage(fi);
             }

             using (xlPackage) {


                 try {

                     #region Create xls worksheet and header

                     ExcelWorksheet
                         worksheet = CreateSheet(xlPackage,
                             DT.TableName); // Creazione del foglio dati con impostazione delle sue proprietà
                     int RowCount = DT.Select().Length; // Numero Righe del datatable
                     int ColumnCount = DT.Columns.Count; // Numero Colonne del datatable
                     int Step = 0;

                     // Formattazione dell'Header del foglio Excel
                     if (Header) {
                         SetHeader(worksheet, DT);
                         Step = 1;
                     }

                     #endregion

                     int mincol = 99999;


                     for (int Colonna = 0; Colonna < ColumnCount; Colonna++) {
                         DataColumn Col = DT.Columns[Colonna];
                         int ColonnaExcel = Colonna + 1;
                         if (Col.ExtendedProperties["ListColPos"] != null)
                             ColonnaExcel = Convert.ToInt32(Col.ExtendedProperties["ListColPos"]);
                         if (ColonnaExcel == -1) continue;
                         if (mincol > ColonnaExcel) mincol = ColonnaExcel;
                     }

                     int disp = 0;
                     if (mincol == 0) disp = 1;
                     DataRow[] Rows = DT.Select(null, (string) DT.ExtendedProperties["ExcelSort"]);
                     if (Rows.Length == 0) return LM.emptyExport;

                     //per ogni colonna del datatable:
                     int Excel_Col_Index = 0;

                     Dictionary<int, int> colLookup = new Dictionary<int, int>();
                     Dictionary<int, string> tagLookup = new Dictionary<int, string>();
                     Dictionary<int, int> inverseLookup = new Dictionary<int, int>();
                     Dictionary<int, string> colFormat = new Dictionary<int, string>();

                     //Inserisce le eventuali intestazioni e stabilisce la corrispondenza delle colonne
                     for (int Colonna = 0; Colonna < ColumnCount; Colonna++) {
                         DataColumn Col = DT.Columns[Colonna];
                         string caption = (string) Col.ExtendedProperties["ExcelTitle"];
                         if (caption == null) caption = DT.Columns[Colonna].Caption;
                         if (caption == "") continue;

                         if ((Col.ExtendedProperties["ListColPos"] == null) && (caption.StartsWith("."))) continue;

                         int ColonnaExcel = Excel_Col_Index + 1;
                         if (Col.ExtendedProperties["ListColPos"] != null)
                             ColonnaExcel = Convert.ToInt32(Col.ExtendedProperties["ListColPos"]);

                         if (ColonnaExcel == -1) continue;

                         if (caption.StartsWith(".")) caption = caption.Remove(0, 1);
                         Excel_Col_Index++;

                         string Tag = "x.y";
                         Tag = CompleteTag(Tag, Col);
                         colLookup[Colonna] = ColonnaExcel + disp;
                         tagLookup[Colonna] = Tag;

                         if (Col.ExtendedProperties["ExcelFormat"] != null) {
                             colFormat[Colonna] = Col.ExtendedProperties["ExcelFormat"].ToString();
                         }
                         else {
                             switch (GetFieldLower(Tag, 2)) {
                                 case "n":
                                     colFormat[Colonna] = "0.00";
                                     break;

                                 case "c":
                                     colFormat[Colonna] = "€#,##0.00_);[Red](€#,##0.00)";
                                     break;

                                 case "d":
                                     colFormat[Colonna] = "dd/mm/yyyy";
                                     break;
                             }
                         }

                         if (Header) {
                             var cell = worksheet.Cells[1, ColonnaExcel + disp];
                             cell.Value = caption;
                         }

                     }

                     for (int i = 0; i < ColumnCount; i++) {
                         if (colLookup.ContainsKey(i)) {
                             inverseLookup[colLookup[i]] = i;
                         }
                     }

                     int maxLevel = groupby == null ? 0 : groupby.Length;

                     //Posizione prima riga dati
                     var startRow = Header ? 2 : 1;

                     int[] startLevel = new int[maxLevel];
                     for (int i = 0; i < maxLevel; i++) {
                         startLevel[i] = startRow;
                     }

                     int rigaExcel =
                         startRow; //Riga Excel in cui andrò la prossima riga di dati o totali                    
                     for (int Riga = 0; Riga < RowCount; Riga++) {
                         DataRow Curr = Rows[Riga];

                         if (maxLevel > 0) {
                             if (Riga > 0)
                                 rigaExcel = EmitTotals(Rows[Riga - 1], Curr, groupby, totals, inverseLookup, rigaExcel,
                                     startLevel, worksheet);
                             worksheet.Row(rigaExcel).OutlineLevel = maxLevel + 1;
                         }

                         //Mette i dati nelle opportune colonne
                         foreach (int Colonna in colLookup.Keys) {
                             var cell = worksheet.Cells[rigaExcel, colLookup[Colonna]];
                             if (Rows[Riga][Colonna] == DBNull.Value) {
                                 cell.Value = "";
                                 continue;
                             }

                             string Tag = tagLookup[Colonna];
                             if (DT.Columns[Colonna].DataType == typeof(String)) {
                                 cell.Value = HelpUi.StringValue(Rows[Riga][Colonna], Tag); //"'" + 
                                 continue;
                             }

                             if (DT.Columns[Colonna].DataType == typeof(Int32)) {
                                 cell.Value = Rows[Riga][Colonna];
                                 continue;
                             }

                             if (DT.Columns[Colonna].DataType == typeof(Int16)) {
                                 cell.Value = Rows[Riga][Colonna];
                                 continue;
                             }

                             switch (GetFieldLower(Tag, 2)) {
                                 case "n":
                                 case "c":
                                     cell.Value = Rows[Riga][Colonna] == DBNull.Value
                                         ? 0
                                         : Convert.ToDecimal(Rows[Riga][Colonna]);
                                     break;
                                 case "d":
                                     DateTime value;
                                     if (DateTime.TryParse(Rows[Riga][Colonna].ToString(), out value)) {
                                         cell.Value = value;
                                     }

                                     break;

                                 default:
                                     cell.Value = HelpUi.StringValue(Rows[Riga][Colonna], Tag);
                                     break;
                             }
                         }

                         rigaExcel++;



                     }

                     if (maxLevel > 0) {
                         //Mette il totale generale
                         int lastDataRow = rigaExcel - 1;
                         rigaExcel = EmitTotals(Rows[RowCount - 1], null, groupby, totals, inverseLookup, rigaExcel,
                             startLevel, worksheet);

                         worksheet.Row(rigaExcel).OutlineLevel = 0;
                         for (int col = 1; col <= totals.Length; col++) {
                             //Alla colonna totals[col] metti il subtotale della stessa colonna da startLevel[currLevel] a baseRow-1
                             int colonna = (int) totals[col - 1];
                             //ws.Cells[currRow, colonna].Formula = "=SUM(" + ws.Cells[startLevel[currLevel], colonna] + ":" + ws.Cells[baseRow - 1, colonna] + ")";
                             worksheet.Cells[rigaExcel, colonna].Formula =
                                 "=SUBTOTAL(9," + worksheet.Cells[startRow, colonna] +
                                 ":" + worksheet.Cells[lastDataRow, colonna] + ")";
                         }

                         var cellTotaleGen = worksheet.Cells[rigaExcel, groupby[0]];
                         cellTotaleGen.Value = LM.overallTotal;
                         cellTotaleGen.Style.Font.Bold = true;
                     }

                     //Per ogni colonna:  imposta il formato e la dimensiona
                     for (int Colonna = 0; Colonna < ColumnCount; Colonna++) {
                         if (!colLookup.ContainsKey(Colonna)) continue;
                         int ColonnaExcel = colLookup[Colonna];
                         ExcelRange ExcCol = worksheet.Cells[1 + Step, ColonnaExcel, rigaExcel, ColonnaExcel];
                         if (colFormat.ContainsKey(Colonna)) {
                             ExcCol.Style.Numberformat.Format = colFormat[Colonna];
                         }

                         ExcCol.AutoFitColumns();
                     }



                     #endregion

                     // Save file id filename is set
                     if (filename.Length != 0) {

                         byte[] data = xlPackage.GetAsByteArray();
                         if (File.Exists(filename)) File.Delete(filename); // Se esiste il file lo cancello
                         File.WriteAllBytes(filename, data);
                     }

                 }
                 catch (Exception e) {
                     //QueryCreator.ShowException(E);
                     return e.Message;
                 }

             }

             return null;
         }

         /// <summary>
         /// Create an excel work sheet (EPPlus Object) and set font type, font dimension and sheet name
         /// </summary>
         /// <param name="p">Excel Package</param>
         /// <param name="sheetName">Sheet Name</param>
         /// <returns></returns>
         private static ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName) {
             
                 if (sheetName == "") sheetName = LM.translate("dataFolder", false);
                 p.Workbook.Worksheets.Add(sheetName);
                 ExcelWorksheet ws = p.Workbook.Worksheets[1];
                 ws.Name = sheetName; //Setting Sheet's name
                 ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                 ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                 return ws;

             
         }

         private static void SetHeader(ExcelWorksheet ws, System.Data.DataTable dt) {

             // Formattazione di Header 
             for (int i = 1; i <= dt.Columns.Count; i++) {

                 var cell = ws.Cells[1, i];
                 cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                 cell.Style.Font.Bold = true;
             }

         }

         /// <summary>
         /// Restituisce il selettore della riga in base alle colonne di groupby
         /// </summary>
         /// <param name="r"></param>
         /// <param name="groupBy"></param>
         /// <param name="colLookup">Lookup per ottnere il numero effettivo di colonna (non quello logico) </param>
         /// <param name="level"></param>
         /// <returns></returns>
         static string GetSelector(DataRow r, int[] groupBy, Dictionary<int, int> colLookup, int level) {
             string res = "";
             for (int i = 0; i <= level; i++) {
                 // riga precedente by Nino
                 // res += "§" + ws.Cells[row,groupBy[i]];
                 // Ma forse andrebbe preso il valore del campo
                 res += "§" + r[colLookup[groupBy[i]]];
             }

             return res;
         }

         static string GetGroupByTotal(DataRow R, int[] groupBy, Dictionary<int, int> colLookup, int level) {
             return R[colLookup[groupBy[level]]] + " " + LM.translate("total", false);
         }


         static int EmitTotals(DataRow prevDataRow, DataRow currRow, int[] groupby, object[] totals,
             Dictionary<int, int> lookup, int rigaExcel,
             int[] startLevel, ExcelWorksheet ws) {
             //Si occupa di inserire le righe di raggruppamento
             List<int> daAggiornare = new List<int>();
             int maxLevel = groupby.Length;
             int firstDifferentLevel = currRow == null ? 0 : -1;
             for (int i = 0; i < groupby.Length; i++) {
                 if (currRow == null || currRow[lookup[groupby[i]]].Equals(prevDataRow[lookup[groupby[i]]])) continue;
                 firstDifferentLevel = i;
                 break;
             }

             if (firstDifferentLevel == -1) return rigaExcel;
             int baseRow = rigaExcel;

             for (int currLevel = maxLevel - 1; currLevel >= firstDifferentLevel; currLevel--) {
                 //Se è diverso emetto il totale su quel livello e continuo il controllo sui livelli precedenti
                 ws.Row(rigaExcel).OutlineLevel = currLevel + 1;
                 //Vanno messi i totalizzatori vari - ALLA RIGA currrow
                 for (int col = 1; col <= totals.Length; col++) {
                     //Alla colonna totals[col] metti il subtotale della stessa colonna da startLevel[currLevel] a baseRow-1
                     int colonna = (int) totals[col - 1];
                     //ws.Cells[currRow, colonna].Formula = "=SUM(" + ws.Cells[startLevel[currLevel], colonna] + ":" + ws.Cells[baseRow - 1, colonna] + ")";
                     ws.Cells[rigaExcel, colonna].Formula =
                         "SUBTOTAL(9," + ws.Cells[startLevel[currLevel], colonna] +
                         ":" + ws.Cells[baseRow - 1, colonna] + ")";

                 }

                 //Nella stessa riga la prima colonna va valorizzata con "totale " + i valori della g.by
                 // di posizioni groupBy[0.. currLevel] 
                 var cellTotale = ws.Cells[rigaExcel, groupby[currLevel]];
                 cellTotale.Value = GetGroupByTotal(prevDataRow, groupby, lookup, currLevel);
                 cellTotale.Style.Font.Bold = true;
                 rigaExcel++;
                 daAggiornare.Add(currLevel);
                 foreach (int toUpdate in daAggiornare) {
                     startLevel[toUpdate] = rigaExcel;
                 }
             }


             return rigaExcel;
         }



         /// <summary>
         /// Esegue il Fill del ListView passato, leggendo i dati dal datatable avente tre colonne
         /// Descrizione
         /// Importo
         /// Formato
         /// </summary>
         /// <param name="head">ListView to fill with Headings</param>
         /// <param name="body">ListView to fill with Body</param>
         /// <param name="dt">DataTable da visualizzare come listview</param>
         /// <returns></returns>
         public static void SituazioneToListView(ListView head,
             ListView body,
             System.Data.DataTable dt) {
             head.BeginUpdate();
             body.BeginUpdate();

             head.Clear();
             body.Items.Clear();

             body.View = View.Details; //necessario per la visualizzazione del GridLines
             body.GridLines = true;
             head.View = View.Details;
             head.GridLines = true;
             head.Scrollable = false;

             var F = body.Font;
             var FB = new System.Drawing.Font(F.FontFamily.Name, F.Size, FontStyle.Bold);

             head.Columns.Add("void", 0, System.Windows.Forms.HorizontalAlignment.Left);
             head.Columns.Add(LM.translate("title", false), head.Size.Width - 4, System.Windows.Forms.HorizontalAlignment.Center);
//			Body.Columns.Add("Descrizione",L,HorizontalAlignment.Left);
//			Body.Columns.Add("Importo",R,HorizontalAlignment.Right);

             bool inHeader = true;
             foreach (DataRow dr in dt.Rows) {
                 string myFormat = Kind(dr).ToString().Trim();
                 switch (myFormat) {
                     case "H": //Riga di intestazione
                         string t = NN(Value(dr).ToString()); //DR["Descrizione"].ToString());
                         if (inHeader) {
                             var myItemH = new ListViewItem("");
                             myItemH.SubItems.Add(new ListViewItem.ListViewSubItem(myItemH, t));
                             head.Items.Add(myItemH);
                         }
                         else {
                            var myItemH = new ListViewItem("") {
                                Font = FB
                            };
                            body.Items.Add(myItemH);
                         }

                         break;
                     case "": //Normale
                         inHeader = false;
                         var myItemN = new ListViewItem(NN(Value(dr).ToString()));

                         if (Amount(dr) != DBNull.Value) {
                             decimal d = 0;
                             d = Convert.ToDecimal(Amount(dr));
                             myItemN.SubItems.Add(d.ToString("c"));
                         }
                         else {
                             myItemN.SubItems.Add("");
                         }

                         body.Items.Add(myItemN);
                         break;
                     case "N": //Riga Vuota
                         if (!inHeader) {
                             var myItemNn = new ListViewItem(NN(Value(dr).ToString()));
                             var fn = new System.Drawing.Font(F.FontFamily.Name, F.Size, FontStyle.Bold);
                             myItemNn.Font = fn;
                             myItemNn.EnsureVisible();
                             if (Value(dr).ToString() != "") myItemNn.BackColor = Color.LightYellow;
                             body.Items.Add(myItemNn);
                         }

                         inHeader = false;
                         break;
                     case "S": //Riga in grassetto
                         inHeader = false;
                        var myItemS = new ListViewItem(NN(Value(dr).ToString())) {
                            Font = FB
                        };
                        myItemS.EnsureVisible();
                         myItemS.BackColor = Color.LightYellow;
                         ListViewItem.ListViewSubItem MysubItem;
                         if (Amount(dr) == DBNull.Value) {
                             MysubItem = new ListViewItem.ListViewSubItem(myItemS, "");
                         }
                         else {
                             var d2 = Convert.ToDecimal(Amount(dr));
                             MysubItem = new ListViewItem.ListViewSubItem(myItemS, d2.ToString("c"));
                         }

                         MysubItem.Font = myItemS.Font;
                         myItemS.SubItems.Add(MysubItem);
                         body.Items.Add(myItemS);
                         break;

                 } //Fine switch

             } //Fine foreach

             head.EndUpdate();
             head.Refresh();
             head.PerformLayout();
             body.EndUpdate();
             body.Refresh();
             body.PerformLayout();
             return;
         } //Fine SituazioneToListView


         /// <summary>
         /// Esporta una situazione come da sp sit_ in un file xlsx
         /// </summary>
         /// <param name="dt"></param>
         /// <param name="header">non usato</param>
         public static void SituazioneToExcel(System.Data.DataTable dt, bool header) {            
                ISituazioneToExcel(dt);
         }

        /// <summary>
        /// Esporta tutti i dati di una situazione in un foglio Excel
        /// </summary>
        /// <param name="dt"></param>
        private static void ISituazioneToExcel(System.Data.DataTable dt) {

            if (dt.Select().Length == 0) return;
            string filename = Path.GetTempFileName() + ".xlsx";
            //string retCode = iDataTableToOfficeXML(DT, Header, groupby, totals, filename);
            string retCode = iSituazioneToOfficeXML(dt, filename);

            if (retCode != null) return;
            Process.Start(filename);
        }

        private static string iSituazioneToOfficeXML(System.Data.DataTable dt, string filename){ 
            if (dt.Rows.Count == 0) return null;

            ExcelPackage xlPackage = new ExcelPackage();
            ExcelWorkbook ExWB = xlPackage.Workbook;

            using (xlPackage) {
                try {
                    ExcelWorksheet ExWS = ExWB.Worksheets.Add(dt.TableName);

                    int totrow = dt.Rows.Count;
                    int nrow = 1;
                    int lasthead = 0;
                    //Range R;
                    ExcelRange R;

                    R = ExWS.Cells[1, 1, totrow, 1];
                    R._forEach(cell => cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left);

                    R = ExWS.Cells[1, 2, totrow, 2];
                    R._forEach(cell => cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right);


                    bool inHeader = true;
                    foreach (DataRow DR in dt.Rows) {
                        string MyFormat = Kind(DR).ToString().Trim();
                        switch (MyFormat) {
                            case "H": //Riga di intestazione
                                string t = NN(Value(DR).ToString());
                                ExcelRange X = ExWS.Cells[nrow, 1, nrow, 2];
                                X.Merge = true;
                                X.Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                                X.Style.Font.Bold = true;
                                ExWS.Cells[nrow, 1].Value = t;
                                lasthead = nrow;
                                break;
                            case "": //Normale
                                inHeader = false;
                                string t1 = NN(Value(DR).ToString());
                                ExWS.Cells[nrow, 1].Value = t1;
                                Decimal d = 0;
                                if (Amount(DR) != DBNull.Value) d = Convert.ToDecimal(Amount(DR));
                                ExWS.Cells[nrow, 2].Value = d.ToString("c");
                                break;
                            case "N": //Riga Vuota
                                if (!inHeader) {
                                    string t22 = NN(Value(DR).ToString());
                                    ExWS.Cells[nrow, 1].Value = t22;
                                    //Decimal d2=0;
                                    //if (DR["Importo"]!=DBNull.Value) d2 = Convert.ToDecimal(DR["Importo"]);
                                    //ExWS.Cells[nrow,2]=d2.ToString("c");
                                    ExcelRange X2 = ExWS.Cells[nrow, 1, nrow, 1];
                                    X2.Style.Font.Bold = true;
                                    //X2.Interior.ColorIndex = 19;
                                    //X2.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                }

                                inHeader = false;

                                break;
                            case "S": //Riga in grassetto
                                inHeader = false;
                                string t2 = NN(Value(DR).ToString());
                                ExWS.Cells[nrow, 1].Value = t2;
                                Decimal d2 = 0;
                                if (Amount(DR) != DBNull.Value) d2 = Convert.ToDecimal(Amount(DR));
                                ExWS.Cells[nrow, 2].Value = d2.ToString("c");
                                ExcelRange X3 = ExWS.Cells[nrow, 1, nrow, 2];
                                X3.Style.Font.Bold = true;
                                //X2.Interior.ColorIndex = 19;
                                //X3.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                                //X2.Font.Background = XlBackground.xlBackgroundOpaque;
                                break;

                        } //Fine switch

                        nrow++;
                    } //Fine foreach

                    R = ExWS.Cells[lasthead, 1, totrow, 1];
                    R.AutoFitColumns();
                    R = ExWS.Cells[lasthead, 2, totrow, 2];
                    R.AutoFitColumns();

                    // Save file id filename is set
                    if (filename.Length != 0) {

                        byte[] data = xlPackage.GetAsByteArray();
                        if (File.Exists(filename)) File.Delete(filename); // Se esiste il file lo cancello
                        File.WriteAllBytes(filename, data);
                    }
                }
                catch (Exception e) {
                    return e.Message;
                }
            }

            return null;
        }

         /// <summary>
         /// Esporta tutti i dati del Datatable in un "File a lunghezza fissa"
         /// </summary>
         /// <param name="dt"></param>
         /// <param name="header"></param>
         /// <param name="fileName"></param>
         public static void dataTableToFixedLengthFile(System.Data.DataTable dt, bool header, string fileName) {
             StreamWriter sw = new StreamWriter(fileName, false, Encoding.Default);

             int[] lunghezza = new int[dt.Columns.Count];
             if (header) {
                 for (int i = 0; i < dt.Columns.Count; i++) {
                     lunghezza[i] = dt.Columns[i].ColumnName.Length;
                 }
             }

             foreach (DataRow r in dt.Rows) {
                 for (int i = 0; i < dt.Columns.Count; i++) {
                     int length = r[i].ToString().Length;
                     if (length > lunghezza[i]) lunghezza[i] = length;
                 }
             }

             if (header) {
                 for (int i = 0; i < dt.Columns.Count - 1; i++) {
                     sw.Write(dt.Columns[i].ColumnName + ' ');
                 }

                 sw.WriteLine(dt.Columns[dt.Columns.Count - 1].ColumnName);
             }

             foreach (DataRow r in dt.Rows) {
                 for (int i = 0; i < dt.Columns.Count; i++) {
                     var align = HelpUi.GetAlignForColumn(dt.Columns[i]);
                     string campo = (align == mdl_utils.HorizontalAlignment.Right)
                         ? r[i].ToString().PadLeft(lunghezza[i], ' ')
                         : r[i].ToString().PadRight(lunghezza[i], ' ');
                     if (i < dt.Columns.Count - 1) {
                         sw.Write(campo + ' ');
                     }
                     else {
                         sw.WriteLine(campo);
                     }
                 }
             }

             sw.Close();
         }

         /// <summary>
         /// Esporta tutti i dati del Datatable in un file di testo con i valori delle colonne separati da un carattere specificato.
         /// </summary>
         /// <param name="dt">DataTable da importare</param>
         /// <param name="header">true se si vuole importare anche l'intestazione delle colonne</param>
         /// <param name="separator">carattere separatore delle colonne</param>
         /// <param name="fileName"></param>
         private static void dataTableToSeparatedValues(System.Data.DataTable dt, bool header, char separator,
             string fileName) {
             StreamWriter sw = new StreamWriter(fileName, false, Encoding.Default);

             if (header) {
                 for (int i = 0; i < dt.Columns.Count - 1; i++) {
                     sw.Write(dt.Columns[i].ColumnName + separator);
                 }

                 sw.WriteLine(dt.Columns[dt.Columns.Count - 1].ColumnName);
             }

             foreach (DataRow r in dt.Rows) {
                 for (int i = 0; i < dt.Columns.Count - 1; i++) {
                     sw.Write(r[i].ToString() + separator);
                 }

                 sw.WriteLine(r[dt.Columns.Count - 1]);
             }

             sw.Close();
         }

         /// <summary>
         /// Esporta tutti i dati del DataTable in un "File con campi separati da tabulatori"
         /// </summary>
         /// <param name="dt">Datatable da importare</param>
         /// <param name="header">true se si vuole importare anche l'intestazione delle colonne</param>
         /// <param name="filename"></param>
         public static void
             dataTableToTabulationSeparatedValues(System.Data.DataTable dt, bool header, string filename) {
             dataTableToSeparatedValues(dt, header, '\t', filename);
         }

         /// <summary>
         /// Esporta tutti i dati del Datatable in un "File con campi separati da punto e virgola"
         /// </summary>
         /// <param name="dt">DataTable da importare</param>
         /// <param name="header">true se si vuole importare anche l'intestazione delle colonne</param>
         ///    /// <param name="filename"></param>
         public static void dataTableToCommaSeparatedValues(System.Data.DataTable dt, bool header, string filename) {
             dataTableToSeparatedValues(dt, header, ';', filename);
         }


         /// <summary>
         /// Restituisce una stringa vuota se è NULL la stringa passata
         /// Altrimenti restituisce la stringa originale
         /// </summary>
         /// <param name="MyString"></param>
         /// <returns></returns>
         private static string NN(string MyString) {
             if (MyString.ToString() == "NULL") return "";
             return MyString;
         }

         /// <summary>
         /// Create a string that represent a datatable in CSV  format
         /// </summary>
         /// <param name="DT"></param>
         /// <param name="Header"></param>
         /// <returns></returns>
         public static string DataTableToCSV(System.Data.DataTable DT, bool Header) {
             if (DT.Select().Length == 0) return "";
             string separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

             int RowCount = DT.Select().Length; //Numero Righe del datatable
             int ColumnCount = DT.Columns.Count; //Numero Colonne del datatable
             DataRow[] Rows = DT.Select(null,
                 (string) DT.ExtendedProperties["ExcelSort"]);
             //per ogni colonna del datatable:
             int[] cols = new int[ColumnCount + 1];
             string[] captions = new string[ColumnCount + 1];
             int colfound = 0;
             int unmarkedcols = 0;
             for (int Colonna = 0; Colonna < ColumnCount; Colonna++) {
                 DataColumn Col = DT.Columns[Colonna];
                 string caption = (string) Col.ExtendedProperties["ExcelTitle"];
                 if (caption == null) caption = DT.Columns[Colonna].Caption;
                 if (caption == "") continue;
                 if ((Col.ExtendedProperties["ListColPos"] == null) &&
                     (caption.StartsWith("."))) continue;

                 int ColonnaExcel;
                 if (Col.ExtendedProperties["ListColPos"] != null) {
                     ColonnaExcel = Convert.ToInt32(Col.ExtendedProperties["ListColPos"]);
                 }
                 else {
                     ColonnaExcel = unmarkedcols;
                     unmarkedcols++;
                 }

                 if (ColonnaExcel < 0) continue;

                 if (caption.StartsWith(".")) caption = caption.Remove(0, 1);
                 cols[ColonnaExcel] = Colonna;
                 captions[ColonnaExcel] = caption;
                 colfound++;
             }







             string crlf = "\r\n";
             StringBuilder SB = new StringBuilder();
             if (Header) {
                 bool first = true;
                 for (int i = 0; i <= ColumnCount; i++) {
                     if (captions[i] == null) continue;
                     if (!first) SB.Append(separator);
                     first = false;
                     SB.Append(captions[i]);
                 }

                 SB.Append(crlf);
             }

             for (int Riga = 0; Riga < RowCount; Riga++) {
                 string S = "";
                 bool firstcol = true;
                 for (int i = 0; i <= ColumnCount; i++) {
                     if (captions[i] == null) continue;
                     if (!firstcol) S += separator;
                     firstcol = false;

                     int Colonna = cols[i];
                     string Tag = "x.y";
                     DataColumn Col = DT.Columns[Colonna];
                     Tag = HelpUi.CompleteTag(Tag, Col);
                     if (Rows[Riga][Colonna] == DBNull.Value) {
                         continue;
                     }

                     if (Tag == "x.y.c") Tag = "x.y.n";
                     string val = HelpUi.StringValue(Rows[Riga][Colonna], Tag);
                     if (val.IndexOf("\"") >= 0 || val.IndexOf(separator) >= 0 || val.IndexOf("\r") >= 0 ||
                         val.IndexOf("\n") >= 0) {
                         val = "\"" + val.Replace("\"", "\"\"") + "\"";
                     }

                     S += val;

                 }

                 SB.Append(S);
                 SB.Append(crlf);
             }

             return SB.ToString();
         }

     }
 }

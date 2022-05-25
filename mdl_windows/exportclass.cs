 using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
//using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
//using System.Reflection;
using System.Threading;


using System.Globalization;
using System.Linq;
// Utilizzato per EPPlus
using mdl_utils;
using mdl;
using LM=mdl_language.LanguageManager;
using OfficeOpenXml;
using OfficeOpenXml.Style;
#pragma warning disable IDE1006 // Naming Styles



namespace mdl_windows {


     /// <summary>
     /// Classe per l'esportazione di Dati.
     /// Reference: Microsoft Excel 9.0 Object Library
     /// Metodo da utilizzare per l'esportazione in Excel:
     /// public static void DataTableToExcel(System.Data.DataTable DT,bool Header)
     /// Codice di chiamata: exportdata.exportclass.DataTableToExcel(MyDS.Tables[0],true);
     /// Max 08 ottobre 2002
     /// </summary>
     public class exportclass {




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
         /// Esporta una situazione come da sp sit_ in un file xlsx
         /// </summary>
         /// <param name="dt"></param>
         /// <param name="header">non usato</param>
         public static void SituazioneToExcel(System.Data.DataTable dt, bool header) {
            
             ISituazioneToExcel(dt);
            
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

      


        private static string iSituazioneToOfficeXML(System.Data.DataTable dt, string filename) {
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
                    foreach (var cell in R){
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }
                    

                    R = ExWS.Cells[1, 2, totrow, 2];

                    foreach (var cell in R) {
                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    }
                    


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
                     HorizontalAlignment align = HelpUi.GetAlignForColumn(dt.Columns[i]);
                     string campo = (align == HorizontalAlignment.Right)
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

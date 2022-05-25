using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using mdl;
using DialogResult = System.Windows.Forms.DialogResult;
using LM = mdl_language.LanguageManager;
using mdl_utils;
using q = mdl.MetaExpression;


namespace mdl_winform {
    public class utils {

       

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
        /// Gets all custom objects and columntypes info from db
        /// </summary>
        public virtual void GenerateCustomObjects(DataAccess conn) {
            FrmMeter F = new FrmMeter {
                Text = "Analisi struttura tabelle"
            };
            var QHS = conn.GetQueryHelper();
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(F,null);
            F.Show();
            conn.Descriptor.Reset();
            ArrayList Tables = dbanalyzer.TableListFromDB(conn).GetAwaiter().GetResult();
            F.pBar.Maximum = Tables.Count;
            foreach (string tablename in Tables) {
                F.pBar.Increment(1);
                if (conn.Count("customobject",filter: q.eq("objectname", tablename)).GetAwaiter().GetResult() > 0) {
                    continue;
                }
                //Application.DoEvents();
                dbstructure DBS = conn.Descriptor.GetStructure(tablename,conn).GetAwaiter().GetResult();
                if (DBS.customobject.Rows.Count == 0) {
                    DataRow newobj = DBS.customobject.NewRow();
                    newobj["objectname"] = tablename;
                    newobj["isreal"] = "S";
                    DBS.customobject.Rows.Add(newobj);
                }
                dbanalyzer.ReadColumnTypes(DBS.columntypes, tablename, conn).GetAwaiter().GetResult();
            }
            ArrayList Views = dbanalyzer.ViewListFromDB(conn).GetAwaiter().GetResult();
            F.Text = "Analisi struttura viste";
            F.pBar.Value = 0;
            F.pBar.Maximum = Views.Count;
            foreach (string tablename in Views) {
                F.pBar.Increment(1);
                if (conn.Count("customobject", q.eq("objectname", tablename)).GetAwaiter().GetResult() > 0){
                    continue;
                }

                //Application.DoEvents();
                dbstructure DBS = (dbstructure)conn.Descriptor.GetStructure(tablename,conn).GetAwaiter().GetResult();
                if (DBS.customobject.Rows.Count == 0) {
                    DataRow newobj = DBS.customobject.NewRow();
                    newobj["objectname"] = tablename;
                    newobj["isreal"] = "N";
                    DBS.customobject.Rows.Add(newobj);
                }
                dbanalyzer.ReadColumnTypes(DBS.columntypes, tablename, conn).GetAwaiter().GetResult();
            }
            F.Close();
        }

          /// <summary>
        /// Gets the MetaData linked to a form. Generally called on MetaData_AfterLink 
        ///		method.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static MetaData GetMetaData( System.Windows.Forms.Form f) {
	        if (f?.Tag == null) return null;
            try {
	            var h = (Hashtable) f.Tag;
                var metadata = (MetaData) h?["MetaData"];
                return metadata;
            }
            catch {
                return null;
            }
        }

        /// <summary>
        ///  Contructor callable from MetaData-Linked forms to create new MetaData's 
        ///   through the dispatcher of their linked MetaData
        /// </summary>
        /// <param name="f">Calling form</param>
        /// <param name="metaDataName">Name of entity to create</param>
        public static MetaData GetMetaData( System.Windows.Forms.Form f, string metaDataName) {
            var parent = GetMetaData(f);
            if (parent == null) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(f,
                    $"Form {f.Name} can\'t call MetaData(Form F, string PrimaryTable) because  has no MetaData linked.");
                return null;
            }

            var dispatcher = f.getInstance<IMetaDataDispatcher>(); // parent.DllDispatcher;
            return dispatcher.Get(metaDataName);
        }


         /// <summary>
        /// Set custom colors for a control, eventually recursively. Should be used only in not-managed forms
        /// </summary>
        /// <param name="c"></param>
        /// <param name="recursive"></param>
        public static void SetColorOneTime(Control c, bool recursive) {
	        SetColor(c);
	        if (!recursive || !c.HasChildren) return;
	        foreach (Control cc in c.Controls) {
		        SetColor(cc, true);
	        }
        }

           /// <summary>
        /// Set custom colors for a control, eventually recursively. Should be used only in not-managed forms
        /// </summary>
        /// <param name="c"></param>
        /// <param name="recursive"></param>
        public static void SetColor(Control c, bool recursive) {
	        var ctrl = c?.FindForm()?.getInstance<IFormController>();
	        if (ctrl != null) {
				ctrl.setColor(c,recursive);
				return;
	        }
	        SetColor(c);
	        if (!recursive || !c.HasChildren) return;
	        foreach (Control cc in c.Controls) {
		        SetColor(cc, true);
	        }
        }

         /// <summary>
        /// Set custom color for a specific Control
        /// </summary>
        /// <param name="c"></param>
        public static void SetColor(Control c) {
	        var s = mdl_utils.MetaProfiler.StartTimer($"SetColor * {c?.GetType()}");
	        if (c == null) {
		        mdl_utils.MetaProfiler.StopTimer(s);
		        return;
	        }

	        var ParentColor = formcolors.MainBackColor();
            if (c.Parent is GroupBox box) {
                ParentColor = box.BackColor;
            }


            if (c is MdiClient l) {
                //L.ForeColor = formcolors.MainForeColor();
                l.BackColor = ParentColor;
            }

            if (c is Label lab) {
                //L.ForeColor = formcolors.MainForeColor();
                lab.BackColor = ParentColor;
            }

            if (c is TabControl) {
                c.BackColor = formcolors.MainBackColor();
                c.ForeColor = formcolors.MainForeColor();
                var tb = (TabControl) c;
                tb.DrawMode = TabDrawMode.OwnerDrawFixed;
                tb.DrawItem -= Tb_DrawItem;
                tb.DrawItem += Tb_DrawItem;
                mdl_utils.MetaProfiler.StopTimer(s);
                return;
            }

            if (c is GroupBox g) {
                g.ForeColor = formcolors.MainForeColor();
                g.Paint -= paintGbox;
                g.Paint += paintGbox;
                if (g.Tag != null && g.Tag.ToString().ToLower().StartsWith("auto")) {
                    g.BackColor = formcolors.AutoChooseBackColor();
                }
                else {
                    g.BackColor = formcolors.MainBackColor();
                }

                g.Refresh();
                mdl_utils.MetaProfiler.StopTimer(s);

                return;
            }

            if (c is Button b) {
                if (b.Enabled) {
                    string Tag = tagUtils.GetStandardTag(b.Tag);
                    if (Tag == null)
                        Tag = "";
                    Tag = Tag.ToLower();
                    if (Tag.StartsWith("edit") || Tag.StartsWith("delete") || Tag.StartsWith("insert") ||
                        Tag.StartsWith("unlink")
                    ) {
                        if (Tag.StartsWith("edit"))
                            b.Text = LM.editLable;
                        if (Tag.StartsWith("delete"))
                            b.Text = LM.deleteLable;
                        if (Tag.StartsWith("insert"))
                            b.Text = LM.addLabel;
                        //if (Tag.StartsWith("unlink"))((Button )C).Text="Correggi";
                        b.BackColor = formcolors.GridButtonBackColor();
                        b.ForeColor = formcolors.GridButtonForeColor();

                    }
                    else {
                        b.BackColor = formcolors.ButtonBackColor();
                        b.ForeColor = formcolors.ButtonForeColor();
                    }

                    b.EnabledChanged -= Cmb_EnabledChanged;
                    b.EnabledChanged += Cmb_EnabledChanged;
                }
                else {
                    b.BackColor = formcolors.DisabledButtonBackColor();
                    b.ForeColor = formcolors.DisabledButtonForeColor();
                    b.EnabledChanged -= Cmb_EnabledChanged;
                    b.EnabledChanged += Cmb_EnabledChanged;
                }
                mdl_utils.MetaProfiler.StopTimer(s);

                return;

            }

            if (c is ComboBox cmb) {
                if (cmb.Enabled) {
                    if (cmb.DropDownStyle!=ComboBoxStyle.DropDown && 
                        cmb.Tag != null && cmb.Tag.ToString() != "" && 
                        cmb.SelectedIndex <= 0) cmb.DropDownStyle = ComboBoxStyle.DropDown;
                    cmb.BackColor = formcolors.TextBoxNormalBackColor();
                    cmb.ForeColor = formcolors.TextBoxNormalForeColor();
                }
                else {
                    cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmb.BackColor = formcolors.TextBoxReadOnlyBackColor();
                    cmb.ForeColor = formcolors.TextBoxReadOnlyForeColor();
                }

                cmb.EnabledChanged -= Cmb_EnabledChanged;
                cmb.EnabledChanged += Cmb_EnabledChanged;
                mdl_utils.MetaProfiler.StopTimer(s);

                return;
            }

            if (c is TextBox) {
                var T = c as TextBox;
                if (T.ReadOnly || !T.Enabled) {
                    T.BackColor = formcolors.TextBoxReadOnlyBackColor();
                    T.ForeColor = formcolors.TextBoxReadOnlyForeColor();
                }
                else {
                    T.BackColor = formcolors.TextBoxNormalBackColor();
                    T.ForeColor = formcolors.TextBoxNormalForeColor();
                }
                mdl_utils.MetaProfiler.StopTimer(s);

                return;
            }

            if (c is RadioButton) {
                var T = c as RadioButton;
                if (!T.Enabled) {
                    T.BackColor = formcolors.TextBoxReadOnlyBackColor();
                    T.ForeColor = formcolors.TextBoxReadOnlyForeColor();
                }
                else {
                    T.BackColor = ParentColor;
                    T.ForeColor = formcolors.TextBoxNormalForeColor();
                }

                T.EnabledChanged -= Cmb_EnabledChanged;
                T.EnabledChanged += Cmb_EnabledChanged;
                mdl_utils.MetaProfiler.StopTimer(s);

                return;
            }

            if (c is CheckBox) {
                var T = c as CheckBox;
                if (!T.Enabled) {
                    T.BackColor = formcolors.TextBoxReadOnlyBackColor();
                    T.ForeColor = formcolors.TextBoxReadOnlyForeColor();
                }
                else {
                    T.BackColor = ParentColor;
                    T.ForeColor = formcolors.TextBoxNormalForeColor();
                }

                T.EnabledChanged -= Cmb_EnabledChanged;
                T.EnabledChanged += Cmb_EnabledChanged;
                mdl_utils.MetaProfiler.StopTimer(s);

                return;
            }

            if (c is TreeView) {
                ((TreeView) c).BackColor = formcolors.TreeBackColor();
                ((TreeView) c).ForeColor = formcolors.TreeForeColor();
                mdl_utils.MetaProfiler.StopTimer(s);
                return;
            }

            if (c is DataGrid gg) {
                gg.BackgroundColor = formcolors.GridBackgroundColor();
                mdl_utils.MetaProfiler.StopTimer(s);
                return;
            }

            c.BackColor = formcolors.MainBackColor();
            c.ForeColor = formcolors.MainForeColor();
            mdl_utils.MetaProfiler.StopTimer(s);
        }


        static void Cmb_EnabledChanged(object sender, EventArgs e) {
            SetColor(sender as Control);
        }

        
        private static void paintGbox(object o, PaintEventArgs p) {
	        //var ss = metaprofiler.StartTimer($"lock-paintGbox()");
            //lock (_ispainting) {
                var g = o as GroupBox;
                if (!g.Visible) {
	                //metaprofiler.StopTimer(ss);
	                return;
                }
                var s = mdl_utils.MetaProfiler.StartTimer($"paintGbox * {g.Name}");
                try {

                    //get the text size in groupbox
                    var tSize = TextRenderer.MeasureText(g.Text, g.Font);
                    tSize.Width = Convert.ToInt32(tSize.Width * 1.1);

                    var borderRect = g.ClientRectangle;
                    borderRect.Y = (borderRect.Y + (tSize.Height / 2));
                    borderRect.Height = (borderRect.Height - (tSize.Height / 2));

                    p.Graphics.Clear(g.BackColor);
                    ControlPaint.DrawBorder(p.Graphics, borderRect, formcolors.GboxBorderColor(),ButtonBorderStyle.Inset);
                    //ControlPaint.DrawBorder(p.Graphics, ((GroupBox)o).ClientRectangle, formcolors.GboxBorderColor(), ButtonBorderStyle.Inset);

                    var textRect = g.ClientRectangle;
                    textRect.X = (textRect.X + 6);
                    textRect.Width = tSize.Width;
                    textRect.Height = tSize.Height;
                    p.Graphics.FillRectangle(new SolidBrush(g.BackColor), textRect);
                    p.Graphics.DrawString(g.Text, g.Font, new SolidBrush(g.ForeColor), textRect);
                }
                catch {
                    // ignored
                }
				mdl_utils.MetaProfiler.StopTimer(s);
            //}
            //metaprofiler.StopTimer(ss);
        }

          /// <summary>
        /// Change the appearance of TabControls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Tb_DrawItem(object sender, DrawItemEventArgs e) {
            //lock (_ispainting) {
                var tc = sender as TabControl;
                if (!tc.Visible) return;
                if (e.Index >= tc.TabPages.Count) return;
                var page = tc.TabPages[e.Index];
                var doclear = false;
                if (e.Index == 1 && tc.SelectedIndex == 0)
                    doclear = true;
                else if (e.Index == 0 && tc.SelectedIndex > 0)
                    doclear = true;
                //Debug.WriteLine(TC.Name+"-"+DateTime.Now.ToLongTimeString()+" Drawing "+e.Index +" Selected is " + TC.SelectedIndex);
                if (doclear) {
                    var r = new Rectangle(tc.ClientRectangle.Location,
                        new Size(tc.ClientRectangle.Width, e.Bounds.Height));
                    e.Graphics.FillRectangle(new SolidBrush(formcolors.MainBackColor()), r);
                    //Debug.WriteLine(TC.Name + "-" + DateTime.Now.ToLongTimeString() + " Cleared at " + e.Index);
                }

                e.Graphics.FillRectangle(new SolidBrush(formcolors.TabControlHeaderColor()), e.Bounds);
                var paddedBounds = e.Bounds;
                var yOffset = (e.State == DrawItemState.Selected) ? -2 : 1;
                paddedBounds.Inflate(1, yOffset);
                TextRenderer.DrawText(e.Graphics, page.Text, tc.Font, paddedBounds, formcolors.MainForeColor());
                //e.Graphics.DrawString(TC.TabPages[e.Index].Text, TC.Font, new System.Drawing.SolidBrush(formcolors.MainForeColor()), paddedBounds);
            //}
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
                 if (DR != DialogResult.OK) return;
                 filename = FD.FileName;
             }



             //try {
                 string S = exportclass.DataTableToCSV(dt, true);
                 var SWR = new StreamWriter(filename, false, Encoding.Default);
                 SWR.Write(S);
                 SWR.Close();
                 //SWR.Dispose();
             //}
             //catch (Exception E) {
             //    QueryCreator.ShowException(E);
             //}

             Process.Start(filename);
         }


    }
    
         

}

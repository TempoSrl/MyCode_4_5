using System;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#pragma warning disable IDE1006 // Naming Styles
using mdl;
using mdl_windows;
using mdl_winform;
using mdl_utils;
using static mdl_utils.tagUtils;


namespace mdl_winform {
    /*
		 *   Gestione dei controlli nel Form
		 * Il form serve a visualizzare e modificare le informazioni relative ad un entità
		 *  del sistema (qui entità principale, a cui è associata una tabella primaria). 
		 * Queste informazioni possono essere essenzialmente di tre tipi:
		 * 1) Semplici, es. nomi, date, numeri, commenti, e le altre informazioni che per essere
		 *  visualizzate non hanno bisogno di informazioni appartenenti ad altre tabelle.
		 * 2) Codici, ossia chiavi esterne per altre tabelle. Per visualizzare il codice, è necessario
		 *  estrarre un campo dalla tabella a cui il codice si riferisce. E'possibile anche che
		 *  il codice sia composto da più campi e che per permetterne l'editing sia necessaria una 
		 *  selezione a più livelli. Questi codici rappresentano in genere l'associazione tra l'entità
		 *  principale ed una entità correlata (rappresentata da un altra tabella).
		 * 3) Entità correlate. Queste sono in corrispondenza uno a molti infatti, se fossero in 
		 *  corrispondenza uno a uno, potrebbero essere inglobate nell'entità principale. Tali entità saranno 
		 *  visualizzate con un datagrid e la loro modifica, quando possibile, è effettuata con form 
		 *  esterni. Tali entità sono considerate "subordinate" e la loro chiave primaria include
		 *  quella dell'entità principale. Per tali considerazioni, queste sono tabelle CHILD 
		 *  rispetto alla tabella primaria.
		 * Per visualizzare le informazioni di tipo 1 e 3 è necessario collegare e riempire altre tabelle,
		 *  che saranno considerate "periferiche", ed il cui unico scopo è permettere una corretta
		 *  visualizzazione dei codici presenti nei campi della tabella principale e subordinate.
		 * 
		 */




    /// <summary>
    /// Contains functions to help handling data in Windows Forms
    /// </summary>
    /// 
    public class HelpForm : IDisposable, IHelpForm {

        /// <summary>
        /// Kind of data currently inserted in a combo source
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        [Obsolete]
        public static string ComboTableKind(DataTable T) {
            return T.ExtendedProperties["ComboTableKind"] as string;
        }

        /// <summary>
        /// Start consider a table as an entity when drawing the form and reading data from the form. There should be only a row in the table connected to primary row.
        /// </summary>
        /// <param name="tableName"></param>
        public void addExtraEntity(string tableName) {
            ExtraEntities[tableName] = tableName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> getExtraEntities() {
            return (from string tab in ExtraEntities.Keys select tab).ToList();
        }

        /// <summary>
        /// Possible draw states for a form. 
        /// </summary>
        public enum drawmode {
            /// <summary>
            /// used when adding a new row on primary table
            /// </summary>
            insert,

            /// <summary>
            /// used when modifying an existing row
            /// </summary>
            edit,

            /// <summary>
            /// used when setting a search filter
            /// </summary>
            setsearch

        };

        /// <summary>
        /// Enables or disable Tooltips
        /// </summary>
        public bool toolTipOnControl { get; set; } = false;

        /// <summary>
        /// Controls that manages the list
        /// </summary>
        public Control mainTableSelector {
            get { return MainTableSelector; }
            set { MainTableSelector = value; }
        }

        delegate void ApplyOnControlBool(Control C, bool Insert);

        /// <summary>
        /// Method to be applied ricorsively through IterateControls
        /// </summary>
        /// <param name="C"></param>
        public delegate void ApplyOnControl(Control C);


        delegate void ApplyOnControlSelList(Control C, List<SelectBuilder> selList);

        delegate void ApplyOnControlTable(Control C, string tablename);

        delegate void ApplyOnControlTableSelList(Control C, string tablename, List<SelectBuilder> selList);

        delegate void ApplyOnChildControl(Control C, Control Parent);

        /// <summary>
        /// Tooltip for showing standard debug tooltip
        /// </summary>
        public ToolTip tip = new ToolTip();

        /// <summary>
        /// Delegate for the event "AfterRowSelect" 
        /// </summary>
        public delegate void AfterRowSelectDelegate(
            DataTable SelectedTable,
            DataRow SelectedRow);

        /// <summary>
        /// Event called after row selection
        /// </summary>
        public AfterRowSelectDelegate AfterRowSelect { get; set; }

        /// <summary>
        /// Event called before row selection
        /// </summary>
        public AfterRowSelectDelegate BeforeRowSelect { get; set; }

        /// <summary>
        /// QueryHelper for db conditions
        /// </summary>
        public QueryHelper dbQuery = new SqlServerQueryHelper();

        ///// <summary>
        ///// GetData Object to use for retrieving data from DataBase
        ///// </summary>
        //[Obsolete] public GetData getd;


        //public IGetData getData { get; set; }
        //public bool Insert;

        /// <summary>
        /// Tables linked to "SubEntity" controls. These are read during GetForm(). 
        /// </summary>
        private Hashtable ExtraEntities;

        /// <summary>
        /// When true combobox are refilled. This is true when a main rows is read the first time, to 
        ///  properly consider security conditions
        /// </summary>
        //[Obsolete] public bool ComboBoxToRefilter = true;

        DataSet DS;

        /// <summary>
        /// Extended property for tree-filtering
        /// </summary>
        public const string FilterTree = "MetaData_TreeFilterTable";



        /// <summary>
        /// Current draw mode, set by the MetaData that owns this class
        /// </summary>
        private drawmode dmode;

        /// <summary>
        /// Phase of drawing insert / edit /setsearch
        /// </summary>
        public virtual drawmode drawMode {
            get { return dmode; }
            set { dmode = value; }
        }

        ContextMenu ExcelMenu;


        private void excelClick(object menusender, EventArgs e) {
            if (destroyed) return;
            var contextMenu = (menusender as MenuItem)?.Parent.GetContextMenu();
            object sender = contextMenu?.SourceControl;
            if (!(sender is DataGrid)) return;
            var g = (DataGrid) sender;
            var dds = g.DataSource;
            if (!(dds is DataSet)) return;
            var ddt = g.DataMember;
            if (ddt == null) return;
            var T = ((DataSet) dds).Tables[ddt];
            if (T == null) return;
            mdl_winform.exportclass.DataTableToExcel(T, true);
        }

        private void csvClick(object menusender, EventArgs e) {
            if (destroyed) return;
            var contextMenu = (menusender as MenuItem)?.Parent.GetContextMenu();
            object sender = contextMenu?.SourceControl;
            if (!(sender is DataGrid)) return;
            var g = (DataGrid) sender;
            var dds = g.DataSource;
            if (!(dds is DataSet)) return;
            var ddt = g.DataMember;
            if (ddt == null) return;
            var T = ((DataSet) dds).Tables[ddt];
            if (T == null) return;

            var fd = new OpenFileDialog {
                Title = "Seleziona il file da creare",
                AddExtension = true,
                DefaultExt = "CSV",
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false
            };
            var dr = fd.ShowDialog();
            if (dr != System.Windows.Forms.DialogResult.OK) return;




            try {
                var s = mdl_windows.exportclass.DataTableToCSV(T, true);
                var swr = new StreamWriter(fd.FileName, false, Encoding.Default);
                swr.Write(s);
                swr.Close();
                //SWR.Dispose();
            }
            catch (Exception ex) {
                shower.ShowException(null,null,ex);
            }
            Process.Start(fd.FileName);
        }

        bool destroyed = false;


        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Destroy() {
            if (destroyed) return;
            destroyed = true;
            additionalInfo.Clear();
            additionalInfo = null;
            //getd = null;
            DS = null;
            ExtraEntities.Clear();
            ExcelMenu.MenuItems.Clear();
            ExcelMenu = null;
            MainTableSelector = null;
            if (tip != null) {
                tip.RemoveAll();
                tip.Dispose();
                tip = null;
            }

            LastTextBoxChanged = null;
            controlEnabler = null;
            comboBoxManager = null;
            conn = null;
            dbQuery = null;
            eventManager = null;
            listViewManager = null;
            primaryTable = null;
            shower = null;
            cQuery = null;
        }

        private DataTable primaryTable;

        private IComboBoxManager comboBoxManager;

        

        private IControEnabler controlEnabler = MetaFactory.factory.createInstance<IControEnabler>();
        private IFormEventsManager eventManager { get; set; }

        /// <summary>
        /// CQueryHelper instance used in this class
        /// </summary>
        public CQueryHelper cQuery = new CQueryHelper();

        /// <summary>
        /// Initialize the class with a GetData object
        /// </summary>
        /// <param name="getd"></param>
        /// <param name="ds"></param>
        [Obsolete]
        public HelpForm(GetData getd, DataSet ds) {
            this.DS = ds;
            this.primaryTable = getd.PrimaryDataTable;
            this.dbQuery = getd.Conn.GetQueryHelper();
            this.conn = getd.Conn;
            tip.ShowAlways = true;
            ExtraEntities = new Hashtable();
            ExcelMenu = new ContextMenu();
            ExcelMenu.MenuItems.Add("Excel", excelClick);
            ExcelMenu.MenuItems.Add("CSV", csvClick);
            //_myAutoEventEnabled = 0;
            _tableToMonitor = null;
            MainTableSelector = null;
        }

        private IMetaModel model;
        /// <summary>
        /// Initialize the class with a GetData object
        /// </summary>
        public HelpForm() { //DataTable primaryTable, IDataAccess conn, IFormEventsManager eventsManager
            //this.primaryTable = primaryTable;
            //this.DS = primaryTable.DataSet;
            //this.dbQuery = conn.GetQueryHelper();
            //this.conn = conn;
            //this.eventManager = eventsManager;
            ExtraEntities = new Hashtable();
            ExcelMenu = new ContextMenu();
            _tableToMonitor = null;
            MainTableSelector = null;
          
        }

        /// <summary>
        /// Do all necessary initializations
        /// </summary>
        public virtual void Init(Form f) { //IGetData getData   DataTable primaryTable, IDataAccess conn, IFormEventsManager eventsManager

            primaryTable = f.getInstance<DataTable>();
            DS = primaryTable.DataSet;
            conn = f.getInstance<IDataAccess>();
            dbQuery = conn.GetQueryHelper();
            eventManager = f.getInstance<IFormEventsManager>();
            tip.ShowAlways = true;
            controlEnabler.init(primaryTable);
            comboBoxManager = f.createInstance<IComboBoxManager>();
            registerToEventManager(eventManager);
            model = MetaFactory.factory.getSingleton<IMetaModel>();


            ExcelMenu.MenuItems.Add("Excel", excelClick);
            ExcelMenu.MenuItems.Add("CSV", csvClick);
        }

        void registerToEventManager(IFormEventsManager eventManager) {
            comboBoxManager.registerToEventManager(eventManager);
        }

        private IDataAccess conn;






        #region Gestione Semaforo AutoEventEnabled

        ///// <summary>
        ///// Events are Enabled only if AutoEventEnabled=0
        ///// </summary>
        ///// private int _myAutoEventEnabled;



        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeTable"></param>
        /// <param name="rowsAllowed"></param>
        public static void SetFilterToTree(DataTable treeTable, DataTable rowsAllowed) {
            treeTable.ExtendedProperties[FilterTree] = rowsAllowed;
        }



        #region Funzioni di Gestione dei Tag



      


      

        

        


      




      

        #endregion

        #region Funzioni di abilitazione/disabilitazione dei controlli





        #endregion


        #region Get Current Related Parent / Child Rows

        /// <summary>
        /// Gets a child row of parentRow in childTable.
        /// It's necessary that only one child row exists in that table.
        /// </summary>
        /// <param name="parentRow"></param>
        /// <param name="childTable"></param>
        /// <returns></returns>
        public static DataRow GetCurrChildRow(DataRow parentRow, DataTable childTable) {
            if (parentRow == null) return null;
            if (childTable == null) return null;
            if (parentRow.RowState == DataRowState.Detached) return null;
            var rel = QueryCreator.GetParentChildRel(parentRow.Table, childTable);
            if (rel == null) {
                if (parentRow.Table.Rows.Count == 1) {
                    if (childTable.Rows.Count == 1) return childTable.Rows[0];
                }
                if (childTable.ParentRelations.Count != 1) return null;
                var childRel = childTable.ParentRelations[0];
                var parentTable = childRel.ParentTable;
                var currParent = GetCurrChildRow(parentRow, parentTable);
                var currChilds = currParent?.iGetChildRows(childRel); //corretto 2017, c'era rel ma rel è sempre null
                return currChilds?.Length == 1 ? currChilds[0] : null;
            }
            var childs = parentRow.iGetChildRows(rel);
            return childs.Length == 1 ? childs[0] : null;
        }

        /// <summary>
        /// Takes parent row of current selected primary row. Also apply some logic
        ///  to give reasonable results on errors:
        ///  - if a parent relation is not found and the both the Primary and the Parent 
        ///		table only contains one  row, that row is retuned
        ///  - if the parent table only has one child table and that child table has a
        ///   row parent of the given row (by the same logic), that row is returned
        /// </summary>
        /// <param name="childRow">Primary whose parent is searched</param>
        /// <param name="parentTable">Parent Table</param>
        /// <returns>Parent Row of given Row</returns>
        public static DataRow GetCurrParentRow(DataRow childRow, DataTable parentTable) {
            if (childRow == null) return null;
            if (parentTable == null) return null;
            if (childRow.RowState == DataRowState.Detached) return null;
            var rel = QueryCreator.GetParentChildRel(parentTable, childRow.Table);
            if (rel == null) {
                if (childRow.Table.Rows.Count == 1) {
                    if (parentTable.Rows.Count == 1) return parentTable.Rows[0];
                }
                if (parentTable.ChildRelations.Count != 1) return null;
                var myRel = parentTable.ChildRelations[0];
                var childTable = myRel.ChildTable;
                var currChild = GetCurrParentRow(childRow, childTable);
                var currParents = currChild?.iGetParentRows(myRel);
                return currParents?.Length == 1 ? currParents[0] : null;
            }
            var parents = childRow.iGetParentRows(rel);
            return parents.Length == 1 ? parents[0] : null;
        }


        /// <summary>
        /// Gets the current parent Row of the specified Primary row. Can return null if the answer is not sure.
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="parent"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DataRow GetCurrParentRow(DataRow primary, DataTable parent, string field) {
            if (primary == null) return null;
            if (parent == null) return null;
            var rel = QueryCreator.GetParentChildRel(parent, primary.Table);
            if (rel == null) {
                if (primary.Table.Rows.Count == 1) {
                    if (parent.Rows.Count == 1) return parent.Rows[0];
                }
                if (parent.ChildRelations.Count != 1) return null;
                var myRel = parent.ChildRelations[0];
                var childTable = myRel.ChildTable;
                var currChild = GetCurrParentRow(primary, childTable);
                var currParents = currChild?.iGetParentRows(myRel);
                return currParents?.Length != 1 ? null : currParents[0];
            }
            var parents = primary.iGetParentRows(rel);
            if (parents.Length == 1) return parents[0];
            if (parents.Length == 0) return null;
            var firstval = parents[0][field].ToString().ToUpper().TrimEnd();
            for (var i = 1; i < parents.Length; i++) {
                if (firstval != parents[i][field].ToString().ToUpper().TrimEnd()) return null;
            }
            return parents[0];
        }

        #endregion


        //Note a control is read only if:
        // - is linked to PrimaryTable
        // - is linked to a subentity table and it's name starts with "SubEntity" 
        //    and related table contains ONE row.

        #region Get Data From Control

        void getControl(Control C) {
            System.Type cType = C.GetType();
            if (typeof(Label).IsAssignableFrom(cType)) return;
            if (typeof(Button).IsAssignableFrom(cType)) return;
            var Tag = GetStandardTag(C.Tag);
            if (Tag == null) return;
            var table = GetTableName(Tag);
            if (table == null) return;
            if (DS.Tables[table] == null) return;

            if (typeof(ListView).IsAssignableFrom(cType)) {
                listViewManager.getListView((ListView) C, primaryTable);
                return;
            }

            var column = GetColumnName(Tag);
            if (column == null) return;
            if (DS.Tables[table].Columns[column] == null) return;
            DataRow R = null;

            if (table != primaryTable.TableName) {
                var subEntity = DS.Tables[table];
                if (C.Name.StartsWith("SubEntity")) {
                    var currPrimary = GetLastSelected(primaryTable);
                    if (currPrimary != null) R = GetCurrChildRow(currPrimary, subEntity);
                    //R=DS.Tables[table].Rows[0];
                    addExtraEntity(table); //mark the table
                }
            }
            else {
                R = GetLastSelected(DS.Tables[table]);
            }
            if (R == null) return;

            //source is the field to fill in the control
            if (typeof(ComboBox).IsAssignableFrom(cType)) {
                getCombo((ComboBox) C, column, R);
            }
            if (typeof(TextBox).IsAssignableFrom(cType)) {
                GetText((TextBox) C, column, R);
            }
            if (typeof(CheckBox).IsAssignableFrom(cType)) {
                getCheckBox((CheckBox) C, column, R);
            }
            if (typeof(RadioButton).IsAssignableFrom(cType)) {
                getRadioButton((RadioButton) C, column, R);
            }


            if (typeof(GroupBox).IsAssignableFrom(cType)) {
                if (GetFieldLower(Tag, 2) == "valuesigned") {
                    GetValueSignedGroup((GroupBox) C, column, R);
                    return;
                }
            }
        }




        /// <summary>
        /// Gets data from all textbox, checkboxes, radiobuttons and comboboxes of the form linked
        ///  to the primary table.
        /// </summary>
        /// <param name="F">Form to Get</param>
        /// <remarks>The primary assumption for this function to work is that for every control
        ///    in the form, a tag is set that logically links it to a field of the Primary Table.
        ///    The exact format of the tag depends on the Control Type:
        ///    TextBox:  fieldname
        ///    ComboBox: master[:parenttable.parentfield]
        ///    RadioButton: fieldname:value (to assign when checked)
        ///    CheckBox: fieldname:valueYes:valueNo
        ///   </remarks>
        public void GetControls(Form F) {
            //MarkEvent("GetControls on form "+F.Text+" called.\n\r");
            int handle = metaprofiler.StartTimer("myBeforeRowSelect()");
            IterateGetControls(F.Controls);
            metaprofiler.StopTimer(handle);

        }

        /// <summary>
        /// Recursively Iterate GetControl over a Control Collection
        /// </summary>
        /// <param name="Cs"></param>
        public void IterateGetControls(Control.ControlCollection Cs) {
            List<Control> List = getSortedControlList(Cs);
            foreach (Control C in List) {
                //int getchandle= metaprofiler.StartTimer("GetControl(Control C)");
                getControl(C);
                //metaprofiler.StopTimer(getchandle);
                if (!C.HasChildren) continue;
                if (isManagedCollection(C)) continue;
                IterateGetControls(C.Controls);
            }
        }


        void getCombo(ComboBox C, string column, DataRow R) {
            if ((C.SelectedValue == null) || (C.SelectedIndex <= 0)) {
                R[column] = QueryCreator.clearValue(R.Table.Columns[column]);
            }
            else {
                if (R[column].ToString() != C.SelectedValue.ToString()) R[column] = C.SelectedValue;
            }
        }

        

        /// <summary>
        /// Sets R[fieldname] to (string) S but:
        /// - don't do anything if R[fieldname] is already equal to S
        /// - convert S to  R[fieldname] type according to S'tag 
        /// </summary>
        /// <param name="S"></param>
        /// <param name="fieldname"></param>
        /// <param name="R"></param>
        /// <param name="tag"></param>
        void getString(string S, string fieldname, DataRow R, string tag) {
            tag = CompleteTag(tag, R.Table.Columns[fieldname]);
            if (S == HelpUi.StringValue(R[fieldname], tag)) return;
            try {
                if (S.TrimEnd() == "") {
                    try {
                        R[fieldname] = DBNull.Value;
                    }
                    catch {
                        //R[fieldname] = "";
                    }
                }
                else {
                    object SS = HelpUi.GetObjectFromString(R.Table.Columns[fieldname].DataType, S, tag);
                    if (SS != null) R[fieldname] = SS;
                }

            }
            catch (Exception e) {
                ErrorLogger.Logger.markException(e,$"GetString({S})");
            }
        }

        void getRadioButton(RadioButton C, string column, DataRow R) {
            string Tag = GetStandardTag(C.Tag);
            int pos = Tag.IndexOf(':');
            if (pos == -1) return;
            string Cvalue = Tag.Substring(pos + 1).Trim();
            if (!Cvalue.StartsWith(":")) {
                string rowvalue = R[column].ToString();
                if (C.Checked) {
                    if (rowvalue != Cvalue) R[column] = Cvalue;
                }
            }
            else {
                bool negato = false;
                Cvalue = Cvalue.Substring(1);
                if (Cvalue.StartsWith("#")) {
                    Cvalue = Cvalue.Substring(1);
                    negato = true;
                }


                //if (C.CheckState == CheckState.Indeterminate) return; //salta il bit, non ha effetto
                int Nbit = Convert.ToInt32(Cvalue);
                UInt64 val = 1;
                val <<= (Nbit);
                object currval = R[column];
                if (currval == DBNull.Value) currval = (UInt64) 0;
                var X = Convert.ToUInt64(currval);
                bool valore = C.Checked;
                if (negato) valore = !valore;
                if (valore)
                    X |= val;
                else
                    X &= (~val);
                R[column] = X;
            }


        }


        void getCheckBox(CheckBox C, string column, DataRow R) {
            string Tag = GetStandardTag(C.Tag);
            int pos = Tag.IndexOf(':');
            if (pos == -1) return;
            string values = Tag.Substring(pos + 1).Trim();
            if (values.IndexOf(":") == -1) {
                bool negato = false;
                if (values.StartsWith("#")) {
                    negato = true;
                    values = values.Substring(1);
                }

                //if (C.CheckState == CheckState.Indeterminate) return; //salta il bit, non ha effetto
                int Nbit = Convert.ToInt32(values);
                ulong val = 1;
                val <<= (Nbit);
                object currval = R[column];
                if (currval == DBNull.Value) currval = (ulong) 0;
                ulong X = Convert.ToUInt64(currval);
                bool valore = C.Checked;
                if (negato) valore = !valore;
                if (valore)
                    X |= val;
                else
                    X &= (~val);
                R[column] = X;
                return;
            }

            string Yvalue = values.Split(new Char[] {':'}, 2)[0].Trim();
            string Nvalue = values.Split(new Char[] {':'}, 2)[1].Trim();

            string newvalue;
            string rowvalue = R[column].ToString();
            if (C.CheckState == CheckState.Indeterminate) {
                if (R[column] != DBNull.Value) R[column] = DBNull.Value;
            }
            else {
                if (C.Checked)
                    newvalue = Yvalue;
                else
                    newvalue = Nvalue;

                if (newvalue != rowvalue) R[column] = newvalue;
            }
        }





        /// <summary>
        /// Takes value from a TextBox and put it in a row field
        /// </summary>
        /// <param name="T"></param>
        /// <param name="fieldname"></param>
        /// <param name="R"></param>
        public void GetText(TextBox T, string fieldname, DataRow R) {
            //if (!T.Modified) return;
            string Tag = GetStandardTag(T.Tag);
            getString(T.Text, fieldname, R, Tag);
        }



        /// <summary>
        /// Gets a value in a ValueSigned groupbox
        /// </summary>
        /// <param name="G"></param>
        /// <param name="fieldname"></param>
        /// <param name="R"></param>
        public void GetValueSignedGroup(GroupBox G, string fieldname, DataRow R) {
            //if (!T.Modified) return;
            var T = searchValueTextBox(G);
            if (T == null) return;
            var O = HelpUi.GetObjectFromString(
                R.Table.Columns[fieldname].DataType,
                T.Text,
                null);
            if (O == null) {
                getString("", fieldname, R, null);
                return;
            }
            bool sign = getSignForValueSigned(G);
            if (!sign) O = invertSign(O);
            string tag = GetStandardTag(T.Tag);
            string S = HelpUi.StringValue(O, tag);
            getString(S, fieldname, R, tag);
        }





        #endregion


        #region Fill targeted Controls 

        /// <summary>
        /// Sets controls of forms so that primary table controls describes a child of ParentRow
        ///  Affected controls are only TextBox, prefilled combobox, prefilled
        ///    TreeView, RadioButtons and CheckBoxes. Assumes form in "setsearch" mode
        /// </summary>
        /// <param name="f">Calling Form</param>
        /// <param name="parentTable">Table considered as Parent </param>
        /// <param name="parentRow">Row that should belong to getd.DS and should be
        ///  in a parent table of primary table</param>
        public void FillParentControls(Form f,
            DataTable parentTable,
            DataRow parentRow
        ) {

            FillParentControls(f.Controls, parentTable, parentRow);
        }


        /// <summary>
        /// Fills a collection of controls related to a specified parent Table
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="parentTable"></param>
        /// <param name="parentRow"></param>
        public void FillParentControls(Control.ControlCollection cs,
            DataTable parentTable,
            DataRow parentRow
        ) {
            //Search relation between PrimaryTable and ParentRow.Table
            DataRelation rfound = null;
            foreach (DataRelation r in primaryTable.ParentRelations) {
                if (r.ParentTable.TableName == parentTable.TableName) {
                    rfound = r;
                    break;
                }
            }
            FillParentControls(cs, parentRow, rfound);
        }



        /// <summary>
        /// Fills all form controls related to a specified parent Table by a specified condition
        /// </summary>
        /// <param name="f"></param>
        /// <param name="parentRow"></param>
        /// <param name="rel"></param>
        public void FillParentControls(Form f, DataRow parentRow, DataRelation rel) {
            FillParentControls(f.Controls, parentRow, rel);
        }

        /// <summary>
        /// Fills controls of forms to display a Parent Row so that primary table 
        ///		controls becomes a child of ParentRow
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="parentRow"></param>
        /// <param name="rel"></param>
        public void FillParentControls(Control.ControlCollection cs,
            DataRow parentRow, DataRelation rel) {
            if (rel == null) return;
            for (var i = 0; i < rel.ParentColumns.Length; i++) {
                var cparent = rel.ParentColumns[i];
                var cchild = rel.ChildColumns[i];
                fillSpecificControls(cs,
                    primaryTable,
                    cchild.ColumnName,
                    parentRow != null ? parentRow[cparent.ColumnName] : DBNull.Value);
            }
        }

        /// <summary>
        /// Fill  parent's related controls so that current primary row controls
        ///  describe a child of that row
        /// </summary>
        /// <param name="f"></param>
        /// <param name="parentRow"></param>
        /// <param name="relname"></param>
        public void FillParentControls(Form f, DataRow parentRow, string relname) {
            var rfound = primaryTable.ParentRelations[relname];
            if (rfound.ParentTable.TableName != parentRow.Table.TableName) return;
            FillParentControls(f, parentRow, rfound);
        }


        /// <summary>
        /// Fills form's controls linked to a Table. If a Row is given,
        ///  it is used for getting values. Otherwise, values are cleared
        /// </summary>
        /// <param name="f"></param>
        /// <param name="table"></param>
        /// <param name="row"></param>
        public void FillTableControls(Form f, DataTable table, DataRow row) {

            FillSpecificRowControls(f.Controls, table, row);
            
        }

        /// <summary>
        /// Fill form's control related to some fields of a row
        /// </summary>
        /// <param name="f">Form to fill</param>
        /// <param name="table">Table whose controls have to be filled</param>
        /// <param name="row">Row from which values have to be taken</param>
        /// <param name="cs">Collection of columns to be displayed</param>
        public void FillTableControls(Form f,
            DataTable table, DataRow row,
            DataColumn[] cs) {            
            foreach (var c in cs) {
                object val = DBNull.Value;
                if (row != null) val = row[c.ColumnName];
                fillSpecificControls(f.Controls,
                    table,
                    c.ColumnName,
                    val);
            }
        }

        /// <summary>
        /// Fill form's control related to all fields of a row
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="table"></param>
        /// <param name="r">Row from which values have to be taken</param>
        public void FillSpecificRowControls(Control.ControlCollection cs, DataTable table,DataRow r) {
            foreach (Control c in cs) {
                fillSpecificRowControl(c, table, r);
                if (!c.HasChildren) continue;
                if (isManagedCollection(c)) continue;
                FillSpecificRowControls(c.Controls, table, r);
            }

        }

        void fillSpecificRowControl(Control c, DataTable table, DataRow r) {
            var tag = GetStandardTag(c.Tag);
            if (tag == null) return;

            var tableName = GetTableName(tag);
            if (tableName == null) return;
            if (tableName != table.TableName) return;

            var column = GetColumnName(tag);
            if (column == null) return;
            object fieldvalue;
            if (r == null) {
                fieldvalue = DBNull.Value;
            }
            else {
                if (r.Table.Columns[column] == null) return;
                fieldvalue = r[column];
            }

            if (c is ComboBox) {
                //if (comboBoxToRefilter) comboBoxManager.checkComboBoxSource((ComboBox) C, fieldvalue, drawMode);
                comboBoxManager.setCombo((ComboBox) c, table, column, fieldvalue);
                //controlEnabler.enableDisable(C, Table, column, drawMode);
            }

            if (c is TextBox) {
                SetText((TextBox) c, table, column, fieldvalue);
            }
            if (c is CheckBox) {
                setCheckBox((CheckBox) c, table, column, fieldvalue);
            }
            if (c is RadioButton) {
                setRadioButton((RadioButton) c, table, column, fieldvalue);
            }

            if (c is GroupBox) {
                if (GetFieldLower(tag, 2) == "valuesigned") {
                    fillValueSignedGroup((GroupBox) c, table, column, fieldvalue);
                    return;
                }
            }

        }


        void fillSpecificControls(IEnumerable cs, DataTable table,string colname,
            object fieldvalue) {
            if (cs == null) return;
            foreach (Control c in cs) {
                fillSpecificControl(c, table, colname, fieldvalue);
                if (!c.HasChildren) continue;
                if (isManagedCollection(c)) continue;
                fillSpecificControls(c.Controls, table, colname, fieldvalue);
            }
        }


        [SuppressMessage("ReSharper", "MergeCastWithTypeCheck")]
        void fillSpecificControl(Control c,
            DataTable table,
            string colname,
            object fieldvalue) {
            var tag = GetStandardTag(c.Tag);
            if (tag == null) return;

            var tableName = GetTableName(tag);
            if (tableName == null) return;
            if (tableName != table.TableName) return;

            //TODO: impostare il treeview in base al valore scelto
//            //Ancora non testato nemmeno logicamente.
//            if (typeof(TreeView).IsAssignableFrom(C.GetType())){
//                SetTree((TreeView) C, R, Insert);
//                return;
//            }

            var column = GetColumnName(tag);
            if (column == null) return;
            if ((colname != null) && (column != colname)) return;

            if (c is ComboBox) {
                //if (ComboBoxToRefilter) comboBoxManager.checkComboBoxSource((ComboBox) C, fieldvalue, dmode);
                comboBoxManager.setCombo((ComboBox) c, table, column, fieldvalue);
                controlEnabler.enableDisable(c, table, column, drawMode);
            }

            if (c is TextBox) {
                SetText((TextBox) c, table, column, fieldvalue);
            }
            if (c is CheckBox) {
                setCheckBox((CheckBox) c, table, column, fieldvalue);
            }
            if (c is RadioButton) {
                setRadioButton((RadioButton) c, table, column, fieldvalue);
            }

            if (c is GroupBox) {
                if (GetFieldLower(tag, 2) == "valuesigned") {
                    fillValueSignedGroup((GroupBox) c, table, column, fieldvalue);
                    return;
                }
            }

        }

        #endregion


        #region Set DataGrid 

       





        /// <summary>
        /// Set association between grid columns and DataTable Columns and
        ///  sets Caption and NullText of any grid columns.	
        /// </summary>
        /// <param name="g"></param>
        /// <param name="T"></param>
        static public void SetGridStyle(DataGrid g, DataTable T) {
            var setStyleHandle = metaprofiler.StartTimer("SetGridStyle");
            //if (G.DataSource == null) return;

            g.AllowNavigation = false;
            foreach (DataGridTableStyle dgt in g.TableStyles) {
                if (dgt.MappingName == T.TableName) {
                    metaprofiler.StopTimer(setStyleHandle);
                    return;
                }
            }
            //G.TableStyles.Clear(); decommentando diventa rigorosamente vietato avere nomi diversi per i datatable sullo stesso datagrid
            // siccome a volte sono usate tabelle temporanee, meglio lasciarlo

            var ts = new DataGridTableStyle {
                ReadOnly = true,
                BackColor = formcolors.GridBackColor(),
                ForeColor = formcolors.GridForeColor(),
                AlternatingBackColor = formcolors.GridAlternatingBackColor(),
                SelectionBackColor = formcolors.GridSelectionBackColor(),
                SelectionForeColor = formcolors.GridSelectionForeColor(),
                HeaderBackColor = formcolors.GridHeaderBackColor(),
                HeaderForeColor = formcolors.GridHeaderForeColor(),
                MappingName = T.TableName
            };
            var cols = new DataColumn[T.Columns.Count];
            if ((cols.Length == 0) || (T.Columns[0].ExtendedProperties["ListColPos"] == null)) {
                for (var i = 0; i < cols.Length; i++) cols[i] = T.Columns[i];
            }
            else {
                for (var i = 0; i < cols.Length; i++) {
                    try {
                        var colPos = Convert.ToInt32(T.Columns[i].ExtendedProperties["ListColPos"]);
                        if (colPos != -1) cols[colPos] = T.Columns[i];
                    }
                    catch {
                        //ignore
                    }
                }
            }

            foreach (var c in cols) {
                if (c == null) continue;
                if (c.Caption == "") continue;
                if (c.Caption.StartsWith(".")) c.Caption = " ";

                var c1 = new MyGridColumn(null, GetFormatForColumn(c)) {
                    ReadOnly = true,
                    Alignment = (System.Windows.Forms.HorizontalAlignment) HelpUi.GetAlignForColumn(c),
                    MappingName = c.ColumnName
                };

                if (c1.Alignment == System.Windows.Forms.HorizontalAlignment.Right && c.Caption != " ") {
                    c1.HeaderText = c.Caption + ".";
                }
                else {
                    c1.HeaderText = c.Caption;
                }
                c1.NullText = "";

                ts.GridColumnStyles.Add(c1);
            }

            //G.TableStyles.Clear();
            g.TableStyles.Add(ts);


            addNavigatorEventToGrid(g);
            FormController.addEditEventToGrid(g);


            metaprofiler.StopTimer(setStyleHandle);
        }






        public static IMetaModel staticModel = MetaFactory.factory.getSingleton<IMetaModel>();

        /// <summary>
        /// Sets some property of DataGrid to make it good-looking: 
        ///  - decimal viewed as "numbers"
        ///  - Headers = Caption
        ///  - no "null" values
        ///  - adjust column width
        /// and binds a (eventually) filtered duplicate of the table to the grid
        /// </summary>
        /// <param name="G"></param>
        /// <param name="T"></param>
        public static void SetDataGrid(DataGrid G, DataTable T) {
            var setgridhandle = metaprofiler.StartTimer("SetDataGrid(DataGrid G, DataTable T)");
            SetGridStyle(G, T);
        

            DataTable tt = T;
            if (G.Tag != null) {
                var listType = GetField(G.Tag.ToString(), 1);
                calcParentRelation(G, T.DataSet);
                
                tt = getFilteredTable(T, listType);
            }

            if ((G.DataSource == null) || (tt != T)) {
                FormController.SetLinkedGrid(tt, G);
                new gridTableManager(staticModel, tt, G);
                G.ReadOnly = true;
                //G.BeginInit();
                G.SetDataBinding(tt.DataSet, tt.TableName);
                //G.EndInit();

                //MarkEvent("Data binding was set.");
                if (G.CaptionText == "") G.CaptionVisible = false;
                if (tt.Rows.Count > 0) {
                    //G.CurrentRowIndex=0;
                    //G.Select(0);
                    //G.Focus();
                }

                //MarkEvent("Formatting grid...");
                var formatting = metaprofiler.StartTimer("Format grid");
                var format = new formatgrids(G);
                format.AutosizeColumnWidth();
                metaprofiler.StopTimer(formatting);


            }

            //MarkEvent("Grid Formatted.");
            metaprofiler.StopTimer(setgridhandle);
        }



        void setDataGrid(DataGrid g) {
	        if (g == null) return;
            var currPrimary = GetLastSelected(primaryTable);
            var table = GetTableName(g?.Tag?.ToString());
            if (table == null) return;
            var T = DS?.Tables[table];
            if (T == null) return;
            var currrow = g.CurrentRowIndex;

            var res = GetCurrentRow(g, out var tempT1, out var tempRow1);
            if (!res) tempRow1 = null;

            if (T.TableName != primaryTable.TableName) eventManager.DisableAutoEvents();
            DataRelation gridParent = QueryCreator.GetParentChildRel(primaryTable, T);
            if ((gridParent == null) || (currPrimary != null)) {
                SetDataGrid(g, T);
            }
            else {
                //Il grid non ha datasource se non c'è una riga principale attiva
                if (T.TableName != primaryTable.TableName) {
                    g.DataSource = null;
                }
            }

            if (T.TableName != primaryTable.TableName) eventManager.EnableAutoEvents();

            if (T.TableName != primaryTable.TableName) {
                ControlChanged(g, null);
            }
            else {
                var res2 = GetCurrentRow(g, out var tempT2, out var tempRow2);
                if (!res2) tempRow2 = null;
                string k1 = null;
                if (tempRow1 != null) k1 = QueryCreator.WHERE_KEY_CLAUSE(tempRow1, DataRowVersion.Default, false);
                string k2 = null;
                if (tempRow2 != null) k2 = QueryCreator.WHERE_KEY_CLAUSE(tempRow2, DataRowVersion.Default, false);
                if (k1 != k2) ControlChanged(g, null);
            }

            //G.Invalidate();	13 maggio 2005 : DA VERIFICARE CHE NON CREA DANNI
            //G.Update();		13 maggio 2005 : DA VERIFICARE CHE NON CREA DANNI
            if (currrow == -1) return;

            var gridDs =  g?.DataSource as DataSet;
            var gridTb = gridDs?.Tables[g.DataMember];
            if (gridTb == null) return;
            var nGridRows = g.VisibleRowCount;
            if (nGridRows > currrow) {
                if (currrow < 0) currrow = 0;
                if (g.IsSelected(currrow)) return;
                ClearSelection(g);
                if (g.CurrentRowIndex != currrow) g.CurrentRowIndex = currrow;
                //if (!G.IsSelected(currrow)) G.Select(currrow);
                //if (G.Enabled) G.Select(currrow);
                return;
            }
            if (nGridRows > 0) {
                if (g.IsSelected(0)) return;
                ClearSelection(g);
                if (g.CurrentRowIndex != 0) g.CurrentRowIndex = 0;
                //if (!G.IsSelected(currrow)) G.Select(0);
                //if (G.Enabled) G.Select(0);
                return;
            }
        }

        /// <summary>
        /// Allow grid multiple selection on a DataTable
        /// </summary>
        /// <param name="T"></param>
        /// <param name="allow"></param>
        public static void SetAllowMultiSelection(DataTable T, bool allow) {
            T.ExtendedProperties["AllowMultiSelection"] = allow;
        }

        /// <summary>
        /// Check if grid multiple selection is enabled on a DataTable
        /// </summary>
        /// <param name="T"></param>
        public static bool GetAllowMultiSelection(DataTable T) {
            if (T.ExtendedProperties["AllowMultiSelection"] == null) return false;
            return (bool) T.ExtendedProperties["AllowMultiSelection"];
        }

        /// <summary>
        /// Removes the selection from all frid rows
        /// </summary>
        /// <param name="G"></param>
        public static void ClearSelection(DataGrid G) {
            var gridDs = (DataSet) G.DataSource;
            if (gridDs == null) return;
            var gridTb = gridDs.Tables[G.DataMember.ToString()];
            if (GetAllowMultiSelection(gridTb)) return;
            if (gridTb == null) return;
            try {
                for (var i = 0; i < gridTb.Rows.Count; i++) {
                    if (G.IsSelected(i)) G.UnSelect(i);
                }
            }
            catch {
                //ignore
            }

        }

        /// <summary>
        /// Delegate for DataTable Adjuster. It is called when a DataTable
        ///  is going to be displayed on a gird or on a treeviw
        /// </summary>
        public delegate void AdjustTable(Control c, DataTable T, string tag);

        /// <summary>
        /// Adjust all tables in order to be displayed on grids or tree
        /// </summary>
        /// <param name="F"></param>
        /// <param name="adjust"></param>
        public void AdjustTablesForDisplay(Form F, AdjustTable adjust) {
            iterateAdjustTablesForDisplay(F.Controls, adjust);
        }

        void adjustForDisplay(Control c, AdjustTable adjust) {
            if ((c.Tag == null) || (c.Tag.ToString() == "")) return;
            string tag = GetStandardTag(c.Tag);
            if (tag == null) return;
            string table = GetTableName(tag);
            if (tag.StartsWith("TreeNavigator")) {
                table = primaryTable.TableName;
                tag = tag.Replace("TreeNavigator", table);
            }
            if (table == null) return;
            if (DS.Tables[table] == null) return;
            adjust(c, DS.Tables[table], tag);
        }

        void iterateAdjustTablesForDisplay(IEnumerable cs, AdjustTable adjust) {
            foreach (Control c in cs) {
                if (c is DataGrid ||
                    c is TreeView ||
                    c is ListView ||
                    c is ComboBox ||
                    c is TextBox
                ) {
                    adjustForDisplay(c, adjust);
                }
                if (c.HasChildren) {
                    iterateAdjustTablesForDisplay(c.Controls, adjust);
                }
            }
        }

   


        /// <summary>
        /// Tells whether a row should be display or not within a certain list-type
        /// </summary>
        /// <param name="r"></param>
        /// <param name="listType"></param>
        /// <returns></returns>
        public static bool FilterRow(DataRow r, string listType) {
            if (r == null) return false;
            if (r.RowState == DataRowState.Deleted) return false;
            var T = r.Table;
                        
            var filter = T.getFilterFunction();
            if (filter== null) return true;
                
            return filter(r, listType);
        }

        static DataTable getFilteredTable(DataTable T, string listType) {
            var filteredhandle = metaprofiler.StartTimer("GetFilteredTable");
            //MarkEvent("Get Filtered table of "+T.TableName+"."+list_type);
            var parentRelation = (string) T.ExtendedProperties["ParentRelation"];
            var filter = T.getFilterFunction();
            if ((T.ExtendedProperties["gridmaster"] == null) &&
                (filter == null) &&
                (parentRelation == null)) {
                metaprofiler.StopTimer(filteredhandle);
                return T;
            }

            if (T.ExtendedProperties["FilteredTables"] == null) {
                T.ExtendedProperties["FilteredTables"] = new Hashtable();
            }
            var filteredTables = (Hashtable) T.ExtendedProperties["FilteredTables"];
            var newDs = new DataSet("temp") {
                EnforceConstraints = false
            };
            var newDt = T.Clone();
            foreach (var k in T.ExtendedProperties.Keys)
                newDt.ExtendedProperties[k] = T.ExtendedProperties[k];
            newDs.Tables.Add(newDt);
            filteredTables[listType] = newDt;
            var t2 = newDt;
            t2.BeginLoadData();
            staticModel.clear(t2); //T2.Clear();

            //if (ParentRelation!=null){
            var childRows = T.Select(parentRelation, T.getSorting());
            foreach (var r in childRows) {
                if ((filter != null) && (!filter(r, listType))) continue;
                var newRow = t2.NewRow();
                foreach (DataColumn c in T.Columns) {
                    newRow[c.ColumnName] = r[c.ColumnName];
                }
                t2.Rows.Add(newRow);
                //NewRow.AcceptChanges();
            }
            //}
            t2.EndLoadData();
            t2.AcceptChanges();
            t2.ExtendedProperties["UnfilteredTable"] = T;
            metaprofiler.StopTimer(filteredhandle);
            return t2;
        }

        /// <summary>
        /// Sets the current selected row of a grid
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r">Row that must become the current row of the grid.</param>
        public void SetGridCurrentRow(DataGrid g, DataRow r) {
            if (r == null) return;
            if (r.RowState == DataRowState.Deleted) return;

            var dsv = (DataSet) g?.DataSource;
            var tv = dsv?.Tables[g.DataMember];
            if (tv == null) return;
            if (tv.Rows.Count == 0) return;

            var drv =  (DataRowView) g.BindingContext[dsv, tv.TableName].Current;
            if (drv == null) return;

            var dv = drv.DataView;

            var rk = QueryCreator.WHERE_KEY_CLAUSE(r, DataRowVersion.Default, false);
            if ((rk == "") || (rk == null)) return;

            if (dv.Sort == "") {
                ClearSelection(g);
                var count = -1;
                for (var index = 0; index < tv.Rows.Count; index++) {
                    if (tv.Rows[index].RowState == DataRowState.Deleted) continue;
                    count++;
                    var rFk2 = QueryCreator.WHERE_KEY_CLAUSE(tv.Rows[index],DataRowVersion.Default, false);
                    if (rFk2 == rk) {
                        g.CurrentRowIndex = count;
                        GridSelectRow(g, count);
                        //G.Select(count);
                        return;
                    }
                }
                return;
            }


            var found = r.Table.Select(dv.RowFilter,
                dv.Sort, dv.RowStateFilter);
            if (found.Length == 0) return;


            var i = 0;
            foreach (var rf in found) {
                var rFk = QueryCreator.WHERE_KEY_CLAUSE(rf, DataRowVersion.Default, false);
                if (rFk == rk) {
                    ClearSelection(g);
                    g.CurrentRowIndex = i;
                    GridSelectRow(g, i);
                    //G.Select(i);
                    return;
                }
                i++;
            }

        }



        #endregion


        #region Gestione ListView



        #endregion

        #region iteratori vari

        void iterateSetMainManagers(IEnumerable cs, SetMainManagerDelegate mainManager) {
            foreach (Control c in cs) {
                if (c is DataGrid ||
                    c is TreeView) {
                    setMainManager(c, mainManager);
                }
                if (c.HasChildren) {
                    iterateSetMainManagers(c.Controls, mainManager);
                }
            }
        }

        /// <summary>
        /// Apply a method on each control of the form
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="apply"></param>
        public void IterateControls(Control.ControlCollection cs, ApplyOnControl apply) {
            foreach (Control c in cs) {
                apply(c);
                if (c.HasChildren) {
                    IterateControls(c.Controls, apply);
                }
            }
        }


        private void iterateControlsSelList(IEnumerable cs, ApplyOnControlSelList apply,
            List<SelectBuilder> selList) {
            foreach (Control c in cs) {
                apply(c, selList);
                if (c.HasChildren) {
                    iterateControlsSelList(c.Controls, apply, selList);
                }
            }
        }

        void iterateControlsSelListName(IEnumerable cs, ApplyOnControlTableSelList apply,
            string tableName, List<SelectBuilder> selList) {
            foreach (Control c in cs) {
                apply(c, tableName, selList);
                if (c.HasChildren) {
                    iterateControlsSelListName(c.Controls, apply, tableName, selList);
                }
            }
        }

        #endregion

        #region Aggiunta Eventi al Form

        /// <summary>
        /// Delegate for using with SetMainManagers function
        /// </summary>
        public delegate void SetMainManagerDelegate(Control C);

        DateTime _lastGridClick;

        static void addNavigatorEventToGrid(DataGrid G) {
            if (G?.Tag == null) return;
            if (!G.Tag.ToString().StartsWith("TreeNavigator")) return;

            var f = G.FindForm();
            if (f == null) return;
            //var m = MetaData.GetMetaData(f);
            //if (m == null) return;
            var h = f.getInstance<IHelpForm>();

            if (G.TableStyles.Count == 1) {
                var dgt = G.TableStyles[0];
                foreach (DataGridColumnStyle dgc in dgt.GridColumnStyles) {
                    if(!(dgc is DataGridTextBoxColumn dgtbc))  continue;
                    dgtbc.TextBox.DoubleClick += h.txtDoubleClick;
                    dgtbc.TextBox.MouseDown += h.txtMouseDown;
                }

            }

        }

        private void helpForm_MouseDown(object sender, MouseEventArgs e) {
            if (destroyed) return;
            if (!(sender is DataGrid)) return;
            _lastGridClick = DateTime.Now;

        }

        /// <summary>
        /// Invoked on grid textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void txtDoubleClick(object sender, EventArgs e) {
            if (destroyed) return;
            var T = sender as TextBox;
            if(!(T?.Parent is DataGrid g))
                return;
            NavigatorDoubleClick(g, null);
        }

        /// <summary>
        /// Invoked on grid txt mousedown 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void txtMouseDown(object sender, MouseEventArgs e) {
            if (destroyed) return;
            if (DateTime.Now >= _lastGridClick.AddMilliseconds(SystemInformation.DoubleClickTime)) return;
            var T = sender as TextBox;
            if(!(T?.Parent is DataGrid g))
                return;
            NavigatorDoubleClick(g, null);
        }



        /// <summary>
        /// Call MainManager Delegate for an entire form. MainManager function is called
        ///  for every TreeNavigator control, and for every control linked to PrimaryTable
        /// </summary>
        /// <param name="f"></param>
        /// <param name="mainManager"></param>
        public void SetMainManagers(Form f, SetMainManagerDelegate mainManager) {
            iterateSetMainManagers(f.Controls, mainManager);
        }

        void setMainManager(Control c, SetMainManagerDelegate mainManager) {
            if (c.Tag == null) return;
            var tag = GetStandardTag(c.Tag);
            if (tag == null) return;
            if (tag.StartsWith("TreeNavigator")) {
                mainManager(c);
            }
            var table = GetTableName(tag);
            if (table == null) return;
            if (table != primaryTable.TableName) return;
            mainManager(c);
            c.TabStop = false;
        }
        
        public class gridTableManager {
	        private DataTable t;
	        private DataGrid g;
	        private bool applied = false;
	        public gridTableManager(IMetaModel model, DataTable t, DataGrid g) {
		        this.t = t;
		        this.g = g;		        
		        model.setAction(t, TableAction.beginLoad, beginLoadGridTable, true);
		        model.setAction(t, TableAction.endLoad, endLoadGridTable, true);
                model.setAction(t, TableAction.startClear, beginClearAction, true);
                model.setAction(t, TableAction.endClear, endClearAction, true);               
            }
            static IMetaModel staticmodel = MetaFactory.factory.getSingleton<IMetaModel>();

            void beginClearAction(DataTable T) {
                var G = FormController.GetLinkedGrid(T);
                if (G != null) {
                    if (T.Rows.Count > 1 && G.DataSource == T.DataSet) {
                        try {
                            G.CurrentRowIndex = 0;
                        }
                        catch (Exception E) {
                            ErrorLogger.Logger.markException(E, "MyClear(T)");
                        }
                    }
                    G.SuspendLayout();
                }
            }

            void endClearAction(DataTable T) {
                g.ResumeLayout();
            }

            void beginLoadGridTable(DataTable T) {
		        if (applied) return;
		        if (g.DataSource !=t.DataSet) return;
		        if (g.DataMember != t.TableName) return;
		        if (t != T) return;
                //g.BeginInit();
                //g.SuspendLayout();
                //g.SetDataBinding(null,null);

                g.DataSource = null;
                g.DataBindings.Clear();
                applied = true;
	        }

	        void endLoadGridTable(DataTable T) {
		        if (!applied) return;
		        if (t != T) return;
                //g.EndInit();
                g.SetDataBinding(t.DataSet, t.TableName);
                //g.ResumeLayout();
               
                applied = false;
	        }
        }


        public class comboTableManager {
	        private DataTable t;
	        private ComboBox c;
	        private bool applied = false;
	        public comboTableManager(IMetaModel model, DataTable t, ComboBox c) {
		        this.t = t;
		        this.c = c;
		        
		        model.setAction(t, TableAction.beginLoad, beginLoadComboTable,true);
		        model.setAction(t, TableAction.endLoad, endLoadComboTable,true);
	        }

	        void beginLoadComboTable(DataTable T) {
		        if (applied) return;
		        if (c.DataSource !=t) return;
		        if (t != T) return;
                c.BeginUpdate();
		        //displayMember = c.DisplayMember;
		        //valueMember = c.ValueMember;
		        //c.DataSource = null;
		        //c.DataBindings.Clear();
		        applied = true;
	        }

	        void endLoadComboTable(DataTable T) {
		        if (!applied) return;
		        if (t != T) return;
		        //c.DataSource = t;
		        //if (c.DisplayMember != displayMember) c.DisplayMember = displayMember;
		        //if (c.ValueMember != valueMember) c.ValueMember = valueMember;
                c.EndUpdate();
		        applied = false;
	        }
        }
        public class listBoxTableManager {
	        private DataTable t;
	        private ListBox c;
	        private bool applied = false;
	        public listBoxTableManager(IMetaModel model, DataTable t, ListBox c) {
		        this.t = t;
		        this.c = c;

                model.setAction(t, TableAction.beginLoad, beginLoad, true);
                model.setAction(t, TableAction.endLoad, endLoad, true);
	        }

	        void beginLoad(DataTable T) {
		        if (applied) return;
		        if (c.DataSource !=t) return;
		        if (t != T) return;
                c.BeginUpdate();
				//c.SuspendLayout();
		        //displayMember = c.DisplayMember;
		        //valueMember = c.ValueMember;
		        //c.DataSource = null;
		        //c.DataBindings.Clear();
		        applied = true;
	        }

	        void endLoad(DataTable T) {
		        if (!applied) return;
		        if (t != T) return;
                c.EndUpdate();
		        //c.DataSource = t;
		        //if (c.DisplayMember != displayMember) c.DisplayMember = displayMember;
		        //if (c.ValueMember != valueMember) c.ValueMember = valueMember;
          //      c.ResumeLayout();
		        applied = false;
	        }
        }



        /// <summary>
        /// Add standard events to a control
        /// </summary>
        /// <param name="c"></param>
        public void AddEvents(Control c) {
            var grid = c as DataGrid;
            if (grid != null) {
	            var t = grid.DataSource as DataTable;
	            if (t != null) {
		            new gridTableManager(model, t, grid);
	            }
                grid.ContextMenu = ExcelMenu;
            }

            if (c is ListBox listBox) {
	            var t = listBox.DataSource as DataTable;
	            if (t != null) {
		            new listBoxTableManager(model, t, listBox);
	            }
            }

            if(c is TextBox textBox) {
                textBox.GotFocus += helpFormTextBox_GotFocus;
                textBox.LostFocus += helpFormTextBox_LostFocus;
                textBox.Enter += helpFormTextBox_Enter;
                textBox.Leave += helpFormTextBox_Leave;
                textBox.TextChanged += helpFormTextBox_TextChanged;
                textBox.ReadOnlyChanged += helpFormTextBox_ReadOnlyChanged;
            }

            var tag = GetStandardTag(c.Tag);
            if (tag == null) return;
            if ((tag.StartsWith("TreeNavigator")) &&
                (c is DataGrid)) {
                var g = (DataGrid) c;
                g.ReadOnly = true;
                g.AllowNavigation = false;
                g.CurrentCellChanged += NavigatorChanged;
                g.DoubleClick += NavigatorDoubleClick;
                g.MouseDown += helpForm_MouseDown;
                g.MouseUp += HelpForm_MouseUp;
                g.KeyUp += HelpForm_KeyUp;

            }


            var table = GetTableName(tag);
            if (table == null) return;
            if (DS.Tables[table] == null) {
                //MetaFactory.factory.getSingleton<IMessageShower>().Show("Tabella "+table+" non trovata nel DataSet ("+C.Name+")");
                return;
            }
            
            if (grid != null) {
                grid.CurrentCellChanged += ControlChanged;
                grid.DataSourceChanged += ControlChanged;
                grid.MouseDown += helpForm_MouseDown;
                grid.MouseUp += HelpForm_MouseUp;
                grid.KeyUp += HelpForm_KeyUp;
                return;
            }
            if (c is TreeView) {
                ((TreeView) c).AfterSelect += treeControlChanged;
                return;
            }
            var column = GetColumnName(tag);
            if (column == null) return;
            if (DS.Tables[table].Columns[column] == null) {
                //MetaFactory.factory.getSingleton<IMessageShower>().Show("Colonna "+table+"."+column+" non trovata nel DataSet ("+C.Name+")");
                return;
            }

            if(c is ComboBox comboBox) {
	            var t = comboBox.DataSource as DataTable;
	            if (t != null) {
		            new comboTableManager(model, t, comboBox);
	            }
	            
                comboBoxManager.addEvents(comboBox);
                comboBox.SelectedIndexChanged += ControlChanged;
                return;
            }

            if(c is TextBox box) {
                alignTextBox(box, DS.Tables[table].Columns[column].DataType);

                if(DS.Tables[table].Columns[column].DataType.Name == "Decimal") {
                    box.GotFocus += EnterDecTextBox;
                    box.LostFocus += GeneralLeaveTextBox;
                    return;
                }
                if(DS.Tables[table].Columns[column].DataType.Name == "Int32") {
                    box.LostFocus += GeneralLeaveTextBox;
                    return;
                }
                if(DS.Tables[table].Columns[column].DataType.Name == "Int16") {
                    box.LostFocus += GeneralLeaveTextBox;
                    return;
                }

                if(DS.Tables[table].Columns[column].DataType.Name == "Double") {
                    box.GotFocus += EnterNumTextBox;
                    box.LostFocus += GeneralLeaveTextBox;
                    return;
                }

                if(DS.Tables[table].Columns[column].DataType.Name == "DateTime") {
                    //                    ((TextBox)C).Leave += new System.EventHandler(LeaveDateTimeTextBox);
                    box.LostFocus += GeneralLeaveDateTextBox;
                    return;
                }
            }

        }





        private void helpFormTextBox_Enter(object sender, EventArgs e) {
            if (destroyed) return;
            if(!(sender is TextBox T))
                return;
            if (T.ReadOnly) return;
            T.ForeColor = formcolors.TextBoxEditingForeColor();
            T.BackColor = formcolors.TextBoxEditingBackColor();
        }

        /// <summary>
        /// Last TextBox modified by user
        /// </summary>
        private TextBox LastTextBoxChanged;

        /// <summary>
        /// Last manually modified textbox
        /// </summary>
        public virtual TextBox lastTextBoxChanged {
            get { return LastTextBoxChanged; }
            set { LastTextBoxChanged = value; }
        }

        private void helpFormTextBox_TextChanged(object sender, EventArgs e) {
            if (destroyed) return;
            if(!(sender is TextBox T)) return;
            if (T.ReadOnly) return;
            LastTextBoxChanged = T;
        }



        private void helpFormTextBox_Leave(object sender, EventArgs e) {
            if (destroyed) return;
            if(!(sender is TextBox T)) return;
            if (T.IsDisposed) return;
            if (T.ReadOnly) return;
            T.ForeColor = formcolors.TextBoxNormalForeColor();
            T.BackColor = formcolors.TextBoxNormalBackColor();
        }

        private void helpFormTextBox_GotFocus(object sender, EventArgs e) {
            if (destroyed) return;
            if(!(sender is TextBox T)) return;
            if (T.ReadOnly) return;
            T.ForeColor = formcolors.TextBoxEditingForeColor();
            T.BackColor = formcolors.TextBoxEditingBackColor();
        }

        private void helpFormTextBox_LostFocus(object sender, EventArgs e) {
            if (destroyed) return;
            if(!(sender is TextBox T)) return;
            if (T.IsDisposed) return;
            if (T.ReadOnly) return;
            T.ForeColor = formcolors.TextBoxNormalForeColor();
            T.BackColor = formcolors.TextBoxNormalBackColor();
        }

        private void helpFormTextBox_ReadOnlyChanged(object sender, EventArgs e) {
            if (destroyed) return;
            if(!(sender is TextBox T))
                return;
            if (T.IsDisposed) return;
            if (T.ReadOnly || (T.Enabled == false)) {
                T.ForeColor = formcolors.TextBoxReadOnlyForeColor();
                T.BackColor = formcolors.TextBoxReadOnlyBackColor();
            }
            else {
                T.ForeColor = formcolors.TextBoxNormalForeColor();
                T.BackColor = formcolors.TextBoxNormalBackColor();
            }
        }


        /// <summary>
        /// Add Helpform events to form controls (Buttons click, grid click,
        ///  combobox slection changed and so on)
        /// </summary>
        /// <param name="f"></param>
        public void AddEvents(Form f) {
            f.Resize += f_Resize;
            IterateControls(f.Controls, AddEvents);
        }
        
        private void f_Resize(object sender, EventArgs e) {
            if (destroyed) return;
            if (sender is Form f) {
	            refreshAllGrids(f.Controls);
            };
            

        }

        void refreshAllGrids(IEnumerable cs) {
            if (destroyed) return;
            foreach (Control c in cs) {
                if (c.HasChildren) refreshAllGrids(c.Controls);
                if (c is DataGrid) {
                    var g = c as DataGrid;
                    g.Refresh();
                }

            }
        }



        #endregion


        #region Gestione DataGrid Navigator

        /// <summary>
        /// Points to primary table datagrid/treeview, if any is present
        /// </summary>
        public Control MainTableSelector;

        void fillNavigator(DataGrid Nav) {
            try {
                //Scroll bars are going bad, so I remove the Suspend/Resume Layout...
                Nav.SuspendLayout();
                fillNavigator2(Nav);
                Nav.ResumeLayout();
            }
            catch (Exception E) {
                //QueryCreator.ShowException(Nav.FindForm(), null, E);
                MetaFactory.factory.getSingleton<IMessageShower>().Show(Nav.FindForm(), E.Message);
            }

        }


        /// <summary>
        /// Fills grid in order to follow selected tree node changes
        /// </summary>
        /// <param name="Nav"></param>
        void fillNavigator2(DataGrid Nav) {
            if (eventManager.AutoEventEnabled == false) return;

            if (!(MainTableSelector is TreeView)) return;
            TreeViewManager.setNavigator(primaryTable, Nav);

            SetGridStyle(Nav, primaryTable);


            if (Nav.DataSource == null) {
                var dst = new DataSet("temp");
                var dtt = primaryTable.Clone();
                dst.Tables.Add(dtt);
                Nav.SetDataBinding(dst, dtt.TableName);
                Nav.ReadOnly = true;
                //parent of selected node, and of other nodes in datagrid
                dtt.ExtendedProperties["parentnode"] = null;
            }
            var myDs = (DataSet) Nav.DataSource;
            var myDt = myDs.Tables[Nav.DataMember];
            var prevParent = (TreeNode) myDt.ExtendedProperties["parentnode"];


            TreeNode n = ((TreeView) MainTableSelector).SelectedNode;
            if (n == null) return;
            int selindex = -1;
            var isNotRoot = n.Parent != null;
            var isLeaf = n.Nodes.Count == 0;
            var hasDummyChild = (n.Nodes.Count == 1) && (n.Nodes[0].Tag == null);
            if (isNotRoot &&
                (isLeaf || hasDummyChild)) {
                selindex = n.Parent.Nodes.IndexOf(n);
                n = n.Parent;
                if (prevParent == n) {
                    eventManager.DisableAutoEvents();
                    try {
                        Nav.UnSelect(Nav.CurrentRowIndex);
                        Nav.CurrentRowIndex = selindex;
                        Nav.Select(selindex);
                    }
                    catch (Exception E) {
                        logException(Nav.FindForm(), "Fillnavigator2_bis", E);
                        ;
                    }
                    eventManager.EnableAutoEvents();
                    return;
                }
            }
            if (n == null) return;


            eventManager.DisableAutoEvents();
            model.clear(myDt); // myDT.Clear();
            Nav.Update();
            //EnableAutoEvents(); //it was here

            //TreeViewManager TM = TreeViewManager.GetManager(primaryTable);
            foreach (TreeNode child in n.Nodes) {
                if (child.Tag == null) continue;
                var tn = (tree_node) child.Tag;
                var r = tn.Row;
                if (r.RowState == DataRowState.Detached || r.RowState==DataRowState.Deleted) continue;
                var newRow = myDt.NewRow();
                foreach (DataColumn c in myDt.Columns) {
                    newRow[c.ColumnName] = r[c.ColumnName];
                }
                myDt.Rows.Add(newRow);
                newRow.AcceptChanges();
            }

            var format = new formatgrids(Nav);
            format.AutosizeColumnWidth();

            eventManager.EnableAutoEvents(); //it was upper

            myDt.ExtendedProperties["parentnode"] = null;
            if (selindex != -1) {
                try {
                    eventManager.DisableAutoEvents();
                    try {
                        Nav.UnSelect(Nav.CurrentRowIndex);
                        Nav.CurrentRowIndex = selindex;
                        Nav.Select(selindex);
                    }
                    catch {
                        //ignore
                    }

                    eventManager.EnableAutoEvents();
                    myDt.ExtendedProperties["parentnode"] = n;
                }
                catch (Exception e) {
                    shower.ShowException(Nav.FindForm(), null, e);                    
                }
            }
            else {
                eventManager.DisableAutoEvents();
                try {
                    Nav.CurrentRowIndex = 0;
                }
                catch (Exception e) {
                    logException(Nav.FindForm(), "Fillnavigator2", e);
                }
                if (myDt.Rows.Count > 0) Nav.Select(0);
                eventManager.EnableAutoEvents();
            }
        }

        /// <summary>
        /// Sostituisce ogni  (LT)% Form[ControlName] %> con 
        /// QueryCreator.quotedstrvalue(valore,true) ove valore è
        ///  il valore del controllo di nome ControlName. Per i ComboBox  è considerato
        ///  il SelectedValue.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public string CompileFormFilter(Form f, string s) {
            //if (s == null) return null;
            //var next = s.IndexOf("<%Form[", 0);
            return s;
        }

        /// <summary>
        /// Event fired when a navigator is double clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NavigatorDoubleClick(object sender, EventArgs e) {
            if (destroyed) return;
            if (eventManager.AutoEventEnabled == false) return;
            if (!(MainTableSelector is TreeView)) return;
            var TV = (TreeView) MainTableSelector;
            var N = TV.SelectedNode;
            if (N == null) return;
            var tn = (tree_node) N.Tag;

            var g = (DataGrid) sender;
            var myDS = (DataSet) g.DataSource;
            var myDT = myDS.Tables[g.DataMember];
            if (myDT.ExtendedProperties["parentnode"] != null) {
                if (N.Nodes.Count == 0) {
                    FormController.MainSelect(TV.FindForm());
                    return;
                }
                if (N.Nodes[0].Tag == null) return;
                N.Expand();
                fillNavigator((DataGrid) sender);
                return;
            }
            N.Expand();
            int sel = g.CurrentRowIndex;
            if (sel == -1) return;
            var selectedRow = getCurrNavRow(g);
            if (selectedRow != null) {
                SetLastSelected(selectedRow.Table, selectedRow);
                var tm = TreeViewManager.GetManager(selectedRow.Table);
                tm.SelectNode(selectedRow);
            }

            TV.SelectedNode.Expand();

//			if (N.Nodes.Count<=sel) return;
//			TV.SelectedNode= N.Nodes[sel];
//			tree_node newselected_node= (tree_node) (TV.SelectedNode.Tag);
//			if (newselected_node!=null){
//				DataTable T = newselected_node.Row.Table;
//				SetLastSelected(T, newselected_node.Row);
//			}
//			TV.SelectedNode.Expand();
        }

        DataRow getCurrNavRow(DataGrid g) {
            var sel = g.CurrentRowIndex;
            if (sel == -1) return null;

            var dsv = (DataSet) g.DataSource;
            DataTable tv = dsv?.Tables[g.DataMember];
            if (tv == null) return null;

            if (tv.Rows.Count == 0) return null;
            DataRowView dv;
            try {
                dv = (DataRowView) g.BindingContext[dsv, tv.TableName].Current;
            }
            catch {
                dv = null;
            }
            if (dv == null) return null;

            var selectedRow = dv.Row;
            
            var selectedTable = selectedRow.Table;
            var key = QueryCreator.WHERE_KEY_CLAUSE(selectedRow, DataRowVersion.Default, false);
            var T = DS.Tables[selectedTable.TableName];
            var myRows = T.Select(key);
            return myRows.Length == 1 ? myRows[0] : null;
        }

        /// <summary>
        /// Event fired where the selection of a Navigator changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NavigatorChanged(object sender, EventArgs e) {
            if (destroyed) return;
            var nav = (DataGrid) sender;
            try {
                //Scroll bars are going bad, so I remove the Suspend/Resume Layout...
                nav.SuspendLayout();
                NavigatorChanged2(sender, e);
                nav.ResumeLayout();
            }
            catch (Exception E) {
                shower.ShowException(nav.FindForm(), null, E);
                //MetaFactory.factory.getSingleton<IMessageShower>().Show(Nav.FindForm(), E.Message);
            }
        }

        /// <summary>
        /// Internal event called by NavigatorChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NavigatorChanged2(object sender, EventArgs e) {
            if (destroyed) return;
            if (eventManager.AutoEventEnabled == false) return;
            DataGrid Nav = (DataGrid) sender;
            DataSet myDS = (DataSet) Nav.DataSource;
            DataTable myDT = myDS.Tables[Nav.DataMember];
            if (!(MainTableSelector is TreeView)) return;
            var tv = (TreeView) MainTableSelector;
            var parent = (TreeNode) myDT.ExtendedProperties["parentnode"];
            if (parent == null) {
                var curr = ((TreeView) MainTableSelector).SelectedNode;
            }


            var currNav = getCurrNavRow(Nav);
            if (currNav == null) return;
            var tm = TreeViewManager.GetManager(currNav.Table);

            try {
                eventManager.DisableAutoEvents();
                tm.SelectNode(currNav);
                myDT.ExtendedProperties["parentnode"] = tv.SelectedNode.Parent;
                tree_node n = (tree_node) tv.SelectedNode.Tag;
                SetLastSelected(currNav.Table, currNav);
                eventManager.EnableAutoEvents();
            }
            catch (Exception ex) {
                shower.ShowException(Nav.FindForm(), null, ex);
            }



        }


        #endregion


        #region Gestione Control Changed / Last Selected Row / Table Monitored

        /// <summary>
        /// Table to which belongs LastSelectedRow 
        /// </summary>
        string _tableToMonitor;

        ///// <summary>
        ///// Sets the table that will be used for returning the selected row
        ///// </summary>
        ///// <param name="tablename"></param>
        //private void SetTableToMonitor(string tablename) {
        //    _tableToMonitor = tablename;
        //    if (DS?.Tables[tablename] == null) return;
        //    SetLastSelected(DS.Tables[tablename], null);
        //}

        /// <summary>
        /// Last selected row in TableToMonitor DataTable
        /// </summary>
        public DataRow LastSelectedRow {
            get {
                if (_tableToMonitor == null) return null;
                if (DS?.Tables[_tableToMonitor] == null) return null;
                return GetLastSelected(DS.Tables[_tableToMonitor]);
            }
        }

        /// <summary>
        /// Keeps the last selected row of a Table in an extended properties of the Table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="r"></param>
        public static void SetLastSelected(DataTable T, DataRow r) {
            if (T == null) return;
            T.ExtendedProperties["LastSelectedRow"] = r;
        }

        /// <summary>
        /// Get Last Selected Row in a specified DataTable
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static DataRow GetLastSelected(DataTable T) {
            var r = (DataRow) T?.ExtendedProperties["LastSelectedRow"];
            if (r == null) return null;
            if (r.RowState == DataRowState.Deleted) return null;
            if (r.RowState == DataRowState.Detached) return null;
            return r;
        }

        /// <summary>
        /// Invoked when any DataRow selection control is changed (grid, combo, listview..)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ControlChanged(object sender, System.EventArgs e) {
            if (destroyed) return;
            extendedControlChanged(sender, e, null);
        }

        /// <summary>
        /// Called whenever the selection of a combobox, datagrid or treeview changes.
        /// Sets LastSelectedRow of the Table. Further, if table is not primary, every 
        ///  table's control in the same box as sender is refilled.
        ///  If changed row belongs to primary table, a DO_GET(false, RowChanged) is
        ///   performed and form refilled.
        ///  Also calls AfterRowSelect of the linked form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="defaultRow">Row used as changed row where it can't be sorted out from sender</param>
        public void extendedControlChanged(object sender, EventArgs e, DataRow defaultRow) {
            if (destroyed) return;
            if (!eventManager.AutoEventEnabled) return;
            if (sender == null) {
                //MarkEvent("Control Changed (null)");
                return;
            }

            //if (getd.PrimaryDataTable.Rows.Count!=1) return;
            var c = (Control) sender;

            var res = GetCurrentRow(c, out var changed, out var rowChanged);
            if (!res) return;
            if (changed == null) return; //should not happen

            if (c.GetType() == typeof(ComboBox)) {
                additionalInfo[c.Name] = comboBoxManager.comboTip((ComboBox) c);
                setToolTip(c);
            }

            if (rowChanged != null && rowChanged.RowState == DataRowState.Detached) rowChanged = null;
            if (rowChanged == null) rowChanged = defaultRow;

            var changehandle = metaprofiler.StartTimer("Inside ControlChange()");
            SetLastSelected(changed, rowChanged);
            BeforeRowSelect?.Invoke(changed, rowChanged);

            //IterateFillRelatedControls(C.FindForm().Controls, C, Changed, index); 
            if (changed.TableName != primaryTable.TableName) {
                if (c.Parent != null)
                    IterateFillRelatedControls(c.Parent.Controls, c, changed, rowChanged);
            }
            else {
                var cr = new crono("CC");
                var ctrl = FormController.GetController(c.FindForm());
                eventManager.dispatch(new StartMainRowSelectionEvent(rowChanged));
                if (ctrl != null) {
                    //TODO: remove this cyclic dependency
                    ctrl.DO_GET(false, rowChanged); //fresh peripherals table, not entity tables
                    ctrl.FreshForm(false, false);

                }
                eventManager.dispatch(new StopMainRowSelectionEvent(rowChanged));
                FormController.setLastLoadTime(cr.GetDuration());

            }
            metaprofiler.StopTimer(changehandle);

            AfterRowSelect?.Invoke(changed, rowChanged);
        }
        
       

        private void treeControlChanged(object sender, TreeViewEventArgs e) {
            if (destroyed) return;
            ControlChanged(sender, null);
        }

        #endregion


        #region gestione treeview


        /// <summary>
        /// Data of the tree has already been retrieved.
        /// In this case, the tree refers to primary DataTable, and should be
        ///  displayed in a LIST-type form.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="tableName"></param>
        public void DisplayTree(TreeView c, string tableName) {
            if (tableName == primaryTable.TableName) return;

            DataTable T = DS.Tables[tableName];
            TreeViewManager tm = TreeViewManager.GetManager(T);
            tm?.FillNodes();
        }

        /// <summary>
        /// Fill a treeview. If SetFilterTree has been called, the nodes are taken
        ///  from Extended property (not from DB) ex SetTreeByStart
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="rootfilterSql"></param>
        /// <param name="clear"></param>
        /// <returns></returns>
        public bool StartTreeView(TreeView treeView, string rootfilterSql, bool clear) {
            string tname = GetField(treeView.Tag.ToString(), 0);
            DataTable T = DS.Tables[tname];
            if (T == null) return false;

            var tm = TreeViewManager.GetManager(T);
            if (tm == null) return false;
            tm.Start(rootfilterSql, clear);



            tm.FillNodes();
            if (T.Rows.Count == 0) return false;

            return true;
        }



        /// <summary>
        /// Fills a tree given a start condition. Also Accepts FilterTree
        /// </summary>
        /// <param name="C"></param>
        /// <param name="startCondition"></param>
        /// <param name="startValueWanted"></param>
        /// <param name="startFieldWanted"></param>
        /// <returns></returns>
        public bool SetTreeByStart(TreeView C, string startCondition,
            string startValueWanted,
            string startFieldWanted) {
            //MarkEvent("SetTreeByStart START");
            string tname = GetField(C.Tag.ToString(), 0);
            DataTable T = DS.Tables[tname];
            if (T == null) return false;


            var tm = TreeViewManager.GetManager(T);
            eventManager.DisableAutoEvents();
            //era DataRow R = getd.GetSpecificChild(T, StartCondition, StartValueWanted, StartFieldWanted);
            var selected = tm.startWithField(startCondition, startValueWanted, startFieldWanted);
            if (selected == null) {
                eventManager.EnableAutoEvents();
                return false;
            }
            
            SetLastSelected(T, selected);
            
            eventManager.EnableAutoEvents();


            C.Select();
            C.Focus();

            return true;
        }

        #endregion


        //Note that a control is filled if 
        // - it belongs to primary table
        // - it belongs to a table that contains only a row

        #region Set Controls (Fill)


        /// <summary>
        /// Fills a set of controls (with childs)
        /// </summary>
        /// <param name="Cs"></param>
        public void FillControls(Control.ControlCollection Cs) {
            iterateFillControls(Cs);
        }

        /// <summary>
        /// Fills all textbox, checkboxes, radiobuttons and comboboxes of the form linked
        ///  to the primary table.
        /// </summary>
        /// <param name="F">Form to Fill</param>
        public void FillControls(Form F) {
	        controller = FormController.GetController(F);
            //MarkEvent("FillControls (F,bool) called on "+F.Text);
            iterateFillControls(F.Controls);
        }

        /// <summary>
        /// Get a list of controls, putting parent-controls before childs
        /// </summary>
        /// <param name="Cs"></param>
        /// <returns></returns>
        List<Control> getSortedControlList(Control.ControlCollection Cs) {
            int handle = metaprofiler.StartTimer("GetSortedControlList");
            List<Control> L = new List<Control>(Cs.Count);
            //create a list of all control to fill in Cs, putting comboboxes and grids in front of
            // the list (from position 0 to pos. ncombos)
            //Combo are put before datagrids
            int ncombos = 0;
            //int withoutchildrens = 0;
            foreach (Control C in Cs) {
                //Adds those with childrens after all others
                if (C.HasChildren) {
                    L.Add(C);
                    continue;
                }
                if ((C.Tag == null) || (C.Tag.ToString() == "")) continue;
                //withoutchildrens++;

                if ((!typeof(DataGrid).IsAssignableFrom(C.GetType())) &&
                    (!typeof(ComboBox).IsAssignableFrom(C.GetType()))) {
                    //L.Insert(withoutchildrens - 1, C);
                    L.Add(C);
                    continue;
                }
                if (typeof(ComboBox).IsAssignableFrom(C.GetType())) {
                    L.Insert(ncombos, C);
                    ncombos++;
                }
                else {
                    L.Insert(ncombos, C);
                }
            }
            if (ncombos < 2) {
                metaprofiler.StopTimer(handle);
                return L;
            }

            //puts every child-combo after parent-combo
            bool somethingdone = true;
            while (somethingdone) {
                bool mustbreak = false;
                somethingdone = false;
                for (int i = 0; i < ncombos - 1; i++) {
                    var ChildC = (ComboBox) L[i];
                    var ChildET = (DataTable) ChildC.DataSource;
                    if (ChildET == null) continue;
                    var ChildT = DS.Tables[ChildET.TableName];
                    for (int j = i + 1; j < ncombos; j++) {
                        var ParentC = (ComboBox) L[j];
                        var ParentET = (DataTable) ParentC.DataSource;
                        if (ParentET == null) continue;
                        var ParentT = DS.Tables[ParentET.TableName];
                        if (QueryCreator.GetParentChildRel(ParentT, ChildT) != null) {
                            //swap parent and child
                            L[i] = ParentC;
                            L[j] = ChildC;
                            somethingdone = true;
                            mustbreak = true;
                            break;
                        }
                    }
                    if (mustbreak) break;
                }
            }
            metaprofiler.StopTimer(handle);
            return L;
        }

        void iterateFillControls(Control.ControlCollection Cs) {
            var List = getSortedControlList(Cs);
            foreach (Control c in List) {
                int fillhandle = metaprofiler.StartTimer("FillControl(C)");
                FillControl(c);
                metaprofiler.StopTimer(fillhandle);
                if (!c.HasChildren) continue;
                if (isManagedCollection(c)) continue;
                iterateFillControls(c.Controls);
            }
        }



        void setRadioButton(RadioButton C, DataTable Table, string column, object val) {
            string Tag = GetStandardTag(C.Tag);
            int pos = Tag.IndexOf(':');
            string Cvalue = Tag.Substring(pos + 1).Trim();
            if (!Cvalue.StartsWith(":")) {
                string rowvalue = "";
                if (val != null) rowvalue = val.ToString();
                if (rowvalue.Equals(Cvalue))
                    C.Checked = true;
                else
                    C.Checked = false;
            }
            else {
                //E' un campo bit,
                Cvalue = Cvalue.Substring(1);
                bool negato = false;
                if (Cvalue.StartsWith("#")) {
                    Cvalue = Cvalue.Substring(1);
                    negato = true;
                }

                int Nbit = Convert.ToInt32(Cvalue);
                ulong aval = 1;
                aval <<= (Nbit);
                object currval = val;
                if (currval == DBNull.Value) currval = (ulong) 0;
                var X = Convert.ToUInt64(currval);
                bool valore = ((X & aval) == aval);
                if (negato) valore = !valore;
                C.Checked = valore;



            }
            controlEnabler.enableDisable(C, Table, column, drawMode);
        }




        void fillValueSignedGroup(GroupBox g, object o) {
            var T = searchValueTextBox(g);
            if (T == null) return;

            if ((o == null) || (o == DBNull.Value)) {
                T.Text = "";
                return;
            }

            var sign = signOf(o);
            var q = o;
            if (!sign) q = invertSign(q);
            T.Text = HelpUi.StringValue(q, null);
            setSignForValueSigned(g, sign);
        }

        void fillValueSignedGroup(GroupBox g, DataTable T, string col, object o) {
            controlEnabler.reEnable(g);
            fillValueSignedGroup(g, o);
            controlEnabler.enableDisable(g, T, col, drawMode);
        }

      


        private void alignTextBox(TextBox T, Type coltype) {
            if ((typeof(int) == coltype) ||
                (typeof(decimal) == coltype) ||
                (typeof(float) == coltype) ||
                (typeof(double) == coltype) ||
                (typeof(DateTime) == coltype) ||
                (typeof(short) == coltype)) T.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        }

        /// <summary>
        /// Sets the content and the status of a textbox basing on his tag
        /// </summary>
        /// <param name="T"></param>
        /// <param name="table"></param>
        /// <param name="fieldname"></param>
        /// <param name="val"></param>
        public void SetText(TextBox T, DataTable table, string fieldname, object val) {
            //ReEnable(T);           
            string Tag = GetStandardTag(T.Tag);
            Tag = CompleteTag(Tag, table.Columns[fieldname]);

            T.Text = HelpUi.StringValue(val, Tag);
            //AlignTextBox(T,Table.Columns[fieldname].DataType);
            controlEnabler.enableDisable(T, table, fieldname, drawMode);
        }


        void setCheckBox(CheckBox c, DataTable T, string column, object val) {
            c.ThreeState = (T.Columns[column].AllowDBNull &&
                            !T.Columns[column].IsDenyNull()
            ); // ||	(val==DBNull.Value);

            string Tag = GetStandardTag(c.Tag);
            int pos = Tag.IndexOf(':');
            if (pos == -1) return;
            string values = Tag.Substring(pos + 1).Trim();
            if (values.IndexOf(":") == -1) {
                //if (C.CheckState == CheckState.Indeterminate) return; //salta il bit, non ha effetto
                bool negato = false;
                if (values.StartsWith("#")) {
                    negato = true;
                    values = values.Substring(1);
                }
                int Nbit = Convert.ToInt32(values);
                ulong aval = 1;
                aval <<= (Nbit);
                var currval = val;
                if (currval == DBNull.Value) currval = (ulong) 0;
                var X = Convert.ToUInt64(currval);
                var valore = ((X & aval) == aval);
                if (negato) valore = !valore;
                c.ThreeState = false;
                c.Checked = valore;
                c.CheckState = c.Checked ? CheckState.Checked : CheckState.Unchecked;
            }
            else {
                var yvalue = values.Split(new Char[] {':'}, 2)[0].Trim();
                var nvalue = values.Split(new Char[] {':'}, 2)[1].Trim();

                if ((val == null) || (val == DBNull.Value)) {
                    if (c.Visible && T.Columns[column].IsDenyNull())
                        c.CheckState = CheckState.Unchecked;
                    else
                        c.CheckState = CheckState.Indeterminate;
                }
                else {
                    var rowvalue = val.ToString();
                    if (rowvalue.Equals(yvalue)) {
                        c.Checked = true;
                        c.CheckState = CheckState.Checked;
                    }
                    else {
                        c.Checked = false;
                        c.CheckState = CheckState.Unchecked;
                    }
                }
            }
            controlEnabler.enableDisable(c, T, column, drawMode);

        }



        /// <summary>
        /// Fills a control basing on it's tag
        /// </summary>
        /// <param name="c"></param>
        public void FillControl(Control c) {
            Type cType = c.GetType();
            //if (typeof(Label).IsAssignableFrom(C_Type))return;
            if (controller == null) controller =  FormController.GetController(c.FindForm());

			string tag = GetStandardTag(c.Tag);
            if (tag == null) return;
            if (c is Button) {
                setEnableGridButtons((Button) c); //,true
                setMainButtons((Button) c); //,false
                controller.setColor(c);
                return;
            }

            string treetag = GetField(tag, 0);
            if ((treetag.StartsWith("TreeNavigator") && (c is DataGrid))) {
                fillNavigator((DataGrid) c);
                controller.setColor(c);
                return;
            }
            var table = GetTableName(tag);
            if (table == null) return;
            DataTable Table = DS.Tables[table];
            if (Table == null) return;

            //sets & fills datagrid
            if (c is DataGrid) {
                int gridhandle = metaprofiler.StartTimer("SetDataGrid(G)");
                //if (C.Enabled)	
                setDataGrid((DataGrid) c);
                controller.setColor(c);
                metaprofiler.StopTimer(gridhandle);
                return;
            }


            //sets & fills ListView
            if (c is ListView) {
                listViewManager.setListView((ListView) c, primaryTable);
                controller.setColor(c);
                return;
            }

            //sets & fills treeview if table is primary
            if ((table.Equals(primaryTable.TableName)) &&
                (typeof(TreeView).IsAssignableFrom(cType))) {
                DisplayTree((TreeView) c, table);
                controller.setColor(c);
            }

            var column = GetColumnName(tag);
            if (column == null) return;
            if (DS.Tables[table].Columns[column] == null) return;
            var currPrimary = GetLastSelected(primaryTable);
            if (currPrimary?.RowState == DataRowState.Deleted) currPrimary = null;
            DataRow r;
            if((currPrimary == null) || (table == primaryTable.TableName)) {
                r = currPrimary;
            }
            else {
                controlEnabler.enableDisable(c, Table, column, drawMode);
                //if the table is parent or child of primary table, try to draw it
                r = GetCurrChildRow(currPrimary, Table) ?? GetCurrParentRow(currPrimary, Table, column);
                if(r == null) {
                    var nFound = 0;
                    DataRow found = null;
                    //check if it is parent of an extra entity row
                    foreach(var extraName in getExtraEntities()) {
                        var extraTable = DS.Tables[extraName];
                        var extraRel = QueryCreator.GetParentChildRel(primaryTable, extraTable);
                        if(extraRel == null)
                            continue;
                        var childRow = currPrimary.iGetChildRows(extraRel);
                        if(childRow.Length != 1)
                            continue;
                        var toConsider = childRow[0];
                        var r1 = GetCurrParentRow(toConsider, Table, column);
                        if(r1 != null) {
                            nFound++;
                            found = r1;
                        }
                        if(nFound > 1)
                            break;
                    }
                    if(nFound == 1) {
                        r = found;
                    }
                }
            }

            if (r?.RowState == DataRowState.Deleted) r = null;
            if (r == null) {
                clearControl(c);
                controlEnabler.enableDisable(c, Table, column, drawMode);
                //doDisable(C, false);
                return;
            }

            //            //Ancora non testato nemmeno logicamente.
            //            if (typeof(TreeView).IsAssignableFrom(C.GetType())){
            //                SetTree((TreeView) C, R, Insert);
            //                return;
            //            }

            if (c is TextBox) {
                SetText((TextBox) c, Table, column, r[column]);
                controller.setColor(c);

                if (((TextBox) c).Focused && lastTextNoFound == null) {
                    lastTextNoFound = c.Name + "#" + ((TextBox) c).Text;
                }
                return;
            }

            if(c is ComboBox box) {
                comboBoxManager.setCombo(box, Table, column, r[column]);
                controlEnabler.enableDisable(box, Table, column, drawMode);
                if(box.SelectedValue != null) {
                    additionalInfo[box.Name] = "\nValore:" + box.SelectedValue;
                }
                else {
                    additionalInfo[box.Name] = "\nValore: null";
                }
                setToolTip(box);
                controller.setColor(c);
                return;
            }

            if(c is CheckBox checkBox) {
                setCheckBox(checkBox, Table, column, r[column]);
                controller.setColor(c);
                return;
            }

            if(c is RadioButton button) {
                setRadioButton(button, Table, column, r[column]);
                controller.setColor(c);
                return;
            }

            if(c is GroupBox groupBox) {
                if(GetFieldLower(tag, 2) == "valuesigned") {
                    fillValueSignedGroup(groupBox, Table, column, r[column]);
                    //MetaData.SetColor(groupBox);
                    return;
                }
            }

            if(c is Label label) {
                label.Text = HelpUi.StringValue(r[column], null);
                controller.setColor(c);
                return;
            }


        }

        #endregion

        /// <summary>
        /// Hash for the last focused textbox name and content
        /// </summary>        
        /// <inheritdoc />
        public virtual string lastTextNoFound { get; set; }


        #region Prefill Controls

        /// <summary>
        /// Reads some row related to a tree in order to display it at beginning
        /// </summary>
        /// <param name="c">treeView to fill</param>
        /// <param name="filter">filter to apply when getting root nodes</param>
        /// <param name="skipPrimary">if true, no action is done if tree-table is 
        ///		primary table</param>
        public void FilteredPreFillTree(TreeView c, string filter, bool skipPrimary) {
            var table = GetTableName(c.Tag.ToString());
            if (table == primaryTable.TableName) MainTableSelector = c;

            if (skipPrimary && table == primaryTable.TableName) return;
            //can't prefill primary table!!!

            if (table == null) return;
            var T = DS.Tables[table];

            var tm = TreeViewManager.GetManager(T);
            if (tm == null) return;

            //Checks that the table is not a child of another table. Infact in that case,
            // the list will be built depending of the selected row of the other table
            //if ((filter==null) && (T.ParentRelations.Count>0)) return;
            filter = GetData.MergeFilters(filter, tm.RootsCondition_SQL());
            eventManager.DisableAutoEvents();
            StartTreeView(c, filter, true);
            eventManager.EnableAutoEvents();
        }

        ///// <summary>
        ///// Prefill a control. This has sense when control is a combobox, a tree, or a
        /////  datagrid. Otherwise, no action is performed
        ///// </summary>
        ///// <param name="Co"></param>
        //public void PreFillControls(Control Co) {
        //    PreFillControlsTable(Co, null);
        //}

        /// <summary>
        /// Prefill a control, with an optional select list to compile
        /// </summary>
        /// <param name="Co"></param>
        /// <param name="selList"></param>
        public void PreFillControls(Control Co, List<SelectBuilder> selList) {
            PreFillControlsTable(Co, null, selList);
        }

        

      



        /// <summary>
        /// Prefill controls for a specified table
        /// </summary>
        /// <param name="Co"></param>
        /// <param name="tablewanted"></param>
        public void PreFillControlsTable(Control Co, string tablewanted) {
            PreFillControlsTable(Co, tablewanted, null);
        }

        /// <summary>
        /// Set the standard tooltip for a control 
        /// </summary>
        /// <param name="c"></param>
        public void setToolTip(Control c) {
            if (c?.Tag == null) return;
            if (toolTipOnControl) {

                string ss = "Name:" + c.Name + "\nTag:" + c.Tag;
                if (additionalInfo[c.Name] != null) ss += additionalInfo[c.Name].ToString();
                tip.SetToolTip(c, "Name:" + c.Name + "\nTag:" + ss);
            }
        }



        /// <summary>
        /// Some additional information displayed in tooltips
        /// </summary>
        public Hashtable additionalInfo = new Hashtable();

        /// <summary>
        /// Returns additional information on a control given its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string getAdditionalTooltip(string name) {
            return additionalInfo[name] as string;
        }

        /// <summary>
        /// Sets additional information on a control given its name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setAdditionalTooltip(string name, string value) {
            additionalInfo[name] = value;
        }


        /// <summary>
        /// prefill controls of tablewanted (or all if tablewanted is null)
        /// </summary>
        /// <param name="Co"></param>
        /// <param name="tablewanted"></param>
        /// <param name="selList"></param>
        public void PreFillControlsTable(Control Co, string tablewanted, List<SelectBuilder> selList) {

            setToolTip(Co);


            //if(Co is Crownwood.Magic.Controls.TabControl TC) {
            //    var J = Co.FindForm();
            //    TC.Appearance = Crownwood.Magic.Controls.TabControl.VisualAppearance.MultiBox;
            //    TC.PositionTop = true;
            //    TC.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            //    TC.AutoScroll = false;
            //    TC.HotTrack = true;
            //    TC.Font = new Font(FontFamily.GenericSansSerif, 8.25f, GraphicsUnit.Point);
            //}

            if (Co.Tag == null) return;
            string tag = GetStandardTag(Co.Tag);
            string table = GetTableName(tag);
            if (table == null) return;

            var Table = DS.Tables[table];
            if (Table == null) {
                //MarkEvent("Nel dataset non esiste la tabella "+table);
                return;
            }

            if (Co is TextBox T) {
                if ((tablewanted != null) && (table != tablewanted)) return;
                string column = GetColumnName(tag);
                if (column == null) return;
                var C = Table.Columns[column];
                if (C == null) return;
                int maxlen = C.GetMaxLen();
                T.MaxLength = maxlen > 0 ? maxlen : 0;
                return;
            }

            if (Co is ComboBox) {
                var Source = (DataTable) ((ComboBox) Co).DataSource;
                if (Source == null) return;
                if ((Source != null) && (tablewanted != null) && (Source.TableName != tablewanted)) return;
                //Il prefill non deve quasi mai impostare il valore, ma solo la tabella!!!!
                //MarkEvent("To prefill "+Co.Name+"...");
                int handlecombo = metaprofiler.StartTimer("filteredPreFillCombo3 * " + Co.Name);
                //Co.SuspendLayout();
                comboBoxManager.filteredPreFillCombo((ComboBox) Co, null, false, selList);
                //Co.ResumeLayout();
                metaprofiler.StopTimer(handlecombo);
                //MarkEvent(Co.Name+" prefilled.");
                return;
            }

            if (Co is TreeView) {
                if ((tablewanted != null) && (table != tablewanted)) return;
                FilteredPreFillTree((TreeView) Co, null, true);
                if (table.Equals(primaryTable.TableName)) MainTableSelector = Co;
                return;
            }

            //sets & fills datagrid
            if (Co is DataGrid) {
                if ((tablewanted != null) && (table != tablewanted)) return;
                if ((MainTableSelector == null) &&
                    (Table == primaryTable)) MainTableSelector = Co;
                return;
            }

            //sets & fills datagrid
            if (Co is ListView) {
                ListView l = Co as ListView;
                IGetData getData = l.FindForm().getInstance<IGetData>();
                if ((tablewanted != null) && (table != tablewanted)) return;
                listViewManager.prefillListView((ListView) Co, primaryTable, selList, getData);
                return;
            }

        }


        /// <summary>
        /// Prefills a combo with a specified filter, optionally changing its value
        /// </summary>
        /// <param name="C"></param>
        /// <param name="filter"></param>
        /// <param name="freshvalue"></param>
        [Obsolete("Use comboBoxManager.filteredPreFillCombo(comboBoxManager.filteredPreFillCombo(ComboBox, string, bool))")]
        public void FilteredPreFillCombo(ComboBox C, string filter, bool freshvalue) {
            comboBoxManager.filteredPreFillCombo(C, filter, freshvalue, null);
        }


        /// <summary>
        /// Fills a combobox with related data. It adds a dummy empty row to 
        ///  the PARENT table if the master selector allows null. This row
        ///  is marked as temp_row
        /// </summary>        /// <param name="C"></param>
        /// <param name="filter"></param>
        /// <param name="freshvalue"></param>
        /// <param name="selList"></param>
        /// <param name="dmode"></param>
        [Obsolete("Use comboBoxManager.filteredPreFillCombo with same parameters")]
        public void FilteredPreFillCombo(ComboBox C, string filter, bool freshvalue, List<SelectBuilder> selList, HelpForm.drawmode dmode) {
            comboBoxManager.filteredPreFillCombo(C, filter, freshvalue, selList);
        }

        /// <summary>
        /// Fill the table  related to a combobox.
        /// </summary>
        /// <param name="C"></param>
        /// <param name="freshvalue"></param>
        [Obsolete("use comboBoxManager.fillComboBoxTable")]
        public void FillComboBoxTable(ComboBox C, bool freshvalue) {
            comboBoxManager.fillComboBoxTable(C, freshvalue);
        }

        /// <summary>
        /// Re)Enable Automatic Events (i.e. ControlChanged)
        /// </summary>
        [Obsolete("Use eventManager.EnableAutoEvents()")]
        public void EnableAutoEvents() {
            eventManager.EnableAutoEvents();
        }

        /// <summary>
        /// Disable Automatic Events, i.e. ControlChanged Events
        /// </summary>
        [Obsolete("Use eventManager.DisableAutoEvents()")]
        public void DisableAutoEvents() {
            eventManager.DisableAutoEvents();
        }

        /// <summary>
        /// Reimposta il ComboBox C per far si che contenga le voci di search
        /// </summary>
        /// <param name="C"></param>
        /// <param name="tablename"></param>
        /// <param name="dmode"></param>
        [Obsolete("Use comboBoxManager.resetComboBoxSource(C, tablename)")]
        public void ResetComboBoxSource(ComboBox C, string tablename, HelpForm.drawmode dmode) {
            comboBoxManager.resetComboBoxSource(C, tablename);
        }

        /// <summary>
        ///  Reimposta il ComboBox C per far si che contenga le voci di search
        /// </summary>
        /// <param name="C"></param>
        /// <param name="tablename"></param>
        [Obsolete("Use comboBoxManager.resetComboBoxSource(C, tablename)")]
        public void ResetComboBoxSource(ComboBox C, string tablename) {
            comboBoxManager.resetComboBoxSource(C, tablename);
        }

        /// <summary>
        /// Reimposta il ComboBox C per far si che contenga tutte le voci operative in inserimento
        /// </summary>
        /// <param name="C"></param>
        /// <param name="tablename"></param>
        [Obsolete("Use RefilterComboBoxSource(ComboBox C, string tablename)")]
        public void RefilterComboBoxSource(ComboBox C, string tablename) {
            comboBoxManager.refilterComboBoxSource(C, tablename);
        }


        /// <summary>
        /// Used IListViewManager
        /// </summary>
        public IListViewManager listViewManager = MetaFactory.factory.createInstance<IListViewManager>();

        private IFormController controller;

        /// <summary>
        /// Fills all tables related as parent to primary table who have
        ///  some linked combobox in the form and do not have parent themself.
        /// </summary>
        /// <param name="F">Form to scan for comboboxes</param>
        public void PreFillControls(Form F) {
            //MarkEvent("PreFillControls on form "+F.Text+" called.\n\r");
            controller =  FormController.GetController(F);
            List<SelectBuilder> selList = new List<SelectBuilder>();
            iterateControlsSelList(F.Controls, PreFillControls, selList);
            if (selList.Count > 0) {
                //var conn = MetaData.getConnection(F);
                conn.MULTI_RUN_SELECT(selList);
            }
        }

        /// <summary>
        /// Prefills every control on a form belonging to a table
        /// </summary>
        /// <param name="F"></param>
        /// <param name="tablename"></param>
        public void PreFillControls(Form F, string tablename) {
	        controller =  FormController.GetController(F);
            var selList = new List<SelectBuilder>();
            iterateControlsSelListName(F.Controls, PreFillControlsTable, tablename,
                selList);

            if (selList.Count > 0) {
                //var conn = MetaData.getConnection(F);
                conn.MULTI_RUN_SELECT(selList);
            }
        }




        #endregion


        #region Eventi di Check (utilizzati in GetFormData, prima di IsValid)

        private IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();
        private bool checkForNulls(Control C) {
            var tag = GetStandardTag(C.Tag);
            if (tag == null) return true;
            var table = GetTableName(tag);
            if (table == null) return true;
            if (table != primaryTable.TableName) return true;

            var column = GetColumnName(tag);
            if (column == null) return true;
            var T = DS.Tables[table];
            if (T == null) {
                //MarkEvent("Nel dataset non esiste la tabella "+table);
                return true;
            }
            var col = T.Columns[column];
            if (!col.AllowDBNull) {
                var mes = "Il valore immesso nella casella non può essere vuoto";
                shower.Show(C.FindForm(), mes, "Avviso", mdl.MessageBoxButtons.OK);//,MessageBoxIcon.Warning
                C.Focus();
                C.Select();
                return false;
            }
            return true;
        }

        #endregion


        #region Eventi di Enter / Leave dai TextBox


        private void intTextBox_Leave(object sender, EventArgs e) {
            if (destroyed) return;
            var T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            if (!T.Modified) return;
            T.Text = T.Text.Trim();
            if (T.Text == "") {
                checkForNulls((Control) sender);
                return;
            }
            int D;
            try {
                D = Convert.ToInt32(T.Text);
            }
            catch {

                string Mes = "Il valore immesso nella casella non è un numero intero";
                shower.Show(T.FindForm(), Mes, "Avviso", mdl.MessageBoxButtons.OK);
                    //,System.Windows.Forms.MessageBoxIcon.Warning);
                T.Focus();
                T.Select();
                return;
            }
        }

        /// <summary>
        /// Event called when leaving a numeric textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void LeaveNumTextBox(object sender, EventArgs e) {
            var T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            string tag = GetStandardTag(T.Tag);
            ExtLeaveNumTextBox(T, tag);
        }

        /// <summary>
        /// Formats a numeric textbox when leaving it
        /// </summary>
        /// <param name="T"></param>
        /// <param name="tag"></param>
        public static void ExtLeaveNumTextBox(TextBox T, string tag) {
            if (T.IsDisposed) return;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            //if (!T.Modified) return;
            string FieldType = GetFieldLower(tag, 2);
            T.Text = T.Text.Trim();
            if (T.Text == "") return;
            object O = HelpUi.GetObjectFromString(typeof(double), T.Text, tag);
            T.Text = HelpUi.StringValue(O, tag);

        }

        /// <summary>
        /// Event called when leaving a decimal textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void LeaveDecTextBox(object sender, EventArgs e) {
            var T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            string tag = GetStandardTag(T.Tag);
            ExtLeaveDecTextBox(T, tag);
        }

        /// <summary>
        /// Formats a Decimal textbox when exiting it
        /// </summary>
        /// <param name="T"></param>
        /// <param name="tag"></param>
        public static void ExtLeaveDecTextBox(TextBox T, string tag) {
            if (T.IsDisposed) return;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            T.Text = T.Text.Trim();
            if (T.Text == "") return;
            string FieldType = GetFieldLower(tag, 2);
            object O = HelpUi.GetObjectFromString(typeof(decimal), T.Text, tag);
            T.Text = HelpUi.StringValue(O, tag);
        }


        /// <summary>
        /// Called on textBox enter-event of numeric fields.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void EnterNumTextBox(object sender, EventArgs e) {
            TextBox T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            string tag = GetStandardTag(T.Tag);
            ExtEnterNumTextBox(T, tag);
        }

        /// <summary>
        /// Formats the content of a TextBox so it's more easy to edit for
        ///  the user.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="tag">tag to consider for the format. The only managed 
        ///  format is actually "fixed"</param>
        public static void ExtEnterNumTextBox(TextBox T, string tag) {
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            T.Text = T.Text.Trim();
            if (T.Text == "") return;
            string FieldType = GetFieldLower(tag, 2);

            if (FieldType == null || FieldType == "n") {
                string S = T.Text;
                S = S.Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, "");
                T.Text = S.Trim();
                return;
            }
            if (FieldType == "c") {
                string S = T.Text;
                S = S.Replace(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator, "");
                T.Text = S.Trim();
                return;
            }
            if (FieldType == "fixed") {
                string sdec = GetFieldLower(tag, 3);
                int dec = Convert.ToInt32(sdec);
                string prefix = GetFieldLower(tag, 4);
                if (prefix == null) prefix = "";
                string suffix = GetFieldLower(tag, 5);
                if (suffix == null) suffix = "";
                string s = T.Text;
                if (prefix != "") s = s.Replace(prefix, "");
                if (suffix != "") s = s.Replace(suffix, "");
                s = s.Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, "");
                T.Text = s.Trim();
            }
        }

        /// <summary>
        /// Formats a decimal textbox when entering in it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void EnterDecTextBox(object sender, EventArgs e) {
            TextBox T = (TextBox) sender;
            if (T.ReadOnly) return;
            if (!T.Enabled) return;
            string tag = GetStandardTag(T.Tag);
            ExtEnterDecTextBox(T, tag);
        }


        /// <summary>
        /// Formats a TextBox when entering on it
        /// </summary>
        /// <param name="T"></param>
        /// <param name="tag"></param>
        public static void ExtEnterDecTextBox(TextBox T, string tag) {
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            T.Text = T.Text.Trim();
            if (T.Text == "") return;
            string FieldType = GetFieldLower(tag, 2);

            if (FieldType == "n") {
                string S = T.Text;
                S = S.Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, "");
                T.Text = S.Trim();
                return;
            }
            if (FieldType == "c" || FieldType == null) {
                string S = T.Text;
                S = S.Replace(NumberFormatInfo.CurrentInfo.CurrencySymbol, "");
                S = S.Replace(NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator, "");
                T.Text = S.Trim();
                return;
            }

            if (FieldType == "fixed") {
                string sdec = GetFieldLower(tag, 3);
                int dec = Convert.ToInt32(sdec);
                string prefix = GetFieldLower(tag, 4);
                if (prefix == null) prefix = "";
                string suffix = GetFieldLower(tag, 5);
                if (suffix == null) suffix = "";
                string s = T.Text;
                if (prefix != "") s = s.Replace(prefix, "");
                if (suffix != "") s = s.Replace(suffix, "");
                s = s.Replace(NumberFormatInfo.CurrentInfo.NumberGroupSeparator, "");
                T.Text = s.Trim();
            }
        }




        /// <summary>
        /// Formats a (int) TextBox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void LeaveIntTextBox(object sender, EventArgs e) {
            TextBox T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            string tag = GetStandardTag(T.Tag);
            ExtLeaveIntTextBox(T, tag);
        }

        /// <summary>
        /// Formats an int TextBox when leaving it
        /// </summary>
        /// <param name="T"></param>
        /// <param name="tag"></param>
        public static void ExtLeaveIntTextBox(TextBox T, string tag) {
            if (T.IsDisposed) return;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            if (!T.Modified) return;
            T.Text = T.Text.Trim();
            if (T.Text == "") return;
            try {
	            object O = HelpUi.GetObjectFromString(typeof(int), T.Text, tag);
	            T.Text = HelpUi.StringValue(O, tag);
            }
            catch {
            }
            string FieldType = GetFieldLower(tag, 2);
            if (FieldType == null) return;
            if (FieldType != "year") return;
            FormatLikeYear(T);

        }


        DataColumn getDataColumn(string tag) {

            string table = GetTableName(tag);
            if (table == null) return null;
            string column = GetColumnName(tag);
            if (column == null) return null;
            if (DS == null) return null;
            return DS.Tables[table].Columns[column];

        }

        /// <summary>
        /// Formats a DateTime TextBox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LeaveDateTimeTextBox(object sender, EventArgs e) {
            var T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            string tag = GetStandardTag(T.Tag);
            var C = getDataColumn(tag);
            if (C != null) tag = CompleteTag(tag, C);
            ExtLeaveDateTimeTextBox(T, tag);
        }

        /// <summary>
        /// Formats a DateTime TextBox when leaving it
        /// </summary>
        /// <param name="T"></param>
        /// <param name="tag"></param>
        public static void ExtLeaveDateTimeTextBox(TextBox T, string tag) {
            if (T.IsDisposed) return;
            if (!T.Enabled) return;
            if (!T.Modified) return;
            if (T.ReadOnly) return;
            T.Text = T.Text.Trim();
            if (T.Text == "") return;
            try {
                var D = (DateTime) HelpUi.GetObjectFromString(typeof(DateTime), T.Text, tag);
                //Convert.ToDateTime(T.Text);
                T.Text = HelpUi.StringValue(D, tag);
                //D.ToShortDateString();
            }
            catch {
            }
        }



        /// <summary>
        /// Formats a textbox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GeneralLeaveDateTextBox(object sender, EventArgs e) {
            if (destroyed) return;
            var T = (TextBox) sender;
            if (T.IsDisposed) return;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            if (T.Text == "") return;
            string tag = GetStandardTag(T.Tag);
            var C = getDataColumn(tag);
            if (C != null) tag = CompleteTag(tag, C);
            string hhsep = CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator;
            string ppsep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            string S = T.Text;
            try {
                object O1 = HelpUi.GetObjectFromString(C.DataType, S, tag);
                if ((O1 != null) && (O1 != DBNull.Value)) {
                    T.Text = HelpUi.StringValue(O1, tag);
                    return;
                }
            }
            catch {

            }
            //S+=hhsep+"0"+ppsep+"0";

            int len = S.Length;
            object O = DBNull.Value;
            while (len > 0) {
                try {
                    O = HelpUi.GetObjectFromString(C.DataType, S, tag);
                    //MarkEvent("trying " + S);
                    if ((O != null) && (O != DBNull.Value)) break;
                }
                catch {
                }
                len -= 1;
                S = S.Substring(0, len);
            }
            //MarkEvent("found " + S);
            T.Text = HelpUi.StringValue(O, tag);
            return;
        }

        /// <summary>
        /// Formats a textbox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GeneralLeaveTextBox(object sender, EventArgs e) {
            if (destroyed) return;
            var T = (TextBox) sender;
            if (!T.Enabled) return;
            if (T.ReadOnly) return;
            if (T.Text == "") return;
            string tag = GetStandardTag(T.Tag);
            var C = getDataColumn(tag);
            if (C != null) tag = CompleteTag(tag, C);
            try {
                object O = HelpUi.GetObjectFromString(C.DataType, T.Text, tag);
                T.Text = HelpUi.StringValue(O, tag);
            }
            catch {
            }

        }

        

        #endregion


        #region Funzioni che estraggono la riga corrente di un controllo (grid,tree,combo..)
            

        /// <summary>
        /// Takes from a table T a row having same key as R
        /// </summary>
        /// <param name="T"></param>
        /// <param name="r">Row to search</param>
        /// <returns>Row found in T with same key of given Row</returns>
        public static DataRow FindExternalRow(DataTable T, DataRow r) {
            var condition = QueryCreator.WHERE_REL_CLAUSE(r, T.PrimaryKey, T.PrimaryKey, DataRowVersion.Default, false);
            var found = T.Select(condition);
            return found.Length == 0 ? null : found[0];
        }

        /// <summary>
        /// Gets current row from ComboBox, Grids and tree-views, return false on errors
        /// </summary>
        /// <param name="c">Control to analyze</param>
        /// <param name="T">Table containing rows</param>
        /// <param name="currentRow">Current selected row (null if none)</param>
        /// <returns>false on errors</returns>
        public bool GetCurrentRow(Control c, out DataTable T, out DataRow currentRow) {
            currentRow = null; //in case of errors, always return a value
            T = null;
            if(c is ComboBox combo) {
                if(combo.DataSource == null) return false;
                T = DS.Tables[((DataTable)(combo.DataSource)).TableName];

                if(combo.SelectedIndex <= 0) {
                    //index 0 is used for blank row
                    return true;
                }
                var rowView = (DataRowView)((ComboBox)c).Items[combo.SelectedIndex];
                if(rowView == null) return true;
                currentRow = rowView.Row;
                if(currentRow.Table == T) return true;
                currentRow = FindExternalRow(T, currentRow);
                return true;
            }

            //T is taken from DS, using TAG Table
            //Row is taken from T, using a filter using current grid row
            if(c is DataGrid grid) {
                if(c.Tag == null) return false;
                var tablename = GetTableName(c.Tag.ToString());
                if(tablename == null) return false;
                T = DS.Tables[tablename];
                if(T == null) {
                    ErrorLogger.Logger.markEvent("Nel dataset non esiste la tabella " + tablename);
                    return false;
                }



                var dsv = (DataSet)grid.DataSource;
                var tv = dsv?.Tables[grid.DataMember];
                if(tv == null) return false;

                if(tv.Rows.Count == 0) return true;
                DataRowView dv;
                try {
                    dv = (DataRowView)grid.BindingContext[dsv, tv.TableName].Current;
                }
                catch {
                    dv = null;
                }
                if(dv == null) return true;

                currentRow = dv.Row;
                if(tv.Equals(T)) return true;

                currentRow = FindExternalRow(T, currentRow);
                return true;
            }

            if(c is TreeView tree) {
                var tablename = GetTableName(tree.Tag.ToString());
                if(tablename == null) return false;
                T = DS.Tables[tablename];
                if(T == null) {
                    ErrorLogger.Logger.markEvent("Nel dataset non esiste la tabella " + tablename);
                    return false;
                }
                var node = tree.SelectedNode;
                if(node == null) return true;
                try {
                    if (node.Tag == null) return true;
                    var treenode = (tree_node)(node.Tag);
                    T = treenode.Row.Table;
                    currentRow = treenode.Row;
                    return true;
                }
                catch {
                    return true;
                }
            }
            return false;

        }

        #endregion
        

        void logException(Form f, string msg, Exception e) {
            ErrorLogger.Logger.logException(msg, exception: e, meta:f?.getInstance<IMetaData>());
        }


        #region Abilitazione / Disabilitazione bottoni del form

        void setEnableGridButtons(Button b) {
            if (b.Tag == null) return;
            var btninfo = b.Tag.ToString();
            var cmd = GetFieldLower(btninfo, 0);
            var g = GetLinkedGrid(b);
            if (g?.Tag == null) return;
            var tablename = GetTableName(g.Tag.ToString());
            if (tablename == null) return;
            //bool state=false;
            var someCurrData = (GetLastSelected(primaryTable) != null);
            if (cmd.Equals("edit") ||
                cmd.Equals("insert") ||
                cmd.Equals("delete") ||
                cmd.Equals("unlink")
            ) {
                controlEnabler.enableButton(b, someCurrData);
            }

        }


        /// <summary>
        /// Enable/Disable a button depending on form status information
        /// </summary>
        /// <param name="B"></param>
        void setMainButtons(Button B) {
            if (B.Tag == null) return;
            var btninfo = B.Tag.ToString();
            var cmd = GetFieldLower(btninfo, 0);
            var someCurrData = (GetLastSelected(primaryTable) != null);
            switch (cmd) {
                case "mainselect":
                case "mainsetsearch":
                case "comboedit":
                case "maininsert":
                    controlEnabler.enableButton(B, true);
                    return;
                case "maindosearch":
                    controlEnabler.enableButton(B,
                        (drawMode == drawmode.setsearch)); //TO CHECK FOR MAINDOSEARCH IN LIST FORMS
                    return;
                case "mainsave":
                case "maindelete":
                    bool toenable = (drawMode != drawmode.setsearch) && someCurrData;
                    controlEnabler.enableButton(B, toenable); //TO CHECK FOR MAINDOSEARCH IN LIST FORMS
                    return;
                case "maininsertcopy":
                    controlEnabler.enableButton(B, (drawMode == drawmode.edit) && someCurrData);
                    return;
                case "choose":
                case "manage":
                    if (drawMode == drawmode.setsearch) {
                        controlEnabler.enableButton(B, true);
                        return;
                    }

                    var tablename = GetField(btninfo, 1);
                    if (tablename == null) {
                        controlEnabler.enableButton(B, false);
                        return;
                    }
                    var chooseTable = DS.Tables[tablename];
                    if (chooseTable == null) {
                        controlEnabler.enableButton(B, false);
                        return;
                    }

                    if (drawMode == drawmode.insert) {
                        controlEnabler.enableButton(B, true);
                        return;
                    }


                    //Check if relation implies primary key fields of primary table
                    if (QueryCreator.CheckKeyParent(chooseTable, primaryTable)) {
                        controlEnabler.enableButton(B, false);
                        return;
                    }
                    controlEnabler.enableButton(B, true);
                    break;

            }
        }

        #endregion




        #region Funzioni di Clear del Form

        void clearControl(Control c) {
	        if (controller == null) controller =  FormController.GetController(c.FindForm());
	        controller.setColor(c);
            if (c.Tag == null) return;
            if (c.Tag.ToString().Trim() == "") return;
            var cType = c.GetType();
            if (typeof(Label).IsAssignableFrom(cType)) return;
            controlEnabler.reEnable(c);

            //source is the field to fill in the control
            if (c is ComboBox) {
                var tt = (DataTable) ((ComboBox) c).DataSource;
                if (tt == null) return;
                var tt2 = DS.Tables[tt.TableName];
                if (tt2.ParentRelations.Count > 0) {
                    model.clear(tt);
                    model.clear(tt2);
                }
                //if (ComboBoxToRefilter) comboBoxManager.resetComboBoxSource((ComboBox) C, TT.TableName, dmode);
                //((ComboBox) C).SelectedIndex = -1;
                comboBoxManager.clearCombo((ComboBox)c,tt);
                return;
            }

            if (typeof(TextBox).IsAssignableFrom(cType)) {
                ((TextBox) c).Text = "";
                return;
            }
            if (c is CheckBox chk) {
                chk.ThreeState = true;
                chk.CheckState = CheckState.Indeterminate;
                return;
            }
            if (c is RadioButton rad) {
                rad.Checked = false;
                return;
            }

            if (c is DataGrid g) {
	            var s = metaprofiler.StartTimer($"Clear grid {g.Name}");
                g.SuspendLayout();
                g.SetDataBinding(null, "");
                g.ResumeLayout();
                metaprofiler.StopTimer(s);
                return;
            }

            if(c is ListView l) {
                foreach(ListViewItem li in l.Items) li.Checked = false;
                return;
            }

            if (c is Button) {
                setEnableGridButtons((Button) c);
                setMainButtons((Button) c); // 2nd parameter is ignored because primary table is empty
                return;
            }
        }

        /// <summary>
        /// Clears form controls, unbinding datagrids and setting comboboxes to "no selection" state.
        /// Note that combobox lists are cleared only if combobox has no parent tables
        /// </summary>
        /// <param name="f"></param>
        public void ClearForm(Form f) {
	        int startClear = metaprofiler.StartTimer("ClearForm * " + f.Name);
	        controller =  FormController.GetController(f); // f.getInstance<IFormController>();
            iterateClear(f.Controls);
            metaprofiler.StopTimer(startClear);
            //ReEnable();
        }

        /// <summary>
        /// Clear control specified in a collection, recursively
        /// </summary>
        /// <param name="cs"></param>
        public void ClearControls(Control.ControlCollection cs) {
            eventManager.DisableAutoEvents();
            iterateClear(cs);
            eventManager.EnableAutoEvents();
        }

        void iterateClear(IEnumerable cs) {
            foreach (Control c in cs) {
                //int startClear = metaprofiler.StartTimer("Clear " + c.Name);
                clearControl(c);
                //metaprofiler.StopTimer(startClear);
                if (c.HasChildren) {
                    iterateClear(c.Controls);
                }
            }
        }

        #endregion


        #region Calcolo della Search Condition

        string getSearchCombo(ComboBox c, DataColumn col, string condition) {
            if (c.SelectedValue == null) return condition;
            if (c.SelectedIndex == 0) return condition;
            string search = GetSearchTag(c.Tag);
            string searchcol = GetColumnName(search);

            if (c.SelectedValue.ToString() == "") return condition;

            object o = HelpUi.GetObjectFromString(col.DataType, c.SelectedValue.ToString(), null);
            if (o == null) return condition;

            try {
                return GetData.MergeFilters(condition,QueryCreator.comparelikefields(searchcol, o, o.GetType(), true)
                );
            }
            catch {
                return condition;
            }

        }

        /// <summary>
        /// Gets the mainsearch condition scanning all control of a Form
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public string GetSearchCondition(Form f) {
            var condition = "";
            return iterateGetSearchCondition(f.Controls, condition);
        }

        /// <summary>
        /// Gets the mainsearch condition scanning a specified set of controls (and childs)
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public string GetSpecificCondition(Control.ControlCollection cs, string table) {
            var condition = "";
            return iterateGetSpecificSearchCondition(cs, condition, table);
        }

        string iterateGetSearchCondition(IEnumerable cs, string condition) {
            foreach (Control c in cs) {
                if (isManagedCollection(c)) {
                    condition = getSearchFromManagedCollection(c, condition);
                }
                else {
                    condition = getSearchCondition(c, condition);
                    if (c.HasChildren) {
                        condition = iterateGetSearchCondition(c.Controls, condition);
                    }
                }
            }
            return condition;
        }

        string getSearchRadioButton(RadioButton c, DataColumn col, string condition, string tag) {
            string Tag = GetSearchTag(tag);
            string searchcol = GetColumnName(Tag);
            int pos = Tag.IndexOf(':');
            string cvalue = Tag.Substring(pos + 1).Trim();
            if (!cvalue.StartsWith(":")) {
                if (c.Checked) {
                    var O = HelpUi.GetObjectFromString(col.DataType, cvalue, null);
                    if (O == null) return condition;
                    try {
                        return GetData.MergeFilters(condition, QueryCreator.comparelikefields(searchcol, O, O.GetType(), true));
                    }
                    catch {
                    }
                }
            }
            else {
                cvalue = cvalue.Substring(1);
                bool negato = false;
                if (cvalue.StartsWith("#")) {
                    negato = true;
                    cvalue = cvalue.Substring(1);
                }

                var nbit = Convert.ToInt32(cvalue);
                var val = 1;
                val <<= (nbit);
                var tocompare = val;
                var cond = "";
                if (negato) {
                    tocompare = 0;
                }
                if (c.Checked)cond = $"(({searchcol} & {val})={tocompare})";
                return GetData.MergeFilters(condition, cond);

            }
            return condition;
        }


        string iterateGetSpecificSearchCondition(IEnumerable cs, string condition, string table) {
            foreach (Control c in cs) {
                if (isManagedCollection(c)) continue;
                condition = getSpecificSearchCondition(c, condition, table);
                if (c.HasChildren) {
                    condition = iterateGetSpecificSearchCondition(c.Controls, condition, table);
                }
            }
            return condition;
        }


        string getSearchFromManagedCollection(Control c, string condition) {
            string tag = GetSearchTag(c.Tag);
            if (tag == null) return condition;
            if (!(c is GroupBox)) return condition;
            return GetFieldLower(tag, 2) == "valuesigned" ? getSearchFromValueSigned((GroupBox) c, condition) : condition;
        }

        TextBox searchValueTextBox(GroupBox g) {
            foreach (Control c in g.Controls) {
                if (c is TextBox) {
                    return (TextBox) c;
                }
            }
            return null;
        }

        string getSearchFromValueSigned(GroupBox c, string condition) {
            var tag = GetSearchTag(c.Tag);
            if (tag == null) return condition;

            var T = searchValueTextBox(c);
            if (T == null) return condition;
            if (T.Text == "") return condition;

            var tn = GetTableName(tag);
            var cn = GetColumnName(tag);
            if (cn == null) return condition;

            var colname = cn;
            Type coltype = typeof(string);
            var Tn = DS.Tables[tn];
            var Col = Tn?.Columns[cn];
            if (Col != null) {
                coltype = Col.DataType;
            }

            object o = HelpUi.GetObjectFromString(coltype, T.Text, null);
            if (o == null) return condition;
            bool sign = getSignForValueSigned(c);
            if (!sign) o = invertSign(o);

            try {
                return GetData.MergeFilters(condition,
                    QueryCreator.comparelikefields(colname,
                        o, o.GetType(), true));
            }
            catch {
                return condition;
            }
        }

        string getSearchFromControl(Control c, DataColumn col, string condition, string tag) {
	        try {
		        //source is the field to fill in the control
		        if (c is ComboBox) {
			        return getSearchCombo((ComboBox) c, col, condition);
		        }

		        if (c is TextBox) {
			        return getSearchText((TextBox) c, col, condition);
		        }

		        if (c is CheckBox) {
			        return getSearchCheckBox((CheckBox) c, col, condition, tag);
		        }

		        if (c is RadioButton) {
			        return getSearchRadioButton((RadioButton) c, col, condition, tag);
		        }

		        if (c is RichTextBox) {
			        return getSearchRichText((RichTextBox) c, col, condition);
		        }
	        }
	        catch (Exception e) {
		        logException(null, $"getSearchFromControl({c.Name}) with tag {c.Tag}", e);
	        }

	        return condition;

        }

        DataColumn getSuitableColumnForSearchTag(string Tag) {
            var colname = GetColumnName(Tag);
            if (colname == null || (colname == "")) colname = "temp";
            var fmt = GetFieldLower(Tag, 2) ?? "";
            fmt = fmt.ToUpper();
            switch (fmt) {
                case "":
                    var tabname = GetTableName(Tag);
                    DataTable T ;
                    if (DS.Tables.Contains(tabname)) {
                        T = DS.Tables[tabname];
                        if (T.Columns.Contains(colname)) return T.Columns[colname];
                    }
                    T = conn.CreateTableByName(tabname, "*");
                    return T.Columns.Contains(colname) ? T.Columns[colname] : new DataColumn(colname, typeof(string));
                case "C": return new DataColumn(colname, typeof(decimal));
                case "N": return new DataColumn(colname, typeof(decimal));
                case "FIXED":return new DataColumn(colname, typeof(decimal));
            }

            return new DataColumn(colname, typeof(string));
        }

        string getSearchCondition(Control C, string condition) {
            var tag = GetSearchTag(C.Tag);
            var tag2 = GetStandardTag(C.Tag);
            if (tag == null) return condition;

            var table = GetTableName(tag); //SEARCH TABLE 
            var table2 = GetTableName(tag2); //EDIT TABLE

            if ((table == table2) && (table != primaryTable.TableName) && !HasSpecificSearchTag(C.Tag)) return condition;
            var searchTable = DS.Tables[table];
            var column = GetColumnName(tag);
            if (column == null) return condition;
            var maincol = GetColumnName(tag2);
            DataColumn col = null;
            if (maincol != null) {
                if (primaryTable.Columns[maincol] != null) col = primaryTable.Columns[maincol];
            }
            if (col == null) col = getSuitableColumnForSearchTag(tag);
            if (searchTable?.Columns[column] != null) col = searchTable.Columns[column];

            return getSearchFromControl(C, col, condition, tag);
        }


        string getSpecificSearchCondition(Control c, string condition, string specTable) {
            if (c.Enabled == false) return condition;

            var tag = GetStandardTag(c.Tag);
            if (tag == null) return condition;
            string table = GetTableName(tag); //SEARCH TABLE
            if (table == null) return condition;

            if (table != specTable) return condition;
            var searchTable = DS.Tables[table];

            string column = GetColumnName(tag);
            if (column == null) return condition;
            var col = searchTable.Columns[column];

            if (c is TextBox) {
                if (((TextBox) c).ReadOnly) return condition;
            }

            return getSearchFromControl(c, col, condition, tag);
        }

        string getSearchRichText(RichTextBox T, DataColumn col, string condition) {
            if (T.Text == "") return condition;
            var tag = GetSearchTag(T.Tag);
            var searchcol = GetColumnName(tag);
            tag = CompleteTag(tag, col);

            object o = HelpUi.GetObjectFromString(col.DataType, T.Text, tag);

            var sqltype = col.ExtendedProperties["sqltype"] as string;
            if ((o != DBNull.Value) && (o != null) && (sqltype == "text")) {
                var S = o.ToString();
                if (S.IndexOf("%") == -1) S += "%";
                o = S;
            }

            if (o == null) return condition;
            string fmt = GetFieldLower(tag, 2);
            try {
                return GetData.MergeFilters(condition,
                    QueryCreator.comparelikefields(searchcol,
                        o, o.GetType(), true));
            }
            catch {
                return condition;
            }
        }

        string getSearchText(TextBox T, DataColumn col, string condition) {
            if (T.Text == "") return condition;
            string tag = GetSearchTag(T.Tag);
            string searchcol = GetColumnName(tag);
            tag = CompleteTag(tag, col);

            var o = HelpUi.GetObjectFromString(col.DataType, T.Text, tag);

            string sqltype = col.ExtendedProperties["sqltype"] as string;
            if ((o != DBNull.Value) && (o != null) && (sqltype == "text")) {
                string s = o.ToString();
                if (s.IndexOf("%") == -1) s += "%";
                o = s;
            }

            if (o == null) return condition;
            string fmt = GetFieldLower(tag, 2);
            if ((col.DataType.Name == "DateTime") &&
                (HelpUi.IsOnlyTimeStyle(fmt)) && (o != DBNull.Value) &&
                (!o.Equals(HelpUi.EmptyDate()))) {
                var dt = Convert.ToDateTime(o);
                string filter = $"(DATEPART(hh,{searchcol})={dt.Hour})AND(DATEPART(n,{searchcol})={dt.Minute})AND(DATEPART(s,{searchcol})={dt.Second})";
                return GetData.MergeFilters(condition, filter);
            }
            try {
                return GetData.MergeFilters(condition, QueryCreator.comparelikefields(searchcol,o, o.GetType(), true));
            }
            catch {
                return condition;
            }
        }

        string getSearchCheckBox(CheckBox c, DataColumn col, string condition, string tag) {
            if (c.CheckState == CheckState.Indeterminate) return condition;

            string Tag = GetSearchTag(tag);
            string searchcol = GetColumnName(Tag);
            int pos = Tag.IndexOf(':');
            if (pos == -1) return condition;
            string values = Tag.Substring(pos + 1).Trim();

            if (values.IndexOf(":") == -1) {
                bool negato = false;
                if (values.StartsWith("#")) {
                    negato = true;
                    values = values.Substring(1);
                }

                int Nbit = Convert.ToInt32(values);
                int val = 1;
                val <<= (Nbit);
                string cond;
                bool valore = c.Checked;
                if (negato) valore = !valore;
                if (valore)
                    cond = $"(({searchcol} & {val})={val})";
                else
                    cond = $"(({searchcol} & {val})=0)";
                return GetData.MergeFilters(condition, cond);
            }

            var yvalue = values.Split(new char[] {':'}, 2)[0].Trim();
            var nvalue = values.Split(new char[] {':'}, 2)[1].Trim();

            var newvalue = c.Checked ? yvalue : nvalue;

            var o = HelpUi.GetObjectFromString(col.DataType, newvalue, null);
            if (o == null) return condition;
            try {
                return GetData.MergeFilters(condition,
                    QueryCreator.comparelikefields(searchcol,
                        o, o.GetType(), true));
            }
            catch {
                return condition;
            }

        }


        #endregion

        #region Funzioni di gestione ValueSigned and ManagedCollections

        bool isManagedCollection(Control c) {
            var handle = metaprofiler.StartTimer("IsManagedCollection");
            try {
                if (!(c is GroupBox)) return false;
                var tag = GetStandardTag(c.Tag);
                if (tag == null) return false;
                return GetFieldLower(tag, 2) == "valuesigned";
            }
            finally {
                metaprofiler.StopTimer(handle);
            }
        }

        //true when POSITIVE sign
        bool getSignForValueSigned(GroupBox g) {
            foreach (Control c in g.Controls) {
                if (c is RadioButton) {
                    if (c.Tag == null) continue;
                    if (c.Tag.ToString() == "-") {
                        return !((RadioButton) c).Checked;
                    }
                }
            }
            return true; //default sign
        }

        void setSignForValueSigned(GroupBox g, bool sign) {
            foreach (Control c in g.Controls) {
                if (c is RadioButton) {
                    if (c.Tag == null) continue;
                    if (c.Tag.ToString() == "-") {
                        ((RadioButton) c).Checked = !sign;
                    }
                    if (c.Tag.ToString() == "+") {
                        ((RadioButton) c).Checked = sign;
                    }
                }
            }
        }



        object invertSign(object o) {
            Type T = o.GetType();
            try {
                switch (T.Name) {
                    case "Double":
                        double d1 = (double) o;
                        return -d1;
                    case "Decimal":
                        decimal d2 = (decimal) o;
                        return -d2;
                    case "Int16":
                        short i1 = (short) o;
                        return -i1;
                    case "Int32":
                        int i2 = (int) o;
                        return -i2;
                    default:
                        return o;
                }
            }
            catch {
                return o;
            }
        }

        bool signOf(object o) {
            Type T = o.GetType();
            try {
                switch (T.Name) {
                    case "Double":
                        double d1 = (double) o;
                        return (d1 >= 0);
                    case "Decimal":
                        decimal d2 = (decimal) o;
                        return (d2 >= 0);
                    case "Int16":
                        short i1 = (short) o;
                        return (i1 >= 0);
                    case "Int32":
                        int i2 = (int) o;
                        return (i2 >= 0);
                    default:
                        return true;

                }
            }
            catch {
                return true;
            }
        }

        #endregion


        #region Fill Controls Related to a Changed Table/Row


        /// <summary>
        /// Fills a collection of controls (and childs) to reflect a new row selected
        ///  in a Control
        /// </summary>
        /// <param name="cs">Controls to fill</param>
        /// <param name="changed">Control that generated the row change event</param>
        /// <param name="T">Table containing the changed row</param>
        /// <param name="rowChanged">New selected row</param>
        public void IterateFillRelatedControls(Control.ControlCollection cs,
            Control changed,
            DataTable T,
            DataRow rowChanged) {
            if (!eventManager.AutoEventEnabled) return;
            if (cs == null) return;
            //MarkEvent("IterateFillRelatedCControls on Control "+Changed.Name+" - Table "+T.TableName+" called.\n\r");			

            foreach (Control c in cs) {
                if (c == changed) continue;
                //FillRelatedToRowControl(C, T, RowChanged);

                if (isManagedCollection(c)) {
                    fillRelatedToRowControl(c, T, rowChanged);
                }
                else {
                    fillRelatedToRowControl(c, T, rowChanged);
                    if (c.HasChildren) {
                        IterateFillRelatedControls(c.Controls, changed, T, rowChanged);
                    }
                }
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        [Obsolete("Use ComboBoxManager.SetComboBoxValue(ComboBox c, object s)")]
        public static void SetComboBoxValue(ComboBox c, object s) {
            ComboBoxManager.SetComboBoxValue(c, s);
        }


        /// <summary>
        /// Prefills a combo with a specified filter, optionally changing its value
        /// </summary>
        /// <param name="C"></param>
        /// <param name="filter"></param>
        /// <param name="freshvalue"></param>
        /// <param name="dmode"></param>
        [Obsolete("comboBoxManager.filteredPreFillCombo(C, filter, freshvalue)")]
        public void FilteredPreFillCombo(ComboBox C, string filter, bool freshvalue, drawmode dmode) {
            comboBoxManager.filteredPreFillCombo(C, filter, freshvalue);
        }



        void fillRelatedToRowControl(TextBox c,
            DataTable changed,
            DataRow changedRow) {
            string ChangedName = changed.TableName;

            string tag = GetStandardTag(c.Tag);
            if (tag == null) return;

            DataTable tagTable;

            var tagTableName = GetTableName(tag);
            if (tagTableName == null) {
                shower.Show(c.FindForm(), $"{mydescr(c)} has not a valid Table in (Standard) Tag ({tag})");
                return;
            }
            tagTable = DS.Tables[tagTableName];
            if (tagTable == null) {
                shower.Show(c.FindForm(), $"{mydescr(c)} has not a valid Table in (Standard) Tag ({tag})");
                return;
            }

            string tagColumnName = GetColumnName(tag);
            if (tagColumnName == null) {
                shower.Show(c.FindForm(), $"{mydescr(c)} has not a valid Column in (Standard) Tag ({tag})");
                return;
            }

            if (ChangedName == tagTableName) {
                c.Text = changedRow != null ? HelpUi.StringValue(changedRow[tagColumnName], tag) : "";
                return;
            }
            var rfound = QueryCreator.GetParentChildRel(tagTable, changed);
            if (rfound == null) return;
            if (changedRow == null) {
                if (!checkToClear(tagTable, tagColumnName, rfound)) return;
                c.Text = "";
                return;
            }
            DataRow parentRow = null;
            if (rfound.DataSet == changedRow.Table.DataSet) parentRow = changedRow.GetParentRow(rfound);
            if (parentRow == null) return;


            c.Text = HelpUi.StringValue(parentRow[tagColumnName], tag);
        }

        /// <summary>
        /// States if a control displaying  childcolumn of child table is to clear, knowing that
        /// relation with parent was on column tagcolumn, assuming Parent Row was not found
        /// </summary>
        /// <param name="childtable">Child table linked to Control</param>
        /// <param name="childcolumn">Child column linked to COntrol</param>
        /// <param name="rchild">relation between ChildTable and Parent(Changed) row</param>
        /// <returns>true if control is to clear</returns>
        bool checkToClear(DataTable childtable,
            string childcolumn,
            DataRelation rchild) {
            if (rchild.ChildColumns.Any(c => QueryCreator.IsPrimaryKey(childtable, c.ColumnName))) {
                return false;
            }

            return rchild.ChildColumns.Any(c => c.ColumnName == childcolumn);
        }

        void fillRelatedToRowControl(CheckBox C,
            DataTable changed,
            DataRow changedRow) {
            var changedName = changed.TableName;

            var tag = GetStandardTag(C.Tag);
            if (tag == null) return;

            var tagTableName = GetTableName(tag);
            if (tagTableName == null) {
                shower.Show(C.FindForm(), $"{mydescr(C)} has not a valid Table in (Standard) Tag ({tag})");
                return;
            }
            var tagTable = DS.Tables[tagTableName];
            if (tagTable == null) {
                shower.Show(C.FindForm(), $"{mydescr(C)} has not a valid Table in (Standard) Tag ({tag})");
                return;
            }

            string tagColumnName = GetColumnName(tag);
            if (tagColumnName == null) {
                shower.Show(C.FindForm(), $"{mydescr(C)} has not a valid Column in (Standard) Tag ({tag})");
                return;
            }

            if (changedName == tagTableName) {
                setCheckBox(C, changed, tagColumnName, changedRow?[tagColumnName]);
                return;
            }
            var rfound = QueryCreator.GetParentChildRel(tagTable, changed);
            if (rfound == null) return;
            if (changedRow == null) {
                if (!checkToClear(tagTable, tagColumnName, rfound)) return;

                setCheckBox(C, tagTable, tagColumnName, null);
                return;
            }
            var parentRow = changedRow.GetParentRow(rfound);
            if (parentRow == null) return;

            setCheckBox(C, tagTable, tagColumnName, parentRow[tagColumnName]);
        }

        void fillRelatedToRowControl(RadioButton c,
            DataTable changed,
            DataRow changedRow) {
            var changedTableName = changed.TableName;

            var tag = GetStandardTag(c.Tag);
            if (tag == null) return;

            var tagTableName = GetTableName(tag);
            if (tagTableName == null) {
                shower.Show(c.FindForm(), mydescr(c) + " has not a valid Table in (Standard) Tag (" + tag + ")");
                return;
            }
            var tagTable = DS.Tables[tagTableName];
            if (tagTable == null) {
                shower.Show(c.FindForm(), mydescr(c) + " has not a valid Table in (Standard) Tag (" + tag + ")");
                return;
            }
            var tagColumnName = GetColumnName(tag);
            if (tagColumnName == null) {
                shower.Show(c.FindForm(), mydescr(c) + " has not a valid Column in (Standard) Tag (" + tag + ")");
                return;
            }
            if (changedTableName == tagTableName) {

                setRadioButton(c, changed, tagColumnName, changedRow?[tagColumnName]);
                return;
            }
            //find a Relation between the changed row and the tag related table
            var rfound = QueryCreator.GetParentChildRel(tagTable, changed);
            if (rfound == null) return;
            if (changedRow == null) {
                if (!checkToClear(tagTable, tagColumnName, rfound)) return;

                setRadioButton(c, tagTable, tagColumnName, null);
                return;
            }
            var parentRow = changedRow.GetParentRow(rfound);
            if (parentRow == null) return;

            setRadioButton(c, tagTable, tagColumnName, parentRow[tagColumnName]);
        }



        void fillValueSignedRelatedToRowControl(GroupBox C,
            DataTable changed,
            DataRow changedRow) {
            var changedName = changed.TableName;

            string tag = GetStandardTag(C.Tag);
            if (tag == null) return;

            var tagTableName = GetTableName(tag);
            if (tagTableName == null) {
                shower.Show(C.FindForm(), $"{mydescr(C)} has not a valid Table in (Standard) Tag ({tag})");
                return;
            }
            var tagTable = DS.Tables[tagTableName];
            if (tagTable == null) {
                shower.Show(C.FindForm(), $"{mydescr(C)} has not a valid Table in (Standard) Tag ({tag})");
                return;
            }

            string tagColumnName = GetColumnName(tag);
            if (tagColumnName == null) {
                shower.Show(C.FindForm(), $"{mydescr(C)} has not a valid Column in (Standard) Tag ({tag})");
                return;
            }

            if (changedName == tagTableName) {
                fillValueSignedGroup(C, changedRow?[tagColumnName]);
                return;
            }

            var rfound = QueryCreator.GetParentChildRel(tagTable, changed);
            if (rfound == null) return;


            if (changedRow == null) {
                fillValueSignedGroup(C, null);
                return;
            }
            var parentRow = changedRow.GetParentRow(rfound);
            if (parentRow == null) return;

            fillValueSignedGroup(C, parentRow[tagColumnName]);

        }



        static void calcParentRelation(DataGrid G, DataSet DS) {
            if (G.Tag == null) return;
            if (DS == null) return;
            string gridtablename = GetField(G.Tag.ToString(), 0);
            if (gridtablename == null) return;
            var gridtable = DS.Tables[gridtablename];
            if (gridtable?.ExtendedProperties["gridmaster"] == null) return;
            string gridmaster = gridtable.ExtendedProperties["gridmaster"].ToString();
            var mastertable = DS.Tables[gridmaster];
            if (mastertable == null) return;

            if (gridtable.ExtendedProperties["CustomParentRelation"] == null) {
                var parentChildRel = QueryCreator.GetParentChildRel(mastertable, gridtable);
                if (parentChildRel == null) return;
                var masterRow = GetLastSelected(DS.Tables[gridmaster]);
                string cond;
                if (masterRow != null) {
                    cond = QueryCreator.WHERE_REL_CLAUSE(masterRow,
                        parentChildRel.ParentColumns, parentChildRel.ChildColumns,
                        DataRowVersion.Default, true);
                }
                else {
                    cond = "(1=0)";
                }
                gridtable.ExtendedProperties["ParentRelation"] = cond;
            }
            else {
                gridtable.ExtendedProperties["ParentRelation"] = gridtable.ExtendedProperties["CustomParentRelation"];
            }


        }

        void fillRelatedToRowControl(DataGrid g,
            DataTable changed,
            DataRow changedRow) {
            if (g.Tag == null) return;
            var gridtablename = GetTableName(g.Tag.ToString());
            if (gridtablename == null) return;
            DataTable gridtable = DS.Tables[gridtablename];
            if (gridtable?.ExtendedProperties["gridmaster"] == null) return;
            if (gridtable.ExtendedProperties["gridmaster"].ToString() != changed.TableName) return;
            if (gridtable.ExtendedProperties["CustomParentRelation"] == null) {

                DataRelation parentChildRel = QueryCreator.GetParentChildRel(changed, gridtable);
                if (parentChildRel == null) return;
                string cond = null;
                if (changedRow != null) {
                    cond = QueryCreator.WHERE_REL_CLAUSE(changedRow,
                        parentChildRel.ParentColumns, parentChildRel.ChildColumns,
                        DataRowVersion.Default, true);
                }
                gridtable.ExtendedProperties["ParentRelation"] = cond;
            }
            else {
                gridtable.ExtendedProperties["ParentRelation"] = gridtable.ExtendedProperties["CustomParentRelation"];
            }


            var gridhandle = metaprofiler.StartTimer("SetDataGrid(G)");
            setDataGrid((DataGrid) g);
            metaprofiler.StopTimer(gridhandle);

        }


        void fillRelatedToRowControl(Control C,
            DataTable parentTable,
            DataRow changedRow) {
            if (!eventManager.AutoEventEnabled) return;

            if (C is ComboBox) {
                comboBoxManager.fillRelatedToRowControl((ComboBox) C, parentTable,
                    changedRow);
                additionalInfo[C.Name] = comboBoxManager.comboTip(C as ComboBox);
                setToolTip(C);
                return;
            }

            //If C is a TextBox, set it when is linked to Parent table
            if (C is TextBox) {
                fillRelatedToRowControl((TextBox) C, parentTable, changedRow);
                return;
            }

            if (C is CheckBox) {
                fillRelatedToRowControl((CheckBox) C, parentTable, changedRow);
                return;
            }

            if (C is RadioButton) {
                fillRelatedToRowControl((RadioButton) C, parentTable, changedRow);
                return;
            }

            if (C is DataGrid) {
                fillRelatedToRowControl((DataGrid) C, parentTable, changedRow);
                return;
            }

            //If C is a TreeView, set it when it's parent row is changed or 
            // when selected row is changed
            if (C is TreeView) {
                return;
            }

            var tag = GetStandardTag(C.Tag);
            if (tag == null) return;
            //If C is a Managed GroupBox, set it when is linked to Parent table
            if (C is GroupBox) {
                if (GetFieldLower(tag, 2) == "valuesigned") {
                    fillValueSignedRelatedToRowControl((GroupBox) C, parentTable, changedRow);
                }
            }
        }



        /// <summary>
        /// Fills a control in order to display a specified row. 
        /// Only controls linked to the right table are affected. All other are left
        ///  unchanged.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="T"></param>
        /// <param name="changedRow"></param>
        public void SetDataRowRelated(Form f,
            DataTable T,
            DataRow changedRow) {
            //MarkEvent("SetDataRowRelated on Form "+F.Text+" called.\n\r");			
            IterateSetDataRowRelated(f.Controls, T, changedRow);
        }

        /// <summary>
        /// Fills a collection of controls in order to display a specified row. 
        /// Only controls linked to the right table are affected. All other are left
        ///  unchanged.
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="T"></param>
        /// <param name="changedRow">Row to display</param>
        public void IterateSetDataRowRelated(Control.ControlCollection cs,
            DataTable T,
            DataRow changedRow) {
            //MarkEvent("IterateSetDataRowRelated on Table "+T.TableName+" called.\n\r");			
            foreach (Control c in cs) {
                if (isManagedCollection(c)) {
                    fillRelatedToRowControl(c, T, changedRow);
                }
                else {
                    fillRelatedToRowControl(c, T, changedRow);
                    if (c.HasChildren) {
                        IterateSetDataRowRelated(c.Controls, T, changedRow);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Sets the value for a combobox
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        [Obsolete("ComboBoxManager.SetComboBoxStringValue")]
        public static void SetComboBoxValue(ComboBox c, string s) {
            ComboBoxManager.SetComboBoxStringValue(c, s);
        }

        /// <summary>
        /// Sets the focus on a field having a specified (standard) tag
        /// </summary>
        /// <param name="f"></param>
        /// <param name="fieldTag"></param>
        [Obsolete] public static void FocusField(Form f, string fieldTag) {
            FocusField(f.Controls, fieldTag);
        }

        /// <summary>
        /// Makes a Control Focuses &amp; visible
        /// </summary>
        /// <param name="C"></param>
        [Obsolete] static public void FocusControl(Control C) {
            var CP = C.Parent;
            while (CP != null) {
                if (CP is TabPage) break;
                //if (typeof(Crownwood.Magic.Controls.TabPage).IsAssignableFrom(CP.GetType())) break;
                if (CP is Form) {
                    CP = null;
                    break;
                }
                CP = CP.Parent;
            }
            if ((CP != null) && (CP is TabPage)) {
                var TP = (TabPage) CP;
                if(TP.Parent is TabControl TC) {
                    TC.SelectedTab = TP;
                }
            }
            //if ((CP != null) && (typeof(Crownwood.Magic.Controls.TabPage).IsAssignableFrom(CP.GetType()))) {
            //    var TP = (Crownwood.Magic.Controls.TabPage) CP;
            //    if (typeof(Crownwood.Magic.Controls.TabControl).IsAssignableFrom(TP.Parent.GetType())) {
            //        var TC = (Crownwood.Magic.Controls.TabControl) TP.Parent;
            //        TC.SelectedTab = TP;
            //    }
            //}

            C.Focus();
        }

        [Obsolete] static void FocusField(Control.ControlCollection Cs, string FieldTag) {
            foreach (Control C in Cs) {
                string tag = GetStandardTag(C.Tag);
                if (tag != null) {
                    if ((GetField(tag, 0) != null) && (GetField(tag, 1) != null)) {
                        tag = GetField(tag, 0) + "." + GetField(tag, 1);

                        if (tag == FieldTag) {
                            FocusControl(C);
                            return;
                        }
                    }
                }
                if (C.HasChildren) {
                    FocusField(C.Controls, FieldTag);
                }
            }

        }

        /// <summary>
        /// Give developer info about a control
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string mydescr(Control c) {
            if (c == null) return "";
            string s = c.GetType().Name + " named " + c.Name;
            var ff = c.FindForm();
            if (ff != null) s += " in form " + ff.Name;
            s += "(Tag =";
            if (c.Tag == null)
                s += "(null)";
            else
                s += c.Tag;
            s += ") ";
            return s;
        }

        /// <summary>
        /// Formats a textbox assuming it contains a year value
        /// </summary>
        /// <param name="T"></param>
        public static void FormatLikeYear(TextBox T) {
            int D;
            try {
                D = Convert.ToInt32(T.Text);
            }
            catch {
                T.Text = "";
                return;
            }
            var Year = DateTime.Now.Year;
            int group = (D < 100) ? 100:1000;
            int half = (D < 100) ? 50 : 500;
            if (D >= Year-100 && D<Year+100) return;
           
            int aa = Year % group;
            int CC = Year - aa;
            D += CC;
            if (D > Year + half) D -= group;
            if (D < Year - half) D += group;

            T.Text = D.ToString();
        }



        /// <summary>
        /// Get a grid contained in the same container of C
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public DataGrid GetLinkedGrid(Control C) {
            Control Parent = C.Parent;
            if (Parent == null) return null;
            foreach (Control Child in Parent.Controls) {
                if (Child is DataGrid) {
                    return (DataGrid) Child;
                }
            }
            return null;
        }



        private static void GridSelectRow(DataGrid G, int Row) {
            if (G.CurrentRowIndex == -1 && Row == -1) return;
            if (G.VisibleRowCount == 0 && Row == -1) return;
            try {
                G.Select(Row);
            }
            catch (Exception e) {
                ErrorLogger.Logger.markException(e,"GridSelectRow");
            }
        }

        /// <summary>
        /// Event called when key up is operated on a grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void HelpForm_KeyUp(object sender, KeyEventArgs e) {
            if (!(sender is DataGrid))return;
            var g = (DataGrid) sender;
            var d = g.DataSource as DataSet;
            var T = d?.Tables[g.DataMember];
            if (T == null) return;
            if (e.Shift && !GetAllowMultiSelection(T)) {
                ClearSelection(g);
            }
        }


        /// <summary>
        /// Event called when mouse up is operated on a grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void HelpForm_MouseUp(object sender, MouseEventArgs e) {
            if (!(sender is DataGrid)) return;
            var g = (DataGrid) sender;
            var d = g.DataSource as DataSet;
            var T = d?.Tables[g.DataMember];
            if (T == null) return;

            var myHitTest = g.HitTest(e.X, e.Y);
            if (myHitTest.Type == DataGrid.HitTestType.Cell) {
                int row = myHitTest.Row;
                if (!g.IsSelected(row)) {
                    ClearSelection(g);
                    //if (HelpForm.GetAllowMultiSelection(T)) 
                    GridSelectRow(g, row);
                }
                else {
                    if (GetAllowMultiSelection(T))
                        g.UnSelect(row);
                }
            }
            else {
                int row = myHitTest.Row;
                ClearSelection(g);
                if (row != -1) {
                    if (!GetAllowMultiSelection(T)) GridSelectRow(g, row);
                }
            }


        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Implements Idisposable interface
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                    if (tip != null) {
                        tip.Dispose();
                        tip = null;
                    }
                    if (ExcelMenu != null) {
                        ExcelMenu.Dispose();
                        ExcelMenu = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HelpForm() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// This code added to correctly implement the disposable pattern. 
        /// </summary>
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion


    }

}

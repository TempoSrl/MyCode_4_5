using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;
using LM = mdl_language.LanguageManager;
//user defined
using Xceed;
using Xceed.Grid;
using Xceed.Grid.Editors;
using q = mdl.MetaExpression;
using static mdl_winform.HelpForm;
using mdl;
using mdl_utils;
using mdl_windows;
namespace mdl_winform {
	

    /// <summary>
    /// Summary description for FormCustomViewList.
    /// </summary>
    class FormCustomViewList : System.Windows.Forms.Form {

        private CQueryHelper QHC = new CQueryHelper();

        #region Dichiarazione variabili e controlli
        //contiene i dati del listytpe corrente, utilizzato nei tab Colonne, Selezione, Ordinamento
        //private System.ComponentModel.IContainer components;

        //user defined
        private const string C_TAB_CUSTOMVIEW = "customview";
		private const string C_TAB_CUSTOMVIEWCOLUMN = "customviewcolumn";
		private const string C_TAB_CUSTOMVIEWWHERE = "customviewwhere";
		private const string C_TAB_CUSTOMVIEWORDERBY = "customvieworderby";
        private  string C_MSG_SAVE = LM.askSaveChanges;// "Salvare le modifiche?";
		private  string C_MSG_TITLE_SAVE = LM.translate("saving");
		private  string C_MSG_TITLE_EXCLAMATION = LM.warningLabel;
		private string C_MSG_DELETE = LM.confirmDeleting;
		private  string C_MSG_TITLE_DELETE = LM.translate("delete",true);
		private  string C_MSG_TITLE_ERRORE = LM.ErrorTitle;
		/// <summary>
		/// DataTable containing data displayed in the list
		/// </summary>
		public DataTable DT;

		/// <summary>
		/// True when form is active
		/// </summary>
		public bool running;

		private IWinFormMetaData m_linked;
        
        private MetaData m_linkedview;
        private string m_listtype;		//current listtype
        private string m_top ;
        //private string m_lastlisttype;	//pevious listtype (se creato uno nuovo)

        /// <summary>
        ///true se il listtype è di sistema 
        /// </summary>
        private bool m_listtypeissystem; //
		private string m_basefilter;	//filtro passato al costruttore
		private string m_basesorting;   //sorting passato al costruttore
		//private string m_filter;		//eventuale filtro personalizzato
		private string m_sorting;		//eventuale sorting personalizzato
		private System.Data.DataRow m_LastSelectRowGridElenchi;
		private System.Data.DataRow m_LastSelectRowGridSelezione;
		private System.Data.DataRow m_LastSelectRowGridOrderBy;
		private bool update_enabled;
		private string m_tablename;
		private string m_columnList;
		private DataSet DSDati;				//utilizzato per la visualizzazione dei dati (tab Elenchi)
		private VistaFormCustomView DSCopy;	//utilizzato per la copia elenco
		private MetaData m_metaData;
		private ContextMenu mnuContextMenu;
        private MyMenuItem mnuItemFilter;
        private MyMenuItem mnuItemHide;
		private MyMenuItem mnuItemApply;
		private MyMenuItem mnuItemCopy;
		private MyMenuItem mnuItemDelete;
		private MyMenuItem mnuExpandAll;
		private MyMenuItem mnuCompressAll;

		//mi dice in che stato si trova il page selezione
		private string m_selectionstate;	
		//mi dice in che stato si trova il page ordinamento
		private string m_orderbystate;	
		private bool m_IAmAdmin;

		private System.Windows.Forms.TabPage tabElenco;
		private System.Windows.Forms.TabPage tabColonne;
		private System.Windows.Forms.TabPage tabSelect;
		private System.Windows.Forms.TabPage tabOrder;
		private System.Windows.Forms.ComboBox cboList;
		private Xceed.Grid.GridControl gridCol;
		private Xceed.Grid.DataRow dataRow1;
		private Xceed.Grid.ColumnManagerRow colManagerColonne;
		private System.Windows.Forms.Button btnValore;
		private System.Windows.Forms.TextBox txtValore;
		private System.Windows.Forms.CheckBox chkRuntime;
		private System.Windows.Forms.ListBox lbOperatore;
		private System.Windows.Forms.ListBox lbColonna;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Panel panelConnector;
		private System.Windows.Forms.RadioButton radioOR;
		private System.Windows.Forms.RadioButton radioAND;
		private System.Windows.Forms.Button btnDeleteCond;
		private System.Windows.Forms.Button btnAddCond;
		private System.Windows.Forms.Button btnDeleteAllCond;
		private System.Windows.Forms.Button btnSaveCond;
		private System.Windows.Forms.Button btnCancelCond;
		private Xceed.Grid.ColumnManagerRow columnManagerRow1;
		private Xceed.Grid.GridControl gridSelezione;
		private Xceed.Grid.DataRow rowTemplateSelect;
		private System.Windows.Forms.Button btnEditCond;
		private System.Windows.Forms.Button btnCancelOrderBy;
		private System.Windows.Forms.Button btnSaveOrderBy;
		private System.Windows.Forms.Button btnEditOrderBy;
		private System.Windows.Forms.Button btnDeleteAllOrderBy;
		private System.Windows.Forms.Button btnDeleteOrderBy;
		private System.Windows.Forms.Button btnAddOrderBy;
		private System.Windows.Forms.ListBox lbColOrderBy;
		private System.Windows.Forms.RadioButton radioASC;
		private System.Windows.Forms.RadioButton radioDESC;
		private Xceed.Grid.DataRow rowTemplateOrderBy;
		private System.Windows.Forms.Panel panelOrderBy;
		private Xceed.Grid.GridControl gridOrderBy;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox chkBaseFilter;
		private System.Windows.Forms.TextBox txtBaseFilter;
		private System.Windows.Forms.Button BtnExcel;
		private System.Windows.Forms.Button btnPreview;
		private System.Windows.Forms.Button btnPrint;


		/// <summary>
		/// True se DescribColumns è stato applicato su listing type
		/// (di sistema) corrente.
		/// </summary>
		bool DescribeColumnsApplied;
		bool filterlocked;
		/// <summary>
		/// Last DataRow Selected in the list
		/// </summary>
		public System.Data.DataRow LastSelectedRow;
		private mdl_winform.VistaFormCustomView DS;
		#endregion

		#region Dich. Controlli
		private System.Windows.Forms.TextBox txtBaseSorting;
		private System.Windows.Forms.CheckBox chkBaseSorting;
		bool UpdateFormDisabled;
		private Xceed.Grid.GridControl gridX;
		private Xceed.Grid.ColumnManagerRow columnManagerRow2;
		private Xceed.Grid.GroupByRow groupByRow1;
		private Xceed.Grid.DataRow dataRowTemplate1;
		private System.Windows.Forms.TabControl tabList;
		private Xceed.Grid.DataBoundColumn colobjectname2;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneobjectname;
		private Xceed.Grid.DataBoundColumn colviewname2;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneviewname;
		private Xceed.Grid.DataBoundColumn colcolnumber;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnecolnumber;
		private Xceed.Grid.DataBoundColumn colheading;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneheading;
		private Xceed.Grid.DataBoundColumn colcolwidth;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnecolwidth;
		private Xceed.Grid.DataBoundColumn colvisible;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnevisible;
		private Xceed.Grid.DataBoundColumn colfontname;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnefontname;
		private Xceed.Grid.DataBoundColumn colfontsize;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnefontsize;
		private Xceed.Grid.DataBoundColumn colbold;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnebold;
		private Xceed.Grid.DataBoundColumn colitalic;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneitalic;
		private Xceed.Grid.DataBoundColumn colunderline;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneunderline;
		private Xceed.Grid.DataBoundColumn colstrikeout;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnestrikeout;
		private Xceed.Grid.DataBoundColumn colcolor;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnecolor;
		private Xceed.Grid.DataBoundColumn colformat;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneformat;
		private Xceed.Grid.DataBoundColumn colisreal;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneisreal;
		private Xceed.Grid.DataBoundColumn colexpression;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonneexpression;
		private Xceed.Grid.DataBoundColumn colcolname;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnecolname;
		private Xceed.Grid.DataBoundColumn colsystemtype;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnesystemtype;
		private Xceed.Grid.DataBoundColumn collastmodtimestamp2;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnelastmodtimestamp;
		private Xceed.Grid.DataBoundColumn collastmoduser2;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnelastmoduser;
		private Xceed.Grid.DataBoundColumn colcreatetimestamp2;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnecreatetimestamp;
		private Xceed.Grid.DataBoundColumn colcreateuser2;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnecreateuser;
		private Xceed.Grid.DataBoundColumn collistcolpos;
		private Xceed.Grid.ColumnManagerCell cellcolManagerColonnelistcolpos;
		private Xceed.Grid.DataCell celldataRow1objectname;
		private Xceed.Grid.DataCell celldataRow1viewname;
		private Xceed.Grid.DataCell celldataRow1colnumber;
		private Xceed.Grid.DataCell celldataRow1heading;
		private Xceed.Grid.DataCell celldataRow1colwidth;
		private Xceed.Grid.DataCell celldataRow1visible;
		private Xceed.Grid.DataCell celldataRow1fontname;
		private Xceed.Grid.DataCell celldataRow1fontsize;
		private Xceed.Grid.DataCell celldataRow1bold;
		private Xceed.Grid.DataCell celldataRow1italic;
		private Xceed.Grid.DataCell celldataRow1underline;
		private Xceed.Grid.DataCell celldataRow1strikeout;
		private Xceed.Grid.DataCell celldataRow1color;
		private Xceed.Grid.DataCell celldataRow1format;
		private Xceed.Grid.DataCell celldataRow1isreal;
		private Xceed.Grid.DataCell celldataRow1expression;
		private Xceed.Grid.DataCell celldataRow1colname;
		private Xceed.Grid.DataCell celldataRow1systemtype;
		private Xceed.Grid.DataCell celldataRow1lastmodtimestamp;
		private Xceed.Grid.DataCell celldataRow1lastmoduser;
		private Xceed.Grid.DataCell celldataRow1createtimestamp;
		private Xceed.Grid.DataCell celldataRow1createuser;
        private Xceed.Grid.DataCell celldataRow1listcolpos;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1connector;
        private Xceed.Grid.DataCell cellrowTemplateSelectconnector;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1objectname;
        private Xceed.Grid.DataCell cellrowTemplateSelectobjectname;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1viewname;
        private Xceed.Grid.DataCell cellrowTemplateSelectviewname;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1periodnumber;
        private Xceed.Grid.DataCell cellrowTemplateSelectperiodnumber;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1columnname;
        private Xceed.Grid.DataCell cellrowTemplateSelectcolumnname;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1operator;
        private Xceed.Grid.DataCell cellrowTemplateSelectoperator;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1value;
        private Xceed.Grid.DataCell cellrowTemplateSelectvalue;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1runtime;
        private Xceed.Grid.DataCell cellrowTemplateSelectruntime;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1lastmodtimestamp;
        private Xceed.Grid.DataCell cellrowTemplateSelectlastmodtimestamp;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1lastmoduser;
        private Xceed.Grid.DataCell cellrowTemplateSelectlastmoduser;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1createtimestamp;
        private Xceed.Grid.DataCell cellrowTemplateSelectcreatetimestamp;
		private Xceed.Grid.ColumnManagerCell cellcolumnManagerRow1createuser;
		private Xceed.Grid.DataCell cellrowTemplateSelectcreateuser;
		private Xceed.Grid.DataBoundColumn colobjectname;
		private Xceed.Grid.DataCell cellrowTemplateOrderByobjectname;
		private Xceed.Grid.DataBoundColumn colviewname;
		private Xceed.Grid.DataCell cellrowTemplateOrderByviewname;
		private Xceed.Grid.DataBoundColumn colperiodnumber;
		private Xceed.Grid.DataCell cellrowTemplateOrderByperiodnumber;
		private Xceed.Grid.DataBoundColumn colcolumnname;
		private Xceed.Grid.DataCell cellrowTemplateOrderBycolumnname;
		private Xceed.Grid.DataBoundColumn coldirection;
		private Xceed.Grid.DataCell cellrowTemplateOrderBydirection;
		private Xceed.Grid.DataBoundColumn collastmodtimestamp;
		private Xceed.Grid.DataCell cellrowTemplateOrderBylastmodtimestamp;
		private Xceed.Grid.DataBoundColumn collastmoduser;
		private Xceed.Grid.DataCell cellrowTemplateOrderBylastmoduser;
		private Xceed.Grid.DataBoundColumn colcreatetimestamp;
		private Xceed.Grid.DataCell cellrowTemplateOrderBycreatetimestamp;
		private Xceed.Grid.DataBoundColumn colcreateuser;
		private Xceed.Grid.DataCell cellrowTemplateOrderBycreateuser;
		private Xceed.Grid.VisualGridElementStyle visualGridElementStyle1;
		private Xceed.Grid.VisualGridElementStyle visualGridElementStyle2;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.Button btnCopy;
		private System.Windows.Forms.TabPage tabGestione;
		private System.Windows.Forms.TabPage TabSommatorie;
		private System.Windows.Forms.CheckBox chkSommatorie;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.CheckedListBox lbsumfield;
		private System.Windows.Forms.CheckBox chkOttimizzaSomme;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		#endregion
		private System.Windows.Forms.Label label7;
        private DataBoundColumn colobjectname1;
        private DataBoundColumn colviewname1;
        private DataBoundColumn colperiodnumber1;
        private DataBoundColumn colconnector;
        private DataBoundColumn colcolumnname1;
        private DataBoundColumn coloperator;
        private DataBoundColumn colvalue;
        private DataBoundColumn colruntime;
        private DataBoundColumn collastmodtimestamp1;
        private DataBoundColumn collastmoduser1;
        private DataBoundColumn colcreatetimestamp1;
        private DataBoundColumn colcreateuser1;
       
        private DataBoundColumn colobjectname3;
        private DataBoundColumn colviewname3;
        private DataBoundColumn colcolnumber1;
        private DataBoundColumn colheading1;
        private DataBoundColumn colcolwidth1;
        private DataBoundColumn colvisible1;
        private DataBoundColumn colfontname1;
        private DataBoundColumn colfontsize1;
        private DataBoundColumn colbold1;
        private DataBoundColumn colitalic1;
        private DataBoundColumn colunderline1;
        private DataBoundColumn colstrikeout1;
        private DataBoundColumn colcolor1;
        private DataBoundColumn colformat1;
        private DataBoundColumn colisreal1;
        private DataBoundColumn colexpression1;
        private DataBoundColumn colcolname1;
        private DataBoundColumn colsystemtype1;
        private DataBoundColumn collastmodtimestamp3;
        private DataBoundColumn collastmoduser3;
        private DataBoundColumn colcreatetimestamp3;
        private DataBoundColumn colcreateuser3;
        private DataBoundColumn collistcolpos1;
       
        private DataBoundColumn colobjectname4;
        private DataBoundColumn colviewname4;
        private DataBoundColumn colperiodnumber2;
        private DataBoundColumn colcolumnname2;
        private DataBoundColumn coldirection1;
        private DataBoundColumn collastmodtimestamp4;
        private DataBoundColumn collastmoduser4;
        private DataBoundColumn colcreatetimestamp4;
        private DataBoundColumn colcreateuser4;
      
        private Label label8;
        private ComboBox comboTOP;
        private ToolTip toolTip1;
        private IContainer components;
        private Button btnCsv;
        private DataBoundColumn colobjectname5;
        private DataBoundColumn colviewname5;
        private DataBoundColumn colperiodnumber3;
        private DataBoundColumn colconnector1;
        private DataBoundColumn colcolumnname3;
        private DataBoundColumn coloperator1;
        private DataBoundColumn colvalue1;
        private DataBoundColumn colruntime1;
        private DataBoundColumn collastmodtimestamp5;
        private DataBoundColumn collastmoduser5;
        private DataBoundColumn colcreatetimestamp5;
        private DataBoundColumn colcreateuser5;
        
        private DataBoundColumn colobjectname6;
        private DataBoundColumn colviewname6;
        private DataBoundColumn colcolnumber2;
        private DataBoundColumn colheading2;
        private DataBoundColumn colcolwidth2;
        private DataBoundColumn colvisible2;
        private DataBoundColumn colfontname2;
        private DataBoundColumn colfontsize2;
        private DataBoundColumn colbold2;
        private DataBoundColumn colitalic2;
        private DataBoundColumn colunderline2;
        private DataBoundColumn colstrikeout2;
        private DataBoundColumn colcolor2;
        private DataBoundColumn colformat2;
        private DataBoundColumn colisreal2;
        private DataBoundColumn colexpression2;
        private DataBoundColumn colcolname2;
        private DataBoundColumn colsystemtype2;
        private DataBoundColumn collastmodtimestamp6;
        private DataBoundColumn collastmoduser6;
        private DataBoundColumn colcreatetimestamp6;
        private DataBoundColumn colcreateuser6;
        private DataBoundColumn collistcolpos2;    
        private DataCell celldataRow1colnumber2;
      
        private DataBoundColumn colobjectname7;
        private DataBoundColumn colviewname7;
        private DataBoundColumn colperiodnumber4;
        private DataBoundColumn colcolumnname4;
        private DataBoundColumn coldirection2;
        private DataBoundColumn collastmodtimestamp7;
        private DataBoundColumn collastmoduser7;
        private DataBoundColumn colcreatetimestamp7;
        private DataBoundColumn colcreateuser7;

        private IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();
		DataTable ToMerge;
        bool destroyed = false;

        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Destroy() {
            if (destroyed) return;
            var FParent = this.ParentForm;
            if (FParent != null) FParent.RemoveOwnedForm(this);
            if (toolTip1 != null) {
                toolTip1.Dispose();
                toolTip1 = null;
            }
            if (comboTOP != null) {
                comboTOP.DataSource = null;
                comboTOP = null;
            }

            if (DS != null) {
                DS.Clear();
                DS.Dispose();
                DS = null;
            }


            if (DSCopy != null) {
                DSCopy.Clear();
                DSCopy.Dispose();
                DSCopy = null;                
            }
            
            //if (DT != null) {
            //    DT.Clear();
            //    DT = null;
            //}
            //if (DSDati != null) {
            //    DSDati.Clear();
            //    DSDati.Dispose();
            //    DSDati = null;
            //}
            
            if (gridX.DataBindings != null) {
                gridX.DataBindings.Clear();
            }
            try {
                this.gridX.Clear();
                this.gridX.Dispose();
                this.gridX = null;
            }
            catch { }
            lbColonna.DataSource = null;
            lbColOrderBy.DataSource = null;
            lbOperatore.DataSource = null;
            

            gridSelezione.Columns.Clear();
            this.gridSelezione.Clear();
            this.gridSelezione.Dispose();
            this.gridSelezione = null;

            this.gridOrderBy.Clear();
            this.gridOrderBy.Dispose();
            this.gridOrderBy = null;
            this.ToMerge = null;
            if (this.ColumnsToRestore != null) {
                this.ColumnsToRestore.Clear();
                this.ColumnsToRestore = null;
            }
            //columnManagerRow2.Cells.Clear();
            //columnManagerRow1.Cells.Clear();
            if (m_linked != null) {
                controller.currentListForm = null;
                m_linked = null;
            }
            
            if (m_linkedview != null) {
                m_linkedview.Destroy();
                m_linkedview = null;
            }

            if (mnuContextMenu != null) {
                mnuContextMenu.Dispose();
                mnuContextMenu = null;
            }
            dataRowTemplate1 = null;
            rowTemplateSelect = null;

            destroyed = true;
        }

        /// <summary>
        /// MetaModel used
        /// </summary>
        public IMetaModel metaModel = MetaFactory.factory.getSingleton<IMetaModel>();

        static FormCustomViewList() {
          
        }
        void setColor(Xceed.Grid.GridControl g) {
            g.BackColor = formcolors.GridBackColor();
            g.ForeColor = formcolors.GridForeColor();
            g.SelectionBackColor = formcolors.GridSelectionBackColor();
            g.SelectionForeColor = formcolors.GridSelectionForeColor();
            g.RowSelectorPane.BackColor = formcolors.GridHeaderBackColor();
            g.RowSelectorPane.ForeColor = formcolors.GridHeaderForeColor();            
            g.InactiveSelectionBackColor = formcolors.GridHeaderBackColor();
            g.InactiveSelectionForeColor = formcolors.GridHeaderForeColor();

            g.GroupAdded += g_GroupAdded;
        }
        bool inside ;

        void g_GroupAdded(object sender, GroupAddedEventArgs e) {
            if (destroyed) return;

            if (inside)
                return;
            inside = true;
            foreach (Group r in ((GridControl)sender).Groups) {
                cascadeUpdateGroup(r);
            }
            //((Xceed.Grid.GridControl)sender).UpdateGrouping();
            inside = false;
            //foreach (Xceed.Grid.Group r in ((Xceed.Grid.GridControl)sender).Groups) {

            
            //    r.BackColor = formcolors.GridHeaderBackColor();
            //    r.ForeColor = formcolors.GridHeaderForeColor();

            //}

        }

        void cascadeUpdateGroup(Group gr) {
            gr.BackColor = formcolors.GridHeaderBackColor();
            gr.ForeColor = formcolors.GridHeaderForeColor();

            foreach (Row hr in gr.HeaderRows) {
                hr.BackColor = formcolors.GridHeaderBackColor();
                hr.ForeColor = formcolors.GridHeaderForeColor();
            }
            foreach (Group gr2 in gr.Groups)
                cascadeUpdateGroup(gr2);
        }

        //void g_GroupingUpdated(object sender, EventArgs e) {
        //    GridControl g = sender as GridControl;
        //    foreach (Group gr in g.Groups) {
        //        gr.BackColor = formcolors.GridHeaderBackColor();
        //        gr.ForeColor = formcolors.GridHeaderForeColor();                  
        //    }
        //    g.UpdateGrouping();
        //}

        private IFormController controller;
        IMetaDataDispatcher dispatcher;
        IDataAccess conn;

        void inizializza(IWinFormMetaData linked, string columnlist,
            string filter,
            string tablename,
            string listtype,
            DataTable ToMerge,
            string sorting,
            bool filterlocked,
            int top) {
            //int startinit = metaprofiler.StartTimer("INIT XCEED GRID");

            this.ToMerge = ToMerge;
            this.filterlocked = filterlocked;
            LastSelectedRow = null;
            UpdateFormDisabled = false;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            utils.SetColorOneTime(this, true);

            setColor(gridX);
            setColor(gridCol);
            setColor(gridSelezione);
            setColor(gridOrderBy);

            gridX.TabStop = true;
            gridX.TabIndex = 1;
            visualGridElementStyle1.ForeColor = formcolors.GridForeColor();
            visualGridElementStyle1.BackColor= formcolors.GridBackColor();
            visualGridElementStyle2.ForeColor = formcolors.GridForeColor();
            visualGridElementStyle2.BackColor = formcolors.GridAlternatingBackColor();
           
            columnManagerRow1.BackColor = formcolors.GridHeaderBackColor();
            columnManagerRow1.ForeColor = formcolors.GridHeaderForeColor();
            columnManagerRow2.BackColor = formcolors.GridHeaderBackColor();
            columnManagerRow2.ForeColor = formcolors.GridHeaderForeColor();
            groupByRow1.BackColor = formcolors.MainBackColor();
            groupByRow1.ForeColor = formcolors.MainForeColor();
            groupByRow1.GroupTemplate.BackColor = formcolors.GridHeaderBackColor();
            groupByRow1.GroupTemplate.ForeColor = formcolors.GridHeaderForeColor();

            PostData.MarkAsTemporaryTable(DS.fieldtosum, false);

            
            new listBoxTableManager(model, DS.customoperator, lbOperatore);
            new listBoxTableManager(model, DS.customviewcolumn, lbColOrderBy);
            new listBoxTableManager(model, DS.customviewcolumn, lbColonna);

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            Cursor.Current = Cursors.WaitCursor;
         
            m_linked = linked;
            controller = linked.controller;
            conn = controller.conn;

            dispatcher = controller.dispatcher;
            m_linkedview = dispatcher.Get(tablename);
            if (dispatcher.unrecoverableError) {
                controller.ErroreIrrecuperabile = true;
                shower.ShowError(controller.linkedForm,LM.errorLoadingMeta(tablename), LM.ErrorTitle);
            }
            if (columnlist.Trim() == "*") {
                DataTable DT2 = conn.CreateTableByName(tablename, columnlist);
                m_linkedview.DescribeColumns(DT2, listtype);
                columnlist = QueryCreator.SortedColumnNameList(DT2);

                if (DT2.Columns.Count == 0) {
                    shower.Show(this,"Non sono disponibile le informazioni relative alle colonne di " +
                        tablename + ". Questo può accadere a causa di una erronea installazione. " +
                        "Ad esempio, non è stato eseguito AnalizzaStruttura.", "Errore");
                }

            }



            //m_listtype = listtype;
            m_columnList = columnlist;
            m_tablename = tablename;


            //All'inizio il customfilter coincide con il filtro passato al costruttore
            m_basefilter = filter;
            //m_filter = filter;
            if (top >= 0) {
                comboTOP.Text = top.ToString();
            }
            else {
                comboTOP.Text = "";
            }

            if (sorting == null) sorting = "";
            m_sorting = sorting;
            m_basesorting = sorting;
            running = true;
            m_LastSelectRowGridElenchi = null;
            m_LastSelectRowGridSelezione = null;

            createContextMenu();

            //tab page elenchi
            gridX.ReadOnly = true;
            update_enabled = true;

            restartWithNewListType(listtype);

            //Impostazioni statiche dei grid (incluso binding con DS)
            impostaTabsImpostazioni();

            Cursor.Current = Cursors.Default;

            //per default il textbox txtBaseFilter è disabilitato
            //se sono amministratore viene abilitato
            if (controller.security.GetSys("IsSystemAdmnin") != null) {
                m_IAmAdmin = Convert.ToBoolean(controller.security.GetSys("IsSystemAdmin"));
                if (m_IAmAdmin) {
                    txtBaseFilter.Enabled = true;
                    txtBaseSorting.Enabled = true;
                }
            }

            txtBaseFilter.Text = m_basefilter;
            txtBaseSorting.Text = m_basesorting;
            chkBaseFilter.Checked = !string.IsNullOrEmpty(m_basefilter);

            if (filterlocked) chkBaseFilter.Enabled = false;
            //	metaprofiler.StopTimer(startinit);
        }

        /// <summary>
        /// Creates a list
        /// </summary>
        /// <param name="linked">Linked metadata</param>
        /// <param name="columnlist">list of columns to be read</param>
        /// <param name="filter">filter to apply</param>
        /// <param name="tablename">table name for reading data</param>
        /// <param name="listtype">listtype to use</param>
        /// <param name="ToMerge">A table to be merged with results from db</param>
        /// <param name="sorting">sorting to appy to list</param>
        /// <param name="filterlocked">When true, user is not allowed to change list filter</param>
        /// <param name="top"></param>
        public FormCustomViewList(IWinFormMetaData linked, string columnlist,
            string filter,
            string tablename,
            string listtype,
            DataTable ToMerge,
            string sorting,
            bool filterlocked,
            int top=1000) {
            inizializza(linked, columnlist, filter, tablename, listtype, ToMerge, sorting, filterlocked,top);
        }
		

		/// <summary>
		/// Seleziona un nuovo listing type e riempie il combo dei listing type.
		/// </summary>
		/// <param name="listtype"></param>
		void restartWithNewListType(string listtype){
			selectNewListType(listtype,true);
			cboList.Enabled=false;
			fillComboList();
			//imposto l'ambiente sul listtype passato al costruttore
			if (listtype!=null)
				cboList.SelectedValue = listtype;
			if (!filterlocked) cboList.Enabled=true;
			

		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
                if (components != null) {
                    components.Dispose();
                }

                Destroy();
			}
			base.Dispose( disposing );
		}



		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.tabElenco = new System.Windows.Forms.TabPage();
            this.btnCsv = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.comboTOP = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.gridX = new Xceed.Grid.GridControl();
            this.dataRowTemplate1 = new Xceed.Grid.DataRow();
            this.visualGridElementStyle1 = new Xceed.Grid.VisualGridElementStyle();
            this.visualGridElementStyle2 = new Xceed.Grid.VisualGridElementStyle();
            this.groupByRow1 = new Xceed.Grid.GroupByRow();
            this.columnManagerRow2 = new Xceed.Grid.ColumnManagerRow();
            this.btnPreview = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.BtnExcel = new System.Windows.Forms.Button();
            this.cboList = new System.Windows.Forms.ComboBox();
            this.tabSelect = new System.Windows.Forms.TabPage();
            this.txtBaseFilter = new System.Windows.Forms.TextBox();
            this.chkBaseFilter = new System.Windows.Forms.CheckBox();
            this.gridSelezione = new Xceed.Grid.GridControl();
            this.rowTemplateSelect = new Xceed.Grid.DataRow();
            this.DS = new VistaFormCustomView();
            this.columnManagerRow1 = new Xceed.Grid.ColumnManagerRow();
            this.btnCancelCond = new System.Windows.Forms.Button();
            this.btnSaveCond = new System.Windows.Forms.Button();
            this.btnEditCond = new System.Windows.Forms.Button();
            this.btnDeleteAllCond = new System.Windows.Forms.Button();
            this.panelConnector = new System.Windows.Forms.Panel();
            this.radioOR = new System.Windows.Forms.RadioButton();
            this.radioAND = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnValore = new System.Windows.Forms.Button();
            this.txtValore = new System.Windows.Forms.TextBox();
            this.chkRuntime = new System.Windows.Forms.CheckBox();
            this.lbOperatore = new System.Windows.Forms.ListBox();
            this.lbColonna = new System.Windows.Forms.ListBox();
            this.btnDeleteCond = new System.Windows.Forms.Button();
            this.btnAddCond = new System.Windows.Forms.Button();
            this.colobjectname5 = new Xceed.Grid.DataBoundColumn();
            this.colviewname5 = new Xceed.Grid.DataBoundColumn();
            this.colperiodnumber3 = new Xceed.Grid.DataBoundColumn();
            this.colconnector1 = new Xceed.Grid.DataBoundColumn();
            this.colcolumnname3 = new Xceed.Grid.DataBoundColumn();
            this.coloperator1 = new Xceed.Grid.DataBoundColumn();
            this.colvalue1 = new Xceed.Grid.DataBoundColumn();
            this.colruntime1 = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp5 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser5 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp5 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser5 = new Xceed.Grid.DataBoundColumn();
            this.colobjectname1 = new Xceed.Grid.DataBoundColumn();
            this.colviewname1 = new Xceed.Grid.DataBoundColumn();
            this.colperiodnumber1 = new Xceed.Grid.DataBoundColumn();
            this.colconnector = new Xceed.Grid.DataBoundColumn();
            this.colcolumnname1 = new Xceed.Grid.DataBoundColumn();
            this.coloperator = new Xceed.Grid.DataBoundColumn();
            this.colvalue = new Xceed.Grid.DataBoundColumn();
            this.colruntime = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp1 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser1 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp1 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser1 = new Xceed.Grid.DataBoundColumn();
            this.cellrowTemplateSelectconnector = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectobjectname = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectviewname = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectperiodnumber = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectcolumnname = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectoperator = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectvalue = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectruntime = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectlastmodtimestamp = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectlastmoduser = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectcreatetimestamp = new Xceed.Grid.DataCell();
            this.cellrowTemplateSelectcreateuser = new Xceed.Grid.DataCell();
            this.cellcolumnManagerRow1connector = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1objectname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1viewname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1periodnumber = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1columnname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1operator = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1value = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1runtime = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1lastmodtimestamp = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1lastmoduser = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1createtimestamp = new Xceed.Grid.ColumnManagerCell();
            this.cellcolumnManagerRow1createuser = new Xceed.Grid.ColumnManagerCell();
            this.tabColonne = new System.Windows.Forms.TabPage();
            this.gridCol = new Xceed.Grid.GridControl();
            this.dataRow1 = new Xceed.Grid.DataRow();
            this.colManagerColonne = new Xceed.Grid.ColumnManagerRow();
            this.colobjectname6 = new Xceed.Grid.DataBoundColumn();
            this.colviewname6 = new Xceed.Grid.DataBoundColumn();
            this.colcolnumber2 = new Xceed.Grid.DataBoundColumn();
            this.colheading2 = new Xceed.Grid.DataBoundColumn();
            this.colcolwidth2 = new Xceed.Grid.DataBoundColumn();
            this.colvisible2 = new Xceed.Grid.DataBoundColumn();
            this.colfontname2 = new Xceed.Grid.DataBoundColumn();
            this.colfontsize2 = new Xceed.Grid.DataBoundColumn();
            this.colbold2 = new Xceed.Grid.DataBoundColumn();
            this.colitalic2 = new Xceed.Grid.DataBoundColumn();
            this.colunderline2 = new Xceed.Grid.DataBoundColumn();
            this.colstrikeout2 = new Xceed.Grid.DataBoundColumn();
            this.colcolor2 = new Xceed.Grid.DataBoundColumn();
            this.colformat2 = new Xceed.Grid.DataBoundColumn();
            this.colisreal2 = new Xceed.Grid.DataBoundColumn();
            this.colexpression2 = new Xceed.Grid.DataBoundColumn();
            this.colcolname2 = new Xceed.Grid.DataBoundColumn();
            this.colsystemtype2 = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp6 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser6 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp6 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser6 = new Xceed.Grid.DataBoundColumn();
            this.collistcolpos2 = new Xceed.Grid.DataBoundColumn();
            this.celldataRow1colnumber2 = new Xceed.Grid.DataCell();
            this.colobjectname3 = new Xceed.Grid.DataBoundColumn();
            this.colviewname3 = new Xceed.Grid.DataBoundColumn();
            this.colcolnumber1 = new Xceed.Grid.DataBoundColumn();
            this.colheading1 = new Xceed.Grid.DataBoundColumn();
            this.colcolwidth1 = new Xceed.Grid.DataBoundColumn();
            this.colvisible1 = new Xceed.Grid.DataBoundColumn();
            this.colfontname1 = new Xceed.Grid.DataBoundColumn();
            this.colfontsize1 = new Xceed.Grid.DataBoundColumn();
            this.colbold1 = new Xceed.Grid.DataBoundColumn();
            this.colitalic1 = new Xceed.Grid.DataBoundColumn();
            this.colunderline1 = new Xceed.Grid.DataBoundColumn();
            this.colstrikeout1 = new Xceed.Grid.DataBoundColumn();
            this.colcolor1 = new Xceed.Grid.DataBoundColumn();
            this.colformat1 = new Xceed.Grid.DataBoundColumn();
            this.colisreal1 = new Xceed.Grid.DataBoundColumn();
            this.colexpression1 = new Xceed.Grid.DataBoundColumn();
            this.colcolname1 = new Xceed.Grid.DataBoundColumn();
            this.colsystemtype1 = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp3 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser3 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp3 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser3 = new Xceed.Grid.DataBoundColumn();
            this.collistcolpos1 = new Xceed.Grid.DataBoundColumn();
            this.colobjectname2 = new Xceed.Grid.DataBoundColumn();
            this.colviewname2 = new Xceed.Grid.DataBoundColumn();
            this.colcolnumber = new Xceed.Grid.DataBoundColumn();
            this.colheading = new Xceed.Grid.DataBoundColumn();
            this.colcolwidth = new Xceed.Grid.DataBoundColumn();
            this.colvisible = new Xceed.Grid.DataBoundColumn();
            this.colfontname = new Xceed.Grid.DataBoundColumn();
            this.colfontsize = new Xceed.Grid.DataBoundColumn();
            this.colbold = new Xceed.Grid.DataBoundColumn();
            this.colitalic = new Xceed.Grid.DataBoundColumn();
            this.colunderline = new Xceed.Grid.DataBoundColumn();
            this.colstrikeout = new Xceed.Grid.DataBoundColumn();
            this.colcolor = new Xceed.Grid.DataBoundColumn();
            this.colformat = new Xceed.Grid.DataBoundColumn();
            this.colisreal = new Xceed.Grid.DataBoundColumn();
            this.colexpression = new Xceed.Grid.DataBoundColumn();
            this.colcolname = new Xceed.Grid.DataBoundColumn();
            this.colsystemtype = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp2 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser2 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp2 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser2 = new Xceed.Grid.DataBoundColumn();
            this.collistcolpos = new Xceed.Grid.DataBoundColumn();
            this.celldataRow1objectname = new Xceed.Grid.DataCell();
            this.celldataRow1viewname = new Xceed.Grid.DataCell();
            this.celldataRow1colnumber = new Xceed.Grid.DataCell();
            this.celldataRow1heading = new Xceed.Grid.DataCell();
            this.celldataRow1colwidth = new Xceed.Grid.DataCell();
            this.celldataRow1visible = new Xceed.Grid.DataCell();
            this.celldataRow1fontname = new Xceed.Grid.DataCell();
            this.celldataRow1fontsize = new Xceed.Grid.DataCell();
            this.celldataRow1bold = new Xceed.Grid.DataCell();
            this.celldataRow1italic = new Xceed.Grid.DataCell();
            this.celldataRow1underline = new Xceed.Grid.DataCell();
            this.celldataRow1strikeout = new Xceed.Grid.DataCell();
            this.celldataRow1color = new Xceed.Grid.DataCell();
            this.celldataRow1format = new Xceed.Grid.DataCell();
            this.celldataRow1isreal = new Xceed.Grid.DataCell();
            this.celldataRow1expression = new Xceed.Grid.DataCell();
            this.celldataRow1colname = new Xceed.Grid.DataCell();
            this.celldataRow1systemtype = new Xceed.Grid.DataCell();
            this.celldataRow1lastmodtimestamp = new Xceed.Grid.DataCell();
            this.celldataRow1lastmoduser = new Xceed.Grid.DataCell();
            this.celldataRow1createtimestamp = new Xceed.Grid.DataCell();
            this.celldataRow1createuser = new Xceed.Grid.DataCell();
            this.celldataRow1listcolpos = new Xceed.Grid.DataCell();
            this.cellcolManagerColonneobjectname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneviewname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnecolnumber = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneheading = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnecolwidth = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnevisible = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnefontname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnefontsize = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnebold = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneitalic = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneunderline = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnestrikeout = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnecolor = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneformat = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneisreal = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonneexpression = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnecolname = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnesystemtype = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnelastmodtimestamp = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnelastmoduser = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnecreatetimestamp = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnecreateuser = new Xceed.Grid.ColumnManagerCell();
            this.cellcolManagerColonnelistcolpos = new Xceed.Grid.ColumnManagerCell();
            this.tabOrder = new System.Windows.Forms.TabPage();
            this.txtBaseSorting = new System.Windows.Forms.TextBox();
            this.chkBaseSorting = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.gridOrderBy = new Xceed.Grid.GridControl();
            this.rowTemplateOrderBy = new Xceed.Grid.DataRow();
            this.panelOrderBy = new System.Windows.Forms.Panel();
            this.radioDESC = new System.Windows.Forms.RadioButton();
            this.radioASC = new System.Windows.Forms.RadioButton();
            this.lbColOrderBy = new System.Windows.Forms.ListBox();
            this.btnCancelOrderBy = new System.Windows.Forms.Button();
            this.btnSaveOrderBy = new System.Windows.Forms.Button();
            this.btnEditOrderBy = new System.Windows.Forms.Button();
            this.btnDeleteAllOrderBy = new System.Windows.Forms.Button();
            this.btnDeleteOrderBy = new System.Windows.Forms.Button();
            this.btnAddOrderBy = new System.Windows.Forms.Button();
            this.colobjectname7 = new Xceed.Grid.DataBoundColumn();
            this.colviewname7 = new Xceed.Grid.DataBoundColumn();
            this.colperiodnumber4 = new Xceed.Grid.DataBoundColumn();
            this.colcolumnname4 = new Xceed.Grid.DataBoundColumn();
            this.coldirection2 = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp7 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser7 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp7 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser7 = new Xceed.Grid.DataBoundColumn();
            this.colobjectname4 = new Xceed.Grid.DataBoundColumn();
            this.colviewname4 = new Xceed.Grid.DataBoundColumn();
            this.colperiodnumber2 = new Xceed.Grid.DataBoundColumn();
            this.colcolumnname2 = new Xceed.Grid.DataBoundColumn();
            this.coldirection1 = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp4 = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser4 = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp4 = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser4 = new Xceed.Grid.DataBoundColumn();
            this.colobjectname = new Xceed.Grid.DataBoundColumn();
            this.colviewname = new Xceed.Grid.DataBoundColumn();
            this.colperiodnumber = new Xceed.Grid.DataBoundColumn();
            this.colcolumnname = new Xceed.Grid.DataBoundColumn();
            this.coldirection = new Xceed.Grid.DataBoundColumn();
            this.collastmodtimestamp = new Xceed.Grid.DataBoundColumn();
            this.collastmoduser = new Xceed.Grid.DataBoundColumn();
            this.colcreatetimestamp = new Xceed.Grid.DataBoundColumn();
            this.colcreateuser = new Xceed.Grid.DataBoundColumn();
            this.cellrowTemplateOrderByobjectname = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderByviewname = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderByperiodnumber = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderBycolumnname = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderBydirection = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderBylastmodtimestamp = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderBylastmoduser = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderBycreatetimestamp = new Xceed.Grid.DataCell();
            this.cellrowTemplateOrderBycreateuser = new Xceed.Grid.DataCell();
            this.tabList = new System.Windows.Forms.TabControl();
            this.TabSommatorie = new System.Windows.Forms.TabPage();
            this.chkOttimizzaSomme = new System.Windows.Forms.CheckBox();
            this.lbsumfield = new System.Windows.Forms.CheckedListBox();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chkSommatorie = new System.Windows.Forms.CheckBox();
            this.tabGestione = new System.Windows.Forms.TabPage();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabElenco.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataRowTemplate1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnManagerRow2)).BeginInit();
            this.tabSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSelezione)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowTemplateSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnManagerRow1)).BeginInit();
            this.panelConnector.SuspendLayout();
            this.tabColonne.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataRow1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.colManagerColonne)).BeginInit();
            this.tabOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOrderBy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowTemplateOrderBy)).BeginInit();
            this.panelOrderBy.SuspendLayout();
            this.tabList.SuspendLayout();
            this.TabSommatorie.SuspendLayout();
            this.tabGestione.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabElenco
            // 
            this.tabElenco.AllowDrop = true;
            this.tabElenco.Controls.Add(this.btnCsv);
            this.tabElenco.Controls.Add(this.label8);
            this.tabElenco.Controls.Add(this.comboTOP);
            this.tabElenco.Controls.Add(this.label7);
            this.tabElenco.Controls.Add(this.gridX);
            this.tabElenco.Controls.Add(this.btnPreview);
            this.tabElenco.Controls.Add(this.btnPrint);
            this.tabElenco.Controls.Add(this.BtnExcel);
            this.tabElenco.Controls.Add(this.cboList);
            this.tabElenco.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabElenco.Location = new System.Drawing.Point(4, 22);
            this.tabElenco.Name = "tabElenco";
            this.tabElenco.Size = new System.Drawing.Size(942, 459);
            this.tabElenco.TabIndex = 0;
            this.tabElenco.Text = "Elenchi";
            this.tabElenco.Click += new System.EventHandler(this.tabElenco_Click);
            // 
            // btnCsv
            // 
            this.btnCsv.Location = new System.Drawing.Point(520, 3);
            this.btnCsv.Name = "btnCsv";
            this.btnCsv.Size = new System.Drawing.Size(101, 23);
            this.btnCsv.TabIndex = 19;
            this.btnCsv.Text = "Esporta in CSV";
            this.btnCsv.Click += new System.EventHandler(this.btnCsv_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(280, 7);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(34, 14);
            this.label8.TabIndex = 18;
            this.label8.Text = "Limite";
            // 
            // comboTOP
            // 
            this.comboTOP.FormattingEnabled = true;
            this.comboTOP.Items.AddRange(new object[] {
            "100",
            "500",
            "1000"});
            this.comboTOP.Location = new System.Drawing.Point(320, 2);
            this.comboTOP.Name = "comboTOP";
            this.comboTOP.Size = new System.Drawing.Size(90, 22);
            this.comboTOP.TabIndex = 17;
            this.comboTOP.TabStop = false;
            this.comboTOP.Text = "1000";
            this.toolTip1.SetToolTip(this.comboTOP, "Imposta il numero massimo di righe restituite per la ricerca. Se si desidrea otte" +
        "nere tutte le righe lasciare vuota questa casella");
            this.comboTOP.SelectedValueChanged += new System.EventHandler(this.comboTOP_SelectedValueChanged);
            this.comboTOP.Leave += new System.EventHandler(this.comboTOP_Leave);
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(10, 3);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 20);
            this.label7.TabIndex = 16;
            this.label7.Text = "Elenco";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gridX
            // 
            this.gridX.AllowCellNavigation = false;
            this.gridX.AllowDrop = true;
            this.gridX.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridX.CellEditorDisplayConditions = Xceed.Grid.CellEditorDisplayConditions.None;
            this.gridX.DataRowTemplate = this.dataRowTemplate1;
            this.gridX.DataRowTemplateStyles.Add(this.visualGridElementStyle1);
            this.gridX.DataRowTemplateStyles.Add(this.visualGridElementStyle2);
            // 
            // 
            // 
            this.gridX.FixedColumnSplitter.AllowDrop = true;
            this.gridX.FixedColumnSplitter.AllowRepositioning = true;
            this.gridX.FixedHeaderRows.Add(this.groupByRow1);
            this.gridX.FixedHeaderRows.Add(this.columnManagerRow2);
            this.gridX.Font = new System.Drawing.Font("Arial", 8.25F);
            this.gridX.ForeColor = System.Drawing.Color.Black;
            this.gridX.InactiveSelectionBackColor = System.Drawing.Color.Yellow;
            this.gridX.InactiveSelectionForeColor = System.Drawing.Color.Black;
            this.gridX.Location = new System.Drawing.Point(10, 30);
            this.gridX.Name = "gridX";
            this.gridX.ReadOnly = true;
            // 
            // 
            // 
            this.gridX.RowSelectorPane.BackColor = System.Drawing.Color.Red;
            this.gridX.SelectionMode = System.Windows.Forms.SelectionMode.One;
            this.gridX.SingleClickEdit = true;
            this.gridX.Size = new System.Drawing.Size(928, 425);
            this.gridX.TabIndex = 1;
            this.gridX.Trimming = System.Drawing.StringTrimming.None;
            this.gridX.CurrentRowChanged += new System.EventHandler(this.gridX_CurrentRowChanged);
            this.gridX.Sorted += new System.EventHandler(this.gridX_Sorted);
            this.gridX.GroupingUpdated += new System.EventHandler(this.gridX_GroupingUpdated);
            this.gridX.DoubleClick += new System.EventHandler(this.gridX_DoubleClick);
            // 
            // dataRowTemplate1
            // 
            this.dataRowTemplate1.AutoHeightMode = Xceed.Grid.AutoHeightMode.None;
            // 
            // 
            // 
            this.dataRowTemplate1.RowSelector.BackColor = System.Drawing.SystemColors.Control;
            // 
            // visualGridElementStyle1
            // 
            this.visualGridElementStyle1.BackColor = System.Drawing.Color.White;
            this.visualGridElementStyle1.Trimming = System.Drawing.StringTrimming.None;
            // 
            // visualGridElementStyle2
            // 
            this.visualGridElementStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.visualGridElementStyle2.Trimming = System.Drawing.StringTrimming.None;
            // 
            // groupByRow1
            // 
            this.groupByRow1.AllowDrop = true;
            this.groupByRow1.AutoHeightMode = Xceed.Grid.AutoHeightMode.Minimum;
            this.groupByRow1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.groupByRow1.CellBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(102)))));
            this.groupByRow1.CellFont = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
            this.groupByRow1.NoGroupText = "Trascina qui l\'intestazione di una colonna per formare un raggruppamento";
            this.groupByRow1.Trimming = System.Drawing.StringTrimming.None;
            // 
            // columnManagerRow2
            // 
            this.columnManagerRow2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(153)))), ((int)(((byte)(102)))));
            this.columnManagerRow2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(717, 3);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPreview.TabIndex = 14;
            this.btnPreview.Text = "Anteprima...";
            this.btnPreview.Visible = false;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(636, 2);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(75, 23);
            this.btnPrint.TabIndex = 13;
            this.btnPrint.Text = "Stampa";
            this.btnPrint.Visible = false;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // BtnExcel
            // 
            this.BtnExcel.Location = new System.Drawing.Point(416, 3);
            this.BtnExcel.Name = "BtnExcel";
            this.BtnExcel.Size = new System.Drawing.Size(98, 23);
            this.BtnExcel.TabIndex = 12;
            this.BtnExcel.Text = "Esporta in Excel";
            this.BtnExcel.Click += new System.EventHandler(this.btnExcel_Click);
            // 
            // cboList
            // 
            this.cboList.DropDownHeight = 162;
            this.cboList.IntegralHeight = false;
            this.cboList.Location = new System.Drawing.Point(65, 4);
            this.cboList.Name = "cboList";
            this.cboList.Size = new System.Drawing.Size(209, 22);
            this.cboList.TabIndex = 11;
            this.cboList.TabStop = false;
            this.toolTip1.SetToolTip(this.cboList, "Nome dell\'elenco standard o personalizzato da visualizzare");
            this.cboList.SelectedValueChanged += new System.EventHandler(this.cboList_SelectedValueChanged);
            // 
            // tabSelect
            // 
            this.tabSelect.Controls.Add(this.txtBaseFilter);
            this.tabSelect.Controls.Add(this.chkBaseFilter);
            this.tabSelect.Controls.Add(this.gridSelezione);
            this.tabSelect.Controls.Add(this.btnCancelCond);
            this.tabSelect.Controls.Add(this.btnSaveCond);
            this.tabSelect.Controls.Add(this.btnEditCond);
            this.tabSelect.Controls.Add(this.btnDeleteAllCond);
            this.tabSelect.Controls.Add(this.panelConnector);
            this.tabSelect.Controls.Add(this.label3);
            this.tabSelect.Controls.Add(this.label2);
            this.tabSelect.Controls.Add(this.label1);
            this.tabSelect.Controls.Add(this.btnValore);
            this.tabSelect.Controls.Add(this.txtValore);
            this.tabSelect.Controls.Add(this.chkRuntime);
            this.tabSelect.Controls.Add(this.lbOperatore);
            this.tabSelect.Controls.Add(this.lbColonna);
            this.tabSelect.Controls.Add(this.btnDeleteCond);
            this.tabSelect.Controls.Add(this.btnAddCond);
            this.tabSelect.Location = new System.Drawing.Point(4, 22);
            this.tabSelect.Name = "tabSelect";
            this.tabSelect.Size = new System.Drawing.Size(942, 459);
            this.tabSelect.TabIndex = 2;
            this.tabSelect.Text = "Selezione";
            // 
            // txtBaseFilter
            // 
            this.txtBaseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseFilter.Enabled = false;
            this.txtBaseFilter.Location = new System.Drawing.Point(700, 230);
            this.txtBaseFilter.Multiline = true;
            this.txtBaseFilter.Name = "txtBaseFilter";
            this.txtBaseFilter.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBaseFilter.Size = new System.Drawing.Size(230, 215);
            this.txtBaseFilter.TabIndex = 36;
            // 
            // chkBaseFilter
            // 
            this.chkBaseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBaseFilter.Checked = true;
            this.chkBaseFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBaseFilter.Location = new System.Drawing.Point(820, 200);
            this.chkBaseFilter.Name = "chkBaseFilter";
            this.chkBaseFilter.Size = new System.Drawing.Size(110, 24);
            this.chkBaseFilter.TabIndex = 35;
            this.chkBaseFilter.Text = "Includi filtro base";
            // 
            // gridSelezione
            // 
            this.gridSelezione.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridSelezione.DataMember = "customviewwhere";
            this.gridSelezione.DataRowTemplate = this.rowTemplateSelect;
            this.gridSelezione.DataSource = this.DS;
            this.gridSelezione.FixedHeaderRows.Add(this.columnManagerRow1);
            this.gridSelezione.GridLineColor = System.Drawing.Color.Gray;
            this.gridSelezione.GridLineStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            this.gridSelezione.Location = new System.Drawing.Point(20, 230);
            this.gridSelezione.Name = "gridSelezione";
            this.gridSelezione.Size = new System.Drawing.Size(660, 215);
            this.gridSelezione.TabIndex = 34;
            // 
            // rowTemplateSelect
            // 
            // 
            // 
            // 
            this.rowTemplateSelect.RowSelector.Click += new System.EventHandler(this.rowTemplateSelect_RowSelector_Click);
            // 
            // DS
            // 
            this.DS.DataSetName = "VistaFormCustomView";
            this.DS.Locale = new System.Globalization.CultureInfo("en-US");
            // 
            // columnManagerRow1
            // 
            this.columnManagerRow1.Visible = false;
            // 
            // btnCancelCond
            // 
            this.btnCancelCond.Location = new System.Drawing.Point(460, 130);
            this.btnCancelCond.Name = "btnCancelCond";
            this.btnCancelCond.Size = new System.Drawing.Size(80, 20);
            this.btnCancelCond.TabIndex = 33;
            this.btnCancelCond.Text = "Annulla";
            this.btnCancelCond.Click += new System.EventHandler(this.btnCancelCond_Click);
            // 
            // btnSaveCond
            // 
            this.btnSaveCond.Location = new System.Drawing.Point(460, 110);
            this.btnSaveCond.Name = "btnSaveCond";
            this.btnSaveCond.Size = new System.Drawing.Size(80, 20);
            this.btnSaveCond.TabIndex = 32;
            this.btnSaveCond.Text = "Salva";
            this.btnSaveCond.Click += new System.EventHandler(this.btnSaveCond_Click);
            // 
            // btnEditCond
            // 
            this.btnEditCond.Location = new System.Drawing.Point(460, 50);
            this.btnEditCond.Name = "btnEditCond";
            this.btnEditCond.Size = new System.Drawing.Size(80, 20);
            this.btnEditCond.TabIndex = 31;
            this.btnEditCond.Text = "Modifica";
            this.btnEditCond.Click += new System.EventHandler(this.btnEditCond_Click);
            // 
            // btnDeleteAllCond
            // 
            this.btnDeleteAllCond.Location = new System.Drawing.Point(460, 90);
            this.btnDeleteAllCond.Name = "btnDeleteAllCond";
            this.btnDeleteAllCond.Size = new System.Drawing.Size(80, 20);
            this.btnDeleteAllCond.TabIndex = 30;
            this.btnDeleteAllCond.Text = "Elimina tutte";
            this.btnDeleteAllCond.Click += new System.EventHandler(this.btnDeleteAllCond_Click);
            // 
            // panelConnector
            // 
            this.panelConnector.Controls.Add(this.radioOR);
            this.panelConnector.Controls.Add(this.radioAND);
            this.panelConnector.Location = new System.Drawing.Point(380, 30);
            this.panelConnector.Name = "panelConnector";
            this.panelConnector.Size = new System.Drawing.Size(60, 70);
            this.panelConnector.TabIndex = 29;
            // 
            // radioOR
            // 
            this.radioOR.Location = new System.Drawing.Point(10, 40);
            this.radioOR.Name = "radioOR";
            this.radioOR.Size = new System.Drawing.Size(40, 24);
            this.radioOR.TabIndex = 30;
            this.radioOR.Text = "OR";
            // 
            // radioAND
            // 
            this.radioAND.Location = new System.Drawing.Point(10, 10);
            this.radioAND.Name = "radioAND";
            this.radioAND.Size = new System.Drawing.Size(50, 24);
            this.radioAND.TabIndex = 29;
            this.radioAND.Text = "AND";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(20, 170);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 20);
            this.label3.TabIndex = 26;
            this.label3.Text = "Valore";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(240, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 20);
            this.label2.TabIndex = 25;
            this.label2.Text = "Operatore";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(20, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 20);
            this.label1.TabIndex = 24;
            this.label1.Text = "Colonna";
            // 
            // btnValore
            // 
            this.btnValore.Location = new System.Drawing.Point(340, 200);
            this.btnValore.Name = "btnValore";
            this.btnValore.Size = new System.Drawing.Size(30, 23);
            this.btnValore.TabIndex = 22;
            this.btnValore.Text = "...";
            this.btnValore.Click += new System.EventHandler(this.btnValore_Click);
            // 
            // txtValore
            // 
            this.txtValore.Location = new System.Drawing.Point(20, 200);
            this.txtValore.Name = "txtValore";
            this.txtValore.Size = new System.Drawing.Size(310, 20);
            this.txtValore.TabIndex = 21;
            // 
            // chkRuntime
            // 
            this.chkRuntime.Location = new System.Drawing.Point(160, 170);
            this.chkRuntime.Name = "chkRuntime";
            this.chkRuntime.Size = new System.Drawing.Size(250, 24);
            this.chkRuntime.TabIndex = 20;
            this.chkRuntime.Text = "Richiedi il valore al momento dell\'esecuzione";
            this.chkRuntime.CheckedChanged += new System.EventHandler(this.chkRuntime_CheckedChanged);
            // 
            // lbOperatore
            // 
            this.lbOperatore.DataSource = this.DS.customoperator;
            this.lbOperatore.DisplayMember = "name";
            this.lbOperatore.Location = new System.Drawing.Point(240, 30);
            this.lbOperatore.Name = "lbOperatore";
            this.lbOperatore.Size = new System.Drawing.Size(130, 134);
            this.lbOperatore.TabIndex = 19;
            this.lbOperatore.ValueMember = "idoperator";
            // 
            // lbColonna
            // 
            this.lbColonna.DataSource = this.DS.customviewcolumn;
            this.lbColonna.DisplayMember = "heading";
            this.lbColonna.Location = new System.Drawing.Point(20, 30);
            this.lbColonna.Name = "lbColonna";
            this.lbColonna.Size = new System.Drawing.Size(200, 134);
            this.lbColonna.TabIndex = 18;
            this.lbColonna.ValueMember = "colname";
            // 
            // btnDeleteCond
            // 
            this.btnDeleteCond.Location = new System.Drawing.Point(460, 70);
            this.btnDeleteCond.Name = "btnDeleteCond";
            this.btnDeleteCond.Size = new System.Drawing.Size(80, 20);
            this.btnDeleteCond.TabIndex = 17;
            this.btnDeleteCond.Text = "Elimina";
            this.btnDeleteCond.Click += new System.EventHandler(this.btnDeleteCond_Click);
            // 
            // btnAddCond
            // 
            this.btnAddCond.Location = new System.Drawing.Point(460, 30);
            this.btnAddCond.Name = "btnAddCond";
            this.btnAddCond.Size = new System.Drawing.Size(80, 20);
            this.btnAddCond.TabIndex = 16;
            this.btnAddCond.Text = "Aggiungi";
            this.btnAddCond.Click += new System.EventHandler(this.btnAddCond_Click);
            // 
            // colobjectname5
            // 
            this.colobjectname5.Title = "objectname";
            this.colobjectname5.VisibleIndex = 0;
            // 
            // colviewname5
            // 
            this.colviewname5.Title = "viewname";
            this.colviewname5.VisibleIndex = 1;
            // 
            // colperiodnumber3
            // 
            this.colperiodnumber3.Title = "periodnumber";
            this.colperiodnumber3.VisibleIndex = 2;
            // 
            // colconnector1
            // 
            this.colconnector1.Title = "connector";
            this.colconnector1.VisibleIndex = 3;
            // 
            // colcolumnname3
            // 
            this.colcolumnname3.Title = "columnname";
            this.colcolumnname3.VisibleIndex = 4;
            // 
            // coloperator1
            // 
            this.coloperator1.Title = "operator";
            this.coloperator1.VisibleIndex = 5;
            // 
            // colvalue1
            // 
            this.colvalue1.Title = "value";
            this.colvalue1.VisibleIndex = 6;
            // 
            // colruntime1
            // 
            this.colruntime1.Title = "runtime";
            this.colruntime1.VisibleIndex = 7;
            // 
            // collastmodtimestamp5
            // 
            this.collastmodtimestamp5.Title = "lastmodtimestamp";
            this.collastmodtimestamp5.VisibleIndex = 8;
            // 
            // collastmoduser5
            // 
            this.collastmoduser5.Title = "lastmoduser";
            this.collastmoduser5.VisibleIndex = 9;
            // 
            // colcreatetimestamp5
            // 
            this.colcreatetimestamp5.Title = "createtimestamp";
            this.colcreatetimestamp5.VisibleIndex = 10;
            // 
            // colcreateuser5
            // 
            this.colcreateuser5.Title = "createuser";
            this.colcreateuser5.VisibleIndex = 11;
            // 
            // colobjectname1
            // 
            this.colobjectname1.Title = "objectname";
            this.colobjectname1.VisibleIndex = 0;
            // 
            // colviewname1
            // 
            this.colviewname1.Title = "viewname";
            this.colviewname1.VisibleIndex = 1;
            // 
            // colperiodnumber1
            // 
            this.colperiodnumber1.Title = "periodnumber";
            this.colperiodnumber1.VisibleIndex = 2;
            // 
            // colconnector
            // 
            this.colconnector.Title = "connector";
            this.colconnector.VisibleIndex = 3;
            // 
            // colcolumnname1
            // 
            this.colcolumnname1.Title = "columnname";
            this.colcolumnname1.VisibleIndex = 4;
            // 
            // coloperator
            // 
            this.coloperator.Title = "operator";
            this.coloperator.VisibleIndex = 5;
            // 
            // colvalue
            // 
            this.colvalue.Title = "value";
            this.colvalue.VisibleIndex = 6;
            // 
            // colruntime
            // 
            this.colruntime.Title = "runtime";
            this.colruntime.VisibleIndex = 7;
            // 
            // collastmodtimestamp1
            // 
            this.collastmodtimestamp1.Title = "lastmodtimestamp";
            this.collastmodtimestamp1.VisibleIndex = 8;
            // 
            // collastmoduser1
            // 
            this.collastmoduser1.Title = "lastmoduser";
            this.collastmoduser1.VisibleIndex = 9;
            // 
            // colcreatetimestamp1
            // 
            this.colcreatetimestamp1.Title = "createtimestamp";
            this.colcreatetimestamp1.VisibleIndex = 10;
            // 
            // colcreateuser1
            // 
            this.colcreateuser1.Title = "createuser";
            this.colcreateuser1.VisibleIndex = 11;
            // 
            // tabColonne
            // 
            this.tabColonne.Controls.Add(this.gridCol);
            this.tabColonne.Location = new System.Drawing.Point(4, 22);
            this.tabColonne.Name = "tabColonne";
            this.tabColonne.Size = new System.Drawing.Size(942, 459);
            this.tabColonne.TabIndex = 1;
            this.tabColonne.Text = "Colonne";
            // 
            // gridCol
            // 
            this.gridCol.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridCol.DataMember = "customviewcolumn";
            this.gridCol.DataRowTemplate = this.dataRow1;
            this.gridCol.DataSource = this.DS;
            this.gridCol.FixedHeaderRows.Add(this.colManagerColonne);
            this.gridCol.Location = new System.Drawing.Point(0, 0);
            this.gridCol.Name = "gridCol";
            this.gridCol.SingleClickEdit = true;
            this.gridCol.Size = new System.Drawing.Size(942, 445);
            this.gridCol.TabIndex = 5;
            // 
            // colobjectname6
            // 
            this.colobjectname6.SortDirection = Xceed.Grid.SortDirection.None;
            this.colobjectname6.Title = "objectname";
            this.colobjectname6.VisibleIndex = 0;
            // 
            // colviewname6
            // 
            this.colviewname6.SortDirection = Xceed.Grid.SortDirection.Descending;
            this.colviewname6.Title = "viewname";
            this.colviewname6.VisibleIndex = 1;
            // 
            // colcolnumber2
            // 
            this.colcolnumber2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolnumber2.Title = "colnumber";
            this.colcolnumber2.VisibleIndex = 2;
            // 
            // colheading2
            // 
            this.colheading2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colheading2.Title = "heading";
            this.colheading2.VisibleIndex = 3;
            // 
            // colcolwidth2
            // 
            this.colcolwidth2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolwidth2.Title = "colwidth";
            this.colcolwidth2.VisibleIndex = 4;
            // 
            // colvisible2
            // 
            this.colvisible2.CellEditorDisplayConditions = Xceed.Grid.CellEditorDisplayConditions.Always;
            this.colvisible2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colvisible2.Title = "visible";
            this.colvisible2.VisibleIndex = 5;
            // 
            // colfontname2
            // 
            this.colfontname2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colfontname2.Title = "fontname";
            this.colfontname2.VisibleIndex = 6;
            // 
            // colfontsize2
            // 
            this.colfontsize2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colfontsize2.Title = "fontsize";
            this.colfontsize2.VisibleIndex = 7;
            // 
            // colbold2
            // 
            this.colbold2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colbold2.Title = "bold";
            this.colbold2.VisibleIndex = 8;
            // 
            // colitalic2
            // 
            this.colitalic2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colitalic2.Title = "italic";
            this.colitalic2.VisibleIndex = 9;
            // 
            // colunderline2
            // 
            this.colunderline2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colunderline2.Title = "underline";
            this.colunderline2.VisibleIndex = 10;
            // 
            // colstrikeout2
            // 
            this.colstrikeout2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colstrikeout2.Title = "strikeout";
            this.colstrikeout2.VisibleIndex = 11;
            // 
            // colcolor2
            // 
            this.colcolor2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolor2.Title = "color";
            this.colcolor2.VisibleIndex = 12;
            // 
            // colformat2
            // 
            this.colformat2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colformat2.Title = "format";
            this.colformat2.VisibleIndex = 13;
            // 
            // colisreal2
            // 
            this.colisreal2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colisreal2.Title = "isreal";
            this.colisreal2.VisibleIndex = 14;
            // 
            // colexpression2
            // 
            this.colexpression2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colexpression2.Title = "expression";
            this.colexpression2.VisibleIndex = 15;
            // 
            // colcolname2
            // 
            this.colcolname2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolname2.Title = "colname";
            this.colcolname2.VisibleIndex = 16;
            // 
            // colsystemtype2
            // 
            this.colsystemtype2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colsystemtype2.Title = "systemtype";
            this.colsystemtype2.VisibleIndex = 17;
            // 
            // collastmodtimestamp6
            // 
            this.collastmodtimestamp6.SortDirection = Xceed.Grid.SortDirection.None;
            this.collastmodtimestamp6.Title = "lastmodtimestamp";
            this.collastmodtimestamp6.VisibleIndex = 18;
            // 
            // collastmoduser6
            // 
            this.collastmoduser6.SortDirection = Xceed.Grid.SortDirection.None;
            this.collastmoduser6.Title = "lastmoduser";
            this.collastmoduser6.VisibleIndex = 19;
            // 
            // colcreatetimestamp6
            // 
            this.colcreatetimestamp6.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcreatetimestamp6.Title = "createtimestamp";
            this.colcreatetimestamp6.VisibleIndex = 20;
            // 
            // colcreateuser6
            // 
            this.colcreateuser6.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcreateuser6.Title = "createuser";
            this.colcreateuser6.VisibleIndex = 21;
            // 
            // collistcolpos2
            // 
            this.collistcolpos2.SortDirection = Xceed.Grid.SortDirection.None;
            this.collistcolpos2.Title = "listcolpos";
            this.collistcolpos2.VisibleIndex = 22;
            // 
            // celldataRow1colnumber2
            // 
            this.celldataRow1colnumber2.CellEditorDisplayConditions = Xceed.Grid.CellEditorDisplayConditions.Always;
            // 
            // colobjectname3
            // 
            this.colobjectname3.Title = "objectname";
            this.colobjectname3.Visible = false;
            this.colobjectname3.VisibleIndex = 0;
            // 
            // colviewname3
            // 
            this.colviewname3.Title = "viewname";
            this.colviewname3.Visible = false;
            this.colviewname3.VisibleIndex = 1;
            // 
            // colcolnumber1
            // 
            this.colcolnumber1.Title = "colnumber";
            this.colcolnumber1.Visible = false;
            this.colcolnumber1.VisibleIndex = 2;
            // 
            // colheading1
            // 
            this.colheading1.Title = "Colonna";
            this.colheading1.VisibleIndex = 3;
            // 
            // colcolwidth1
            // 
            this.colcolwidth1.Title = "colwidth";
            this.colcolwidth1.Visible = false;
            this.colcolwidth1.VisibleIndex = 4;
            // 
            // colvisible1
            // 
            this.colvisible1.Title = "Visibile";
            this.colvisible1.VisibleIndex = 5;
            // 
            // colfontname1
            // 
            this.colfontname1.Title = "Carattere";
            this.colfontname1.VisibleIndex = 6;
            // 
            // colfontsize1
            // 
            this.colfontsize1.Title = "Dim.carattere";
            this.colfontsize1.VisibleIndex = 7;
            // 
            // colbold1
            // 
            this.colbold1.Title = "Grassetto";
            this.colbold1.VisibleIndex = 8;
            // 
            // colitalic1
            // 
            this.colitalic1.Title = "Italico";
            this.colitalic1.VisibleIndex = 9;
            // 
            // colunderline1
            // 
            this.colunderline1.Title = "Sottolineato";
            this.colunderline1.VisibleIndex = 10;
            // 
            // colstrikeout1
            // 
            this.colstrikeout1.Title = "Barrato";
            this.colstrikeout1.VisibleIndex = 11;
            // 
            // colcolor1
            // 
            this.colcolor1.Title = "color";
            this.colcolor1.Visible = false;
            this.colcolor1.VisibleIndex = 12;
            // 
            // colformat1
            // 
            this.colformat1.Title = "format";
            this.colformat1.Visible = false;
            this.colformat1.VisibleIndex = 13;
            // 
            // colisreal1
            // 
            this.colisreal1.Title = "isreal";
            this.colisreal1.Visible = false;
            this.colisreal1.VisibleIndex = 14;
            // 
            // colexpression1
            // 
            this.colexpression1.Title = "expression";
            this.colexpression1.Visible = false;
            this.colexpression1.VisibleIndex = 15;
            // 
            // colcolname1
            // 
            this.colcolname1.Title = "campo DB";
            this.colcolname1.Visible = true;
            this.colcolname1.VisibleIndex = 16;
            // 
            // colsystemtype1
            // 
            this.colsystemtype1.Title = "systemtype";
            this.colsystemtype1.Visible = false;
            this.colsystemtype1.VisibleIndex = 17;
            // 
            // collastmodtimestamp3
            // 
            this.collastmodtimestamp3.Title = "lastmodtimestamp";
            this.collastmodtimestamp3.Visible = false;
            this.collastmodtimestamp3.VisibleIndex = 18;
            // 
            // collastmoduser3
            // 
            this.collastmoduser3.Title = "lastmoduser";
            this.collastmoduser3.Visible = false;
            this.collastmoduser3.VisibleIndex = 19;
            // 
            // colcreatetimestamp3
            // 
            this.colcreatetimestamp3.Title = "createtimestamp";
            this.colcreatetimestamp3.Visible = false;
            this.colcreatetimestamp3.VisibleIndex = 20;
            // 
            // colcreateuser3
            // 
            this.colcreateuser3.Title = "createuser";
            this.colcreateuser3.Visible = false;
            this.colcreateuser3.VisibleIndex = 21;
            // 
            // collistcolpos1
            // 
            this.collistcolpos1.Title = "Ordinamento";
            this.collistcolpos1.VisibleIndex = 22;
            // 
            // colobjectname2
            // 
            this.colobjectname2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colobjectname2.Title = "objectname";
            this.colobjectname2.Visible = false;
            this.colobjectname2.VisibleIndex = 0;
            // 
            // colviewname2
            // 
            this.colviewname2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colviewname2.Title = "viewname";
            this.colviewname2.Visible = false;
            this.colviewname2.VisibleIndex = 1;
            // 
            // colcolnumber
            // 
            this.colcolnumber.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolnumber.Title = "N.Colonna";
            this.colcolnumber.Visible = false;
            this.colcolnumber.VisibleIndex = 2;
            // 
            // colheading
            // 
            this.colheading.SortDirection = Xceed.Grid.SortDirection.None;
            this.colheading.Title = "Colonna";
            this.colheading.VisibleIndex = 3;
            // 
            // colcolwidth
            // 
            this.colcolwidth.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolwidth.Title = "Dimensione";
            this.colcolwidth.Visible = false;
            this.colcolwidth.VisibleIndex = 4;
            // 
            // colvisible
            // 
            this.colvisible.SortDirection = Xceed.Grid.SortDirection.None;
            this.colvisible.Title = "Visibile";
            this.colvisible.VisibleIndex = 5;
            // 
            // colfontname
            // 
            this.colfontname.SortDirection = Xceed.Grid.SortDirection.None;
            this.colfontname.Title = "Carattere";
            this.colfontname.VisibleIndex = 6;
            // 
            // colfontsize
            // 
            this.colfontsize.SortDirection = Xceed.Grid.SortDirection.None;
            this.colfontsize.Title = "Dim.Carattere";
            this.colfontsize.VisibleIndex = 7;
            // 
            // colbold
            // 
            this.colbold.SortDirection = Xceed.Grid.SortDirection.None;
            this.colbold.Title = "Grassetto";
            this.colbold.VisibleIndex = 8;
            // 
            // colitalic
            // 
            this.colitalic.SortDirection = Xceed.Grid.SortDirection.None;
            this.colitalic.Title = "Italico";
            this.colitalic.VisibleIndex = 9;
            // 
            // colunderline
            // 
            this.colunderline.SortDirection = Xceed.Grid.SortDirection.None;
            this.colunderline.Title = "Sottolineato";
            this.colunderline.VisibleIndex = 10;
            // 
            // colstrikeout
            // 
            this.colstrikeout.SortDirection = Xceed.Grid.SortDirection.None;
            this.colstrikeout.Title = "Barrato";
            this.colstrikeout.VisibleIndex = 11;
            // 
            // colcolor
            // 
            this.colcolor.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolor.Title = "color";
            this.colcolor.Visible = false;
            this.colcolor.VisibleIndex = 12;
            // 
            // colformat
            // 
            this.colformat.SortDirection = Xceed.Grid.SortDirection.None;
            this.colformat.Title = "format";
            this.colformat.Visible = false;
            this.colformat.VisibleIndex = 13;
            // 
            // colisreal
            // 
            this.colisreal.SortDirection = Xceed.Grid.SortDirection.None;
            this.colisreal.Title = "isreal";
            this.colisreal.Visible = false;
            this.colisreal.VisibleIndex = 14;
            // 
            // colexpression
            // 
            this.colexpression.SortDirection = Xceed.Grid.SortDirection.None;
            this.colexpression.Title = "expression";
            this.colexpression.Visible = false;
            this.colexpression.VisibleIndex = 15;
            // 
            // colcolname
            // 
            this.colcolname.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcolname.Title = "campo DB";
            this.colcolname.VisibleIndex = 16;
            // 
            // colsystemtype
            // 
            this.colsystemtype.SortDirection = Xceed.Grid.SortDirection.None;
            this.colsystemtype.Title = "systemtype";
            this.colsystemtype.Visible = false;
            this.colsystemtype.VisibleIndex = 17;
            // 
            // collastmodtimestamp2
            // 
            this.collastmodtimestamp2.SortDirection = Xceed.Grid.SortDirection.None;
            this.collastmodtimestamp2.Title = "lastmodtimestamp";
            this.collastmodtimestamp2.Visible = false;
            this.collastmodtimestamp2.VisibleIndex = 18;
            // 
            // collastmoduser2
            // 
            this.collastmoduser2.SortDirection = Xceed.Grid.SortDirection.None;
            this.collastmoduser2.Title = "lastmoduser";
            this.collastmoduser2.Visible = false;
            this.collastmoduser2.VisibleIndex = 19;
            // 
            // colcreatetimestamp2
            // 
            this.colcreatetimestamp2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcreatetimestamp2.Title = "createtimestamp";
            this.colcreatetimestamp2.Visible = false;
            this.colcreatetimestamp2.VisibleIndex = 20;
            // 
            // colcreateuser2
            // 
            this.colcreateuser2.SortDirection = Xceed.Grid.SortDirection.None;
            this.colcreateuser2.Title = "createuser";
            this.colcreateuser2.Visible = false;
            this.colcreateuser2.VisibleIndex = 21;
            // 
            // collistcolpos
            // 
            this.collistcolpos.SortDirection = Xceed.Grid.SortDirection.Ascending;
            this.collistcolpos.Title = "Ordinamento";
            this.collistcolpos.VisibleIndex = 22;
            // 
            // celldataRow1createuser
            // 
            this.celldataRow1createuser.Visible = false;
            // 
            // tabOrder
            // 
            this.tabOrder.Controls.Add(this.txtBaseSorting);
            this.tabOrder.Controls.Add(this.chkBaseSorting);
            this.tabOrder.Controls.Add(this.label4);
            this.tabOrder.Controls.Add(this.gridOrderBy);
            this.tabOrder.Controls.Add(this.panelOrderBy);
            this.tabOrder.Controls.Add(this.lbColOrderBy);
            this.tabOrder.Controls.Add(this.btnCancelOrderBy);
            this.tabOrder.Controls.Add(this.btnSaveOrderBy);
            this.tabOrder.Controls.Add(this.btnEditOrderBy);
            this.tabOrder.Controls.Add(this.btnDeleteAllOrderBy);
            this.tabOrder.Controls.Add(this.btnDeleteOrderBy);
            this.tabOrder.Controls.Add(this.btnAddOrderBy);
            this.tabOrder.Location = new System.Drawing.Point(4, 22);
            this.tabOrder.Name = "tabOrder";
            this.tabOrder.Size = new System.Drawing.Size(942, 459);
            this.tabOrder.TabIndex = 3;
            this.tabOrder.Text = "Ordinamento";
            // 
            // txtBaseSorting
            // 
            this.txtBaseSorting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBaseSorting.Enabled = false;
            this.txtBaseSorting.Location = new System.Drawing.Point(750, 355);
            this.txtBaseSorting.Multiline = true;
            this.txtBaseSorting.Name = "txtBaseSorting";
            this.txtBaseSorting.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBaseSorting.Size = new System.Drawing.Size(170, 90);
            this.txtBaseSorting.TabIndex = 45;
            // 
            // chkBaseSorting
            // 
            this.chkBaseSorting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkBaseSorting.Checked = true;
            this.chkBaseSorting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBaseSorting.Location = new System.Drawing.Point(750, 325);
            this.chkBaseSorting.Name = "chkBaseSorting";
            this.chkBaseSorting.Size = new System.Drawing.Size(160, 24);
            this.chkBaseSorting.TabIndex = 44;
            this.chkBaseSorting.Text = "Includi ordinamento base";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(10, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 20);
            this.label4.TabIndex = 43;
            this.label4.Text = "Colonna";
            // 
            // gridOrderBy
            // 
            this.gridOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridOrderBy.DataMember = "customvieworderby";
            this.gridOrderBy.DataRowTemplate = this.rowTemplateOrderBy;
            this.gridOrderBy.DataSource = this.DS;
            this.gridOrderBy.Location = new System.Drawing.Point(10, 335);
            this.gridOrderBy.Name = "gridOrderBy";
            this.gridOrderBy.Size = new System.Drawing.Size(720, 110);
            this.gridOrderBy.TabIndex = 42;
            // 
            // rowTemplateOrderBy
            // 
            // 
            // 
            // 
            this.rowTemplateOrderBy.RowSelector.Click += new System.EventHandler(this.rowTemplateOrderBy_RowSelector_Click);
            // 
            // panelOrderBy
            // 
            this.panelOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panelOrderBy.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelOrderBy.Controls.Add(this.radioDESC);
            this.panelOrderBy.Controls.Add(this.radioASC);
            this.panelOrderBy.Location = new System.Drawing.Point(740, 20);
            this.panelOrderBy.Name = "panelOrderBy";
            this.panelOrderBy.Size = new System.Drawing.Size(100, 70);
            this.panelOrderBy.TabIndex = 41;
            // 
            // radioDESC
            // 
            this.radioDESC.Location = new System.Drawing.Point(10, 40);
            this.radioDESC.Name = "radioDESC";
            this.radioDESC.Size = new System.Drawing.Size(90, 24);
            this.radioDESC.TabIndex = 1;
            this.radioDESC.Text = "Decrescente";
            // 
            // radioASC
            // 
            this.radioASC.Location = new System.Drawing.Point(10, 10);
            this.radioASC.Name = "radioASC";
            this.radioASC.Size = new System.Drawing.Size(80, 24);
            this.radioASC.TabIndex = 0;
            this.radioASC.Text = "Crescente";
            // 
            // lbColOrderBy
            // 
            this.lbColOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbColOrderBy.DataSource = this.DS.customviewcolumn;
            this.lbColOrderBy.DisplayMember = "heading";
            this.lbColOrderBy.Location = new System.Drawing.Point(10, 30);
            this.lbColOrderBy.Name = "lbColOrderBy";
            this.lbColOrderBy.Size = new System.Drawing.Size(720, 264);
            this.lbColOrderBy.TabIndex = 40;
            this.lbColOrderBy.ValueMember = "colname";
            // 
            // btnCancelOrderBy
            // 
            this.btnCancelOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelOrderBy.Location = new System.Drawing.Point(850, 120);
            this.btnCancelOrderBy.Name = "btnCancelOrderBy";
            this.btnCancelOrderBy.Size = new System.Drawing.Size(80, 20);
            this.btnCancelOrderBy.TabIndex = 39;
            this.btnCancelOrderBy.Text = "Annulla";
            this.btnCancelOrderBy.Click += new System.EventHandler(this.btnCancelOrderBy_Click);
            // 
            // btnSaveOrderBy
            // 
            this.btnSaveOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveOrderBy.Location = new System.Drawing.Point(850, 100);
            this.btnSaveOrderBy.Name = "btnSaveOrderBy";
            this.btnSaveOrderBy.Size = new System.Drawing.Size(80, 20);
            this.btnSaveOrderBy.TabIndex = 38;
            this.btnSaveOrderBy.Text = "Salva";
            this.btnSaveOrderBy.Click += new System.EventHandler(this.btnSaveOrderBy_Click);
            // 
            // btnEditOrderBy
            // 
            this.btnEditOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditOrderBy.Location = new System.Drawing.Point(850, 40);
            this.btnEditOrderBy.Name = "btnEditOrderBy";
            this.btnEditOrderBy.Size = new System.Drawing.Size(80, 20);
            this.btnEditOrderBy.TabIndex = 37;
            this.btnEditOrderBy.Text = "Modifica";
            this.btnEditOrderBy.Click += new System.EventHandler(this.btnEditOrderBy_Click);
            // 
            // btnDeleteAllOrderBy
            // 
            this.btnDeleteAllOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteAllOrderBy.Location = new System.Drawing.Point(850, 80);
            this.btnDeleteAllOrderBy.Name = "btnDeleteAllOrderBy";
            this.btnDeleteAllOrderBy.Size = new System.Drawing.Size(80, 20);
            this.btnDeleteAllOrderBy.TabIndex = 36;
            this.btnDeleteAllOrderBy.Text = "Elimina tutte";
            this.btnDeleteAllOrderBy.Click += new System.EventHandler(this.btnDeleteAllOrderBy_Click);
            // 
            // btnDeleteOrderBy
            // 
            this.btnDeleteOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteOrderBy.Location = new System.Drawing.Point(850, 60);
            this.btnDeleteOrderBy.Name = "btnDeleteOrderBy";
            this.btnDeleteOrderBy.Size = new System.Drawing.Size(80, 20);
            this.btnDeleteOrderBy.TabIndex = 35;
            this.btnDeleteOrderBy.Text = "Elimina";
            this.btnDeleteOrderBy.Click += new System.EventHandler(this.btnDeleteOrderBy_Click);
            // 
            // btnAddOrderBy
            // 
            this.btnAddOrderBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOrderBy.Location = new System.Drawing.Point(850, 20);
            this.btnAddOrderBy.Name = "btnAddOrderBy";
            this.btnAddOrderBy.Size = new System.Drawing.Size(80, 20);
            this.btnAddOrderBy.TabIndex = 34;
            this.btnAddOrderBy.Text = "Aggiungi";
            this.btnAddOrderBy.Click += new System.EventHandler(this.btnAddOrderBy_Click);
            // 
            // colobjectname7
            // 
            this.colobjectname7.Title = "objectname";
            this.colobjectname7.VisibleIndex = 0;
            // 
            // colviewname7
            // 
            this.colviewname7.Title = "viewname";
            this.colviewname7.VisibleIndex = 1;
            // 
            // colperiodnumber4
            // 
            this.colperiodnumber4.Title = "periodnumber";
            this.colperiodnumber4.VisibleIndex = 2;
            // 
            // colcolumnname4
            // 
            this.colcolumnname4.Title = "columnname";
            this.colcolumnname4.VisibleIndex = 3;
            // 
            // coldirection2
            // 
            this.coldirection2.Title = "direction";
            this.coldirection2.VisibleIndex = 4;
            // 
            // collastmodtimestamp7
            // 
            this.collastmodtimestamp7.Title = "lastmodtimestamp";
            this.collastmodtimestamp7.VisibleIndex = 5;
            // 
            // collastmoduser7
            // 
            this.collastmoduser7.Title = "lastmoduser";
            this.collastmoduser7.VisibleIndex = 6;
            // 
            // colcreatetimestamp7
            // 
            this.colcreatetimestamp7.Title = "createtimestamp";
            this.colcreatetimestamp7.VisibleIndex = 7;
            // 
            // colcreateuser7
            // 
            this.colcreateuser7.Title = "createuser";
            this.colcreateuser7.VisibleIndex = 8;
            // 
            // colobjectname4
            // 
            this.colobjectname4.Title = "objectname";
            this.colobjectname4.VisibleIndex = 0;
            // 
            // colviewname4
            // 
            this.colviewname4.Title = "viewname";
            this.colviewname4.VisibleIndex = 1;
            // 
            // colperiodnumber2
            // 
            this.colperiodnumber2.Title = "periodnumber";
            this.colperiodnumber2.VisibleIndex = 2;
            // 
            // colcolumnname2
            // 
            this.colcolumnname2.Title = "columnname";
            this.colcolumnname2.VisibleIndex = 3;
            // 
            // coldirection1
            // 
            this.coldirection1.Title = "direction";
            this.coldirection1.VisibleIndex = 4;
            // 
            // collastmodtimestamp4
            // 
            this.collastmodtimestamp4.Title = "lastmodtimestamp";
            this.collastmodtimestamp4.VisibleIndex = 5;
            // 
            // collastmoduser4
            // 
            this.collastmoduser4.Title = "lastmoduser";
            this.collastmoduser4.VisibleIndex = 6;
            // 
            // colcreatetimestamp4
            // 
            this.colcreatetimestamp4.Title = "createtimestamp";
            this.colcreatetimestamp4.VisibleIndex = 7;
            // 
            // colcreateuser4
            // 
            this.colcreateuser4.Title = "createuser";
            this.colcreateuser4.VisibleIndex = 8;
            // 
            // colobjectname
            // 
            this.colobjectname.Title = "objectname";
            this.colobjectname.VisibleIndex = 0;
            // 
            // colviewname
            // 
            this.colviewname.Title = "viewname";
            this.colviewname.VisibleIndex = 1;
            // 
            // colperiodnumber
            // 
            this.colperiodnumber.Title = "periodnumber";
            this.colperiodnumber.VisibleIndex = 2;
            // 
            // colcolumnname
            // 
            this.colcolumnname.Title = "columnname";
            this.colcolumnname.VisibleIndex = 3;
            // 
            // coldirection
            // 
            this.coldirection.Title = "direction";
            this.coldirection.VisibleIndex = 4;
            // 
            // collastmodtimestamp
            // 
            this.collastmodtimestamp.Title = "lastmodtimestamp";
            this.collastmodtimestamp.Visible = false;
            this.collastmodtimestamp.VisibleIndex = 5;
            // 
            // collastmoduser
            // 
            this.collastmoduser.Title = "lastmoduser";
            this.collastmoduser.Visible = false;
            this.collastmoduser.VisibleIndex = 6;
            // 
            // colcreatetimestamp
            // 
            this.colcreatetimestamp.Title = "createtimestamp";
            this.colcreatetimestamp.Visible = false;
            this.colcreatetimestamp.VisibleIndex = 7;
            // 
            // colcreateuser
            // 
            this.colcreateuser.Title = "createuser";
            this.colcreateuser.Visible = false;
            this.colcreateuser.VisibleIndex = 8;
            // 
            // tabList
            // 
            this.tabList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabList.Controls.Add(this.tabElenco);
            this.tabList.Controls.Add(this.tabColonne);
            this.tabList.Controls.Add(this.tabSelect);
            this.tabList.Controls.Add(this.tabOrder);
            this.tabList.Controls.Add(this.TabSommatorie);
            this.tabList.Controls.Add(this.tabGestione);
            this.tabList.Location = new System.Drawing.Point(0, 0);
            this.tabList.Name = "tabList";
            this.tabList.SelectedIndex = 0;
            this.tabList.Size = new System.Drawing.Size(950, 485);
            this.tabList.TabIndex = 0;
            this.tabList.SelectedIndexChanged += new System.EventHandler(this.tabList_Click);
            // 
            // TabSommatorie
            // 
            this.TabSommatorie.Controls.Add(this.chkOttimizzaSomme);
            this.TabSommatorie.Controls.Add(this.lbsumfield);
            this.TabSommatorie.Controls.Add(this.label6);
            this.TabSommatorie.Controls.Add(this.label5);
            this.TabSommatorie.Controls.Add(this.chkSommatorie);
            this.TabSommatorie.Location = new System.Drawing.Point(4, 22);
            this.TabSommatorie.Name = "TabSommatorie";
            this.TabSommatorie.Size = new System.Drawing.Size(942, 459);
            this.TabSommatorie.TabIndex = 5;
            this.TabSommatorie.Text = "Sommatorie";
            // 
            // chkOttimizzaSomme
            // 
            this.chkOttimizzaSomme.Location = new System.Drawing.Point(10, 60);
            this.chkOttimizzaSomme.Name = "chkOttimizzaSomme";
            this.chkOttimizzaSomme.Size = new System.Drawing.Size(380, 24);
            this.chkOttimizzaSomme.TabIndex = 4;
            this.chkOttimizzaSomme.Text = "Inserisci la riga dei totali solo se il gruppo è costituito da più di una riga";
            // 
            // lbsumfield
            // 
            this.lbsumfield.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbsumfield.CheckOnClick = true;
            this.lbsumfield.ContextMenu = this.contextMenu1;
            this.lbsumfield.Location = new System.Drawing.Point(10, 120);
            this.lbsumfield.MultiColumn = true;
            this.lbsumfield.Name = "lbsumfield";
            this.lbsumfield.Size = new System.Drawing.Size(920, 304);
            this.lbsumfield.Sorted = true;
            this.lbsumfield.TabIndex = 3;
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem2});
            this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.Text = "Seleziona tutti";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "Deseleziona tutti";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(10, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(370, 23);
            this.label6.TabIndex = 2;
            this.label6.Text = "Colonne su cui si vogliono effettuare le somme:";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(10, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(690, 20);
            this.label5.TabIndex = 1;
            this.label5.Text = "Configurazione delle sommatorie parziali sui raggruppamenti";
            // 
            // chkSommatorie
            // 
            this.chkSommatorie.Location = new System.Drawing.Point(10, 30);
            this.chkSommatorie.Name = "chkSommatorie";
            this.chkSommatorie.Size = new System.Drawing.Size(270, 24);
            this.chkSommatorie.TabIndex = 0;
            this.chkSommatorie.Text = "Abilita il calcolo delle somme sui raggruppamenti";
            this.chkSommatorie.CheckedChanged += new System.EventHandler(this.chkSommatorie_CheckedChanged);
            // 
            // tabGestione
            // 
            this.tabGestione.Controls.Add(this.btnDelete);
            this.tabGestione.Controls.Add(this.btnApply);
            this.tabGestione.Controls.Add(this.btnCopy);
            this.tabGestione.Location = new System.Drawing.Point(4, 22);
            this.tabGestione.Name = "tabGestione";
            this.tabGestione.Size = new System.Drawing.Size(942, 459);
            this.tabGestione.TabIndex = 4;
            this.tabGestione.Text = "Gestione";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(30, 100);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(160, 30);
            this.btnDelete.TabIndex = 42;
            this.btnDelete.Text = "Elimina elenco";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete1_Click);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(30, 60);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(160, 30);
            this.btnApply.TabIndex = 41;
            this.btnApply.Text = "Salva elenco";
            this.btnApply.Click += new System.EventHandler(this.btnApply1_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(30, 20);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(160, 30);
            this.btnCopy.TabIndex = 40;
            this.btnCopy.Text = "Salva elenco con nome";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy1_Click);
            // 
            // FormCustomViewList
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(952, 488);
            this.Controls.Add(this.tabList);
            this.Name = "FormCustomViewList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormCustomViewList";
            this.Closed += new System.EventHandler(this.formCustomViewList_Closed);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.formCustomViewList_FormClosed);
            this.tabElenco.ResumeLayout(false);
            this.tabElenco.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataRowTemplate1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnManagerRow2)).EndInit();
            this.tabSelect.ResumeLayout(false);
            this.tabSelect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridSelezione)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowTemplateSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.columnManagerRow1)).EndInit();
            this.panelConnector.ResumeLayout(false);
            this.tabColonne.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataRow1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.colManagerColonne)).EndInit();
            this.tabOrder.ResumeLayout(false);
            this.tabOrder.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridOrderBy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rowTemplateOrderBy)).EndInit();
            this.panelOrderBy.ResumeLayout(false);
            this.tabList.ResumeLayout(false);
            this.TabSommatorie.ResumeLayout(false);
            this.tabGestione.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		/// Nota: i metodi delle varie griglie si distinguono in
		/// XXXGridNomeXXXXX , al momento abbiamo:
		/// GridElenchi (grid del tab elenchi)
		/// GridColonne (grid del tab colonne)
		/// Attenzione, i metodi che terminano per Columns fanno riferimento
		/// alle impostazioni delle colonne nella relativa griglia e non
		/// al tab colonne.

		#region Impostazioni e metodi della griglia Elenchi

		void selectNewListType(string newvalue, bool readfromdisk){
			//se è diverso il listtype aggiorno le impostazioni di colonna
            string currtop = getTop();
			if ((m_listtype != newvalue) || (m_top!=currtop)) {
				m_listtype = newvalue;
                m_top = currtop;
				
				if (readfromdisk) leggiListingType(m_listtype);
				applicaListType();		
				allineaDatasetImpostazioni(sampleTable);
			}

		}

		private void cboList_SelectedValueChanged(object sender, System.EventArgs e) {
            if (destroyed) return;
            if (filterlocked) return;
			if (cboList.Enabled==false) return;
			if (cboList.SelectedIndex == -1) 
				return;

			var cboItem = (DataRowView) cboList.SelectedItem;
			string newvalue = cboItem.Row["viewname"].ToString();
			selectNewListType(newvalue,true);
		}

//		/// <summary>
//		/// Segna come di distema il listtype corrente
//		/// </summary>
//		/// <param name="issystemvalue">Valore di issytem (S/N) presente in customview</param>
//		private void SetListTypeIsSystem(string issystemvalue) {
//			if (issystemvalue.ToUpper() == "S")
//				m_listtypeissystem = true;
//			else
//				m_listtypeissystem = false;
//
//			EnableDisableButtons();		
//		}

		/// <summary>
		/// Imposta l'abilitazione dei bottoni
		/// </summary>
		private void enableDisableButtons(bool m_listtypeissystem) {
			btnApply.Enabled = (!filterlocked) && (!m_listtypeissystem);
			btnDelete.Enabled = (!filterlocked) && (!m_listtypeissystem);
			mnuItemApply.Enabled = (!filterlocked) && (!m_listtypeissystem);
			mnuItemDelete.Enabled = (!filterlocked) && (!m_listtypeissystem);
			mnuItemCopy.Enabled= (!filterlocked);
			btnCopy.Enabled= (!filterlocked);
		}

		/// <summary>
		/// Add events to cells. Must be called after every databinding
		/// </summary>
		void addEventsToGridElenchi(){
			foreach (Cell cell in columnManagerRow2.Cells){
				cell.Click += cell_Click;
                cell.MouseEnter += cell_Click;
            }
//			foreach (Cell cell in gridX.DataRowTemplate.Cells) {
//				//cell.Click += new EventHandler(this.GridElenchiCell_Click);
//				cell.DoubleClick += new EventHandler(GridElenchiCell_DoubleClick);
//			}
		}

        
        
     


		/// <summary>
		/// Crea un listtype temporaneo con tutti i dati a null eccetto per il colname
		/// Si verifica se per quel tablename non è presente sul DB nessun listtype
		/// Output: DS
		/// </summary>
		private void creaESalvaDummyListType(string tablename, string listtype) {
			//non dovrebbe accadere mai, ma nel malaugurato caso
			//dovessero essere cancellati tutti i listtype non
			//viene memorizzato nessuno con listtype a ""
			if (m_listtype == "") return;            
			// controllo esistenza riga master
			if (DS.customview.Rows.Count < 1) {
				//creo un listtype di sistema
				System.Data.DataRow row = DS.customview.NewRow();
				row["objectname"] = tablename;
				row["viewname"] = listtype;
				row["issystem"] = "S";
				DS.customview.Rows.Add(row);
				//Eseguo la commit solo della riga master per la gestione
				//dei listtype nella combo
				eseguiPostData(DS);
			}
			string[] colonne = m_columnList.Split(',');
			int i = 0;
			model.invokeActions(DS.customviewcolumn,TableAction.beginLoad);
			foreach (string columnname in colonne) {
				System.Data.DataRow row = DS.customviewcolumn.NewRow();
				row["objectname"] = tablename.Trim();
				row["viewname"] = listtype;
				row["listcolpos"] = i++;
				row["colnumber"] = i;
				row["colname"] = columnname.Trim();
				row["heading"] = columnname.Trim();
				row["visible"] = 1;
				row["bold"] = 0;
				row["italic"] = 0;
				row["underline"] = 0;
				row["strikeout"] = 0;
				DS.customviewcolumn.Rows.Add(row);
			}
			model.invokeActions(DS.customviewcolumn,TableAction.endLoad);
		}

	


		/*
			private void SetGridElenchiColumnsFromMetaData() {
				//leggo le impostazioni delle colonne
				m_metaData = m_linked.Dispatcher.Get(m_tablename);
				m_metaData.DescribeColumns(DT, m_listtype);

				foreach (DataColumn col in DT.Columns) {
					if ((col.Caption == "") || (col.Caption.StartsWith("."))) {
						gridX.Columns[col.ColumnName].Visible = false;
						col.ExtendedProperties["ListColPos"]=-1;
						continue;
					}
					//autosize della colonna
					gridX.Columns[col.ColumnName].Width = gridX.Columns[col.ColumnName].GetFittedWidth();
					//imposto a true il visible (altrimenti rimarebbe a false nel caso
					//una colonna fosse stata nascosta)
					gridX.Columns[col.ColumnName].Visible = true;
					gridX.Columns[col.ColumnName].Title = col.Caption;
					gridX.Columns[col.ColumnName].FormatSpecifier = HelpForm.GetFormatForColumn(col);
					//ricavo fontname, fontsize, bold, italic, underlined, strikeout
					gridX.Columns[col.ColumnName].Font = GetFont(col.ColumnName);
				}
			}
	*/


		/// <summary>
		/// Ottiene il Font con tutte le sue proprietà (Stile, family, etc.)
		/// </summary>
		/// <param name="row">Datarow che contiene tutti i parametri
		/// delle proprietà del Font</param>
		/// <returns></returns>
		private Font getFont(System.Data.DataRow row) {
			try {
				FontFamily family = getFontFamily(row);
				if (family == null) return Font;
				float emSize = getFontSize(row);
				FontStyle style = getFontStyle(row);
				Font font = new Font(family, emSize, style);
				return font;
			}
			catch {
				return Font;
			}
		}

		//ricavo la family di appartenenza del font
		private FontFamily getFontFamily(System.Data.DataRow row) {
			string fontname = row["fontname"].ToString();
		    if (fontname == "") fontname = "Microsoft Sans Serif";
			FontFamily[] families = FontFamily.Families;
			foreach (FontFamily family in families) {
				if (fontname.ToLower() == family.Name.ToLower())
					return family;
			}
			return FontFamily.GenericSansSerif;;
		}

		//ricavo la size del font
		private float getFontSize(System.Data.DataRow row) {
			object obj = row["fontsize"];
			if (obj == null || obj == DBNull.Value) 
				return Font.Size;
			float valore = Convert.ToSingle(obj);
			return valore;
		}

		//lo stile
		private FontStyle getFontStyle(System.Data.DataRow row) {
			FontStyle style = FontStyle.Regular;
			if (row["bold"].ToString() == "1") style |= FontStyle.Bold;
			if (row["italic"].ToString() == "1") style |= FontStyle.Italic;
			if (row["underline"].ToString() == "1") style |= FontStyle.Underline;
			if (row["strikeout"].ToString() == "1") style |= FontStyle.Strikeout;
			return style;
		}

		/// <summary>
		/// Fills the combo of list-types, getting values from DB
		/// Does not change current listing type
		/// </summary>
		private void fillComboList() {
			string filter = "(objectname = '" + m_tablename + "')";
			DataTable tabObject = getDataTable("customview", "*", filter);
			cboList.DataSource = tabObject;
			cboList.DisplayMember = "viewname";
			cboList.ValueMember = "viewname";
		}

		private DataTable getDataTable(string tablename, string columnList, string filter) {
			return conn.RUN_SELECT(tablename, columnList, null, filter, 
				null, null, false);
		}

		private void gridElenchiCell_Click(object sender, EventArgs e ) {
            if (destroyed) return;
            gridElenchiSelectCell();
		}

		private void gridElenchiCell_DoubleClick(object sender, EventArgs e) {
            if (destroyed) return;
		   
            gridElenchiSelectCell();
			if (filterlocked)
				DialogResult = System.Windows.Forms.DialogResult.OK;
			else 
				Close();
		}

        System.Data.DataRow getMainRow(System.Data.DataRow viewRow) {
            if (viewRow == null) return null;
            string filtertoApply = filtroSuVistaApplicato;
            string vista = viewRow.Table.TableName;


            DataTable mainTable = conn.CreateTableByName(vista, "*"); //m_linked.TableName
            //DataTable viewTable = m_linked.Conn.CreateTableByName(vista, "*");                      
            DataColumn[] primarykey = mainTable.PrimaryKey;

            if (primarykey.Length == 0) {
                string[] metaKey = m_linkedview.PrimaryKey();
                if (metaKey != null) {
                    primarykey =
                        (from string colName in metaKey select mainTable.Columns[colName]).ToArray<DataColumn>();
                }
            }

            if (primarykey.Length == 0) {
                string[] metaKey = m_linked.PrimaryKey();
                if (metaKey != null) {
                    primarykey =
                        (from string colName in metaKey select mainTable.Columns[colName]).ToArray<DataColumn>();
                }
            }

            if (primarykey.Length > 0) {
                QueryHelper QHS = conn.GetQueryHelper();
                string filterKey = QHS.MCmp(viewRow, (from s in primarykey select s.ColumnName).ToArray<string>());
                filtertoApply = filterKey;
                DataTable tt = conn.RUN_SELECT(vista, "*", null, filtertoApply, null, false);
                if (tt.Rows.Count == 1) return tt.Rows[0];
                if (tt.Rows.Count > 1) {
                    errorLogger.logException(
                        $"getMainRow:La definizione della chiave per la tabella/vista:{mainTable.TableName} vista {vista} non è completa. Filtro Applicato:{filtertoApply} (righe trovate:{tt.Rows.Count})");
                }
            }
            else {
                errorLogger.logException($"getMainRow Warn:Manca la definizione della chiave per la tabella:{mainTable.TableName}/{m_tablename} vista {vista} metaview {m_linkedview.Name}. ");
            }
            QueryHelper QHS2 = conn.GetQueryHelper();
            string[] all = new string[viewRow.Table.Columns.Count];
            for (int i = 0; i < viewRow.Table.Columns.Count; i++) all[i] = viewRow.Table.Columns[i].ColumnName;
            filtertoApply = QHS2.MCmp(viewRow, all);
         


            var t = conn.RUN_SELECT(vista, "*", null, filtertoApply, null, false);
            if (t.Rows.Count == 1) return t.Rows[0];
            if (t.Rows.Count == 0) return null;

            return viewRow;
        }

        /// <summary>
        /// Called when users clicks on the list grid
        /// </summary>
        public void gridElenchiSelectCell(){
			if (!running) return;
            if (destroyed) return;
			if (UpdateFormDisabled) return;
			if (!update_enabled) return;
            
            if (!GOTOPilotato){
               
                
				//this.Activate();
                if (destroyed) {                    
                    return;
                }
                
                //this.Select();
                if (destroyed) {
                    return;
                }
                
				//gridX.Select();
              
                if (destroyed) {
                    return;
                }

                //gridX.Focus();
                //if (!gridX.Focused) {
                //    return;
                //}
                
            }
			update_enabled = false;
			if (DT.Rows.Count==0){
				m_LastSelectRowGridElenchi=null;
				update_enabled=true;
                return;
			}
			int rowselect = metaprofiler.StartTimer("RowListSelect");
            crono MyC = new crono("LoadTime");
			try {
				var DR = getSelectedRow(gridX, DSDati, DT.TableName);
			    DR = getMainRow(DR);
			    if (DR != null) {
			        if (m_LastSelectRowGridElenchi == DR) {
			            update_enabled = true;
			            long ms = MyC.GetDuration();
			            setLastRowSelectTime(ms);
			            metaprofiler.StopTimer(rowselect);
			            return;
			        }
			        if (filterlocked || (m_linked == null)) {
			            LastSelectedRow = DR;

			        }
			        else {			            
			            if (controller == null || controller.TryToSelectRow(DR, m_listtype)) {
			                m_LastSelectRowGridElenchi = DR;

			                if (!gridX.IsDisposed) {
			                    //gridX.SelectedRows.Clear();
			                    if (gridX.CurrentRow != null && gridX.CurrentRow.CanBeSelected) {
			                        gridX.SelectedRows.Add(gridX.CurrentRow);
			                    }
			                }
			            }
			        }
			    }
			}
			catch (Exception E){
			    shower.ShowException(this,"Errore nella selezione del grid",E);
			}
			update_enabled=true;

            long msec = MyC.GetDuration();
            setLastRowSelectTime(msec);

			metaprofiler.StopTimer(rowselect);
        }

        private void setLastRowSelectTime(long ms) {
            FormController.setLastLoadTime(ms);
        }

		#endregion Impostazioni e metodi della griglia Elenchi

		#region Impostazioni e metodi della griglia Colonne
		

		/// <summary>
		/// Imposta i due grid leggendo le impostazioni sul listtype
		/// </summary>
		private void impostaTabsImpostazioni() {

			//Imposta il grid delle impostazioni
			setGridColonneColumns();

			//Imposta il tab page delle selezioni
			setPageSelezione();

			//Imposta il tab page degli ordinamenti
			setPageOrdinamento();

			//Imposta il tab page delle somme parziali
			setPageSum();
		}

        
		/// <summary>
		/// Assegna le impostazioni statiche del Grid Colonne
		/// </summary>
		private void setGridColonneColumns() {
			//			gridCol.Columns["colwidth"].NullText = gridCol.Columns["colwidth"].Width.ToString();
			GridComboBox fontComboBox = getGridComboFont();
			gridCol.Columns["fontname"].CellEditor = fontComboBox;
			gridCol.Columns["fontname"].CellViewer = fontComboBox;
			gridCol.Columns["fontname"].NullText = Font.Name;
			gridCol.Columns["fontsize"].NullText = Font.Size.ToString();

            GridCheckBox check = new GridCheckBox {
                ThreeState = false,
                AutoCheck = true
            };
            check.MouseLeave += check_LostFocus;
			check.CheckAlign = ContentAlignment.MiddleCenter;
			gridCol.Columns["visible"].CellViewer = check;
			gridCol.Columns["visible"].CellEditor = check;

            check = new GridCheckBox {
                ThreeState = false,
                AutoCheck = true
            };
            check.MouseLeave += check_LostFocus;
			check.CheckAlign = ContentAlignment.MiddleCenter;
			gridCol.Columns["bold"].CellViewer = check;
			gridCol.Columns["bold"].CellEditor = check;

            check = new GridCheckBox {
                ThreeState = false,
                AutoCheck = true
            };
            check.MouseLeave += check_LostFocus;
			check.CheckAlign = ContentAlignment.MiddleCenter;
			gridCol.Columns["italic"].CellViewer = check;
			gridCol.Columns["italic"].CellEditor = check;

            check = new GridCheckBox {
                ThreeState = false,
                AutoCheck = true
            };
            check.MouseLeave += check_LostFocus;
			check.CheckAlign = ContentAlignment.MiddleCenter;
			gridCol.Columns["underline"].CellViewer = check;
			gridCol.Columns["underline"].CellEditor = check;

            check = new GridCheckBox {
                ThreeState = false,
                AutoCheck = true
            };
            check.MouseLeave += check_LostFocus;
			check.CheckAlign = ContentAlignment.MiddleCenter;
			gridCol.Columns["strikeout"].CellViewer = check;
			gridCol.Columns["strikeout"].CellEditor = check;

            gridCol.Columns["objectname"].Title = LM.translate("dbTableName",true);            
            gridCol.Columns["viewname"].Title = LM.translate("list",true);
            gridCol.Columns["colnumber"].Visible = false;
            gridCol.Columns["heading"].Title = LM.translate("columnName",true);
            gridCol.Columns["colwidth"].Title = LM.translate("width",true);
            gridCol.Columns["visible"].Title = LM.translate("visible",true);
            gridCol.Columns["fontname"].Title = LM.translate("fontName",true);
            gridCol.Columns["fontsize"].Title = LM.translate("fontSize",true);
            gridCol.Columns["bold"].Title = LM.translate("bold",true);
            gridCol.Columns["italic"].Title = LM.translate("italic",true); //"Corsivo";
            gridCol.Columns["underline"].Title = LM.translate("underline",true); //"Sottolineato";
            gridCol.Columns["strikeout"].Visible = false;
            gridCol.Columns["color"].Visible = false;
            gridCol.Columns["format"].Visible = false;
            gridCol.Columns["isreal"].Visible = false;
            gridCol.Columns["expression"].Visible = false;
            gridCol.Columns["colname"].Title = LM.dbFieldName;
            gridCol.Columns["systemtype"].Visible = false;
            gridCol.Columns["lastmodtimestamp"].Visible = false;
            gridCol.Columns["lastmoduser"].Visible = false;
            gridCol.Columns["createtimestamp"].Visible = false;
            gridCol.Columns["createuser"].Visible = false;
            gridCol.Columns["listcolpos"].Title = LM.listColPos;


            foreach (Column C in gridCol.Columns) {
                if (C.Visible) C.Width = C.GetFittedWidth();
            }
            










			//per la gestione della sola colonna color
			/*			foreach (DataCell cell in gridCol.DataRowTemplate.Cells) {
							if (cell.ParentColumn.FieldName == "color")
								cell.Click += new EventHandler(GridColonneCell_Click);
						}
			*/		
		}

        void check_LostFocus(object sender, EventArgs e) {
            if (destroyed) return;
            var c = sender as GridCheckBox;
            var p = c?.Parent;
            if(!(p?.Parent is GridControl g)) return;
            if (g.CurrentCell.IsBeingEdited)
                g.CurrentCell.LeaveEdit(true);
        }

		private GridComboBox getGridComboFont() {
			var gridCombo = getBaseGridComboBox();
			var families = FontFamily.Families;
			foreach (var family in families) {
				gridCombo.Items.Add(family.Name);
			}
			return gridCombo;
		}

		private void gridColonneCell_Click(object sender, EventArgs e) {
            if (destroyed) return;
            var cell = (DataCell) sender;
			if (cell.ParentColumn.FieldName != "color") 
				return;

			var cd = new ColorDialog();
			if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				cell.BackColor = cd.Color;
				cell.ForeColor = cd.Color;
				cell.Value = cd.Color.ToArgb();
			}
		}

		int lasttabselected;
		private void tabList_Click(object sender, System.EventArgs e) {
            if (destroyed) return;
            switch (tabList.SelectedIndex) {
				case 0: //elenco
					lasttabselected=0;
					applicaListType();
					aggiornaSommatorie();
					//SetGridElenchiColumnsFromDS();
					//FillDataSetGridElenchi();
					break;
				case 1: //selezione (filtro WHERE)
					if (lasttabselected!=0)return;
					allineaDatasetImpostazioni(sampleTable);
					lasttabselected=1;
					break;
				case 2: //colonne visibili/non visibili
					if (lasttabselected!=0)return;
					allineaDatasetImpostazioni(sampleTable);
					lasttabselected=2;
					break;
				case 3:// order by
					if (lasttabselected!=0)return;
					allineaDatasetImpostazioni(sampleTable);
					lasttabselected=3;
					break;
				case 4:
					setPageSum();
					if (lasttabselected!=0)return;
					allineaDatasetImpostazioni(sampleTable);
					lasttabselected=4;
					break;
				case 5:
					if (lasttabselected!=0)return;
					allineaDatasetImpostazioni(sampleTable);
					lasttabselected=5;
					break;
				default:

					break;
			}
		}


		string compattaOrderBy(string orderby){
			if (orderby==null) return null;
			orderby = orderby.Trim();
			if (orderby=="") return null;
			string [] clauses = orderby.Split(new char[1]{','});
			string result="";
			foreach (string mclause in clauses){
				string clause =mclause;
				clause = clause.Trim().ToLower();
				if (clause=="")continue;
				string [] clausecomps = clause.Split(new char[1]{' '});
				string fieldname= clausecomps[0].Trim();
				if (result.StartsWith(fieldname))continue;
				if (result.IndexOf(","+fieldname+" ")>0) continue;
				string sort="ASC";
				if (clausecomps.Length>1) sort = clausecomps[1].ToUpper();
				if ((sort!="ASC")&&(sort!="DESC"))sort="ASC";
				if (result!="") result+=",";
				result+= fieldname+" "+sort;
			}
			return result;
		}
		/// <summary>
		/// Costruisce la condizione di ordinamento
		/// </summary>
		private string buildOrderByCondition() {
			System.Data.DataRow[] rows = DS.customvieworderby.Select();

			string m_sorting = getOrderByFromCustomViewOrderBy(rows);
	
			string mybasesorting = m_basesorting;
			if (m_IAmAdmin) mybasesorting= txtBaseSorting.Text.Trim();
			if (mybasesorting!="") {
			
				if (chkBaseSorting.Checked) {
					if ((m_sorting != "")&&(m_sorting!=null))
						m_sorting += ", " + mybasesorting;
					else
						m_sorting = mybasesorting;
				}
			}
			if (m_sorting == "")
				m_sorting = null;
			return compattaOrderBy(m_sorting);
		}

		private string getOrderByFromCustomViewOrderBy(System.Data.DataRow[] orderClauses) {
			string orderby = null;
			foreach (System.Data.DataRow row in orderClauses) {
				orderby += row["columnname"].ToString() + " " +
					getOrderByValue(row["direction"].ToString()) + ",";
			}
			if (orderby!=null) orderby = orderby.Remove(orderby.Length - 1, 1);
			return orderby;
		}

		private string getOrderByValue(string direction) {
			switch(direction) {
				case "1": 
					return "DESC";
				default:
					return "ASC";
			}
		}

	
		/// <summary>
		/// Memorizzo nel DS l'indice di visualizzazione, la width e la caption
		/// </summary>
		/// <param name="sample"></param>
		private void allineaDatasetImpostazioni(DataTable sample) {
            var captions= new Dictionary<string, string>();
		    foreach (DataColumn col in sample.Columns) {
		        captions[col.ColumnName] = col.Caption;
		    }
		    System.Data.DataRow[] headingRows = DS.customviewcolumn.Select();
		    foreach (var rHead in headingRows) {
		        string colName = rHead["colname"].ToString();
                if (!captions.ContainsKey(colName)) continue;
		        string title = captions[colName];
		        if (title == "") continue;
		        if (title.StartsWith(".")) title = title.Substring(1);
		        if (title == colName) continue;
                if (rHead["heading"].ToString() != title) {
                    rHead["heading"] = title;
                    if (DT.Columns.Contains(colName)) {
                        DT.Columns[colName].ExtendedProperties["fitted"] = null;
                    }
                }
            }

            //Aggiorna impostazioni listcolpos e visible
            int n_visibili = 0;
            int n_invisibili = 0;
			foreach (Column col in gridX.Columns) {
                if (!col.Visible) continue;
                n_visibili++;
				string filter = "colname = '" + col.FieldName + "'";
				System.Data.DataRow[] rows = DS.customviewcolumn.Select(filter);
				if (rows.Length > 0) {
                    if (rows[0]["visible"].ToString() != "1") {
                        rows[0]["visible"] = "1";
                        if (DT.Columns.Contains(col.FieldName)) {
                            DT.Columns[col.FieldName].ExtendedProperties["fitted"] = null;
                        }
                    }

					//in prima posizione l'indice è zero
					rows[0]["listcolpos"] = convertPositionToIndex(col.VisibleIndex,false);
						//ex = col.VisibleIndex;

					rows[0]["colwidth"] = col.Width;				   
				}
			}
            foreach (Column col in gridX.Columns) {
                if (col.Visible) continue;
                n_invisibili++;
                string filter = "colname = '" + col.FieldName + "'";
                System.Data.DataRow[] rows = DS.customviewcolumn.Select(filter);
                if (rows.Length > 0) {
                    rows[0]["visible"] = "0";

                    //in prima posizione l'indice è zero
                    rows[0]["listcolpos"] = n_invisibili + n_visibili;
                    //ex = col.VisibleIndex;

                    rows[0]["colwidth"] = col.Width;
                }
            }


			ArrayList todelete= new ArrayList();
			//Aggiorna inpostazioni orderby
			//elimina dal dataset gli orderby che ci sono e che non ci dovrebbero essere
			//aggiorna inoltre quelli che ci sono
			foreach (System.Data.DataRow ExistClause in DS.customvieworderby.Select()){
				if (ExistClause.RowState== DataRowState.Deleted) continue;
				string fieldname = ExistClause["columnname"].ToString();
				Column C = gridX.Columns[fieldname];
				int SortedIndex = gridX.SortedColumns.IndexOf(C);
				if (SortedIndex<0){
					//DataRow must be deleted.
					todelete.Add(ExistClause);
				}
				else {
					//Column exists-> update it, or delete it if SortDirection = none
					switch (C.SortDirection){
						case SortDirection.Ascending: 
							ExistClause["direction"]=0;
							break;
						case SortDirection.Descending: 
							ExistClause["direction"]=1;
							break;
						default:
							todelete.Add(ExistClause);
							break;
					}
				}
			}
			for (int delr=0; delr< todelete.Count; delr++) 
					((System.Data.DataRow) todelete[delr]).Delete();


			//Aggiunge al dataset gli order by presenti nel grid e non presenti nel dataset
			foreach (Column SortCol in gridX.SortedColumns){
				if (SortCol.SortDirection== SortDirection.None) continue;
				string fieldname= SortCol.FieldName;
				string filter = "(columnname='"+fieldname+"')";
				System.Data.DataRow RowFound;
				System.Data.DataRow []DeletedFound= 
					DS.customvieworderby.Select(filter,null,DataViewRowState.Deleted);
				if (DeletedFound.Length>0){
					RowFound = DeletedFound[0];
					RowFound.RejectChanges();
					if (SortCol.SortDirection== SortDirection.Ascending)
						RowFound["direction"]=0;
					else
						RowFound["direction"]=1;
					continue;
				}
				System.Data.DataRow []Found= DS.customvieworderby.Select(filter);
				if (Found.Length>0) continue;
				System.Data.DataRow NewSort = DS.customvieworderby.NewRow();
				NewSort["objectname"] = DT.TableName;
				NewSort["viewname"]= m_listtype;
				NewSort["periodnumber"]= myMaxFromColumn(DS.customvieworderby,"periodnumber")+1;
				NewSort["columnname"]= fieldname;
				if (SortCol.SortDirection== SortDirection.Ascending)
					NewSort["direction"]=0;
				else
					NewSort["direction"]=1;
				DS.customvieworderby.Rows.Add(NewSort);				
			}
		    last_columnlist = getColumnlist();

		}
		int myMaxFromColumn(DataTable T, string field){
			int max=-1;
			foreach(System.Data.DataRow R in T.Rows){
				object valore;
				if (R.RowState== DataRowState.Deleted)
					valore = R[field, DataRowVersion.Original];
				else
					valore = R[field];
				if (valore==DBNull.Value) continue;
				int ivalore= Convert.ToInt32(valore);
				if (ivalore>max) max=ivalore;
			}
			return max;
		}

		#endregion Impostazioni e metodi della griglia Colonne

		#region Impostazioni e metodi della griglia Selezione

		/// <summary>
		/// Imposta il tab di selezione ed effettua il binding con DS
		/// </summary>
		private void setPageSelezione() {
			
			//Imposta valori di default
			
			applicaStatoSelezione("save");//x default lo stato è save		
			radioAND.Checked = true;//x default il connector vale AND

			setGridSelezione();

			//E' necessario reimpostare le property x aggiornare la listbox
			//sul cambio di listtype da combo
			lbColonna.DataSource = DS.customviewcolumn;
			lbColonna.DisplayMember = "heading";
			lbColonna.ValueMember = "colname";

			//Alla partenza controllo il numero di righe in viewwhere
			//per abilitare il button DeleteAll
			enableDisableDeleteAllCond();
		}

		/// <summary>
		/// Abilita / disabilita il tasto Elimina tutte (le condizioni di where)
		/// </summary>
		private void enableDisableDeleteAllCond() {
			System.Data.DataRow[] rows = DS.customviewwhere.Select();
			btnDeleteAllCond.Enabled = (rows.Length > 0);
		}

		/// <summary>
		/// Imposta  gridSelezione ed effettua binding con DS
		/// </summary>
		private void setGridSelezione() {
			//serve per il BindingContext
			gridSelezione.SetDataBinding(DS, "customviewwhere");

			//Visible
			gridSelezione.Columns["objectname"].Visible = false;
			gridSelezione.Columns["viewname"].Visible = false;
			gridSelezione.Columns["periodnumber"].Visible = false;
			gridSelezione.Columns["connector"].Visible = false;
			gridSelezione.Columns["runtime"].Visible = false;
			gridSelezione.Columns["lastmodtimestamp"].Visible = false;
			gridSelezione.Columns["lastmoduser"].Visible = false;
			gridSelezione.Columns["createtimestamp"].Visible = false;
			gridSelezione.Columns["createuser"].Visible = false;
			//VisibleIndex
			gridSelezione.Columns["columnname"].VisibleIndex = 0;
			gridSelezione.Columns["operator"].VisibleIndex = 1;
			gridSelezione.Columns["value"].VisibleIndex = 2;
			//Autosize
			gridSelezione.Columns["columnname"].Width = 120;
//				gridSelezione.Columns["columnname"].GetFittedWidth();
			gridSelezione.Columns["operator"].Width = 140;
//				gridSelezione.Columns["operator"].GetFittedWidth();
//			gridSelezione.Columns["value"].Width = 
//				gridSelezione.Columns["value"].GetFittedWidth();
			//Allineamento
			gridSelezione.Columns["operator"].HorizontalAlignment =
				Xceed.Grid.HorizontalAlignment.Left;
			//Combo
			var combo = getBaseGridComboBox();
			gridSelezione.Columns["columnname"].CellViewer = combo;
			combo.DataSource = DS.customviewcolumn;
			combo.DisplayMember = "heading";
			combo.ValueMember = "colname";
			new comboTableManager(model, DS.customviewcolumn, combo);

			combo = getBaseGridComboBox();
            gridSelezione.Columns["operator"].CellViewer = combo;
			combo.DataSource = DS.customoperator;
			combo.DisplayMember = "name";
			combo.ValueMember = "idoperator";
			//Solo visualizzazione
			gridSelezione.ReadOnly = true;
			new comboTableManager(model, DS.customoperator, combo);


			foreach (Cell cell in gridSelezione.DataRowTemplate.Cells) {
				cell.Click += rowTemplateSelect_RowSelector_Click;
			}
		}

		private void rowTemplateSelect_RowSelector_Click(object sender, System.EventArgs e) {
			gridSelezioneRowClick();
		}

		private void gridSelezioneRowClick() {
			if (!update_enabled) return;
			update_enabled = false;
			try {
				var row = getSelectedRow(gridSelezione, DS, "customviewwhere");
				m_LastSelectRowGridSelezione = row;
				btnEditCond.Enabled = true;
				btnDeleteCond.Enabled = true;
				lbColonna.SelectedValue = row["columnname"];
				lbOperatore.SelectedValue = row["operator"];
				if (row["runtime"].ToString() != "1")
					txtValore.Text = row["value"].ToString();
				else
					chkRuntime.Checked = true;
				if (row["connector"].ToString() != "1")
					radioAND.Checked = true;
				else
					radioOR.Checked = true;
			}
			catch {
			}
			update_enabled = true;
		}

		/// <summary>
		/// In base allo stato del form abilita/disabilita buttons
		/// </summary>
		/// <param name="stato">Stato del form</param>
		private void applicaStatoSelezione(string stato) {
			m_selectionstate = stato;
			switch(stato.ToLower()) {
				case "insert":
					btnAddCond.Enabled = false;
					btnEditCond.Enabled = false;
					btnDeleteCond.Enabled = false;
					btnDeleteAllCond.Enabled = false;
					btnSaveCond.Enabled = true;
					btnCancelCond.Enabled = true;
					enableDisableWhereEnvironment(true);
					break;
				case "edit":
					btnAddCond.Enabled = false;
					btnEditCond.Enabled = false;
					btnDeleteCond.Enabled = false;
					btnDeleteAllCond.Enabled = false;
					btnSaveCond.Enabled = true;
					btnCancelCond.Enabled = true;
					enableDisableWhereEnvironment(true);
					break;
				case "save":
					btnAddCond.Enabled = true;
					btnEditCond.Enabled = false;
					btnDeleteCond.Enabled = false;
					enableDisableDeleteAllCond();
					btnSaveCond.Enabled = false;
					btnCancelCond.Enabled = false;
					enableDisableWhereEnvironment(false);		
					break;
			}
		}

		private void btnAddCond_Click(object sender, EventArgs e) {
            if (destroyed) return;
            applicaStatoSelezione("insert");
		}

		private void btnEditCond_Click(object sender, EventArgs e) {
            if (destroyed) return;
            applicaStatoSelezione("edit");		
		}

		private void btnCancelCond_Click(object sender, EventArgs e) {
            if (destroyed) return;
            applicaStatoSelezione("save");
		}

		private void btnDeleteCond_Click(object sender, EventArgs e) {
            if (destroyed) return;
            deleteCustomViewWhere();
			applicaStatoSelezione("save");
		}

		private void btnDeleteAllCond_Click(object sender, EventArgs e) {
            if (destroyed) return;
            deleteAllCustomViewWhere();
			applicaStatoSelezione("save");
		}

		private void btnSaveCond_Click(object sender, EventArgs e) {
            if (destroyed) return;
            if ((lbColonna.SelectedIndex == -1)) {
				showMsg(LM.colRequired, LM.warningLabel,
                    mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Exclamation);
				return;
			}		
			if (lbOperatore.SelectedIndex == -1) {
				showMsg(LM.operatorRequired, LM.warningLabel,
                    mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Exclamation);
				return;
			}		
			if (!chkRuntime.Checked && txtValore.Text == "") {
				if (CountOperands(lbOperatore.SelectedIndex)>0){
					showMsg(LM.valueRequired, LM.warningLabel, mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Exclamation);
					return;
				}
			}

		    if (salvaWhereCondition()) {
		        applicaStatoSelezione("save");
		    }			
		}

		/// <summary>
		/// Memorizza/cancella nel DS le where condition
		/// </summary>
		private bool salvaWhereCondition() {
			switch (m_selectionstate) {
				case "insert":
					return addToCustomViewWhere();
				case "edit":
					return updateCustomViewWhere();
			}

		    return false;
		}

		/// <summary>
		/// Aggiorna la riga selezionata
		/// </summary>
		private bool updateCustomViewWhere() {
			if (m_LastSelectRowGridSelezione == null) return false;
			return setDataWhereCond(m_LastSelectRowGridSelezione);
		}

		/// <summary>
		/// Elimina la riga selezionata da customviewwhere
		/// </summary>
		private void deleteCustomViewWhere() {
			if (showMsg(C_MSG_DELETE, C_MSG_TITLE_DELETE,
                mdl.MessageBoxButtons.YesNoCancel, mdl.MessageBoxIcon.Question) !=
                mdl.DialogResult.Yes) return;

			if (m_LastSelectRowGridSelezione == null) return;
			m_LastSelectRowGridSelezione.Delete();
			enableDisableDeleteAllCond();
		}

		/// <summary>
		/// Elimina dal dataset tutte le righe di customviewwhere per listtype
		/// </summary>
		private void deleteAllCustomViewWhere() {
			if (showMsg(C_MSG_DELETE, C_MSG_TITLE_DELETE,
                mdl.MessageBoxButtons.YesNoCancel, mdl.MessageBoxIcon.Question) !=
                mdl.DialogResult.Yes) return;
			
			try {
				System.Data.DataRow[] rows = DS.customviewwhere.Select();
				foreach (System.Data.DataRow row in rows)
					row.Delete();
			}
			catch (Exception exc) {
			    shower.ShowException(this, LM.impossibleToDeleteConditions, exc);
			}
		}

		/// <summary>
		/// Valorizza la datarow con i dati selezionati dall'utente
		/// </summary>
		/// <param name="row">riga da valorizzare</param>
		private bool setDataWhereCond(System.Data.DataRow row) {
			row["columnname"] = lbColonna.SelectedValue.ToString();
			row["operator"] = lbOperatore.SelectedValue.ToString();
			if (chkRuntime.Checked)  {
				row["value"] = null;
				row["runtime"] = "1";
			}
			else {
				if (txtValore.Text.Contains(";")) {
					row["value"] = txtValore.Text;
				}
				else {
					var B = conn.CreateTableByName(mainTableName, "*");
					if (B.Columns.Contains(row["columnname"].ToString())) {
						object val = HelpUi.GetObjectFromString(B.Columns[row["columnname"].ToString()].DataType,
							txtValore.Text,
							"x.y.g");
						if (val == null) {
							MetaFactory.factory.getSingleton<IMessageShower>().Show(
								$"Il valore {txtValore.Text} non è adatto al tipo della colonna selezionata ({row["columnname"]})",
								"Errore");

							return false;
						}

						row["value"] = val.ToString();

					}
					else {
						row["value"] = txtValore.Text;
					}
				}

				row["runtime"] = "0";
			}
			if (radioAND.Checked)
				row["connector"] = 0;
			else
				row["connector"] = 1;
		    return true;
		}

		/// <summary>
		/// Aggiunge una riga customviewwhere al Dataset
		/// </summary>
		private bool addToCustomViewWhere() {
			try {
				var row = DS.customviewwhere.NewRow();
				if (RowChange.MakeChild(DS.customview.Rows[0], DS.customview,
					row, "customviewcustomviewwhere")) {
					RowChange.MarkAsAutoincrement(DS.customviewwhere.Columns["periodnumber"],
						null, null, 6);
					RowChange.SetSelector(DS.customviewwhere, "objectname");
					RowChange.SetSelector(DS.customviewwhere, "viewname");
					RowChange.CalcTemporaryID(row);
					if (!setDataWhereCond(row)) return false;
					DS.customviewwhere.Rows.Add(row);
				}
			    else {
			        showMsg("Failed MakeChild", C_MSG_TITLE_ERRORE,
			            mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Error);
				    return false;
				}
			}
			catch (Exception e) {
                shower.ShowException(this,LM.impossibleToSaveCondition,e);
			    return false;
			}

		    return true;
		}

		private void chkRuntime_CheckedChanged(object sender, EventArgs e) {
            if (destroyed) return;
            txtValore.Enabled = !chkRuntime.Checked;
			btnValore.Enabled = !chkRuntime.Checked;
			txtValore.Text = null;
		}

		/// <summary>
		/// Abilita/disabilita campi del form (DIPENDE DA ApplicaStato)
		/// </summary>
		/// <param name="valore"></param>
		private void enableDisableWhereEnvironment(bool valore) {
			lbColonna.Enabled = valore;
			lbOperatore.Enabled = valore;
			txtValore.Enabled = valore;
			btnValore.Enabled = valore;
			chkRuntime.Enabled = valore;
			panelConnector.Enabled = valore;
		}

		#endregion Impostazioni e metodi della griglia Selezione


		#region Impostazioni e metodi della griglia Somme Parziali

        void setPageSum() {
            //lbsumfield.Items.Clear();
            foreach(System.Data.DataRow R in DS.customviewcolumn.Select()) {
                string fieldtoadd = R["colname"].ToString();
				if (gridX.Columns[fieldtoadd]==null) continue;
				Column CC = gridX.Columns[fieldtoadd];
				if (CC.Visible==false) continue;
				if ((CC.DataType != typeof(decimal)) &&
					(CC.DataType != typeof(double)) &&
					(CC.DataType != typeof(float)) &&
					(CC.DataType != typeof(int))&&
					(CC.DataType != typeof(short))
					) continue;

				if (lbsumfield.Items.IndexOf(R["heading"].ToString())>=0) continue;
				lbsumfield.Items.Add(R["heading"].ToString(), false);
			}
			int i=0;
			while (i< lbsumfield.Items.Count){
				string currfield = lbsumfield.Items[i].ToString();
				string filter = "(heading="+mdl_utils.Quoting.quotedstrvalue(currfield,false)+")";
				if (DS.customviewcolumn.Select(filter).Length>0) {
					i++;
				}
				else {
					lbsumfield.Items.RemoveAt(i);
				}
			}
		}



		#endregion

		#region Impostazioni e metodi della griglia Ordinamento

		/// <summary>
		/// Imposta Tab Ordinamento ed effettua binding con DS
		/// </summary>
		private void setPageOrdinamento() {
			//x default lo stato è save
			applicaStatoOrdinamento("save");

			//x default l'ordianmento vale crescente
			radioASC.Checked = true;

			setGridOrdinamento();

			//E' necessario reimpostare le property x aggiornare la listbox
			//sul cambio di listtype da combo
			lbColOrderBy.DataSource = DS.customviewcolumn;
			lbColOrderBy.DisplayMember = "heading";
			lbColOrderBy.ValueMember = "colname";
		}

		/// <summary>
		/// Imposta il grid Ordinamento ed effettua binding con DS
		/// </summary>
		private void setGridOrdinamento() {
			//serve per il BindingContext
			gridOrderBy.SetDataBinding(DS, "customvieworderby");

			//Visible
			gridOrderBy.Columns["objectname"].Visible = false;
			gridOrderBy.Columns["viewname"].Visible = false;
			gridOrderBy.Columns["periodnumber"].Visible = false;
			gridOrderBy.Columns["lastmodtimestamp"].Visible = false;
			gridOrderBy.Columns["lastmoduser"].Visible = false;
			gridOrderBy.Columns["createtimestamp"].Visible = false;
			gridOrderBy.Columns["createuser"].Visible = false;
			//VisibleIndex
			gridOrderBy.Columns["columnname"].VisibleIndex = 0;
			gridOrderBy.Columns["direction"].VisibleIndex = 1;
			//Allineamento
			gridOrderBy.Columns["direction"].HorizontalAlignment =
				Xceed.Grid.HorizontalAlignment.Left;
			//CellViewer
			var combo = getBaseGridComboBox();
			gridOrderBy.Columns["columnname"].CellViewer = combo;
			combo.DataSource = DS.customviewcolumn;
			combo.DisplayMember = "heading";
			combo.ValueMember = "colname";
			new comboTableManager(model, DS.customviewcolumn, combo);

			loadCustomDirection();
			combo = getBaseGridComboBox();
			gridOrderBy.Columns["direction"].CellViewer = combo;
			combo.DataSource = DS.customdirection;
			combo.DisplayMember = "valore";
			combo.ValueMember = "direction";

			//Solo visualizzazione
			gridOrderBy.ReadOnly = true;

			foreach (Cell cell in gridOrderBy.DataRowTemplate.Cells) {
				cell.Click += this.rowTemplateOrderBy_RowSelector_Click;
			}
		}

		/// <summary>
		/// Inserisce due righe nella tabella temporanea customdirection
		/// per mappare 0 - Crescente e 1 - Decrescente
		/// </summary>
		private void loadCustomDirection() {
			var row = DS.customdirection.NewRow();
			row["direction"] = "0";
			row["valore"] = "Crescente";
			DS.customdirection.Rows.Add(row);
			row = DS.customdirection.NewRow();
			row["direction"] = "1";
			row["valore"] = "Decrescente";
			DS.customdirection.Rows.Add(row);
			DS.customdirection.AcceptChanges();
		}

		private void rowTemplateOrderBy_RowSelector_Click(object sender, System.EventArgs e) {
            if (destroyed) return;
            gridOrdinamentoRowClick();
		}

		private void gridOrdinamentoRowClick() {
			if (!update_enabled) return;
			update_enabled = false;
			try {
				var row = getSelectedRow(gridOrderBy, DS, "customvieworderby");
				m_LastSelectRowGridOrderBy = row;
				btnEditOrderBy.Enabled = true;
				btnDeleteOrderBy.Enabled = true;
				lbColOrderBy.SelectedValue = row["columnname"];
				if (row["direction"].ToString() != "1")
					radioASC.Checked = true;
				else
					radioDESC.Checked = true;
			}
			catch {
			}
			update_enabled = true;
		}

		/// <summary>
		/// In base allo stato del form abilita/disabilita buttons
		/// </summary>
		/// <param name="stato">Stato del form</param>
		private void applicaStatoOrdinamento(string stato) {
			m_orderbystate = stato;
			switch(stato.ToLower()) {
				case "insert":
					btnAddOrderBy.Enabled = false;
					btnEditOrderBy.Enabled = false;
					btnDeleteOrderBy.Enabled = false;
					btnDeleteAllOrderBy.Enabled = false;
					btnSaveOrderBy.Enabled = true;
					btnCancelOrderBy.Enabled = true;
					enableDisableOrderByEnvironment(true);
					break;
				case "edit":
					btnAddOrderBy.Enabled = false;
					btnEditOrderBy.Enabled = false;
					btnDeleteOrderBy.Enabled = false;
					btnDeleteAllOrderBy.Enabled = false;
					btnSaveOrderBy.Enabled = true;
					btnCancelOrderBy.Enabled = true;
					enableDisableOrderByEnvironment(true);
					break;
				case "save":
					btnAddOrderBy.Enabled = true;
					btnEditOrderBy.Enabled = false;
					btnDeleteOrderBy.Enabled = false;
					enableDisableDeleteAllOrderBy();
					btnSaveOrderBy.Enabled = false;
					btnCancelOrderBy.Enabled = false;
					enableDisableOrderByEnvironment(false);		
					break;
			}
		}

		private void btnAddOrderBy_Click(object sender, EventArgs e) {
			applicaStatoOrdinamento("insert");
		}

		private void btnEditOrderBy_Click(object sender, EventArgs e) {
			applicaStatoOrdinamento("edit");		
		}

		private void btnDeleteOrderBy_Click(object sender, EventArgs e) {
			deleteCustomViewOrderBy();
			applicaStatoOrdinamento("save");
		}

		private void btnDeleteAllOrderBy_Click(object sender, EventArgs e) {
			deleteAllCustomViewOrderBy();
			applicaStatoOrdinamento("save");
		}

		private void btnSaveOrderBy_Click(object sender, EventArgs e) {
			if (lbColOrderBy.SelectedIndex == -1) {
				showMsg("E' necessario selezionare una colonna", "Attenzione",
                    mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Exclamation);
				return;
			}		
			salvaOrderBy();
			applicaStatoOrdinamento("save");
		}

		private void btnCancelOrderBy_Click(object sender, EventArgs e) {
			applicaStatoOrdinamento("save");
		}

		/// <summary>
		/// Memorizza/cancella nel DS gli orderby
		/// </summary>
		private void salvaOrderBy() {
			switch (m_orderbystate) {
				case "insert":
					addToCustomViewOrderBy();
					break;
				case "edit":
					updateCustomViewOrderBy();
					break;
			}
		}

		/// <summary>
		/// Aggiunge una riga customvieworderby al Dataset
		/// </summary>
        private void addToCustomViewOrderBy() {
            try {
                System.Data.DataRow row = DS.customvieworderby.NewRow();
                if(RowChange.MakeChild(DS.customview.Rows[0], DS.customview,
                    row, "customviewcustomvieworderby")) {
                    RowChange.MarkAsAutoincrement(DS.customvieworderby.Columns["periodnumber"],
                        null, null, 6);
					RowChange.SetSelector(DS.customviewwhere, "objectname");
					RowChange.SetSelector(DS.customviewwhere, "viewname");
					RowChange.CalcTemporaryID(row);
					setDataOrderBy(row);
					DS.customvieworderby.Rows.Add(row);
				}
				else
					showMsg("Failed MakeChild", C_MSG_TITLE_ERRORE, mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Error);
			}
			catch (Exception e) {
				showMsg($"Impossibile salvare la riga\r\rDettaglio: {e.Message}", C_MSG_TITLE_ERRORE, mdl.MessageBoxButtons.OK,
                    mdl.MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Valorizza la datarow con i dati selezionati dall'utente
		/// </summary>
		/// <param name="row">riga da valorizzare</param>
		private void setDataOrderBy(System.Data.DataRow row) {
			row["columnname"] = lbColOrderBy.SelectedValue.ToString();
			if (radioASC.Checked)
				row["direction"] = "0";
			else
				row["direction"] = "1";
		}

		/// <summary>
		/// Aggiorna la riga selezionata
		/// </summary>
		private void updateCustomViewOrderBy() {
			if (m_LastSelectRowGridOrderBy == null) return;
			setDataOrderBy(m_LastSelectRowGridOrderBy);
		}

		/// <summary>
		/// Elimina la riga selezionata da customvieworderby
		/// </summary>
		private void deleteCustomViewOrderBy() {
			if (showMsg(C_MSG_DELETE, C_MSG_TITLE_DELETE,
                mdl.MessageBoxButtons.YesNoCancel, mdl.MessageBoxIcon.Question) !=
                mdl.DialogResult.Yes) return;

			if (m_LastSelectRowGridOrderBy == null) return;
			m_LastSelectRowGridOrderBy.Delete();
			enableDisableDeleteAllOrderBy();
		}

		/// <summary>
		/// Elimina dal dataset tutte le righe di customvieworderby per listtype
		/// </summary>
		private void deleteAllCustomViewOrderBy() {
			if (showMsg(C_MSG_DELETE, C_MSG_TITLE_DELETE,
                mdl.MessageBoxButtons.YesNoCancel, mdl.MessageBoxIcon.Question) !=
                mdl.DialogResult.Yes) return;
			
			try {
				System.Data.DataRow[] rows = DS.customvieworderby.Select();
				foreach (System.Data.DataRow row in rows)
					row.Delete();
			}
			catch (Exception exc) {
				showMsg($"Impossibile eliminare le righe\r\rDettaglio: {exc.Message}", C_MSG_TITLE_ERRORE, mdl.MessageBoxButtons.OK,
                    mdl.MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Abilita/disabilita campi del page orderby (DIPENDE DA ApplicaStato)
		/// </summary>
		/// <param name="valore">Abilita/disabilita</param>
		private void enableDisableOrderByEnvironment(bool valore) {
			lbColOrderBy.Enabled = valore;
			panelOrderBy.Enabled = valore;
		}

		/// <summary>
		/// Abilita / disabilita il tasto Elimina tutti (ordinamenti)
		/// </summary>
		private void enableDisableDeleteAllOrderBy() {
			System.Data.DataRow[] rows = DS.customvieworderby.Select();
			btnDeleteAllOrderBy.Enabled = (rows.Length > 0);
		}

		#endregion Impostazioni e metodi della griglia Ordinamento

		#region Menu contestuale
		
		/// <summary>
		/// Crea il menu contestuale con le voci Nascondi colonna, Salva Elenco,
		///  Copia Elenco, Elimina Elenco 
		/// </summary>
		private void createContextMenu() {

			mnuContextMenu = new ContextMenu();
			this.ContextMenu = mnuContextMenu;

            mnuItemFilter = new MyMenuItem("Filtra") {
                Tag = "filter",
                Enabled = true
            };
            mnuContextMenu.MenuItems.Add(mnuItemFilter);

            mnuItemHide = new MyMenuItem("Nascondi colonna") {
                Tag = "hidecolumn",
                Enabled = true
            };
            mnuContextMenu.MenuItems.Add(mnuItemHide);


            mnuCompressAll = new MyMenuItem("Comprimi tutti i gruppi") {
                Tag = "compress",
                Enabled = true
            };
            mnuContextMenu.MenuItems.Add(mnuCompressAll);

            mnuExpandAll = new MyMenuItem("Espandi tutti i gruppi") {
                Tag = "expand",
                Enabled = true
            };
            mnuContextMenu.MenuItems.Add(mnuExpandAll);

			mnuContextMenu.MenuItems.Add("-");

            mnuItemApply = new MyMenuItem("Salva elenco") {
                Tag = "save"
            };
            mnuContextMenu.MenuItems.Add(mnuItemApply);
            mnuItemCopy = new MyMenuItem("Copia elenco") {
                Tag = "copy"
            };
            mnuContextMenu.MenuItems.Add(mnuItemCopy);
            mnuItemDelete = new MyMenuItem("Elimina l'elenco " + m_listtype) {
                Tag = "delete"
            };
            mnuContextMenu.MenuItems.Add(mnuItemDelete);

            // Add functionality to the menu items using the Click event. 
            mnuItemFilter.Click += new EventHandler(eventPopupMenuItem);
            mnuItemHide.Click += new EventHandler(eventPopupMenuItem);
			mnuItemApply.Click += new EventHandler(eventPopupMenuItem);
			mnuItemCopy.Click += new EventHandler(eventPopupMenuItem);
			mnuItemDelete.Click += new EventHandler(eventPopupMenuItem);
			mnuExpandAll.Click+= new EventHandler(mnuExpandAll_Click);
			mnuCompressAll.Click+= new EventHandler(mnuCompressAll_Click);
		}

		/// <summary>
		/// Abilita/Disabilita le voci del menu contestuale in base al fatto che 
		///   il listing type è di tipo system e/o filter locked
		/// </summary>
		/// <param name="m_listtypeissystem"></param>
		private void updateContextMenu(bool m_listtypeissystem) {
			mnuItemApply.Enabled = (!filterlocked) && (!m_listtypeissystem);
			mnuItemDelete.Enabled = (!filterlocked) && (!m_listtypeissystem);
			mnuItemCopy.Enabled= (!filterlocked);
		}


		Xceed.Grid.Column LastSelectedColumn;
		void hideColumn(){
			if (LastSelectedColumn==null) return;
			if (!LastSelectedColumn.Visible) return;
//			Cell CC = gridX.CurrentCell;
//			Column C = CC.ParentColumn;
//			if (C==null) return;
			
			int lastpos = gridX.SortedColumns.IndexOf(LastSelectedColumn);
			if (lastpos!=-1) gridX.SortedColumns.Remove(LastSelectedColumn);
			//gridX.SortedColumns[lastpos].SortDirection= SortDirection.None;
			LastSelectedColumn.Visible=false;
		}

	    void filterColumn() {
	        if (LastSelectedColumn == null) return;
	        if (!LastSelectedColumn.Visible) return;
	        string field = LastSelectedColumn.FieldName;
	        var f = new FrmFiltraColonna();
	        f.txtColName.Text = LastSelectedColumn.Title;
            MetaFactory.factory.getSingleton<IFormCreationListener>().create(f, this);
            if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK) return;
	        int op = f.result;
	        string val = f.txtValore.Text;

	        var row = DS.customviewwhere.NewRow();
	        if (RowChange.MakeChild(DS.customview.Rows[0], DS.customview,
	            row, "customviewcustomviewwhere")) {
	            RowChange.MarkAsAutoincrement(DS.customviewwhere.Columns["periodnumber"],
	                null, null, 6);
	            RowChange.SetSelector(DS.customviewwhere, "objectname");
	            RowChange.SetSelector(DS.customviewwhere, "viewname");
	            RowChange.CalcTemporaryID(row);
	            
	            

                row["columnname"] = field;
                row["operator"] = op.ToString();
                row["value"] = val;
                row["runtime"] = "0";
                row["connector"] = 0;
                DS.customviewwhere.Rows.Add(row);

            }
	        applicaListType();

	    }

	    private void eventPopupMenuItem(object sender, EventArgs e) {
            if (destroyed) return;
            var mnuItem = (MyMenuItem) sender;
			switch(mnuItem.Tag.ToString().ToLower()) {
				case "save":
					salvaElenco(); break;
				case "copy":
					copiaElenco(); break;
				case "delete":
					eliminaElenco(); break;
				case "hidecolumn":
					hideColumn();
					break;
                case "filter":
			        filterColumn();
                    break;
			}
		}

		#endregion Menu contestuale

		#region Metodi di utility e globali

		/// <summary>
		/// Ultima riga selezionata sulla grid
		/// </summary>
		/// <param name="grid">Xceed GridControl</param>
		/// <param name="DataSource">Il dataset a cui è agganciato il grid</param>
		/// <param name="TableName">Il nome della tabella</param>
		/// <returns>DataRow</returns>
		private System.Data.DataRow getSelectedRow(GridControl grid,
			DataSet DataSource, string TableName) {
            try {
                var view = (DataRowView)grid.BindingContext[DataSource, TableName].Current;
                return  view.Row;
			}
			catch (Exception E) {
                errorLogger.logException(LM.errorSelectingRow, E);
			    shower.ShowException(this,LM.errorSelectingRow, E);
				return null;
			}
		}

		/// <summary>
		/// Esegue il commit dei dati relativi alle impostazioni della grid
		/// Elenchi
		/// </summary>
		/// <param name="ds">Il dataset tipizzato custom view</param>
		/// <returns></returns>
        private bool eseguiPostData(VistaFormCustomView ds) {
            ds.fieldtosum.AcceptChanges();
            var MyMeta = dispatcher.Get("customview");
            PostData.MarkAsTemporaryTable(ds.fieldtosum, false);

            var mPostdata = MyMeta.Get_PostData();
            mPostdata.initClass(ds, conn);
            return mPostdata.DO_POST();
        }


		/// <summary>
		/// Impostazioni comuni a tutte le combo della grid
		/// </summary>
		/// <returns></returns>
		private GridComboBox getBaseGridComboBox() {
            return new GridComboBox {DropDownStyle = ComboBoxStyle.DropDownList};
        }

		private mdl.DialogResult showMsg(string msg, string caption,
			mdl.MessageBoxButtons button, mdl.MessageBoxIcon icon) {
			return shower.Show(null,msg, caption, button);
		}

		#endregion Metodi di utility e globali

		#region Salva elenco

		private bool salvaElenco() {
			
			var Curr= (DataRowView) gridSelezione.BindingContext[DS,DS.customviewcolumn.TableName].Current;
			if ((Curr!=null)&& (Curr.IsEdit)) Curr.EndEdit();
			

			if (showMsg(C_MSG_SAVE, C_MSG_TITLE_SAVE, mdl.MessageBoxButtons.YesNo, mdl.MessageBoxIcon.Question) == mdl.DialogResult.Yes) {

				//applico modifiche
				bool res = eseguiPostData(DS);
				return res;
			}
			else
				return false;
		}

		private void btnApply1_Click(object sender, System.EventArgs e) {
			salvaElenco();
		}

		private void btnApply2_Click(object sender, System.EventArgs e) {
			salvaElenco();	
		}

		private void btnApply3_Click(object sender, System.EventArgs e) {
			salvaElenco();
		}

		#endregion Salva elenco

		#region Copia elenco

        private void copiaElenco() {

            try {
                var Curr = (DataRowView)gridSelezione.BindingContext[DS,DS.customviewcolumn.TableName].Current;
                if((Curr != null) && (Curr.IsEdit)) Curr.EndEdit();
            }
            catch { }

			string[] list = getListTypes();
			var f = new FormCopyList(list);
            MetaFactory.factory.getSingleton<IFormCreationListener>().create(f, null);
            var res = f.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK) {
				string newlist= f.newlisttype;			
				salvaConNome(newlist);
				restartWithNewListType(newlist);
			}
		}

		private string[] getListTypes() {
			string[] elenchi = new string[cboList.Items.Count];
			int i = 0;
			foreach (DataRowView rowview in cboList.Items) {
				elenchi[i] = rowview.Row["viewname"].ToString();
				i++;
			}
			return elenchi;
		}

//		/// <summary>
//		/// Change the current listing type
//		/// </summary>
//		/// <param name="newlisttype"></param>
//		public void SetListType(string newlisttype) {
//			m_lastlisttype = m_listtype;
//			m_listtype = newlisttype;
//		}

		/// <summary>
		/// Gets the current List Type name
		/// </summary>
		/// <returns></returns>
		public string getListType() {
			return m_listtype;
		}

		private void btnCopy1_Click(object sender, EventArgs e) {
			copiaElenco();
		}
		private void btnCopy2_Click(object sender, EventArgs e) {
			copiaElenco();
		}
		private void btnCopy3_Click(object sender, EventArgs e) {
			copiaElenco();
		}

		/// <summary>
		/// Cambia il nome del listing type corrente, modificando il dataset DS,
		///  e poi salva su DB la nuova versione
		/// </summary>
		/// <param name="newlisttype"></param>
		private void salvaConNome(string newlisttype) {
			try {
				DSCopy = new VistaFormCustomView();
				PostData.MarkAsTemporaryTable(DSCopy.fieldtosum,false);
				ClearDataSet.RemoveConstraints(DSCopy);
				copyCustomTable(DS, DSCopy, DS.connector.TableName, null);
				DSCopy.connector.AcceptChanges();
				copyCustomTable(DS,DSCopy, DS.customoperator.TableName, null);
				DSCopy.customoperator.AcceptChanges();

				copyCustomTable(DS, DSCopy, DS.customview.TableName, newlisttype);
				copyCustomTable(DS, DSCopy, DS.customviewcolumn.TableName, newlisttype);
				copyCustomTable(DS, DSCopy, DS.customviewwhere.TableName, newlisttype);
				copyCustomTable(DS, DSCopy, DS.customvieworderby.TableName, newlisttype);
				eseguiPostData(DSCopy);
			}
			catch (Exception e) {
				showMsg($"Impossibile copiare l'elenco\r\rDettaglio: {e.Message}", C_MSG_TITLE_ERRORE, mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Error);
				return;
			}
		}

		//
		private bool copyCustomTable(
			VistaFormCustomView Source,
			VistaFormCustomView Dest,
			string tabella,
			string newlisttype
			) {
			foreach (System.Data.DataRow row in Source.Tables[tabella].Rows) {
				var rowcopy = Dest.Tables[tabella].NewRow();
				foreach (DataColumn col in Source.Tables[tabella].Columns) {
					switch(col.ColumnName.ToLower()) {
						case "viewname":
							rowcopy[col.ColumnName] = newlisttype;
							break;
						case "issystem":
							rowcopy[col.ColumnName] = "N";
							break;
						default:
							rowcopy[col.ColumnName] = row[col.ColumnName];
							break;
					}
				}
				DSCopy.Tables[tabella].Rows.Add(rowcopy);
			}
			return true;
		}

		#endregion Copia elenco

		#region Elimina elenco

		private void btnDelete1_Click(object sender, EventArgs e) {
			eliminaElenco();
		}
		private void btnDelete2_Click(object sender, EventArgs e) {
			eliminaElenco();
		}
		private void btnDelete3_Click(object sender, EventArgs e) {
			eliminaElenco();
		}

		private void eliminaElenco() {
			if (showMsg(LM.confirmDeleteListType(m_listtype),
				C_MSG_TITLE_EXCLAMATION, mdl.MessageBoxButtons.YesNo,
                mdl.MessageBoxIcon.Question) != mdl.DialogResult.Yes)
				return ;

			try {
				//riga master da eliminare
				var row = DS.customview.Rows[0];
				//Applico l'OndeDeleteCascade
				RowChange.ApplyCascadeDelete(row);
				if (eseguiPostData(DS)) {
					cboList.Enabled=false;
					fillComboList();
					cboList.Enabled=true;

					if (cboList.Items.Count>0){
						cboList.SelectedIndex=0;
					}

				}
			}
			catch (Exception e) {
				shower.ShowException(this,LM.errorDeletingListType(m_listtype), e);
				return;
			}
		}

		#endregion Elimina elenco

		#region Eventi Form

		private void formCustomViewList_Closed(object sender, EventArgs e) {
			running = false;
			if (m_linked==null) return;
            controller?.UnlinkListForm();
            Destroy();
		}

		
		#endregion Eventi Form

		#region Gestione Nascondi Grouped Columns

		//void resetGestioneGroupedColumns(){
		//	ColumnsToRestore=null;
		//}

		string[] getGroupedColumns(){
			var result = new ArrayList();
			var Groups= gridX.Groups;
			while (Groups.Count>0){
				result.Add(Groups[0].GroupBy);
				Groups = Groups[0].Groups;
			}
			string []res= new string[result.Count];
			for (int i=0; i< result.Count; i++) res[i]= (string) result[i];
			return res;
		}

		Hashtable ColumnsToRestore;
		/// <summary>
		/// nasconde le colonne presenti nel group-by
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void gridX_GroupingUpdated(object sender, EventArgs e) {
            if (destroyed) return;
            if (ColumnsToRestore==null) ColumnsToRestore=new Hashtable();		

			//fa riapparire le colonne che erano grouped e ora non lo sono più
			string []grouped= getGroupedColumns();
			var ToDel= new ArrayList();
			foreach (string field in ColumnsToRestore.Keys){
				if (ColumnsToRestore[field]==null) continue;
				bool found=false;
				foreach (string f in grouped){
					if (f == field) found=true;
				}
				if (!found) {
					gridX.Columns[field].Visible=true;
					ToDel.Add(field);
				}
			}
			foreach (string fieldtodel in ToDel) {
				ColumnsToRestore[fieldtodel]=null;
			}


			foreach (string fieldtohide in grouped){
				if (ColumnsToRestore[fieldtohide]!=null) continue;
				var C = gridX.Columns[fieldtohide];
				if (C.Visible) {
					C.Visible=false;				
					ColumnsToRestore[fieldtohide]="1";
				}
			}
			aggiornaSommatorie();

		}

		void aggiornaSommatorie(){
			azzeraFooters();
			if (!chkSommatorie.Checked)return;
			calcMainInsertionRow();
			addFooterToGroups(gridX.Groups);
		}

		void azzeraFooters(){
			gridX.FooterRows.Clear();
			azzeraFooters(gridX.Groups);
		}
		void azzeraFooters(Xceed.Grid.Collections.ReadOnlyGroupList GL){
			foreach (Xceed.Grid.Group G in GL){
                try {
                    G.FooterRows.Clear();
                }
                catch { }
				azzeraFooters(G.Groups);
			}
		}


		void addFooterToGroups(Xceed.Grid.Collections.ReadOnlyGroupList GL){
			foreach (Xceed.Grid.Group G in GL){
				if (chkOttimizzaSomme.Checked && G.GetSortedDataRows(true).Count<=1) continue;
				//footer = ;
                var IR = new ValueRow {BackColor = Color.GreenYellow};
                G.FooterRows.Add(IR);
				calcInsertionRow(G, IR);
				addFooterToGroups(G.Groups);
			}

		}

		bool isToSum(Column C){
			string field = C.Title;
			foreach (object cb in lbsumfield.CheckedItems){
				string h = cb.ToString();
				if (h==field) return true;
				//if (cb.Text== field) return true;
			}
			return false;
		}
		void calcInsertionRow(Group G, ValueRow IR){
			calcolaSomme(IR, G.GetSortedDataRows(true));
		}
		void calcolaSomme(ValueRow IR, 
					Xceed.Grid.Collections.ReadOnlyDataRowList GR){
			foreach(Column C in gridX.Columns){
				if (!isToSum(C)) continue;
				if (!C.Visible) continue;
				if (C.DataType == typeof(decimal)) {
					decimal sumd=0;
					foreach (Xceed.Grid.DataRow R in GR ){
						if (!typeof (CellRow).IsAssignableFrom(R.GetType()))continue;
						var CR = (CellRow) R;
						if (CR.Cells[C.FieldName].Value == null) continue;
						if (CR.Cells[C.FieldName].Value == DBNull.Value) continue;
						try {
							sumd+= Decimal.Round(Convert.ToDecimal(CR.Cells[C.FieldName].Value),5);
						}
						catch {
						}
					}
					IR.Cells[C.FieldName].Value = sumd;
					//IR.Cells[C.FieldName].Value= sumd;
				}

				if ((C.DataType == typeof(double)||(C.DataType == typeof(float)))) {
					double sumdo=0;
					foreach (Xceed.Grid.DataRow R in GR ){
						if (!typeof (Xceed.Grid.CellRow).IsAssignableFrom(R.GetType()))continue;
						Xceed.Grid.CellRow CR = (Xceed.Grid.CellRow) R;
						if (CR.Cells[C.FieldName].Value == null) continue;
						if (CR.Cells[C.FieldName].Value == DBNull.Value) continue;
						try {
							sumdo+= Convert.ToDouble(CR.Cells[C.FieldName].Value);
						}
						catch {
						}
					}
					IR.Cells[C.FieldName].Value = sumdo;
				}

				if ((C.DataType == typeof(int))||(C.DataType == typeof(System.Int16))) {
					double sumi=0;
					foreach (Xceed.Grid.DataRow R in GR ){
						if (!typeof (Xceed.Grid.CellRow).IsAssignableFrom(R.GetType()))continue;
						Xceed.Grid.CellRow CR = (Xceed.Grid.CellRow) R;
						if (CR.Cells[C.FieldName].Value == null) continue;
						if (CR.Cells[C.FieldName].Value == DBNull.Value) continue;
						try {
							sumi+= Convert.ToInt32(CR.Cells[C.FieldName].Value);
						}
						catch {
						}
					}
					IR.Cells[C.FieldName].Value = sumi;
				}
			}


		}

		void calcMainInsertionRow(){
            var IR = new ValueRow {
                BackColor = Color.Green
            };
            gridX.FooterRows.Add(IR);
			calcolaSomme(IR, gridX.GetSortedDataRows(true));
		}

		#endregion


		#region class myMenuItem : MenuItem
	
		//Derivo semplicemente per aggiungere la proprietà tag
		class MyMenuItem : MenuItem {
			//public string Tag;
			public MyMenuItem(string Text): base(Text) {
			}
		}

		#endregion class myMenuItem : MenuItem

		string last_filter_applied="??";
        string last_sort_applied = "??";
        string last_columnlist = "??";

      
        string getTop() {
            if(comboTOP.Text != "") {
                try {
                    var NUM = Convert.ToInt32(HelpUi.GetObjectFromString(typeof(int), comboTOP.Text, "x.y"));
                    if (NUM >= 0) {
                        return  NUM.ToString();
                    }
                }
                catch {
                }
            }
            return null;

        }

        private string mainTableName;

        private string filtroSuVistaApplicato = null;
        private IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();
        private string lastColTable = "??";
		void leggiTabellaElenco(IWinFormMetaData linked,
				string columnlist,
				string tablename,
				string filter,
                string orderby,
				DataTable ToMerge
			){
            this.mainTableName=tablename;
            var controller = linked.controller;

            var QHS = conn.GetQueryHelper();
			if ((ToMerge==null)||(ToMerge.Rows.Count==0)){
                string filtersec = filter;


				if (DSDati==null || lastColTable!=columnlist) {
				    lastColTable = columnlist;
                    DSDati = new DataSet();
				    ClearDataSet.RemoveConstraints(DSDati);
                    filtersec = QHS.AppAnd(filtersec, controller.security.SelectCondition(tablename, true));

				    filtroSuVistaApplicato = filtersec;

                    //if (DataAccess.LocalToDB){ //linked.Conn
                    //    DT = linked.Conn.RUN_SELECT(tablename, columnlist, orderby,
                    //        filter, GetTop(), null, true);
                    //}
                    //else {
                        DT = conn.RUN_SELECT( tablename, columnlist, orderby,	
							filtersec, getTop() , null,(filter==filtersec));
                    //}
					DSDati.Tables.Add(DT);
				}
				else {
                    if (!model.isSkipSecurity(DT)) filtersec = QHS.AppAnd(filtersec,
                                controller.security.SelectCondition(tablename, true));

					update_enabled=false;
					model.clear(DT);
					update_enabled=true;
                    filtroSuVistaApplicato = filtersec;
                    conn.RUN_SELECT_INTO_TABLE( DT, orderby, filtersec, getTop(), (filter == filtersec));
				}

				//Elimina le righe non selezionabili
				int testcanselect2 = metaprofiler.StartTimer("Removing not selectable");
				//linked.Conn.DeleteAllUnselectable(DT);
//				foreach (System.Data.DataRow RR  in DT.Select()){
//					if (!linked.Conn.CanSelect(RR)){
//						RR.Delete();
//						RR.AcceptChanges();
//					}
//				}
				metaprofiler.StopTimer(testcanselect2);
				return;
			}
			DSDati = new DataSet("elenco");
		    ClearDataSet.RemoveConstraints(DSDati);
			DT = conn.CreateTableByName(tablename,columnlist);
			if (DT.Columns.Count==0){
				shower.Show(this,"Non sono disponibile le informazioni relative alle colonne di "+
					tablename + ". Questo può accadere a causa di una erronea installazione. "+
					"Ad esempio, non è stato eseguito AnalizzaStruttura.","Errore");
			}

			foreach(DataColumn C in DT.Columns){
				C.AllowDBNull=true;//necessary cause not all fields are read from DB, infact merge can be
				// done from a table to a view-table or viceversa.
			}

            string filtersec2 = filter;
            if (!model.isSkipSecurity(DT)) {
                    filtersec2 = QHS.AppAnd(filtersec2,
                                controller.security.SelectCondition(DataAccess.GetTableForReading(DT),true));
            }
            filtroSuVistaApplicato = filtersec2;
            conn.RUN_SELECT_INTO_TABLE(DT, orderby, filtersec2, getTop(), (filtersec2 == filter));
			DSDati.Tables.Add(DT);


			//Elimina le righe non selezionabili
			//int testcanselect = metaprofiler.StartTimer("Removing not selectable");
			//linked.Conn.DeleteAllUnselectable(DT);
//			foreach (System.Data.DataRow RR  in DT.Select()){
//				if (!linked.Conn.CanSelect(RR)){
//					RR.Delete();
//					RR.AcceptChanges();
//				}
//			}
			//metaprofiler.StopTimer(testcanselect);

			string nochildfilter = ToMerge.ExtendedProperties["NotEntityChild"].ToString();

			//Delete from list those who have not the filter property in the ToMerge Table
			System.Data.DataRow [] ToExclude = ToMerge.Select("NOT("+nochildfilter+")");
			foreach (var R in ToExclude){
				string cond = QueryCreator.WHERE_REL_CLAUSE(R, ToMerge.PrimaryKey,
					ToMerge.PrimaryKey, DataRowVersion.Default,false);                    
				System.Data.DataRow[] ToDelete = DT.Select(cond);
				if (ToDelete.Length>0) {
					ToDelete[0].Delete();
					ToDelete[0].AcceptChanges();
				}
			}

			//Add to list those who are not present in the list and are present in the ToMerge
			// table
			//string mergedfilter= GetData.MergeFilters(nochildfilter,filter);
			System.Data.DataRow [] ToAdd = ToMerge.Select(nochildfilter);  //was nochildfilter ( 13/1/2005)
			foreach (var R in ToAdd){
				//string cond = QueryCreator.WHERE_REL_CLAUSE(R, ToMerge.PrimaryKey,ToMerge.PrimaryKey, DataRowVersion.Default,false);                    
				System.Data.DataRow[] ToInsert = DT._Filter(q.mCmp(R,ToMerge.PrimaryKey));

				//Removes eventually present row from DT
				foreach (var RR in ToInsert){
					RR.Delete();
					RR.AcceptChanges();
				}
				var NewRow = DT.NewRow();
				foreach(DataColumn C in ToMerge.Columns){
					if(DT.Columns.Contains(C.ColumnName)){
						NewRow[C.ColumnName] = R[C.ColumnName, DataRowVersion.Original];
					}
				}
				DT.Rows.Add(NewRow);
			}


		}


        string last_top = null;
		/// <summary>
		/// LEGGE DSDati ed effettua BINDING e ORDINAMENTO
		/// </summary>
		/// <param name="linked"></param>
		/// <param name="filter"></param>
		/// <param name="tablename"></param>
		/// <param name="columnlist"></param>
		/// <param name="orderby"></param>
		/// <param name="ToMerge"></param>
		void leggiElenco(
				IWinFormMetaData linked,
				string filter, 
				string tablename, 
				string columnlist,
				string orderby,
				DataTable ToMerge){
	
			if (filter=="") filter=null;
			if (orderby=="") orderby=null;
			gridX.BeginInit();
            string new_top = getTop();

			//gridX.SuspendLayout();
			if ((last_filter_applied != filter) || (new_top!=last_top) || (last_sort_applied!=orderby) || (last_columnlist!=columnlist)){
                last_sort_applied = orderby;
                last_filter_applied = filter;
			    last_columnlist = columnlist;
                last_top = new_top;
                int leggitabella = metaprofiler.StartTimer("LeggiTabellaElenco");
			    leggiTabellaElenco(linked, columnlist, tablename, filter,orderby, ToMerge); //era filterlocked ? "*":columnlist
                metaprofiler.StopTimer(leggitabella);

				int setdatabind = metaprofiler.StartTimer("Set DataBinding");
				//il binding dei dati alla grid è effettuato (solo) qui
				try {
					gridX.SetDataBinding(DSDati, DT.TableName);
					
				}
				catch {}

				foreach (Cell cell in gridX.DataRowTemplate.Cells) {
					//cell.Click += new EventHandler(this.GridElenchiCell_Click);
					cell.DoubleClick += gridElenchiCell_DoubleClick;
				}
				metaprofiler.StopTimer(setdatabind);
				
				 //necessaria ad ogni databinding
//				int setdataeve = metaprofiler.StartTimer("Set XceedGrid Events");
				addEventsToGridElenchi();
//				metaprofiler.StopTimer(setdataeve);


				//Azzera il flag "fitted" di tutte le colonne
				clearFittedFlag();

			}

			int setsort= metaprofiler.StartTimer("Sorting Xceed grid...");
			if (gridX.SortedColumns.Count>0) gridX.SortedColumns.Clear();
			if (orderby!=null){
				string []orderclauses = orderby.Split(new char[] {','});
				var fields = new Hashtable(orderclauses.Length);
				int nfields=0;
				foreach (string orderc in orderclauses){
					string sortclause = orderc.Trim();
					if (sortclause=="") continue;
					string [] sortcomp = orderc.Split(new char[] {' '});
					if (sortcomp.Length==0) continue;
					string fieldname = sortcomp[0].Trim();
					string sorttype="ASC";
					if (sortcomp.Length>1){
						sorttype= sortcomp[1].ToUpper();
					}
					if ((sorttype!="ASC")&&(sorttype!="DESC"))continue;
					if (fields[fieldname]!=null){
						int fieldnum = (int) fields[fieldname];
						if (sorttype=="ASC")
							gridX.SortedColumns[fieldnum].SortDirection = SortDirection.Ascending;
						else
							gridX.SortedColumns[fieldnum].SortDirection = SortDirection.Descending;
					}
					else {
                        if (DT.Columns.Contains(fieldname)) {
                            try {
                                gridX.SortedColumns.Add(fieldname, (sorttype == "ASC"));
                                fields[fieldname] = nfields;
                                nfields++;
                            }
                            catch {
                            }
                        }
					}
					
				}
			}
			metaprofiler.StopTimer(setsort);
            GOTOPilotato = true;

			gridX.EndInit();
            GOTOPilotato = false;

            this.Text = "Elenco " + linked.Name + " (" + DT.Rows.Count + " righe)";

			//gridX.ResumeLayout();
		}

		/// <summary>
		/// Riempie il DataSet (DS) delle impostazioni dell'elenco
		/// </summary>
		/// <param name="list_type">nome dell'elenco da caricare</param>
		private void leggiImpostazioniListType(string list_type) {

			gridCol.BeginInit();
			ClearDataSet.RemoveConstraints(DS);
			int leggiImpostaDSClear = metaprofiler.StartTimer("leggiImpostaDSClear");
			DS.Clear();
			metaprofiler.StopTimer(leggiImpostaDSClear);


			GetData MyGetData = new GetData();
			MyGetData.InitClass(DS, conn, C_TAB_CUSTOMVIEW);
			System.Data.DataRow DR = DS.customview.NewRow();
			
			DR["objectname"] = m_tablename;
			DR["viewname"] = list_type;
			
			MyGetData.SEARCH_BY_KEY(DR);

			GetData.CacheTable(DS.customoperator);
			GetData.CacheTable(DS.connector);

			MyGetData.DO_GET(false,null);
			MyGetData.ReadCached();
            MyGetData.Destroy();

			loadCustomDirection();
			gridCol.EndInit();
		}

        /// <summary>
		/// N. of available connector
		/// </summary>
		public const int n_connector = 2;

		/// <summary>
		/// AND connector code
		/// </summary>
		public const  int AND_CONNECTOR=0;

		/// <summary>
		/// OR connector code
		/// </summary>
		public const  int OR_CONNECTOR=1;

		/// <summary>
		/// "AND with base filter" connector code
		/// </summary>
		public const int PREV_CONNECTOR=-1;
		
		/// <summary>
		/// Gives Connector name given its code
		/// </summary>
		/// <param name="_connector"></param>
		/// <returns></returns>
		public static string DescribeConnector(int _connector){
			switch(_connector){
				case AND_CONNECTOR:return "AND";
				case OR_CONNECTOR:return "OR";
				default:return "(PREVIOUS) AND ";
			}
			
		}

        		   

        /// <summary>
        /// N. of operators available
        /// </summary>
        public const int n_operators = 14;

        const int op_eq  =0;
        const int op_lt  =1;
        const int op_le  =2;
        const int op_lk  =3;
        const int op_in  =4;
        const int op_btw =5;
        const int op_nul =6;
        const int op_ne  =7;
        const int op_gt  =8;
        const int op_ge  =9;
        const int op_nlk =10;
        const int op_notin =11;
        const int op_notbtw=12;
        const int op_notnul=13;
        const int op_nulloreq = 14;
        const int op_nullorgt = 15;
        const int op_nullorge = 16;
        const int op_nullorlt = 17;
        const int op_nullorle = 18;
        const int op_nullorne = 19;
        const int op_nullorlike = 20;


        /// <summary>
        /// Gets n. of operands of operator.
        /// </summary>
        /// <param name="_operator"></param>
        /// <returns>-1 if variable number (one or more)</returns>
        public static int CountOperands(int _operator){
            switch(_operator){
                case op_eq:return 1;
                case op_lt:return 1;
                case op_le:return 1;
                case op_lk:return 1;
                case op_in:return -1;
                case op_btw:return 2;
                case op_nul:return 0;
                case op_ne:return 1;
                case op_gt:return 1;
                case op_ge:return 1;
                case op_nlk:return 1;
                case op_notin:return -1;
                case op_notbtw:return 2;
                case op_notnul:return 0;
                case op_nulloreq:return 1;
                case op_nullorgt: return 1;
                case op_nullorge: return 1;
                case op_nullorlt: return 1;
                case op_nullorle: return 1;
                case op_nullorne: return 1;
                case op_nullorlike: return 1;
            }
            return 0; //never reached
        }

         /// <summary>
        /// Returns true if params got, false if clause was skipped
        /// </summary>
        /// <param name="result">parameters got</param>
        /// <param name="campo">field name for which parameters are read</param>
        /// <param name="caption"></param>
        /// <param name="operatore">Operator code that will be used with parameters</param>
        /// <returns>true when successfull</returns>
        public static bool GetRunTimeParams(out string [] result, 
            string campo, string caption,
            int operatore){			
            int countoperands = CountOperands(operatore);
            if (countoperands==0){
                result = new string[0];
                return true;
            }
            AskParameter F = new AskParameter(caption, DescribeOperator(operatore),countoperands);
            MetaFactory.factory.getSingleton<IFormCreationListener>().create(F, null);
            var res = F.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.Cancel) {
                result = new string[0];
                return false;
            }
            if (GetParams(out result, F.val, countoperands)) return true;
            return false;
        }

        /// <summary>
        /// Get operator description given its code
        /// </summary>
        /// <param name="_operator"></param>
        /// <returns></returns>
        public static string DescribeOperator(int _operator){
            switch(_operator){
                case op_eq:return "è uguale a";
                case op_lt:return "è minore di";
                case op_le:return "è minore o uguale a";
                case op_lk:return "è simile a";
                case op_in:return "appartiene a";
                case op_btw:return "è incluso in";
                case op_nul:return "è nullo";
                case op_ne:return "è diverso da";
                case op_gt:return "è maggiore di";
                case op_ge:return "è maggiore o uguale a";
                case op_nlk:return "non è simile a";
                case op_notin:return "non appartiene a";
                case op_notbtw:return "non è incluso in";
                case op_notnul:return "non è nullo";
                case op_nulloreq: return "nullo o uguale a";
                case op_nullorgt: return "nullo o maggiore di";
                case op_nullorge: return "nullo o maggiore uguale a";
                case op_nullorlt: return "nullo o minore di";
                case op_nullorle: return "nullo o minore uguale a";
                case op_nullorne: return "nullo o diverso da";
                case op_nullorlike: return "nullo o simile a";
            }
            //never reached
            return null;
        }

	    /// <summary>
	    /// Evaluates a single where clause, eventually asking parameters at run time
	    ///  if CanAskAtRuntime is true and Clause["runtime"]="1"
	    /// </summary>
	    /// <param name="security"></param>
	    /// <param name="clause">customviewwhere row</param>
	    /// <param name="T"></param>
	    /// <param name="canAskAtRunTime"></param>
	    /// <returns>compiled where clause</returns>
	    static string compileOneClause(ISecurity security, System.Data.DataRow clause, DataTable T, bool canAskAtRunTime){			
			string [] result;
			int oper = Convert.ToInt32(clause["operator"]);
			int nparams = CountOperands(oper);

			if (clause["runtime"].ToString().ToLower()!="1"){
				string values = security.Compile(clause["value"].ToString(),true);				
				bool done = GetParams(out result, values,  nparams);
				if (!done) return null;
			}
			else {
				if (canAskAtRunTime){
					bool done = GetRunTimeParams(out result, 
						clause["columnname"].ToString(), T.Columns[clause["columnname"].ToString()].Caption, oper);
					if (!done) return null;
				}
				else {
					result = new string[3] {"<<Ask>>","<<Ask>>","<<Ask...>>"};
				}
			}
            // result è un array di stringhe che poi viene messo nella query con un quotedstrvalue( , true)
            // dobbiamo invece convertirlo in un array di oggetti in base al tipo dei vari parametri
            object[] ORes = new object[result.Length];
	        for (int i = 0; i < result.Length; i++) {
                    
	            ORes[i] = HelpUi.GetObjectFromString(T.Columns[clause["columnname"].ToString()].DataType, result[i],"x.y");
	        }

            //if (T.Columns[clause["columnname"].ToString()].DataType == typeof(DateTime)) {
            //    for (int i = 0; i < result.Length; i++) {
                    
            //        ORes[i] = HelpForm.GetObjectFromString(typeof(DateTime), result[i],"x.y");
            //    }
            //}
            //else {
            //    for (int i = 0; i < result.Length; i++) {
            //        ORes[i] = result[i];
            //    }
            //}

            string expr = GetSqlClause(clause["columnname"].ToString(),oper, ORes);
			return "("+expr+")";

		}
        	/// <summary>
		/// Gets a single where clause
		/// </summary>
        /// <param name="fieldname"></param>
		/// <param name="_operator">operator code</param>
		/// <param name="operands">array of quoted operands</param>
		/// <returns></returns>
		public static string GetSqlClause(string fieldname,int _operator, object [] operands){
			string mask="";
			switch(_operator){
				case op_eq:mask= "%fieldname=%s1";break;
				case op_lt:mask= "%fieldname<%s1"; break;
				case op_le:mask= "%fieldname<=%s1"; break;
				case op_lk:mask= "%fieldname LIKE %s1"; break;
				case op_in:mask= null;break;
				case op_btw:mask= "%fieldname BETWEEN %s1 AND %s2726541312"; break;
				case op_nul:mask= "%fieldname IS NULL "; break;
				case op_ne:mask= "%fieldname <> %s1"; break;
				case op_gt:mask= "%fieldname > %s1"; break;
				case op_ge:mask= "%fieldname>= %s1"; break;
				case op_nlk:mask= "%fieldname NOT LIKE %s1"; break;
				case op_notin:mask= null;break;
				case op_notbtw:mask= "%fieldname NOT BETWEEN %s1 AND %s2726541312"; break;
				case op_notnul:mask= "%fieldname IS NOT NULL"; break;
                case op_nulloreq: mask = "%fieldname is null or  %fieldname = %s1"; break;
                case op_nullorgt: mask = "%fieldname is null or  %fieldname > %s1"; break;
                case op_nullorge: mask = "%fieldname is null or  %fieldname >= %s1"; break;
                case op_nullorlt: mask = "%fieldname is null or  %fieldname < %s1"; break;
                case op_nullorle: mask = "%fieldname is null or  %fieldname <= %s1"; break;
                case op_nullorne: mask = "%fieldname is null or  %fieldname <> %s1"; break;
                case op_nullorlike: mask = "%fieldname is null or  %fieldname like %s1"; break;
            }
			if (mask!=null){
				int n_op = CountOperands(_operator);

                mask = mask.Replace("%fieldname", fieldname);

                switch (n_op){
					case 0: return mask;
					case 1: return mask.Replace("%s1", mdl_utils.Quoting.quotedstrvalue(operands[0],true));
					case 2: mask = mask.Replace("%s1", mdl_utils.Quoting.quotedstrvalue(operands[0],true));
						mask = mask.Replace("%s2726541312", mdl_utils.Quoting.quotedstrvalue(operands[1],true));
						return mask;
				}
				//code never reached
				return null;
			}

			switch (_operator){
				case op_in:    mask= fieldname+" IN ("; break;
				case op_notin: mask= fieldname + " NOT IN(";break;
			}
			bool first=true;
			foreach (object s in operands){
				if (first)
					first=false;
				else
					mask += ",";
				mask += mdl_utils.Quoting.quotedstrvalue(s,true);
			}
			mask += ")";
			return mask;
		}


		/// <summary>
		/// Splits val in nparams substrings, returns true when successfull
		/// </summary>
		/// <param name="result">result parameter array</param>
		/// <param name="val">string containing parameters separated by semicolon (;)</param>
		/// <param name="nparams">N. of parameter to get</param>
		/// <returns>true when successfull</returns>
		public static bool GetParams(out string [] result, string val, int nparams){
			if (nparams==0) {
				result = new string[0];
				return true;
			}
			if (nparams!=-1){
				result = val.Split(new char[]{';'}, nparams);
				if (result.Length != nparams) return false;
			}
			else{
				result = val.Split(new char[]{';'});
				if (result.Length==0) return false;
			}						
			return true;
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="security"></param>
        /// <param name="T"></param>
        /// <param name="basefilter"></param>
        /// <param name="whereClauses"></param>
        /// <param name="canAskAtRunTime"></param>
        /// <returns></returns>
	    public static string GetFilterFromCustomViewWhere(ISecurity security, DataTable T,
	        string basefilter,
	        System.Data.DataRow[] whereClauses,
	        bool canAskAtRunTime
	    ) {
	        string where = "";
	        bool first = true;
	        foreach (var Clause in whereClauses) {
	            int connector = Convert.ToInt32(Clause["connector"]);
	            string connstring = "";
	            if (first && (connector == PREV_CONNECTOR)) {
	                where = basefilter;
	                connector = 0;
	            }
	            if (!first) {
	                switch (connector) {
	                    case AND_CONNECTOR:
	                        connstring = "AND";
	                        break;
	                    case OR_CONNECTOR:
	                        connstring = "OR";
	                        break;
	                }
	            }
	            string compiledclause = compileOneClause(security, Clause, T, canAskAtRunTime);
	            if (compiledclause != null) {
	                where = where + connstring + compiledclause;
	                first = false;
	            }

	        }
	        return where;
        }

		/// <summary>
		/// Costruisce la where condition  (merge di Impostazioni e filtro ingresso)
		/// Input: Filtro iniziale, DS, checkbox "usa"
		/// </summary>
        /// param name="T" usato per le caption dei parametri a run time
		private string ottieniFiltro(DataTable T) {
            string mybasefilter;
            try {
				var Base = DS.customview.Rows[0];
				mybasefilter = Base["staticfilter"].ToString().Trim();
			}
			catch {}


            string m_filter;
            try {
                System.Data.DataRow[] rows = DS.customviewwhere.Select();
                m_filter = GetFilterFromCustomViewWhere(controller.security, T,null, rows, true);
            }
            catch(Exception e) {
                showMsg($"Errore nella costruzione del filtro\r\rDettaglio: {e.Message}",
                    C_MSG_TITLE_ERRORE, mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Error);
                m_filter = "";
            }

            if (m_filter!="") m_filter="("+m_filter+")";
			string filtrocomplessivo= m_filter;

			mybasefilter = m_basefilter;

			if (m_IAmAdmin && (!filterlocked))
				mybasefilter = txtBaseFilter.Text.Trim();

			if ((mybasefilter!="")&&(mybasefilter!=null)) {
				mybasefilter="("+mybasefilter+")";
				if (chkBaseFilter.Checked) {
					filtrocomplessivo= GetData.MergeFilters(m_filter,mybasefilter);
				}
			}

			if (filtrocomplessivo == "")
				filtrocomplessivo = null;

            string statfilter = m_linkedview.GetStaticFilter(m_listtype);
            filtrocomplessivo = GetData.MergeFilters(statfilter, filtrocomplessivo);

			return filtrocomplessivo;
		}

        /// <summary>
        /// Imposta le proprietà (ordine colonne, captio, visibilità) del grid dei dati in base a customviewcolumn
        /// </summary>
        /// <remarks>Il binding dei dati deve essere già stato fatto.
        /// </remarks>
        private void applicaImpostazioniListType() {
			int appImp = metaprofiler.StartTimer("ApplicaImpostazioniListType");
			int autoindex=0;
			gridX.SuspendLayout();
			foreach (var row in DS.customviewcolumn.Select(null,"listcolpos asc")) {
				string colname = row["colname"].ToString();
				if (gridX.Columns[colname]==null) continue;

				//colname può essere null su elenchi pre-esistenti
				if (colname == "")	continue; 

				if (row["visible"].ToString() != "1") {
					gridX.Columns[colname].Visible = false;
					continue;
				}
				autoindex++;

				//imposto a true il visible 
				gridX.Columns[colname].Visible = true;
				gridX.Columns[colname].Title = row["heading"].ToString();

				//ricavo fontname, fontsize, bold, italic, underlined, strikeout
				gridX.Columns[colname].Font = getFont(row);

				gridX.Columns[colname].FormatSpecifier = 
					tagUtils.GetFormatForColumn(DSDati.Tables[m_tablename].Columns[colname]);

				//autosize della colonna  ---> spostato in fase di lettura dei dati
				//gridX.Columns[colname].Width = gridX.Columns[colname].GetFittedWidth();

				//ordine di visualizzazione delle colonne
				if (row["listcolpos"]==DBNull.Value){
					gridX.Columns[colname].VisibleIndex = autoindex;
				}
				else {
					gridX.Columns[colname].VisibleIndex = Convert.ToInt32(row["listcolpos"]);
				}
			}
            foreach (Column CC in gridX.Columns) {
                if (DS.customviewcolumn.Select("colname="+ mdl_utils.Quoting.quotedstrvalue(CC.FieldName,false)).Length==0)
                    CC.Visible=false;
            }
			gridX.ResumeLayout();
			metaprofiler.StopTimer(appImp);
		}

        /// <summary>
        /// Applica il DescribeColumns del meta al fine di impostare il campo visibile di customviewcolumn a 0 per i campi nascosti
        /// </summary>
        /// <param name="listingtype"></param>
        /// <param name="issystem"></param>
        private void applicaImpostazioniDaCodice_pre(string listingtype, bool issystem) {
            //Se il listtype non è di sistema non eseguo la DescribeColumns
            if (!issystem) return;
            
            var DT = conn.CreateTableByName(m_tablename,"*");
            var d = new DataSet();
            d.Tables.Add(DT);
            //il metadato ha impatto solo sulla caption e/o visibilità
            m_metaData = dispatcher.Get(m_tablename);
            m_metaData.DescribeColumns(DT, listingtype);
            m_metaData.Destroy();

            foreach (DataColumn col in DT.Columns) {               
                //string filter = "colname = '" + col.ColumnName + "'";
                System.Data.DataRow[] rows = DS.customviewcolumn._Filter(q.eq("colname",col.ColumnName));
                if (rows.Length == 0) {
                    continue;
                }

                if ((col.Caption == "") || (col.Caption.StartsWith("."))) {
                    rows[0]["visible"] = 0;
                    continue;
                }               
            }
        }

        /// <summary>
        /// Restituisce la caption senza l'eventuale punto davanti
        /// </summary>
        /// <param name="caption"></param>
        /// <returns></returns>
        string getCaptionNoDot(string caption) {
            if (caption == "") return caption;
            if (caption.StartsWith(".")) return caption.Substring(1);
            return caption;
        }
        /// <summary>
        /// Imposta caption e visibilità delle colonne dei dati in base alle impostazioni da codice, solo per 
        ///  gli elenchi di sistema.  Inoltre nasconde colonne senza dati e colonna senza impostazioni nel dataset
        /// </summary>
        private void applicaImpostazioniDaCodice_post(string listingtype, bool issystem) {
			
			//Se il listtype non è di sistema non eseguo la DescribeColumns
			if (!issystem) return;

			//il metadato ha impatto solo sulla caption e/o visibilità
			m_metaData = dispatcher.Get(m_tablename);
			m_metaData.DescribeColumns(DT, listingtype);
            m_metaData.Destroy();

			foreach (DataColumn col in DT.Columns) {
				bool allnull=true;

                //nasconde le colonne che non contengono dati
				foreach(System.Data.DataRow RR in DT.Rows){
					if (RR[col.ColumnName].ToString().Trim()!=""){
						allnull=false;
						break;
					}
				}
                if (DT.Rows.Count == 0) allnull = false;
				if (allnull){
                    gridX.Columns[col.ColumnName].Title = getCaptionNoDot(col.Caption);
                    gridX.Columns[col.ColumnName].FormatSpecifier = tagUtils.GetFormatForColumn(col);
                    gridX.Columns[col.ColumnName].Visible = false;  //nasconde la colonna dal grid
					continue;

				}

                //Vede se ci sono impostazioni nel ds sulla colonna
				System.Data.DataRow[] rows = DS.customviewcolumn._Filter(q.eq("colname",col.ColumnName));
				if (rows.Length == 0) {
					//il dataset non contiene la riga per quel columnname (es. campi desc. foreign key) , la colonna è nascosta
					gridX.Columns[col.ColumnName].Title = getCaptionNoDot(col.Caption);
                    gridX.Columns[col.ColumnName].FormatSpecifier = tagUtils.GetFormatForColumn(col);
                    gridX.Columns[col.ColumnName].Visible = false;
					continue;
				}

                //La colonna ha caption vuota o inizia col punto, la colonna è nascosta
				if ((col.Caption == "") || (col.Caption.StartsWith("."))) {
                    gridX.Columns[col.ColumnName].Title = getCaptionNoDot(col.Caption);
                    gridX.Columns[col.ColumnName].FormatSpecifier = tagUtils.GetFormatForColumn(col);
                    gridX.Columns[col.ColumnName].Visible = false;
					rows[0]["visible"] = 0;
					continue;
				}
				//imposto a true il visible (altrimenti rimarrebbe a false nel caso
				//una colonna fosse stata nascosta)
				gridX.Columns[col.ColumnName].Visible = true;
				gridX.Columns[col.ColumnName].Title = col.Caption;
				gridX.Columns[col.ColumnName].FormatSpecifier = tagUtils.GetFormatForColumn(col);
			}
		}

		/// <summary>
		/// Aggiorna il flag m_listtypeissystem, i bottoni ed il menu contestuale
		/// </summary>
		/// <param name="listtype"></param>
		void updateSysListType(string listtype){
			System.Data.DataRow []CurrList = DS.customview._Filter(q.eq("viewname",listtype));
			m_listtypeissystem = ((CurrList.Length>0)&& (CurrList[0]["issystem"].ToString()!="N"));
			enableDisableButtons(m_listtypeissystem);
			updateContextMenu(m_listtypeissystem);
		}

		void refitGridColonne(){
            foreach (Column C in gridCol.Columns){
                C.Width= C.GetFittedWidth();
			}
		}
		/// <summary>
		/// Legge il listing type e ne crea uno dummy di sistema se non esiste
		/// Aggiorna il menu contestuale ed i bottoni
		/// Output = DS
		/// </summary>
		/// <param name="listtype"></param>
		void leggiListingType(string listtype){
			int leggilis = metaprofiler.StartTimer("LeggiListingType");
			Cursor = Cursors.WaitCursor;
            chkBaseSorting.Checked = false;
			leggiImpostazioniListType(listtype);
			
			//Se il listtype non ha righe in customviewcolumn
			//le aggiungo con tutti i valori a null eccetto per il colname
			if (DS.customviewcolumn.Rows.Count < 1) {
				creaESalvaDummyListType(m_tablename, listtype);
			}
			refitGridColonne();

			updateSysListType(listtype);
            if (m_listtypeissystem) chkBaseSorting.Checked=true;
			DescribeColumnsApplied=false;
			Cursor  = Cursors.Default;
			metaprofiler.StopTimer(leggilis);
		}


		void clearFittedFlag(){
			foreach(Column C in gridX.Columns) {
			    if (!DT.Columns.Contains(C.FieldName)) continue;
				DT.Columns[C.FieldName].ExtendedProperties["fitted"]=null;
			}
		}


		int calculateWidthForColumn(Graphics g,  MyGridColumn myg, Column C){
			var MySizeF = (SizeF)myg.GetPrefSize(g, C.Title );
			int MaxColumnSize = Convert.ToInt32(MySizeF.Width);
				
			int nrighe = DT.Rows.Count;
			if (nrighe>100) nrighe=100;
			string colname = C.FieldName;
			//System.Data.DataColumn COL = DT.Columns[colname];
			var COL  = DSDati.Tables[m_tablename].Columns[colname];

			for(int i=0; i<nrighe; i++) {	//MyRowCount
				var FF = DT.Rows[i];
				var O = FF[colname];
				//Object O = rows[Ro][colname];
				if (O==null) continue;
				if (O.ToString()=="") continue;
				string S= HelpUi.StringValue(O,"x.y",COL);
					
				MySizeF = myg.GetPrefSize(g,S);
				int Result = Convert.ToInt32(MySizeF.Width);	//larghezza della cella corrente
				if(Result > MaxColumnSize) MaxColumnSize = Result;
			}
				
			return MaxColumnSize;

		}
		/// <summary>
		/// Effettua il refit di tutte le colonne visibili per cui non è 
		///  ancora stato effettuato
		/// </summary>
		void refitColumns(){
			var g = gridX.CreateGraphics();
			var GG = new DataGrid();
			var TS = new DataGridTableStyle();
			var myg= new MyGridColumn();
			TS.GridColumnStyles.Add(myg);
			GG.TableStyles.Add(TS);

			gridX.SuspendLayout();
			//Refit delle colonne
			foreach(Column C in gridX.Columns){
				if (!C.Visible) continue;
				//int ll= metaprofiler.StartTimer("Fit column "+C.Title);
				if (DT.Columns[C.FieldName].ExtendedProperties["fitted"]==null){
					C.Width = calculateWidthForColumn(g,myg,C);//C.GetFittedWidth();
				    //C.Width = C.GetFittedWidth();
					DT.Columns[C.FieldName].ExtendedProperties["fitted"]="1";
				}
				//metaprofiler.StopTimer(ll);
			}
			gridX.ResumeLayout();
		}

        string getColumnlist() {
            var Temp = conn.CreateTableByName(m_tablename, "*");
            DataColumn[] primarykey = Temp.PrimaryKey;

            if (primarykey.Length == 0 && m_linked.TableName != m_tablename) {
                if (m_linkedview.PrimaryKey() != null) {
                    primarykey = (from s in m_linkedview.PrimaryKey() select Temp.Columns[s]).ToArray();
                    //Temp.PrimaryKey = primarykey;                 potrebbe contenere null
                    foreach (string s in m_linkedview.PrimaryKey()) {
                        if (!Temp.Columns.Contains(s)) {
                            errorLogger.logException(
                                $"La vista {m_linkedview.TableName} non ha un campo di nome {s} presente invece nella chiave.",
                                meta:m_linked);                            
                        }
                    }
                }
            }


            if (primarykey.Length == 0 && m_linked.TableName != m_tablename) {
                var mainTable = conn.CreateTableByName(m_linked.TableName, "*");
                primarykey = mainTable.PrimaryKey;
            }
            if (primarykey.Length == 0) {
                if (m_linked.PrimaryKey() != null) {
                    primarykey = (from s in m_linked.PrimaryKey() select Temp.Columns[s]).ToArray();
                    //Temp.PrimaryKey = primarykey;    potrebbe contenere null
                    foreach (string s in m_linked.PrimaryKey()) {
                        if (!Temp.Columns.Contains(s)) {
                            errorLogger.logException(
                                $"La tabella {m_linked.TableName} non ha un campo di nome {s} presente invece nella chiave.",
                                meta:m_linked);
                        }
                    }
                }
            }

            var DTemp = new DataSet();
            DTemp.Tables.Add(Temp);
            var MTemp = dispatcher.Get(m_tablename);
            if (dispatcher.unrecoverableError) {
                controller.ErroreIrrecuperabile = true;
                shower.ShowError(controller?.linkedForm,
                    $"Errore nel caricamento del metadato {m_tablename} è necessario riavviare il programma.", "Errore");
            }

            MTemp.DescribeColumns(Temp, m_listtypeissystem ? m_listtype : (string.IsNullOrEmpty(MTemp.DefaultListType) ? "default" : MTemp.DefaultListType));
            MTemp.Destroy();

            if (!DescribeColumnsApplied) {
                int appcode = metaprofiler.StartTimer("ApplicaImpostazioniDaCodice_pre");//210
                applicaImpostazioniDaCodice_pre(m_listtype, m_listtypeissystem);
                metaprofiler.StopTimer(appcode);
            }

            string[] allfields = m_columnList.Split(',');
            string newCollist = "";
            foreach (string col in allfields) {
                string col2 = col.Trim();
                if (!Temp.Columns.Contains(col2)) continue;
                bool iskey = false;
                for (int i = 0; i < primarykey.Length; i++) {
                    if (primarykey[i].ColumnName == col2) {
                        iskey = true;
                        break;
                    }
                }

                if (!iskey) {
                    //if (Temp.Columns[col2].Caption == "") continue;
                    //if (Temp.Columns[col2].Caption.StartsWith(".")) continue;
                    //if (Temp.Columns[col2].Caption.StartsWith("!")) continue;
                    System.Data.DataRow[] cc = DS.customviewcolumn.Select($"colname=\'{col2}\'");
                    if (cc.Length == 0) continue;
                    if (cc[0]["visible"].ToString() != "1") {
                        bool grouped = false;
                        if (gridX != null) {
                            Xceed.Grid.Collections.ReadOnlyGroupList RGL = gridX.Groups;
                            while ((RGL != null) && (RGL.Count > 0)) {
                                Column cg = gridX.Columns[RGL[0].GroupBy];
                                if (cg.FieldName == col2) {
                                    grouped = true;
                                    break;
                                }
                                RGL = RGL[0].Groups;
                            }
                        }
                        if (!grouped) continue;


                    }

                }
                if (newCollist != "") newCollist += ",";
                newCollist += col2;
            }

            return newCollist;
        }
        /// <summary>
        /// Class for logging errors
        /// </summary>
        public IErrorLogger errorLogger { get; set; } = ErrorLogger.Logger;


        private DataTable sampleTable;
		/// <summary>
		/// Input = DS
		/// Legge i dati (se necessario) e li visualizza con il listtype corrente 
		/// Assume m_listtype e m_listtypeissystem già calcolati
		/// </summary>
		void applicaListType(){
			int applist = metaprofiler.StartTimer("Applica ListType");//18066  //9414
			Cursor= Cursors.WaitCursor;

            var d = new DataSet();
            var Temp2 = conn.CreateTableByName(m_tablename, "*");
            d.Tables.Add(Temp2);

            string filter = ottieniFiltro(Temp2);
            string orderby = buildOrderByCondition();
            int leggiel = metaprofiler.StartTimer("Leggi Elenco"); //5628   //8612

		    string newCollist = getColumnlist();

            m_metaData = dispatcher.Get(m_tablename);
            m_metaData.DescribeColumns(Temp2, m_listtype);
            m_metaData.Destroy();
            sampleTable = Temp2;


            leggiElenco(m_linked, filter, m_tablename, newCollist, orderby, ToMerge); // m_columnList
            metaprofiler.StopTimer(leggiel);

            int ApplicaImpo = metaprofiler.StartTimer("ApplicaImpostazioniListType");//282
            applicaImpostazioniListType();
            metaprofiler.StopTimer(ApplicaImpo);


            if (!DescribeColumnsApplied){
				int appcode = metaprofiler.StartTimer("ApplicaImpostazioniDaCodice_post");//210
				applicaImpostazioniDaCodice_post(m_listtype, m_listtypeissystem);
				metaprofiler.StopTimer(appcode);
				DescribeColumnsApplied=true;
                
			}

            
		    last_columnlist = getColumnlist();

            int refit = metaprofiler.StartTimer("RefitColumns");//12087  //111
			refitColumns();
			metaprofiler.StopTimer(refit);
			Cursor = Cursors.Default;
			metaprofiler.StopTimer(applist);
		}


		private void gridX_CurrentRowChanged(object sender, System.EventArgs e) {
            if (destroyed) return;
			gridElenchiSelectCell();
		}

		private void gridX_DoubleClick(object sender, System.EventArgs e) {
            if (destroyed) return;
            gridElenchiSelectCell();
			if (Modal) 
				DialogResult = System.Windows.Forms.DialogResult.OK;			
			else
				Close();
		}

		/// <summary>
		/// First column has index 1
		/// </summary>
		/// <param name="Position"></param>
        /// <param name="AbilitaGruppi"></param>
		/// <returns></returns>
		int convertPositionToIndex(int Position, bool AbilitaGruppi){
			int count=0;
			foreach (DataColumn C in DT.Columns){
				if ((gridX.Columns[C.ColumnName].Visible)||
					(AbilitaGruppi&&(isGrouped(C.ColumnName)))){
					if (gridX.Columns[C.ColumnName].VisibleIndex<=Position)
						count++;
				}
			}
			return count;
		}

		bool isGrouped(string fieldname){
			string [] grouped = getGroupedColumns();
			foreach (string gname in grouped){
				if (gname==fieldname) return true;
			}
			return false;
		}

		private void btnExcel_Click(object sender, System.EventArgs e) {

			int []groupby=null;
			object []totals=null;
			string BaseExcelSortBy = "";
			int nest =0;
			int forcedvisible=0;
			int lastgrouped=0;
			foreach (string groupcol in getGroupedColumns()){
				Column CG = gridX.Columns[groupcol];
				if (!CG.Visible) continue;
				if (CG.VisibleIndex>lastgrouped) lastgrouped= CG.VisibleIndex;
			}
			var RGL = gridX.Groups;
			while ((RGL!=null)&&(RGL.Count>0)){
				Column C = gridX.Columns[RGL[0].GroupBy];
				if (C.Visible==false) {
					forcedvisible++;
					//C.VisibleIndex=  lastgrouped+forcedvisible;
					C.Visible=false;
				}
				if (BaseExcelSortBy !="") BaseExcelSortBy+=",";
				BaseExcelSortBy+= C.FieldName;
				int sortdirpos = gridX.SortedColumns.IndexOf(C);
				string sortdir = " ASC ";
				if (sortdirpos>=0) {
					if (gridX.SortedColumns[sortdirpos].SortDirection== SortDirection.Descending)
						sortdir = " DESC ";
				}
				BaseExcelSortBy += sortdir;
				nest++;
				RGL = RGL[0].Groups;
			}
			bool AbilitaGruppi= (nest>0)&&(lbsumfield.CheckedItems.Count>0);

//			if (AbilitaGruppi){ 
//				//Rende visibile le colonne in group by
//				foreach (DataColumn C in DT.Columns){
//					if (gridX.Columns[C.ColumnName].Visible) continue;
//					if (!IsGrouped(C.ColumnName))continue;
//					gridX.Columns[C.ColumnName].Visible=true;
//				}
//			}

			foreach (DataColumn C in DT.Columns){
				int pos;
				if ((gridX.Columns[C.ColumnName].Visible)||
					(AbilitaGruppi && isGrouped(C.ColumnName)))				
					pos= convertPositionToIndex(gridX.Columns[C.ColumnName].VisibleIndex,AbilitaGruppi);
				else
					pos=-1;
				C.ExtendedProperties["ListColPos"]= pos;
				if (pos==-1){
					C.ExtendedProperties["ExcelTitle"]= null;
				}
				else {
					C.ExtendedProperties["ExcelTitle"]= gridX.Columns[C.ColumnName].Title;
				}
			}

			string ExcelSorting= "";
			if (AbilitaGruppi) ExcelSorting = BaseExcelSortBy;
			foreach (Column Sort in gridX.SortedColumns){
				string sortclause = Sort.FieldName;
				if (Sort.SortDirection== SortDirection.None) continue;
				if (Sort.SortDirection == SortDirection.Ascending){
					sortclause+= " ASC ";
				}
				else {
					sortclause+= " DESC ";
				}
				if (ExcelSorting!="") ExcelSorting+=",";
				ExcelSorting+= sortclause;
			}
			if (ExcelSorting=="")ExcelSorting=null;
			DT.ExtendedProperties["ExcelSort"]=ExcelSorting;

			if (AbilitaGruppi){
				groupby = new int[nest];
				int nest2=0;
				Xceed.Grid.Collections.ReadOnlyGroupList RGL2 = gridX.Groups;
				while ((RGL2!=null)&&(RGL2.Count>0)){
					Column C = gridX.Columns[RGL2[0].GroupBy];
					groupby[nest2]= convertPositionToIndex(C.VisibleIndex,true);
					nest2++;
					RGL2 = RGL2[0].Groups;
				}
//				for (int i=0; i<groupby.Length; i++){
//					groupby[i]= gridX.Columns[gridX.Groups[i].GroupBy].Index;
//				}

				totals= new object[lbsumfield.CheckedItems.Count];
				int count=0;
				for (int i=0; i< lbsumfield.Items.Count; i++){
					if (!lbsumfield.GetItemChecked(i))continue;
					int pos=0;
					foreach(Xceed.Grid.Column C in gridX.Columns){
						if (C.Title != lbsumfield.Items[i].ToString())continue;
						pos = convertPositionToIndex(C.VisibleIndex,true);
					}
					totals[count]= pos;
					count++;
				}
			}
			
			exportclass.DataTableToExcel(DT,true,groupby,totals);

		}

		private void btnValore_Click(object sender, System.EventArgs e) {
			if (lbColonna.SelectedItem==null) return;
			string currcol= ((DataRowView)lbColonna.SelectedItem).Row["colname"].ToString();
			if (currcol=="") return;
			var F = new frmSelezioneValori(DT, currcol);
            MetaFactory.factory.getSingleton<IFormCreationListener>().create(F, null);
            var Res= F.ShowDialog();
			if (Res!= System.Windows.Forms.DialogResult.OK) return;
			txtValore.Text= F.listBox1.Text;
		}

		private void btnPrint_Click(object sender, System.EventArgs e) {
            var PDlg = new PrintDialog {AllowPrintToFile = true, AllowSelection = true, AllowSomePages = true};
            var GPD = new GridPrintDocument(gridX);
			PDlg.Document= GPD;
			if (PDlg.ShowDialog(this)!= System.Windows.Forms.DialogResult.OK)return;
			GPD.PrinterSettings = PDlg.PrinterSettings;
			GPD.Print();
			//gridX.Print();
		}

		private void btnPreview_Click(object sender, System.EventArgs e) {
			gridX.PrintPreview();
		}



        /// <summary>
        /// Check if there is a next row in the list
        /// </summary>
        /// <returns></returns>
        public bool hasNext(){
			if (IsDisposed) return false;
			if (!running)return false;
			if (UpdateFormDisabled) return false;
			UpdateFormDisabled=true;
			Row OldRow= gridX.CurrentRow;
			gotoNext();
			Row NewRow= gridX.CurrentRow;
			if (NewRow!=OldRow) gotoPrev();
			UpdateFormDisabled=false;
			
			return (OldRow!=NewRow);
			//if (BM.Position< (BM.Count-1)) return true;
			//return false;
		}

        /// <summary>
        /// Check if there is a previous row in the list
        /// </summary>
        /// <returns></returns>
		public bool hasPrev(){
			if (IsDisposed) return false;
			if (!running)return false;
			if (UpdateFormDisabled) return false;
			UpdateFormDisabled=true;            
			Row OldRow= gridX.CurrentRow;
			gotoPrev();
			Row NewRow= gridX.CurrentRow;
			if (NewRow!=OldRow) gotoNext();
			UpdateFormDisabled=false;
			
			return (OldRow!=NewRow);
			//			return false;
		}
        /// <summary>
        /// True if rowchange is made by runing code
        /// </summary>
		public bool GOTOPilotato=false;

        /// <summary>
        /// Advance to next row  in list
        /// </summary>
		public void gotoNext(){
			if (IsDisposed) return;
			if (!running)return;
			GOTOPilotato=true;
			var First = gridX.FirstVisibleRow;
			var ROld = gridX.CurrentRow;
			gridX.MoveCurrentRow(VerticalDirection.Down);
            gridX.SelectedRows.Clear();

            if (gridX.CurrentRow != null) {
                if (gridX.CurrentRow.CanBeSelected) {
                    gridX.SelectedRows.Add(gridX.CurrentRow);
                }
            }

			if (First!= ROld) gridX.Scroll(ScrollDirection.Down);
			GOTOPilotato=false;
			var R = gridX.CurrentRow;
			if (R==ROld) return;
			if ((typeof(GroupManagerRow) ==R.GetType())||
				(typeof(ValueRow) == R.GetType())
				){
				//GroupManagerRow GR = (Xceed.Grid.GroupManagerRow)R;
				//GotoNext();
			}
		}

        /// <summary>
        /// Goes on row up in the list
        /// </summary>
		public void gotoPrev(){
			if (IsDisposed) return;
			if (!running)return;
			GOTOPilotato=true;
			var ROld = gridX.CurrentRow;
			gridX.MoveCurrentRow(VerticalDirection.Up);
			gridX.Scroll(ScrollDirection.Up);
            gridX.SelectedRows.Clear();
            if (gridX.CurrentRow != null) {
                if (gridX.CurrentRow.CanBeSelected) {
                    gridX.SelectedRows.Add(gridX.CurrentRow);
                }
            }
            GOTOPilotato =false;
			var R = gridX.CurrentRow;
			if (R==ROld) return;
			if ((typeof(Xceed.Grid.GroupManagerRow)==R.GetType())||
				(typeof(Xceed.Grid.ValueRow) == R.GetType())
				){
				//GotoPrev();
			}
		}

        /// <summary>
        /// Move to the first row of the list
        /// </summary>
		public void gotoFirst(){
			if (IsDisposed) return;
			if (!running)return;
			GOTOPilotato=true;
			gridX.MoveCurrentRow(VerticalDirection.Top);
			GOTOPilotato=false;
		}

        /// <summary>
        /// Go to the last   row of the list
        /// </summary>
		public void gotoLast(){
			if (IsDisposed) return;
			if (!running)return;
			GOTOPilotato=true;
			gridX.MoveCurrentRow(VerticalDirection.Bottom);
			GOTOPilotato=false;
		}

		private void cell_Click(object sender, EventArgs e) {
            if (destroyed) return;
            if (!typeof(Cell).IsAssignableFrom(sender.GetType())) return;
			LastSelectedColumn= ((Cell)sender).ParentColumn;
		}

		private void chkSommatorie_CheckedChanged(object sender, System.EventArgs e) {
			//AggiornaSommatorie();
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e) {
		
		}

		private void menuItem1_Click(object sender, System.EventArgs e) {
			for(int i=0; i< lbsumfield.Items.Count; i++)
				lbsumfield.SetItemChecked(i,true);
		}

		private void menuItem2_Click(object sender, System.EventArgs e) {
			for(int i=0; i< lbsumfield.Items.Count; i++)
				lbsumfield.SetItemChecked(i,false);
		}

		void expandAll(Xceed.Grid.Collections.ReadOnlyGroupList GL){
			foreach (Xceed.Grid.Group G in GL){
				G.Expand();
				expandAll(G.Groups);
			}
		}
		void compressAll(Xceed.Grid.Collections.ReadOnlyGroupList GL){
            Row ROld = gridX.CurrentRow;
            bool saveflag = UpdateFormDisabled;
            UpdateFormDisabled = true;
            foreach (Xceed.Grid.Group G in GL){
				G.Collapse();
				compressAll(G.Groups);
			}
            UpdateFormDisabled = saveflag;
            //gridX.CurrentRow = ROld;

        }

		private void mnuExpandAll_Click(object sender, EventArgs e) {
			expandAll(gridX.Groups);
		}

		private void mnuCompressAll_Click(object sender, EventArgs e) {
			compressAll(gridX.Groups);
		}

		

        private void gridX_Sorted(object sender, EventArgs e) {
            if (destroyed) return;
            gridX.Invalidate();
        }
        private void comboTOP_SelectedValueChanged(object sender, EventArgs e) {
            //if (filterlocked) return;
            if (destroyed) return;
            var cboItem = (System.Data.DataRowView)cboList.SelectedItem;
            string newvalue = cboItem.Row["viewname"].ToString();
            selectNewListType(newvalue, false);
            gridX.Refresh();
        }

        private void comboTOP_Leave(object sender, EventArgs e) {
            if (destroyed) return;
            comboTOP_SelectedValueChanged(sender, e);
            gridX.Refresh();
        }

        private void btnCsv_Click(object sender, EventArgs e) {
            var FD = new OpenFileDialog {
                Title = "Seleziona il file da creare",
                AddExtension = true,
                DefaultExt = "CSV",
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false
            };
            var DR = FD.ShowDialog();
            if (DR != System.Windows.Forms.DialogResult.OK) return;


          
            string BaseExcelSortBy = "";
            int nest = 0;
            int forcedvisible = 0;
            int lastgrouped = 0;
            foreach (string groupcol in getGroupedColumns()) {
                Column CG = gridX.Columns[groupcol];
                if (!CG.Visible) continue;
                if (CG.VisibleIndex > lastgrouped) lastgrouped = CG.VisibleIndex;
            }

            var RGL = gridX.Groups;
            while ((RGL != null) && (RGL.Count > 0)) {
                var C = gridX.Columns[RGL[0].GroupBy];
                if (C.Visible == false) {
                    forcedvisible++;
                    C.VisibleIndex = lastgrouped + forcedvisible;
                    C.Visible = false;
                }
                if (BaseExcelSortBy != "") BaseExcelSortBy += ",";
                BaseExcelSortBy += C.FieldName;
                int sortdirpos = gridX.SortedColumns.IndexOf(C);
                string sortdir = " ASC ";
                if (sortdirpos >= 0) {
                    if (gridX.SortedColumns[sortdirpos].SortDirection == SortDirection.Descending)
                        sortdir = " DESC ";
                }
                BaseExcelSortBy += sortdir;
                nest++;
                RGL = RGL[0].Groups;
            }


            foreach (DataColumn C in DT.Columns) {
                int pos;
                if (gridX.Columns[C.ColumnName].Visible)
                    pos = convertPositionToIndex(gridX.Columns[C.ColumnName].VisibleIndex, false);
                else
                    pos = -1;
                C.ExtendedProperties["ListColPos"] = pos;
                if (pos == -1) {
                    C.ExtendedProperties["ExcelTitle"] = null;
                }
                else {
                    C.ExtendedProperties["ExcelTitle"] = gridX.Columns[C.ColumnName].Title;
                }
            }

            string ExcelSorting = "";
            foreach (Column Sort in gridX.SortedColumns) {
                string sortclause = Sort.FieldName;
                if (Sort.SortDirection == SortDirection.None) continue;
                if (Sort.SortDirection == SortDirection.Ascending) {
                    sortclause += " ASC ";
                }
                else {
                    sortclause += " DESC ";
                }
                if (ExcelSorting != "") ExcelSorting += ",";
                ExcelSorting += sortclause;
            }
            if (ExcelSorting == "") ExcelSorting = null;
            DT.ExtendedProperties["ExcelSort"] = ExcelSorting;

            try {
                string S = exportclass.DataTableToCSV(DT, true);
                var SWR = new StreamWriter(FD.FileName, false, Encoding.Default);
                SWR.Write(S);
                SWR.Close();
                //SWR.Dispose();
            }
            catch (Exception E){
                shower.ShowException(this,"Errore nella scrittura del file "+FD.FileName,E);
                //m_linked.LogError("Errore in btnCsv_Click", E);
            }
            Process.Start(FD.FileName);


        }

      

        private void formCustomViewList_FormClosed(object sender, FormClosedEventArgs e) {
            this.Destroy();
        }

        private void tabElenco_Click(object sender, EventArgs e) {

        }
    }
    
   

   
    public class SimpleCustomViewListForm :ICustomViewListForm {
	    private FrmSimpleList f;
	    public void  init (IWinFormMetaData linked, string columnlist,
		    string mergedfilter, string searchtable, string listingType, DataTable toMerge, string sorting, int top,bool filterLocked,string Text) {
		    //FormCustomViewList o FrmSimpleList
		    f = new FrmSimpleList(linked,
			    columnlist,
			    mergedfilter,
			    searchtable,
			    listingType,
			    toMerge,
			    sorting,
			    filterLocked, top
		    );
		    //F.Text= "Elenco "+ this.Name;
	    }

	    public System.Windows.Forms.DialogResult ShowDialog(Form parent) {
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, parent);
		    return f.ShowDialog(parent);
	    }

	    public System.Data.DataRow getLastSelectedRow() {
		    return f?.LastSelectedRow;
	    }
	    public void setStartPosition(FormStartPosition p) {
		    f.StartPosition = p;
	    }


	    public void setFormPosition(Form linkedForm, IFormController ctrl) {
		    if (f == null) return;
		    if (linkedForm != null) {
			    if (linkedForm.IsMdiChild) {
				    //frm.StartPosition= FormStartPosition.CenterParent;
				    if (!f.Modal) {
					    var main = linkedForm.ParentForm;
					    if (main != null) {
						    f.Size = new Size(main.ClientSize.Width - 5, f.Size.Height - 2);
					    }
					    else {
						    f.Size = new Size(0, f.Size.Height - 2);
					    }

					    if (main != null) {
						    f.DesktopLocation = new Point(
							    1,
							    main.ClientSize.Height - f.Size.Height - ctrl.getToolBarManager().getSizeBarHeight());
					    }

					    //Main.AddOwnedForm(frm);
				    }

				    f.MdiParent = linkedForm.MdiParent;
				    //frm.Dock = DockStyle.Bottom;
			    }
			    else {
				    var fParent = (Form) linkedForm.Parent;
				    f.StartPosition = FormStartPosition.CenterScreen;
				    fParent?.AddOwnedForm(f);
				    //frm.Dock = DockStyle.Bottom;
			    }
		    }
		    else {
			    f.StartPosition = FormStartPosition.CenterScreen;
		    }
	    }
	    public void close() {
            f?.Close();
	    }

	    public bool hasNext() {
		    return f.hasNext();
	    }

	    public bool hasPrev() {
		    return f.hasPrev();
	    }

	    public void gotoPrev() {
		    f.gotoPrev();
	    }
	    public void gotoNext() {
		    f.gotoNext();
	    }

	    public void destroy() {
            f?.Destroy();
            f = null;
	    }

	    public void selectSomething() {
		    f.GOTOPilotato = true;
		    f.gridElenchiSelectCell();
		    f.GOTOPilotato = false;
	    }

	    public void show() {
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f,null);
            f.Show();
        }
    }

    public class DefaultCustomViewListForm :ICustomViewListForm {
	    private FormCustomViewList f;
	    public void  init (IWinFormMetaData linked, string columnlist,
		    string mergedfilter, string searchtable, string listingType, DataTable toMerge, string sorting, int top,bool filterLocked,string Text) {
		    //FormCustomViewList o FrmSimpleList
		    f = new FormCustomViewList(linked,
			    columnlist,
			    mergedfilter,
			    searchtable,
			    listingType,
			    toMerge,
			    sorting,
			    filterLocked, top
		    );
		    //F.Text= "Elenco "+ this.Name;
	    }

	    public System.Windows.Forms.DialogResult ShowDialog(Form parent) {
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, parent);
		    return f.ShowDialog(parent);
	    }

	    public System.Data.DataRow getLastSelectedRow() {
		    return f?.LastSelectedRow;
	    }
	    public void setStartPosition(FormStartPosition p) {
		    f.StartPosition = p;
	    }


	    public void setFormPosition(Form linkedForm, IFormController ctrl) {
		    if (f == null) return;
		    if (linkedForm != null) {
			    if (linkedForm.IsMdiChild) {
				    //frm.StartPosition= FormStartPosition.CenterParent;
				    if (!f.Modal) {
					    var main = linkedForm.ParentForm;
					    if (main != null) {
						    f.Size = new Size(main.ClientSize.Width - 5, f.Size.Height - 2);
					    }
					    else {
						    f.Size = new Size(0, f.Size.Height - 2);
					    }

					    if (main != null) {
						    f.DesktopLocation = new Point(
							    1,
							    main.ClientSize.Height - f.Size.Height - ctrl.getToolBarManager().getSizeBarHeight());
					    }

					    //Main.AddOwnedForm(frm);
				    }

				    f.MdiParent = linkedForm.MdiParent;
				    //frm.Dock = DockStyle.Bottom;
			    }
			    else {
				    var fParent = (Form) linkedForm.Parent;
				    f.StartPosition = FormStartPosition.CenterScreen;
				    fParent?.AddOwnedForm(f);
				    //frm.Dock = DockStyle.Bottom;
			    }
		    }
		    else {
			    f.StartPosition = FormStartPosition.CenterScreen;
		    }
	    }
	    public void close() {
            f?.Close();
	    }

	    public bool hasNext() {
		    return f.hasNext();
	    }

	    public bool hasPrev() {
		    return f.hasPrev();
	    }

	    public void gotoPrev() {
		    f.gotoPrev();
	    }
	    public void gotoNext() {
		    f.gotoNext();
	    }

	    public void destroy() {
            f?.Destroy();
            f = null;
	    }

	    public void selectSomething() {
		    f.GOTOPilotato = true;
		    f.gridElenchiSelectCell();
		    f.GOTOPilotato = false;
	    }

	    public void show() {
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f,null);
            f.Show();
	    }

        /// <summary>
        /// Evaluates a filetr string given a basefilter set of WhereClauses
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="T"></param>
        /// <param name="basefilter">Filter to merge with WhereClauses</param>
        /// <param name="whereClauses">DataRow of customviewwhwere type</param>
        /// <param name="canAskAtRunTime">if true, it's allowed to ask parameter 
        ///		at run time when the whereclause needs it</param>
        /// <returns>evaluated filter</returns>
        [Obsolete]
        public static string GetFilterFromCustomViewWhere(DataAccess conn, DataTable T,
            string basefilter,
            System.Data.DataRow[] whereClauses,
            bool canAskAtRunTime
            ) {
            return FormCustomViewList.GetFilterFromCustomViewWhere(conn.Security, T, basefilter, whereClauses, canAskAtRunTime);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="security"></param>
        /// <param name="T"></param>
        /// <param name="basefilter">Filter to merge with WhereClauses</param>
        /// <param name="whereClauses">DataRow of customviewwhwere type</param>
        /// <returns></returns>
        public static string getFilterFromCustomViewWhere(ISecurity security, DataTable T,
            string basefilter,
            System.Data.DataRow[] whereClauses
        ) {
            return FormCustomViewList.GetFilterFromCustomViewWhere(security, T, basefilter, whereClauses, false);
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mdl;
using LM = mdl_language.LanguageManager;
using q = mdl.MetaExpression;
using static mdl_winform.HelpForm;

namespace mdl_winform{
	public partial class FrmSimpleList :Form {
		public FrmSimpleList() {
			InitializeComponent();
		}
		/// <summary>
		/// Class for logging errors
		/// </summary>
		public IErrorLogger errorLogger { get; set; } = ErrorLogger.Logger;
	


		bool UpdateFormDisabled;
		DataTable ToMerge;
		bool DescribeColumnsApplied;
		bool filterlocked;
		/// <summary>
		/// Last DataRow Selected in the list
		/// </summary>
		public System.Data.DataRow LastSelectedRow;
		private VistaFormCustomView DS;

		private string mainTableName;

		private string filtroSuVistaApplicato = null;
		private IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();
		private string lastColTable = "??";


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

		private MetaData m_linked;
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

		private IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();
		bool destroyed = false;

		//private string m_filter;		//eventuale filtro personalizzato
		private string m_sorting;		//eventuale sorting personalizzato
		private System.Data.DataRow m_LastSelectRowGridElenchi;
		private bool update_enabled;
		private string m_tablename;
		private string m_columnList;
		private DataSet DSDati;				//utilizzato per la visualizzazione dei dati (tab Elenchi)
		private VistaFormCustomView DSCopy;	//utilizzato per la copia elenco
		private MetaData m_metaData;
		private ContextMenu mnuContextMenu;



		void setColor(DataGrid g) {
			g.BackColor = formcolors.GridBackColor();
			g.ForeColor = formcolors.GridForeColor();
			g.SelectionBackColor = formcolors.GridSelectionBackColor();
			g.SelectionForeColor = formcolors.GridSelectionForeColor();
		}

		private IFormController controller;
		IDataAccess conn;
		IMetaDataDispatcher dispatcher;
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
            DS = new VistaFormCustomView();

		
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            utils.SetColorOneTime(this, true);

            setColor(g);
            g.KeyUp += HelpForm_KeyUp;
            g.MouseUp += HelpForm_MouseUp;


            PostData.MarkAsTemporaryTable(DS.fieldtosum, false);

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            Cursor.Current = Cursors.WaitCursor;
            

            this.controller = linked.controller;
			conn = controller.conn;
			dispatcher = controller.dispatcher;

			m_linkedview = controller.dispatcher.Get(tablename);
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


            //tab page elenchi
            
            update_enabled = true;

            restartWithNewListType(listtype);
            Cursor.Current = Cursors.Default;

            ////per default il textbox txtBaseFilter è disabilitato
            ////se sono amministratore viene abilitato
            //if (controller.security.GetSys("IsSystemAdmnin") != null) {
            //    m_IAmAdmin = Convert.ToBoolean(controller.security.GetSys("IsSystemAdmin"));
            //}

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
		public FrmSimpleList(IWinFormMetaData linked, string columnlist,
            string filter,
            string tablename,
            string listtype,
            DataTable ToMerge,
            string sorting,
            bool filterlocked,
            int top) {
            inizializza(linked, columnlist, filter, tablename, listtype, ToMerge, sorting, filterlocked,top);
        }
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="linked">MetaData linked to the list</param>
		/// <param name="columnlist">list of column names separated by commas</param>
		/// <param name="filter">search condition to use</param>
		/// <param name="tablename">name of table where data must be retrieved</param>
		/// <param name="listtype">list type to use</param>
		/// <param name="ToMerge">DataTable containing rows that must be merged with found rows</param>
        /// <param name="sorting"></param>
		/// <param name="filterlocked">if true, filter can't be cleared and is not possible to change listtype</param>
		public FrmSimpleList(IWinFormMetaData linked, string columnlist, 
			string filter,
			string tablename, 
			string listtype,
			DataTable ToMerge, 
			string sorting,
			bool filterlocked) {
            inizializza(linked, columnlist, filter, tablename, listtype, ToMerge, sorting, filterlocked, 1000);
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
		private DataTable getDataTable(string tablename, string columnList, string filter) {
			return conn.RUN_SELECT(tablename, columnList, null, filter, 
				null, null, false);
		}
		private void fillComboList() {
			string filter = "(objectname = '" + m_tablename + "')";
			DataTable tabObject = getDataTable("customview", "*", filter);
			cboList.DataSource = tabObject;
			cboList.DisplayMember = "viewname";
			cboList.ValueMember = "viewname";
		}

		string getTop() {
			if(comboTOP.Text != "") {
				try {
					var NUM = Convert.ToInt32(mdl_utils.HelpUi.GetObjectFromString(typeof(int), comboTOP.Text, "x.y"));
					if (NUM >= 0) {
						return  NUM.ToString();
					}
				}
				catch {
				}
			}
			return null;

		}

		void selectNewListType(string newvalue, bool readfromdisk){
			//se è diverso il listtype aggiorno le impostazioni di colonna
			string currtop = getTop();
			if ((m_listtype != newvalue) || (m_top!=currtop)) {
				m_listtype = newvalue;
				m_top = currtop;
				
				if (readfromdisk) leggiListingType(m_listtype);
				applicaListType();		
				//allineaDatasetImpostazioni(sampleTable);
			}

		}

		/// <summary>
		/// Legge il listing type e ne crea uno dummy di sistema se non esiste
		/// Aggiorna il menu contestuale ed i bottoni
		/// Output = DS
		/// </summary>
		/// <param name="listtype"></param>
		void leggiListingType(string listtype){
			int leggilis = mdl_utils.metaprofiler.StartTimer("LeggiListingType");
			Cursor = Cursors.WaitCursor;
			leggiImpostazioniListType(listtype);
			
			//Se il listtype non ha righe in customviewcolumn
			//le aggiungo con tutti i valori a null eccetto per il colname
			if (DS.customviewcolumn.Rows.Count < 1) {
				creaESalvaDummyListType(m_tablename, listtype);
			}
			refitGridColonne();

			updateSysListType(listtype);
			DescribeColumnsApplied=false;
			Cursor  = Cursors.Default;
			mdl_utils.metaprofiler.StopTimer(leggilis);
		}

		

		void refitGridColonne(){
			var f = new formatgrids(g);
			f.AutosizeColumnWidth();
			f.SuppressNulls();
		}

		/// <summary>
		/// Aggiorna il flag m_listtypeissystem, i bottoni ed il menu contestuale
		/// </summary>
		/// <param name="listtype"></param>
		void updateSysListType(string listtype){
			System.Data.DataRow []CurrList = DS.customview._Filter(q.eq("viewname",listtype));
			m_listtypeissystem = ((CurrList.Length>0)&& (CurrList[0]["issystem"].ToString()!="N"));
	
		}

		/// <summary>
		/// Riempie il DataSet (DS) delle impostazioni dell'elenco
		/// </summary>
		/// <param name="list_type">nome dell'elenco da caricare</param>
		private void leggiImpostazioniListType(string list_type) {

			ClearDataSet.RemoveConstraints(DS);
			DS.Clear();


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
		/// <summary>
		/// Input = DS
		/// Legge i dati (se necessario) e li visualizza con il listtype corrente 
		/// Assume m_listtype e m_listtypeissystem già calcolati
		/// </summary>
		void applicaListType(){
			int applist = mdl_utils.metaprofiler.StartTimer("Applica ListType");//18066  //9414
			Cursor= Cursors.WaitCursor;

			var d = new DataSet();
			var Temp2 = conn.CreateTableByName(m_tablename, "*");
			d.Tables.Add(Temp2);

			string filter = ottieniFiltro(Temp2);
			string orderby = buildOrderByCondition();
			int leggiel = mdl_utils.metaprofiler.StartTimer("Leggi Elenco"); //5628   //8612

			string newCollist = getColumnlist();

			m_metaData = m_linked.dispatcher.Get(m_tablename);
			m_metaData.DescribeColumns(Temp2, m_listtype);
			m_metaData.Destroy();

			leggiElenco(m_linked, filter, m_tablename, newCollist, orderby, ToMerge); // m_columnList
			mdl_utils.metaprofiler.StopTimer(leggiel);

			int ApplicaImpo = mdl_utils.metaprofiler.StartTimer("ApplicaImpostazioniListType");//282
			applicaImpostazioniListType();
			mdl_utils.metaprofiler.StopTimer(ApplicaImpo);


			if (!DescribeColumnsApplied){
				int appcode = mdl_utils.metaprofiler.StartTimer("ApplicaImpostazioniDaCodice_post");//210
				applicaImpostazioniDaCodice_post(m_listtype, m_listtypeissystem);
				mdl_utils.metaprofiler.StopTimer(appcode);
				DescribeColumnsApplied=true;
                
			}

            
			last_columnlist = getColumnlist();
			g.Tag = DT.TableName + "." + m_listtype;
			HelpForm.SetDataGrid(g,DT);
			
			
			refitGridColonne();

			Cursor = Cursors.Default;
			mdl_utils.metaprofiler.StopTimer(applist);
		}
		HelpForm helpF = new HelpForm();
		string last_filter_applied="??";
		string last_sort_applied = "??";
		string last_columnlist = "??";
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
			MetaData linked,
			string filter,
			string tablename,
			string columnlist,
			string orderby,
			DataTable ToMerge) {

			if (filter == "") filter = null;
			if (orderby == "") orderby = null;
			g.BeginInit();
			string new_top = getTop();

			//gridX.SuspendLayout();
			if ((last_filter_applied != filter) || (new_top != last_top) || (last_sort_applied != orderby) ||
			    (last_columnlist != columnlist)) {
				last_sort_applied = orderby;
				last_filter_applied = filter;
				last_columnlist = columnlist;
				last_top = new_top;
				int leggitabella = mdl_utils.metaprofiler.StartTimer("LeggiTabellaElenco");
				leggiTabellaElenco(linked, columnlist, tablename, filter, orderby,
					ToMerge); //era filterlocked ? "*":columnlist
				mdl_utils.metaprofiler.StopTimer(leggitabella);

				int setdatabind = mdl_utils.metaprofiler.StartTimer("Set DataBinding");
				//il binding dei dati alla grid è effettuato (solo) qui
				try {
					g.SetDataBinding(DSDati, DT.TableName);

				}
				catch {
				}

				mdl_utils.metaprofiler.StopTimer(setdatabind);

			


				//Azzera il flag "fitted" di tutte le colonne
				clearFittedFlag();

			}

			GOTOPilotato = true;

			g.EndInit();
			GOTOPilotato = false;

			this.Text = "Elenco " + linked.Name + " (" + DT.Rows.Count + " righe)";

			//gridX.ResumeLayout();
		}
		/// <summary>
		/// True if rowchange is made by runing code
		/// </summary>
		public bool GOTOPilotato=false;

		void clearFittedFlag(){
			
		}


		
		void leggiTabellaElenco(MetaData linked,
				string columnlist,
				string tablename,
				string filter,
                string orderby,
				DataTable ToMerge
			){
            this.mainTableName=tablename;

            var QHS = conn.GetQueryHelper();
			if ((ToMerge==null)||(ToMerge.Rows.Count==0)){
                string filtersec = filter;


				if (DSDati==null || lastColTable!=columnlist) {
				    lastColTable = columnlist;
                    DSDati = new DataSet();
				    ClearDataSet.RemoveConstraints(DSDati);
                    filtersec = QHS.AppAnd(filtersec, linked.security.SelectCondition(tablename, true));

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
                                linked.security.SelectCondition(tablename, true));

					update_enabled=false;
					model.clear(DT);
					update_enabled=true;
                    filtroSuVistaApplicato = filtersec;
					conn.RUN_SELECT_INTO_TABLE( DT, orderby, filtersec, getTop(), (filter == filtersec));
				}

				//Elimina le righe non selezionabili
				int testcanselect2 = mdl_utils.metaprofiler.StartTimer("Removing not selectable");
				//linked.Conn.DeleteAllUnselectable(DT);
//				foreach (System.Data.DataRow RR  in DT.Select()){
//					if (!linked.Conn.CanSelect(RR)){
//						RR.Delete();
//						RR.AcceptChanges();
//					}
//				}
				mdl_utils.metaprofiler.StopTimer(testcanselect2);
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
                                linked.security.SelectCondition(DataAccess.GetTableForReading(DT),true));
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
				m_filter = GetFilterFromCustomViewWhere(m_linked.security, T,null, rows, true);
			}
			catch(Exception e) {
				showMsg($"Errore nella costruzione del filtro\r\rDettaglio: {e.Message}",
					C_MSG_TITLE_ERRORE, mdl.MessageBoxButtons.OK, mdl.MessageBoxIcon.Error);
				m_filter = "";
			}

			if (m_filter!="") m_filter="("+m_filter+")";
			string filtrocomplessivo= m_filter;

			mybasefilter = m_basefilter;

			if ((mybasefilter!="")&&(mybasefilter!=null)) {
				mybasefilter="("+mybasefilter+")"; 
				filtrocomplessivo= GetData.MergeFilters(m_filter,mybasefilter);
			}

			if (filtrocomplessivo == "")
				filtrocomplessivo = null;

			string statfilter = m_linkedview.GetStaticFilter(m_listtype);
			filtrocomplessivo = GetData.MergeFilters(statfilter, filtrocomplessivo);

			return filtrocomplessivo;
		}

		
		private mdl.DialogResult showMsg(string msg, string caption,
			mdl.MessageBoxButtons button, mdl.MessageBoxIcon icon) {
			return shower.Show(null,msg, caption, button);
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
            AskParameter F = new AskParameter(caption,
                DescribeOperator(operatore),countoperands);
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(F,null);
            var res = F.ShowDialog(null);
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
                    
	            ORes[i] = mdl_utils.HelpUi.GetObjectFromString(T.Columns[clause["columnname"].ToString()].DataType, result[i],"x.y");
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
            var MTemp = controller.dispatcher.Get(m_tablename);
            if (m_linked.dispatcher.unrecoverableError) {
                m_linked.ErroreIrrecuperabile = true;
                shower.ShowError(controller?.linkedForm,
                    $"Errore nel caricamento del metadato {m_tablename} è necessario riavviare il programma.", "Errore");
            }

            MTemp.DescribeColumns(Temp, m_listtypeissystem ? m_listtype : (string.IsNullOrEmpty(MTemp.DefaultListType) ? "default" : MTemp.DefaultListType));
            MTemp.Destroy();

            if (!DescribeColumnsApplied) {
                int appcode = mdl_utils.metaprofiler.StartTimer("ApplicaImpostazioniDaCodice_pre");//210
                applicaImpostazioniDaCodice_pre(m_listtype, m_listtypeissystem);
				mdl_utils.metaprofiler.StopTimer(appcode);
            }

            string[] allfields = m_columnList.Split(',');
            string newCollist = "";
            foreach (string col in allfields) {
                string col2 = col.Trim();
                if (!Temp.Columns.Contains(col2)) continue;
                if (newCollist != "") newCollist += ",";
                newCollist += col2;
            }

            return newCollist;//restituisce solo i campi esistenti nella tabella/vista 
        }


		/// <summary>
		/// Costruisce la condizione di ordinamento
		/// </summary>
		private string buildOrderByCondition() {
			System.Data.DataRow[] rows = DS.customvieworderby.Select();

			string m_sorting = getOrderByFromCustomViewOrderBy(rows);
	
			string mybasesorting = m_basesorting;
			if (mybasesorting!="") {
			
				
					if ((m_sorting != "")&&(m_sorting!=null))
						m_sorting += ", " + mybasesorting;
					else
						m_sorting = mybasesorting;
				
			}
			if (m_sorting == "")
				m_sorting = null;
			return compattaOrderBy(m_sorting);
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
		/// <summary>
		/// Esegue il commit dei dati relativi alle impostazioni della grid
		/// Elenchi
		/// </summary>
		/// <param name="ds">Il dataset tipizzato custom view</param>
		/// <returns></returns>
		private bool eseguiPostData(VistaFormCustomView ds) {
			ds.fieldtosum.AcceptChanges();
			var MyMeta = controller.dispatcher.Get("customview");
			PostData.MarkAsTemporaryTable(ds.fieldtosum, false);

			var mPostdata = MyMeta.Get_PostData();
			mPostdata.initClass(ds, conn);
			return mPostdata.DO_POST();
		}

		/// <summary>
		/// Imposta le proprietà (ordine colonne, captio, visibilità) del grid dei dati in base a customviewcolumn
		/// </summary>
		/// <remarks>Il binding dei dati deve essere già stato fatto.
		/// </remarks>
		private void applicaImpostazioniListType() {
			int appImp = mdl_utils.metaprofiler.StartTimer("ApplicaImpostazioniListType");
			int autoindex=0;
			g.SuspendLayout();
			foreach (var row in DS.customviewcolumn.Select(null,"listcolpos asc")) {
				string colname = row["colname"].ToString();
				if (!DT.Columns.Contains(colname)) continue;

				//colname può essere null su elenchi pre-esistenti
				if (colname == "")	continue; 

				if (row["visible"].ToString() != "1") {
					MetaData.DescribeAColumn(DT,colname,DT.Columns[colname].Caption,-1);
					continue;
				}
				autoindex++;

				//imposto a true il visible 
				string caption= row["heading"].ToString();

				//g.Columns[colname].FormatSpecifier = HelpForm.GetFormatForColumn(DSDati.Tables[m_tablename].Columns[colname]);

				//autosize della colonna  ---> spostato in fase di lettura dei dati
				//gridX.Columns[colname].Width = gridX.Columns[colname].GetFittedWidth();

				//ordine di visualizzazione delle colonne
				if (row["listcolpos"]==DBNull.Value){
					MetaData.DescribeAColumn(DT,colname,caption,autoindex);
				}
				else {
					MetaData.DescribeAColumn(DT,colname,caption,Convert.ToInt32(row["listcolpos"]));
				}
			}
			foreach (DataColumn CC in DT.Columns) {
				if (DS.customviewcolumn.Select("colname=" + mdl_utils.Quoting.quotedstrvalue(CC.ColumnName, false)).Length ==0) {
					MetaData.DescribeAColumn(DT,CC.ColumnName,CC.Caption,-1);
				}
					
			}
			g.ResumeLayout();
			mdl_utils.metaprofiler.StopTimer(appImp);
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
			m_metaData = controller.dispatcher.Get(m_tablename);
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
        /// Imposta caption e visibilità delle colonne dei dati in base alle impostazioni da codice, solo per 
        ///  gli elenchi di sistema.  Inoltre nasconde colonne senza dati e colonna senza impostazioni nel dataset
        /// </summary>
        private void applicaImpostazioniDaCodice_post(string listingtype, bool issystem) {
			
			//Se il listtype non è di sistema non eseguo la DescribeColumns
			if (!issystem) return;

			//il metadato ha impatto solo sulla caption e/o visibilità
			m_metaData = controller.dispatcher.Get(m_tablename);
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
                if (allnull) {
	                MetaData.DescribeAColumn(DT, col.ColumnName, col.Caption, -1);
	                continue;
                }

                //Vede se ci sono impostazioni nel ds sulla colonna
				System.Data.DataRow[] rows = DS.customviewcolumn._Filter(q.eq("colname",col.ColumnName));
				if (rows.Length == 0) {
					//il dataset non contiene la riga per quel columnname (es. campi desc. foreign key) , la colonna è nascosta
					MetaData.DescribeAColumn(DT,col.ColumnName,col.Caption,-1);
					continue;
				}

                //La colonna ha caption vuota o inizia col punto, la colonna è nascosta
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
		  /// Check if there is a next row in the list
		  /// </summary>
		  /// <returns></returns>
		  public bool hasNext(){
			  if (IsDisposed) return false;
			  if (!running)return false;
			  if (UpdateFormDisabled) return false;
			  UpdateFormDisabled=true;
			  var OldRow= g.CurrentRowIndex;
			  gotoNext();
			  var NewRow= g.CurrentRowIndex;
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
			  var OldRow= g.CurrentRowIndex;
			  gotoPrev();
			  var NewRow= g.CurrentRowIndex;
			  if (NewRow!=OldRow) gotoNext();
			  UpdateFormDisabled=false;
			
			  return (OldRow!=NewRow);
			  //			return false;
		  }

		  /// <summary>
		  /// Advance to next row  in list
		  /// </summary>
		  public void gotoNext(){
			  if (IsDisposed) return;
			  if (!running)return;
			  GOTOPilotato=true;
			  var ROld = g.CurrentRowIndex;
			  if (ROld < DT.Rows.Count) {
				  GOTOPilotato=false;
				  g.CurrentRowIndex = ROld + 1;
			  }
		  }

		  /// <summary>
		  /// Goes on row up in the list
		  /// </summary>
		  public void gotoPrev(){
			  if (IsDisposed) return;
			  if (!running)return;
			  GOTOPilotato=true;
			  var ROld = g.CurrentRowIndex;
			  if (ROld > 0) {
				  GOTOPilotato=false;
				  g.CurrentRowIndex = ROld -1;
			  }
		  }


        /// <summary>
        /// Dispose all resources
        /// </summary>
        public void Destroy() {
            if (destroyed) return;
            var FParent = this.ParentForm;
            if (FParent != null) FParent.RemoveOwnedForm(this);
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
            
            if (g.DataBindings != null) {
                g.DataBindings.Clear();
            }
            try {
                this.g.Dispose();
                this.g = null;
            }
            catch { }
            this.ToMerge = null;

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
       

            destroyed = true;
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

		  /// <summary>
        /// Called when users clicks on the list grid
        /// </summary>
        public void gridElenchiSelectCell(){
			if (!running) return;
            if (destroyed) return;
			if (UpdateFormDisabled) return;
			if (!update_enabled) return;
            
            if (!GOTOPilotato){
	            if (destroyed) {                    
                    return;
                }
                if (destroyed) {
                    return;
                }
                if (destroyed) {
                    return;
                }

                
            }
			update_enabled = false;
			if (DT.Rows.Count==0){
				m_LastSelectRowGridElenchi=null;
				update_enabled=true;
                return;
			}
			int rowselect = mdl_utils.metaprofiler.StartTimer("RowListSelect");
            mdl_utils.crono MyC = new mdl_utils.crono("LoadTime");
			try {
				DataRow DR=null;
				try {
					DR = ((DataRowView)(g.BindingContext[DT.DataSet, DT.TableName].Current)).Row;
				}
				catch {
				}

			    DR = getMainRow(DR);
			    if (DR != null) {
			        if (m_LastSelectRowGridElenchi == DR) {
			            update_enabled = true;
			            long ms = MyC.GetDuration();

						mdl_utils.metaprofiler.StopTimer(rowselect);
			            return;
			        }
			        if (filterlocked || (m_linked == null)) {
			            LastSelectedRow = DR;

			        }
			        else {			            
			            if (controller== null || controller.TryToSelectRow(DR, m_listtype)) {
			                m_LastSelectRowGridElenchi = DR;
			            }
			        }
			    }
			}
			catch (Exception E){
			    shower.Show(this,E.Message);
			}
			update_enabled=true;

            long msec = MyC.GetDuration();


			mdl_utils.metaprofiler.StopTimer(rowselect);
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
                    m_linked.LogError(
                        $"getMainRow:La definizione della chiave per la tabella/vista:{mainTable.TableName} vista {vista} non è completa. Filtro Applicato:{filtertoApply} (righe trovate:{tt.Rows.Count})");
                }
            }
            else {
                m_linked.LogError($"getMainRow Warn:Manca la definizione della chiave per la tabella:{mainTable.TableName}/{m_tablename} vista {vista} metaview {m_linkedview.Name}. ");
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
	}
}

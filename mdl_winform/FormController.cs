using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DataTable = System.Data.DataTable;
using System.Reflection;
using LM = mdl_language.LanguageManager;
using System.Linq;
using mdl;
using q = mdl.MetaExpression;
using static mdl_utils.tagUtils;
using System.Diagnostics.CodeAnalysis;
using wDialogResult = System.Windows.Forms.DialogResult;


#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable IDE1006 // Naming Styles


namespace mdl_winform {

    /// <summary>
    /// Manages events of a form and its interaction with the user
    /// </summary>
    public class FormController : IFormController {

        public ICustomViewListForm currentListForm { get; set; }
        /// <summary>
        /// When a insert operation is invoked, a call to AfterClear is implied. This tells the form that an insert is coming
        /// </summary>
        public bool IsClearBeforeInsert { get; set; }

        /// <summary>
        /// When user selects a row from a list, a call to AfterClear is implied. This tells the form that another row select is coming
        /// </summary>
        public bool IsClearBeforeEdit { get; set; }

      
        /// <summary>
        /// Edit type of current form
        /// </summary>
        public string editType  { get; set; }
       
        /// <summary>
        /// Event manager used by the controller
        /// </summary>
        public static IFormEventsManager MainEventsManager = new FormEventsManager();

        /// <summary>
        /// Current row selected in the controller
        /// </summary>
        public DataRow lastSelectedRow {
            get { return meta.LastSelectedRow; }
            set { meta.LastSelectedRow = value; } }

        /// <summary>
        /// Check if linked form is a list
        /// </summary>
        public bool isList { get { return meta.IsList; } }

        /// <summary>
        /// Check if linked form is a list
        /// </summary>
        public bool startEmpty { get { return meta.StartEmpty; } }


        /// <summary>
        /// Check if linked form is a tree
        /// </summary>
        public bool isTree { get { return meta.IsTree; } }

        //private MetaData.form_types myFormType= MetaData.form_types.main;

        ///// <summary>
        ///// Get/set the type of a form (main/detail/unknown)
        ///// </summary>
        //public MetaData.form_types formType {
        //    get { return myFormType; }      // era meta.formType
        //    set { myFormType = value; } // era meta.formType
        //}

        /// <summary>
        /// Check if a form is a subEntity detail of main form
        /// </summary>
        public bool isSubentity { get; set; }

     

        /// <summary>
        /// true if MainRefresh is enabled
        /// </summary>
        public bool MainRefreshEnabled { get; set; }

        /// <summary>
        /// True if "mainselect" is enabled
        /// </summary>
        public bool MainSelectionEnabled { get; set; }

        /// <summary>
        /// When true, does not warn if canceling an insert operation.
        /// </summary>
        public bool DontWarnOnInsertCancel{ get; set; }


        /// <summary>
        /// must be set to false if SetSearch/DoSearch must be disabled on form.
        /// </summary>
        public bool SearchEnabled { get; set; }

        /// <summary>
        /// When false main insert button is disabled
        /// </summary>
        public bool CanInsert { get; set; }

        /// <summary>
        /// When false main insert copy button is disabled
        /// </summary>
        public bool CanInsertCopy { get; set; }

        
        /// <summary>
        /// When false main Cancel button is disabled
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// when false, "mainsave" button is disabled
        /// </summary>
        public bool CanSave{ get; set; }

        /// <summary>
        /// Dataset with current data
        /// </summary>
        public DataSet ds { get; set; }


		/// <summary>
		/// True if the entity edited by the form (or subentities) has been modified
		/// </summary>
		public bool entityChanged { get; set; } 
		



         /// <summary>
        /// Message shower for windows client
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="btns"></param>
        /// <returns></returns>
        public bool WindowsShowClientMsg(string message, string title, mdl.MessageBoxButtons btns) {
            var d = shower.Show(linkedForm,message, title, btns);
            return d == mdl.DialogResult.OK || d == mdl.DialogResult.Yes;
        }


		/// <summary>
		/// Container of entity detail controls in a list form
		/// </summary>
		public Control formDetailControl { get; set; }

        /// <summary>
        /// Enable automatic events
        /// </summary>
        [Obsolete]
        public void EnableAutoEvents() {
            eventManager.EnableAutoEvents();
        }

        /// <summary>
        /// Disable automatic events
        /// </summary>
        [Obsolete]
        public void DisableAutoEvents() {
            eventManager.DisableAutoEvents();
        }


     

        /// <summary>
        /// Connection to db
        /// </summary>
        public IDataAccess conn { get; set; }

        public DataTable primaryTable { get; set; }

        public string primaryTableName {
            get {return primaryTable.TableName;}
        }

        /// <summary>
        /// getData class used
        /// </summary>
        public IGetData getData;


        /// <summary>
        /// True if form has been destroyed
        /// </summary>
        public bool destroyed { get; set; }

        /// <summary>
        /// Helpform class that manages the form
        /// </summary>
        public IHelpForm helpForm { get; set; }

        /// <summary>
        /// Error logger 
        /// </summary>
        public IErrorLogger errorLogger;

        /// <summary>
        /// Linked metadata
        /// </summary>
        public WinFormMetaData meta { get; set; }


        private Form _linkedForm;



        public IWinEntityDispatcher dispatcher { get; set; }

        private QueryHelper qhs;
        CQueryHelper qhc = new CQueryHelper();

        /// <summary>
        /// Events manager 
        /// </summary>
        public IFormEventsManager eventManager { get; set; } = MetaFactory.create<IFormEventsManager>();

        /// <summary>
        /// Security class
        /// </summary>
        public ISecurity security { get; set; }

        /// <summary>
        /// MetaModel used
        /// </summary>
        public IMetaModel metaModel = MetaFactory.factory.getSingleton<IMetaModel>();

        
        /// <summary>
        /// Message shower used to display messages and ask questions
        /// </summary>
        public IMessageShower shower { get; set; } = MetaFactory.factory.getSingleton<IMessageShower>();
     

        void setDBUnrecoverableError() {
            MetaFactory.factory.getSingleton<IMessageShower>().Show(null,
                "La connessione al db è stata interrotta. E' necessario disconnettersi, ripristinare la rete e riconnettersi al db.",
                "Errore");
            ErroreIrrecuperabile = true;
        }

        void setUnrecoverableError(string msg) {
            MetaFactory.factory.getSingleton<IMessageShower>().Show(null,msg,"Error");
            ErroreIrrecuperabile = true;
        }

        void createIndexes() {
	        if (ds.getIndexManager() != null) return;
	        var idm = new IndexManager(ds);
	        idm.createPrimaryKeysIndexes();
            //Crea gli indici sulle relazioni entity-subentity e sulle loro tabelle parent
	        createSubEntityIndexes(primaryTable);
        }


        /// <summary>
        /// Attaches to a form the instances of eventManager, dataset, datatable
        /// </summary>
        /// <param name="form"></param>
        public virtual void Init(Form form) {
            //linkedForm.attachInstance(this,typeof(IFormController));
            //ottiene dal form le istanze di IDataAccess IGetData IMetaData ISecurity dispatcher IErrorLogger
            _linkedForm = form;
            conn = form.getInstance<IDataAccess>();
            getData = form.getInstance<IGetData>();
            meta = form.getInstance<IMetaData>() as WinFormMetaData;
            security = form.getInstance<ISecurity>();
            dispatcher = form.getInstance<IWinEntityDispatcher>();            
            errorLogger = form.getInstance<IErrorLogger>();            

            //collega al form le istanze di IFormController IFormEventsManager ISecurity            
            form.attachInstance(eventManager, typeof(IFormEventsManager));

            if (conn?.BrokenConnection != false) {
                setDBUnrecoverableError();
                return;
            }

            if (!checkConn()) {
                setDBUnrecoverableError();
                return;
            }


            ds = form.safeGetInstance<DataSet>();
            if (ds == null) {
                ds=getFormDataSet(form);
                form.attachInstance(ds, typeof(DataSet));
            }


            var primaryTableName = meta.TableName;
            if (ds == null) {
                setUnrecoverableError(
                    $"Form {_linkedForm.Name} has no DataSet named DS. Can't bind a {primaryTableName}  MetaData");
                return;
            }

            ClearDataSet.RemoveConstraints(ds);

            if (!ds.Tables.Contains(primaryTableName)) {
                setUnrecoverableError($"Form {_linkedForm.Name} has no table named {primaryTableName}");
                return;
            }

            primaryTable = ds.Tables[primaryTableName];
            form.attachInstance(primaryTable, typeof(DataTable));

			createIndexes();

            qhs = conn.GetQueryHelper();

            formState = mdl_winform.form_states.setsearch;
            DrawState = mdl_winform.form_drawstates.building;
            curroperation = mainoperations.none;
            isClosing = false;

        }


        /// <summary>
        /// Attaches all framework events to a form
        /// </summary>
        public virtual void doLink() {
            // ReSharper disable once VirtualMemberCallInConstructor
            helpForm = linkedForm.createInstance<IHelpForm>(); //Instanzia un HelpForm  primaryTable, dbConn,_eventManager
            _formDetailControl = detailControl(linkedForm);

            if (meta.SourceRow != null) {
                formState = mdl_winform.form_states.edit; //non da problemi di 5176
                if (meta.SourceRow.RowState == DataRowState.Added) formState = form_states.insert;
            }

            FormPrefilled = false;
            formInited = false; //implicitely sets meta.formInited=false

            Hashtable h;
            var tag = _linkedForm.Tag;
            if (tag as string == "") tag = null;
            if (tag == null) {
                h = new Hashtable();
                _linkedForm.Tag = h;
            }
            else {
                h = tag as Hashtable;
                if (h == null) {
                    errorLogger.MarkEvent($"Bad tag found on form {_linkedForm.Name}:{_linkedForm.Tag}");
                    h = new Hashtable();
                    _linkedForm.Tag = h;
                }
            }

            h[MetaData.MetaDataKey] = meta;
            //_privateLinkedForm = true;

            CallMethod("AfterLink");
            if (ErroreIrrecuperabile) return;

            helpProvider= setHelpProviderForForm(linkedForm);
            meta.DescribeColumns(primaryTable);

            if (security.GetSys("FlagMenuAdmin") != null) {
                helpForm.toolTipOnControl = (security.GetSys("FlagMenuAdmin").ToString() == "S");
            }

            helpForm.AddEvents(linkedForm);

            CMM = GetContextMenuManager();
            CMM.addContextMenuToForm(linkedForm);
            linkedForm.Closing += form_Closing;
            linkedForm.FormClosed += f_FormClosed;
            addAfterRowSelect();
            linkedForm.Text = meta.getName();

            linkedForm.AutoScaleDimensions = new SizeF(96F, 96F);
            linkedForm.AutoScaleMode = AutoScaleMode.Inherit;

            conn.ReadStructures(ds).GetAwaiter().GetResult();
            //new Task(() => {
	           // foreach (DataTable t in ds.Tables) dispatcher.Get(t.TableName);
            //}).Start();

            getData.ReadCached();
            CallMethod("BeforeActivation");

            linkedForm.Activated += frm_Activated;
            linkedForm.Enter += frm_Activated;
        }

         static readonly object __o = new object();
        static long lastLoadTime;

        internal static void setLastLoadTime(long ms) {
        //lock (__o) {
            lastLoadTime = ms;
        //}

        if (LastLoadTimeChanged != null) LastLoadTimeChanged.Invoke(ms);
        }

          /// <summary>
        /// Type of delegate called when a row is selected with the load time length
        /// </summary>
        /// <param name="ms"></param>
        [SuppressMessage("Microsoft.Design", "CA1009")]
        public delegate void LastLoadTimeChangedDelegate(long ms);

        /// <summary>
        /// called when a row is selected with the load time length
        /// </summary>
        /// 
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public static event LastLoadTimeChangedDelegate LastLoadTimeChanged;

               
        /// <summary>
        /// Gets last load time of the selected row
        /// </summary>
        /// <returns></returns>
        public static long getLastLoadTime() {
            //lock (__o) {
                return lastLoadTime;
            //}
        }

        private HelpProvider helpProvider;
        /// <summary>
        /// Searches a row on primarytable given  a base filter and select it in the form
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="baseFilter"></param>
        /// <param name="emptylist">When 0, an empty list is displayed</param>
        /// <returns></returns>
        public bool searchRow(string listType, q baseFilter, bool emptylist) {
            if (primaryTable == null) return false;
            q filter = helpForm.GetSearchCondition(linkedForm);
            filter = q.and(filter, baseFilter);
            var top = 1000;
            if (emptylist) top = 0;
            meta.listTop = top;
            var r = meta.SelectOne(listType, filter, null);

            if (r == null) {
                return false;
            }

            SelectRow(r, listType);
            r.Delete();
            return true;
        }

        void frm_Activated(object sender, EventArgs e) {
            if (destroyed) return;
            if (ErroreIrrecuperabile) return;
            if (linkedForm == null) return;
            if (linkedForm.IsDisposed) return;
            if (isClosing) return;
            if (helpForm == null) return;
            internalFrm_Activated(sender, e);
            MainEventsManager.dispatch(new FormActivated(linkedForm));

        }

        void internalFrm_Activated(object sender, EventArgs e) {
         
            if (ds == null) {
                errorLogger.logException("DS is null, an exception will be generated in frmActivated (5067).");
            }

            if (formPrefilled) {
                if (isClosing) return;
                if (formInited) FreshToolBar();
                return;
            }

            formPrefilled = true;
            checkConn();
            if (conn == null || conn.BrokenConnection) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(null,
                    LM.dbConnectionInterrupted,                    
                    LM.ErrorTitle);
                DrawState = form_drawstates.done;
                formInited = true;
                ErroreIrrecuperabile = true;
                return;
            }

            var handle = mdl_utils.MetaProfiler.StartTimer("frm_Activated");
            Cursor.Current = Cursors.AppStarting;


            DrawState = form_drawstates.prefilling;

            meta.SetDefaults(primaryTable);
            updateHelpFormState();
            //helpForm.drawMode = HelpForm.drawmode.setsearch;
            myAdjustTablesForGridDisplay(linkedForm);
            Do_Prefill();
            setMyHandler(linkedForm);
            iterateSetMyHandlers(linkedForm.Controls);


            if (isList && meta.StartEmpty) {
                eventManager.dispatch(new StartClearMainRowEvent());
                Clear(); //(true)
                helpForm.SetMainManagers(linkedForm, setMetaDataManager);
                Cursor.Current = Cursors.Default;
                CallMethod("AfterActivation");

                eventManager.dispatch(new StopClearMainRowEvent());

                formInited = true;
                mdl_utils.MetaProfiler.StopTimer(handle);
                DrawState = form_drawstates.done;
                FreshToolBar();
                return;
            }

            //THIS IS THE CRITIC ZONE
            if (isList) {
                DrawState = form_drawstates.prefilling;
                helpForm.SetMainManagers(linkedForm, setMetaDataManager);
                if (isTree) {
                    if (helpForm.mainTableSelector == null) {
                        Cursor.Current = Cursors.Default;
                        MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, $"Form {linkedForm.Name} has no main treeview","Design time error");
                        DrawState = form_drawstates.done;
                        formInited = true;
                        FreshToolBar();
                        return;
                    }

                    var tv = (TreeView) helpForm.mainTableSelector;
                    var treeFilled = false;
                    if (meta.StartFieldWanted != null) {
                        treeFilled = helpForm.SetTreeByStart(tv, meta.StartFilter, meta.StartValueWanted,
                            meta.StartFieldWanted);
                        if (!treeFilled) {
                            shower.Show(LM.rowNotFound, LM.warningLabel, mdl.MessageBoxButtons.OK);
                        }
                    }

                    if (!treeFilled) helpForm.FilteredPreFillTree(tv, meta.StartFilter, false);
                }
                else {
                    eventManager.DisableAutoEvents();
                    getData.GetPrimaryTable(meta.StartFilter);
                    security.DeleteAllUnselectable(primaryTable);
                    eventManager.EnableAutoEvents();
                    //myGetData.DO_GET(false,null);
                }

                if (primaryTable.Rows.Count > 0 && HelpForm.GetLastSelected(primaryTable) == null) {
                    HelpForm.SetLastSelected(primaryTable,primaryTable.Rows[0]);
                }

                //In realtà qui andrebbe controllato se, nel caso di form lista non
                // tree (ossia grid), è stato effettivamente letto qualcosa.
                //In tal caso il form dovrebbe andare in search e non in edit.
                if (HelpForm.GetLastSelected(primaryTable) != null) {
                    formState = form_states.edit; //assumes something has been displayed (fighting 5176)
                }

                CallMethod("AfterActivation");
                if (ErroreIrrecuperabile) {
                    formInited = true;
                    DrawState = form_drawstates.done;
                    return;
                }

                linkedForm.BringToFront();
                firstFillForThisRow = true;

                eventManager.dispatch(new StartMainRowSelectionEvent(HelpForm.GetLastSelected(primaryTable)));
                //helpForm.comboBoxToRefilter = true;

                ReFillControls(); //DOES NOT ANYMORE sets form_drawstate to "done"

                if (HelpForm.GetLastSelected(primaryTable) != null) {
                    formState = form_states.edit; //assumes something has been displayed (fighting 5176)
                }

                CallMethod("ListFilled");

                eventManager.dispatch(new StopMainRowSelectionEvent(HelpForm.GetLastSelected(primaryTable)));
                //helpForm.comboBoxToRefilter = false;

                firstFillForThisRow = false;
                //LinkedForm.ResumeLayout();
                Cursor.Current = Cursors.Default;
                mdl_utils.MetaProfiler.StopTimer(handle);
                //Application.DoEvents();
                formInited = true;
                DrawState = form_drawstates.done;
                FreshToolBar();
                return;
            }

            //Form singolo
            if (formState == form_states.setsearch || meta.SourceRow == null) {
                CallMethod("AfterActivation");
                if (ErroreIrrecuperabile) return;
                formInited = true; //se no non funzionano i comandi eventuali nell'afterclear
                if (formState == form_states.setsearch) {
                    eventManager.dispatch(new StartClearMainRowEvent());
                    Clear(); //(true)
                    eventManager.dispatch(new StopClearMainRowEvent());
                }
            }
            else {
                //if (meta.sourceRow == null) {
                //    meta.sourceRow = HelpForm.GetLastSelected(primaryTable);
                //    LogError($"Il campo sourceRow è stato impostato in base al lastSelectedRow. "+
                //             $"Nome form:{linkedForm.Name}, parentForm:{linkedForm?.ParentForm?.Name}, editType:{meta.editType}",null
                //             );
                //}
                if (meta.SourceRow == null) {
                    logError(
                        $"SourceRow is null, an exception will be thrown in frmActivated (5176).{formState}\n\r" +
                        $"Form name:{linkedForm.Name}, parentForm:{linkedForm?.ParentForm?.Name}, editType:{editType}",
                        null);
                    //EditMode has been received
                }

                DrawState = form_drawstates.prefilling;
                primaryTable.copyAutoIncrementPropertiesFrom(meta.SourceRow.Table);

                metaModel.SetExtraParams(primaryTable, metaModel.GetExtraParams(meta.SourceRow.Table));
                //meta.SetEntityDetail(meta.sourceRow);
                getData.StartFrom(meta.SourceRow).GetAwaiter().GetResult();
                var start = HelpForm.GetLastSelected(primaryTable);
                formState = meta.SourceRow.RowState == DataRowState.Added ? form_states.insert : form_states.edit;
                eventManager.DisableAutoEvents();
                DO_GET(true, start); //it was false              
                eventManager.EnableAutoEvents();
                CallMethod("AfterActivation");
                if (ErroreIrrecuperabile) return;
                firstFillForThisRow = true;
                //helpForm.comboBoxToRefilter = true;
                eventManager.dispatch(new StartMainRowSelectionEvent(start));
                ReFillControls();
                eventManager.dispatch(new StopMainRowSelectionEvent(start));
                //helpForm.comboBoxToRefilter = false;
                firstFillForThisRow = false;
            }

            //LinkedForm.ResumeLayout();
            Cursor.Current = Cursors.Default;
            mdl_utils.MetaProfiler.StopTimer(handle);
            //Application.DoEvents();
            formInited = true;
            DrawState = form_drawstates.done;
            if (meta.FirstSearchFilter != null) {
                var listtype = meta.DefaultListType;
                DoMainCommand($"maindosearch.{listtype}.{meta.FirstSearchFilter}");
                meta.FirstSearchFilter = null;
            }

            FreshToolBar();
        }

      

        /// <summary>
        /// Event called when user double clicks a TreeNavigator
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DoubleClickNavManager(object sender, EventArgs e) {
            if (destroyed) return;
            if (!formInited) return;
            doubleClickTreeManager(null, null);
        }

        void doubleClickTreeManager(object sender, EventArgs e) {
            if (destroyed) return;
            if (!formInited) return;
            if (!MainSelectionEnabled) return;
            var c = helpForm.mainTableSelector;
            if (!(c is TreeView)) return;
            var tv = (TreeView) c;
            var n = tv.SelectedNode;
            var tn = (tree_node) n?.Tag;
            if (tn == null) return;
            if (!tn.CanSelect()) return;
            if (tn.Row.Table.TableName != primaryTable.TableName) return;
            var tvm = TreeViewManager.GetManager(primaryTable);
            if (!tvm.DoubleClickForSelect) return;
            MainSelect();
        }

        private void mainGridDoubleClick(object sender, EventArgs e) {
            if (destroyed) return;
            if (!isList) return;
            if (!MainSelectionEnabled) return;
            if (!meta.CommandEnabled("mainselect")) return;
            DoMainCommand("mainselect");
        }


        /// <summary>
        /// Index of DataTable Extended Property used for storing 
        ///		Form ExtraParameters
        /// </summary>
        [Obsolete] internal const string extraParams = "ExtraParameters";

        /// <summary>
        /// Sets the Control Manager for datagrids, treeviews
        /// </summary>
        /// <param name="c"></param>
        void setMetaDataManager(Control c) {
            if (c is DataGrid g) {
                if (g.Tag == null) {
                    return;
                }

                if (g.Tag.ToString().StartsWith("TreeNavigator")) {
                    g.DoubleClick += DoubleClickNavManager;
                }
                else {
                    g.Enter += enterManager; //G.GotFocus
                    g.Scroll += gridScrollManager;
                    g.DoubleClick += mainGridDoubleClick;
                }

                return;
            }

            if (c is TreeView tv) {
                tv.BeforeSelect += beforeSelectTreeManager;
                tv.AfterSelect += afterSelectTreeManager;
                tv.BeforeCollapse += beforeCollapseManager;
                tv.DoubleClick += doubleClickTreeManager;
            }
        }

        bool SuspendListManager = false;

        /// <summary>
        /// True when a row is selected in the list control during the fill
        /// </summary>
        [Obsolete]
        public bool suspendListManager {
            get { return SuspendListManager; }
            set { SuspendListManager = value; }
        }


        /// <summary>
        /// Event called when the form is tree and before a node is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void beforeSelectTreeManager(object sender, TreeViewCancelEventArgs e) {
            if (destroyed) return;
            if (!formInited) return;
            if (suspendListManager) return;
            if (warnUnsaved()) {
                ds.RejectChanges();
                formState = form_states.edit;
            }
            else {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Event called when the form is tree and after a node is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void afterSelectTreeManager(object sender, TreeViewEventArgs e) {
            //FreshForm(false);
        }

        /// <summary>
        /// Event called when a node is collapsed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void beforeCollapseManager(object sender, TreeViewCancelEventArgs e) {
            if (destroyed) return;
            if (!formInited) return;
            var tv = (TreeView) sender;
            var n = tv.SelectedNode;
            if (n == null) return;
            while ((n.Parent != null) && (n.Parent != e.Node)) n = n.Parent;
            if (n.Parent == null) return;
            if (warnUnsaved()) {
                ds.RejectChanges();
                formState = form_states.edit;
            }
            else {
                e.Cancel = true;
                return;
            }

            tv.SelectedNode = e.Node;
        }

        bool _enterManagerDisable;

        void enterManager(object sender, EventArgs e) {
            if (destroyed) return;
            if (!formInited) return;
            if (suspendListManager) return;
            if (_enterManagerDisable) return;
            var c = (Control) sender;
            c.Enabled = false;

            _enterManagerDisable = true;
            if (warnUnsaved()) {
                if (ds.HasChanges()) {
                    ds.RejectChanges();
                    formState = form_states.edit; //Aggiunta 2005
                    helpForm.ControlChanged(sender, null);
                }

                c.Enabled = true;
            }
            else {
                focusDetail();
                c.Enabled = true;
            }

            _enterManagerDisable = false;
        }




        void gridScrollManager(object sender, EventArgs e) {
            if (!formInited) return;
            focusDetail();
        }

        void iterateSetMyHandlers(Control.ControlCollection cs) {
            foreach (Control c in cs) {
                setMyHandler(c);
                if (c.HasChildren) {
                    iterateSetMyHandlers(c.Controls);
                }
            }
        }

        /// <summary>
        /// Sender is a BUTTON - tag like table.edit_type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void edit_Click(object sender, EventArgs e) {
            if (destroyed) return;
            var g = helpForm.GetLinkedGrid((Control) sender);
            if (g == null) return;
            var b = sender as Button;
            if (b != null) {
                if (!b.Enabled) return;
                b.Enabled = false;
            }

            try {
                Edit_Grid_Row(g, GetFieldLower(((Control) sender).Tag.ToString(), 1));
            }
            catch {
                // ignored
            }

            if (b != null) b.Enabled = true;
        }

        /// <summary>
        /// Function called when a grid-edit button is pressed
        /// </summary>
        /// <param name="g">Data Grid containing entity rows</param>
        /// <param name="editType">edit type to use for editing current grid row</param>
        /// <returns>Edited DataRow</returns>
        public virtual DataRow Edit_Grid_Row(DataGrid g, string editType) {
            if (g?.FindForm() == null) return null;
            if (!formInited) return null;
            var rowIndex = g.CurrentRowIndex;
            if (rowIndex < 0) return null;

            //gets data from form
            GetFormData(true);

            if (!(g.DataSource is DataSet sourceDataSet)) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm,
                    $"DataGrid {g.Name} in Form {g.FindForm()?.Name} has a wrong Tag ({mdl_utils.Quoting.quote(g.Tag, false)})");
                return null;
            }

            DataTable sourceTable;
            DataRow currDr;
            var res = helpForm.GetCurrentRow(g, out sourceTable, out currDr);
            if (!res) return null;
            if (currDr == null) return null;
            if (sourceTable == null) return null;
            if (ErroreIrrecuperabile) return null;

            var editRes = EditDataRow(currDr, editType, out var newCurr);
            if (!editRes) return currDr;
            currDr = newCurr;
            if (MetaModel.IsSubEntity(sourceTable, primaryTable)) {
                entityChanged = true;
            }

            helpForm.IterateFillRelatedControls(g.Parent.Controls, g, sourceTable, currDr);

            //It's necesssary to do something cause Grid must re-evaluated:
            // - calculated fields
            // - resize of columns 
            //2019 non serve farlo, lo fa già il FreshForm
            //getData.GetTemporaryValues(sourceTable);    //Chiama indirettamente CalculateTable 

            Do_Prefill(sourceTable.TableName);

            FreshForm(true, false); //2 feb 2003

            return currDr;
        }


        private void insert_Click(object sender, EventArgs e) {
            if (destroyed) return;
            var g = helpForm.GetLinkedGrid((Control) sender);
            if (g == null) return;
            var b = sender as Button;
            if (b != null) {
                if (!b.Enabled) return;
                b.Enabled = false;
            }

            try {
                Insert_Grid_Row(g, GetFieldLower(((Control) sender).Tag.ToString(), 1));
            }
            catch {
                // ignored
            }

            if (b != null) b.Enabled = true;
        }
         /// <summary>
        /// Statically callable function to implement a grid-add event
        /// </summary>
        /// <param name="g">Grid containing table where row has to be added</param>
        /// <param name="editType">edit type to use for editing</param>
        /// <returns>new row or null if canceled</returns>
        public static DataRow Insert_Grid(DataGrid g, string editType) {
            var f = g.FindForm();
            return GetController(f).Insert_Grid_Row(g, editType);
            //return GetMetaData(f)?.Insert_Grid_Row(g, editType);
        }
         /// <summary>
        /// Function to link with an grid-add button
        /// </summary>
        /// <param name="g">Grid into which add the row</param>
        /// <param name="_editType">Edit Type to use</param>
        /// <returns>new row or null if action canceled</returns>
        public virtual DataRow Insert_Grid_Row(DataGrid g, string _editType) {
            if (!formInited) return null;

            //gets data from form
            GetFormData(true);


            var sourceDataSet = (DataSet) g?.DataSource;
            if (sourceDataSet == null || g.Tag==null) {
                shower.Show(linkedForm,
                    // ReSharper disable once PossibleNullReferenceException
                    $"DataGrid {g.Name} in Form {g.FindForm().Name} has a wrong Tag ({mdl_utils.Quoting.quote(g?.Tag ?? "(empty)", false)})");
                return null;
            }

            var tablename = GetTableName(g.Tag.ToString());
            var sourceTable = ds.Tables[tablename];

            var parent = HelpForm.GetLastSelected(primaryTable);
            if (parent == null) {
                return null;
            }

            var unaliased = sourceTable.tableForReading();

            var m = dispatcher.GetWinFormMeta(unaliased);
            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm,
                    LM.errorLoadingMeta(unaliased) //$"Errore nel caricamento del metadato {unaliased} è necessario riavviare il programma."
                    , LM.ErrorTitle,null);
                return null;
            }

            m.ExtraParameter = metaModel.GetExtraParams(sourceTable); //SourceTable.ExtendedProperties[FormController.extraParams];

            
            m.SetDefaults(sourceTable, _editType);
            var r = m.GetNewRow(parent, sourceTable,_editType);
            if (r == null) {
                shower.Show(linkedForm,
                    LM.invalidDataOnTable(sourceTable.TableName)
                    //$"La tabella {sourceTable.TableName} contiene dati non validi. Contattare il servizio di assistenza."
                    );
                return null;
            }

            m.SourceRow = r; //r is not null
            //M.IsInsert= true;	  Automatic, cause R is "added"
            m.Edit(linkedForm, EditType(_editType, 0), true);

            if (m.EntityChanged) {
                r = m.NewSourceRow;
                if (MetaModel.IsSubEntity(sourceTable, ds.Tables[primaryTableName])) {
                    entityChanged = true;
                }

                helpForm.IterateFillRelatedControls(g.Parent.Controls, g, sourceTable, r);
                //UnsavedChanges=true;
            }
            else {
                r.Delete();
                r = null;
                //R.AcceptChanges();//No database activity is needed
            }

            m.Destroy();
            //It's necesssary to do something cause Grid must re-evaluated:
            // - calculated fields
            // - resize of columns 
            //2019 : non serve farlo, lo fa già il FreshForm
            //getData.GetTemporaryValues(r);
            
            //GetData.CalculateTable(SourceTable);

            Do_Prefill(sourceTable.TableName); //put again on 31/1/2003
            //FreshForm(SourceTable.TableName); //removed on 21/1/2003 -

            helpForm.IterateFillRelatedControls(g.Parent.Controls, null, sourceTable, r);

            FreshForm(true); //2 feb 2003

            return r;
        }


        private void unlink_Click(object sender, EventArgs e) {
            if (destroyed) return;
            var g = helpForm.GetLinkedGrid((Control) sender);
            if (g == null) return;
            var b = sender as Button;
            if (b != null) b.Enabled = false;
            try {
                Unlink_Grid(g);
            }
            catch {
                //ignore
            }

            if (b != null) b.Enabled = true;

        }

        private void delete_Click(object sender, EventArgs e) {
            if (destroyed) return;
            var g = helpForm.GetLinkedGrid((Control) sender);
            if (g == null) return;
            var b = sender as Button;
            if (b != null) {
                if (!b.Enabled) return;
                b.Enabled = false;
            }

            try {
                Delete_Grid(g);
            }
            catch {
                //ignore
            }

            if (b != null) b.Enabled = true;

        }

        /// <summary>
        /// Managed click of grid buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Button_Click(object sender, EventArgs e) {
            if (destroyed) return;
            if (sender is Button b) {
                if (b.Tag == null)
                    return;
                var cmd = GetFieldLower(b.Tag.ToString(), 0);

                if (!IsEmpty) {
                    if (cmd.Equals("edit")) {
                        edit_Click(b, e);
                        return;
                    }

                    if (cmd.Equals("insert")) {
                        insert_Click(b, e);
                        return;
                    }

                    if (cmd.Equals("delete")) {
                        delete_Click(b, e);
                        return;
                    }

                    if (cmd.Equals("unlink")) {
                        unlink_Click(b, e);
                        return;
                    }
                }

                DoMainCommand(b.Tag.ToString());
                return;
            }

            if (sender is DataGrid grid) {
                var tag = grid.Tag;
                if (tag == null)
                    return;
                var editType = GetFieldLower(tag.ToString(), 2);
                if (editType == null)
                    return;
                Edit_Grid_Row(grid, editType);
            }
        }

        void setMyHandler(Control c) {
            if (c is Button button) {
                var tag = GetStandardTag(button.Tag) ?? "";
                tag = tag.ToLower();
                if (tag.StartsWith("edit") || tag.StartsWith("delete") || tag.StartsWith("insert") ||
                    tag.StartsWith("unlink")
                ) {
                    if (tag.StartsWith("edit"))
                        button.Text = LM.editLable;
                    if (tag.StartsWith("delete"))
                        button.Text = LM.deleteLable;
                    if (tag.StartsWith("insert"))
                        button.Text = LM.addLabel;

                }

                button.Click -= Button_Click;
                button.Click += Button_Click;
            }

            if (c is DataGrid grid) {
                grid.DoubleClick += Button_Click;
                return;
            }

            var box = c as GroupBox;
            if (box?.Tag != null) {
                if ((box.Tag.ToString().StartsWith("AutoChoose")) ||
                    (box.Tag.ToString().StartsWith("AutoManage")))
                    SetAutoMode(box);
            }

        }

        /// <summary>
        /// G has tag: AutoChoose.TextBoxName.ListType.StartFilter or
        ///            AutoManage.TextBoxName.EditType.StartFilter
        /// </summary>
        /// <param name="g"></param>
        public void SetAutoMode(GroupBox g) {
            if (g.Tag == null) return;
            var tag = g.Tag.ToString();
            g.BackColor = formcolors.AutoChooseBackColor();
            string tablename = null;
            var kind = GetField(tag, 0);
            var type = GetField(tag, 2);
            var startFilter = MetaExpression.fromString(GetLastField(tag, 3));


            //Gets start value - start field from control named textboxname
            string startf = null;
            TextBox T = null;
            var tname = GetField(tag, 1);
            if (tname != null) {
                foreach (Control c in g.Controls) {
                    if (c.Name != tname) continue;
                    if (!(c is TextBox)) break;
                    if (c.Tag == null) break;
                    var tag2 = GetStandardTag(c.Tag);
                    if (tag2 == null) break;
                    var ttag = GetStandardTag(tag2);

                    tablename = GetFieldLower(ttag, 0);
                    if (tablename == null) break;
                    if (ds.Tables[tablename] == null) break;

                    var tcol = GetColumnName(ttag);
                    if (tcol == null) break;
                    startf = tcol;
                    T = (TextBox) c;
                    break;
                }
            }

            if (T == null) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(
                    $"A textBox named {tname} was not found in a groupbox named {g.Name} and text set to {(g.Text ?? "vuota")}",
                    "Design time error");
                return;
            }

            var ai = new AutoInfo(g, type, startFilter, startf, tablename, kind);
            var unlinked = (_ae[T.Name] == null);
            _ae[T.Name] = ai;
            if (unlinked) {
                T.LostFocus += textBoxLostFocus;
                T.GotFocus += textBoxGotFocus;
                if (addInvisibleTextBox(ai)) {
                    T.Tag = GetStandardTag(T.Tag) + "?x";
                }

            }

            setToolTipAuto(g, ai);
            setColor(g, true);

        }

        private Hashtable _ae = new Hashtable();

        /// <summary>
        /// Get the Autoinfo relate to a TextBox given the TextBon name
        /// </summary>
        /// <param name="textBoxName"></param>
        /// <returns></returns>
        public AutoInfo GetAutoInfo(string textBoxName) {
            return _ae[textBoxName] as AutoInfo;
        }


        /// <summary>
        /// Called for AutoManage purposes
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// 
        private void textBoxGotFocus(object s, EventArgs e) {
            if (destroyed) return;
            if (_insideTexBoxLeave) return;
            if (s == null) return;
            var T = (TextBox) s;
            if (T.IsDisposed) return;
            if (T.Enabled == false) return;
            if (T.ReadOnly) return;
            if (helpForm == null) return;
            helpForm.lastTextNoFound = $"{T.Name}#{T.Text}";
        }


        bool _insideTexBoxLeave;



        void recursiveSelect(Control c) {
            if (c.Parent != null) {
                if (c.GetType() != typeof(Form)) recursiveSelect(c.Parent);
            }

            c.Focus();
            c.Select();
        }


        private void textBoxLostFocus(object s, EventArgs e) {
            if (destroyed) return;
            if (_insideTexBoxLeave) return;
            if (s == null) return;
            if (isClosing) return;
            if (ErroreIrrecuperabile) return;
            if (!DrawStateIsDone) return;
            var savedLastTextNoFound = helpForm.lastTextNoFound;
            var T = (TextBox) s;
            if (T.IsDisposed) return;

            if (T.Enabled == false) return;
            if (T.ReadOnly) return;
            if (T.Name + "#" + T.Text == helpForm.lastTextNoFound) return;


            var ft = T.FindForm();
            var g = T.Parent;

            var ai = (AutoInfo) _ae[T.Name];
            if (ai == null) return;
            if (ai.busy) return;
            ai.busy = true;

            var saved = closeDisabled;
            closeDisabled = true;

            //Get all conditions from "table" linked control in groupbox
            q filter;
            var oldtag = T.Tag.ToString();
            if (ai.kind == "AutoManage") {
                T.Tag = null;
                filter = helpForm.GetSpecificCondition(g.Controls, ai.table); //ex AI.G
                T.Tag = oldtag;
            }
            else {
                var oldval = T.Text;
                try {
                    var txtBoxTag = GetStandardTag(oldtag);
                    T.Tag = txtBoxTag;
                    var table = GetTableName(txtBoxTag);
                    var col = GetColumnName(txtBoxTag);
                    var c = ds.Tables[table].Columns[col];
                    if (c.DataType == typeof(string)) {
                        if (!oldval.EndsWith("%")) T.Text += "%";
                    }

                    filter = helpForm.GetSpecificCondition(g.Controls, ai.table); //ex AI.G
                }
                catch {
                    filter = helpForm.GetSpecificCondition(g.Controls, ai.table); //ex AI.G
                }

                T.Text = oldval;
                T.Tag = oldtag;
            }


            //Get all conditions from "primary table - table" relation linked controls
            //take startvalue
            var startv = T.Text.Trim();
            var selected = false;

            if (startv == "") {
                Choose($"choose.{ai.table}.unknown.clear", g);
                selected = true;
            }

            filter = q.and(filter, ai.startfilter);

            var newStr = T.Name + "#" + T.Text;
            if (newStr == helpForm.lastTextNoFound) {
                ai.busy = false;
                closeDisabled = saved;
                return;
            }

            _insideTexBoxLeave = true;
            helpForm.lastTextNoFound = newStr;

            //do a choose.table.listtype.filter
            if ((!selected) && (ai.kind == "AutoChoose")) {
                selected = Choose($"choose.{ai.table}.{ai.type}.{filter}");
            }

            //do a manage.table.edittype.filter with startfield/value specified
            if ((!selected) && (ai.kind == "AutoManage")) {
                selected = Manage($"manage.{ai.table}.{ai.type}.{filter}", ai.startfield, startv, g);
            }

            if (selected)
                helpForm.lastTextNoFound = $"{T.Name}#{T.Text}"; //CAN BE DIFFERENT FROM THE PREVIOUS!!
            else
                helpForm.lastTextNoFound = savedLastTextNoFound;
            helpForm.lastTextBoxChanged = null;
            _insideTexBoxLeave = false;

            Stopwatch.GetTimestamp(); // GetTimer.timeGetTime();


            if (isClosing) {
                closeDisabled = false;
                return;
            }

            if (ErroreIrrecuperabile) {
                closeDisabled = false;
                return;
            }

            if (!selected) {
                _insideTexBoxLeave = true;
                HelpForm.FocusControl(T);

                ft.Activate();
                recursiveSelect(T);
                if (!T.Focused) {
                    helpForm.FillControl(T);
                }

                _insideTexBoxLeave = false;

            }

            ai.busy = false;
            setToolTipAuto(g, ai);
            closeDisabled = saved;

        }

        DataRelation mainChildRelation(DataTable parentTable) {
	        if (parentTable == null) return null;
			//Searches for primary table as child
			DataRelation foundExtraEntity=null;
			DataRelation foundAnyTable=null;
			foreach (DataRelation rel in parentTable.ChildRelations) {
				if (rel.ChildTable == primaryTable) return rel;
				foreach (var extra in helpForm.getExtraEntities()) {
					if (rel.ChildTable.TableName == extra) foundExtraEntity = rel;
				}
				foundAnyTable = rel;
			}
			return foundExtraEntity ?? foundAnyTable;

        }

        bool addInvisibleTextBox(AutoInfo ai) {
            var parenttable = ai.table;
            var parentTable = ds.Tables[parenttable];
            var rel = mainChildRelation(parentTable);
            if (rel == null) return false;
            //if (parentTable?.ChildRelations.Count != 1) return false;
            //var rel = parentTable.ChildRelations[0];
            var childtable = rel.ChildTable.TableName;
            if (rel.ChildColumns.Length > 1) return false;

            ai.ParentTable = parentTable;
            ai.ChildTable = rel.ChildTable;
            ai.childfield = rel.ChildColumns[0].ColumnName;
            ai.parentfield = rel.ParentColumns[0].ColumnName;

            var txtName = $"InvisibleTxt{parenttable}_{childtable}";
            if (childtable != primaryTable.TableName) {
                //txtName = "SubEntity" + txtName;
                helpForm.addExtraEntity(childtable);
            }

            var T = new TextBox {
                Name = txtName,
                Tag = $"{parenttable}.{ai.parentfield}?{childtable}.{ai.childfield}",
                Visible = false,
                ReadOnly = true
            };
            ai.G.Controls.Add(T);
            ai.InvisibleTextBox = T;
            return true;
        }

        private void setToolTipAuto(Control g, AutoInfo ai) {
            if (g == null) return;
            if (ai?.InvisibleTextBox?.Tag == null) return;
            if (ai.ParentTable?.TableName == null) return;
            if (ai.parentfield == null) return;
            var s =
                $"\nHidden tag:{ai.InvisibleTextBox.Tag}\nHidden value:{ai.GetInvisibleText()}\nParent:{ai.ParentTable.TableName}.{ai.parentfield}";

            helpForm.setAdditionalTooltip(g.Name, s);
            helpForm.setToolTip(g);
        }

        /// <summary>
        /// Add calculated fields in order to display grid
        /// </summary>
        /// <param name="f"></param>
        private void myAdjustTablesForGridDisplay(Form f) {
            helpForm.AdjustTablesForDisplay(f, adjustTableForDisplay);
        }


        private void adjustTableForDisplay(Control c, DataTable T, string gridTag) {
            //if (typeof(TextBox).IsAssignableFrom(C.GetType()))return; INUTILE!!!
            if (c is TextBox tx) {
                if (tx.Multiline) {
                    tx.AcceptsReturn = true;
                }

                return;
            }

            var metaT = dispatcher.Get(T.tableForReading()) as IWinFormMetaData ;
            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm,LM.errorLoadingMeta(T.tableForReading()),LM.ErrorTitle);
                return;
            }

            if (metaT == null) return;        
           
            if (c is DataGrid) {
                metaT.DescribeColumns(T, ListingType(gridTag, 1));
            }

            if (c is ListView) {
                metaT.DescribeColumns(T, ListingType(gridTag, 1));
                var parent2 = T;
                var parent1 = primaryTable;
                var middle = QueryCreator.GetMiddleTable(parent1, parent2);
                if (middle != null) {
                    var metaMiddle = dispatcher.Get(middle.TableName);
                    metaMiddle.SetDefaults(middle);
                }

                return;
            }

            if (c is TreeView view) {
                //Here the treeViewManager is created
                metaT.DescribeTree(view, T, ListingType(gridTag, 1));
                ITreeViewManager m = TreeViewManager.GetManager(T);
                m.security = security;
                m.calcTreeViewDataAccess(getData);
                return;
            }

            if (c is ComboBox box) {
                if (box.DataSource == null) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show($"Combobox {box.Name} has no datasource.", "Design time error");
                    return;
                }

                var tt = (DataTable)box.DataSource;
                var realTable = ds.Tables[tt.TableName];
                var metaSource = dispatcher.Get(tt.tableForReading());
                if (realTable == null)
                    return;
                var insertfilter = QueryCreator.GetFilterForInsert(realTable) ?? metaSource.GetFilterForInsert(realTable);
                if (insertfilter != null)
                    QueryCreator.SetInsertFilter(realTable, insertfilter);
                var searchfilter = QueryCreator.GetFilterForSearch(realTable) ?? metaSource.GetFilterForSearch(realTable);
                if (searchfilter != null)
                    QueryCreator.SetSearchFilter(realTable, searchfilter);

            }


        }

        /// <summary>
        /// Unregister all events from a control
        /// </summary>
        /// <param name="c"></param>
        public static void UnregisterAllEvents(Control c) {
            if (c.Controls.Count > 0) {
                foreach (Control cc in c.Controls) UnregisterAllEvents(cc);
            }
            //EventSuppressor.EventSuppress(C);

            var theType = c.GetType();
            if (theType == typeof(TextBox)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(RadioButton)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(ComboBox)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(CheckBox)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(Button)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(DataGrid)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(TreeView)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(TabControl)) {
                EventSuppressor.EventSuppress(c);
                return;
            }

            if (theType == typeof(GroupBox)) {
                EventSuppressor.EventSuppress(c);
                return;
            }
        }

        /// <summary>
        /// Disconnects from list form 
        /// </summary>
        public void UnlinkListForm() {
            if (linkedForm != null && !isClosing) linkedForm.KeyPreview = false;
            currentListForm = null;
        }

        private void f_FormClosed(object sender, FormClosedEventArgs e) {
	        //MetaFactory.factory.getSingleton<IFormCreationListener>()?.hide(linkedForm);

            if (linkedForm != null) UnregisterAllEvents(linkedForm);
            ErrorLogger.Logger.WarnEvent("CloseForm:"+linkedForm.GetType().Assembly.ManifestModule.ToString().Replace(".dll",""));
            isClosing = true;
            UnlinkToolBar();
            if (currentListForm != null) currentListForm.close();
            UnlinkListForm();
            Destroy();
        }
        

        /// <summary>
        /// Set to true to disable the close Command
        /// </summary>
        public bool closeDisabled {get;set;}


         /// <summary>
        /// True when some main command is running
        /// </summary>
        public bool doingCommand {get;set;}

        private void form_Closing(object sender, CancelEventArgs e) {
            if (e == null || e.Cancel) return;
            if (destroyed) return;

            if (linkedForm != null) linkedForm.ActiveControl = null;
            if (ErroreIrrecuperabile) return;

            if (linkedForm != null && linkedForm.OwnedForms.Length > 0) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, LM.childFormStillOpened,LM.adviceLabel );
                e.Cancel = true;
                return;
            }

            if (closeDisabled || doingCommand) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, LM.cantCloseWait, LM.adviceLabel);
                e.Cancel = true;
                return;
            }

            if (!formInited) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, LM.stillNotOpenedForm);
                e.Cancel = true;
                return;
            }

            if (!DrawStateIsDone && !ErroreIrrecuperabile) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, LM.waitForOperationEnd);
                e.Cancel = true;
                return;
            }

            if (!warnUnsaved()) {
                e.Cancel = true;
            }


        }


        //private bool _privateLinkedForm;


        /// <inheritdoc />
        public void focusDetail() {
            if (isClosing) return;
            if (ErroreIrrecuperabile) return;
            if (_formDetailControl == null) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, "No PUBLIC Control named 'MetaDataDetail' was found in Form.","Design time error");
                return;
            }

            _formDetailControl.Focus();
            _formDetailControl.Select();
        }

        /// <summary>
        /// Uses reflection functions to detect the DataSet linked to a Form
        /// </summary>
        DataSet getFormDataSet(Form f) {
            var dsInfo = f.GetType().GetField("DS");
            if (dsInfo == null) return null;
            if (!typeof(DataSet).IsAssignableFrom(dsInfo.FieldType)) return null;
            return (DataSet) dsInfo.GetValue(f);
        }

        /// <summary>
        /// Gets the form control having name "MetaDataSetail"
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        Control detailControl(Form f) {
            if (f == null) return null;
            var detailInfo = f.GetType().GetField("MetaDataDetail");
            if (detailInfo == null) return null;
            if (!typeof(Control).IsAssignableFrom(detailInfo.FieldType)) return null;
            var c = (Control) detailInfo.GetValue(f);
            return c;
        }

        //Control to use for SetFocus() on List forms
        Control _formDetailControl;

        bool checkConn() {
            if (conn == null) return false;
            if (conn.Open()) {
                conn.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Destructor
        /// </summary>
        public void Destroy() {
            if (CMM != null) {
                CMM.destroy();
                CMM = null;
            }

            if (_ae != null) {
                _ae.Clear();
                _ae = null;
            }
            ds.getIndexManager()?.Dispose();
            ds = null;

            helpProvider?.Dispose();
			helpProvider = null;

            linkedForm.Activated -= frm_Activated;
            linkedForm.Enter -= frm_Activated;
            linkedForm.Closing -= form_Closing;
            linkedForm.FormClosed -= f_FormClosed;

            if (helpForm != null) {
                helpForm.BeforeRowSelect = null;
                helpForm.AfterRowSelect = null;

                helpForm.Destroy();
                helpForm = null;
            }

            if (currentListForm != null) {
                currentListForm.destroy();
                currentListForm = null;
            }


            if (linkedForm != null) {
                //LinkedForm.Enabled = false;
                var f = linkedForm;
                try {
                    Hashtable h = f.Tag as Hashtable;
                    h?.Clear();
                }
                catch {
                    // ignored
                }

                FormController.UnregisterAllEvents(f);
                f.Tag = null;

                f.KeyDown -= F_KeyDown;

                //LinkedForm = null;
            }

         

            eventManager = null;

            shower = null;

            _linkedForm = null;

        }

        /// <summary>
        /// Creates a ContextMenuManager for this form
        /// </summary>
        /// <returns></returns>
        public virtual IContextMenuManager GetContextMenuManager() {
            return new ContextMenuManager();
        }

        /// <summary>
        /// Gets the Help file name for a form
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static string GetHelpFileName(Form form) {
            var codebase = form.GetType().Assembly.CodeBase;
            var nomedll = Path.GetFileNameWithoutExtension(codebase);
            var filename = nomedll + ".mht";
            return filename;
        }

        private static HelpProvider setHelpProviderForForm(Form form) {
            try {
                var filename = GetHelpFileName(form);
                if (!File.Exists(filename)) return null;
                var helpProvider = new HelpProvider {HelpNamespace = filename};
                helpProvider.SetHelpNavigator(form, System.Windows.Forms.HelpNavigator.TableOfContents);
                form.HelpButton = true;
                return helpProvider;
            }
            catch {
                return null;
            }
        }

        public bool formInited { get; set; }
        ///// <summary>
        ///// True if form has been correctly inited
        ///// </summary>
        //public bool formInited {
        //    get { return meta.formInited; }
        //    set { meta.formInited = value; }
        //}

        bool FormPrefilled;

        /// <summary>
        /// True if form has been prefilled
        /// </summary>
        public bool formPrefilled {
            get { return FormPrefilled; }
            set { FormPrefilled = value; }
        }



        /// <summary>
        /// Adds AfterRowSelect event to all form's conrols
        /// </summary>
        internal void addAfterRowSelect() {
            if (linkedForm == null) return;
            Type FormType = linkedForm.GetType();
            MethodInfo FormMethod = FormType.GetMethod("MetaData_AfterRowSelect", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance); 
            if (FormMethod != null) helpForm.AfterRowSelect = afterRowSelect;

            FormMethod = FormType.GetMethod("MetaData_BeforeRowSelect", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (FormMethod != null) helpForm.BeforeRowSelect = beforeRowSelect;

        }

        /// <summary>
        /// Context menu menager linked to form
        /// </summary>
        public IContextMenuManager CMM;

        /// <summary>
        /// Fill form controls inside a container
        /// </summary>
        /// <param name="container"></param>
        public void ReFillControls(Control.ControlCollection container) {
            var saved = DrawState;
            DrawState = form_drawstates.filling;
            setFormText();
            if (HelpForm.GetLastSelected(primaryTable) != null) {
                CallMethod("BeforeFill");
                if (ErroreIrrecuperabile) {
                    DrawState = saved;
                    return;
                }
            }

            if (container == null) container = linkedForm.Controls;
            helpForm.FillControls(container);
            FreshToolBar();
            if (HelpForm.GetLastSelected(primaryTable) != null) {
                CallMethod("AfterFill");
                if (ErroreIrrecuperabile) {
                    DrawState = saved;
                    return;
                }
            }

            DrawState = saved;
        }


        /// <summary>
        /// Calls MetaData_AfterRowSelect
        /// </summary>
        /// <param name="T"></param>
        /// <param name="R"></param>
        public void afterRowSelect(DataTable T, DataRow R) {
            if (ErroreIrrecuperabile) return;
            if (isClosing) return;
            if (destroyed) return;
            if (R != null && R.RowState == DataRowState.Detached) R = null;
            var formType = linkedForm.GetType();
            var formMethod = formType.GetMethod("MetaData_AfterRowSelect", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (formMethod == null) return;
            var handle = mdl_utils.MetaProfiler.StartTimer("AfterRowSelect()");
            bool savedLocked = locked;
            try {
                locked = true;
                formMethod.Invoke(linkedForm, new object[2] {T, R});
            }
            catch (Exception e) {
                var err = $"Errore chiamando il metodo AfterRowSelect su tabella {T.TableName}  del form {linkedForm.Name} {primaryTable}:\r\n {ErrorLogger.GetErrorString(e)}";
                shower.ShowException(linkedForm, err, e);
                ErroreIrrecuperabile = true;
                logError(err, e);
            }

            locked = savedLocked;
            mdl_utils.MetaProfiler.StopTimer(handle);
        }

        /// <summary>
        /// Used internally to safely call MetaData_BeforeRowSelect
        /// </summary>
        /// <param name="T"></param>
        /// <param name="R"></param>
        public void beforeRowSelect(DataTable T, DataRow R) {
            if (ErroreIrrecuperabile) return;
            Type FormType = linkedForm.GetType();
            var FormMethod =
                FormType.GetMethod("MetaData_BeforeRowSelect",  BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (FormMethod == null) return;

            int handle = mdl_utils.MetaProfiler.StartTimer("myBeforeRowSelect()");
            bool savedLocked = locked;
            try {
                locked = true;
                //MarkEvent("AfterRow Select on Table "+T.TableName);
                FormMethod.Invoke(linkedForm, new object[2] {T, R});

            }
            catch (Exception E) {
                var err = $"Errore calling BeforeRowSelect of form {primaryTable}:\r\n {E.ToString()}";
                shower.ShowException(linkedForm, err,E);
                ErroreIrrecuperabile = true;
                logError(err, E);
            }
            locked = savedLocked;
            mdl_utils.MetaProfiler.StopTimer(handle);

        }

        /// <summary>
        /// Invoke a method of the linked Form
        /// </summary>
        /// <param name="method"></param>
        public void CallMethod(string method) {
            if (ErroreIrrecuperabile || isClosing) return;
            if ((conn != null) && (conn.BrokenConnection)) {
                return;
            }

            Type FormType = linkedForm.GetType();
            MethodInfo FormMethod = FormType.GetMethod("MetaData_" + method,  BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);//| BindingFlags.Static
            if (FormMethod == null) return;
            int handle = mdl_utils.MetaProfiler.StartTimer(primaryTable + ":MetaData_" + method);
            try {
                FormMethod.Invoke(linkedForm, new Type[0]);

            }
            catch (Exception e) {
                if (conn == null || conn.BrokenConnection) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(LM.dbConnectionInterrupted,LM.ErrorTitle);
                    ErroreIrrecuperabile = true;
                    linkedForm.Close();
                    return;
                }

                string oggetto = "";

                if (primaryTable != null) {
                    oggetto = primaryTable.TableName;

                    if (primaryTable.Rows.Count == 0) {
                        oggetto += "(tab.vuota)";
                    }
                    else {
                        q key = null;
                        try {
                            key = q.keyCmp(primaryTable.Rows[0]);
                        }
                        catch {
                            // ignored
                        }

                        oggetto += key;
                    }
                }


                string err = $"Errore calling  {method} of entity {oggetto}:\r\n{e}";
                if (e.InnerException != null) err += $"\r\nInner:\r\n{e.InnerException}";
                logError(err, e);
                err += LM.windowMustBeClosed;
                CanSave = false;
                CanInsert = false;
                CanCancel = false;
                SearchEnabled = false;
                CanInsertCopy = false;
                ErroreIrrecuperabile = true;
                shower.ShowException(linkedForm,err, e);
            }

            mdl_utils.MetaProfiler.StopTimer(handle);

        }

        void logError(string message, Exception e) {
            errorLogger.logException(message, e, meta: meta);
        }

        void Cmb_EnabledChanged(object sender, EventArgs e) {
	        setColor(sender as Control);
        }

        private void paintGbox(object o, PaintEventArgs p) {
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
        private  void Tb_DrawItem(object sender, DrawItemEventArgs e) {
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
      
        private bool eventAssigned = false;

        /// <summary>
        /// Set custom colors for a control, eventually recursively
        /// </summary>
        /// <param name="c"></param>
        /// <param name="recursive"></param>
        public void setColor(Control c, bool recursive) {
	        if (!IsRealClear) return;
	        setColor(c);
	        if (!recursive || !c.HasChildren) return;
	        foreach (Control cc in c.Controls) {
		        setColor(cc, true);
	        }

	        if (c is Form) {
		        eventAssigned = true;
	        }
        }

		  /// <summary>
        /// Set custom color for a specific Control
        /// </summary>
        /// <param name="c"></param>
        private void setColor(Control c) {
	        var s = mdl_utils.MetaProfiler.StartTimer($"SetColorCtrl * {c?.GetType()}");
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
                if (!eventAssigned) {
	                tb.DrawItem -= Tb_DrawItem;
	                tb.DrawItem += Tb_DrawItem;
                }

                mdl_utils.MetaProfiler.StopTimer(s);
                return;
            }

            if (c is GroupBox g) {
                g.ForeColor = formcolors.MainForeColor();
                if (!eventAssigned) {
	                g.Paint -= paintGbox;
	                g.Paint += paintGbox;
                }

                if (g.Tag != null && g.Tag.ToString().ToLower().StartsWith("auto")) {
                    g.BackColor = formcolors.AutoChooseBackColor();
                }
                else {
                    g.BackColor = formcolors.MainBackColor();
                }

                //g.Refresh();
                mdl_utils.MetaProfiler.StopTimer(s);

                return;
            }

            if (c is Button b) {
                if (b.Enabled) {
                    string Tag = GetStandardTag(b.Tag);
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
                }
                else {
                    b.BackColor = formcolors.DisabledButtonBackColor();
                    b.ForeColor = formcolors.DisabledButtonForeColor();
                }
                if (!eventAssigned) {
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
                    //cmb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cmb.BackColor = formcolors.TextBoxReadOnlyBackColor();
                    cmb.ForeColor = formcolors.TextBoxReadOnlyForeColor();
                }

                if (!eventAssigned) {
	                cmb.EnabledChanged -= Cmb_EnabledChanged;
	                cmb.EnabledChanged += Cmb_EnabledChanged;
                }

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

                if (!eventAssigned) {
	                T.EnabledChanged -= Cmb_EnabledChanged;
	                T.EnabledChanged += Cmb_EnabledChanged;
                }

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

                if (!eventAssigned) {
	                T.EnabledChanged -= Cmb_EnabledChanged;
	                T.EnabledChanged += Cmb_EnabledChanged;
                }

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
            if (c is DataGridView gv) {
	            gv.BackgroundColor = formcolors.GridBackgroundColor();
	            mdl_utils.MetaProfiler.StopTimer(s);
	            return;
            }

            c.BackColor = formcolors.MainBackColor();
            c.ForeColor = formcolors.MainForeColor();
            mdl_utils.MetaProfiler.StopTimer(s);
        }

        /// <summary>
        /// Clear form controls and empty data
        /// </summary>
        public void Clear() {
            int handle = mdl_utils.MetaProfiler.StartTimer("Clear()");
            helpForm.lastTextNoFound = "";
            formState = form_states.setsearch;
            DrawState = form_drawstates.clearing;
            metaModel.AllowAllClear(ds);
            getData.ClearTables();
            //if (comboboxtorefilter) helpForm.comboBoxToRefilter = true;
            helpForm.ClearForm(linkedForm);
            //if (comboboxtorefilter) helpForm.comboBoxToRefilter = false;
            HelpForm.SetLastSelected(primaryTable, null);
            lastSelectedRow = null;
            if (IsRealClear) {
                FreshToolBar();
                setFormText();
            }

            //UnsavedChanges=false;
            entityChanged = false;
            //StartFilter=null;
            CallMethod("AfterClear");
            DrawState = form_drawstates.done;
            mdl_utils.MetaProfiler.StopTimer(handle);
        }

        /// <summary>
        /// Called when a row is selected form a list, should fill the mainform 
        ///  subsequently. In case of a list-form, entity table should not be cleared
        /// R is the row from which start the filling of the form  - does not belong to DS
        /// </summary>
        /// <param name="r"></param>
        /// <param name="listType"></param>
        public void SelectRow(DataRow r, string listType) {
            if (ErroreIrrecuperabile) return;
            if (isClosing) return;
            if (destroyed) return;
            if (r == null) return;
            if (primaryTable == null) return;
            var handle = mdl_utils.MetaProfiler.StartTimer("SelectRow(R,Listtype)");
            try {
                if (isList) {
                    if (isTree) TreeSelectRow(r, listType);
                    return;
                }

                formState = form_states.setsearch;
                DrawState = form_drawstates.clearing;
                getData.ClearTables();
                GoingToEditMode = true;
                eventManager.dispatch(new StartClearMainRowEvent());
                Clear(); //(true)
                CallMethod("AfterClear");
                eventManager.dispatch(new StopClearMainRowEvent());
                if (ErroreIrrecuperabile) return;

                GoingToEditMode = false;

                getData.SearchByKey(r).GetAwaiter().GetResult();
                if (primaryTable.Rows.Count == 0) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(LM.selectedRowNotPresent);
                    //Clear(true);
                    return;
                }

                eventManager.DisableAutoEvents();
                DO_GET(false, null);
                eventManager.EnableAutoEvents();

                if ((conn != null) && (conn.BrokenConnection)) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(LM.dbConnectionInterrupted);
                    ErroreIrrecuperabile = true;
                    return;
                }

                HelpForm.SetLastSelected(primaryTable, primaryTable.Rows[0]);

                //formType = MetaData.form_types.main;
                //IsList=false;	  //????
                formState = form_states.edit; //last selected row is not null
                entityChanged = false;
                //R.Delete();
                firstFillForThisRow = true;
                eventManager.dispatch(new StartMainRowSelectionEvent(primaryTable.Rows[0]));
                //helpForm.comboBoxToRefilter = true;
                ReFillControls();
                eventManager.dispatch(new StopMainRowSelectionEvent(primaryTable.Rows[0]));
                //helpForm.comboBoxToRefilter = false;
                firstFillForThisRow = false;
                DrawState = form_drawstates.done;
            }
            finally {
                mdl_utils.MetaProfiler.StopTimer(handle);
            }
        }

        /// <summary>
        /// Gets form data, starting from primary table.
        /// </summary>
        /// <param name="onlyperipherals">if true, only pheriperals table are read,
        ///  i.e primary table and childs of primary table are not read</param>
        /// <param name="oneRow">if true, data getting only considers one row 
        ///  of primary table</param>
        public virtual void DO_GET(bool onlyperipherals, DataRow oneRow) {
            var handle = mdl_utils.MetaProfiler.StartTimer("DO_GET");
            if (!onlyperipherals) CallMethod("BeforeGet");
            try {
                if (helpForm != null) eventManager.DisableAutoEvents();
                getData.ReadCached();
                getData.Get(onlyperipherals, oneRow);
                if (helpForm != null) eventManager.EnableAutoEvents();
            }
            catch (Exception E) {
                ErroreIrrecuperabile = true;
                errorLogger.MarkEvent(ErrorLogger.GetErrorString(E));
                shower.ShowException(linkedForm, "Error",E);
            }

            mdl_utils.MetaProfiler.StopTimer(handle);
        }

        /// <summary>
        /// Called when a node in a tree form is selected
        /// </summary>
        /// <param name="R"></param>
        /// <param name="listType"></param>
        public void TreeSelectRow(DataRow R, string listType) {

            if (R == null) return;


            var handle = mdl_utils.MetaProfiler.StartTimer("TreeSelectRow");
            eventManager.DisableAutoEvents();
            try {
                HelpForm.SetLastSelected(primaryTable, null);

                var tm = TreeViewManager.GetManager(primaryTable); //was R.Table but that did not have a manager
                if (tm == null) return;

                var r = tm.selectRow(R, listType);
                formState = form_states.edit;
                eventManager.EnableAutoEvents();
                helpForm.extendedControlChanged(helpForm.mainTableSelector, null, r);

                entityChanged = false;
                setFormText();
                FreshToolBar();

            }
            finally {
                mdl_utils.MetaProfiler.StopTimer(handle);

            }

        }

        /// <summary>
        /// Set the caption for this form
        /// </summary>
        public void setFormText() {
            if (linkedForm == null) return;
            string descr = "";
            switch (formState) {
                case form_states.edit:
                    descr = "Modifica";
                    break;
                case form_states.insert:
                    descr = "Inserimento";
                    break;
                case form_states.setsearch:
                    descr = "Ricerca";
                    break;
            }            
            linkedForm.Text = meta.getName() + " (" + descr + ") ";
        }


        /// <summary>
        /// Fill controls of a form
        /// </summary>
        public void ReFillControls() {
            if (linkedForm==null)return;
            
            int handle = mdl_utils.MetaProfiler.StartTimer("ReFillControls");
            setFormText();
            try {
                ReFillControls(linkedForm.Controls);
            }
            catch (Exception E) {
                ErroreIrrecuperabile = true;
                errorLogger.logException("error in RefillControls()", E);
                shower.ShowException(linkedForm, LM.errorShowingForm, E);
            }

            mdl_utils.MetaProfiler.StopTimer(handle);
        }

        /// <summary>
        /// True if an unrecoverable error occurred
        /// </summary>
        public bool ErroreIrrecuperabile { get; set; }

        /// <summary>
        /// Refills the form. If RefreshPeripherals is set to true, secondary tables
        ///  are read again from DB (i.e. all tables in the view that are not
        ///  cached, primary or child of primary).
        /// </summary>
        /// <param name="refreshPeripherals">when true, not -entity-or-cached- tables are cleared and read again from DB</param>
        /// <param name="doPrefill">When true, also prefill is done, this is more expensive and should be done only once in a form</param>
        public virtual void FreshForm(bool refreshPeripherals=true, bool doPrefill=false) {
            if (linkedForm==null)return;
            var saved = DrawState;
            DrawState = form_drawstates.filling;
            //gets all pheripherals tables
            var r = HelpForm.GetLastSelected(primaryTable);
            if (refreshPeripherals) {
                getData.Get(true, r); //fresh peripherals table, not entity tables
                if (ErroreIrrecuperabile) {
                    DrawState = saved;
                    return;
                }
            }

            if (doPrefill) {
                Do_Prefill();
            }

            //if (RefreshPeripherals) helpForm.comboBoxToRefilter = true;
            if (formDetailControl == null || (isList == false)) {
                ReFillControls();
            }
            else {
                ReFillControls(formDetailControl.Controls);
            }

            //if (RefreshPeripherals) helpForm.comboBoxToRefilter = false;
            DrawState = saved;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Do_Prefill() {
            //int doprefill=metaprofiler.StartTimer("Do_Prefill()");
            var saved = DrawState;
            DrawState = form_drawstates.prefilling;
            conn.Open();
            try {
                helpForm.PreFillControls(linkedForm);
            }
            catch (Exception E) {
                errorLogger.logException("Do_Prefill():"+LM.errorShowingForm, E, meta: meta);
                ErroreIrrecuperabile = true;
            }

            conn.Close();
            DrawState = saved;
            //metaprofiler.StopTimer(doprefill);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tablename"></param>
        public void Do_Prefill(string tablename) {
            var saved = DrawState;
            DrawState = form_drawstates.prefilling;
            conn.Open();
            try {
                helpForm.PreFillControls(linkedForm, tablename);
            }
            catch (Exception e) {
                errorLogger.logException($"Do_Prefill({tablename}):"+LM.errorShowingForm, e, meta: meta);
                ErroreIrrecuperabile = true;
            }

            conn.Close();
            DrawState = saved;
        }

        /// <summary>
        /// True if form is being closed
        /// </summary>
        public bool isClosing {
            get { return _isClosing; }
            set {
                _isClosing = value;
                //if (meta.isClosing != value) meta.isClosing = value;
            }
        }

        /// <summary>
        /// Managed form
        /// </summary>
        public Form linkedForm {
            get { return _linkedForm; }
        }

        /// <summary>
        /// Ignores any incoming command to this form
        /// </summary>
        public bool locked { get; set; }

        /// <summary>
        /// True  the  first time an AfterFill is invoked on a certain row  
        /// </summary>
        public bool firstFillForThisRow { get; set; }

        /// <summary>
        /// True if an insert is coming after the clear
        /// </summary>
        public bool GointToInsertMode { get; set; }

        /// <summary>
        /// True if an edit is coming after the clear
        /// </summary>
        public bool GoingToEditMode { get; set; }

        /// <summary>
        /// True if  no insert or edit are coming after the clear
        /// </summary>
        public bool IsRealClear {
            get {
                return !(GointToInsertMode || GoingToEditMode);
            }
        }


        //bool empty;
        /// <summary>
        /// true if the MetaData object has not been filled
        /// </summary>
        public bool IsEmpty {
            get {
                if (primaryTable == null) return true;
                if (primaryTable.Rows.Count == 0) return true;
                return (_hiddenformState == form_states.setsearch);
            }
        }


        

        form_states _hiddenformState;

        /// <summary>
        /// Current form state
        /// </summary>
        public form_states formState {
            get { return _hiddenformState; }
            set {
                _hiddenformState = value;
                if (value == form_states.insert) eventManager.dispatch(new ChangeFormState(ApplicationFormState.Insert));
                if (value == form_states.edit)   eventManager.dispatch(new ChangeFormState(ApplicationFormState.Edit));
                if (value == form_states.setsearch) eventManager.dispatch(new ChangeFormState(ApplicationFormState.Empty));
                updateHelpFormState();
            }
        }

        void updateHelpFormState() {
            if (helpForm == null) return;
            if (formState == form_states.insert) helpForm.drawMode = HelpForm.drawmode.insert;
            if (formState == form_states.edit) helpForm.drawMode = HelpForm.drawmode.edit;
            if (formState == form_states.setsearch) helpForm.drawMode = HelpForm.drawmode.setsearch;
        }

        ///// <summary>
        ///// Current operation runned
        ///// </summary>
        //public MetaData.mainoperations curroperation {
        //    get { return meta.currOperation; }
        //    set { meta.currOperation = value; }
        //}

        public mainoperations curroperation { get; set; }

        /// <summary>
        /// Prefill combo and other one-time operations
        /// </summary>
        public void prefillControls() {
            var saved = DrawState;
            DrawState = form_drawstates.prefilling;
            conn.Open();
            try {
                helpForm.PreFillControls(linkedForm);
            }
            catch (Exception e) {
                errorLogger.logException("prefillControls():"+LM.errorShowingForm, e, meta: meta);
                ErroreIrrecuperabile = true;
            }

            conn.Close();
            DrawState = saved;
        }

        /// <summary>
        /// Current Draw state of the form
        /// </summary>
        public form_drawstates DrawState { get; set; }

        /// <summary>
        /// True when operation is actived by the user and not inside a fill
        /// </summary>
        public bool DrawStateIsDone => (DrawState == form_drawstates.done) && (!isClosing);


        /// <summary>
        /// True when form is in insert mode (NOT EDIT!!)
        /// </summary>
        public bool InsertMode {
            get {
                if (IsEmpty) return false;
                return (_hiddenformState == form_states.insert);
            }
        }

        /// <summary>
        /// True when form is in "edit mode" (not INSERT!)
        /// </summary>
        public bool EditMode {
            get {
                if (IsEmpty) return false;
                return (_hiddenformState == form_states.edit);
            }
        }


        ToolBar getToolBar() {
            if (linkedForm == null) return null;
            var tb = MainToolBarManager.GetToolBar(linkedForm);
            var owner = linkedForm.IsMdiChild ? linkedForm.ParentForm : linkedForm.Owner;
            if (linkedForm.Modal) owner = null;

            if ((tb == null) && (owner != null)) {
                tb = MainToolBarManager.GetToolBar(owner);
            }

            return tb;
        }

        /// <summary>
        /// Gets the current toolbar manager
        /// </summary>
        /// <returns></returns>
        public IMainToolBarManager getToolBarManager() {
            var tb = getToolBar();
            return tb == null ? null : MainToolBarManager.GetToolBarManager(tb);
        }

        /// <summary>
        /// unlink the current toolbar
        /// </summary>
        public void UnlinkToolBar() {
            //MarkEvent("UnlinkToolBar");
            var tb = getToolBar();
            if (tb == null) return;
            var tm = MainToolBarManager.GetToolBarManager(tb);
            tm.unlink(meta);
        }

      

        /// <summary>
        /// Refreshes toolbar basing it to this MetaData linked form
        /// </summary>
        public void FreshToolBar() {
            if (!formInited && !isList) return;
            var handle = mdl_utils.MetaProfiler.StartTimer("FreshToolBar");
            try {
                var tb = getToolBar();
                if (tb == null) return;
                var tm = MainToolBarManager.GetToolBarManager(tb);
                tm.linkTo(meta, this);
                //TM.FreshButtons(); already in linkTo
            }
            finally {
                mdl_utils.MetaProfiler.StopTimer(handle);
            }
        }


        private bool _isClosing = false;



        /// <summary>
        /// Calls the appropriate IsValid method on R and if not valid cancels form closing
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        bool manageValidResult(DataRow r) {
            if (isClosing) return false;
            if (ErroreIrrecuperabile) {
                return false;
            }

            var metaToCheck = r.Table == primaryTable ? meta : dispatcher.Get(r.Table.TableName);

            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm, LM.errorLoadingMeta(r.Table.TableName),null);
                return false;
            }

            var valid = metaToCheck.IsValid(r, out string errmess, out string errfield);
            if (valid) return true;
            if (errfield != null) {
                var tag = errfield;
                if (tag.IndexOf('.') < 0) tag = $"{r.Table.TableName}.{errfield}";
                HelpForm.FocusField(linkedForm, tag);
            }

            if (!string.IsNullOrEmpty(errmess)) MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, errmess);
            linkedForm.DialogResult = System.Windows.Forms.DialogResult.None;
            ReFillControls();
            return false;
        }

        void createParentIndexes(IIndexManager idm, DataTable t) {
	        foreach (DataRelation r in t.ChildRelations) {
		        if (MetaModel.IsSubEntityRelation(r)) continue;
		        idm.createPrimaryKeyIndex(r.ParentTable);
	        }
        }
        void createSubEntityIndexes(DataTable primary) {
	        var idm = primary?.DataSet.getIndexManager();
	        foreach (DataRelation rel in primary.ChildRelations) {
		        if (!GetData.CheckChildRel(rel)) continue;
		        var keys = (from c in rel.ChildColumns select c.ColumnName).ToArray();
		        if (idm.hasIndex(rel.ChildTable, keys)) continue;
		        idm.addIndex(rel.ChildTable, new MetaTableNotUniqueIndex(rel.ChildTable,keys));
		        createSubEntityIndexes(rel.ChildTable);
		        createParentIndexes(idm, rel.ChildTable);
	        }
        }

        /// <summary>
        /// Takes values for the Source Row from linked Form Data. The goal is to propagate to
        ///  the parent form the changes made (in LinkedForm) in this form
        /// </summary>
        /// <remarks>
        ///  Necessary condition is that FormDataSet does contain only one row of the same
        ///  table as SourceRow. This function can be redefined to implement additional operations
        ///  to do in SourceRow.Table when changes to SourceRow are accepted. 
        ///  </remarks>
        ///  <returns>true when operation successfull</returns>
        public virtual bool GetSourceChanges() {
            if (!isSubentity) return true;
            //if (formType != MetaData.form_types.detail) return true;
            curroperation = mainoperations.save;
            CallMethod("BeforePost");
            if (ErroreIrrecuperabile) {
                curroperation = mainoperations.none;
                return false;
            }

            var sourceRow = meta.SourceRow; //collegato errore 5176
            meta.NewSourceRow = sourceRow; //temporary value

            if (isList) return false; //it should never happen (a form-list can't be a subentity!)
            var unaliased = sourceRow.Table.tableForReading();
            var T = ds.Tables[unaliased];
            if (T == null) return true;
            if (T.Rows.Count != 1) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm, LM.moreThanOneRowInTable(T.TableName));
                return false;
            }


            var externalRow = T.Rows[0]; //Riga DI QUESTO FORM (OSSIA IL DETTAGLIO)

            var dsSource = sourceRow.Table.DataSet;
            var changes = MetaModel.xVerifyChangeChilds(sourceRow.Table, externalRow);
            if (!changes) changes = MetaModel.xVerifyChangeChilds(T, sourceRow);
            if (!changes) {
                metaModel.CalculateRow(sourceRow);
                if (sourceRow.IsFalseUpdate()) sourceRow.AcceptChanges();
                return true;
            }

            //Here should be done a backup of SourceRow before changing it, in order to
            // undo modification when needed.
            try {
                if (sourceRow.RowState == DataRowState.Added) {
                    var oldselector = RowChange.GetHashSelectors(T, sourceRow);
                    var newselector = RowChange.GetHashSelectors(T, externalRow);
                    if (oldselector != newselector) {
                        RowChange.CalcTemporaryID(externalRow, sourceRow.Table);
                    }

                    var filter = q.keyCmp(externalRow); //, DataRowVersion.Default, false);

                    var existentFound = sourceRow.Table.filter(filter);
                    if (existentFound.Length > 0) {
                        if (existentFound[0] != sourceRow) {
                            MetaFactory.factory.getSingleton<IMessageShower>().Show(LM.primaryKeyConflict,LM.adviceLabel);
                            return false;
                        }
                    }
                }
                // xCopy(DataSet source, DataSet dest, DataRow rSource, DataRow rDest) 
                meta.NewSourceRow = xCopy(ds, sourceRow.Table.DataSet, externalRow, sourceRow);
                meta.SourceRow = meta.NewSourceRow; //qui potrebbe porre a null il sourceRow

                CallMethod("AfterPost");
                if (ErroreIrrecuperabile) {
                    curroperation = mainoperations.none;
                    return false;
                }
            }
            catch (Exception e) {
                shower.ShowException(linkedForm, "GetSourceChanges():"+LM.wrongData, e);
                logError(meta.getName() + ".GetSourceChanges():"+LM.wrongData, e);
                return false;
            }


            entityChanged = true;

            return true;
        }

        /// <summary>
        /// Copia un DataRow da un DS ad un altro.
        /// Ipotesi abbastanza fondamentale è che RSource e RDest abbiano la stessa chiave, o perlomeno
        ///  che RSource non generi conflitti in Dest
        /// </summary>
        /// <param name="source">DataSet origine della copia</param>
        /// <param name="dest">Dataset di destinazione</param>
        /// <param name="rSource">Riga da copiare</param>                                                                         
        /// <param name="rDest">Riga di destinazione</param>
        private DataRow xCopy(DataSet source, DataSet dest, DataRow rSource, DataRow rDest) {
            var destIsInsert = (rDest.RowState == DataRowState.Added);
            xRemoveChilds(source, rDest);
            return xCopyChilds(dest, rDest.Table, source, rSource, destIsInsert);  //era xMoveChilds ma non si vede il motivo di rimuovere le righe dall'origine
        }

        /// <summary>
        /// Removes a Row with all his subentity childs. 
        /// Only considers tables of D inters. Rif
        /// </summary>
        /// <param name="rif">Referring DataSet. Tables not existing in this DataSet are not recursively scanned</param>
        /// <param name="rDest">DataRow to be removed with all subentities</param>
        /// <returns></returns>
        void xRemoveChilds(DataSet rif, DataRow rDest) {
            DataTable T = rDest.Table;
            foreach (DataRelation rel in T.ChildRelations) {
                if (!rif.Tables.Contains(rel.ChildTable.TableName)) continue;
                if (!GetData.CheckChildRel(rel)) continue; //not a subentityrel
                var childs = rDest.getChildRows(rel);
                foreach (var child in childs) {
                    xRemoveChilds(rif, child);
                }
            }

            rDest.Delete();
            if (rDest.RowState != DataRowState.Detached) rDest.AcceptChanges();
        }

       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="TDest"></param>
        /// <param name="rif"></param>
        /// <param name="rSource">Belongs to Rif</param>
        /// <param name="forceAddState"></param>
        DataRow xCopyChilds(DataSet dest, DataTable TDest, DataSet rif, DataRow rSource, bool forceAddState) {
            var T = rSource.Table;
            var resultRow = copyDataRow(TDest, rSource, forceAddState);

            foreach (DataRelation rel in T.ChildRelations) {
                if (!dest.Tables.Contains(rel.ChildTable.TableName)) continue;
                if (!GetData.CheckChildRel(rel)) continue; //not a subentityrel
                var childTable = rif.Tables[rel.ChildTable.TableName];
                dest.Tables[childTable.TableName].copyAutoIncrementPropertiesFrom(childTable);

                for (int i=0; i<childTable.Rows.Count;i++) {
                    var child = childTable.Rows[i];
                    xCopyChilds(dest, dest.Tables[childTable.TableName], rif, child, false);
                }
            }
         
            return resultRow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destTable"></param>
        /// <param name="toCopy"></param>
        /// <param name="forceAddState"></param>
        /// <returns></returns>
        DataRow copyDataRow(DataTable destTable, DataRow toCopy, bool forceAddState) {
            var dest = destTable.NewRow();
            DataRowVersion toConsider = DataRowVersion.Current;
            if (toCopy.RowState == DataRowState.Deleted) toConsider = DataRowVersion.Original;
            if (toCopy.RowState == DataRowState.Modified) toConsider = DataRowVersion.Original;
            if (toCopy.RowState != DataRowState.Added && !forceAddState) {
                foreach (DataColumn c in destTable.Columns) {
                    if (destTable.Columns[c.ColumnName].ReadOnly) continue;
                    if (toCopy.Table.Columns.Contains(c.ColumnName)) {
                        dest[c.ColumnName] = toCopy[c.ColumnName, toConsider];
                    }
                }

                destTable.Rows.Add(dest);
                dest.AcceptChanges();
            }

            if (toCopy.RowState == DataRowState.Deleted) {
                dest.Delete();
                return dest;
            }

            foreach (DataColumn c in destTable.Columns) {
                if (destTable.Columns[c.ColumnName].ReadOnly) continue;
                if (toCopy.Table.Columns.Contains(c.ColumnName)) {
                    dest[c.ColumnName] = toCopy[c.ColumnName];
                }
            }

            if ((toCopy.RowState == DataRowState.Modified || toCopy.RowState == DataRowState.Unchanged)
                && !forceAddState) {
                metaModel.CalculateRow(dest);
                if (dest.IsFalseUpdate()) dest.AcceptChanges();
                return dest;
            }

            //Vede se nella tab. di dest. c'è una riga cancellata che matcha
            var filter = q.keyCmp(toCopy);
            var deletedFound = destTable.filter(filter, all:true).Where(r=>r.RowState == DataRowState.Deleted).ToArray();
            if (deletedFound.Length == 1) {
                dest.BeginEdit();
                destTable.Columns._forEach(c => {
                    if (c.ReadOnly) return;
                    dest[c.ColumnName] = deletedFound[0][c.ColumnName, DataRowVersion.Original];
                });


                //RowChange.CalcTemporaryID(SourceRow);
                dest.EndEdit();

                //Elimina la riga cancellata dal DataSet
                deletedFound[0].AcceptChanges();

                //Considera la riga sorgente non più cancellata	ma invariata
                destTable.Rows.Add(dest);
                dest.AcceptChanges();

                foreach (DataColumn CC in destTable.Columns) {
                    if (destTable.Columns[CC.ColumnName].ReadOnly) continue;
                    if (toCopy.Table.Columns.Contains(CC.ColumnName)) {
                        dest[CC.ColumnName] = toCopy[CC.ColumnName, DataRowVersion.Current];
                    }
                }

                metaModel.CalculateRow(dest);
                if (dest.IsFalseUpdate()) dest.AcceptChanges();
                return dest;
            }

            destTable.Rows.Add(dest);
            metaModel.CalculateRow(dest);
            return dest;

        }

        ///// <summary>
        ///// Save data to db
        ///// </summary>
        //public void SaveFormData() {
        //    meta.SaveFormData();
        //}



        /// <summary>
        /// Save all changes made on the DataSet to DB. This is invoked when user clicks 
        ///  "save" button or "Ok" button. Infact, both those buttons have a
        ///   "mainsave" tag.
        /// </summary>
        public void SaveFormData() {
            //if (!GetFormData(false))return; //error during data getting
            if (ErroreIrrecuperabile) return;
            if (dispatcher.unrecoverableError) return;
            curroperation = mainoperations.save;
            if (!isSubentity) {
                CallMethod("BeforePost");
                if (ErroreIrrecuperabile) {
                    curroperation = mainoperations.none;
                    return;
                }
            }

            var last = HelpForm.GetLastSelected(primaryTable);

            var wasadelete = (last == null) || (last.RowState == DataRowState.Deleted);

            var res = true;
            if (isSubentity) {
                entityChanged = true;
            }
            else {
                if (ds.HasChanges()) {
                    var postD = meta.Get_PostData();
                    var err = postD.InitClass(ds, conn).GetAwaiter().GetResult();
                    res = false;
                    if (err == null) res = postD.InteractiveSaveData();
                    if (res) entityChanged = true;
                }
            }

            last = HelpForm.GetLastSelected(primaryTable);

            if (res) {
                if (!isSubentity) {
                    CallMethod("AfterPost");

                    if (ErroreIrrecuperabile) {
                        curroperation = mainoperations.none;
                        return;
                    }
                }

                if (isSubentity && (MainSelectionEnabled == false)) {
                    linkedForm.DialogResult = System.Windows.Forms.DialogResult.OK;
                    curroperation = mainoperations.none;
                    return;
                }

                metaModel.AllowAllClear(ds);
                if (last == null) {
                    //It was a successfully delete
                    HelpForm.SetLastSelected(primaryTable, null);
                    if (!isList) {
                        eventManager.dispatch(new StartClearMainRowEvent());
                        Clear(); //(true)
                        eventManager.dispatch(new StopClearMainRowEvent());
                        curroperation = mainoperations.none;
                        return;
                    }

                    if (isTree) {
                        var tv = (TreeView) helpForm.mainTableSelector;
                        var TN = tv.SelectedNode;
                        if (TN != null) {
                            var tn = (tree_node) TN.Tag;
                            if ((tn != null) && (tn.Row.RowState == DataRowState.Detached)) {
                                var tm = TreeViewManager.GetManager(primaryTable);
                                tm.DeleteCurrentNode();
                            }
                            else {
                                beforeSelectTreeManager(tv, null);
                                afterSelectTreeManager(tv, null);
                            }
                        }

                        var curr = HelpForm.GetLastSelected(primaryTable);
                        if (curr == null) {
                            var saved = doingCommand;
                            doingCommand = false;
                            DoMainCommand("mainsetsearch");
                            doingCommand = saved;
                        }

                        curroperation = mainoperations.none;
                        return;
                    }

                    //clears data from entity Controls
                    selectARowInGridList();
                    curroperation = mainoperations.none;
                    return;
                }

                if (formState == form_states.insert) {
                    if (isTree) {
                        formState = form_states.edit; //must be done BEFORE changing tree selected node
                        var tv = (TreeView) helpForm.mainTableSelector;
                        var tm = TreeViewManager.GetManager(primaryTable);
                        var TN = tv.SelectedNode;
                        var newNode = tm.AddRow(TN, last);
                        newNode.EnsureVisible();
                        //TM.AutoEventsEnabled=false;
                        suspendListManager = true;
                        tv.SelectedNode = newNode;
                        suspendListManager = false;
                        //TM.AutoEventsEnabled=true;
                    }
                    else {
                        formState = form_states.edit; //it was an insert or update		
                        eventManager.dispatch(new StartMainRowSelectionEvent(last));
                        FreshForm(true, false);
                        var g = (DataGrid) helpForm.mainTableSelector;
                        helpForm.SetGridCurrentRow(g, last);
                        eventManager.dispatch(new StopMainRowSelectionEvent(last));
                        last = null;
                    }
                }

                formState = form_states.edit; //it was an insert or update					
            }
            else {
                //an error occurred or user canceled operation
                if (wasadelete) {
                    //il reject viene fatto dal chiamante
	                //var idm = ds.getIndexManager();
                    //var modifiedTable = (from DataTable t in ds.Tables where t.HasChanges() select t).ToArray();
                    //idm?.suspend(false,modifiedTable);
                    //ds.RejectChanges();
                    //idm?.resume(false,modifiedTable);

                    //ds.RejectChanges(); //a seguito del task 9394 penso che il commento sopra sia corretto, non si può
                    //annullare solo la riga padre, va annullato tutto altrimenti si rischia di cancellare solo la riga padre
                    // perchè potrebbe essere in stato di inserimento mentre le figlie sarebbero salvate senza padre,
                    // come è effettivamente accaduto
                    last = null;
                }

                linkedForm.DialogResult = System.Windows.Forms.DialogResult.None;
                //Chi imposta lo stato a SetSearch???? 
            }

            if (last != null) FreshForm(true, false); //21/1/2003
            curroperation = mainoperations.none;
        }


        void selectARowInGridList() {
            helpForm.SetDataRowRelated(linkedForm, primaryTable, null);
            HelpForm.SetLastSelected(primaryTable, null);
            if (primaryTable.Rows.Count > 0) {
                var g = (DataGrid) helpForm.mainTableSelector;
                if (g.CurrentRowIndex != 0 && g.DataSource != null) {
                    g.CurrentRowIndex = 0; //PERCHE' NON SCATTA CONTROLCHANGED???
                }
                else {
                    helpForm.ControlChanged(g, null);
                }

                formState = form_states.edit; //it was an insert or update					
                firstFillForThisRow = true;
                eventManager.dispatch(new StartClearMainRowEvent());
                FreshForm(true, false); //Per fare scattare l'AfterFill()
                eventManager.dispatch(new StopClearMainRowEvent());
                firstFillForThisRow = false;
            }
            else {
                eventManager.dispatch(new StartClearMainRowEvent());
                Clear();
                eventManager.dispatch(new StopClearMainRowEvent());
            }
        }



        /// <summary>
        /// Gets data from linked Form control, returning false if some errors occured
        /// </summary>
        /// <param name="quick">true if no validity checks have to be made</param>
        /// <returns>true on success</returns>
        public bool GetFormData(bool quick) {
            if (formState == form_states.setsearch) return true;

            var primaryDataRow = HelpForm.GetLastSelected(primaryTable);

            if (isList) {
                if (primaryDataRow == null) return true;
            }

            if (primaryDataRow == null) {
                return true;
            }

            if (quick) {
                helpForm.GetControls(linkedForm);
                CallMethod("AfterGetFormData");
                return !ErroreIrrecuperabile;
            }

            helpForm.GetControls(linkedForm);
            CallMethod("AfterGetFormData");
            if (ErroreIrrecuperabile)
                return false;

            var valid = manageValidResult(primaryDataRow);
            if (!valid) return false;
            if (isSubentity) {
                //GetSourceChanges also sets EntityChanged when needed
                
                if (!GetSourceChanges()) {
                    linkedForm.DialogResult = System.Windows.Forms.DialogResult.None;
                    ReFillControls();
                    return false;
                }
            }

            foreach (var subentity in helpForm.getExtraEntities()) {
                var subTable = ds.Tables[subentity];
                if (subTable.Select().Length == 0)
                    continue; //There is no sub-entity.  This is not considered a problem.
                var subEntityRow = HelpForm.GetCurrChildRow(primaryDataRow, subTable);
                if (subEntityRow == null) {
                    var err = $"More than one row present in table \'{subentity}\'. Can\'t validate update";
                    errorLogger.MarkEvent(err);
                    logError(err, null);
                    continue;
                }

                valid = manageValidResult(subEntityRow);
                if (!valid) return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if there are unsaved changes
        /// </summary>
        /// <returns></returns>
        public bool HasUnsavedChanges() {
            var handle = mdl_utils.MetaProfiler.StartTimer("HasUnsavedChanges()");
            try {
                GetFormData(true); //gets data without checks  
                return metaModel.HasChanges(primaryTable, meta.SourceRow, isSubentity);
            }
            finally {
                mdl_utils.MetaProfiler.StopTimer(handle);
            }
        }

        /// <summary>
        /// Dsiplays a message and stop form closing if ther are unsaved changes
        /// </summary>
        /// <returns></returns>
        public virtual bool warnUnsaved() {
            if (isList && HelpForm.GetLastSelected(primaryTable) == null) return true;
            if (linkedForm.DialogResult == wDialogResult.OK) return true;
            if (!HasUnsavedChanges()) return true;
            if (InsertMode && DontWarnOnInsertCancel) {
                return true;
            }
            
            var res = MetaFactory.factory.getSingleton<IMessageShower>().Show(linkedForm,LM.unsavedDataWarn,
                LM.adviceLabel,mdl.MessageBoxButtons.YesNo);
            if (res == mdl.DialogResult.Yes) {
                return true;
            }

            linkedForm.DialogResult = wDialogResult.None;
            return false;

        }

        bool _tryingToSelectRow;

        /// <summary>
        /// Tries to select a row in the Form. Checks for unsaved data, and also
        ///  verifies whether the row is selectable (CanSelect(R,listtype)==true).
        /// </summary>
        /// <param name="r">Row to select</param>
        /// <param name="listtype">listing type used for Selectability Check</param>
        /// <returns>true if Row has been selected, false otherwise</returns>
        public bool TryToSelectRow(DataRow r, string listtype) {

            if (linkedForm == null) return true;
            if (linkedForm.IsDisposed) return false;
            if (_tryingToSelectRow) return false;
            _tryingToSelectRow = true;
            if (!warnUnsaved()) {
                _tryingToSelectRow = false;
                return false;
            }

            if (!meta.CanSelect(r)) {
                _tryingToSelectRow = false;
                return false;
            }

            SelectRow(r, listtype);
            _tryingToSelectRow = false;
            return true;
        }



        /// <summary>
        /// If possible, makes PrimaryEntity child (or other subentity) of R (of table T) 
        /// </summary>
        /// <param name="r">Possible Parent Row (can be null)</param>
        /// <param name="T">DataTable to which R belongs (can't be null)</param>
        /// <param name="relname">relation to use between PrimaryTable and T</param>
        public void makeChild(DataRow r, DataTable T, string relname) {
            //			if (IsList) return;
            //			if (IsATree) return;
            var primary = HelpForm.GetLastSelected(primaryTable);
            if (primary == null) {
                //MetaFactory.factory.getSingleton<IMessageShower>().Show(LinkedForm,"No primary data selected");
                return;
            }

            //            if (PrimaryDataTable.Rows.Count!=1) return;
            //            DataRow primary = PrimaryDataTable.Rows[0];
            if (primary.MakeChildByRelation(r, relationName:relname)) return;
            foreach (var extraName in helpForm.getExtraEntities()) {
                var extraTable = ds.Tables[extraName];
                var rel = QueryCreator.GetParentChildRel(primaryTable, extraTable);
                if (rel == null) continue;
                var childRow = primary.getChildRows(rel);
                if (childRow.Length != 1) continue;
                if (childRow[0].MakeChildOf(r)) return;
            }
        }





        /// <summary>
        /// If possible, makes R child of current PrimaryEntity 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="relname"></param>
        public void CheckEntityChildRowAdditions(DataRow r, string relname) {

            if (r == null) return;
            var primary = HelpForm.GetLastSelected(primaryTable);
            if (primary == null) {
                //MetaFactory.factory.getSingleton<IMessageShower>().Show(LinkedForm,"No primary data selected");
                return;
            }

            if (r.MakeChildByRelation(primary, primary.Table,  relname)) {
                if (relname == null) {
                    metaModel.AddNotEntityChild(primary.Table, r.Table);
                }
                else {
                    metaModel.AddNotEntityChild(r.Table, relname);
                }
            }
        }

        /// <summary>
        /// Optimized version to unlink a set of rows
        /// </summary>
        /// <param name="toUnlink"></param>
        public virtual void unlinkMultipleRows(List<DataRow> toUnlink) {
            if (toUnlink == null) return;
            if (toUnlink.Count == 0) return;
            var linkedTable = toUnlink[0].Table;
            foreach (DataRow r in toUnlink) {
                if (metaModel.UnlinkDataRow(primaryTable, r) == null) continue;
            }
          

            if (linkedTable.Rows.Count > 0) {
                metaModel.AddNotEntityChild(primaryTable, linkedTable);
            }
            else {
                metaModel.UnMarkTableAsNotEntityChild(linkedTable);
            }

            helpForm.SetDataRowRelated(linkedForm, linkedTable, null);

            FreshForm(true, false);

        }

        /// <summary>
        /// Unlinks a specified row and set/unset the table as entitychild consequently. 
        /// Invoked during a Unlink_Grid_Row grid command
        /// </summary>
        /// <param name="r">Row to unlink</param>
        /// <returns>Unliked row or null if action canceled</returns>
        public virtual DataRow unlink(DataRow r) {
            if (r == null) return null;
            var linkedTable = r.Table;
            if (metaModel.UnlinkDataRow(primaryTable, r) == null) return null;

            if (linkedTable.Rows.Count > 0) {
                metaModel.AddNotEntityChild(primaryTable, linkedTable);
            }
            else {
                metaModel.UnMarkTableAsNotEntityChild(linkedTable);
            }


            helpForm.SetDataRowRelated(linkedForm, linkedTable, null);

            //UnsavedChanges=true;			
            FreshForm(true, false);
            //FreshForm(SourceTable.TableName); //21/1/2003
            return r;
        }


        /// <summary>
        /// Prefills a Table and Refills a set of controls
        /// </summary>
        /// <param name="cs">Collection of controls to fill (whith childs)</param>
        /// <param name="freshperipherals">when true, not -entity-or-cached- tables are cleared and read again from DB</param>
        /// <param name="tablename">Table to Prefill</param>
        public virtual void FreshForm(Control.ControlCollection cs,
            bool freshperipherals,
            string tablename) {
            form_drawstates saved = DrawState;
            DrawState = form_drawstates.filling;
            var r = HelpForm.GetLastSelected(primaryTable);
            if (freshperipherals) {
                DO_GET(true, r); //fresh peripherals table, not entity tables
            }

            Do_Prefill(tablename);
            //if (freshperipherals) helpForm.comboBoxToRefilter = true;
            ReFillControls(cs);
            //if (freshperipherals) helpForm.comboBoxToRefilter = false;
            DrawState = saved;
        }

           /// <summary>
        /// Adds the event to a navigator grid
        /// </summary>
        /// <param name="g"></param>
        internal static void addEditEventToGrid(DataGrid g) {
            if (g?.Tag == null) return;
            if (g.Tag.ToString().StartsWith("TreeNavigator")) return;

            var f = g.FindForm();
            if (f == null) return;
            var m =  HelperMetaFactory.getInstance<Form>(f);
            var ctrl =  HelperMetaFactory.getInstance<FormController>(f);
            if (m == null) return;

            if (g.TableStyles.Count != 1) return;

            var dgt = g.TableStyles[0];
            foreach (DataGridColumnStyle dgc in dgt.GridColumnStyles) {
                if (!(dgc is DataGridTextBoxColumn dgtbc)) continue;
                dgtbc.TextBox.DoubleClick += ctrl.textBoxDoubleClick;
                dgtbc.TextBox.MouseDown += ctrl.textBoxMouseDown;
            }

            g.MouseDown += ctrl.Grid_MouseDown;

        }

        
        private DateTime _lastGridClick;
        
        private void Grid_MouseDown(object sender, MouseEventArgs e) {
            if (destroyed) return;
            if (!(sender is DataGrid)) return;
            _lastGridClick = DateTime.Now;

        }

        private void textBoxDoubleClick(object sender, EventArgs e) {
            if (destroyed) return;
            var T = sender as TextBox;
            if (!(T?.Parent is DataGrid g)) return;
            Button_Click(g, null);
        }

        private void textBoxMouseDown(object sender, MouseEventArgs e) {
            if (destroyed) return;
            if (DateTime.Now >= _lastGridClick.AddMilliseconds(SystemInformation.DoubleClickTime)) return;
            var T = sender as TextBox;
            if (!(T?.Parent is DataGrid g)) return;
            Button_Click(g, null);


        }


         /// <summary>
        /// Get the DataTable linked to a grid
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
		public static DataGrid GetLinkedGrid(DataTable T){
			return (DataGrid)T.ExtendedProperties["LinkedGrid"];
		}

        /// <summary>
        /// Links a DataTable to a grid
        /// </summary>
        /// <param name="T"></param>
        /// <param name="G"></param>
		public static void SetLinkedGrid(DataTable T, DataGrid G){
			T.ExtendedProperties["LinkedGrid"]=G;
		}

        private static IMetaModel staticModel = MetaFactory.factory.getSingleton<IMetaModel>();

       

          /// <summary>
        /// Do a "mainselect" command, i.e. the current primary table row is "choosen"
        ///  and returned to the caller Form
        /// </summary>
        /// <remarks>Needs PrimaryTable, TableToMonitor, DllDispatcher,linkedForm,helpForm.mainTableSelector,LastSelectedRow </remarks>
        public virtual void MainSelect() {
            //MarkEvent("Start MainSelect");
            //LastSelectedRow = null;
            //var sel = !string.IsNullOrEmpty(TableToMonitor)
            //    ? HelpForm.GetLastSelected(ds.Tables[TableToMonitor])
            //    : helpForm.LastSelectedRow;
            var sel = HelpForm.GetLastSelected(primaryTable);

            if (sel == null) return;
            if (sel.RowState == DataRowState.Deleted) return;
            if (sel.RowState == DataRowState.Detached) return;

            var metaToConsider = meta;
            //if (PrimaryTable != TableToMonitor) {
            //    metaToConsider = DllDispatcher.Get(TableToMonitor);
            //    if (DllDispatcher.ErroreGrave) {
            //        ErroreIrrecuperabile = true;
            //        QueryCreator.ShowError(linkedForm,
            //            LM.errorLoadingMeta(TableToMonitor),//$"Errore nel caricamento del metadato {TableToMonitor} è necessario riavviare il programma.",
            //            LM.ErrorTitle);
            //        return;
            //    }
            //}

            linkedForm.DialogResult = System.Windows.Forms.DialogResult.None;
            if (!metaToConsider.CanSelect(sel)) return;
            if ( helpForm.mainTableSelector is TreeView) {		//PrimaryTable == TableToMonitor &&
                var tNode = ((TreeView) helpForm.mainTableSelector).SelectedNode;
                var tn = (tree_node) tNode?.Tag;
                if (tn == null) return;
                if (!tn.CanSelect()) {
                    shower.Show(linkedForm, tn.UnselectableMessage());
                    return;
                }
            }

            lastSelectedRow = sel;
            linkedForm.DialogResult = System.Windows.Forms.DialogResult.OK;
            //MarkEvent("Stop MainSelect");
        }
        public static IWinFormMetaData GetMetaData(Form f) {
            return f?.getInstance<IWinFormMetaData>();
        }

        public static IFormController GetController(Form f) {
            return f?.getInstance<IFormController>();
        }


        /// <summary>
        /// Do a MainSelect command
        /// </summary>
        /// <param name="f"></param>
        public static void MainSelect(Form f) {
            var m = GetController(f);
            m?.MainSelect();
        }

         /// <summary>
        /// Do a generic command
        /// </summary>
        /// <param name="f"></param>
        /// <param name="command"></param>
        public static void DoMainCommand(Form f, string command) {
            f.ActiveControl = null;
            var ctrl = GetController(f);
            if (ctrl == null) return;            
            if (ctrl.isClosing) return;
            if (ctrl.ErroreIrrecuperabile) return;
            var dispatcher = HelperMetaFactory.getInstance<IMetaDataDispatcher>(f);
            if (dispatcher.unrecoverableError) return;
            ctrl.DoMainCommand(command);
        }

      

       

       

        /// <summary>
        /// choose.table.listtype.filter or choose.table.listtype.clear
        /// </summary>
        /// <param name="command"></param>
        /// <param name="origin">Controls that originated the command</param>
        /// <returns>true if something was selected</returns>
        public bool Choose(string command, Control origin=null) {
            if (!formInited) return false;
            var controlliTarget = linkedForm.Controls;
            if (origin != null) controlliTarget = origin.Controls;

            GetFormData(true);
            string cmd = GetFieldLower(command, 0);
            if (cmd != "choose") return false;
            string entity = GetFieldLower(command, 1);
            string unaliased = ds.Tables[entity].tableForReading();
            if (unaliased == null) {
                shower.ShowError(linkedForm, LM.errorRunningCommand(command), LM.ErrorTitle);
                    //$"Errore nell\'esecuzione del comando {command}", "Errore");
                errorLogger.logException(LM.errorRunningCommand(command), meta: meta);
                return false;
            }

            string listtype = GetFieldLower(command, 2);
            string originalFilter  = GetLastField(command, 3);
            q filter = originalFilter=="clear"? null:MetaExpression.fromString(originalFilter);
            var M = dispatcher.GetWinFormMeta(unaliased);
            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm, LM.errorLoadingMeta(unaliased), LM.ErrorTitle);
                    //$"Errore nel caricamento del metadato {unaliased} è necessario riavviare il programma.", "Errore");
                return false;
            }

            if (M == null) {
                shower.Show(linkedForm, $"Entity {unaliased} was not found in form {linkedForm.Text}");
                return false;
            }

            M.filterLocked = true;
            M.shower = shower;
            //M.setExternalForm( linkedForm); aborted

            var entityTable = ds.Tables[entity];

            //sometimes unnecessary cause SelectOne does the filter merging
            if (originalFilter != "clear" && entity != unaliased) {
                filter = GetData.MergeFilters(filter, entityTable);
            }

            //Commento perché un choose dovrebbe sempre filtrare il filtro per la selezione non altro
            //if (InsertMode && filter != "clear")
            //    filter = QHS.AppAnd(filter, M.GetFilterForInsert(EntityTable));
            //if (IsEmpty && filter != "clear")
            //    filter = QHS.AppAnd(filter, M.GetFilterForSearch(EntityTable));
            if (originalFilter != "clear")
                filter = q.and(filter, M.GetFilterForSearch(entityTable));



            M.ExtraParameter =
                metaModel.GetExtraParams( ds.Tables[entity]); // DS.Tables[entity].ExtendedProperties[FormController.extraParams];
            M.ds = ds.Clone();
            M.ds.Tables[entity].Clear();


            DataRow SelectedRow = null;

            if (originalFilter == "clear") {
                helpForm.FillSpecificRowControls(controlliTarget, entityTable, null);
                //FillTableControls(LinkedForm, EntityTable, null);
                //myAfterRowSelect(EntityTable,null);
                ManageSelectedRow(null, entityTable, true, controlliTarget);
                M.Destroy();
                return false;
            }

            if (originalFilter != "clear") {
                //filter = GetData.MergeFilters(filter,DS.Tables[entity]);
                DataTable Exclude = null;
                if (metaModel.IsNotEntityChild(entityTable)) Exclude = entityTable;
                M.getData = getData; // era myGetData;
                SelectedRow = M.SelectOne(listtype, filter, unaliased,  Exclude);
            }

            if (SelectedRow == null) {
                M.Destroy();
                return false;
                //				myHelpForm.FillTableControls(LinkedForm, EntityTable, null);
                //				myAfterRowSelect(EntityTable,null);
                //				return false;
            }

            //SelectedRow may have been retrieved from a view
            if (SelectedRow.Table != entityTable) {
                //search selected row in EntityTable
                //var keyfilter = QueryCreator.WHERE_REL_CLAUSE(SelectedRow, entityTable.PrimaryKey, entityTable.PrimaryKey, DataRowVersion.Default, false);
                var existingRow = entityTable.filter(q.mCmp(SelectedRow, entityTable.PrimaryKey));//.Select(keyfilter);
                if (existingRow.Length == 0) {
                    var newRow = entityTable.NewRow();
                    if (M.GetRowFromList(SelectedRow, listtype, newRow)) {
                        if (!IsEmpty) {
                            entityTable.Rows.Add(newRow);
                            newRow.AcceptChanges();
                            SelectedRow = newRow;
                        }

                    }
                    else {
                        if (RowChange.FindChildRelation(primaryTable, entityTable) != null)
                            SelectedRow = getData.GetByKey(entityTable, SelectedRow).GetAwaiter().GetResult();
                        else
                            SelectedRow.Table.TableName = entityTable.TableName;
                    }
                }
                else {
                    SelectedRow = existingRow[0];
                }
            }

            ManageSelectedRow(SelectedRow, entityTable, true, controlliTarget);
            M.Destroy();
            return true;
        }


        /// <summary>
        /// Do a "Choose" command
        /// </summary>
        /// <param name="f"></param>
        /// <param name="command"></param>
        /// <returns>true if a row has been choosed</returns>
        public static bool Choose(Form f, string command) {
            var c = GetController(f);
            return c != null && c.Choose(command);
        }

         //void ManageSelectedRow(DataRow Selected, DataTable Monitored) {
        //    ManageSelectedRow(Selected, Monitored, true);
        //}

        /// <summary>
        /// Opens the multiple link/unlink form template
        /// </summary>
        /// <param name="formTitle">Caption of the form to create</param>
        /// <param name="labelAdded">Caption for the already linked grid</param>
        /// <param name="labelToAdd">Caption for the rows to add grid</param>
        /// <param name="notEntityChildTable">Table containing the rows to link/unlink</param>
        /// <param name="filter">Filter used to retrieve the "to add" rows (in memory)</param>
        /// <param name="filterSql">Filter used to retrieve the "to add" rows (in database)</param>
        /// <param name="listingtype">listing type to use for the two grids</param>
        public void MultipleLinkUnlinkRows(string formTitle,
            string labelAdded, string labelToAdd,
            DataTable notEntityChildTable,
            string filter,
            string filterSql,
            string listingtype) {
            if (IsEmpty) return;
            var curr = HelpForm.GetLastSelected(primaryTable);
            if (curr == null) return;
            var f = new frmMultipleSelection(
                meta, this, formTitle, labelAdded, labelToAdd, notEntityChildTable,
                filter, filterSql, listingtype);
            MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, null);
            f.ShowDialog();
            FreshForm(false);
        }

          /// <summary>
        /// 
        /// </summary>
        /// <param name="selected">A row that May (in case of choose) or may NOT belong to Monitored</param>
        /// <param name="monitored">current DataSet Table</param>
        /// <param name="canprefill"></param>
        /// <param name="cs">Control collection to eventually update</param>
        void ManageSelectedRow(DataRow selected, DataTable monitored, bool canprefill, Control.ControlCollection cs) {
            q keyfilter = null;
            if (selected != null) {
                DataSetUtils.CopyPrimaryKey(selected.Table, monitored);
                keyfilter = q.keyCmp(selected);
            }

            if (CanRecache(monitored)) {
                metaModel.ReCache(monitored);
            }

            beforeRowSelect(monitored, selected);

            if (IsEmpty) {
                var saved = DrawState;
                DrawState = form_drawstates.filling;
                getData.ReadCached();
                if (canprefill) Do_Prefill(monitored.TableName); //s.manzio	  //ELIMINATA 21/1/2003

                if (selected != null) {
	                selected.Table.TableName = monitored.TableName; //trick to tell helpform!
                    var oneSelected = monitored.filter(keyfilter);		//.Select(keyfilter);
                    if (oneSelected.Length > 0) selected = oneSelected[0];
                }

                beforeRowSelect(monitored, selected);
                helpForm.FillParentControls(cs, monitored, selected); //ex LinkedForm invece di CS
                helpForm.FillSpecificRowControls(cs, monitored, selected);
                helpForm.IterateFillRelatedControls(cs, null, monitored, selected);
                DrawState = saved;
            }
            else {
                if (selected != null && selected.RowState == DataRowState.Detached && selected.Table == monitored) {
                    conn.SelectIntoTable(monitored, filter: keyfilter);
                    var oneSelected = monitored.filter(keyfilter);	//q.keyCmp(selected)	//.Select(keyfilter);
                    if (oneSelected.Length > 0) {
                        selected = oneSelected[0];
                    }
                    else {
                        meta.LogError(LM.couldNotLinkTable(monitored.TableName,meta.Name));
                            //$"Non sono riuscito a collegare la riga alla tabella {monitored.TableName} nel metadato  {this.Name}");
                        return;
                    }
                }

                makeChild(selected, monitored, null);
                if ((!isSubentity) ||
                    (formState != form_states.insert))
                    CheckEntityChildRowAdditions(selected, null);

                //APRILE 2004
                //myHelpForm.ComboBoxToRefilter = true;
                eventManager.dispatch(new StartRowSelectionEvent(selected));
                FreshForm(cs, canprefill, monitored.TableName);
                eventManager.dispatch(new StopRowSelectionEvent(selected));
                //myHelpForm.ComboBoxToRefilter = false;
                //PRIMA DI APRILE 2004 era:
                //  FreshForm(canprefill); //LinkedForm.Controls, canprefill, Monitored.TableName);//s.manzio
                //PRIMA ANCORA:
                //FreshForm(Monitored.TableName); //21/1/2003
                if (selected?.RowState == DataRowState.Detached) selected = null;
                if (selected != null && keyfilter is null) {
                    meta.LogError(
                        $"Accesso a tabella senza chiave sulla tabella {monitored.TableName} nel form {linkedForm?.Name}",
                        null);
                }
                
                if (selected == null && !(keyfilter  is null)) {
                    //Some times keyfilter is not really a key so it's necessary to use also static filter of related table
                    conn.SelectIntoTable(monitored, filter:GetData.MergeFilters(keyfilter,monitored));
                    var oneSelected = monitored.filter(keyfilter);	//.Select(keyfilter);
                    if (oneSelected.Length > 0) selected = oneSelected[0];
                }

                if (selected != null) {
                    var oneSelected = monitored.filter(q.keyCmp(selected));	//.Select(keyfilter);
                    if (oneSelected.Length == 0) {
                        //Some times keyfilter is not really a key so it's necessary to use also static filter of related table
                        conn.SelectIntoTable(monitored, filter: GetData.MergeFilters(keyfilter,monitored));
                        oneSelected = monitored.filter(q.keyCmp(selected));	//.Select(keyfilter);
                    }

                    if (oneSelected.Length > 0) selected = oneSelected[0];
                }

                beforeRowSelect(monitored, selected);
                helpForm.IterateFillRelatedControls(cs, null, monitored, selected);

            }

            FreshToolBar();
            afterRowSelect(monitored, selected);
        }

          /// <summary>
        /// Edits a datarow using a specified listig type. Also Extra parameter
        ///  of R.Table is considered.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="_editType"></param>
        /// <param name="outputRow"></param>
        /// <returns>true if row has been modified</returns>
        public bool EditDataRow(DataRow r, string _editType, out DataRow outputRow) {
            //OutputRow = null;
            var sourceTable = r.Table;
            var unaliased = sourceTable.tableForReading();

            var m = dispatcher.GetWinFormMeta(unaliased);
            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm,LM.errorLoadingMeta(unaliased) 
                        //$"Errore nel caricamento del metadato {unaliased} è necessario riavviare il programma."
                    ,LM.ErrorTitle);
                outputRow = r;
                return false;
            }

            if (m == null) {
                outputRow = r;
                return false;
            }

            //la seg. è causa di errori,poiché non sempre DS è valorizzato.
            //M.ExtraParameter= DS.Tables[SourceTable.TableName].ExtendedProperties[ExtraParams];
            m.ExtraParameter =
                metaModel.GetExtraParams(sourceTable); //SourceTable.ExtendedProperties[FormController.extraParams];

            m.SetDefaults(sourceTable);
            m.SourceRow = r; //r is not null
            m.Edit(linkedForm, EditType(_editType, 0), true);
            outputRow = m.NewSourceRow;  //Impostato da getSourceChanges a sua volta chiamato da getFormData
            var res = m.EntityChanged;
            //if (res) getData.CalcTemporaryValues(outputRow);
            m.Destroy();
            return res;

        }

    
        


     
        #region Gestione AutoChoose 

        /// <summary>
        /// Informations about a groupbox used witg an autochoose/automanage 
        /// </summary>
        public class AutoInfo {
            /// <summary>
            /// Groupbox where the autochoose/automanage is located
            /// G has tag: AutoChoose.TextBoxName.ListType.StartFilter or
            ///            AutoManage.TextBoxName.EditType.StartFilter
            /// </summary>
            public GroupBox G;

            /// <summary>
            /// Edittype for automanage, ListingType for autochoose
            /// </summary>
            public string type;

            /// <summary>
            /// Startfilter specified for the search 
            /// </summary>
            public MetaExpression startfilter;

            /// <summary>
            /// Field linked to the activating textbox
            /// </summary>
            public string startfield;

            /// <summary>
            /// Table to search into
            /// </summary>
            public string table;

            /// <summary>
            /// Can be AutoManage or AutoChoose
            /// </summary>
            public string kind;

            /// <summary>
            /// True if currently operating
            /// </summary>
            public bool busy = false;

            /// <summary>
            /// Table linked to hidden textbox, usually is table
            /// </summary>
            public DataTable ParentTable;

            /// <summary>
            /// Table for search the main table, usually a view on the main table
            /// </summary>
            public DataTable ChildTable;

            /// <summary>
            /// Field for searching the main table
            /// </summary>
            public string childfield;


            /// <summary>
            /// Field linked to parentTable
            /// </summary>
            public string parentfield;


            /// <summary>
            /// Hidden TextBox linked to the GroupBox. Hiddent textbox has tag ParentTable.parentfield?ChildTable.childfield
            /// </summary>
            public TextBox InvisibleTextBox;

            /// <summary>
            /// Gets text from the hidden TextBox
            /// </summary>
            /// <returns></returns>
            public string GetInvisibleText() {
                if (InvisibleTextBox == null) return "";
                return InvisibleTextBox.Text;
            }

            /// <summary>
            /// Creates an autoinfo structure
            /// </summary>
            /// <param name="G">Linked GroupBox</param>
            /// <param name="type">EditType or ListingType</param>
            /// <param name="startfilter">always applied filter</param>
            /// <param name="startfield">field to search</param>
            /// <param name="table">parent table to search</param>
            /// <param name="kind">AutoManage or AutoChoose</param>
            public AutoInfo(GroupBox G,
                string type,
                q startfilter,
                string startfield,
                string table,
                string kind) {
                this.G = G;
                this.type = type;
                this.startfield = startfield;
                this.startfilter = startfilter;
                this.table = table;
                this.kind = kind;
            }

        }
        /// <summary>
        /// Set the value linked to  a textBox located in a AutoManage or AutoChoose groupbox. Eventually calls AfterRowSelect
        /// </summary>
        /// <param name="idValue"></param>
        /// <param name="T"></param>
        public void SetAutoField(object idValue, TextBox T) {
            AutoInfo A = GetAutoInfo(T.Name);
            if (A == null) return;
            object oldval = GetAutoField(T);
            if (oldval == null) return;
            bool mustcallAfterRowSelect = !oldval.Equals(idValue);

            if (idValue == DBNull.Value) {
                helpForm.ClearControls(A.G.Controls);
                if (A.ChildTable.Rows.Count == 1) {
                    A.ChildTable.Rows[0][A.childfield] = DBNull.Value;
                }

                if (mustcallAfterRowSelect) afterRowSelect(A.ParentTable, null);
                return;
            }

            if (A.ParentTable.filter(q.eq(A.parentfield,idValue)).Length == 0) {		//.Select(QHC.CmpEq(A.parentfield, idValue
                A.ParentTable.Clear();
                conn.SelectIntoTable(A.ParentTable, filter: q.eq(A.parentfield, idValue)).GetAwaiter().GetResult();
            }

            //if (A.ParentTable.Select(QHC.CmpEq(A.parentfield, idValue)).Length == 0) return; //Errore nei dati
            if (A.ParentTable.filter(q.eq(A.parentfield,idValue)).Length == 0) return; //Errore nei dati
            helpForm.FillSpecificRowControls(A.G.Controls, A.ParentTable, A.ParentTable.Rows[0]);
            if (A.ChildTable.Rows.Count == 1) {
                A.ChildTable.Rows[0][A.childfield] = idValue;
            }

            if (mustcallAfterRowSelect) afterRowSelect(A.ParentTable, A.ParentTable.Rows[0]);

        }

        /// <summary>
        /// Gets the value in the HiddenTextBox linked to an AutoManage or AutoChoose TextBox
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public object GetAutoField(TextBox T) {
            var a = GetAutoInfo(T.Name);
            if (a == null) return null;

            var id = a.GetInvisibleText();
            return id == ""
                ? DBNull.Value
                : HelpUi.GetObjectFromString(a.ChildTable.Columns[a.childfield].DataType, id, "g");
        }

        #endregion


        
        #region Grid Events




        /// <summary>
        /// Function callable statically from a form to implement a grid-edit event
        /// </summary>
        /// <param name="g">Grid containing row to edit</param>
        /// <param name="editType">edit type to use</param>
        /// <returns>Edited Row</returns>
        public static DataRow Edit_Grid(DataGrid g, string editType) {
            var f = g.FindForm();
            var c = GetController(f);
            return c?.Edit_Grid_Row(g, editType);
        }



       


       

        /// <summary>
        /// statically callable function to implement a delete - grid event
        /// </summary>
        /// <param name="g"></param>
        public static DataRow Delete_Grid(DataGrid g) {
            var f = g.FindForm();
            var m = GetController(f);
            return m?.Delete_Grid_Row(g);
        }

        /// <summary>
        /// Function to link with an "unlink" button
        /// </summary>
        /// <param name="g">Grid containing row to unlink</param>
        /// <returns>unliked row or null if action canceled</returns>
        public static DataRow Unlink_Grid(DataGrid g) {
            var f = g.FindForm();
            var m = GetController(f);
            return m?.Unlink_Grid_Row(g);
        }






        /// <summary>
        /// Event to link with a delete grid button
        /// </summary>
        /// <param name="g">grid containing row to delete</param>
        /// <returns>deleted row or null if action canceled</returns>
        public virtual DataRow Delete_Grid_Row(DataGrid g) {
            if (g.CurrentRowIndex < 0) return null;
            if (!formInited) return null;

            //gets data from form
            GetFormData(true);

            var sourceDataSet = (DataSet) g.DataSource;
            if (sourceDataSet == null) {
                shower.Show(linkedForm,
                    $"DataGrid {g.Name} in Form {g.FindForm()?.Name} has a wrong Tag ({mdl_utils.Quoting.quote(g.Tag, false)})");
                return null;
            }

            bool res = helpForm.GetCurrentRow(g, out var sourceTable, out var currDr);
            if (!res) return null;
            if (currDr == null) return null;

            var m = dispatcher.Get(sourceTable.TableName);
            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm, LM.errorLoadingMeta(sourceTable.TableName), LM.ErrorTitle);
                    //$"Errore nel caricamento del metadato {sourceTable.TableName} è necessario riavviare il programma.","Errore");
                return null;
            }

            if (shower.Show(linkedForm,
                    LM.deleteSelectedRowFromTable(m.Name, sourceTable.TableName),
                    //$"Cancello la riga selezionata dalla tabella {m.Name}({sourceTable.TableName})",
                    LM.confirmTitle, mdl.MessageBoxButtons.OKCancel) == mdl.DialogResult.Cancel) return null;

            currDr.Delete();
            if (MetaModel.IsSubEntity(sourceTable, ds.Tables[primaryTableName])) {
                entityChanged = true;
            }

            HelpForm.SetLastSelected(sourceTable, null);

            helpForm.SetDataRowRelated(linkedForm, sourceTable, null);
            
            //2019: non credo sia necessario calcolare i valori temporanei della tabella sourceTable dopo averne cancellato una riga
            //getData.GetTemporaryValues(sourceTable);
            
            //GetData.CalculateTable(SourceTable);

            //UnsavedChanges=true;			
            FreshForm();
            //FreshForm(SourceTable.TableName); //21/1/2003
            return currDr;
        }

        /// <summary>
        /// Unlinks a row contained in a grid
        /// </summary>
        /// <param name="g">Grid containing row to unlink</param>
        /// <returns>unliked row or null if action canceled</returns>
        public virtual DataRow Unlink_Grid_Row(DataGrid g) {
            if (g.CurrentRowIndex < 0) return null;
            if (!formInited) return null;
            //gets data from form
            GetFormData(true);

            var sourceDataSet = (DataSet) g.DataSource;
            if (sourceDataSet == null) {
                shower.Show(linkedForm, $"DataGrid {g.Name} in Form {g.FindForm()?.Name} has a wrong Tag ({mdl_utils.Quoting.quote(g.Tag, false)})");
                return null;
            }

            var res = helpForm.GetCurrentRow(g, out DataTable sourceTable, out DataRow currDr);
            if (!res) return null;
            return currDr == null ? null : unlink(currDr);
        }

        #endregion

        
        /// <summary>
        /// Opens an external (not sub-entity) new form.
        /// From new form a datatable is monitored to refresh current form basing on
        ///  last row selected in the new form. This is generally the primary table of
        ///  the metadata linked to the new form.
        /// </summary>
        /// <param name="f">Parent form</param>
        /// <param name="command">tag with syntax "manage.tablename.edittype.filter"</param>
        /// <param name="startf">Start Field wanted</param>
        /// <param name="startv">Start value wanted</param>
        public static bool Manage(Form f, string command, string startf, string startv) {
            var c = GetController(f);
            return c != null && c.Manage(command, startf, startv,null);
        }

        /// <summary>
        /// Return true if something has been selected, false if selection was canceled
        /// </summary>
        /// <param name="f"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool Manage(Form f, string command) {
            return Manage(f, command, null, null);
        }

       

        /// <summary>
        /// Opens a list form and select a row from it
        /// </summary>
        /// <param name="command"></param>
        /// <param name="startfield"></param>
        /// <param name="startvalue"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public bool Manage(string command, string startfield, string startvalue, Control origin=null) {
            var controlliTarget = linkedForm.Controls;
            if (origin != null) controlliTarget = origin.Controls;
            GetFormData(true); //            GetFormData(true);
            var cmd = GetFieldLower(command, 0);
            if (cmd != "manage") return false;
            //entity is the name of the table in the DataSet
            var entity = GetFieldLower(command, 1);
            //unaliased is the name of the actual metadata to build and get from db
            var unaliased = ds.Tables[entity].tableForReading() ?? entity;

            var editmode = GetFieldLower(command, 2);
            var filter = MetaExpression.fromString( GetLastField(command, 3));

            var m = dispatcher.GetWinFormMeta(unaliased);
            if (dispatcher.unrecoverableError) {
                ErroreIrrecuperabile = true;
                shower.ShowError(linkedForm,
                    LM.errorLoadingMeta(unaliased), LM.ErrorTitle);
                    //$"Errore nel caricamento del metadato {unaliased} è necessario riavviare il programma.", 
                    //"Errore");
                return false;
            }

            if (m == null) {
                shower.Show(linkedForm,
                    $"Entity {unaliased} was not found in form {linkedForm.Text}");
                return false;
            }

            m.filterLocked = true;
            filter = GetData.MergeFilters(filter, ds.Tables[entity]);

            //M.IsInsert= false;
            m.SearchEnabled = false;
            m.MainSelectionEnabled = true;
            m.StartFilter = filter;
            m.StartFieldWanted = startfield;
            m.StartValueWanted = startvalue;
            if (ds.Tables[entity] == null) {
                shower.Show($"{entity} table not found in Dataset", "Errore");
                return false;
            }

            m.ExtraParameter = metaModel.GetExtraParams(ds.Tables[entity]); //.ExtendedProperties[FormController.extraParams];
            m.edit_type = editmode;
            m.ds = ds.Clone();
            m.ds.Tables[entity].Clear();

            DataTable entityTable = null;
            DataRow selected = null;

            if (startvalue != null) {
                //try to load a row directly, without opening a new form		
                var stripped = startvalue;
                if (stripped.EndsWith("%")) stripped = stripped.TrimEnd('%');
                var filter2 = q.and(filter, q.eq(startfield, stripped));
                //GetData.MergeFilters(filter,"("+startfield+"='"+stripped+"')");			
                //m.myGetData = myGetData; //inutile, c'è già la successiva
                m.getData = getData;
                selected = m.SelectByCondition(filter2, unaliased);
                if (selected != null) {
                    entityTable = selected.Table;
                    while (entityTable.Rows.Count > 1) {
                        entityTable.Rows[1].Delete();
                        entityTable.Rows[1].AcceptChanges();
                    }

                    if (entityTable.TableName == unaliased) entityTable.TableName = entity;
                    DataSetUtils.CopyPrimaryKey(entityTable, ds.Tables[entity]);
                }
            }


            if (selected == null) {
                var res = m.Edit(linkedForm, editmode, true);
                var monitored = m.TableName;
                if (monitored == unaliased) monitored = entity;
                entityTable = ds.Tables[monitored];
                if (!res) { //user canceled the operation
                    if (m.EntityChanged && (entityTable != null)) {
                        if (CanRecache(entityTable)) {
                            GetData.ReCache(entityTable);
                            getData.ReadCached();
                        }

                        if (IsEmpty)
                            Do_Prefill(monitored); //ex (), S.manzio
                        else
                            FreshForm(controlliTarget, true, monitored); //ex LinkedForm.Controls, true,monitored
                    }

                    m.Destroy();
                    return false;
                }

                selected = m.LastSelectedRow;
                m.LastSelectedRow = null;

            }

            if (selected == null) {
                var saved = DrawState;
                DrawState = form_drawstates.filling;
                if (CanRecache(entityTable)) {
                    GetData.ReCache(entityTable);
                    getData.ReadCached();
                }

                beforeRowSelect(entityTable, null);
                DO_GET(true, null);
                Do_Prefill(); //ELIMINATA 21/1/2003
                DrawState = saved;
                afterRowSelect(entityTable, null);
                m.Destroy();
                return true;
            }

            //Entity Table is the actual DataTable in the DataSet. Select belongs to another
            // DataSet and possibly has a different TableName
            ManageSelectedRow(selected, entityTable, true, controlliTarget);
            //myHelpForm.SetDataRowRelated(LinkedForm, F.Table, index);
            m.Destroy();
            return true;
        }

        
        #region Gestione Form elenchi

        public void SetNewListForm(ICustomViewListForm NewListForm) {
            
            if (isClosing)return;
            if (linkedForm != null ) linkedForm.KeyPreview = true;            
            currentListForm?.close();

            currentListForm = NewListForm;
            FreshToolBar();
        }

        public bool hasNext() {
            if (currentListForm == null) return false;
            return currentListForm.hasNext();
        }

        public bool hasPrev() {
            if (currentListForm == null) return false;
            return currentListForm.hasPrev();
        }

        //void GotoFirst() {
        //    if (CurrentListForm == null) return;
        //    CurrentListForm.GotoFirst();
        //    formController.FreshToolBar();
        //}

        void GotoNext() {
            if (currentListForm == null) return;
            currentListForm.gotoNext();
            FreshToolBar();
        }

        void GotoPrev() {
            if (currentListForm == null) return;
            currentListForm.gotoPrev();
            FreshToolBar();
        }

        //void GotoLast() {
        //    if (CurrentListForm == null) return;
        //    CurrentListForm.GotoLast();
        //    formController.FreshToolBar();
        //}

        private void F_KeyDown(object sender, KeyEventArgs e) {
            //if (!e.Control) return;
            //if (e.KeyCode== Keys.Right) {
            //    if (HasNext()) GotoNext();
            //}
            //if (e.KeyCode== Keys.Left){
            //    if (HasPrev()) GotoPrev();
            //}
        }

        #endregion

       


             /// <summary>
        /// Called when maindosearch is called on a list form
        /// </summary>
        /// <param name="baseFilter">Initial filter to apply when filling form</param>
        /// <returns></returns>
        bool filterList(q baseFilter) {           
            var filter = helpForm.GetSearchCondition(linkedForm);
            filter = q.and(filter, baseFilter);

            eventManager.DisableAutoEvents();
            getData.GetPrimaryTable(filter);
            eventManager.EnableAutoEvents();
            if (primaryTable.Rows.Count == 0) {
                shower.Show(linkedForm, LM.noElementFound);
                return false;
            }

            //TODO: verificare che non si possa eliminare questa istruzione, che appare prematura visto che
            //  la riga principale ancora non è stata visualizzata. Si potrebbe aspettare che venga eseguita altrove
            //  in conseguenza del rowselect sulla tabella principale
            formState = form_states.edit; //assumes something will be displayed 

            HelpForm.SetLastSelected(primaryTable, null);
            eventManager.dispatch(new StartClearMainRowEvent());
            ReFillControls();
            eventManager.dispatch(new StopClearMainRowEvent());
            return true;
        }

         /// <summary>
        /// It's the main operation executed when maindosearch is invoked
        /// </summary>
        /// <param name="f"></param>
        /// <param name="listType">Listing type to use for searching</param>
        /// <param name="filterstart">additional filter to append to the
        ///  form-retrived condition</param>
        /// <returns></returns>
        public static bool searchRow(Form f, string listType, q filterstart) {
            var ctrl = f.getInstance<IFormController>();
            return ctrl != null && ctrl.searchRow(listType, filterstart, false);
        }

         /// <summary>
        /// A table is recachable if it is clearable and is not entity or subentity
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool CanRecache(DataTable T) {
            if (!metaModel.CanClear(T)) return false;
            if (T == primaryTable) return false;
            return !MetaModel.IsSubEntity(T, primaryTable);
        }

         /// <summary>
        /// Redraws the form getting cached and peripheral tables
        /// </summary>
        public void MainRefresh() {
            if (!MainRefreshEnabled) return;
            if (!formInited) return;
            if (!IsEmpty) GetFormData(true);

            if ( (!isSubentity) && (!IsEmpty)) {  //(formController.formType == form_types.main) &&
                if (!HasUnsavedChanges()) {
                    var oneRow = HelpForm.GetLastSelected(primaryTable);
                    if (oneRow != null) {
                        var ds2 = ds.Copy();
                        ds2.copyIndexFrom(ds);
                        //var keyfilter = QueryCreator.WHERE_KEY_CLAUSE(oneRow,DataRowVersion.Default, false);
                        var myRow = ds2.Tables[primaryTableName].filter(q.keyCmp(oneRow)).FirstOrDefault();//.Select(keyfilter)[0];

                        SelectRow(myRow, meta.DefaultListType);
                        //	DO_GET(false,OneRow);
                        // FreshForm(true);
                        return;
                    }
                }
            }

            foreach (DataTable T in ds.Tables) {
                if (!CanRecache(T)) continue;
                if (metaModel.CanRead(T)) continue;
                metaModel.Clear(T); // T.Clear();
                GetData.ReCache(T);
            }

            getData.ReadCached();

            if (!IsEmpty) {
                //GetFormData(true);
                FreshForm(true, true);
            }
            else {
                Do_Prefill();
                Clear();
            }
        }
        
       
        
        void MainDelete() {
            if (!formInited) return;
            GetFormData(true);

            curroperation = mainoperations.delete;
            var currEntity = HelpForm.GetLastSelected(primaryTable);
            if (currEntity == null) return;

            if (isSubentity) {
                linkedForm.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                curroperation = mainoperations.none;
                return;
            }

            if (InsertMode) {
                if (!DontWarnOnInsertCancel) {
                    if (shower.Show(linkedForm,
                            LM.insertCancelOnTable(meta.Name,primaryTable.TableName),
                                //$"Annullo l\'inserimento dell\'oggetto {Name} nella tabella {PrimaryDataTable.TableName}",
                            LM.confirmTitle, mdl.MessageBoxButtons.OKCancel) == mdl.DialogResult.Cancel) {
                        curroperation = mainoperations.none;
                        return;
                    }
                }
            }
            else {
                if (shower.Show(linkedForm,
                        LM.deleteFromTable(meta.Name,primaryTableName), //$"Cancello l\'oggetto {Name} dalla tabella {PrimaryDataTable.TableName}",
                        LM.confirmTitle, mdl.MessageBoxButtons.OKCancel) == mdl.DialogResult.Cancel) {
                    curroperation = mainoperations.none;
                    return;
                }
            }

            var idm = ds.getIndexManager();
            var allTables = (from DataTable t in ds.Tables  select t).ToArray();
            bool currEntityWasAdded = currEntity.RowState == DataRowState.Added;
            try {
                //form_drawstate = form_drawstates.clearing; dangerous??
                eventManager.DisableAutoEvents();
                idm?.suspend(true,allTables);
                RowChange.ApplyCascadeDelete(currEntity);
                idm?.resume(true,allTables);
                eventManager.EnableAutoEvents();
                //form_drawstate = form_drawstates.done;
                entityChanged = true;
                SaveFormData();
                if (currEntity.RowState != DataRowState.Detached) {
	                //modifiedTable = (from DataTable t in ds.Tables where t.HasChanges() select t).ToArray();
	                //var unchangedTable = (from DataTable t in ds.Tables where !t.HasChanges() select t).ToArray();
	                ds.RejectChanges();
	                idm?.resume(true,allTables);
                    FreshForm(true, false);
                    curroperation = mainoperations.none;
                    return;
                }
            }
            catch (Exception e) {
	            ds.RejectChanges();

                shower.ShowException(linkedForm, LM.cantDeleteObject, e);
                errorLogger.logException(LM.cantDeleteObject, e, meta: meta);
                //MetaFactory.factory.getSingleton<IMessageShower>().Show(LinkedForm,"Impossibile eliminare l'oggetto.\r\nDettaglio:\r\n"+E.Message);
                FreshForm(true, false);
                curroperation = mainoperations.none;
                return;
            }

            setFormText();
            if (InsertMode) return; //AfterClear has caused an insert --> nothing else to do
            curroperation = mainoperations.none;

            //			if (IsTree){
            //				DataRow Curr = HelpForm.GetLastSelected(PrimaryDataTable);
            //				if (Curr==null){
            //					DoMainCommand("mainsetsearch");
            //				}
            //				return;
            //			}
            //If not IsList Clear() has been called in SaveFormData()

            //			if (IsList){
            //				HelpForm.SetLastSelected(PrimaryDataTable,null);
            //				if (PrimaryDataTable.Rows.Count>0){
            //					DataGrid G = (DataGrid) myHelpForm.MainTableSelector;
            //					if (G.CurrentRowIndex!=0)
            //						G.CurrentRowIndex=0; //PERCHE' NON SCATTA CONTROLCHANGED???
            //					else
            //						myHelpForm.ControlChanged(G,null);
            //					FreshForm(true); //Per fare scattare l'AfterFill()
            //				}
            //				else {
            //					//DoMainCommand("mainsetsearch");  //Clear già effettuato in DO_POST
            //				}			
            //			}
            //
        }

        
       

        /// <summary>
        /// Executes a command described by the tag
        /// </summary>
        /// <param name="tag"></param>
        public void DoMainCommand(string tag) {
            if (destroyed) return;
            if (!formInited) return;
            if (linkedForm != null && linkedForm.IsDisposed) return;

            checkConn();
            if ((conn != null) && (conn.BrokenConnection)) {
                shower.Show(LM.dbConnectionInterrupted);
                return;
            }

            if (doingCommand) return;
            doingCommand = true;
            int handler = mdl_utils.MetaProfiler.StartTimer("doMainCommand * " + tag);
            try {
                //QueryCreator.MarkEvent(linkedForm.Name+":"+tag);
                InternalDoMainCommand(tag);
            }
            catch (Exception E) {
                mdl_utils.MetaProfiler.StopTimer(handler);
                doingCommand = false;
                if (tag == null) tag = LM.emptyWithinPar;
                logError(LM.errorRunningCommand(tag), E);  //"Errore eseguendo il comando " + tag, E);
                shower.ShowException(linkedForm, LM.errorRunningCommandCloseWindow(tag), //$"Errore eseguendo il comando {tag}\r\nE\' necessario chiudere la maschera.",
                    E);
                ErroreIrrecuperabile = true;
                return;
            }
            mdl_utils.MetaProfiler.StopTimer(handler);
            if ((conn != null) && (conn.BrokenConnection)) {
                shower.Show(LM.dbConnectionInterrupted);
            }

            doingCommand = false;

        }


        
        /// <summary>
        /// Do a generic command
        /// </summary>
        /// <param name="tag"></param>
        private void InternalDoMainCommand(string tag) {
            if (isClosing) return;
            string cmd = GetFieldLower(tag, 0);

            if (cmd == "crea_ticket") {
                meta.doHelpDesk();
                return;
            }

            if (ErroreIrrecuperabile || dispatcher.unrecoverableError) return;

            if (cmd.Equals("mainselect")) {
                if (!warnUnsaved()) return;
                ds.RejectChanges();
                MainSelect();
                return;
            }

            if (cmd.Equals("mainsetsearch")) {
                if (!warnUnsaved()) return;
                curroperation = mainoperations.setsearch;
                helpForm.lastTextNoFound = "";
                ds.RejectChanges();
                if (isTree) {
                    treeSetSearch();
                    curroperation = mainoperations.none;
                    return;
                }

                eventManager.dispatch(new StartClearMainRowEvent());
                Clear();
                eventManager.dispatch(new StopClearMainRowEvent());
                curroperation = mainoperations.none;
                return;
            }

            if (cmd.Equals("maindosearch") || cmd.Equals("emptylist")) {
                curroperation = mainoperations.search;
                bool emptylist = cmd.Equals("emptylist");
                var listtype = GetFieldLower(tag, 1) ?? meta.DefaultListType;
                q startfilter = q.fromString(GetLastField(tag, 2)) ?? meta.StartFilter;
                startfilter = q.and(startfilter, meta.additional_search_condition);
                if ((!isList) || isTree) {
                    searchRow(listtype, startfilter, emptylist);
                    curroperation = mainoperations.none;
                    return;
                }

                filterList( startfilter);
                curroperation = mainoperations.none;
                return;
            }

            if (cmd.Equals("showlast")) {
                var r = HelpForm.GetLastSelected(primaryTable);
                if (r == null) return;
                if (r.RowState == DataRowState.Deleted || r.RowState == DataRowState.Detached) return;
                var txtcreate = "";
                if (r.Table.Columns.Contains("cu")) {
                    txtcreate = LM.createdBy(r["cu"].ToString());
                }

                if (r.Table.Columns.Contains("createuser")) {
                    txtcreate =  LM.createdBy(r["createuser"].ToString());
                }

                if (r.Table.Columns.Contains("ct") && r["ct"]!=DBNull.Value) {
                    if (txtcreate == "") {
                        txtcreate = LM.createdAt((DateTime) r["ct"]);
                    }
                    else {
                        txtcreate += LM.createdAt( (DateTime)r["ct"]);
                    }
                }

                if (r.Table.Columns.Contains("createtimestamp") && r["createtimestamp"]!=DBNull.Value) {
                    if (txtcreate == "") {
                        txtcreate = LM.createdAt((DateTime) r["createtimestamp"]);
                    }
                    else {
                        txtcreate += LM.createdAt( (DateTime)r["createtimestamp"]);
                    }
                }
                

                if (txtcreate != "") txtcreate += "\n";
                var txtupdate = "";
                if (r.Table.Columns.Contains("lu")) {
                    txtupdate = LM.modifiedBy(r["lu"].ToString());
                }

                if (r.Table.Columns.Contains("lastuser")) {
                    txtupdate =  LM.modifiedBy(r["lastuser"].ToString());
                }
                
                if (r.Table.Columns.Contains("lt") && r["lt"]!=DBNull.Value) {
                    if (txtupdate == "") {
                        txtupdate = LM.modifiedAt((DateTime) r["lt"]);
                    }
                    else {
                        txtupdate += LM.modifiedAt( (DateTime)r["lt"]);
                    }
                }
                
                if (r.Table.Columns.Contains("lastmodtimestamp") && r["lastmodtimestamp"]!=DBNull.Value) {
                    if (txtupdate == "") {
                        txtupdate = LM.modifiedAt((DateTime) r["lastmodtimestamp"]);
                    }
                    else {
                        txtupdate += LM.modifiedAt( (DateTime)r["lastmodtimestamp"]);
                    }
                }
                

                shower.Show(null, txtcreate + txtupdate, LM.infoAboutObject);
                return;
            }

            if (cmd.Equals("mainsave")) {
                if (!GetFormData(false)) return;
                SaveFormData();
                return;
            }

            if (cmd.Equals("gotonext")) {
                if (!hasNext()) return;
                GotoNext();
            }

            if (cmd.Equals("gotoprev")) {
                if (!hasPrev()) return;
                GotoPrev();
            }

            if (cmd.Equals("editnotes") || cmd.Equals("addnotes")) {
                NotesOleNotes frmNotes = new NotesOleNotes(this);
                MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(frmNotes, null);
                frmNotes.ShowDialog();
                return;
            }

            if (cmd.Equals("maininsert")) {
                if (!warnUnsaved()) return;
                ds.RejectChanges();
                if (!isList) {
                    EditNew();
                    return;
                }

                //List form -> finds main datagrid/datatable
                var gridtreetag = helpForm.mainTableSelector.Tag.ToString();
                string editTypeRequested = GetFieldLower(gridtreetag, 2);
                if (editTypeRequested == null) {
                    //no sub-form to open: in-form insert mode
                    focusDetail();
                    EditNew();
                    return;
                }

                //Sezione che gestisce l'edit di un dettaglio dell'entità stessa in un nuovo form
                // non è usata da alcun form al momento, poiché si preferisce l'in-form -detail
                var m = dispatcher.GetWinFormMeta(primaryTableName);
                if (dispatcher.unrecoverableError) {
                    ErroreIrrecuperabile = true;
                    shower.ShowError(linkedForm,LM.errorLoadingMeta(primaryTableName),LM.ErrorTitle);
                    return;
                }

                var r = m.GetNewRow(null, primaryTable);
                if (r == null) return;

                m.SourceRow = r; //r is not null
                m.Edit(linkedForm, editTypeRequested, true);

                if (m.EntityChanged) {
                    SaveFormData();
                    entityChanged = true;
                }
                else {
                    r.Delete();
                }

                m.Destroy();

                eventManager.dispatch(new StartMainRowSelectionEvent(r));
                FreshForm(true, true); //21/1/2003
                eventManager.dispatch(new StopMainRowSelectionEvent(r));



                return;
            }

            if (cmd.Equals("maininsertcopy")) {
                if (shower.Show(linkedForm,LM.insertCopyConfirm, LM.confirmTitle, mdl.MessageBoxButtons.YesNo) != mdl.DialogResult.Yes) {
                    return;
                }

                if (!warnUnsaved()) return;
                ds.RejectChanges();
                if (!isList) {
                    EditNewCopy();
                    return;
                }

                //List form -> finds main datagrid/datatable
                string gridtreetag = helpForm.mainTableSelector.Tag.ToString();
                string editTypeRequested = GetFieldLower(gridtreetag, 2);
                if (editTypeRequested == null) {
                    //no sub-form to open: in-form insert mode
                    focusDetail();
                    EditNewCopy();
                    return;
                }

                var currCopy = HelpForm.GetLastSelected(primaryTable);
                if (currCopy == null) return;

                var m = dispatcher.GetWinFormMeta(primaryTableName);
                if (dispatcher.unrecoverableError) {
                    ErroreIrrecuperabile = true;
                    shower.ShowError(linkedForm,LM.errorLoadingMeta(primaryTableName),LM.ErrorTitle);
                    return;
                }

                var r = m.GetNewRow(null, primaryTable);
                if (r == null) return;

                r.BeginEdit();
                for (var i = 0; i < primaryTable.Columns.Count; i++) {
                    var c = primaryTable.Columns[i];
                    //don't copy autoincrements
                    if (c.IsAutoIncrement()) continue;
                    //don't copy keys
                    if (QueryCreator.IsPrimaryKey(primaryTable, c.ColumnName)) continue;
                    r[i] = currCopy[i];
                }

                r.EndEdit();

                m.SourceRow =r; //r is not null
                m.Edit(linkedForm, editTypeRequested, true);

                if (m.EntityChanged) {
                    SaveFormData();
                    entityChanged = true;
                }
                else {
                    r.Delete();
                }

                m.Destroy();

                eventManager.dispatch(new StartMainRowSelectionEvent(r));
                FreshForm(true, true); //21/1/2003	
                eventManager.dispatch(new StopMainRowSelectionEvent(r));

                return;
            }

            if (cmd.Equals("maindelete")) {
                MainDelete();
                return;
            }

            if (cmd.Equals("mainrefresh")) {
                MainRefresh();
                return;
            }

            if (cmd.Equals("horizwin")) {
                //metaprofiler.ShowAll();
                if (linkedForm == null) return;
                if (linkedForm.Modal) return;
                var main = linkedForm.MdiParent;
                main?.LayoutMdi(MdiLayout.TileHorizontal);
                return;
            }

            if (cmd.Equals("manage")) {
                Manage(tag, null, null);
                return;
            }

            if (cmd.Equals("choose")) {
                Choose(tag);
            }


        }

        
        void treeSetSearch() {
            formState = form_states.setsearch;
            DrawState = form_drawstates.clearing;

            HelpForm.SetLastSelected(primaryTable, null);
            lastSelectedRow = null;

            metaModel.AllowAllClear(ds);
            eventManager.dispatch(new StartClearMainRowEvent());

            //myHelpForm.ComboBoxToRefilter = true;
            //myHelpForm.SetDataRowRelated(LinkedForm, PrimaryDataTable, null);
            helpForm.lastTextNoFound = "";
            helpForm.ClearForm(linkedForm);
            //myHelpForm.ComboBoxToRefilter = false;
            eventManager.dispatch(new StopClearMainRowEvent());
            FreshToolBar();
            linkedForm.Text = meta.Name + " "+ LM.searchWithinPar;
            entityChanged = false;
            CallMethod("AfterClear");
            DrawState = form_drawstates.done;
        }

        /// <summary>
        /// True when data in the form has been modified and not saved
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool HasUnsavedChanges(Form f) {
            var c = GetController(f);
            return c != null && c.HasUnsavedChanges();
        }



              
        /// <summary>
        /// Create a new entity (eventually clearing current one) and updates form.
        /// </summary>
        /// <returns></returns>
        public virtual bool EditNew() {            

            curroperation = mainoperations.insert;
            if (!isList) {
                GointToInsertMode = true;
                eventManager.dispatch(new StartClearMainRowEvent());
                Clear(); //(false)
                eventManager.dispatch(new StopClearMainRowEvent());
                GointToInsertMode = false;
                if (!IsEmpty) return true; //AfterClear has generated an "insert"
            }

            DataRow parent = null;
            if (isTree) {
                parent = HelpForm.GetLastSelected(ds.Tables[primaryTableName]);
                //TreeParentRow= Parent;
            }

            meta.SetDefaults(primaryTable);
            var r = meta.GetNewRow(parent, ds.Tables[primaryTableName]);
            if (r == null) {
                curroperation = mainoperations.none;
                return false;
            }

            HelpForm.SetLastSelected(ds.Tables[primaryTableName], r);
            //R now is the row from which start the filling of the form

            formState = form_states.insert;

            DO_GET(false, r);

            //UnsavedChanges=true;
            entityChanged = true;
            //IsList=false;
            firstFillForThisRow = true;
            eventManager.dispatch(new StartMainRowSelectionEvent(r));
            //myHelpForm.ComboBoxToRefilter = true;
            ReFillControls();
            eventManager.dispatch(new StopMainRowSelectionEvent(r));
            //myHelpForm.ComboBoxToRefilter = false;
            firstFillForThisRow = false;

            curroperation = mainoperations.none;
            return true;

        }


        /// <summary>
        /// Function to statically link to "Main Insert" button
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool MainAdd(Form f) {
            
            var controller = GetController(f);
            if (!controller.warnUnsaved()) return false;
            controller.ds.RejectChanges();
            return controller.EditNew();
        }


      

        /// <summary>
        /// Create a new entity as a copy of current row
        /// </summary>
        /// <returns></returns>
        public virtual bool EditNewCopy() {
            if (primaryTable == null) return false;

            var currRow = HelpForm.GetLastSelected(primaryTable);
            if (currRow == null) return false;

            var dsCopy = ds.Copy();
            dsCopy.copyIndexFrom(ds);
            //var keyfilter = QueryCreator.WHERE_KEY_CLAUSE(currRow, DataRowVersion.Default, false);
            var primaryRowCopy = dsCopy.Tables[primaryTable.TableName].filter(q.keyCmp(currRow)).FirstOrDefault();//.Select(keyfilter)[0];


            if (!isList) {
                IsClearBeforeInsert = true;
                eventManager.dispatch(new StartClearMainRowEvent());
                Clear(); //(false)
                eventManager.dispatch(new StopClearMainRowEvent());

                IsClearBeforeInsert = false;
                if (!IsEmpty) return true; //AfterClear has generated an "insert"
            }

            meta.SetDefaults(primaryTable);
            var r = meta.GetNewRow(null, ds.Tables[primaryTable.TableName]);
            if (r == null) {
                shower.Show(linkedForm, LM.invalidDataOnTable(primaryTable.TableName));
                //$"La tabella {TableName} contiene dati non validi. Contattare il servizio di assistenza.");
                return false;
            }

            for (var i = 0; i < primaryTable.Columns.Count; i++) {
                var c = primaryTable.Columns[i];
                if (isList) {
                    if (c.IsAutoIncrement()) continue;                             //don't copy autoincrements                    
                    if (QueryCreator.IsPrimaryKey(primaryTable, c.ColumnName)) continue;//don't copy keys
                }

                meta.InsertCopyColumn(c, primaryRowCopy, r);
            }

            HelpForm.SetLastSelected(ds.Tables[primaryTable.TableName], r);
            formState = form_states.insert;

            if (isList) {
                //UnsavedChanges=true;
                entityChanged = true;
                //IsList=false;
                firstFillForThisRow = true;
                //myHelpForm.ComboBoxToRefilter = true;
                eventManager.dispatch(new StartMainRowSelectionEvent(r));
                ReFillControls();
                eventManager.dispatch(new StopMainRowSelectionEvent(r));
                //myHelpForm.ComboBoxToRefilter = false;
                firstFillForThisRow = false;
                return true;
            }

            //EFFETTUA IL DEEP-COPY
            recursiveNewCopyChilds(r, primaryRowCopy);



            //R now is the row from which start the filling of the form
            DO_GET(true, r);

            //UnsavedChanges=true;
            entityChanged = true;
            //IsList=false;
            firstFillForThisRow = true;
            eventManager.dispatch(new StartMainRowSelectionEvent(r));
            // myHelpForm.ComboBoxToRefilter = true;
            ReFillControls();
            eventManager.dispatch(new StopMainRowSelectionEvent(r));
            // myHelpForm.ComboBoxToRefilter = false;
            firstFillForThisRow = false;
            return true;

        }


        void recursiveNewCopyChilds(DataRow destRow, DataRow sourceRow) {
            var relC = sourceRow.Table.ChildRelations;
            foreach (DataRelation rel in relC) {
                if (QueryCreator.IsSkipInsertCopy(rel.ChildTable)) continue; //salta la tabella se è di tipo SkipInsertCopy
                var childTableName = rel.ChildTable.TableName;
                if (!MetaModel.IsSubEntity(ds.Tables[childTableName], destRow.Table)) continue;
                if (childTableName == sourceRow.Table.TableName) continue;
                var childsRowCopy = sourceRow.getChildRows(rel);

                foreach (var childSourceRow in childsRowCopy) {
                    var metaChild = dispatcher.Get(childTableName);
                    if (dispatcher.unrecoverableError) {
                        ErroreIrrecuperabile = true;
                        shower.ShowError(linkedForm, LM.errorLoadingMeta(primaryTable.TableName), LM.ErrorTitle);
                        //$"Errore nel caricamento del metadato {PrimaryTable} è necessario riavviare il programma.","Errore");
                    }

                    metaChild.SetDefaults(ds.Tables[childTableName]);
                    var newChildRow = metaChild.GetNewRow(destRow, ds.Tables[childTableName]);
                    newChildRow.BeginEdit();
                    foreach (DataColumn childCol in ds.Tables[childTableName].Columns) {
                        var skipthis = false;
                        foreach (var cc in rel.ChildColumns) {//testa se ChildCol fa parte della relazione padre-figlio
                            if (cc.ColumnName == childCol.ColumnName) skipthis = true;
                        }

                        if (skipthis) continue;
                        metaChild.InsertCopyColumn(childCol, childSourceRow, newChildRow);
                    }

                    newChildRow.EndEdit();

                    recursiveNewCopyChilds(newChildRow, childSourceRow);
                }
            }

        }


    }



}

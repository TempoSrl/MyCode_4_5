using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mdl;
using LM = mdl_language.LanguageManager;

namespace mdl_winform {
    public interface IWinFormMetaData: IMetaData, IFormInit {
        bool filterLocked { get; }

        /// <summary>
        /// If true, the list form is not filled when it is filled at start
        /// </summary>
        bool StartEmpty { get; set; }

        /// <summary>
        /// 
        /// </summary>
        DataRow SourceRow { get; set; }

        /// <summary>
        /// 
        /// </summary>
        DataRow NewSourceRow { get; set; }

        IFormController controller { get; set; }

        /// <summary>
        /// True if "mainselect" is enabled
        /// </summary>
        bool MainSelectionEnabled { get; set; }

        /// <summary>
        /// must be set to false if SetSearch/DoSearch must be disabled on form.
        /// </summary>
        bool SearchEnabled { get; set; }

        /// <summary>
        /// When false main insert button is disabled
        /// </summary>
        bool CanInsert { get; set; }

        /// <summary>
        ///   true if this is a sub-entity of an entity displayed in a parent form, i.e.   SourceRow is not null
        /// </summary>
        bool IsSubentity { get;}


        /// <summary>
        /// True when this metadata is used to display a list collection
        /// </summary>
        bool IsList { get; set; }

        /// <summary>
        /// True if linked form is a tree
        /// </summary>
        bool IsTree { get; set; }


        /// <summary>
        /// When false main insert copy button is disabled
        /// </summary>
        bool CanInsertCopy { get; set; }

        /// <summary>
        /// When false main Cancel button is disabled
        /// </summary>
        bool CanCancel { get; set; }

        /// <summary>
        /// when false, "mainsave" button is disabled
        /// </summary>
        bool CanSave { get; set; }




        /// <summary>
        /// Delegate for showing messages to generic client. Returns true if user decided to ignore the message and go on.
        /// </summary>
        ShowClientMsgDelegate ShowClientMsg { get; set; }


        /// <summary>
        ///// Sets the form for displaying messages
        ///// </summary>
        ///// <param name="f"></param>
        //void setExternalForm(Form f);

        /// <summary>
        /// Gets the form linked to an edittype
        /// </summary>
        /// <param name="edittype"></param>
        /// <returns></returns>
        Form GetPublicForm(string edittype);

         /// <summary>
        /// Is called when a tree_view is linked to a MetaData in a form
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="T"></param>
        /// <param name="listingType"></param>
        void DescribeTree(TreeView tree, DataTable T, string listingType);

        /// <summary>
        /// Return a custom list form
        /// </summary>
        /// <param name="controller">Controller linked to searchtable</param>
        /// <param name="columnlist">comma separated list of column names to show (or "*")</param>
        /// <param name="mergedfilter">filter to use for getting data</param>
        /// <param name="searchtable">Table in which rows have been searched</param>
        /// <param name="ListingType">Name of listing to be used</param>
        /// <param name="sorting"></param>
        /// <param name="top"></param>
        /// <returns>Custom List Form</returns>        


        ICustomViewListForm GetListForm(IWinFormMetaData linked, string columnlist,
            string mergedfilter, string searchtable, string listingType, string sorting, int top);






        /// <summary>
        /// Edits the current entity using a specified form
        /// If a source row exists, it is automatically updated.
        /// </summary>
        /// <param name="ParentForm">Form parent</param>
        /// <param name="EditType">Name of Form to open</param>
        /// <param name="Modal">true if Modal Form wanted</param>
        /// <returns>True if a modal form has returned DialogResult.Ok</returns>
        bool Edit(Form ParentForm, string EditType, bool Modal);

          /// <summary>
        /// Selects a row from a Table using linked MetaData specified grid-listing
        /// </summary>
        /// <param name="ListingType"></param>
        /// <param name="filter">SQL filter to apply in data retrieving</param>
        /// <param name="searchtable">Table from which data has to be retrieved</param>
        /// <param name="ToMerge">in-memory Table which has some changes to apply to searchtable</param>
        /// <returns>The selected row or null if no row selected </returns>
        /// <remarks> If the entity is selected, a row is loaded in the primary table
        ///  and all other data is cleared. </remarks>
        DataRow SelectOne(string ListingType,
            string filter,
            string searchtable,
            DataTable ToMerge);



        /// <summary>
        /// Returns a row searched by a filter condition if there is only one row that satisfy 
        ///		the filter, and it is a selectable row. Otherwise returns null
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="searchtable"></param>
        /// <returns>A row belonging to a table equal to PrimaryTable</returns>
        DataRow SelectByCondition(string filter,string searchtable);



       
       

    


 
        /// <summary>
        /// true when changes to this entity or to sub-entity of this have been made
        /// </summary>
        bool EntityChanged { get; set; }

        

        
         /// <summary>
        /// 
        /// </summary>
        string EditType { get; set; }

      

        /// <summary>
        /// Sends an error message to the log service
        /// </summary>
        /// <param name="errmsg"></param>
        /// <param name="e"></param>
        [Obsolete("use errorLogger.logException")]
        void LogError(string errmsg, Exception e);

        /// <summary>
        /// Sends an error message to the log service
        /// </summary>
        /// <param name="mess"></param>
        [Obsolete("use errorLogger.logException")]
        void LogError(string mess);

      

      

        /// <summary>
        /// Return R if R is selectable, null otheriwse
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        DataRow CheckSelectRow(DataRow R);


        /// <summary>
        /// Verifies if a certain command can be runned, i.e. if the corrisponding button
        ///  should be "enabled".
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        bool CommandEnabled(string tag);

        /// <summary>
        /// Primary DataTable of the MetaData
        /// </summary>
        DataTable primaryTable { get; }


        /// <summary>
        /// 
        /// </summary>
        string StartFilter { get; set; }



        /// <summary>
        /// String wanted in first list selected row 
        /// </summary>
        string StartValueWanted { get; set; }

        /// <summary>
        /// Fields to which StartWantedValue refers to
        /// </summary>
        string StartFieldWanted { get; set; }


        /// <summary>
        /// Filter calculated by the context menu manager that has opened this form
        /// </summary>
        string ContextFilter { get; }


        /// <summary>
        /// 
        /// </summary>
        object ExtraParameter { get; set; }


        DataRow LastSelectedRow { get; set; }

        DataRow CurrentRow { get; }

    }

    public class WinFormMetaData :MetaData, IWinFormMetaData {

        /// <summary>
        /// Delegate for showing messages to generic client. Returns true if user decided to ignore the message and go on.
        /// </summary>
        public ShowClientMsgDelegate ShowClientMsg { get; set; }


        public bool IsList { get; set; }
        public bool IsTree { get; set; }

        /// <summary>
        /// If true, the list form is not filled when it is filled at start
        /// </summary>
        public bool StartEmpty { get; set; }

        /// <inheritdoc />
        public string ContextFilter { get;internal set;}


        public DataRow CurrentRow {
            get {
                if (PrimaryDataTable == null)
                    return null;
                return PrimaryDataTable._getLastSelected();
            }
        }

        public DataRow LastSelectedRow { get; set; }

        /// <summary>
		/// true when changes to this entity or to sub-entity of this have been made
		/// </summary>
		public bool EntityChanged { get; set; }

        /// <summary>
        ///   true if this is a sub-entity of an entity displayed in a parent form, i.e.   SourceRow is not null
        /// </summary>
        public bool IsSubentity {
            get { return SourceRow != null; }
            //set { subentity = value; }
        }


        /// <summary>
        /// StartFilter is used to filter data collected in lists and trees. 
        /// </summary>
        public string StartFilter { get;set;}

        /// <summary>
        /// String wanted in first list selected row 
        /// </summary>
        public string StartValueWanted { get; set; }

        /// <summary>
        /// Fields to which StartWantedValue refers to
        /// </summary>
        public string StartFieldWanted { get; set; }

     



        public IFormController controller { get; set; }

        /// <summary>
        /// True if SelectOne should not allow to modify input filter
        /// </summary>
        public bool filterLocked { get; set; }

        /// <summary>
        /// True if "mainselect" is enabled
        /// </summary>
        public bool MainSelectionEnabled { get; set; }

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
        public bool CanSave { get; set; }

    


        /// <summary>
        /// Is called when a tree_view is linked to a MetaData in a form
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="T"></param>
        /// <param name="ListingType"></param>
        public virtual void DescribeTree(TreeView tree, DataTable T, string ListingType) {
            describeListType(conn, T, ListingType);
        }

          /// <summary>
        /// Return a custom list form in which the filtercan be locked.
        /// </summary>
        /// <param name="linked">MetaData linked to searchtable</param>
        /// <param name="columnlist">comma separated list of column names to show (or "*")</param>
        /// <param name="mergedfilter">filter to use for getting data</param>
        /// <param name="searchtable">Table in which rows have been searched</param>
        /// <param name="listingType">Name of listing to be used</param>
        /// <param name="sorting"></param>
        /// <param name="filterlocked">if true, the filter is locked and user cannot change listingtype</param>
        /// <param name="toMerge">Rows to "merge" with those found in DB</param>
        /// <returns>List Form</returns>
        ICustomViewListForm getMergeListForm(IWinFormMetaData linked,
            string columnlist,
            string mergedfilter,
            string searchtable,
            string listingType,
            string sorting,
            bool filterlocked,
            DataTable toMerge) {

	        var f = MetaFactory.factory.createInstance<ICustomViewListForm>();
	        f.init(linked,
                columnlist,
                mergedfilter,
                searchtable,
                listingType,
                toMerge,
                sorting,1000,
                filterlocked,LM.listOfName(Name)) ;
	        return f;
        }

       

          /// <summary>
        /// Returns a row searched by a filter condition if there is only one row that satisfy 
        ///		the filter, and it is a selectable row. Otherwise returns null
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="searchtable"></param>
        /// <returns>A row belonging to a table equal to PrimaryTable</returns>
        public virtual DataRow SelectByCondition(string filter,
            string searchtable) {
            //string mergedfilter = GetData.MergeFilters(filter, PrimaryDataTable);

            var resultCount = conn.RUN_SELECT_COUNT(searchtable, filter, true);
            if (resultCount != 1) return null;

            var t2 = conn.RUN_SELECT(TableName, null, null, filter, null, null, true);
            if (t2 == null) return null;
            return t2.Rows.Count == 0 ? null : CheckSelectRow(t2.Rows[0]);
        }


           /// <summary>
        /// Return a custom list form
        /// </summary>
        /// <param name="linked">MetaData linked to searchtable</param>
        /// <param name="columnlist">comma separated list of column names to show (or "*")</param>
        /// <param name="mergedfilter">filter to use for getting data</param>
        /// <param name="searchtable">Table in which rows have been searched</param>
        /// <param name="listingType">Name of listing to be used</param>
        /// <param name="sorting"></param>
        /// <param name="top"></param>
        /// <returns>Custom List Form</returns>
        public virtual ICustomViewListForm GetListForm(IWinFormMetaData linked, string columnlist,
            string mergedfilter, string searchtable, string listingType, string sorting, int top) {

	        var f = MetaFactory.factory.createInstance<ICustomViewListForm>();
            f.init(linked, columnlist,mergedfilter,searchtable,listingType,null,sorting,top,false,"Elenco");
            return f;
        }

         /// <summary>
        /// E' in alternativa al costruttore con IDataAccess, IMetaDataDispatcher e ISecurity
        /// </summary>
        /// <param name="parentForm"></param>
        public void Init(Form parentForm) {
            conn = parentForm.getInstance<IDataAccess>();
            dispatcher = parentForm.getInstance<IMetaDataDispatcher>();
            security = parentForm.getInstance<ISecurity>();
            controller = FormController.GetController(parentForm);
            primaryTable = controller.primaryTable;
            //Name = PrimaryTable;
            QHS = conn.GetQueryHelper();
            ShowClientMsg = controller.WindowsShowClientMsg;
            SourceRow = null;
            EntityChanged = false;
            StartEmpty = false;
            StartFilter = null;
            base.Init();
        }

          /// <summary>
        /// Gets a Form class given its name
        /// </summary>
        /// <param name="dllName"></param>
        /// <returns></returns>
        public static Form GetFormByDllName(string dllName) {
            return WinEntityDispatcher.GetFormByDllName(dllName);

        }

           /// <summary>
        /// Gets the form linked to an edittype
        /// </summary>
        /// <param name="edittype"></param>
        /// <returns></returns>
        virtual public Form GetPublicForm(string edittype) {
            Form F = GetForm(edittype);
            return F;
        }

        ///// <summary>
        ///// Form for displaying messages where this meta has not a form directly associated
        ///// </summary>
        //private Form externalForm = null;

        ///// <summary>
        ///// Sets the form for displaying messages
        ///// </summary>
        ///// <param name="f"></param>
        //public void setExternalForm(Form f) {
        //    externalForm = f;
        //}

        
   

        /// <summary>
        /// Form linked to the MetaData
        /// </summary>
        public Form linkedForm {get;set;}

         /// <summary>
        /// Attach main instances to form, and invokes form controller link
        /// </summary>
        /// <param name="f"></param>
        private void linkToForm(Form f) {
            getData = Get_GetData();
            privateGetData = true;
            linkedForm = f;
            f.attachInstance(getData, typeof(IGetData));//l'ha appena creata

            f.attachInstance(conn, typeof(IDataAccess));//il DataAccess è in input al metadato
            f.attachInstance(dispatcher, typeof(IMetaDataDispatcher));//anche il dispatcher
            f.attachInstance(this, typeof(IMetaData));  //
            f.attachInstance(security, typeof(ISecurity));//Security è preso dal DataAccess
            f.attachInstance(ErrorLogger, typeof(IErrorLogger));//classe statica

            //Se il form ha definito il proprio controller, prende quello
            controller = f.safeGetInstance<IFormController>();
            if (controller == null) {
                controller = f.createInstance<IFormController>();
            }
            else {
                controller.Init(f); //inizializza il formcontroller ove sia stato definito dal form
            }

            //formController.formType = SourceRow!=null? form_types.detail:form_types.detail;

            //form Controller ottiene dal form le istanze di 
            //  IDataAccess IGetData IMetaData ISecurity dispatcher IErrorLogger
            //  Di base il controller potrebbe essere creato con un DataAccess ed un Dispatcher + metadato
            ds = f.getInstance<DataSet>();
            if (ExtraParameter != null) {
                metaModel.setExtraParams(controller.primaryTable, ExtraParameter);
            }

            //Ora può fare l'InitClass del GetData, prima non aveva il DataSet
            getData.InitClass(ds, conn, TableName);
            controller.doLink(); //crea IHelpForm            
            helpForm = f.getInstance<IHelpForm>();
            eventManager = f.getInstance<IFormEventsManager>();
        }

        /// <summary>
        /// Extra parameter eventually given by caller (with field parameter of
        ///  Edit() function)
        /// </summary>
        public object ExtraParameter { get;set;}


        IHelpForm helpForm;
        IFormEventsManager  eventManager;
        
           /// <summary>
           /// Gets the primary key fields of the main table
           /// </summary>
           /// <returns></returns>
        public override string[] PrimaryKey() {
            if (controller.primaryTable?.PrimaryKey.Length > 0) {
                return (from s in controller.primaryTable.PrimaryKey.ToArray() select s.ColumnName).ToArray();
            }

            return base.PrimaryKey();
        }


        /// <summary>
        /// Destroy and unlink this MetaData from anything
        /// </summary>
        public override void Destroy() {
            base.Destroy();
            ExtraParameter = null;
            SourceRow = null;
        }


        #region Gestione Form Detail: GetSourceChanges, SourceRow

        /// <summary>
        /// Gets the main row from a detail in Parent form 
        /// This row has to be updated in the parent form when the editing of this 
        ///  entity has been completed, i.e. this entity is a sub-entity of another entity 
        ///  currently being edited in a parent form.
        /// </summary>
        /// <param name="row">Row to import in the primary table</param>
        public virtual DataRow SourceRow { get; set; }

             
        /// <summary>
        /// Row of CURRENT DataSet mapped to the SourceRow (which belongs to the PARENT DataSet)
        /// </summary>
        public virtual DataRow NewSourceRow { get;set;}

       


        #endregion


        bool checkConn() {
            if (conn == null) return false;
            if (conn.Open()) {
                conn.Close();
                return true;
            }

            return false;
        }

        public IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();
        public DataTable primaryTable { get; set; }

        /// <summary>
        /// Edit type of current form
        /// </summary>
        public string edit_type;

        /// <summary>
        /// Edit type of current form
        /// </summary>
        public string EditType {get;set;}


        /// <summary>
        /// Edits the current entity using a specified form.
        /// If a source row exists, it is automatically updated.
        /// </summary>
        /// <param name="parentForm">Form parent</param>
        /// <param name="editType">Name of Form to open</param>
        /// <param name="modal">true if Modal Form wanted</param>
        /// <returns>True if a modal form has returned DialogResult.Ok</returns>
        // ReSharper disable once ParameterHidesMember
        public virtual bool Edit(Form parentForm, string editType, bool modal) {
            //Ci sono  molti controlli perchè questo è uno dei metodi chiamati dopo che il programma rimane acceso tutta la notte e la connessione 
            // spesso è caduta. O se uno decide di aprire un form ma poi cambia idea e chiude il programma o spegne il computer
            //Essenzialmente è un involucro per caricare il form e poi chiamare doEdit
            if (destroyed) return false;
            if (controller != null && controller.isClosing) return false;

            //Se connessione assente esci 
            checkConn();
            if ((conn != null) && (conn.openError)) {
                shower.Show(LM.dbConnectionInterrupted);
                ErroreIrrecuperabile = true;
                return false;
            }

            var f = GetForm(editType); //ottiene il form dall'override di GetForm
            if (f == null) return false;
            if (parentForm != null && parentForm.IsDisposed) return false;
            this.edit_type = editType;
            try {
                return doEdit(parentForm, f, modal);
            }
            catch (Exception e) {
                if (parentForm != null && parentForm.IsDisposed) parentForm = null;
                shower.ShowException(parentForm, null, e);
                LogError($"{Name}.Edit({editType},{modal}) error.", e);
            }

            return false;
        }

        /// <summary>
        /// Edits the entity
        /// </summary>
        /// <param name="parent">Parent Form, who is calling this function </param>
        /// <param name="f">Form to edit the entity</param>
        /// <param name="modal">true when Form has to be opened as Modal</param>
        /// <returns>true when a modal form has returned DialogResult.Ok</returns>
        private bool doEdit(Form parent, Form f, bool modal) {
            //Effettua prima il link al form, poi lo mostra e poi lo attiva
            try {
                linkToForm(f);
            }
            catch (Exception e) {
                var err = $@"Errore building form {primaryTable} : {e.ToString()}";
                shower.ShowException(linkedForm, err, e);
                LogError(err, e);
                return false;
            }

            if (ErroreIrrecuperabile) return false;
            if (f == null || f.IsDisposed) return false;

            controller.setColor(f, true);

            if (parent != null && parent.IsDisposed) return false;
            f.FormClosed += F_FormClosed;

            var res = showForm(f, parent, modal);


            return res == System.Windows.Forms.DialogResult.OK;

        }

        private void F_FormClosed(object sender, FormClosedEventArgs e) {
            if (sender is Form f) {
                f.FormClosed -= F_FormClosed;
                var meta = f.getInstance<MetaData>();
                meta.Destroy();
            }

        }



        /// <summary>
        /// Shows a form inside a parent form
        /// </summary>
        /// <param name="f">Form to display</param>
        /// <param name="parent">container form</param>
        /// <param name="modal"></param>
        /// <returns>ShowDialog result or none if non modal form</returns>
        System.Windows.Forms.DialogResult showForm(Form f, Form parent, bool modal) {
            var res = System.Windows.Forms.DialogResult.None; //Usato nel caso form non modale
            var mdi = (parent != null && parent.IsMdiContainer);
            if (parent != null) f.Icon = parent.Icon;

            var posx = 0;
            if (parent != null) {
                if (parent.WindowState == FormWindowState.Minimized) parent.WindowState = FormWindowState.Normal;
                posx = (parent.ClientSize.Width - f.Size.Width - 8) / 2;
                if (posx < 0) posx = 0;
            }

            if (mdi) {
                if (modal) {
                    f.StartPosition = FormStartPosition.CenterParent;
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, parent);
                    res = f.ShowDialog(parent);
                }
                else {
                    f.MdiParent = parent;
                    f.StartPosition = FormStartPosition.Manual;
                    f.Location = new Point(posx, 0);
                    if (f.FormBorderStyle != FormBorderStyle.FixedSingle) {
                        f.FormBorderStyle = FormBorderStyle.Sizable;
                        f.AutoScroll = true;
                        f.MaximizeBox = true;
                        f.MinimizeBox = true;
                        f.Show();
                        f.AutoScrollMinSize = new Size(f.ClientSize.Width, f.ClientSize.Height);
                    }
                    else {
                        f.Show();
                    }
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, null);
                    f.Activate();
                }
            }
            else {
                f.StartPosition = FormStartPosition.CenterParent;
                if (modal) {
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, parent);
                    res = f.ShowDialog(parent);
                    if (parent != null) {
                        parent.Select();
                        parent.BringToFront();
                    }
                }
                else {
                    parent?.AddOwnedForm(f);
                    if (f.FormBorderStyle != FormBorderStyle.FixedSingle) {
                        f.FormBorderStyle = FormBorderStyle.Sizable;
                        f.AutoScroll = true;
                        f.MaximizeBox = true;
                        f.MinimizeBox = true;
                        f.Show();
                        f.AutoScrollMinSize = new Size(f.ClientSize.Width, f.ClientSize.Height);
                    }
                    else {
                        f.Show();
                    }
                    MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f, null);
                    f.Activate();
                }
            }



            return res;
        }


        /// <summary>
        /// Gets the form named FormName to edit this entity
        /// </summary>
        /// <param name="EditType">Logical name of the form wanted</param>
        /// <returns></returns>
        virtual protected Form GetForm(string EditType) {
            if (conn == null) return null;
            DataRow formDesc = conn.GetFormInfo(this.TableName, EditType);
            if (formDesc != null) {
                Form f = GetFormByDllName(formDesc["dllname"].ToString());
                if (f != null) {
                    if (formDesc["caption"] != DBNull.Value)
                        Name = formDesc["caption"].ToString();
                    if (formDesc["list"].ToString().ToUpper() == "S") {
                        IsList=true;
                        if (formDesc["startempty"].ToString().ToUpper() == "S")
                            StartEmpty = true;
                        if (formDesc["tree"].ToString().ToUpper() == "S")
                            IsTree = true;
                    }

                    if (formDesc["searchenabled"].ToString().ToUpper() == "S") {
                        SearchEnabled = true;
                        if (formDesc["defaultlisttype"] != DBNull.Value)
                            DefaultListType = formDesc["defaultlisttype"].ToString();
                    }
                    else {
                        SearchEnabled = false;
                    }

                    return f;
                }
            }

            var err = "GetForm(" + EditType + ") called on " + this.Name + " but GetForm() was not ovverridden.";
            if (EditTypes != null) {
                err += "Available edittypes:";
                foreach (string ed in this.EditTypes) {
                    err += ed + ",";
                }

            }

            err += "Assembly:" + this.GetType().Assembly.FullName;
            ErrorLogger.markEvent(err);
            ErrorLogger.logException(err, meta: this);

            return null;
        }

            /// <summary>
        /// Verifies if a certain command can be runned, i.e. if the corrisponding button
        ///  should be "enabled".
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool CommandEnabled(string tag) {
            if (controller.isClosing) return false;
            if (destroyed) return false;
            if (dispatcher != null && dispatcher.unrecoverableError) return false;
            var cmd = mdl_utils.tagUtils.GetFieldLower(tag, 0);
            var someData = (helpForm != null) && (HelpForm.GetLastSelected(controller.primaryTable) != null);
            DataRow currRow = null;
            if (someData) currRow = HelpForm.GetLastSelected(controller.primaryTable);
            if ((conn != null) && (conn.openError)) return false;

            // Input potrebbe essere dato dalla PrimaryDataTable
            // L'input sin qua è dato dalla  GetLastSelected(PrimaryDataTable)
            if (cmd == "crea_ticket") return helpdeskEnabled;

            if (ErroreIrrecuperabile || dispatcher.unrecoverableError) return false;

            switch (cmd) {
                case "mainselect":
                    if (!MainSelectionEnabled) return false;
                    if (currRow == null) return false;
                    return security.CanSelect(currRow);
                case "editnotes":
                    if (currRow==null) return false;
                    if (!(HasNotes() || HasOleNotes())) return false; //input sin qua è dato dalla  GetLastSelected(PrimaryDataTable)
                    return true;
                case "addnotes":
                    if (currRow == null) return false;
                    if (!(HasNotes() || HasOleNotes())) return false; //input sin qua è dato dalla  GetLastSelected(PrimaryDataTable)
                    return true;
                case "maininsert":
                    if (!CanInsert) return false;
                    //if (!CanSave) return false;
                    if (IsSubentity) return false;
                    if (controller.formState == form_states.insert) return false;
                    if (security.CantUnconditionallyPost(controller.primaryTable, "I")) return false;
                    return true;
                case "maininsertcopy":
                    if (!CanInsert) return false;
                    if (!CanInsertCopy) return false;
                    //if (!CanSave) return false;
                    if (IsSubentity) return false;
                    if (controller.formState == form_states.insert) return false;
                    if (controller.formState == form_states.setsearch) return false;
                    if (!someData) return false;
                    if (security.CantUnconditionallyPost(controller.primaryTable, "I")) return false;
                    return true;
                case "maindosearch":
                    if (SearchEnabled == false) return false;
                    if (IsSubentity) return false;
                    if (controller.formState != form_states.setsearch) return false;
                    return true;
                case "emptylist":
                    if (SearchEnabled == false) return false;
                    if (IsSubentity) return false;
                    if (controller.formState != form_states.setsearch) return false;
                    return true;
                case "mainsetsearch":
                    if (SearchEnabled == false) return false;
                    if (IsSubentity) return false;
                    return true;
                case "gotonext":
                    return controller?.hasNext() ?? false;
                case "gotoprev":
                    return controller?.hasPrev() ?? false;
                case "showlast":
                    return !(controller.formState == form_states.setsearch);
                case "mainsave":
                    if (!CanSave) return false;
                    if (controller.formState == form_states.setsearch) return false;
                    if (!someData) return false;
                    if (currRow.RowState == DataRowState.Added) {
                        return !security.CantUnconditionallyPost(controller.primaryTable, "I");
                    }

                    return security.CanPost(currRow);
                case "maindelete":
                    if (!CanSave) return false;
                    if (!CanCancel) return false;
                    if (controller.formState == form_states.setsearch) return false;
                    if ((controller.formState == form_states.edit) && IsSubentity) return false;
                    if (!someData) return false;
                    if (currRow.RowState == DataRowState.Added) return true;
                    return security.CanPost(currRow);
                case "mainrefresh":
                    if (!controller.MainRefreshEnabled) return false;
                    if (IsTree) return false;
                    return true;
                case "horizwin":
                    if (linkedForm == null) return false;
                    if (!linkedForm.IsMdiChild) return false;
                    if (linkedForm.Modal) return false;
                    var main = linkedForm.MdiParent;
                    if (main == null) return false;
                    return true;
                case "crea_ticket":
                    return helpdeskEnabled;
                default: return false;

            }
        }

        /// <summary>
        /// Selects a row from a Table using linked MetaData specified grid-listing
        /// </summary>
        /// <param name="listingType"></param>
        /// <param name="filter">SQL filter to apply in data retrieving</param>
        /// <param name="searchtable">Table from which data has to be retrieved</param>
        /// <param name="toMerge">in-memory Table which has some changes to apply to searchtable</param>
        /// <returns>The selected row or null if no row selected </returns>
        /// <remarks> If the entity is selected, a row is loaded in the primary table
        ///  and all other data is cleared. </remarks>
        public virtual DataRow SelectOne(string listingType,
            string filter,
            string searchtable,
            DataTable toMerge = null
            ) {
            if (destroyed) return null;
            if (searchtable == null) searchtable = primaryTable.TableName;
            var columnlist = "*";
            var mergedfilter = filter;

            if (listingType == null) {
                try {
                    var err =
                        $"ListingType=null calling SelectOne on table {searchtable} with filter {filter} in form {linkedForm.Text}";
                    ErrorLogger.markEvent(err);
                }
                catch {
                    //ignore
                }
            }
            else {

                if (ManagedByDB) {
                    listingType = conn.GetListType(out var dbs, searchtable, listingType);
                    searchtable = DataAccess.PrimaryTableOf(dbs);
                    var qhc = new CQueryHelper();
                    var viewFound = dbs.customview.Select(qhc.CmpEq("viewname",listingType));
                    if (viewFound.Length > 0) {
                        var staticfilter = viewFound[0]["staticfilter"].ToString().Trim();
                        staticfilter = security.Compile(staticfilter, true);
                        if (staticfilter != "") mergedfilter = GetData.MergeFilters(mergedfilter, staticfilter);
                    }
                }

            }

            WinFormMetaData metaToConsider = this;
            if (searchtable != primaryTable.TableName) {
                metaToConsider = dispatcher.Get(searchtable) as WinFormMetaData;
                if (dispatcher.unrecoverableError) {
                    ErroreIrrecuperabile = true;
                    shower.ShowError(linkedForm, LM.errorLoadingMeta(searchtable), LM.ErrorTitle);
                    //$"Errore nel caricamento del metadato {searchtable} è necessario riavviare il programma.","Errore");
                }

                metaToConsider.listTop = this.listTop;
            }

            string sortBy = metaToConsider.GetSorting(listingType);

            if (ds?.Tables[searchtable] != null) {
                metaToConsider.DescribeColumns(ds.Tables[searchtable], listingType);
                mergedfilter = GetData.MergeFilters(mergedfilter, ds.Tables[searchtable]);
                columnlist = QueryCreator.SortedColumnNameList(ds.Tables[searchtable]);
                sortBy = sortBy ?? ds.Tables[searchtable].getSorting();
            }
            else {
                var temp = conn.CreateTableByName(searchtable, "*");
                if (temp.PrimaryKey == null || temp.PrimaryKey.Length == 0) {
                    if (metaToConsider.PrimaryKey() != null && metaToConsider.PrimaryKey().Length > 0) {
                        temp.PrimaryKey =
                            (from fName in metaToConsider.PrimaryKey() select temp.Columns[fName]).ToArray();
                    }
                }
                metaToConsider.DescribeColumns(temp, listingType);
                columnlist = QueryCreator.SortedColumnNameList(temp);
            }

            string prefilter = mergedfilter;
            if (!this.ManagedByDB) {
                var staticfilter = metaToConsider.GetStaticFilter(listingType);
                staticfilter = security.Compile(staticfilter, true);
                mergedfilter = GetData.MergeFilters(mergedfilter, staticfilter);
            }

            if (filter == null) filter = "";
            filter = filter.Trim();
            if (metaToConsider.listTop != 0 || filterLocked) {
                var tabTemp = conn.RUN_SELECT(searchtable, "*", null, mergedfilter,"2", true);
                var resultCount = tabTemp.Rows.Count;// dbConn.RUN_SELECT_COUNT(searchtable, mergedfilter, true);
                if ((toMerge == null) && (resultCount == 0)) {
                    conn.Close();
                    var mess = LM.noRowFoundInTable(searchtable); //$"Nella tabella \'{searchtable}\' non è stata trovata alcuna riga.\r\n";
                    mess += filter != ""
                        ? LM.conditionSetWas(mergedfilter) //La condizione di ricerca impostata era: \'{filter}\'.
                        : LM.noConditionUsed;  //Nessuna condizione è stata usata.
                    if (listingType != null) mess += LM.listNameIs(listingType);    //Nome Elenco: \'{listingType}\'.
                    var shortmsg = metaToConsider.GetNoRowFoundMessage(listingType);
                    shower.ShowNoRowFound(linkedForm, shortmsg, mess);
                    return null;
                }

                //When an external table is present, always display a list (no implicit selection done)
                if (resultCount == 1 &&
                    (toMerge == null || toMerge.Rows.Count == 0)
                ) { //ex ((ToMerge==null)&&
                    var T = conn.RUN_SELECT(searchtable, columnlist, sortBy, mergedfilter, null, null,
                        true); //mergefilter
                    return T.Rows.Count == 0 ? null : metaToConsider.CheckSelectRow(T.Rows[0]);
                }
            }

            conn.Close();
            if (filterLocked) {
                var frm = getMergeListForm(this,
                    columnlist, prefilter, searchtable, listingType, sortBy,
                    filterLocked, toMerge);

                if (primaryTable.TableName != searchtable) {
                    metaToConsider.shower = shower;
                }

                frm.setStartPosition(FormStartPosition.CenterScreen);
                var res = frm.ShowDialog(linkedForm);

                return res != System.Windows.Forms.DialogResult.OK ? null : CheckSelectRow(frm.getLastSelectedRow());
            }
            else {
                //				frmElencoCustom frm = 
                //					new frmElencoCustom(this, columnlist, mergedfilter, searchtable, ListingType );
                controller.closeDisabled = true;
                var frm = GetListForm(this, columnlist, prefilter, searchtable, listingType, sortBy, listTop);


                //frm.Text= "Elenco "+ this.Name;			

                frm.setFormPosition(linkedForm, controller);

                controller.SetNewListForm(frm);


                frm.show();

                frm.selectSomething();

                controller.closeDisabled = false;
                return null;

            }
        }


        /// <summary>
        /// Return R if R is selectable, null otheriwse
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public virtual DataRow CheckSelectRow(DataRow r) {
            if (r != null && !CanSelect(r)) {
                shower.Show(LM.couldNotSelectRow, LM.ErrorTitle, mdl.MessageBoxButtons.OK);
                //MetaFactory.factory.getSingleton<IMessageShower>().Show(LinkedForm, "La voce selezionata non poteva essere scelta.");
                return null;
            }

            return r;
        }





    }
}

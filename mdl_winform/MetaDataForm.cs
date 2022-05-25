using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LM=mdl_language.LanguageManager;
using mdl;

namespace mdl_winform {
    public partial class MetaDataForm : Form {

        /// <summary>
        /// Linked metadata
        /// </summary>
        public virtual IMetaData meta {
            get { return this.getInstance<IMetaData>(); }
        }

        /// <summary>
        /// Form controller
        /// </summary>
        public virtual IFormController controller {
            get { return this.getInstance<IFormController>(); }
        }

        /// <summary>
        /// DataAccess
        /// </summary>
        public virtual IDataAccess conn {
            get { return this.getInstance<IDataAccess>(); }
        }


        /// <summary>
        /// Factory
        /// </summary>
        public virtual IMetaFactory factory {
	        get { return MetaFactory.factory; }
        }

        /// <summary>
        /// Security class
        /// </summary>
        public ISecurity security {
            get { return this.getInstance<ISecurity>(); }
        }

        /// <summary>
        /// QueryHelper to filter datatables
        /// </summary>
        public CQueryHelper qhc { get; } = new CQueryHelper();

        private QueryHelper _qhs;

        /// <summary>
        /// QueryHelper to compose sql filter
        /// </summary>
        public virtual QueryHelper qhs {
            get {
                if (_qhs != null) return _qhs;
                _qhs = conn?.GetQueryHelper();
                return _qhs;
            }

        }

        /// <summary>
        /// Esercizio environment variable
        /// </summary>
        public virtual int esercizio {
            get { return (int) security.GetSys("esercizio"); }
        }

        /// <summary>
        /// dataContabile environment variable
        /// </summary>
        public virtual DateTime dataContabile {
            get { return (DateTime) security.GetSys("datacontabile"); }
        }

        /// <summary>
        /// MetaModel used
        /// </summary>
        public virtual IMetaModel model {
            get { return MetaFactory.factory.getSingleton<IMetaModel>(); }
        }

        /// <summary>
        /// Linked helpForm class
        /// </summary>
        public virtual IHelpForm helpForm {
            get { return this.getInstance<IHelpForm>(); }
        }


        /// <summary>
        /// True if empty
        /// </summary>
        public virtual bool isEmpty {
            get {
                return controller.IsEmpty;
            }
        }

        /// <summary>
        /// True if InsertMode
        /// </summary>
        public virtual bool insertMode {
            get {
                return controller.InsertMode;
            }
        }

        /// <summary>
        /// True if EditMode
        /// </summary>
        public virtual bool editMode {
            get {
                return controller.EditMode;
            }
        }

        /// <summary>
        /// True if the form is under user control
        /// </summary>
        public virtual bool drawStateIsDone {
            get {
                return controller.DrawStateIsDone;
            }
        }

        

        /// <summary>
        /// Gets data from linked Form control, returning false if some errors occured
        /// </summary>
        /// <param name="quick">true if no validity checks have to be made</param>
        /// <returns>true on success</returns>
        public virtual bool getFormData(bool quick=true) {
            return controller.GetFormData(quick);
        }
        /// <summary>
        /// True if the form is under user control
        /// </summary>
        public virtual bool firstFillForThisRow {
            get {
                return controller.firstFillForThisRow;
            }
        }

        /// <summary>
        /// Apply a filter on a table during any further read
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        public virtual void setStaticFilter(DataTable T, object filter) {
	        if (filter is MetaExpression) filter = toString(filter as MetaExpression);
            model.SetStaticFilter(T, filter);
        }

        /// <summary>
        /// Set Table T to be read once for all when ReadCached will be called next time
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="addBlankRow">when true, a blank row is added as first row of T</param>
        public virtual void cacheTable(DataTable T, object filter, string sort=null, bool addBlankRow=true) {
            model.CacheTable(T, filter, sort, addBlankRow);
        }


        /// <inheritdoc />
        public virtual void setExtraParams(DataTable t, object o) {
            t.ExtendedProperties["ExtraParameters"] = o;
        }

        /// <inheritdoc />
        public virtual object getExtraParams(DataTable t) {
            return t.ExtendedProperties["ExtraParameters"];
        }

        /// <summary>
        /// Extra parameter eventually given by caller (with field parameter of
        ///  Edit() function)
        /// </summary>
        public object ExtraParameter {get; set;}


        /// <summary>
        /// Set the table as NotEntitychild. So the table isn't cleared during freshform and refills
        /// </summary>
        /// <param name="child"></param>
        public virtual void addNotEntityChild( DataTable child) {
            child.SetDenyClear();
            model.AddNotEntityChildFilter(controller.primaryTable, child);
        }


        /// <summary>
        /// Remove a table from being a  NotEntitychild
        /// </summary>
        /// <param name="T"></param>        
        public virtual void unMarkTableAsNotEntityChild(DataTable T) {
            model.UnMarkTableAsNotEntityChild(T);

        }

        /// <summary>
        /// Returns a single value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public virtual object readValue(string table, object filter, string expr) {
            
            return conn.ReadValue(table, (MetaExpression) filter, expr, orderBy:null);
            
        }


        /// <summary>
        /// Returns a value executing a generic sql command 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual object ExecuteScalar(string cmd) {
            return conn.ExecuteScalar(cmd).GetAwaiter().GetResult();
        }




        /// <summary>
        ///  Executes a generic SQL command that returns a Table
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="silent">set true non visualizza messaggi di errore</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public virtual DataTable ExecuteQuery(string sqlCommand, bool silent=true, int timeout=-1) {
            try {
                return conn.ExecuteQuery(sqlCommand, timeout: timeout).GetAwaiter().GetResult();
            }
            catch (Exception e) {
                var errmsg = e.ToString();
                if (!silent)shower.ShowError(null, LM.errorRunningCommand(sqlCommand), errmsg);
            }
            
            return null;
        }

      


        /// <summary>
        /// Reads data into an existing table
        /// </summary>
        /// <param name="T">Table into which data will be read</param>
        /// <param name="sortBy">sorting for db reading</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="top"></param>
        public virtual void selectIntoTable(DataTable T, object filter, string sortBy=null, string top=null) {
            conn.SelectIntoTable(T, orderBy:sortBy,filter:filter, top:top).GetAwaiter().GetResult();          
        }

        /// <summary>
        /// Reads data into a given table, skipping temporary columns
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sql">sorting for db reading</param>
        public virtual void ExecuteQueryIntoTable(DataTable T, string sql) {
            conn.ExecuteQueryIntoTable(T, sql).GetAwaiter().GetResult();
        }

       
        /// <summary>
        /// Reads data into a table. The table is created at run-time using information
        ///  stored in columntypes
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist">list of field names separated by commas</param>
        /// <param name="sortBy">list of field names separated by commas</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="top">how many rows to get</param>
        public virtual DataTable getTable(string tablename,
            object filter=null,
            string columnlist=null,
            string sortBy=null,
            string top=null) {
            return conn.Select(tablename, columnlist:columnlist,order_by: sortBy, filter: filter, top:top).GetAwaiter().GetResult();           
        }


        
        /// <summary>
        /// Executes a SELECT COUNT on a table.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual int selectCount(string tablename, MetaExpression filter) {
            return conn.Count(tablename, filter:filter).GetAwaiter().GetResult() ;            
        }



         /// <summary>
        /// Removes all rows that can't be selected
        /// </summary>
        /// <param name="T"></param>
        public virtual void deleteAllUnselectable(DataTable T) {
            security.DeleteAllUnselectable(T);
        }


        
        /// <summary>
        /// Check if current user has system administration privileges
        /// </summary>
        /// <returns></returns>
        public virtual bool isSystemAdmin() {
            return security.IsSystemAdmin();
        }

        /// <summary>
        /// Check if a generic write operation is allowed on a row. The operation depends on the row status
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public virtual bool canPost( DataRow r) {
            if (model.IsSkipSecurity(r.Table)) return true;
            var T = DataAccess.SimplifiedTableClone(r.Table);
            T.TableName = r.Table.tableForPosting();
            DataSetUtils.SafeImportRow(T, r);
            return security.CanPost(T.Rows[0]);
        }



        /// <summary>
        /// Get a system environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual object getSys(string name) {
            return security?.GetSys(name);
        }

        /// <summary>
        /// Set a system environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="O">value to set</param>
        public virtual void setSys(string name, object O) {
            security?.SetSys(name, O);
        }

       

        /// <summary>
        /// Get a user environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public object getUsr(string name) {
            return security?.GetUsr(name);
        }

        /// <summary>
        /// Set a user environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="O">value to set</param>
        public void setUsr(string name, object O) {
            security?.SetUsr(name, O);                                                                 
        }



        /// <summary>
        ///  Check if a specified row of a table can be selected
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public virtual bool canSelect(DataRow r) {
            if (model.IsSkipSecurity(r.Table)) return true;
            var T = DataAccess.SimplifiedTableClone(r.Table);
            DataSetUtils.SafeImportRow(T, r);
            return security.CanSelect(T.Rows[0]);
        }


        /// <summary>
        /// Creates a DataTable given it's db name
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnList">list of columns to include in the table</param>
        /// <returns></returns>
        public virtual DataTable createTable(string tablename, string columnList=null) {
            return conn.CreateTable(tablename, columnList).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a filter that compares every single field of an object 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public virtual MetaExpression mCmp(object o) {
            return MetaExpression.mCmp(o);
        }

        /// <summary>
        /// Creates a filter that compares specified fields of an object 
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public virtual MetaExpression mCmp(DataRow sample, params string[] fields){
            return MetaExpression.mCmp(sample,fields);
        }

        /// <summary>
        /// Returns a filter that compares two expression
        /// </summary>
        /// <param name="par1"></param>
        /// <param name="par2"></param>
        /// <returns></returns>
        public virtual MetaExpression eq(object par1, object par2) {
            return MetaExpression.eq(par1, par2);
        }

        /// <summary>
        /// Returns a filter that checks two expressions for inequality
        /// </summary>
        /// <param name="par1"></param>
        /// <param name="par2"></param>
        /// <returns></returns>
        public virtual MetaExpression ne(object par1, object par2) {
            return MetaExpression.ne(par1, par2);
        }

        /// <summary>
        /// Return a an expression that checks if an expression is null
        /// </summary>
        /// <param name="par"></param>
        /// <returns></returns>
        public virtual MetaExpression isNull(object par) {
            return MetaExpression.isNull(par);
        }

        /// <summary>
        /// Return a an expression that checks if an expression is not null
        /// </summary>
        /// <param name="par"></param>
        /// <returns></returns>
        public virtual MetaExpression isNotNull(object par) {
            return MetaExpression.isNotNull(par);
        }

        /// <summary>
        /// keyCmp(obj,fields) is a shortcut for (r[field1]=sample[field1) and (r[field2]=sample[field2]) and... for 
        ///   each primary key column
        /// </summary>
        /// <param name="o"></param>
        public virtual MetaExpression keyCmp(DataRow o) {
            return MetaExpression.keyCmp(o);
        }

        /// <summary>
        /// returns current row edited in the form
        /// </summary>
        public DataRow currentRow {
            get {
                return isEmpty? null:  HelpForm.GetLastSelected(controller.ds.Tables[meta.TableName]);
            }
        }


        /// <summary>
        /// Refills the form. If RefreshPeripherals is set to true, secondary tables
        ///  are read again from DB (i.e. all tables in the view that are not
        ///  cached, primary or child of primary).
        /// </summary>
        /// <param name="refreshPeripherals">when true, not -entity-or-cached- tables are cleared and read again from DB</param>
        /// <param name="doPrefill">When true, also prefill is done, this is more expensive and should be done only once in a form</param>
        public virtual void freshForm(bool refreshPeripherals=false, bool doPrefill=false) {
            controller.FreshForm(refreshPeripherals, doPrefill);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(string text) {
            return shower.Show(text);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(string text, string caption) {
            return shower.Show(text, caption);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(string text, string caption, mdl.MessageBoxButtons btns) {
            return shower.Show(text, caption, btns);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <param name="icons"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(string text, string caption, mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons) {
            return shower.Show(text, caption, btns, icons);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <param name="icons"></param>
        /// <param name="defBtn"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(string text, string caption, mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons, mdl.MessageBoxDefaultButton defBtn) {
            return shower.Show(text, caption, btns, icons, defBtn);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(IWin32Window ctrl, string text) {
            return shower.Show(ctrl, text);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(IWin32Window ctrl, string text, string caption) {
            return shower.Show(ctrl, text, caption);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(IWin32Window ctrl, string text, string caption, mdl.MessageBoxButtons btns) {
            return shower.Show(ctrl, text, caption, btns);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <param name="icons"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(IWin32Window ctrl, string text, string caption, mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons) {
            return shower.Show(ctrl, text, caption, btns, icons);
        }

        /// <summary>
        /// Shows a message
        /// </summary>
        /// <param name="ctrl"></param>
        /// <param name="text"></param>
        /// <param name="caption"></param>
        /// <param name="btns"></param>
        /// <param name="icons"></param>
        /// <param name="defBtn"></param>
        /// <returns></returns>
        public virtual mdl.DialogResult show(IWin32Window ctrl, string text, string caption, mdl.MessageBoxButtons btns, mdl.MessageBoxIcon icons, mdl.MessageBoxDefaultButton defBtn) {
            return shower.Show(ctrl, text, caption, btns, icons, defBtn);
        }

        /// <summary>
        /// Shows an Exception
        /// </summary>
        /// <param name="f"></param>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        /// <param name="logUrl"></param>
        public virtual void showException(Form f, string msg, Exception e, string logUrl=null) {
            shower.ShowException(f,msg,e,logUrl);
        }

        /// <summary>
        /// Show an error with optional detail message
        /// </summary>
        /// <param name="F"></param>
        /// <param name="MainMessage"></param>
        /// <param name="LongMessage"></param>
        /// <param name="logUrl"></param>
        public virtual void showError(Form F, string MainMessage, string LongMessage=null, string logUrl=null) {
            shower.ShowError(F,MainMessage,LongMessage,logUrl);
        }

        /// <summary>
        /// Show an error with optional detail message
        /// </summary>
        /// <param name="MainMessage"></param>
        /// <param name="LongMessage"></param>
        /// <param name="logUrl"></param>
        public virtual void showError(string MainMessage, string LongMessage=null, string logUrl=null) {
            shower.ShowError(null,MainMessage,LongMessage,logUrl);
        }

        /// <summary>
        /// Converts a metaexpression into a sql string using currente dataAccess class
        /// </summary>
        /// <param name="filter"></param>
        public virtual string toString(MetaExpression filter) {
            return filter?.toSql(qhs, conn.Security);
        }
        /// <summary>
        /// Message shower
        /// </summary>
        public IMessageShower shower = MetaFactory.factory.getSingleton<IMessageShower>();

        /// <summary>
        /// OpenFileDialog
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public IOpenFileDialog createOpenFileDialog(OpenFileDialog d) {
            return factory.create<IOpenFileDialog>().init(d);
        }

        /// <summary>
        /// SaveFileDialog
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public ISaveFileDialog createSaveFileDialog(SaveFileDialog d) {
	        return factory.create<ISaveFileDialog>().init(d);
        }

        /// <summary>
        /// FolderBrowserDialog
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public IFolderBrowserDialog createFolderBrowserDialog(FolderBrowserDialog d) {
	        return factory.create<IFolderBrowserDialog>().init(d);
        }

        /// <summary>
        /// ProcessRun
        /// Server Side execute a .bat or open a document
        /// Web Side execute a .bat or download a document
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="onClient"></param>
        public void runProcess(string fileName,bool onClient) {
	        factory.getSingleton<IProcessRunner>().start(fileName, onClient);
        }

        /// <summary>
        /// Create Form
        /// </summary>
        /// <param name="F"></param>
        /// <param name="Parent"></param>
        public void createForm(Form F, Form Parent) {
            factory.getSingleton<IFormCreationListener>().create(F, Parent);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MetaDataForm() {
        }
    }
}

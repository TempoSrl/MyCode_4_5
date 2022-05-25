using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#pragma warning disable IDE1006 // Naming Styles
using mdl;
using q = mdl.MetaExpression;

namespace mdl_winform {
    /// <summary>
    /// Current operation
    /// </summary>
    public enum mainoperations {
        /// <summary>
        /// editing a search mask
        /// </summary>
        setsearch,

        /// <summary>
        /// doing a mainsearch
        /// </summary>
        search,

        /// <summary>
        /// Doing a main delete
        /// </summary>
        delete,

        /// <summary>
        /// Doing a main insert
        /// </summary>
        insert,

        /// <summary>
        /// Doing a mainsave
        /// </summary>
        save,

        /// <summary>
        /// Doing nothing
        /// </summary>
        none
    };


    /// <summary>
    /// Possible states for a form
    /// </summary>
    public enum form_states {
            /// <summary>
            /// Current row is an added row
            /// </summary>
            insert,

            /// <summary>
            /// Current Row is unchanged or modified
            /// </summary>
            edit,

            /// <summary>
            /// There is no current row. The form is empty
            /// </summary>
            setsearch
        };

        /// <summary>
        /// Current Draw-State. It describe in some detail what is happening to 
        ///		the linked form
        /// </summary>
        public enum form_drawstates {
            /// <summary>
            /// The form is been cleared
            /// </summary>
            clearing,

            /// <summary>
            /// The form is being filled
            /// </summary>
            filling,

            /// <summary>
            /// The form is being prefilled
            /// </summary>
            prefilling,

            /// <summary>
            /// Form is being built up
            /// </summary>
            building,

            /// <summary>
            /// The form is under user control
            /// </summary>
            done
        };


    /// <summary>
    /// Manages interaction between a form and the user
    /// </summary>
    public interface IFormController:IFormInit {

        bool IsClearBeforeInsert { get; set; }

        bool closeDisabled { get; set; }

        bool hasNext();
        bool hasPrev();


        /// <summary>
        /// Edits a datarow using a specified listig type. Also Extra parameter
        ///  of R.Table is considered.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="editType"></param>
        /// <param name="outputRow"></param>
        /// <returns>true if row has been modified</returns>
        bool EditDataRow(DataRow r, string editType, out DataRow outputRow);



        /// <summary>
        /// Set the value linked to  a textBox located in a AutoManage or AutoChoose groupbox. Eventually calls AfterRowSelect
        /// </summary>
        /// <param name="idValue"></param>
        /// <param name="T"></param>
        void SetAutoField(object idValue, TextBox T);

        /// <summary>
        /// Gets the value in the HiddenTextBox linked to an AutoManage or AutoChoose TextBox
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        object GetAutoField(TextBox T);

        WinFormMetaData meta { get; }

        bool WindowsShowClientMsg(string message, string title, mdl.MessageBoxButtons btns);

        ICustomViewListForm currentListForm { get; set; }

        /// <summary>
        /// Attaches all framework events to a form
        /// </summary>
        void doLink();

        /// <summary>
        /// Event manager linked
        /// </summary>
        IFormEventsManager eventManager { get; }

        void setColor(Control c,bool recursive=false);

        /// <summary>
        /// Used for monitoring datatables in Forms
        /// </summary>        
        DataRow lastSelectedRow { get; set; }

        DataTable primaryTable { get; set; }

      

        /// <summary>
        /// true if MainRefresh is enabled
        /// </summary>
        bool MainRefreshEnabled { get; set; }

       

        /// <summary>
        /// When true, does not warn if canceling an insert operation.
        /// </summary>
        bool DontWarnOnInsertCancel { get; set; }

       

        /// <summary>
        /// 
        /// </summary>
        DataSet ds { get; set; }

        /// <summary>
        /// True when an unrecoverable error has occurred
        /// </summary>
        bool ErroreIrrecuperabile { get; set; }

        void SetNewListForm(ICustomViewListForm NewListForm);

        /// <summary>
        /// Reads table cached connected to combo, treeview and similar controls on a specific table
        /// </summary>
        /// <param name="tablename"></param>
        void Do_Prefill(string tablename);

        /// <summary>
        ///  Reads table cached connected to combo, treeview and similar controls
        /// </summary>
        void Do_Prefill();

        /// <summary>
        /// Called whenever a row is selected on a main tree
        /// </summary>
        /// <param name="R"></param>
        /// <param name="ListType"></param>
        void TreeSelectRow(DataRow R, string ListType);

        /// <summary>
        /// Used db connection
        /// </summary>
        IDataAccess conn { get; set; }

        /// <summary>
        /// True when form has been destroyed
        /// </summary>
        bool destroyed { get; set; }

        /// <summary>
        /// Called to release all resources
        /// </summary>
        void Destroy();

        /// <summary>
        /// Managed form
        /// </summary>
        Form linkedForm { get; }

        /// <summary>
        /// Ignores any incoming command to this form
        /// </summary>
        bool locked { get; set; }


        ISecurity security { get; set; }

        /// <summary>
        /// True  the  first time an AfterFill is invoked on a certain row  
        /// </summary>
        bool firstFillForThisRow { get; set; }

        /// <summary>
        /// True if an insert is coming after the clear
        /// </summary>
        bool GointToInsertMode { get; set; }

        /// <summary>
        /// True if an edit is coming after the clear
        /// </summary>
        bool GoingToEditMode { get; set; }

        /// <summary>
        /// True if  no insert or edit are coming after the clear
        /// </summary>
        bool IsRealClear { get; }

        /// <summary>
        /// True when form is in insert mode (NOT EDIT!!)
        /// </summary>
        bool InsertMode { get; }

        /// <summary>
        /// True when form is in "edit mode" (not INSERT!)
        /// </summary>
        bool EditMode { get; }

        /// <summary>
        /// Current form state
        /// </summary>
        form_states formState { get; set; }

        /// <summary>
        /// Current Draw state of the form
        /// </summary>
        form_drawstates DrawState { get; set; }

        /// <summary>
        /// True if the form is under user control
        /// </summary>
        bool DrawStateIsDone { get; }

        /// <summary>
        /// true if the MetaData object has not been filled
        /// </summary>
        bool IsEmpty { get; }


        /// <summary>
        /// Linked HelpForm classes
        /// </summary>
        [Obsolete]
        IHelpForm helpForm { get; set; }

        /// <summary>
        /// True during form closing
        /// </summary>
        bool isClosing { get; set; }


        /// <summary>
        /// Create a new entity (eventually clearing current one) and updates form.
        /// </summary>
        /// <returns></returns>
        bool EditNew();

        /// <summary>
        /// Current operation runned
        /// </summary>
        mainoperations curroperation { get; set; }

        /// <summary>
        /// Opens a list form and select a row from it
        /// </summary>
        /// <param name="command"></param>
        /// <param name="startfield"></param>
        /// <param name="startvalue"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        bool Manage(string command, string startfield, string startvalue, Control origin = null);

        /// <summary>
        /// Prefill combo and other one-time operations
        /// </summary>
        void prefillControls();

        /// <summary>
        /// Gets the current toolbar manager
        /// </summary>
        /// <returns></returns>
        IMainToolBarManager getToolBarManager();

        /// <summary>
        /// Called when a row is selected form a list, should fill the mainform 
        ///  subsequently. In case of a list-form, entity table should not be cleared
        /// R is the row from which start the filling of the form  - does not belong to DS
        /// </summary>
        /// <param name="R"></param>
        /// <param name="ListType"></param>
        void SelectRow(DataRow R, string ListType);

        /// <summary>
        /// Fill controls of a form
        /// </summary>
        void ReFillControls();

        /// <summary>
        /// Fill form controls inside a container
        /// </summary>
        /// <param name="Cs"></param>
        void ReFillControls(Control.ControlCollection Cs);

        /// <summary>
        /// Invoke a method of the linked Form
        /// </summary>
        /// <param name="method"></param>
        void CallMethod(string method);

        /// <summary>
        /// Clear form controls and empty data
        /// </summary>
        void Clear(); 

        /// <summary>
        /// Refreshes toolbar basing it to this MetaData linked form
        /// </summary>
        void FreshToolBar();

        /// <summary>
        /// unlink the current toolbar
        /// </summary>
        void UnlinkToolBar();

        /// <summary>
        /// Unlinks a specified row and set/unset the table as entitychild consequently. 
        /// Invoked during a Unlink_Grid_Row grid command
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        DataRow unlink(DataRow R);

        /// <summary>
        /// Optimized version to unlink a set of rows
        /// </summary>
        /// <param name="toUnlink"></param>
        void unlinkMultipleRows(List<DataRow> toUnlink);

        /// <summary>
        /// If possible, makes R child of current PrimaryEntity 
        /// </summary>
        /// <param name="R"></param>
        /// <param name="relname"></param>
        void CheckEntityChildRowAdditions(DataRow R, string relname);

        /// <summary>
        /// If possible, makes PrimaryEntity child (or other subentity) of R (of table T) 
        /// </summary>
        /// <param name="R"></param>
        /// <param name="T"></param>
        /// <param name="relname"></param>
        void makeChild(DataRow R, DataTable T, string relname);


        /// <summary>
        /// Tries to select a row in the Form. Checks for unsaved data, and also
        ///  verifies whether the row is selectable (CanSelect(R,listtype)==true).
        /// </summary>
        /// <param name="R"></param>
        /// <param name="listtype"></param>
        /// <returns></returns>
        bool TryToSelectRow(DataRow R, string listtype);

        /// <summary>
        /// Prefills a Table and Refills a set of controls
        /// </summary>
        /// <param name="Cs"></param>
        /// <param name="freshperipherals"></param>
        /// <param name="tablename"></param>
        void FreshForm(Control.ControlCollection Cs, bool freshperipherals, string tablename);

        /// <summary>
        /// Refills the form. If RefreshPeripherals is set to true, secondary tables
        ///  are read again from DB (i.e. all tables in the view that are not
        ///  cached, primary or child of primary.
        /// </summary>
        /// <param name="RefreshPeripherals">when true, not -entity-or-cached- tables are cleared and read again from DB</param>
        /// <param name="DoPrefill">When true, also prefill is done, this is more expensive and should be done only once in a form</param>
        void FreshForm(bool RefreshPeripherals = true, bool DoPrefill = false);

        /// <summary>
        ///  Gets form data, starting from primary table.
        /// </summary>
        /// <param name="onlyperipherals"></param>
        /// <param name="OneRow"></param>
        void DO_GET(bool onlyperipherals, DataRow OneRow);

        /// <summary>
        /// Set the caption for this form
        /// </summary>
        void setFormText();

        /// <summary>
        /// true when changes to this entity or to sub-entity of this have been made
        /// </summary>
        bool entityChanged { get; set; }


        IMessageShower shower { get; set; }

        void MainSelect();

        bool Choose(string command, Control origin = null);

        DataRow Delete_Grid_Row(DataGrid g);
        DataRow Unlink_Grid_Row(DataGrid g);

        /// <summary>
        /// Get/set the type of a form (main/detail/unknown)
        /// </summary>
        //MetaData.form_types formType { get; set; }


        /// <summary>
        /// Container of entity detail controls in a list form, Control to use for SetFocus() on List forms
        /// </summary>
        [Obsolete]
        Control formDetailControl { get; set; }

        /// <summary>
        /// Dsiplays a message and stop form closing if ther are unsaved changes
        /// </summary>
        /// <returns></returns>
        bool warnUnsaved();


        /// <summary>
        /// Returns true if there are unsaved changes
        /// </summary>
        /// <returns></returns>
        bool HasUnsavedChanges();

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
        bool GetSourceChanges();

        /// <summary>
        /// Gets data from linked Form control, returning false if some errors occured
        /// </summary>
        /// <param name="Quick">true if no validity checks have to be made</param>
        /// <returns>true on success</returns>
        bool GetFormData(bool Quick);

        /// <summary>
        /// Save data to db
        /// </summary>
        void SaveFormData();

        /// <summary>
        /// Check if a form is a subEntity detail of main form
        /// </summary>
        bool isSubentity { get; }

        /// <summary>
        /// Enable automatic events
        /// </summary>
        [Obsolete]
        void EnableAutoEvents();

        /// <summary>
        /// Disable automatic events
        /// </summary>
        [Obsolete]
        void DisableAutoEvents();

        /// <summary>
        ///  True if form has been correctly inited
        /// </summary>
        [Obsolete]
        bool formInited { get; set; }

        /// <summary>
        /// True if form has been prefilled
        /// </summary>
        [Obsolete]
        bool formPrefilled { get; set; }

        string editType { get; set; }

        /// <summary>
        /// Set focus to the formDetailControl
        /// </summary>
        void focusDetail();

        /// <summary>
        /// Searches a row on primarytable given  a base filter and select it in the form
        /// </summary>
        /// <param name="listType"></param>
        /// <param name="filterstart"></param>
        /// <param name="emptylist"></param>
        /// <returns></returns>
        bool searchRow(string listType, q filterstart, bool emptylist);

        /// <summary>
        /// G has tag: AutoChoose.TextBoxName.ListType.StartFilter or
        ///            AutoManage.TextBoxName.EditType.StartFilter
        /// </summary>
        /// <param name="G"></param>
        void SetAutoMode(GroupBox G);

        /// <summary>
        /// Calls MetaData_AfterRowSelect
        /// </summary>
        /// <param name="T"></param>
        /// <param name="R"></param>
        void afterRowSelect(DataTable T, DataRow R);

        /// <summary>
        /// Call MetaData_BeforeRowSelect
        /// </summary>
        /// <param name="T"></param>
        /// <param name="R"></param>
        void beforeRowSelect(DataTable T, DataRow R);

        /// <summary>
        /// Do a main command
        /// </summary>
        /// <param name="tag"></param>
        void DoMainCommand(string tag);
        
        /// <summary>
        /// Function called when a grid-edit button is pressed
        /// </summary>
        /// <param name="G"></param>
        /// <param name="edit_type"></param>
        /// <returns></returns>
        DataRow Edit_Grid_Row(DataGrid G, string edit_type);

        /// <summary>
        /// Managed click of grid buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Button_Click(object sender, System.EventArgs e);


        /// <summary>
        /// Event called when the form is tree and before a node is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void beforeSelectTreeManager(object sender, TreeViewCancelEventArgs e);

        /// <summary>
        /// Event called when the form is tree and after a node is selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void afterSelectTreeManager(object sender, TreeViewEventArgs e);

       
        /// <summary>
        /// Disconnects from list form 
        /// </summary>
        void UnlinkListForm();
        
        DataRow Insert_Grid_Row(DataGrid g, string _editType);

        IWinEntityDispatcher dispatcher { get; set; }
    }


    /// <summary>
    /// Listener Creazione dei Form
    /// </summary>
    public interface IFormCreationListener {
	    void create(Form f,Form parent);
    }

    // Usage: è necessario sostituire in tutti i form
    // Form.create()
    // con
    // MetaFactory.factory.getSingleton<IFormCreationListener>().create(F, null);
    class DefaultCreationListener : IFormCreationListener {
	    public void create(Form f, Form parent) { }
    }


    /// <summary>
    /// Execute a file    
    /// </summary>
    public interface IProcessRunner {
        void start(string filepath, bool onClient=true);
    }

    // Usage: è necessario sostituire in tutti i form
    // System.Diagnostics.Process.Start(sw);
    // con
    // runProcess(sw, "onClient");
    class DefaultProcessRunner : IProcessRunner {
        public void start(string sw, bool onClient) {
            System.Diagnostics.Process.Start(sw);
        }
    }
}

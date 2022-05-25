using mdl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using q = mdl.MetaExpression;

namespace mdl_winform {

    /// <summary>
    /// Interface for helping managing form controls
    /// </summary>
    public interface IHelpForm :IFormInit {
     

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        void addExtraEntity(string tableName);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<string> getExtraEntities();

        //IGetData getData { get; set; }

        /// <summary>
        /// Ultima riga selezionata in un eventuale datagrid associato alla tabella da monitorare
        /// </summary>
        DataRow LastSelectedRow { get; }

        /// <summary>
        /// Dispose all resources
        /// </summary>
        void Destroy();
        

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
        void GetControls(Form F);

        /// <summary>
        /// Recursively Iterate GetControl over a Control Collection
        /// </summary>
        /// <param name="Cs"></param>
        void IterateGetControls(Control.ControlCollection Cs);

        /// <summary>
        /// Takes value from a TextBox and put it in a row field
        /// </summary>
        /// <param name="T"></param>
        /// <param name="fieldname"></param>
        /// <param name="R"></param>
        void GetText(TextBox T, string fieldname, DataRow R);

        /// <summary>
        /// Gets a value in a ValueSigned groupbox
        /// </summary>
        /// <param name="G"></param>
        /// <param name="fieldname"></param>
        /// <param name="R"></param>
        void GetValueSignedGroup(GroupBox G, string fieldname, DataRow R);

        /// <summary>
        /// Sets controls of forms so that primary table controls describes a child of ParentRow
        ///  Affected controls are only TextBox, prefilled combobox, prefilled
        ///    TreeView, RadioButtons and CheckBoxes. Assumes form in "setsearch" mode
        /// </summary>
        /// <param name="F">Calling Form</param>
        /// <param name="ParentTable">Table considered as Parent </param>
        /// <param name="ParentRow">Row that should belong to getd.DS and should be
        ///  in a parent table of primary table</param>
        void FillParentControls(Form F,
            DataTable ParentTable,
            DataRow ParentRow
        );

        /// <summary>
        /// Fills a collection of controls related to a specified parent Table
        /// </summary>
        /// <param name="CS"></param>
        /// <param name="ParentTable"></param>
        /// <param name="ParentRow"></param>
        void FillParentControls(Control.ControlCollection CS,
            DataTable ParentTable,
            DataRow ParentRow
        );

        /// <summary>
        /// Fills all form controls related to a specified parent Table by a specified condition
        /// </summary>
        /// <param name="F"></param>
        /// <param name="ParentRow"></param>
        /// <param name="Rel"></param>
        void FillParentControls(Form F, DataRow ParentRow, DataRelation Rel);

        /// <summary>
        /// Fills controls of forms to display a Parent Row so that primary table 
        ///		controls becomes a child of ParentRow
        /// </summary>
        /// <param name="CS"></param>
        /// <param name="ParentRow"></param>
        /// <param name="Rel"></param>
        void FillParentControls(Control.ControlCollection CS,
            DataRow ParentRow, DataRelation Rel);

        /// <summary>
        /// Fill  parent's related controls so that current primary row controls
        ///  describe a child of that row
        /// </summary>
        /// <param name="F"></param>
        /// <param name="ParentRow"></param>
        /// <param name="relname"></param>
        void FillParentControls(Form F, DataRow ParentRow, string relname);

        /// <summary>
        /// Fills form's controls linked to a Table. If a Row is given,
        ///  it is used for getting values. Otherwise, values are cleared
        /// </summary>
        /// <param name="F"></param>
        /// <param name="Table"></param>
        /// <param name="Row"></param>
        void FillTableControls(Form F, DataTable Table, DataRow Row);

        /// <summary>
        /// Fill form's control related to some fields of a row
        /// </summary>
        /// <param name="F">Form to fill</param>
        /// <param name="Table">Table whose controls have to be filled</param>
        /// <param name="Row">Row from which values have to be taken</param>
        /// <param name="Cs">Collection of columns to be displayed</param>
        void FillTableControls(Form F,
            DataTable Table, DataRow Row,
            DataColumn[] Cs);

        /// <summary>
        /// Fill form's control related to all fields of a row
        /// </summary>
        /// <param name="Cs"></param>
        /// <param name="Table"></param>
        /// <param name="R">Row from which values have to be taken</param>
        void FillSpecificRowControls(Control.ControlCollection Cs, DataTable Table,
            DataRow R);

        /// <summary>
        /// Adjust all tables in order to be displayed on grids or tree
        /// </summary>
        /// <param name="F"></param>
        /// <param name="Adjust"></param>
        void AdjustTablesForDisplay(Form F, HelpForm.AdjustTable Adjust);

        /// <summary>
        /// Sets the current selected row of a grid
        /// </summary>
        /// <param name="G"></param>
        /// <param name="R">Row that must become the current row of the grid.</param>
        void SetGridCurrentRow(DataGrid G, DataRow R);

        /// <summary>
        /// Apply a method on each control of the form
        /// </summary>
        /// <param name="Cs"></param>
        /// <param name="Apply"></param>
        void IterateControls(Control.ControlCollection Cs, HelpForm.ApplyOnControl Apply);

        /// <summary>
        /// Call MainManager Delegate for an entire form. MainManager function is called
        ///  for every TreeNavigator control, and for every control linked to PrimaryTable
        /// </summary>
        /// <param name="F"></param>
        /// <param name="MainManager"></param>
        void SetMainManagers(Form F, HelpForm.SetMainManagerDelegate MainManager);

        /// <summary>
        /// Add standard events to a control
        /// </summary>
        /// <param name="C"></param>
        void AddEvents(Control C);

        /// <summary>
        /// Add Helpform events to form controls (Buttons click, grid click,
        ///  combobox slection changed and so on)
        /// </summary>
        /// <param name="F"></param>
        void AddEvents(Form F);

        /// <summary>
        /// Sostituisce ogni  (LT)% Form[ControlName] %> con 
        /// QueryCreator.quotedstrvalue(valore,true) ove valore è
        ///  il valore del controllo di nome ControlName. Per i ComboBox  è considerato
        ///  il SelectedValue.
        /// </summary>
        /// <param name="F"></param>
        /// <param name="S"></param>
        /// <returns></returns>
        string CompileFormFilter(Form F, string S);

        /// <summary>
        /// Event fired when a navigator is double clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NavigatorDoubleClick(object sender, EventArgs e);

        /// <summary>
        /// Event fired where the selection of a Navigator changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NavigatorChanged(object sender, EventArgs e);

        /// <summary>
        /// Internal event called by NavigatorChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NavigatorChanged2(object sender, EventArgs e);

        /// <summary>
        /// Sets the table that will be used for returning the selected row
        /// </summary>
        /// <param name="tablename"></param>
        //void SetTableToMonitor(string tablename);

        /// <summary>
        /// Invoked when any DataRow selection control is changed (grid, combo, listview..)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ControlChanged(object sender, System.EventArgs e);

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
        /// <param name="defaultRow">Row taken when it's not possible to evaluate a row from sender</param>
        void extendedControlChanged(object sender, System.EventArgs e, DataRow defaultRow);

        /// <summary>
        /// Data of the tree has already been retrieved.
        /// In this case, the tree refers to primary DataTable, and should be
        ///  displayed in a LIST-type form.
        /// </summary>
        /// <param name="C"></param>
        /// <param name="TableName"></param>
        void DisplayTree(TreeView C, string TableName);

        /// <summary>
        /// Fill a treeview. If SetFilterTree has been called, the nodes are taken
        ///  from Extended property (not from DB)
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="rootfilterSql"></param>
        /// <param name="clear"></param>
        /// <returns></returns>
        bool StartTreeView(TreeView treeView, q rootfilterSql, bool clear);

        /// <summary>
        /// Fills a tree given a start condition. Also Accepts FilterTree
        /// </summary>
        /// <param name="c"></param>
        /// <param name="startCondition"></param>
        /// <param name="startValueWanted"></param>
        /// <param name="startFieldWanted"></param>
        /// <returns></returns>
        bool SetTreeByStart(TreeView c, q startCondition,
            string startValueWanted,
            string startFieldWanted);

        /// <summary>
        /// Fills a set of controls (with childs)
        /// </summary>
        /// <param name="Cs"></param>
        void FillControls(Control.ControlCollection Cs);

        /// <summary>
        /// Fills all textbox, checkboxes, radiobuttons and comboboxes of the form linked
        ///  to the primary table.
        /// </summary>
        /// <param name="F">Form to Fill</param>
        void FillControls(Form F);

        /// <summary>
        /// Sets the content and the status of a textbox basing on his tag
        /// </summary>
        /// <param name="T"></param>
        /// <param name="Table"></param>
        /// <param name="fieldname"></param>
        /// <param name="val"></param>
        void SetText(TextBox T, DataTable Table, string fieldname, object val);

        /// <summary>
        /// Fills a control basing on it's tag
        /// </summary>
        /// <param name="C"></param>
        void FillControl(Control C);

        /// <summary>
        /// Reads some row related to a tree in order to display it at beginning
        /// </summary>
        /// <param name="C">treeView to fill</param>
        /// <param name="filter">filter to apply when getting root nodes</param>
        /// <param name="skipPrimary">if true, no action is done if tree-table is 
        ///		primary table</param>
        void FilteredPreFillTree(TreeView C, q filter, bool skipPrimary);

        /// <summary>
        /// Prefill a control, with an optional select list to compile
        /// </summary>
        /// <param name="Co"></param>
        /// <param name="selList"></param>
        void PreFillControls(Control Co, List<SelectBuilder> selList);

        /// <summary>
        /// Prefill controls for a specified table
        /// </summary>
        /// <param name="Co"></param>
        /// <param name="tablewanted"></param>
        void PreFillControlsTable(Control Co, string tablewanted);

        /// <summary>
        /// Set the standard tooltip for a control 
        /// </summary>
        /// <param name="c"></param>
        void setToolTip(Control c);

        /// <summary>
        /// Last TextBox modified by the user interaction
        /// </summary>
        TextBox lastTextBoxChanged { get; set; }

        /// <summary>
        /// prefill controls of tablewanted (or all if tablewanted is null)
        /// </summary>
        /// <param name="Co"></param>
        /// <param name="tablewanted"></param>
        /// <param name="selList"></param>
        void PreFillControlsTable(Control Co, string tablewanted, List<SelectBuilder> selList);


        void FilteredPreFillCombo(ComboBox C, q filter, bool freshvalue);

        void FilteredPreFillCombo(ComboBox C, q filter, bool freshvalue, List<SelectBuilder> selList,
            HelpForm.drawmode dmode);

        void FillComboBoxTable(ComboBox C, bool freshvalue);
        void EnableAutoEvents();
        void DisableAutoEvents();
        void ResetComboBoxSource(ComboBox C, string tablename, HelpForm.drawmode dmode);
        void ResetComboBoxSource(ComboBox C, string tablename);
        void RefilterComboBoxSource(ComboBox C, string tablename);

        /// <summary>
        /// Fills all tables related as parent to primary table who have
        ///  some linked combobox in the form and do not have parent themself.
        /// </summary>
        /// <param name="F">Form to scan for comboboxes</param>
        void PreFillControls(Form F);

        /// <summary>
        /// Prefills every control on a form belonging to a table
        /// </summary>
        /// <param name="F"></param>
        /// <param name="tablename"></param>
        void PreFillControls(Form F, string tablename);

        /// <summary>
        /// Formats a DateTime TextBox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LeaveDateTimeTextBox(object sender, System.EventArgs e);

        /// <summary>
        /// Formats a textbox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GeneralLeaveDateTextBox(object sender, System.EventArgs e);

        /// <summary>
        /// Formats a textbox when leaving it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GeneralLeaveTextBox(object sender, System.EventArgs e);

        /// <summary>
        /// Gets current row from ComboBox, Grids and tree-views, return false on errors
        /// </summary>
        /// <param name="C">Control to analyze</param>
        /// <param name="T">Table containing rows</param>
        /// <param name="R">Current selected row (null if none)</param>
        /// <returns>false on errors</returns>
        bool GetCurrentRow(Control C, out DataTable T, out DataRow R);

        /// <summary>
        /// Clears form controls, unbinding datagrids and setting comboboxes to "no selection" state.
        /// Note that combobox lists are cleared only if combobox has no parent tables
        /// </summary>
        /// <param name="F"></param>
        void ClearForm(Form F);

        /// <summary>
        /// Clear control specified in a collection, recursively
        /// </summary>
        /// <param name="Cs"></param>
        void ClearControls(Control.ControlCollection Cs);

        /// <summary>
        /// Gets the mainsearch condition scanning all control of a Form
        /// </summary>
        /// <param name="F"></param>
        /// <returns></returns>
        q GetSearchCondition(Form F);

        /// <summary>
        /// Gets the mainsearch condition scanning a specified set of controls (and childs)
        /// </summary>
        /// <param name="Cs"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        q GetSpecificCondition(Control.ControlCollection Cs, string table);

        /// <summary>
        /// Fills a collection of controls (and childs) to reflect a new row selected
        ///  in a Control
        /// </summary>
        /// <param name="Cs">Controls to fill</param>
        /// <param name="Changed">Control that generated the row change event</param>
        /// <param name="T">Table containing the changed row</param>
        /// <param name="RowChanged">New selected row</param>
        void IterateFillRelatedControls(Control.ControlCollection Cs,
            Control Changed,
            DataTable T,
            DataRow RowChanged);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="C"></param>
        /// <param name="filter"></param>
        /// <param name="freshvalue"></param>
        /// <param name="dmode"></param>
        void FilteredPreFillCombo(ComboBox C, q filter, bool freshvalue, HelpForm.drawmode dmode);

        /// <summary>
        /// Fills a control in order to display a specified row. 
        /// Only controls linked to the right table are affected. All other are left
        ///  unchanged.
        /// </summary>
        /// <param name="F"></param>
        /// <param name="T"></param>
        /// <param name="ChangedRow"></param>
        void SetDataRowRelated(Form F,
            DataTable T,
            DataRow ChangedRow);

        /// <summary>
        /// Fills a collection of controls in order to display a specified row. 
        /// Only controls linked to the right table are affected. All other are left
        ///  unchanged.
        /// </summary>
        /// <param name="Cs"></param>
        /// <param name="T"></param>
        /// <param name="ChangedRow">Row to display</param>
        void IterateSetDataRowRelated(Control.ControlCollection Cs,
            DataTable T,
            DataRow ChangedRow);

        /// <summary>
        /// Get a grid contained in the same container of C
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        DataGrid GetLinkedGrid(Control C);

     

        /// <summary>
        /// This code added to correctly implement the disposable pattern. 
        /// </summary>
        void Dispose();


        /// <summary>
        /// 
        /// </summary>
        //IFormEventsManager eventManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool toolTipOnControl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Control mainTableSelector { get; set; }

        //bool comboBoxToRefilter { get; set; }

        HelpForm.drawmode drawMode { get; set; }
        string lastTextNoFound { get; set; }

        HelpForm.AfterRowSelectDelegate AfterRowSelect { get; set; }
        HelpForm.AfterRowSelectDelegate BeforeRowSelect { get; set; }
        string getAdditionalTooltip(string name);
        void setAdditionalTooltip(string name, string value);

        void txtDoubleClick(object sender, EventArgs e);
        void txtMouseDown(object sender, MouseEventArgs e);
    }
}
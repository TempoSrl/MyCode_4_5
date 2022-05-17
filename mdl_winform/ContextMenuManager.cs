using System;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using LM = mdl_language.LanguageManager;
using mdl;

namespace mdl_winform {
    

    /// <summary>
    ///  Class derived from MenuItem in order to add some properties
    /// </summary>
    internal class ContextMenuItem : MenuItem {
        /// <summary>
        /// True if it is a "create document menu item"
        /// </summary>
        public bool Insert;

        /// <summary>
        /// Relation implemented by this menu item
        /// </summary>
        public DataRow Relation;

        /// <summary>
        /// Creates a navigation menu item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="insert"></param>
        /// <param name="relation"></param>
        public ContextMenuItem(string text, bool insert, DataRow relation) :
            base(text) {
            Insert = insert;
            Relation = relation;
        }
    }

    /// <summary>
    ///  Class derived from MenuItem in order to add some properties
    /// </summary>
    internal class CommandMenuItem : MenuItem {
        /// <summary>
        /// Attached menu item main command
        /// </summary>
        public string Command;

        /// <summary>
        /// Creates a main command menu item
        /// </summary>
        /// <param name="text"></param>
        /// <param name="command"></param>
        public CommandMenuItem(string text, string command) :
            base(text) {
            Command = command;
        }
    }


    /// <summary>
    /// Manager for context menu (activated with the right mouse button)
    /// </summary>
    public interface IContextMenuManager {
        /// <summary>
        /// Release all resources
        /// </summary>
        void destroy();

        /// <summary>
        /// Adds a context menu to a form
        /// </summary>
        /// <param name="f"></param>
        void addContextMenuToForm(Form f);

    }

    /// <summary>
    /// Manager for context menu (activated with the right mouse button)
    /// </summary>
    public class ContextMenuManager :IDisposable, IContextMenuManager {
        private Form _linkedForm;
        /// <summary>
        /// Release all resources
        /// </summary>
        public void destroy() {           
                        
            if (_linkedForm != null) {
                if (!_linkedForm.IsDisposed) {
                    _linkedForm.ContextMenu = null;
                }               
                _linkedForm = null;
            }            
            _meta = null;
            _conn = null;
            _cm.Popup -= cM_Popup;
            _cm = null;
            _cdr = null;
            _qhs = null;           
        }


        private IWinFormMetaData _meta;
        private IDataAccess _conn;
        private ISecurity _security;
        private IMessageShower shower;

        /// <summary>
        /// Managed context menu
        /// </summary>
        private ContextMenu _cm;


        private customobjectrelations _cdr;
        private QueryHelper _qhs;
        private string _primary;


        private bool _dataSetRead;

        private IFormController _formController;
        private IWinEntityDispatcher _dispatcher ;

        /// <summary>
        /// Adds a context menu to a form
        /// </summary>
        /// <param name="f"></param>
        public virtual void addContextMenuToForm(Form f) { //IMetaData meta, IFormController f,IDataAccess dbConn
            _formController = f.getInstance<IFormController>();
            _meta = f.getInstance<IMetaData>() as IWinFormMetaData;
            _conn = f.getInstance<IDataAccess>();
            _linkedForm = f;
            _security = f.getInstance<ISecurity>();    
            _dispatcher = f.getInstance<IMetaDataDispatcher>() as IWinEntityDispatcher;
            _qhs = _conn.GetQueryHelper();
            _primary = _formController.primaryTable.TableName;
            _cdr = new customobjectrelations {EnforceConstraints = false};
            _cm = new ContextMenu();
            _linkedForm.ContextMenu = _cm;
            _cm.Popup += cM_Popup;
            shower = MetaFactory.factory.getSingleton<IMessageShower>();
        }

        private void cM_Popup(object sender, EventArgs e) {
            updateMenu();
        }

        private void getRelations() {
            if (_dataSetRead)
                return;
            var getd = new GetData();
            getd.InitClass(_cdr, _conn,  "customobject");
            getd.GET_PRIMARY_TABLE($"(objectname={mdl_utils.Quoting.quotedstrvalue(_primary, true)})");
            getd.DO_GET(false, null);
            _dataSetRead = true;
        }

        private const int CanNavigateFromParentToChild = 1;
        private const int CanInsertFromParentToChild = 2;
        private const int CanNavigateFromParent1ToParent2 = 1;
        private const int CanInsertFromParent1ToParent2 = 2;
        private const int CustomFilterForNavigation = 4;
        private const int CustomFilterForInsert = 8;
        private const int CustomDefaults = 16;
        private const string ForSearch = "S";
        private const string ForInsert = "I";



        /// <summary>
        /// Check if a condition is satisfied by a row
        /// </summary>
        /// <param name="r"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private bool checkFilter(DataRow r, string filter) {
            if (filter == null) return true;
            if (filter == "") return true;
            if (r == null) return false;
            filter = _security.Compile(filter, false);
            var T = r.Table;
            DataRow[] found;
            try {
                found = T.Select(filter);
            }
            catch {
                return false;
            }

            return found.Any(rr => r == rr);
        }

        /// <summary>
        /// Check if a row satisfies custom conditions given by    sp_getcustomrelationfilter
        /// </summary>
        /// <param name="fromTable"></param>
        /// <param name="toTable"></param>
        /// <param name="editType"></param>
        /// <param name="operation"></param>
        /// <param name="fromRow"></param>
        /// <returns></returns>
        private bool verifyCustomCondition(string fromTable,
                    string toTable,
                    string editType,
                    string operation,
                    DataRow fromRow
                    ) {
            DataSet Out;
            try {
                Out = _conn.CallSP("sp_getcustomrelationfilter", new object[] { fromTable, toTable, editType, operation },true);
            }
            catch {
                return true;
            }
            if (Out == null) return true;
            if (Out.Tables.Count == 0) return true;
            if (Out.Tables[0].Rows.Count == 0) return true;
            var filter = Out.Tables[0].Rows[0][0].ToString().Trim();
            return checkFilter(fromRow, filter);
        }

        /// <summary>
        /// Sets custom default on dataTable T given by sp_getcustomrelationdefault
        /// </summary>
        /// <param name="fromTable"></param>
        /// <param name="toTable"></param>
        /// <param name="editType"></param>
        /// <param name="T"></param>
        private void setCustomDefaults(string fromTable,
                    string toTable,
                    string editType,
                    DataTable T) {
            DataSet Out;
            try {
                Out = _conn.CallSP("sp_getcustomrelationdefault", new object[] { fromTable, toTable, editType },true);
            }
            catch {
                return;
            }
            if (Out == null) return;
            if (Out.Tables.Count == 0) return;
            foreach (DataRow defaultRow in Out.Tables[0].Rows) {
                var fieldname = defaultRow["fieldname"].ToString().ToLower().Trim();
                if (!T.Columns.Contains(fieldname)) continue;
                var defaultValue = _security.Compile(defaultRow["defaultvalue"].ToString(), false);
                if (string.IsNullOrEmpty(defaultValue)) {
                    if (T.Columns[fieldname].AllowDBNull) {
                        T.Columns[fieldname].DefaultValue = DBNull.Value;
                    }
                    continue;
                }

                try {
                    var oDef = mdl_utils.HelpUi.GetObjectFromString(T.Columns[fieldname].DataType, defaultValue, "x.y");
                    T.Columns[fieldname].DefaultValue = oDef;
                }
                catch {
                    //ignore
                }
            }
        }

        /// <summary>
        /// Check if an edittype is available for current user
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="edittype"></param>
        /// <returns></returns>
        private bool checkOpenFormAuthorizations(string tablename, string edittype) {
            try {
                var menu = new DataTable("menu");
                menu.Columns.Add("metadata", typeof(string));
                menu.Columns.Add("edittype", typeof(string));
                menu.Columns.Add("idmenu", typeof(string));
                menu.PrimaryKey = new[] { menu.Columns["idmenu"] };
                var rMenu = menu.NewRow();
                rMenu["idmenu"] = "";
                rMenu["metadata"] = tablename;
                rMenu["edittype"] = edittype;
                menu.Rows.Add(rMenu);
                var res= _security.CanSelect( rMenu);
				menu.Dispose();
                return res;
            }
            catch {
                //ignore
            }
            return false;
        }


        private void setCustomDefaultsIndirect(string fromTable, string toTable, string editType,
                    DataSet outDs) {
            DataSet Out;
            try {
                Out = _conn.CallSP("sp_getcustomrelationindirectdefault",
                    new object[] { fromTable, toTable, editType },true);
            }
            catch {
                return;
            }
            if (Out == null) return;
            if (Out.Tables.Count == 0) return;
            foreach (DataRow defaultRow in Out.Tables[0].Rows) {
                var tablename = defaultRow["tablename"].ToString().ToLower().Trim();
                var fieldname = defaultRow["fieldname"].ToString().ToLower().Trim();
                var destTable = outDs.Tables[tablename];
                if (destTable == null) continue;
                if (!destTable.Columns.Contains(fieldname)) continue;
                var defaultValue = _security.Compile(defaultRow["defaultvalue"].ToString(), false);
                if (string.IsNullOrEmpty(defaultValue)) {
                    if (destTable.Columns[fieldname].AllowDBNull) {
                        destTable.Columns[fieldname].DefaultValue = DBNull.Value;
                    }
                    continue;
                }

                try {
                    var oDef = mdl_utils.HelpUi.GetObjectFromString(destTable.Columns[fieldname].DataType, defaultValue, "x.y");
                    destTable.Columns[fieldname].DefaultValue = oDef;
                }
                catch {
                    //ignore
                }
            }
        }

		/// <summary>
		/// Creates submenu "OpenDocument"
		/// </summary>
		private void addMenuOpenDocument() {
            var f = _linkedForm;
            if (f == null) return;

            var curr = HelpForm.GetLastSelected(_formController.primaryTable);
            if (curr == null) return;

            getRelations();

            var menuOpenDoc = new ContextMenuItem(LM.OpenRelatedDocument, false, null);
            _cm.MenuItems.Add(menuOpenDoc);

            //Add navigation relations to child tables
            foreach (DataRow dirRel in _cdr.customdirectrel.Select(null, "totable")) {
                var flag = Convert.ToInt32(dirRel["flag"]);
                if ((flag & CanNavigateFromParentToChild) == 0) continue;
                if (dirRel["edittype"].ToString() == "") continue;
                //Check if DirRel is a good parent relation
                //Checks that all child fields are not empty
                var dirParentRelCols = dirRel.iGetChildRows("customdirectrelcustomdirectrelcol");
                var relgood = true;
                foreach (var dirParentRelCol in dirParentRelCols) {
                    if (!curr.Table.Columns.Contains(dirParentRelCol["fromfield"].ToString())) {
                        relgood = false;
                        break;
                    }

                    if (curr[dirParentRelCol["fromfield"].ToString()].ToString() != "") continue;
                    relgood = false;
                    break;
                }
                if (!relgood) continue;

                if (!checkFilter(curr, dirRel["navigationfilterparent"].ToString())) continue;
                if ((flag & CustomFilterForNavigation) != 0) {
                    if (!verifyCustomCondition(dirRel["fromtable"].ToString(),
                        dirRel["totable"].ToString(),
                        dirRel["edittype"].ToString(),
                        ForSearch,
                        curr)) continue;
                }
                if (!checkOpenFormAuthorizations(dirRel["totable"].ToString(),
                        dirRel["edittype"].ToString())) continue;
                var newm = new ContextMenuItem(dirRel["description"].ToString(),false,dirRel);
                menuOpenDoc.MenuItems.Add(newm);
                newm.Click += newm_Click;
            }

            //Add navigation relations to child tables
            foreach (DataRow indirRel in _cdr.customindirectrel.Rows) {
                var flag = Convert.ToInt32(indirRel["flag"]);
                if ((flag & CanNavigateFromParent1ToParent2) == 0) continue;
                if (indirRel["edittype"].ToString() == "") continue;
                //Check if DirRel is a good parent relation
                //Checks that all child fields are not empty
                var dirParentRelCols = indirRel.iGetChildRows("customindirectrelcustomindirectrelcol");
                var relgood = true;
                foreach (var dirParentRelCol in dirParentRelCols) {
                    if (dirParentRelCol["parentnumber"].ToString() != "1") continue;
                    if (!curr.Table.Columns.Contains(dirParentRelCol["parentfield"].ToString())) {
                        relgood = false;
                        break;
                    }
                    if (curr[dirParentRelCol["parentfield"].ToString()].ToString() != "") continue;
                    relgood = false;
                    break;
                }
                if (!relgood) continue;

                if (!checkFilter(curr, indirRel["navigationfilterparenttable1"].ToString())) continue;

                if ((flag & CustomFilterForNavigation) != 0) {
                    if (!verifyCustomCondition(indirRel["parenttable1"].ToString(),
                        indirRel["parenttable2"].ToString(),
                        indirRel["edittype"].ToString(),
                        ForSearch,
                        curr)) continue;
                }
                if (!checkOpenFormAuthorizations(indirRel["parenttable2"].ToString(),
                    indirRel["edittype"].ToString())) continue;

                var newm = new ContextMenuItem(indirRel["description"].ToString(),false,indirRel);
                menuOpenDoc.MenuItems.Add(newm);
                newm.Click += newm_Click;
            }
            if (menuOpenDoc.MenuItems.Count == 0) menuOpenDoc.Enabled = false;
        }

        /// <summary>
        /// Creates submenu "CreateDocument"
        /// </summary>
        private void addMenuCreateDocument() {
            var f = _linkedForm;
            if (f == null || f.Modal) return;
            if (_formController.InsertMode) return;

            var curr = HelpForm.GetLastSelected(_formController.primaryTable);
            if (curr == null) return;

            getRelations();

            var menuCreateDoc = new ContextMenuItem("Crea documento collegato", false, null);
            _cm.MenuItems.Add(menuCreateDoc);

            //Add insert menu of child tables
            foreach (DataRow dirRel in _cdr.customdirectrel.Rows) {
                var flag = Convert.ToInt32(dirRel["flag"]);
                if ((flag & CanInsertFromParentToChild) == 0) continue;
                if (dirRel["edittype"].ToString() == "") continue;
                //Check if DirRel is a good parent relation
                //Checks that all child fields are not empty
                var dirParentRelCols = dirRel.iGetChildRows("customdirectrelcustomdirectrelcol");
                var relgood = true;
                foreach (var dirParentRelCol in dirParentRelCols) {
                    if (!curr.Table.Columns.Contains(dirParentRelCol["fromfield"].ToString())) {
                        relgood = false;
                        break;
                    }
                    if (curr[dirParentRelCol["fromfield"].ToString()].ToString() != "") continue;
                    relgood = false;
                    break;
                }
                if (!relgood) continue;
                if (!checkFilter(curr, dirRel["insertfilterparent"].ToString())) continue;
                if ((flag & CustomFilterForInsert) != 0) {
                    if (!verifyCustomCondition(dirRel["fromtable"].ToString(),
                        dirRel["totable"].ToString(),
                        dirRel["edittype"].ToString(),
                        ForInsert,
                        curr)) continue;
                }
                if (!checkOpenFormAuthorizations(dirRel["totable"].ToString(),
                    dirRel["edittype"].ToString())) continue;
                var newm = new ContextMenuItem(dirRel["description"].ToString(),true,dirRel);
                newm.Click += newm_Click;
                menuCreateDoc.MenuItems.Add(newm);
            }


            //Add insert menu of child tables
            foreach (DataRow indirRel in _cdr.customindirectrel.Rows) {
                var flag = Convert.ToInt32(indirRel["flag"]);
                if ((flag & CanInsertFromParent1ToParent2) == 0) continue;
                if (indirRel["edittype"].ToString() == "") continue;
                //Check if DirRel is a good parent relation
                //Checks that all child fields are not empty
                var indirParentRelCols = indirRel.iGetChildRows("customindirectrelcustomindirectrelcol");
                var relgood = true;
                foreach (var indirParentRelCol in indirParentRelCols) {
                    if (indirParentRelCol["parentnumber"].ToString() != "1") continue;
                    if (!curr.Table.Columns.Contains(indirParentRelCol["parentfield"].ToString())) {
                        relgood = false;
                        break;
                    }
                    if (curr[indirParentRelCol["parentfield"].ToString()].ToString() != "") continue;
                    relgood = false;
                    break;
                }
                if (!relgood) continue;
                if (!checkFilter(curr, indirRel["insertfilterparenttable1"].ToString())) continue;
                if ((flag & CustomFilterForInsert) != 0) {
                    if (!verifyCustomCondition(indirRel["parenttable1"].ToString(),
                        indirRel["parenttable2"].ToString(),
                        indirRel["edittype"].ToString(),
                        ForInsert,
                        curr)) continue;
                }
                if (!checkOpenFormAuthorizations(indirRel["parenttable2"].ToString(), indirRel["edittype"].ToString())) continue;

                var newm = new ContextMenuItem(indirRel["description"].ToString(),true,indirRel);
                newm.Click += newm_Click;
                menuCreateDoc.MenuItems.Add(newm);
            }

            if (menuCreateDoc.MenuItems.Count == 0) menuCreateDoc.Enabled = false;
        }


        private void addMainCommandMenu(string text, string command) {
            var m = new CommandMenuItem(text, command);
            m.Click += click;
            _cm.MenuItems.Add(m);
        }

        /// <summary>
        /// Add all main commands to main menu
        /// </summary>
        private void addMainCommands() {
            if (_meta.CommandEnabled("mainselect")) {
                addMainCommandMenu(LM.selectLabel, "mainselect");
            }
            if (_meta.CommandEnabled("maininsert")) {
                addMainCommandMenu(LM.insertLabel, "maininsert");
            }
            if (_meta.CommandEnabled("maininsertcopy")) {
                addMainCommandMenu(LM.insertCopyLabel, "maininsertcopy");
            }
            if (_meta.CommandEnabled("mainsetsearch")) {
                addMainCommandMenu(LM.setSearchLabel, "mainsetsearch");
            }
            if (_meta.CommandEnabled("maindosearch")) {
                addMainCommandMenu(LM.doSearchLabel, "maindosearch");
            }
            if (_meta.CommandEnabled("mainsave")) {
                addMainCommandMenu(LM.saveLabel, "mainsave");
            }
            if (_meta.CommandEnabled("maindelete")) {
                addMainCommandMenu(LM.deleteCancelLabel, "maindelete");
            }
            if (_meta.CommandEnabled("mainrefresh")) {
                addMainCommandMenu(LM.refreshLabel, "mainrefresh");
            }
            if (_meta.CommandEnabled("crea_ticket")) {
                addMainCommandMenu(LM.createTicketLabel, "crea_ticket");
            }

            _cm.MenuItems.Add("-");
            if (_meta.HasNotes() || _meta.HasOleNotes()) {
                addMainCommandMenu(LM.notesLabel, "editnotes");
            }

            if (_security.GetSys("FlagMenuAdmin") != null) {
                if (_security.GetSys("FlagMenuAdmin").ToString() == "S") {
                    _cm.MenuItems.Add("-");
                    addMainCommandMenu("Evaluate expression", "evalexpr");
                    addMainCommandMenu(LM.dataDictionary, "datadict");
                }
            }
            if (_meta.CommandEnabled("ShowLast")) {
                addMainCommandMenu(LM.createLastMod, "ShowLast");
            }

        }



        /// <summary>
        /// Must update menu items
        /// </summary>
        public virtual void updateMenu() {
            _cm.MenuItems.Clear();
            var curr = HelpForm.GetLastSelected(_meta.primaryTable);
            if (curr != null) {
                addMenuOpenDocument();
                addMenuCreateDocument();
                _cm.MenuItems.Add("-");
            }
            addMainCommands();
        }

        private void newm_Click(object sender, EventArgs e) {
            if (_formController.locked) return;
            var mym = (ContextMenuItem)sender;
            if (mym.Insert) {
                if (mym.Relation.Table.TableName == "customdirectrel")
                    insertDirect(mym.Relation);
                else
                    insertIndirect(mym.Relation);
            }
            else {
                if (mym.Relation.Table.TableName == "customdirectrel")
                    navigateDirect(mym.Relation);
                else
                    navigateIndirect(mym.Relation);
            }
        }

        /// <summary>
        /// Do a navigate direct with a specified relation
        /// </summary>
        /// <param name="relation"></param>
        private void navigateDirect(DataRow relation) {
            if (_security==null)return;
            if (_conn==null)return;
            if (_meta==null)return;
            if (_formController == null) return;
            var curr = HelpForm.GetLastSelected(_meta.primaryTable);
            if (curr == null) return;
            if (relation==null)return;

            var relCols = relation.iGetChildRows("customdirectrelcustomdirectrelcol");
            if (relCols==null)return;
            var toTable = relation["totable"].ToString();
            var edittype = relation["edittype"].ToString();
            var toFilter = _security.Compile(relation["filter"].ToString(), true);
            var checkfilter = toFilter;
            // ReSharper disable once LoopCanBeConvertedToQuery : it's more readable with a foreach
            foreach (var relCol in relCols) {
                checkfilter = _qhs.AppAnd(checkfilter,
                    _qhs.CmpEq(relCol["tofield"].ToString(), curr[relCol["fromfield"].ToString()])
                );
            }

            var searchTable = toTable;
            if (relation["totableview"].ToString() != "") {
                searchTable = relation["totableview"].ToString();
            }

            var rowsfound = _conn.RUN_SELECT_COUNT(searchTable, checkfilter, true);
            if (rowsfound == 0) {
                shower.ShowNoRowFound(_linkedForm,LM.noRowFound,LM.tableFilterApplied(searchTable, checkfilter));
                return;
            }

            var toMeta = _dispatcher.GetWinFormMeta(toTable);
            if (_dispatcher.unrecoverableError) {
                _formController.ErroreIrrecuperabile = true;
                shower.ShowError(_linkedForm, LM.errorLoadingMeta(toTable), LM.ErrorTitle);
                    //$"Errore nel caricamento del metadato {toTable} è necessario riavviare il programma.", "Errore");
                return;
            }
            if (toMeta == null) return;
            toMeta.ContextFilter = checkfilter;
            toMeta.Edit(getMain(_linkedForm), edittype, false);
            if (_linkedForm != null) {
                if (_linkedForm.Modal) {
                    toMeta.filterLocked = true;
                }
            }
            var listtype = relation["listtype"].ToString();
            if (listtype == "") listtype = toMeta.DefaultListType;
            var r = toMeta.SelectOne(listtype, checkfilter, null, null);
            if (r != null && toMeta.controller!=null) toMeta.controller.SelectRow(r, listtype);
            //_formController.locked = false;

        }


        private Form getMain(Form f) {
            if (f.ParentForm != null) return getMain(f.ParentForm);
            if (f.MdiParent != null) return getMain(f.MdiParent);
            return f.Owner != null ? getMain(f.Owner) : f;
        }

        /// <summary>
        /// Do a navigate indirect  using the specified relation
        /// </summary>
        /// <param name="relation"></param>
        private void navigateIndirect(DataRow relation) {
            if (_security==null)return;            
            var curr = HelpForm.GetLastSelected(_meta.primaryTable);
            if (curr == null) return;
            var relCols = relation.iGetChildRows("customindirectrelcustomindirectrelcol");
            if (relCols==null)return;
            var toTable = relation["parenttable2"].ToString();
            var middle = relation["middletable"].ToString();
            var edittype = relation["edittype"].ToString();

            var actAsInsert = (_formController?.ds?.Tables[middle] != null);
            //Prende la riga dalla tabella middle
            var middleFilter = _security.Compile(relation["filtermiddle"].ToString(), !actAsInsert);
            var clearMiddleFilter = middleFilter;
            foreach (var relCol in relCols) {
                if (relCol["parentnumber"].ToString() != "1") continue;
                var clause = _qhs.CmpEq(relCol["middlefield"].ToString(), curr[relCol["parentfield"].ToString()]);
                middleFilter = GetData.MergeFilters(middleFilter, clause);
                clause = _qhs.CmpEq(relCol["middlefield"].ToString(), curr[relCol["parentfield"].ToString()]);
                clearMiddleFilter = GetData.MergeFilters(clearMiddleFilter, clause);
            }

            DataTable metaTable;
            DataRow oneMiddleRow = null;

            if (actAsInsert) {
                if (_formController.ds.Tables.Contains(middle)) {
                    metaTable = _formController.ds.Tables[middle];
                }
                else {
                    return;
                    //metaTable = _conn.RUN_SELECT(middle, "*", null, clearMiddleFilter, null, null, true);
                }
                
                if (metaTable.Rows.Count == 0) {
                    shower.ShowNoRowFound(_linkedForm,LM.noRowFound,LM.tableMiddleInMemory(middle));                 
                    return;
                }
                if (metaTable.Rows.Count == 1) oneMiddleRow = metaTable.Rows[0];
            }
            else {
                metaTable = _conn.RUN_SELECT(middle, "*", null, clearMiddleFilter, null, null, true);
                if ((metaTable == null) || (metaTable.Rows.Count == 0)) {
                    shower.ShowNoRowFound(_linkedForm,LM.noRowFound,LM.tableFilterApplied(middle, clearMiddleFilter));   
                    return;
                }
                if (metaTable.Rows.Count > 1) {
                    QueryCreator.MarkEvent("More than one row found in middle table");
                }
                else {
                    oneMiddleRow = metaTable.Rows[0];
                }
            }

            var joinfilter = " EXISTS (SELECT * from " + middle + " WHERE  ";
            //Cerca le righe parent di Middle in ParentTable2
            var parent2Filter = _security.Compile(relation["filterparenttable2"].ToString(), true);

            var searchTable = toTable;
            if (relation["parenttable2view"].ToString() != "") {
                searchTable = relation["parenttable2view"].ToString();
            }

            if (oneMiddleRow == null) {
                var joinmiddlefilter = middleFilter;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var relCol in relCols) {
                    if (relCol["parentnumber"].ToString() != "2") continue;
                    var clause = _qhs.CmpEq($"{middle}.{relCol["middlefield"]}", _qhs.Field($"{searchTable}.{relCol["parentfield"]}"));
                    joinmiddlefilter = _qhs.AppAnd(joinmiddlefilter, clause);
                }
                joinfilter += joinmiddlefilter + ") ";
                parent2Filter = _qhs.AppAnd(parent2Filter, joinfilter);
            }
            else {
                var joinmiddlefilter = "";
                foreach (var relCol in relCols) {
                    if (relCol["parentnumber"].ToString() != "2") continue;
                    var clause = _qhs.CmpEq(relCol["parentfield"].ToString(), oneMiddleRow[relCol["middlefield"].ToString()] );
                    joinmiddlefilter = _qhs.AppAnd(joinmiddlefilter, clause);
                }
                parent2Filter = _qhs.AppAnd(parent2Filter, joinmiddlefilter);
            }

            var rowsfound = _conn.RUN_SELECT_COUNT(searchTable, parent2Filter, true);
            if (rowsfound == 0) {
                shower.ShowNoRowFound(_linkedForm,LM.noRowFound,LM.tableFilterApplied(searchTable,parent2Filter));   
                return;
            }


            var toMeta = _dispatcher.GetWinFormMeta(toTable);
            if (_dispatcher.unrecoverableError) {
                _formController.ErroreIrrecuperabile = true;
                shower.ShowError(_linkedForm, LM.errorLoadingMeta(toTable),LM.ErrorTitle);
                return;
            }
            if (toMeta == null) return;
            _security.SetSys("Parent2Filter", parent2Filter);
            _security.SetSys("ComingFromRow", curr);
            if (metaTable.Rows.Count == 1) _security.SetSys("MiddleRow", metaTable.Rows[0]);

            toMeta.Edit(getMain(_linkedForm), edittype, false);
            var listtype = relation["listtype"].ToString();
            if (_linkedForm != null) {
                if (_linkedForm.Modal) {
                    toMeta.filterLocked = true;
                }
            }
            if (listtype == "") listtype = toMeta.DefaultListType;
            //ToMeta.FilterLocked = true;
            var r = toMeta.SelectOne(listtype, parent2Filter, null, null);
            if (r != null && toMeta.controller!=null) toMeta.controller.SelectRow(r, listtype);
            _security.SetSys("Parent2Filter", null);
            _security.SetSys("ComingFromRow", null);
            _security.SetSys("MiddleRow", null);
            //if ((rowsfound==1)&&(R!=null)) ToMeta.SelectRow(R,listtype);

        }


        

        /// <summary>
        /// Insert a row directly using the specified relation
        /// </summary>
        /// <param name="relation"></param>
        private void insertDirect(DataRow relation) {
            var curr = HelpForm.GetLastSelected(_meta.primaryTable);
            if (curr == null) return;
            var relCols = relation.iGetChildRows("customdirectrelcustomdirectrelcol");
            var toTable = relation["totable"].ToString();
            var edittype = relation["edittype"].ToString();
            var toMeta = _dispatcher.GetWinFormMeta(toTable);
            if (_dispatcher.unrecoverableError) {                
                _formController.ErroreIrrecuperabile = true;
                shower.ShowError(_linkedForm, LM.errorLoadingMeta(toTable), LM.ErrorTitle);                    
                return;
            }
            if (toMeta == null) return;
            toMeta.Edit(getMain(_linkedForm), edittype, false);
            var destController = toMeta.controller;
            var saveddefaults = new Dictionary<string,object>();
            destController.primaryTable.Columns._forEach(c => { saveddefaults[c.ColumnName] = c.DefaultValue;});

            foreach (var relCol in relCols) {
                destController.primaryTable.Columns[relCol["tofield"].ToString()].DefaultValue =
                        curr[relCol["fromfield"].ToString()];
            }
          
            var flag = Convert.ToInt32(relation["flag"]);
            if ((flag & CustomDefaults) != 0) {
                setCustomDefaults(relation["fromtable"].ToString(),
                            relation["totable"].ToString(),
                            relation["edittype"].ToString(), destController.primaryTable);
            }
            destController.DoMainCommand("maininsert");
            destController.primaryTable.Columns._forEach(c => { c.DefaultValue = saveddefaults[c.ColumnName];});

        }

        /// <summary>
        /// Insert a row indirectly using the specified relation
        /// </summary>
        /// <param name="relation"></param>
        private void insertIndirect(DataRow relation) {
            var curr = HelpForm.GetLastSelected(_meta.primaryTable);
            if (curr == null) return;
            var relCols = relation.iGetChildRows("customindirectrelcustomindirectrelcol");
            var toTable = relation["parenttable2"].ToString();
            var middleTable = relation["middletable"].ToString();
            var edittype = relation["edittype"].ToString();
            var toMeta = _dispatcher.GetWinFormMeta(toTable);
            var destController = toMeta.controller;
            if (_dispatcher.unrecoverableError) {
                _formController.ErroreIrrecuperabile = true;
                shower.ShowError(_linkedForm, LM.errorLoadingMeta(toTable), LM.ErrorTitle);
                return;
            }
            if (toMeta == null) return;
            toMeta.Edit(getMain(_linkedForm), edittype, false);
            var tMiddleTable = toMeta.ds.Tables[middleTable];
            var saveddefaults = new Hashtable();
            tMiddleTable.Columns._forEach(c => saveddefaults[c.ColumnName]=c.DefaultValue);

            foreach (var relCol in relCols) {
                if (relCol["parentnumber"].ToString() != "1") continue;
                tMiddleTable.Columns[relCol["middlefield"].ToString()].DefaultValue =curr[relCol["parentfield"].ToString()];
            }

            var flag = Convert.ToInt32(relation["flag"]);
            if ((flag & CustomDefaults) != 0) {
                setCustomDefaultsIndirect(relation["parenttable1"].ToString(),
                    relation["parenttable2"].ToString(), relation["edittype"].ToString(), toMeta.ds);
            }
            if (_formController.isClosing) return;
            destController.DoMainCommand("maininsert");
            if (_formController.isClosing) return;

            tMiddleTable.Columns._forEach(c => c.DefaultValue=saveddefaults[c.ColumnName]);
        }

        private bool _doingCommand ;
        private void click(object sender, EventArgs e) {
            if (_doingCommand) {
                ErrorLogger.Logger.logException($"Multiple command ignored: {((CommandMenuItem) sender).Command}",meta:_meta);
                shower.Show(_linkedForm, LM.waitForCommandCompletion, LM.warningLabel);
                return;
            }
            _doingCommand = true;
            var m2 = (CommandMenuItem)sender;
            if (m2.Command == "evalexpr") {
                ErrorLogger.Logger.warnEvent("Opening EvaluateExpression");
                var f = new FrmCheckExpression(_formController, _formController.ds);
                MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f,null);
                f.Show();
                _doingCommand = false;
                return;
            }
            if (m2.Command == "datadict") {
                var m1 = _dispatcher.GetWinFormMeta("tabledescr");
                if (m1.EditTypes.Contains("default")) {
                    m1.Edit(null, "default", false);
                    m1.controller.DoMainCommand($"maindosearch.default.{_qhs.CmpEq("tablename", _meta.TableName)}");
                    _doingCommand = false;
                    return;
                }

                var f = new FrmCheckExpression(_formController, _formController.ds);
                MetaFactory.factory.getSingleton<IFormCreationListener>()?.create(f,null);
                f.Show();
                _doingCommand = false;
                return;
            }
            try {
                if (_meta.CommandEnabled(m2.Command)) _formController.DoMainCommand(m2.Command);
            }
            catch {
                _doingCommand = false;
                throw;
            }
            _doingCommand = false;
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Necessary method to implement IDisposable
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!_disposedValue) {
                if (disposing) {
                    // dispose managed state (managed objects).
                    _cdr.Dispose();
                    _cm.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ContextMenuManager() {
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

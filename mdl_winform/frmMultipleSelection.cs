using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using mdl;
using q = mdl.MetaExpression;

namespace mdl_winform {
    /// <summary>
    /// Summary description for frmMultipleSelection.
    /// </summary>
    public class frmMultipleSelection : System.Windows.Forms.Form {
        private System.Windows.Forms.DataGrid gridToAdd;
        private System.Windows.Forms.Label labToAdd;
        private System.Windows.Forms.Label labAdded;
        private System.Windows.Forms.DataGrid gridAdded;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClose;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        IMetaData linked;
        DataSet myDS;
        DataTable ToAdd;
        DataTable Added;
        DataTable SourceTable;
        IDataAccess Conn;
        string tablename;
        string sorting;
        string filter;
        string filterSQL;
        string listingtype;
        q notentitychildfilter;
        private System.Windows.Forms.ContextMenu cmenuAdd;
        private System.Windows.Forms.MenuItem btnAddAll;
        private System.Windows.Forms.MenuItem btnAddNone;
        private System.Windows.Forms.ContextMenu cmenuRemove;
        private System.Windows.Forms.MenuItem btnRemoveAll;
        private System.Windows.Forms.MenuItem btnRemoveNone;
        private System.Windows.Forms.Label labAdvice;
        //string entitychildfilter;

        private IFormController controller;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linked">Linked metadata</param>
        /// <param name="formController"></param>
        /// <param name="Title">Caption for the form</param>
        /// <param name="labelAdded">Label for already added rows</param>
        /// <param name="labelToAdd">Label for rows to add</param>
        /// <param name="SourceTable">Source table to edit</param>
        /// <param name="filter">filter to search rows to add in memory</param>
        /// <param name="filterSQL">filter to search rows to add in database</param>
        /// <param name="listingtype">listtype for grids</param>
        public frmMultipleSelection(IMetaData linked,   IFormController controller,
                string Title, string labelAdded, string labelToAdd,
                DataTable SourceTable,
                string filter,
                string filterSQL,
                string listingtype
            ) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            utils.SetColorOneTime(this, true);

            this.linked = linked;
            this.controller = controller;
            this.Conn = controller.conn;
            this.SourceTable = SourceTable;
            this.filter = filter;
            this.filterSQL = filterSQL;
            this.sorting = SourceTable.getSorting();
            this.listingtype = listingtype;
            this.tablename = SourceTable.tableForReading();


            Text = Title;
            labAdded.Text = labelAdded;
            labToAdd.Text = labelToAdd;

            DataTable primaryTable = SourceTable.DataSet.Tables[linked.TableName];
            metaModel.AddNotEntityChild(primaryTable, SourceTable);
            notentitychildfilter = metaModel.GetNotEntityChildFilter(SourceTable);               
            //entitychildfilter= "NOT("+notentitychildfilter+")";

            initTables();


        }
        IMetaModel metaModel = MetaFactory.factory.getSingleton<IMetaModel>();

        void CopyKeyWhenBlank(DataTable Source, DataTable T) {
            if ((T.PrimaryKey.Length > 0)) return;

            DataColumn[] newKey = new DataColumn[Source.PrimaryKey.Length];
            for (var i = 0; i < Source.PrimaryKey.Length; i++) {
                newKey[i] = T.Columns[Source.PrimaryKey[i].ColumnName];
            }
            T.PrimaryKey = newKey;
        }

        void initTables() {
            myDS = new DataSet();
            ClearDataSet.RemoveConstraints(myDS);

            string columnlist = QueryCreator.SortedColumnNameList(SourceTable);

            Added = Conn.CreateTable(tablename, columnlist).GetAwaiter().GetResult();
            Added.TableName = "added";
            Added.Namespace = SourceTable.Namespace;

            myDS.Tables.Add(Added);
            Added.setTableForReading(tablename);
            CopyKeyWhenBlank(SourceTable, Added);

            ToAdd = Conn.CreateTable(tablename, columns:columnlist).GetAwaiter().GetResult();
            ToAdd.TableName = "toadd";
            myDS.Tables.Add(ToAdd);
            ToAdd.setTableForReading(tablename);
            CopyKeyWhenBlank(SourceTable, ToAdd);

            //Riempie la Table delle righe "ToAdd" prendendole dal DB. Questa tabella 
            // contiene anche righe già "added" in memoria, che vanno quindi escluse. 
            //Inoltre va integrata con righe che erano "added" e sono state rimosse
            // in memoria
            Conn.SelectIntoTable(ToAdd, orderBy:sorting, filter:filterSQL);

            //Riempie la Table delle righe "Added". Questa contiene anche righe che sono
            // state rimosse in memoria, e quindi vanno rimosse (e integrate a "ToAdd")
            DataSetUtils.MergeDataTable(Added, SourceTable);

            //Per tutte le righe rimosse in memoria (che rispettano il filtro): le toglie da 
            // Added e le mette in ToAdd.
            q tomovefilter = q.and(notentitychildfilter, filter);
            DataRow[] RowsToMove = Added.filter(tomovefilter);
            foreach (DataRow ToMove in RowsToMove) {
                var verifyexistentfilter = q.keyCmp(ToMove);
                //Just for sure I remove from ToAdd those rows I'm going to add to it!
                DataRow[] ToRemoveFromToAdd = ToAdd.filter(verifyexistentfilter);
                foreach (DataRow ToRemFromToAdd in ToRemoveFromToAdd) {
                    ToRemFromToAdd.Delete();
                    ToRemFromToAdd.AcceptChanges();
                }
                //Adds the row to ToAdd
                addRowToTable(ToAdd, ToMove);

                //Remove the row from Added
                ToMove.Delete();
                if (ToMove.RowState != DataRowState.Detached) ToMove.AcceptChanges();

            }


            //Per tutte le righe rimosse in memoria rimanenti (ossia che NON rispettano
            // il filtro) : le rimuovo da Added
            DataRow[] ToRemoveFromAdded = Added.filter(notentitychildfilter);
            foreach (DataRow ToRemFromAdded in ToRemoveFromAdded) {
                ToRemFromAdded.Delete();
                if (ToRemFromAdded.RowState != DataRowState.Detached) ToRemFromAdded.AcceptChanges();
            }

            //Per tutte le righe rimaste in Added: le rimuove da ToAdd
            DataRow[] ToRemoveFromToAdd2 = Added.Select();
            foreach (DataRow ToRemFromToAdd in ToRemoveFromToAdd2) {
                q ToRemKeyFilter = q.keyCmp(ToRemFromToAdd);
                DataRow[] ToRemove = ToAdd.filter(ToRemKeyFilter);
                foreach (DataRow ToRem in ToRemove) {
                    ToRem.Delete();
                    if (ToRem.RowState != DataRowState.Detached) ToRem.AcceptChanges();
                }
            }
            MetaData M = controller.dispatcher.Get(tablename);
            if (controller.dispatcher.unrecoverableError) {
                controller.ErroreIrrecuperabile = true;
                controller.shower.ShowError(controller.linkedForm,
                    "Errore nel caricamento del metadato " + tablename + " è necessario riavviare il programma.", "Errore");
            }

            M.DescribeColumns(ToAdd, listingtype);
            M.DescribeColumns(Added, listingtype);

            HelpForm.SetDataGrid(gridAdded, Added);
            HelpForm.SetDataGrid(gridToAdd, ToAdd);

        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.gridToAdd = new System.Windows.Forms.DataGrid();
            this.cmenuAdd = new System.Windows.Forms.ContextMenu();
            this.btnAddAll = new System.Windows.Forms.MenuItem();
            this.btnAddNone = new System.Windows.Forms.MenuItem();
            this.labToAdd = new System.Windows.Forms.Label();
            this.labAdded = new System.Windows.Forms.Label();
            this.gridAdded = new System.Windows.Forms.DataGrid();
            this.cmenuRemove = new System.Windows.Forms.ContextMenu();
            this.btnRemoveAll = new System.Windows.Forms.MenuItem();
            this.btnRemoveNone = new System.Windows.Forms.MenuItem();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.labAdvice = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridToAdd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridAdded)).BeginInit();
            this.SuspendLayout();
            // 
            // gridToAdd
            // 
            this.gridToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.gridToAdd.ContextMenu = this.cmenuAdd;
            this.gridToAdd.DataMember = "";
            this.gridToAdd.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.gridToAdd.Location = new System.Drawing.Point(8, 40);
            this.gridToAdd.Name = "gridToAdd";
            this.gridToAdd.Size = new System.Drawing.Size(584, 136);
            this.gridToAdd.TabIndex = 0;
            // 
            // cmenuAdd
            // 
            this.cmenuAdd.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                     this.btnAddAll,
                                                                                     this.btnAddNone});
            // 
            // btnAddAll
            // 
            this.btnAddAll.Index = 0;
            this.btnAddAll.Text = "Seleziona tutto";
            this.btnAddAll.Click += new System.EventHandler(this.btnAddAll_Click);
            // 
            // btnAddNone
            // 
            this.btnAddNone.Index = 1;
            this.btnAddNone.Text = "Deseleziona tutto";
            this.btnAddNone.Click += new System.EventHandler(this.btnAddNone_Click);
            // 
            // labToAdd
            // 
            this.labToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            this.labToAdd.Location = new System.Drawing.Point(8, 12);
            this.labToAdd.Name = "labToAdd";
            this.labToAdd.Size = new System.Drawing.Size(584, 23);
            this.labToAdd.TabIndex = 1;
            this.labToAdd.Text = "Da aggiungere";
            this.labToAdd.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labAdded
            // 
            this.labAdded.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.labAdded.Location = new System.Drawing.Point(8, 232);
            this.labAdded.Name = "labAdded";
            this.labAdded.Size = new System.Drawing.Size(584, 23);
            this.labAdded.TabIndex = 3;
            this.labAdded.Text = "Aggiunti";
            this.labAdded.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gridAdded
            // 
            this.gridAdded.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAdded.ContextMenu = this.cmenuRemove;
            this.gridAdded.DataMember = "";
            this.gridAdded.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.gridAdded.Location = new System.Drawing.Point(8, 260);
            this.gridAdded.Name = "gridAdded";
            this.gridAdded.Size = new System.Drawing.Size(584, 136);
            this.gridAdded.TabIndex = 2;
            // 
            // cmenuRemove
            // 
            this.cmenuRemove.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                        this.btnRemoveAll,
                                                                                        this.btnRemoveNone});
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Index = 0;
            this.btnRemoveAll.Text = "Seleziona tutto";
            this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
            // 
            // btnRemoveNone
            // 
            this.btnRemoveNone.Index = 1;
            this.btnRemoveNone.Text = "Deseleziona tutto";
            this.btnRemoveNone.Click += new System.EventHandler(this.btnRemoveNone_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnAdd.Location = new System.Drawing.Point(8, 196);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.TabIndex = 4;
            this.btnAdd.Text = "Aggiungi";
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnRemove.Location = new System.Drawing.Point(96, 196);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "Rimuovi";
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnClose.Location = new System.Drawing.Point(512, 192);
            this.btnClose.Name = "btnClose";
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Chiudi";
            // 
            // labAdvice
            // 
            this.labAdvice.Location = new System.Drawing.Point(200, 192);
            this.labAdvice.Name = "labAdvice";
            this.labAdvice.Size = new System.Drawing.Size(280, 32);
            this.labAdvice.TabIndex = 7;
            this.labAdvice.Text = "Con il tasto destro è possibile selezionare o deselezionare tutte le righe di un " +
                "elenco";
            // 
            // frmMultipleSelection
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(600, 413);
            this.Controls.Add(this.labAdvice);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.labAdded);
            this.Controls.Add(this.gridAdded);
            this.Controls.Add(this.labToAdd);
            this.Controls.Add(this.gridToAdd);
            this.Name = "frmMultipleSelection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Selezione multipla";
            this.Resize += new System.EventHandler(this.frmMultipleSelection_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.gridToAdd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridAdded)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion


        private void frmMultipleSelection_Resize(object sender, System.EventArgs e) {
            int hform = Height;
            int available = hform - 150;
            int hgrid = available / 2;
            int level1 = hgrid + 70;
            gridAdded.Height = hgrid;
            gridToAdd.Height = hgrid;
            labAdded.Location = new Point(labAdded.Location.X, level1 + 16);
            labAdvice.Location = new Point(labAdvice.Location.X, level1);
            gridAdded.Location = new Point(gridToAdd.Location.X, level1 + 40);
            btnAdd.Location = new Point(btnAdd.Location.X, level1);
            btnRemove.Location = new Point(btnRemove.Location.X, level1);
            btnClose.Location = new Point(btnClose.Location.X, level1);
        }

        DataRow addRowToTable(DataTable T, DataRow R) {
            DataRow NewRow = T.NewRow();
            foreach (DataColumn C in T.Columns) {
                if (!R.Table.Columns.Contains(C.ColumnName)) continue;
                NewRow[C.ColumnName] = R[C.ColumnName];
            }
            T.Rows.Add(NewRow);
            NewRow.AcceptChanges();
            return NewRow;
        }

        void rimuoviRighe() {
            int nrows = Added.Rows.Count;
            ArrayList Selected = new ArrayList();
            ArrayList ToRemoveFromAdded = new ArrayList();
            for (int i = 0; i < nrows; i++) {
                if (gridAdded.IsSelected(i)) Selected.Add(i);
            }

            foreach (int index in Selected) {
                //Prende una riga selezionata
                gridAdded.CurrentRowIndex = index;
                DataRowView CurrDV = (DataRowView)gridToAdd.BindingContext[myDS, Added.TableName].Current;
                DataRow Curr = CurrDV.Row;
                ToRemoveFromAdded.Add(Curr);

                //La  aggiunge a ToAdd
                addRowToTable(ToAdd, Curr);
            }

            //Rimuove tutte le righe da Added
            foreach (DataRow ToRemove in ToRemoveFromAdded) {
                ToRemove.Delete();
                if (ToRemove.RowState != DataRowState.Detached) ToRemove.AcceptChanges();
            }
            updateSourceTable();
        }


        void aggiungiTutti() {
            //Seleziona tutto e chiama Aggiungi Righe
            int nrows = ToAdd.Rows.Count;
            for (int i = 0; i < nrows; i++) {
                gridToAdd.Select(i);
            }
            aggiungiRighe();
        }

        void aggiungiRighe() {
            int nrows = ToAdd.Rows.Count;
            ArrayList Selected = new ArrayList();
            ArrayList ToRemoveFromToAdd = new ArrayList();
            for (int i = 0; i < nrows; i++) {
                if (gridToAdd.IsSelected(i)) Selected.Add(i);
            }

            foreach (int index in Selected) {
                //Prende una riga selezionata
                gridToAdd.CurrentRowIndex = index;
                DataRowView CurrDV = (DataRowView)gridToAdd.BindingContext[myDS, ToAdd.TableName].Current;
                DataRow Curr = CurrDV.Row;
                ToRemoveFromToAdd.Add(Curr);

                //La  aggiunge ad Added
                addRowToTable(Added, Curr);
            }

            //Rimuove tutte le righe da ToAdd
            foreach (DataRow ToRemove in ToRemoveFromToAdd) {
                ToRemove.Delete();
                if (ToRemove.RowState != DataRowState.Detached) ToRemove.AcceptChanges();
            }
            updateSourceTable();
        }

        void updateSourceTable() {
            //Scollega le righe presenti in ToAdd, ove presenti in SourceTable
            DataRow[] ToAddRows = ToAdd.Select();
            List<DataRow> rowsToUnlink = new List<DataRow>();
            foreach (DataRow ToUnlinkRow in ToAddRows) {
                q unlinkkeyfilter = q.keyCmp(ToUnlinkRow);
                DataRow[] ToUnlinkRows = SourceTable.filter(unlinkkeyfilter);
                if (ToUnlinkRows.Length == 0) continue;
                DataRow ToUnlink = ToUnlinkRows[0];
                rowsToUnlink.Add(ToUnlink);                
            }
            controller.unlinkMultipleRows(rowsToUnlink);


            //Collega le righe presenti in Added, aggiungendole se non presenti
            DataRow[] AddedRows = Added.Select();
            foreach (DataRow ToLinkRow in AddedRows) {
                q linkkeyfilter = q.keyCmp(ToLinkRow);
                DataRow[] TolinkRows = SourceTable.filter(linkkeyfilter);
                DataRow AddedRow;
                if (TolinkRows.Length == 0) {
                    //La riga va aggiunta
                    AddedRow = addRowToTable(SourceTable, ToLinkRow);
                }
                else {
                    AddedRow = TolinkRows[0];
                }
                controller.CheckEntityChildRowAdditions(AddedRow, null);
            }

            HelpForm.SetDataGrid(gridAdded, Added);
            HelpForm.SetDataGrid(gridToAdd, ToAdd);

        }

        private void btnAdd_Click(object sender, System.EventArgs e) {
            aggiungiRighe();
        }

        private void btnRemove_Click(object sender, System.EventArgs e) {
            rimuoviRighe();
        }


        private void btnAddAll_Click(object sender, System.EventArgs e) {
            for (int i = 0; i < ToAdd.Rows.Count; i++) gridToAdd.Select(i);
        }

        private void btnAddNone_Click(object sender, System.EventArgs e) {
            for (int i = 0; i < ToAdd.Rows.Count; i++) gridToAdd.UnSelect(i);
        }

        private void btnRemoveAll_Click(object sender, System.EventArgs e) {
            for (int i = 0; i < Added.Rows.Count; i++) gridAdded.Select(i);
        }

        private void btnRemoveNone_Click(object sender, System.EventArgs e) {
            for (int i = 0; i < Added.Rows.Count; i++) gridAdded.UnSelect(i);
        }

    }
}

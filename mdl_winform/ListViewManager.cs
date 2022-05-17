using mdl;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace mdl_winform {
    public interface IListViewManager {
        void getListView(ListView listView, DataTable primaryTable);
        void setListView(ListView listView, DataTable primaryTable);
        void fillListView(DataTable realTable, object Context);
        void prefillListView(ListView listView, DataTable primaryTable,List<SelectBuilder> selList, IGetData getd);
    }

    public class ListViewManager : IListViewManager {
        


      

        protected DataTable getSecondaryTable(ListView l, DataSet d) {
            var tag = mdl_utils.tagUtils.GetStandardTag(l.Tag);
            var table = mdl_utils.tagUtils.GetTableName(tag);
            return d.Tables[table];
        }
        public void getListView(ListView listView, DataTable primaryTable) {
            DataRow P1Row = HelpForm.GetLastSelected(primaryTable);
            if (P1Row == null) return;
            DataTable otherParentTable = getSecondaryTable(listView, primaryTable.DataSet);

            DataTable Middle = QueryCreator.GetMiddleTable(primaryTable, otherParentTable);
            DataRelation PRel1 = QueryCreator.GetParentChildRel(primaryTable, Middle);
            DataRelation PRel2 = QueryCreator.GetParentChildRel(otherParentTable, Middle);

            //Updates (Add/Deletes) rows in middle table
            foreach (object LVIo in listView.Items) {
                if (!typeof(ListViewItem).IsAssignableFrom(LVIo.GetType())) continue;
                ListViewItem LVI = (ListViewItem)LVIo;
                DataRow P2Row = (DataRow)LVI.Tag;

                //Get Common child row if present
                string par1filter = QueryCreator.WHERE_REL_CLAUSE(
                    P1Row, PRel1.ParentColumns, PRel1.ChildColumns,
                    DataRowVersion.Current, false);
                string par2filter = QueryCreator.WHERE_REL_CLAUSE(
                    P2Row, PRel2.ParentColumns, PRel2.ChildColumns,
                    DataRowVersion.Current, false);
                string par12filter = GetData.MergeFilters(par1filter, par2filter);
                DataRow[] CurrChilds = Middle.Select(par12filter, null, DataViewRowState.CurrentRows);
                DataRow[] DelChilds = Middle.Select(par12filter, null, DataViewRowState.Deleted);
                if ((LVI.Checked) && (CurrChilds.Length == 0)) {
                    DataRow newMid;
                    //Middle Row must be added
                    if (DelChilds.Length > 0) {
                        //It was deleted... take it back
                        DelChilds[0].RejectChanges();
                        newMid = DelChilds[0];
                        //DelChilds[0].AcceptChanges();
                    }
                    else {
                        newMid = Middle.NewRow();
                    }
                    RowChange.MakeChild(P1Row, primaryTable, newMid, null);
                    RowChange.MakeChild(P2Row, otherParentTable, newMid, null);
                    if (DelChilds.Length == 0) Middle.Rows.Add(newMid);
                }
                if ((!LVI.Checked) && (CurrChilds.Length > 0)) {
                    //Middle Row must be removed
                    CurrChilds[0].Delete();
                }
            }
        }


        public void setListView(ListView listView, DataTable primaryTable) {           
            DataRow P1Row = HelpForm.GetLastSelected(primaryTable);
            if (P1Row == null) return;
            DataTable otherParentTable = getSecondaryTable(listView,primaryTable.DataSet);
            DataTable Middle = QueryCreator.GetMiddleTable(primaryTable, otherParentTable);
            DataRelation PRel1 = QueryCreator.GetParentChildRel(primaryTable, Middle);
            DataRelation PRel2 = QueryCreator.GetParentChildRel(otherParentTable, Middle);

            //Checks / Unchecks items dependingly on current primary table row
            foreach (object LVIo in listView.Items) {
                if (!typeof(ListViewItem).IsAssignableFrom(LVIo.GetType())) continue;
                ListViewItem LVI = (ListViewItem)LVIo;
                DataRow P2Row = (DataRow)LVI.Tag;

                //Get Common child row if present
                string par1filter = QueryCreator.WHERE_REL_CLAUSE(
                    P1Row, PRel1.ParentColumns, PRel1.ChildColumns,
                    DataRowVersion.Current, false);
                string par2filter = QueryCreator.WHERE_REL_CLAUSE(
                    P2Row, PRel2.ParentColumns, PRel2.ChildColumns,
                    DataRowVersion.Current, false);
                string par12filter = GetData.MergeFilters(par1filter, par2filter);
                DataRow[] CurrChilds = Middle.Select(par12filter, null, DataViewRowState.CurrentRows);
                if (CurrChilds.Length == 0) {
                    //Must Uncheck
                    LVI.Checked = false;
                }
                else {
                    LVI.Checked = true;
                }

            }

        }

        class FillListViewContext {
            internal ListView L;
            internal DataTable T2;
            internal string ValueMember;
            internal string DisplayMember;
            internal bool freshvalue;
            internal object OldValue;

        }
        public void fillListView(DataTable realTable, object Context) {
            FillListViewContext Ctx = Context as FillListViewContext;
            ListView L = Ctx.L;
            DataTable parent2 = Ctx.T2;

            //Sets ListView Header Columns
            L.BeginUpdate();
            L.Clear();
            string[] colnames = new string[parent2.Columns.Count];
            int ncols = 0;
            Graphics GG = Graphics.FromHwnd(L.FindForm().Handle);
            int[] sizes = new int[parent2.Columns.Count];
            foreach (DataColumn C in parent2.Columns) {
                if (C.Caption == "") continue;
                colnames[ncols] = C.ColumnName;
                sizes[ncols] = Convert.ToInt32(GG.MeasureString(C.ColumnName, L.Font).Width) + 5;
                ncols++;
            }

            string[] items = new string[ncols];
            //Fills ListBox
            foreach (DataRow R in parent2.Rows) {
                for (int i = 0; i < ncols; i++) {
                    string colname = colnames[i];
                    items[i] = mdl_utils.HelpUi.StringValue(R[colname],
                        "",
                        parent2.Columns[colname]);
                    int ss = Convert.ToInt32(GG.MeasureString(items[i], L.Font).Width) + 5;
                    if (sizes[i] < ss) sizes[i] = ss;
                }
                ListViewItem LVII = new ListViewItem(items[0]);
                LVII.Tag = R;
                for (int j = 1; j < ncols; j++) LVII.SubItems.Add(items[j]);
                L.Items.Add(LVII);
            }
            int ii = 0;
            foreach (DataColumn C in parent2.Columns) {
                if (C.Caption == "") continue;
                L.Columns.Add(C.Caption, sizes[ii], (System.Windows.Forms.HorizontalAlignment ) mdl_utils.HelpUi.GetAlignForColumn(C));
                ii++;
            }

            L.CheckBoxes = true;
            L.FullRowSelect = true;
            L.View = View.Details;
            L.GridLines = true;

            L.EndUpdate();
            L.Refresh();
        }

        public void prefillListView(ListView listView, DataTable primaryTable,List<SelectBuilder> selList, IGetData getd) {

            string tag = mdl_utils.tagUtils.GetStandardTag(listView.Tag);
            string table = mdl_utils.tagUtils.GetTableName(tag);
            DataTable otherParentTable = primaryTable.DataSet.Tables[table];
            if (!PostData.IsTemporaryTable(otherParentTable)) {
                GetData.CacheTable(otherParentTable);
            }


            var FC = new FillListViewContext();
            FC.L = listView;
            FC.T2 = otherParentTable;


            SelectBuilder sel = getd.DO_GET_TABLE(otherParentTable, null, null, true, null, selList);


            if (sel == null) {
                fillListView(otherParentTable, FC);
            }
            else {
                sel.AddOnRead(fillListView, FC);
            }

        }

    }
}

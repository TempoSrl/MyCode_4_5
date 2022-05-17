using System;
using System.Collections;
using System.Data;

namespace mdl {
    /// <summary>
    /// Helper class interface that retrieves data for treeviews
    /// </summary>
    public interface ITreeViewDataAccess {
        /// <summary>
        /// Gets some row from a datatable, with all child rows in the same table
        /// </summary>
        /// <param name="T">DataTable to Get from DataBase</param>
        /// <param name="filter">Filter to apply in order to retrieve roots</param>
        /// <param name="clear">true if table has to be cleared</param>
        void DO_GET_TABLE_ROOTS(DataTable T, string filter, bool clear);

		/// <summary>
		/// Gets all necessary rows from table in order to rebuild R genealogy
		/// </summary>
		/// <param name="R"></param>
		/// <param name="AddChild">when true, all child of every parent found are retrieved  </param>
		/// <param name="autoParentRelation"></param>
		void DO_GET_PARENTS(DataRow R, bool AddChild, DataRelation autoParentRelation);

        /// <summary>
        /// Gets a row from a table T taking the first row by the filter
        ///  StartConndition AND (startfield like startval%)
        /// If more than oe row is found, the one with the smallest startfield is
        ///  returned. Used for AutoManage functions.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="StartCondition"></param>
        /// <param name="startval"></param>
        /// <param name="startfield"></param>
        /// <returns>null if no row was found</returns>
        DataRow GetSpecificChild(DataTable T,
            string StartCondition,
            string startval,
            string startfield);
      
        /// <summary>
        /// Gets child of a selected set of rows, and gets related tables
        /// </summary>
        /// <param name="ToExpand"></param>
        void expandChilds(DataRow[] ToExpand);

        /// <summary>
        /// Gets a primary table DataRow from db, given its primary key
        /// </summary>
        /// <param name="dest">Table into which putting the row read</param>
        /// <param name="Key">DataRow with the same key as wanted row</param>
        /// <returns>null if row was not found</returns>
        DataRow GetByKey(DataTable dest, DataRow Key);
    }

    /// <summary>
    /// implementation of  a ITreeViewDataAccess
    /// </summary>
    public class TreeViewDataAccess : ITreeViewDataAccess {
        /// <summary>
        /// GetData used
        /// </summary>
        public IGetData getData;

        /// <summary>
        /// QueryHelper used for d access
        /// </summary>
        public QueryHelper Qhs;

        /// <summary>
        /// MetaModel used
        /// </summary>
        private IMetaModel model;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="getData"></param>
        /// <param name="qhs"></param>
        public TreeViewDataAccess(IGetData getData, QueryHelper qhs) {
            this.getData = getData;
            this.Qhs = qhs;
            model = MetaFactory.factory.getSingleton<IMetaModel>();
        }
      

        /// <summary>
        /// Gets child of a selected set of rows, and gets related tables
        /// </summary>
        /// <param name="toExpand"></param>
        public void expandChilds(DataRow[] toExpand) {
            if (toExpand.Length == 0) return;
            var T = toExpand[0].Table;
            var toVisit = new Hashtable {[T.TableName] = T};
            getData.GetAllChildRows(toExpand, toVisit, null);
        }

        /// <summary>
        /// Gets some row from a datatable, with all child rows in the same table
        /// </summary>
        /// <param name="T">DataTable to Get from DataBase</param>
        /// <param name="filter">Filter to apply in order to retrieve roots</param>
        /// <param name="clear">true if table has to be cleared</param>
        public void DO_GET_TABLE_ROOTS(DataTable T, string filter, bool clear) {
            //getData.ReadCached();           //HO RIMOSSO questa riga nella fase di refactoring 
            if (!model.canRead(T)) return;
            //string sort = GetData.GetSorting(T,null);
            getData.DO_GET_TABLE(T, null, filter, clear, null, null);

            //TableHasBeenRead(T); //HO RIMOSSO questa riga nella fase di refactoring 
        }


        /// <summary>
        /// Gets all necessary rows from table in order to rebuild R genealogy
        /// </summary>
        /// <param name="r"></param>
        /// <param name="addChild">when true, all child of every parent found are retrieved </param>
        /// <param name="autoParentRelation"></param>
        public void DO_GET_PARENTS(DataRow r, bool addChild, DataRelation autoParentRelation) {
            var handle = mdl_utils.metaprofiler.StartTimer("DO_GET_PARENTS");
            try {
                var parents = new DataRow[20];
                parents[0] = r;
                var found = 1;

                var allowed = new Hashtable();
                var T = r.Table;
                allowed[T.TableName] = T;

                var autoParent = autoParentRelation;//GetAutoParentRelation(T);
                if (autoParent == null) return;
                //Get the strict genealogy of R (max 20 levels)
                while (found < 20) {
                    //Gets the parent of Parents[found-1];
                    var child = parents[found - 1];
                    var res = getData.GetParentRows(child, allowed, null);

                    //finds parent of Child
                    var foundparents = child.iGetParentRows(autoParent);
                    if (foundparents.Length != 1) break;
                    parents[found] = foundparents[0];
                    found++;
                    if (res) break;
                }
                if (!addChild) return;
                if (found == 1) return;
                //			if (res) {
                //				found--; //skip last parent, which was already in tree
                //			}
                var list = new DataRow[found - 1];
                for (var i = 1; i < found; i++) list[i - 1] = parents[i];
                expandChilds(list);
            }
            finally {
                mdl_utils.metaprofiler.StopTimer(handle);
            }
        }

        /// <summary>
        /// Gets a primary table DataRow from db, given its primary key
        /// </summary>
        /// <param name="dest">Table into which putting the row read</param>
        /// <param name="key">DataRow with the same key as wanted row</param>
        /// <returns>null if row was not found</returns>
        public virtual DataRow GetByKey(DataTable dest, DataRow key) {
            return getData.GetByKey(dest, key);
        }

        /// <summary>
        /// Gets a row from a table T taking the first row by the filter
        ///  StartConndition AND (startfield like startval%)
        /// If more than oe row is found, the one with the smallest startfield is
        ///  returned. Used for AutoManage functions.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="StartCondition"></param>
        /// <param name="startval"></param>
        /// <param name="startfield"></param>
        /// <returns>null if no row was found</returns>
        public DataRow GetSpecificChild(DataTable T,
            string StartCondition,
            string startval,
            string startfield) {
            //if (!startval.Contains("%")) startval += "%";
            string filter = Qhs.AppAnd(StartCondition, Qhs.Like(startfield, startval));
            getData.DO_GET_TABLE(T, "len(" + startfield + ")", filter, true, "1", null);
            if (T.Rows.Count == 0) return null;
            return T.Rows[0];
        }
    }
}

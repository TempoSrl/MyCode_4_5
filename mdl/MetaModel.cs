using System;
using System.Collections.Generic;
using System.Data;

namespace mdl {

    public enum TableAction {
        beginLoad, endLoad,startClear, endClear
    }

    /// <summary>
    /// Interface for a model manager
    /// </summary>
    public interface IMetaModel {

        void clear(DataTable T);

        void clearActions(DataTable T, TableAction actionType, Action<DataTable> a);
        void setAction(DataTable T,  TableAction actionType, Action<DataTable> a, bool clear=false);
        void invokeActions(DataTable T, TableAction actionType );


        /// <summary>
        /// Mark a table for skipping security controls
        /// </summary>
        /// <param name="T"></param>
        /// <param name="value"></param>
        void setSkipSecurity(DataTable T, bool value);

        /// <summary>
        /// Check if a table ha been marked as SkipSecurity
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool isSkipSecurity(DataTable T);

        /// <summary>
        /// Unlink R from parent-child relation with primary table. I.E., R stops being a child of main row. 
        /// If R becomes unchanged, it is removed from DataSet
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        DataRow UnlinkDataRow(DataTable primaryTable, DataRow r);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        string getNotEntityChildFilter(DataTable t);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="filter"></param>
        void setNotEntityChildFilter(DataTable t, string filter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="child"></param>
        void MarkTableAsNotEntityChild(DataTable primary, DataTable child);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="child"></param>
        /// <param name="relName"></param>
        void MarkTableAsNotEntityChild(DataTable primaryTable, DataTable child, string relName);

        /// <summary>
        /// Set the table as NotEntitychild. So the table isn't cleared during freshform and refills
        /// </summary>
        /// <param name="T"></param>
        /// <param name="ParentRelName"></param>
        void addNotEntityChild(DataTable T, string ParentRelName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="RelName"></param>
        void addNotEntityChildFilter(DataTable child, string RelName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="child"></param>
        void addNotEntityChild(DataTable primaryTable, DataTable child);


        /// <summary>
        /// Establish if R is a Entity-SubEntity relation
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        bool isSubEntityRelation(DataRelation R);
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="child"></param>
        void addNotEntityChildFilter(DataTable primaryTable, DataTable child);

        /// <summary>
        ///  Set the extra parameter for a table
        /// </summary>
        /// <param name="t"></param>
        /// <param name="o"></param>
        void setExtraParams(DataTable t, object o);

        /// <summary>
        /// Get the extra parameter from a table
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        object getExtraParams(DataTable t);

        /// <summary>
        /// Remove a table from being a  NotEntitychild
        /// </summary>
        /// <param name="T"></param>        
        void UnMarkTableAsNotEntityChild(DataTable T);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        void AllowAllClear(DataSet ds);

        /// <summary>
        /// Check if an entity has changes
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="primary"></param>
        /// <param name="sourceRow"></param>
        /// <param name="isSubentity"></param>
        /// <returns></returns>
        bool hasChanges(DataSet ds, DataTable primary, DataRow sourceRow, bool isSubentity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Dest"></param>
        /// <param name="TDest"></param>
        /// <param name="Source"></param>
        /// <param name="RSource"></param>
        /// <returns></returns>
        bool XVerifyRowChange(DataSet Dest, DataTable TDest, DataSet Source, DataRow RSource);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="TDest"></param>
        /// <param name="rif"></param>
        /// <param name="rSource"></param>
        /// <returns></returns>
        bool XVerifyChangeChilds(DataSet dest, DataTable TDest, DataSet rif, DataRow rSource);


        /// <summary>
        /// Check if a table is not an entity child
        /// </summary>
        /// <param name="childTable"></param>
        /// <returns></returns>
        bool isNotEntityChild(DataTable childTable);

        /// <summary>
        /// Check whether a Table as a cached one
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool isCached(DataTable T);

        /// <summary>
        /// Establish a Table as a cached one
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        void cacheTable(DataTable T);


        /// <summary>
        /// Establish a Table as a not cached one
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        void uncacheTable(DataTable T);

        /// <summary>
        /// Establish a filtered cached table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="addBlankRow"></param>
        void cacheTable(DataTable T, object filter, string sort, bool addBlankRow);

        /// <summary>
        /// Establish that if a blank row will be added to the table when it is emptied
        /// </summary>
        /// <param name="T"></param>
        void markToAddBlankRow(DataTable T);

        /// <summary>
        /// Sets a filter that will be applied  every times that a table will be read from db
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        void setStaticFilter(DataTable T, object filter);

        /// <summary>
        /// blocks further reads by the framework of a table 
        /// </summary>
        /// <param name="T"></param>
        void lockRead(DataTable T);

        /// <summary>
        /// Set a table as "read". Has no effect if table isn't a child table
        /// </summary>
        /// <param name="T"></param>
        void tableHasBeenRead(DataTable T);

        /// <summary>
        /// blocks empty actions by the framework of a table 
        /// </summary>
        /// <param name="T"></param>
        void denyClear(DataTable T);

        /// <summary>
        /// allow empty actions by the framework of a table 
        /// </summary>
        /// <param name="T"></param>
        void allowClear(DataTable T);


        /// <summary>
        /// Checks if a table can be cleared by the framework
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool canClear(DataTable T);

        /// <summary>
        /// Check if a blank row will be added to the table when it is emptied
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool markedToAddBlankRow(DataTable T);


        /// <summary>
        /// Checks if a table is b
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool canRead(DataTable T);
    }

  
    /// <summary>
    /// Manages conventions over the model
    /// </summary>
    public class MetaModel : IMetaModel {

        static Dictionary <TableAction, string> actionNames = new Dictionary<TableAction, string>(){
            { TableAction.beginLoad , "mdl_beginLoad" },
            { TableAction.endLoad , "mdl_endLoad" },
            { TableAction.startClear , "mdl_startClear" },
            { TableAction.endClear , "mdl_endClear" }
         


        };

        /// <summary>
        /// Clears a DataTable setting the rowindex of the linked grid to 0
        /// </summary>
        /// <param name="T"></param>
        public virtual void clear(DataTable T) {
            if (T.Rows.Count == 0) return;
            var metaclear = mdl_utils.metaprofiler.StartTimer($"MyClear * {T.TableName}");
            invokeActions(T, TableAction.startClear);
            invokeActions(T, TableAction.beginLoad);
            
            T.BeginLoadData();
            T.Clear();

            T.EndLoadData();
            invokeActions(T, TableAction.endLoad);
            invokeActions(T, TableAction.endClear);
            
            mdl_utils.metaprofiler.StopTimer(metaclear);
        }

        public virtual void clearActions(DataTable T, TableAction actionType, Action<DataTable> a) {
            T.ExtendedProperties[actionNames[actionType]] = null;
        }
        public virtual void setAction(DataTable T, TableAction actionType, Action<DataTable> a, bool clear = false) {
            tableEventManager actions = clear? null: T.ExtendedProperties[actionNames[actionType]] as tableEventManager;
            if (actions == null) {
                actions = new tableEventManager();
                T.ExtendedProperties["mdl_EndLoad"] = actions;
            }
            actions.addAction(a);
        }
        public virtual void invokeActions(DataTable T, TableAction actionType) {
            var actions = T.ExtendedProperties[actionNames[actionType]] as tableEventManager;
            actions?.invokeAction(T);
        }



        /// <summary>
        /// Tells if a table should be cleared and read again during a refresh.
        /// Cached tables are not read again during refresh if they have been already been read
        /// </summary>
        /// <param name="T"></param>
        /// <returns>true if table should be read</returns>
        public bool canRead(DataTable T){
            if (T.ExtendedProperties["cached"]==null) return true;
            if (T.ExtendedProperties["cached"].ToString()=="0")return true;
            return false;
        }

        public bool isSubEntityRelation(DataRelation R) {
	        if (R.ExtendedProperties["isSubentity"] != null) return (bool) R.ExtendedProperties["isSubentity"];
	        var Parent = R.ParentTable;
	        var Child = R.ChildTable;
			
	        foreach (var C in R.ParentColumns){
		        var found=false;				
		        //searches Relation parent columns in the primary key of parent table
		        foreach (var K in Parent.PrimaryKey){
			        if (K.ColumnName==C.ColumnName){
				        found=true;
				        break;
			        }                    
		        }

		        if (!found) {
			        R.ExtendedProperties["isSubentity"] = false;
			        return false;
		        }
	        }

	        if (R.ParentColumns.Length != Parent.PrimaryKey.Length) {
		        R.ExtendedProperties["isSubentity"] = false;
		        return false;
	        }

	        //Check that ALL columns of primary table must be key for child
	        foreach (var C in R.ChildColumns){
		        var found=false;				
		        //searches Relation parent columns in the primary key of parent table
		        foreach (var K in Child.PrimaryKey){
			        if (K.ColumnName==C.ColumnName){
				        found=true;
				        break;
			        }                    
		        }

		        if (!found) {
			        R.ExtendedProperties["isSubentity"] = false;
			        return false;
		        }
	        }
	        R.ExtendedProperties["isSubentity"] = true;
			
	        return true;
		}

        /// <summary>
        /// Mark a table for skipping security controls
        /// </summary>
        /// <param name="T"></param>
        /// <param name="value"></param>
        public void setSkipSecurity(DataTable T,bool value) {
            T.ExtendedProperties["SkipSecurity"] = value ? true : (object) null;
        }
        
        /// <summary>
        /// Check if a table ha been marked as SkipSecurity
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public bool isSkipSecurity(DataTable T) {
            return T.ExtendedProperties["SkipSecurity"] != null;
        }

        /// <summary>
        /// Must be called for combobox-related tables
        /// </summary>
        /// <param name="T"></param>
        public void markToAddBlankRow(DataTable T){
            T.ExtendedProperties["AddBlankRow"]=true;
        }

        /// <summary>
        /// Check if a table was marjed to add a blank row 
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public bool markedToAddBlankRow(DataTable T){
            return T.ExtendedProperties["AddBlankRow"]!=null;
        }

        /// <summary>
        /// Apply a filter on a table during any further read
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        public  void setStaticFilter(DataTable T, object filter){
            T.ExtendedProperties["filter"]=filter;
        }

        
        

        internal class tableEventManager {
	        delegate void tableHandler(DataTable t);
	        event tableHandler actions;

	        public void addAction(Action<DataTable> a) {
		        actions += new tableHandler(a);
	        }

	        public void invokeAction(DataTable t) {
                actions?.Invoke(t);
	        }

        }

     
        /// <summary>
        /// Set Table T to be read once for all when ReadCached will be called next time
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="addBlankRow">when true, a blank row is added as first row of T</param>
        public void cacheTable(DataTable T,object filter, string sort, bool addBlankRow){
            T.ExtendedProperties["cached"]="0";
            if (addBlankRow) markToAddBlankRow(T);
            if (sort!=null) T.ExtendedProperties["sort_by"] = sort;
            if (filter!=null) setStaticFilter(T,filter);
        }

        /// <summary>
        /// Deny table clear when DO_GET() is called. If this is not called, a
        ///   table that is not cached, entity or subentity will be cleared during DO_GET
        /// </summary>
        /// <param name="T"></param>
        public  void denyClear(DataTable T){
            T.ExtendedProperties["DenyClear"]="y";
        }

        /// <summary>
        /// Re-Allow table clear when DO_GET() is called. Undoes the effect of a DenyClear
        /// </summary>
        /// <param name="T"></param>
        public  void allowClear(DataTable T){
            T.ExtendedProperties["DenyClear"]=null;
        }

        /// <summary>
        /// Tells if Table will be cleared during next DO_GET()
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public  bool canClear(DataTable T){
            if (T.ExtendedProperties["DenyClear"]==null) return true;
            return false;
        }

        /// <summary>
        /// Tells GetData to read T once for all
        /// </summary>
        /// <param name="T"></param>
        public void cacheTable(DataTable T){
            T.ExtendedProperties["cached"]="0";
        }

        /// <summary>
        /// Tells GetData to read T once for all
        /// </summary>
        /// <param name="T"></param>
        public void uncacheTable(DataTable T){
            T.ExtendedProperties["cached"]=null;
        }

        
        /// <summary>
        /// Table T will never be read. It is marked like a cached table that has already been read.
        /// </summary>
        /// <param name="T"></param>
        public void lockRead(DataTable T){
            T.ExtendedProperties["cached"]="1";
        }

        /// <summary>
        /// Set a table as "read". Has no effect if table isn't a chaed table
        /// </summary>
        /// <param name="T"></param>
        public void tableHasBeenRead(DataTable T){
            if (T.ExtendedProperties["cached"]==null) return;
            if (T.ExtendedProperties["cached"].ToString()=="0")lockRead(T);
        }

        /// <summary>
        /// Returns true if table is cached (the table may or may not 
        ///  have been read) 
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public bool isCached(DataTable T){
            if (T.ExtendedProperties["cached"]==null) return false;
            return true;
        }

        //private DataTable primaryTable;
        //private DataSet ds { get { return primaryTable?.DataSet; } }

        /// <summary>
        /// class instance of cqueryhelper
        /// </summary>
        protected QueryHelper q = new CQueryHelper();
        


        /// <inheritdoc />
        public virtual void setExtraParams(DataTable t, object o) {
            t.ExtendedProperties["ExtraParameters"] = o;
        }

        /// <inheritdoc />
        public virtual object getExtraParams(DataTable t) {
            return t.ExtendedProperties["ExtraParameters"];
        }


        /// <summary>
        /// Returns true if there are unsaved changes
        /// </summary>
        /// <returns></returns>
        public bool hasChanges(DataSet ds, DataTable primary, DataRow sourceRow, bool isSubentity) {
            var handle = mdl_utils.metaprofiler.StartTimer("HasUnsavedChanges()");
            try {                
                PostData.RemoveFalseUpdates(ds);
                //return ds.HasChanges();

                //Per una subentità (detail form) confronta i dati con quelli dell'origine

                if (!isSubentity) return ds.HasChanges();
                DataSet sourceDataSet = sourceRow.Table.DataSet;
                if (primary.Rows.Count == 0) return false;
                var myRow = primary.Rows[0];
                if (XVerifyChangeChilds(sourceDataSet, sourceRow.Table, ds, myRow)) return true;
                return XVerifyChangeChilds(ds, primary, sourceDataSet, sourceRow);
            }
            finally {
                mdl_utils.metaprofiler.StopTimer(handle);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Dest"></param>
        /// <param name="TDest"></param>
        /// <param name="Rif"></param>
        /// <param name="RSource"></param>
        /// <returns></returns>
        public bool XVerifyChangeChilds(DataSet Dest, DataTable TDest, DataSet Rif, DataRow RSource) {
            DataTable T = RSource.Table;
            //if (RSource.RowState != DataRowState.Unchanged) return true;
            if (XVerifyRowChange(Dest, TDest, Rif, RSource)) return true;
            foreach (DataRelation Rel in T.ChildRelations) {
                if (!Dest.Tables.Contains(Rel.ChildTable.TableName)) continue;
                if (!GetData.CheckChildRel(Rel)) continue; //not a subentityrel
                foreach (DataRow Child in RSource.iGetChildRows(Rel)) {
                    if (XVerifyChangeChilds(Dest, Dest.Tables[Child.Table.TableName], Rif, Child)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Restituisce true se ci sono differenze nella riga considerata
        /// </summary>
        /// <param name="Dest"></param>
        /// <param name="TDest"></param>
        /// <param name="Source"></param>
        /// <param name="RSource"></param>
        /// <returns></returns>
        public bool XVerifyRowChange(DataSet Dest, DataTable TDest, DataSet Source, DataRow RSource) {
            if (RSource.RowState == DataRowState.Deleted) return false;
            //string source_unaliased = DataAccess.GetTableForReading(RSource.Table);

            //DataTable TDest= Dest.Tables[source_unaliased];
            var TSource = RSource.Table;
            string filter = q.CmpKey(RSource);
            DataRow[] found = TDest.Select(filter);
            if (found.Length == 0) return true;
            foreach (DataColumn C in TSource.Columns) {
                if (QueryCreator.IsTemporary(C)) continue;
                if (!TDest.Columns.Contains(C.ColumnName)) continue;
                if (QueryCreator.IsTemporary(TDest.Columns[C.ColumnName])) continue;
                if (found[0][C.ColumnName].Equals(RSource[C.ColumnName])) continue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unlink R from parent-child relation with primary table. I.E., R becomes a not-child of main row. 
        /// If R becomes unchanged, it is removed from DataSet
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="R"></param>
        /// <returns></returns>
        public DataRow UnlinkDataRow(DataTable primaryTable, DataRow R) {
            if (R == null) return null;
            var SourceTable = R.Table;
            //Unlink R from parent-child relation with primary table.
            DataRelation Rfound = null;
            foreach (DataRelation Rel in primaryTable.ChildRelations) {
                if (Rel.ChildTable == SourceTable) {
                    Rfound = Rel;
                    foreach (DataColumn C in Rfound.ChildColumns) {
                        if (QueryCreator.IsPrimaryKey(SourceTable, C.ColumnName)) continue;
                        R[C.ColumnName] = DBNull.Value;
                    }
                }
            }
            if (Rfound == null) {
                ErrorLogger.Logger.markEvent($"Can't unlink. DataTable {SourceTable.TableName} is not child of {primaryTable.TableName}.");
                return null;
            }
            if (PostData.CheckForFalseUpdate(R)) {  //toglie la riga se inutile
                R.Delete();
                R.AcceptChanges();
            }
            return R;
        }


        
        /// <summary>
        /// Set the table as NotEntitychild. So the table isn't cleared during freshform and refills
        /// </summary>
        /// <param name="T"></param>
        /// <param name="parentRelName"></param>
        public void addNotEntityChild(DataTable T, string parentRelName) {
            T.setDenyClear();
            addNotEntityChildFilter(T, parentRelName);
        }


        /// <summary>
        /// Set the table as NotEntitychild. So the table isn't cleared during freshform and refills
        /// </summary>
        /// <param name="T"></param>
        /// <param name="child"></param>
        public void addNotEntityChild(DataTable T, DataTable child) {
            child.setDenyClear();
            addNotEntityChildFilter(T, child);
        }

        /// <summary>
        /// Sets all "NotSubEntityChild" tables as "CanClear". Called when form is cleared or data
        ///  is posted
        /// </summary>
        public void AllowAllClear(DataSet ds) {
            foreach (DataTable T in ds.Tables) {
                if (isNotEntityChild(T)) {
                    T.setAllowClear();
                    clearNotEntityChild(T); ;
                }
            }
        }

        /// <summary>
        /// Establish that a table has to be considered as a child even though it is not a pure subentity
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="child"></param>
        /// <param name="relName"></param>
        public void MarkTableAsNotEntityChild(DataTable primaryTable, DataTable child, string relName) {
            //Bisogna fare denyclear altrimenti la tabella non è preservata
            child.setDenyClear();
            if (relName == null) {
                addNotEntityChild(primaryTable, child);
            }
            else {
                addNotEntityChild(child, relName);
            }
        }

        /// <summary>
        /// Establish that a table has to be considered as a child even though it is not a pure subentity
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="child"></param>
        public void MarkTableAsNotEntityChild(DataTable primaryTable, DataTable child) {
            //Bisogna fare denyclear altrimenti la tabella non è preservata
            child.setDenyClear();
            addNotEntityChild(primaryTable, child);
        }

        /// <summary>
        /// Remove a table from being a  NotEntitychild
        /// </summary>
        /// <param name="T"></param>
        public void UnMarkTableAsNotEntityChild(DataTable T) {
            T.setAllowClear();
            clearNotEntityChild(T);
        }



        /// <summary>
        /// removes the filter for a NotEntityChild filter
        /// </summary>
        /// <param name="childTable"></param>
        void clearNotEntityChild(DataTable childTable) {
            setNotEntityChildFilter(childTable, (string) null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="childTable"></param>
        /// <returns></returns>
        public bool isNotEntityChild(DataTable childTable) {
            return getNotEntityChildFilter(childTable) != null;
        }

        /// <summary>
        /// Get the filter for a NotEntityChild filter
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public string getNotEntityChildFilter(DataTable t) {
            return t.ExtendedProperties["NotEntityChild"] as string;
        }

        /// <summary>
        /// Set the filter for a NotEntityChild filter
        /// </summary>
        /// <param name="t"></param>
        /// <param name="filter"></param>
        public void setNotEntityChildFilter(DataTable t, string filter) {
            t.ExtendedProperties["NotEntityChild"] =filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="relName"></param>
        public virtual void addNotEntityChildFilter(DataTable child, string relName) {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (relName == null) throw new ArgumentNullException(nameof(relName));


            if (!child.DataSet.Relations.Contains(relName)) return;
            var rel = child.DataSet.Relations[relName];
            string filter = null;
            foreach (var c in rel.ChildColumns) {
                if (QueryCreator.IsPrimaryKey(child, c.ColumnName)) continue;
                filter = q.AppAnd(filter, q.IsNull(c.ColumnName));
            }
            setNotEntityChildFilter(child,filter);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="primaryTable"></param>
        /// <param name="child"></param>
        public virtual void addNotEntityChildFilter(DataTable primaryTable, DataTable child) {
            if (isNotEntityChild(child)) return;
            var r = QueryCreator.GetParentChildRel(primaryTable, child);
            if (r == null) return;
            string filter = null;
            foreach (DataColumn c in r.ChildColumns) {
                if (QueryCreator.IsPrimaryKey(child, c.ColumnName)) continue;
                filter = q.AppAnd(filter, q.IsNull(c.ColumnName));
            }
            setNotEntityChildFilter(child, filter);
        }

       


    }
}

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using q  = mdl.MetaExpression;
using static mdl_utils.metaprofiler;

#pragma warning disable IDE1006 // Naming Styles



namespace mdl {
    /// <summary>
    /// Interface for GetData class
    /// </summary>
    public interface IGetData {
        /// <summary>
        /// Dispose all resource
        /// </summary>
        void Destroy();

        /// <summary>
        /// Primary Table Name
        /// </summary>
        string PrimaryTable { get; }

        /// <summary>
        /// Primary Table of the DataSet. Primary Table is the first table scanned 
        ///  when data is read from db.
        /// </summary>
        DataTable PrimaryDataTable { get; }

        /// <summary>
        /// Used ISecurity
        /// </summary>
        ISecurity security { get; set; }

        /// <summary>
        /// Used MetaFactory
        /// </summary>
        IMetaFactory factory { get; set; }
    

    /// <summary>
        /// Set a table as "cached", i.e. it will be read only one time 
        ///		in the life of the form.
        /// </summary>
        /// <param name="tablename"></param>
        void CacheTable(string tablename);

        /// <summary>
        /// Read all tables marked as "ToCache" with CacheTable() that haven't yet been read
        /// </summary>
        void ReadCached();

        /// <summary>
        /// Initialize class. Necessary before doing any other operation
        /// </summary>
        /// <param name="DS"></param>
        /// <param name="Conn"></param>
        /// <param name="PrimaryTable"></param>
        /// <returns></returns>
        string InitClass(DataSet DS, DataAccess Conn, string PrimaryTable);

        /// <summary>
        /// Initialize class. Necessary before doing any other operation
        /// </summary>
        /// <param name="DS"></param>
        /// <param name="Conn"></param>
        /// <param name="PrimaryTable"></param>
        /// <returns></returns>
        string InitClass(DataSet DS, IDataAccess Conn,  string PrimaryTable);

        /// <summary>
        /// Apply a filter on a table during any further read
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        void SetStaticFilter(string tablename, string filter);

        /// <summary>
        /// Clears all tables except for temporary and cached (including pre-filled combobox).
        /// Also undoes the effect of denyclear on all secondary tables setting tables 
        ///  with AllowClear()
        /// </summary>
        void CLEAR_ENTITY();

        /// <summary>
        /// Fill the primary table starting with a row equal to Start. Start Row should not
        ///  belong to PrimaryTable. Infact PrimaryTable is cleared before getting values from Start fields 
        /// </summary>
        /// <param name="Start"></param>
        void START_FROM(DataRow Start);

        /// <summary>
        /// Fill the primary table with a row searched from database. R is not required to belong to
        ///  PrimaryTable, but should have the same primary key columns.
        /// </summary>
        /// <param name="R"></param>
        void SEARCH_BY_KEY(DataRow R);

        /// <summary>
        /// Gets a primary table DataRow from db, given its primary key
        /// </summary>
        /// <param name="Dest">Table into which putting the row read</param>
        /// <param name="Key">DataRow with the same key as wanted row</param>
        /// <returns>null if row was not found</returns>
        DataRow GetByKey(DataTable Dest, DataRow Key);

        /// <summary>
        /// Try to get a row from an in-memory view if there is one. This function
        ///  is obsolete cause now it's possible to write view table as if they
        ///  were real table.
        /// </summary>
        /// <param name="Dest"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        DataRow GetFromViewByKey(DataTable Dest, DataRow Key);

        /// <summary>
        /// Clears &amp; Fill the primary table with all records from a database table
        /// </summary>
        /// <param name="filter"></param>
        void GET_PRIMARY_TABLE(string filter);

        /// <summary>
        /// Gets all data of the DataSet cascated-related to the primary table.
        /// The first relations considered are child of primary, then
        ///  proper child / parent relations are called in cascade style.
        /// </summary>
        ///  <param name="onlyperipherals">if true, only peripheral (not primary or secondary) tables are refilled</param>
        ///  <param name="OneRow">The (eventually) only primary table row on which
        ///   get the entire sub-graph. Can be null if PrimaryDataTable 
        ///   already contains rows.  R is not required to belong to PrimaryDataTable.</param>
        /// <returns>always true</returns>
        bool DO_GET(bool onlyperipherals, DataRow OneRow);

        /// <summary>
        /// Checks for DataTable existence in a DataSet
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns>tablename if table exists, null otherwise</returns>
        string VerifyTableExistence(string TableName);

        /// <summary>
        /// Checks for column existence in a table
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns>columnname if column exists, null otherwise</returns>
        string VerifyColumnExistence(string FieldName, string TableName);

        /// <summary>
        /// Reads a filtered table from DB
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sort_by"></param>
        /// <param name="filter"></param>
        /// <param name="clear"></param>
        /// <param name="TOP"></param>
        void DO_GET_TABLE(DataTable T, string sort_by, string filter, bool clear, string TOP);

        /// <summary>
        /// Gets a DataTable with an optional set of Select 
        /// </summary>
        /// <param name="T">DataTable to Get from DataBase</param>
        /// <param name="sort_by">parameter to pass to "order by" clause</param>
        /// <param name="filter"></param>
        /// <param name="clear">if true table is cleared before reading</param>
        /// <param name="TOP">parameter for "top" clause of select</param>
        /// <param name="selList"></param>
        SelectBuilder DO_GET_TABLE(DataTable T, string sort_by, string filter, bool clear, string TOP, List<SelectBuilder> selList);

        /// <summary>
        /// Gets a table from DB using sort,filter and clear parameter
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="sort_by">list of sort-clauses. Es. "code ASC, name DESC</param>
        /// <param name="filter">filter to apply for reading</param>
        /// <param name="clear">true if existent table must be cleared</param>
        void DO_GET_TABLE(string TableName, string sort_by, string filter, bool clear);

        /// <summary>
        /// Reads a table accordingly to DataSet properties:
        /// if it is a cached table, it is read entirely again
        /// if it is a non-cached table, it is considered a lookup table, i.e. a parent 
        ///  table, so parent rows of existent childs are read.
        /// </summary>
        /// <param name="T"></param>
        void RefreshTable(DataTable T);

        /// <summary>
        /// Gets a Table from DB filtering by filter and clearing existent
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        void DO_GET_TABLE(string T, string filter);

       

        /// <summary>
        /// Gets evaluated fields, taking the ExtendedProperties QueryCreator.IsTempColumn
        ///   and considering it in a "childtable.childfield" format
        /// Also calls CalcFieldsDelegate of the table for every rows (when needed)
        /// </summary>
        /// <param name="T"></param>
        void GetTemporaryValues(DataTable T);



        /// <summary>
        /// Gets evaluated fields, taking the ExtendedProperties QueryCreator.IsTempColumn
        ///   and considering it in a "childtable.childfield" format
        /// Also calls CalcFieldsDelegate of the table for every rows (when needed)
        /// </summary>
        /// <param name="T"></param>
        void GetTemporaryValues(DataRow r);



        /// <summary>
        /// Gets calculated fields from related table (Calculated fields are those 
        ///		provided with an expression). 
        /// </summary>
        /// <param name="R"></param>
        void CalcTemporaryValues(DataRow R);

        /// <summary>
        /// Gets a DataTable related with PrimaryTable via a given Relation Name.
        /// Also gets columns implied in the relation of related table.
        /// </summary>
        /// <param name="relname"></param>
        /// <returns>related DataTable</returns>
        DataTable EntityRelatedByRel(string relname);

        /// <summary>
        /// Gets a DataTable related with PrimaryTable via a given Relation Name.
        /// Also gets columns implied in the relation of related table 
        /// </summary>
        /// <param name="relname"></param>
        /// <param name="Cs">Columns of related table, implied in the relation</param>
        /// <returns>Related table</returns>
        DataTable EntityRelatedByRel(string relname, out DataColumn[] Cs);

        /// <summary>
        /// Get all child rows in allowed child tables
        /// </summary>
        /// <param name="RR"></param>
        /// <param name="Allowed"></param>
        /// <param name="SelList"></param>
        void GetAllChildRows(DataRow[] RR, Hashtable Allowed, List<SelectBuilder> SelList);


        /// <summary>
        /// Get parent rows of a given Row, in a set of specified  tables.
        /// </summary>
        /// <param name="R">DataRow whose parents are wanted</param>
        /// <param name="Allowed">Tables in which to search parent rows</param>
        /// <param name="selList"></param>
        /// <returns>true if any of parent DataRows was already in memory. This is not 
        ///  granted if rows are taken from a view</returns>
        bool GetParentRows(DataRow R, Hashtable Allowed, List<SelectBuilder> selList);
    }

    /// <summary>
	/// GetData is a class to automatically get all data related to a set of rows
	///  in a primary table, given a DataSet that describes all relations between data
	///  to get. 
	/// When getting data, temporary tables are skipped, and temporary field are
	///   calculated when possible.
	/// GetData is part of the Model Layer
	/// </summary>
	public class GetData : IGetData {
        bool destroyed;
        /// <summary>
        /// Dispose all resource
        /// </summary>
        public void Destroy() {
            if (destroyed) {
                return;
            }
            destroyed = true;
            this.DS = null;
            this.PrimaryDataTable = null;
            initCacheParentView();
            if (preScannedTablesRows != null) {
                preScannedTablesRows.Clear();
                preScannedTablesRows = null;
            }
            if (cachedChildSourceColumn != null) {
                cachedChildSourceColumn.Clear();
                cachedChildSourceColumn = null;
            }
            if (this.VisitedFully != null) {
                this.VisitedFully.Clear();
                VisitedFully = null;
            }            
            this.QHS = null;
        }
        /// <summary>
        /// DataSet on which the instance works
        /// </summary>
        private DataSet DS { get; set; }

        /// <summary>
        /// Primary Table Name
        /// </summary>
        public string PrimaryTable { get; private set; }

        /// <summary>
        /// Primary Table of the DataSet. Primary Table is the first table scanned 
        ///  when data is read from db.
        /// </summary>
        public DataTable PrimaryDataTable { get; private set; }


        /// <summary>
        /// Connection to DataBase
        /// </summary>
		public IDataAccess Conn;

        /// <summary>
        /// Factory used by the instance
        /// </summary>
        public IMetaFactory factory { get; set; } = MetaFactory.factory;

	    /// <summary>
	    /// 
	    /// </summary>
	    public ISecurity security {
	        get { return _security ?? Conn as ISecurity; }
            set { _security = value; }
	    }


		//bool isLocalToDB;


        QueryHelper QHS;


		/// <summary>
		/// A collection of tables that have been read with a null filter. These are not read 
		///  again.
		/// </summary>
		Hashtable VisitedFully;

        /// <summary>
        /// MetaModel used in this class
        /// </summary>
        private IMetaModel model;

		/// <summary>
		/// Public constructor
		/// </summary>
		public GetData() {
		    model = factory.getSingleton<IMetaModel>();
		}

        private static IMetaModel staticModel = MetaFactory.factory.getSingleton<IMetaModel>();

		
		#region Chached Table Ext.Property Management
		/// <summary>
		/// Set Table T to be read once for all when ReadCached will be called next time
		/// </summary>
		/// <param name="T"></param>
		/// <param name="filter"></param>
		/// <param name="sort"></param>
		/// <param name="AddBlankRow">when true, a blank row is added as first row of T</param>
		public static void CacheTable(DataTable T,string filter, string sort, bool AddBlankRow){
			//T.ExtendedProperties["cached"]="0";
            staticModel.cacheTable(T);
			if (AddBlankRow) staticModel.markToAddBlankRow(T);
			if (sort!=null) T.ExtendedProperties["sort_by"] = sort;
            if (filter != null) {
                T.setStaticFilter(filter);
                //SetStaticFilter(T,filter);
            }
		}

		/// <summary>
		/// Tells GetData to read T once for all
		/// </summary>
		/// <param name="T"></param>
		public static void CacheTable(DataTable T) {
            staticModel.cacheTable(T);
            //T.ExtendedProperties["cached"]="0";
        }

		/// <summary>
		/// Undo any CacheTable / DenyClear on T
		/// </summary>
		/// <param name="T"></param>
		public static void UnCacheTable(DataTable T) {
            staticModel.uncacheTable(T);
            //T.ExtendedProperties["cached"]=null;
        }

		/// <summary>
		/// Set a table as "cached", i.e. it will be read only one time 
		///		in the life of the form.
		/// </summary>
		/// <param name="tablename"></param>
		public void CacheTable(string tablename){
            staticModel.cacheTable( DS.Tables[tablename]);
		}
		
		/// <summary>
		/// If a table is cached, is marked to be read again in next
		///  ReadCached. If the table is not cached, has no effect
		/// </summary>
		/// <param name="T">Table to cache again</param>
		public static void ReCache(DataTable T){
			if (!staticModel.isCached(T)) return;
			CacheTable(T);
		}


		

		/// <summary>
		/// Set a table as "read". Has no effect if table isn't a child table
		/// </summary>
		/// <param name="T"></param>
		void tableHasBeenRead(DataTable T) {
            staticModel.tableHasBeenRead(T);
			//if (T.ExtendedProperties["cached"]==null) return;
			//if (T.ExtendedProperties["cached"].ToString()=="0")model.lockRead(T);
		}

		


		/// <summary>
		/// Read all tables marked as "ToCache" with CacheTable() that haven't yet been read
		/// </summary>
		public void ReadCached(){
            int handle = StartTimer("ReadCached()");
            List<SelectBuilder> selList = new List<SelectBuilder>();
			foreach (DataTable T in DS.Tables){
				if (!model.isCached(T))continue;
				if (!model.canRead(T)) continue;
				DO_GET_TABLE(T,null,null,true,null,selList);
				tableHasBeenRead(T);
			}
            if (selList.Count > 0) {
                Conn.MULTI_RUN_SELECT(selList);
                foreach (SelectBuilder Sel in selList) {
                    GetTemporaryValues(Sel.DestTable);
                }
            }
            StopTimer(handle);
		}
		#endregion


		#region Calculated Fields Management
        
		/// <summary>
		/// Delegates for custom field-calculations
		/// </summary>
		public delegate void CalcFieldsDelegate(DataRow R, string list_type);

		/// <summary>
		/// Tells MetaData Engine to call CalculateFields(R,ListingType) whenever:
		///  - a row is loaded from DataBase
		///  - a row is changed in a sub-entity form and modification accepted with mainsave
		/// </summary>
		/// <param name="T">DataTable to be custom calculated</param>
		/// <param name="ListingType">Listing type to use for delegate calling</param>
		/// <param name="Calc">Delegate function to call</param>
		public static void ComputeRowsAs(DataTable T, string ListingType, CalcFieldsDelegate Calc ){
			T.ExtendedProperties["CalculatedListing"] = ListingType;
			T.ExtendedProperties["CalculatedFunction"] = Calc;
		}


		/// <summary>
		/// Evaluates custom fields for every row of a Table. Calls the delegate linked to the table,
		///  corresponding to the MetaData.CalculateFields() virtual method (if it has been defined).
		/// </summary>
		/// <param name="T"></param>
		/// <remarks>No action is taken on deleted rows. Unchanged rows remain unchanged anyway</remarks>
		public static void CalculateTable(DataTable T){            
			if (T==null) return;
			//MarkEvent("Calculate Start on "+T.TableName);
			if (T.ExtendedProperties["CalculatedListing"]==null) return;
			string ListType = T.ExtendedProperties["CalculatedListing"].ToString();
			if (T.ExtendedProperties["CalculatedFunction"]==null) return;
			int handle = mdl_utils.metaprofiler.StartTimer("CalculateTable * " + T.TableName);
			CalcFieldsDelegate Calc = (CalcFieldsDelegate) T.ExtendedProperties["CalculatedFunction"];
			staticModel.invokeActions(T, TableAction.beginLoad);
			if (T.HasChanges()) {
				foreach (DataRow R in T.Rows) {
					if (R.RowState == DataRowState.Deleted) continue;
					bool toMark = (R.RowState == DataRowState.Unchanged);
					Calc(R, ListType);
					if (toMark) R.AcceptChanges();
				}
			}
			else {
				foreach (DataRow R in T.Rows) {
					if (R.RowState == DataRowState.Deleted) continue;
					Calc(R, ListType);
				}
				T.AcceptChanges();

			}
			
			staticModel.invokeActions(T, TableAction.endLoad);
			mdl_utils.metaprofiler.StopTimer(handle);
			//MarkEvent("Calculate Stop");
		}

		/// <summary>
		/// Evaluates custom fields for a single row. Calls the delegate linked to the table,
		///  corresponding to the MetaData.CalculateFields() virtual method (if it has been defined).
		/// </summary>
		/// <param name="R"></param>
		/// <remarks>No action is taken on deleted rows. Unchanged rows remain unchanged anyway</remarks>
		public static void CalculateRow(DataRow R){            
			if (R==null) return;
			if (R.RowState== DataRowState.Deleted) return;
			DataTable T=R.Table;
			if (T.ExtendedProperties["CalculatedListing"]==null) return;
			string ListType = T.ExtendedProperties["CalculatedListing"].ToString();
			CalcFieldsDelegate Calc = (CalcFieldsDelegate) T.ExtendedProperties["CalculatedFunction"];
			if (Calc==null)return;
			bool toMark= (R.RowState == DataRowState.Unchanged);
			Calc(R, ListType);
			if (toMark) R.AcceptChanges();
		}
		#endregion


		/// <summary>
		/// Initialize class. Necessary before doing any other operation
		/// </summary>
		/// <param name="DS"></param>
		/// <param name="Conn"></param>
		/// <param name="PrimaryTable"></param>
		/// <returns></returns>
		public string InitClass(DataSet DS, DataAccess Conn, string PrimaryTable) {
			this.DS=DS;
			this.Conn=Conn;
		    _security = Conn.Security;
            //isLocalToDB = DataAccess.IsLocal;//Conn.LocalToDB;
			this.PrimaryTable = PrimaryTable;
			this.PrimaryDataTable = DS.Tables[PrimaryTable];
			idm = DS.getCreateIndexManager();
			VisitedFully  = new Hashtable();
            QHS = Conn.GetQueryHelper();
			return null;
		}

		private IIndexManager idm;
	    private ISecurity _security;
        /// <summary>
        /// Initialize class. Necessary before doing any other operation
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="conn"></param>
        /// <param name="primaryTable"></param>
        /// <returns></returns>
        public string InitClass(DataSet ds, IDataAccess conn,  string primaryTable) {
	        this.DS = ds;
	        this.Conn = conn;
            this._security = conn.Security;
            //isLocalToDB = DataAccess.IsLocal;//Conn.LocalToDB;
	        this.PrimaryTable = primaryTable;
	        this.PrimaryDataTable = ds.Tables[primaryTable];
	        idm = DS.getCreateIndexManager();
	        VisitedFully = new Hashtable();
	        QHS = conn.GetQueryHelper();
	        return null;
	    }

        /// <summary>
        /// Apply a filter on a table during any further read
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        [Obsolete("use extension DataTable.setStaticFilter")]
        public static void SetStaticFilter(DataTable T, string filter){
			T.ExtendedProperties["filter"]=filter;
		}

		/// <summary>
		/// Apply a filter on a table during any further read
		/// </summary>
		/// <param name="tablename"></param>
		/// <param name="filter"></param>
		[Obsolete("use extension DataTable.setStaticFilter")] 
        public void SetStaticFilter(string tablename, string filter){
			DataTable T = DS.Tables[tablename];
			if (T==null) return;
			T.ExtendedProperties["filter"]=filter;
		}

		/// <summary>
		/// Clears all tables except for temporary and cached (including pre-filled combobox).
		/// Also undoes the effect of denyclear on all secondary tables setting tables 
		///  with AllowClear()
		/// </summary>
		public void CLEAR_ENTITY(){   
			int metaclear = StartTimer("CLEAR_ENTITY");
			foreach (DataTable T in DS.Tables){
				if (PostData.IsTemporaryTable(T)) continue;
				if (model.isCached(T))continue;
				if (VisitedFully[T.TableName]==null) model.clear(T); // T.Clear();
                model.allowClear(T);
			}
			StopTimer(metaclear);
		}


        void xCopyChilds(DataSet Dest, DataSet Rif, DataRow RSource) {
            DataTable T = RSource.Table;
            string source_unaliased = DataAccess.GetTableForReading(RSource.Table);
            if (!Dest.Tables.Contains(source_unaliased)) source_unaliased = RSource.Table.TableName;
            copyDataRow(Dest.Tables[source_unaliased], RSource);
            model.denyClear(Dest.Tables[source_unaliased]);

            foreach (DataRelation Rel in T.ChildRelations) {
                if (!Dest.Tables.Contains(Rel.ChildTable.TableName)) continue;
                if (!CheckChildRel(Rel)) continue; //not a subentityrel
                DataTable ChildTable = Rif.Tables[Rel.ChildTable.TableName];
                model.denyClear(Dest.Tables[ChildTable.TableName]);
                foreach (DataRow Child in RSource.iGetChildRows(Rel)) {
                    xCopyChilds(Dest, Rif, Child);
                }
            }
        }
        
        /// <summary>
        /// Check if a relation connects any field that is primarykey for both parent and child
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        public static bool CheckChildRel(DataRelation R) {
            //Autorelation are not childrel
            if (R.ParentTable.TableName == R.ChildTable.TableName) return false;

            bool linkparentkey = false;

            for (int N = 0; N < R.ParentColumns.Length; N++) {
                DataColumn ParCol = R.ParentColumns[N];
                DataColumn ChildCol = R.ChildColumns[N];
                if (QueryCreator.IsPrimaryKey(R.ParentTable, ParCol.ColumnName) &&
                     QueryCreator.IsPrimaryKey(R.ChildTable, ChildCol.ColumnName)) linkparentkey = true;
            }
            return linkparentkey;
        }

        void copyDataRow(DataTable DestTable, System.Data.DataRow ToCopy) {
            System.Data.DataRow Dest = DestTable.NewRow();
            DataRowVersion ToConsider = DataRowVersion.Current;
            if (ToCopy.RowState == DataRowState.Deleted) ToConsider = DataRowVersion.Original;
            if (ToCopy.RowState == DataRowState.Modified) ToConsider = DataRowVersion.Original;
            if (ToCopy.RowState != DataRowState.Added) {
                foreach (DataColumn C in DestTable.Columns) {
                    if (ToCopy.Table.Columns.Contains(C.ColumnName)) {
                        Dest[C.ColumnName] = ToCopy[C.ColumnName, ToConsider];
                    }
                }
                DestTable.Rows.Add(Dest);
                Dest.AcceptChanges();
            }
            if (ToCopy.RowState == DataRowState.Deleted) {
                Dest.Delete();
                return;
            }
            foreach (DataColumn C in DestTable.Columns) {
                if (ToCopy.Table.Columns.Contains(C.ColumnName)) {
                    if (C.ReadOnly) continue;
                    Dest[C.ColumnName] = ToCopy[C.ColumnName];
                }
            }
            if ((ToCopy.RowState == DataRowState.Modified || ToCopy.RowState == DataRowState.Unchanged)) {
                GetData.CalculateRow(Dest);
                if (PostData.CheckForFalseUpdate(Dest)) Dest.AcceptChanges(); 
                return;
            }


            DestTable.Rows.Add(Dest);
            GetData.CalculateRow(Dest);

        }

		/// <summary>
		/// Fill the primary table starting with a row equal to Start. Start Row should not
		///  belong to PrimaryTable. Infact PrimaryTable is cleared before getting values from Start fields 
		/// </summary>
		/// <param name="Start"></param>
		public void START_FROM(DataRow Start){
			ReadCached();
            xCopyChilds(DS, Start.Table.DataSet, Start);
            DataRow R = PrimaryDataTable.Rows[0];

			//QueryCreator.MyClear(PrimaryDataTable); // PrimaryDataTable.Clear();
			//DataRow R = PrimaryDataTable.NewRow();
			////            for(int i=0;i< PrimaryDataTable.Columns.Count;i++){
			////                R[i] = Start[i];
			////            }
			//foreach (DataColumn C in PrimaryDataTable.Columns){
			//    if (QueryCreator.IsTemporary(C)) continue;
			//    R[C.ColumnName]= Start[C.ColumnName];
			//}

			//PrimaryDataTable.Rows.Add(R);			
			////if Start is an added row, it will be incomplete.
			//// In this case, there is no original value
			//if (Start.RowState!= DataRowState.Added) 
			// R.AcceptChanges();	
			PrimaryDataTable._setLastSelected(R);
			
		}

		/// <summary>
		/// Fill the primary table with a row searched from database. R is not required to belong to
		///  PrimaryTable, but should have the same primary key columns.
		/// </summary>
		/// <param name="R"></param>
		public void SEARCH_BY_KEY(DataRow R){
            int handle = StartTimer("SEARCH_BY_KEY");
			ReadCached();
			//It's necessary to take the filter BEFORE clearing PrimaryTable, cause
			//  R could belong to PrimaryTable!
			string filter = 
				QueryCreator.WHERE_REL_CLAUSE(R, 
				PrimaryDataTable.PrimaryKey,                
				PrimaryDataTable.PrimaryKey, 
				DataRowVersion.Default,
				true);                
			GetRowsByFilter(filter,null, PrimaryTable, null,true,null);
            StopTimer(handle);
		}

		/// <summary>
		/// Gets a primary table DataRow from db, given its primary key
		/// </summary>
		/// <param name="Dest">Table into which putting the row read</param>
		/// <param name="Key">DataRow with the same key as wanted row</param>
		/// <returns>null if row was not found</returns>
		public DataRow GetByKey(DataTable Dest, DataRow Key) {	
			DataRow Res = GetFromViewByKey(Dest,Key);
			if (Res==null){
				string filter = 
					QueryCreator.WHERE_REL_CLAUSE(Key,Dest.PrimaryKey,                
					Dest.PrimaryKey, 
					DataRowVersion.Default,
					true);                
				GetRowsByFilter(filter,null, Dest.TableName, null,true,null);//reads from db
				DataRow[] found = Dest._Filter(q.mCmp(Key, Dest.PrimaryKey));
								//Dest.Select(filter); 
				if (found.Length==0) return null;
				Res= found[0];
			}
			GetTemporaryValues(Dest);
			return Res;
		}

		/// <summary>
		/// Try to get a row from an in-memory view if there is one. This function
		///  is obsolete cause now it's possible to write view table as if they
		///  were real table.
		/// </summary>
		/// <param name="Dest"></param>
		/// <param name="Key"></param>
		/// <returns></returns>
		public DataRow GetFromViewByKey(DataTable Dest, DataRow Key){
			DataTable ViewTable = (DataTable) Dest.ExtendedProperties["ViewTable"];
			if (ViewTable==null) return null;
			DataTable TargetTable = (DataTable) ViewTable.ExtendedProperties["RealTable"];
			if (TargetTable!=Dest) return null;

			var dict = new Dictionary<string, object>();
			
			//key columns of TargetTable in ViewTable
			DataColumn [] Vkey = new DataColumn[TargetTable.PrimaryKey.Length];
			//key columns of Dest Table
			DataColumn [] Ckey = TargetTable.PrimaryKey;
			for (int i=0; i<TargetTable.PrimaryKey.Length; i++){
				bool found=false;
				string colname= TargetTable.TableName+"."+Ckey[i].ColumnName;
				//search the column in view corresponding to Rel.ParentCol[i]
				foreach (DataColumn CV in ViewTable.Columns){
					if (CV.ExtendedProperties["ViewSource"].ToString()==colname){
						found=true;
						Vkey[i]= CV;
						dict[CV.ColumnName] = Key[Ckey[i].ColumnName];
						break;
					}
				}
				if (!found) return null; //key columns were not found
			}

			//string viewtablefilterNOSQL = QueryCreator.WHERE_REL_CLAUSE(Key, Ckey, Vkey, DataRowVersion.Default,false);
			//DataRow [] ViewTableRows = ViewTable.Select(viewtablefilterNOSQL);
			DataRow[] ViewTableRows = ViewTable._Filter(q.mCmp(dict));
			if (ViewTableRows.Length==0) {
				string viewtablefilter = QueryCreator.WHERE_REL_CLAUSE(Key, Ckey, Vkey, DataRowVersion.Default,true);
				MultiCompare MC = QueryCreator.GET_MULTICOMPARE(Key, Ckey, Vkey, DataRowVersion.Default, true);
				GetRowsByFilter(viewtablefilter, MC, ViewTable.TableName, null, true, null);
				ViewTableRows = ViewTable._Filter(q.mCmp(dict));
				if (ViewTableRows.Length==0) return null;
			}

			DataRow RR = ViewTableRows[0];

			//copy row from view to dest
			DataRow NewR = TargetTable.NewRow();
			foreach (DataColumn CC in TargetTable.Columns){
				string colname= TargetTable.TableName+"."+CC.ColumnName;
				foreach (DataColumn CV in ViewTable.Columns){
					if (CV.ExtendedProperties["ViewSource"]==null)continue;
					if (CV.ExtendedProperties["ViewSource"].ToString()==colname){
						NewR[CC]= RR[CV];
						break;
					}
				}				
			}
			TargetTable.Rows.Add(NewR);
			NewR.AcceptChanges();
			return NewR;
		}


		/// <summary>
		/// Merge a filter (Filter1) with the static filter of a DataTable and
		///  gives the resulting (AND) filter
		/// </summary>
		/// <param name="Filter1"></param>
		/// <param name="T"></param>
		/// <returns></returns>
		public static string MergeFilters(string Filter1, DataTable T){
            if (T == null) return Filter1;
			string Filter2=null;
			if (T.ExtendedProperties["filter"]!=null) {
				Filter2 = T.ExtendedProperties["filter"].ToString();
			}
			return MergeFilters(Filter1, Filter2);
		}

        /// <summary>
		/// Merges two filters with an operator without throwing exception if some or 
		///		both are null
		/// </summary>
		/// <param name="Filter1"></param>
		/// <param name="Filter2"></param>
		/// <param name="op">operator to apply</param>
		/// <returns></returns>
		public static string MergeWithOperator(string Filter1, string Filter2,string op){
			if ((Filter1=="")||(Filter1==null))return Filter2;
			if ((Filter2=="")||(Filter2==null)) return Filter1;
			return QueryCreator.putInPar(Filter1)+op+QueryCreator.putInPar(Filter2);

		}

        /// <summary>
		/// Merges two filters (AND) without throwing exception if some or 
		///		both are null
		/// </summary>
		/// <param name="filter"></param>
		/// <param name="op"></param>
		/// <returns></returns>
		public static string MergeWithOperator(string []filter, string op){
            if (filter==null || filter.Length==0)			return "";
            if (filter.Length==1)return filter[0];
			string res="";
            foreach(string operando in filter) {
                if ((operando == null) || (operando == "")) continue;
                if ((operando.Trim()=="")) continue;
                if (res!="")res+=op;
                res+=QueryCreator.putInPar(operando);
            }
            return "("+res+")";

		}

		/// <summary>
		/// Merges two filters (AND) without throwing exception if some or 
		///		both are null
		/// </summary>
		/// <param name="Filter1"></param>
		/// <param name="Filter2"></param>
		/// <returns></returns>
		public static string MergeFilters(string Filter1, string Filter2){
			if ((Filter1=="")||(Filter1==null))return Filter2;
			if ((Filter2=="")||(Filter2==null)) return Filter1;
			return QueryCreator.putInPar(Filter1)+"AND"+QueryCreator.putInPar(Filter2);

		}
        /// <summary>
        /// Merge two condition A and B as  (A) OR (B)
        /// </summary>
        /// <param name="Filter1"></param>
        /// <param name="Filter2"></param>
        /// <returns></returns>
        public static string AppendOR(string Filter1, string Filter2) {
            if ((Filter1 == "") || (Filter1 == null)) return Filter2;
            if ((Filter2 == "") || (Filter2 == null)) return Filter1;
            return QueryCreator.putInPar(Filter1)+"OR"+QueryCreator.putInPar(Filter2);
        }
		/// <summary>
		/// Clears &amp; Fill the primary table with all records from a database table
		/// </summary>
		/// <param name="filter"></param>
		public void GET_PRIMARY_TABLE(string filter){
			ReadCached();
			//inutile poiché GetRowsByFilter efettua merge dei filtri
			//string Filter = MergeFilters(filter, PrimaryDataTable);
			model.clear(PrimaryDataTable); // PrimaryDataTable.Clear();
			GetRowsByFilter(filter,null, PrimaryTable, null,true,null);
		}

        public void recursivelyMarkSubEntityAsVisited(DataTable mainTable, Hashtable Visited, Hashtable ToVisit) {
            var model = MetaFactory.factory.getSingleton<IMetaModel>();
            foreach (DataRelation Rel in mainTable.ChildRelations){
                string childtable = Rel.ChildTable.TableName;
                if ((!staticModel.isSubEntityRelation(Rel) &&  model.canClear(Rel.ChildTable)) 
                    || Visited.ContainsKey(childtable)) continue; //if continue--> it will be cleared
                //Those tables will not be cleared
                Visited[childtable] = Rel.ChildTable;			
                ToVisit[childtable] = Rel.ChildTable;
                recursivelyMarkSubEntityAsVisited(Rel.ChildTable, Visited, ToVisit);
            } 
          
        }

		/// <summary>
		/// Gets all data of the DataSet cascated-related to the primary table.
		/// The first relations considered are child of primary, then
		///  proper child / parent relations are called in cascade style.
		/// </summary>
		///  <param name="onlyperipherals">if true, only peripheral (not primary or secondary) tables are refilled</param>
		///  <param name="OneRow">The (eventually) only primary table row on which
		///   get the entire sub-graph. Can be null if PrimaryDataTable 
		///   already contains rows.  R is not required to belong to PrimaryDataTable.</param>
		/// <returns>always true</returns>
		public bool DO_GET(bool onlyperipherals, DataRow OneRow){
			int dogethandle = StartTimer("Inside DO_GET()");

            initCacheParentView();
            
            //Tables whose child and tables rows have to be retrieved
            Hashtable ToVisit= new Hashtable();
			//Tables from which rows have NOT to be retrieved
			Hashtable Visited = new Hashtable();

			//Set Fully-Visited and Cached tables as Visited
			foreach (DataTable T in DS.Tables){
				if ((model.isCached(T))||(VisitedFully[T.TableName]!=null)|| PostData.IsTemporaryTable(T)) {
					Visited[T.TableName] = T;
					//ToVisit[T.TableName] = T;
				}
			}
            string[] toPreScan = (from DataTable T in DS.Tables where !PostData.IsTemporaryTable(T) select T.TableName).ToArray();
            Conn.preScanStructures(toPreScan);
			ToVisit[PrimaryTable]= PrimaryDataTable;
			Visited[PrimaryTable]= PrimaryDataTable;
            
			if (onlyperipherals){
				//Marks child tables as ToVisit+Visited
			    recursivelyMarkSubEntityAsVisited(PrimaryDataTable, Visited, ToVisit);

				//foreach (DataRelation Rel in PrimaryDataTable.ChildRelations){
				//	string childtable = Rel.ChildTable.TableName;
				//	if ((!QueryCreator.IsSubEntity(Rel, Rel.ChildTable, PrimaryDataTable)) && 
				//		CanClear(Rel.ChildTable)) continue; //if continue--> it will be cleared
				//	//Those tables will not be cleared
				//	Visited[childtable] = Rel.ChildTable;			
				//	ToVisit[childtable] = Rel.ChildTable;
				//} 


				foreach (DataTable T in DS.Tables){
					string childtable = T.TableName;
					if (!model.canClear(T)){  //Skips DenyClear Tables
						Visited[childtable] = T;
						ToVisit[childtable] = T;
					}
				}
			}
			
			//Clears all other tables
			foreach (DataTable T in DS.Tables){
				if (Visited[T.TableName]!=null) continue;
				if (PostData.IsTemporaryTable(T)) continue;
                if(T.ExtendedProperties["RealTable"] is DataTable Main) {
                    if(Visited[Main.TableName] != null)
                        continue; //tratta le viste come le relative main
                }
				model.clear(T); // T.Clear();
				T.AcceptChanges();
			}

			//Set as Visited all child tables linked by autoincrement fields
			if ((OneRow!=null) && (OneRow.RowState==DataRowState.Added)){
				foreach(DataRelation Rel in PrimaryDataTable.ChildRelations){
					string childtable = Rel.ChildTable.TableName;
					bool toskip=false;
					foreach(var C in Rel.ParentColumns){
						if (RowChange.IsAutoIncrement(C)){
							toskip=true;
							break;
						}
					}
					if (toskip) Visited[childtable]=Rel.ChildTable;					
				}
			}


			bool waspersisting= Conn.persisting;
			Conn.persisting=true;
			Conn.Open();

            int h1 = StartTimer("ScanTables");
			ScanTables(ToVisit, Visited, OneRow);
            StopTimer(h1);

			if (onlyperipherals){
				//Freshes calculated fields of entity tables and Dont't-clear-tables
				foreach (DataRelation Rel in PrimaryDataTable.ChildRelations){
					string childtable = Rel.ChildTable.TableName;
                    if ((!staticModel.isSubEntityRelation(Rel)) && model.canClear(Rel.ChildTable)) continue;
                    GetTemporaryValues(Rel.ChildTable);				//
                }

				if (OneRow != null) {
					GetTemporaryValues(OneRow);
				}
				else {
					GetTemporaryValues(PrimaryDataTable);
				}
				
			}
			Conn.Close();
			Conn.persisting= waspersisting;
			StopTimer(dogethandle);
			return true;  
		}


		/// <summary>
		/// Checks for DataTable existence in a DataSet
		/// </summary>
		/// <param name="TableName"></param>
		/// <returns>tablename if table exists, null otherwise</returns>
		public string VerifyTableExistence(string TableName){
			if (DS.Tables[TableName]==null) return null;
			return TableName;
		}

 
		/// <summary>
		/// Checks for column existence in a table
		/// </summary>
		/// <param name="FieldName"></param>
		/// <param name="TableName"></param>
		/// <returns>columnname if column exists, null otherwise</returns>
		public string VerifyColumnExistence(string FieldName, string TableName){
			if (DS.Tables[TableName]==null) return null;
			if (DS.Tables[TableName].Columns[FieldName]==null) return null;
			return FieldName;
		}

	

        /// <summary>
        /// Reads a filtered table from DB
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sort_by"></param>
        /// <param name="filter"></param>
        /// <param name="clear"></param>
        /// <param name="TOP"></param>
        public void DO_GET_TABLE(DataTable T, string sort_by, string filter, bool clear, string TOP) {
            DO_GET_TABLE(T, sort_by, filter, clear, TOP,null);
        }

		/// <summary>
		/// Gets a DataTable with an optional set of Select 
		/// </summary>
		/// <param name="T">DataTable to Get from DataBase</param>
		/// <param name="sortBy">parameter to pass to "order by" clause</param>
		/// <param name="filter"></param>
		/// <param name="clear">if true table is cleared before reading</param>
		/// <param name="top">parameter for "top" clause of select</param>
        /// <param name="selList"></param>
        public SelectBuilder DO_GET_TABLE(DataTable T, string sortBy, string filter, bool clear, string top, List<SelectBuilder> selList) {

			if (!model.canRead(T))return null;
			var table  = T.TableName;
			if (clear) {
				model.clear(T); // T.Clear();
			}
			if (T.Rows.Count==0) Add_Blank_Row(T);
			var mergedfilter = MergeFilters(filter,T); 
			sortBy = sortBy ?? T.getSorting();

            SelectBuilder mySel = null;

            if (selList == null) {
                Conn.RUN_SELECT_INTO_TABLE(T, sortBy, mergedfilter, top, true);
            }
            else {
                mySel = new SelectBuilder().Where(mergedfilter).Top(top).OrderBy(sortBy).IntoTable(T);
                selList.Add(mySel);
            }

            if (mergedfilter == null) VisitedFully[table] = T;
			tableHasBeenRead(T);
			if (selList==null) GetTemporaryValues(T);
            return mySel;
		}


       



		/// <summary>
		/// Gets the sorting property for a table
		/// </summary>
		/// <param name="T"></param>
		/// <param name="sort"></param>
		/// <returns></returns>
		public static string GetSorting(DataTable T, string sort) {
		    return sort ?? T?.ExtendedProperties["sort_by"]?.ToString();
		}

		/// <summary>
		/// Set sorting property of a DataTable
		/// </summary>
		/// <param name="T"></param>
		/// <param name="sort"></param>
		public static void SetSorting(DataTable T, string sort){			
			T.ExtendedProperties["sort_by"]=sort;
		}
		/// <summary>
		/// Adds an empty (all fields blank) row to a table if the table has been marked
		///  with MarkToAddBlankRow
		/// </summary>
		/// <param name="T"></param>
		public static void Add_Blank_Row(DataTable T) {
			if (T.ExtendedProperties["AddBlankRow"]==null) return;
			int handle = mdl_utils.metaprofiler.StartTimer("Add_Blank_Row * " + T.TableName);
			DataRow BlankRow = T.NewRow();
			QueryCreator.ClearRow(BlankRow);
			staticModel.invokeActions(T, TableAction.beginLoad);
			T.Rows.Add(BlankRow);
			staticModel.invokeActions(T, TableAction.endLoad);
			BlankRow.AcceptChanges();
			mdl_utils.metaprofiler.StopTimer(handle);
		}

		/// <summary>
		/// Gets a table from DB using sort,filter and clear parameter
		/// </summary>
		/// <param name="TableName"></param>
		/// <param name="sort_by">list of sort-clauses. Es. "code ASC, name DESC</param>
		/// <param name="filter">filter to apply for reading</param>
		/// <param name="clear">true if existent table must be cleared</param>
		public void DO_GET_TABLE(string TableName, string sort_by, string filter, bool clear){
			DO_GET_TABLE(DS.Tables[TableName],sort_by,filter,clear,null,null);
		}



		/// <summary>
		/// Reads a table accordingly to DataSet properties:
		/// if it is a cached table, it is read entirely again
		/// if it is a non-cached table, it is considered a lookup table, i.e. a parent 
		///  table, so parent rows of existent childs are read.
		/// </summary>
		/// <param name="T"></param>
		public void RefreshTable(DataTable T){
			//string filter="";
			//esamina tutte le tabelle figlie di T

		}

		/// <summary>
		/// Gets a Table from DB filtering by filter and clearing existent
		/// </summary>
		/// <param name="T"></param>
		/// <param name="filter"></param>
		public void DO_GET_TABLE(string T, string filter){
			DO_GET_TABLE(DS.Tables[T],null,filter,true,null,null);
		}

	
	
		
		
		

		

        
	
		
		
		
		/// <summary>
		/// Checks that a Column Property is in the format parenttable.parentcolumn
		/// </summary>
		/// <param name="Tag">Extended Property of a DataColumn</param>
		/// <param name="table">table part if successfull</param>
		/// <param name="column">column part if successfull</param>
		/// <returns>true if the property is in a correct format</returns>
		bool CheckColumnProperty(string Tag, out string table, out string column){
			table=null;
			column=null;
			if (Tag==null) return false;
			if (Tag=="")return false;
			Tag = Tag.Trim();
			int pos=Tag.IndexOf('.');
			if (pos==-1)return false;
			table = VerifyTableExistence((Tag.Split(
				new char[] {'.'}, 2)[0]).Trim());
			if (table==null) return false;
			//column = VerifyColumnExistence(Tag.Substring(pos+1), table);
			column=Tag.Substring(pos+1);
			if (column.ToString()=="") column=null;
			if (column==null) return false;
			return true;
		}


        /// <summary>
        /// Gets some row from a datatable, with all child rows in the same table
        /// </summary>
        /// <remarks>TODO: This method will be removed</remarks>
        /// <param name="T">DataTable to Get from DataBase</param>
        /// <param name="filter">Filter to apply in order to retrieve roots</param>
        /// <param name="clear">true if table has to be cleared</param>
        [Obsolete("use treeViewDataAccess.DO_GET_TABLE_ROOTS when possible")]
        public void DO_GET_TABLE_ROOTS(DataTable T, string filter, bool clear) {
            //ReadCached();           //HO RIMOSSO questa riga nella fase di refactoring 
            if (!staticModel.canRead(T)) return;
            DO_GET_TABLE(T, null, filter, clear, null, null);

            //TableHasBeenRead(T); //HO RIMOSSO questa riga nella fase di refactoring 
        }

        /// <summary>
        /// Gets child of a selected set of rows, and gets related tables
        /// </summary>
        /// <remarks>TODO: This method will be removed </remarks>
        /// <param name="ToExpand"></param>
        public void expandChilds(DataRow[] ToExpand) {
            if (ToExpand.Length == 0) return;
            var T = ToExpand[0].Table;
            var toVisit = new Hashtable { [T.TableName] = T };
            GetAllChildRows(ToExpand, toVisit, null);

        }


        /// <summary>
        /// Gets all necessary rows from table in order to rebuild R genealogy
        /// </summary>
        /// <param name="R"></param>
        /// <param name="AddChild">when true, all child of every parent found
        ///  are retrieved
        ///  </param>
        public void DO_GET_PARENTS(DataRow R, bool AddChild) {
            int handle = StartTimer("DO_GET_PARENTS");
            try {
                DataRow[] Parents = new DataRow[20];
                Parents[0] = R;
                int found = 1;

                var Allowed = new Hashtable();
                var T = R.Table;
                Allowed[T.TableName] = T;

                var AutoParent = GetAutoParentRelation(T);
                if (AutoParent == null) return;
                bool res = false;
                //Get the strict genealogy of R (max 20 levels)
                while (found < 20) {
                    //Gets the parent of Parents[found-1];
                    DataRow Child = Parents[found - 1];
                    res = GetParentRows(Child, Allowed, null);

                    //finds parent of Child
                    DataRow[] foundparents = Child.iGetParentRows(AutoParent);
                    if (foundparents.Length != 1) break;
                    Parents[found] = foundparents[0];
                    found++;
                    if (res) break;
                }
                if (!AddChild) return;
                if (found == 1) return;
                //			if (res) {
                //				found--; //skip last parent, which was already in tree
                //			}
                DataRow[] list = new DataRow[found - 1];
                for (int i = 1; i < found; i++) list[i - 1] = Parents[i];
                ExpandChilds(list); 
            }
            finally {
                StopTimer(handle);
            }
        }

        //TODO: remove
        /// <summary>
        /// Gets a relation that connects a table with its self
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        private static DataRelation GetAutoParentRelation(DataTable T) {
            foreach (DataRelation R in T.ParentRelations) {
                if (R.ParentTable.TableName == T.TableName) return R;
            }
            return null;
        }

        //TODO: remove

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
        [Obsolete]
        public DataRow GetSpecificChild(DataTable T,
            string StartCondition,
            string startval,
            string startfield) {

            string filter = QHS.AppAnd(StartCondition, QHS.Like(startfield, startval));
            DO_GET_TABLE(T, $"len({startfield})", filter, true, "1", null);
            if (T.Rows.Count == 0) return null;
            return T.Rows[0];
        }

        //TODO: remove

        /// <summary>
        /// Gets a relation that connects a table with its self. Should be the same
        ///  as AutoParent
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        [Obsolete]
        public static DataRelation GetAutoChildRelation(DataTable T) {
            foreach (DataRelation R in T.ChildRelations) {
                if ((R.ParentTable.TableName == T.TableName) &&
                    (R.ChildTable.TableName == T.TableName)
                ) return R;
            }
            return null;
        }

        //TODO: remove

        /// <summary>
        /// Gets child of a selected set of rows, and gets related tables
        /// </summary>
        /// <param name="ToExpand"></param>
        private void ExpandChilds(DataRow[] ToExpand) {
            if(ToExpand.Length == 0)
                return;
            var T = ToExpand[0].Table;
            var ToVisit = new Hashtable {
                [T.TableName] = T
            };
            GetAllChildRows(ToExpand, ToVisit, null);
            //			foreach (DataRow R in ToExpand){
            //				GetChildRows(R, ToVisit);				
            //			}

            //Tables from which rows have NOT to be retrieved
            //var Visited = EntityAndFullVisited();
            //Visited[T.TableName] = T;

            //ScanTables(ToVisit, Visited,null);
            //GetTemporaryValues(T);		
        }

        //TODO: remove


        //Hashtable EntityAndFullVisited() {
        //    var Visited = new Hashtable();
        //    //Set Fully-Visited tables as Visited
        //    foreach (DataTable T2 in DS.Tables) {
        //        if (VisitedFully[T2.TableName] != null) {
        //            Visited[T2.TableName] = T2;
        //        }
        //    }
        //    Visited[PrimaryTable] = PrimaryDataTable;
        //    //Marks child tables as Visited
        //    foreach (DataRelation Rel in PrimaryDataTable.ChildRelations) {
        //        string childtable = Rel.ChildTable.TableName;
        //        if (!QueryCreator.IsSubEntity(Rel, Rel.ChildTable, PrimaryDataTable)) continue;
        //        Visited[childtable] = Rel.ChildTable;
        //    }
        //    return Visited;
        //}


        /// <summary>
        /// Gets all necessary rows from table in order to rebuild R genealogy
        /// </summary>
        /// <remarks>TODO: This method will be removed  </remarks>
        /// <param name="r"></param>
        /// <param name="addChild">when true, all child of every parent found are retrieved </param>
        /// <param name="autoParentRelation"></param>
        [Obsolete]
        public void DO_GET_PARENTS(DataRow r, bool addChild, DataRelation autoParentRelation) {
            int handle = StartTimer("DO_GET_PARENTS");
            try {
                var Parents = new DataRow[20];
                Parents[0] = r;
                int found = 1;

                var allowed = new Hashtable();
                var T = r.Table;
                allowed[T.TableName] = T;

                DataRelation autoParent = autoParentRelation;//GetAutoParentRelation(T);
                if (autoParent == null) return;
                //Get the strict genealogy of R (max 20 levels)
                while (found < 20) {
                    //Gets the parent of Parents[found-1];
                    DataRow child = Parents[found - 1];
                    var res = GetParentRows(child, allowed, null);

                    //finds parent of Child
                    DataRow[] foundparents = child.iGetParentRows(autoParent);
                    if (foundparents.Length != 1) break;
                    Parents[found] = foundparents[0];
                    found++;
                    if (res) break;
                }
                if (!addChild) return;
                if (found == 1) return;
                //			if (res) {
                //				found--; //skip last parent, which was already in tree
                //			}
                DataRow[] list = new DataRow[found - 1];
                for (int i = 1; i < found; i++) list[i - 1] = Parents[i];
                expandChilds(list);
            }
            finally {
                StopTimer(handle);
            }
        }



  //      /// <summary>
  //      /// Gets only directly related (Parents and eventually childs) rows to OneRow
  //      /// </summary>
  //      /// <param name="Visited">Not-to-visit tables</param>
  //      /// <param name="OneRow">Row to scan for related ros</param>
  //      /// <param name="OnlyParents">true if only parent rows have to be taken</param>
  //      void ScanRow2(Hashtable Visited, DataRow OneRow, bool OnlyParents){
		//	var ToVisit = new Hashtable();
		//	ToVisit[OneRow.Table.TableName]= OneRow.Table;
		//	var T = OneRow.Table;
		//	var NextVisit = new Hashtable();			

		//	//searches child tables of T & pre-set them to visited
		//	foreach (DataRelation Rel in T.ChildRelations){
		//		string childtable = Rel.ChildTable.TableName;
		//		if (Visited[childtable]!=null) continue;
		//		if (ToVisit[childtable]!=null) continue;                        
		//		Visited[childtable] = Rel.ChildTable;
		//		NextVisit[childtable] = Rel.ChildTable; 
		//	}

		//	//searches parent tables of T & pre-set them to visited + NextVisit
		//	foreach (DataRelation Rel in T.ParentRelations){
		//		string parenttable = Rel.ParentTable.TableName;
		//		if (Visited[parenttable]!=null) continue;
		//		if (ToVisit[parenttable]!=null) continue;
		//		Visited[parenttable] = Rel.ParentTable;
		//		NextVisit[parenttable] = Rel.ParentTable;
		//	}
                
		//	//Only rows in NextVisit tables will be loaded in this step
		//	GetParentRows(OneRow, NextVisit,null);
		//	if (!OnlyParents) {						
		//		GetChildRows(OneRow, NextVisit,null);
		//	}		
		//	GetTemporaryValues(T);
		//}
		


		/// <summary>
		/// Get all child and parent rows of tables in "ToVisit", assuming that Tables in
		/// "Visited" table's rows have already been retrieved and so must not be 
		/// retrieved again. "Visited" can be considered as a barrier that can't be 
		/// overpassed in the scanning process.
		/// </summary>
		/// <param name="ToVisit">List of tables to scan</param>
		/// <param name="Visited">List of tables that are not to be scanned</param>
		/// <param name="OneRow">when not null, is the only primary table row for 
		///  which are taken child rows.</param>
		void ScanTables(Hashtable ToVisit, Hashtable Visited, DataRow OneRow){
            //EnableCache();
            //DisableCache();

            while (ToVisit.Count > 0) {
	            //tables from which retrieve rows in this step
	            Hashtable NextVisit = new Hashtable();
	            List<SelectBuilder> selList = new List<SelectBuilder>();

	            //Mark tables directly related to "ToVisit" tables as "Visited" +NextVisit, 
	            // so they will not be back-scanned in future iterations
	            foreach (DataTable T in ToVisit.Values) {
		            if (PostData.IsTemporaryTable(T)) continue;
		            //searches child tables of T & pre-set them to visited
		            foreach (DataRelation Rel in T.ChildRelations) {
			            string childtable = Rel.ChildTable.TableName;
			            if (Visited[childtable] != null) continue;
			            if (ToVisit[childtable] != null) continue;
			            Visited[childtable] = Rel.ChildTable;
			            NextVisit[childtable] = Rel.ChildTable;
		            }

		            //searches parent tables of T & pre-set them to visited + NextVisit
		            foreach (DataRelation Rel in T.ParentRelations) {
			            string parenttable = Rel.ParentTable.TableName;
			            if (Visited[parenttable] != null) continue;
			            if (ToVisit[parenttable] != null) continue;
			            Visited[parenttable] = Rel.ParentTable;
			            NextVisit[parenttable] = Rel.ParentTable;
		            }
	            }

	            //Only rows in NextVisit tables will be loaded in this step
	            foreach (DataTable T in ToVisit.Values) {
		            if ((OneRow == null) || (OneRow.Table.TableName != T.TableName)) {
			            foreach (DataRow R in T.Rows) {
				            if (R.RowState == DataRowState.Deleted) continue;
				            if (R.RowState == DataRowState.Detached) continue;
				            GetParentRows(R, NextVisit, selList);
			            }

			            GetAllChildRows(T.Select(), NextVisit, selList);
			            //GetTemporaryValues(T);
			            continue;
		            }

		            //Caso di (OneRow!=null) && (OneRow.Table == T)
		            foreach (DataRow R in T.Rows) {
			            if (R.RowState == DataRowState.Deleted) continue;
			            if (R.RowState == DataRowState.Detached) continue;

			            //If OneRow present, take childs only from OneRow in OneRow.Table
			            //this was below (*)
			            if (OneRow != R) continue;

			            GetParentRows(R, NextVisit, selList);

			            // (*) it was here
			            GetChildRows(R, NextVisit, selList);
		            }

		            //if (selList != null) {
		            //	if (OneRow != null && OneRow.Table == T) {
		            //		GetTemporaryValues(OneRow);
		            //	}
		            //	else {
		            //		GetTemporaryValues(T);
		            //	}
		            //}

	            }

	            if (selList.Count > 0) {
		            Conn.MULTI_RUN_SELECT(selList);
		            //foreach (SelectBuilder S in selList) {
		            // if (OneRow != null && S.DestTable == OneRow.Table) {
		            //  GetTemporaryValues(OneRow);
		            // }
		            // else {
		            //  GetTemporaryValues(S.DestTable);
		            // }
		            //}
	            }

	            foreach (DataTable T in ToVisit.Values) {
		            if (OneRow != null && T == OneRow.Table) {
			            GetTemporaryValues(OneRow);
		            }
		            else {
			            GetTemporaryValues(T);
		            }
	            }


	            OneRow = null;
	            ToVisit = NextVisit;
            }

            //DisableCache();
		}


		/// <summary>
		/// Gets evaluated fields, taking the ExtendedProperties QueryCreator.IsTempColumn
		///   and considering it in a "childtable.childfield" format
		/// Also calls CalcFieldsDelegate of the table for every rows (when needed)
		/// </summary>
		/// <param name="r"></param>
		public void GetTemporaryValues(DataRow r) {
		    if (destroyed) return;
		    DataTable T = r.Table;
		    if (r.RowState == DataRowState.Deleted) return;
		    bool toMark = r.RowState==DataRowState.Unchanged;
		    int handle = StartTimer("GetTemporaryValues DataRow * " + T.TableName);
		    var iManager = T.DataSet?.getIndexManager();
			foreach (DataColumn c in T.Columns){                
                if (!QueryCreator.IsTemporary(c))continue;
                if (c.ExtendedProperties["mdl_foundInGetViewChildTable"] != null) {
	                c.ExtendedProperties["mdl_foundInGetViewChildTable"] = null;
	                continue;
                };
				object tagObj = QueryCreator.GetExpression(c);                    
				if (tagObj==null) {				   
				        var fn  = QueryCreator.GetMetaExpression(c);
                        if (fn==null)continue;
			            r[c] = fn.apply(r,Conn) ?? DBNull.Value;
				    continue;
				}
				var tag = tagObj.ToString().Trim();
                if(!CheckColumnProperty(tag, out var table, out var column))continue;
                if (table==""||column=="") continue;
				if (DS.Tables[table] == null) continue;
				DataTable sourceTable = DS.Tables[table];
				if (!sourceTable.Columns.Contains(column)) continue;
				var sourceCol = sourceTable.Columns[column];
				var parentRel = parentRelation(T, table);
				if (parentRel == null) continue;
				var parentHasher = iManager?.getParentHasher(parentRel);
				if (parentHasher == null || parentHasher.noIndex) {
						var Related = r.GetParentRow(parentRel);	//R.iGetParentRows(Rel);
						if (Related == null) continue;
						r[c] = Related[sourceCol] ; //GetRelatedRow(r, table, column,parentRel) ?? DBNull.Value;
				}
				else {
					Dictionary<string,object> hashed= new Dictionary<string, object>();
						string hash = parentHasher.hash.get(r);
						if (!hashed.TryGetValue(hash, out object o)) {
							var Related = parentHasher.getRow(r);	//R.iGetParentRows(Rel);
							if (Related == null) continue;
							o  = Related[sourceCol] ; //GetRelatedRow(r, table, column,parentRel) ?? DBNull.Value;
							hashed[hash] = o;
						}
						r[c] = o;
				}
				
			}
			if (toMark)T.AcceptChanges();
			CalculateTable(T);
			StopTimer(handle);
		}

    
		/// <summary>
		/// Gets evaluated fields, taking the ExtendedProperties QueryCreator.IsTempColumn
		///   and considering it in a "childtable.childfield" format
		/// Also calls CalcFieldsDelegate of the table for every rows (when needed)
		/// </summary>
		/// <param name="T"></param>
		public void GetTemporaryValues(DataTable T) {
		    if (destroyed) return;
		    bool lateAcceptChanges = !T.HasChanges();
		    int handle = StartTimer("GetTemporaryValues * " + T.TableName);
		    var iManager = T.DataSet?.getIndexManager();
		    staticModel.invokeActions(T,TableAction.beginLoad);
			foreach (DataColumn c in T.Columns){                
                if (!QueryCreator.IsTemporary(c))continue;
                if (c.ExtendedProperties["mdl_foundInGetViewChildTable"] != null) {
	                c.ExtendedProperties["mdl_foundInGetViewChildTable"] = null;
	                continue;
                };
				object tagObj = QueryCreator.GetExpression(c);                    
				if (tagObj==null) {				   
				        var fn  = QueryCreator.GetMetaExpression(c);
                        if (fn==null)continue;
                        int handle3 = StartTimer("GetTemporaryValues GetMetaExpression * " + T.TableName);
				        foreach (DataRow r in T.Rows) {
				            if (r.RowState == DataRowState.Deleted) continue;
				            var toMark = (r.RowState == DataRowState.Unchanged);
				            r[c] = fn.apply(r,Conn) ?? DBNull.Value;
				            if (toMark&& ! lateAcceptChanges) r.AcceptChanges();
				        }	
				        StopTimer(handle3);
				    continue;
				}
				var tag = tagObj.ToString().Trim();
                if(!CheckColumnProperty(tag, out var table, out var column))
                    continue;
                if (table==""||column=="") continue;
				if (DS.Tables[table] == null) continue;
				DataTable sourceTable = DS.Tables[table];
				if (!sourceTable.Columns.Contains(column)) continue;
				var sourceCol = sourceTable.Columns[column];
				var parentRel = parentRelation(T, table);
				if (parentRel == null) continue;
				var parentHasher = iManager?.getParentHasher(parentRel);
				int handle2 = StartTimer("GetTemporaryValues GetRelatedRow * " + T.TableName);
				if (parentHasher == null || parentHasher.noIndex) {
					foreach(DataRow r in T.Rows){
						if (r.RowState== DataRowState.Deleted) continue;
						var toMark= (r.RowState == DataRowState.Unchanged);
						var Related = r.GetParentRow(parentRel);	//R.iGetParentRows(Rel);
						if (Related == null) continue;
						r[c] = Related[sourceCol] ; //GetRelatedRow(r, table, column,parentRel) ?? DBNull.Value;
						if (toMark&& ! lateAcceptChanges) r.AcceptChanges();
					}
				}
				else {
					Dictionary<string,object> hashed= new Dictionary<string, object>();
					foreach(DataRow r in T.Rows){
						if (r.RowState== DataRowState.Deleted) continue;
						var toMark= (r.RowState == DataRowState.Unchanged);
						string hash = parentHasher.hash.get(r);
						if (!hashed.TryGetValue(hash, out object o)) {
							var Related = parentHasher.getRow(r);	//R.iGetParentRows(Rel);
							if (Related == null) continue;
							o  = Related[sourceCol] ; //GetRelatedRow(r, table, column,parentRel) ?? DBNull.Value;
							hashed[hash] = o;
						}

						r[c] = o;
						if (toMark && ! lateAcceptChanges) r.AcceptChanges();
						
					}
				}
				StopTimer(handle2);
				
			}
			if (lateAcceptChanges)T.AcceptChanges();
			staticModel.invokeActions(T,TableAction.endLoad);
			CalculateTable(T);
			StopTimer(handle);
		}

		/// <summary>
		/// Gets calculated fields from related table (Calculated fields are those 
		///		provided with an expression). 
		/// </summary>
		/// <param name="R"></param>
		public void CalcTemporaryValues(DataRow R){
			bool toMark= (R.RowState == DataRowState.Unchanged);
			R.BeginEdit();
			foreach (DataColumn C in R.Table.Columns){         
			    if (!QueryCreator.IsTemporary(C))continue;
                object TagObj = QueryCreator.GetExpression(C); 
				if (TagObj==null) {
				    var fn  = QueryCreator.GetMetaExpression(C);
				    if (fn==null)continue;
        	        R[C] = fn.apply(R,Conn) ?? DBNull.Value;
				    continue;
				}
				string Tag = TagObj.ToString().Trim();
                if(!CheckColumnProperty(Tag, out var Table, out var Column)) continue;
                if (Column=="") continue;
				R[C] = GetRelatedRow(R, Table, Column);
			}
			R.EndEdit();
			if ((toMark) && (R.RowState != DataRowState.Unchanged))	R.AcceptChanges();

		}


	

		DataRelation parentRelation(DataTable T, string parentTableName) {
			DataTable RelatedTable = DS.Tables[parentTableName];
			return (from DataRelation rel in T.ParentRelations where rel.ParentTable.Equals(RelatedTable) select rel).FirstOrDefault();
		}
		

		/// <summary>
		/// Evaluate a field of a row R taking the value from a related row of
		///   a specified Table - Column
		/// </summary>
		/// <param name="R">DataRow to fill</param>
		/// <param name="relatedTableName">Table from which value has to be taken</param>
		/// <param name="relatedColumn">Column from which value has to be taken</param>
		/// <param name="parentRelations"></param>
		/// <returns></returns>
		object GetRelatedRow(DataRow R, string relatedTableName, string relatedColumn, DataRelation parentRelation){
			DataTable T = R.Table;
			DataTable RelatedTable = DS.Tables[relatedTableName];            
			DataRow Related=null;
			var iManager = R.Table.DataSet?.getIndexManager();
			//return iManager?.getParentRows(rChild, rel) ?? rChild.GetParentRows(rel);
			
				if (parentRelation.ParentTable.Equals(RelatedTable)){
					Related = iManager?.getParentRow(R, parentRelation) ?? R.GetParentRow(parentRelation);	//R.iGetParentRows(Rel);
					
				}
			
			return Related?[relatedColumn]??DBNull.Value;
		}


		/// <summary>
		/// Evaluate a field of a row R taking the value from a related row of
		///   a specified Table - Column
		/// </summary>
		/// <param name="R">DataRow to fill</param>
		/// <param name="relatedTableName">Table from which value has to be taken</param>
		/// <param name="relatedColumn">Column from which value has to be taken</param>
		/// <returns></returns>
		object GetRelatedRow(DataRow R, string relatedTableName, string relatedColumn){
			DataTable T = R.Table;
			DataTable RelatedTable = DS.Tables[relatedTableName];            
			DataRow[] Related=null;
			var iManager = R.Table.DataSet?.getIndexManager();
			//return iManager?.getParentRows(rChild, rel) ?? rChild.GetParentRows(rel);

			foreach (DataRelation Rel in T.ChildRelations){
				if (Rel.ChildTable.Equals(RelatedTable)){
					Related = iManager?.getChildRows(R, Rel) ?? R.GetChildRows(Rel);	// R.iGetChildRows(Rel);
					if (Related.Length==0) continue;
					break;
				}
			}
			if (Related==null){
				foreach (DataRelation Rel in T.ParentRelations){
					if (Rel.ParentTable.Equals(RelatedTable)){
						Related = iManager?.getParentRows(R, Rel) ?? R.GetParentRows(Rel);	//R.iGetParentRows(Rel);
						if (Related.Length==0) continue;
						break;
					}
				}
			}


			if (Related==null) {
				//                MetaFactory.factory.getSingleton<IMessageShower>().Show("The field "+ChildColumn+" of table"+ChildTable +
				//                      " could not be looked-up in table "+ T.TableName);
				return DBNull.Value;
			}
			if (Related.Length==0) return DBNull.Value;
			if (RelatedTable.Columns[relatedColumn]==null) {
				//                MetaFactory.factory.getSingleton<IMessageShower>().Show("The field "+ChildColumn+" was not contained in table "+ChildTable);
				return DBNull.Value;
			}
			return Related[0][relatedColumn];
		}

	    void GetViewChildTable(DataRow R, DataRelation Rel) {
	        int chrono = StartTimer("GetViewChildTable * " + Rel.RelationName );
	        iGetViewChildTable(R, Rel);
	        StopTimer(chrono);
	    }

        /// <summary>
        /// Gets a table reading it from a view
        /// Here ViewTable.ExtendedProperties["RealTable"]==Rel.ChildTable
        /// </summary>
        /// <param name="R"></param>
        /// <param name="Rel"></param>
        void iGetViewChildTable(DataRow R, DataRelation Rel){
			DataTable TargetTable = Rel.ChildTable;
			DataTable ParentTable = R.Table; //== ViewTable.ExtProp["RealTable"]
			DataTable ViewTable = (DataTable) TargetTable.ExtendedProperties["ViewTable"];
			if (ViewTable == null) return;

			//search columns in view corresponding to Rel.ChildColumns
			DataColumn [] VCol = new DataColumn[Rel.ChildColumns.Length];
			DataColumn [] TCol = new DataColumn[Rel.ChildColumns.Length];
			for (int i=0; i< Rel.ChildColumns.Length; i++){
				TCol[i] = Rel.ChildColumns[i];
				string colname= TargetTable.TableName+"."+TCol[i].ColumnName;
				foreach (DataColumn CV in ViewTable.Columns){
					if (CV.ExtendedProperties["ViewSource"]==null) continue;
					if (CV.ExtendedProperties["ViewSource"].ToString()==colname){
						VCol[i]= CV;
						break;
					}
				}				
			}

			string searchfilter= QueryCreator.WHERE_REL_CLAUSE(R, 
				Rel.ParentColumns, 
				VCol,
				DataRowVersion.Default, true);
			DO_GET_TABLE(ViewTable, null, searchfilter, false, null,null);

			//search columns in view corresponding to Rel.ChildColumns
			
			DataColumn [] VKCol = new DataColumn[TargetTable.PrimaryKey.Length];
			DataColumn [] TKCol = new DataColumn[TargetTable.PrimaryKey.Length];
			for (int i2=0; i2< TargetTable.PrimaryKey.Length; i2++){
				TKCol[i2] = TargetTable.PrimaryKey[i2];
				string colname= TargetTable.TableName+"."+TKCol[i2].ColumnName;
				foreach (DataColumn CV in ViewTable.Columns){
					if (CV.ExtendedProperties["ViewSource"]==null) {continue;}
					if (CV.ExtendedProperties["ViewSource"].ToString()==colname){
						VKCol[i2]= CV;
						break;
					}
				}				
			}
            bool emptyStartTable = TargetTable.Rows.Count == 0;
            staticModel.invokeActions(TargetTable,TableAction.beginLoad);

			foreach (DataRow RR in ViewTable.Rows){
				var dict = new Dictionary<string, object>();
				for (int i = 0; i < VKCol.Length; i++) {
					dict[TKCol[i].ColumnName] = RR[VKCol[i].ColumnName];
				}
				//string filterKeyChild = QueryCreator.WHERE_REL_CLAUSE(RR,VKCol, TKCol, DataRowVersion.Default,false);
				//if RV already present in TargetTable, continue
			    if (!emptyStartTable) {
			        DataRow[] found2 = TargetTable._Filter(q.mCmp(dict)); //Select(filterKeyChild);
			        if (found2.Length > 0) continue;
			    }

			    List<string>skippedColumns = new List<string>();
			    DataRow newR = TargetTable.NewRow();
				foreach (DataColumn CC in TargetTable.Columns){
					string colname= TargetTable.TableName+"."+CC.ColumnName;
				    string k = Rel.RelationName + "|" + colname;
				    if (cachedChildSourceColumn.ContainsKey(k)) {
                        newR[CC] = RR[cachedChildSourceColumn[k]];
                    }
				    else {
					    if (skippedColumns.Contains(colname)) continue;
				        bool colFound = false;
				        foreach (DataColumn CV in ViewTable.Columns) {
				            if (CV.ExtendedProperties["ViewSource"] == null) {
				                continue;
				            }
				            if (CV.ExtendedProperties["ViewSource"].ToString() == colname) {
				                newR[CC] = RR[CV];
                                cachedChildSourceColumn[k] = CV;
				                colFound = true;
                                break;
				            }
				        }

				        string expr = QueryCreator.GetExpression(CC);
				        if (expr != null) {
					        var parts = expr.Split('.');
					        if (parts.Length == 2) {
						        string exprTable = parts[0], exprCol = parts[1];
						        foreach (DataColumn CV in ViewTable.Columns) {
							        if (CV.ExtendedProperties["ViewSource"] as string !=  expr) {
								        continue;
							        }
							        newR[CC] = RR[CV];
							        cachedChildSourceColumn[k] = CV;
							        CC.ExtendedProperties["mdl_foundInGetViewChildTable"] = "S";
							        colFound = true;
							       
						        }
					        }
				        }
				        if (!colFound) {
					        skippedColumns.Add(CC.ColumnName);
				        }
				    }
				}

			    try {
			        TargetTable.Rows.Add(newR);
			    }
			    catch (Exception e) {
					var qhc= new CQueryHelper();
                    ErrorLogger.Logger.logException(
                        $"iGetViewChildTable TargetTable.Rows.Add(newR) TargetTable={TargetTable.TableName} Relation={Rel.RelationName} DataSet={TargetTable.DataSet?.DataSetName ?? "no dataset"} "+
                        $" skippedColumns ={string.Join(",",skippedColumns.ToArray())} key={qhc.CmpKey(newR)} ", 
                        e,null,Conn);
			        continue;
			    }


			        //collego la stringa k|chiavefiglio alla riga madre nella vista
                addRowToCache(ViewTable, TargetTable.PrimaryKey, QueryCreator.WHERE_KEY_CLAUSE(newR,DataRowVersion.Default,false) , RR);
                //incPrescannedTable(TargetTable);
                newR.AcceptChanges();
			}
			staticModel.invokeActions(TargetTable,TableAction.endLoad);
		}

	    void initCacheParentView() {
	        tableCache.Clear();
	        cachedParentNoKey.Clear();
	        cachedParentVkey.Clear();
	        cachedParentCkey.Clear();
	        cachedParentSourceColumn.Clear();
	        cachedChildSourceColumn.Clear();
			preScannedTablesRows.Clear();
	    }

        DataRow getRowFromCache(DataTable t, DataColumn[] col, string filter, out bool found) {
	        string cols = "§"+String.Join("§",(from DataColumn c in col select c.ColumnName).ToArray());
	        string tabKey = t.TableName + cols;
	        found = false;
            checkPreScannedTable(t,col);
	        if (!tableCache.ContainsKey(tabKey)) return null;
            if (!tableCache[tabKey].ContainsKey(filter)) return null;
            found = true;
	        return tableCache[tabKey][filter];
	    }
       
        private Dictionary<string, Dictionary<string, DataRow>> tableCache =new Dictionary<string, Dictionary<string, DataRow>>();

	    private Dictionary<string, bool> cachedParentNoKey = new Dictionary<string, bool>();
	    private Dictionary<string, DataColumn[]> cachedParentVkey = new Dictionary<string, DataColumn[]>();
        private Dictionary<string, DataColumn[]> cachedParentCkey = new Dictionary<string, DataColumn[]>();
        private Dictionary<string, DataColumn> cachedParentSourceColumn = new Dictionary<string, DataColumn>();
       
        private Dictionary<string, DataColumn> cachedChildSourceColumn = new Dictionary<string, DataColumn>();

	    private Dictionary<string, int> preScannedTablesRows = new Dictionary<string, int>();

	    bool checkPreScannedTable(DataTable t,DataColumn[]cols) {
		    string cc = "§"+String.Join("§",(from DataColumn c in cols select c.ColumnName).ToArray());
		    string tabKey = t.TableName + cc;

	        if (!preScannedTablesRows.ContainsKey(tabKey)) {
	            preScannedTablesRows[tabKey] = 0;
	        }
	        if (preScannedTablesRows[tabKey] > 0) return false;
	        foreach (DataRow r in t.Rows) {
	            addRowToCache(t, cols,
							 QueryCreator.WHERE_REL_CLAUSE(r, cols,cols, DataRowVersion.Default, false),
							r);
	        }
	        preScannedTablesRows[tabKey] = t.Rows.Count;
            return true;
	    }

	    void incPrescannedTable(DataTable t,DataColumn[]cols) {
		    string cc = "§"+String.Join("§",(from DataColumn c in cols select c.ColumnName).ToArray());
		    string tabKey = t.TableName + cc;
            if (!preScannedTablesRows.ContainsKey(tabKey)) {
                preScannedTablesRows[tabKey] = 0;
            }
	        preScannedTablesRows[tabKey] = preScannedTablesRows[tabKey] + 1;
	    }

        void addRowToCache(DataTable parent, DataColumn[] col, string filter,DataRow r) {
	        string cols = "§"+String.Join("§",(from DataColumn c in col select c.ColumnName).ToArray());
	        string tabKey = parent.TableName + cols;

	        if (!tableCache.ContainsKey(tabKey)) {
                tableCache[tabKey] = new Dictionary<string, DataRow>();
	        }
            tableCache[tabKey][filter] = r;            
        }

	    bool GetParentRowsFromView(DataRow R, DataRelation Rel) {
            int chrono = StartTimer("GetParentRowsFromView * " + Rel.RelationName );
            bool res = iGetParentRowsFromView(R, Rel);
            StopTimer(chrono);
	        return res;
	    }

        /// <summary>
        /// Gets R parent (by relation Rel)row from a view. Assumes that the view table has
        ///  already been read.
        /// Here ViewTable.ExtendedProperties["RealTable"]==R.Table
        /// </summary>
        /// <param name="R"></param>
        /// <param name="Rel"></param>
        /// <returns>true if row has been read (it was in the view)</returns>
        bool iGetParentRowsFromView(DataRow R, DataRelation Rel){
		    if (cachedParentNoKey.ContainsKey(Rel.RelationName)) {
                return false;
		    }
			//Table to retrieve rows
			var TargetTable = Rel.ParentTable;
			var ViewTable = (DataTable) TargetTable.ExtendedProperties["ViewTable"];
			var MainTable = R.Table; //== ViewTable.ExtProp["RealTable"]


            DataColumn[] Ckey;
            DataColumn[] Vkey;
            if(cachedParentVkey.ContainsKey(Rel.RelationName)) {
                Vkey = cachedParentVkey[Rel.RelationName];
                Ckey = cachedParentCkey[Rel.RelationName];
            }
            else {
                //key columns of Parent Table in ViewTable
                Vkey = new DataColumn[TargetTable.PrimaryKey.Length];
                //key columns of Parent Table
                Ckey = new DataColumn[TargetTable.PrimaryKey.Length];
                for(int i = 0; i < Vkey.Length; i++) {
                    bool found = false;
                    string colname = MainTable.TableName + "." + Rel.ChildColumns[i].ColumnName;
                    Ckey[i] = Rel.ParentColumns[i];
                    //search the column in view corresponding to Rel.ParentCol[i]
                    foreach(DataColumn CV in ViewTable.Columns) {
                        if(CV.ExtendedProperties["ViewSource"] == null)
                            continue;
                        if(CV.ExtendedProperties["ViewSource"].ToString() == colname) {
                            found = true;
                            Vkey[i] = CV;
                            break;
                        }
                    }
                    if(!found) {
                        cachedParentNoKey.Add(Rel.RelationName, true);
                        return false; //relation columns were not found
                    }
                }
                cachedParentVkey[Rel.RelationName] = Vkey;
                cachedParentCkey[Rel.RelationName] = Ckey;
            }


            string viewparentfilter = QueryCreator.WHERE_REL_CLAUSE(R, Rel.ChildColumns,Rel.ChildColumns, DataRowVersion.Default,false);
            //era WHERE_REL_CLAUSE(R, Rel.ChildColumns, VKey 
            var RV = getRowFromCache(ViewTable, Rel.ChildColumns, viewparentfilter, out var foundR); //Cerca con la chiave sul campo della parent
            if (foundR && RV==null) return false;
		    if (RV == null) {
			    //string kFilter = QueryCreator.WHERE_KEY_CLAUSE(R, DataRowVersion.Default, false);
                RV = getRowFromCache(ViewTable,Rel.ChildColumns, viewparentfilter, out var foundRMain);	//cerca con la chiave sul campo della vista
                if (RV == null) {
	                var ViewParentRows = ViewTable._Filter(q.mCmp(R,Rel.ChildColumns));//.Select(viewparentfilter);
                    if (ViewParentRows.Length == 0) {
                        addRowToCache(ViewTable, Rel.ChildColumns, viewparentfilter, null);
                        return false;
                    }
                    RV = ViewParentRows[0];

                    addRowToCache(ViewTable, Rel.ChildColumns, viewparentfilter, RV);
                    incPrescannedTable(ViewTable,Rel.ChildColumns);
                }
               
		       
		    }


            //get search condition for child row				
            string filterparent = QueryCreator.WHERE_REL_CLAUSE(RV,Vkey, Ckey, DataRowVersion.Default, false);
		    var childFound = getRowFromCache(TargetTable, Rel.ChildColumns, filterparent, out var unused);            
		    if (childFound != null) return true;

          

            //         //if RV already present in TargetTable, continue---> NO: return true!
   //         DataRow[] found2 = TargetTable.Select(filterparent);
			//if (found2.Length>0) {
   //             addRowToCache(TargetTable, filterparent, found2[0]);
   //             return true; //ex continue
   //         }
			
			var NewChild = TargetTable.NewRow();
			//copy key from view to new row
			for (int ii=0; ii<Vkey.Length; ii++){
                string colname2 = TargetTable.TableName + "." + Ckey[ii].ColumnName;
                string k = Rel.RelationName + "|" + colname2;
			    cachedParentSourceColumn[k] = Vkey[ii];
                //NewChild[Ckey[ii]] = RV[Vkey[ii]];
			}
			
			//copy values from view to new row
			foreach (DataColumn CCT in TargetTable.Columns){
				string colname2= TargetTable.TableName+"."+CCT.ColumnName;
			    string k = Rel.RelationName + "|" + colname2;
			    if (cachedParentSourceColumn.ContainsKey(k)) {
			        NewChild[CCT] = RV[cachedParentSourceColumn[k]];
			    }
			    else {

			        foreach (DataColumn CCV in ViewTable.Columns) {
			            if (CCV.ExtendedProperties["ViewSource"] == null) continue;
			            if (CCV.ExtendedProperties["ViewSource"].ToString() == colname2) {
			                NewChild[CCT] = RV[CCV];
                            cachedParentSourceColumn[k] = CCV;
                            break;
			            }
			        }
			    }
			}
			TargetTable.Rows.Add(NewChild);
			NewChild.AcceptChanges();
            addRowToCache(TargetTable, Ckey, filterparent, NewChild);
            incPrescannedTable(TargetTable,Rel.ChildColumns);
            return true;
		
		}

		bool DataRowInList(DataRow R, DataRow []List){
			foreach(DataRow RR in List){
				if (R.Equals(RR)) return true;
			}
			return false;
		}

        /// <summary>
        /// Get all child rows of rows in RR, only navigating in tables whose name is in Allowed.keys
        /// </summary>
        /// <param name="RR"></param>
        /// <param name="Allowed"></param>
        /// <param name="SelList"></param>
		public void GetAllChildRows(DataRow []RR, Hashtable Allowed, List<SelectBuilder> SelList){
			if (RR.Length==0) return;
			var currTable= RR[0].Table;
            foreach (DataRelation rel in currTable.ChildRelations) {
                var allowedParents = currTable.Select(QueryCreator.GetRelationActivationFilter(rel));

                var childtable = rel.ChildTable.TableName;
                if (currTable.Rows.Count == 0) continue;
                if (Allowed[childtable] == null) continue;

                var viewTable = (DataTable) rel.ChildTable.ExtendedProperties["ViewTable"];//Vede se la tabella è da leggere da una vista
                if ((viewTable != null) &&
                    (viewTable.ExtendedProperties["RealTable"] == rel.ChildTable) //vede se la vista ha come tabella principale quella data
                ) {
                    foreach (var r in RR) {
                        //if (R.RowState== DataRowState.Added) continue; //NEW!
                        if (DataRowInList(r, allowedParents)) GetViewChildTable(r, rel);//allowedParents di solito ha una riga sola
                    }
                    continue;
                }

                //if ((RR.Length==1)||(Rel.ChildColumns.Length!=1)) {
                foreach (var r in RR) {
                    if (r.RowState == DataRowState.Deleted) continue;
                    //if (R.RowState== DataRowState.Added) continue; //NEW!
                    if (!DataRowInList(r, allowedParents)) continue;
                    var childfilter = QueryCreator.WHERE_REL_CLAUSE(r, rel.ParentColumns, rel.ChildColumns,DataRowVersion.Default, true);
                    var mc = QueryCreator.GET_MULTICOMPARE(r, rel.ParentColumns, rel.ChildColumns,DataRowVersion.Default, true);
                    GetRowsByFilter(childfilter, mc, childtable, null, true, SelList);
                }
            }

        }

		/// <summary>
		/// Gets R childs in a set of allowed Tables
		/// </summary>
		/// <param name="R"></param>
		/// <param name="Allowed">List of tables of which childs must be searched</param>
        /// <param name="selList"></param>
		void GetChildRows(DataRow R, Hashtable Allowed, List<SelectBuilder> selList){
            //bool HadChanges = DS.HasChanges();
			foreach (DataRelation Rel in R.Table.ChildRelations){
				DataRow []AllowedParents = R.Table.Select(QueryCreator.GetRelationActivationFilter(Rel));
				if (!DataRowInList(R,AllowedParents)) continue;
				
				string childtable= Rel.ChildTable.TableName;
				if (Allowed[childtable]==null) continue;

				//Retrieve child rows
				if (QueryCreator.ContainsNulls(R, Rel.ParentColumns, 
					DataRowVersion.Default)) continue;

				DataTable ViewTable = (DataTable) Rel.ChildTable.ExtendedProperties["ViewTable"];
				if ((ViewTable!=null)&&
					(ViewTable.ExtendedProperties["RealTable"]==Rel.ChildTable)
					) {
					GetViewChildTable(R,Rel);
					continue;
				}

				string childfilter= QueryCreator.WHERE_REL_CLAUSE(R, Rel.ParentColumns, Rel.ChildColumns, 
					DataRowVersion.Default,true);
                if (childfilter == "") continue;
                MultiCompare MC = QueryCreator.GET_MULTICOMPARE(R, Rel.ParentColumns, Rel.ChildColumns,
                    DataRowVersion.Default, true);
				//inutile, poiché GetRowByFilter effettua il merge
				//childfilter = MergeFilters(childfilter, Rel.ChildTable);
				
				GetRowsByFilter(childfilter,MC, childtable, null,true, selList);     
			}
            //bool HasChanges = DS.HasChanges();
            //if (HadChanges != HasChanges) {
            //    MarkEvent("Errore in GetChildRows di "+R.Table.TableName);
            //}
		}


		

        void GetRowsByFilter(string filter, MultiCompare MC, string Table, string TOP, bool prepare, List<SelectBuilder> selList) {
            var T = DS.Tables[Table];
            if (!model.canRead(T)) return;
            var mergedfilter = GetData.MergeFilters(filter, T);

            if (selList == null) {
                Conn.RUN_SELECT_INTO_TABLE(T, T.getSorting(), mergedfilter, TOP, prepare);
            }
            else {
                selList.Add(new SelectBuilder().IntoTable(T).Where(mergedfilter).MultiCompare(MC).Top(TOP).OrderBy(T.getSorting()));
            }

            //Cache(cachedcmd);
            tableHasBeenRead(T);

        }
         		

		/// <summary>
		/// Get parent rows of a given Row, in a set of specified  tables.
		/// </summary>
		/// <param name="r">DataRow whose parents are wanted</param>
		/// <param name="allowed">Tables in which to search parent rows</param>
        /// <param name="selList"></param>
		/// <returns>true if any of parent DataRows was already in memory. This is not 
		///  granted if rows are taken from a view</returns>
		public bool GetParentRows(DataRow r, Hashtable allowed, List<SelectBuilder> selList){
			var inmemory = false;
			if (r==null) return false;
			if (r.RowState==DataRowState.Detached) return false;
			if (r.RowState==DataRowState.Deleted) return false;
			foreach (DataRelation rel in r.Table.ParentRelations){

				var parenttable= rel.ParentTable.TableName;
				if (allowed[parenttable]==null) continue;
				if (QueryCreator.ContainsNulls(r, rel.ChildColumns, DataRowVersion.Default)) continue;

                //DataRow []AllowedChilds= R.Table.Select(QueryCreator.GetParentRelationActivationFilter(Rel));
                //if (!DataRowInList(R,AllowedChilds)) continue;

				var parentfilter= QueryCreator.WHERE_REL_CLAUSE(r, rel.ChildColumns, rel.ParentColumns, DataRowVersion.Default, true);
                if (parentfilter == "") continue;
               


               //correggo colonne da usare nella ricerca nel parent, l'errore stava causando rallentamenti nei tree, task 15039
                var rFound = getRowFromCache(rel.ParentTable, rel.ParentColumns, parentfilter, out var parentFound);
                if (parentFound) {
					inmemory=true;
				}
				else {
	                
				
	                //Non usato in chiamata a GetRowsByFilter
	                //var parentfilternoSql= QueryCreator.WHERE_REL_CLAUSE(r, rel.ChildColumns, rel.ParentColumns, DataRowVersion.Default,false);
				
	                //17 APRILE 2003:
	                //La metto di nuovo poiché non credo a quanto scritto sotto!!
	                //string mergedfilter = MergeFilters(parentfilternoSql, rel.ParentTable)

	                //Retrieve parent rows only if not already present in table
	                //TODO: VERIFICARE SE AVER RIMOSSO MERGEDFILTER PROVOCA DANNI.
	                //MERGEDFILTER E' STATO SOSTITUITO CON parentfilterNOSQL poichè nella ricerca
	                //del fondo di ricerca era posta una condizione su di un campo della vista, 
	                //la qual cosa provocava un eccezione qui.
	                //31 LUGLIO 2007 ho definitivamente stabilito che non va usato il filtro statico, perché 
	                // 1) si riferisce alla vista
	                // 2) è di tipo SQL e può andare in eccezione sul dataset (infatti ci va!)
	                // 3) è inutile controllare la parte statica poiché comune a tutte gli accessi quindi superflua
	                //    ossia tutte le righe in memoria sodisfano quella parte del filtro!
	                //2019: ho trovato che se la chiave è incompleta, qui serve ancora il filtro statico, che si riferisce alla vista

					var viewTable = (DataTable) rel.ParentTable.ExtendedProperties["ViewTable"];
					if ( (viewTable!=null)&&  (viewTable.ExtendedProperties["RealTable"]==r.Table) ) {
						if (!GetParentRowsFromView(r, rel)) {
							var multiComp = QueryCreator.GET_MULTICOMPARE(r, rel.ChildColumns, rel.ParentColumns, DataRowVersion.Default, true);
							GetRowsByFilter(parentfilter,multiComp,  parenttable, null,true,selList);
						}					
					}
					else	   {
						var multiComp = QueryCreator.GET_MULTICOMPARE(r, rel.ChildColumns, rel.ParentColumns, DataRowVersion.Default, true);
						GetRowsByFilter(parentfilter, multiComp, parenttable, null,true,selList);
					}
				}
			}
			return inmemory;
		}

		/// <summary>
		/// Gets a DataTable related with PrimaryTable via a given Relation Name.
		/// Also gets columns implied in the relation of related table.
		/// </summary>
		/// <param name="relname"></param>
		/// <returns>related DataTable</returns>
		public DataTable EntityRelatedByRel(string relname){
            return EntityRelatedByRel(relname, out var Cs);
        }

		/// <summary>
		/// Gets a DataTable related with PrimaryTable via a given Relation Name.
		/// Also gets columns implied in the relation of related table 
		/// </summary>
		/// <param name="relname"></param>
		/// <param name="Cs">Columns of related table, implied in the relation</param>
		/// <returns>Related table</returns>
		public DataTable EntityRelatedByRel(string relname, out DataColumn[] Cs){
			Cs=null;
			if (PrimaryDataTable.ParentRelations[relname]!=null){
				var ParentRel = PrimaryDataTable.ParentRelations[relname];
				Cs = ParentRel.ParentColumns;
				return ParentRel.ParentTable;
			}
			if (PrimaryDataTable.ChildRelations[relname]!=null){
				var childRel = PrimaryDataTable.ChildRelations[relname];
				Cs= childRel.ChildColumns;
				return childRel.ChildTable;
			}
			return null;
		}


		#region Gestione Custom Query 
	

		

      

        class MyParameter {
			public string name;
			public string val;
			public MyParameter(string name, string val){
				this.name=name;
				this.val=val;
			}

			public static string SearchInArray(string name, ArrayList val){
				for (int i=0; i<val.Count; i++){
					var P = (MyParameter)val[i];
					if (P.name.Equals(name))return  P.val;
				}
				return null;
			}
		}
	
		#endregion



	}



}

    



using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using q = mdl.MetaExpression;
#pragma warning disable IDE1006 // Naming Styles
//using System.EnterpriseServices;


namespace mdl {

	/// <summary>
	/// Represents the change of a single row that has to be performed on a 
	///  Database Table
	/// </summary>
	public class RowChange {

		/// <summary>
		/// DataRow linked to the RowChange
		/// </summary>
		public DataRow DR;       



		/// <summary>
		/// Extended Property of DataColumn that states that the column has to be
		///  calculated during Post Process when it is added to DataBase.
		/// </summary>
		/// <remarks>
		/// The field is calculated like:
		/// [Row[PrefixField]] [MiddleConst] [LeftPad(newID, IDLength)]
		/// so that, called the first part [Row[PrefixField]] [MiddleConst] as PREFIX,
		/// if does not exists another row with the same PREFIX for the ID, the newID=1
		/// else newID = max(ID of same PREFIX-ed rows) + 1
		/// </remarks>
		const string AutoIncrement = "IsAutoIncrement";
		const string CustomAutoIncrement = "CustomAutoIncrement";
		const string PrefixField = "PrefixField";
		const string MiddleConst = "MiddleConst";
		const string IDLength    = "IDLength";
		const string Selector    = "Selector";
        const string SelectorMask = "SelectorMask";
        const string MySelector = "MySelector";
        const string MySelectorMask = "MySelectorMask";
        const string LinearField = "LinearField";
        
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="dr"></param>
        public RowChange(DataRow dr) {
            this.DR = dr;
            EnforcementMessages = null; //new ProcedureMessageCollection();
        }
        
        /// <summary>
        /// Assigns this RowChange to a Collection
        /// </summary>
        /// <param name="rc"></param>
        public void SetCollection(RowChangeCollection rc){
            myCollection=rc;        
        }
        RowChangeCollection myCollection = null;
		/// <summary>
		/// Creates a new RowChange linked to a given DataRow
		/// </summary>
		/// <param name="dr"></param>
        /// <param name="parentCollection"></param>
		public RowChange(DataRow dr, RowChangeCollection parentCollection){
			this.DR=dr;
			EnforcementMessages= null; //new ProcedureMessageCollection();
            myCollection = parentCollection;		    
		}

        /// <summary>
        /// List of tables incrementally scanned in the analysis
        /// </summary>
        public List<string> HasBeenScanned = new List<string>();


		/// <summary>
		/// String representaion of the change
		/// </summary>
		/// <returns></returns>
		public override String ToString() {
			return TableName+"."+DR.ToString();
		}    

		/// <summary>
		/// gets the DataTable that owns the chenged row
		/// </summary>
		public DataTable Table {
			get {
				return DR.Table;
			}
		}

		/// <summary>
		/// Gets the name of the table to which the changed row belongs
		/// </summary>
		public string TableName {
			get {
				return DR.Table.TableName;
			}
		}            

		/// <summary>
		/// Gets the real table that will be used to write the row to the DB
		/// </summary>
		public string PostingTable {
			get {
				return DR.Table.tableForPosting();
			}
		}
    
      

		/// <summary>
		/// Short description for update, used for composing business rule 
		///  stored procedure names
		/// </summary>
		public const string short_update_descr = "u";
		/// <summary>
		/// Short description for insert, used for composing business rule 
		///  stored procedure names
		/// </summary>
		public const string short_insert_descr = "i";
		/// <summary>
		/// Short description for delete, used for composing business rule 
		///  stored procedure names
		/// </summary>
		public const string short_delete_descr = "d";
		/// <summary>
		/// Used in the composition of the key, during logging
		/// </summary>
		public const string KeyDelimiter = " | ";

		/// <summary>
		/// Gets a i/u/d description of the row status
		/// </summary>
		/// <returns></returns>
		public virtual string ShortStatus(){
			String Op="";
			switch(DR.RowState){
				case DataRowState.Added: 
					Op = short_insert_descr;//"i"
					break;
				case DataRowState.Deleted:
					Op = short_delete_descr;//"d"
					break;
				case DataRowState.Modified:
					Op= short_update_descr;//"u"
					break;
			}
			return Op;

		}

		/// <summary>
		/// Get the name of Stored procedure to call in pre-check phase
		/// </summary>
		/// <returns></returns>
		public virtual String PreProcNameToCall(){
			return "sp_"+ShortStatus()+"_"+PostingTable;
		}
		/// <summary>
		/// get the name of Stored procedure to call in post-check phase
		/// </summary>
		/// <returns></returns>
		public virtual String PostProcNameToCall(){
			return "sp_"+ShortStatus()+"__"+PostingTable;
		}
        
		/// <summary>Gets a filter of TableName AND dboperation (I/U/D)</summary>
		/// <returns>filter String</returns>
		public virtual String FilterTableOp(){
			return "(dbtable = '"+TableName+"' ) AND (dboperation = '"+ShortStatus()+"' )";
		}
 
		/// <summary>
		/// Gets a filter on Posting Table and DB operation 
		/// </summary>
		/// <returns></returns>
		public virtual String FilterPostTableOp(){
			return "(dbtable = '"+PostingTable+"' ) AND (dboperation = '"+ShortStatus()+"' )";
		}

		/// <summary>
		/// Error messages about related stored procedures
		/// </summary>
		public ProcedureMessageCollection EnforcementMessages;

		/// <summary>
		/// Related rows on other tables
		/// </summary>
        public Dictionary<string, DataRow> Related = new Dictionary<string, DataRow>(); //ex SortedList

		/// <summary>
		/// Get a new rowchange class linked to a given DataRow
		/// </summary>
		/// <param name="R"></param>
		/// <returns></returns>
		virtual protected RowChange GetNewRowChange(DataRow R){
			return new RowChange(R);
		}

        
		/// <summary>
		/// Gets the list of primary key column name separated by KeyDelimiter
		/// </summary>
		/// <returns></returns>
		public string PrimaryKey(){
			string Key = "";
			bool first = true;
			var DV = DataRowVersion.Default;
			if (DR.RowState== DataRowState.Deleted) DV = DataRowVersion.Original;
			foreach (var C in DR.Table.PrimaryKey){
				if (!first) Key += KeyDelimiter;
				Key += DR[C.ColumnName, DV];
				first=false;
			}
			return Key;
		}



		/// <summary>
		/// Copy an extended property of a datacolumn into another one.
		/// Checks for column existence in both tables.
		/// </summary>
		/// <param name="In"></param>
		/// <param name="Out"></param>
		/// <param name="colname"></param>
		/// <param name="property"></param>
		static void copyproperty(DataTable In, DataTable Out, string colname, string property){
			if (In.Columns[colname]==null) return;
			if (Out.Columns[colname]==null) return;
			if (In.Columns[colname].ExtendedProperties[property]==null) return;

			Out.Columns[colname].ExtendedProperties[property]=
				In.Columns[colname].ExtendedProperties[property];
		}

	    /// <summary>
	    /// Class for logging errors
	    /// </summary>
	    public IErrorLogger errorLogger { get; set; } = ErrorLogger.Logger;

        /// <summary>
        /// Get a DataRow related to the RowChange, in a given tablename
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public DataRow GetRelated(string tablename) {
            string realname = DR.Table.tableForPosting();
            if (realname == tablename) return DR;                    
            if (Related.ContainsKey(tablename)) return Related[tablename];
            SEARCH_RELATED(tablename);
            if (Related.ContainsKey(tablename)) return Related[tablename];
            return null;
        }

        public bool useIndex=true;

        /// <summary> Search for rows related to the "Change" variation (referred to ONE row R of ONE table T)
        ///  For each table in DS, are examined the relation to T
        ///  For each table having exactly ONE row related to R, this row is added to Related.
        ///  Rows are added to a list that is indexed by the name of the row DataTable 
        /// </summary>
        /// <param name="tablename"></param>   
        /// <remarks>
        ///   This Function assumes master/child relations to be NOT circular.
        ///   This function does not consider the possibility of raising different rows 
        ///     in the same table using more than one path. It assumes that there is
        ///     only one path connecting one table to another.
        /// </remarks>
        public void SEARCH_RELATED(string tablename) {
            if (HasBeenScanned.Contains(tablename)) return;
            HasBeenScanned.Add(tablename);

            DataRow ROW = this.DR;
            DataTable T = ROW.Table;
            string realname = T.tableForPosting();
            //Tryes to get rows related to ROW.
            DataRowVersion toConsider = DataRowVersion.Default;
            if (ROW.RowState == DataRowState.Deleted) toConsider = DataRowVersion.Original;

            //Scans relations where T is the parent and T2 s the CHILD
            //We want to find rows in Rel.ChildTable
            foreach (DataRelation rel in T.ChildRelations) {
                //Here ROW is PARENT TABLE and we search in Rel.ChildTable, T2 is CHILD TABLE
                DataTable childTable = rel.ChildTable;  //table to search in
                if (childTable.tableForPosting() !=tablename) continue;
                try {
                    if (rel.ParentTable.TableName != T.TableName) continue;
                    //string whereclause = QueryCreator.WHERE_REL_CLAUSE(ROW, rel.ParentColumns,rel.ChildColumns, toConsider, false);
                    var rr = childTable._Filter(q.mGetChilds(ROW, rel, toConsider));  
								//childTable.Select(whereclause, null, DataViewRowState.CurrentRows);
                    int n = rr.Length;
                    //if (n == 0) {
	                   // string whereclause = QueryCreator.WHERE_REL_CLAUSE(ROW, rel.ParentColumns,rel.ChildColumns, toConsider, false);
                    //    rr = childTable.Select(whereclause, null, DataViewRowState.Deleted);
                    //    n = rr.Length;
                    //}

                    if (n == 1) {
                        Related[tablename] = rr[0];
                        return;
                    }
                }
                catch (Exception e) {
                    errorLogger.logException("Errore in SEARCH_RELATED (1)(" + tablename + ")",exception:e);
                    errorLogger.markException(e,"Error in RowChange.SearchRelated.");                  
                }
            }//foreach DataRelation

            //Scans relations where T is the CHILD
            //We want to find rows in Rel.ParentTable
            foreach (DataRelation rel in T.ParentRelations) {
                //Here ROW is CHILD TABLE and we search in Rel.ParentTable
                DataTable parentTable = rel.ParentTable;
                if (parentTable.tableForPosting() != tablename) continue;
                try {
                    if (rel.ChildTable.TableName != T.TableName) continue;
                    //string whereclause = QueryCreator.WHERE_REL_CLAUSE(ROW, rel.ChildColumns,rel.ParentColumns, toConsider, false);
                    //var rr = parentTable.Select(whereclause, null, DataViewRowState.CurrentRows);
                    var rr = parentTable._Filter(q.mGetParents(ROW, rel, toConsider)); 
                    int n = rr.Length;
                    //if (n == 0) {
                    //    rr = parentTable.Select(whereclause, null, DataViewRowState.Deleted);
                    //    n = rr.Length;
                    //}
                    if (n != 1) continue;
                    Related[tablename] = rr[0];
                    return;
                }
                catch (Exception e) {
                    errorLogger.logException("Errore in SEARCH_RELATED (2)(" + tablename + ")",e); 
                    errorLogger.markException(e,"Error in RowChange.SearchRelated.");
                }
            }//foreach DataRelation

        }

	 


        #region AUTOINCREMENT FIELD GET/SET

        /// <summary>
        /// Sets selector for a specified DataColumn
        /// </summary>
        /// <param name="C"></param>
        /// <param name="ColumnName"></param>
        /// <param name="mask"></param>
        public static void SetMySelector(DataColumn C,string ColumnName, ulong mask) {
            var amask = C.ExtendedProperties[MySelectorMask] as string;
            if (!(C.ExtendedProperties[MySelector] is string sel)) {
                sel = ColumnName;
                amask = mask.ToString();
                C.ExtendedProperties[MySelector] = sel;
                C.ExtendedProperties[MySelectorMask] = amask;
                return;
            }
            if (sel == ColumnName)
                return;
            if (sel.StartsWith(ColumnName+","))
                return;
            if (sel.EndsWith("," + ColumnName))
                return;
            if (sel.Contains(","+ColumnName+",")) return;
            sel = sel + "," + ColumnName;
            amask = amask + "," + mask.ToString();

            C.ExtendedProperties[MySelector] = sel;
            C.ExtendedProperties[MySelectorMask] = amask;
        }


		/// <summary>
		/// Add a selector-column to the table. AutoIncrement columns are calculated between
		///  equal selectors-column rows
		/// </summary>
		/// <param name="T"></param>
		/// <param name="ColumnName"></param>
		public static void SetSelector(DataTable T, string ColumnName){
			DataColumn C = T.Columns[ColumnName];
			C.ExtendedProperties[Selector]="y";
            C.ExtendedProperties[SelectorMask] = null;
		}

        /// <summary>
        /// Mark a column  as a general selector for a table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="ColumnName"></param>
        /// <param name="mask"></param>
        public static void SetSelector(DataTable T, string ColumnName, UInt64 mask) {
            DataColumn C = T.Columns[ColumnName];
            C.ExtendedProperties[Selector] = "y";
            C.ExtendedProperties[SelectorMask] = mask;
        }


        /// <summary>
        /// Remove a column from general selectors of a table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="ColumnName"></param>
		public static void ClearSelector(DataTable T, string ColumnName){
			DataColumn C = T.Columns[ColumnName];
			C.ExtendedProperties[Selector]=null;
            C.ExtendedProperties[SelectorMask] = null;
        }

        /// <summary>
        /// Remove all columns as specific selector ofa a column
        /// </summary>
        /// <param name="T"></param>
        /// <param name="ColumnName"></param>
        public static void ClearMySelector(DataTable T, string ColumnName) {
            DataColumn C = T.Columns[ColumnName];
            C.ExtendedProperties[MySelector] = null;
            C.ExtendedProperties[MySelectorMask] = null;
        }


        /// <summary>
        /// Gets the selector combination for a DataRow
        /// </summary>
        /// <param name="DR"></param>
        /// <param name="C"></param>
        /// <param name="QH"></param>
        /// <returns></returns>
		public static string GetSelector(DataRow DR, DataColumn C, QueryHelper QH){
			return GetSelector(DR.Table, DR, C, QH);
		}

        /// <summary>
        /// Gets all field selector for a datacolumn
        /// </summary>
        /// <param name="Col"></param>
        /// <returns></returns>
        public static List<DataColumn> GetSelectors(DataColumn Col) {
            var res = new List<DataColumn>();
            var T = Col.Table;
            foreach (DataColumn C in T.Columns) {
                if (C.ExtendedProperties[Selector] != null) {
                    res.Add(C);
                }
            }
            if(!(Col.ExtendedProperties[MySelector] is string mysel)) return res;

            string[] fname = mysel.Split(',');            
            for (int i = 0; i < fname.Length; i++) {
                res.Add(T.Columns[fname[i]]);
            }
            return res;
        }
		
		/// <summary>
		/// Gets a condition of all selector fields of a row
		/// </summary>
        /// <param name="T"></param>
		/// <param name="DR"></param>
        /// <param name="CC"></param>
        /// <param name="QH"></param>
		/// <returns></returns>
		public static string GetSelector(DataTable T, DataRow DR, DataColumn CC, QueryHelper QH){            
			string sel="";
            bool onlykey = false; // QueryCreator.IsPrimaryKey(T, CC.ColumnName);
            foreach (DataColumn C in T.Columns) {
                if (onlykey) {
                    if (!QueryCreator.IsPrimaryKey(T, C.ColumnName)) continue;
                }
                if (C.ExtendedProperties[Selector] != null) {
                    if (C.ExtendedProperties[SelectorMask] != null) {
                        if (DR[C.ColumnName] == DBNull.Value) {
                            sel = QH.AppAnd(sel, QH.IsNull(C.ColumnName)); 
                            continue;
                        }
                        var mask = (ulong) C.ExtendedProperties[SelectorMask] ;
                        var val = Convert.ToUInt64( DR[C.ColumnName]);
                        sel = QH.AppAnd(sel,QH.CmpMask(C.ColumnName,mask,val));
                    }
                    else {
                        sel = QH.AppAnd(sel,QH.CmpEq(C.ColumnName,DR[C.ColumnName]));
                    }
                }
            }
            //Now gets the SPECIFIC selector
            if(!(CC.ExtendedProperties[MySelector] is string mysel)) return sel;
            string []fname = mysel.Split(',');
            string[] fmask = CC.ExtendedProperties[MySelectorMask].ToString().Split(',');
            for (int i = 0; i < fname.Length; i++) {
                string col = fname[i];
                if (onlykey) {
                    if (!QueryCreator.IsPrimaryKey(T, col)) continue;
                }
                if (DR[col] == DBNull.Value) {
                    sel = QH.AppAnd(sel, QH.IsNull(col));
                    continue;
                }
                var mask = Convert.ToUInt64(fmask[i]);
                var val = Convert.ToUInt64(DR[col]);                
                sel = QH.AppAnd(sel, QH.CmpMask(col, mask, val));
            }

            return sel;
		}

        /// <summary>
        /// Gets a condition of all selector fields of a row
        /// </summary>
        /// <param name="T">Table row</param>
        /// <param name="DR"></param>
        /// <param name="QH"></param>
        /// <returns></returns>
        public static string GetAllSelectors(DataTable T, DataRow DR,  QueryHelper QH) {
            string sel = "";
            string allfield = "";
            string allmask = "";
            foreach (DataColumn C in T.Columns) {
                if (C.ExtendedProperties[Selector] != null) {
                    if (C.ExtendedProperties[SelectorMask] != null) {
                        UInt64 mask = (UInt64)C.ExtendedProperties[SelectorMask];
                        UInt64 val = Convert.ToUInt64(DR[C.ColumnName]);
                        if (allfield == "") {
                            allfield = C.ColumnName;
                            allmask = val.ToString();
                        }
                        else {
                            allfield += "," + C.ColumnName;
                            allmask += "," + val.ToString();
                        }
                    }
                    else {
                        if (allfield == "") {
                            allfield = C.ColumnName;
                            allmask = "0";
                        }
                        else {
                            allfield += "," + C.ColumnName;
                            allmask += ",0";
                        }

                    }
                }
                if(!(C.ExtendedProperties[MySelector] is string mysel)) continue;
                string[] fname = mysel.Split(',');
                string[] fmask = C.ExtendedProperties[MySelectorMask].ToString().Split(',');
                for (int i = 0; i < fname.Length; i++) {
                    if (allfield == fname[i]) continue;
                    if (allfield.StartsWith(fname[i] + ",")) continue;
                    if (allfield.EndsWith("," + fname[i])) continue;
                    if (allfield.Contains("," + fname[i] + ",")) continue;
                    var mask = Convert.ToUInt64(fmask[i]);
                    
                    if (allfield == "") {
                        allfield = fname[i];
                        allmask = mask.ToString();
                    }
                    else {
                        allfield += "," + fname[i];
                        allmask += ","+mask.ToString();
                    }

                }

            }

            //Now gets the SPECIFIC selector
            string mysel2 = allfield;
            if (mysel2 == "") return sel;
            string[] fname2 = mysel2.Split(',');
            string[] fmask2 = allmask.Split(',');
            for (int i = 0; i < fname2.Length; i++) {
                string col = fname2[i];
                UInt64 mask = Convert.ToUInt64(fmask2[i]);
                if (mask == 0) {
                    sel = QH.AppAnd(sel, QH.CmpEq(col, DR[col]));
                }
                else {
                    //if (mask == 0) continue;
                    UInt64 val = Convert.ToUInt64(DR[col]);
                    sel = QH.AppAnd(sel, QH.CmpMask(col, mask, val));
                }
            }

            return sel;
        }



		/// <summary>
		/// Copy all autoincrement properties of a table into another one.
		/// </summary>
		/// <param name="In"></param>
		/// <param name="Out"></param>
		public static void CopyAutoIncrementProperties(DataTable In, DataTable Out){
			foreach(DataColumn C in Out.Columns){
				foreach (string prop in new string [] { 
														  AutoIncrement, CustomAutoIncrement, 
														   PrefixField, MiddleConst, IDLength,
														  Selector, SelectorMask, MySelector, MySelectorMask,
                    LinearField}){
					copyproperty(In, Out, C.ColumnName, prop);
				}
			}
		}


		/// <summary>
		/// Mark a column as an autoincrement, specifying how the calculated ID must be
		///  composed.
		/// </summary>
		/// <param name="C">Column to set</param>
		/// <param name="prefix">field of rows to be put in front of ID</param>
		/// <param name="middle">middle constant part of ID</param>
		/// <param name="length">length of the variable part of the ID</param>
		/// <remarks>
		/// The field will be calculated like:
		/// [Row[PrefixField]] [MiddleConst] [LeftPad(newID, IDLength)]
		/// so that, called the first part [Row[PrefixField]] [MiddleConst] as PREFIX,
		/// if does not exists another row with the same PREFIX for the ID, the newID=1
		/// else newID = max(ID of same PREFIX-ed rows) + 1
		/// </remarks>
		static public void MarkAsAutoincrement(DataColumn C, 
				string prefix, 
				string middle,
				int length){
			C.ExtendedProperties[AutoIncrement]="s";
			C.ExtendedProperties[PrefixField]=prefix;
			C.ExtendedProperties[MiddleConst]=middle;
			C.ExtendedProperties[IDLength]=length;
		}

		/// <summary>
		/// Set a DataColumn as "AutoIncrement", specifying how the calculated ID must be
		///  composed.
		/// </summary>
		/// <param name="C">Column to set</param>
		/// <param name="prefix">field of rows to be put in front of ID</param>
		/// <param name="middle">middle constant part of ID</param>
		/// <param name="length">length of the variable part of the ID</param>
		/// <param name="linear">if true, Selector Fields, Middle Const and Prefix 
		///		fields are not taken into account while calculating the field</param>
		/// <remarks>
		/// The field will be calculated like:
		/// [Row[PrefixField]] [MiddleConst] [LeftPad(newID, IDLength)]
		/// so that, called the first part [Row[PrefixField]] [MiddleConst] as PREFIX,
		/// if does not exists another row with the same PREFIX for the ID, the newID=1
		/// else newID = max(ID of same PREFIX-ed rows) + 1
		/// </remarks>
		static public void MarkAsAutoincrement(DataColumn C, 
					string prefix, 
					string middle,
					int length, 
					bool linear){
			if (C==null){
				
				ErrorLogger.Logger.markEvent("Cant mark autoincrement a null Column");
				return;
			}
			C.ExtendedProperties[AutoIncrement]="s";
			C.ExtendedProperties[PrefixField]=prefix;
			C.ExtendedProperties[MiddleConst]=middle;
			C.ExtendedProperties[IDLength]=length;
			if (linear) C.ExtendedProperties[LinearField]="1";
		}

		/// <summary>
		/// Removes autoincrement property from a DataColumn
		/// </summary>
		/// <param name="C"></param>
		static public void ClearAutoIncrement(DataColumn C){
			C.ExtendedProperties[AutoIncrement]=null;
		}

		/// <summary>
		/// Tells whether a Column is a AutoIncrement 
		/// </summary>
		/// <param name="C"></param>
		/// <returns>true if Column is Auto Increment</returns>
		static public bool IsAutoIncrement(DataColumn C){
			if (C.ExtendedProperties[AutoIncrement]!=null) return true; 
			return false;
		}

        /// <summary>
        /// Tells PostData to evaluate a specified column through the specified customFunction
        /// </summary>
        /// <param name="C">Column to evaluate</param>
        /// <param name="CustomFunction">delegate to call for evaluating autoincrement column</param>
        [Obsolete]
        static public void MarkAsCustomAutoincrement(DataColumn C, 
			CustomCalcAutoID CustomFunction){
			C.ExtendedProperties[CustomAutoIncrement]= CustomFunction;
		}


        /// <summary>
        ///  Tells PostData to evaluate a specified column through the specified customFunction
        /// </summary>
        /// <param name="C"></param>
        /// <param name="CustomFunction"></param>
        static public void markAsCustomAutoincrement(DataColumn C,
	        CustomCalcAutoId CustomFunction) {
	        C.ExtendedProperties[CustomAutoIncrement] = CustomFunction;
	    }

        /// <summary>
        /// Tells whether a Column is a Custom AutoIncrement one
        /// </summary>
        /// <param name="C"></param>
        /// <returns>true if Column is Custom Auto Increment</returns>
        static public bool IsCustomAutoIncrement(DataColumn C){
			if (C.ExtendedProperties[CustomAutoIncrement]!=null) return true;
			return false;
		}

		/// <summary>
		/// Removes Custom-autoincrement property from a DataColumn
		/// </summary>
		/// <param name="C"></param>
		static public void ClearCustomAutoIncrement(DataColumn C){
			C.ExtendedProperties[CustomAutoIncrement]=null;
		}

        #endregion


        #region AUTOINCREMENT FIELD MANAGEMENT
        /// <summary>
        /// Set to true if any custom autoincrement found. In that case, the transaction is runned row by row and not with batches
        /// </summary>
        public bool HasCustomAutoFields = false;

        /// <summary>
        /// Function called to evaluate a custom autoincrement column
        /// </summary>
        /// <param name="dr">DataRow evaluated</param>
        /// <param name="c">Column to evaluate</param>
        /// <param name="conn">Connection to database</param>
        /// <returns></returns>
        [Obsolete]
        public delegate object CustomCalcAutoID(DataRow dr, DataColumn c,DataAccess conn);


	    /// <summary>
	    /// Function called to evaluate a custom autoincrement column
	    /// </summary>
	    /// <param name="dr">DataRow evaluated</param>
	    /// <param name="c">Column to evaluate</param>
	    /// <param name="conn">Connection to database</param>
	    /// <returns></returns>
	    public delegate object CustomCalcAutoId(DataRow dr, DataColumn c, IDataAccess conn);


        /// <summary>
        /// Evaluates a value for all autoincremented key field of a row
        /// </summary>
        /// <param name="DR">DataRow to insert</param>
        /// <param name="Conn"></param>
        protected void CalcAutoID(DataRow DR,IDataAccess Conn){
			DR.BeginEdit();
			foreach (DataColumn C in DR.Table.Columns){
				if (IsAutoIncrement(C)){
					CalcAutoID(DR, C, Conn);
				}
			}            
			DR.EndEdit();
		}

        /// <summary>
        /// Tells PostData that the given table is optimized, i.e. autoincrement values have to be cached
        /// </summary>
        /// <param name="T"></param>
        /// <param name="isOptimized"></param>
        public static void SetOptimized(DataTable T, bool isOptimized) {
            if (T == null) return;
            if (!isOptimized) {
                T.ExtendedProperties["isOptimized"] = null;
                return;
            }
            if (T.ExtendedProperties["isOptimized"] != null) return;
            T.ExtendedProperties["isOptimized"] = new Dictionary<string,int>();
        }

        /// <summary>
        /// Clear all max expression cached on a table
        /// </summary>
        /// <param name="T"></param>
        public static void ClearMaxCache(DataTable T) {
            if (T == null) return;
            if (T.ExtendedProperties["isOptimized"] == null) return;
            T.ExtendedProperties["isOptimized"] = new Dictionary<string, int>();
        }

        /// <summary>
        /// Returns true if special optimization are applied in the autoincrement properties evaluation
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static bool IsOptimized(DataTable T) {
            if (T == null) return false;
            return (T.ExtendedProperties["isOptimized"] != null);
        }

        /// <summary>
        /// Sets the new maximum for a specified combination of table, expression and filter
        /// </summary>
        /// <param name="T"></param>
        /// <param name="expr">expression for the max</param>
        /// <param name="filter">filter applied</param>
        /// <param name="num">new maximum to set</param>
        public static void SetMaxExpr(DataTable T, string expr, string filter,int num) {
            if(!(T.ExtendedProperties["isOptimized"] is Dictionary<string, int> h)) return;
            h[expr + "§" + filter] = num;
        }

        /// <summary>
        /// Gets the optimized max() value for and expression in a table, with a specified filter and minimum value
        /// </summary>
        /// <param name="T">Evaluated table</param>
        /// <param name="expr">expression to evaluate</param>
        /// <param name="filter">filter to apply</param>
        /// <param name="minimum">minimum value wanted</param>
        /// <returns></returns>
        public static int GetMaxExpr(DataTable T, string expr, string filter,int minimum) {
            var h = T.ExtendedProperties["isOptimized"] as Dictionary<string, int>;
            string k=expr + "§" + filter;
            int res=minimum;
            if (h.ContainsKey(k)) {
                res= h[k];
            }
            h[k] = res + 1;
            return res;
        }
        
        /// <summary>
        /// Sets mininimum value for evaluating temporary autoincrement columns
        /// </summary>
        /// <param name="C"></param>
        /// <param name="min"></param>
        public static void setMinimumTempValue(DataColumn C,int min){
            C.ExtendedProperties["minimumTempValue"] = min;
        }

        static string MAX_SUBSTRING(DataRow R,
            string colname,
            int start,
            int len,
            string filter) {
                int minimum = 0;
                if (R.Table.Columns[colname].ExtendedProperties["minimumTempValue"] != null) {
                    minimum = Convert.ToInt32(R.Table.Columns[colname].ExtendedProperties["minimumTempValue"]);
                }
                DataTable T = R.Table;
                if (!IsOptimized(T)) {
                    int n = MAX_SUBSTRING_OLD(T, colname, start, len, filter);
                    if (n < minimum) n = minimum;
                    return n.ToString();
                }
                string expr = colname + "," + len;
                int res = GetMaxExpr(T, expr, filter,minimum);
                if (res > 0) return res.ToString();
                
            /*
             * 2/6/2014: ritengo sbagliata l'ottimizzazione sottostante, qui stiamo parlando di righe in memoria non su db
             *  pertanto l'essere la riga padre in stato di insert non dice nulla sui valori assunti dalle figlie in memoria,
             *  se sto chiamando questa funzione è perchè  voglio aumentare il valore del campo ad autoincremento per non avere
             *   conflitti IN MEMORIA 
            //Vede tutte le parent relation in cui sia contenuta una colonna chiave di R e che sia pure in selectors.
            // se trova una parent relation del genere, in cui il parent row sia uno, e in cui la colonna parent 
            //  sia un campo ad autoincremento per il parent, ed il parent è in stato di ADDED
            //  ALLORA
            //  PUO' EVITARSI LA SELECT e restituire direttamente 0
                DataColumn C = T.Columns[colname];
            List<DataColumn> selectors = RowChange.GetSelectors(C);
            foreach (DataColumn sel in selectors) {
                if (!QueryCreator.IsPrimaryKey(T, sel.ColumnName)) continue;
                foreach (DataRelation parRel in R.Table.ParentRelations) {
                    DataRow[] parRow = R.GetParentRows(parRel);
                    if (parRow.Length != 1) continue;
                    DataRow parent = parRow[0];
                    if (parent.RowState != DataRowState.Added) continue;
                    //vede il nome della colonna corrispondente al selettore sel, nel parent
                    DataColumn parentCol = null;
                    for (int i = 0; i < parRel.ChildColumns.Length; i++) {
                        if (parRel.ChildColumns[i] == sel) {
                            parentCol = parRel.ParentColumns[i];
                        }
                    }
                    if (parentCol == null) continue;
                    if (!RowChange.IsAutoIncrement(parentCol)) continue;
                    return minimum.ToString(); //THIS IS THE CASE!!!!!
                }
            }
            */
            res =  MAX_SUBSTRING_OLD(T, colname, start, len, filter);
            if (res < minimum) res = minimum;
            SetMaxExpr(T, expr, filter, res+1);
            return res.ToString();
        }



        static int MAX_SUBSTRING_OLD(DataTable T,
            string colname,
            int start,
            int len,
            string filter) {


            string MAX = null;
            int maxsub = 0;
            if (start == 0 && len == 0) {
                object mm = T.Compute("MAX(" + colname + ")", filter);
                if (mm != null && mm != DBNull.Value) {
                    try {
                        maxsub = Convert.ToInt32(mm);
                        MAX = maxsub.ToString();
                    }
                    catch { }
                }

            }
            else {
                var filteredRows = T.Select(filter);
                foreach (var r in filteredRows) {
                    //if (R.RowState == DataRowState.Deleted) continue;
                    //if (R.RowState == DataRowState.Detached) continue;
                    string s = r[colname].ToString();
                    if (s.Length <= start) continue;
                    int thislen = len;
                    if (thislen == 0) thislen = s.Length - start;
                    if (start + thislen > s.Length) thislen = s.Length - start;
                    string substr = s.Substring(start, thislen);
                    if (MAX == null) {
                        MAX = substr;
                        try {
                            maxsub = Convert.ToInt32(MAX);
                        }
                        catch {
                            // ignored
                        }
                    }
                    else {
                        int xx = maxsub - 1;
                        try {
                            xx = Convert.ToInt32(substr);
                        }
                        catch {
                            // ignored
                        }

                        //if (substr.CompareTo(MAX)>0) MAX=substr;
                        if (xx > maxsub) maxsub = xx;
                    }
                }
            }
            DataRow[] filteredDeletedRows = T.Select(filter, null, DataViewRowState.Deleted);
            foreach (var r in filteredDeletedRows) {
                //if (R.RowState != DataRowState.Deleted) continue;
                string s = r[colname, DataRowVersion.Original].ToString();
                if (s.Length <= start) continue;
                int thislen = len;
                if (thislen == 0) thislen = s.Length - start;
                if (start + thislen > s.Length) thislen = s.Length - start;
                string substr = s.Substring(start, thislen);
                if (MAX == null) {
                    MAX = substr;
                    try {
                        maxsub = Convert.ToInt32(MAX);
                    }
                    catch {
                        // ignored
                    }
                }
                else {
                    int xx = maxsub - 1;
                    try {
                        xx = Convert.ToInt32(substr);
                    }
                    catch {
                        // ignored
                    }

                    //if (substr.CompareTo(MAX)>0) MAX=substr;
                    if (xx > maxsub) maxsub = xx;
                }

            }

            return maxsub;
        }


        /// <summary>
        /// Evaluate a temporary column of a DataRow
        /// </summary>
        /// <param name="R"></param>
        /// <param name="C"></param>
		public static void CalcTemporaryID(DataRow R, DataColumn C){
			CalcTemporaryID(R.Table, R, C);
		}


		/// <summary>
		/// Evaluates a temporary value for a field of a row, basing on AutoIncrement 
		///  properties of the column, without reading from DB.
		/// </summary>
		/// <param name="T"></param>
		/// <param name="R"></param>
		/// <param name="C"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
		public static void CalcTemporaryID(DataTable T, DataRow R, DataColumn C){
            var QHC = new CQueryHelper();
			string Prefix="";
            if((C.ExtendedProperties[PrefixField] != null) &&
                (C.ExtendedProperties[PrefixField] != DBNull.Value)) {
                Prefix += R[C.ExtendedProperties[PrefixField].ToString()].ToString();
            }
            if ((C.ExtendedProperties[MiddleConst]!=null)&&
				(C.ExtendedProperties[MiddleConst]!=DBNull.Value))  {
				Prefix+= C.ExtendedProperties[MiddleConst].ToString();
			}
			int idSize=7;//default
            int totPrefsize = Prefix.Length;
            

            if ((C.ExtendedProperties[IDLength]!=null) &&
				(C.ExtendedProperties[IDLength]!=DBNull.Value)){
				idSize= Convert.ToInt32(C.ExtendedProperties[IDLength].ToString());
			}

            if ((C.DataType == typeof(int)) || (C.DataType == typeof(short)) || (C.DataType == typeof(long))) {
                if (totPrefsize == 0) idSize = 0;
            }

            string Selection = GetSelector(R,C,QHC);
			string newIdvalue="1";

			if (C.ExtendedProperties[LinearField]!=null){
				string filter=null;
				if (Selection!="") {
					filter = Selection;
				}

				string MAX = MAX_SUBSTRING(R, C.ColumnName,totPrefsize,idSize, filter);

				if (MAX!=null){
					int intFOUND2 = 0;
					try {
						intFOUND2 = Convert.ToInt32(MAX);
					}
				    catch {
				        // ignored
				    }

				    intFOUND2 += 1;
					newIdvalue = intFOUND2.ToString();
				}
			}
			else {
				string SelCmd = $"MAX(CONVERT({C.ColumnName},'System.Int32'))";
				string filter2="";
				if (Prefix!="") filter2 = $"(CONVERT({C.ColumnName},'System.String') LIKE '{Prefix}%') ";
				if (Selection!="") {
					if (filter2!="") filter2 += " AND ";
					filter2 += Selection;
				}

				object MAXv2 =  MAX_SUBSTRING(R,C.ColumnName,Prefix.Length,idSize,filter2);
						//T.Compute(SelCmd, filter2);
				string MAX2=null;
				if ((MAXv2!=null)&&(MAXv2!=DBNull.Value)) MAX2 = MAXv2.ToString();

				if (MAX2!=null){
					string foundSubstr=MAXv2.ToString();

					int intFound=0;
					if (foundSubstr!=""){
						try {
							intFound = Convert.ToInt32(foundSubstr);
						}
					    catch {
					        // ignored
					    }
					}
					intFound += 1;
					newIdvalue = intFound.ToString();
				}
			}

            string NEWID;
            if(idSize != 0) {
                NEWID = Prefix + newIdvalue.PadLeft(idSize, '0');
            }
            else {
                NEWID = Prefix + newIdvalue;
            }

            object oo = NEWID;
			if (C.DataType== typeof(int)) oo = Convert.ToInt32(oo);

			//Applies changes to CHILD rows of R (only necessary while resolving conflicts in POST)
			if (!oo.Equals(R[C.ColumnName])) {
                //Non cerca di riassegnare i figli quando il vecchio valore era stringa vuota, altrimenti capita che 
                // quelli senza parent (ossia il root) gli vengano assegnati come figli
                //if (R[C.ColumnName].ToString()!="") 
	                Cascade_Change_Field(R, C, oo);
				R[C.ColumnName]=oo;
			}
			
		}


        /// <summary>
        /// Evaluates all temporary columns of a row
        /// </summary>
        /// <param name="r"></param>
		public static void CalcTemporaryID(DataRow r){
			CalcTemporaryID(r.Table, r);
		}

		/// <summary>
		/// Evaluates temporary values for autoincrement columns  (reading from memory)
		/// </summary>
        /// <param name="T"></param>
		/// <param name="r"></param>
		/// <remarks>This function should be called when a row is added to a table, 
		///   between DataTable.NewRow() and DataTable.Rows.Add()
		///  </remarks>
		public static void CalcTemporaryID(DataTable T,  DataRow r){
			r.BeginEdit();
			foreach (DataColumn c in T.Columns){
				if (IsAutoIncrement(c)){
					CalcTemporaryID(T, r, c);
				}
			}            
			r.EndEdit();
		}

		/// <summary>
		/// Evaluates a value for a specified key field of a row (reading from db)
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="dr">DataRow to insert</param>
		/// <param name="c">Column to evaluate ID</param>
		/// <remarks>The function takes some parameter from DataColumn ExtendedProperties:
		/// PrefixField = Row Field to consider as a prefix for the ID value
		/// MiddleConst = Constant to append to PrefixField 
		/// IDLength    = Length of automatic calculated ID
		/// </remarks>
		protected void CalcAutoID(DataRow dr, DataColumn c, IDataAccess conn){
			var prefix="";
			object newid="";
            var qhs = conn.GetQueryHelper();

			if (!IsCustomAutoIncrement(c)){
				if ((c.ExtendedProperties[PrefixField]!=null)&&
					(c.ExtendedProperties[PrefixField]!=DBNull.Value)){
					prefix+= dr[c.ExtendedProperties[PrefixField].ToString()].ToString();
				}
				if ((c.ExtendedProperties[MiddleConst]!=null)&&
					(c.ExtendedProperties[MiddleConst]!=DBNull.Value))  {
					prefix+= c.ExtendedProperties[MiddleConst].ToString();
				}
				int idSize=0;
				if ((c.ExtendedProperties[IDLength]!=null) &&
					(c.ExtendedProperties[IDLength]!=DBNull.Value)){
					idSize= Convert.ToInt32(c.ExtendedProperties[IDLength].ToString());
				}
				int totPrefsize=prefix.Length;

				string Selection =GetSelector(dr,c,qhs);


                if((c.DataType == typeof(int)) || (c.DataType == typeof(short)) || (c.DataType == typeof(long))) {
                    if(totPrefsize == 0)
                        idSize = 0;
                }


                string expr2;
                if((c.DataType == typeof(int)) || (c.DataType == typeof(short)) || (c.DataType == typeof(long))) {
                    expr2 = $"MAX({c.ColumnName})";
                }
                else
                    expr2 = $"MAX(CONVERT(int,{c.ColumnName}))";

                if ((totPrefsize>0)||(idSize>0)){ //C'è da fare il substring
					int idToextr= idSize;
					if (idToextr==0) idToextr=12;
					string colnametoconsider=c.ColumnName;
					if (c.DataType!=typeof(String)) 
						colnametoconsider= $"CONVERT(VARCHAR(300),{c.ColumnName})";
					expr2	  = $"MAX(CONVERT(int,SUBSTRING({colnametoconsider},{(totPrefsize+1)},{idToextr})))";
				}
				if (c.ExtendedProperties[LinearField]==null){
					string filter2="";
					if (prefix!="") filter2 = $"({c.ColumnName} LIKE '{prefix}%') ";
					Selection= GetData.MergeFilters(Selection,filter2);
				}
                string unaliased = DataAccess.GetTableForReading(dr.Table);

                object result2;
                if (myCollection != null) 
                    result2 = myCollection.getMax(dr,c, conn, unaliased, Selection, expr2);
                else
                    result2 = conn.DO_READ_VALUE(unaliased, Selection, expr2);


                string newIDVALUE;
                if((result2 == null) || (result2 == DBNull.Value)) {
                    newIDVALUE = "1";
                }
                else {
                    var foundId = result2.ToString();
                    var intFound2 = 0;
                    try {
                        intFound2 = Convert.ToInt32(foundId);
                    }
                    catch {
                        // ignored
                    }

                    intFound2 += 1;
                    newIDVALUE = intFound2.ToString();
                }

                myCollection?.SetMax(unaliased, Selection, expr2, newIDVALUE);

			    if (idSize!=0){
					newid = prefix+newIDVALUE.PadLeft(idSize, '0');
				}
				else {
					newid = prefix+newIDVALUE;
				}
                

			}
			else {
#pragma warning disable 612
                if(c.ExtendedProperties[CustomAutoIncrement] is CustomCalcAutoID fun) {
#pragma warning restore 612
                    newid = fun(dr, c, conn as DataAccess);
                    HasCustomAutoFields = true;
                }
                else {
                    if(c.ExtendedProperties[CustomAutoIncrement] is CustomCalcAutoId funNew) {
                        newid = funNew(dr, c, conn);
                        HasCustomAutoFields = true;
                    }
                }

            }


			var temp = dr.Table.NewRow();
			foreach (DataColumn CC in dr.Table.Columns) temp[CC]= dr[CC];
            //if(C.AllowDBNull && NEWID == "") {
            //    Temp[C] = DBNull.Value;
            //}
            //else {
            //    Temp[C] = NEWID;
            //}
            temp[c] = newid;

            if (!IsOptimized(dr.Table)) {
	            var keyfilter = q.keyCmp(temp);
								//QueryCreator.WHERE_KEY_CLAUSE(temp, DataRowVersion.Default, false);
                var found = dr.Table._Filter(keyfilter);	//.Select(keyfilter);
                foreach (var rfound in found) {
                    if (rfound == dr) continue;
                    CalcTemporaryID(rfound);
                }
            }

			object oo = newid;
			if (c.DataType== typeof(int)&& (oo!=DBNull.Value)) oo = Convert.ToInt32(oo);


			//Applies changes to CHILD rows of R
			if (!oo.Equals(dr[c])) {
				Cascade_Change_Field(dr, c, oo);
				dr[c]=oo;
			}
			

		}



		#endregion

		#region Recursive operations (field change / row delete) 


		/// <summary>
		/// Changes R's child rows to reflect variation of R[ColumnToChange]= newvalue
		/// </summary>
		/// <param name="R"></param>
		/// <param name="ColumnToChange"></param>
		/// <param name="newvalue"></param>
		public static void Cascade_Change_Field(DataRow R, DataColumn ColumnToChange, object newvalue){
			foreach(DataRelation Rel in R.Table.ChildRelations){
				//checks if Rel includes "ColumnToChange" column of R
				for (int i=0; i< Rel.ChildColumns.Length; i++){
					DataColumn C = Rel.ParentColumns[i];
					if (C.ColumnName==ColumnToChange.ColumnName){
						DataColumn ChildColumnToChange = Rel.ChildColumns[i];
						foreach(DataRow ChildRow in R.iGetChildRows(Rel)) {
							if (R.RowState == DataRowState.Deleted) continue;
							Cascade_Change_Field(ChildRow, ChildColumnToChange, newvalue);
							ChildRow[ChildColumnToChange.ColumnName]= newvalue;
						}
					}
				}
			}
		}
        

		/// <summary>
		/// Deletes DataRow R and all it's sub-entities
		/// </summary>
		/// <param name="toDelete"></param>
		public  static void ApplyCascadeDelete(List <DataRow> toDelete) {
			if (toDelete.Count == 0) return;
			var childForTables = new Dictionary<string, List<DataRow>>();
			
	        //return iManager?.getChildRows(rParent, rel)
			//

			DataTable Parent = toDelete[0].Table;
			var iManager = Parent.DataSet?.getIndexManager();
			var rowForTable = new Dictionary<string, int>();
			foreach (DataRelation Rel in Parent.ChildRelations) {
				DataTable Child = Rel.ChildTable;
				if (!rowForTable.TryGetValue(Child.TableName, out int nRow)) {
					nRow = Child.Select().Length;
					rowForTable[Child.TableName] = nRow;
				}

				if (nRow == 0) continue;
                //Cancella le figlie nella tabella Child
                var isSubRel = model.isSubEntityRelation(Rel);
				foreach (DataRow R in toDelete) {
					DataRow[] ChildRows = iManager?.getChildRows(R, Rel) ?? R.GetChildRows(Rel);
					int nChilds = ChildRows.Length;
					if (nChilds == 0) continue;
					if (isSubRel) {
						if (!childForTables.TryGetValue(Child.TableName, out var list)){
							list = new List<DataRow>();
							childForTables[Child.TableName] = list;
						}

						list.AddRange(ChildRows);
						nRow -= nChilds;
						rowForTable[Child.TableName] = nRow;
						if (nRow == 0) break;
					}
					else {
						foreach (DataRow RChild in ChildRows) {
							//if (RChild.RowState== DataRowState.Deleted) continue;
							for (int i = 0; i < Rel.ChildColumns.Length; i++) {
								DataColumn CChild = Rel.ChildColumns[i];
								DataColumn CParent = Rel.ParentColumns[i];
								if (!CChild.AllowDBNull) continue;
								if (QueryCreator.IsPrimaryKey(RChild.Table, CChild.ColumnName)) continue;
								if (!QueryCreator.IsPrimaryKey(Parent, CParent.ColumnName)) continue;
								RChild[CChild.ColumnName] = DBNull.Value;
							}
						}
					}
				}

			}

			foreach (var list in childForTables.Values) {
				ApplyCascadeDelete(list);
			}

			staticModel.invokeActions(Parent,TableAction.beginLoad);

			//if (toDelete.Count > 2000) {
			//	int blockSize = 1000;
			//	int nBlocks = toDelete.Count / blockSize;
			//	var task = new Task[nBlocks];
			//	int last = 0;
			//	for (int i = 0; i < nBlocks; i++) {
			//		int min = last;
			//		int max = last + blockSize-1;
			//		if (i == nBlocks - 1) max = toDelete.Count - 1;
			//		task[i] =  Task.Run(() => deleteAsync(toDelete,min,max));
			//		last = max + 1;
                    
			//	}

			//	Task.WaitAll(task);
			//}
			//else {
				foreach (DataRow R in toDelete) {
					R.Delete();
				}
			//}

			staticModel.invokeActions(Parent,TableAction.endLoad);

		}
		private static IMetaModel staticModel = MetaFactory.factory.getSingleton<IMetaModel>();

		static void deleteAsync(List<DataRow> l, int start, int stop) {
			for (int j = start; j <= stop; j++) l[j].Delete();
		}
		static IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();

		/// <summary>
		/// Deletes DataRow R and all it's sub-entities
		/// </summary>
		/// <param name="r"></param>
		public static void ApplyCascadeDelete (DataRow r) {
			int handle = mdl_utils.metaprofiler.StartTimer("ApplyCascadeDelete");
			ApplyCascadeDelete(new List<DataRow>(){r});

            mdl_utils.metaprofiler.StopTimer(handle);
            
		}


		///// <summary>
		///// Undo a stack of deletions
		///// </summary>
		///// <param name="RollBack"></param>
		//public static void RollBackDeletes (Stack RollBack){
		//	while (RollBack.Count>0){
		//		DataRow R = (DataRow) RollBack.Pop();
		//		R.RejectChanges(); //if it was added-deleted---> ??
		//	}
		//}

		#endregion

        /// <summary>
        /// Change the Child Row fields in order to make it child of Parent. All parent-child relations between the two tables are taken into account.
        /// </summary>
        /// <param name="parent">Parent row</param>
        /// <param name="parentTable"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool MakeChild(DataRow parent,
        DataTable parentTable,
        DataRow child
        ) {
            var madechild = false;
            foreach(DataRelation rel in child.Table.ParentRelations) {
                if(MakeChild(parent, parentTable, child, rel.RelationName)) madechild = true;
            }
            return madechild;            
        }

        /// <summary>
        /// Makes a "Child" DataRow related as child with a Parent Row. 
        ///     This function should be called after calling DataTable.NewRow and
        /// before calling CalcTemporaryID and DataTable.Add()
        /// </summary>
        /// <param name="parent">Parent Row (Can be null)</param>
        /// <param name="parentTable">Parent Table (to which Parent Row belongs)</param>
        /// <param name="child">Row that must become child of Parent (can't be null)</param>
        /// <param name="relname">eventually name of relation to use</param>
        /// <remarks>This function should be called after calling DataTable.NewRow and
        ///         before calling CalcTemporaryID and DataTable.Add()
        /// </remarks>
        public static bool MakeChild(DataRow parent, 
				DataTable parentTable, 
				DataRow child, 
				string relname){
            if(relname == null) return MakeChild(parent, parentTable, child);
			var rel = ChildRelation(parentTable, child.Table, relname);
			if (rel==null) return false;
			for (var i=0; i< rel.ParentColumns.Length; i++){
				var childCol = rel.ChildColumns[i];
				if (parent!=null){
					child[childCol.ColumnName]= parent[rel.ParentColumns[i].ColumnName];					
				}
				else {
                    child[childCol] = QueryCreator.clearValue(childCol);
				}
			}
			return true;
		}

		/// <summary>
		/// Search a Relation in Child's Parent Relations that connect Child to Parent, 
		///		named relname. If it is not found, it is also searched in Parent's
		///		child relations.
		/// </summary>
		/// <param name="parent">Parent table</param>
		/// <param name="child">Child table</param>
		/// <param name="relname">Relation Name, null if it does not matter</param>
		/// <returns>a Relation from Child Parent Relations, or null if not found</returns>
		public static DataRelation ChildRelation(DataTable parent, 
					DataTable child, 
					string relname){
			foreach (DataRelation rel in child.ParentRelations){
				if ((relname!=null)&&(rel.RelationName!=relname))continue;
				if (rel.ParentTable.TableName== parent.TableName){
					return rel;
				}
			}
			foreach (DataRelation rel2 in parent.ChildRelations){
				if ((relname!=null)&&(rel2.RelationName!=relname))continue;
				if (rel2.ChildTable.TableName== child.TableName){
					return rel2;
				}
			}

			return null;
		}


        /// <summary>
        /// Get list of relations where table Parent is the ParentTable and Child is the ChildTable
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static List<DataRelation> FindChildRelation(DataTable parent,
                    DataTable child) {
            List<DataRelation> relList = new List<DataRelation>();
            foreach (DataRelation rel in child.ParentRelations) {
                //if ((relname != null) && (Rel.RelationName != relname)) continue;
                if (rel.ParentTable.TableName == parent.TableName) {
                    relList.Add(rel);
                }
            }
            //foreach (DataRelation Rel2 in Parent.ChildRelations) {
            //    //if ((relname != null) && (Rel2.RelationName != relname)) continue;
            //    if (Rel2.ChildTable.TableName == Child.TableName) {
            //        return Rel2;
            //    }
            //}

            return relList;
        }

        /// <summary>
        /// Evaluates autoincrement values, completes the row to be changed with createuser,createtimestamp,
        /// lastmoduser, lastmodtimestamp fields, depending on the operation type
        ///  and calls CalculateFields for each DataRow involved.
        ///  This must be done INSIDE the transaction.
        /// </summary>
        /// <param name="user">User who is posting</param>
        /// <param name="acc"></param>
        /// <param name="doCalcAutoId"></param>
        [Obsolete]
        public virtual void PrepareForPosting(string user,DataAccess acc,bool doCalcAutoId){
			//SqlDateTime Stamp = new SqlDateTime(System.DateTime.Now);
			var stamp = DateTime.Now;

			switch (ShortStatus()){
				case short_insert_descr:
					if (doCalcAutoId) CalcAutoID(DR, acc);
					if (DR.Table.Columns["createuser"]!=null) DR["createuser"] =user;                    
					if (DR.Table.Columns["createtimestamp"]!=null) DR["createtimestamp"] = stamp;
					if (DR.Table.Columns["lastmoduser"]!=null) DR["lastmoduser"] = $"\'{user}\'";
					if (DR.Table.Columns["lastmodtimestamp"]!=null) DR["lastmodtimestamp"] = stamp;
					break;
				case short_update_descr:
					if (DR.Table.Columns["lastmoduser"]!=null){
						DR["lastmoduser"] = $"\'{user}\'";
					}
					if (DR.Table.Columns["lastmodtimestamp"]!=null){
						DR["lastmodtimestamp"] = stamp;
					}
					break;
				case short_delete_descr:
					//nothing to do!
					break;
			}                        
			try {
				GetData.CalculateRow(DR);
			}
			catch (Exception E) {
			    acc.LogError($"PrepareForPosting({DR.Table.TableName})", E);
			}
		}

	    /// <summary>
	    /// Evaluates autoincrement values, completes the row to be changed with createuser,createtimestamp,
	    /// lastmoduser, lastmodtimestamp fields, depending on the operation type
	    ///  and calls CalculateFields for each DataRow involved.
	    ///  This must be done INSIDE the transaction.
	    /// </summary>
	    /// <param name="user">User who is posting</param>
	    /// <param name="acc"></param>
	    /// <param name="doCalcAutoId"></param>
	    public virtual void prepareForPosting(string user, IDataAccess acc, bool doCalcAutoId) {
	        //SqlDateTime Stamp = new SqlDateTime(System.DateTime.Now);
	        var stamp = DateTime.Now;

	        switch (ShortStatus()) {
	            case short_insert_descr:
	                if (doCalcAutoId) CalcAutoID(DR, acc);
	                if (DR.Table.Columns["createuser"] != null) DR["createuser"] = user;
	                if (DR.Table.Columns["createtimestamp"] != null) DR["createtimestamp"] = stamp;
	                if (DR.Table.Columns["lastmoduser"] != null) DR["lastmoduser"] = $"\'{user}\'";
	                if (DR.Table.Columns["lastmodtimestamp"] != null) DR["lastmodtimestamp"] = stamp;
	                break;
	            case short_update_descr:
	                if (DR.Table.Columns["lastmoduser"] != null) {
	                    DR["lastmoduser"] = $"\'{user}\'";
	                }
	                if (DR.Table.Columns["lastmodtimestamp"] != null) {
	                    DR["lastmodtimestamp"] = stamp;
	                }
	                break;
	            case short_delete_descr:
	                //nothing to do!
	                break;
	        }
	        try {
	            GetData.CalculateRow(DR);
	        }
	        catch (Exception e) {
	            acc.LogError($"PrepareForPosting({DR.Table.TableName})", e);
	        }
	    }

    }

	/// <summary>
	/// Collection of RowChange
	/// </summary>
	public class RowChangeCollection : System.Collections.ArrayList {
        /// <summary>
        /// Connection to use, used in derived classes
        /// </summary>
	    public IDataAccess connectionToUse;
        /// <summary>
        /// Total number of deleted rows
        /// </summary>
	    public int nDeletes = 0;

        /// <summary>
        /// Total number of updated rows
        /// </summary>
	    public int nUpdates = 0;

        /// <summary>
        /// Total number of added rows
        /// </summary>
	    public int nAdded = 0;

        private  Dictionary<string, string> AllMax = new Dictionary<string, string>();
        internal bool is_temporaryCollection = false;

        /// <summary>
        /// Azzera  i massimi presenti nella cache di transazione
        /// </summary>
        public void EmptyCache() {
            AllMax.Clear();
        }

    

        private string getHash(string table, string filter, string expr){
            return table+"§"+filter+"§"+expr;
        }

        /// <summary>
        /// Evaluates the maximum value for an expression referring to a Column of a DataRow, 
        /// </summary>
        /// <param name="R">DataRow being calculated</param>
        /// <param name="C">DataColumn to evaluate</param>
        /// <param name="conn">Connection</param>
        /// <param name="table">table implied</param>
        /// <param name="filter">Filter for evaluating the expression</param>
        /// <param name="expr">expression to evaluate</param>
        /// <returns></returns>
        internal string getMax(DataRow R, DataColumn C, IDataAccess conn, string table, string filter, string expr){
            string k = getHash(table,filter,expr).ToUpper();
            if (AllMax.ContainsKey(k)) return AllMax[k];
            
            DataTable T = C.Table;
            //Vede tutte le parent relation in cui sia contenuta una colonna chiave di R e che sia pure in selectors.
            // se trova una parent relation del genere, in cui il parent row sia uno, e in cui la colonna parent 
            //  sia un campo ad autoincremento per il parent, ed il parent è in stato di ADDED
            //  ALLORA
            //  PUO' EVITARSI LA SELECT e restituire direttamente null
            List<DataColumn> selectors = RowChange.GetSelectors(C);
            foreach (DataColumn sel in selectors) {
                if (!QueryCreator.IsPrimaryKey(T, sel.ColumnName)) continue;
                foreach (DataRelation parRel in R.Table.ParentRelations) {
                    DataRow[] parRow = R.iGetParentRows(parRel);
                    if (parRow.Length != 1) continue;
                    DataRow parent = parRow[0];
                    if (parent.RowState != DataRowState.Added) continue;
                    //vede il nome della colonna corrispondente al selettore sel, nel parent
                    DataColumn parentCol = null;
                    for (int i = 0; i < parRel.ChildColumns.Length; i++) {
                        if (parRel.ChildColumns[i] == sel) {
                            parentCol = parRel.ParentColumns[i];
                        }
                    }
                    if (parentCol == null) continue;
                    if (!RowChange.IsAutoIncrement(parentCol)) continue;
                    return null; //THIS IS THE CASE!!!!!
                }
            }
           
                


            object res = conn.DO_READ_VALUE(table, filter, expr);
            if (res == null || res == DBNull.Value) return null;
            AllMax[k] = res.ToString();
            return res.ToString();
        }

        /// <summary>
        /// Sets the max value for a specific combination
        /// </summary>
        /// <param name="table">table to set the max value</param>
        /// <param name="filter">filter connected (usually a bunch of selector and static filters)</param>
        /// <param name="expr">expression that the max value refers to</param>
        /// <param name="value">value to set as new maximum</param>
        public void SetMax(string table, string filter, string expr, string value){
             string k = getHash(table,filter,expr).ToUpper();
            AllMax[k]=value;
        }

		internal void Add(RowChange C) {
			base.Add(C);
		    if (C.DR.RowState == DataRowState.Deleted) nDeletes++;
		    if (C.DR.RowState == DataRowState.Modified) nUpdates++;
		    if (C.DR.RowState == DataRowState.Added) nAdded++;

            if (!is_temporaryCollection) C.SetCollection(this);
		}
        
		/// <summary>
		/// Gets the RowChange in the specified Table
		/// </summary>
		/// <param name="TableName">Name of the DataTable where the related row is to be found</param>
		/// <returns>The table-related row in the collection</returns>
        public RowChange GetByName(string TableName) {
            foreach (RowChange R in this) {
                if (R.TableName == TableName) return R;
            }

            foreach (RowChange R in this) {
                if (R.Table.tableForPosting() == TableName) return R;
            }
            foreach (RowChange R in this) {
                if (R.Table.tableForReading() == TableName) return R;
            }

            //Tablename was not found. Try searching in tablename+view			
            foreach (RowChange R in this) {
                if (R.TableName == TableName + "view") return R;
            }

            return null;
        }
    }

	/// <summary>
	/// Class that manages log
	/// </summary>
	public class DataJournaling{
		/// <summary>
		/// Should return the log rows to add to db 
		/// for a given set of changes that have been made to DB
		/// </summary>
		/// <param name="Changes"></param>
		/// <returns></returns>
		virtual public DataRowCollection DO_Journaling(RowChangeCollection Changes){
			return null;
		}
	}



	/// <summary>
	/// Configurable String Parser: 
	/// It is able to find occurencies of strings with predefined delimiters,
	///  giving the string found and the string type (referring to the delimiters)
	/// </summary>
	public class MsgParser {
		String Message;
		int next_position;
		String[] StartString;
		String[] StopString;
		int NumString;
        
		/// <summary>
		/// Create a Parser able to recognize more than one Start/Stop Tag
		/// </summary>
		/// <param name="message">Message to Parse</param>
		/// <param name="start">array of Start tags</param>
		/// <param name="stop">array of (corresponding) Stop Tags</param>
		public MsgParser(String message, String[] start, String[] stop){
			this.Message= message;
			this.NumString = start.Length;
			StartString = start;
			StopString  = stop;
			next_position=0;
		}
        
		/// <summary>
		/// Create a Parser able to recognize one Start/Stop Tag
		/// </summary>
		/// <param name="Message">Message to Parse</param>
		/// <param name="Start">Start tag</param>
		/// <param name="Stop">Stop Tag</param>
		public MsgParser(String Message, String Start, String Stop){
			this.Message= Message;
			this.NumString = 1;
			StartString = new String[]  {Start};
			StopString  = new String[]  {Stop};
			Reset();
		}
                
		/// <summary>
		/// Reset the parser to the beginning of the string
		/// </summary>
		public void Reset() {
			next_position=0;

		}
        
		/// <summary>
		///   Find the next occurrence delimited by the defined tags
		///   This function DOES NOT allow nested tags.
		/// </summary>
		/// <param name="Found" type="output">String found between delimiters</param>
		/// <param name="Skipped" type="output">String found before the first delimiter</param>
		/// <param name="Kind">Index of the delimiters</param>
		/// <returns>true when an occurrence is found</returns>
		public bool GetNext(out String Found, out String Skipped, out int Kind){
			if (next_position >= Message.Length) {
				Found="";
				Skipped="";
				Kind=-1;
				return false;
			}
            
			int found_at=-1;   //not found
			int after_end_tag=-1;
			int next_at=-1;
			int found_kind=-1;
			int new_start=-1;
			int end_tag;
			for (int i=0; i<NumString; i++){
				int curr_found_at =Message.IndexOf(StartString[i],next_position);
                
				if (curr_found_at >=0){ //checks for the presence of the end tag
					new_start = curr_found_at + StartString[i].Length;
					end_tag = Message.IndexOf(StopString[i], new_start);
					if (end_tag == -1) 
						curr_found_at=-1; //aborts the element
					else
						after_end_tag= end_tag+StopString[i].Length;
				}

				if (curr_found_at >=0){

					if ((found_at==-1) || (found_at>curr_found_at)){
						found_at = curr_found_at;
						found_kind=i;
						next_at=after_end_tag;
					}
				}
			}

			if (found_at>=0){ //string was found
				int len= next_at-StopString[found_kind].Length-new_start;
				Found = Message.Substring(new_start, len);
				Skipped = Message.Substring(next_position, found_at-next_position);
				Kind = found_kind;
				next_position = next_at;
				return true;
			}
			else { //string was not found
				Kind=-1;
				Found="";
				Skipped=Message.Substring(next_position);
				next_position= Message.Length;
				return false;
			}
		} 

		/// <summary>
		///   Find the next occurrence delimited by the defined tags
		///   This function DOES NOT allow nested tags.
		/// </summary>
		/// <param name="Found">String found between delimiters</param>
		/// <param name="Skipped">String found before the first delimiter</param>
		/// <returns>true when an occurrence is found</returns>
		public bool GetNext(out String Found, out String Skipped){
            return GetNext(out Found, out Skipped, out var Kind);
        }

    
	}


	/// <summary>
	/// Fills the field EnforcementRule (ProcedureMessageCollection) of any RowChange in Cs
	/// </summary>
	public class MetaDataRules {
		/// <summary>
		/// Query the business logic to get a binary representation of a list
		///  of error messages to be shown to the user. 
		/// </summary>
		/// <param name="R">Change to scan for messages</param>
		/// <param name="result">Array in which every row represents the need
		///  to display a corresponding message</param>
		virtual public void DO_CALC_MESSAGES(RowChange R, bool[] result){
		}
	}


  


	/// <summary>
	/// Business Rule Error Message 
	/// </summary>
	public class ProcedureMessage {
		/// <summary>
		/// CanIgnore is true if user is allowed to ignore the message. False if error is Severe
		/// </summary>
		public bool CanIgnore;
        
        /// <summary>
        /// True if it's a post-check, false if it is a pre-check
        /// </summary>
        public bool PostMsgs;
		/// <summary>
		/// Business Rule Error Message Text
		/// </summary>
		public String LongMess;

		/// <summary>
		/// Gets a Key that makes the message unique in the RowChange.EnforceMessages List 
		/// </summary>
		/// <returns>RuleID@@@EnforcementID</returns>
		public virtual string GetKey(){
			
			return LongMess; //RuleID + "@@@"  + EnforcementNumber;
		}

	}


	/// <summary>
	/// Collection of messages to be displayed to the user
	/// </summary>
	public class ProcedureMessageCollection : System.Collections.ArrayList{
		/// <summary>
		/// CanIgnore is True if Messages are Warning and can be ignored by the user
		///		(there are no Severe Errors)
		/// </summary>
		public bool CanIgnore=true;

		/// <summary>
		/// PostMsgs is true is Messages are "Post-Messages", i.e. generated after posting all changes to DB
		/// </summary>
		public bool PostMsgs=false;

		/// <summary>
		/// Show messages to user and return true if he decided to ignore them
		/// </summary>
		/// <returns>true if no messages or messages ignored</returns>
		public virtual bool ShowMessages(){
			return true;
		}

        /// <summary>
        /// Append a db unrecoverable error  in the list of messages
        /// </summary>
        /// <param name="message"></param>
		public virtual void AddDBSystemError(string message){
            var P = new ProcedureMessage {
                CanIgnore = false,
                LongMess = $"Errore nella scrittura sul DB.{message}"
            };
            Add(P);
		}
	    /// <summary>
	    /// Append a db recoverable error  in the list of messages
	    /// </summary>
	    /// <param name="message"></param>
	    public virtual void AddWarning(string message){
            var P = new ProcedureMessage {
                CanIgnore = true,
                LongMess = message
            };
            Add(P);
	    }
		/// <summary>
		/// Gets the Message at a specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public ProcedureMessage GetMessage(int index){
			return (ProcedureMessage)  this[index];
		}

        /// <summary>
        /// evaluates CanIgnore flag
        /// </summary>
	    public void recalcIgnoreFlag() {
	        CanIgnore = true;
	        foreach (ProcedureMessage r in this) {
	            if (r == null) continue;
	            if (!r.CanIgnore) CanIgnore = false;
	        }
	    }


		/// <summary>
		/// Adds a message to the list, updating CanIgnore status
		/// </summary>
		/// <param name="Msg"></param>
		public virtual void Add(ProcedureMessage Msg) {
			base.Add(Msg);
            Msg.PostMsgs = this.PostMsgs;
			if (!Msg.CanIgnore) CanIgnore = false;
		}

        /// <summary>
        ///  Appends a list of messages to this
        /// </summary>
        /// <param name="otherList"></param>
        public virtual void Add(ProcedureMessageCollection otherList) {
            foreach (ProcedureMessage p in otherList) {
                base.Add(p);
                if (!p.CanIgnore) CanIgnore = false;
            }
        }

		/// <summary>
		/// Remove from this list every message in MsgToIgnore
		/// </summary>
		/// <param name="MsgToIgnore"></param>
		public void SkipMessages(Hashtable MsgToIgnore){
			ArrayList list = new ArrayList();
			foreach(ProcedureMessage PP in this){
				if (!PP.CanIgnore)continue;
				if (MsgToIgnore[PP.LongMess]!=null) list.Add(PP);
			}
			foreach (ProcedureMessage PP in list){
				Remove(PP);
			}
		}

        /// <summary>
        /// Sets an Hashtables with Messages to ignore with all messages contained in this collection
        /// </summary>
        /// <param name="MsgToIgnore">Hashtable containg all messages to ignore</param>
		public void AddMessagesToIgnore(Hashtable MsgToIgnore){
			foreach(ProcedureMessage PP in this){
				MsgToIgnore[PP.LongMess]=1;
			}
		}


	}

    /// <summary>
    /// Class used to nest posting of different datasets
    /// </summary>
    public interface InnerPosting {
        /// <summary>
        /// inner PostData class
        /// </summary>
        //PostData innerPostClass { get; }
        Hashtable hashMessagesToIgnore();

        /// <summary>
        /// Called to initialize the class, inside the transaction
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="conn"></param>
        void  initClass(DataSet ds, IDataAccess conn);


        /// <summary>
        /// Unisce i messaggi dati a quelli finali
        /// </summary>
        /// <param name="messages"></param>
        void mergeMessages(ProcedureMessageCollection messages);

        /// <summary>
        /// Called after data has been committed or rolled back
        /// </summary>
        /// <param name="committed"></param>
        void afterPost(bool committed);
        
        /// <summary>
        /// Reads all data about views (also in inner posting classes)
        /// </summary>
        void reselectAllViewsAndAcceptChanges();

        /// <summary>
        /// Get innerPosting class 
        /// </summary>
        /// <returns></returns>
        InnerPosting getInnerPosting();

        /// <summary>
        /// Set innerPosting mode with a set of already raised messages
        /// </summary>
        /// <param name="ignoredMessages"></param>
        void setInnerPosting(Hashtable ignoredMessages);

        /// <summary>
        /// Post inner data to db
        /// </summary>
        /// <returns></returns>
        ProcedureMessageCollection DO_POST_SERVICE();
    }

	//[Transaction(TransactionOption.Required)]
	/// <summary> 
	/// PostData manages updates to DB Tables
	/// Necessary pre-conditions are that:
	/// - DataBase Tables name are the same as DataSet DataTable names
	/// - Temporary (not belonging to DataBase) table are "marked" with the ExtendedProperty[IsTempTable]!=null
	/// </summary>	
	/// <remarks>
	/// If rows are added to datatable contains ID that have to be evaluated as max+1 from
	///  the database, additional information have to be put in the ID datacolumn:
	///  IsAutoIncrement = "s"    -   REQUIRED 
	///  PrefixField              -   optional 
	///  MiddleConst              -   optional 
	///  IDLength                 -   optional
	///  see RowChange for additional info
	/// </remarks>
	public class PostData {   //: ServicedComponent {

        

	    /// <summary>
	    /// Called to  invoke posting data. The assumption is that if it returns empty collection, data should be committed
	    /// </summary>
	    /// <param name="IgnoredMessages"></param>
	    /// <returns></returns>
	    public virtual ProcedureMessageCollection innerDoPostService(Hashtable IgnoredMessages) {
	        this.IgnoredMessages = IgnoredMessages;
	        return DO_POST_SERVICE();
	    }


        /// <summary>
        /// Returns an empty list of error messages
        /// </summary>
        /// <returns></returns>
		public virtual ProcedureMessageCollection GetEmptyMessageCollection(){
			return new ProcedureMessageCollection();
		}

        /// <summary>
        /// Connection of the main posting process
        /// </summary>
        [Obsolete]
        public DataAccess Conn;


        /// <summary>
        /// String Constant used to pass null parameter to stored procedures
        /// </summary>
        public const string NullParameter = "null";
		ProcedureMessageCollection ResultList=null;

		/// <summary>
		/// refresh dataset rows when update fails
		/// </summary>
		/// 
		public bool refresh_dataset;
        
		const string IsTempTable  = "IsTemporaryTable";

        /// <summary>
        /// Automatically discards non-blocking errors while saving
        /// </summary>
        public bool autoIgnore = false;

		// <summary>
		// true if Object is valid
		// </summary>
		//        public bool WellObject;

		

		string lasterror;

		/// <summary>
		/// Get last Error message and automatically clears it
		/// </summary>
		public string GetErrorMsg {
			get {
				string res = lasterror;
				lasterror="";
				return res;
			}
		}

        /// <summary>
        /// Manage the posting of a singleDataSet
        /// </summary>
        protected class singleDatasetPost {
            /// <summary>
            /// MetaModel used by the metadata
            /// </summary>
            public IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();

	        IDataAccess privateConn;
            /// <summary>
            /// Dataset posted from this class
            /// </summary>
	        public DataSet DS;
	        string user;
            /// <summary>
            /// List of rows changed in DS
            /// </summary>
            public RowChangeCollection RowChanges;

            /// <summary>
            /// Last error occurred in this posting class
            /// </summary>
            public string lasterror;



            #region Row Changes Sorting And Classifying

            /// <summary>
            ///  Evaluates the list of the changes to apply to the DataBase, in the order they
            ///  should be "reasonably" done:
            ///  All operation on None
            ///  Deletes on  Child 
            ///  Deletes on Both
            ///  Deletes on Parent
            ///  Insert, Update on Parent
            ///  Insert, Update on Both (in the evaluated list - reversed order )
            ///  Insert, Update on Child
            ///  Excluding all temporary table!
            /// </summary>
            /// <param name="Original">DataBase to be scanned for changes</param>
            /// <returns>List of changes to be done, in a reasonably good order</returns>
            RowChangeCollection ChangeList(DataSet Original) {
                var ParentFirst = new ArrayList(3);
                var ChildFirst = new ArrayList(3);
                var Result = new RowChangeCollection {
                    connectionToUse = privateConn
                };
                do_SortTables(Original, ParentFirst, ChildFirst);
                AddTablesOps(Result, ChildFirst, DataRowState.Deleted, false);
                AddTablesOps(Result, ParentFirst, DataRowState.Added, false);
                AddTablesOps(Result, ParentFirst, DataRowState.Modified, false);
                return Result;
            }

            void do_SortTables(DataSet D, ArrayList ParentFirst, ArrayList ChildFirst) {
                bool added = true;
                var Added = new Hashtable();
                while (added) {
                    added = false;
                    foreach (DataTable T in D.Tables) {
                        if (Added[T.TableName] != null) continue;
                        if (IsTemporaryTable(T)) continue;
                        if (CheckIsNotChild(T, Added)) {
                            ParentFirst.Add(T);
                            Added[T.TableName] = "1";
                            added = true;
                            continue;
                        }
                    }
                }
                Added = new Hashtable();
                added = true;
                while (added) {
                    added = false;
                    foreach (DataTable T in D.Tables) {
                        if (Added[T.TableName] != null) continue;
                        if (IsTemporaryTable(T)) continue;
                        if (CheckIsNotParent(T, Added)) {
                            ChildFirst.Add(T);
                            Added[T.TableName] = "1";
                            added = true;
                            continue;
                        }
                    }
                }
            }

            bool CheckIsNotChild(DataTable T, Hashtable ParentToIgnore) {
                if (T.ParentRelations.Count == 0) return true;
                foreach (DataRelation Rel in T.ParentRelations) {
                    DataTable ParentTable = Rel.ParentTable;
                    if (ParentTable.TableName == T.TableName) continue;
                    if (ParentToIgnore[ParentTable.TableName] != null) continue;
                    if (IsTemporaryTable(ParentTable)) continue;
                    foreach (DataRow RParent in ParentTable.Rows) {
                        if (RParent.RowState != DataRowState.Unchanged) return false;
                    }
                }
                return true;
            }

            bool CheckIsNotParent(DataTable T, Hashtable ChildToIgnore) {
                if (T.ChildRelations.Count == 0) return true;
                foreach (DataRelation Rel in T.ChildRelations) {
                    var ChildTable = Rel.ChildTable;
                    if (ChildToIgnore[ChildTable.TableName] != null) continue;
                    if (ChildTable.TableName == T.TableName) continue;
                    if (IsTemporaryTable(ChildTable)) continue;
                    if (ChildTable.Rows.Count > 0) return false;
                }
                return true;
            }

            //bool CheckIsParent(DataTable T) {
            //    if (T.ChildRelations.Count == 0) return false;
            //    foreach (DataRelation Rel in T.ChildRelations) {
            //        DataTable ChildTable = Rel.ChildTable;
            //        if (ChildTable.Rows.Count > 0) return true;
            //    }
            //    return false;
            //}

            //bool CheckIsChild(DataTable T) {
            //    if (T.ParentRelations.Count == 0) return false;
            //    foreach (DataRelation Rel in T.ParentRelations) {
            //        DataTable ParentTable = Rel.ParentTable;
            //        foreach (DataRow RParent in ParentTable.Rows) {
            //            if (RParent.RowState != DataRowState.Unchanged) return true;
            //        }
            //    }
            //    return false;
            //}

            ///// <summary>
            ///// Classifies DataTables as: 
            /////  None,Child,Parent,Both ("Both" = Child and Parent)
            /////   If a DataTable is in "Both", it should possibly appear before it's parent 
            /////   DataTable, if it is also in "Both".
            ///// </summary>
            ///// <param name="D">DataSet to classify</param>
            ///// <param name="None" type="output">Unrelated DataTables</param>
            ///// <param name="Child" type="output">DataTables linked as Child in some Relation</param>
            ///// <param name="Parent" type="output">DataTables linked as Parent in some Relation</param>
            ///// <param name="Both" type="output">DataTables linked as Child in some Relation, and
            ///// as Parent in some (other) Relation</param>
            //void do_Classify(DataSet D,
            //    ArrayList None,
            //    ArrayList Child,
            //    ArrayList Parent,
            //    ArrayList Both) {

            //    ArrayList TempBoth = new ArrayList();
            //    foreach (DataTable T in D.Tables) {
            //        //DataTable CG = T.GetChanges();
            //        //if (CG == null || CG.Rows.Count == 0) continue;
            //        bool is_child = false;
            //        bool is_parent = false;
            //        //Checks whether the table has childs
            //        is_parent = CheckIsParent(T);
            //        is_child = CheckIsChild(T);
            //        //				if (T.ChildRelations.Count > 0) is_parent = true;
            //        //				if (T.ParentRelations.Count > 0) is_child = true;
            //        if (is_child) {
            //            if (is_parent)
            //                TempBoth.Add(T);
            //            else
            //                Child.Add(T);
            //        }
            //        else {
            //            if (is_parent)
            //                Parent.Add(T);
            //            else
            //                None.Add(T);
            //        }
            //    }

            //    //Sort TempBoth array so that Child DataTable appear before related Parent
            //    System.Collections.IEnumerator myEnum = TempBoth.GetEnumerator();
            //    while (myEnum.MoveNext()) {
            //        DataTable T = (DataTable)myEnum.Current;
            //        //Add table T in Both, in the right order
            //        //Here we assume that a that parent-child relation can't be circular!!

            //        //Find first Table in Both who is a T parent. If it is found, put T before it.
            //        //  else put T at the end of Both
            //        int i;
            //        for (i = 0; i < Both.Count; i++) {
            //            DataTable P = (DataTable)Both[i];
            //            bool is_parent_table = false;
            //            foreach (DataRelation R in P.ParentRelations) {
            //                if (R.ChildTable.Equals(P)) {
            //                    is_parent_table = true;
            //                    break; //abort foreach
            //                }
            //            } //foreach

            //            if (is_parent_table) break; //put T just before P .. quits "for" cycle

            //        }//for   
            //        Both.Insert(i, T);
            //    } //while

            //}


            /// <summary> 
            ///  Adds all Rows (of every Tables referred by "Tables")with a specified State  
            ///  to Result list.
            /// </summary>
            /// <param name="Result" type="output">Updated list of all specified rows</param>
            /// <param name="Tables">Name list of the DataTables to scan</param>
            /// <param name="State">Row state to consider</param>
            /// <param name="reverse">true if Tables is to be scanned in reverse order</param>
            void AddTablesOps(RowChangeCollection Result, ArrayList Tables, DataRowState State, bool reverse) {
                if (reverse) {
                    for (int i = Tables.Count - 1; i >= 0; i--) {
                        var T = (DataTable)Tables[i];
                        if (IsTemporaryTable(T)) continue;
                        foreach (DataRow R in T.Rows) {
                            if (R.RowState == State) Result.Add(mainPost.GetNewRowChange(R));
                        }
                    }
                }
                else {

                    for (int i = 0; i < Tables.Count; i++) {
                        var T = (DataTable)Tables[i];
                        if (IsTemporaryTable(T)) continue;
                        if (State == DataRowState.Deleted) {
                            foreach (DataRow R in T.Rows) {
                                if (R.RowState == State) Result.Add(mainPost.GetNewRowChange(R));
                            }

                        }
                        else {
                            foreach (var R in T.Select(null, GetPostingOrder(T))) {
                                if (R.RowState == State) Result.Add(mainPost.GetNewRowChange(R));
                            }
                        }
                    }
                }
            }





            #endregion

            private PostData mainPost;

            /// <summary>
            /// Rules that must be applied for the current set of changes
            /// </summary>
            public MetaDataRules Rules;
           
            /// <summary>
            /// Posting class that saves a single DataSet with a specified DataAccess
            /// </summary>
            /// <param name="DS"></param>
            /// <param name="Conn"></param>
            /// <param name="p">main Posting class</param>
            public singleDatasetPost(DataSet DS, IDataAccess Conn, PostData p) {
	            this.DS = DS;
	            privateConn = Conn;       //every   singleDatasetPost has his connection
                user = Conn.externalUser;
                mainPost = p;
                ClearDataSet.RemoveConstraints(DS);
	            RemoveFalseUpdates(DS);
	            RowChanges = ChangeList(DS);
                if (RowChanges.nDeletes > 10000) {
                    ErrorLogger.Logger.markEvent($"Deleting {RowChanges.nDeletes} rows ");
                }
	            if (!DS.HasChanges()) return;
	            this.Rules = p.GetRules(RowChanges);

            }

            /// <summary>
            /// get ID of current posting process
            /// </summary>InnerPosting
            /// <param name="d"></param>
            /// <returns></returns>
            public string postingGuid(DataSet d) {
                if (d.ExtendedProperties["postingGuid"] == null) return null;
                return (d.ExtendedProperties["postingGuid"]).ToString();
            }

            /// <summary>
            /// Send a message of startPosting to a privateConn if different from postConn
            /// </summary>
            /// <param name="postConn"></param>
            public void startPosting(IDataAccess postConn) {
                DS.ExtendedProperties["postingGuid"] = Guid.NewGuid().ToString();
                if (postConn != privateConn) {                    
                    privateConn.startPosting(postConn);
                }                
            }

            /// <summary>
            /// Send a message of stopPosting to a privateConn if different from postConn
            /// </summary>
            /// <param name="postConn"></param>
            public void stopPosting(IDataAccess postConn) {
                DS.ExtendedProperties["postingGuid"] = null;
                if (postConn != privateConn) {
                    privateConn.stopPosting();
                }
            }

            /// <summary>
            /// Evalueates autoincrement values, completes the row to be changed with createuser,createtimestamp,
            /// lastmoduser, lastmodtimestamp fields, depending on the operation type
            ///  and eventually calls CalculateFields for each DataRow involved.
            ///  This must be done OUTSIDE the transaction.
            /// </summary>
            public void prepareForPosting() {
	            foreach (RowChange RToPreSet in RowChanges) {
	                //Adjust lastmoduser etc. in order to be able of properly calling checks                
	                RToPreSet.prepareForPosting(user, privateConn, false);
	            }
	        }

            /// <summary>
            /// Collection of PRE- checks
            /// </summary>
            public ProcedureMessageCollection precheck_msg = null;


            #region CALL PRE/POST CHECKS

            /// <summary>
            /// Call pre- checks and fill precheck_msg
            /// </summary>
            /// <returns>true if the changes on the DataSet are possible</returns>              
            /// <remarks>Related Row must have been already filled</remarks>
            public bool DO_PRE_CHECK(Hashtable ignoredMessages) {
                precheck_msg = null;
                ProcedureMessageCollection result;
                try {
                    privateConn.Open();

                    //Call all necessary stored procedures for checking changement
                    result = mainPost.DO_CALL_CHECKS(false, RowChanges);
                    result.PostMsgs = false;
                    //if (RowChanges.nDeletes > 10000) {
                    //    result.AddWarning($"Si stanno cancellando {RowChanges.nDeletes} righe. ");                        
                    //}

                    privateConn.Close();
                    result.SkipMessages(ignoredMessages);
                }
                catch (Exception e) {
	                Trace.Write($"Error :{QueryCreator.GetErrorString(e)}\n", "PostData.DO_PRECHECK\n");
                    privateConn.LogError("PostData.DO_PRECHECK", e);
                    lasterror = QueryCreator.GetErrorString(e);
                    result = mainPost.GetEmptyMessageCollection();
                    result.AddDBSystemError($"Errore nella chiamata delle regole pre:\n{lasterror}");
                    privateConn.Close();
                    return false;
                }
                precheck_msg = result;
                return true;
            }

            private DataJournaling Journal;

            /// <summary>
            /// Gets the Journaling class connected for the posting operation
            /// </summary>
            public void getJournal() {
                Journal = mainPost.getJournal(privateConn, RowChanges);
            }


            /// <summary>
            /// Save all change log (journal) to database
            /// </summary>
            /// <returns></returns>
            public bool DoJournal() {
                DataRowCollection RCs = Journal.DO_Journaling(RowChanges);
                if (RCs == null) return true;
                foreach (DataRow R in RCs) {
                    if (DO_PHYSICAL_POST_ROW(R) != 1) return false;
                }
                return true;
            }
            

            /// <summary>
            /// As above, but returns the error collection. Also sets precheck_msg
            /// </summary>
            public ProcedureMessageCollection SILENT_DO_PRE_CHECK(Hashtable IgnoredMessages) {
                ProcedureMessageCollection result;
                try {
                    privateConn.Open();

                    //Call all necessary stored procedures for checking changement
                    result = mainPost.DO_CALL_CHECKS(false, RowChanges);
                    result.PostMsgs = false;
                    result.SkipMessages(IgnoredMessages);
                }
                catch (Exception e) {
                    result = mainPost.GetEmptyMessageCollection();
                    result.AddDBSystemError(QueryCreator.GetErrorString(e));
                    Trace.Write("Error :" + QueryCreator.GetErrorString(e) + "\r", "PostData.SILENT_DO_PRE_CHECK\r");
                    lasterror = QueryCreator.GetErrorString(e);
                    result.CanIgnore = false;
                    result.PostMsgs = false;
                }
                finally {
                    privateConn.Close();
                }
                precheck_msg = result;
                return result;
            }


            /// <summary>
            /// Query the Business logic to establish whether the operation 
            ///  violates any non ignorable Post-Checks. If it happens, returns false
            /// </summary>
            /// <returns>true if the changes on the DataSet are possible</returns>              
            /// <remarks>Related rows must have been already filled</remarks>
            public ProcedureMessageCollection DO_POST_CHECK(Hashtable IgnoredMessages) {

                //Evaluates every error message & attach them to RowChanges elements
                ProcedureMessageCollection Res=null;
                try {
                    //Call all necessary stored procedures for checking changement
                    Res = mainPost.DO_CALL_CHECKS(true, RowChanges);
                    Res.SkipMessages(IgnoredMessages);
                    Res.PostMsgs = true;
                }
                catch (Exception e) {
                    if (Res==null) Res = mainPost.GetEmptyMessageCollection();
                    Res.AddDBSystemError(QueryCreator.GetErrorString(e));
                    Trace.Write("Error :" + QueryCreator.GetErrorString(e) + "\r", "PostData.DO_POSTCHECK\r");
                    lasterror = QueryCreator.GetErrorString(e);
                    Res.CanIgnore = false;
                    Res.PostMsgs = true;
                }

                return Res;
            }


            #endregion


            #region DO PHYSICAL OPERATION

            ///// <summary>
            ///// Do the phisical changes of the underneath DataBase.
            ///// In this phase, Rows are completed with lastmoduser/lastmodtimestamp fields
            ///// </summary>
            ///// <remarks>On fail, all changes should be rolled-back by the CALLER!!!</remarks>
            ///// <returns>true when successfull </returns>
            //bool DO_PHYSICAL_POST() {

            //    foreach (RowChange R in RowChanges) {
            //        //post the change

            //        R.prepareForPosting(user, privateConn, true);
            //        if (!privateConn.Security.CanPost(R.DR)) {
            //            lasterror =
            //                $"L\'operazione richiesta sulla tabella {R.TableName} è vietata dalle regole di sicurezza.";
            //            return false;
            //        }
            //        int nrowupdated;
            //        try {
            //            nrowupdated = DO_PHYSICAL_POST_ROW(R.DR);
            //        }
            //        catch (Exception E) {
            //            lasterror = "From " + E.Source + ": " + QueryCreator.GetErrorString(E);
            //            privateConn.LogError("DO_PHYSICAL_POST" + lasterror, E);
            //            nrowupdated = 0;
            //        }
            //        if (nrowupdated != 1) {
            //            return false;
            //        }
            //    }

            //    return true;
            //}

            /// <summary>
            /// Write all changed rows to db, returns true if succeeds
            /// </summary>
            /// <returns></returns>
            public bool DO_PHYSICAL_POST_BATCH() {
                var nn = mdl_utils.metaprofiler.StartTimer("DO_PHYSICAL_POST_BATCH()");
                RowChanges.EmptyCache();
                var sb = new StringBuilder();
                var batchedRows = new List<RowChange>();
                var rowindex = 0;
                foreach (RowChange r in RowChanges) {
                    //post the change                
                    r.prepareForPosting(user, privateConn, true); //calls calcAutoID and eventually set R.HasCustomAutoFields to true
                    if (!model.isSkipSecurity(r.Table)) {
                        if (!privateConn.Security.CanPost(r.DR)) {
                            lasterror =
                                $"L\'operazione richiesta sulla tabella {r.TableName} è vietata dalle regole di sicurezza.";
                            mdl_utils.metaprofiler.StopTimer(nn);
                            return false;
                        }
                    }

                    string cmd = getPhysicalPostCommand(r.DR);
                    sb.AppendLine(cmd+";");
                    sb.AppendLine($"if (@@ROWCOUNT=0) BEGIN select {rowindex}; RETURN; END;");
                    batchedRows.Add(r);

                    if (sb.Length > 40000 || r.HasCustomAutoFields) {
                        var res = executeBatch(sb, batchedRows);
                        if (!res) return false;
                        sb = new StringBuilder();
                        batchedRows = new List<RowChange>();
                        rowindex = 0;
                    }
                    else {
                        rowindex++;
                    }

                }
                bool result;
                if (rowindex > 0) {
                    result = executeBatch( sb, batchedRows);
                }
                else {
                    result = true;
                }
                mdl_utils.metaprofiler.StopTimer(nn);
                return result;
            }


            bool executeBatch( StringBuilder batch, List<RowChange> rows) {
                batch.Append("SELECT -1");
                //DataTable T = Conn.SQLRunner(Batch.ToString(), 60, out errmess);
                object res = privateConn.DO_SYS_CMD_LASTRESULT(batch.ToString(), out var errmess);


                if (errmess != null) {
                    ErrorLogger.Logger.markEvent($"Errore su db:{errmess}");
                    lasterror = errmess;
                    return false;
                }

                //Get Bad Row
                int n = Convert.ToInt32(res);
                if (n == -1) {
                    privateConn.SetLastWrite();
                    return true;
                }
                if (n < 0 || n >= rows.Count) {
                    lasterror = $"Errore interno eseguendo:{batch}";
                    ErrorLogger.Logger.markEvent(lasterror);
                    return false;
                }

                RowChange R = rows[n];
                if (R.DR.RowState == DataRowState.Added) {
                    lasterror = $"Error running command:{getPhysicalInsertCmd(R.DR)}";
                    ErrorLogger.Logger.markEvent(lasterror);
                    return false;
                }
                if (R.DR.RowState == DataRowState.Deleted) {
                    lasterror = $"Error running command:{getPhysicalDeleteCmd(R.DR)}";
                    ErrorLogger.Logger.markEvent(lasterror);
                    return false;
                }
                if (R.DR.RowState == DataRowState.Modified) {
                    string err = $"Error running command:{getPhysicalUpdateCmd(R.DR)}";
                    ErrorLogger.Logger.markEvent(err);
                    R.DR.RejectChanges();
                    RESELECT(R.DR, DataRowVersion.Default);
                    lasterror = err;
                    return false;
                }
                return false;

            }

           

            string getPhysicalPostCommand(DataRow R) {
                switch (R.RowState) {
                    case DataRowState.Added:
                        return getPhysicalInsertCmd(R);
                    case DataRowState.Modified:
                        return getPhysicalUpdateCmd(R);
                    case DataRowState.Deleted:
                        return getPhysicalDeleteCmd(R);
                }
                return "";
            }

            int DO_PHYSICAL_POST_ROW(DataRow R) {

                switch (R.RowState) {
                    case DataRowState.Added:
                        return DO_PHYSICAL_INSERT(R);
                    case DataRowState.Modified:
                        return DO_PHYSICAL_UPDATE(R);
                    case DataRowState.Deleted:
                        return DO_PHYSICAL_DELETE(R);
                }
                return 0;
            }


            string getPhysicalDeleteCmd(DataRow R) {
                var T = R.Table;
                string tablename = T.tableForPosting();
                string condition = mainPost.GetOptimisticClause(R);
                return privateConn.GetDeleteCommand(tablename, condition);
            }

            int DO_PHYSICAL_DELETE( DataRow r) {
                var T = r.Table;
                var tablename = T.tableForPosting();
                var condition = mainPost.GetOptimisticClause(r);
                var msg = privateConn.DO_DELETE(tablename, condition);
                if (msg == null) return 1;
                lasterror = msg;
                r.RejectChanges();
                RESELECT(r, DataRowVersion.Default);
                return 0;
            }


            string getPhysicalInsertCmd(DataRow R) {
                DataTable T = R.Table;
                string tablename = T.tableForPosting();
                int npar = 0;
                string[] names = new string[T.Columns.Count];
                string[] values = new string[T.Columns.Count];

                foreach (DataColumn C in T.Columns) {
                    if (!QueryCreator.IsRealColumn(C)) continue;
                    if (R[C, DataRowVersion.Default] == DBNull.Value) continue; //non inserisce valori null
                    string postcolname = QueryCreator.PostingColumnName(C); // C.ColumnName;
                    if (postcolname == null) continue;
                    names[npar] = postcolname;
                    values[npar] = mdl_utils.Quoting.quotedstrvalue(R[C, DataRowVersion.Default], C.DataType, true);
                    npar++;
                }

                return privateConn.getInsertCommand(tablename, names, values, npar);
            }

            int DO_PHYSICAL_INSERT(DataRow R) {
                DataTable T = R.Table;
                string tablename =T.tableForPosting();
                int npar = 0;
                string[] names = new string[T.Columns.Count];
                string[] values = new string[T.Columns.Count];

                foreach (DataColumn C in T.Columns) {
                    if (!QueryCreator.IsRealColumn(C)) continue;
                    if (R[C, DataRowVersion.Default] == DBNull.Value) continue; //non inserisce valori null
                    string postcolname = QueryCreator.PostingColumnName(C); // C.ColumnName;
                    if (postcolname == null) continue;
                    names[npar] = postcolname;
                    values[npar] = mdl_utils.Quoting.quotedstrvalue(R[C, DataRowVersion.Default], C.DataType, true);
                    npar++;
                }

                string msg = privateConn.DO_INSERT(tablename, names, values, npar);
                if (msg == null) return 1;
                lasterror = msg;
                return 0;
            }

           

            string getPhysicalUpdateCmd(DataRow R) {
                DataTable T = R.Table;
                string tablename = T.tableForPosting();
                int npar = 0;

                string[] names = new string[T.Columns.Count];
                string[] values = new string[T.Columns.Count];

                foreach (DataColumn C in T.Columns) {
                    if (!QueryCreator.IsRealColumn(C)) continue;
                    if (R[C, DataRowVersion.Original].Equals(R[C, DataRowVersion.Current])) continue;
                    string postcolname = QueryCreator.PostingColumnName(C); // C.ColumnName;
                    if (postcolname == null) continue;
                    names[npar] = postcolname;
                    values[npar] = mdl_utils.Quoting.quotedstrvalue(R[C, DataRowVersion.Current], C.DataType, true);
                    npar++;
                }
                return privateConn.getUpdateCommand(tablename, mainPost.GetOptimisticClause(R), names, values, npar);
            }

            int DO_PHYSICAL_UPDATE(DataRow R) {
                DataTable T = R.Table;
                string tablename = T.tableForPosting();
                int npar = 0;

                string[] names = new string[T.Columns.Count];
                string[] values = new string[T.Columns.Count];

                foreach (DataColumn C in T.Columns) {
                    if (!QueryCreator.IsRealColumn(C)) continue;
                    if (R[C, DataRowVersion.Original].Equals(R[C, DataRowVersion.Current])) continue;
                    string postcolname = QueryCreator.PostingColumnName(C); // C.ColumnName;
                    if (postcolname == null) continue;
                    names[npar] = postcolname;
                    values[npar] = mdl_utils.Quoting.quotedstrvalue(R[C, DataRowVersion.Current], C.DataType, true);
                    npar++;
                }
                if (npar == 0) return 1;

                string msg = privateConn.DO_UPDATE(tablename, mainPost.GetOptimisticClause(R), names, values, npar);
                if (msg == null) return 1;
                lasterror = msg;
                R.RejectChanges();
                RESELECT(R, DataRowVersion.Default);
                return 0;
            }


            /// <summary>
            /// Re-fetch a row from DB by primary key
            /// </summary>
            /// <param name="R"></param>
            /// <param name="ver"></param>
            void RESELECT(DataRow R, DataRowVersion ver) {
                privateConn.RUN_SELECT_INTO_TABLE(R.Table, null,
                    QueryCreator.WHERE_KEY_CLAUSE(R, ver, true), null, true);
            }

            /// <summary>
            /// Reads from DB all views that contains data from other tables than 
            ///  primary table of the view 
            /// </summary>
            public void ReselectAllViews() {
                foreach (DataTable T in DS.Tables) {
                    if (T.TableName == T.tableForPosting()) continue;
                    bool HasExtraColumns = false;
                    foreach (DataColumn C in T.Columns) {
                        if (QueryCreator.PostingColumnName(C) == null) {
                            HasExtraColumns = true;
                            break;
                        }
                    }
                    if (!HasExtraColumns) continue;
                    foreach (DataRow R in T.Rows) {
                        if ((R.RowState == DataRowState.Added) ||
                            (R.RowState == DataRowState.Modified)) {
                            R.AcceptChanges();
                            RESELECT(R, DataRowVersion.Default);
                        }
                    }
                }
            }


            #endregion
        }

        /// <summary>
        /// DataSet beng posted
        /// </summary>
	    public DataSet DS;


        /// <summary>
        /// Return a new RowChange linked to a Row
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        virtual public RowChange GetNewRowChange(DataRow R) {
            return new RowChange(R);
        }


        /// <summary>
        /// Set the row order for posting a table to db
        /// </summary>
        /// <param name="T"></param>
        /// <param name="order"></param>
        public static void SetPostingOrder(DataTable T, string order) {
            T.ExtendedProperties["postingorder"] = order;
        }

        /// <summary>
        /// Gets the order for posting rows in the db
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static string GetPostingOrder(DataTable T) {
            return T.ExtendedProperties["postingorder"] as string;
        }

        /// <summary>
        /// Gives a string for using as a condition to test for assuring that no
        ///  changes have been made to the row to update/delete since it was read. 
        ///  Uses Posting Table/Columns names
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        virtual public string GetOptimisticClause(DataRow R) {
            return QueryCreator.WHERE_CLAUSE(R, DataRowVersion.Original,
                true, true);
        }

        /// <summary>
        /// Se le regole in input non sono ignorabili restituisce il set in input. Altrimenti calcola un nuovo set di regole PRE.
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="ignoredMessages">Messages to ignore</param>
        /// <returns></returns>
	    ProcedureMessageCollection silentDoAllPrecheck(ProcedureMessageCollection resultList, Hashtable ignoredMessages) {
            if ((resultList != null) && (!resultList.CanIgnore)) return resultList;

	        resultList = GetEmptyMessageCollection();
	        foreach (singleDatasetPost p in allPost) {
	            var curr = p.SILENT_DO_PRE_CHECK(ignoredMessages);
	            //if (p.RowChanges.nDeletes > 10000) {
	            //    curr.AddWarning($"Si stanno cancellando {p.RowChanges.nDeletes} righe. ");                  
	            //}
	            resultList.Add(curr);
	        }
	        return resultList;

	    }
        /// <summary>
        /// Should return true if it is allowed to Post a DataRow to DB
        /// As Default returns Conn.CanPost(R)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        protected virtual bool Can_Post(DataRow r) {
            return dbConn.Security.CanPost(r);
        }

        /// <summary>
        /// Constructor. After building a PostData, it's necessary to call
        ///  InitClass.
        /// </summary>
        public PostData() {
			//            WellObject = false;
			refresh_dataset=true;
            autoIgnore = false;
			lasterror="";
		}

		/// <summary>
		/// Gets the set of business rule for a given set of changes
		/// </summary>
		/// <param name="Cs"></param>
		/// <returns></returns>
		virtual protected MetaDataRules GetRules(RowChangeCollection Cs){
			return new MetaDataRules();
		}

		/// <summary>
		/// Tells MDE that a table is temporary and should 
		///  not be used for calling stored procedure, messages, logs, or updates.
		/// Temporary tables are never read or written to db by the library
		/// </summary>
		/// <param name="T">Table to mark</param>
		/// <param name="createblankrow">true if a row has to be added to table</param>
		public static void MarkAsTemporaryTable(DataTable T, bool createblankrow){
			T.ExtendedProperties[IsTempTable]="y";
			if (createblankrow){
				GetData.Add_Blank_Row(T);
			}
		}

		/// <summary>
		/// Returns true if a DataTable has been marked as Temporary
		/// </summary>
		/// <param name="T"></param>
		/// <returns></returns>
		public static bool IsTemporaryTable(DataTable T){
			if (T.ExtendedProperties[IsTempTable]==null) return false;
			return true;
		}

		/// <summary>
		/// Undo "MarkAsTemporaryTable"
		/// </summary>
		/// <param name="T"></param>
		public static void MarkAsRealTable(DataTable T){
			T.ExtendedProperties[IsTempTable]=null;
		}

	    private IDataAccess _dbConn;

        /// <summary>
        /// Connection to database
        /// </summary>
	    public IDataAccess dbConn {
#pragma warning disable 612
	        get { return _dbConn ?? Conn; }
            set { _dbConn = value; Conn=value as DataAccess; }
#pragma warning restore 612
	    }
        /// <summary>
        /// List of PostData classes that concurr in the transaction
        /// </summary>
	    protected List<singleDatasetPost> allPost = new List<singleDatasetPost>();

        /// <summary>
        /// Initialize PostData. Must be called before DO_POST
        /// </summary>
        /// <param name="ds">DataSet to handle</param>
        /// <param name="conn">Connection to the DataBase</param>
        /// <remarks>This function must be called AFTER the changes have
        ///  been applied to DS.</remarks>
        /// <returns>error string if errors, null otherwise</returns>        
        [Obsolete]
        public virtual string InitClass(DataSet ds, DataAccess conn ) {
		    if (this.Conn == null) {
		        this.Conn = conn;
		    }
		    if (this.DS == null) {
                this.DS = ds;
		    }
		    allPost.Add(new singleDatasetPost(ds, conn, this));
            return null;
		}

	    /// <summary>
	    /// Initialize PostData. Must be called before DO_POST
	    /// </summary>
	    /// <param name="ds">DataSet to handle</param>
	    /// <param name="conn">Connection to the DataBase</param>
	    /// <remarks>This function must be called AFTER the changes have
	    ///  been applied to DS.</remarks>
	    /// <returns>error string if errors, null otherwise</returns>   
	    public virtual string initClass(DataSet ds, IDataAccess conn) {
	        if (_dbConn == null) {
	            _dbConn = conn;
	        }
	        if (DS == null) {
	            DS = ds;
	        }
	        allPost.Add(new singleDatasetPost(ds, conn, this));
	        return null;
        }

        /// <summary>
        /// Calls business logic and return error messages
        /// </summary>
        /// <param name="post">if true, it is a "AFTER-POST" check</param>
        /// <param name="RC">Collection of changes posted to the DB</param>
        /// <returns>Collection of Error/warings</returns>
        protected virtual ProcedureMessageCollection DO_CALL_CHECKS(bool post, RowChangeCollection RC){
			return new ProcedureMessageCollection();
		}

    

        #region CHECKS FOR TRUE/FALSE UPDATES
        /// <summary>
        /// returns true if row (modified) is an improperly set modified row
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        public static bool CheckForFalseUpdate(DataRow R){
            if (R.RowState != DataRowState.Modified) return false;
			foreach (DataColumn C in R.Table.Columns){
				if (QueryCreator.IsTemporary(C))continue;
				if (!R[C,DataRowVersion.Original].Equals(R[C,DataRowVersion.Current])) return false;
			}
			return true;
		}

		/// <summary>
		/// Remove false update from a DataSet, i.e. calls AcceptChanges
		///  for any DataRow set erroneously as modified
		/// </summary>
		/// <param name="DS"></param>
		public static void RemoveFalseUpdates(DataSet DS){
			foreach(DataTable T in DS.Tables){
				if (IsTemporaryTable(T))continue;
				foreach (DataRow R in T.Rows) {
					if (R.RowState != DataRowState.Modified) continue;
					if (CheckForFalseUpdate(R))	R.AcceptChanges();
				}
			}
		}

        /// <summary>
        /// Check if table contains any row not in Unchanged state
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
	    public static bool hasChanges(DataTable t) {
            if (t == null) return false;
            foreach (DataRow R in t.Rows) {
                if (R.RowState == DataRowState.Unchanged) continue;
                if (R.RowState != DataRowState.Modified) return true;
                if (CheckForFalseUpdate(R)) {
                    R.AcceptChanges();
                    continue;
                }
                return true;
            }
            return false;
        }

		/// <summary>
		/// return true if row has really been modified
		/// </summary>
		/// <param name="R"></param>
		public static bool CheckRowForUpdates(DataRow R){
			if (R.RowState == DataRowState.Detached) return false;
			if (R.RowState == DataRowState.Unchanged) return false;
			if (R.RowState == DataRowState.Added) return true;
			if (R.RowState == DataRowState.Deleted) return true;
			if (CheckForFalseUpdate(R)){
				R.AcceptChanges();
				return false;
			}
			else {
				return true;
			}
		}


		#endregion


		/// <summary>
		/// Gets a new DataJournaling object 
		/// </summary>
		/// <param name="Conn"></param>
		/// <param name="Cs"></param>
		/// <returns></returns>
		protected virtual DataJournaling GetJournal(DataAccess Conn, RowChangeCollection Cs){
			return new DataJournaling();
		}

	    /// <summary>
	    /// Gets a new DataJournaling object 
	    /// </summary>
	    /// <param name="Conn"></param>
	    /// <param name="Cs"></param>
	    /// <returns></returns>
	    protected virtual DataJournaling getJournal(IDataAccess Conn, RowChangeCollection Cs) {
	        return new DataJournaling();
	    }

        /// <summary>
        /// Checks if there is any rowchange in all dataset
        /// </summary>
        /// <returns></returns>
        bool someChange() {
	        foreach (singleDatasetPost p in allPost) {
	            if (p.RowChanges.Count > 0) return true;
	        }
	        return false;
	    }

	    string lastError() {
	        return dbConn.LastError;
	    }

        /// <summary>
        /// Completes the row to be changed with createuser,createtimestamp,
        /// lastmoduser, lastmodtimestamp fields, depending on the operation type
        ///  and calls CalculateFields for each DataRow involved.
        ///  This CAN be done OUTSIDE the transaction.
        /// </summary>
        void prepareForPosting() {
            foreach (var p in allPost) {
                p.prepareForPosting();
            }
        }

	    void emptyCache() {
            foreach (var p in allPost) {
                p.RowChanges.EmptyCache();
            }
        }

	    void clear_precheck_msg() {
            foreach (var p in allPost) {
                p.precheck_msg = null;
            }
        }

	    bool do_all_precheck() {
		    var handle = mdl_utils.metaprofiler.StartTimer("do_all_precheck");
            bool res = true;
            foreach (var p in allPost) {
                bool thisRes = p.DO_PRE_CHECK( IgnoredMessages);
                res &= thisRes;
            }
            mdl_utils.metaprofiler.StopTimer(handle);
            return res;
        }
        ProcedureMessageCollection do_all_postcheck() {
	        var handle = mdl_utils.metaprofiler.StartTimer("do_all_postcheck");
            var res = GetEmptyMessageCollection();
            foreach (var p in allPost) {
                var thisRes = p.DO_POST_CHECK(IgnoredMessages);
                res.Add(thisRes);
            }
            mdl_utils.metaprofiler.StopTimer(handle);
            return res;
        }

	    void merge_all_precheck(ProcedureMessageCollection m) {
            foreach (var p in allPost) {
                var thisPre= p.precheck_msg;            
                if (thisPre == null) continue;
                p.precheck_msg = null;
                m.Add(thisPre);
            }
        }
        


        void getAllJournal() {
            foreach (var p in allPost) {
                p.getJournal();
            }
        }

	    bool doAllPhisicalPostBatch() {
		    var handle = mdl_utils.metaprofiler.StartTimer("doAllPhisicalPostBatch");

            foreach (var p in allPost) {
                bool thisRes = p.DO_PHYSICAL_POST_BATCH();
                if (!thisRes) { //Mette in lasterror l'errore verificatosi nel singleDatasetPost
                    if (lasterror == null) lasterror = "";
                    lasterror += p.lasterror+"\r\n";
                    mdl_utils.metaprofiler.StopTimer(handle);
                    return false;                    
                }                
            }
            mdl_utils.metaprofiler.StopTimer(handle);
            return true;            
        }

	    bool doAllJournaling() {
            foreach (var p in allPost) {
                if (!p.DoJournal()) return false;
            }
            return true;
            
        }

	    bool all_externalUpdate(out string lasterror) {
            lasterror = null;
            foreach (var p in allPost) {                
                if (!myDoExternalUpdate(p.DS, out lasterror)) return false;
            }
            return true;
	    }

        /// <summary>
        /// Reads all data about views (also in inner posting classes)
        /// </summary>
	    public void reselectAllViewsAndAcceptChanges() {
	        var handle = mdl_utils.metaprofiler.StartTimer("reselectAllViewsAndAcceptChanges");
	        foreach (var p in allPost) {
	            p.ReselectAllViews();
	            var h = mdl_utils.metaprofiler.StartTimer("reselectAllViewsAndAcceptChanges - AcceptChanges");
	            p.DS.AcceptChanges();
                mdl_utils.metaprofiler.StopTimer(h);
	        }

	        var inner = getInnerPosting(DS);
	        if (inner != null) {
		        inner.reselectAllViewsAndAcceptChanges();
	        }
            mdl_utils.metaprofiler.StopTimer(handle);
	    }

	    void startPosting() {
	        foreach (var p in allPost) {
	            p.startPosting(dbConn);
	        }
	    }

	    void stopPosting() {
                foreach (var p in allPost) {
                    p.stopPosting(dbConn);
                }
            }
        #region DO POST 

        /// <summary>
        /// Instruct to successively ignore a set of messages
        /// </summary>
        /// <param name="msgs"></param>
	    public void addMessagesToIgnore(Hashtable msgs) {
	        foreach (string s in msgs.Keys) {
	            IgnoredMessages[s] = msgs[s];
	        }
	    }

	    Hashtable IgnoredMessages = new Hashtable();

        /// <summary>
        /// Fills the list of ignored messages with the collection specified
        /// </summary>
        /// <param name="msgs"></param>
        public void IgnoreMessages(ProcedureMessageCollection msgs) {
            if (ResultList == null) {
                ResultList = GetEmptyMessageCollection();
            }
            else {
                ResultList.Clear();
            }

            ResultList.AddRange(msgs);
        }

	    int totalDeletes(out string msg) {
	        int nDel = 0;
	        msg = "";
	        foreach (var p in allPost) {
	            nDel += p.RowChanges.nDeletes;
	        }

	        if (nDel > 10000) {
	            foreach (var p in allPost) {
	                
	                foreach (DataTable t in p.DS.Tables) {
	                    int currNdel = 0;
	                    foreach (DataRow r in t.Rows) {
	                        if (r.RowState == DataRowState.Deleted) currNdel++;
	                    }

	                    if (currNdel > 10) {
	                        msg += $"Table {t.TableName}: {currNdel} deletion;\n\r";
	                    }
	                }
	            }
	        }

	        return nDel;
	    }
	    private bool internalDoPost() {
            if (!someChange()) return true;
            if (innerPosting)throw new Exception("internalDoPost called with innerPosting=true");

	        InnerPosting innerPostingClass = getInnerPosting(DS);

	        var postMsgs = GetEmptyMessageCollection();
	        

            string ignoredError = lastError();
            if (!string.IsNullOrEmpty(ignoredError)) {
                ErrorLogger.Logger.markEvent("Error: " + ignoredError + " has been IGNORED before starting POST!");
            }

            prepareForPosting();

            //EVALUATE PRE - CONDITIONS
            lasterror = "";           
            bool result = false;
            bool tryAgain = true;
            clear_precheck_msg();
            getAllJournal();
            int nDeletes = totalDeletes(out var logDel);
            while (tryAgain) {
                tryAgain = false;

                if (postMsgs.Count>0) {
                    //Visualizza le regole post scattate nell'iterazione precedente.
                    if (postMsgs.ShowMessages()) {
                        postMsgs.AddMessagesToIgnore(IgnoredMessages);
                    }
                    else {
                        return false; //L'utente ha scelto di non ignorare le regole
                    }
                }

                dbConn.Open();      //Connection must been opened before setting isolation level
                string msg = dbConn.BeginTransaction(IsolationLevel.ReadCommitted);
                if (msg != null) {
                    dbConn.Close();
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(msg);
                    return false;
                }

                try {
                    clear_precheck_msg();

                    result = do_all_precheck(); //fills precheck_msg, already cleared by IgnoredMessages
                    var allPreCheck = GetEmptyMessageCollection();

                    if (!result) {
                        var err = dbConn.LastError ?? "Nessun dettaglio disponibile";
                        allPreCheck.AddDBSystemError("Errore durante l'interrogazione della logica business. (Precheck)\nDettaglio: " + err);
                    }
                    merge_all_precheck(allPreCheck);
                    if (allPreCheck.CanIgnore && autoIgnore) {
                        allPreCheck.Clear();
                    }

                    postMsgs = allPreCheck; //lo assegna subito a scanso di equivoci, caso mai non entra nel ramo interno
                    if (!allPreCheck.CanIgnore) {
                        result = false;
                    }
                    //DO PHYSICAL CHANGES (auto-filling lastmoduser/timestamps fields)
                    if (result) {
                        emptyCache();
                        foreach(DataTable t in DS.Tables)RowChange.ClearMaxCache(t);
                        result = doAllPhisicalPostBatch();
                        if (!result) {
                            allPreCheck.AddDBSystemError(!string.IsNullOrEmpty(lasterror)
                                ? lasterror
                                : "Errore nella scrittura sul database");
                        }
                    }
                    
                    if (result) {
                        result = doAllJournaling();
                        if (!result)allPreCheck.AddDBSystemError("Si sono verificati errori durante la scrittura nel log");
                    }
                  
                    if (result) {
                        //DB WRITE SUCCEEDED
                        //EVALUATE POST - CONDITION					
                        postMsgs = do_all_postcheck();
                        postMsgs.Add(allPreCheck);
                        postMsgs.SkipMessages(IgnoredMessages);

                        if (!dbConn.validTransaction()) {
                            string err = dbConn.LastError ?? "Nessun dettaglio disponibile";
                            postMsgs.AddDBSystemError(
                                "Errore durante l'interrogazione della logica business. (Postcheck).\nDettaglio: " + err);
                        }

                        if (postMsgs.CanIgnore) {
                            if (autoIgnore) {
                                postMsgs.Clear();
                            }
                            if (postMsgs.Count > 0) {
                                tryAgain = true;
                            }
                            else {
                                result = all_externalUpdate(out lasterror);
                                if (result) {
                                    if (innerPostingClass != null) {
                                        innerPostingClass.initClass(DS,dbConn);
                                        innerPostingClass.setInnerPosting(IgnoredMessages);
                                        var innerRules = innerPostingClass.DO_POST_SERVICE();
                                        if (innerRules.CanIgnore && autoIgnore) {
                                            innerRules.Clear();
                                        }
                                        if (innerRules.Count > 0) {
                                            postMsgs.Add(innerRules);
                                            result = false;
                                            tryAgain = true;
                                        }            
                                    }

                                    if (result) {
                                        msg = dbConn.Commit();
                                        if (msg != null) {
                                            postMsgs.AddDBSystemError("Errore nella commit:" + msg);
                                            lasterror = msg;
                                            result = false;
                                        }
                                        else {
                                            if (nDeletes > 10000) {
                                                ErrorLogger.Logger.logException(logDel);
                                            }
                                        }
                                    }
                                }
                                else {                                    
                                    postMsgs.AddDBSystemError("Errore nella scrittura su DB. La routine di aggiornamento hanno fallito.");
                                }
                            }
                        }
                        else {
                            result = false;
                        }
                    }
                }
                catch (Exception e) {
                    lasterror = QueryCreator.GetErrorString(e);
                    postMsgs.AddDBSystemError("Eccezione nel salvataggio (internalDoPost)."+lasterror);
                    dbConn.LogError("internalDoPost", e);
                    result = false;
                }

                if ((result == false) || tryAgain) {
                    var nretry = 0;

                    string msg2= dbConn.RollBack();
                    while (msg2 != null && nretry<3) {
                        nretry++;
                        msg2 = dbConn.RollBack();
                    }
                    if (msg2 != null) {
                        result = false;
                        lasterror += msg2;
                        tryAgain = false;
                        postMsgs.AddDBSystemError("Errore nella scrittura su DB. Rollback fallito.");
                    }
                }
                
                dbConn.Close();
                
                //cicla sin quando ci sono nuovi errori
                if (result==false) recursiveCallAfterPost(false);
                ResultList = null; //inutile, non l'ha usata qui
            }


            if (result) {
                reselectAllViewsAndAcceptChanges();
                if (innerPostingClass != null) {
                    recursiveCallAfterPost(true);
                }
                return true;
            }
            postMsgs.SkipMessages(IgnoredMessages);

            if (postMsgs.Count>0) {
                //Visualizza le regole post o gli errori di db che hanno bloccato il salvataggio                
                postMsgs.ShowMessages();
                return false;
            }

	        recursiveCallAfterPost(false);
            postMsgs.AddDBSystemError("Impossibile scrivere le modifiche. Probabilmente il contenuto del database è cambiato durante le modifiche." +
                lasterror);
            return false;
                        

        }
        /// <summary>
        /// Call afterPost on all inner posting classes
        /// </summary>
        /// <param name="committed"></param>
	    void recursiveCallAfterPost(bool committed) {
	        var inner = getInnerPosting(DS);
	        while (inner!=null) {
	            inner.afterPost(committed);
	            inner = inner.getInnerPosting();	          
	        }
	        
	    }
        /// <summary>
        /// Do ALL the necessary operation about posting data to DataBase. 
        /// If fails, rolles back all (eventually) changes made.
        /// Management of the transaction commit / rollback is done HERE!
        /// </summary>
        /// <returns>true if success</returns>
        /// <remarks>If it succeeds, DataSet.AcceptChanges should be called</remarks>
        public virtual bool DO_POST() {
            startPosting();
            bool res = internalDoPost();
            stopPosting();
            return res;
        }

        /// <summary>
        /// Marks a dataset so that when it will be posted it will use an innerPosting class
        /// </summary>
        /// <param name="d"></param>
        /// <param name="post"></param>
	    public static void setInnerPosting(DataSet d, InnerPosting post) {
	        d.ExtendedProperties["MDL_innerDoPostService"] = post;
	    }

        /// <summary>
        /// Gets the inner posting class that will be used when a dataset will be posted
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
	    public static InnerPosting getInnerPosting(DataSet d) {
	        return  d.ExtendedProperties["MDL_innerDoPostService"] as InnerPosting;
	    }


        /// <summary>
        /// Do the post as a service, same as doPost except for output data kind
        /// </summary>
        /// <returns></returns>
        private ProcedureMessageCollection internalDoPostService() {
            if (!someChange()) return GetEmptyMessageCollection();

            var innerPostingClass = getInnerPosting(DS);

            


            string IgnoredError = dbConn.LastError;
            if (!string.IsNullOrEmpty(IgnoredError)) {
                ErrorLogger.Logger.markEvent($"Error: {IgnoredError} has been IGNORED before starting POST!");
            }

            //Calc rows related to changes & attach them to RowChanges elements
            //This is a necessary step to call DO_PRE_CHECK & DO_POST_CHECK
            //DO_CALC_RELATED();
            //If resultList already exists, add them to Ignored Messages
            ResultList?.AddMessagesToIgnore(IgnoredMessages);

            ResultList = GetEmptyMessageCollection();
            getAllJournal();

            prepareForPosting();
          
            var result=true; //se false ci sono errori bloccanti
            try {
                clear_precheck_msg();

                if (!innerPosting) dbConn.Open(); //if inner, connection is already opened
                string msg = innerPosting? null:dbConn.BeginTransaction(IsolationLevel.ReadCommitted);
                if (msg != null) {
                    dbConn.Close();
                    ResultList.AddDBSystemError($"Errore creando la transazione.\n{msg}");
                    return ResultList;
                }

                int nDeletes = totalDeletes(out var logDel);

                //Ricalcola le regole PRE e ignora eventuali regole già ignorate, ma se le regole sono non ignorabili non fa nulla
                ResultList = silentDoAllPrecheck(ResultList, IgnoredMessages);
                //ResultList.SkipMessages(IgnoredMessages);//lo fa già silentDoAllPrecheck                

                if (!dbConn.validTransaction()) {
                    var err = dbConn.LastError ?? "Nessun dettaglio disponibile";
                    ResultList.AddDBSystemError($"Errore durante l\'interrogazione della logica business. (Precheck).\nDettaglio: {err}");
                }
                if (ResultList.CanIgnore && autoIgnore) {
                    ResultList.Clear();
                }

                if (ResultList.Count > 0 && ResultList.CanIgnore == false) {
                    result=false; //non procede con regola check bloccanti
                }

                //DO PHYSICAL CHANGES (auto-filling lastmoduser/timestamps fields)
                if (result) {
                    emptyCache();
                    foreach(DataTable t in DS.Tables)RowChange.ClearMaxCache(t);
                    result = doAllPhisicalPostBatch();
                    if (!result) ResultList.AddDBSystemError($"Errore nella scrittura sul database:{lasterror}");
                }                

                if (result) {
                    result = doAllJournaling();
                    if (!result) ResultList.AddDBSystemError("Si sono verificati errori durante la scrittura nel log");
                }

                if (result) {  //DB WRITE SUCCEEDED
                               //EVALUATE POST - CONDITION
                    ProcedureMessageCollection postResultList = do_all_postcheck();
                    ResultList.Add(postResultList);
                    ResultList.SkipMessages(IgnoredMessages);

                    if (!dbConn.validTransaction()) {
                        var err = dbConn.LastError ?? "Nessun dettaglio disponibile";
                        ResultList.AddDBSystemError("Errore durante l'interrogazione della logica business. (Postcheck).\nDettaglio: " + err);
                    }

                    if (ResultList.CanIgnore && autoIgnore) {
                        ResultList.Clear();
                    }                

                    if (ResultList.Count > 0) result = false;

                    if (result) {
                        result = all_externalUpdate(out lasterror);
                        if (!result) {
                            ResultList.AddDBSystemError("Errore nella scrittura su DB. Le routine di aggiornamento hanno fallito.");
                        }
                    }

                    if (result && innerPostingClass != null) {
                        //Effettua il post interno
                        innerPostingClass.initClass(DS,dbConn);
                        innerPostingClass.setInnerPosting(IgnoredMessages);

                        var innerRules = innerPostingClass.DO_POST_SERVICE();
                        innerRules.SkipMessages(IgnoredMessages);

                        if (innerRules.CanIgnore && autoIgnore) {
                            innerRules.Clear();
                        }
                        if (innerRules.Count > 0) {
                            ResultList.Add(innerRules);
                            result = false;
                        }                       
                    }

                    if (result) {
                        msg = innerPosting? null:dbConn.Commit();
                        if (msg != null) {
                            ResultList.AddDBSystemError("Errore nella commit:" + msg);
                            lasterror = msg;
                            result = false;
                        }
                        else {
                            if (nDeletes > 10000) {
                                ErrorLogger.Logger.logException(logDel);
                            }
                        }

                    }                    
                }
            }
            catch (Exception E) {
                if (ResultList == null) ResultList = GetEmptyMessageCollection();
                ResultList.AddDBSystemError(QueryCreator.GetErrorString(E));
                ResultList.CanIgnore = false;
                ResultList.PostMsgs = true;
                Trace.Write("Error:" + QueryCreator.GetErrorString(E) + "\r", "PostData.DO_POST_SERVICE\r");
                lasterror = E.Message;
                result = false;
            }

            if ((result==false) || ResultList.Count > 0) {
                //rolls back transaction
                string msg2= innerPosting? null:dbConn.RollBack();
                if (msg2 != null) {
                    //result = false; unused
                    lasterror += msg2;
                    ResultList.AddDBSystemError("Errore nella scrittura su DB. Rollback fallito.");
                }
                if (!innerPosting) dbConn.Close();
                ResultList.PostMsgs = true; //necessario se DO_POST_CHECK non è stata chiamata
                if (!innerPosting) recursiveCallAfterPost(false);
                return ResultList;
            }

            if (!innerPosting) {
                dbConn.Close();
                reselectAllViewsAndAcceptChanges();
            }

            if (innerPostingClass != null && !innerPosting) {
                recursiveCallAfterPost(true);
            }
            return ResultList; //è una lista vuota se tutto è andato bene

        }

	   


        /// <summary>
        /// True if this posting is placed insider another posting
        /// </summary>
	    public bool innerPosting = false;
        /// <summary>
        /// Do ALL the necessary operation about posting data to DataBase. 
        /// If fails, rolles back all (eventually) changes made.
        /// Management of the transaction commit / rollback is done HERE!
        /// </summary>
        /// <returns>An empty Message Collection if success, Null if severe errors</returns>
        /// <remarks>If it succeeds, DataSet.AcceptChanges should be called</remarks>
        public virtual ProcedureMessageCollection DO_POST_SERVICE(){
            startPosting();
            

            ProcedureMessageCollection res = internalDoPostService();
            stopPosting();
            return res;
        }


		#endregion

        /// <summary>
        /// Updates a remote service with data involved in this transaction
        /// </summary>
        /// <param name="D"></param>
        /// <param name="ErrMsg">Error msg or null if all ok</param>
        /// <returns>true if operation has been correctly performed</returns>
        public delegate bool DoExternalUpdateDelegate(DataSet D, out string ErrMsg);

        /// <summary>
        /// Delegate to be runned when DataSet has been posted on db, before committing the transaction.
        /// If it throws an exception the transaction is rolledback and the Exception message showed to the user
        /// </summary>
        public DoExternalUpdateDelegate DoExternalUpdate;

        bool myDoExternalUpdate(DataSet d, out string errMsg) {
            errMsg = null;
            if (DoExternalUpdate == null) return true;
            try {
                return DoExternalUpdate(d, out errMsg);
            }
            catch (Exception E) {
                errMsg = QueryCreator.GetErrorString(E);
                return false;
            }
        }


	}     

    /// <summary>
    /// Default inner posting class, implements InnerPosting interface
    /// </summary>
    public class Base_InnerPoster : InnerPosting {
        /// <summary>
        /// 
        /// </summary>
        public PostData p;


        //private IDataAccess conn;
        Hashtable msgsToIgnore= new Hashtable();

        /// <summary>
        /// inner PostData class
        /// </summary>
        //PostData innerPostClass { get; }
        public Hashtable hashMessagesToIgnore() {
            return msgsToIgnore;
        }

        /// <summary>
        /// Called to initialize the class, inside the transaction
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="conn"></param>
        public virtual void initClass(DataSet ds, IDataAccess conn) {
            //this.conn = conn;
            msgsToIgnore.Clear();
        }


        /// <summary>
        /// Unisce i messaggi dati a quelli finali
        /// </summary>
        /// <param name="messages"></param>
        public void mergeMessages(ProcedureMessageCollection messages) {
            messages.SkipMessages(msgsToIgnore);

        }

        /// <summary>
        /// Called after data has been committed or rolled back
        /// </summary>
        /// <param name="committed"></param>
        public virtual void afterPost(bool committed) {

        }

        /// <summary>
        /// Reads all data in dataset views 
        /// </summary>
        public virtual void reselectAllViewsAndAcceptChanges() {
            p.reselectAllViewsAndAcceptChanges();
        }

        /// <summary>
        /// Get inner posting class that will be used during posting 
        /// </summary>
        /// <returns></returns>
        public virtual InnerPosting getInnerPosting() {
            if (p == null) return null;
            return PostData.getInnerPosting(p.DS);
        }

        /// <summary>
        /// Set inner posting messages that have already been raisen
        /// </summary>
        /// <param name="ignoredMessages"></param>
        public virtual void setInnerPosting(Hashtable ignoredMessages) {
            foreach (var s in ignoredMessages.Keys) {
                msgsToIgnore[s] = 1;
            }
        }

        /// <summary>
        /// Proxy to inner DO_POST_SERVICE
        /// </summary>
        /// <returns></returns>
        public virtual ProcedureMessageCollection DO_POST_SERVICE() {
            //effettua tutte le operazioni che avrebbe fatto
            // Il beforePost è già stato invocato correttamente
            var msg = p.DO_POST_SERVICE();
            mergeMessages(msg);
            return msg;
        }
    }
}

using System;
using System.Data;
using System.Diagnostics;

using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable IDE1006 // Naming Styles

namespace mdl
{
	/// <summary>
	/// Help class to build SQL statements
	/// </summary>
	public class QueryCreator
	{
        /// <summary>
        /// Extended property that means that the column does not really belong to 
        ///   a real table. For example, expression-like column
        /// </summary>
        private const string IsTempColumn = "IsTemporaryColumn";
		private static IMetaModel staticModel = MetaFactory.factory.getSingleton<IMetaModel>();

		//		public QueryCreator()
		//		{
		//			//
		//			// TODO: Add constructor logic here
		//			//
		//
		//		}

		/// <summary>
		/// Get a string representation of an Exception (includes InnerException)
		/// </summary>
		/// <param name="E"></param>
		/// <returns></returns>
		public static string GetErrorString(Exception E) {
		    if (E == null) return "";
            var msg = GetPrintable(E.ToString());
    //        if (E is SqlException) {
				//if (!msg.Contains(E.StackTrace))msg += E.StackTrace;
    //        }
		    //if (E.InnerException != null) {
		    //    msg += "\r\nInnerException:\r\n" + GetPrintable(E.InnerException.ToString());
		    //}
            return msg;
		}

      




        /// <summary>
        /// Check if s starts and ends with a  ( and a ) and contains a pair values of open and close parenthesis
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool isABlock(string s) {
            if (!s.StartsWith("("))return false;
            if (!s.EndsWith(")"))return false;
            return StringParser.closeBlock(s,1,'(',')')==s.Length;
        }

        /// <summary>
        /// Return an expression wrapped in parenthesis
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string putInPar(string expression) {
            if (expression == null) return expression;
            if (expression == "") return expression;
            if (isABlock(expression)) {
                return expression;
            }
            return "(" + expression + ")";
        }

		

        /// <summary>
        /// Checks that primary key of Temp is the same of Source
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Temp"></param>
        public static void CheckKey(DataTable Source, ref DataTable Temp){
			if (Source.PrimaryKey==null) return;
			if (Source.PrimaryKey.Length<1) return;
			try {
				Temp.PrimaryKey=null;
				var NewKey = new DataColumn[Source.PrimaryKey.Length];
				for (var i=0;i< Source.PrimaryKey.Length; i++){
					NewKey[i]= Temp.Columns[Source.PrimaryKey[i].ColumnName];
				}
				Temp.PrimaryKey=NewKey;
			}
			catch{
			}
		}

        /// <summary>
        /// Merge rows of a source table into an empy table
        /// </summary>
        /// <param name="emptyTable"></param>
        /// <param name="sourceTable"></param>
	    public static void MergeIntoEmptyDataTable(DataTable emptyTable, DataTable sourceTable) {
	        var handle1 = mdl_utils.metaprofiler.StartTimer($"MergeIntoEmptyDataTable * {sourceTable.TableName}");
	        try {
	            if (emptyTable.DataSet!=null){                        
	                emptyTable.BeginLoadData();
	                emptyTable.DataSet.Merge(sourceTable,false,MissingSchemaAction.Ignore);
	                emptyTable.EndLoadData();
	            }
	            else {
	                var temp = new DataSet {EnforceConstraints = false};
	                temp.Tables.Add(emptyTable);
	                emptyTable.BeginLoadData();
	                temp.Merge(sourceTable, true, MissingSchemaAction.Ignore);
	                emptyTable.EndLoadData();
	                temp.Tables.Remove(emptyTable);
					temp.Dispose();
	            }
	        }
	        catch(Exception e) {
	            ErrorLogger.Logger.markException(e,"MergeIntoEmptyDataTable");				
	        }
	        mdl_utils.metaprofiler.StopTimer(handle1);
	    }

        /// <summary>
        /// Copy All fields of source row into dest row
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
	    public static void copyRow(DataRow source, DataRow dest) {
	        DataTable destTable = dest.Table;
	        DataTable sourceTable = source.Table;
	        foreach (DataColumn dc in destTable.Columns) {
	            if (!sourceTable.Columns.Contains(dc.ColumnName)) continue;
	            if (!IsReal(dc)) continue; //if (IsTemporary(DC))continue;
	            //if (QueryCreator.IsPrimaryKey(destTable, dc.ColumnName)) continue;
	            if (!string.IsNullOrEmpty(dc.Expression)) continue;
	            var ro = dc.ReadOnly;
	            if (ro) dc.ReadOnly = false;
	            try {
	                dest[dc.ColumnName] = source[dc.ColumnName];
	            }
	            catch (Exception e) {
	                ErrorLogger.Logger.markException(e,"copyRow");
	            }
	            if (ro) dc.ReadOnly = true;
	        }
	    }

        /// <summary>
        /// Merge source into destTable searching one row at a time using Select(filterKey)
        /// </summary>
        /// <param name="destTable"></param>
        /// <param name="sourceTable"></param>
	    public static void MergeIntoDataTableRowByRow(DataTable destTable, DataTable sourceTable) {
	        var handle2 = mdl_utils.metaprofiler.StartTimer("MergeIntoDataTableRowByRow * " + sourceTable.TableName);
	        foreach (DataRow dr in sourceTable.Rows) {
	            //OutTable.ImportRow(DR);
	            //OutTable.LoadDataRow(DR.ItemArray, true);
	            var filter = QueryCreator.WHERE_REL_CLAUSE(
	                dr,
	                destTable.PrimaryKey,
	                destTable.PrimaryKey,
	                DataRowVersion.Default,
	                false);
	            DataRow myDr;
	            var found = destTable.Select(filter);
	            if ((filter != "") && (found.Length > 0)) {
	                myDr = found[0];
	                myDr.BeginEdit();
	                copyRow(dr, myDr);
	                myDr.EndEdit();
	            }
	            else {
	                myDr = destTable.NewRow();
	                copyRow(dr, myDr);
	                destTable.Rows.Add(myDr);
	            }

	            myDr.AcceptChanges();
	        }
	        mdl_utils.metaprofiler.StopTimer(handle2);
	    }

        /// <summary>
        /// Create a concatanation of all columns  value of a r
        /// </summary>
        /// <param name="r"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
	    public static string hashColumns(DataRow r,string []columns) {
	        var keys = (from string field in columns select r[field].ToString());
            return string.Join("§",keys);
	    }

	  
        /// <summary>
        /// Merge source into destTable searching one row at a time using Dictionary on keyColumns
        /// </summary>
        /// <param name="destTable"></param>
        /// <param name="sourceTable"></param>
	    public static void MergeIntoDataTableWithDictionary(DataTable destTable, DataTable sourceTable) {
	        var handle2 = mdl_utils.metaprofiler.StartTimer("MergeIntoDataTableWithDictionary * " + sourceTable.TableName);
	        var destRows = new Dictionary<string, DataRow>();
            string []keys = (from DataColumn c in destTable.PrimaryKey select c.ColumnName).ToArray();
	        foreach (DataRow r in destTable.Rows) {
	            destRows[hashColumns(r, keys)] = r;
	        }

	        foreach (DataRow dr in sourceTable.Rows) {
	            string hashSource = hashColumns(dr, keys);
                if(destRows.TryGetValue(hashSource, out var destRow)) {
                    destRow.BeginEdit();
                    copyRow(dr, destRow);
                    destRow.EndEdit();
                }
                else {
                    destRow = destTable.NewRow();
                    copyRow(dr, destRow);
                    destTable.Rows.Add(destRow);
                    destRows[hashSource] = destRow;
                }
                destRow.AcceptChanges();
	        }

	        mdl_utils.metaprofiler.StopTimer(handle2);
	    }



	    /// <summary>
		/// Merge ToMerge rows into OutTable. Tables should have a primary key
		///  set in order to use this function.
		/// </summary>
		/// <param name="outTable"></param>
		/// <param name="toMerge"></param>
		public static void MergeDataTable(DataTable outTable, DataTable toMerge) {	
			var handle= mdl_utils.metaprofiler.StartTimer("MergeDataTable");
            if ((outTable.TableName != toMerge.TableName) &&
                (toMerge.TableName == "Table")) {
                toMerge.TableName = outTable.TableName;
                toMerge.Namespace = outTable.Namespace;
            }
           
			if (toMerge.TableName== outTable.TableName && toMerge.Namespace==outTable.Namespace && outTable.Rows.Count==0) {
			    MergeIntoEmptyDataTable(outTable, toMerge);
			}
			else {
				var index = outTable?.DataSet.getIndexManager()?.getPrimaryKeyIndex(outTable);
				if (index != null) {
					MergeIntoDataTableWithIndex(outTable, toMerge,index);
				}
				else {
					if (toMerge.Rows.Count > 300 || outTable.Rows.Count > 300) {
						MergeIntoDataTableWithDictionary(outTable, toMerge);
					}
					else {
						MergeIntoDataTableRowByRow(outTable, toMerge);

					}
				}
			}

			mdl_utils.metaprofiler.StopTimer(handle);
		}

	    /// <summary>
	    /// Merge source into destTable searching one row at a time using Dictionary on keyColumns
	    /// </summary>
	    /// <param name="destTable"></param>
	    /// <param name="sourceTable"></param>
	    public static void MergeIntoDataTableWithIndex(DataTable destTable, DataTable sourceTable, IMetaIndex index) {
		    var handle2 = mdl_utils.metaprofiler.StartTimer("MergeIntoDataTableWithIndex * " + sourceTable.TableName);
		    
		    foreach (DataRow dr in sourceTable.Rows) {
			    string hashSource = index.hash.get(dr);
			    var destRow = index.getRow(hashSource);
			    if(destRow!=null) {
				    destRow.BeginEdit();
				    copyRow(dr, destRow);
				    destRow.EndEdit();
			    }
			    else {
				    destRow = destTable.NewRow();
				    copyRow(dr, destRow);
				    destTable.Rows.Add(destRow);
			    }
			    destRow.AcceptChanges();
		    }

		    mdl_utils.metaprofiler.StopTimer(handle2);
	    }

        /// <summary>
        /// Write a string to the debugger output
        /// </summary>
        /// <param name="e"></param>
		public static void MarkEvent(string e){
          ErrorLogger.Logger.markEvent(e);
        }

	    /// <summary>
	    /// Write a string to the debugger output
	    /// </summary>
	    /// <param name="e"></param>
	    public static void WarnEvent(string e){
	        //myLastError= QueryCreator.GetPrintable(e);
	        var msg = "$$"+DateTime.Now.ToString("HH:mm:ss.fff") + ":"+e;
	        Trace.WriteLine(msg);
	        Trace.Flush();
	    }

        /// <summary>
        /// Returns a printable version of a message fixing newlines to  CR LF
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
		public static string GetPrintable(string msg){
            if (msg == null) return "";
			var S= msg.Replace("\r\n","\n");
			S=S.Replace("\r","\n");
			S=S.Replace("\n","\r\n");
			return S;			
		}

	    /// <summary>
	    /// Sets a field to DBNull (or -1(int)  or 0-like values when DBNull is not allowed)
	    /// </summary>
	    /// <param name="C"></param>
	    public static object clearValue(DataColumn C) {
	        if (C.AllowDBNull) {
	            return  DBNull.Value;
	        }
	        var typename = C.DataType.Name;
	        switch (typename) {
	            case "String":
	                return "";
	            case "Char":
	                return "";
	            case "Double": {
	                return 0d;
	            }
	            case "Single": {
	                return 0f;
	            }
	            case "Decimal": {
	                return 0d;
	            }
	            case "DateTime": {
	                return mdl_utils.HelpUi.EmptyDate();
	            }
	            case "Int16":
	                return 0;
	            case "Int32":
	                return 0;
	            case "Byte":
	                return 0;	                
	            default:
	               return "";
	        }

	    }

        /// <summary>
        /// Sets a field to DBNull (or -1(int)  or 0-like values when DBNull is not allowed)
        /// </summary>
        /// <param name="R"></param>
        /// <param name="C"></param>
        [Obsolete]
        public static void ClearField(DataRow R, DataColumn C) {
            R[C] = clearValue(C);

        }

        /// <summary>
        /// Copy parent table fields to a view when possible
        /// </summary>
        /// <param name="ViewRow"></param>
        /// <param name="Parent"></param>
        /// <param name="ParentRow"></param>
		public static void CopyViewFieldFromParentTable(DataRow ViewRow, 
			DataTable Parent,
			DataRow ParentRow){
			var View= ViewRow.Table;

            var ParentName= Parent.tableForPosting();

			if (ParentName!=Parent.TableName){
                foreach(DataColumn C in View.Columns){
                    var ViewExpr = ViewExpression(C);
					if (ViewExpr==null)continue;
				
					foreach (DataColumn PC in Parent.Columns){
						var PExpr = ParentName+"."+ViewExpression(PC);
						if (PExpr==ViewExpr){
							ViewRow[C] = ParentRow == null ? clearValue(C) : ParentRow[PC];
                        }
					}							
				}
			}
			else {
				foreach(DataColumn C in View.Columns){
                    var ViewExpr = ViewExpression(C);
					if (ViewExpr==null)continue;
				
					foreach (DataColumn PC in Parent.Columns){
						var PExpr = ParentName+"."+PC.ColumnName;
						if (PExpr==ViewExpr){
                            ViewRow[C] = ParentRow == null ? clearValue(C) : ParentRow[PC];
						}
					}															
				}
			}
		}

        /// <summary>
        /// Sets to "zero" or "" all columns of a row that does not allow nulls
        /// </summary>
        /// <param name="R"></param>
        public static void ClearRow(DataRow R){
            foreach(DataColumn C in R.Table.Columns){
                if (C.AllowDBNull) continue;
                if ((R[C]!=null)&&(R[C]!=DBNull.Value)) continue;
                R[C] = clearValue(C);
            }
        }


       
		
		/// <summary>
		/// Sets the table that must be used for write values of a DataTable into DB
		/// </summary>
		/// <param name="T"></param>
		/// <param name="TableForPosting"></param>
		public static void SetTableForPosting(DataTable T, string TableForPosting){
			T.ExtendedProperties["ForPosting"]=TableForPosting;
		}

		/// <summary>
		/// Gets the table that must be used for write values of a given DataTable into DB.
		/// If a posting table has not been defined, the "Real" table (unaliased) is used.
		/// If an alias has not been defined, T.TableName is returned.
		/// </summary>
		/// <param name="T"></param>
		/// <returns>Name of Table that must be used for reading into T</returns>
		[Obsolete("Use DataTable.tableForPosting() instead")]
        public static string PostingTableName(DataTable T){
			if (T.ExtendedProperties["ForPosting"]==null) return DataAccess.GetTableForReading(T);
			return T.ExtendedProperties["ForPosting"].ToString();
		}

       
		/// <summary>
		/// To avoid posting of a field, it's posting col name must be "" (not null)
		/// </summary>
		/// <param name="C"></param>
		/// <param name="ColumnForPosting"></param>
		public static void SetColumnNameForPosting(DataColumn C, string ColumnForPosting){
			C.ExtendedProperties["ForPosting"]= ColumnForPosting;
		}

        /// <summary>
        /// Gets the filter to be used in insert operation for a table
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
		public static string GetInsertFilter(DataTable T){
			return T.ExtendedProperties["myFilterForInsert"] as string;
		}

        /// <summary>
        /// Sets the filter to be used in insert operation for a table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="S"></param>
		public static void SetInsertFilter(DataTable T,string S){
			T.ExtendedProperties["myFilterForInsert"]=S;
		}

        /// <summary>
        /// Gets the filter to be used in search operation for a table
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static string GetSearchFilter(DataTable T) {
            return T.ExtendedProperties["myFilterForSearch"] as string;
        }

        /// <summary>
        /// Sets the filter to be used in search operation for a table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="S"></param>
        public static void SetSearchFilter(DataTable T, string S) {
            T.ExtendedProperties["myFilterForSearch"] = S;
        }

        /// <summary>
        /// skip this table when insertcopy command is invoked
        /// </summary>
        /// <param name="t"></param>
        /// <param name="skip"></param>
	    public static void setSkipInsertCopy(DataTable t, bool skip) {
            t.ExtendedProperties["skipInsertCopy"] = skip;
        }
        /// <summary>
        /// check if  this table is to skip when insertcopy command is invoked
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool SkipInsertCopy(DataTable t) {
            if (t.ExtendedProperties["skipInsertCopy"] == null) return false;
            return (bool) t.ExtendedProperties["skipInsertCopy"];
        }


      
		/// <summary>
		/// Gets the Column name to use for posting a given field into DB
		/// </summary>
		/// <param name="C"></param>
		/// <returns>null if column is not for posting</returns>
		public static string PostingColumnName(DataColumn C){
			//Se non c'è ForPosting Table o PostingColumn la posting column è la stessa
			if (C.Table.ExtendedProperties["ForPosting"]==null) return C.ColumnName;
			if (C.ExtendedProperties["ForPosting"]==null) return C.ColumnName;
			if (C.ExtendedProperties["ForPosting"].ToString()=="") return null;
			//Altrimenti è la PostingColumn
			return C.ExtendedProperties["ForPosting"].ToString();
		}



		/// <summary>
		/// Gets the common child of two tables
		/// </summary>
		/// <param name="Parent1"></param>
		/// <param name="Parent2"></param>
		/// <returns>Common child Table</returns>
		public static DataTable GetMiddleTable(DataTable Parent1, DataTable Parent2){
			foreach(DataRelation R1 in Parent1.DataSet.Relations){
				if (R1.ParentTable!=Parent1) continue;
				var Middle= R1.ChildTable;
				foreach(DataRelation R2 in Parent1.DataSet.Relations){
					if((R2.ParentTable==Parent2)&&(R2.ChildTable==Middle)) return Middle;
				}
			}
			return null;
		}


		/// <summary>
		/// Sets the view Expression for a DataColumn. Ex. bilancio.codicebilancio
		/// </summary>
		/// <param name="C"></param>
		/// <param name="expr"></param>
		public static void SetViewExpression(DataColumn C, string expr){
			C.ExtendedProperties["ViewExpression"] = expr;
		}


		/// <summary>
		/// Gets the view expression assigned to a DataColumn
		/// </summary>
		/// <param name="C"></param>
		/// <returns></returns>
		public static string ViewExpression(DataColumn C){
			return (string) C.ExtendedProperties["ViewExpression"];
		}

        /// <summary>
        /// get the general condition in order to activate a datarelation during getdata phase
        /// </summary>
        /// <param name="Rel"></param>
        /// <returns></returns>
		public static string GetRelationActivationFilter(DataRelation Rel){
			if (Rel.ExtendedProperties["activationfilter"]==null) return null;
			return Rel.ExtendedProperties["activationfilter"].ToString();
		}

        /// <summary>
        /// set the general condition in order to activate a datarelation during getdata phase
        /// </summary>
        /// <param name="Rel"></param>
        /// <param name="filter"></param>
		public static void SetRelationActivationFilter(DataRelation Rel,string filter){
			Rel.ExtendedProperties["activationfilter"]=filter;
		}

        ///// <summary>
        ///// get the condition on parent in order to activate a datarelation during getdata phase
        ///// </summary>
        ///// <param name="Rel"></param>
        ///// <returns></returns>
		//public static string GetParentRelationActivationFilter(DataRelation Rel){
		//	if (Rel.ExtendedProperties["parentactivationfilter"]==null) return null;
		//	return Rel.ExtendedProperties["parentactivationfilter"].ToString();
		//}

  //      /// <summary>
  //      ///  set the condition on parent in order to activate a datarelation during getdata phase
  //      /// </summary>
  //      /// <param name="Rel"></param>
  //      /// <param name="filter"></param>
		//private static void SetParentRelationActivationFilter(DataRelation Rel,string filter){
		//	Rel.ExtendedProperties["parentactivationfilter"]=filter;
		//}


        /// <summary>
        /// Set filter to fill the table when attaced to a combobox and main row is in insert mode
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
		public static void SetFilterForInsert(DataTable T,string filter){
			T.ExtendedProperties["FilterForInsert"]=filter;
		}
        /// <summary>
        /// Get filter to fill the table when attaced to a combobox and main row is in insert mode
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
		public static string  GetFilterForInsert(DataTable T){
			if (T==null) return null;
			return T.ExtendedProperties["FilterForInsert"] as string;
		}

        /// <summary>
        /// Set filter to fill the table when attaced to a combobox and main row is in search mode
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        public static void SetFilterForSearch(DataTable T, string filter) {
            T.ExtendedProperties["FilterForSearch"] = filter;
        }

        /// <summary>
        ///  Get filter to fill the table when attaced to a combobox and main row is in search mode
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static string GetFilterForSearch(DataTable T) {
            if (T == null) return null;
            return T.ExtendedProperties["FilterForSearch"] as string;
        }


       

	
		


		/// <summary>
		/// Build a string that represents the Object O of type T. This string
		///  is built so that it can be used in a SQL instruction for assigning in
		///  VALUES lists.
		/// </summary>
		/// <param name="O">Object to display in the output string</param>
		/// <param name="T">Base Type of O</param>
		/// <returns>String representation of O</returns>
		public static string CrystalValue(Object O, System.Type T){            
			var typename= T.Name;
			switch(typename){
				case "String": 
					if (O==null) return "\"\"";
					if (O== DBNull.Value) return "\"\"";
					return "'"+O.ToString().Replace("\"","''")+"\"";
				case "Char": return "'"+O.ToString().Replace("'","''")+"'";
				case "Double": {                    
					var group = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
					var dec   = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
					var s1 = ((Double)O).ToString("n").Replace(group,"");
					return s1.Replace(dec,".");
				}
				case "Single": {                    
					var group = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
					var dec   = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
					var s1 = ((Single)O).ToString("n").Replace(group,"");
					return s1.Replace(dec,".");
				}

				case "Decimal":{
					var group = System.Globalization.NumberFormatInfo.CurrentInfo.NumberGroupSeparator;
					var dec   = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
					var s1 = ((Decimal)O).ToString("n").Replace(group,"");
					return s1.Replace(dec,".");
				}
				case "DateTime":{
					var TT = (DateTime) O; //Convert.ToDateTime(s);                    
					return "DateTime("+

						TT.Month.ToString()+ "/"+ TT.Day.ToString() + "/" +
						TT.Year.ToString()+" "+TT.Hour.ToString()+":"+
						TT.Minute.ToString()+":"+TT.Second.ToString()+"."+
						TT.Millisecond.ToString().PadLeft(3,'0');
				}
				case "Int16": return O.ToString();
				case "Int32": return O.ToString();
				default: return O.ToString();
			}          
		}


		

        /// <summary>
        /// Returns a SQL condition that tests a field name to be EQUAL to an 
        ///  object O of type T. 
        /// </summary>
        /// <param name="fieldname">Name of the field that appears in the result</param>
        /// <param name="O">Value to compare with the field</param>
        /// <param name="T">Base type of O</param>
        /// <param name="SQL">if true, SQL compatible strings are used</param>
        /// <returns>"(fieldname='...')" or "(fieldname IS NULL)"</returns>
        static string comparefields(string fieldname, 
            object O, System.Type T, bool SQL){
            if (O==DBNull.Value) return "("+fieldname+" IS NULL)";
            return "("+fieldname+" = "+ mdl_utils.Quoting.quotedstrvalue(O, T,SQL)+")";
        }


        /// <summary>
        ///  Returns a SQL condition that tests a field name to be LIKE an 
        ///  object O of type T, if the string representation of O contains
        ///  a % character, or EQUAL if it does not happen.
        /// </summary>
        /// <param name="fieldname">Name of the field that appears in the result</param>
        /// <param name="O">Value to compare with the field</param>
        /// <param name="T">Base type of O</param>
        /// <param name="SQL">if true, a SQL string is returned</param>
        /// <returns>"(fieldname='...')" or "(fieldname LIKE '...')"</returns>
        /// <remarks>Does not manages NULL values of O</remarks>
        public static string comparelikefields(string fieldname, 
            object O, System.Type T, bool SQL){
            var s=mdl_utils.Quoting.quotedstrvalue(O, T, SQL);
            if (s.IndexOf("%")>=0) 
                return "(" + fieldname + " LIKE " +s+ ")";
            else
                return "("+fieldname+" = "+s+")";
        }
        
		static string ColName(DataColumn C, bool forposting){
			if (!forposting) return C.ColumnName;
			return PostingColumnName(C);
		}

        /// <summary>
        /// Creates a string like: (field1='..') and (field2='..') ...
        ///   for all real (not expression or temporary) fields of a datarow
        ///   if forposting=true uses posting-columnnames for columnsnames
        /// </summary>
        /// <param name="R">Row to consider for the values to compare</param>
        /// <param name="ver">Version of Data in the row to consider</param>
        /// <param name="forposting">if posting table/columns must be used</param>
        /// <param name="SQL">if true, SQL representation for values are used</param>
        /// <returns>condition string on all values</returns>
        static public string WHERE_CLAUSE (DataRow R, DataRowVersion ver, bool forposting, bool SQL){
            var T = R.Table;
            var outstring = "";
            var first=true;
            foreach (DataColumn C in T.Columns){
				if (!IsRealColumn(C)) continue;
                var colname = ColName(C, forposting);
				if (colname==null) continue;
				if ((C.ExtendedProperties["sqltype"]==null)
						||(C.ExtendedProperties["sqltype"].ToString()!="text")){
					if (first)
						first=false;
					else
						outstring += " AND ";
					outstring += comparefields(colname, 
						R[C,ver]
						, C.DataType,
						SQL);               
				}
            }
            return outstring;
        }



		/// <summary>
		/// Gets a filter string that compares given fields. For examle: (name='nino')
		/// </summary>
		/// <param name="R">DataRow to use for getting values for comparisons</param>
		/// <param name="Colnames">array of field names of column to compare</param>
		/// <param name="ver">row version to use for getting values from R</param>
		/// <param name="sql">if true, SQL compatible constants are used</param>
		/// <returns></returns>
		static public string WHERE_COLNAME_CLAUSE (DataRow R, string []Colnames, 
			DataRowVersion ver, bool sql){
			var T = R.Table;
			var outstring = "";
			var first=true;
			foreach (var col in Colnames){
				var C = T.Columns[col];
				if (C==null) {
					ErrorLogger.Logger.markEvent($"Column {col} was not found in DataTable {T.TableName}");
					continue;
				}
				if (!IsRealColumn(C)) continue;
				if (first)
					first=false;
				else
					outstring += " AND ";
				outstring += comparefields(col, 
					R[C,ver]
					, C.DataType, sql);               
			}
			return outstring;

		}

        
        /// <summary>
        /// Creates a string of type (field1='..') and (field2='..') ... for all
        ///   the keyfields of the Primary Key of a datarow
        /// </summary>
        /// <param name="r">Row to use for getting values to compare</param>
        /// <param name="ver">Version of the DataRow to use</param>
        /// <param name="sql">if true, SQL compatible string values are used</param>
        /// <returns></returns>
        static public string WHERE_KEY_CLAUSE (DataRow r, DataRowVersion ver, bool sql){
            var T = r.Table;
            return WHERE_REL_CLAUSE(r, T.PrimaryKey, T.PrimaryKey, ver, sql);
        }


        /// <summary>
        /// Creates a string of type (field1='..') and (field2='..') ... for all
        ///   the fields specified by a DataColumn Collection
        /// </summary>
        /// <param name="ValueRow">Row to use for getting values to compare</param>
        /// <param name="ValueCol">RowColumns of ParentRow from which values to be
        ///     compare have to be taken</param>
        /// <param name="FilterCol">RowColumn of ChildRows for which the Column NAMES have
        ///   to be taken</param>
        /// <param name="ver">Version of ParentRow to consider</param>
        /// <param name="SQL">if true, SQL representation of values are used</param>
        /// <returns>SQL comparison string on all fields</returns>
        static public string WHERE_REL_CLAUSE(DataRow ValueRow, 
            DataColumn[] ValueCol, 
            DataColumn[] FilterCol, 
            DataRowVersion ver,
			bool SQL){
            var outstring = "";
            var first=true;
            for (var i=0; i< ValueCol.Length; i++){
                var valueColumn = ValueCol[i];
                var filterColumn  = FilterCol[i];
                if (first)
                    first=false;
                else
                    outstring += " AND ";

				var fieldname = filterColumn.ColumnName;
				if (SQL) fieldname= PostingColumnName(filterColumn);
				var val = ValueRow[valueColumn.ColumnName,ver];
				outstring += comparefields(
						fieldname, 
						val,
                        filterColumn.DataType, 
						SQL);               
			}
            return outstring;

        }


        /// <summary>
        /// Gets a multicompare that connects a parentrow to his child   with a specified set of parent/child columns
        /// </summary>
        /// <param name="ParentRow"></param>
        /// <param name="ParentCol"></param>
        /// <param name="ChildCol"></param>
        /// <param name="ver"></param>
        /// <param name="SQL"></param>
        /// <returns></returns>
        static public MultiCompare GET_MULTICOMPARE(DataRow ParentRow,
            DataColumn[] ParentCol,
            DataColumn[] ChildCol,
            DataRowVersion ver, bool SQL) {
            var val = new object[ParentCol.Length];
            var fields = new string[ParentCol.Length];
            for (var i = 0; i < ParentCol.Length; i++) {
                var Parent = ParentCol[i];
                var Child = ChildCol[i];
              
                var fieldname = Child.ColumnName;
                if (SQL) fieldname = QueryCreator.PostingColumnName(Child);
                val[i] = ParentRow[Parent.ColumnName, ver];
                fields[i] = fieldname;
            }
            return new MultiCompare(fields, val);

        }

		/// <summary>
		/// Gets the expression that has been assigned to a DataColumn
		/// </summary>
		/// <param name="C"></param>
		/// <returns></returns>
        public static string GetExpression(DataColumn C){
		    if (!(C.ExtendedProperties[IsTempColumn] is string))return null;
			var s= C.ExtendedProperties[IsTempColumn].ToString();
			if (s=="") return null;
            return s;
        }


	    /// <summary>
	    /// Gets the expression that has been assigned to a DataColumn
	    /// </summary>
	    /// <param name="C"></param>
	    /// <returns></returns>
	    public static MetaExpression GetMetaExpression(DataColumn C){
	        return C.ExtendedProperties[IsTempColumn] as MetaExpression;
	    }



		/// <summary>
		/// Assign an expression to a given DataColumn. After this operation,
		///  the DataColumn is no longer considered "real"
		/// </summary>
		/// <param name="C"></param>
		/// <param name="S"></param>
        public static void SetExpression(DataColumn C, string S){
            C.ExtendedProperties[IsTempColumn]=S;
        }


		/// <summary>
		/// Tells if a column is temporary, i.e. is not real.
		/// </summary>
		/// <param name="C"></param>
		/// <returns></returns>
        public static bool IsTemporary(DataColumn C){
            if (C.ColumnName.StartsWith("!")) return true;
            if (C.ExtendedProperties[IsTempColumn]==null) return false;
            return true;
        }

		
		/// <summary>
		///  Determines wheter a DataColumn is real or not. For example,
		///  columns that have been assigned expressions are not real.
		///  Also, columns whose name starts with "!" are considered not real.
		///  If a column is not real, it is never read/written to DB
		/// </summary>
		/// <param name="C"></param>
		/// <returns></returns>
        public static bool IsReal(DataColumn C){
            if (C.ColumnName.StartsWith("!")) return false;
            if (C.ExtendedProperties[IsTempColumn]!=null) return false;
            return true;
        }


        /// <summary>
        /// Checks a column to be "real", i.e. not Temporary, and with a null or
        ///  empty expression.
        /// </summary>
        /// <param name="C">Column to check</param>
        /// <returns>true if column is real</returns>
        public static bool IsRealColumn(DataColumn C){
            if (IsTemporary(C)) return false;
            if (C.Expression==null) return true;
            if (C.Expression=="") return true;
            return false;
        }
        

        /// <summary>
        /// Creates a string of type (field1 LIKE '..') and (field2 LIKE'..') ... for all
        ///   the keyfields of a datarow
        /// </summary>
        /// <param name="R">Row from where values to compare have to be taken</param>
        /// <param name="ver">RowVersion to consider for R</param>
        /// <param name="SQL">if true, SQL compatible string constants are used</param>
        /// <returns>SQL "LIKE" comparison  string on all fields</returns>
        static public string WHERE_LIKE_CLAUSE (DataRow R, DataRowVersion ver, bool SQL){
            var T = R.Table;
            var outstring = "";
            var first=true;
            foreach (DataColumn C in T.Columns){
                if (!IsRealColumn(C)) continue;
                if (R[C, ver] == DBNull.Value) continue;
                if (R[C, ver] == null) continue;
                if (R[C, ver].ToString() == "") continue;
                if (R[C, ver].Equals(mdl_utils.HelpUi.EmptyDate())) continue;
                if (first)
                    first=false;
                else
                    outstring += " AND ";
                outstring += comparelikefields(C.ColumnName, 
                    R[C,ver]
                    , C.DataType, 
					SQL);               
            }
            return outstring;
        }



        /// <summary>
        /// Checks if any of specified columns of a specified version of R
        ///  contains null or DBNull values or empty strings
        /// </summary>
        /// <param name="R"></param>
        /// <param name="Cols"></param>
        /// <param name="ver"></param>
        /// <returns>true if some value is null</returns>
        static public bool ContainsNulls(DataRow R, DataColumn[] Cols, 
             DataRowVersion ver){
            foreach (var C in Cols){
                if (R[C,ver]==null) return true;
                if (R[C,ver]==DBNull.Value) return true;
				if (R[C,ver].ToString()=="") return true;
            }
            return false;
        }
        

        /// <summary>
        /// Get the list of real (not temporary or expression) columns of a Row R
        ///  formatting it like "field1, field2,...."
        /// </summary>
        /// <param name="R"></param>
        /// <returns>row real column list</returns>
        static public string ColumnNameList(DataRow R){
            return ColumnNameList(R.Table);
        }


        /// <summary>
        /// Get the list of real (not temporary or expression) columns NAMES of a table T
        ///  formatting it like "fieldname1, fieldname2,...."
        /// </summary>
        /// <param name="T">Table to scan for columns</param>
        /// <returns>table real column list</returns>
        static public string ColumnNameList(DataTable T){
            var outstring = "";
            var first=true;
            foreach (DataColumn C in T.Columns){
                if (!IsRealColumn(C)) continue;
                if (first)
                    first=false;
                else
                    outstring += ",";
                outstring += C.ColumnName;
            }
            return outstring;
        }

		
		/// <summary>
		/// Get the list of real (not temporary or expression) columns NAMES of a table T
		///  formatting it like "fieldname1, fieldname2,...."
		/// </summary>
		/// <param name="T">Table to scan for columns</param>
		/// <returns>table real column list</returns>
		static public string SortedColumnNameList(DataTable T){
			
			var outstring = "";
			var first=true;
			var L= new DataColumn[T.Columns.Count];
			var N=0;
			foreach(DataColumn C in T.Columns){
				if (C.ExtendedProperties["ListColPos"]==null)continue;
				var currpos= Convert.ToInt32(C.ExtendedProperties["ListColPos"]);
				if (currpos<0) continue;
				//cerca la posizione i dove mettere la colonna (ord.crescente)
				var i=0;
				while (i<N){
					var ThisCol=L[i];
					var thispos= Convert.ToInt32(ThisCol.ExtendedProperties["ListColPos"]);
					if (thispos>currpos) break;
					i++;
					continue;
				}
				//shifta tutti gli elementi da i+1 in poi in avanti
				if (i<N){
					for (var j=N;j>i;j--) L[j]=L[j-1];
				}
				L[i]=C;
				N++;
			}
			foreach(DataColumn C in T.Columns){
				if (C.ExtendedProperties["ListColPos"]!=null){
					var currpos= Convert.ToInt32(C.ExtendedProperties["ListColPos"]);
					if (currpos>=0) continue;
				}
				L[N]=C;
				N++;
			}

			foreach (var C in L){
				if (!IsRealColumn(C)) continue;
				if (first)
					first=false;
				else
					outstring += ",";
				outstring += C.ColumnName;
			}
			return outstring;
		}





        /// <summary>
        /// Gets the quoted values of a field in a set of rows
        /// </summary>
        /// <param name="ROWS"></param>
        /// <param name="column"></param>
        /// <param name="SQL"></param>
        /// <returns></returns>
		static public string ColumnValues(DataRow []ROWS, string column, bool SQL){

			var outstring="";
            if (ROWS.Length <= 100) {
                foreach (var R in ROWS) {
                    var quoted = mdl_utils.Quoting.quotedstrvalue(R[column], SQL);
                    if (outstring.IndexOf(quoted) > 0) continue;
                    outstring += "," + quoted;
                }
            }
            else {
                var SS = new StringBuilder();
                var HH = new Hashtable();
                foreach (var R in ROWS) {
                    var quoted = mdl_utils.Quoting.quotedstrvalue(R[column], SQL);
                    if (HH[quoted] != null) continue;
                    HH[quoted] = 1;
                    SS.Append("," + quoted);
                }
                outstring = SS.ToString();
            }
            if (outstring != "") outstring = outstring.Substring(1);
            return outstring;
		}



        /// <summary>
        /// Gets the quoted values of a field in the current rows of a specified table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        /// <param name="column"></param>
        /// <param name="SQL"></param>
        /// <returns></returns>
		static public string ColumnValues(DataTable T, string filter, string column, bool SQL){
			return ColumnValues(T.Select(filter),column,SQL);
		}

        /// <summary>
        /// Get the list of real (not temporary or expression) columns VALUES of a table T
        ///  formatting it like "fieldvalue1, fieldvalue2,...."
        /// </summary>
        /// <param name="R">Row from which take values</param>
        /// <param name="ver">version of the row to consider</param>
        /// <returns>row real value list</returns>
        static public string ValueListVersion(DataRow R, DataRowVersion ver){
            var T = R.Table;
            var outstring = "";
            var first=true;
            foreach (DataColumn C in T.Columns){
                if (!IsRealColumn(C)) continue;
                if (first)
                    first=false;
                else
                    outstring += ",";
                outstring += mdl_utils.Quoting.quotedstrvalue(R[C,ver], C.DataType, true);
            }
            return outstring;
        }

		/// <summary>
		/// Tells wheter a field belongs to primary key
		/// </summary>
		/// <param name="T"></param>
		/// <param name="field"></param>
		/// <returns>true if field belongs to primary key of T</returns>
        public static bool IsPrimaryKey(DataTable T, string field){
            foreach(var C in T.PrimaryKey){
                if (C.ColumnName==field) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if Parent table is related with KEY fields of Child table
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Child"></param>
        /// <returns></returns>
        public static bool CheckKeyParent(DataTable Parent, DataTable Child){
            //finds relation
            DataRelation Rfound=null;
            foreach (DataRelation R in Parent.ChildRelations){
                if (R.ChildTable.Equals(Child)) {
                    Rfound=R;
                    break;
                }
            }
            if (Rfound==null) return false;
            
            foreach(var C in Rfound.ChildColumns){
                if (IsPrimaryKey(Child, C.ColumnName)) return true;
            }
            return false;
        }

		/// <summary>
		/// Gets the relation that links a Parent Table with a Child Table
		/// </summary>
		/// <param name="Parent"></param>
		/// <param name="Child"></param>
		/// <returns>DataRelation or null if the relation does not exists</returns>
        public static DataRelation GetParentChildRel(DataTable Parent, DataTable Child){
			if (Parent==null) return null;
			if (Child==null) return null;
            foreach (DataRelation R in Parent.ChildRelations){
                if (R.ChildTable.TableName == Child.TableName) return R;
            }
            return null;
        }

//		public static SqlDbType SqlReliableType(System.Type T){
//			switch (T.Name){
//				case "DateTime": return SqlDbType.VarChar;
//			}
//			return SqlDbType.VarChar;
//		}
//
		/// <summary>
		/// Tells whether a Child Table is a Sub-Entity of Parent Table.
		/// This is true if:
		/// Exists some relation R that links primary key of Parent to a subset of the 
		///  primary key of Child
		/// </summary>
		/// <param name="Child"></param>
		/// <param name="Parent"></param>
		/// <returns></returns>
		public static bool IsSubEntity(DataTable Child, DataTable Parent){
			foreach (DataRelation Rel in Parent.ChildRelations){
				if (Rel.ChildTable.TableName != Child.TableName) continue;
				if (staticModel.isSubEntityRelation(Rel)) return true;
			}
			return false;
		}

		

		[Obsolete ("Use Model.isSubEntity(DataRelation R)")]
		/// <summary>
		/// Checks that a child table (that represents a sub-entity) is a sub-entity
		///  of primary table entity.
		///  This is true if:
		///  R relates primary key of Parent with a subset of the primary key of Child
		/// </summary>
		/// <param name="R"></param>
		/// <param name="Child">Table to check as sub-entity</param>
		/// <param name="Parent">Table considered the "entity"</param>
		/// <returns>true when child is sub-entity of Parent</returns>
		public static bool IsSubEntity(DataRelation R, DataTable Child, DataTable Parent) {
			if (R.ExtendedProperties["isSubentity"] != null) return (bool) R.ExtendedProperties["isSubentity"];
			if (R.ParentTable!=Parent) return false;
			if (R.ChildTable!=Child) return false;
			
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


	}
}

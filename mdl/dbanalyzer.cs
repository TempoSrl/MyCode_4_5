using System;
using System.Data;
using System.Collections;
using q = mdl.MetaExpression;
using System.Threading.Tasks;

namespace mdl
{
    
	/// <summary>
	/// Summary description for dbanalyzer.
	/// </summary>
	public class dbanalyzer
	{
			/// <summary>
		/// Gets object description from db filtering on xtype field
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="kind">xtype parameter, use U for tables, V for views</param>
		/// <returns>list of specified db object names</returns>
            public static async Task<ArrayList> ObjectListFromDB(DataAccess conn,string kind){
			
			var list = await conn.Select("sysobjects",columnlist:"name",filter:"(xtype='"+kind+"')");
			var outlist= new ArrayList(list.Rows.Count);
			foreach(DataRow r in list.Rows){
                if (!outlist.Contains(r["name"].ToString()))
				    outlist.Add(r["name"]);
			}
			return outlist;
		}

		/// <summary>
		/// Returns an arraylist of names of DataBase (real) tables 
		/// </summary>
		/// <param name="conn"></param>
		/// <returns></returns>
		public static async Task<ArrayList> TableListFromDB(DataAccess conn){
			return await ObjectListFromDB(conn, "U");
		}

		/// <summary>
		/// Returns an arraylist of names of DataBase Views
		/// </summary>
		/// <param name="conn"></param>
		/// <returns></returns>
		public static async Task<ArrayList> ViewListFromDB(DataAccess conn){
			return await ObjectListFromDB(conn, "V");
		}



		/// <summary>
		/// Gets the names of primary keys field of a DB table querying the DB by sp_pkeys
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="conn"></param>
		/// <returns>null on errors</returns>
		public static async Task<ArrayList> GetPrimaryKey(string tableName,DataAccess conn) {
			var myKey = new ArrayList();
			//"sp_pkeys @table_name = 'tabella', @table_owner = 'dbo',@table_qualifier = 'database'";
			var cmdText = $"sp_pkeys @table_name = '{tableName}',@table_qualifier = '{conn.Security.GetSys("database")}'";
			var keyNames = await conn.ExecuteQuery(cmdText);
			if (keyNames==null)  return null;

			foreach(DataRow keyName in keyNames.Rows) {
				myKey.Add(keyName["Column_Name"].ToString());
			}
			return myKey;
		}


		

		/// <summary>
		/// Gets from DataBase info about a table, and add/update/deletes 
		///   ColTypes to reflect info read
		/// </summary>
		/// <param name="colTypes"></param>
		/// <param name="tableName"></param>
		/// <param name="conn"></param>
		/// <returns>true when successfull</returns>
		public static async Task< bool> ReadColumnTypes(DataTable colTypes, string tableName, DataAccess conn){

			//Reads table structure through MShelpcolumns Database call
			var cmdText2 = "exec sp_MShelpcolumns N'" + tableName + "'";
			DataTable dtColumns = await conn.ExecuteQuery(cmdText2);			
			if (dtColumns==null) return false;				
		
			var tableKeys = await GetPrimaryKey(tableName,conn);
			if (tableKeys==null) return false;

			//columns returned are:
			//col_name, col_len, col_prec, col_scale, col_basetypename, col_defname,
			// col_rulname, col_null, col_identity, col_flags, col_seed,
			// col_increment col_dridefname, text, col_iscomputed text, col_NotForRepl,
			// col_fulltext, col_AnsiPad, col_DOwner, col_DName, 
			// col_ROwner, col_RName

			//Per ogni riga della tabella, ovvero per ogni colonna presente nella tabella TableName
			foreach(DataRow myDr in dtColumns.Rows) {
				var colname = myDr["col_name"].ToString();
				//var mySelect = $"(tablename = '{tableName}') AND (field = '{colname}')";
				DataRow[] myDrSelect = colTypes.filter(q.mCmp(new {tablename=tableName,field=colname}));					
				DataRow currCol = null;
				var toadd=false;
				if(myDrSelect.Length == 0) {
					currCol = colTypes.NewRow();
					currCol["tablename"] = tableName; 
					currCol["field"] = colname;
					currCol["defaultvalue"] = "";
					toadd=true;
				}
				else {
					currCol = myDrSelect[0];
				}

				currCol["sqltype"] = myDr["col_typename"];
				currCol["systemtype"] = GetType_Util.GetSystemType_From_SqlDbType(myDr["col_typename"].ToString());
				currCol["col_len"] = myDr["col_len"];
				if(myDr["col_null"].ToString() == "False"){
					currCol["allownull"]="N";
					if (toadd) currCol["denynull"]="S";
				}
				else {
					currCol["allownull"]="S";
					if (toadd) currCol["denynull"]="N";
				}
				currCol["col_precision"] = myDr["col_prec"];
				currCol["col_scale"] = myDr["col_scale"];
				var sqlDecl = myDr["col_typename"].ToString();
                if ((sqlDecl == "varchar") || (sqlDecl == "char") || (sqlDecl == "nvarchar") || (sqlDecl == "nchar")
                    || (sqlDecl == "binary") || (sqlDecl == "varbinary")
                    ) {
                    if ((myDr["col_len"].ToString() == "-1")|| (myDr["col_len"].ToString() == "0")) {
                        sqlDecl += "(max)";
                    }
                    else {
                        sqlDecl += $"({myDr["col_len"]})";
                    }
				}
				if (sqlDecl == "decimal"){
					sqlDecl += $"({myDr["col_prec"]},{myDr["col_scale"]})"; 
				}
																			 
				currCol["sqldeclaration"] = sqlDecl; 
				var isKey = "N";				
				foreach(string colName in tableKeys) {
				    if (myDr["col_name"].ToString() != colName) continue;
				    isKey = "S";
				    break;
				}
				currCol["iskey"] = isKey;					
				if(toadd) colTypes.Rows.Add(currCol);
			}

			foreach (var existingCol in colTypes.Select()){
				//var rSelect = "(col_name='"+existingCol["field"]+"')";
				DataRow []exists = dtColumns.filter(q.eq("col_name",existingCol["field"]));
				if (exists.Length>0) continue;
				existingCol.Delete();
			}

			return true;
		}

		/// <summary>
		/// Export Data from a DB table to an XML file. The XML also contains 
		/// extended informations that allow re-creating the table into another DB
		/// </summary>
		/// <param name="filename">XML filename to create</param>
		/// <param name="tablename"></param>
		/// <param name="conn"></param>
		/// <param name="filter"></param>
		/// <returns>true if OK</returns>
		public static async Task<bool> ExportTableToXML(string filename, string tablename,
				DataAccess conn, string filter) {
			DataSet ds = new DataSet();
			DataTable T = await conn.CreateTable(tablename,null);
			await DataAccess.addExtendedProperty(conn,T);
			ds.Tables.Add(T);

            //reads table
            await conn.SelectIntoTable(T,filter:filter);

			try {
				ds.WriteXml(filename, XmlWriteMode.WriteSchema);
			}
			catch(Exception e) {
				MetaFactory.factory.getSingleton<IMessageShower>().Show($"Couldn't write to file {filename} - {e.ToString()}","ExportTableToXML");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Export an entire DataSet to XML file. The XML (on request) also contains 
		/// extended informations that allow re-creating the table into another DB
		/// </summary>
		/// <param name="filename">XML file name to be created</param>
		/// <param name="conn"></param>
		/// <param name="ds">DataSet to Export</param>
		/// <param name="addExtedendProperties">when true, extended information is added 
		///  to the DataSet in order to allo all tables to be re-generated in the
		///  target DB</param>
		/// <returns></returns>
		public static bool ExportDataSetToXML(string filename, DataAccess conn, 
			DataSet ds, bool addExtedendProperties){			
			DataSet myDS= ds.Copy();
			myDS.copyIndexFrom(ds);
			if (addExtedendProperties){
				foreach(DataTable T in myDS.Tables) DataAccess.addExtendedProperty(conn,T);
			}
			try {
				myDS.WriteXml(filename, XmlWriteMode.WriteSchema);
			}
			catch(Exception E) {
			    MetaFactory.factory.getSingleton<IMessageShower>().Show(null, $"Couldn't write to file {filename} - {E.ToString()}", "ExportDataSetToXML");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Reads a DataSet from an XML file and returns it
		/// </summary>
		/// <param name="filename">XML filename to read</param>
		/// <param name="conn"></param>
		/// <param name="DS">returned DataSet (empty on errors)</param>
		/// <returns>true if Ok</returns>
		public static bool ImportDataSetFromXML(string filename, 
					DataAccess conn, 
					out DataSet DS){
			DS = new DataSet();
			try {
				DS.ReadXml(filename, XmlReadMode.ReadSchema);
			}
			catch(Exception E) {
			    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,
                    $"Couldn't read from file {filename} - {E}", "ImportDataSetFromXML");
				return false;
			}
			return true;
		}
	

		
		/// <summary>
		/// Returns a representation of all real tables of the DB.  Foreach DB
		///  table, a corresponding table is created in DS (without extended properties)
		/// </summary>
		/// <param name="Conn"></param>
		/// <returns></returns>
		public static async Task<DataSet> GetOverallDataSet(DataAccess Conn){
			ArrayList TableList = await TableListFromDB(Conn);
			DataSet DS= new DataSet("OverAll");
			foreach (string TableName in TableList){
				DataTable T = await Conn.CreateTable(TableName);
				DS.Tables.Add(T);
			}
			return DS;
		}


		/// <summary>
		/// Reads data from a XML table into a DataSet 
		/// </summary>
		/// <param name="filename">Esempio C:\\cartella\\nomefile.xml</param>
		/// <param name="DS"></param>
		/// <returns></returns>
		public static bool ImportTableFromXML (string filename, out DataSet DS){
			DS = new DataSet();
			try {	
				DS.ReadXml(filename,XmlReadMode.ReadSchema);
			}
			catch(Exception E) {			    
			    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,
			        $"Impossibile leggere il file {filename} - {E.ToString()}","ImportTableFromXML");
				return false;
			}
			return true;			
		}//Fine ImportTableFromXML


		/// <summary>
		/// Write a DataTable into a DB table with the same name. If the db table
		///  does not exist or miss some columns, it is created or columns
		///  are added to dbtable
		/// </summary>
		/// <param name="T">Table to store in the DB</param>
		/// <param name="Conn"></param>
		/// <param name="Clear">when true, table is cleared before being written</param>
		/// <param name="Replace">when true, rows with matching keys are update. When false,
		///  those are skipped</param>
		/// <param name="filter">condition to apply on data in DataTable</param>
		/// <returns>true if OK</returns>
		public static async Task<bool> WriteDataTableToDB(DataTable T, DataAccess Conn, 
			bool Replace, string filter){
			DataSet Existent = new DataSet();
			DataTable ExistentTable = T.Clone();
			Existent.Tables.Add(ExistentTable);
//			GetData get2= new GetData();
//			get2.InitClass(Existent, Conn, ExistentTable.TableName);

			if (!await CheckDbTableStructure(T.TableName, T, Conn))return false;
			
			//ArrayList MyArraySkipRows = new ArrayList();
			
			//Takes all rows from T that satisfy the filter condition
			DataRow []Filtered = T.Select(filter);
			
			//Per ogni riga della tabella T
			foreach(DataRow DR in Filtered) {


                
                    //Controlla se è gia presente nella tabella del DB una riga con chiave uguale
                    var WhereKey = q.keyCmp(DR);
                    int count = await Conn.Count(T.TableName, filter:WhereKey);


                    //if a row exists, update it
                    if (count > 0) {
                        if (!Replace) continue;
                        await Conn.SelectIntoTable(ExistentTable, filter: WhereKey);
                        DataRow Curr = ExistentTable.filter(WhereKey)[0];
                        foreach (DataColumn C in ExistentTable.Columns) {
                            Curr[C.ColumnName] = DR[C.ColumnName];
                        }
                        continue;
                    }
                
			
				//insert new row
				DataRow newR= ExistentTable.NewRow();
				foreach (DataColumn C in ExistentTable.Columns){
					newR [C.ColumnName]= DR[C.ColumnName];
				}
				ExistentTable.Rows.Add(newR);
			}
			PostData post = new PostData();
			await post.InitClass(Existent, Conn);
			return post.InteractiveSaveData();

		}

		/// <summary>
		/// Apply the structure of DS tables to the DB (create tables, adds columns)
		/// This function does not delete columns or rows. No data is written to db.
		/// Only DB schema is (eventually) modified.
		/// </summary>
		/// <param name="DS"></param>
		/// <param name="Conn"></param>
		/// <returns></returns>
		public static async Task<bool> ApplyStructureToDB(DataSet DS, DataAccess Conn){
			if(DS == null)return false;
			foreach (DataTable T in DS.Tables){
				if (! await CheckDbTableStructure(T.TableName, T, Conn))return false;
			}
			return true;
		}

		/// <summary>
		/// Writes all data in DS into corresponding DB tables. If some DB table
		///  does not exist or misses come columns, it is created or columns are
		///  added to it.
		/// </summary>
		/// <param name="DS"></param>
		/// <param name="Conn"></param>
		/// <param name="clear"></param>
		/// <param name="Replace"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		public static async Task<bool> WriteDataSetToDB(DataSet DS, DataAccess Conn, 
			 bool Replace, string filter){

			if(DS == null)return false;
			foreach (DataTable T in DS.Tables){
				bool res = await WriteDataTableToDB(T, Conn,  Replace, filter);
				if (!res) return false;
			}
			return true;
		}


		/// <summary>
		/// Check that tablename exists on DB and that has all necessary fields.
		/// If it does not exist, it is created. If it lacks some fields,
		///  those are added to table
		/// </summary>
		/// <param name="tablename">Name of table to be checked fo existence</param>
		/// <param name="T">Table (extended with schema info)  to check</param>
		/// <param name="Conn"></param>
		/// <returns>true when successfull</returns>
		public static async Task<bool> CheckDbTableStructure(string tablename, DataTable T, DataAccess Conn){
			if (!await TableExists(tablename, Conn))	return await CreateTableLike(tablename, T, Conn);

			//Reads table structure through MShelpcolumns Database call
//			string CmdText2 = "exec sp_MShelpcolumns N'[dbo].[" + tablename + "]'";
//			DataTable ExistingColumns = Conn.SQLRunner(CmdText2);			
			//columns returned are:
			//col_name, col_len, col_prec, col_scale, col_basetypename, col_defname,
			// col_rulname, col_null, col_identity, col_flags, col_seed,
			// col_increment col_dridefname, text, col_iscomputed text, col_NotForRepl,
			// col_fulltext, col_AnsiPad, col_DOwner, col_DName, 
			// col_ROwner, col_RName


			ArrayList ToAdd= new ArrayList();
			foreach(DataColumn C in T.Columns){
				if (await ColumnExists(tablename, C.ColumnName, Conn)) {
					//CheckColumnType(tablename, C.ColumnName,Conn, ExistingColumns);
					continue;
				}
				ToAdd.Add(C.ColumnName);
			}
			if (ToAdd.Count==0) return true;
			string [,] cols = new string[ToAdd.Count,3];
			for (int i=0; i< ToAdd.Count; i++){
				string ColToAddName= ToAdd[i].ToString();
				cols[i,0] = T.Columns[ColToAddName].ExtendedProperties["field"].ToString();
				cols[i,1] = T.Columns[ColToAddName].ExtendedProperties["sqldeclaration"].ToString();
				if (T.Columns[ColToAddName].ExtendedProperties["allownull"].ToString().ToUpper()=="S")	
					cols[i,2]= "NULL";
				else
					cols[i,2]= "NOT NULL";				
			}
			return await AddColumns(tablename, cols, Conn);
		}

		//void CheckColumnType(string tablename, 
		//				string columnname, 
		//				DataAccess Conn,
		//				DataTable ExistingColumns){

		//}

		/// <summary>
		/// Create a DB table like a given DataTable
		/// </summary>
		/// <param name="tablename">name of table to create</param>
		/// <param name="T">DataTable with DataColumn-extended info about DB schema</param>
		/// <param name="Conn"></param>
		/// <returns>true when successfull</returns>
		public static async Task<bool> CreateTableLike(string tablename, DataTable T, DataAccess Conn){
			int colcount = T.Columns.Count;
			string [,]cols = new string[colcount, 3];
			for (int i=0; i< colcount; i++){
				cols[i,0] = T.Columns[i].ExtendedProperties["field"].ToString();
				cols[i,1] = T.Columns[i].ExtendedProperties["sqldeclaration"].ToString();
				if (T.Columns[i].ExtendedProperties["allownull"].ToString().ToUpper()=="S")	
					cols[i,2]= "NULL";
				else
					cols[i,2]= "NOT NULL";				
			}
			string [] key= new string[T.PrimaryKey.Length];
			for (int i=0; i< key.Length; i++) key[i] = T.PrimaryKey[i].ColumnName;
			return await CreateTable(tablename, cols,key, Conn);
		}

		/// <summary>
		/// Verify the existence of a Table
		/// </summary>
		/// <param name="TableName">Name of the Table</param>
		/// <param name="Conn"></param>
		/// <returns>True when Table exists </returns>
		public static async Task<bool> TableExists(string TableName, DataAccess Conn) {
			string Sql= $"select count(*) from [dbo].[sysobjects] where id = object_id(N'[{TableName}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
			int N = Convert.ToInt32(await Conn.ExecuteScalar(Sql));
			if(N  == 0) {
				return false;
			}
			else {
				return true;
			}
		}

		/// <summary>
		/// Verify the existence of a column in a DB table. Makes use of db system tables.
		/// </summary>
		/// <param name="TableName"></param>
		/// <param name="ColumnName"></param>
		/// <param name="Conn"></param>
		/// <returns></returns>
		public static async Task<bool> ColumnExists(string TableName, string ColumnName, DataAccess Conn) {
			string MyCmd=
                $"select count(*) from [dbo].[sysobjects] as T join [dbo].[syscolumns] as C on C.ID = T.ID where T.name='{TableName}' and C.name='{ColumnName}'";
			int N =  Convert.ToInt32(await Conn.ExecuteScalar(MyCmd));
			if(N  == 0) {
				return false;
			}
			else {
				return true;
			}
		}

		

		/// <summary>
		/// Build and Execute an SqlCommand that creates the Table 
		/// </summary>
		/// <param name="TableName">Name of the Table</param>
		/// <param name="Column">Array containing schema of the table (columnname, 
		///		type, [NOT] NULL</param>
		/// <param name="PKey">Array containg the Primary Key of the Table</param>
		/// <param name="Conn"></param>
		/// <returns></returns>
		public static async Task<bool> CreateTable(string TableName, string[,] Column, string[] PKey, DataAccess Conn) {
			int i;
			string command;
			command = "CREATE TABLE " + TableName + " (\r";
			int ncol= Column.GetLength(0);
			for(i = 0; i < ncol; i++) {
				command += Column[i,0] + " " + Column[i,1] + " " +  Column[i,2];
				if ((i+1<ncol)||(PKey.Length>0))command += ",\r";
			}
			if (PKey.Length>0){
				command += " CONSTRAINT xpk" + TableName + " PRIMARY KEY (";
				for(i = 0; i < PKey.Length; i++) {
					command+= PKey[i];
					if (i+1<PKey.Length)command += ",";
					command +="\r";
				}
				command += ")\r";
			}
			command +=  ")";

			//MetaFactory.factory.getSingleton<IMessageShower>().Show(command);

			try {
				await Conn.ExecuteNonQuery(command);
				return true;
			}
			catch (Exception E) {
				Conn.Close();
			    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,E.ToString(),"Errore");
				return false;
			}
		}

		
		/// <summary>
		/// Add columns to a DB table
		/// </summary>
		/// <param name="TableName">name of table to which add columns</param>
		/// <param name="Column">array of columns to add (fieldname, sqltype)</param>
		/// <param name="Conn"></param>
		/// <returns>true when successfull</returns>
		public static async Task<bool> AddColumns(string TableName, string[,] Column, DataAccess Conn) {
			int i;
			string command;
			command = "ALTER TABLE " + TableName + " ADD \r";
			for(i = 0; i < Column.GetLength(0); i++) {
				if (i>0) command += " , ";
				command += Column[i,0] + " " + Column[i,1] + " " +  Column[i,2] + "\r";
			}
		    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,command,"Informazione");

			try {
				await Conn.ExecuteNonQuery(command);
				return true;
			}
			catch (Exception E) {
			    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,E.ToString(),"Errore");
				return false;
			}
		}


	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
#pragma warning disable IDE1006 // Naming Styles

namespace mdl {

    /// <summary>
    /// Interface that manages database transactions
    /// </summary>
    public interface ITransactionManagement {
      

        /// <summary>
        /// Set a connection as the currently connected to a transaction
        /// </summary>
        /// <param name="c"></param>
        void setTransactionConnection(SqlConnection c);

        /// <summary>
        /// Clears the current connection transaction
        /// </summary>
        void clearTransactionConnection();

        /// <summary>
        /// Start a "post" process, this doesnt mean to be called by applications
        /// </summary>
        /// <param name="mainConn"></param>
        void startPosting(IDataAccess mainConn);

        /// <summary>
        /// Ends a "post" process , this doesnt mean to be called by applications
        /// </summary>
        void stopPosting();

        /// <summary>
        /// Gets Current used Transaction
        /// </summary>
        /// <returns>null if no transaction is open</returns>
        SqlTransaction CurrTransaction();

        /// <summary>
        /// Starts a new transaction 
        /// </summary>
        /// <param name="L"></param>
        /// <returns>error message, or null if OK</returns>
        string BeginTransaction(IsolationLevel L);

        /// <summary>
        /// Commit the transaction
        /// </summary>
        /// <returns>error message, or null if OK</returns>
        string Commit();

        /// <summary>
        /// Rollbacks transaction
        /// </summary>
        /// <returns>Error message, or null if OK</returns>
        string RollBack();

        /// <summary>
        /// True if current transaction  is still alive, i.e. has a connection attached to it
        /// </summary>
        /// <returns></returns>
        bool validTransaction();

        /// <summary>
        /// 
        /// </summary>
        SqlConnection sqlConnection { get; set; }
    }


    /// <summary>
    /// Interface to db access
    /// </summary>
    public interface IDataAccess:ITransactionManagement {

        IMessageShower shower { get; set; }

        /// <summary>
        /// Gets the db name of a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        string GetCentralizedTableName(string table);

        /// <summary>
        /// Get user environment variable 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Obsolete]
        object GetUsr(string key);

        /// <summary>
        /// Get system environment variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [Obsolete]
        object GetSys(string key);


		/// <summary>
		/// NON USARE !
		/// </summary>
		/// <param name="key"></param>
		/// <param name="O"></param>
		[Obsolete]
		void SetUsr(string key, object O);

		/// <summary>
		/// Sets user environment variable
		/// </summary>
		/// <param name="key"></param>
		/// <param name="O"></param>
		[Obsolete]
		void SetUsr(string key, string O);

		/// <summary>
		/// Manages security conditions with this connection 
		/// </summary>
		ISecurity Security { get; set; }

        /// <summary>
        /// Actual namen of the connected user
        /// </summary>
        string externalUser { get; set; }
       
        /// <summary>
        /// 
        /// </summary>
        bool openError { get; set; }

        /// <summary>
        /// Return true if Connection is using Persisting connections mode, i.e.
        ///  it is open at the beginning aand closed at the end
        /// </summary>
        bool persisting { get; set; }


        /// <summary>
        /// Returns last error and resets it.
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// Get last error without clearing it
        /// </summary>
        /// <returns></returns>
        string SecureGetLastError();

      

        /// <summary>
        /// Get Sql Server Version
        /// </summary>
        /// <returns></returns>
        string ServerVersion();

        /// <summary>
        /// Async version of open
        /// </summary>
        /// <returns></returns>
        Task<bool> openAsync();

        /// <summary>
        /// async executeQueryTables with callback
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="packetSize"></param>
        /// <param name="timeout"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task executeQueryTablesCallback(string commandString, int packetSize, int timeout, Func<object, Task> callback);

        /// <summary>
        ///  Async version of a run selectBuilder
        /// </summary>
        /// <param name="selList"></param>
        /// <param name="packetSize"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        void  executeSelectBuilderCallback(List<SelectBuilder> selList, int packetSize,Action<SelectBuilder ,Dictionary<string, object>> callback, int timeout);

        /// <summary>
        /// Async version of a run select into table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task executeQueryIntoTable(DataTable table,object filter, int timeout=-1);
        
        
        /// <summary>
        /// Async query a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnList"></param>
        /// <param name="filter"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task <DataTable> executeQueryTable(string tablename, string columnList, object filter, int timeout=-1);
        
        
        /// <summary>
        /// Async execute a sql statement to retrieve a table
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        Task <DataTable> executeQuery(string sql, int timeout=-1);
        
        /// <summary>
        /// Async execute a sql statement to retrieve a value
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        Task<object> executeQueryValue(string commandString, int timeOut=-1);
        
        /// <summary>
        /// Async execute a sql statement that returns nothing
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="timeOut"></param>
        /// <returns></returns>
        Task<int> executeNonQuery(string commandString, int timeOut=-1);

        /// <summary>
        /// Forces read of all tables info structure again
        /// </summary>
        void Reset();

        /// <summary>
        /// Forces read of all tables info structure again
        /// </summary>
        /// <param name="clearDbStructure"></param>
        void Reset(bool clearDbStructure);

        /// <summary>
        /// When true, access to the table are prefixed with DBO.  
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        bool TableIsCentralized(string tablename);

        /// <summary>
        /// When true, access to the table are prefixed with DBO. 
        /// </summary>
        /// <param name="procname"></param>
        /// <returns></returns>
        bool ProcedureIsCentralized(string procname);

     

        /// <summary>
        /// Use another database with this connection
        /// </summary>
        /// <param name="DBName"></param>
        void ChangeDataBase(string DBName);

        /// <summary>
        /// Updates last read access stamp to db 
        /// </summary>
        void SetLastRead();

        /// <summary>
        /// Updates last write access stamp to db 
        /// </summary>
        void SetLastWrite();

        /// <summary>
        /// Crea un duplicato di un DataAccess, con una nuova connessione allo stesso DB. 
        /// Utile se la connessione deve essere usata in un nuovo thread.
        /// </summary>
        /// <returns></returns>
        DataAccess Duplicate();

        /// <summary>
        /// Release resources
        /// </summary>
        void Destroy();

        /// <summary>
        /// Open the connection (or increment nesting if already open)
        /// </summary>
        /// <returns> true when successfull </returns>
        bool Open();

        /// <summary>
        /// Close the connection
        /// </summary>
        void Close();

        /// <summary>
        /// Reads all data from MetaData-System Tables into a new DBstructure
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        dbstructure GetEntireStructure(string filter);

        /// <summary>
        /// When false table is not cached in the initialization for a given table
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        bool IsToRead(dbstructure DBS, string tablename);

        /// <summary>
        /// Get structure of a table without reading columntypes
        /// </summary>
        /// <param name="objectname"></param>
        /// <returns></returns>
        dbstructure GetStructureNoCustom(string objectname);

        /// <summary>
        /// Reads table structure of a list of tables
        /// </summary>
        /// <param name="tableName"></param>
        void preScanStructures(params string[] tableName);

        /// <summary>
        /// Gets DB structure related to table objectname. The dbstructure returned
        ///  is the same used for sys operations (it is not a copy of it)
        /// </summary>
        /// <param name="objectname"></param>
        /// <returns></returns>
        dbstructure GetStructure(string objectname);

		/// <summary>
		/// Reads all table structures from db
		/// </summary>
        void readStructuresFromDb();

        /// <summary>
        /// Read a bunch of table structures, all those present in the DataSet
        /// </summary>
        /// <param name="D"></param>
        /// <param name="primarytable"></param>
        void PrefillStructures(DataSet D, string primarytable);

        /// <summary>
        /// Saves a table structure to DB (customobject, columntypes..)
        /// </summary>
        /// <param name="DBS"></param>
        /// <returns></returns>
        bool SaveStructure(dbstructure DBS);

        /// <summary>
        /// Saves all changes made to all dbstructures
        /// </summary>
        /// <returns></returns>
        bool SaveStructure();

        /// <summary>
        /// Evaluate columntypes and customobject analizing db table properties
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="objectname"></param>
        /// <param name="forcerefresh">if false, only new tables are scanned</param>
        void AutoDetectTable(dbstructure DBS, string objectname, bool forcerefresh);

       

        /// <summary>
        /// Forces ColumnTypes to be read again from DB for tablename
        /// </summary>
        /// <param name="tablename"></param>
        void RefreshStructure(string tablename);

        /// <summary>
        /// Reads extended informations for a table related to a view,
        ///  in order to use it for posting. Reads data from viewcolumn.
        ///  Sets table and columnfor posting and also 
        ///  sets ViewExpression as tablename.columnname (for each field)
        /// </summary>
        /// <param name="T"></param>
        void GetViewStructureExtProperties(DataTable T);

		/// <summary>
		/// Gets all dbstructure stored
		/// </summary>
		/// <returns></returns>
        Dictionary<string, dbstructure> getStructures();

		/// <summary>
		/// Sets dbDtructures of a set of tables
		/// </summary>
		/// <param name="structures"></param>
		void setStructures(Dictionary<string, dbstructure> structures);

        ///// <summary>
        ///// Creates a table and returns it in a packed dataset
        ///// </summary>
        ///// <param name="tablename"></param>
        ///// <param name="columnlist"></param>
        ///// <returns></returns>
        //byte[] CreateByteTableByName(string tablename, string columnlist);

        /// <summary>
        ///  Creates a DataTable given it's db name
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <returns></returns>
        DataTable CreateTableByName(string tablename, string columnlist);

        /// <summary>
        /// Creates a new table basing on columntypes info. Adds also primary key 
        ///  information to the table, and allownull to each field.
        ///  Columnlist must include primary table, or can be "*"
        /// </summary>
        /// <param name="tablename">name of table to create. Can be in the form DBO.tablename or department.tablename</param>
        /// <param name="columnlist"></param>
        /// <param name="addextprop">Add db information as extended propery of columns (column length, precision...)</param>
        /// <returns>a table with same types as DB table</returns>
        DataTable CreateTableByName(string tablename, string columnlist, bool addextprop);

        /// <summary>
        /// Adds all extended information to table T reading it from columntypes.
        /// Every Row of columntypes is assigned to the corresponding extended 
        ///  properties of a DataColumn of T. Each Column of the Row is assigned
        ///  to an extended property with the same name of the Column
        ///  Es. R["a"] is assigned to Col.ExtendedProperty["a"]
        /// </summary>
        /// <param name="T"></param>
        void AddExtendedProperty(DataTable T);

   

        /// <summary>
        /// Adds where clauses to Cmd, using variables to store constants found in
        ///  filter. DateTime values should be like {ts "yyyy:mm:dd hh:mm:ss:mmmm"}
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="filter"></param>
        /// <param name="tablename"></param>
        void AddWhereClauses(ref SqlCommand Cmd, string filter, string tablename);

        /// <summary>
        /// Empty table structure information about a listing type of a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="listtype"></param>
        void ResetListType(string tablename, string listtype);

        /// <summary>
        /// Empty table structure information about any listing type of a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="listtype"></param>
        void ResetAllListType(string tablename, string listtype);

        /// <summary>
        /// Gets a DBS to describe columns of a list. returns also target-list type, that
        ///  can be different from input parameter listtype. Reads from customview,
        ///   customviewcolumn, customorderby, customviewwhere and from customredirect
        ///  Target-Table can be determined as DBS.customobject.rows[0]
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="tablename"></param>
        /// <param name="listtype"></param>
        /// <returns></returns>
        string GetListType(out dbstructure DBS, string tablename, string listtype);

        /// <summary>
        /// Get information about an edit type. Reads from customedit 
        /// </summary>
        /// <param name="objectname"></param>
        /// <param name="edittype"></param>
        /// <returns>CustomEdit DataRow about an edit-type</returns>
        DataRow GetFormInfo(string objectname, string edittype);

        /// <summary>
        /// Gets the system type name of a field named fieldname
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        string GetFieldSystemTypeName(dbstructure DBS, string fieldname);

        /// <summary>
        /// Gets the corresponding system type of a db column named fieldname
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        Type GetFieldSystemType(dbstructure DBS, string fieldname);

        /// <summary>
        /// Marks an Exception and set Last Error
        /// </summary>
        /// <param name="main">Main description</param>
        /// <param name="E"></param>
        string MarkException(string main, Exception E);

      

        /// <summary>
        ///   Read a set of fields from a table  and return a dictionary fieldName -&gt; value assuming that
        ///    the table contains only one row
        /// </summary>
        /// <param name="table"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        Dictionary<string, object> readObject(string table, string expr="*");

        /// <summary>
        ///  Read a set of fields from a table  and return a dictionary fieldName -&gt; value
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        Dictionary<string, object> readObject(string table, MetaExpression filter, string expr);

        /// <summary>
        /// Read a set of fields from a table  and return a dictionary fieldName -&gt; value
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="expr">list of fields to read</param>
        /// <returns>An object dictionary</returns>
        Dictionary<string, object> readObject(string table, string condition, string expr);

        /// <summary>
        /// Returns a single value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="expr"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        object readValue(string table, MetaExpression filter, string expr, string orderby=null);

        /// <summary>
        /// Returns a single value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="expr"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        object DO_READ_VALUE(string table, string condition, string expr, string orderby);

        /// <summary>
        /// Returns a value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        object DO_READ_VALUE(string table, string condition, string expr);

        /// <summary>
        /// Returns a value executing a generic sql command 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        object DO_SYS_CMD(string cmd);

        /// <summary>
        /// Returns a value executing a generic sql command 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ErrMsg">eventual error message</param>
        /// <returns></returns>
        object DO_SYS_CMD(string cmd, out string ErrMsg);

        /// <summary>
        /// Reads all value from a generic sql command and returns the last value read
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        object DO_SYS_CMD_LASTRESULT(string cmd, out string ErrMsg);

        /// <summary>
        /// Runs a sql command that returns a single value
        /// </summary>
        /// <param name="cmd">command to run</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <returns></returns>
        object DO_SYS_CMD(string cmd, bool silent);

        
        /// <summary>
        /// Get a list of "objects" from a table using  a specified query, every object is encapsulated in a dictionary
        /// </summary>
        /// <param name="query">sql command to run</param>
        /// <param name="timeout"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        Dictionary<string, object>[] readObjectArray(string query, int timeout, out string ErrMsg);

        /// <summary>
        /// Runs a sql command that return a DataTable
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMsg">Error message or null when no errors</param>
        /// <returns></returns>
        DataTable SQLRunner(string command, out string ErrMsg, int timeout = -1);

        /// <summary>
        /// Executes a generic SQL command that returns a Table
        /// </summary>
        /// <param name="command"></param>
        /// <param name="silent">set true non visualizza messaggi di errore</param>
        /// <param name="timeout">Timeout in secondi</param>
        /// <returns></returns>
        DataTable SQLRunner(string command, bool silent = false, int timeout = -1);


        /// <summary>
        /// Builds a sql DELETE command 
        /// </summary>
        /// <param name="table">table implied</param>
        /// <param name="condition">condition for the deletion</param>
        /// <returns></returns>
        string GetDeleteCommand(string table, string condition);

        /// <summary>
        /// Executes a delete command using current transaction
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <returns>Error message or null if OK</returns>
        string DO_DELETE(string table, string condition);

        /// <summary>
        /// Builds a sql INSERT command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns">column names</param>
        /// <param name="values">values to insert</param>
        /// <param name="len">number of columns</param>
        /// <returns></returns>
        string getInsertCommand(string table, string[] columns, string[] values, int len);

        /// <summary>
        /// Executes an INSERT command using current tranactin
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns">column names</param>
        /// <param name="values">values to insert</param>
        /// <param name="len">number of columns</param>
        /// <returns>Error message or null if OK</returns>
        string DO_INSERT(string table, string[] columns, string[] values, int len);

        /// <summary>
        /// Builds an UPDATE sql command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="columns">column names</param>
        /// <param name="values">values to be set</param>
        /// <param name="ncol">number of columns to update</param>
        /// <returns>Error msg or null if OK</returns>
        string getUpdateCommand(string table, string condition,
            string[] columns, string[] values, int ncol);

        /// <summary>
        /// Executes an UPDATE command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition">where condition to apply</param>
        /// <param name="columns">Name of columns to update</param>
        /// <param name="values">Values to set</param>
        /// <param name="ncol">N. of columns</param>
        /// <returns>Error msg or null if OK</returns>
        string DO_UPDATE(string table, string condition,
            string[] columns, string[] values, int ncol);

        /// <summary>
        /// Executes an UPDATE command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition">where condition to apply</param>
        /// <param name="columns">Name of columns to update</param>
        /// <param name="values">Values to set</param>
        /// <returns>Error msg or null if OK</returns>
        string doUpdate(string table, MetaExpression condition, string[] columns, object[]values);

        /// <summary>
        /// Executes an UPDATE command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition">where condition to apply</param>
        /// <param name="fieldValues">Values to set</param>
        /// <returns>Error msg or null if OK</returns>
        string doUpdate(string table, MetaExpression condition, Dictionary< string,object> fieldValues);

        /// <summary>
        /// Executes an UPDATE command
        /// </summary>
        /// <param name="r">sample data to pick</param>
        /// <param name="condition">where condition to apply</param>
        /// <param name="fields">Values to set</param>
        /// <returns>Error msg or null if OK</returns>
        string doUpdate(DataRow r, MetaExpression condition=null, string []fields=null);

     
        /// <summary>
        /// Calls a stored procedure with specified parameters
        /// </summary>
        /// <param name="procname">stored proc. name</param>
        /// <param name="list">Parameter list</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <returns></returns>
        DataSet CallSP(string procname, object[] list, bool silent);

        /// <summary>
        /// Execute a sql cmd that returns a dataset (eventually with more than one table in it)
        /// </summary>
        /// <param name="sql">sql command to run</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMess">null if ok, Error message otherwise</param>
        /// <returns></returns>
        DataSet sqlRunnerDataSet(string sql, int timeout, out string ErrMess);

        /// <summary>
        /// Calls a stored procedure with specified parameters
        /// </summary>
        /// <param name="procname">stored proc. name</param>
        /// <param name="list">Parameter list</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMess">null if ok, Error message otherwise</param>
        /// <returns></returns>
        DataSet CallSP(string procname, object[] list, out string ErrMess, int timeout );

        /// <summary>
        /// Calls a stored procedure and reads output in a DataSet. First table can be retrieved in result.Tables[0]
        /// </summary>
        /// <param name="procname">name of stored procedure to call</param>
        /// <param name="list">parameters to give to the stored procedure</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns>null on errors, in which case also LastError is set</returns>
        DataSet CallSP(string procname, object[] list, bool silent, int timeout);

        /// <summary>
        /// Calls a stored procedure, return true if ok
        /// </summary>
        /// <param name="sp_name">name of stored procedure to call</param>
        /// <param name="ParamName">parameter names to give to the stored procedure</param>
        /// <param name="ParamType">parameter types to give to the stored procedure</param>
        /// <param name="ParamTypeLength">Length of parameters</param>
        /// <param name="ParamDirection">Type of parameters</param>
        /// <param name="ParamValues">Value for parameters</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMsg">null if ok, Error message otherwise</param>
        /// <returns></returns>
        bool CallSPParameter(string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            int timeout, out string ErrMsg);

        /// <summary>
        /// Calls a stored procedure and returns a DataSet. First table can be retrieved in result.Tables[0]
        /// </summary>
        /// <param name="sp_name">name of stored procedure to call</param>
        /// <param name="ParamName">parameter names to give to the stored procedure</param>
        /// <param name="ParamType">parameter types to give to the stored procedure</param>
        /// <param name="ParamTypeLength">Length of parameters</param>
        /// <param name="ParamDirection">Type of parameters</param>
        /// <param name="ParamValues">Value for parameters</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMsg">null if ok, Error message otherwise</param>
        /// <returns></returns>
        DataSet CallSPParameterDataSet(string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            int timeout, out string ErrMsg);

        /// <summary>
        /// Calls a stored procedure and returns a DataSet.  return true if ok
        /// </summary>
        /// <param name="sp_name">name of stored procedure to call</param>
        /// <param name="ParamName">parameter names to give to the stored procedure</param>
        /// <param name="ParamType">parameter types to give to the stored procedure</param>
        /// <param name="ParamTypeLength">Length of parameters</param>
        /// <param name="ParamDirection">Type of parameters</param>
        /// <param name="ParamValues">Value for parameters</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        bool CallSPParameter(string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            bool silent, int timeout);

        /// <summary>
        /// Reads data into a given table, skipping temporary columns
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        /// <param name="sort_by"></param>
        /// <param name="TOP"></param>
        void selectIntoTable(DataTable T, MetaExpression filter, string sort_by=null, string TOP=null);

        /// <summary>
        /// Reads data into a given table, skipping temporary columns
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sort_by">sorting for db reading</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="TOP"></param>
        /// <param name="prepare"></param>
        void RUN_SELECT_INTO_TABLE(DataTable T, string sort_by, string filter, string TOP, bool prepare);

        /// <summary>
        /// Executes sql into a dataset
        /// </summary>
        /// <param name="d"></param>
        /// <param name="sql"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        string sqlRunnerintoDataSet(DataSet d, string sql, int timeout);

        /// <summary>
        /// Reads data into a table. The table is created at run-time using information
        ///  stored in columntypes
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist">list of field names separated by commas</param>
        /// <param name="order_by">list of field names separated by commas</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="TOP">how many rows to get</param>
        /// <param name="prepare">if true the command is prepared before being runned</param>
        /// <returns>DataTable read</returns>
        DataTable RUN_SELECT(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            bool prepare);

        /// <summary>
        /// Reads data from db and return a dataset serialized to a byte array
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="order_by"></param>
        /// <param name="filter"></param>
        /// <param name="TOP"></param>
        /// <param name="group_by"></param>
        /// <param name="prepare"></param>
        /// <returns></returns>
        byte[] MAIN_RUN_SELECT(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare
        );

        /// <summary>
        /// Return something like SELECT {colTable1} from {table1.TableName} JOIN {table2} ON  {joinFilter} [WHERE {whereFilter}]
        /// </summary>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="filterTable1"></param>
        /// <param name="filterTable2"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string getJoinSql(DataTable table1, string table2, MetaExpression filterTable1, MetaExpression filterTable2,
            params string[] columns);


        /// <summary>
        /// Runs a sql command to fill an existent empty table
        /// </summary>
        /// <param name="EmptyTable"></param>
        /// <param name="sql"></param>
        /// <param name="timeout">timeout in second, 0 is no timeout, -1 is default timeout</param>
        /// <returns>Read rows</returns>
        DataRow [] SQLRUN_INTO_EMPTY_TABLE(DataTable EmptyTable, string sql, int timeout=-1);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sql"></param>
        /// <param name="timeout">timeout in second, 0 is no timeout, -1 is default timeout</param>
        /// <returns></returns>
        DataRow [] SQLRUN_INTO_TABLE(DataTable T, string sql, int timeout=-1);

        /// <summary>
        /// Executes something like  SELECT {colTable1} from {table1.TableName} JOIN {table2} ON  {joinFilter} [WHERE {whereFilter}]
        /// </summary>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="filterTable1"></param>
        /// <param name="filterTable2"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        DataRow [] readTableJoined(DataTable table1, string table2,
            MetaExpression filterTable1, MetaExpression filterTable2,
            params string[] columns);

        /// <summary>
        /// Reads data into a table. The table is created at run-time using information
        ///  stored in columntypes
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        /// <param name="columnlist"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        DataTable readTable(string tablename,
            MetaExpression filter=null,
            string columnlist="*",
            string order_by=null,             
            string TOP=null);

        /// <summary>
        /// Reads data into a table. The table is created at run-time using information
        ///  stored in columntypes
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist">list of field names separated by commas</param>
        /// <param name="order_by">list of field names separated by commas</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="TOP">how many rows to get</param>
        /// <param name="group_by">list of field names separated by commas</param>
        /// <param name="prepare">if true the command is prepared before being runned</param>
        /// <returns></returns>
        DataTable RUN_SELECT(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare);

        /// <summary>
        /// Reads data into a table. The table is created at run-time using information
        ///  stored in columntypes
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist">list of field names separated by commas</param>
        /// <param name="order_by">list of field names separated by commas</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="TOP">how many rows to get</param>
        /// <param name="group_by">list of field names separated by commas</param>
        /// <param name="prepare">if true the command is prepared before being runned</param>
        /// <returns></returns>
        DataTable RUN_SELECT_2ndVer(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare);

        /// <summary>
        /// Creates a dictionary from a query on a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="keyField"></param>
        /// <param name="valueField"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Dictionary<T, S> readSimpleDictionary<T, S>(string tablename,
            MetaExpression filter,
            string keyField, string valueField              
        );

        /// <summary>
        /// Creates a dictionary from a query on a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="keyField">key field of dictionary</param>
        /// <param name="valueField">value field of dictionary</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Dictionary<T, S> readSimpleDictionary<T,S>(string tablename,
            string keyField, string valueField,
            string filter=null
        );

        /// <summary>
        /// Creates a dictionary key => rowObject from a query on a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="keyField"></param>
        /// <param name="fieldList"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Dictionary<T, RowObject> readRowObjectDictionary<T>(string tablename,
            MetaExpression filter,
            string keyField, string fieldList
                
        );

        /// <summary>
        /// Creates a dictionary key => rowObject from a query on a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="keyField">key field of dictionary</param>
        /// <param name="fieldList">list value to read (must not include keyField)</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Dictionary<T, RowObject> readRowObjectDictionary<T>(string tablename,
            string keyField, string fieldList,
            string filter = null
        );

        /// <summary>
        /// Selects a set of rowobjects from db
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tables">logical table names</param>
        /// <returns></returns>
        Dictionary<string, List<RowObject>> multiRowObject_Select(string cmd, params string[] tables);

        /// <summary>
        /// Selects a set of rowobjects from db
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="filter"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        List<RowObject> RowObjectSelect(string tablename,
            string columnlist,
            MetaExpression filter,
            string order_by = null,
            string TOP = null);

        /// <summary>
        /// Selects a set of rowobjects from db
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="filter"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        List<RowObject> RowObject_Select(string tablename,
            string columnlist,
            string filter,
            string order_by = null,
            string TOP = null);

        /// <summary>
        /// Executes a List of Select, returning data in the tables specified by each select. 
        /// </summary>
        /// <param name="SelList"></param>
        void MULTI_RUN_SELECT(List<SelectBuilder> SelList);

        /// <summary>
        /// Experimental function, unused
        /// </summary>
        /// <param name="t"></param>
        /// <param name="order_by"></param>
        /// <param name="filter"></param>
        /// <param name="TOP"></param>
        /// <param name="group_by"></param>
        /// <param name="prepare"></param>
        void RUN_SELECT_INTO_TABLE_direct(DataTable t,
            string order_by,
            string filter,
            string TOP, string group_by,
            bool prepare);

        /// <summary>
        /// Reads a table without reading the schema. Result table has no primary key set.
        /// This is quicker than a normal select but slower than a RowObject_select
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="filter"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        DataTable readFromTable(string tablename, MetaExpression filter, string columnlist="*", string order_by = null, string TOP = null);

        /// <summary>
        /// Reads a table without reading the schema. Result table has no primary key set.
        /// This is quicker than a normal select but slower than a RowObject_select
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="filter"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        DataTable readFromTable(string tablename, string columnlist, string filter, string order_by = null,string TOP = null);

        /// <summary>
        /// Executes a SELECT COUNT on a table.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        int count(string tablename, MetaExpression filter);

        /// <summary>
        /// Executes a SELECT COUNT on a table.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter">condition to apply</param>
        /// <param name="prepare">when true, command has to be prepared</param>
        /// <returns></returns>
        int RUN_SELECT_COUNT(string tablename, string filter, bool prepare);

        /// <summary>
        /// Logs an error to the remote logger
        /// </summary>
        /// <param name="errmsg"></param>
        /// <param name="E"></param>
        void LogError(string errmsg, Exception E);

      

        /// <summary>
        /// Returns the queryhelper attached to this kind of DataAccess
        /// </summary>
        /// <returns></returns>
        QueryHelper GetQueryHelper();

        /// <summary>
        /// Compile a filter substituting environment variables, that can be a string or a MetaExpression
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        string compile(object filter);
    }
}
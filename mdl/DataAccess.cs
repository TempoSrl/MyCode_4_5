using System;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Data;
//using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using LM=mdl_language.LanguageManager;
using mdl_utils;
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CA2100

namespace mdl {


  

    /// <summary>
    /// Information about connection
    /// </summary>
    /// <remarks>
    /// This class exists for historical reasons. 
    /// Actually it only stores some data about the work session. It is an
    ///  envelope for future addition to user-session information.
    /// </remarks>
    public class DataAccess : MarshalByRefObject, IDisposable, IDataAccess {
        //		long reading;
        //		long compiling;
        //		long preparing;

        /// <summary>
        /// Unused
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService() {
            return null;
        }

        public IMessageShower shower { get; set; } = null;

        /// <summary>
        /// Get a system environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public object GetSys(string name) {
            return Security?.GetSys(name);
        }

        /// <summary>
        /// Set a system environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="O">value to set</param>
        public void SetSys(string name, string O) {
            Security?.SetSys(name, O);
        }

       /// <summary>
       /// Set a system environment variable
       /// </summary>
       /// <param name="name">variable name</param>
       /// <param name="O">value to set</param>
        public void SetSys(string name, object O) {
            Security?.SetSys(name, O);
        }

        /// <summary>
        /// Get a user environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public object GetUsr(string name) {
            return Security?.GetUsr(name);
        }

        /// <summary>
        /// Set a user environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="O">value to set</param>
        public void SetUsr(string name, object O) {
            Security?.SetUsr(name, O);                                                                 
        }
        /// <summary>
        /// Set a user environment variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="O">value to set</param>
        public void SetUsr(string name, string O) {
            Security?.SetUsr(name, O);
        }

        private ISecurity security;
        /// <summary>
        /// 
        /// </summary>
        public ISecurity Security {
            get {
                if (security == null) {
                    security= createSecurity();
                }
                return security;
            }
            set { security = value; }
        }

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;

       
        /// <summary>
        ///  Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;

            if (disposing) {
                Destroy();

            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }


        /// <summary>
        /// Query helper for DB query
        /// </summary>
        protected QueryHelper QHS;

        /// <summary>
        /// Query helper for dataset query
        /// </summary>
        protected CQueryHelper QHC;

        private static QueryHelper qhc = MetaFactory.factory.getSingleton<CQueryHelper>();

        private SqlConnection transactionConnection = null;
        private SqlConnection privateConnection = null;

        /// <summary>
        /// Set a connection as the currently connected to a transaction
        /// </summary>
        /// <param name="c"></param>
	    public virtual void setTransactionConnection(SqlConnection c) {
            transactionConnection = c;
        }

        /// <summary>
        /// Clears the current connection transaction
        /// </summary>
        public virtual void clearTransactionConnection() {
            transactionConnection = null;
        }

        IDataAccess mainConnection = null;

        /// <summary>
        /// Start a "post" process, this doesnt mean to be called by applications
        /// </summary>
        /// <param name="mainConn"></param>
	    public virtual void startPosting(IDataAccess mainConn) {
            if (mainConn == this) return;
            this.mainConnection = mainConn;
            this.transactionConnection = mainConn.sqlConnection;
        }

        /// <summary>
        /// Ends a "post" process , this doesnt mean to be called by applications
        /// </summary>
        public void stopPosting() {
            this.mainConnection = null;
            this.transactionConnection = null;
        }

        string dbConnectionString;

        /// <summary>
        /// Sql connection used for physical connection. Should not be
        ///  used from external classes
        /// </summary>
        protected SqlConnection MySqlConnection
        {
            get
            {
                if (transactionConnection != null) return transactionConnection;
                return privateConnection;
            }
            set
            {
                privateConnection = value;
            }
        }

        /// <summary>
        /// When true (default), connection is opened at first and closed at end of program
        /// When false, connection is opened/closed at every db access
        /// </summary>
        bool Mypersisting;


        /// <summary>
        ///  True if all listtype and db properties are contained in system tables
        /// </summary>
		public bool ManagedByDB = false;

        /// <summary>
        /// Closes the connection without throwing exceptions
        /// </summary>
		protected void SureClosing() {
            if (MySqlConnection == null) return;
            try {
                if (MySqlConnection.State == ConnectionState.Open) MySqlConnection.Close();
                //nesting=0;
            }
            catch (Exception E) {
                MarkException("SureClosing: Error Disconnecting from DB", E);
            }
        }

        //new byte[]{75,12,0,215,   93,89,45,11,   171,96,4,64,  13,158,36,190};
        //private static byte [] GetArr11(){
        //    byte []arr=	new byte[]{75,12,0,215+23,   93,89-19,45,11,   171,96+68,4,64,  13+8,158,36,190};
        //    arr[3]-= 23;
        //    arr[5]+=19;
        //    arr[9]-=68;
        //    arr[12]-=8;
        //    return arr;
        //}

        /// <summary>
        /// Return true if Connection is using Persisting connections mode, i.e.
        ///  it is open at the beginning aand closed at the end
        /// </summary>
        public bool persisting
        {
            get
            {
                return Mypersisting;
            }
            set
            {
                if (Mypersisting == value) return;
                if (MySqlConnection == null) return;
                if (Mypersisting) {
                    //Was persisting, must become not-persisting
                    if (nesting == 0) SureClosing();
                }
                else {
                    //Was not persisting, must become perrsisting
                    if (nesting == 0) MySqlConnection.Open();
                }
                Mypersisting = value;
            }
        }



        /// <summary>
        /// Gets all dbstructure stored
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, dbstructure> getStructures() {
	        var res = new Dictionary<string, dbstructure>();
	        foreach (var k in DBstructures.Keys) {
				res.Add(k.ToString(),DBstructures[k] as dbstructure);
	        }
	        return res;
        }

        /// <summary>
        /// Sets dbDtructures of a set of tables
        /// </summary>
        /// <param name="structures"></param>
        public void setStructures(Dictionary<string, dbstructure> structures) {
	        foreach (var k in structures.Keys) {
		        DBstructures[k] = structures[k];
	        }
        }

        /// <summary>
        /// 
        /// </summary>
        public SqlConnection sqlConnection { get { return MySqlConnection; } set { MySqlConnection = value; } }

        /// <summary>
        /// When true (default) preparing of command is enabled
        /// </summary>
        public bool PrepareEnabled;

        /// <summary>
        /// True When runned locally. False if is used in a remote client
        /// </summary>
        public bool LocalToDB = true; //= true se eseguito in locale (non in remoting)

        /// <summary>
        /// Normally true, false if operating in 3-tier mode (obsolete)
        /// </summary>
        public static bool IsLocal = true;
        string myLastError;

        /// <summary>
        /// Returns last error and resets it.
        /// </summary>
        public string LastError
        {
            get
            {
                if (mainConnection != null) return mainConnection.LastError;
                string S = myLastError;
                myLastError = "";
                return S;
            }
        }




        /// <summary>
        /// Get last error without clearing it
        /// </summary>
        /// <returns></returns>
		public string SecureGetLastError() {
            if (mainConnection != null) return mainConnection.SecureGetLastError();
            return myLastError;
        }
        /// <summary>
        /// Prepared SQL commands. Key is the SqlCommandText
        /// </summary>
        Hashtable PreparedCommands;


        /// <summary>
        /// the dbstructure dataset  - one dataset for each objectname
        /// </summary>
        protected Hashtable DBstructures = new Hashtable();

        int nesting;
        /// <summary>
        /// True if SSPI is used, False if SQL security is used
        /// </summary>
        public bool SSPI;


#pragma warning disable 1591
#pragma warning disable 612
        public string externalUser { get; set; }
#pragma warning restore 612
#pragma warning restore 1591

       
        
        /// <summary>
        /// Gets accounting year
        /// </summary>
        /// <returns></returns>
        public int GetEsercizio() {
            return security.GetEsercizio();
        }
        
        /// <summary>
        /// Gets logging date
        /// </summary>
        /// <returns></returns>
        public DateTime GetDataContabile() {
            return security.GetDataContabile();
        }

       

		/// <summary>
		/// True if opening problems encountered  
		/// </summary>
		public bool openError { get; set; }

		/// <summary>
		/// True if connection was establisched, then system errors has broken it
		/// </summary>
		public bool ConnectionHasBeenClosedBySystem;


        /// <summary>
        /// True if Multi DB connection is to be used
        /// </summary>
        public bool MultiDB = false;

        /// <summary>
        /// If true, customobject and column types are used to describe table structure, 
        ///  when false, those are always obtained from DB
        /// </summary>
        public bool UseCustomObject = true;

     

        /// <summary>
        /// Convert a string like a='2';b=#3#;c='12'.. into a string hashtable
        /// </summary>
        /// <param name="S1"></param>
        /// <returns></returns>
        public static Hashtable GetHashFromString(string S1) {
	        var HH = new Hashtable();

            byte[] B1 = mdl_utils.Quoting.StringToByteArray(S1);
            string S = CryptDecrypt.DecryptString(B1);

            int i = 0;
            while (i < S.Length) {
                //prende l'identificatore all'inizio di S, fino all'uguale
                int poseq = S.IndexOf("=");
                if (poseq <= 0) break;
                string myfield = S.Substring(0, poseq).Trim();
                S = S.Substring(poseq + 1);
                char SEP = S[0];
                if (SEP != '\'' && SEP != '#') break;
                int index = 1;
                while (index < S.Length) {
                    //ad ogni iterazione index è la posizione da cui partire (inclusa) per la ricerca del prossimo apice
                    index = S.IndexOf(SEP, index);
                    if (index <= 0) break;
                    if ((index + 1) >= S.Length) break; //ha trovato l'apice (finale)
                    if (S[index + 1] != SEP) break; //ha trovato l'apice (non è seguito da un altro apice)
                    index += 2;
                }
                if ((index < 1) || (index >= S.Length)) break; //non ha trovato l'apice
                if (S[index] != SEP) break;  //non ha trovato l'apice
                string val = S.Substring(1, index - 1);
                if (SEP == '\'') {
                    val = val.Replace("''", "'"); //toglie il doppio apice in tutto val
                }
                else {
                    val = "#" + val + "#";
                }
                try {
                    HH[myfield] = val;
                }
                catch { }
                if (index + 2 >= S.Length) break; //Se S è finita esci
                S = S.Substring(index + 2);
            }
            return HH;
        }

        /// <summary>
        /// Convert an hashtable into a string like a='2';b=#3#;c='12'.. 
        /// </summary>
        /// <param name="H"></param>
        /// <returns></returns>
        public static string GetStringFromHashTable(Hashtable H) {
	        var QHC = new CQueryHelper();
            string S = "";
            foreach (object key in H.Keys) {
                if (S != "") S += ";";
                S = S + key.ToString() + "=" + QHC.quote(H[key]);
            }

            byte[] B2 = CryptDecrypt.CryptString(S); //dati criptati
            string SS = mdl_utils.Quoting.ByteArrayToString(B2); //stringa dei dati criptati
            return SS;
        }




       


        #region Constructors (with or without SSPI)

        /// <summary>
        /// Creates a DataAccess
        /// </summary>
        /// <param name="MainConnection"></param>
        /// <param name="DSN"></param>
        /// <param name="Server"></param>
        /// <param name="Database"></param>
        /// <param name="UserDB"></param>
        /// <param name="PasswordDB"></param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
        public void createDataAccess(bool MainConnection,
            string DSN,
            string Server,
            string Database,
            string UserDB,
            string PasswordDB,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(MainConnection, DSN, Server, Database, UserDB, PasswordDB, User, Password,
                esercizio_sessione, DataContabile);
        }

        /// <summary>
        /// Constructor of DataAccess
        /// </summary>
        /// <param name="MainConnection"></param>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="UserDB"></param>
        /// <param name="PasswordDB"></param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
        virtual protected void CreateDataAccess(
            bool MainConnection,
            string DSN,
            string Server,
            string Database,
            string UserDB,
            string PasswordDB,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            
            string AppName=ErrorLogger.applicationName??"MetaData";
            if (MainConnection)
                AppName = $"{AppName}(" + User.ToString() + ")";
            else
                AppName = $"{AppName}Temp(" + User.ToString() + ")";
            dbConnectionString = "data source=" + Server +
                ";initial catalog=" + Database +
                ";User ID =" + UserDB +
                ";Password=" + PasswordDB +
                ";Application Name=" + AppName + ";" +
                "WorkStation ID =" + Environment.MachineName +
                ";Pooling=false" +
                ";Connection Timeout=300;"
                //+"Connection Lifetime = 1"
                //+";persist security info=True;packet size=4096"
                ;
            try {
                MySqlConnection = new SqlConnection(dbConnectionString);
            }
            catch (Exception E) {
                myLastError = QueryCreator.GetErrorString(E);
                openError = true;
                return;
            }
            Security?.SetSys("dsn",DSN);
            Security?.SetSys("userdb", UserDB.ToUpper());
            Security?.SetSys("user", User.ToUpper());
            Security?.SetSys("passworddb", PasswordDB);
            Security?.SetSys("database", Database);
            Security?.SetSys("server", Server);
            Security?.SetSys("esercizio", esercizio_sessione);
            Security?.SetSys("datacontabile", DataContabile);
            Security?.SetSys("computername", Environment.MachineName + "-" + GetOSVersion());
            Security?.SetSys("computeruser", Environment.UserName);
            
            SSPI = false;
            MultiDB = true;
            Init();

        }
        
        /// <summary>
        /// Get Sql Server Version
        /// </summary>
        /// <returns></returns>
	    public string ServerVersion() {
            if (MySqlConnection == null) return "no connection";
            if (MySqlConnection.State == ConnectionState.Open) return MySqlConnection.ServerVersion;
            return "closed";
        }

        /// <summary>
        /// Gets OS Version
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion() {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.Win32S:
                    return "Win 3.1";
                case PlatformID.Win32Windows:
                    switch (Environment.OSVersion.Version.Minor) {
                        case 0:
                            return "Win95";
                        case 10:
                            return "Win98";
                        case 90:
                            return "WinME";
                    }
                    break;

                case PlatformID.Win32NT:
                    switch (Environment.OSVersion.Version.Major) {
                        case 3:
                            return "NT 3.51";
                        case 4:
                            return "NT 4.0";
                        case 5:
                            switch (Environment.OSVersion.Version.Minor) {
                                case 0:
                                    return "Win2000";
                                case 1:
                                    return "WinXP";
                                case 2:
                                    return "Win2003";
                            }
                            break;

                        case 6:
                            switch (Environment.OSVersion.Version.Minor) {
                                case 0:
                                    return "Vista/Win2008Server";
                                case 1:
                                    return "Win7/Win2008Server R2";
                                case 2:
                                    return "Win8/Win2012Server";
                                case 3:
                                    return "Win8.1/Win2012Server R2";
                            }
                            break;
                        case 10:
                            switch (Environment.OSVersion.Version.Minor) {
                                case 0:
                                    return "Win10/Win2016Server";
                            }
                            break;
                    }
                    break;

                case PlatformID.WinCE:
                    return "Win CE";
            }

            return "Unknown";
        }


        /// <summary>
        /// Constructor for WEB 
        /// </summary>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="UserDB"></param>
        /// <param name="PasswordDB"></param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
        public DataAccess(string DSN, string Server,
            string Database,
            string UserDB,
            string PasswordDB,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(true, DSN, Server, Database, UserDB, PasswordDB, User, Password, esercizio_sessione, DataContabile);
        }

        /// <summary>
        /// protected constructor
        /// </summary>
        /// <param name="MainConn"></param>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="UserDB"></param>
        /// <param name="PasswordDB"></param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		protected DataAccess(bool MainConn, string DSN, string Server,
            string Database,
            string UserDB,
            string PasswordDB,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(MainConn, DSN, Server, Database, UserDB, PasswordDB, User, Password, esercizio_sessione, DataContabile);
        }

        /// <summary>
        /// Creates a DataAccess
        /// </summary>
        /// <param name="MainConnection"></param>
        /// <param name="DSN"></param>
        /// <param name="Server"></param>
        /// <param name="Database"></param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
        public virtual void createDataAccess(bool MainConnection,
            string DSN,
            string Server,
            string Database,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(MainConnection, DSN, Server, Database, User, Password, esercizio_sessione, DataContabile);
        }

        /// <summary>
        /// protected constructor
        /// </summary>
        /// <param name="MainConnection"></param>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		protected virtual void CreateDataAccess(
            bool MainConnection,
            string DSN,
            string Server,
            string Database,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
	        string AppName=ErrorLogger.applicationName??"MetaData";
            if (MainConnection)
	            AppName = $"{AppName}(" + User.ToString() + ")";
            else
	            AppName = $"{AppName}Temp(" + User.ToString() + ")";
            dbConnectionString = "data source=" + Server +
                ";initial catalog=" + Database +
                ";User ID =" + User.ToUpper() +
                ";Password=" + Password +
                ";Application Name=" + AppName + ";" +
                "WorkStation ID =" + Environment.MachineName.ToUpper() +
                ";Pooling=false" +
                ";Connection Timeout=300;"
                //+"Connection Lifetime = 1"
                //+";persist security info=True;packet size=4096"
                ;
            try {
                MySqlConnection = new SqlConnection(dbConnectionString);
            }
            catch (Exception E) {
                myLastError = QueryCreator.GetErrorString(E);
                openError = true;
                return;
            }


            Security.SetSys("dsn", DSN);
            Security.SetSys("userdb", User.ToUpper());
            Security.SetSys("user", User.ToUpper());
            Security.SetSys("passworddb", Password);
            Security.SetSys("password", Password);
            Security.SetSys("database", Database);
            Security.SetSys("server", Server);
            Security.SetSys("esercizio", esercizio_sessione);
            Security.SetSys("datacontabile", DataContabile);
            Security.SetSys("computername", Environment.MachineName + "-" + GetOSVersion());
            Security.SetSys("computeruser", Environment.UserName);


            SSPI = false;
            Init();

        }

        /// <summary>
        /// Called when a security class is needed
        /// </summary>
        /// <returns></returns>
        protected virtual ISecurity createSecurity() {
           return new DefaultSecurity(this); 
        }
        /// <summary>
        /// Constructuctor for SQL Based Security  
        /// </summary>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
        public DataAccess(string DSN, string Server,
            string Database,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(true, DSN, Server, Database, User, Password, esercizio_sessione, DataContabile);
        }


        /// <summary>
        /// Constructuctor for SQL Based Security  that accept a MainConnection parameter
        /// </summary>
        /// <param name="MainConn">True for Main connection, false for temporary connections</param>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="User"></param>
        /// <param name="Password"></param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		public DataAccess(bool MainConn,
            string DSN,
            string Server,
            string Database,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(MainConn, DSN, Server, Database, User, Password, esercizio_sessione, DataContabile);
        }

        /// <summary>
        /// Create a DataAccess using SSPI Integrated Security
        /// </summary>
        /// <param name="MainConnection">It's true for the main connection of the application, false for Connection used in threads</param>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		public virtual void CreateDataAccess(
            bool MainConnection,
            string DSN,
            string Server,
            string Database,
            int esercizio_sessione,
            DateTime DataContabile) {
            string AppName=ErrorLogger.applicationName??"MetaData";
            if (MainConnection)
                AppName = $"{AppName}(" + System.Environment.UserName + ")";
            else
                AppName = $"{AppName}Temp(" + System.Environment.UserName + ")";
            dbConnectionString = "data source=" + Server +
                ";initial catalog=" + Database +
                ";integrated security=SSPI;" +
                "Application Name=" + AppName + ";" +
                "WorkStation ID =" + Environment.MachineName.ToUpper() +
                ";Pooling=false" +
                ";Connection Timeout=300;"
                //+"Connection Lifetime = 1"
                //+";persist security info=True;packet size=4096"
                ;
            try {
                MySqlConnection = new SqlConnection(dbConnectionString);
            }
            catch (Exception E) {
                myLastError = QueryCreator.GetErrorString(E);
                openError = true;
                return;
            }


            Security.SetSys("dsn", DSN);
            Security.SetSys("userdb", System.Environment.UserName.ToUpper());
            Security.SetSys("user", System.Environment.UserName.ToUpper());
            Security.SetSys("database", Database);
            Security.SetSys("server", Server);
            Security.SetSys("esercizio", esercizio_sessione);
            Security.SetSys("datacontabile", DataContabile);
            Security.SetSys("computername", Environment.MachineName + "-" + GetOSVersion());
            Security.SetSys("computeruser", Environment.UserName);

                                                                      
            SSPI = true;

            if (Environment.UserName == "") {
                openError = true;
                myLastError = "L'utente non si è autenticato a windows.";
                return;
            }

            Init();
        }



        /// <summary>
        /// Constructor for Windows Based Security, the connection is marked as primary
        /// </summary>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
        public DataAccess(
            string DSN,
            string Server,
            string Database,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(true, DSN, Server, Database, esercizio_sessione, DataContabile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MainConn">true for the main connection of the application, false for Connection used in threads</param>
        /// <param name="DSN">Simbolic name for the connection</param>
        /// <param name="Server">Server name or Server address</param>
        /// <param name="Database">DataBase name, eventually with port</param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		protected DataAccess(
            bool MainConn,
            string DSN,
            string Server,
            string Database,
            int esercizio_sessione,
            DateTime DataContabile) {
            CreateDataAccess(MainConn, DSN, Server, Database, esercizio_sessione, DataContabile);
        }

       

        void Init() {
            QHC = new CQueryHelper();
#pragma warning disable CA2214    //Do not call overridable methods in constructors
            //QHS = GetQueryHelper();     creates warning 2214
            QHS = new SqlServerQueryHelper();
#pragma warning restore CA2214
            openError = false;
            ConnectionHasBeenClosedBySystem = false;
            //int hreset = metaprofiler.StartTimer("Reset() in Init");
            Reset(false);
            //metaprofiler.StopTimer(hreset);
            if (openError) return;

            //object O = DO_SYS_CMD("select IS_SRVROLEMEMBER ('sysadmin') AS issysadmin");
            //if ((O != null) && (O.ToString() == "1")) {
            //    sys["IsSystemAdmin"] = true;
            //}
            //else {
            //    sys["IsSystemAdmin"] = false;
            //}

            //System.Reflection.Assembly []myAssemblies =  
            //    AppDomain.CurrentDomain.GetAssemblies();
            //foreach (System.Reflection.Assembly A in myAssemblies){
            //    string []parts= A.FullName.Split(new char[]{','});
            //    string modulename = parts[0].ToLower();
            //    if (modulename=="metadatalibrary"){
            //        string versdescr = parts[1].ToLower();
            //        string modulver= versdescr.Split(new char[]{'='})[1];
            //        sys["MetaDataVersion"]= modulver;
            //    }
            //}
            AssemblyName AN = this.GetType().Assembly.GetName();
            Security.SetSys("MetaDataVersion",  AN.Version.Major + "." + AN.Version.Minor + "." + AN.Version.Build);
            externalUser = Security.GetSys("user").ToString();
        }

        /// <summary>
        /// Forces read of all tables info structure again
        /// </summary>
        public virtual void Reset() {
            Reset(true);
        }

        private static readonly IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();

        /// <summary>
        /// Forces read of all tables info structure again
        /// </summary>
        /// <param name="clearDbStructure"></param>
        public virtual void Reset(bool clearDbStructure) {
            Mypersisting = true;
            PrepareEnabled = false; //vedi eccezioni varie http://support.microsoft.com/kb/913765/it#appliesto
            PreparedCommands = new Hashtable();
            //extendeddbstructure ext= new extendeddbstructure();

            if (!clearDbStructure) return;

            DBstructures = new Hashtable();


            var DD = new dbstructure();
            foreach (string tablename in new string[] { "customobject", "columntypes" }) {
                DataTable T = DD.Tables[tablename];

                //string tablename=T.TableName;

                var DS = new dbstructure();
                DBstructures[tablename] = DS;


                //riempie customobject
                DataRow R1 = DS.customobject.NewRow();
                R1["objectname"] = tablename;
                R1["isreal"] = "S";
                DS.customobject.Rows.Add(R1);
                R1.AcceptChanges();

                //riempie columntypes
                if (tablename == "customobject") creaDatiSistema.Settacustomobject(DS);
                if (tablename == "columntypes") creaDatiSistema.Settacolumntypes(DS);
                //if (tablename=="customtablestructure")	creaDatiSistema.Settacustomtablestructure(DS);				
                //if (tablename=="customview")creaDatiSistema.Settacustomview(DS);				
                //if (tablename=="customviewcolumn") creaDatiSistema.Settacustomviewcolumn(DS);
                //if (tablename=="customviewwhere") creaDatiSistema.Settacustomviewwhere(DS);
                //if (tablename=="customvieworderby") creaDatiSistema.Settacustomvieworderby(DS);
                //if (tablename=="customredirect") creaDatiSistema.Settacustomredirect(DS);
                //if (tablename=="customedit") creaDatiSistema.Settacustomedit(DS);
                //if (tablename=="viewcolumn") creaDatiSistema.Settaviewcolumn(DS);




            }
            //QueryCreator.MarkEvent(OUT);

        }

        #endregion

        /// <summary>
        /// When true, access to the table are prefixed with DBO.  
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public virtual bool TableIsCentralized(string tablename) {
            return true;
        }

        /// <summary>
        /// When true, access to the table are prefixed with DBO. 
        /// </summary>
        /// <param name="procname"></param>
        /// <returns></returns>
		public virtual bool ProcedureIsCentralized(string procname) {
            return true;
        }

      

        /// <summary>
        /// Use another database with this connection
        /// </summary>
        /// <param name="DBName"></param>
		public virtual void ChangeDataBase(string DBName) {
            MySqlConnection.Open();
            MySqlConnection.ChangeDatabase(DBName);
            MySqlConnection.Close();
        }


        /// <summary>
        /// Updates last read access stamp to db 
        /// </summary>
		public  void SetLastRead() {
            Security.SetSys("DataAccessLastRead",DateTime.Now);            
        }

        /// <summary>
        /// Updates last write access stamp to db 
        /// </summary>
        public void SetLastWrite() {
            Security.SetSys("DataAccessLastWrite", DateTime.Now);            
        }

        /// <summary>
        /// Crea un duplicato di un DataAccess, con una nuova connessione allo stesso DB. 
        /// Utile se la connessione deve essere usata in un nuovo thread.
        /// </summary>
        /// <returns></returns>
        public virtual DataAccess Duplicate() {
            DataAccess C;
            if (MultiDB) {
                C = new DataAccess(false, Security.GetSys("dsn").ToString(),
                    Security.GetSys("server").ToString(),
                    Security.GetSys("database").ToString(),
                    Security.GetSys("userdb").ToString(),
                    DefaultSecurity.decryptKey((byte[])Security.GetSys("passworddb")),
                    Security.GetSys("user").ToString(),
                    DefaultSecurity.decryptKey((byte[])Security.GetSys("password")),
                    Security.GetEsercizio(),
                    Security.GetDataContabile());
            }
            else {
                if (SSPI) {
                    C = new DataAccess(false, Security.GetSys("dsn").ToString(),
                        Security.GetSys("server").ToString(),
                        Security.GetSys("database").ToString(),
                        Security.GetEsercizio(),
                        Security.GetDataContabile());
                }
                else {
                    C = new DataAccess(false, Security.GetSys("dsn").ToString(),
                        Security.GetSys("server").ToString(),
                        Security.GetSys("database").ToString(),
                        Security.GetSys("user").ToString(),
                        DefaultSecurity.decryptKey((byte[])Security.GetSys("password")),
                        Security.GetEsercizio(),
                        Security.GetDataContabile());
                }
            }
            C.externalUser = this.externalUser;
            foreach(object tableName in DBstructures.Keys) {
                C.DBstructures[tableName] = DBstructures[tableName];
            }
            
            return C;
        }


        /// <summary>
        /// Release resources
        /// </summary>
        public void Destroy() {
            if (MySqlConnection == null) return;

            SqlTransaction s = CurrTransaction();
            if (s?.Connection != null) {
                s.Rollback();
                LogError("Effettuato un rollback durante una destroy", null);
            }
            if (MySqlConnection.State == ConnectionState.Open) {
                nesting = 0;
                persisting = false;
                SureClosing();
            }


            MySqlConnection.Dispose();
            MySqlConnection = null;
          
            if (PreparedCommands != null) {
                PreparedCommands.Clear();
            }
            //if (DBstructures != null) {
            //    DBstructures.Clear();
            //}



        }
        bool settedArithAborth = false;

        #region Open/Close connection

        /// <summary>
        /// Opens connection asyncronously
        /// </summary>
        /// <returns></returns>
        public virtual async Task<bool> openAsync() {
            if (mainConnection != null) return await mainConnection.openAsync();

            if (openError) return false;
            if (MySqlConnection == null) return false;

            if (MySqlConnection.State == ConnectionState.Open && persisting) {
                if (assureOpen()) {
                    nesting++;
                    openError = false;
                    return true;
                }
                openError = true;
                return false;
            }

            //not persisting
            if ((nesting == 0) || (MySqlConnection.State == ConnectionState.Broken) ||
                (MySqlConnection.State == ConnectionState.Closed)) { //Open only if is not open
                try {
                    if (MySqlConnection.State == ConnectionState.Broken) {
                        MySqlConnection.Close();
                        //PreparedCommands = new Hashtable();
                        MySqlConnection.Open();
                    }

                    if (MySqlConnection.State == ConnectionState.Closed) {
                        //PreparedCommands = new Hashtable();
                        MySqlConnection.Open();
                    }

                }
                catch (Exception E) {
                    //myLastError= E.Message;
                    MarkException("Open: Error connecting to DB", E);
                    openError = true;
                    return false;
                }
            }
            nesting++;
            openError = false;
            if (!settedArithAborth) {
                settedArithAborth = true;
                try {
                    this.DO_SYS_CMD("SET ARITHABORT ON", true);
                }
                catch {
                }
                
            }
            return true;
        }

        /// <summary>
        /// Open the connection (or increment nesting if already open)
        /// </summary>
        /// <returns> true when successfull </returns>
        public virtual bool Open() {
            if (mainConnection != null) return mainConnection.Open();

            if (openError) return false;
            if (MySqlConnection == null) return false;

            if (MySqlConnection.State == ConnectionState.Open && persisting) {
                if (assureOpen()) {
                    nesting++;
                    openError = false;
                    return true;
                }
                openError = true;
                return false;
            }

            //not persisting
            if ((nesting == 0) || (MySqlConnection.State == ConnectionState.Broken) ||
                (MySqlConnection.State == ConnectionState.Closed)) { //Open only if is not open
                try {
                    if (MySqlConnection.State == ConnectionState.Broken) {
                        MySqlConnection.Close();
                        //PreparedCommands = new Hashtable();
                        MySqlConnection.Open();
                    }

                    if (MySqlConnection.State == ConnectionState.Closed) {
                        //PreparedCommands = new Hashtable();
                        MySqlConnection.Open();
                    }

                }
                catch (Exception E) {
                    //myLastError= E.Message;
                    MarkException("Open: Error connecting to DB", E);
                    openError = true;
                    return false;
                }
            }
            nesting++;
            openError = false;
            if (!settedArithAborth) {
                settedArithAborth = true;
                try {
                    this.DO_SYS_CMD("SET ARITHABORT ON", true);
                }
                catch {
                }
                
            }
            return true;
        }


        /// <summary>
        /// Close the connection
        /// </summary>
        public virtual void Close() {
            if (mainConnection != null) {
                mainConnection.Close();
                return;
            }
            if (persisting) {
                if (nesting > 0) nesting--;
                //never closes
                return;
            }
            //not persisting

            if (nesting == 0) return;   //should not happen

            if (nesting == 1) {
                nesting = 0;
                SureClosing();
                return;
            }
            nesting--;
            return;
        }

        DateTime nextCheck = DateTime.Now;

        //async Task<bool>  assureOpenAsync() {
        //    if (NTRANS > 0) return true;
        //    if (nesting > 0) return true;
        //    if (DateTime.Now < nextCheck) return true;
        //    if (await checkStillOpenAsync()) {
        //        nextCheck = DateTime.Now.AddMinutes(3);
        //        return true;
        //    }
        //    return await tryToOpenAsync();
        //}
      

        //async Task<bool> tryToOpenAsync() {
        //    try {
        //        if (MySqlConnection.State == ConnectionState.Broken
        //        ) {
        //            MySqlConnection.Close();
        //            //PreparedCommands = new Hashtable();
        //            await MySqlConnection.OpenAsync();
        //        }

        //        if (MySqlConnection.State == ConnectionState.Closed) {
        //            //PreparedCommands = new Hashtable();
        //            await MySqlConnection.OpenAsync();
        //        }
        //        return true;

        //    }
        //    catch (Exception e) {
        //        //myLastError= E.Message;
        //        MarkException("tryToOpen: Error connecting to DB", e);
        //        openError = true;
        //        return false;
        //    }
        //}

        //async  Task<bool> checkStillOpenAsync() {

        //    if (MySqlConnection.State == ConnectionState.Closed) return false;
        //    if (MySqlConnection.State == ConnectionState.Broken) return false;

        //    SqlCommand Cmd = new SqlCommand("select getdate()", MySqlConnection, null);
        //    Cmd.CommandTimeout = 100;
        //    SqlDataReader Read = null;
        //    try {
        //        Read = await Cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
        //        if (Read.HasRows) {
        //            await Read.ReadAsync();
        //            Read.Close();
        //        }
        //    }
        //    catch (Exception e) {
        //        if (Read != null && !Read.IsClosed) Read.Close();
        //        MarkException("checkStillOpenAsync: connessione assente", e);
        //        return false;
        //    }
        //    return true;
        //}


        bool assureOpen() {
            if (NTRANS > 0) return true;
            if (nesting > 0) return true;
            if (DateTime.Now < nextCheck) return true;
            if (checkStillOpen()) {
                nextCheck = DateTime.Now.AddMinutes(3);
                return true;
            }
            return tryToOpen();
        }

        bool checkStillOpen() {

            if (MySqlConnection.State == ConnectionState.Closed) return false;
            if (MySqlConnection.State == ConnectionState.Broken) return false;

            SqlCommand Cmd = new SqlCommand("select getdate()", MySqlConnection, null) {
                CommandTimeout = 100
            };
            SqlDataReader Read = null;
            try {
                Read = Cmd.ExecuteReader(CommandBehavior.SingleRow);
                object Result = null;
                if (Read.HasRows) {
                    Read.Read();
                    Result = Read[0];
                    Read.Close();
                }
            }
            catch (Exception E) {
                if (Read != null && !Read.IsClosed) Read.Close();
                MarkException("checkStillOpen: connessione assente", E);
                return false;
            }
            return true;
        }

        bool tryToOpen() {
            try {
                if (MySqlConnection.State == ConnectionState.Broken
                    ) {
                    MySqlConnection.Close();
                    //PreparedCommands = new Hashtable();
                    MySqlConnection.Open();
                }

                if (MySqlConnection.State == ConnectionState.Closed) {
                    //PreparedCommands = new Hashtable();
                    MySqlConnection.Open();
                }
                return true;

            }
            catch (Exception E) {
                //myLastError= E.Message;
                MarkException("tryToOpen: Error connecting to DB", E);
                openError = true;
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Reads all data from MetaData-System Tables into a new DBstructure
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual dbstructure GetEntireStructure(string filter) { //MUST BECOME PROTECTED
            
            var DS = new dbstructure();
            foreach (DataTable T in DS.Tables) {
                AddExtendedProperty(T);
                RUN_SELECT_INTO_TABLE(T, null, filter, null, true);
            }
            
            return DS;
        }

        /// <summary>
        /// When false table is not cached in the initialization for a given table
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
		public virtual bool IsToRead(dbstructure DBS, string tablename) { //MUST BECOME PROTECTED
            if ((!ManagedByDB) || (DBS.Tables[tablename] == null)) return false;
            return true;
        }


        /// <summary>
        /// Get structure of a table without reading columntypes
        /// </summary>
        /// <param name="objectname"></param>
        /// <returns></returns>
        public virtual async Task<dbstructure> GetStructureNoCustomAsync(string objectname) {
            object objid = await executeQueryValue("select object_id(" + QHS.quote(objectname) + ")");
            if (objid == DBNull.Value || objid == null) {
                return new dbstructure();
            }

            object sysobj_type =
                await executeQueryValue("select xtype from syscolumns where " + QHS.CmpEq("id", objid));

            string keys = "";
            if (sysobj_type.ToString().ToUpper() == "U") {
                object cnstid = await executeQueryValue("select id from sysobjects where " +
                                                        QHS.AppAnd(QHS.CmpEq("parent_obj", objid),
                                                            QHS.CmpEq("xtype", "PK")));
                if (cnstid != null && cnstid != DBNull.Value) {
                    object indid = await executeQueryValue("select indid from sysindexes where " +
                                                           QHS.AppAnd(QHS.CmpEq("id", objid),
                                                               QHS.CmpEq("name",
                                                                   QHS.Field("object_name(" + QHS.quote(cnstid) +
                                                                             ")"))));

                    int i = 1;
                    object currkey = await executeQueryValue("SELECT index_col(" + QHS.quote(objectname) + "," +
                                                             QHS.quote(indid) + "," + QHS.quote(i) + ")");
                    while (currkey != null && currkey != DBNull.Value) {
                        keys += currkey.ToString() + ",";
                        currkey = await executeQueryValue("SELECT index_col(" + QHS.quote(objectname) + "," +
                                                          QHS.quote(indid) + "," + QHS.quote(i) + ")");
                    }
                }

            }
            
            DataTable T = await executeQuery(
            "select " +
            "'Column_name'			= name," +
            "'Type'					= type_name(xusertype), " +
            "'Length'				= convert(int, length),		" +
            "'Prec'					= convert(char(5),ColumnProperty(id, name, 'precision'))," +
            "'Scale'					= convert(char(5),OdbcScale(xtype,xscale)),		" +
            "'Nullable'				= case when isnullable = 0 then 'no' else 'yes' end " +
            //"--,'Key'					= case when charindex(name+',',@keys)>0 then 'S' else 'N' end"+
            //--'keypos'				= #tempkey.pos
            " from syscolumns  " +
            //--left outer join #tempkey on #tempkey.kname= syscolumns.name
            " where " + QHS.AppAnd(QHS.CmpEq("id", objid), QHS.CmpEq("number", 0) +
            " order by colid "));

            dbstructure DS = new dbstructure();
            ClearDataSet.RemoveConstraints(DS);
            DataRow custobj = DS.customobject.NewRow();
            custobj["objectname"] = objectname;
            custobj["isreal"] = (sysobj_type.ToString().ToUpper() == "U") ? "S" : "N";
            DS.customobject.Rows.Add(custobj);
            string dectypes = "'tinyint,smallint,decimal,int,real,money,float,numeric,smallmoney";
            foreach (DataRow R in T.Rows) {
                DataRow Col = DS.columntypes.NewRow();
                Col["tablename"] = objectname;
                Col["field"] = R["Column_name"];
                Col["defaultvalue"] = "";


                Col["sqltype"] = R["Type"];
                Col["systemtype"] = GetType_Util.GetSystemType_From_SqlDbType(R["Type"].ToString());
                Col["col_len"] = R["Length"];
                if (R["Nullable"].ToString() == "no") {
                    Col["allownull"] = "N";
                    Col["denynull"] = "S";
                }
                else {
                    Col["allownull"] = "S";
                    Col["denynull"] = "N";
                }
                if (dectypes.IndexOf(R["Type"].ToString()) >= 0) {
                    Col["col_precision"] = R["Prec"];
                    Col["col_scale"] = R["Scale"];
                }
                string SqlDecl = R["Type"].ToString();
                if ((SqlDecl == "varchar") || (SqlDecl == "char") || (SqlDecl == "nvarchar") || (SqlDecl == "binary") || (SqlDecl == "varbinary")) {
                    SqlDecl += "(" + R["Length"].ToString() + ")";
                }
                if (SqlDecl == "decimal") {
                    SqlDecl += "(" + R["Prec"].ToString() +
                                "," + R["Scale"].ToString() + ")";
                }
                if (keys.IndexOf(R["Column_name"].ToString() + ",") >= 0) {
                    Col["iskey"] = "S";
                }
                else {
                    Col["iskey"] = "N";
                }
                DS.columntypes.Rows.Add(Col);

            }
            return DS;
        }

        /// <summary>
        /// Get structure of a table without reading columntypes
        /// </summary>
        /// <param name="objectname"></param>
        /// <returns></returns>
        public virtual dbstructure GetStructureNoCustom(string objectname) {
            object objid = DO_SYS_CMD("select object_id(" + QHS.quote(objectname) + ")");
            if (objid == DBNull.Value || objid == null) {
                return new dbstructure();
            }
            object sysobj_type = DO_SYS_CMD("select xtype from syscolumns where " + QHS.CmpEq("id", objid));

            string keys = "";
            if (sysobj_type.ToString().ToUpper() == "U") {
                object cnstid = DO_SYS_CMD("select id from sysobjects where " +
                    QHS.AppAnd(QHS.CmpEq("parent_obj", objid), QHS.CmpEq("xtype", "PK")));
                if (cnstid != null && cnstid != DBNull.Value) {
                    object indid = DO_SYS_CMD("select indid from sysindexes where " +
                        QHS.AppAnd(QHS.CmpEq("id", objid), QHS.CmpEq("name", QHS.Field("object_name(" + QHS.quote(cnstid) + ")"))));

                    int i = 1;
                    object currkey = DO_SYS_CMD("SELECT index_col(" + QHS.quote(objectname) + "," +
                                QHS.quote(indid) + "," + QHS.quote(i) + ")");
                    while (currkey != null && currkey != DBNull.Value) {
                        keys += currkey.ToString() + ",";
                        currkey = DO_SYS_CMD("SELECT index_col(" + QHS.quote(objectname) + "," +
                                QHS.quote(indid) + "," + QHS.quote(i) + ")");
                    }
                }

            }

            DataTable T = this.SQLRunner(
            "select " +
            "'Column_name'			= name," +
            "'Type'					= type_name(xusertype), " +
            "'Length'				= convert(int, length),		" +
            "'Prec'					= convert(char(5),ColumnProperty(id, name, 'precision'))," +
            "'Scale'					= convert(char(5),OdbcScale(xtype,xscale)),		" +
            "'Nullable'				= case when isnullable = 0 then 'no' else 'yes' end " +
            //"--,'Key'					= case when charindex(name+',',@keys)>0 then 'S' else 'N' end"+
            //--'keypos'				= #tempkey.pos
            " from syscolumns  " +
            //--left outer join #tempkey on #tempkey.kname= syscolumns.name
            " where " + QHS.AppAnd(QHS.CmpEq("id", objid), QHS.CmpEq("number", 0) +
            " order by colid "));

            dbstructure DS = new dbstructure();
            ClearDataSet.RemoveConstraints(DS);
            DataRow custobj = DS.customobject.NewRow();
            custobj["objectname"] = objectname;
            custobj["isreal"] = (sysobj_type.ToString().ToUpper() == "U") ? "S" : "N";
            DS.customobject.Rows.Add(custobj);
            string dectypes = "'tinyint,smallint,decimal,int,real,money,float,numeric,smallmoney";
            foreach (DataRow R in T.Rows) {
                DataRow Col = DS.columntypes.NewRow();
                Col["tablename"] = objectname;
                Col["field"] = R["Column_name"];
                Col["defaultvalue"] = "";


                Col["sqltype"] = R["Type"];
                Col["systemtype"] = GetType_Util.GetSystemType_From_SqlDbType(R["Type"].ToString());
                Col["col_len"] = R["Length"];
                if (R["Nullable"].ToString() == "no") {
                    Col["allownull"] = "N";
                    Col["denynull"] = "S";
                }
                else {
                    Col["allownull"] = "S";
                    Col["denynull"] = "N";
                }
                if (dectypes.IndexOf(R["Type"].ToString()) >= 0) {
                    Col["col_precision"] = R["Prec"];
                    Col["col_scale"] = R["Scale"];
                }
                string SqlDecl = R["Type"].ToString();
                if ((SqlDecl == "varchar") || (SqlDecl == "char") || (SqlDecl == "nvarchar") || (SqlDecl == "binary") || (SqlDecl == "varbinary")) {
                    SqlDecl += "(" + R["Length"].ToString() + ")";
                }
                if (SqlDecl == "decimal") {
                    SqlDecl += "(" + R["Prec"].ToString() +
                                "," + R["Scale"].ToString() + ")";
                }

                R["sqldeclaration"] = SqlDecl;
                if (keys.IndexOf(R["Column_name"].ToString() + ",") >= 0) {
                    Col["iskey"] = "S";
                }
                else {
                    Col["iskey"] = "N";
                }
                DS.columntypes.Rows.Add(Col);

            }
            return DS;
        }

        /// <summary>
        /// Reads table structure of a list of tables
        /// </summary>
        /// <param name="tableName"></param>
        public virtual void preScanStructures(params string[] tableName) {
            string[] toread = (from t in tableName where !DBstructures.ContainsKey(t) select t).ToArray();
            if (toread.Length == 0) return;
            int handle = metaprofiler.StartTimer("preScanStructures()");
            string cmd = "select objectname,description,isreal,realtable,lastmodtimestamp,lastmoduser from customobject where " + QHS.FieldIn("objectname", toread) + ";" +
                "select tablename,field,iskey,sqltype,col_len,col_precision,col_scale,systemtype,sqldeclaration,allownull,defaultvalue,"+
                    "format,denynull,lastmodtimestamp,lastmoduser,createuser,createtimestamp from columntypes where " + QHS.FieldIn("tablename", toread);
            int nSetToRead = 2;
            if (ManagedByDB) {
                cmd+= ";select * from customtablestructure where " + QHS.FieldIn("objectname", toread);
                nSetToRead = 3;
            }
            
            var ST = new Dictionary<string, dbstructure>();
            foreach(string tName in toread) {
                var di = new dbstructure();
                ClearDataSet.RemoveConstraints(di);
                ST[tName] = di;
            }            
            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return;
            }
            var Cmd = new SqlCommand(cmd, MySqlConnection, CurrTransaction()) {
                CommandTimeout = 600
            };
            SqlDataReader rdr = null;
           
            try {
                rdr = Cmd.ExecuteReader();
                int nSet = 0;
                int countField = rdr.FieldCount;

                while (nSet < nSetToRead) {
                    if (!rdr.HasRows) {
                        nSet++;
                        rdr.NextResult();
                        countField = rdr.FieldCount;
                        continue;
                    }
                    while (rdr.Read()) {
                        switch (nSet) {
                            case 0:
                                //gets customobject
                                if (!ST.ContainsKey(rdr["objectname"].ToString())) {
									QueryCreator.MarkEvent("manca tabella "+rdr["objectname"].ToString());
									break;
                                }
                                var d = ST[rdr["objectname"].ToString()];
                                var rObj = d.customobject.NewRow();
                                for (int i = countField-1; i >= 0; i--) rObj[i] = rdr[i];
                                d.customobject.Rows.Add(rObj);
                                break;
                            case 1:
                                //gets columntypes
                                var d1 = ST[rdr["tablename"].ToString()];
                                var rCol = d1.columntypes.NewRow();
                                for (int i = countField - 1; i >= 0; i--) rCol[i] = rdr[i];
                                d1.columntypes.Rows.Add(rCol);
                                break;
                            case 2:
                                var d2 = ST[rdr["objectname"].ToString()];
                                var rTabStr = d2.customtablestructure.NewRow();
                                for (int i = countField - 1; i >= 0; i--) rTabStr[i] = rdr[i];
                                d2.customtablestructure.Rows.Add(rTabStr);
                                break;                            
                        }
                    }

                    rdr.NextResult();
                    countField = rdr.FieldCount;
                    nSet++;
                }

            }
            catch (Exception e){
                MarkException("preScan", e);
            }
            rdr?.Dispose();

			Cmd.Dispose();
            Close();
            foreach (string tName in toread) {
                ST[tName].AcceptChanges();
                DBstructures[tName] = ST[tName];
            }
            metaprofiler.StopTimer(handle);

        }
        /// <summary>
        /// Gets DB structure related to table objectname. The dbstructure returned
        ///  is the same used for sys operations (it is not a copy of it)
        /// </summary>
        /// <param name="objectname"></param>
        /// <returns></returns>
        public virtual dbstructure GetStructure(string objectname) {

            var DS = (dbstructure)DBstructures[objectname];
            if (DS != null) return DS;

            if (UseCustomObject == false) {
                //return GetStructureNoCustom(objectname);
                DBstructures[objectname] = GetStructureNoCustom(objectname);
                return (dbstructure)DBstructures[objectname];
            }

            //int handle = metaprofiler.StartTimer("GetStructure("+objectname+")");
            //if (objectname== "sysobjects") return new dbstructure();

            int handle = metaprofiler.StartTimer($"GetStructure*{objectname}");
            DS = new dbstructure();
            ClearDataSet.RemoveConstraints(DS);
            string filtercol = QHS.CmpEq("tablename", objectname);// "(tablename='" + objectname + "')";
            string filtertab = QHS.CmpEq("objectname", objectname);// "(objectname='" + objectname + "')";
            RUN_SELECT_INTO_TABLE(DS.customobject, null, filtertab, null, true);
            if (DS.customobject.Rows.Count == 0) {
                AutoDetectTable(DS, objectname, false);
            }
            RUN_SELECT_INTO_TABLE(DS.columntypes, null, filtercol, null, true);
            if (IsToRead(DS, "customtablestructure"))
                RUN_SELECT_INTO_TABLE(DS.customtablestructure, null, filtertab, null, true);
            DBstructures[objectname] = DS;
            metaprofiler.StopTimer(handle);
            return DS;
        }

        /// <summary>
        /// Return something like SELECT {colTable1} from {table1.TableName} JOIN {table2} ON  {joinFilter} [WHERE {whereFilter}]
        /// </summary>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="filterTable1"></param>
        /// <param name="filterTable2"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual string getJoinSql(DataTable table1, string table2, MetaExpression filterTable1, MetaExpression filterTable2,
            params string[] columns) {
            string colTable1 = string.Join(",", (from c in table1.Columns._names() where QueryCreator.IsReal(table1.Columns[c]) select table1.TableName+"."+c  ).ToArray());
            MetaExpression exprJoin = null;
            var clauseJoin = (from c in columns select MetaExpression.eq(MetaExpression.field(c,table1.TableName), MetaExpression.field(c, table2))).ToArray();
            if (clauseJoin.Length == 1) {
                exprJoin = clauseJoin[0];
            }
            else {
                exprJoin = MetaExpression.and(clauseJoin);
            }

            var qhs = GetQueryHelper();
            string joinFilter = exprJoin.toSql(qhs);
            MetaExpression filter = null;
            if (filterTable1 != null) {
                filterTable1.cascadeSetTable(table1.TableName);
                filter = filterTable1;
            }

            if (filterTable2 != null) {
                filterTable2.cascadeSetTable(table2);
                if (filter == null) {
                    filter = filterTable2;
                }
                else {
                    filter &= filterTable2;
                }
            }

            string whereFilter = filter.toSql(qhs);
            return    (whereFilter == null) ?
                $"SELECT {colTable1} from {table1.TableName} JOIN {table2} ON  {joinFilter}":
                $"SELECT {colTable1} from {table1.TableName} JOIN {table2} ON  {joinFilter} WHERE {whereFilter} ";
        }

        /// <summary>
        /// Gets DB structure related to table objectname. The dbstructure returned
        ///  is the same used for sys operations (it is not a copy of it)
        /// </summary>
        /// <param name="objectname"></param>
        /// <returns></returns>
        public virtual async Task<dbstructure> GetStructureAsync(string objectname) {

            dbstructure DS = (dbstructure)DBstructures[objectname];
            if (DS != null) return DS;

            if (UseCustomObject == false) {
                //return GetStructureNoCustom(objectname);
                DBstructures[objectname] = await GetStructureNoCustomAsync(objectname);
                return (dbstructure)DBstructures[objectname];
            }

            //int handle = metaprofiler.StartTimer("GetStructure("+objectname+")");
            //if (objectname== "sysobjects") return new dbstructure();

            DS = new dbstructure();
            ClearDataSet.RemoveConstraints(DS);
            string filtercol = "(tablename='" + objectname + "')";
            string filtertab = "(objectname='" + objectname + "')";
            await executeQueryIntoTable(DS.customobject, filtertab);
            if (DS.customobject.Rows.Count == 0) {
                AutoDetectTable(DS, objectname, false);
            }
            await executeQueryIntoTable(DS.columntypes, filtercol);
            if (IsToRead(DS, "customtablestructure")) {
                await executeQueryIntoTable(DS.customtablestructure, filtertab);

            }             
            DBstructures[objectname] = DS;
            return DS;
        }
        /// <summary>
        /// Read a bunch of table structures, all those present in the DataSet
        /// </summary>
        /// <param name="D"></param>
        /// <param name="primarytable"></param>
		public virtual void PrefillStructures(DataSet D, string primarytable) {
            string[] tabnames = new string[D.Tables.Count];
            int ntables = 0;
            var ListaDS = new dbstructure();
            ClearDataSet.RemoveConstraints(ListaDS);
            string filtercol = "";//"(tablename in ("+tablelist+"))";
            string filtertab = "";//"(objectname in ("+tablelist+"))";

            foreach (DataTable T in D.Tables) {
                if (DBstructures[T.TableName] != null) continue;
                if (model.isCached(T) ||
                    QueryCreator.IsSubEntity(T, D.Tables[primarytable])) {
                    tabnames[ntables] = T.TableName;
                    ntables++;
                    continue;
                }
            }
            if (ntables == 0) return;

            for (int i = 0; i < ntables; i++) {
                if (filtercol != "") filtercol += "OR";
                if (filtertab != "") filtertab += "OR";
                filtercol += "(tablename=" + mdl_utils.Quoting.quotedstrvalue(tabnames[i], true) + ")";
                filtertab += "(objectname=" + mdl_utils.Quoting.quotedstrvalue(tabnames[i], true) + ")";
            }

            //Effettua le letture in blocco e poi le smista
            RUN_SELECT_INTO_TABLE(ListaDS.customobject, null, filtertab, null, true);
            RUN_SELECT_INTO_TABLE(ListaDS.columntypes, null, filtercol, null, true);
            if (IsToRead(ListaDS, "customtablestructure"))
                RUN_SELECT_INTO_TABLE(ListaDS.customtablestructure, null, filtertab, null, true);


            for (int i = 0; i < ntables; i++) {
                string tname = tabnames[i];
                dbstructure DS = new dbstructure();
                ClearDataSet.RemoveConstraints(DS);
                if ((ListaDS.Tables["customobject"].Select("objectname=" +
                            mdl_utils.Quoting.quotedstrvalue(tname, false)).Length == 0) &&
                    (DS.Tables[tname] == null)) {
                    AutoDetectTable(DS, tname, false);
                }
                string filteronecol = "(tablename='" + tname + "')";
                string filteronetab = "(objectname='" + tname + "')";
                DS.Merge(ListaDS.Tables["customobject"].Select(filteronetab));
                DS.Merge(ListaDS.Tables["columntypes"].Select(filteronecol));
                DS.Merge(ListaDS.Tables["customtablestructure"].Select(filteronetab));
                DBstructures[tname] = DS;
            }
           
        }


        /// <summary>
        /// Saves a table structure to DB (customobject, columntypes..)
        /// </summary>
        /// <param name="DBS"></param>
        /// <returns></returns>
		public virtual bool SaveStructure(dbstructure DBS) { //MUST BECOME PROTECTED						
            PostData.RemoveFalseUpdates(DBS);
            if (!DBS.HasChanges()) return true;
            PostData post = new PostData();
            post.initClass(DBS, this);
            ProcedureMessageCollection MC = null;
            while ((MC == null) ||
                ((MC != null) && MC.CanIgnore && (MC.Count > 0))) {
                MC = post.DO_POST_SERVICE();
            }
            if (MC.Count == 0) return true;
            return false;
        }


        /// <summary>
        /// Saves all changes made to all dbstructures
        /// </summary>
        /// <returns></returns>
        public virtual bool SaveStructure() {
            bool res = true;
            foreach (dbstructure DBS in DBstructures.Values) {
                res = SaveStructure(DBS);
                if (!res) break;
            }
            return res;
        }

        /// <summary>
        /// Evaluate columntypes and customobject analizing db table properties
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="objectname"></param>
        /// <param name="forcerefresh">if false, only new tables are scanned</param>
		public virtual void AutoDetectTable(dbstructure DBS, string objectname, bool forcerefresh) {//MUST BECOME PROTECTED
            if ((DBS.customobject.Rows.Count > 0) && (!forcerefresh)) return;
            bool IsReal = false;
            bool IsView = false;
            object nameReal = DO_READ_VALUE("sysobjects", QHS.AppAnd(QHS.CmpEq("xtype", "U"), QHS.CmpEq("name", objectname)), "name", null);
            if (nameReal == DBNull.Value || nameReal == null) {
                object nameView = DO_READ_VALUE("sysobjects", QHS.AppAnd(QHS.CmpEq("xtype", "V"), QHS.CmpEq("name", objectname)), "name", null);
                if (nameView != DBNull.Value && nameView != null) {
                    IsView = true;
                }
            }
            else {
                IsReal = true;
            }

            if ((!IsReal) && (!IsView)) return;
            DataRow CurrObj;
            if (DBS.customobject.Rows.Count == 0) {
                CurrObj = DBS.customobject.NewRow();
                CurrObj["objectname"] = objectname;
                DBS.customobject.Rows.Add(CurrObj);
            }
            else {
                CurrObj = DBS.customobject.Rows[0];
            }
            CurrObj["isreal"] = IsReal ? "S" : "N";
            dbanalyzer.ReadColumnTypes(DBS.columntypes, objectname, this);
            SaveStructure(DBS);

        }
		/// <summary>
		/// Reads all table structures from db
		/// </summary>
        public virtual void readStructuresFromDb() {
	        ArrayList Tables = dbanalyzer.TableListFromDB(this);
	        foreach (string tablename in Tables) {
		        //Application.DoEvents();
		        dbstructure DBS = GetStructure(tablename);
		        if (DBS.customobject.Rows.Count == 0) {
			        DataRow newobj = DBS.customobject.NewRow();
			        newobj["objectname"] = tablename;
			        newobj["isreal"] = "S";
			        DBS.customobject.Rows.Add(newobj);
		        }
		        dbanalyzer.ReadColumnTypes(DBS.columntypes, tablename, this);
	        }
	        ArrayList Views = dbanalyzer.ViewListFromDB(this);
	        foreach (string tablename in Views) {
		        //Application.DoEvents();
		        dbstructure DBS = (dbstructure)GetStructure(tablename);
		        if (DBS.customobject.Rows.Count == 0) {
			        DataRow newobj = DBS.customobject.NewRow();
			        newobj["objectname"] = tablename;
			        newobj["isreal"] = "N";
			        DBS.customobject.Rows.Add(newobj);
		        }
		        dbanalyzer.ReadColumnTypes(DBS.columntypes, tablename, this);
	        }
        }

      

        /// <summary>
        /// Forces ColumnTypes to be read again from DB for tablename
        /// </summary>
        /// <param name="tablename"></param>
        public virtual void RefreshStructure(string tablename) {
            dbstructure DBS = (dbstructure)GetStructure(tablename);
            if (DBS == null) return;
            dbanalyzer.ReadColumnTypes(DBS.columntypes, tablename, this);
        }





        /// <summary>
        /// Reads extended informations for a table related to a view,
        ///  in order to use it for posting. Reads data from viewcolumn.
        ///  Sets table and columnfor posting and also 
        ///  sets ViewExpression as tablename.columnname (for each field)
        /// </summary>
        /// <param name="T"></param>
        public virtual void GetViewStructureExtProperties(DataTable T) {
            if (T.tableForPosting() != T.TableName) return;
            int handle = metaprofiler.StartTimer("GetViewStructureExtProperties(" + T.TableName + ")");
            dbstructure DBS = (dbstructure)GetStructure(T.TableName);
            if (DBS.customobject.Rows.Count == 0) {
                metaprofiler.StopTimer(handle);
                return;
            }
            DataRow CurrObj = DBS.customobject.Rows[0];
            if (CurrObj["isreal"].ToString().ToLower() != "n") {
                metaprofiler.StopTimer(handle);
                return;
            }
            string primarytable = CurrObj["realtable"].ToString();
            if (primarytable == "") {
                metaprofiler.StopTimer(handle);
                return;
            }
            QueryCreator.SetTableForPosting(T, primarytable);

            Hashtable Read = (Hashtable)DBS.viewcolumn.ExtendedProperties["AlreadyRead"];
            if (Read == null) {
                Read = new Hashtable();
                DBS.viewcolumn.ExtendedProperties["AlreadyRead"] = Read;
            }

            if (Read["1"] == null) {
                string filter = "(objectname='" + T.TableName + "')";
                RUN_SELECT_INTO_TABLE(DBS.viewcolumn, null, filter, null, true);
                Read["1"] = "1";
            }

            //as default, no column is to post
            foreach (DataColumn C in T.Columns) {
                QueryCreator.SetColumnNameForPosting(C, "");
            }
            foreach (DataRow curCol in DBS.viewcolumn.Rows) {
                DataColumn Col = T.Columns[curCol["colname"].ToString()];
                if (Col == null) continue;
                string postingcol;
                string viewexpr;
                if (curCol["realtable"].ToString() == primarytable) {
                    postingcol = curCol["realcolumn"].ToString();
                    viewexpr = curCol["realcolumn"].ToString();
                }
                else {
                    postingcol = ""; //correctly, instead of null which would mean realcolumn
                    viewexpr = curCol["realtable"].ToString() + "." + curCol["realcolumn"].ToString();
                }
                QueryCreator.SetColumnNameForPosting(Col, postingcol);
                QueryCreator.SetViewExpression(Col, viewexpr);
            }
            metaprofiler.StopTimer(handle);

        }

          /// <summary>
        /// Reads extended informations for a table related to a view,
        ///  in order to use it for posting. Reads data from viewcolumn.
        ///  Sets table and columnfor posting and also 
        ///  sets ViewExpression as tablename.columnname (for each field)
        /// </summary>
        /// <param name="T"></param>
        public virtual async Task GetViewStructureExtPropertiesAsync(DataTable T) {
            if (T.tableForPosting() != T.TableName) return;

              dbstructure DBS = await GetStructureAsync(T.TableName);
            if (DBS.customobject.Rows.Count == 0) {
                return;
            }
            DataRow CurrObj = DBS.customobject.Rows[0];
            if (CurrObj["isreal"].ToString().ToLower() != "n") {
                return;
            }
            string primarytable = CurrObj["realtable"].ToString();
            if (primarytable == "") {
                return;
            }
            QueryCreator.SetTableForPosting(T, primarytable);

            Hashtable Read = (Hashtable)DBS.viewcolumn.ExtendedProperties["AlreadyRead"];
            if (Read == null) {
                Read = new Hashtable();
                DBS.viewcolumn.ExtendedProperties["AlreadyRead"] = Read;
            }

            if (Read["1"] == null) {
                string filter = "(objectname='" + T.TableName + "')";
                await executeQueryIntoTable(DBS.viewcolumn, filter);
                Read["1"] = "1";
            }

            //as default, no column is to post
            foreach (DataColumn C in T.Columns) {
                QueryCreator.SetColumnNameForPosting(C, "");
            }
            foreach (DataRow curCol in DBS.viewcolumn.Rows) {
                DataColumn Col = T.Columns[curCol["colname"].ToString()];
                if (Col == null) continue;
                string postingcol;
                string viewexpr;
                if (curCol["realtable"].ToString() == primarytable) {
                    postingcol = curCol["realcolumn"].ToString();
                    viewexpr = curCol["realcolumn"].ToString();
                }
                else {
                    postingcol = ""; //correctly, instead of null which would mean realcolumn
                    viewexpr = curCol["realtable"].ToString() + "." + curCol["realcolumn"].ToString();
                }
                QueryCreator.SetColumnNameForPosting(Col, postingcol);
                QueryCreator.SetViewExpression(Col, viewexpr);
            }
            

        }

        /// <summary>
        /// Creates a DataTable given it's db name
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="tablename"></param>
        /// <param name="columnlist">list of columns to include in the table</param>
        /// <returns></returns>
		public static DataTable CreateTableByName(DataAccess Conn, string tablename, string columnlist) {
            //byte []B = Conn.CreateByteTableByName(tablename,columnlist);
            //DataSet D = UnpackDataSet(Conn,B);
            //DataTable T = D.Tables[0];
            //D.Tables.Remove(T);
            //return T;
            return Conn.CreateTableByName(tablename, columnlist);
        }

  //      /// <summary>
  //      /// Creates a table and returns it in a packed dataset
  //      /// </summary>
  //      /// <param name="tablename"></param>
  //      /// <param name="columnlist"></param>
  //      /// <returns></returns>
		//private byte[] CreateByteTableByName(string tablename, string columnlist) {
  //          DataTable T = CreateTableByName(tablename, columnlist);
  //          DataSet D = new DataSet();
  //          D.Tables.Add(T);
  //          return PackDataSet(this, D);
  //      }



        /// <summary>
        ///  Creates a DataTable given it's db name
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <returns></returns>
		public virtual DataTable CreateTableByName(string tablename, string columnlist) {
            return CreateTableByName(tablename, columnlist, false);
        }


         /// <summary>
        /// Creates a new table basing on columntypes info. Adds also primary key 
        ///  information to the table, and allownull to each field.
        ///  Columnlist must include primary table, or can be "*"
        /// </summary>
        /// <param name="tablename">name of table to create. Can be in the form DBO.tablename or department.tablename</param>
        /// <param name="columnlist"></param>
        /// <param name="addextprop">Add db information as extended propery of columns (column length, precision...)</param>
        /// <returns>a table with same types as DB table</returns>
        public virtual async Task<DataTable> createTableByNameAsync(string tablename, string columnlist, bool addextprop) {
            if (tablename.Contains(".")) {
                int N = tablename.LastIndexOf('.');
                tablename = tablename.Substring(N + 1);
            }


            if (columnlist == null) columnlist = "*";
            else columnlist = columnlist.Trim();


            if (tablename == "customobject") {
	            var DBSS = new dbstructure();
	            var TT = SingleTableClone(DBSS.customobject, addextprop);
                if (addextprop) await AddExtendedPropertyAsync(TT);
                return TT;
            }
            if (tablename == "columntypes") {
	            var DBSS = new dbstructure();
	            var TT = SingleTableClone(DBSS.columntypes, addextprop);
                if (addextprop) await AddExtendedPropertyAsync(TT);
                return TT;
            }
            if (tablename == "customtablestructure") {
	            var DBSS = new dbstructure();
	            var TT = SingleTableClone(DBSS.customtablestructure, addextprop);
                if (addextprop) await AddExtendedPropertyAsync(TT);
                return TT;
            }


            var T = new DataTable(tablename);
            var DBS = await GetStructureAsync(tablename);
            if (DBS.columntypes.Rows.Count == 0) {
                return T;
            }

            if (columnlist == "*") {
                foreach (var Col in DBS.columntypes.Select(null, "iskey desc, field asc")) {
	                var C = new DataColumn(Col["field"].ToString());
                    if (Col["allownull"].ToString() == "N") C.AllowDBNull = false;
                    else C.AllowDBNull = true;
                    C.DataType = GetType_Util.GetSystemType_From_SqlDbType(Col["sqltype"].ToString());
                    T.Columns.Add(C);
                }
            }
            else {
                string[] ColNames = columnlist.Split(new char[] { ',' });
                foreach (string ColName in ColNames) {
                    string filterCol = "(field=" + mdl_utils.Quoting.quotedstrvalue(ColName.Trim(), false) + ")";
                    DataRow[] Cols = DBS.columntypes.Select(filterCol);
                    if (Cols.Length == 0) continue;
                    var Col = Cols[0];
                    var C = new DataColumn(Col["field"].ToString());
                    if (Col["allownull"].ToString() == "N") C.AllowDBNull = false;
                    else C.AllowDBNull = true;
                    C.DataType = GetType_Util.GetSystemType_From_SqlDbType(Col["sqltype"].ToString());
                    T.Columns.Add(C);
                }
            }

            //Add primary key to table
            string filterkey = "(iskey='S')";
            DataRow[] keycols = DBS.columntypes.Select(filterkey);
            DataColumn[] Key = new DataColumn[keycols.Length];
            for (int i = 0; i < keycols.Length; i++) {
                Key[i] = T.Columns[keycols[i]["field"].ToString()];
            }
            if (Key.Length > 0) T.PrimaryKey = Key;
            await GetViewStructureExtPropertiesAsync(T);
            

            if (addextprop) await AddExtendedPropertyAsync(T);

            return T;
        }

        /// <summary>
        /// Creates a new table basing on columntypes info. Adds also primary key 
        ///  information to the table, and allownull to each field.
        ///  Columnlist must include primary table, or can be "*"
        /// </summary>
        /// <param name="tablename">name of table to create. Can be in the form DBO.tablename or department.tablename</param>
        /// <param name="columnlist"></param>
        /// <param name="addextprop">Add db information as extended propery of columns (column length, precision...)</param>
        /// <returns>a table with same types as DB table</returns>
        public virtual DataTable CreateTableByName(string tablename, string columnlist, bool addextprop) {
            if (tablename.Contains(".")) {
                int N = tablename.LastIndexOf('.');
                tablename = tablename.Substring(N + 1);
            }


            if (columnlist == null) columnlist = "*";
            else columnlist = columnlist.Trim();


            if (tablename == "customobject") {
	            var DBSS = new dbstructure();
	            var TT = SingleTableClone(DBSS.customobject, addextprop);
                if (addextprop) AddExtendedProperty(TT);
                return TT;
            }
            if (tablename == "columntypes") {
	            var DBSS = new dbstructure();
	            var TT = SingleTableClone(DBSS.columntypes, addextprop);
                if (addextprop) AddExtendedProperty(TT);
                return TT;
            }
            if (tablename == "customtablestructure") {
	            var DBSS = new dbstructure();
	            var TT = SingleTableClone(DBSS.customtablestructure, addextprop);
                if (addextprop) AddExtendedProperty(TT);
                return TT;
            }

            //int handle = metaprofiler.StartTimer("Inside CreateTableByName("+tablename+")");
            int handle = metaprofiler.StartTimer("Inside CreateTableByName()");

            var T = new DataTable(tablename);
            var DBS = GetStructure(tablename);
            if (DBS.columntypes.Rows.Count == 0) {
                metaprofiler.StopTimer(handle);
                return T;
            }

            if (columnlist == "*") {
                foreach (var Col in DBS.columntypes.Select(null, "iskey desc, field asc")) {
                    var C = new DataColumn(Col["field"].ToString()) {
                        AllowDBNull = Col["allownull"].ToString() != "N",
                        DataType = GetType_Util.GetSystemType_From_SqlDbType(Col["sqltype"].ToString())
                    };
                    if (Col["sqldeclaration"].ToString() == "text") {
	                    C.ExtendedProperties["sqldeclaration"] = "text";
                    }
                    T.Columns.Add(C);
                }
            }
            else {
                string[] ColNames = columnlist.Split(new char[] { ',' });
                foreach (string ColName in ColNames) {
                    var Cols = DBS.columntypes.Select(QHC.CmpEq("field",ColName.Trim()));
                    if (Cols.Length == 0) continue;
                    var Col = Cols[0];
                    var C = new DataColumn(Col["field"].ToString()) {
                        AllowDBNull = Col["allownull"].ToString() != "N",
                        DataType = GetType_Util.GetSystemType_From_SqlDbType(Col["sqltype"].ToString())
                    };
                    if (Col["sqldeclaration"].ToString() == "text") {
	                    C.ExtendedProperties["sqldeclaration"] = "text";
                    }
                    T.Columns.Add(C);
                }
            }

            //Add primary key to table
            DataRow[] keycols = DBS.columntypes.Select(QHC.CmpEq("iskey","S"));
            DataColumn[] Key = new DataColumn[keycols.Length];
            for (int i = 0; i < keycols.Length; i++) {
                Key[i] = T.Columns[keycols[i]["field"].ToString()];
            }
            if (Key.Length > 0) T.PrimaryKey = Key;
            GetViewStructureExtProperties(T);
            metaprofiler.StopTimer(handle);

            if (addextprop) AddExtendedProperty(T);

            return T;
        }

        /// <summary>
        /// Adds all extended information to table T reading it from columntypes.
        /// Every Row of columntypes is assigned to the corresponding extended 
        ///  properties of a DataColumn of T. Each Column of the Row is assigned
        ///  to an extended property with the same name of the Column
        ///  Es. R["a"] is assigned to Col.ExtendedProperty["a"]
        /// </summary>
        /// <param name="T"></param>
        public virtual async Task AddExtendedPropertyAsync(DataTable T) {

	        var dbs = await GetStructureAsync(T.TableName);
            foreach (DataRow col in dbs.columntypes.Select()) {
                string field = col["field"].ToString();
                if (!T.Columns.Contains(field)) continue;
                var c = T.Columns[col["field"].ToString()];
                foreach (DataColumn columnProperty in dbs.columntypes.Columns) {
                    c.ExtendedProperties[columnProperty.ColumnName] =
                        col[columnProperty].ToString();
                }
            }

        }

        /// <summary>
        /// Adds all extended information to table T reading it from columntypes.
        /// Every Row of columntypes is assigned to the corresponding extended 
        ///  properties of a DataColumn of T. Each Column of the Row is assigned
        ///  to an extended property with the same name of the Column
        ///  Es. R["a"] is assigned to Col.ExtendedProperty["a"]
        /// </summary>
        /// <param name="T"></param>
        public virtual void AddExtendedProperty(DataTable T) {
            int handle = metaprofiler.StartTimer($"AddExtendedProperty*{T.TableName}");

            var dbs = GetStructure(T.tableForReading());
            foreach (var col in dbs.columntypes.Select()) {
                string field = col["field"].ToString();
                if (!T.Columns.Contains(field)) continue;
                var c = T.Columns[col["field"].ToString()];
                foreach (DataColumn columnProperty in dbs.columntypes.Columns) {
                    c.ExtendedProperties[columnProperty.ColumnName] =
                        col[columnProperty].ToString();
                }
            }
            metaprofiler.StopTimer(handle);

        }

     

        /// <summary>
        /// Adds extended properties on the columns of a table
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="T"></param>
        public static void addExtendedProperty(IDataAccess Conn, DataTable T) {
            int handle = metaprofiler.StartTimer("AddExtendedProperty*"+T.TableName);
            var COLTYPES = Conn.RUN_SELECT("columntypes", "*", null, qhc.CmpEq("tablename",T.TableName), null, false);

            foreach (DataRow Col in COLTYPES.Select()) {
	            var C = T.Columns[Col["field"].ToString()];
                if (C == null) continue;
                foreach (DataColumn ColumnProperty in COLTYPES.Columns) {
                    C.ExtendedProperties[ColumnProperty.ColumnName] =
                        Col[ColumnProperty].ToString();
                }
            }
            metaprofiler.StopTimer(handle);

        }



        //			public static void AddExtendedProperty(DataSet DS, string tablename){
        //				int handle = metaprofiler.StartTimer("AddExtendedProperty(DataTable T)");
        //			
        //				dbstructure DBS = GetStructure(T.TableName);
        //				foreach(DataRow Col in DBS.columntypes.Rows){
        //					DataColumn C = T.Columns[Col["field"].ToString()];
        //					if (C==null) continue;				
        //					foreach (DataColumn ColumnProperty in DBS.columntypes.Columns){
        //						C.ExtendedProperties[ColumnProperty.ColumnName]=
        //							Col[ColumnProperty].ToString();
        //					}
        //				}
        //				metaprofiler.StopTimer(handle);
        //
        //			}



        /// <summary>
        /// Returns the primary table of a given DBstructure. It is the objectname of
        ///  the only row contained in DBS.customobject
        /// </summary>
        /// <param name="DBS"></param>
        /// <returns></returns>
        public static string PrimaryTableOf(dbstructure DBS) {
            return DBS.customobject.Rows[0]["objectname"].ToString();
        }


        #region Select command preparing

        /// <summary>
        /// Returns a prepared version of cmd, retrieving it from cache if it
        ///  has already been prepared.
        /// </summary>
        SqlCommand GetPreparedCommand(SqlCommand cmd) {
            if (!PrepareEnabled) return cmd;
            int handle = 0;
            try {
                handle = metaprofiler.StartTimer("Getting Prepared Cmd...");
                var prepared = (SqlCommand)PreparedCommands[cmd.CommandText];
                if (prepared != null) {
                    prepared.Transaction = cmd.Transaction;
                    for (int i = 0; i < prepared.Parameters.Count; i++)
                        prepared.Parameters[i].Value = cmd.Parameters[i].Value;
                    metaprofiler.StopTimer(handle);
                    return prepared;
                }
                cmd.Prepare();
                PreparedCommands[cmd.CommandText] = cmd;
                metaprofiler.StopTimer(handle);
                return cmd;
            }
            catch (Exception E) {
                MarkException("GetPreparedCommand: Error Preparing command ["
                    + cmd.CommandText
                    + "]"
                    , E);
                metaprofiler.StopTimer(handle);
                return cmd;
            }
        }


      

        const string param_mask = "@par";

        /// <summary>
        /// Does not Assumes Open parenthesis read
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="filter"></param>
        /// <param name="len"></param>
        /// <param name="nesting"></param>
        /// <param name="pos">index of first character to be read</param>
        /// <param name="compiled"></param>
        /// <param name="paramsfound"></param>
        /// <param name="ColumnTypes"></param>
        void CompileConditionInParenthesis(
            ref SqlCommand Cmd,
            string filter,
            int len,
            int nesting,
            ref int pos,
            ref string compiled,
            ref int paramsfound,
            DataTable ColumnTypes) {
            while (pos < len) {
                //copy connector skipping blanks
                while ((pos < len) && (filter[pos] != '(')) {

                    if (filter[pos] == '\'') {
                        //skips  the string constant 
                        compiled += filter[pos];
                        pos++;
                        //skips the string
                        while (pos < len) {
                            if (filter[pos] != '\'') {
                                compiled += filter[pos];
                                pos++;
                                continue;
                            }
                            //it could be an end-string character
                            if (((pos + 1) < len) && (filter[pos + 1] == '\'')) {
                                //it isn't
                                compiled += filter[pos];
                                pos++;
                                compiled += filter[pos];
                                pos++;
                                continue;
                            }
                            compiled += filter[pos];
                            pos++;
                            break;
                        }
                        continue; //resume primary cicle (skip blanks)
                    }

                    if (filter[pos] != ' ') compiled += filter[pos];
                    pos++;
                    if ((filter[pos - 1] == ')') && (nesting > 0)) return; //it was nested
                }
                if (pos == len) break;
                compiled += '(';
                pos++;
                //skips blanks
                while (filter[pos] == ' ') pos++;

                if (filter[pos] == '(') {
                    CompileConditionInParenthesis(ref Cmd, filter, len, nesting + 1,
                        ref pos, ref compiled, ref paramsfound, ColumnTypes);
                    continue;
                }

                //gets fieldname
                string fieldname = "";
                while (IsIdentifier(filter[pos])) fieldname += filter[pos++];
                compiled += fieldname;
                DataRow[] descfield = ColumnTypes.Select("(field='" + fieldname + "')");
                SqlDbType type = SqlDbType.Int;   //dummy
                DataRow FieldDesc = null; //parameter type description

                if (descfield.Length > 0) {
                    FieldDesc = descfield[0];
                    type = GetType_Util.GetSqlType_From_StringSqlDbType(FieldDesc["sqltype"].ToString());
                }

                bool prevwasidentifier = (fieldname != "");

                //finds eventually constants in string and compiles them, till closed parentesis ")"
                // nested parenthesis are not managed
                int level = 1;
                while ((pos < len) && (level > 0)) {
                    char Curr = filter[pos];
                    if (Curr == '\'') {
                        paramsfound++;
                        string paramname = param_mask + paramsfound.ToString();
                        string paramval = "";
                        pos++;
                        //gets constant into SqlParam, naming it @parX
                        while (pos < len) {
                            if (filter[pos] != '\'') {
                                paramval += filter[pos++];
                                continue;
                            }
                            if (pos == len - 1) break;
                            if (filter[pos + 1] == '\'') {
                                paramval += "'";
                                pos += 2;
                                continue;
                            }
                            break;
                        }
                        pos++; //pos points to character after constant

                        //creates and adds new parameter
                        SqlParameter Par = new SqlParameter(paramname, paramval);
                        if (FieldDesc != null) {
                            Par.SqlDbType = type;
                            if (FieldDesc["col_len"] != DBNull.Value)
                                Par.Size = (int)FieldDesc["col_len"];
                            if (FieldDesc["col_precision"] != DBNull.Value) {
                                Par.Precision = Convert.ToByte(FieldDesc["col_precision"]);
                            }
                            if (FieldDesc["col_scale"] != DBNull.Value) {
                                Par.Scale = Convert.ToByte(FieldDesc["col_scale"]);
                            }
                            switch (Par.SqlDbType) {
                                case SqlDbType.Decimal:
                                    Par.Value = AdjustCurrencyDecimalSeparator(paramval);
                                    break;
                                case SqlDbType.Float:
                                    Par.Value = AdjustNumberDecimalSeparator(paramval);
                                    break;
                                case SqlDbType.Money:
                                    Par.Value = AdjustCurrencyDecimalSeparator(paramval);
                                    break;
                                case SqlDbType.Real:
                                    Par.Value = AdjustNumberDecimalSeparator(paramval);
                                    break;
                                case SqlDbType.SmallMoney:
                                    Par.Value = AdjustCurrencyDecimalSeparator(paramval);
                                    break;
                                default:
                                    break;
                            }
                        }

                        Cmd.Parameters.Add(Par);
                        if (prevwasidentifier) compiled += ' ';
                        compiled += paramname;
                        prevwasidentifier = true;
                        continue;
                    }

                    if (IsIdentifier(Curr)) {
                        if (prevwasidentifier) compiled += ' ';
                        compiled += Curr;
                        pos++;
                        while (IsIdentifier(filter[pos])) compiled += filter[pos++];
                        prevwasidentifier = true;
                        continue;
                    }
                    if (Curr == ' ') {
                        pos++;
                        continue;
                    }
                    //operator character
                    compiled += Curr;
                    prevwasidentifier = false;
                    pos++;
                    if (Curr == ')') {
                        level--;
                        continue;
                    }
                    if (Curr == '(') {
                        level++;
                        continue;
                    }
                }//end of ( .... ) clause

                //pos points after closed parenthesis
            }

        }

        string AdjustCurrencyDecimalSeparator(string S) {
            //string dec  = System.Globalization.NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
            string dec = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            return S.Replace(".", dec);
        }

        string AdjustNumberDecimalSeparator(string S) {
            string dec = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            return S.Replace(".", dec);
        }

        /// <summary>
        /// Adds where clauses to Cmd, using variables to store constants found in
        ///  filter. DateTime values should be like {ts "yyyy:mm:dd hh:mm:ss:mmmm"}
        /// </summary>
        /// <param name="Cmd"></param>
        /// <param name="filter"></param>
        /// <param name="tablename"></param>
        public void AddWhereClauses(ref SqlCommand Cmd, string filter, string tablename) { //MUST BECOME PROTECTED
            int handle = metaprofiler.StartTimer("AddWhereClauses(...)");
            dbstructure DBS = GetStructure(tablename);

            //			if ((tablename=="customobject")||(tablename=="columntypes")){
            //				Cmd.CommandText+=filter;
            //				return;
            //			}

            int pos = 0;
            int paramsfound = 0;         //n. of parameters found
            string compiled = "";           //output string to add to Cmd
            int len = filter.Length;        //to avoid recalculations

            CompileConditionInParenthesis(ref Cmd, filter, len, 0,
                ref pos, ref compiled, ref paramsfound, DBS.columntypes);

            //removes "{ts @parxxx}"
            int tspos = compiled.IndexOf("{ts @par");
            while (tspos != -1) {
                int endpar = tspos + 8;
                while (compiled[endpar] != '}') endpar++;
                int startpar = tspos + 4;
                string par = compiled.Substring(startpar, endpar - startpar);
                string oldpar = compiled.Substring(tspos, endpar - tspos + 1);
                compiled = compiled.Replace(oldpar, par);
                tspos = compiled.IndexOf("{ts @par");
            }

            tspos = compiled.IndexOf("{d @par");
            while (tspos != -1) {
                int endpar = tspos + 8;
                while (compiled[endpar] != '}') endpar++;
                int startpar = tspos + 3;
                string par = compiled.Substring(startpar, endpar - startpar);
                string oldpar = compiled.Substring(tspos, endpar - tspos + 1);
                compiled = compiled.Replace(oldpar, par);
                tspos = compiled.IndexOf("{d @par");
            }
            Cmd.CommandText += compiled;
            metaprofiler.StopTimer(handle);
            return;
        }

        #endregion

        /// <summary>
        /// Empty table structure information about a listing type of a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="listtype"></param>
        public virtual void ResetListType(string tablename, string listtype) {
            dbstructure DBS = GetStructure(tablename);
            Hashtable Read = (Hashtable)DBS.customview.ExtendedProperties["AlreadyRead"];
            if (Read == null) return;
            Read[listtype] = null;

        }

        /// <summary>
        /// Empty table structure information about any listing type of a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="listtype"></param>
        public virtual void ResetAllListType(string tablename, string listtype) {
            dbstructure DBS = GetStructure(tablename);
            DBS.customview.ExtendedProperties["AlreadyRead"] = null;
        }

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
        public virtual string GetListType(out dbstructure DBS, string tablename, string listtype) {
            //int handle = metaprofiler.StartTimer("GetListType("+tablename+")");
            int handle = metaprofiler.StartTimer("GetListType*"+tablename);
            DBS = GetStructure(tablename);
            if (listtype == null) return listtype;
            string DBfilter = QHS.MCmp(new {objectname = tablename, viewname = listtype});
            string CFilter = QHC.MCmp(new {objectname = tablename, viewname = listtype});
                //"(objectname=" + QueryCreator.quotedstrvalue(tablename, true) +")AND(viewname=" + QueryCreator.quotedstrvalue(listtype, true) + ")";

            Hashtable Read = (Hashtable)DBS.customview.ExtendedProperties["AlreadyRead"];
            if (Read == null) {
                Read = new Hashtable();
                DBS.customview.ExtendedProperties["AlreadyRead"] = Read;
            }
            if (Read[listtype] != null) {
                metaprofiler.StopTimer(handle);
                return listtype;
            }

            if (DBS.customview.Select(CFilter).Length == 0) {
                if (IsToRead(DBS, "customview")) RUN_SELECT_INTO_TABLE(DBS.customview, null, DBfilter, null, true);
                if (IsToRead(DBS, "customredirect")) RUN_SELECT_INTO_TABLE(DBS.customredirect, null, DBfilter, null, true);
            }
            DataRow[] found = DBS.customredirect.Select(CFilter);
            if (found.Length > 0) {
                var R = found[0];
                string viewtable = R["objecttarget"].ToString();
                GetListType(out DBS, viewtable, R["viewtarget"].ToString());
                metaprofiler.StopTimer(handle);
                return R["viewtarget"].ToString();
            }

            foreach (DataRow R in DBS.customviewwhere.Select(CFilter)) {
                R.Delete();
                R.AcceptChanges();
            }
            foreach (DataRow R in DBS.customvieworderby.Select(CFilter)) {
                R.Delete();
                R.AcceptChanges();
            }
            foreach (DataRow R in DBS.customviewcolumn.Select(CFilter)) {
                R.Delete();
                R.AcceptChanges();
            }

            if (IsToRead(DBS, "customviewcolumn")) RUN_SELECT_INTO_TABLE(DBS.customviewcolumn, null, DBfilter, null, true);
            if (IsToRead(DBS, "customvieworderby")) RUN_SELECT_INTO_TABLE(DBS.customvieworderby, null, DBfilter, null, true);
            if (IsToRead(DBS, "customviewwhere")) RUN_SELECT_INTO_TABLE(DBS.customviewwhere, null, DBfilter, null, true);
            
            DataRow[] List = DBS.customview.Select(DBfilter);
            if (List.Length == 0)
                Read[listtype] = "1";
            if ((List.Length > 0) && (List[0]["issystem"].ToString().ToUpper() == "S")) {
                Read[listtype] = "1";
            }
            metaprofiler.StopTimer(handle);
            return listtype;
        }

        //		public void GetTableInfo(string objectname){
        //			dbstructure DBS = GetStructure(objectname);
        //			if (DBS.customtablestructure.Rows.Count>0) return;
        //			string filtertab="(objectname='"+objectname+"')";			
        //			RUN_SELECT_INTO_TABLE(DBS.customtablestructure, null, filtertab, null,true);
        //		}

        /// <summary>
        /// Get information about an edit type. Reads from customedit 
        /// </summary>
        /// <param name="objectname"></param>
        /// <param name="edittype"></param>
        /// <returns>CustomEdit DataRow about an edit-type</returns>
        public virtual DataRow GetFormInfo(string objectname, string edittype) {
            dbstructure DBS = GetStructure(objectname);
            string filter = QHS.MCmp(new { objectname, edittype }); //"(objectname='" + objectname + "')AND(edittype='" + edittype + "')";
            string dtfilter = QHC.MCmp(new {edittype}); //"(edittype='" + edittype + "')"};
            DataRow[] found = DBS.customedit.Select(dtfilter);
            if (found.Length > 0) return found[0];
            if (IsToRead(DBS, "customedit")) RUN_SELECT_INTO_TABLE(DBS.customedit, null, filter, null, true);
            found = DBS.customedit.Select(dtfilter);
            if (found.Length == 0) return null;
            return found[0];
        }

        /// <summary>
        /// Gets the system type name of a field named fieldname
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public virtual string GetFieldSystemTypeName(dbstructure DBS, string fieldname) {//MUST BECOME PROTECTED
            string filter = QHC.CmpEq("field",fieldname);
            DataRow[] found = DBS.columntypes.Select(filter);
            if (found.Length == 0) return null;
            return found[0]["systemtype"].ToString();
        }

        /// <summary>
        /// Gets the corresponding system type of a db column named fieldname
        /// </summary>
        /// <param name="DBS"></param>
        /// <param name="fieldname"></param>
        /// <returns></returns>
        public virtual Type GetFieldSystemType(dbstructure DBS, string fieldname) {//MUST BECOME PROTECTED
            string name = GetFieldSystemTypeName(DBS, fieldname);
            if (name == null) return null;
            return GetType_Util.GetSystemType_From_StringSystemType(name);
        }



        //			/// <summary>
        //			/// Marks an event 
        //			/// </summary>
        //			/// <param name="e"></param>
        //			public void MarkEvent(string e){			
        //				myLastError= QueryCreator.GetPrintable(e);
        //				string msg = "At "+QueryCreator.unquotedstrvalue(DateTime.Now,true)+":";
        //				Debug.Write(e+"\r",msg);
        //			}


        /// <summary>
        /// Marks an Exception and set Last Error
        /// </summary>
        /// <param name="main">Main description</param>
        /// <param name="E"></param>
        public virtual string MarkException(string main, Exception E) {            
            myLastError = errorLogger.formatException(E);
            errorLogger.markException(E, main);
            return myLastError;
        }


        /// <summary>
        /// Class for logging errors
        /// </summary>
        public IErrorLogger errorLogger { get; set; } = ErrorLogger.Logger;

        #region Transaction Management

        int NTRANS = 0;
        bool DoppiaRollBack = false;
        

        /// <summary>
        /// Gets Current used Transaction
        /// </summary>
        /// <returns>null if no transaction is open</returns>
        public virtual SqlTransaction CurrTransaction() {
            if (mainConnection != null) return mainConnection.CurrTransaction();
            return (SqlTransaction)Security.GetSys("Transaction");
        }

        /// <summary>
        /// Starts a new transaction 
        /// </summary>
        /// <param name="L"></param>
        /// <returns>error message, or null if OK</returns>
        public virtual string BeginTransaction(IsolationLevel L) {
            if (mainConnection != null) return mainConnection.BeginTransaction(L);
            //if (sys["Transaction"]!=null){
            //    return "Impossibile accedere alla connessione. C'è già un altra transazione in corso.";
            //}
            if (NTRANS > 0) {
                //NTRANS=NTRANS+1;
                return "ERRORE BEGIN TRANSACTION DI TRANSAZIONE ANNIDATA"; //va ad aggiungersi alla transaz. corrente
            }
            try {
                SqlTransaction Tran;
                DO_SYS_CMD("set XACT_ABORT ON", true);
                Tran = MySqlConnection.BeginTransaction(L);
                DoppiaRollBack = false;
                UpdateSysTransaction(Tran);
                NTRANS = 1;
            }
            catch (Exception e) {
                return QueryCreator.GetErrorString(e);
            }
            return null;

        }
        void UpdateSysTransaction(SqlTransaction Trans) {
            Security.SetSys("Transaction", Trans);

        }
        /// <summary>
        /// Commit the transaction
        /// </summary>
        /// <returns>error message, or null if OK</returns>
        public virtual string Commit() {
            if (mainConnection != null) return mainConnection.Commit();

            var Tran = CurrTransaction();
            if (Tran.Connection == null) {
                return "La transazione corrente non è più valida";
            }

            if (NTRANS == 0) {
                string err = "ERRORE COMMIT NON PRECEDUTO DA BEGIN TRANSACTION";
                errorLogger.logException(err,dataAccess:this);                
                return err;
            }
            if (DoppiaRollBack) {
                string err = "Errore, due transazioni annidate, della prima è stato fatto già il rollback";
                errorLogger.logException(err, dataAccess: this);
                RollBack();
                return err;
            }

            try {
                Tran.Commit();
                Tran.Dispose();
                NTRANS = 0;
                UpdateSysTransaction(null);
            }
            catch (Exception e) {
                return QueryCreator.GetErrorString(e);
            }
            return null;
        }

        /// <summary>
        /// Rollbacks transaction
        /// </summary>
        /// <returns>Error message, or null if OK</returns>
        public virtual string RollBack() {
            if (mainConnection != null) return mainConnection.RollBack();
            var Tran = CurrTransaction();
            if (Tran == null) {
                DoppiaRollBack = true;
                string err = "RollBack senza una transazione attiva";
                errorLogger.logException(err, dataAccess: this);
                return err;
            }
            if (NTRANS == 0) {
                DoppiaRollBack = true;
                string err = "RollBack NON PRECEDUTO DA BEGIN TRANSACTION";
                errorLogger.logException(err, dataAccess: this);
                return err;
            }
            try {
                if (Tran.Connection != null) {//normale a volte, se si usa xact_abort
                    Tran.Rollback();
                }
                Tran.Dispose();
                NTRANS -= 1;
                if (NTRANS == 0) UpdateSysTransaction(null);
            }
            catch (Exception e) {
                string err = $"FALLIMENTO DI UN ROLLBACK:\r\n{QueryCreator.GetErrorString(e)}";
                errorLogger.logException(err, dataAccess: this);
                return err;
            }
            return null;


        }

        #endregion

        /// <summary>
        /// True if current transaction  is still alive, i.e. has a connection attached to it
        /// </summary>
        /// <returns></returns>
        public virtual bool validTransaction() {
            SqlTransaction s = CurrTransaction();
            return s?.Connection != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public virtual string GetCentralizedTableName(string table) {
            if (TableIsCentralized(table)) return "[DBO]." + table;
            return table;
        }

        #region Primitives for DB-interface

        /// <summary>
        ///   Read a set of fields from a table  and return a dictionary fieldName -&gt; value assuming that
        ///    the table contains only one row
        /// </summary>
        /// <param name="table"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> readObject(string table, string expr="*") {
            return readObject(table, (string) null, expr);
        }

        /// <summary>
        ///  Read a set of fields from a table  and return a dictionary fieldName -&gt; value
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object> readObject(string table, MetaExpression filter, string expr) {
            return readObject(table, filter?.toSql(QHS, this), expr);
        }

        /// <summary>
        /// Read a set of fields from a table  and return a dictionary fieldName -&gt; value
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="expr">list of fields to read</param>
        /// <returns>An object dictionary</returns>
        public virtual Dictionary<string, object> readObject(string table, string condition, string expr) {
            if (openError) return null;
            string cmd = null;
            int NN = 0;
            var res = new Dictionary<string, object>();
            try {
                NN = metaprofiler.StartTimer("DO_READ_VALUE*" + table);
                cmd = $"SELECT {expr} FROM {GetCentralizedTableName(table)}";
                if ((condition != null) && (condition != "")) cmd += " WHERE " + condition;
                Open();
                if (openError) {
                    metaprofiler.StopTimer(NN);
                    return null;
                }
                var Cmd = new SqlCommand(cmd, MySqlConnection, CurrTransaction());
                var Read = Cmd.ExecuteReader(CommandBehavior.SingleRow);
                var fieldNames= new string[Read.FieldCount];
                for (int i = 0; i < Read.FieldCount; i++) {
                    fieldNames[i] = Read.GetName(i);
                }
                try {
                    if (Read.HasRows) {
                        Read.Read();
                        for (int i = 0; i < Read.FieldCount; i++) {
                            res.Add(fieldNames[i], Read[i]);
                        }
                        SetLastRead();
                    }
                    else {
                        Read.Close();
                        Close();
                        metaprofiler.StopTimer(NN);
                        return null;
                    }
                }
                catch (Exception E) {
                    MarkException("DO_READ_VALUE: Error running " + cmd, E);
                    Read.Close();
                    Close();
                    metaprofiler.StopTimer(NN);
                    return null;
                }
                Cmd.Dispose();
                Read.Close();
                Close();
                metaprofiler.StopTimer(NN);
                return res;
            }
            catch (Exception E) {
                MarkException("DO_READ_VALUE: Error running " + cmd, E);
                Close();
                metaprofiler.StopTimer(NN);
                return null;
            }
        }

        /// <summary>
        /// Returns a single value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="expr"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public virtual object readValue(string table, MetaExpression filter, string expr, string orderby=null) {
            return DO_READ_VALUE(table, filter?.toSql(QHS, this), expr, orderby);
        }

        /// <summary>
        /// Returns a single value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="expr"></param>
        /// <param name="orderby"></param>
        /// <returns></returns>
        public virtual object DO_READ_VALUE(string table, string condition, string expr, string orderby) {
            if (openError) return null;
            string cmd = null;
            int NN = 0;
            try {
                NN = metaprofiler.StartTimer("DO_READ_VALUE*" + table);
                cmd = $"SELECT {expr} FROM {GetCentralizedTableName(table)}";
                if ((condition != null) && (condition != "")) cmd += " WHERE " + condition;
                if ((orderby != null) && (orderby.ToUpper() != "")) cmd += " ORDER BY " + orderby;
                Open();
                if (openError) {
                    metaprofiler.StopTimer(NN);
                    return null;
                }
                var Cmd = new SqlCommand(cmd, MySqlConnection, CurrTransaction());
                var Read = Cmd.ExecuteReader(CommandBehavior.SingleRow);
                object Result = null;
                try {
                    if (Read.HasRows) {
                        Read.Read();
                        Result = Read[0];
                        SetLastRead();
                    }
                }
                catch {
                }
                Read.Close();
                Close();
                metaprofiler.StopTimer(NN);
                return Result;
            }
            catch (Exception E) {
                MarkException("DO_READ_VALUE: Error running " + cmd, E);
                Close();
                metaprofiler.StopTimer(NN);
                return null;
            }
        }

        /// <summary>
        /// Returns a value executing a SELECT expr FROM table WHERE condition. If no row is found, NULL is returned 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        public virtual object DO_READ_VALUE(string table, string condition, string expr) {
            return DO_READ_VALUE(table, condition, expr, null);
        }


        /// <summary>
        /// Returns a value executing a generic sql command 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual object DO_SYS_CMD(string cmd) {
            return DO_SYS_CMD(cmd, true);
        }

        /// <summary>
        /// Returns a value executing a generic sql command 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ErrMsg">eventual error message</param>
        /// <returns></returns>
		public virtual object DO_SYS_CMD(string cmd, out string ErrMsg) {
            ErrMsg = null;
            if (openError) return null;
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = LM.noValidTransaction;
                    return null;
                }
            }

            int NN = metaprofiler.StartTimer("DO_SYS_CMD()"); ;
            try {
                Open();
                if (openError) {
                    ErrMsg = LM.errorOpeningConnection;
                    metaprofiler.StopTimer(NN);
                    return null;
                }


                var Cmd = new SqlCommand(cmd, MySqlConnection, currTran) {CommandTimeout = 600};
                var Read = Cmd.ExecuteReader(CommandBehavior.SingleRow);
                object Result = null;
                try {
                    if (Read.HasRows) {
                        Read.Read();
                        Result = Read[0];
                        SetLastRead();

                    }
                }
                catch (Exception E) {
                    ErrMsg = MarkException("DO_SYSCMD: Error running " + cmd, E);
                    errorLogger.logException("DO_SYSCMD: Error running " + cmd,E,dataAccess:this);
                }
                Read.Close();
                Close();
                Cmd.Dispose();
                metaprofiler.StopTimer(NN);

                if (currTran != null) {
                    if (currTran.Connection == null) {
                        ErrMsg = LM.cmdInvalidatedTransaction(cmd);// "Il comando " + cmd + " ha invalidato la transazione";
                        return null;
                    }
                }
                return Result;
            }
            catch (Exception E) {
                Close();
                ErrMsg = MarkException("Error running " + cmd, E);
                errorLogger.logException("DO_SYSCMD: Error running " + cmd,E,dataAccess:this);
                metaprofiler.StopTimer(NN);
                return null;
            }

        }



        /// <summary>
        /// Reads all value from a generic sql command and returns the last value read
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public virtual object DO_SYS_CMD_LASTRESULT(string cmd, out string ErrMsg) {
            ErrMsg = null;
            if (openError) return null;
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = LM.noValidTransaction;
                    return null;
                }
            }
            int NN = metaprofiler.StartTimer("DO_SYS_CMD_LASTRESULT()");
            var sbRes = new StringBuilder();
            int nRes = 0;
            try {
                Open();
                if (openError) {
                    metaprofiler.StopTimer(NN);
                    ErrMsg = LM.errOpeningDuringSave;
                    return null;
                }
                var Cmd = new SqlCommand(cmd, MySqlConnection, currTran) {
                    CommandTimeout = 600
                };
                var Read = Cmd.ExecuteReader();
                object Result = null;
                bool second = false;

                try {

                    while (Read.HasRows) {
                        while (Read.Read()) {
                            nRes++;
                            if (second) ErrorLogger.Logger.markEvent("dirty found on" + cmd);
                            Result = Read[0];
                            sbRes.AppendLine($"{nRes}:{Result}");
                            second = true;
                            SetLastRead();
                        }
                        Read.NextResult();
                    }


                }
                catch (Exception E) {
                    ErrMsg = MarkException(LM.doSysCmdError(cmd,sbRes.ToString()), E);
                }
                Read.Close();
                Read.Dispose();
                Close();
				Cmd.Dispose();

                if (currTran != null) {
                    if (currTran.Connection == null) {
                        ErrMsg = LM.cmdInvalidatedTransaction(cmd);
                        return null;
                    }
                }
                metaprofiler.StopTimer(NN);
                return Result;
            }
            catch (Exception E) {
                Close();
                ErrMsg = MarkException(LM.doSysCmdError(cmd,sbRes.ToString()), E);
                metaprofiler.StopTimer(NN);
                return null;
            }

        }




        /// <summary>
        /// Runs a sql command that returns a single value
        /// </summary>
        /// <param name="cmd">command to run</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <returns></returns>
        public virtual object DO_SYS_CMD(string cmd,bool silent) {
            object O = DO_SYS_CMD(cmd, out string errmess);
            if (LocalToDB && (errmess != null) && (!silent)) shower?.ShowError(null, LM.errorRunningCommand(cmd), errmess);
            return O;
        }

        /// <summary>
        /// Runs a sql command that returns a single value
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="cmd">command to run</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <returns></returns>
		public static object DO_SYS_CMD(DataAccess Conn, string cmd, IMessageShower shower=null) {
            object O = Conn.DO_SYS_CMD(cmd, out string errmess);
            if ((errmess != null) && (shower != null)) shower.ShowError(null, LM.errorRunningCommand(cmd), errmess);
            return O;
        }


    
       

    

       

        /// <summary>
        /// Get a list of "objects" from a table using  a specified query, every object is encapsulated in a dictionary
        /// </summary>
        /// <param name="query">sql command to run</param>
        /// <param name="timeout"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public virtual Dictionary<string, object>[] readObjectArray(string query, int timeout, out string ErrMsg) {
            ErrMsg = null;
            if (query == null) return null;
            //command = MyCompile(command);
            if (openError) {
                ErrMsg = LM.errorOpeningConnection;
                return null;
            }
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = LM.noValidTransaction;
                    return null;
                }
            }

            using (var cmd = new SqlCommand(query, MySqlConnection, currTran)) {
	            if (timeout != -1) cmd.CommandTimeout = timeout;

	            int NN = 0;
	            var resList = new List<Dictionary<string, object>>();
	            try {
		            Open();
		            if (openError) {
			            metaprofiler.StopTimer(NN);
			            return null;
		            }

		            var Read = cmd.ExecuteReader();
		            string[] fieldNames = new string[Read.FieldCount];
		            for (int i = 0; i < Read.FieldCount; i++) {
			            fieldNames[i] = Read.GetName(i);
		            }

		            try {
			            if (Read.HasRows) {
				            while (Read.Read()) {
					            var curr = new Dictionary<string, object>();
					            for (int i = 0; i < Read.FieldCount; i++) {
						            curr.Add(fieldNames[i], Read[i]);
					            }

					            resList.Add(curr);
					            SetLastRead();
				            }
			            }
		            }
		            catch (Exception e) {
			            MarkException(LM.readObjArrayError(query), e);
			            ErrMsg = e.ToString();
		            }

		            Read.Close();
		            Close();
		            metaprofiler.StopTimer(NN);

		            if (currTran != null) {
			            if (currTran.Connection == null) {
				            ErrMsg = LM.cmdInvalidatedTransaction(query);
				            return null;
			            }
		            }

		            return resList.ToArray();
	            }
	            catch (Exception E) {
		            MarkException(LM.readObjArrayError(query), E);
		            Close();
		            metaprofiler.StopTimer(NN);
		            return null;
	            }
            }
        }

        public virtual DataTable SQLRunner(string command, bool silent=false, int timeout = -1) {
            DataTable T = SQLRunner(command,  out string errmsg,timeout);
            if (LocalToDB && (errmsg != null) && (!silent)) {
                shower.ShowError(null, LM.errorRunningCommand(command), errmsg);
            }
            return T;
        }
        /// <summary>
        /// Runs a sql command that return a DataTable
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMsg">Error message or null when no errors</param>
        /// <returns></returns>
        public virtual DataTable SQLRunner(string command, out string ErrMsg, int timeout=-1) {
            ErrMsg = null;
            if (command == null) return null;
            //command = MyCompile(command);
            if (openError) {
                ErrMsg =LM.errorOpeningConnection;
                return null;
            }
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = LM.noValidTransaction;
                    return null;
                }
            }
            var cmd = new SqlCommand(command, MySqlConnection, currTran);
            if (timeout != -1) cmd.CommandTimeout = timeout;

            using (var MyDataAdapter = new SqlDataAdapter(cmd))
            using (var T = new DataTable()) {

	            int handle = metaprofiler.StartTimer("SQLRUNNER()");
	            try {
		            Open();
		            if (openError) {
			            metaprofiler.StopTimer(handle);
			            ErrMsg = LM.errorOpeningConnection;
			            return null;
		            }

		            MyDataAdapter.Fill(T);
		            Close();
		            MyDataAdapter.Dispose();
		            SetLastRead();
	            }
	            catch (Exception E) {
		            //if (command.Length>80000) command = command.Substring(0,79997)+"...";
		            ErrMsg = MarkException($"SQLRunner:{LM.errorRunningCommand(command)}", E);
		            errorLogger.logException($"SQLRunner:{LM.errorRunningCommand(command)}", E, dataAccess: this);
		            Close();
		            metaprofiler.StopTimer(handle);
		            return null;
	            }

	            metaprofiler.StopTimer(handle);
	            if (currTran != null) {
		            if (currTran.Connection == null) {
			            ErrMsg = LM.cmdInvalidatedTransaction(command);
			            return null;
		            }
	            }

	            return T;
            }
        }

       
        


        /// <summary>
        /// Builds a sql DELETE command 
        /// </summary>
        /// <param name="table">table implied</param>
        /// <param name="condition">condition for the deletion</param>
        /// <returns></returns>
        public virtual string GetDeleteCommand(string table, string condition) {
            string DeleteCmd = $"DELETE FROM {GetCentralizedTableName(table)}";
            if ((condition != null) && (condition.Trim() != "")) DeleteCmd += " WHERE " + condition;
            return DeleteCmd;
        }


        /// <summary>
        /// Executes a delete command using current transaction
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <returns>Error message or null if OK</returns>
        public virtual string DO_DELETE(string table, string condition) {
            if (condition == null) {              
                throw new Exception($"DO_DELETE without condition on table {table}");
            }

            string DeleteCmd = GetDeleteCommand(table, condition);
            SqlTransaction currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    return LM.noValidTransaction;
                }
            }
            SqlCommand Cmd = new SqlCommand(DeleteCmd, MySqlConnection, currTran);

            int count;
            try {
                count = Cmd.ExecuteNonQuery();
            }
            catch (Exception e) {
                //LastError= e.Message+ " running command "+DeleteCmd;
                return e.ToString() + " running command " + DeleteCmd;
            }
            if (count == 0) {
                return $"There was no row in table {table} to delete with condition {condition}";
            }
            SetLastWrite();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    return LM.cmdInvalidatedTransaction(DeleteCmd);
                }
            }
            return null;
        }

        string CreateList(string[] values, int len) {
            string result = "";
            for (int i = 0; i < len; i++) {
                if (i > 0) result += ",";
                result += values[i];
            }
            return result;
        }

        /// <summary>
        /// Builds a sql INSERT command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns">column names</param>
        /// <param name="values">values to insert</param>
        /// <param name="len">number of columns</param>
        /// <returns></returns>
        public virtual string getInsertCommand(string table, string[] columns, string[] values, int len) {
            return $"INSERT INTO {GetCentralizedTableName(table)}({CreateList(columns, len)}) VALUES ({CreateList(values, len)})";
        }


        /// <summary>
        /// Executes an INSERT command using current tranactin
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columns">column names</param>
        /// <param name="values">values to insert</param>
        /// <param name="len">number of columns</param>
        /// <returns>Error message or null if OK</returns>
        public virtual string DO_INSERT(string table, string[] columns, string[] values, int len) {

            string InsertCmd = getInsertCommand(table, columns, values, len);
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    return LM.noValidTransaction;
                }
            }

            using (var Cmd = new SqlCommand(InsertCmd, MySqlConnection, currTran) {CommandTimeout = 300}) {
	            int count;
	            try {
		            count = Cmd.ExecuteNonQuery();
	            }
	            catch (Exception e) {
		            return e.ToString() + " running command " + InsertCmd;
	            }

	            if (count > 0) {
		            SetLastWrite();
		            if (currTran != null) {
			            if (currTran.Connection == null) {
				            return $"Il comando {InsertCmd} ha invalidato la transazione";
			            }
		            }

		            return null;
	            }

	            return "Error running command " + InsertCmd;
            }
        }

        /// <summary>
        /// Builds an UPDATE sql command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
       	/// <param name="columns">column names</param>
        /// <param name="values">values to be set</param>
        /// <param name="ncol">number of columns to update</param>
        /// <returns>Error msg or null if OK</returns>
        public virtual string getUpdateCommand(string table, string condition,
            string[] columns, string[] values, int ncol) {
            string UpdateCmd = "UPDATE " + GetCentralizedTableName(table) + " SET ";
            string outstring = "";
            bool first = true;
            for (int i = 0; i < ncol; i++) {
                if (first)
                    first = false;
                else
                    outstring += ",";
                outstring += columns[i] + "=" + values[i];
            }
            UpdateCmd += outstring;
            if (condition != null && condition != "") UpdateCmd += " WHERE " + condition;
            return UpdateCmd;
        }


        /// <summary>
        /// Executes an UPDATE command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition">where condition to apply</param>
        /// <param name="columns">Name of columns to update</param>
        /// <param name="values">Values to set</param>
        /// <param name="ncol">N. of columns</param>
        /// <returns>Error msg or null if OK</returns>
        public virtual string DO_UPDATE(string table, string condition,
            string[] columns, string[] values, int ncol) {

            //Creates a update command            
            string UpdateCmd = getUpdateCommand(table, condition, columns, values, ncol);
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    return LM.noValidTransaction;
                }
            }

            using (var Cmd = new SqlCommand(UpdateCmd, MySqlConnection, currTran) {
	            CommandTimeout = 300
            }) {
	            int count;
	            try {
		            count = Cmd.ExecuteNonQuery();
	            }
	            catch (Exception e) {
		            return e.ToString() + "Error running command " + UpdateCmd;
	            }

	            if (count > 0) {
		            SetLastWrite();
		            //Cmd.Dispose();

		            if (currTran != null) {
			            if (currTran.Connection == null) {
				            return "Il comando " + UpdateCmd + " ha invalidato la transazione";
			            }
		            }

		            return null;
	            }

	            return "Error running command " + UpdateCmd;
            }
         
        }

        /// <summary>
        /// Calls a stored procedure with specified parameters
        /// </summary>
        /// <param name="procname">stored proc. name</param>
        /// <param name="list">Parameter list</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <returns></returns>
		virtual public DataSet CallSP(string procname, object[] list, bool silent) {
            return CallSP(procname, list, silent, -1);
        }


        /// <summary>
        /// Calls a stored procedure with specified parameters
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="procname">stored proc. name</param>
        /// <param name="list">Parameter list</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public static DataSet CallSP(DataAccess Conn,
            string procname, object[] list, out string error, int timeout=-1) {
            DataSet D = Conn.CallSP(procname, list, out error, timeout);
            //if ((ErrMess != null) && (!silent)) QueryCreator.ShowError(null, "Error calling stored procedure " + procname, ErrMess);
            return D;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="sql"></param>
        /// <param name="timeout">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public virtual string sqlRunnerintoDataSet(DataSet d, string sql, int timeout=-1) {
            SqlTransaction currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    return LM.noValidTransaction;
                }
            }
            using (var SPCall = new SqlCommand(sql, MySqlConnection, currTran)
	            {CommandTimeout = timeout != -1 ? timeout : 90}) {

	            int NN = metaprofiler.StartTimer("sqlRunnerDataSet*" + sql);
	            try {
		            var MyDA = new SqlDataAdapter(SPCall);
		            if (SPCall.Transaction != null) MyDA.SelectCommand.Transaction = SPCall.Transaction;
		            Open();
		            if (openError) {
			            metaprofiler.StopTimer(NN);
			            return "Errore aprendo la connessione";
		            }

		            MyDA.Fill(d);
                    MyDA.Dispose();
		            Close();
		            SetLastRead();
		            metaprofiler.StopTimer(NN);

		            if (currTran != null) {
			            if (currTran.Connection == null) {
				            return $"sqlRunnerDataSet {sql} ha invalidato la transazione";
			            }
		            }

		            return null;
	            }
	            catch (Exception E) {
		            Close();
		            metaprofiler.StopTimer(NN);
		            return MarkException("sqlRunnerDataSet: Error running " + sql, E);
	            }
            }
        }

        /// <summary>
        /// Execute a sql cmd that returns a dataset (eventually with more than one table in it)
        /// </summary>
        /// <param name="sql">sql command to run</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMess">null if ok, Error message otherwise</param>
        /// <returns></returns>
        virtual public DataSet sqlRunnerDataSet(string sql, int timeout, out string ErrMess) {
            var d = new DataSet();
            ErrMess = sqlRunnerintoDataSet(d, sql, timeout);
            if (ErrMess != null) return null;
            return d;
        }


        /// <summary>
        /// Calls a stored procedure with specified parameters
        /// </summary>
        /// <param name="procname">stored proc. name</param>
        /// <param name="list">Parameter list</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="ErrMess">null if ok, Error message otherwise</param>
        /// <returns></returns>
        virtual public DataSet CallSP(string procname, object[] list, out string ErrMess, int timeout=-1) {
            ErrMess = null;
            string cmd = procname + " ";
            if (ProcedureIsCentralized(procname) && !cmd.StartsWith("[DBO].")) cmd = "[DBO]." + cmd;

            bool first = true;
            for (int i = 0; i < list.Length; i++) {
                if (!first) cmd += ", ";
                first = false;
                cmd += mdl_utils.Quoting.quotedstrvalue(list[i], true);
            }

            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMess = LM.noValidTransaction;
                    return null;
                }
            }

            var SPCall = new SqlCommand(cmd, MySqlConnection, currTran) {
                CommandTimeout = timeout != -1 ? timeout : 90
            };
            //if (sys["Transaction"]!=null) SPCall.Transaction= (SqlTransaction)	sys["Transaction"];

            int NN = metaprofiler.StartTimer("CallSP " + procname);
            try {
                var MyDS = new DataSet();
                var MyDA = new SqlDataAdapter(SPCall);
                if (SPCall.Transaction != null) MyDA.SelectCommand.Transaction = SPCall.Transaction;
                Open();
                if (openError) {
                    ErrMess = "Errore aprendo la connessione";
                    SPCall.Dispose();
                    metaprofiler.StopTimer(NN);
                    return null;
                }

                MyDA.Fill(MyDS);
                SPCall.Dispose();
                MyDA.Dispose();
                Close();
                SetLastRead();
                metaprofiler.StopTimer(NN);

                if (currTran != null) {
                    if (currTran.Connection == null) {
                        ErrMess = $"La chiamata alla SP {procname}{paramString(list)} ha invalidato la transazione";
                        return null;
                    }
                }

                return MyDS;
            }
            catch (SqlException sqlE) {
                Close();
                ErrMess = MarkException($"CALLSP: Error calling stored procedure {procname}{paramString(list)}", sqlE);
                metaprofiler.StopTimer(NN);
                return null;
                //myLastError= E.Me
            }
            catch (Exception E) {
                Close();
                ErrMess = MarkException($"CALLSP: Error calling stored procedure {procname}{paramString(list)}", E);
                metaprofiler.StopTimer(NN);
                //myLastError= E.Message;
                return null;
            }

        }


        // CallSP(string procname, object[] list, int timeout, out string ErrMess) {

        /// <summary>
        /// Calls a stored procedure and reads output in a DataSet. First table can be retrieved in result.Tables[0]
        /// </summary>
        /// <param name="procname">name of stored procedure to call</param>
        /// <param name="list">parameters to give to the stored procedure</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns>null on errors, in which case also LastError is set</returns>
        virtual public DataSet CallSP(string procname, object[] list, bool silent, int timeout) {
            DataSet D = CallSP(procname, list, out string ErrMess, timeout);
            if (LocalToDB && (ErrMess != null) && (!silent)) shower?.ShowError(null, "Error calling stored procedure " + procname+paramString(list),
                                                                 ErrMess);
            return D;
        }


        /*

			//esempio di utilizzo, come vedi per i tipi intero puoi definire come lunghezza il valore 0 (viene ignorato)

	string[] ParamName= new string[4]{"@returnvalue","@codiceinventario","@numiniziale","@quantita"};

	SqlDbType[] tipi=new SqlDbType[4]{SqlDbType.Char, SqlDbType.VarChar, SqlDbType.Int,SqlDbType.Int};

	int[] len=new int[4]{1,10,0,0};

	ParameterDirection[] dir=new ParameterDirection[4]{ParameterDirection.Output,
		ParameterDirection.Input,ParameterDirection.Input,ParameterDirection.Input};
	
	object[] valori=new object[4]{null,codiceinventario,numiniziale,quantita};

	if (CallSPParameter("sp_calc_checkinventario",ParamName,tipi,len,dir,ref valori)) {
		//leggo il valore di output (primo parametro)
		string s=valori[0].ToString();
	}



		
			*/

        static string paramString(string[] ParamName, object[] ParamValues) {
            string p = "";
            for (int i = 0; i < ParamName.Length; i++) {
                if (i > 0) p += ",";
                p += ParamName[i] + "=";
                if (ParamValues[i] == null || ParamValues[i] == DBNull.Value) {
                    p += "(null)";
                }
                else {
                    p += ParamValues[i].ToString();
                }
            }

            return "("+p+")";
        }

        static string paramString( object[] ParamValues) {
            string p = "";
            for (int i = 0; i < ParamValues.Length; i++) {
                if (i > 0) p += ",";
                if (ParamValues[i] == null || ParamValues[i] == DBNull.Value) {
                    p += "(null)";
                }
                else {
                    p += ParamValues[i].ToString();
                }
            }

            return "("+p+")";
        }

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
        virtual public bool CallSPParameter(string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            int timeout, out string ErrMsg) {
            ErrMsg = null;
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = LM.noValidTransaction;
                    return false;
                }
            }
            
            var MyCommand = new SqlCommand(sp_name, MySqlConnection, currTran);
            if (timeout != -1) MyCommand.CommandTimeout = timeout;
            else MyCommand.CommandTimeout = 90;

            int NN = metaprofiler.StartTimer("CallSPParameter*" + sp_name);

            MyCommand.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < ParamName.Length; i++) {
                SqlParameter MyParam;
                if (ParamType[i] == SqlDbType.Decimal) {
                    MyParam = new SqlParameter(ParamName[i], ParamType[i], ParamTypeLength[i]) {
                        Precision = 23,
                        Scale = 6
                    };
                }
                else {
                    MyParam = new SqlParameter(ParamName[i], ParamType[i], ParamTypeLength[i]);
                }
                MyParam.Direction = ParamDirection[i];
                if (MyParam.Direction == ParameterDirection.Input ||
                    MyParam.Direction == ParameterDirection.InputOutput) MyParam.Value = ParamValues[i];
                MyCommand.Parameters.Add(MyParam);
            }

            try {
                Open();
                if (openError) {
                    ErrMsg = "CALLSPPARAMETER: la connessione col db è chiusa. SP:" + sp_name+paramString(ParamName,ParamValues);
                    metaprofiler.StopTimer(NN);
                    //myLastError= E.Message;
                    return false;
                }
                MyCommand.ExecuteNonQuery();
                Close();
                SetLastRead();
                metaprofiler.StopTimer(NN);
            }
            catch (Exception E) {
                Close();
                ErrMsg = MarkException("CALLSPPARAMETER: Error calling stored procedure " + sp_name+paramString(ParamName,ParamValues), E);
                metaprofiler.StopTimer(NN);
                //myLastError= E.Message;
                return false;
            }

            MyCommand.Dispose();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = "La chiamata alla SP " + sp_name+paramString(ParamName,ParamValues) + " ha invalidato la transazione";
                    return false;
                }
            }

            for (int i = 0; i < ParamName.Length; i++) {
                if (ParamDirection[i] == ParameterDirection.Output ||
                    ParamDirection[i] == ParameterDirection.InputOutput)
                    ParamValues[i] = MyCommand.Parameters[ParamName[i]].Value;
            }
            return true;

        }


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
        virtual public DataSet CallSPParameterDataSet(string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            int timeout, out string ErrMsg) {
            ErrMsg = null;
            SqlTransaction currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = LM.noValidTransaction;
                    return null;
                }
            }
            SqlCommand MyCommand = new SqlCommand(sp_name, MySqlConnection, currTran);
            if (timeout != -1) {
                MyCommand.CommandTimeout = timeout;
            }
            else {
                MyCommand.CommandTimeout = 90;
            }
            var MyDS = new DataSet();

            int NN = metaprofiler.StartTimer("CallSPParameterDataSet " + sp_name);

            MyCommand.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < ParamName.Length; i++) {
                SqlParameter MyParam;
                if (ParamType[i] == SqlDbType.Decimal) {
                    MyParam = new SqlParameter(ParamName[i], ParamType[i], ParamTypeLength[i]) {
                        Precision = 23,
                        Scale = 6
                    };
                }
                else {
                    MyParam = new SqlParameter(ParamName[i], ParamType[i], ParamTypeLength[i]);
                }

                MyParam.Direction = ParamDirection[i];
                if (MyParam.Direction == ParameterDirection.Input ||
                    MyParam.Direction == ParameterDirection.InputOutput) MyParam.Value = ParamValues[i];
                MyCommand.Parameters.Add(MyParam);
            }
            bool opened = false;
            try {
                var MyDA = new SqlDataAdapter(MyCommand);
                if (MyCommand.Transaction != null) MyDA.SelectCommand.Transaction = MyCommand.Transaction;
                Open();
                if (openError) {
                    ErrMsg = "Errore aprendo la connessione";
                    MyCommand.Dispose();
                    metaprofiler.StopTimer(NN);
                    return null;
                }
                opened = true;
                MyDA.Fill(MyDS);
                MyDA.Dispose();
                Close();
                SetLastRead();
            }
            catch (Exception E) {
                if (opened) Close();
                ErrMsg = MarkException("CALLSPPARAMETER: Error calling stored procedure " + sp_name+paramString(ParamName,ParamValues), E);
                //myLastError= E.Message;
                metaprofiler.StopTimer(NN);
                return null;
            }
            MyCommand.Dispose();
            metaprofiler.StopTimer(NN);
            if (currTran != null) {
                if (currTran.Connection == null) {
                    ErrMsg = "La chiamata alal SP " + sp_name+paramString(ParamName,ParamValues) + " ha invalidato la transazione";
                    return null;
                }
            }

            for (int i = 0; i < ParamName.Length; i++) {
                if (ParamDirection[i] == ParameterDirection.Output ||
                    ParamDirection[i] == ParameterDirection.InputOutput)
                    ParamValues[i] = MyCommand.Parameters[ParamName[i]].Value;
            }

            return MyDS;

        }




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
        public virtual bool CallSPParameter(string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            bool silent=false, int timeout=-1) {
            bool res = CallSPParameter(sp_name, ParamName, ParamType, ParamTypeLength, ParamDirection,
                ref ParamValues, timeout, out string ErrMsg);
            if (LocalToDB && (ErrMsg != null) && (!silent)) shower.ShowError(null, $"Error calling stored procedure {sp_name}", ErrMsg);
            return res;

        }

        /// <summary>
        /// Calls a stored procedure and returns a DataSet.  return true if ok
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="sp_name">name of stored procedure to call</param>
        /// <param name="ParamName">parameter names to give to the stored procedure</param>
        /// <param name="ParamType">parameter types to give to the stored procedure</param>
        /// <param name="ParamTypeLength">Length of parameters</param>
        /// <param name="ParamDirection">Type of parameters</param>
        /// <param name="ParamValues">Value for parameters</param>
        /// <param name="silent">when false a message box appears on errors</param>
        /// <param name="timeout">Timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
		public static bool CallSPParameter(DataAccess Conn, string sp_name, string[] ParamName, SqlDbType[] ParamType,
            int[] ParamTypeLength, ParameterDirection[] ParamDirection, ref object[] ParamValues,
            IMessageShower shower, int timeout=-1) {
            bool res = Conn.CallSPParameter(sp_name, ParamName, ParamType, ParamTypeLength, ParamDirection,
                ref ParamValues, timeout, out string ErrMsg);
            if ((ErrMsg != null) && (shower!=null)) shower.ShowError(null, $"Error calling stored procedure {sp_name}", ErrMsg);
            return res;
        }

        #endregion

        bool IsIdentifier(char C) {
            if (char.IsLetterOrDigit(C)) return true;
            if (C == '@') return true;
            if (C == '_') return true;
            return false;
        }


        //static string MergeFilters(string Filter1, DataTable T) {
        //    string Filter2 = null;
        //    if (T.ExtendedProperties["filter"] != null) {
        //        Filter2 = T.ExtendedProperties["filter"].ToString();
        //    }
        //    return MergeFilters(Filter1, Filter2);
        //}

        //static string MergeFilters(string Filter1, string Filter2) {
        //    if (Filter1 == "") return Filter2;
        //    if (Filter1 == null) return Filter2;
        //    if (Filter2 == null) return Filter1;
        //    if (Filter2 == "") return Filter1;
        //    return Filter1 + "AND" + Filter2;
        //}


        #region SELECT COMMANDS



        /// <summary>
        /// Set the table from which T will be read. I.e. T is a virtual ALIAS for tablename.
        /// </summary>
        /// <param name="T">Table to set as Alias</param>
        /// <param name="tablename">Real table name</param>
        public static void SetTableForReading(DataTable T, string tablename) {
            T.ExtendedProperties["TableForReading"] = tablename;
        }

        /// <summary>
        /// Gets the "unaliased" name of T, i.e. the table to use for reading into T
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static string GetTableForReading(DataTable T) {
            if (T == null) return null;
            if (T.ExtendedProperties["TableForReading"] == null) return T.TableName;
            return T.ExtendedProperties["TableForReading"].ToString();
        }
        /// <summary>
        /// Only copy columns without any costraints, key or other ilarious things
        /// </summary>
        /// <param name="T"></param>
        /// <param name="copykey">if true primary key is copied</param>
        /// <returns></returns>
        public static DataTable SimplifiedTableClone(DataTable T, bool copykey = false) {
            DataTable T2 = new DataTable(T.TableName) {
                Namespace = T.Namespace
            };
            for (int i = 0; i < T.Columns.Count; i++) {
                DataColumn C = T.Columns[i];
                DataColumn C2 = new DataColumn(C.ColumnName, C.DataType, C.Expression) {
                    AllowDBNull = true
                };
                T2.Columns.Add(C2);

            }
            if (copykey) {
                DataColumn[] key = T.PrimaryKey;
                DataColumn[] key2 = new DataColumn[key.Length];
                for (int i = 0; i < key.Length; i++) {
                    key2[i] = T2.Columns[key[i].ColumnName];
                }
                T2.PrimaryKey = key2;
            }
            return T2;
        }

        /// <summary>
        /// Returns the copy of a single DataTable. This is quicker than .Clone(), especially if copyProperties is false
        /// DataTable properties are always copyed.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="copyProperties">true if ext.properties should be copyed</param>
        /// <returns></returns>
        public static DataTable SingleTableClone(DataTable T, bool copyProperties) {
            //#if DEBUG
            int handle = metaprofiler.StartTimer("SingleTableClone");
            //#endif
            DataTable T2 = new DataTable(T.TableName) {
                Namespace = T.Namespace
            };
            foreach (DataColumn C in T.Columns) {
                DataColumn C2 = new DataColumn(C.ColumnName, C.DataType, C.Expression) {
                    AllowDBNull = C.AllowDBNull
                };
                T2.Columns.Add(C2);
            }
            DataColumn[] key = T.PrimaryKey;
            DataColumn[] key2 = new DataColumn[key.Length];
            for (int i = 0; i < key.Length; i++) {
                key2[i] = T2.Columns[key[i].ColumnName];
            }
            T2.PrimaryKey = key2;
            if (!copyProperties) {
                //#if DEBUG
                metaprofiler.StopTimer(handle);
                //#endif
                return T2;
            }
            //DataTable properties are always copyed
            foreach (object kt in T.ExtendedProperties.Keys) T2.ExtendedProperties[kt] = T.ExtendedProperties[kt];

            foreach (DataColumn C in T.Columns) {
                DataColumn C2 = T2.Columns[C.ColumnName];
                foreach (object kc in C.ExtendedProperties.Keys)
                    C2.ExtendedProperties[kc] = C.ExtendedProperties[kc];
            }
            //#if DEBUG
            metaprofiler.StopTimer(handle);
            //#endif
            return T2;
        }


        /// <summary>
        /// Get the condition the rows in a list must satisfy
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="SQL"></param>
        /// <returns></returns>
        public string SelectCondition(string tablename, bool SQL) {
            return Security.SelectCondition(tablename, SQL);
        }

        /// <summary>
        /// Reads data into an existing table
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="T">Table into which data will be read</param>
        /// <param name="sortBy">sorting for db reading</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="top"></param>
        /// <param name="prepare">leave this to false</param>
        public static void RUN_SELECT_INTO_TABLE(DataAccess conn, DataTable T, string sortBy,
            string filter, string top, bool prepare) {
            if (conn.openError) return;
            int handle = metaprofiler.StartTimer("static RUN_SELECT_INTO_TABLE*" + T.TableName );
            string columlist = QueryCreator.ColumnNameList(T);
            try {
                string filtersec = filter;
                if ( //T.ExtendedProperties["AddBlankRow"] != null && 
                        model.isSkipSecurity(T) == false) {
                    filtersec = conn.QHS.AppAnd(filter,
                                conn.security.SelectCondition(GetTableForReading(T), true));
                }
                DataTable data = RUN_SELECT(conn, GetTableForReading(T), columlist, sortBy,
                                    filtersec, top, null, prepare && (filtersec == filter));
                QueryCreator.CheckKey(T, ref data);
                data.Namespace = T.Namespace;
                QueryCreator.MergeDataTable(T, data);
            }
            catch (Exception e) {
                ErrorLogger.Logger.markException(e,"static RUN_SELECT_INTO_TABLE(" + T.TableName + ")");
            }
            metaprofiler.StopTimer(handle);

        }

        /// <summary>
        /// Reads data into a given table, skipping temporary columns
        /// </summary>
        /// <param name="T"></param>
        /// <param name="filter"></param>
        /// <param name="sort_by"></param>
        /// <param name="TOP"></param>
        public virtual void selectIntoTable(DataTable T, MetaExpression filter, string sort_by=null,
            string TOP=null) {
            RUN_SELECT_INTO_TABLE(T, sort_by, filter?.toSql(QHS, this), TOP,false);
        }

        /// <summary>
        /// Reads data into a given table, skipping temporary columns
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sortBy">sorting for db reading</param>
        /// <param name="filter">condition to apply</param>
        /// <param name="top"></param>
        /// <param name="prepare"></param>
        public virtual void RUN_SELECT_INTO_TABLE(DataTable T, string sortBy, string filter, string top, bool prepare) {
            if (openError) return;
            var handle = metaprofiler.StartTimer($"public RUN_SELECT_INTO_TABLE({T.TableName})");
            string columnlist = QueryCreator.ColumnNameList(T);
            var EmptyTable = SingleTableClone(T, false);
            if (model.markedToAddBlankRow(T)) model.markedToAddBlankRow(EmptyTable);
            if (model.isSkipSecurity(T))model.setSkipSecurity(EmptyTable,true);                
            EmptyTable.TableName = GetTableForReading(T);
            try {
                RUN_SELECT_INTO_EMPTY_TABLE(ref EmptyTable, columnlist, sortBy, filter, top, null, prepare);
                EmptyTable.AcceptChanges();
            }
            catch (Exception E) {
                MarkException("RUN_SELECT_INTO_TABLE", E);
            }
            EmptyTable.TableName = T.TableName;
            EmptyTable.Namespace = T.Namespace;
            QueryCreator.MergeDataTable(T, EmptyTable);
            metaprofiler.StopTimer(handle);
        }

        /// <summary>
        /// Fills an empty table with the result of a sql join
        /// </summary>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="filterTable1"></param>
        /// <param name="filterTable2"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual DataRow [] readEmptyTableJoined(DataTable table1, string table2, 
            MetaExpression filterTable1, MetaExpression filterTable2, 
            params string[] columns) {

            string sql = getJoinSql(table1, table2, filterTable1, filterTable2, columns);
            DataRow[] res = null;
            try {
                res = SQLRUN_INTO_EMPTY_TABLE(table1, sql);
                table1.AcceptChanges();
            }
            catch (Exception E) {
                MarkException("readEmptyTableJoined", E);
            }

            return res;
        }

        /// <summary>
        ///Executes something like  SELECT {colTable1} from {table1.TableName} JOIN {table2} ON  {joinFilter} [WHERE {whereFilter}]
        /// </summary>
        /// <param name="table1"></param>
        /// <param name="table2"></param>
        /// <param name="filterTable1"></param>
        /// <param name="filterTable2"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public virtual DataRow [] readTableJoined(DataTable table1, string table2, 
                MetaExpression filterTable1, MetaExpression filterTable2, 
                params string[] columns) {
            string sql = getJoinSql(table1, table2, filterTable1, filterTable2, columns);
            return SQLRUN_INTO_TABLE(table1,sql);
        }

        /// <summary>
        /// Reads data into a given table, skipping temporary columns
        /// </summary>
        /// <param name="T"></param>
        /// <param name="sql">sorting for db reading</param>
        /// <param name="timeout">Timeout in second, 0 means no timeout, -1 means default timeout</param>
        public virtual DataRow [] SQLRUN_INTO_TABLE(DataTable T, string sql, int timeout=-1) {
            if (openError) return null;

            if (timeout == -1) timeout = defaultTimeOut;
            int handle = metaprofiler.StartTimer($"public SQLRUN_INTO_TABLE*{T.TableName}");
            var EmptyTable = SingleTableClone(T, false);
            EmptyTable.TableName = GetTableForReading(T);
            try {
                DataRow[] res = SQLRUN_INTO_EMPTY_TABLE(EmptyTable, sql,timeout);
                EmptyTable.AcceptChanges();
            }
            catch (Exception E) {
                MarkException("SQLRUN_INTO_EMPTY_TABLE", E);
            }
            EmptyTable.TableName = T.TableName;
            EmptyTable.Namespace = T.Namespace;
            
            QueryCreator.MergeDataTable(T, EmptyTable);
            metaprofiler.StopTimer(handle);
            return null;
        }

     

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
        public virtual DataTable RUN_SELECT(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            bool prepare) {
            return RUN_SELECT(tablename, columnlist, order_by, filter, TOP, null, prepare);
        }


        static int transferred = 0;

        /// <summary>
        /// Reads data from db and return a DataTable
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="order_by"></param>
        /// <param name="filter"></param>
        /// <param name="TOP"></param>
        /// <param name="group_by"></param>
        /// <param name="prepare"></param>
        /// <returns></returns>
		public static DataTable RUN_SELECT(DataAccess Conn, string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare) {
            try {
                if (IsLocal) {
                    return Conn.RUN_SELECT(tablename, columnlist, order_by, filter, TOP, group_by, prepare);
                }

                byte[] A = Conn.MAIN_RUN_SELECT(
                    tablename, columnlist, order_by, filter, TOP, group_by, prepare);
                transferred += A.Length;

                DataSet D = DataSetUtils.UnpackDataSet( A);
                DataTable T = D.Tables[0];
                ErrorLogger.Logger.warnEvent(tablename + "(" + filter + ")" +
                    " Righe: " + T.Rows.Count.ToString() +
                    " Trasferiti: " + A.Length.ToString() + " totali = " + transferred.ToString());

                D.Tables.Remove(T);
                return T;
            }
            catch (Exception E) {
                ErrorLogger.Logger.markException(E,"RUN_SELECT("+tablename+")");
            }
            return null;
        }

        /// <summary>
        /// Reads data from db and return a DataTable 
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="order_by"></param>
        /// <param name="filter"></param>
        /// <param name="TOP"></param>
        /// <param name="prepare"></param>
        /// <returns></returns>
        public static DataTable RUN_SELECT(DataAccess Conn, string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            bool prepare) {
            return RUN_SELECT(Conn, tablename, columnlist, order_by, filter, TOP, null, prepare);
        }

        /// <summary>
        /// Reads data from db and return a DataTable
        /// </summary>
        /// <param name="Conn"></param>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="order_by"></param>
        /// <param name="filter"></param>
        /// <param name="prepare"></param>
        /// <returns></returns>
        public static DataTable RUN_SELECT(DataAccess Conn, string tablename,
            string columnlist,
            string order_by,
            string filter,
            bool prepare) {
            return RUN_SELECT(Conn, tablename, columnlist, order_by, filter, null, null, prepare);
        }



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
        public virtual byte[] MAIN_RUN_SELECT(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare
            ) {
            var T = CreateTableByName(tablename, columnlist);
            var MyDS = new DataSet("dummy");
            MyDS.Tables.Add(T);
            //			if (T.Columns.Count==0){
            //				.Show("Non sono disponibile le informazioni relative alle colonne di "+
            //					tablename + ". Questo può accadere a causa di una erronea installazione. "+
            //					"Ad esempio, non è stato eseguito AnalizzaStruttura.","Errore");
            //			}
            try {
                RUN_SELECT_INTO_EMPTY_TABLE(ref MyDS, columnlist, order_by, filter, TOP, group_by, prepare);
            }
            catch (Exception E) {
                ErrorLogger.Logger.markException(E,$"MAIN_RUN_SELECT({tablename})");
            }

            return mdl_utils.DataSetUtils.PackDataSet(MyDS);
        }


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
        public virtual DataTable readTable(string tablename,
            MetaExpression filter=null,
             string columnlist="*",
             string order_by=null,             
             string TOP=null) {
            return RUN_SELECT(tablename, columnlist, order_by, filter?.toSql(QHS, this), TOP, false);
        }

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
        public virtual DataTable RUN_SELECT(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare) {
	        var T = CreateTableByName(tablename, columnlist);
	        var MyDS = new DataSet("dummy");
            MyDS.Tables.Add(T);
            ClearDataSet.RemoveConstraints(MyDS);
            //			if (T.Columns.Count==0){
            //				.Show("Non sono disponibile le informazioni relative alle colonne di "+
            //					tablename + ". Questo può accadere a causa di una erronea installazione. "+
            //					"Ad esempio, non è stato eseguito AnalizzaStruttura.","Errore");
            //			}
            RUN_SELECT_INTO_EMPTY_TABLE(ref MyDS, columnlist, order_by, filter, TOP, group_by, prepare);
            MyDS.Tables.Remove(T);
            return T;
        }

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
		public virtual DataTable RUN_SELECT_2ndVer(string tablename,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare) {
	        var MyDS = new DataSet("dummy");
            ClearDataSet.RemoveConstraints(MyDS);
            DataTable EmptyTable = null;
            if (columnlist == null) columnlist = "*";
            filter = Security.quotedCompile(filter);

            if (openError) return null;

            int handle = metaprofiler.StartTimer("RUN_SELECT_2ndVer(" + tablename + ")");
            var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

            string SelCmd = "SELECT ";
            if (TOP != null) SelCmd += " TOP " + TOP + " ";
            SelCmd += columnlist;
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            Cmd.CommandText = SelCmd;
            Cmd.CommandTimeout = 600;

            //if (GetData.IsSkipSecurity(EmptyTable) == false) {
            //    filter = QHS.AppAnd(filter,
            //                SelectCondition(tablename, true));
            //}

            if (filter != null && filter != "") {
                if (prepare && PrepareEnabled) {
                    Cmd.CommandText += " WHERE ";
                    AddWhereClauses(ref Cmd, filter, tablename);
                }
                else {
                    Cmd.CommandText += " WHERE " + filter + " ";
                }
            }

            //			if (T.ExtendedProperties["sort_by"]!=null){
            //				sort_by = T.ExtendedProperties["sort_by"].ToString();
            //			}
            if (group_by != null) {
                Cmd.CommandText += " GROUP BY " + group_by;
                //				T.ExtendedProperties["sort_by"] = sort_by;
            }

            if (order_by != null) {
                Cmd.CommandText += " ORDER BY " + order_by;
            }


            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }

            try {
                if (prepare && PrepareEnabled) Cmd = GetPreparedCommand(Cmd);
            }
            catch (Exception E) {
                MarkException("RUN_SELECT_2ndVer: Error preparing " + Cmd.CommandText.ToString(), E);
            }
            //MarkEvent("running "+SelCmd);
            var DA = new SqlDataAdapter(Cmd);
            DA.TableMappings.Add("Table", tablename);
            DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;

            try {
                handleFill = metaprofiler.StartTimer("DA.Fill*" + tablename);
                DA.Fill(MyDS);
                EmptyTable = MyDS.Tables[tablename];
                //if (EmptyTable.Rows.Count > 10000 && EmptyTable.Rows.Count < 100000 && filter == null || filter == "") {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
                //if (EmptyTable.Rows.Count > 100000) {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException($"RUN_SELECT_2ndVer: Connection truncated. Running {Cmd.CommandText}", SqlE);

                }
                else {
                    MarkException("RUN_SELECT_2ndVer: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }

            }
            catch (Exception E) {

                MarkException("RUN_SELECT_2ndVer: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Cmd.Dispose();
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            DA.Dispose();
            MyDS.Tables.Remove(EmptyTable);

            var DBS = GetStructure(tablename);
            if (DBS.columntypes.Rows.Count == 0) {
                metaprofiler.StopTimer(handle);
                return EmptyTable;
            }
            string filterkey = "(iskey='S')";
            DataRow[] keycols = DBS.columntypes.Select(filterkey);
            if (keycols.Length > 0) {
                DataColumn[] Key = new DataColumn[keycols.Length];
                for (int i = 0; i < keycols.Length; i++) {
                    Key[i] = EmptyTable.Columns[keycols[i]["field"].ToString()];
                }
                EmptyTable.PrimaryKey = Key;
            }
            metaprofiler.StopTimer(handle);
            SetLastRead();



            return EmptyTable;
        }

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
        public virtual Dictionary<T, S> readSimpleDictionary<T, S>(string tablename,
                MetaExpression filter,
               string keyField, string valueField              
               ) {
            return readSimpleDictionary<T,S>(tablename, keyField, valueField, filter?.toSql(QHS, this));
        }

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
        public virtual Dictionary<T, S> readSimpleDictionary<T,S>(string tablename,
                string keyField, string valueField,
                string filter=null
                ) {
            filter = Security.quotedCompile(filter);
            if (openError) return null;
            var result = new Dictionary<T, S>();

            int handle = metaprofiler.StartTimer("readSimpleDictionary*" + tablename);
            var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

            string SelCmd = $"SELECT {keyField},{valueField} ";
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            Cmd.CommandTimeout = 600;
            
            if (filter != null && filter != "") {
                SelCmd += " WHERE " + filter + " ";
            }
            Cmd.CommandText = SelCmd;

            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }




            //DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            SqlDataReader reader = null;
            try {
                handleFill = metaprofiler.StartTimer("Cmd.ExecuteReader*" + tablename );

                reader = Cmd.ExecuteReader();
                if (reader.HasRows) {
                    int countField = reader.FieldCount;
                    while (reader.Read()) {
                        result[(T)reader[0]] = (S)reader[1];
                    }
                }
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("readSimpleDictionary: Connection truncated. Running " +
                        Cmd.CommandText.ToString(), SqlE);

                }
                else {
                    MarkException("readSimpleDictionarty: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }

            }
            catch (Exception E) {
                MarkException("readSimpleDictionary: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            if (reader!=null) reader.Dispose();
            SetLastRead();
            metaprofiler.StopTimer(handle);
            return result;
        }



        /// <summary>
        /// Creates a dictionary key => rowObject from a query on a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="keyField"></param>
        /// <param name="fieldList"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual Dictionary<T, RowObject> readRowObjectDictionary<T>(string tablename,
                MetaExpression filter,
                string keyField, string fieldList
                
                ) {
            return readRowObjectDictionary<T>(tablename, keyField, fieldList, filter?.toSql(QHS, this));

        }
        /// <summary>
        /// Creates a dictionary key => rowObject from a query on a table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablename"></param>
        /// <param name="keyField">key field of dictionary</param>
        /// <param name="fieldList">list value to read (must not include keyField)</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual Dictionary<T, RowObject> readRowObjectDictionary<T>(string tablename,
                string keyField, string fieldList,
                string filter = null
                ) {
            filter = Security.quotedCompile(filter);
            if (openError) return null;
            var result = new Dictionary<T, RowObject>();

            int handle = metaprofiler.StartTimer("RowObject_Select*" + tablename );
            var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

            string SelCmd = $"SELECT {keyField},{fieldList} ";
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            Cmd.CommandText = SelCmd;
            Cmd.CommandTimeout = 600;

            if (filter != null && filter != "") {
                SelCmd += " WHERE " + filter + " ";
                Cmd.CommandText += " WHERE " + filter + " ";
            }

            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }




            //DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            SqlDataReader reader = null;
            try {
                handleFill = metaprofiler.StartTimer("Cmd.ExecuteReader*" + tablename );

                reader = Cmd.ExecuteReader();
                if (reader.HasRows) {
                    int countField = reader.FieldCount;
                    var lookup = new Dictionary<string, int>();
                    for (int i = 0; i < countField; i++) {
                        lookup[reader.GetName(i)] = i;
                    }
                    string[] fieldNames = Enumerable.Range(0, countField).Select(reader.GetName).ToArray();
                    while (reader.Read()) {
                        object[] arr = new object[countField];
                        reader.GetValues(arr);                        
                        result[(T)arr[0]]=new RowObject(lookup, arr);
                    }
                }
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("readRowObjectDictionary: Connection truncated. Running " +
                        Cmd.CommandText.ToString(), SqlE);

                }
                else {
                    MarkException("readSimpleDictionarty: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }

            }
            catch (Exception E) {
                MarkException("readRowObjectDictionary: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            reader.Dispose();
            SetLastRead();
            return result;
        }

        /// <summary>
        /// Selects a set of rowobjects from db
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="tables">logical table names</param>
        /// <returns></returns>
        public virtual Dictionary<string, List<RowObject>> multiRowObject_Select(string cmd, params string[] tables) {

            if (openError) return null;

            var result = new Dictionary<string, List<RowObject>>();

            int handle = metaprofiler.StartTimer("multiRowObject_Select()");
            var Cmd = new SqlCommand(cmd, MySqlConnection, CurrTransaction()) {
                CommandTimeout = 600
            };

            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }

            //DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            SqlDataReader reader = null;
            try {
                handleFill = metaprofiler.StartTimer("Cmd.ExecuteReader*(multitable)");

                reader = Cmd.ExecuteReader();
                int nSet = 0;
                while (nSet < tables.Length) {
                    List<RowObject> currSet = new List<RowObject>();
                    result[tables[nSet]] = currSet;

                    if (!reader.HasRows) {
                        nSet++;
                        reader.NextResult();
                        continue;
                    }

                    int countField = reader.FieldCount;
                    var lookup = new Dictionary<string, int>();
                    for (int i = 0; i < countField; i++) {
                        lookup[reader.GetName(i)] = i;
                    }

                    while (reader.Read()) {
                        object[] arr = new object[countField];
                        reader.GetValues(arr);
                        currSet.Add(new RowObject(lookup, arr));
                    }

                    reader.NextResult();
                    nSet++;
                }


            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("multiRowObject_Select: Connection truncated. Running " +
                        Cmd.CommandText.ToString(), SqlE);

                }
                else {
                    MarkException("multiRowObject_Select: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }

            }
            catch (Exception E) {

                MarkException("multiRowObject_Select: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            reader?.Dispose();



            SetLastRead();



            return result;
        }


        /// <summary>
        /// Selects a set of rowobjects from db
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="filter"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        public virtual List<RowObject> RowObjectSelect(string tablename,
       string columnlist,
       MetaExpression filter,
       string order_by = null,
       string TOP = null) {
            return RowObject_Select(tablename, columnlist, filter?.toSql(QHS, this), order_by, null);
        }

        /// <summary>
        /// Selects a set of rowobjects from db
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="columnlist"></param>
        /// <param name="filter"></param>
        /// <param name="order_by"></param>
        /// <param name="TOP"></param>
        /// <returns></returns>
        public virtual List<RowObject> RowObject_Select(string tablename,
        string columnlist,
        string filter,
        string order_by = null,
        string TOP = null) {
            if (columnlist == null) columnlist = "*";
            filter = Security.quotedCompile(filter);

            if (openError) return null;

            var result = new List<RowObject>();

            int handle = metaprofiler.StartTimer("RowObject_Select*" + tablename );
            var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

            string SelCmd = "SELECT ";
            if (TOP != null) SelCmd += " TOP " + TOP + " ";
            SelCmd += columnlist;
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            Cmd.CommandTimeout = 600;

            //if (model.isSkipSecurity(EmptyTable) == false) {
            //    filter = QHS.AppAnd(filter,
            //                SelectCondition(tablename, true));
            //}

            if (filter != null && filter != "") {
                SelCmd += " WHERE " + filter + " ";
            }


            if (order_by != null) {
                SelCmd += " ORDER BY " + order_by;
            }
            
            Cmd.CommandText = SelCmd;


            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }




            //DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            SqlDataReader reader = null;
            try {
                handleFill = metaprofiler.StartTimer("Cmd.ExecuteReader*" + tablename );

                reader = Cmd.ExecuteReader();
                if (reader.HasRows) {
                    int countField = reader.FieldCount;
                    Dictionary<string, int> lookup = new Dictionary<string, int>();
                    for (int i = 0; i < countField; i++) {
                        lookup[reader.GetName(i)] = i;
                    }
                    //string[] fieldNames = Enumerable.Range(0, countField).Select(reader.GetName).ToArray();
                    while (reader.Read()) {
                        object[] arr = new object[countField];
                        reader.GetValues(arr);
                        result.Add(new RowObject(lookup, arr));
                    }
                }
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("RowObject_Select: Connection truncated. Running " +
                        Cmd.CommandText.ToString(), SqlE);

                }
                else {
                    MarkException("RowObject_Select: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }

            }
            catch (Exception E) {

                MarkException("RowObject_Select: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            reader?.Dispose();



            SetLastRead();



            return result;
        }

        private void RUN_SELECT_INTO_EMPTY_TABLE(ref DataTable EmptyTable,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare) {
            try {
                RUN_SELECT_INTO_EMPTY_TABLE(EmptyTable,
                    columnlist, order_by, filter, TOP, group_by, prepare);
            }
            catch (Exception E) {
                MarkException("Run_select_into_empty_table", E);
            }

        }


        private void RUN_SELECT_INTO_EMPTY_TABLE(DataTable EmptyTable,
            string columnlist,
            string order_by,
            string filter,
            string TOP,
            string group_by,
            bool prepare) {

	        var NewDS = new DataSet("dummy");
            NewDS.Tables.Add(EmptyTable);
            RUN_SELECT_INTO_EMPTY_TABLE(ref NewDS, columnlist, order_by, filter, TOP, group_by, prepare);
            NewDS.Tables.Remove(EmptyTable);
        }

        List<SelectBuilder> GroupSelect(List<SelectBuilder> L) {
	        var Grouped = new List<SelectBuilder>();
	        var LResult = new List<SelectBuilder>();

            //Cicla solo sugli ottimizzati
            foreach (var S in L) {
                if (!S.isOptimized()) continue;

                //se è ottimizzato lo aggiunge in modo ottimizzato oppure niente
                bool added = false;
                foreach (var G in Grouped) {
                    if (!G.isOptimized()) continue;
                    if (!G.CanAppendTo(S)) continue;
                    if (G.OptimizedAppendTo(S, QHS)) {
                        added = true;
                        break;
                    }
                }
                if (!added) {
                    Grouped.Add(S);
                }


            }

            //riprende gli ottimizzati
            foreach (var S in Grouped) {
                SelectBuilder ToGroup = null;
                foreach (var G in LResult) {
                    if (G.CanAppendTo(S)) {
                        ToGroup = G;
                        break;
                    }

                }
                if (ToGroup != null) {
                    ToGroup.AppendTo(S, QHS);
                }
                else {
                    LResult.Add(S);
                }
            }

            //prende i non  ottimizzati
            foreach (var S in L) {
                if (S.isOptimized()) continue;
                SelectBuilder ToGroup = null;
                foreach (var G in LResult) {
                    if (G.CanAppendTo(S)) {
                        ToGroup = G;
                        break;
                    }

                }
                if (ToGroup != null) {
                    ToGroup.AppendTo(S, QHS);
                }
                else {
                    LResult.Add(S);
                }
            }


            return LResult;
        }

		/// <summary>
		/// Executes a List of Select, returning data into empty tables specified by each select. 
		/// return dataset
		/// </summary>
		/// <param name="SelList"></param>
		public virtual DataSet MULTI_RUN_SELECT_SIMPLIFIED(List<SelectBuilder> SelList) {
			DataSet DD = null;
			if (openError) return DD;  //TODO: verificare se meglio null
			Open();
			if (openError) {
				return DD; //TODO: verificare se meglio null
			}

			int handle1 = metaprofiler.StartTimer("prepare MULTI SELECT");

			SelList = GroupSelect(SelList);

			string multitab = "";
			var SelCmd = new StringBuilder();
			bool first = true;
			//DataSet D = null;

			//string[] alltables = (from Sel in SelList select Sel.tablename).ToArray();
			//preScanStructures(alltables);

			foreach (var Sel in SelList) {
				string filter = Security.quotedCompile(Sel.filter);
				if (!first) SelCmd.Append(";");
				SelCmd.Append("SELECT ");
				if (Sel.TOP != null) SelCmd.Append(" TOP " + Sel.TOP + " ");
				SelCmd.Append(Sel.columnlist);

				SelCmd.Append(" FROM " + Sel.tablename);
				multitab += Sel.tablename + " ";

				string filtersec = filter;
				if (Sel.DestTable == null ||
						model.isSkipSecurity(Sel.DestTable) == false) {
					filtersec = QHS.AppAnd(filter,
								Security.SelectCondition(Sel.tablename, true));
				}
				if (filtersec != null && filtersec.Trim() != "") {
					SelCmd.Append(" WHERE " + filtersec + " ");
				}

				//if (Sel.group_by != null) {
				//	SelCmd.Append(" GROUP BY " + Sel.group_by);
				//}

				if (Sel.order_by != null) {
					SelCmd.Append(" ORDER BY " + Sel.order_by);
				}

				if (Sel.DestTable != null && DD == null) {
					DD = Sel.DestTable.DataSet;
				}

				first = false;
			}
			if (DD==null) DD=new DataSet("x");

			metaprofiler.StopTimer(handle1);

			int handle = metaprofiler.StartTimer("MULTISEL*" + multitab);


			var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction()) {
                CommandText = SelCmd.ToString(),

                CommandTimeout = 600
            };
			var DA = new SqlDataAdapter(Cmd);
			DA.SelectCommand.Transaction = Cmd.Transaction;
		    ClearDataSet.RemoveConstraints(DD);
			
			for (int i = 0; i < SelList.Count; i++) {
				if (i == 0) {
					DA.TableMappings.Add("Table", SelList[i].tablemap);
				} else {
					DA.TableMappings.Add("Table" + i.ToString(), SelList[i].tablemap);
				}
			}

			try {
				int handleFill = 0;
				//int handleFill = metaprofiler.StartTimer("MULTISEL DA.Fill" + multitab);
				//DA.Fill(D);
				//metaprofiler.StopTimer(handleFill);

			    ClearDataSet.RemoveConstraints(DD);
				handleFill = metaprofiler.StartTimer("MULTISEL DA.FillCopy*" + multitab);
				DA.Fill(DD);
				metaprofiler.StopTimer(handleFill);


			} catch (SqlException SqlE) {
				if (SqlE.Class >= 20) {
					ConnectionHasBeenClosedBySystem = true;
					openError = true;
					MarkException("MULTI_RUN_SELECT: Connection truncated. Running " +
						Cmd.CommandText.ToString(), SqlE);

				} else {
					MarkException("MULTI_RUN_SELECT: Error running " +
						Cmd.CommandText.ToString(), SqlE);
				}
			} catch (Exception E) {
				MarkException("RUN_SELECT_INTO_EMPTY_TABLE: Error running " + Cmd.CommandText.ToString(), E);
			} finally {
				Close();
			}
			DA.Dispose();

			metaprofiler.StopTimer(handle);
			SetLastRead();

			return DD;
		}
      


        /// <summary>
		/// Executes a List of Select, returning data in the tables specified by each select. 
		/// </summary>
		/// <param name="SelList"></param>
		public virtual void MULTI_RUN_SELECT(List<SelectBuilder> SelList) {
            if (openError) return;
            Open();
            if (openError) {
                return;
            }

            int handle1 = metaprofiler.StartTimer("prepare MULTI SELECT ");

            SelList = GroupSelect(SelList);

            string multitab = "";
            StringBuilder SelCmd = new StringBuilder();
            bool first = true;
            DataSet D = null;

            //string[] alltables = (from Sel in SelList select Sel.tablename).ToArray();
            //preScanStructures(alltables);

            foreach (SelectBuilder Sel in SelList) {
                string filter = Security.quotedCompile(Sel.filter);
                if (!first) SelCmd.Append(";");
                SelCmd.Append("SELECT ");
                if (Sel.TOP != null) SelCmd.Append(" TOP " + Sel.TOP + " ");
                SelCmd.Append(Sel.columnlist);

                SelCmd.Append(" FROM " + Sel.tablename);
                multitab += Sel.tablename + " ";

                string filtersec = filter;
                if (Sel.DestTable == null || !model.isSkipSecurity(Sel.DestTable)) {
                    filtersec = QHS.AppAnd(filter, Security.SelectCondition(Sel.tablename, true));
                }
                if (filtersec != null && filtersec.Trim() != "") {
                    SelCmd.Append(" WHERE " + filtersec + " ");
                }

                if (Sel.group_by != null) {
                    SelCmd.Append(" GROUP BY " + Sel.group_by);
                }

                if (Sel.order_by != null) {
                    SelCmd.Append(" ORDER BY " + Sel.order_by);
                }

                if (Sel.DestTable != null && D == null) {
                    D = Sel.DestTable.DataSet;
                }
                model.invokeActions(Sel.DestTable,TableAction.beginLoad);
                first = false;
            }
            if (D == null) {
                D = new DataSet("temp");
            }

            metaprofiler.StopTimer(handle1);

            int handle = metaprofiler.StartTimer("MULTISEL " + multitab);


            var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction()) {
                CommandText = SelCmd.ToString(),
                CommandTimeout = 600
            };
            var DA = new SqlDataAdapter(Cmd);
            DA.SelectCommand.Transaction = Cmd.Transaction;
            ClearDataSet.RemoveConstraints(D);
            var DD = new DataSet("x");

            for (var i = 0; i < SelList.Count; i++) {
                if (i == 0) {
                    DA.TableMappings.Add("Table", SelList[i].tablemap);
                }
                else {
                    DA.TableMappings.Add("Table" + i, SelList[i].tablemap);
                }
            }

            try {
                //int handleFill = metaprofiler.StartTimer("MULTISEL DA.Fill" + multitab);
                //DA.Fill(D);
                //metaprofiler.StopTimer(handleFill);

                ClearDataSet.RemoveConstraints(DD);
                var handleFill = metaprofiler.StartTimer("MULTISEL DA.FillCopy*" + multitab);
                DA.Fill(DD);
                
                metaprofiler.StopTimer(handleFill);


            }
            catch (SqlException sqlE) {
                if (sqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("MULTI_RUN_SELECT: Connection truncated. Running " +
                        Cmd.CommandText, sqlE);

                }
                else {
                    MarkException("MULTI_RUN_SELECT: Error running " +
                        Cmd.CommandText, sqlE);
                }
            }
            catch (Exception e) {
                MarkException("RUN_SELECT_INTO_EMPTY_TABLE: Error running " + Cmd.CommandText, e);
            }
            finally {
	            DA.Dispose();
                Cmd.Dispose();
	            

	            Close();
            }
           

            
            foreach (var sel in SelList) {
	            
                if (sel.DestTable == null) {
                    sel.DestTable = D.Tables[sel.tablename];
                    D.Tables.Remove(sel.DestTable);
                    model.invokeActions(sel.DestTable,TableAction.endLoad);
                }
                else {
                    var handleMerge = metaprofiler.StartTimer("MULTISEL DA.Merge*" + multitab);
                    sel.DestTable.BeginLoadData();
                    if (DD.Tables[sel.DestTable.TableName] != null)
                        sel.DestTable.Merge(DD.Tables[sel.DestTable.TableName], true, MissingSchemaAction.Ignore);
                    sel.DestTable.EndLoadData();
                    model.invokeActions(sel.DestTable, TableAction.endLoad);
                    metaprofiler.StopTimer(handleMerge);
                }
                sel.OnRead();
            }

            metaprofiler.StopTimer(handle);
            SetLastRead();

        }

        /// <summary>
        /// Experimental function, unused
        /// </summary>
        /// <param name="t"></param>
        /// <param name="order_by"></param>
        /// <param name="filter"></param>
        /// <param name="TOP"></param>
        /// <param name="group_by"></param>
        /// <param name="prepare"></param>
        public virtual void RUN_SELECT_INTO_TABLE_direct(DataTable t,
            string order_by,
            string filter,
            string TOP, string group_by,
            bool prepare) {

	        var EmptyTable = t;
	        var originalDataSet = t.DataSet;
	        var tempDataSet = originalDataSet;
            if (originalDataSet == null) {
                tempDataSet = new DataSet("x");
                tempDataSet.Tables.Add(t);
            }
            string columnlist = QueryCreator.ColumnNameList(EmptyTable);

            filter = Security.quotedCompile(filter);

            if (openError) return;

            //int handle = metaprofiler.StartTimer("RUN_SELECT_INTO_EMPTY_TABLE("+EmptyTable.TableName+")");
            string tablename = GetTableForReading(EmptyTable);
            int handle = metaprofiler.StartTimer("RUN_SELECT_INTO_TABLE_direct*(" + tablename + ")");
            SqlCommand Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

            string SelCmd = "SELECT ";
            if (TOP != null) SelCmd += " TOP " + TOP + " ";
            SelCmd += columnlist;
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            Cmd.CommandText = SelCmd;
            Cmd.CommandTimeout = 600;

            if (!model.isSkipSecurity(EmptyTable)) {
                filter = QHS.AppAnd(filter, Security.SelectCondition(tablename, true));
            }

            if (filter != null && filter != "") {
                if (prepare && PrepareEnabled) {
                    Cmd.CommandText += " WHERE ";
                    AddWhereClauses(ref Cmd, filter, tablename);
                }
                else {
                    Cmd.CommandText += " WHERE " + filter + " ";
                }
            }

            if (group_by != null) {
                Cmd.CommandText += " GROUP BY " + group_by;
            }

            if (order_by != null) {
                Cmd.CommandText += " ORDER BY " + order_by;
            }

            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return;
            }

            try {
                if (prepare && PrepareEnabled) Cmd = GetPreparedCommand(Cmd);
            }
            catch (Exception E) {
                MarkException("RUN_SELECT_INTO_TABLE_direct: Error preparing " + Cmd.CommandText, E);
            }

            //MarkEvent("running "+SelCmd);
            SqlDataAdapter DA = new SqlDataAdapter(Cmd);
            DA.TableMappings.Add("Table", t.TableName);
            DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            try {
                handleFill = metaprofiler.StartTimer("DA.FillDirect*" + EmptyTable.TableName );
                EmptyTable.BeginLoadData();
                DA.Fill(tempDataSet);
                EmptyTable.EndLoadData();
                //if (EmptyTable.Rows.Count > 10000 && EmptyTable.Rows.Count < 100000 && filter == null || filter == "") {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
                //if (EmptyTable.Rows.Count > 100000) {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("RUN_SELECT_INTO_TABLE_direct: Connection truncated. Running " +
                        Cmd.CommandText.ToString(), SqlE);

                }
                else {
                    MarkException("RUN_SELECT_INTO_TABLE_direct: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }
            }
            catch (Exception E) {
                MarkException("RUN_SELECT_INTO_TABLE_direct: Error running " + Cmd.CommandText, E);
            }
            finally {
                Cmd.Dispose();
                DA.Dispose();
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            if (originalDataSet == null) {
                tempDataSet.Tables.Remove(EmptyTable);
            }
            
            metaprofiler.StopTimer(handle);
            SetLastRead();
        }


        //private void MULTI_RUN_SELECT_2ndVersion(List<SelectBuilder> SelList) {
        //    if (openError) return;
        //    Open();
        //    if (openError) {
        //        return;
        //    }

        //    int handle1 = metaprofiler.StartTimer("prepare MULTI_RUN_SELECT_2ndVersion ");

        //    SelList = GroupSelect(SelList);

        //    string multitab = "";
        //    StringBuilder SelCmd = new StringBuilder();
        //    bool first = true;
        //    DataSet D = null;
        //    List<List<int>> listColNum = new List<List<int>>();

        //    for (int nTable = 0; nTable < SelList.Count; nTable++) {
        //        SelectBuilder Sel = SelList[nTable];
        //        List<int> colNum = new List<int>();
        //        listColNum.Add(colNum);

        //        string filter = Security.quotedCompile(Sel.filter);
        //        if (!first) SelCmd.Append(";");
        //        SelCmd.Append("SELECT ");
        //        if (Sel.TOP != null) SelCmd.Append(" TOP " + Sel.TOP + " ");
        //        DataTable destTable = Sel.DestTable;

        //        if (Sel.columnlist == "*") {
        //            string newColList = null;
        //            for (int i = 0; i < destTable.Columns.Count; i++) {
        //                DataColumn C = destTable.Columns[i];
        //                if (!QueryCreator.IsRealColumn(C)) continue;
        //                if (newColList != null)
        //                    newColList += ",";
        //                else
        //                    newColList = "";
        //                newColList += destTable.Columns[i].ColumnName;
        //                colNum.Add(i);
        //            }
        //            Sel.columnlist = newColList;
        //        }
        //        else {
        //            string[] colNames = Sel.columnlist.Split(',');
        //            foreach (string s in colNames) {
        //                for (int i = destTable.Columns.Count - 1; i >= 0; i--) {
        //                    if (destTable.Columns[i].ColumnName == s) {
        //                        colNum.Add(i);
        //                        break;
        //                    }
        //                }
        //            }
        //            if (colNames.Length < colNum.Count) {
        //                throw new Exception("MULTI_RUN_SELECT_2ndVersion: Error reading " + Sel.tablename);
        //            }
        //        }
        //        SelCmd.Append(Sel.columnlist);

        //        SelCmd.Append(" FROM " + Sel.tablename);
        //        multitab += Sel.tablename + " ";

        //        string filtersec = filter;
        //        if (Sel.DestTable == null ||
        //            model.isSkipSecurity(Sel.DestTable) == false) {
        //            filtersec = QHS.AppAnd(filter,
        //                        Security.SelectCondition(Sel.tablename, true));
        //        }
        //        if (filtersec != null && filtersec.Trim() != "") {
        //            SelCmd.Append(" WHERE " + filtersec + " ");
        //        }

        //        if (Sel.group_by != null) {
        //            SelCmd.Append(" GROUP BY " + Sel.group_by);
        //        }

        //        if (Sel.order_by != null) {
        //            SelCmd.Append(" ORDER BY " + Sel.order_by);
        //        }

        //        if (Sel.DestTable != null && D == null) {
        //            D = Sel.DestTable.DataSet;
        //        }

        //        first = false;
        //    }
        //    if (D == null) {
        //        D = new DataSet("temp");
        //    }

        //    metaprofiler.StopTimer(handle1);

        //    int handle = metaprofiler.StartTimer("MULTI_RUN_SELECT_2ndVersion " + multitab);


        //    SqlCommand Cmd = new SqlCommand("", MySqlConnection, CurrTransaction()) {
        //        CommandText = SelCmd.ToString(),
        //        CommandTimeout = 600
        //    };
        //    ClearDataSet.RemoveConstraints(D);
        //    DataSet DD = new DataSet("x");

        //    SqlDataReader rdr = null;
        //    try {
        //        int handleFill = 0;

        //        ClearDataSet.RemoveConstraints(DD);
        //        handleFill = metaprofiler.StartTimer("MULTI_RUN_SELECT_2ndVersion DA.FillCopy " + multitab);

        //        rdr = Cmd.ExecuteReader();
        //        int nSet = 0;
        //        while (nSet < SelList.Count) {
        //            if (!rdr.HasRows) {
        //                nSet++;
        //                rdr.NextResult();
        //                continue;
        //            }
        //            SelectBuilder Sel = SelList[nSet];
        //            List<int> colNum = listColNum[nSet];
        //            DataTable EmptyTable = SimplifiedTableClone(Sel.DestTable);
        //            EmptyTable.BeginLoadData();
        //            while (rdr.Read()) {
        //                if (rdr.FieldCount != colNum.Count) {
        //                    Console.WriteLine("MULTI_RUN_SELECT_2ndVersion: Error reading from " + EmptyTable.TableName);
        //                }
        //                DataRow dataRow = EmptyTable.NewRow();
        //                for (int i = colNum.Count - 1; i >= 0; i--) {
        //                    dataRow[colNum[i]] = rdr[i];
        //                }
        //                EmptyTable.Rows.Add(dataRow);
        //                dataRow.AcceptChanges();

        //            }
        //            EmptyTable.EndLoadData();
        //            DD.Tables.Add(EmptyTable);
        //            rdr.NextResult();
        //            nSet++;
        //        }
        //        metaprofiler.StopTimer(handleFill);


        //    }
        //    catch (SqlException SqlE) {
        //        if (SqlE.Class >= 20) {
        //            ConnectionHasBeenClosedBySystem = true;
        //            openError = true;
        //            MarkException("MULTI_RUN_SELECT_2ndVersion: Connection truncated. Running " +
        //                Cmd.CommandText.ToString(), SqlE);

        //        }
        //        else {
        //            MarkException("MULTI_RUN_SELECT_2ndVersion: Error running " +
        //                Cmd.CommandText.ToString(), SqlE);
        //        }
        //    }
        //    catch (Exception E) {
        //        MarkException("MULTI_RUN_SELECT_2ndVersion: Error running " + Cmd.CommandText.ToString(), E);
        //    }
        //    finally {
        //        Close();
        //    }
        //    rdr?.Dispose();
        //    Cmd.Dispose();


        //    foreach (SelectBuilder Sel in SelList) {
        //        if (Sel.DestTable == null) {
        //            Sel.DestTable = D.Tables[Sel.tablename];
        //            D.Tables.Remove(Sel.DestTable);
        //        }
        //        else {
        //            int handleMerge = metaprofiler.StartTimer("MULTI_RUN_SELECT_2ndVersion DA.Merge " + multitab);
        //            Sel.DestTable.BeginLoadData();
        //            if (DD.Tables[Sel.DestTable.TableName] != null)
        //                Sel.DestTable.Merge(DD.Tables[Sel.DestTable.TableName], true, MissingSchemaAction.Ignore);
        //            Sel.DestTable.EndLoadData();
        //            metaprofiler.StopTimer(handleMerge);
        //        }
        //        Sel.OnRead();
        //    }

        //    metaprofiler.StopTimer(handle);
        //    SetLastRead();

        //}

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
        public virtual DataTable readFromTable(string tablename, MetaExpression filter, string columnlist="*", string order_by = null, string TOP = null) {
            return readFromTable(tablename, columnlist, filter?.toSql(QHS, this), order_by, TOP);
        }

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
        public virtual DataTable readFromTable(string tablename, string columnlist, string filter, string order_by = null,string TOP = null) {
            if (openError) return null;

            var emptyDs = new DataSet();
            ClearDataSet.RemoveConstraints(emptyDs);
            filter = Security.quotedCompile(filter);

            int handle = metaprofiler.StartTimer("readFromTable*" + tablename );
            var Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());

            string SelCmd = "SELECT ";
            if (TOP != null) SelCmd += " TOP " + TOP + " ";
            SelCmd += columnlist;
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            
            Cmd.CommandTimeout = 600;

            filter = QHS.AppAnd(filter, Security.SelectCondition(tablename, true));
            

            if (filter != null && filter != "") {                
                SelCmd += " WHERE " + filter + " ";
            }

            if (order_by != null) {
                SelCmd += " ORDER BY " + order_by;
            }

            Cmd.CommandText = SelCmd;

            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }
           
            //MarkEvent("running "+SelCmd);
            var DA = new SqlDataAdapter(Cmd);            
            DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            
            try {
                handleFill = metaprofiler.StartTimer("readFromTable*" + tablename);
                DA.Fill(emptyDs);
                //if (EmptyTable.Rows.Count>10000 && EmptyTable.Rows.Count < 100000  && filter==null || filter == "") {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
                //if (EmptyTable.Rows.Count > 100000) {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException("readFromTable: Connection truncated. Running " +
                        Cmd.CommandText.ToString(), SqlE);

                }
                else {
                    MarkException("readFromTable: Error running " +
                        Cmd.CommandText.ToString(), SqlE);
                }

            }
            catch (Exception E) {

                MarkException("readFromTable: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            DA.Dispose();
            metaprofiler.StopTimer(handle);
            SetLastRead();
            DataTable EmptyTable= emptyDs.Tables[0];
            emptyDs.Tables.Remove(EmptyTable);
            return EmptyTable;
        }

        /// <summary>
        /// Creates a data adapter for a given sql command
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public virtual SqlDataAdapter GetSqlAdapter(SqlCommand cmd) {
            return new SqlDataAdapter(cmd);
        }

        /// <summary>
        /// Default Timeout for sql operations
        /// </summary>
        public  int defaultTimeOut=90;

        /// <summary>
        /// Exec  a sql statement to merge rows to a table
        /// </summary>
        /// <param name="EmptyTable"></param>
        /// <param name="sql"></param>
        /// <param name="timeout">timeout in second, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public virtual DataRow [] SQLRUN_INTO_EMPTY_TABLE(DataTable EmptyTable,string sql, int timeout=-1) {
            
            if (openError) return null;
            if (timeout == -1) timeout = defaultTimeOut;
            sql = Security.quotedCompile(sql);


            //int handle = metaprofiler.StartTimer("RUN_SELECT_INTO_EMPTY_TABLE("+EmptyTable.TableName+")");
            int handle = metaprofiler.StartTimer("SQLRUN_INTO_EMPTY_TABLE*(" + sql + ")");
            SqlCommand Cmd = new SqlCommand("", MySqlConnection, CurrTransaction()) {
                CommandText = sql, CommandTimeout = timeout
            };
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];



            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return null;
            }
          
            //MarkEvent("running "+SelCmd);
            SqlDataAdapter DA = GetSqlAdapter(Cmd);
            DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            try {
                handleFill = metaprofiler.StartTimer("DA.Fill*" + EmptyTable.TableName);
                EmptyTable.BeginLoadData();
                DA.Fill(EmptyTable);
                EmptyTable.EndLoadData();
                //if (EmptyTable.Rows.Count>10000 && EmptyTable.Rows.Count < 100000  && filter==null || filter == "") {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
                //if (EmptyTable.Rows.Count > 100000) {
                //    LogError("La query:\n\r" + Cmd.CommandText + "\n\rha restituito " + EmptyTable.Rows.Count + " righe.", null);
                //}
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    MarkException($"SQLRUN_INTO_EMPTY_TABLE: Connection truncated. Running {Cmd.CommandText}", SqlE);
                    errorLogger.logException("SQLRUN_INTO_EMPTY_TABLE: Connection truncated. running " + Cmd.CommandText.ToString(),SqlE);

                }
                else {
                    MarkException($"SQLRUN_INTO_EMPTY_TABLE: SqlException running {Cmd.CommandText}", SqlE);
                    errorLogger.logException("SQLRUN_INTO_EMPTY_TABLE: SqlException running " + Cmd.CommandText.ToString(),SqlE);
                }

            }
            catch (Exception E) {
                errorLogger.logException("SQLRUN_INTO_EMPTY_TABLE: Exception running " + Cmd.CommandText.ToString(),E);
                MarkException("SQLRUN_INTO_EMPTY_TABLE: Error running " + Cmd.CommandText.ToString(), E);
            }
            finally {
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            DA.Dispose();
            metaprofiler.StopTimer(handle);
            SetLastRead();
            return EmptyTable.Select();
        }


        /// <summary>
        /// Reads data into a table. Data are read from DB table named EmptyTable.Tablename
        /// </summary>
        /// <param name="QuiteEmptyDataSet"></param>
        /// <param name="columnlist"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <param name="top"></param>
        /// <param name="groupBy"></param>
        /// <param name="prepare"></param>
        private void RUN_SELECT_INTO_EMPTY_TABLE(ref DataSet QuiteEmptyDataSet,
            string columnlist,
            string orderBy,
            string filter,
            string top,
            string groupBy,
            bool prepare) {

            DataTable EmptyTable = QuiteEmptyDataSet.Tables[0];
            if (columnlist == null) columnlist = QueryCreator.ColumnNameList(EmptyTable);

            filter = Security.quotedCompile(filter);

            if (openError) return;

            //int handle = metaprofiler.StartTimer("RUN_SELECT_INTO_EMPTY_TABLE("+EmptyTable.TableName+")");
            string tablename = EmptyTable.TableName;
            int handle = metaprofiler.StartTimer("RUN_SELECT_INTO_EMPTY_TABLE*" + tablename );
            SqlCommand Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());
            //Cmd.Connection= MySqlConnection;
            //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

            string SelCmd = "SELECT ";
            if (top != null) SelCmd += " TOP " + top + " ";
            SelCmd += columnlist;
            SelCmd += " FROM " + GetCentralizedTableName(tablename);

            Cmd.CommandText = SelCmd;
            Cmd.CommandTimeout = 600;

            if (model.isSkipSecurity(EmptyTable) == false) {
                filter = QHS.AppAnd(filter,
                            Security.SelectCondition(tablename, true));
            }

            if (!string.IsNullOrEmpty(filter)) {
                if (prepare && PrepareEnabled) {
                    Cmd.CommandText += " WHERE ";
                    AddWhereClauses(ref Cmd, filter, tablename);
                }
                else {
                    Cmd.CommandText += " WHERE " + filter + " ";
                }
            }

            if (groupBy != null) {
                Cmd.CommandText += " GROUP BY " + groupBy;
            }

            if (orderBy != null) {
                Cmd.CommandText += " ORDER BY " + orderBy;
            }

            Open();
            if (openError) {
                metaprofiler.StopTimer(handle);
                return;
            }

            try {
                if (prepare && PrepareEnabled) Cmd = GetPreparedCommand(Cmd);
            }
            catch (Exception E) {
                string err = "RUN_SELECT_INTO_EMPTY_TABLE: Error preparing " + Cmd.CommandText.ToString();
                MarkException(err, E);
                errorLogger.logException(err,E,dataAccess:this);    

            }
            SqlDataAdapter DA = GetSqlAdapter(Cmd);
            DA.SelectCommand.Transaction = Cmd.Transaction;
            int handleFill = 0;
            try {
                handleFill = metaprofiler.StartTimer("DA.Fill*" + EmptyTable.TableName);
                EmptyTable.BeginLoadData();
                DA.Fill(EmptyTable);
                EmptyTable.EndLoadData();
            }
            catch (SqlException SqlE) {
                if (SqlE.Class >= 20) {
                    ConnectionHasBeenClosedBySystem = true;
                    openError = true;
                    string err = $"RUN_SELECT_INTO_EMPTY_TABLE: Connection truncated. Running {Cmd.CommandText}\n"+
	                    Environment.StackTrace;
                    MarkException(err, SqlE);
                    errorLogger.logException(err,SqlE,dataAccess:this); 


                }
                else {
                    string err = $"RUN_SELECT_INTO_EMPTY_TABLE: SqlException Error running {Cmd.CommandText}\n"+
                                 Environment.StackTrace;
                    MarkException(err, SqlE);
                    
                    errorLogger.logException(err,SqlE,dataAccess:this); 

                }

            }
            catch (Exception E) {
                string err = $"RUN_SELECT_INTO_EMPTY_TABLE: Exception running {Cmd.CommandText}";
                MarkException(err, E);                                        
                errorLogger.logException(err,E,dataAccess:this); 
            }
            finally {
	            Cmd.Dispose();
	            DA.Dispose();
                Close();
                metaprofiler.StopTimer(handleFill);
            }
            
            metaprofiler.StopTimer(handle);
            SetLastRead();
        }

        //private void RUN_SELECT_INTO_EMPTY_TABLE_2ndVersion(ref DataSet QuiteEmptyDataSet,
        //    string columnlist,
        //    string order_by,
        //    string filter,
        //    string TOP,
        //    string group_by,
        //    bool prepare) {

        //    DataTable EmptyTable = QuiteEmptyDataSet.Tables[0];
        //    if (columnlist == null) columnlist = QueryCreator.ColumnNameList(EmptyTable);

        //    filter = Security.quotedCompile(filter);

        //    if (openError) return;

        //    string tablename = EmptyTable.TableName;
        //    int handle = metaprofiler.StartTimer("RUN_SELECT_INTO_EMPTY_TABLE_2ndVersion(" + tablename + ")");
        //    SqlCommand Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());

        //    string SelCmd = "SELECT ";
        //    if (TOP != null) SelCmd += " TOP " + TOP + " ";

        //    List<int> colNum = new List<int>();
        //    if (columnlist == "*") {
        //        string newColList = null;
        //        for (int i = 0; i < EmptyTable.Columns.Count; i++) {
        //            DataColumn C = EmptyTable.Columns[i];
        //            if (!QueryCreator.IsRealColumn(C)) continue;
        //            if (newColList != null)
        //                newColList += ",";
        //            else
        //                newColList = "";
        //            newColList += EmptyTable.Columns[i].ColumnName;
        //            colNum.Add(i);
        //        }
        //        columnlist = newColList;
        //    }
        //    else {
        //        string[] colNames = columnlist.Split(',');
        //        foreach (string s in colNames) {
        //            for (int i = EmptyTable.Columns.Count - 1; i >= 0; i--) {
        //                if (EmptyTable.Columns[i].ColumnName == s) {
        //                    colNum.Add(i);
        //                    break;
        //                }
        //            }
        //        }
        //        if (colNames.Length < colNum.Count) {
        //            throw new Exception("Problemi nella lettura della tabella " + tablename);
        //        }
        //    }
        //    SelCmd += columnlist;
        //    SelCmd += " FROM " + GetCentralizedTableName(tablename);

        //    Cmd.CommandText = SelCmd;
        //    Cmd.CommandTimeout = 600;

        //    if (filter != null && filter != "") {
        //        if (prepare && PrepareEnabled) {
        //            Cmd.CommandText += " WHERE ";
        //            AddWhereClauses(ref Cmd, filter, tablename);
        //        }
        //        else {
        //            Cmd.CommandText += " WHERE " + filter + " ";
        //        }
        //    }

        //    if (group_by != null) {
        //        Cmd.CommandText += " GROUP BY " + group_by;
        //    }

        //    if (order_by != null) {
        //        Cmd.CommandText += " ORDER BY " + order_by;
        //    }

        //    Open();
        //    if (openError) {
        //        metaprofiler.StopTimer(handle);
        //        return;
        //    }

        //    try {
        //        if (prepare && PrepareEnabled) Cmd = GetPreparedCommand(Cmd);
        //    }
        //    catch (Exception E) {
        //        MarkException("RUN_SELECT_INTO_EMPTY_TABLE_2ndVersion: Error preparing " + Cmd.CommandText.ToString(), E);
        //    }
        //    SqlDataReader rdr = Cmd.ExecuteReader();


        //    int handleFill = 0;
        //    try {
        //        handleFill = metaprofiler.StartTimer("DA.Fill(" + EmptyTable.TableName + ")");
        //        EmptyTable.BeginLoadData();
        //        //DA.Fill(EmptyTable);
        //        while (rdr.Read()) {
        //            DataRow dataRow = EmptyTable.NewRow();
        //            for (int i = colNum.Count - 1; i >= 0; i--) {
        //                dataRow[colNum[i]] = rdr[i];
        //            }
        //            EmptyTable.Rows.Add(dataRow);
        //            dataRow.AcceptChanges();
        //        }

        //        EmptyTable.EndLoadData();
        //    }
        //    catch (SqlException SqlE) {
        //        if (SqlE.Class >= 20) {
        //            ConnectionHasBeenClosedBySystem = true;
        //            openError = true;
        //            MarkException("RUN_SELECT_INTO_EMPTY_TABLE_2ndVersion: Connection truncated. Running " +
        //                Cmd.CommandText.ToString(), SqlE);

        //        }
        //        else {
        //            MarkException("RUN_SELECT_INTO_EMPTY_TABLE_2ndVersion: Error running " +
        //                Cmd.CommandText.ToString(), SqlE);
        //        }

        //    }
        //    catch (Exception E) {

        //        MarkException("RUN_SELECT_INTO_EMPTY_TABLE_2ndVersion: Error running " + Cmd.CommandText.ToString(), E);
        //    }
        //    finally {
        //        Close();
        //        metaprofiler.StopTimer(handleFill);
        //    }
        //    rdr.Dispose();
        //    Cmd.Dispose();
        //    metaprofiler.StopTimer(handle);
        //    SetLastRead();
        //}

        /// <summary>
        /// Executes a SELECT COUNT on a table.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual int count(string tablename, MetaExpression filter) {
            return RUN_SELECT_COUNT(tablename, filter?.toSql(QHS, this), false);
        }

        /// <summary>
        /// Executes a SELECT COUNT on a table.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="filter">condition to apply</param>
        /// <param name="prepare">when true, command has to be prepared</param>
        /// <returns></returns>
        public virtual int RUN_SELECT_COUNT(string tablename, string filter, bool prepare) {
            if (openError) return 0;
            SqlCommand Cmd = new SqlCommand("", MySqlConnection, CurrTransaction());

            string SelCmd = "SELECT COUNT(*)";
            SelCmd += " FROM " + GetCentralizedTableName(tablename);
            Cmd.CommandText = SelCmd;
            filter = Security.quotedCompile(filter);

            if (filter != null) filter = filter.Trim();
            if ((filter != null) && (filter != "")) {
                if (PrepareEnabled && prepare) {
                    Cmd.CommandText += " WHERE ";
                    AddWhereClauses(ref Cmd, filter, tablename);
                }
                else {
                    Cmd.CommandText += " WHERE " + filter + " ";
                }
            }

            Open();
            if (openError) {
                return 0;
            }

            try {
                if (PrepareEnabled && prepare) Cmd = GetPreparedCommand(Cmd);
            }
            catch (Exception E) {
                MarkException("RUN_SELECT_COUNT: Error preparing " + Cmd.CommandText.ToString(), E);
            }

            try {
	            int ResultCount = Convert.ToInt32(Cmd.ExecuteScalar());
	            Close();
	            SetLastRead();
	            return ResultCount;
            }
            catch (Exception e) {
	            MarkException(
		            "RUN_SELECT_COUNT :Error running command" + Cmd.CommandText, e);
	            Close();
	            return 0;
            }
            finally {
                Cmd.Dispose();
            }
        }
        #endregion


        #region Enumerator

        /// <summary>
        /// Reads data row by row
        /// </summary>
        public sealed class DataRowReader : IDisposable,  IEnumerator {
            bool disposed = false;
            SqlDataReader SDR = null;
            string[] cols = null;
            DataRow Curr = null;
            SqlCommand Cmd;
            DataTable T;
            IDataAccess Conn;
            void GetRow() {
                Curr = T.NewRow();
                for (int i = 0; i < cols.Length; i++) {
                    Curr[cols[i]] = SDR.GetValue(i);
                }
            }


            void init(IDataAccess Conn, string table, string columnlist,
                        string order_by, string filter) {
                T = Conn.CreateTableByName(table, "*");
                if (columnlist == null || columnlist == "*") {
                    columnlist = QueryCreator.ColumnNameList(T);
                }
                filter = Conn.Security.quotedCompile(filter);


                Cmd = new SqlCommand("", Conn.sqlConnection, Conn.CurrTransaction());
                //Cmd.Connection= MySqlConnection;
                //if (sys["Transaction"]!=null) Cmd.Transaction= (SqlTransaction)	sys["Transaction"];

                string SelCmd = "SELECT ";
                SelCmd += columnlist;
                SelCmd += " FROM " + Conn.GetCentralizedTableName(table);

                Cmd.CommandText = SelCmd;

                if (filter != null) {
                    Cmd.CommandText += " WHERE " + filter + " ";
                }

                if (order_by != null) {
                    Cmd.CommandText += " ORDER BY " + order_by;
                }

                Conn.Open();
                if (Conn.openError) {
                    return;
                }

                SDR = Cmd.ExecuteReader();
                cols = columnlist.Split(',');

                this.Conn = Conn;
            }


            /// <summary>
            /// Creates the iterator
            /// </summary>
            /// <param name="Conn"></param>
            /// <param name="table"></param>
            /// <param name="columnlist"></param>
            /// <param name="order_by"></param>
            /// <param name="filter"></param>
            public  DataRowReader(IDataAccess Conn, string table, string columnlist,
                        string order_by, string filter) {
                init(Conn, table, columnlist, order_by, filter);

            }

            /// <summary>
            ///  Creates the iterator
            /// </summary>
            /// <param name="Conn"></param>
            /// <param name="table"></param>
            /// <param name="columnlist"></param>
            /// <param name="order_by"></param>
            /// <param name="filter"></param>
            public DataRowReader(IDataAccess Conn, string table, MetaExpression filter,
                    string columnlist=null,string order_by=null ) {
                init(Conn, table, columnlist, order_by, filter?.toSql(Conn.GetQueryHelper(),Conn));

            }
            object IEnumerator.Current => Curr;

            /// <summary>
            /// Necessary method to implement the iteratorinterface
            /// </summary>
            /// <returns></returns>
            public bool MoveNext() {
                if (SDR == null) return false;
                if (!SDR.Read()) return false;
                GetRow();
                return true;
            }

            /// <summary>
            /// Restars the iterator 
            /// </summary>
            public void Reset() {
                SDR?.Dispose();
                SDR = Cmd.ExecuteReader();
            }

            /// <summary>
            /// Disposes the iterator
            /// </summary>
            /// <param name="disposing"></param>
            public void Dispose(bool disposing) {
                if (!disposed) {
                    if (disposing) {
                        SDR?.Dispose();
                        Cmd?.Dispose();
                        Conn?.Close();
                    }
                    // Free your own state (unmanaged objects).
                    // Set large fields to null.
                    disposed = true;
                }
            }

            /// <summary>
            /// Disposes the iterator
            /// </summary>
            public void Dispose() {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Necessary method to implement the iteratorinterface
            /// </summary>
            /// <returns></returns>
            public IEnumerator GetEnumerator() {
                return (IEnumerator)this;
            }

            /// <summary>
            /// public destructor
            /// </summary>
            ~DataRowReader() {
                // Simply call Dispose(false).
                Dispose(false);
            }

        }


        #endregion

        /// <summary>
        /// Run a non query command string and get result asynchronously
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="timeOut">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public async Task<object> executeQueryValue (string cmd, int timeOut=-1) {
            if (openError) throw new Exception("Connection is closed");
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    throw new Exception( "La transazione corrente non è più valida");
                }
            }

            await openAsync();
            if (timeOut < 0) timeOut = defaultTimeOut;
            object result = null;

            using (var sqlCmd = new SqlCommand(cmd, MySqlConnection, currTran) {CommandTimeout = timeOut}) {
	            var read = await sqlCmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
	            
	            try {
		            if (read.HasRows) {
			            read.Read();
			            result = read[0];
			            SetLastRead();

		            }
	            }
	            catch (Exception e) {
		            MarkException("executeQueryValue: Error running " + cmd, e);
		            return null;
	            }
	            read.Close();
            }
            Close();

            if (currTran != null) {
                if (currTran.Connection == null) {
                    myLastError= "Il comando " + cmd + " ha invalidato la transazione";
                    return null;
                }
            }
            return result;          
        }

        /// <summary>
        /// Returns a value executing a generic sql command 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="timeOut">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public virtual async Task<int> executeNonQuery(string cmd, int timeOut = -1) {
	        checkValidConnection();
	        if (openError) throw new Exception("Connection is closed");
	        await openAsync();
	        if (timeOut < 0) timeOut = defaultTimeOut;
	        var currTran = CurrTransaction();
	        using (var sqlCmd = new SqlCommand(cmd, MySqlConnection, currTran) {CommandTimeout = timeOut}) {
		        int result;
		        try {
			        result = await sqlCmd.ExecuteNonQueryAsync();
		        }
		        catch (Exception e) {
			        MarkException("executeNonQuery: Error running " + cmd, e);
			        throw;
		        }

		        Close();

		        if (currTran != null) {
			        if (currTran.Connection == null) {
				        myLastError = "Il comando " + cmd + " ha invalidato la transazione";
				        throw new Exception(myLastError);
			        }
		        }

		        return result;
	        }
        }

        /// <summary>
        /// Run a command string and get result asynchronously
        /// </summary>
        /// <remarks>This method </remarks>
        /// <param name="selList"></param>
        /// <param name="packetSize"></param>
        /// <param name="timeout">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public void executeSelectBuilderCallback(List<SelectBuilder> selList, int packetSize,Action<SelectBuilder ,Dictionary<string, object> > callback, int timeout=-1) {
	         if (openError) return;
	         Open();
	         if (openError) {
		         return;
	         }

            var SelCmd = new StringBuilder();
             bool first = true;
             string multitab = "";
             DataSet D = null;
             if (timeout <0) timeout = defaultTimeOut;

             foreach (var Sel in selList) {
                 string filter = Security.quotedCompile(Sel.filter);
                 if (!first) SelCmd.Append(";");
                 SelCmd.Append("SELECT ");
                 if (Sel.TOP != null) SelCmd.Append(" TOP " + Sel.TOP + " ");
                 SelCmd.Append(Sel.columnlist);

                 SelCmd.Append(" FROM " + Sel.tablename);
                 multitab += Sel.tablename + " ";

                 string filtersec = filter;
                 if (Sel.DestTable == null ||
                     model.isSkipSecurity(Sel.DestTable) == false) {
                     filtersec = QHS.AppAnd(filter,
                         Security.SelectCondition(Sel.tablename, true));
                 }
                 if (filtersec != null && filtersec.Trim() != "") {
                     SelCmd.Append(" WHERE " + filtersec + " ");
                 }

                 if (Sel.order_by != null) {
                     SelCmd.Append(" ORDER BY " + Sel.order_by);
                 }

                 if (Sel.DestTable != null && D == null) {
                     D = Sel.DestTable.DataSet;
                 }
                 first = false;
             }
             ClearDataSet.RemoveConstraints(D);
             if (timeout < 0) timeout = 60;



            using (var command = new SqlCommand(SelCmd.ToString(), sqlConnection,CurrTransaction()) {CommandTimeout=timeout}) {       
	            var DA = new SqlDataAdapter(command);
                DA.SelectCommand.Transaction = command.Transaction;
                int currTable = 0;

                using (var reader =  command.ExecuteReader(CommandBehavior.Default)) {
                    do {
                        var selBuilder = selList[currTable];
                        var table = selBuilder.DestTable;
                        var colNum = new List<int>();
                        for (int i = 0; i < table.Columns.Count; i++) {
	                        var C = table.Columns[i];
                            if (!QueryCreator.IsRealColumn(C)) continue;
                            colNum.Add(i);
                        }
                        
                        var res = new Dictionary<string, object> {["table"] = table};
                        var localRows = new List<DataRow>();
                        if (packetSize != 0) {
                            callback(selBuilder,res); //invia lo "start" su questa tabella
                            //res = new Dictionary<string, object>();
                        }

                        res["rows"] = localRows;
                        var record = (IDataRecord) reader;
                        while (reader.Read()) {
	                        var dataRow = table.NewRow();
                            for (int i = colNum.Count - 1; i >= 0; i--) {
                                dataRow[colNum[i]] = reader[i];
                            }
                            localRows.Add(dataRow);
                            if (packetSize == 0 || localRows.Count != packetSize) continue;
                            callback(selBuilder,res);  //se è arrivato al limite del pacchetto invia le righe
                            localRows = new List<DataRow>();
                            res = new Dictionary<string, object> {["rows"] = localRows,["table"]=table};
                        }

                        if (localRows.Count > 0) {
                            callback(selBuilder,res); //se packetsize==0 include sia "meta" che "rows" altrimenti solo "rows"
                        }

                        currTable++;
                    } while (reader.NextResult());
					reader?.Dispose();
                }
                DA.Dispose();
              
            }
            callback(null,new Dictionary<string, object> {["resolve"] = 1});//alla fine invia sempre un "resolve"
            Close();
        }

        /// <summary>
        /// Executes a sql command and returns a DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="timeout">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public async Task<DataTable> executeQuery(string sql, int timeout = -1) {
            checkValidConnection();
            if (sql == null) return null;
            var currTran = CurrTransaction();
            var cmd = new SqlCommand(sql, MySqlConnection, currTran);
            if (timeout != -1) cmd.CommandTimeout = timeout;
            using (var MyDataAdapter = new SqlDataAdapter(cmd)) {
	            var T = new DataTable();
	            int handle = metaprofiler.StartTimer("executeQuery()");
	            try {
		            await openAsync();
		            if (openError) {
			            metaprofiler.StopTimer(handle);
			            myLastError = "Errore aprendo la connessione";
			            return null;
		            }

		            await Task.Run(() => MyDataAdapter.Fill(T));
		            Close();
		            MyDataAdapter.Dispose();
		            SetLastRead();
	            }
	            catch (Exception E) {
		            //if (command.Length>80000) command = command.Substring(0,79997)+"...";
		            myLastError = MarkException("SQLRunner: Error running " + sql, E);
		            Close();
		            metaprofiler.StopTimer(handle);
		            return null;
	            }

	            metaprofiler.StopTimer(handle);
	            if (currTran != null) {
		            if (currTran.Connection == null) {
			            myLastError = "Il comando " + sql + " ha invalidato la transazione";
			            return null;
		            }
	            }

	            return T;
            }
        }
        /// <summary>
        /// Run a command string and get result asynchronously
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="packetSize"></param>
        /// <param name="timeout">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async Task executeQueryTablesCallback(string commandString, int packetSize, int timeout,Func<object, Task> callback) {
            checkValidConnection();
            await openAsync();

            using (var command = new SqlCommand(commandString, sqlConnection,CurrTransaction()) {CommandTimeout=timeout}) {                
                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.Default)) {
                    int nSet = 0;
                    do {
                        var fieldNames = new object[reader.FieldCount];
                        for (var i = 0; i < reader.FieldCount; i++) {
                            fieldNames[i] = reader.GetName(i);
                        }
                        
                        var res = new Dictionary<string, object> {["meta"] = fieldNames,["resultSet"]=nSet};
                        var localRows = new List<object[]>();
                        if (packetSize != 0) {
                            await callback(res); //invia "meta" separatamente, poi invierà le rows
                            res = new Dictionary<string, object>();
                        }

                        res["rows"] = localRows;
                        var record = (IDataRecord) reader;
                        while (reader.Read()) {
                            var resultRecord = new object[record.FieldCount];
                            record.GetValues(resultRecord);
                            localRows.Add(resultRecord);
                            if (packetSize == 0 || localRows.Count != packetSize) continue;
                            await callback(res);  //se è arrivato al limite del pacchetto invia le righe
                            localRows = new List<object[]>();
                            res = new Dictionary<string, object> {["rows"] = localRows};
                        }

                        if (localRows.Count > 0) {
                            await callback(res); //se packetsize==0 include sia "meta" che "rows" altrimenti solo "rows"
                        }

                        nSet++;
                    } while (await reader.NextResultAsync());

                }
                Close();
            }
            await callback(new Dictionary<string, object> {["resolve"] = 1});//alla fine invia sempre un "resolve"
        }

        async Task executeQueryIntoEmptyTable(DataTable table,string columnList, object filter=null,string top=null, int timeout=-1) {
            checkValidConnection();
            filter = compile(filter);
            string tablename = table.TableName;

            string SelCmd = "SELECT ";
            if (top != null) SelCmd += " TOP " + top + " ";

            var colNum = new List<int>();
            if (columnList == "*" || columnList==null) {
                string newColList = null;
                for (int i = 0; i < table.Columns.Count; i++) {
	                var C = table.Columns[i];
                    if (!QueryCreator.IsRealColumn(C)) continue;
                    if (newColList != null)
                        newColList += ",";
                    else
                        newColList = "";
                    newColList += table.Columns[i].ColumnName;
                    colNum.Add(i);
                }
                columnList = newColList;
            }
            else {
                string[] colNames = columnList.Split(',');
                foreach (string s in colNames) {
                    for (int i = table.Columns.Count - 1; i >= 0; i--) {
                        if (table.Columns[i].ColumnName == s) {
                            colNum.Add(i);
                            break;
                        }
                    }
                }
                if (colNames.Length < colNum.Count) {
                    throw new Exception("Problemi nella lettura della tabella " + tablename);
                }
            }
            SelCmd += columnList;
            SelCmd += " FROM " + GetCentralizedTableName(tablename);
            if (filter != null) {
                SelCmd += " WHERE " + filter;
            }
            await openAsync();
            if (timeout < 0) timeout = 60;

            using (var command = new SqlCommand(SelCmd, sqlConnection, CurrTransaction()) {CommandTimeout = timeout}) {
                try {
                    var rdr = await command.ExecuteReaderAsync();
                    table.BeginLoadData();
                    while (await rdr.ReadAsync()) {
                        DataRow dataRow = table.NewRow();
                        for (int i = colNum.Count - 1; i >= 0; i--) {
                            dataRow[colNum[i]] = rdr[i];
                        }

                        table.Rows.Add(dataRow);
                        dataRow.AcceptChanges();
                    }
                    table.EndLoadData();
                    rdr.Dispose();
                }
                catch (SqlException sqlE) {
                    if (sqlE.Class >= 20) {
                        ConnectionHasBeenClosedBySystem = true;
                        openError = true;
                        MarkException($"executeQueryIntoEmptyTable: Connection truncated. Running {command.CommandText}", sqlE);
                    }
                    else {
                        MarkException($"executeQueryIntoEmptyTable: Error running {command.CommandText}", sqlE);
                    }
                }
                catch (Exception e) {
                    MarkException($"RUN_SELECT_INTO_EMPTY_TABLE: Error running {command.CommandText}", e);
                }
                finally {                  
                    Close();
                }                
            }
        }

        void checkValidConnection() {
            if (openError) throw new Exception("Connection is closed");
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    throw new Exception("La transazione corrente non è più valida");
                }
            }

        }

        /// <summary>
        /// creates a string filter from a generi filter (string o MetaExpression)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string compile(object filter) {
            if (filter == null) return null;
            if (filter == DBNull.Value) return null;
            if (filter is string) {
                return  Security.Compile(filter as string, true);                
            }
            if (filter is MetaExpression) return ((MetaExpression) filter).toSql(QHS, this);
            throw new ArgumentException("Filter must be a string or a MetaExpression");
        }

        /// <summary>
        /// Reads data into a table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="filter"></param>
        /// <param name="timeout">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public virtual async Task executeQueryIntoTable(DataTable table, object filter,int timeout=-1) {

            checkValidConnection();
            string columlist = QueryCreator.ColumnNameList(table);

            try {
                string filtersec = compile(filter);
                if ( !model.isSkipSecurity(table)) {
                    filtersec = QHS.AppAnd(filtersec, security.SelectCondition(GetTableForReading(table), true));
                }

                DataTable data = await executeQueryTable(table.TableName, columlist, filtersec, timeout);
                QueryCreator.CheckKey(table, ref data);
                data.Namespace = table.Namespace;
                QueryCreator.MergeDataTable(table, data);
            }
            catch (Exception e) {
                ErrorLogger.Logger.markException(e,"Task executeQueryIntoTable(" + table.TableName + ")");
            }
            
        }

        /// <summary>
        /// Reads data from a table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columnList"></param>
        /// <param name="filter"></param>
        /// <param name="timeout">timeout in seconds, 0 means no timeout, -1 means default timeout</param>
        /// <returns></returns>
        public virtual async Task<DataTable> executeQueryTable(string tableName, string columnList, object filter, int timeout=-1) {
            if (openError) throw new Exception("Connection is closed");
            var currTran = CurrTransaction();
            if (currTran != null) {
                if (currTran.Connection == null) {
                    throw new Exception("La transazione corrente non è più valida");
                }
            }

            await openAsync();
            var T = await createTableByNameAsync(tableName, columnList,false);
            var MyDS = new DataSet("dummy");
            MyDS.Tables.Add(T);
            ClearDataSet.RemoveConstraints(MyDS);
            await executeQueryIntoEmptyTable(T, compile(filter));
            MyDS.Tables.Remove(T);
            return T;
        }

        /// <summary>
        /// Logs an error to the remote logger
        /// </summary>
        /// <param name="errmsg"></param>
        /// <param name="E"></param>
        public virtual void LogError(string errmsg, Exception E) {
            errorLogger.logException(errmsg, security:Security, exception:E,dataAccess:this);
           
        }

        /// <summary>
        /// Insert a datarow in a table preserving it's state
        /// </summary>
        /// <param name="t"></param>
        /// <param name="r"></param>
        public static void safeImportRow(DataTable t, DataRow r) {
	        var newR = t.NewRow();
            int nCol = t.Columns.Count;
            switch (r.RowState) {
                case DataRowState.Unchanged:
                    for (int i = nCol - 1; i >= 0; i--) {
                        newR[i] = r[i];
                    }
                    t.Rows.Add(newR);
                    newR.AcceptChanges();
                    return;
                case DataRowState.Added:
                    for (int i = nCol - 1; i >= 0; i--) {
                        newR[i] = r[i];
                    }
                    t.Rows.Add(newR);
                    return;
                case DataRowState.Modified:
                    for (var i = nCol - 1; i >= 0; i--) {
                        newR[i] = r[i, DataRowVersion.Original];
                    }

                    t.Rows.Add(newR);
                    newR.AcceptChanges();
                    for (int i = nCol - 1; i >= 0; i--) {
                        newR[i] = r[i];
                    }
                    return;
                case DataRowState.Deleted:
                    for (var i = nCol - 1; i >= 0; i--) {
                        newR[i] = r[i, DataRowVersion.Original];
                    }
                    t.Rows.Add(newR);
                    newR.AcceptChanges();
                    newR.Delete();
                    return;
            }
        }



      
      


        /// <summary>
        /// Builds an UPDATE sql command
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="columns">column names</param>
        /// <param name="values">values to be set</param>
        /// <returns>Error msg or null if OK</returns>
        public virtual string getUpdateCommand(string table, MetaExpression condition, List<string> columns, List<object> values) {
            string UpdateCmd = "UPDATE " + GetCentralizedTableName(table) + " SET ";
            string outstring = "";
            bool first = true;
            for (int i = 0; i < columns.Count; i++) {
                if (first)
                    first = false;
                else
                    outstring += ",";
                outstring += columns[i] + "=" + QHS.quote(values[i]);
            }
            UpdateCmd += outstring;

            if (condition != null && !condition.isTrue()) UpdateCmd += " WHERE " + compile(condition);
            return UpdateCmd;
        }

        /// <summary>
        /// Returns the queryhelper attached to this kind of DataAccess
        /// </summary>
        /// <returns></returns>
        public virtual QueryHelper GetQueryHelper() {
            return new SqlServerQueryHelper();
        }

        /// <summary>
        /// Executes an update on db
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="columns"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public string doUpdate(string table, MetaExpression condition, string[] columns, object[] values) {
            string []strValues = (from object v in values select QHS.quote(v)).ToArray();
            return DO_UPDATE(table, compile(condition), columns, strValues, columns.Length);
        }

        /// <summary>
        /// Executes an update on db
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <param name="fieldValues"></param>
        /// <returns></returns>
        public string doUpdate(string table, MetaExpression condition, Dictionary<string, object> fieldValues) {
            string []fields = (from string k in fieldValues.Keys select k).ToArray();
            string []strValues = (from string k in fieldValues.Keys select QHS.quote(fieldValues[k])).ToArray();
            return DO_UPDATE(table, compile(condition), fields, strValues, fields.Length);
        }

        /// <summary>
        /// Executes an update on db
        /// </summary>
        /// <param name="r">row containing values to be taken</param>
        /// <param name="condition"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public string doUpdate( DataRow r, MetaExpression condition=null, string[] fields = null) {
            if (fields == null) {
                var ff = new List<string>();
                if (r.RowState == DataRowState.Modified) {
                    foreach (DataColumn c in r.Table.Columns) {
                        if (r[c].Equals(r[c,DataRowVersion.Original]))continue;
                        ff.Add(c.ColumnName);
                    }
                }
                else {
                    foreach (DataColumn c in r.Table.Columns) {
                        ff.Add(c.ColumnName);
                    }
                }

                fields = ff.ToArray();
            }

            if (condition == null) condition = MetaExpression.keyCmp(r);            
            object []values = (from string field in fields select QHS.quote(r[field])).ToArray();
            return doUpdate(r.Table.tableForPosting(), condition, fields, values);
        }
    }

    /// <summary>
    /// Compare an ordered set of field to an ordered set of values
    /// </summary>
    public class MultiCompare {
        /// <summary>
        /// Values to compare with the fields
        /// </summary>
        public object[] values;

        /// <summary>
        /// Fields to compare
        /// </summary>
        public string[] fields;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        public MultiCompare(string[] fields, object[] values) {
            this.values = values;
            this.fields = fields;
        }

        /// <summary>
        /// Check if the fields of this comparator are the same of the specified one
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public bool SameFieldsAs(MultiCompare C) {
            if (C.fields.Length != this.fields.Length) return false;
            for (int i = 0; i < C.fields.Length; i++) {
                if (fields[i] != C.fields[i]) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Class for creating optimized queries
    /// </summary>
    public class OptimizedMultiCompare {
        /// <summary>
        /// List of fields to be compared
        /// </summary>
        public string[] fields;

        /// <summary>
        /// List of values to compare with  the multivalue field
        /// </summary>
        public List<object>[] values;

        /// <summary>
        /// Position of the only field that can differ in a join between two OptimizedMulticompare
        /// </summary>
        int multival_pos = -1;

        /// <summary>
        /// True when there is a field to compare with a set of values
        /// </summary>
        /// <returns></returns>
        public bool IsMultivalue() {
            return (multival_pos != -1);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="C"></param>
        public OptimizedMultiCompare(MultiCompare C) {
            this.fields = C.fields;
            this.values = new List<object>[C.values.Length];
            for (int i = 0; i < C.values.Length; i++) {
                values[i] = new List<object> {C.values[i]};
            }
        }


        /// <summary>
        /// Return true if this Compare operates on the same fields as the specified one
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public bool SameFieldsAs(OptimizedMultiCompare C) {
            if (C.fields.Length != this.fields.Length) return false;
            for (int i = 0; i < C.fields.Length; i++) {
                if (fields[i] != C.fields[i]) return false;
            }
            return true;
        }

        bool HaveValue(object O, int pos) {
            foreach (object v in values[pos]) {
                if (v.Equals(O)) return true;
            }
            return false;
        }

        /// <summary>
        /// Join this Multicompare with another one, return false  if it is not possible
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        public bool JoinWith(OptimizedMultiCompare C) {
            if (!SameFieldsAs(C)) return false;
            if (C.IsMultivalue()) return false;

            int pos_diff = -1; //posizione della differenza trovata
            if (multival_pos == -1) {
                //verifica che ci sia al massimo una differenza
                for (int i = 0; i < fields.Length; i++) {
                    if (!HaveValue(C.values[i][0], i)) {
                        if (pos_diff != -1) return false; //più di una differenza trovata
                        pos_diff = i;
                        continue;
                    }
                }
            }
            else {
                //verifica che ci sia al massimo una differenza e che sia in multival_pos
                for (int i = 0; i < fields.Length; i++) {
                    if (!HaveValue(C.values[i][0], i)) {
                        if (i != multival_pos) return false; //più di una differenza trovata
                        pos_diff = i;
                        continue;
                    }
                }
            }
            if (pos_diff == -1) return true;
            values[pos_diff].Add(C.values[pos_diff][0]);
            multival_pos = pos_diff;
            return true;
        }

        /// <summary>
        /// Gets the optimized filter to obtain rows 
        /// </summary>
        /// <param name="QH"></param>
        /// <returns></returns>
        public string GetFilter(QueryHelper QH) {
            string filter = "";
            for (int i = 0; i < fields.Length; i++) {
                if (values[i].Count == 1) {
                    filter = QH.AppAnd(filter, QH.CmpEq(fields[i], values[i][0]));
                }
                else {
                    filter = QH.AppAnd(filter, QH.FieldIn(fields[i], values[i].ToArray()));
                }
            }
            return filter;
        }

    }


    //string tablename, 
    //        string columnlist,
    //        string order_by, 
    //        string filter, 
    //        string TOP,
    //        string group_by


    /// <summary>
    /// Manage the construction of a sql - select command
    /// </summary>
    public interface ISelectBuilder {
        /// <summary>
        /// Overall filter to be used in the select command
        /// </summary>
        string filter { get; }

        /// <summary>
        /// Adds an AfterRead delegate to be called in a specified context
        /// </summary>
        /// <param name="Fun"></param>
        /// <param name="Context"></param>
        void AddOnRead(SelectBuilder.AfterReadDelegate Fun, object Context);

        /// <summary>
        /// Method to be invoked after data have been retrived from db
        /// </summary>
        void OnRead();

        /// <summary>
        /// Specify a filter for the query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        SelectBuilder Where(string filter);

        /// <summary>
        /// Specify a MultiCompare as filter
        /// </summary>
        /// <param name="MC"></param>
        /// <returns></returns>
        SelectBuilder MultiCompare(MultiCompare MC);

        /// <summary>
        /// Specify the table to be read
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        SelectBuilder From(string tablename);

        /// <summary>
        /// Specify the sorting order
        /// </summary>
        /// <param name="order_by"></param>
        /// <returns></returns>
        SelectBuilder OrderBy(string order_by);

        /// <summary>
        /// Specify the TOP clause of the select command
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        SelectBuilder Top(string top);

        /// <summary>
        /// Specify the groupBy clause
        /// </summary>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        SelectBuilder GroupBy(string groupBy);

        /// <summary>
        /// Specify the destination table for reading data
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        SelectBuilder IntoTable(DataTable T);

        /// <summary>
        /// Check if this select can be added to the specified one
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        bool CanAppendTo(SelectBuilder S);

        /// <summary>
        /// Check if this select is optimized (so can be joined to other)
        /// </summary>
        /// <returns></returns>
        bool isOptimized();

        /// <summary>
        /// Merge this select the specified one in an optimized way, return false if it was not possibile
        /// </summary>
        /// <param name="S"></param>
        /// <param name="QH"></param>
        /// <returns></returns>
        bool OptimizedAppendTo(SelectBuilder S, QueryHelper QH);

        /// <summary>
        /// Append this select to another one as a separate command to be executed (not-optimized)
        /// </summary>
        /// <param name="S"></param>
        /// <param name="QH"></param>
        void AppendTo(SelectBuilder S, QueryHelper QH);
    }

    /// <summary>
    /// Manage the construction of a sql - select command
    /// </summary>
    public class SelectBuilder : ISelectBuilder {
        internal string tablename = null;
        internal string columnlist = null;
        internal string order_by = null;
        private string myfilter = null;
        internal string TOP = null;
        internal string group_by = null;

        /// <summary>
        /// Table where rows will be read into
        /// </summary>
        public DataTable DestTable;
        internal string tablemap = null;
        internal int count = 1;
        internal OptimizedMultiCompare OMC = null;

        /// <summary>
        /// Delegate kind to be called after the table is read
        /// </summary>
        /// <param name="T"></param>
        /// <param name="Context"></param>
        public delegate void AfterReadDelegate(DataTable T, object Context);

        private object Context = null;
        private AfterReadDelegate myOnRead = null;

        /// <summary>
        /// Overall filter to be used in the select command
        /// </summary>
        public string filter
        {
            get
            {
                if (myfilter != null) return myfilter;
                if (OMC != null) return GetData.MergeFilters(OMC.GetFilter(myQH), DestTable);
                return null;
            }

        }

        /// <summary>
        /// Adds an AfterRead delegate to be called in a specified context
        /// </summary>
        /// <param name="Fun"></param>
        /// <param name="Context"></param>
        public virtual void AddOnRead(AfterReadDelegate Fun, object Context) {
            this.Context = Context;
            myOnRead += Fun;
        }

        /// <summary>
        /// Method to be invoked after data have been retrived from db
        /// </summary>
        public virtual void OnRead() {
            if (myOnRead == null) return;
            int handle = metaprofiler.StartTimer("OnRead()");
            myOnRead(DestTable, Context);
            metaprofiler.StopTimer(handle);
        }

        /// <summary>
        /// Constructor for reading specified columns
        /// </summary>
        /// <param name="columnlist"></param>
        public SelectBuilder(string columnlist) {
            this.columnlist = columnlist;
        }

        /// <summary>
        /// Constructor for reading all columns
        /// </summary>
        public SelectBuilder() {
            this.columnlist = "*";
        }

        /// <summary>
        /// Specify a filter for the query
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual SelectBuilder Where(string filter) {
            this.myfilter = filter;
            return this;
        }

        /// <summary>
        /// Specify a MultiCompare as filter
        /// </summary>
        /// <param name="MC"></param>
        /// <returns></returns>
        public virtual SelectBuilder MultiCompare(MultiCompare MC) {
            this.OMC = new OptimizedMultiCompare(MC);
            return this;
        }



        /// <summary>
        /// Specify the table to be read
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public virtual SelectBuilder From(string tablename) {
            this.tablename = tablename;
            if (this.tablemap == null) {
                this.tablemap = tablename;
            }
            return this;
        }

        /// <summary>
        /// Specify the sorting order
        /// </summary>
        /// <param name="order_by"></param>
        /// <returns></returns>
        public virtual SelectBuilder OrderBy(string order_by) {
            this.order_by = order_by;
            return this;
        }

        /// <summary>
        /// Specify the TOP clause of the select command
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        public virtual SelectBuilder Top(string top) {
            this.TOP = top;
            return this;
        }

        /// <summary>
        /// Specify the groupBy clause
        /// </summary>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        public virtual SelectBuilder GroupBy(string groupBy) {
            this.group_by = groupBy;
            return this;
        }

        /// <summary>
        /// Specify the destination table for reading data
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public virtual SelectBuilder IntoTable(DataTable T) {
            this.DestTable = T;
            columnlist = QueryCreator.ColumnNameList(T);
            tablename = DataAccess.GetTableForReading(T);
            tablemap = T.TableName;
            return this;
        }


        /// <summary>
        /// Check if this select can be added to the specified one
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public virtual bool CanAppendTo(SelectBuilder S) {
            if (this.tablename != S.tablename) return false;
            if (this.tablemap != S.tablemap) return false;
            if (this.group_by != null || S.group_by != null) return false;
            return true;
        }

        /// <summary>
        /// Check if this select is optimized (so can be joined to other)
        /// </summary>
        /// <returns></returns>
        public virtual bool isOptimized() {
            return (OMC != null);
        }
        QueryHelper myQH;

        /// <summary>
        /// Merge this select the specified one in an optimized way, return false if it was not possibile
        /// </summary>
        /// <param name="S"></param>
        /// <param name="QH"></param>
        /// <returns></returns>
        public virtual bool OptimizedAppendTo(SelectBuilder S, QueryHelper QH) {
            myQH = QH;
            if (OMC == null || S.OMC == null) return false;
            bool res = OMC.JoinWith(S.OMC);
            if (!res) return false;
            myfilter = null;
            return true;
        }



        /// <summary>
        /// Append this select to another one as a separate command to be executed (not-optimized)
        /// </summary>
        /// <param name="S"></param>
        /// <param name="QH"></param>
        public virtual void AppendTo(SelectBuilder S, QueryHelper QH) {
            if (this.filter == null) return;
            if (S.filter == null) {
                this.myfilter = null;
                return;
            }
            if (this.filter == S.filter) return;
            if (count == 1) {
                this.myfilter = QH.AppOr(QH.DoPar(this.filter), QH.DoPar(S.filter));
            }
            else {
                this.myfilter = QH.AppOr(QH.DoPar(this.filter), S.filter);
            }
            count++;
        }
    }


    /// <summary>
    /// Class used to manage system type conversions
    /// </summary>
    public class GetType_Util {

        /// <summary>
        /// Converts a system type name into a aystem type
        /// </summary>
        /// <param name="Stype">type name</param>
        /// <returns>corresponding system type</returns>
        public static Type GetSystemType_From_StringSystemType(string Stype) {
            switch (Stype) {
                case "System.Boolean": return typeof(bool);
                case "System.Byte": return typeof(byte);
                case "System.Char": return typeof(char);
                case "System.DateTime": return typeof(DateTime);
                case "System.DBNull": return typeof(DBNull);
                case "System.Decimal": return typeof(decimal);
                case "System.Double": return typeof(double);
                case "System.Int16": return typeof(short);
                case "System.Int32": return typeof(int);
                case "System.Int64": return typeof(long);
                case "System.Object": return typeof(object);
                case "System.SByte": return typeof(sbyte);
                case "System.Single": return typeof(float);
                case "System.String": return typeof(string);
                case "System.UInt16": return typeof(ushort);
                case "System.UInt32": return typeof(uint);
                case "System.UInt64": return typeof(ulong);
                default: return typeof(string);
            }
        }//Fine GetSystemType_From_StringSystemType

        static readonly Byte[] _BB = new byte[] { };
        /// <summary>
        /// Converts a SqlDBtype into a corresponding .net type suitable to store it.
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static System.Type GetSystemType_From_SqlDbType(string sqlDbType) {
            sqlDbType = sqlDbType.ToLower().Trim();

            switch (sqlDbType) {
                case "bigint": return typeof(long);
                case "bit": return typeof(bool);
                case "char": return typeof(string);
                case "date": return typeof(DateTime);
                case "datetime": return typeof(DateTime);
                case "decimal": return typeof(decimal);
                case "float": return typeof(double);
                case "int": return typeof(int);
                case "image": return _BB.GetType();
                case "binary": return _BB.GetType();
                case "varbinary": return _BB.GetType();
                case "money": return typeof(decimal);
                case "nvarchar": return typeof(string);
                case "real": return typeof(float);
                case "smalldatetime": return typeof(DateTime);
                case "smallint": return typeof(short);
                case "text": return typeof(string);
                case "timestamp": return _BB.GetType();
                case "tinyint": return typeof(byte);
                case "uniqueidentifier": return typeof(Guid);
                case "varchar": return typeof(string);
                case "variant": return typeof(object);
                default: return typeof(string);
            }
        }

       


        /// <summary>
        /// Gets a SQL-specific data type for use in an SQL parameter in order to
        ///  store a given dbtype
        /// </summary>
        /// <param name="sqlDbType"></param>
        /// <returns></returns>
        public static SqlDbType GetSqlType_From_StringSqlDbType(string sqlDbType) {
            sqlDbType = sqlDbType.ToLower().Trim();
            switch (sqlDbType) {
                case "bigint": return SqlDbType.BigInt;
                case "bit": return SqlDbType.Bit;
                case "char": return SqlDbType.Char;
                case "datetime": return SqlDbType.DateTime;
                case "decimal": return SqlDbType.Decimal;
                case "float": return SqlDbType.Float;
                case "int": return SqlDbType.Int;
                case "image": return SqlDbType.Image;
                case "money": return SqlDbType.Money;
                case "binary": return SqlDbType.Binary;
                case "nvarchar": return SqlDbType.NVarChar;
                case "real": return SqlDbType.Real;
                case "smalldatetime": return SqlDbType.SmallDateTime;
                case "date": return SqlDbType.Date;
                case "smallint": return SqlDbType.SmallInt;
                case "text": return SqlDbType.Text;
                case "timestamp": return SqlDbType.Timestamp;
                case "tinyint": return SqlDbType.TinyInt;
                case "uniqueidentifier": return SqlDbType.UniqueIdentifier;
                case "varchar": return SqlDbType.VarChar;
                case "varbinary": return SqlDbType.VarBinary;
                case "variant": return SqlDbType.Variant;
                default:
                    ErrorLogger.Logger.markEvent("DataAccess: Type " + sqlDbType + " was not found in switch().");
                    return SqlDbType.Text;
            }
        }


    }//Fine Classe




    /// <summary>
    /// Connection to database with current user = owner of schema
    /// </summary>
	public class AllLocal_DataAccess : DataAccess{

        /// <summary>
        /// Creates a connection to db with the UserDB being the schema name
        /// </summary>
        /// <param name="DSN"></param>
        /// <param name="Server"></param>
        /// <param name="Database"></param>
        /// <param name="UserDB">This must be the SCHEMA name</param>
        /// <param name="PasswordDB">Password for UserDB</param>
        /// <param name="User">Application user</param>
        /// <param name="Password">Password for application user</param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		public AllLocal_DataAccess(
            string DSN,
            string Server,
            string Database,
            string UserDB,
            string PasswordDB,
            string User,
            string Password,
            int esercizio_sessione,
            DateTime DataContabile)
            : base(DSN, Server, Database, UserDB, PasswordDB, User, Password, esercizio_sessione, DataContabile) {
        }

        /// <summary>
        ///  Creates a connection to db with the UserDB being the schema name
        /// </summary>
        /// <param name="DSN"></param>
        /// <param name="Server"></param>
        /// <param name="Database"></param>
        /// <param name="UserDB">This must be the SCHEMA name</param>
        /// <param name="PasswordDB">Password for UserDB</param>
        /// <param name="esercizio_sessione"></param>
        /// <param name="DataContabile"></param>
		public AllLocal_DataAccess(
            string DSN,
            string Server,
            string Database,
            string UserDB,
            string PasswordDB,
            int esercizio_sessione,
            DateTime DataContabile)
            : base(DSN, Server, Database, UserDB, PasswordDB, esercizio_sessione, DataContabile) {
        }

        /// <summary>
        /// Crea un duplicato di un DataAccess, con una nuova connessione allo stesso DB. 
        /// Utile se la connessione deve essere usata in un nuovo thread.
        /// </summary>
        /// <returns></returns>
        public override DataAccess Duplicate() {
            return new AllLocal_DataAccess(Security.GetSys("dsn").ToString(),
                Security.GetSys("server").ToString(),
                Security.GetSys("database").ToString(),
                Security.GetSys("userdb").ToString(),
                DefaultSecurity.decryptKey((byte[]) Security.GetSys("passworddb")),
                Security.GetSys("user").ToString(),
                DefaultSecurity.decryptKey((byte[]) Security.GetSys("password")),
                Security.GetEsercizio(),
                Security.GetDataContabile());
        }


        /// <summary>
        /// Check if tablename must be prefixed with DBO during access
        /// </summary>
        /// <param name="tablename"></param>
        /// <returns></returns>
		public override bool TableIsCentralized(string tablename) {
            return false;
        }

        /// <summary>
        /// Check if procname must be prefixed with DBO during access
        /// </summary>
        /// <param name="procname"></param>
        /// <returns></returns>
		public override bool ProcedureIsCentralized(string procname) {
            return false;
        }


       

    }



    


}

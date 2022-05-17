using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static mdl_utils.metaprofiler;
#pragma warning disable IDE1006 // Naming Styles


namespace mdl {

    /// <summary>
    /// Manages security conditions and environment variables
    /// </summary>
    public interface ISecurity {

         /// <summary>
         /// Gets the condition on a specific operation on a table
         /// </summary>
         /// <param name="T"></param>
         /// <param name="opkind_IUDSP"></param>
         /// <returns></returns>
         string postingCondition(DataTable T, string opkind_IUDSP);

        /// <summary>
        /// Check if the first row of T is allowed to be written to db
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool CanPostSingleRowInTable(DataTable T);

        /// <summary>
        /// Check if R is allowed to be written to db
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        bool CanPost(DataRow R);

        /// <summary>
        /// Delete all rows from T that are not allowed to be selected
        /// </summary>
        /// <param name="T"></param>
        void DeleteAllUnselectable(DataTable T);

        /// <summary>
        /// Check if a specified row of a table can be selected
        /// </summary>
        /// <param name="T"></param>
        /// <param name="RowIndex"></param>
        /// <returns></returns>
        bool CanSelect(DataTable T, int RowIndex);

        /// <summary>
        /// Check if the first row of T can be selected
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool CanSelectSingleRowInTable(DataTable T);

        /// <summary>
        /// Check if a row can be selected
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        bool CanSelect(DataRow R);

        /// <summary>
        /// Gets the security condition for selecting rows in a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="SQL"></param>
        /// <returns></returns>
        string SelectCondition(string tablename, bool SQL);


        /// <summary>
        /// Check if a the first row of T can be printed
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        bool CanPrintSingleRowInTable(DataTable T);

        /// <summary>
        /// Check if R can be "printed". 
        /// </summary>
        /// <param name="R"></param>
        /// <returns></returns>
        bool CanPrint(DataRow R);

        /// <summary>
        /// Check if there is a total deny of writing/deleting/inserting on a table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="OpKind"></param>
        /// <returns></returns>
        bool CantUnconditionallyPost(DataTable T, string OpKind);

        /// <summary>
        /// Substitute every &lt;%sys[varname]%&gt; and &lt;%usr[varname]%&gt; with actual values
        ///  taken from environment variables
        /// </summary>
        /// <param name="S"></param>
        /// <param name="SQL">When true, SQL representations are used to display values</param>
        /// <returns></returns>
        string Compile(string S, bool SQL);

        /// <summary>
        /// Substitute every &lt;%sys[varname]%&gt; and &lt;%usr[varname]%&gt; with actual values
        ///  taken from environment variables
        /// </summary>
        /// <param name="S"></param>       
        /// <returns></returns>
        string quotedCompile(string S);

        /// <summary>
        /// Subtitutes  every sequence:  openbr sys_name closebr with the unquoted value of sys[sys_name] 
        /// </summary>
        /// <param name="S"></param>
        /// <param name="SQL"></param>
        /// <param name="openbr"></param>
        /// <param name="closebr"></param>
        /// <returns></returns>
        string CompileWeb(string S, bool SQL, string openbr, string closebr);


        /// <summary>
        /// Get user environment variable 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetUsr(string key);

        /// <summary>
        /// Get system environment variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetSys(string key);

        /// <summary>
        /// Enumerates system variable names
        /// </summary>
        /// <returns></returns>
        string[] EnumSysKeys();

        /// <summary>
        /// Enumerates user variable names
        /// </summary>
        /// <returns></returns>
        string[] EnumUsrKeys();

        /// <summary>
        /// NON USARE !
        /// </summary>
        /// <param name="key"></param>
        /// <param name="O"></param>
        void SetUsr(string key, object O);

		/// <summary>
		/// Sets user environment variable
		/// </summary>
		/// <param name="key"></param>
		/// <param name="O"></param>
		void SetUsr(string key, string O);

        /// <summary>
        /// Set system environment variable
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        void SetSys(string key, object o);

        /// <summary>
        /// NON USARE !
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        void SetSys(string key, string o);

        /// <summary>
        /// Returns true if current user has sysadmin membership
        /// </summary>
        /// <returns></returns>
        bool isSystemAdmin();


        /// <summary>
        /// Sets datacontabile system environment variable
        /// </summary>
        /// <param name="D"></param>
        void SetDataContabile(DateTime D);

        /// <summary>
        /// Sets esercizio system environment variable
        /// </summary>
        /// <param name="Eserc"></param>
        void SetEsercizio(int Eserc);

        /// <summary>
        /// Get esercizio   system environment variable
        /// </summary>
        /// <returns></returns>
        int GetEsercizio();

        /// <summary>
        ///  Gets datacontabile system environment variable
        /// </summary>
        /// <returns></returns>
        DateTime GetDataContabile();

        /// <summary>
        /// 
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Base security class
    /// </summary>
    public class DefaultSecurity :ISecurity {
        /// <summary>
        /// Session user variables
        /// </summary>
        private readonly Hashtable _usr=new Hashtable(); //MUST BECOME internal protected

        /// <summary>
        /// Session system variables
        /// </summary>
        private readonly Hashtable _sys = new Hashtable(); //MUST BECOME internal protected

        /// <summary>
        /// Model used 
        /// </summary>
        protected static IMetaModel model = MetaFactory.factory.getSingleton<IMetaModel>();

        /// <summary>
        /// Data access linked 
        /// </summary>
        protected IDataAccess Conn;

        /// <summary>
        /// Basic security manager constructor 
        /// </summary>
        /// <param name="conn"></param>
        public DefaultSecurity(IDataAccess conn) {
            Conn = conn;
        }

        /// <summary>
        /// Returns true if current user has sysadmin membership
        /// </summary>
        /// <returns></returns>
        public virtual bool isSystemAdmin() {
            var o = Conn.DO_SYS_CMD("select IS_SRVROLEMEMBER ('sysadmin') AS issysadmin");
            return o != null && o.ToString() == "1";
        }

        /// <summary>
        /// Empty variables
        /// </summary>
        public void Clear() {
            _usr.Clear();
            _sys.Clear();
        }
        /// <summary>
        /// Crypts a string with 3-des
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal byte[] cryptKey(string key) {
			if (key == null) return null;
			//while ((pwd.Length % 8)!=0) pwd+=" ";
			//char[] a= pwd.ToCharArray();
			//byte []A = new byte[a.Length];
			//for (int i=0; i<a.Length; i++) A[i]= Convert.ToByte(a[i]);
			var a = Encoding.Default.GetBytes(key);

			var ms = new MemoryStream(1000);
			var tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			using (var cryptoS = new CryptoStream(ms,
				tripleDESCryptoServiceProvider.CreateEncryptor(
					new byte[] { 75, 12, 0, 215, 93, 89, 45, 11, 171, 96, 4, 64, 13, 158, 36, 190 },
					new byte[] { 61, 13, 99, 42, 149, 123, 145, 48, 83, 20, 238, 57, 128, 38, 12, 4 }
				), CryptoStreamMode.Write)) {
				cryptoS.Write(a, 0, a.Length);
				cryptoS.FlushFinalBlock();
			}
			var b = ms.ToArray();
			tripleDESCryptoServiceProvider.Dispose();
			return b;
		}

		/// <summary>
		/// Descripts a byte array with 3-des
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		internal static string decryptKey(byte[] b) {
            if (b == null) return null;
			var tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
			var mtdes = tripleDESCryptoServiceProvider.CreateDecryptor(
	            new byte[] {75, 12, 0, 215, 93, 89, 45, 11, 171, 96, 4, 64, 13, 158, 36, 190},
	            new byte[] {61, 13, 99, 42, 149, 123, 145, 48, 83, 20, 238, 57, 128, 38, 12, 4}
            );
            var ms = new MemoryStream();
            var cryptoS = new CryptoStream(ms,mtdes, CryptoStreamMode.Write);
            cryptoS.Write(b, 0, b.Length);
            cryptoS.FlushFinalBlock();
            var key = Encoding.Default.GetString(ms.ToArray()).TrimEnd();
            cryptoS.Dispose();
			//mtdes.Dispose();
			tripleDESCryptoServiceProvider.Dispose();
            return key;
        }

        /// <summary>
        /// Get system environment variable
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetSys(string key) {            
            return _sys[key];
        }

        readonly object _lockSysKeys = new object();
        readonly object _lockUsrKeys = new object();

        /// <summary>
        /// Enumerates system variables
        /// </summary>
        /// <returns></returns>
        public virtual string[] EnumSysKeys() {
            lock (_lockSysKeys) {
                var k = new string[_sys.Keys.Count];
                var i = 0;
                foreach (var o in _sys.Keys) {
                    var key = o.ToString();
                    k[i] = key;
                    i++;
                }
                return k;
            }
        }

        /// <summary>
        /// Enumerates user variables
        /// </summary>
        /// <returns></returns>
        public virtual string[] EnumUsrKeys() {
            lock (_lockUsrKeys) {
                var k = new string[_usr.Keys.Count];
                var i = 0;
                foreach (var o in _usr.Keys) {
                    var key = o.ToString();
                    k[i] = key;
                    i++;
                }
                return k;
            }
        }

        /// <summary>
        /// NON USARE !
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        public void SetUsr(string key, object o) {  //deve diventare internal protected
            lock (_lockUsrKeys) {
                _usr[key] = o;
            }
        }

		/// <summary>
		/// Sets user environment variable
		/// </summary>
		/// <param name="key"></param>
		/// <param name="o"></param>
		public virtual void SetUsr(string key, string o) {  //deve diventare internal protected
			lock (_lockUsrKeys) {
				_usr[key] = o;
			}
		}

		/// <summary>
		/// Get user environment variable 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object GetUsr(string key) {
            return _usr[key];
        }

        /// <summary>
        /// Set system environment variable
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        public virtual void SetSys(string key, object o) {  //deve diventare internal
            lock (_lockSysKeys) {
                if (key == "password" || key == "passworddb") {
                    if (GetSys("user").ToString() == GetSys("userdb").ToString()) {
                        _sys["password"] = cryptKey(o.ToString());
                        _sys["passworddb"] = cryptKey(o.ToString());
                    }
                    else
                        _sys[key] = cryptKey(o.ToString());
                    return;
                }
                _sys[key] = o;
            }
        }

		/// <summary>
		/// NON USARE !
		/// </summary>
		/// <param name="key"></param>
		/// <param name="o"></param>
		public virtual void SetSys(string key, string o) {  //deve diventare internal
			lock (_lockSysKeys) {
				if (key == "password" || key == "passworddb") {
					if (GetSys("user").ToString() == GetSys("userdb").ToString()) {
						_sys["password"] = cryptKey(o);
						_sys["passworddb"] = cryptKey(o);
					} 
					else
						_sys[key] = cryptKey(o);
					return;
				}
				_sys[key] = o;
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <param name="opkind_IUDSP"></param>
		/// <returns></returns>
		public virtual string postingCondition(DataTable t,string opkind_IUDSP) {
           return null;
        }
      

        /// <summary>
        /// Check if the first row of T is allowed to be written to db
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public virtual bool CanPostSingleRowInTable(DataTable T) {
            return model.isSkipSecurity(T) || CanPost(T.Rows[0]);
        }

        /// <summary>
        /// Check if R is allowed to be written to db
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public virtual bool CanPost(DataRow r) {
            return true;
        }

        /// <summary>
        /// Delete all rows from T that are not allowed to be selected
        /// </summary>
        /// <param name="T"></param>
        public virtual void DeleteAllUnselectable(DataTable T) {
            if (model.isSkipSecurity(T)) return;
            foreach (var r in T.Select()) {
                if (CanSelect(r)) continue;
                r.Delete();
                r.AcceptChanges();
            }
        }

        /// <summary>
        /// Check if a specified row of a table can be selected
        /// </summary>
        /// <param name="T"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public virtual bool CanSelect(DataTable T, int rowIndex) {
            return CanSelect(T.Rows[rowIndex]);
        }

   

        /// <summary>
        /// Check if the first row of T can be selected
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public virtual bool CanSelectSingleRowInTable(DataTable T) {
            return CanSelect(T.Rows[0]);
        }

        /// <summary>
        /// Check if a row can be selected
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public virtual bool CanSelect(DataRow r) {
            return true;
        }

        /// <summary>
        /// Gets the security condition for selecting rows in a table
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual string SelectCondition(string tablename, bool sql) {
            return null;
        }

       

        /// <summary>
        /// Check if a the first row of T can be printed
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public virtual bool CanPrintSingleRowInTable(DataTable T) {
            return CanPrint(T.Rows[0]);
        }

        /// <summary>
        /// Check if R can be "printed". 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public virtual bool CanPrint(DataRow r) {
            return true;
        }

        

        /// <summary>
        /// Check if there is a total deny of writing/deleting/inserting on a table
        /// </summary>
        /// <param name="T"></param>
        /// <param name="opKind"></param>
        /// <returns></returns>
        public virtual bool CantUnconditionallyPost(DataTable T, string opKind) {
            return false;
        }

        /// <summary>
        /// Sets datacontabile system environment variable
        /// </summary>
        /// <param name="D"></param>
        public void SetDataContabile(DateTime D) {
            SetSys("datacontabile", D);
        }

        /// <summary>
        /// Sets esercizio system environment variable
        /// </summary>
        /// <param name="eserc"></param>
        public virtual void SetEsercizio(int eserc) {
	        SetSys("esercizio", eserc);
        }

        /// <summary>
        /// Get esercizio   system environment variable
        /// </summary>
        /// <returns></returns>
        public virtual int GetEsercizio() {
            try {
                return (int)_sys["esercizio"];
            }
            catch {
                return 0;
            }
        }

        /// <summary>
        ///  Gets datacontabile system environment variable
        /// </summary>
        /// <returns></returns>
        public virtual DateTime GetDataContabile() {
            try {
                return (DateTime)_sys["datacontabile"];
            }
            catch {
                return DateTime.Now;
            }
        }
        /// <summary>
        /// Substitute every &lt;%sys[varname]%&gt; and &lt;%usr[varname]%&gt; with actual values
        ///  taken from environment variables
        /// </summary>
        /// <param name="S"></param>
        /// <param name="SQL">When true, SQL representations are used to display values</param>
        /// <returns></returns>
        public virtual string Compile(string S, bool SQL) {
            string newS = S;
            if ((S == null) || (S == "")) return newS;
            bool applied = true;
            while (applied) {
                applied = false;
                if (newS.IndexOf("<%sys[", StringComparison.Ordinal) >= 0) {
                    string[] syskeys = EnumSysKeys();
                    foreach (object o in syskeys) {
                        string oldvalue = "<%sys[" + o + "]%>";
                        if (newS.IndexOf(oldvalue, StringComparison.Ordinal) >= 0) {
                            string newvalue = mdl_utils.Quoting.unquotedstrvalue(_sys[o], SQL);
                            //eccezione per la login, che può contenere apici
                            if (o.ToString() == "user") newvalue = newvalue.Replace("'", "''");
                            if (o.ToString() == "idcustomuser") newvalue = newvalue.Replace("'", "''");
                            newS = newS.Replace(oldvalue, newvalue);
                            applied = true;
                        }
                    }
                }

                if (newS.IndexOf("<%usr[", StringComparison.Ordinal) >= 0) {
                    string[] usrkeys = EnumUsrKeys();
                    foreach (object o in usrkeys) {
                        string oldvalue = "<%usr[" + o + "]%>";
                        if (newS.IndexOf(oldvalue, StringComparison.Ordinal) >= 0) {
                            string newvalue = mdl_utils.Quoting.unquotedstrvalue(_usr[o], SQL);
                            newS = newS.Replace(oldvalue, newvalue);
                            applied = true;
                        }
                    }
                }
            }

            if (newS.IndexOf("<%usr") >= 0 || newS.IndexOf("<%sys") >= 0) {
                //ErrorLogger.Logger.markEvent("Trovata variabile di sicurezza non valorizzata nella stringa " + newS);
                return "(1=2)";
            }
            return newS;
        }

        /// <summary>
        /// Compile a string substituting keys with quoted values
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public virtual string quotedCompile(string S) {
            string newS = S;
            if ((S == null) || (S == "")) return newS;
            int handle = StartTimer("MyCompile");
            bool applied = true;
            while (applied) {
                applied = false;
                if (newS.IndexOf("<%sys[", StringComparison.Ordinal) >= 0) {

                    string[] syskeys = EnumSysKeys();
                    foreach (object o in syskeys) {
                        string oldvalue = "<%sys[" + o + "]%>";
                        if (newS.IndexOf(oldvalue, StringComparison.Ordinal) >= 0) {
                            string newvalue = mdl_utils.Quoting.quotedstrvalue(_sys[o], true);
                            newS = newS.Replace(oldvalue, newvalue);
                            applied = true;
                        }
                    }
                }
                if (newS.IndexOf("<%usr[", StringComparison.Ordinal) >= 0) {
                    string[] usrkeys = EnumUsrKeys();
                    foreach (object o in usrkeys) {
                        string oldvalue = "<%usr[" + o + "]%>";
                        if (newS.IndexOf(oldvalue, StringComparison.Ordinal) >= 0) {
                            string newvalue = mdl_utils.Quoting.quotedstrvalue(_usr[o], true);
                            newS = newS.Replace(oldvalue, newvalue);
                            applied = true;
                        }
                    }
                }
            }
            StopTimer(handle);
            return newS;
        }



        /// <summary>
        /// Subtitutes  every sequence:  openbr sys_name closebr with the unquoted value of sys[sys_name] 
        /// </summary>
        /// <param name="S"></param>
        /// <param name="SQL"></param>
        /// <param name="openbr"></param>
        /// <param name="closebr"></param>
        /// <returns></returns>
        public string CompileWeb(string S, bool SQL, string openbr, string closebr) {
            string newS = S;
            if ((S == null) || (S == "")) return newS;
            bool applied = true;
            while (applied) {
                applied = false;
                string[] syskeys = EnumSysKeys();
                foreach (object o in syskeys) {
                    string oldvalue = openbr + o.ToString() + closebr;
                    if (newS.IndexOf(oldvalue) >= 0) {
                        string newvalue = mdl_utils.Quoting.unquotedstrvalue(_sys[o], SQL);
                        newS = newS.Replace(oldvalue, newvalue);
                        applied = true;
                    }
                }
            }
            return newS;
        }


    }
}

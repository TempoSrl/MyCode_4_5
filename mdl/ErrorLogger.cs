using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mdl {
    /// <summary>
    /// Interface for local and remote error logging 
    /// </summary>
    public interface IErrorLogger {

        /// <summary>
        /// This is set when a shutdown is necessary
        /// </summary>
         bool unrecoverableError { get; set; }



        /// <summary>
        /// Marks an Exception and set Last Error
        /// </summary>
        /// <param name="e"></param>
        /// <param name="main">Main description</param>
        void markException(Exception e, string main = "");

        /// <summary>
        /// Gets a string describing an error with an exception
        /// </summary>
        /// <param name="e"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        string formatException(Exception e, string msg = "");

        /// <summary>
        /// Sends an exception to a remote error logger
        /// </summary>
        /// <param name="main"></param>
        /// <param name="exception"></param>
        /// <param name="security"></param>
        /// <param name="dataAccess"></param>
        /// <param name="controller"></param>
        /// <param name="meta"></param>
        void logException(string main, 
            Exception exception = null, 
            ISecurity security=null , 
            IDataAccess dataAccess=null,
            object controller = null,
            IMetaData meta = null);

        /// <summary>
        /// Adds an event to a local log
        /// </summary>
        /// <param name="e"></param>
        void markEvent(string e);


        /// <summary>
        /// Adds a warn (not an error) to a local log
        /// </summary>
        /// <param name="e"></param>
        void warnEvent(string e);
    }

    /// <summary>
    /// sends a post message to website errorLogBaseAddress
    /// </summary>
    public class SendMessage {
        private byte[] _result;     //byte[]
        private readonly string _message;
        private readonly string _type;
        public SendMessage(string message, string type) {
            _message = message;
            _type = type;
        }
        /// <summary>
        /// Address where access to db are logged, actually https://ticket.temposrl.it/LiveLog/
        /// </summary>
        public static string errorLogBaseAddress = "https://ticket.temposrl.it/LiveLog/";

        /// <summary>
        /// Address where client errors are logged, actually https://ticket.temposrl.it/LiveLog/DoEasy.aspx
        /// </summary>
        public static string errorLogUrl = "https://ticket.temposrl.it/LiveLog/DoEasy.aspx";


        public void send() {
            string req = "";
            try {
                var w = new WebClient {BaseAddress = errorLogBaseAddress};
                req = $"{errorLogUrl}?{_type}={_message}";

                string URI =errorLogUrl;
                var reqparm = new System.Collections.Specialized.NameValueCollection {
                        { _type, _message }
                    };
                _result = w.UploadValues(URI, "POST", reqparm);

                //Result = W.DownloadData(errorLogUrl + "?" + type + "=" + message);




                if (_result == null || _result.Length == 0) {
                    Console.WriteLine("No response");
                }
                else {
                    Console.WriteLine(Encoding.ASCII.GetString(_result)); //
                }
            }
            catch (Exception e) {
                Console.WriteLine("Failed request:");
                Console.WriteLine(req);
                Console.WriteLine(e.ToString());
            }

        }
    }



    /// <summary>
    /// Implements logging facilities
    /// </summary>
    public class ErrorLogger : IErrorLogger {

        /// <summary>
        /// Set if application should be closed
        /// </summary>
        public bool unrecoverableError { get; set; }
        /// <summary>
        /// Default logger for the application
        /// </summary>
        public static ErrorLogger Logger= new ErrorLogger();

        /// <summary>
        /// Application name used for error logging
        /// </summary>
        public static string applicationName;

        /// <summary>
        ///  Adds an event to a local log
        /// </summary>
        /// <param name="e"></param>
        public virtual void markEvent(string e) {
            var msg = DateTime.Now.ToString("HH:mm:ss.fff") + ":" + e;
            Trace.WriteLine(msg);
            Trace.Flush();
        }

        /// <summary>
        ///  Adds an event to a local log
        /// </summary>
        /// <param name="e"></param>
        public virtual void warnEvent(string e) {
            var msg = $"$${DateTime.Now.ToString("HH:mm:ss.fff")}:{e}";
            Trace.WriteLine(msg);
            Trace.Flush();
        }

        /// <summary>
        /// Gets a string describing an error with an exception
        /// </summary>
        /// <param name="e"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public virtual string formatException(Exception e, string msg="") {
            msg = msg ?? "";       
            return $"{DateTime.Now.ToString("HH:mm:ss.fff")}:{msg}\n{QueryCreator.GetErrorString(e)}";
        }


        /// <summary>
        /// Sends an exception (type z) to a remote error logger
        /// </summary>
        /// <param name="main"></param>
        /// <param name="exception"></param>
        /// <param name="security"></param>
        /// <param name="dataAccess"></param>
        /// <param name="controller"></param>
        /// <param name="meta"></param>
        public virtual void logException(string main, Exception exception = null, ISecurity security = null,
            IDataAccess dataAccess = null , 
            object  controller= null,
            IMetaData meta = null) {
            //string ErrorLogUrl = "http://ticket.temposrl.it/LiveLog/DoEasy.aspx";
            //if (meta != null && security == null) {
            //    security = meta.security;
            //}
            //if (controller != null && dataAccess == null) {
            //    dataAccess = controller.dbConn;
            //}
            if (security == null && dataAccess != null) {
                security = dataAccess.Security;
            }

            string msg = "";
            string errmsg = main ?? "";
            
            errmsg = $"AppExecutable:{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.SetupInformation?.ApplicationName}\n\r"+errmsg;
       
            
            try {
                string datacont = "";
                if (security != null) {
                    if (security.GetSys("datacontabile") != null) {
                        datacont = security.GetDataContabile().ToString("d");
                    }
                    msg +=
                            "nomedb=" + mdl_utils.Quoting.quotedstrvalue(security.GetSys("database"), true) + ";" +
                            "server=" + mdl_utils.Quoting.quotedstrvalue(security.GetSys("server"), true) + ";" +
                            "username=" + mdl_utils.Quoting.quotedstrvalue(security.GetSys("user"), true) + ";" +
                            "machine=" + mdl_utils.Quoting.quotedstrvalue(security.GetSys("computername"), false) + ";" +
                            "dep=" + mdl_utils.Quoting.quotedstrvalue(security.GetSys("userdb"), false) + ";" +
                            "esercizio=" + mdl_utils.Quoting.quotedstrvalue(security.GetSys("esercizio"), false) + ";" +
                            "datacont=" + mdl_utils.Quoting.quotedstrvalue(datacont, false) + ";";
                }
                else {
                    msg +=
                            "username=" + mdl_utils.Quoting.quotedstrvalue(noNull(Environment.UserName), true) + ";" +
                            "machine=" +
                             mdl_utils.Quoting.quotedstrvalue(
                                noNull(Environment.MachineName) + "-" + noNull(DataAccess.GetOSVersion()), false) + ";";
                }


                string lasterr = dataAccess?.SecureGetLastError();
                if (!string.IsNullOrEmpty(lasterr)) {
                    msg += "dberror=" + mdl_utils.Quoting.quotedstrvalue(lasterr, false) + ";";
                }
                errmsg += "\r\n" + GetOuput();
                msg += "err=" + mdl_utils.Quoting.quotedstrvalue(noNull(errmsg), false);

                string internalMsg = "";
                if (applicationName != null) {
	                msg += "app=" + mdl_utils.Quoting.quotedstrvalue(applicationName, true) +";";
                }
               


                if (exception != null) {
                    var except = QueryCreator.GetErrorString(exception);
                    if (except.Length > 2800) except = except.Substring(0, 2800);
                    internalMsg += except + "\n";
                    Trace.WriteLine(exception.ToString());
                }

                if (internalMsg != "") {
	                msg += ";msg=" + mdl_utils.Quoting.quotedstrvalue(internalMsg, false);
                }
                

                byte[] b2 = mdl_utils.CryptDecrypt.CryptString(msg);
                var ss2 = mdl_utils.Quoting.ByteArrayToString(b2);

                var sm = new SendMessage(ss2, "z");
                sm.send();
                //var TT= Task.Run(() => sm.send() ); I fear that application could be closed in the meanwhile so I do the operation syncronously
                
                

            }
            catch(Exception e) {
	            Trace.WriteLine("Richiesta fallita:"+e.ToString());
            }


        }

        protected static object noNull(object o) {
            if (o == null) return "null";
            return o == DBNull.Value ? "DBNull" : o;
        }

        public  static string GetOuput() {
            string outputview = "";
            foreach (TraceListener tl in Trace.Listeners) {
                //Vede se ha proprietà StringBuilder Errors
                Type myType = tl.GetType();
                var mprop = myType.GetField("Errors");
                if (mprop != null) {
                    var ssb = (StringBuilder)mprop.GetValue(tl);
                    outputview = "Output View:\n\r" + ssb + "\n\r";
                    break;
                }
            }
            if (outputview.Length > 4000) {
                outputview = outputview.Substring(outputview.Length - 4000);
            }
            return outputview;
        }


        /// <summary>
        /// Marks an Exception and set Last Error
        /// </summary>
        /// <param name="e"></param>
        /// <param name="main">Main description</param>       
        public virtual void markException(Exception e, string main="") {
            string msg = formatException( e,main);
            Trace.WriteLine(msg);
            Trace.Flush();
        }

        
    }
}

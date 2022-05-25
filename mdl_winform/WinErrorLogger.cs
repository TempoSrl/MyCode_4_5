using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mdl;

namespace mdl_winform {
    class WinErrorLogger: ErrorLogger {
        /// <summary>
        /// Sends an exception (type z) to a remote error logger
        /// </summary>
        /// <param name="main"></param>
        /// <param name="exception"></param>
        /// <param name="security"></param>
        /// <param name="dataAccess"></param>
        /// <param name="controller"></param>
        /// <param name="meta"></param>
        public override void logException(string main, Exception exception = null, ISecurity security = null,
            IDataAccess dataAccess = null,
            object ctrl= null,
            IMetaData meta = null) {
            IFormController controller = ctrl as IFormController;
            //string ErrorLogUrl = "http://ticket.temposrl.it/LiveLog/DoEasy.aspx";
            if (meta != null && controller == null) {
                security = controller.security;
            }
            if (controller != null && dataAccess == null) {
                dataAccess = controller.conn;
            }
            if (security == null && dataAccess != null) {
                security = dataAccess.Security;
            }

            string msg = "";
            string errmsg = main ?? "";

            errmsg = $"AppExecutable:{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.SetupInformation?.ApplicationName}\n\r" + errmsg;


            try {
                //object omsg = null; //Conn.DO_SYS_CMD("sp_exp_licenzauso '1'");
                //if (omsg!=null) msg=omsg.ToString()+";";
                if (controller != null) {
                    if (controller.InsertMode) errmsg += "{Insert}";
                    if (controller.EditMode) errmsg += "{Edit}";
                    if (controller.IsEmpty) errmsg += "{Empty}";
                    if (controller.linkedForm != null) errmsg += "{" + controller.linkedForm.GetType().AssemblyQualifiedName + "}";
                    if (controller.isClosing) errmsg += "{InChiusura}";
                    if (controller.destroyed) errmsg += "{Destroyed}";
                    if (controller.ErroreIrrecuperabile) errmsg += "{ErroreIrrecuperabile}";
                }

                string datacont = "";
                if (security != null) {
                    if (security.GetSys("datacontabile") != null) {
                        datacont = ((DateTime)(security.GetSys("datacontabile"))).ToString("d");
                    }
                    msg +=
                            "nomedb=" + mdl_utils.Quoting.quote(security.GetSys("database"), true) + ";" +
                            "server=" + mdl_utils.Quoting.quote(security.GetSys("server"), true) + ";" +
                            "username=" + mdl_utils.Quoting.quote(security.GetSys("user"), true) + ";" +
                            "machine=" + mdl_utils.Quoting.quote(security.GetSys("computername"), false) + ";" +
                            "dep=" + mdl_utils.Quoting.quote(security.GetSys("userdb"), false) + ";" +
                            "esercizio=" + mdl_utils.Quoting.quote(security.GetSys("esercizio"), false) + ";" +
                            "datacont=" + mdl_utils.Quoting.quote(datacont, false) + ";";
                }
                else {
                    msg +=
                            "username=" + mdl_utils.Quoting.quote(noNull(Environment.UserName), true) + ";" +
                            "machine=" +
                            mdl_utils.Quoting.quote(
                                noNull(Environment.MachineName) + "-" + noNull(FormStatic.GetOSVersion()), false) + ";";
                }


                string lasterr = dataAccess?.SecureGetLastError();
                if (!string.IsNullOrEmpty(lasterr)) {
                    msg += "dberror=" + mdl_utils.Quoting.quote(lasterr, false) + ";";
                }
                errmsg += "\r\n" + GetOuput();
                msg += "err=" + mdl_utils.Quoting.quote(noNull(errmsg), false);

                string internalMsg = "";
                if (applicationName != null) {
                    msg += "app=" + mdl_utils.Quoting.quote(applicationName, true) + ";";
                }



                if (exception != null) {
                    var except = ErrorLogger.GetErrorString(exception);
                    if (except.Length > 2800) except = except.Substring(0, 2800);
                    internalMsg += except + "\n";
                    Trace.WriteLine(exception.ToString());
                }

                if (internalMsg != "") {
                    msg += ";msg=" + mdl_utils.Quoting.quote(internalMsg, false);
                }


                byte[] b2 = mdl_utils.CryptDecrypt.CryptString(msg);
                var ss2 = mdl_utils.Quoting.ByteArrayToString(b2);

                var sm = new SendMessage(ss2, "z");
                sm.Send();
                //var TT= Task.Run(() => sm.send() ); I fear that application could be closed in the meanwhile so I do the operation syncronously



            }
            catch (Exception e) {
                Trace.WriteLine("Richiesta fallita:" + e.ToString());
            }


        }
    }
}

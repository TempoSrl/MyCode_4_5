using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mdl;
using System.Data;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace mdl_winform {

     public interface ICustomViewListForm {
	    void  init(IWinFormMetaData linked, string columnlist,
		    string mergedfilter, string searchtable, string listingType, 
            DataTable toMerge, 
            string sorting, 
            int top,
            bool filterLocked,
            string Text);

	    void setFormPosition(Form parentForm, IFormController ctrl);
	    void close();
	    bool hasNext();
	    bool hasPrev();
	    void gotoNext();
	    void gotoPrev();
	    void destroy();
	    void selectSomething();
	    void show();
        System.Windows.Forms.DialogResult ShowDialog(Form parent);
	    void setStartPosition(FormStartPosition p);
	    System.Data.DataRow getLastSelectedRow();
    }

      /// <summary>
    /// Event generated at the beginning of the clear of a row
    /// </summary>
    public class FormActivated : IApplicationEvent {
        /// <summary>
        /// Form being actovated
        /// </summary>
        public Form f;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="f"></param>
        public FormActivated(Form f) {
            this.f = f;
        }
    }
    
    /// <summary>
    /// Interface for initable attached to form classes
    /// </summary>
    public interface IFormInit {
        /// <summary>
        /// Inits the class and attaches it to a form
        /// </summary>
        /// <param name="f"></param>
        void Init(Form f);
    }


    static class FormStatic {
        static object noNull(object o) {
            if (o == null) return "null";
            return o == DBNull.Value ? "DBNull" : o;
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
        public static void logException(string main, Exception exception = null, ISecurity security = null,
            IDataAccess dataAccess = null,
            IFormController controller= null,
            IMetaData meta = null) {
            //string ErrorLogUrl = "http://ticket.temposrl.it/LiveLog/DoEasy.aspx";
            if (controller != null && security == null) {
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
                errmsg += "\r\n" + ErrorLogger.GetOuput();
                msg += "err=" + mdl_utils.Quoting.quotedstrvalue(noNull(errmsg), false);

                string internalMsg = "";
                if (ErrorLogger.applicationName != null) {
                    msg += "app=" + mdl_utils.Quoting.quotedstrvalue(ErrorLogger.applicationName, true) + ";";
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

                var sm = new mdl.SendMessage(ss2, "z");
                sm.send();
                //var TT= Task.Run(() => sm.send() ); I fear that application could be closed in the meanwhile so I do the operation syncronously



            }
            catch (Exception e) {
                Trace.WriteLine("Richiesta fallita:" + e.ToString());
            }


        }

        static FormStatic(){
            var factory = MetaFactory.factory;
            factory.registerType(typeof(FormEventsManager), typeof(IFormEventsManager));
            factory.registerType(typeof(ControEnabler), typeof(IControEnabler));
            factory.registerType(typeof(ListViewManager), typeof(IListViewManager));
            factory.registerType(typeof(HelpForm), typeof(IHelpForm));
            factory.registerType(typeof(ComboBoxManager), typeof(IComboBoxManager));
            factory.registerType(typeof(FormController), typeof(IFormController));           
            factory.setSingleton(typeof(IProcessRunner), new DefaultProcessRunner());
            factory.registerType(typeof(DefaultCustomViewListForm), typeof(ICustomViewListForm));           
            factory.registerType(typeof(DefaultOpenFileDialog), typeof(IOpenFileDialog));
            factory.registerType(typeof(DefaultSaveFileDialog), typeof(ISaveFileDialog));
            factory.registerType(typeof(DefaultFolderBrowserDialog), typeof(IFolderBrowserDialog));
            factory.setSingleton(typeof(IMessageShower), new DefaultMessageShower());
            factory.setSingleton(typeof(IFormCreationListener), new DefaultCreationListener());

        }

        private static readonly Hashtable LoadedFormAssembly = new Hashtable();

        private static readonly Dictionary<string, bool> LoadedAss = new Dictionary<string, bool>();

        
        static Assembly loadAssembly(string name) {
	        string folder = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
	        if (name.StartsWith("System.")) return Assembly.Load(name);
	        return Assembly.LoadFrom(Path.Combine(folder, name + ".dll"));
        }

        private static void loadReferencedAssembly(Assembly assembly) {
            foreach (var name in assembly.GetReferencedAssemblies()) {
                if (LoadedAss.ContainsKey(name.Name)) continue;
                //var x = AppDomain.CurrentDomain.GetAssemblies();
                if (AppDomain.CurrentDomain.GetAssemblies().Any(aa => aa.GetName().Name == name.Name)) {
                    LoadedAss[name.Name] = true;
                    continue;
                }
                //var handle = metaprofiler.StartTimer("loadReferencedAssembly * "+name.Name);
                loadAssembly(name.Name.StartsWith("System")? name.FullName: name.Name);
                //metaprofiler.StopTimer(handle);
                LoadedAss[name.Name] = true;
            }
        }


          /// <summary>
        /// Gets a form, given it's dll name. The first public form with a default constructor in the assembly is taken.
        /// </summary>
        /// <param name="dllName"></param>
        /// <returns></returns>
        public static Form GetFormByDllName(string dllName) {
            var errMsg = $"File {dllName}.dll NOT FOUND in run directory.";
            //try {
                var myAssemblyName = dllName;
                Assembly a;
                if (LoadedFormAssembly.Contains(dllName)) {
                    a = LoadedFormAssembly[dllName] as Assembly;
                }
                else {
                    var handle = mdl_utils.metaprofiler.StartTimer("load Form * "+myAssemblyName);
                    a = loadAssembly(myAssemblyName);
                    LoadedFormAssembly[dllName] = a;
                    mdl_utils.metaprofiler.StopTimer(handle);
                    new Task(() => {
	                    loadReferencedAssembly(a);
                    }).Start();
                   
                }

                if (a == null) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(null,errMsg,"Errore");
                    return null;
                }

                Type formType = null;
                foreach (var T in a.GetTypes()) {
                    if (!typeof(Form).IsAssignableFrom(T)) continue;
                    formType = T;
                    var formBuilder =formType.GetConstructor(new Type[] {});

                    errMsg = $"public void constructor of form not found in file {myAssemblyName} ver:{a.GetName().Version}";
                    if (formBuilder == null) continue;                    
                    errMsg = $"Error calling constructor of form in file {myAssemblyName} ver:{a.GetName().Version}";

                    Form f;
                    //try {
                        f = (Form) formBuilder.Invoke(new object[0]);
                    //}
                    //catch (Exception e) {
                    //    shower.ShowException(errMsg, e);
                    //    continue;
                    //}
                    ErrorLogger.Logger.warnEvent($"OpenForm:{dllName} ver:{a.GetName().Version}");
                    return f;
                }

                errMsg = $"No public form found with public void constructor in file {myAssemblyName}";
                if (formType == null) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(errMsg,"Errore");
                    return null;
                }
            //}
            //catch (Exception e) {
            //    shower.ShowException(errMsg, e);
            //}
            return null;
        }
    }

   
    /// <summary>
    /// 
    /// </summary>
    public static class HelperMetaFactory {

        /// <summary>
        /// Creates an instance of T and attaches it to a Form
        /// </summary>
        /// <param name="f"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T createInstance<T>(this Form f) where T : class {
            var o = MetaFactory.create<T>();
            attachInstance(f,o,typeof(T));
            var init = o as IFormInit;
            init?.Init(f);
            return o;
        }

        /// <summary>
        /// Get Interface of a given type in Form dictionary . Throw exception if not found.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static T getInstance<T>(this Form f) where T:class  {
            var h  = f.Tag as Hashtable;
            var o = h?[typeof(T).Name];
            if (o == null) return null; //throw new Exception($@"{typeof(T).Name} not attached to form {f.Name}");
            return o as T;
        }

        /// <summary>
        /// Get Interface of a given type in Form dictionary. Returns null if instance is not found.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static T safeGetInstance<T>(this Form f) where T:class  {
            var h  = f.Tag as Hashtable;
            var o = h?[typeof(T).Name];
            return o as T;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="instance"></param>
        /// <param name="abstractType"></param>
        public static void attachInstance(this Form f, object instance, Type abstractType) {
            if(!(f.Tag is Hashtable h)) {
                h = new Hashtable();
                f.Tag = h;
            }
            h[abstractType.Name] = instance;
        }
    }
}

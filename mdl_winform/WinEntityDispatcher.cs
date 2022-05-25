using System;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using mdl_utils;
using mdl;
using LM = mdl_language;

namespace mdl_winform {

    /// <summary>
    /// Interface for meta data dispatcher
    /// </summary>
    public interface IWinEntityDispatcher:IEntityDispatcher {

        WinFormMetaData GetWinFormMeta(string metaDataName);

    }

    /// <summary>
    /// Application Meta Data Dispatcher
    /// </summary>
    public class WinEntityDispatcher : EntityDispatcher, IWinEntityDispatcher {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>        
        public WinEntityDispatcher(IDataAccess conn) : base(conn) {

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
                loadAssembly(name.Name.StartsWith("System") ? name.FullName : name.Name);
                //metaprofiler.StopTimer(handle);
                LoadedAss[name.Name] = true;
            }
        }



        private static readonly Hashtable LoadedFormAssembly = new Hashtable();

        /// <summary>
        /// Gets a form, given it's dll name. The first public form with a default constructor in the assembly is taken.
        /// </summary>
        /// <param name="dllName"></param>
        /// <returns></returns>
        public static Form GetFormByDllName(string dllName) {
            var errMsg = $"File {dllName}.dll NOT FOUND in run directory.";
            try {
                var myAssemblyName = dllName;
                Assembly a;
                if (LoadedFormAssembly.Contains(dllName)) {
                    a = LoadedFormAssembly[dllName] as Assembly;
                }
                else {
                    var handle = MetaProfiler.StartTimer("load Form * "+myAssemblyName);
                    a = loadAssembly(myAssemblyName);
                    LoadedFormAssembly[dllName] = a;
                    MetaProfiler.StopTimer(handle);
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
                    try {
                        f = (Form) formBuilder.Invoke(new object[0]);
                    }
                    catch (Exception e) {                        
                        ErrorLogger.Logger.logException(errMsg,e);
                        continue;
                    }
                    ErrorLogger.Logger.WarnEvent($"OpenForm:{dllName} ver:{a.GetName().Version}");
                    return f;
                }

                errMsg = $"No public form found with public void constructor in file {myAssemblyName}";
                if (formType == null) {
                    MetaFactory.factory.getSingleton<IMessageShower>().Show(errMsg,"Errore");
                    return null;
                }
            }
            catch (Exception e) {
               ErrorLogger.Logger.logException(errMsg,e);
            }
            return null;
        }

		
        /// <summary>
        /// Gets a MetaData Class given it's name
        /// </summary>
        /// <param name="metaDataName"></param>
        /// <returns></returns>
        /// 
        public virtual WinFormMetaData GetWinFormMeta(string metaDataName) {
           

            var handle = MetaProfiler.StartTimer("Get metaDataName * "+metaDataName);       

            if (NoLoad.Contains(metaDataName)) {
                MetaProfiler.StopTimer(handle);
                return defaultMetaData( metaDataName) as WinFormMetaData;
            }
            var doLog = true;

            try {
                var myAssemblyName = $"meta_{metaDataName}";
                var myClassName = $"{myAssemblyName}.Meta_{metaDataName}";
                Assembly a = null;

                if (LoadedAssembly.Contains(metaDataName)) {
                    a = LoadedAssembly[metaDataName] as Assembly;
                }
                else {
                    var list = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var currA in list) {
                        if (currA.ManifestModule.Name.ToLower() != myAssemblyName.ToLower() + ".dll") continue;
                        a = currA;
                        LoadedAssembly[metaDataName] = a;
                        break;
                    }
                }

                if (a == null) {
                    if (!File.Exists(Path.Combine(getDllFolder(), myAssemblyName + ".dll"))) {
                        NoLoad[metaDataName] = 1;
                        doLog = false;
                        return defaultMetaData( metaDataName) as WinFormMetaData;
                    }
                    var handle2 = MetaProfiler.StartTimer("REAL Get * "+metaDataName);
                    try {
                        lock (MyLockMeta) {
                            a = loadAssembly( myAssemblyName);
                        }
                        LoadedAssembly[metaDataName] = a;
                    }
                    catch (FileNotFoundException f) {
                        LoadError[metaDataName] = ErrorLogger.GetErrorString(f);
                        NoLoad[metaDataName] = 1;
                        doLog = false;
                    }
                    catch (Exception el) {
                        logException($"Errore caricando la DLL {myAssemblyName} che è quindi aggiunta a NOLOAD.", el);
                        LoadError[metaDataName] = ErrorLogger.GetErrorString(el);
                        NoLoad[metaDataName] = 1;
                        unrecoverableError = true;
                    }
                    MetaProfiler.StopTimer(handle2);
                }

                if (a == null) {
                    //Conn.LogError(ErrMsg,null);
                    MetaProfiler.StopTimer(handle);
                    var m =  defaultMetaData(metaDataName) as WinFormMetaData;
                    if (doLog) ErrorLogger.Logger.MarkEvent($"last Error during load:{LoadError[metaDataName]}");
                    return m;
                }
                var errMsg = $"Class {myClassName} not found in file {myAssemblyName}";
                Type metaObjType = a.GetType(myClassName);
                if (metaObjType == null) {
                    ErrorLogger.Logger.MarkEvent(errMsg);
                    NoLoad[metaDataName]=1;
                    unrecoverableError = true;
                    MetaProfiler.StopTimer(handle);
                    return defaultMetaData(metaDataName) as WinFormMetaData;
                }
                var metaObjBuilder = (metaObjType
                        .GetConstructors()
                        .Where(c => c.GetParameters().Length == 3
                                      && c.GetParameters()[0].ParameterType.GetInterfaces().Contains(typeof(IDataAccess))
                                    && c.GetParameters()[1].ParameterType.GetInterfaces().Contains(typeof(IMetaDataDispatcher))
                                    && c.GetParameters()[2].ParameterType.GetInterfaces().Contains(typeof(ISecurity))
                                      )
                        ).FirstOrDefault();
                var parametri = new object[] {dbConn, this as IMetaDataDispatcher, security};
                if (metaObjBuilder == null) {
                    //For retro compatibility
                    metaObjBuilder = metaObjType.GetConstructor(
                             new[] {typeof(DataAccess ), typeof(MetaDataDispatcher)});
#pragma warning disable 612
                    parametri = new object[] { Conn, this };
#pragma warning restore 612

                }
                //ConstructorInfo metaObjBuilder =
                //    metaObjType.GetConstructor(
                //        new Type[] {typeof(DataAccess ), typeof(Dispatcher), typeof(string)});

                errMsg = $"public {myClassName}(DataAccess Conn, EntityDispatcher dispatcher) of Class {myClassName} not found in file {myAssemblyName}";
                if (metaObjBuilder == null) {
                    ErrorLogger.Logger.MarkEvent(errMsg);
                    logException(errMsg, null);
                    NoLoad[metaDataName]= 1;
                    unrecoverableError = true;
                    MetaProfiler.StopTimer(handle);
                    return defaultMetaData(metaDataName) as WinFormMetaData;
                }
                errMsg = $"Error calling constructor of Class {myClassName} in file {myAssemblyName}";

                WinFormMetaData md ;
                try {
                    md = (WinFormMetaData) metaObjBuilder.Invoke(parametri);
                }
                catch (Exception e) {
                    ErrorLogger.Logger.MarkEvent($"{errMsg}(Detail:{e.ToString()})");
                    logException(errMsg, e);
                    NoLoad[metaDataName]= 1;
                    MetaProfiler.StopTimer(handle);
                    unrecoverableError = true;
                    return defaultMetaData( metaDataName) as WinFormMetaData;
                }
                MetaProfiler.StopTimer(handle);
                return md;
            }
            catch (Exception e) {
                logException($"Errore in caricamento {metaDataName}", e);
                MetaProfiler.StopTimer(handle);
                NoLoad[metaDataName]= 1;
                unrecoverableError = true;
                return defaultMetaData( metaDataName) as WinFormMetaData;
            }

        }


            /// <summary>
        /// Edit an entity (tablename) with a specified edit-type
        /// </summary>
        /// <param name="parent">Parent Form</param>
        /// <param name="metaDataName">name of primary table to edit</param>
        /// <param name="editName">logical name of form (edit-type)</param>
        /// <param name="modal">true if Form has to be opened in modal mode</param>
        /// <param name="param">Extra parameter to assign to MetaData before crating the form</param>
        /// <returns></returns>
        public virtual bool Edit(Form parent, string metaDataName, string editName, bool modal, object param) {
            var m = GetWinFormMeta(metaDataName);
            if (m==null) {
                //shower.ShowError(parent,
                //    LM.errorLoadingMeta(metaDataName),                                        
                //    LM.ErrorTitle);
                return false;
            }

            if (m == null) {
                MetaFactory.factory.getSingleton<IMessageShower>().Show($"No entity called {metaDataName} was found.");
                return false;
            }

            if (param != null) m.ExtraParameter = param;
            if (parent == null) return false;
            if (parent.IsDisposed) return false;
            var res = m.Edit(parent, editName, modal);
            if (modal) {
                m.Destroy();
            }
            return res;
        }

    }
}

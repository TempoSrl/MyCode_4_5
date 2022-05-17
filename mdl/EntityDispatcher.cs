using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mdl {

    /// <summary>
    /// Interface for meta data dispatcher
    /// </summary>
    public interface IEntityDispatcher:IMetaDataDispatcher {
      
   
       
    }

    /// <summary>
    /// Application Meta Data Dispatcher
    /// </summary>
    public class EntityDispatcher : MetaDataDispatcher, IEntityDispatcher {
      

        protected static Assembly loadAssembly(string name) {
            string folder = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            if (name.StartsWith("System.")) return Assembly.Load(name);
            return Assembly.LoadFrom(Path.Combine(folder, name + ".dll"));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>        
        public EntityDispatcher(IDataAccess conn) : base(conn) {

        }


        protected static readonly Hashtable NoLoad = new Hashtable();
        protected static readonly Hashtable LoadedAssembly = new Hashtable();


        protected static readonly Dictionary<string, bool> LoadedAss = new Dictionary<string, bool>();


        protected static readonly object MyLockMeta = new object();
        /// <summary>
        /// Errors encountered during dll load phase
        /// </summary>
        protected static readonly Dictionary<string,string> LoadError= new Dictionary<string,string>();

		/// <summary>
		/// Get folder where running dll are located
		/// </summary>
		/// <returns></returns>
        public string getDllFolder() {
            return AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
        }





        /// <summary>
        /// Gets a MetaData Class given it's name
        /// </summary>
        /// <param name="metaDataName"></param>
        /// <returns></returns>
        /// 
        public override MetaData Get(string metaDataName) {
           

            var handle = mdl_utils.metaprofiler.StartTimer("Get metaDataName * "+metaDataName);       

            if (NoLoad.Contains(metaDataName)) {
                mdl_utils.metaprofiler.StopTimer(handle);
                return defaultMetaData( metaDataName);
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
                        return defaultMetaData( metaDataName);
                    }
                    var handle2 = mdl_utils.metaprofiler.StartTimer("REAL Get * "+metaDataName);
                    try {
                        lock (MyLockMeta) {
                            a = loadAssembly( myAssemblyName);
                        }
                        LoadedAssembly[metaDataName] = a;
                    }
                    catch (FileNotFoundException f) {
                        LoadError[metaDataName] = QueryCreator.GetErrorString(f);
                        NoLoad[metaDataName] = 1;
                        doLog = false;
                    }
                    catch (Exception el) {
                        logException($"Errore caricando la DLL {myAssemblyName} che è quindi aggiunta a NOLOAD.", el);
                        LoadError[metaDataName] = QueryCreator.GetErrorString(el);
                        NoLoad[metaDataName] = 1;
                        unrecoverableError = true;
                    }
                    mdl_utils.metaprofiler.StopTimer(handle2);
                }

                if (a == null) {
                    //Conn.LogError(ErrMsg,null);
                    mdl_utils.metaprofiler.StopTimer(handle);
                    MetaData m =  defaultMetaData(metaDataName);
                    if (doLog) ErrorLogger.Logger.markEvent($"last Error during load:{LoadError[metaDataName]}");
                    return m;
                }
                var errMsg = $"Class {myClassName} not found in file {myAssemblyName}";
                Type metaObjType = a.GetType(myClassName);
                if (metaObjType == null) {
                    ErrorLogger.Logger.markEvent(errMsg);
                    NoLoad[metaDataName]=1;
                    unrecoverableError = true;
                    mdl_utils.metaprofiler.StopTimer(handle);
                    return defaultMetaData(metaDataName);
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
                    ErrorLogger.Logger.markEvent(errMsg);
                    logException(errMsg, null);
                    NoLoad[metaDataName]= 1;
                    unrecoverableError = true;
                    mdl_utils.metaprofiler.StopTimer(handle);
                    return defaultMetaData(metaDataName);
                }
                errMsg = $"Error calling constructor of Class {myClassName} in file {myAssemblyName}";

                MetaData md ;
                try {
                    md = (MetaData) metaObjBuilder.Invoke(parametri);
                }
                catch (Exception e) {
                    ErrorLogger.Logger.markEvent($"{errMsg}(Detail:{e.ToString()})");
                    logException(errMsg, e);
                    NoLoad[metaDataName]= 1;
                    mdl_utils.metaprofiler.StopTimer(handle);
                    unrecoverableError = true;
                    return defaultMetaData( metaDataName);
                }
                mdl_utils.metaprofiler.StopTimer(handle);
                return md;
            }
            catch (Exception e) {
                logException($"Errore in caricamento {metaDataName}", e);
                mdl_utils.metaprofiler.StopTimer(handle);
                NoLoad[metaDataName]= 1;
                unrecoverableError = true;
                return defaultMetaData( metaDataName);
            }

        }

    }
}

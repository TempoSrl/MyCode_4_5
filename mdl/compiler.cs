using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.IO;


namespace mdl {

    /// <summary>
    /// Helper class for compiling c# code
    /// </summary>
    public class Compiler {


        private static long _nfun ;

        /// <summary>
        /// Number of temporary vars declared
        /// </summary>
        public int Nvars;

        /// <summary>
        /// Retursn a new function name
        /// </summary>
        /// <returns></returns>
        public static string getNewFunName() {
            _nfun++;
            return $"dynamic_fun{_nfun}";
        }

        /// <summary>
        /// Get a new variable name
        /// </summary>
        /// <returns></returns>
        public string getNewVarName() {
            Nvars++;
            return $"o{Nvars}";
        }

        private readonly List<string> _segments = new List<string>();

        /// <summary>
        /// Adding a segment of code to compile
        /// </summary>
        /// <param name="segment"></param>
        public void addSegment(string segment) {
            _segments.Add(segment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName">Name of the class containing the method to compile</param>
        /// <param name="funcBody">Function body</param>
        /// <param name="referencedAssemblies"></param>
        /// <param name="referencedDll"></param>
        /// <returns></returns>
        public MethodInfo compile(string typeName, string funcBody, IEnumerable<string> referencedAssemblies, IEnumerable<string> referencedDll) {
            var assembly = compileAssembly(typeName, funcBody, referencedAssemblies, referencedDll);
            if (assembly == null) return null;
            var program = assembly.GetType(typeName);
            var main = program.GetMethod("apply");
            return main;
        }
        public static bool compileEnabled=true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeName">Name of the class containing the method to compile</param>
        /// <param name="funcBody">Function body</param>
        /// <param name="referencedAssemblies"></param>
        /// <param name="referencedDll"></param>
        /// <returns></returns>
        public Assembly compileAssembly(string typeName, string funcBody, IEnumerable<string> referencedAssemblies, IEnumerable<string> referencedDll) {
            if (!compileEnabled) return null;
            //int handler = metaprofiler.StartTimer("compileAssembly*" + typeName);
            var options = new Dictionary<string, string> {{ "CompilerVersion", "v4.0" } }; // 
            var provider = new CSharpCodeProvider(options);
            var parameters = new CompilerParameters {
                GenerateInMemory = true,    //false - external file generation
		        GenerateExecutable = false,
                TempFiles =  new TempFileCollection(Path.GetTempPath(),false) // new TempFileCollection(".",false)
	        };

            var code = new StringBuilder();
            foreach (var ass in referencedAssemblies) {
                code.AppendLine($"using {ass};");
            }
            foreach (var ass in referencedDll) {
                parameters.ReferencedAssemblies.Add(ass);
            }

            parameters.ReferencedAssemblies.Add(typeof(MetaData).Assembly.Location);

            foreach (var segment in _segments) {
                code.AppendLine(segment);
            }
            code.AppendLine(funcBody);
            try {
                var results = provider.CompileAssemblyFromSource(parameters, code.ToString());
                provider.Dispose();
                if (results.Errors.HasErrors) {
                    var sb = new StringBuilder();
                    foreach (CompilerError error in results.Errors) {
                        sb.AppendLine($"Error (Line {error.Line} {error.ErrorNumber}): {error.ErrorText} \n\r");
                        sb.AppendLine($"column{error.Column}");
                        sb.AppendLine($"code:");
                        sb.AppendLine($"{code}");
                    }

                    QueryCreator.MarkEvent(sb.ToString());
                    //metaprofiler.StopTimer(handler);
                    return null;
                }
                return results.CompiledAssembly;
            }
            catch (Exception e) {
                compileEnabled = false;
                ErrorLogger.Logger.logException("Compiling " + code, e);
                return null;
            }

            //metaprofiler.StopTimer(handler);

        }
    }


}



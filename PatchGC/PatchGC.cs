using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;

namespace PatchGC
{
    class PatchGC
    {
        private readonly Log log = new Log("PatchGC");

        public void Run()
        {
            try
            {
                var infilename = "mscorlib.orig.dll";
                var outfilename = "mscorlib.dll";
                if (!File.Exists(infilename))
                {
                    log.Error("File {0} not found! Please run me in KSP_DATA\\Managed", infilename);
                    return;
                }
                var asm = AssemblyDefinition.ReadAssembly(infilename);

                var asmLib = AssemblyDefinition.ReadAssembly("GCHelperLib.dll");
                TypeDefinition tUtils = asmLib.MainModule.GetType("GCHelperLib.Utils");
                MethodDefinition startFunc = tUtils.Methods.First(x => x.Name == "StartCollect");
                MethodDefinition stopFunc = tUtils.Methods.First(x => x.Name == "StopCollect");

                DumpWholeFunction(asm, "System.GC", "Collect");

                PatchCollectFuncs(asm, startFunc, stopFunc);

                DumpWholeFunction(asm, "System.GC", "Collect");

                log.Debug("Writing file {0}", outfilename);
                asm.Write(outfilename);

                log.Info("Patched file created.");
            }
            catch (Exception e)
            {
                log.Error("Exception while trying to patch assembly: {0}", e.Message);
            }
        }

        private void PatchCollectFuncs(AssemblyDefinition asm, MethodDefinition startFunc, MethodDefinition stopFunc)
        {
            TypeDefinition type = asm.MainModule.GetType("System.GC");

            MethodDefinition func = type.Methods.First(x => x.Name == "Collect" && x.Parameters.Count == 0);

            var insList = func.Body.Instructions;
            ILProcessor proc = func.Body.GetILProcessor();

            var callStart = proc.Create(OpCodes.Call, func.Module.Import(startFunc));
            var callStop = proc.Create(OpCodes.Call, func.Module.Import(stopFunc));

            proc.InsertBefore(insList[insList.Count - 1], callStop);
            proc.InsertBefore(insList[0], callStart);

            func = type.Methods.First(x => x.Name == "Collect" && x.Parameters.Count == 1);

            insList = func.Body.Instructions;
            proc = func.Body.GetILProcessor();

            callStart = proc.Create(OpCodes.Call, func.Module.Import(startFunc));
            callStop = proc.Create(OpCodes.Call, func.Module.Import(stopFunc));

            proc.InsertBefore(insList[insList.Count - 1], callStop);
            proc.InsertBefore(insList[0], callStart);
        }

        private void DumpWholeFunction(AssemblyDefinition asmdef, string typeName, string functionName)
        {
            var type = asmdef.MainModule.GetType(typeName);
            var func = type.Methods.First(x => x.Name == functionName);

            var instrList = func.Body.Instructions;

            foreach (var instr in instrList)
            {
                Console.WriteLine("Offset {0}: {1}", instr.Offset, EncodeNonAsciiCharacters(instr.ToString()));
            }
        }

        // http://stackoverflow.com/a/1615860
        private static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c < 32 || c > 127)
                {
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        static void Main(string[] args)
        {
            new PatchGC().Run();
        }
    }


    public class Log
    {
        private static readonly string ns = typeof(Log).Namespace;
        private readonly string id = String.Format("{0:X8}", Guid.NewGuid().GetHashCode());
        private readonly string name;

        public Log(string name)
        {
            this.name = name;
        }

        private void Print(string level, string message, params object[] values)
        {
            Console.WriteLine("[" + name + ":" + level + ":" + id + "]  " + String.Format(message, values));
        }

        public void Debug(string message, params object[] values)
        {
            Print("DEBUG", message, values);
        }

        public void Info(string message, params object[] values)
        {
            Print("INFO", message, values);
        }

        public void Error(string message, params object[] values)
        {
            Print("ERROR", message, values);
        }
    }
}

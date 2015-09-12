# PatchMonoGC

This small utility provides a way of finding out the time taken by the last call to System.GC.Collect

It does this by using Mono.Cecil to patch the functions in mscorlib.dll to call functions in GCHelperLib that measure the time.

This can then be queried from a KSP plugin by calling GCHelperLib.Utils.GetLastTime.
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public static class Patcher
{
    // List of assemblies to patch
    // Called DURING patching
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll", "BeppyServer.dll" };

    // Patches the assemblies
    public static void Patch(AssemblyDefinition assembly)
    {

        Console.WriteLine("Patch");

        // Patches WinFormConnection class with the following:
        //  static field "Instance"
        //  constructor ILcode to set "Instance" to itself.
        if (assembly.Name.Name == "Assembly-CSharp.dll")
        {
            TypeDefinition wfcDefinition = assembly.MainModule.Types.First(t => t.Name == "WinFormConnection");
            Console.WriteLine(wfcDefinition);
            MethodDefinition wfcConstructor = wfcDefinition.GetConstructors().First();
            Console.WriteLine(wfcConstructor);

            wfcDefinition = wfcDefinition.Resolve();

            FieldDefinition wfcInstance = new FieldDefinition("Instance", FieldAttributes.Static | FieldAttributes.Public, wfcDefinition);
            Console.WriteLine(wfcInstance);
            wfcDefinition.Fields.Add(wfcInstance);

            wfcInstance = wfcInstance.Resolve();

            ILProcessor proc = wfcConstructor.Body.GetILProcessor();

            var pushInstance = proc.Create(OpCodes.Ldarg_0);
            var allocInstance = proc.Create(OpCodes.Stsfld, wfcInstance);

            proc.InsertBefore(proc.Body.Instructions.Last(), allocInstance);
            proc.InsertBefore(allocInstance, pushInstance);
            foreach (var instruction in proc.Body.Instructions)
            {
                Console.WriteLine(instruction);
            }

            wfcConstructor = wfcConstructor.Resolve();
        }
    }
}
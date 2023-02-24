using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceWarp;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ksp2_mod_loader_patcher
{
	internal class Program
	{
		static readonly string[] DLLS_TO_TRANSFER = new string[]
		{
			"SpaceWarp.dll",
		};

		static void Main(string[] args)
		{
			string pathToDll = args[0];
			string dirPath = Path.GetDirectoryName(pathToDll) + "/";

			foreach(string dll in DLLS_TO_TRANSFER)
			{
				File.Copy(dll, dirPath + dll, true);
			}

			var module = ModuleDefinition.ReadModule(pathToDll, new ReaderParameters()
			{
				ReadWrite = true,
				AssemblyResolver = new CustomResolver(dirPath)
			});

			HookInjector.Hook(module, "KSP.Game.GameManager", "StartGame");

			module.Write();
			module.Dispose();
		}
	}

	public static class HookInjector
	{
		public static bool Hook(ModuleDefinition module, string targetTypeName, string targetFunctionName)
		{
			var target_type = module.Types.First(mod => mod.FullName == targetTypeName);
			var target_method = target_type.Methods.First(method => method.Name == targetFunctionName);

			MethodReference methodReference = module.ImportReference(typeof(StartupManager).GetMethod("OnGameStarted"));

			var processor = target_method.Body.GetILProcessor();
			var newInstruction = processor.Create(OpCodes.Call, methodReference);
			var firstInstruction = target_method.Body.Instructions[0];

			// we've already injected, no need to do it again
			if (firstInstruction.OpCode == OpCodes.Call &&
				firstInstruction.Operand is MethodReference oldReference &&
				oldReference.FullName == methodReference.FullName)
			{
				return false;
			}

			processor.InsertBefore(firstInstruction, newInstruction);

			return true;
		}
	}

	class CustomResolver : BaseAssemblyResolver
	{
		private string _path;

		public CustomResolver(string path)
		{
			_path = path;
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return AssemblyDefinition.ReadAssembly(_path + name.Name + ".dll");
		}
	}
}

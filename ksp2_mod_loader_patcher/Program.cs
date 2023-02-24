using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mod_loader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ksp2_mod_loader_patcher
{
	internal class Program
	{
		const string MOD_LOADER_DLL_NAME = "mod_loader.dll";

		static void Main(string[] args)
		{
			string pathToDll = args[0];
			string dirPath = Path.GetDirectoryName(pathToDll) + "/";

			File.Copy(MOD_LOADER_DLL_NAME, dirPath + MOD_LOADER_DLL_NAME, true);

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
		public static void Hook(ModuleDefinition module, string targetTypeName, string targetFunctionName)
		{
			var target_type = module.Types.First(mod => mod.FullName == targetTypeName);
			var target_method = target_type.Methods.First(method => method.Name == targetFunctionName);

			MethodReference methodReference = module.ImportReference(typeof(StartupManager).GetMethod("OnGameStarted"));

			var processor = target_method.Body.GetILProcessor();
			var newInstruction = processor.Create(OpCodes.Call, methodReference);
			var firstInstruction = target_method.Body.Instructions[0];

			processor.InsertBefore(firstInstruction, newInstruction);
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
			AssemblyDefinition assembly;
			try
			{
				assembly = AssemblyDefinition.ReadAssembly(_path + name.Name + ".dll");
			}
			catch (AssemblyResolutionException ex)
			{
				assembly = null;// ...; // Your resolve logic   
			}
			return assembly;
		}
	}
}

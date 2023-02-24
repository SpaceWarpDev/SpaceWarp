using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceWarp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Threading;

namespace ksp2_mod_loader_patcher
{
	internal class Program
	{
		static readonly string[] DLLS_TO_TRANSFER = new string[]
		{
			"SpaceWarp.dll",
		};

		public const string EXPECTED_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Kerbal Space Program 2";
		public const string OFFSET_INTO_GAME_PATH = "\\KSP2_x64_Data\\Managed\\Assembly-CSharp.dll";

		public const string TARGET_TYPE_NAME = "KSP.Game.GameManager";
		public const string TARGET_METHOD_NAME = "StartGame";

		static void Main(string[] args)
		{
			string pathToDll = LocateGameDirectory(args);

			string dirPath = Path.GetDirectoryName(pathToDll) + "/";

			foreach(string dllPath in DLLS_TO_TRANSFER)
			{
				string targetPath = dirPath + dllPath;

				Console.WriteLine($"Copying {dllPath} to {targetPath}");

				File.Copy(dllPath, targetPath, true);
			}

			var module = ModuleDefinition.ReadModule(pathToDll, new ReaderParameters()
			{
				ReadWrite = true,
				AssemblyResolver = new CustomResolver(dirPath)
			});

			bool patched = HookInjector.Hook(module, TARGET_TYPE_NAME, TARGET_METHOD_NAME);
			if (patched)
			{
				Console.WriteLine($"Patched into {TARGET_TYPE_NAME}.{TARGET_METHOD_NAME}");
			}
			else
			{
				Console.WriteLine("Did not patch anything, patch already present");
			}

			Console.WriteLine("Writing to file...");
			module.Write();
			module.Dispose();
			Console.WriteLine("Done!");

			Thread.Sleep(1000);
		}

		static string LocateGameDirectory(string[] args)
		{
			string pathToDll;

			if (args.Length == 0)
			{
				if (Directory.Exists(EXPECTED_PATH))
				{
					pathToDll = EXPECTED_PATH + OFFSET_INTO_GAME_PATH;
				}
				else
				{
					do
					{
						Console.WriteLine("Unable to locate game install, please specify it");

						pathToDll = Console.ReadLine().Trim('\"');
					} while (!Directory.Exists(pathToDll));
				}
			}
			else
			{
				pathToDll = args[0];
			}

			Console.WriteLine($"Found game path at {pathToDll}");
			return pathToDll;
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

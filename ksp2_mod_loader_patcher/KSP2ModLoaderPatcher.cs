using System;
using System.IO;
using System.Linq;
using SpaceWarp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Threading;

namespace ksp2_mod_loader_patcher
{
	/// <summary>
	/// Class that injects the Ksp2ModLoader dll into the KSP2 game manager assembly.
	/// </summary>
	internal static class Ksp2ModLoaderPatcher
	{
		static readonly string[] DLLS_TO_TRANSFER = 
		{
			"SpaceWarp.dll",
		};

		private const string EXPECTED_PATH = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Kerbal Space Program 2";
		private const string OFFSET_INTO_GAME_PATH = "\\KSP2_x64_Data\\Managed\\Assembly-CSharp.dll";

		private const string TARGET_TYPE_NAME = "KSP.Game.GameManager";
		private const string TARGET_METHOD_NAME = "StartGame";

		private static void Main(string[] args)
		{
			string pathToDll = LocateGameDirectory(args) + OFFSET_INTO_GAME_PATH;

			string dirPath = Path.GetDirectoryName(pathToDll) + "/";

			foreach(string dllPath in DLLS_TO_TRANSFER)
			{
				string targetPath = dirPath + dllPath;

				Console.WriteLine($"Copying {dllPath} to {targetPath}");

				File.Copy(dllPath, targetPath, true);
			}

			ModuleDefinition module = ModuleDefinition.ReadModule(pathToDll, new ReaderParameters
			{
				ReadWrite = true,
				AssemblyResolver = new CustomResolver(dirPath)
			});

			bool patched = HookInjector.Hook(module, TARGET_TYPE_NAME, TARGET_METHOD_NAME);

			Console.WriteLine(patched ? $"Patched into {TARGET_TYPE_NAME}.{TARGET_METHOD_NAME}" : "Did not patch anything, patch already present");
			Console.WriteLine("Writing to file...");

			module.Write();
			module.Dispose();
			
			Console.WriteLine("Patched! You can now open KSP2 and the SpaceWarp modloader will be loaded!");

			Thread.Sleep(10000);
		}

		private static string LocateGameDirectory(string[] args)
		{
			string pathToGameInstall;

			if (args.Length == 0)
			{
				if (Directory.Exists(EXPECTED_PATH))
				{
					pathToGameInstall = EXPECTED_PATH;
				}
				else
				{
					do
					{
						Console.WriteLine("Unable to locate game install, please specify it");

						pathToGameInstall = Console.ReadLine()?.Trim('\"');
					} while (!Directory.Exists(pathToGameInstall));
				}
			}
			else
			{
				pathToGameInstall = args[0];
			}

			Console.WriteLine($"Found game path at \"{pathToGameInstall}\"");
			return pathToGameInstall;
		}
	}

	public static class HookInjector
	{
		public static bool Hook(ModuleDefinition module, string targetTypeName, string targetFunctionName)
		{
			TypeDefinition targetType = module.Types.First(mod => mod.FullName == targetTypeName);
			MethodDefinition targetMethod = targetType.Methods.First(method => method.Name == targetFunctionName);

			MethodReference methodReference = module.ImportReference(typeof(StartupManager).GetMethod("OnGameStarted"));

			ILProcessor processor = targetMethod.Body.GetILProcessor();
			Instruction newInstruction = processor.Create(OpCodes.Call, methodReference);
			Instruction firstInstruction = targetMethod.Body.Instructions[0];

			MethodReference oldReference = firstInstruction.Operand as MethodReference;

			bool isMethodReference = firstInstruction.Operand is MethodReference;
			bool namesAreEqual = oldReference?.FullName == methodReference.FullName;
			bool firstInstructionIsCall = firstInstruction.OpCode == OpCodes.Call;

			// we've already injected, no need to do it again
			if (firstInstructionIsCall && isMethodReference && namesAreEqual)
			{
				return false;
			}

			processor.InsertBefore(firstInstruction, newInstruction);
			return true;
		}
	}

	class CustomResolver : BaseAssemblyResolver
	{
		private readonly string _path;

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

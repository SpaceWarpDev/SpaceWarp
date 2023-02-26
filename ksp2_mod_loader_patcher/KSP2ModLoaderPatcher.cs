using System;
using System.IO;
using System.Linq;
using SpaceWarp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;

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

		private const string OFFSET_INTO_GAME_PATH = "/" + DATA_FOLDER_NAME + "/" + MANAGED_FOLDER_NAME;
		public const string TARGET_ASSEMBLY_NAME = "Assembly-CSharp.dll";
		
		const string DATA_FOLDER_NAME = "KSP2_x64_Data";
		const string MANAGED_FOLDER_NAME = "Managed";

		const string MODS_FOLDER_NAME = "Mods";


		static void Main(string[] args)
		{
			string asmLocation = getGameInstallLocation(args);
			asmLocation = asmLocation.TrimEnd('/', '\\');

			Console.WriteLine("Selected game install location!");

			foreach (string dllPath in DLLS_TO_TRANSFER)
			{
				string targetPath = asmLocation + "/" + dllPath;

				Console.WriteLine($"Copying {dllPath} to {targetPath}");

				File.Copy(dllPath, targetPath, true);
			}
			string modsFolderDirectory = Path.GetDirectoryName(asmLocation) + "/" + MODS_FOLDER_NAME;
			if (Directory.Exists(modsFolderDirectory))
			{
				Console.WriteLine("Created mods folder!");
				Directory.CreateDirectory(modsFolderDirectory);
			}

			using (Patcher patcher = new Patcher(asmLocation, TARGET_ASSEMBLY_NAME))
			{
				patcher.TryRegisterHook("KSP.Game.GameManager", "StartGame", typeof(StartupManager).GetMethod("OnGameStarted"));

				if (patcher.GetPatchedState() != PatchState.Full)
				{
					Console.WriteLine("Patching hooks...");
					patcher.PatchAllUnpatched();
					patcher.Write();
					Console.WriteLine("Done!");
				}
				else
				{
					string response;
					do
					{
						Console.WriteLine("SpaceWard is already patched into the game, do you want to unpatch? y/n");
						response = Console.ReadLine().ToLowerInvariant();
					} while (response != "y" && response != "n");

					if (response == "y")
					{
						Console.WriteLine("Unpatching hooks...");
						patcher.UnpatchAllPatched();
						patcher.Write();
						Console.WriteLine("Done!");
					}

				}
			}

			Console.WriteLine("Press any key to continue");
			Console.ReadKey();
		}

		static string getGameInstallLocation(string[] args)
		{
			if (args.Length != 0)
			{
				string argPath = args[0];
				argPath = argPath.TrimEnd('/', '\\');

				if (tryGetGameInstallLocationFromCustomPath(argPath, out string result))
				{
					return result;
				}

				Console.WriteLine("The provided path was not a valid game install location");
			}

			if (SteamInstallLocationFinder.TryGetInstallPath(out string path))
			{
				string result;
				do
				{
					Console.WriteLine($"Found kerbal installation at \"{path}\", install Space Warp here? y/n");
					result = Console.ReadLine().ToLowerInvariant();
				}
				while (result != "y" && result != "n");

				if (result == "y")
				{
					return path + OFFSET_INTO_GAME_PATH;
				}
			}
			else
			{
				Console.WriteLine("Unable to find kerbal installation");
			}

			Console.WriteLine("Please specify the path to kerbal");
			string output_path = null;
			do
			{
				string content = Console.ReadLine();
				if (tryGetGameInstallLocationFromCustomPath(content, out string output))
				{
					output_path = output;
				}

			} while (output_path != null);

			return output_path;
		}

		static bool tryGetGameInstallLocationFromCustomPath(string path, out string result)
		{
			path = path.TrimEnd('/', '\\');
			if (path.EndsWith(".dll"))
			{
				path = Path.GetDirectoryName(path);
			}

			if (isValidGameLocation(path))
			{
				result = path + OFFSET_INTO_GAME_PATH;
				return true;
			}
			else if (isValidGameLocation(Path.GetDirectoryName(path)))
			{
				result = Path.GetDirectoryName(path) + OFFSET_INTO_GAME_PATH;
				return true;
			}
			else if (isValidGameLocation(Path.GetDirectoryName(Path.GetDirectoryName(path))))
			{
				result = Path.GetDirectoryName(Path.GetDirectoryName(path)) + OFFSET_INTO_GAME_PATH;
				return true;
			}

			result = null;
			return false;
		}

		static bool isValidGameLocation(string path)
		{
			path = path.TrimEnd('/', '\\');

			if (!Directory.Exists(path))
				return false;

			if (!Directory.Exists(path + "/" + DATA_FOLDER_NAME))
				return false;

			if (!Directory.Exists(path + "/" + DATA_FOLDER_NAME + "/" + MANAGED_FOLDER_NAME))
				return false;

			if (!File.Exists(path + "/" + DATA_FOLDER_NAME + "/" + MANAGED_FOLDER_NAME + "/" + TARGET_ASSEMBLY_NAME))
				return false;

			return true;
		}

	}

	public class Patcher : IDisposable
	{
		ModuleDefinition _module;
		List<RegisterdHook> _hooks = new List<RegisterdHook>();

		public Patcher(string managedFolderPath, string targetAsmName)
		{
			managedFolderPath = managedFolderPath.TrimEnd('/', '\\');

			_module = ModuleDefinition.ReadModule(managedFolderPath + "/" + targetAsmName, new ReaderParameters
			{
				ReadWrite = true,
				AssemblyResolver = new CustomResolver(managedFolderPath)
			});
		}

		public bool TryRegisterHook(string targetTypeName, string targetFunctionName, MethodInfo callTarget)
		{
			TypeDefinition targetType = _module.Types.FirstOrDefault(mod => mod.FullName == targetTypeName);
			
			// type not found with the provided name, renamed / removed?
			if(targetType == null)
			{
				return false;
			}

			MethodDefinition targetMethod = targetType.Methods.FirstOrDefault(method => method.Name == targetFunctionName);

			// method not found with the provided name, renamed / removed?
			if (targetType == null)
			{
				return false;
			}

			var reference = _module.ImportReference(callTarget);

			_hooks.Add(new RegisterdHook(targetMethod, reference));
			return true;
		}

		public PatchState GetPatchedState()
		{
			int donePatches = 0;

			foreach (RegisterdHook hook in _hooks)
			{
				if (hook.IsPatched())
				{
					donePatches++;
				}
			}

			if (donePatches == 0)
			{
				return PatchState.None;
			}
			else if (donePatches == _hooks.Count)
			{
				return PatchState.Full;
			}
			else
			{
				return PatchState.Partial;
			}
		}

		public void PatchAllUnpatched()
		{
			foreach(RegisterdHook hook in _hooks)
			{
				if (!hook.IsPatched())
				{
					hook.Patch();
				}
			}
		}

		public void UnpatchAllPatched()
		{
			foreach (RegisterdHook hook in _hooks)
			{
				if (hook.IsPatched())
				{
					hook.Unpatch();
				}
			}
		}

		class RegisterdHook
		{
			public MethodDefinition Target;
			public MethodReference CallTarget;

			public RegisterdHook(MethodDefinition target, MethodReference callTarget)
			{
				Target = target;
				CallTarget = callTarget;
			}

			public bool IsPatched()
			{
				Instruction firstInstruction = Target.Body.Instructions[0];

				MethodReference oldReference = firstInstruction.Operand as MethodReference;
				if (oldReference == null)
					return false;

				return firstInstruction.OpCode == OpCodes.Call &&
					firstInstruction.Operand is MethodReference &&
					oldReference.FullName == CallTarget.FullName;
			}
			public void Patch()
			{
				ILProcessor processor = Target.Body.GetILProcessor();
				Instruction newInstruction = processor.Create(OpCodes.Call, CallTarget);
				Instruction firstInstruction = Target.Body.Instructions[0];

				processor.InsertBefore(firstInstruction, newInstruction);
			}

			public void Unpatch()
			{
				ILProcessor processor = Target.Body.GetILProcessor();

				processor.RemoveAt(0);
			}
		}

		public void Write()
		{
			_module.Write();
		}

		public void Dispose()
		{
			_module.Dispose();
		}
	}

	public enum PatchState
	{
		None,
		Partial,
		Full
	}

	class CustomResolver : BaseAssemblyResolver
	{
		private readonly string _path;

		public CustomResolver(string path)
		{
			path = path.TrimEnd('/', '\\');

			_path = path;
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return AssemblyDefinition.ReadAssembly(_path + "/" + name.Name + ".dll");
		}
	}
}

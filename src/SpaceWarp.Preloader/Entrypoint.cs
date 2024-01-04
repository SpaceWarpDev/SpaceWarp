using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace SpaceWarp.Preloader;

internal static class Entrypoint
{
	private static string _gameFolder;

	[UsedImplicitly]
	public static void Main(string[] args)
	{
		_gameFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;

		StartBepinex();
	}

	private static void StartBepinex()
	{
		var bepinexFolder = Path.Combine(_gameFolder, "BepInEx", "core");
		var bepinexPreloaderPath = Path.Combine(bepinexFolder, "BepInEx.Preloader.dll");

		Assembly.LoadFile(bepinexPreloaderPath);
		BepInEx.Preloader.Entrypoint.Main();
	}
}
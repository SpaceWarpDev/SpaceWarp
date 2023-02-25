using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWarp.API.Loading
{
	public class AssemblyLoading
	{
		void LoadAssembly(string path)
		{
			Assembly assebmly = Assembly.LoadFile(path);

			Type mainModClass = assebmly.GetTypes().FirstOrDefault(type => type.GetCustomAttribute<MainModAttribute>() != null);
			if (mainModClass == null)
			{
				throw new Exception($"No class found with [{nameof(MainModAttribute)}] found");
			}

			if (!inheritsFrom(mainModClass, typeof(Mod)))
			{
				throw new Exception($"The found class \"{mainModClass.FullName}\" doesn't inherit from \"{nameof(Mod)}\"");
			}

		}


		static bool inheritsFrom(Type type, Type targetBase)
		{
			while(type.BaseType != null)
			{
				if (type.BaseType == targetBase)
					return true;

				type = type.BaseType;
			}

			return false;
		}
	}
}

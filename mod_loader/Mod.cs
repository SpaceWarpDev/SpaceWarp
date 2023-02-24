using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mod_loader
{
	public class MainModAttribute : Attribute
	{
	}

	public abstract class Mod
	{
		public abstract void OnLoaded();
	}
}

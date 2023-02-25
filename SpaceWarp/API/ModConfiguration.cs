using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpaceWarp.API
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ModConfiguration
	{
		[JsonProperty]
		public string Name { get; set; }
		
		[JsonProperty]
		public string ModAssemblyName { get; set; }
	}
}

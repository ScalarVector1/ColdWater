global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ColdWater.Core;
global using Terraria;
global using Terraria.Localization;
global using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ColdWater
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class ColdWater : Mod
	{
		public static ColdWater Instance { get; private set; }

		public ColdWater()
		{
			Instance = this;
		}
	}
}

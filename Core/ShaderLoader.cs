using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using static Terraria.ModLoader.Core.TmodFile;

namespace ColdWater.Core
{
	class ShaderLoader : ModSystem
	{
		private static readonly Dictionary<string, Lazy<Asset<Effect>>> shaders = [];

		public override void Load()
		{
			if (Main.dedServ)
				return;

			TmodFile file = ColdWater.Instance.File;

			IEnumerable<FileEntry> shaders = file.Where(n => n.Name.StartsWith("Effects/") && n.Name.EndsWith(".xnb"));

			foreach (FileEntry entry in shaders)
			{
				string name = entry.Name.Replace(".xnb", "").Replace("Effects/", "");
				string path = entry.Name.Replace(".xnb", "");
				LoadShader(name, path);
			}
		}

		public static Asset<Effect> GetShader(string key)
		{
			if (shaders.ContainsKey(key))
			{
				return shaders[key].Value;
			}
			else
			{
				LoadShader(key, $"Effects/{key}");
				return shaders[key].Value;
			}
		}

		public static void LoadShader(string name, string path)
		{
			if (!shaders.ContainsKey(name))
				shaders.Add(name, new(() => ColdWater.Instance.Assets.Request<Effect>(path)));
		}
	}
}

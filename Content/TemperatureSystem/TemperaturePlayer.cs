using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ColdWater.Content.TemperatureSystem
{
	internal class TemperaturePlayer : ModPlayer
	{
		public const int MAX_TEMP = 100000;

		public int temperature = 80000;

		public int damageThresh = 20000;
		public int toolThresh = 70000;

		public override void PostUpdate()
		{
			damageThresh = 20000;
			toolThresh = 70000;

			bool backCovered = Player.behindBackWall || Player.Center.Y > Main.worldSurface * 16;

			int lossRate = 0;

			if (!backCovered && Main.raining)
			{
				lossRate += (int)(5 * Main.cloudAlpha);
			}

			if (Player.wet)
			{
				lossRate += 50;
			}

			if (temperature > lossRate)
				temperature -= lossRate;
			else
				temperature = 0;

			if (lossRate == 0)
			{
				if (temperature < MAX_TEMP)
					temperature += 1;
			}

			if (temperature > MAX_TEMP)
				temperature = MAX_TEMP;

			if (temperature <= 0)
				Player.Hurt(PlayerDeathReason.ByCustomReason(new NetworkText($"{Player.name} froze to death.", NetworkText.Mode.Literal)), 999999, 0);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["temperature"] = temperature;
		}

		public override void LoadData(TagCompound tag)
		{
			temperature = tag.GetInt("temperature");
		}
	}
}

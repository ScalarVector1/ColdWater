using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ColdWater.Content.WaterStyle
{
	internal class ColdWaterSceneEffect : ModSceneEffect
	{
		public override bool IsSceneEffectActive(Player player)
		{
			return true;
		}

		public override float GetWeight(Player player)
		{
			return 1.0f;
		}

		public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

		public override ModWaterStyle WaterStyle => ModContent.GetInstance<ColdWaterStyle>();

		public override void Load()
		{
			On_Main.CalculateWaterStyle += ForceWaterStyle;
		}

		private int ForceWaterStyle(On_Main.orig_CalculateWaterStyle orig, bool ignoreFountains)
		{
			return ModContent.GetInstance<ColdWaterStyle>().Slot;
		}
	}
}

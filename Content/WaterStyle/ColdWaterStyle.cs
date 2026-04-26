using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace ColdWater.Content.WaterStyle
{
	internal class ColdWaterStyle : ModWaterStyle
	{
		public override string Texture => "ColdWater/Assets/MagicPixel";
		public override string BlockTexture => "ColdWater/Assets/MagicPixel";
		public override string SlopeTexture => "ColdWater/Assets/MagicPixel";

		public override Asset<Texture2D> GetRainTexture()
		{
			return Assets.MagicPixel;
		}

		public override int ChooseWaterfallStyle()
		{
			return 0;
		}

		public override int GetDropletGore()
		{
			return ModContent.DustType<ColdWaterDust>();
		}

		public override int GetSplashDust()
		{
			return ModContent.DustType<ColdWaterDust>();
		}

		public override void LightColorMultiplier(ref float r, ref float g, ref float b)
		{
			r = 1.1f;
			g = 1.1f;
			b = 1.1f;
		}
	}
}

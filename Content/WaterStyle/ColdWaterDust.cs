using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace ColdWater.Content.WaterStyle
{
	internal class ColdWaterDust : ModDust
	{
		public override string Texture => "ColdWater/Assets/Invisible";

		public override bool Update(Dust dust)
		{
			dust.fadeIn++;
			dust.position += dust.velocity;

			if (dust.fadeIn == 1)
			{
				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.position.Y += 16;

				dust.velocity.Y -= 2;
			}

			dust.velocity.Y += 0.4f;
			dust.velocity.X *= 0.9f;

			if (dust.velocity.Y > 0)
				dust.velocity = (Collision.TileCollision(dust.position, dust.velocity, 2, 2));

			if (dust.velocity.Y == 0)
				dust.fadeIn += 3;

			if (dust.fadeIn > 60)
				dust.active = false;

			return false;
		}
	}
}

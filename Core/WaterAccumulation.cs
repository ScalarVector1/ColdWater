using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ColdWater.Core
{
	internal class WaterAccumulation : ModSystem
	{
		public static int waterTimer;

		public int ticksTillWater => 30 + (int)((1f - Main.cloudAlpha) * 120);

		public int pickXNearPlayer()
		{
			var activePlayers = Main.player.Where(n => n.active).ToArray();
			var center = (int)(activePlayers[Main.rand.Next(activePlayers.Length)].Center.X / 16);

			int toReturn = center + Main.rand.Next(-100, 100);

			return Math.Clamp(toReturn, 0, Main.maxTilesX);
		}

		public override void PostUpdateEverything()
		{
			waterTimer++;

			if (Main.raining)
			{
				if (waterTimer > ticksTillWater)
				{
					int x = pickXNearPlayer();

					for (int y = 0; y < Main.maxTilesY; y++)
					{
						var tile = Framing.GetTileSafely(x, y);
						if ((tile.HasTile && Main.tileSolid[tile.TileType]) || (tile.LiquidType == 0 && tile.LiquidAmount > 0))
						{
							WorldGen.PlaceLiquid(x, y - 1, 0, 255);

							waterTimer = 0;
							return;
						}
					}

					waterTimer = 0;
				}
			}
			else
			{
				if (waterTimer > 10)
				{
					int x = pickXNearPlayer();

					for (int y = 0; y < Main.maxTilesY; y++)
					{
						var tile = Framing.GetTileSafely(x, y);

						if (tile.LiquidType == 0 && tile.LiquidAmount != 0)
						{
							tile.LiquidAmount = 0;

							WorldGen.SquareTileFrame(x, y);
							if (Main.netMode != 0)
							{
								NetMessage.sendWater(x, y);
							}

							for (int k = 0; k < 6; k++)
								Dust.NewDustPerfect(new Microsoft.Xna.Framework.Vector2(x, y) * 16, DustID.SteampunkSteam, new Microsoft.Xna.Framework.Vector2(Main.rand.NextFloat(-1, 1), -Main.rand.NextFloat(4)));

							waterTimer = 0;
							return;
						}
					}

					waterTimer = 0;
				}
			}
		}
	}
}

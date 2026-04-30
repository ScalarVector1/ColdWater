using ColdWater.Core.UndergroundLevelSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace ColdWater.Content.Levels
{
	internal class DemoLevel : UndergroundLevel
	{
		public override void SetDefaults()
		{
			descentDuration = 300;
			backgroundTexture = Assets.Background.DirtScroll;
			backgroundEdgeTexture = Assets.Background.DirtSide;
		}

		public override void UpdateInDescent()
		{
			if (UndergroundLevelModSystem.descendCounter % 60 == 0)
				NPC.NewNPC(null, (int)UndergroundLevelModSystem.DescendingBaseWorldCenter.X, (int)UndergroundLevelModSystem.DescendingBaseWorldCenter.Y, NPCID.Zombie);
		}

		public override void GenerateLevel(Rectangle region)
		{
			for(int x = region.X; x < region.X + region.Width; x++)
			{
				for (int y = region.Y; y < region.Y + region.Height; y++)
				{
					var tile = Main.tile[x, y];
					tile.ClearEverything();
					tile.TileType = TileID.WoodBlock;
					tile.HasTile = true;
				}
			}
		}
	}
}

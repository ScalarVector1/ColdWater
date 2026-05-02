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
			loopTexture = Assets.Background.DirtLoop;
			edgeTexture = Assets.Background.DirtEdge;
		}

		public override void UpdateInDescent()
		{
			canFinishDescent = Main.LocalPlayer.controlHook;

			if (UndergroundLevelModSystem.descendCounter % 600 == 0)
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

		public override void DrawMapIcon(SpriteBatch spriteBatch, Vector2 center, float scale)
		{
			var tex = Assets.UI.DirtLevelDetails.Value;
			spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, scale, 0, 0);
		}
	}

	internal class DemoLevel2 : DemoLevel
	{
		public override void SetDefaults()
		{
			descentDuration = 300;
			backgroundTexture = Assets.Background.DirtScroll;
			backgroundEdgeTexture = Assets.Background.DirtSide;
			loopTexture = Assets.Background.DirtLoop;
			edgeTexture = Assets.Background.DirtEdge;
			mapLocation = new Vector2(-100, 200);
			possiblePrevious.Add(ModContent.GetInstance<DemoLevel>());
		}

		public override void DrawMapIcon(SpriteBatch spriteBatch, Vector2 center, float scale)
		{
			var tex = Assets.UI.DemoLevel2Details.Value;
			spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, scale, 0, 0);
		}
	}

	internal class DemoLevel3 : DemoLevel
	{
		public override void SetDefaults()
		{
			descentDuration = 300;
			backgroundTexture = Assets.Background.DirtScroll;
			backgroundEdgeTexture = Assets.Background.DirtSide;
			loopTexture = Assets.Background.DirtLoop;
			edgeTexture = Assets.Background.DirtEdge;
			mapLocation = new Vector2(100, 200);
			possiblePrevious.Add(ModContent.GetInstance<DemoLevel>());
		}

		public override void DrawMapIcon(SpriteBatch spriteBatch, Vector2 center, float scale)
		{
			var tex = Assets.UI.DemoLevel3Details.Value;
			spriteBatch.Draw(tex, center, null, Color.White, 0f, tex.Size() / 2f, scale, 0, 0);
		}
	}
}

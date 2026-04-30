using Terraria.ID;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using ColdWater.Core.BasePlatformSystem;
using ColdWater.Core.UndergroundLevelSystem;
using ColdWater.Content.Levels;

namespace ColdWater.Content.Tiles
{
	public class MiningBaseEnginePlatform : MiningBasePlatform
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Width = 16;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };

			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;

			TileObjectData.addTile(Type);

			Main.tileSolid[Type] = true;
			Main.tileFrameImportant[Type] = true;

			DustType = DustID.Dirt;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			// TODO: Render the engine animation here later
		}

		public override bool RightClick(int i, int j)
		{
			if (j < UndergroundLevelModSystem.DescendingRegionStart)
			{
				UndergroundLevelModSystem.ActivateLevel(ModContent.GetInstance<DemoLevel>());
			}

			if (j > UndergroundLevelModSystem.DescendingRegionStart && j < UndergroundLevelModSystem.DescendingRegionEnd)
			{
				// Debug
				UndergroundLevelModSystem.activeState = UndergroundLevelState.Surface;
				Main.LocalPlayer.Center = BasePlatformModSystem.BaseArea.Center.ToVector2() * 16;
			}

			if (j > UndergroundLevelModSystem.DescendingRegionEnd)
			{
				// Ascend
			}

			return true;
		}
	}
}

using Terraria.ID;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using ColdWater.Core.BasePlatformSystem;

namespace ColdWater.Content.Tiles
{
	public class MiningBasePlatform : ModTile
	{
		public override string Texture => "ColdWater/Assets/Tiles/MiningBasePlatform";

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.Width = 16;
			TileObjectData.newTile.Height = 1;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = new[] { 16 };

			TileObjectData.newTile.AnchorLeft = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 1, 0);
			TileObjectData.newTile.AnchorAlternateTiles = new int[]
			{
				ModContent.TileType<MiningBasePlatform>(),
				ModContent.TileType<MiningBaseEnginePlatform>()
			};

			TileObjectData.addTile(Type);

			Main.tileSolid[Type] = true;
			Main.tileFrameImportant[Type] = true;

			DustType = DustID.Dirt;
		}

		public override void PlaceInWorld(int i, int j, Item item)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			Point16 pos = new Point16(i - tile.TileFrameX / 18, j - tile.TileFrameY / 18); // Get the top-left corner of the placed tile

			if (BasePlatformModSystem.baseTopLeft == default)
			{
				BasePlatformModSystem.baseTopLeft = new Point16(pos.X, pos.Y - BasePlatformModSystem.baseBuildHeight);
				BasePlatformModSystem.baseSize = new Point16(16, BasePlatformModSystem.baseBuildHeight);
			}
			else if (BasePlatformModSystem.baseTopLeft.X < pos.X)
			{
				int width = 0;
				while (Framing.GetTileSafely(BasePlatformModSystem.baseTopLeft.X + width, pos.Y).HasTile && Framing.GetTileSafely(pos.X + width, pos.Y).TileType == Type || Framing.GetTileSafely(pos.X + width, pos.Y).TileType == ModContent.TileType<MiningBaseEnginePlatform>())
				{
					width++;
				}

				BasePlatformModSystem.baseSize = new Point16(width, BasePlatformModSystem.baseBuildHeight);
			}
			else if (BasePlatformModSystem.baseTopLeft.X > pos.X)
			{
				int width = 0;
				while (Framing.GetTileSafely(pos.X + width, pos.Y).HasTile && Framing.GetTileSafely(pos.X + width, pos.Y).TileType == Type || Framing.GetTileSafely(pos.X + width, pos.Y).TileType == ModContent.TileType<MiningBaseEnginePlatform>())
				{
					width++;
				}

				BasePlatformModSystem.baseTopLeft = new Point16(pos.X, pos.Y - BasePlatformModSystem.baseBuildHeight);
				BasePlatformModSystem.baseSize = new Point16(width, BasePlatformModSystem.baseBuildHeight);
			}
			else
			{
				// nothing to do here, swap of existing platform
				return;
			}
		}
	}

	public class MiningBasePlatformItem : ModItem
	{
		public override string Texture => "ColdWater/Assets/Tiles/MiningBasePlatformItem";

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 9999;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.createTile = ModContent.TileType<MiningBasePlatform>();
		}
	}
}
	
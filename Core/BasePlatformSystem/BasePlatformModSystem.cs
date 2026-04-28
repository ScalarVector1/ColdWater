using StructureHelper.Util;
using StructureHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class BasePlatformModSystem : ModSystem
	{
		public static StructureData miningBase;
		public static StructurePreview baseTexture;

		public static Point16 baseTopLeft;
		public static Point16 baseSize;

		public static int baseBuildHeight = 40;

		public static Rectangle BaseArea
		{
			get => new Rectangle(baseTopLeft.X, baseTopLeft.Y, baseSize.X, baseSize.Y);
			set
			{
				baseTopLeft = new Point16(value.X, value.Y);
				baseSize = new Point16(value.Width, value.Height);
			}
		}

		public override void Load()
		{
			// This should load the preview rendering queue for the SH assembly embeded into this mod?
			new PreviewRenderQueue().Load(Mod);
		}

		public static void CopyInBase()
		{
			miningBase = StructureData.FromWorld(BaseArea.X, BaseArea.Y, BaseArea.Width, BaseArea.Height);

			baseTexture?.Dispose();
			baseTexture = new StructurePreview("", miningBase);
		}

		public override void PostDrawTiles()
		{
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

			Rectangle area = new Rectangle(baseTopLeft.X * 16 - (int)Main.screenPosition.X, baseTopLeft.Y * 16 - (int)Main.screenPosition.Y, baseSize.X * 16, baseSize.Y * 16);
			var tex = Assets.MagicPixel.Value;

			Main.spriteBatch.Draw(tex, area, new Color(60, 40, 80) * (0.2f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.1f));

			Main.spriteBatch.End();
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (BaseArea.Width == 0 || BaseArea.Height == 0)
				return;

			miningBase = StructureData.FromWorld(BaseArea.X, BaseArea.Y, BaseArea.Width, BaseArea.Height);

			var stream = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(stream);
			miningBase?.Serialize(writer);
			tag["base"] = stream.ToArray();

			tag["baseX"] = baseTopLeft.X;
			tag["baseY"] = baseTopLeft.Y;
			tag["baseWidth"] = baseSize.X;
			tag["baseHeight"] = baseSize.Y;
			tag["baseBuildHeight"] = baseBuildHeight;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			byte[] saved = tag.GetByteArray("base");

			if (saved.Length > 0)
				miningBase = StructureData.FromStream(new BinaryReader(new MemoryStream(saved)));

			int baseX = tag.GetShort("baseX");
			int baseY = tag.GetShort("baseY");
			int baseWidth = tag.GetShort("baseWidth");
			int baseHeight = tag.GetShort("baseHeight");
			baseBuildHeight = tag.GetInt("baseBuildHeight");

			baseTopLeft = new Point16(baseX, baseY);
			baseSize = new Point16(baseWidth, baseHeight);
		}
	}
}

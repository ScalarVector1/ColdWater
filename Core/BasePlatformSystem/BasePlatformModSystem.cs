using Microsoft.Xna.Framework.Graphics;
using StructureHelper.Models;
using StructureHelper.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.Graphics;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Light;
using Terraria.ModLoader.IO;
using ColdWater.Core.UndergroundLevelSystem;
using Terraria.ObjectData;
using Terraria.ID;

namespace ColdWater.Core.BasePlatformSystem
{
	internal struct LightPoint
	{
		public Vector2 offset;
		public Vector3 color;

		public LightPoint(Vector2 offset, Vector3 color)
		{
			this.offset = offset;
			this.color = color;
		}
	}

	internal class BasePlatformModSystem : ModSystem
	{
		public static StructureData miningBase;
		public static RenderTarget2D baseRenderTarget;

		public static Point16 baseTopLeft;
		public static Point16 baseSize;

		public static bool targetNeedsRendered;
		public static bool targetBeingRendered;

		public static int baseBuildHeight = 40;

		public static List<LightPoint> baseLights = new();

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

			On_Main.DoDraw += RenderBaseTargeet;
			On_LightingEngine.GetColor += WhiteDuringTarget;
		}

		private Vector3 WhiteDuringTarget(On_LightingEngine.orig_GetColor orig, LightingEngine self, int x, int y)
		{
			if (targetBeingRendered)
				return Vector3.One;
			else
				return orig(self, x, y);
		}

		private void RenderBaseTargeet(On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
		{
			if (!Main.dedServ && targetNeedsRendered)
			{
				CheckAndUpdateTargetSize();

				Main.graphics.GraphicsDevice.SetRenderTarget(baseRenderTarget);
				Main.graphics.GraphicsDevice.Clear(Color.Transparent);

				var oldWidth = Main.screenWidth;
				var oldHeight = Main.screenHeight;
				var oldPos = Main.screenPosition;
				var oldTranslation = Main.GameViewMatrix._translation;

				var descendingCopyLocation = UndergroundLevelModSystem.DescendingBasePlacementLocation;

				Main.screenWidth = baseSize.X << 4;
				Main.screenHeight = baseSize.Y << 4;
				Main.screenPosition = new Vector2(descendingCopyLocation.X * 16, descendingCopyLocation.Y * 16) + Vector2.One * Main.offScreenRange;
				Main.GameViewMatrix._translation = Vector2.Zero;

				Main.instance.TilesRenderer.PrepareForAreaDrawing(descendingCopyLocation.X, descendingCopyLocation.X + baseSize.X, descendingCopyLocation.Y, descendingCopyLocation.Y + baseSize.Y, prepareLazily: false);
				Main.instance.TilePaintSystem.PrepareAllRequests();

				targetBeingRendered = true;

				var tileBatch = Main.tileBatch;
				var spriteBatch = Main.spriteBatch;

				tileBatch.Begin();
				spriteBatch.Begin();
				Main.instance.DrawWalls();
				tileBatch.End();
				spriteBatch.End();

				Main.screenPosition = new Vector2(descendingCopyLocation.X * 16, descendingCopyLocation.Y * 16);

				bool flag3 = false;
				bool intoRenderTargets = false;
				bool intoRenderTargets2 = false;
				Main.instance.TilesRenderer.SpecificHacksForCapture();				

				Main.instance.TilesRenderer.PreDrawTiles(solidLayer: false, flag3, intoRenderTargets2);

				tileBatch.Begin();
				spriteBatch.Begin();

				Main.screenPosition = new Vector2(descendingCopyLocation.X * 16, descendingCopyLocation.Y * 16) + Vector2.One * Main.offScreenRange;

				Main.instance.DrawTiles(solidLayer: false, flag3, intoRenderTargets);

				Main.screenPosition = new Vector2(descendingCopyLocation.X * 16, descendingCopyLocation.Y * 16);

				tileBatch.End();
				spriteBatch.End();
				Main.instance.DrawTileEntities(solidLayer: false, flag3, intoRenderTargets);

				spriteBatch.Begin();
				tileBatch.Begin();
				Main.instance.waterfallManager.FindWaterfalls(forced: true);
				Main.instance.waterfallManager.Draw(spriteBatch);
				tileBatch.End();
				spriteBatch.End();

				Main.screenPosition = new Vector2(descendingCopyLocation.X * 16, descendingCopyLocation.Y * 16) + Vector2.One * Main.offScreenRange;

				Main.instance.TilesRenderer.PreDrawTiles(solidLayer: true, flag3, intoRenderTargets2);
				tileBatch.Begin();
				spriteBatch.Begin();

				Main.instance.DrawTiles(solidLayer: true, flag3, intoRenderTargets);

				tileBatch.End();
				spriteBatch.End();
				Main.instance.DrawTileEntities(solidLayer: true, flag3, intoRenderTargets);

				tileBatch.Begin();
				spriteBatch.Begin();

				Main.instance.DrawLiquid(bg: false, Main.waterStyle);

				tileBatch.End();
				spriteBatch.End();

				Main.graphics.GraphicsDevice.SetRenderTarget(null);

				Main.screenWidth = oldWidth;
				Main.screenHeight = oldHeight;
				Main.screenPosition = oldPos;
				Main.GameViewMatrix._translation = oldTranslation;

				targetBeingRendered = false;
			}

			orig(self, gameTime);
		}

		private void CheckAndUpdateTargetSize()
		{
			if (baseRenderTarget is null || baseRenderTarget.IsDisposed || baseRenderTarget.Width != baseSize.X * 16 || baseRenderTarget.Height != baseSize.Y * 16)
			{
				baseRenderTarget?.Dispose();
				baseRenderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, baseSize.X * 16, baseSize.Y * 16, false, default, default, default, RenderTargetUsage.PreserveContents);
			}
		}

		public static void SaveAndCopyBaseToDescendingRegion()
		{
			baseLights.Clear();

			for (int x = baseTopLeft.X; x < baseTopLeft.X + baseSize.X; x++)
			{
				for (int y = baseTopLeft.Y; y < baseTopLeft.Y + baseSize.Y; y++)
				{
					var tile = Main.tile[x, y];
					if (Main.tileLighted[tile.TileType])
					{
						Lighting.NewEngine._tileScanner.GetTileLight(x, y, out Vector3 color);
						baseLights.Add(new(new Vector2((x - baseTopLeft.X) * 16, (y - baseTopLeft.Y) * 16), color));
					}
				}
			}

			miningBase = StructureData.FromWorld(BaseArea.X, BaseArea.Y, BaseArea.Width, BaseArea.Height);
			UndergroundLevelModSystem.PlaceBaseDescending();
		}

		public static void ApplyBaseLights(Vector2 pos)
		{
			foreach (var light in baseLights)
			{
				Lighting.AddLight(pos + light.offset, light.color);
			}
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

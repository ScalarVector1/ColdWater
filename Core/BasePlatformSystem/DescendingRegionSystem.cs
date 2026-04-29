using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class DescendingRegionSystem : ModSystem
	{
		public static bool descendingActive;
		public static int descendCounter;
		public static int nextShake;

		public static int DescendingRegionStart => (int)Main.worldSurface + 100;
		public static int DescendingRegionEnd => (int)Main.worldSurface + 300;

		public static Rectangle DescendingRegion => new Rectangle(0, DescendingRegionStart, Main.maxTilesX, DescendingRegionEnd - DescendingRegionStart);
		public static Point16 BasePlacementLocation => new Point16(Main.maxTilesX / 2 - BasePlatformModSystem.miningBase.width / 2, DescendingRegionEnd - BasePlatformModSystem.miningBase.height - 32);
		public static Vector2 BaseWorldCenter => BasePlacementLocation.ToVector2() * 16 + BasePlatformModSystem.baseSize.ToVector2() * 8;

		public override void Load()
		{
			Terraria.GameContent.Events.On_ScreenDarkness.DrawBack += DrawScroll;
		}

		float SmoothStep(float t)
		{
			return t * t * (3f - 2f * t);
		}

		float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		float Random1(int x)
		{
			x = (x << 13) ^ x;
			return 1.0f - ((x * (x * x * 15731 + 789221) + 1376312589)
						   & 0x7fffffff) / 1073741824f;
		}

		float Noise1(float t)
		{
			int t0 = (int)Math.Floor(t);
			int t1 = t0 + 1;

			float localT = t - t0;

			float n0 = Random1(t0);
			float n1 = Random1(t1);

			float smoothT = SmoothStep(localT);

			return Lerp(n0, n1, smoothT); // range ~[-1, 1]
		}

		private void DrawScroll(On_ScreenDarkness.orig_DrawBack orig, SpriteBatch spriteBatch)
		{
			orig(spriteBatch);

			if (descendingActive)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

				var tex = Assets.Background.DirtScroll.Value;
				var edge = Assets.Background.DirtSide.Value;
				float parallaxBase = Main.screenPosition.X + Main.screenWidth / 2f;

				var screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
				var source = new Rectangle((int)((BaseWorldCenter.X - parallaxBase) * -0.95f), (int)((descendCounter * 6) % tex.Height + Main.screenPosition.Y), Main.screenWidth, Main.screenHeight);

				LightingBufferRenderer.DrawWithLighting(tex, screen, source, new Color(160, 160, 160));

				Rectangle edgeSource;

				Vector2 leftPos = new Vector2(BaseWorldCenter.X - BasePlatformModSystem.baseSize.X * 8, 0);

				edgeSource = new Rectangle(0, (int)(descendCounter * 7 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, GetParallax(leftPos - Vector2.UnitX * 200, parallaxBase, 0.4f, edge.Width), edgeSource, new Color(110, 110, 110), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

				edgeSource = new Rectangle(0, (int)(descendCounter * 10 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, GetParallax(leftPos - Vector2.UnitX * 230, parallaxBase, 0.3f, edge.Width), edgeSource, new Color(130, 130, 130), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

				edgeSource = new Rectangle(0, (int)(descendCounter * 13 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, GetParallax(leftPos - Vector2.UnitX * 260, parallaxBase, 0.2f, edge.Width), edgeSource, new Color(190, 190, 190), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

				Vector2 rightPos = new Vector2(BaseWorldCenter.X + BasePlatformModSystem.baseSize.X * 8, 0);

				edgeSource = new Rectangle(0, (int)(descendCounter * 7 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, GetParallax(rightPos + Vector2.UnitX * 200, parallaxBase, 0.4f, edge.Width), edgeSource, new Color(110, 110, 110), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

				edgeSource = new Rectangle(0, (int)(descendCounter * 10 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, GetParallax(rightPos + Vector2.UnitX * 230, parallaxBase, 0.3f, edge.Width), edgeSource, new Color(130, 130, 130), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

				edgeSource = new Rectangle(0, (int)(descendCounter * 13 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, GetParallax(rightPos + Vector2.UnitX * 260, parallaxBase, 0.2f, edge.Width), edgeSource, new Color(190, 190, 190), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

				Matrix rotation = 
					Matrix.CreateTranslation(-Main.screenWidth / 2, - Main.screenHeight / 2, 0) * 
					Matrix.CreateRotationZ(Noise1(descendCounter * 0.1f) * 0.04f) * 
					Matrix.CreateTranslation(Main.screenWidth / 2, Main.screenHeight / 2, 0);

				Main.GameViewMatrix._transformationMatrix = rotation * Main.GameViewMatrix.TransformationMatrix;

				spriteBatch.End();
				spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		public override void PostDrawTiles()
		{
			if (descendingActive)
			{
				var edge = Assets.Background.DirtSide.Value;
				float parallaxBase = Main.screenPosition.X + Main.screenWidth / 2f;

				Rectangle edgeSource;

				Vector2 leftPos = new Vector2(BaseWorldCenter.X - BasePlatformModSystem.baseSize.X * 8 - 200 - Main.screenPosition.X, 0);

				edgeSource = new Rectangle(0, (int)(descendCounter * 16 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, leftPos, edgeSource, Color.White, 0, Vector2.UnitX * edge.Width / 2, Vector2.One);


				Vector2 rightPos = new Vector2(BaseWorldCenter.X + BasePlatformModSystem.baseSize.X * 8 + 200 - Main.screenPosition.X, 0);

				edgeSource = new Rectangle(0, (int)(descendCounter * 16 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
				LightingBufferRenderer.DrawWithLighting(edge, rightPos, edgeSource, Color.White, 0, Vector2.UnitX * edge.Width / 2, Vector2.One);
			}
		}

		public Rectangle GetParallax(Vector2 pos, float baseX, float factor, int width)
		{
			return new Rectangle((int)(pos.X - (pos.X - baseX) * factor - Main.screenPosition.X), 0, width, Main.screenHeight);
			
		}

		public override void PostUpdateEverything()
		{
			if (descendingActive)
			{
				descendCounter++;
				nextShake--;

				if (nextShake <= 0)
				{
					PunchCameraModifier modifier = new PunchCameraModifier(BaseWorldCenter, Vector2.UnitY.RotatedByRandom(0.5f), Main.rand.NextFloat(10, 30), Main.rand.Next(6, 10), Main.rand.Next(20, 60), -1, FullName);
					Main.instance.CameraModifiers.Add(modifier);

					SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode.WithPitchOffset(Main.rand.NextFloat(-1f, -0.5f)));

					for (int k = 0; k < 60; k++)
					{
						Dust.NewDustPerfect(new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y), DustID.Dirt, Vector2.UnitY * Main.rand.NextFloat(20f), 0, default, Main.rand.NextFloat(1f, 4f));
					}

					nextShake = Main.rand.Next(50, 300);
				}

				if (Main.rand.NextBool(10))
					Dust.NewDustPerfect(new Vector2(Main.screenPosition.X + Main.rand.Next(Main.screenWidth), Main.screenPosition.Y), DustID.Dirt, Vector2.UnitY * Main.rand.NextFloat(10f), 0, default, Main.rand.NextFloat(1f, 2f));

				for (int k = 0; k < 2; k++)
					Dust.NewDustPerfect(new Vector2(BaseWorldCenter.X + Main.rand.Next(-BasePlatformModSystem.baseSize.X * 8, BasePlatformModSystem.baseSize.X * 8), BaseWorldCenter.Y + BasePlatformModSystem.baseSize.Y * 8 + 100), DustID.Dirt, Vector2.UnitY * -Main.rand.NextFloat(20f), 0, default, Main.rand.NextFloat(1f, 2f));
			}
		}

		public static void PlaceBase()
		{
			StructureHelper.API.Generator.GenerateFromData(BasePlatformModSystem.miningBase, BasePlacementLocation);
		}
	}
}

using ColdWater.Core.BasePlatformSystem;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace ColdWater.Core.UndergroundLevelSystem
{
	internal class UndergroundLevelModSystem : ModSystem
	{
		public static UndergroundLevel activeLevel;

		public static UndergroundLevelState activeState = UndergroundLevelState.Surface;
	
		public static int descendCounter;
		public static int descendTimer;
	
		public static int levelCounter;
		public static int levelTimer;

		public static int nextShake;

		public static Task generationTask;

		public static bool Descending => activeState == UndergroundLevelState.Descent;
		public static bool InLevel => activeState == UndergroundLevelState.Level;

		public static int DescendingRegionStart => (int)Main.worldSurface + 100;
		public static int DescendingRegionEnd => (int)Main.worldSurface + 300;

		public static int LevelRegionStart => (int)Main.worldSurface + 310;
		public static int LevelRegionEnd => Main.maxTilesY - 300;
		public static Rectangle LevelRect => new(0, LevelRegionStart, Main.maxTilesX, LevelRegionEnd - LevelRegionStart);

		public static Rectangle DescendingRegion => new(0, DescendingRegionStart, Main.maxTilesX, DescendingRegionEnd - DescendingRegionStart);
		public static Point16 DescendingBasePlacementLocation => new(Main.maxTilesX / 2 - BasePlatformModSystem.miningBase.width / 2, DescendingRegionEnd - BasePlatformModSystem.miningBase.height - 80);
		public static Vector2 DescendingBaseWorldCenter => DescendingBasePlacementLocation.ToVector2() * 16 + BasePlatformModSystem.baseSize.ToVector2() * 8;

		public override void Load()
		{
			On_ScreenDarkness.DrawBack += DrawScroll;
		}

		private void DrawScroll(On_ScreenDarkness.orig_DrawBack orig, SpriteBatch spriteBatch)
		{
			orig(spriteBatch);

			if (Descending)
			{
				activeLevel.DrawDescendingBackground(spriteBatch);

				if (activeLevel.ModifyTransformMatrixForDescent(Main.GameViewMatrix.TransformationMatrix, out Matrix matrix))
				{
					Main.GameViewMatrix._transformationMatrix = matrix;

					spriteBatch.End();
					spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}
		}

		public override void PostDrawTiles()
		{
			if (Descending)
			{
				activeLevel.DrawDescendingSides(Main.spriteBatch);
			}
		}

		/// <summary>
		/// Sets the given level and begins all appropriate animations. Should be called from the rig
		/// </summary>
		/// <param name="level">The level to begin</param>
		public static void ActivateLevel(UndergroundLevel level)
		{
			activeLevel = level;
			BaseDescendingAnimation.active = true;
			BaseDescendingAnimation.timer = 0;
		}

		public static void StartDescent()
		{
			activeState = UndergroundLevelState.Descent;
			descendCounter = 0;
			descendTimer = 0;
		}

		public static void StartLevel()
		{
			activeState = UndergroundLevelState.Level;
			levelCounter = 0;
			levelTimer = 0;
		}

		public override void PostUpdateEverything()
		{
			if (Descending)
			{			
				if (descendCounter == 0 && Main.netMode != NetmodeID.MultiplayerClient)
				{
					generationTask = Task.Run(() => activeLevel.GenerateLevel(LevelRect));
				}

				activeLevel.UpdateInDescent();

				descendCounter++;

				if (descendTimer < activeLevel.descentDuration)
				{
					descendTimer++;
				}
				else
				{
					if (activeLevel.canFinishDescent && generationTask.IsCompleted)
					{
						Main.NewText("Level begin");
						// Finish descent
					}
				}

					// TODO: Move this shaking logic somewhere to the base UndergroundLevel?
					nextShake--;

				if (nextShake <= 0)
				{
					var modifier = new PunchCameraModifier(DescendingBaseWorldCenter, Vector2.UnitY.RotatedByRandom(0.5f), Main.rand.NextFloat(10, 30), Main.rand.Next(6, 10), Main.rand.Next(20, 60), -1, FullName);
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
					Dust.NewDustPerfect(new Vector2(DescendingBaseWorldCenter.X + Main.rand.Next(-BasePlatformModSystem.baseSize.X * 8, BasePlatformModSystem.baseSize.X * 8), DescendingBaseWorldCenter.Y + BasePlatformModSystem.baseSize.Y * 8 + 100), DustID.Dirt, Vector2.UnitY * -Main.rand.NextFloat(20f), 0, default, Main.rand.NextFloat(1f, 2f));
			}
		}

		public static void PlaceBase()
		{
			StructureHelper.API.Generator.GenerateFromData(BasePlatformModSystem.miningBase, DescendingBasePlacementLocation);
		}
	}

	public enum UndergroundLevelState
	{
		Surface,
		Descent,
		Level,
		Ascent
	}
}

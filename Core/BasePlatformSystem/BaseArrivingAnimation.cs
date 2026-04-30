using ColdWater.Core.UndergroundLevelSystem;
using ColdWater.Core.Utils;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class BaseArrivingAnimation : ModSystem
	{
		public static int duration = 900;

		public static bool active;
		public static int timer;

		public static BaseArrivingCameraModifier cameraMod = new();

		public static Vector2 baseVisualOffset = Vector2.Zero;

		public Dictionary<int, Vector2> initialPlayerOffsets = [];

		public override void PostUpdateEverything()
		{
			if (active)
			{
				Vector2 descendPos = UndergroundLevelModSystem.DescendingBasePlacementLocation.ToVector2() * 16;
				Vector2 basePos = UndergroundLevelModSystem.LevelBasePlacementLocation.ToVector2() * 16;
				Vector2 offset = Vector2.UnitY * -(UndergroundLevelModSystem.LevelRegion.Height * 8);

				if (timer == 0)
				{
					baseVisualOffset = offset;

					foreach (Player player in Main.ActivePlayers)
					{
						initialPlayerOffsets[player.whoAmI] = player.Center - descendPos;
					}

					//TODO: Do we want to copy down from descending to level incase we made changes during the descent?
				}

				if (timer == 120)
				{
					UndergroundLevelModSystem.StartLevel();
				}

				if (timer > 120)
				{
					baseVisualOffset = Vector2.SmoothStep(offset, Vector2.Zero, (timer - 120) / 780f);
					var digCenter = basePos + baseVisualOffset + BasePlatformModSystem.baseSize.ToVector2() * 8 + Vector2.UnitY * (BasePlatformModSystem.baseSize.Y * 8 + 64);

					if (timer % 5 == 0)
					{
						SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode.WithPitchOffset(Main.rand.NextFloat(1f, 2f)));

						var explosionCenter = digCenter;
						explosionCenter.X += Main.rand.Next(-BasePlatformModSystem.baseSize.X * 8, BasePlatformModSystem.baseSize.X * 8);
						explosionCenter.Y += Main.rand.Next(-64, 64);

						ExplodeTiles(explosionCenter, Main.rand.Next(6, 12));

						for(int k = 0; k < 20; k++)
						{
							Dust.NewDustPerfect(explosionCenter, DustID.Torch, Main.rand.NextVector2Circular(5, 5), 0, default, Main.rand.Next(1, 5));
						}
					}

					for(int x = (int)digCenter.X / 16 - BasePlatformModSystem.baseSize.X / 2; x < (int)digCenter.X / 16 + BasePlatformModSystem.baseSize.X / 2; x++)
					{
						for(int y = (int)digCenter.Y / 16 - 4; y < (int)digCenter.Y / 16 + 4; y++)
						{
							Main.tile[x, y].ClearEverything();
						}
					}

					foreach (Player player in Main.ActivePlayers)
					{
						if (initialPlayerOffsets.TryGetValue(player.whoAmI, out Vector2 off))
						{
							player.Center = basePos + off + baseVisualOffset;
							player.velocity *= 0;
						}
					}
				}

				if (timer < duration)
				{
					timer++;
				}
				else
				{
					UndergroundLevelModSystem.PlaceBaseLevel();

					active = false;
					timer = 0;
				}
			}
		}

		public override void ModifyScreenPosition()
		{
			if (active)
				Main.instance.CameraModifiers.Add(cameraMod);
		}

		public override void PostDrawTiles()
		{
			if (active)
			{
				BasePlatformModSystem.targetNeedsRendered = true;
			}

			if (active && timer >= 150)
			{
				RenderTarget2D tex = BasePlatformModSystem.baseRenderTarget;
				Vector2 pos = UndergroundLevelModSystem.LevelBaseWorldCenter - tex.Size() / 2f + baseVisualOffset;

				BasePlatformModSystem.ApplyBaseLights(pos);

				if (tex != null)
					LightingBufferRenderer.DrawWithLighting(tex, pos - Main.screenPosition, Color.White);
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (active)
			{
				if (timer > 60 && timer <= 120)
				{
					float fade = (timer - 60) / 60f;
					Texture2D tex = Assets.MagicPixel.Value;
					spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * fade);
				}

				if (timer > 120 && timer <= 150)
				{
					Texture2D tex = Assets.MagicPixel.Value;
					spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
				}

				if (timer > 150 && timer <= 210)
				{
					float fade = 1f - (timer - 150) / 60f;
					Texture2D tex = Assets.MagicPixel.Value;

					spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * fade);
				}
			}
		}

		public void ExplodeTiles(Vector2 compareSpot, int radius)
		{
			for (int i = (int)compareSpot.X / 16 - radius; i <= (int)compareSpot.X / 16 + radius; i++)
			{
				for (int j = (int)compareSpot.Y / 16 - radius; j <= (int)compareSpot.Y / 16 + radius; j++)
				{
					float num = Math.Abs(i - compareSpot.X / 16f);
					float num2 = Math.Abs(j - compareSpot.Y / 16f);
					if (!(Math.Sqrt(num * num + num2 * num2) < radius))
						continue;

					if (Main.tile[i, j] != null && Main.tile[i, j].active())
					{
						WorldGen.KillTile(i, j);
						if (!Main.tile[i, j].active() && Main.netMode != 0)
							NetMessage.SendData(17, -1, -1, null, 0, i, j);
					}

					for (int k = i - 1; k <= i + 1; k++)
					{
						for (int l = j - 1; l <= j + 1; l++)
						{
							if (Main.tile[k, l] != null && Main.tile[k, l].wall > 0)
							{
								if (!WallLoader.CanExplode(k, l, Main.tile[k, l].wall))
								{
									continue;
								}

								WorldGen.KillWall(k, l);
								if (Main.tile[k, l].wall == 0 && Main.netMode != 0)
									NetMessage.SendData(17, -1, -1, null, 2, k, l);
							}
						}
					}
				}
			}
		}
	}

	internal class BaseArrivingCameraModifier : ICameraModifier
	{
		public string UniqueIdentity => "ColdWater/BaseArrivingCameraModifier";

		public bool Finished => BaseArrivingAnimation.timer == 0;

		public void Update(ref CameraInfo cameraPosition)
		{
			int timer = BaseArrivingAnimation.timer;

			Vector2 baseCenter = UndergroundLevelModSystem.LevelBaseWorldCenter - Main.ScreenSize.ToVector2() / 2f;
			Vector2 start = new Vector2(baseCenter.X, UndergroundLevelModSystem.LevelRegionStart * 16);

			if (timer < 120)
			{
				cameraPosition.CameraPosition = Vector2.Lerp(cameraPosition.OriginalCameraPosition, cameraPosition.OriginalCameraPosition - Vector2.UnitY * (Main.screenHeight / 2f), timer / 120f);
			}
			else if (timer >= 120 && timer < 870)
			{
				float prog = (timer - 120) / 750f;
				cameraPosition.CameraPosition = Vector2.Lerp(start, baseCenter, 1f - MathF.Pow(1f - prog, 1.1f));
			}
			else
			{
				float prog = (timer - 870) / 30f;
				cameraPosition.CameraPosition = Vector2.SmoothStep(baseCenter, cameraPosition.OriginalCameraPosition, prog);
			}
		}
	}
}

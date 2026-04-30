using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.CameraModifiers;
using ColdWater.Core.UndergroundLevelSystem;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class BaseDescendingAnimation : ModSystem
	{
		public static int duration = 450;

		public static bool active;
		public static int timer;

		public static BaseDescendingCameraModifier cameraMod = new();

		public static Vector2 baseVisualOffset = Vector2.Zero;

		public Dictionary<int, Vector2> initialPlayerOffsets = new();

		public override void PostUpdateEverything()
		{
			if (active)
			{
				Vector2 basePos = BasePlatformModSystem.baseTopLeft.ToVector2() * 16;

				// Setup
				if (timer == 0)
				{
					baseVisualOffset = Vector2.Zero;

					foreach (Player player in Main.ActivePlayers)
					{
						initialPlayerOffsets[player.whoAmI] = player.Center - basePos;
					}

					BasePlatformModSystem.SaveAndCopyBaseToDescendingRegion();
				}

				if (timer == 10) // Slight delay to allow time to generate preview texture in an attempt to prevent visual gap
				{
					// This should hopefully quietly clear everything
					for(int x = BasePlatformModSystem.baseTopLeft.X; x <= BasePlatformModSystem.baseTopLeft.X + BasePlatformModSystem.baseSize.X; x++)
					{
						for(int y = BasePlatformModSystem.baseTopLeft.Y; y <= BasePlatformModSystem.baseTopLeft.Y + BasePlatformModSystem.baseSize.Y; y++)
						{
							Main.tile[x, y].ClearEverything();
						}
					}
				}

				// Move players
				if (timer > 0 && timer < 310)
				{
					foreach (Player player in Main.ActivePlayers)
					{
						if (initialPlayerOffsets.TryGetValue(player.whoAmI, out Vector2 offset))
						{
							player.Center = basePos + offset + baseVisualOffset;
							player.velocity *= 0;
						}
					}
				}

				// Move platform
				if (timer > 30 && timer < 90)
				{
					float prog	= (timer - 30) / 60f;
					baseVisualOffset = Vector2.UnitY * MathF.Sin(prog * MathF.PI * 4) * MathF.Sin(prog * MathF.PI) * 8f;
				}

				if (timer > 90 && timer < 300)
				{
					float prog = (timer - 90) / 210f;
					baseVisualOffset += Vector2.UnitY * prog * 10;
				}

				if (timer == 310)
				{
					UndergroundLevelModSystem.StartDescent();

					foreach (Player player in Main.ActivePlayers)
					{
						if (initialPlayerOffsets.TryGetValue(player.whoAmI, out Vector2 offset))
						{
							player.Center = UndergroundLevelModSystem.DescendingBasePlacementLocation.ToVector2() * 16 + offset;
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
					//Replace base at surface
					StructureHelper.API.Generator.GenerateFromData(BasePlatformModSystem.miningBase, BasePlatformModSystem.baseTopLeft);

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

			if (active && timer >= 10)
			{
				var pos = BasePlatformModSystem.baseTopLeft.ToVector2() * 16 + baseVisualOffset;
				var tex = BasePlatformModSystem.baseRenderTarget;

				BasePlatformModSystem.ApplyBaseLights(pos);

				if (tex != null)
					LightingBufferRenderer.DrawWithLighting(tex, pos - Main.screenPosition, Color.White);
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (active)
			{
				if (timer > 240 && timer <= 300)
				{
					float fade = (timer - 240) / 60f;
					var tex = Assets.MagicPixel.Value;

					spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * fade);
				}

				if (timer > 300 && timer <= 330)
				{
					var tex = Assets.MagicPixel.Value;
					spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black);
				}

				if (timer > 330)
				{
					float fade = 1f - (timer - 330) / 30f;
					var tex = Assets.MagicPixel.Value;

					spriteBatch.Draw(tex, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * fade);
				}
			}
		}
	}

	internal class BaseDescendingCameraModifier : ICameraModifier
	{
		public string UniqueIdentity => "ColdWater/BaseDescendingCameraModifier";

		public bool Finished => BaseDescendingAnimation.timer == 0;

		public void Update(ref CameraInfo cameraPosition)
		{
			int timer = BaseDescendingAnimation.timer;

			Vector2 baseCenter = BasePlatformModSystem.BaseArea.Center() * 16 - Main.ScreenSize.ToVector2() / 2f;

			if (timer < 60)
			{
				cameraPosition.CameraPosition = Vector2.SmoothStep(cameraPosition.OriginalCameraPosition, baseCenter, timer / 60f);
			}
			else
			{
				cameraPosition.CameraPosition = baseCenter;
			}

			if (timer > 90 && timer < 330)
			{
				float prog = (timer - 90) / 210f;
				cameraPosition.CameraPosition = baseCenter + BaseDescendingAnimation.baseVisualOffset * 0.75f;
			}

			if (timer > 330 && timer <= 450)
			{
				var basePos = UndergroundLevelModSystem.DescendingBaseWorldCenter - Main.ScreenSize.ToVector2() / 2f;
				basePos.Y = cameraPosition.OriginalCameraPosition.Y;

				cameraPosition.CameraPosition = Vector2.SmoothStep(basePos + Vector2.UnitY * (Main.screenHeight / 2f + 200), cameraPosition.OriginalCameraPosition, (timer - 330) / 120f);
			}
		}
	}
}
	
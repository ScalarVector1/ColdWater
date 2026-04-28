using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.CameraModifiers;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class BaseDescendingAnimation : ModSystem
	{
		public static int duration = 300;

		public static bool active;
		public static int timer;

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
					foreach (Player player in Main.ActivePlayers)
					{
						initialPlayerOffsets[player.whoAmI] = player.Center - basePos;
					}

					BasePlatformModSystem.CopyInBase();
				}

				if (timer == 10) // Slight delay to allow time to generate preview texture in an attempt to prevent visual gap
				{
					// This should hopefully quietly clear everything
					for(int x = BasePlatformModSystem.baseTopLeft.X; x < BasePlatformModSystem.baseTopLeft.X + BasePlatformModSystem.baseSize.X; x++)
					{
						for(int y = BasePlatformModSystem.baseTopLeft.Y; y < BasePlatformModSystem.baseTopLeft.Y + BasePlatformModSystem.baseSize.Y; y++)
						{
							Main.tile[x, y].ClearEverything();
						}
					}
				}

				// Move players
				if (timer > 0)
				{
					foreach (Player player in Main.ActivePlayers)
					{
						if (initialPlayerOffsets.TryGetValue(player.whoAmI, out Vector2 offset))
						{
							player.Center = basePos + offset + baseVisualOffset;
						}
					}
				}

				// Move platform
				if (timer > 30 && timer < 90)
				{
					float prog	= (timer - 30) / 60f;
					baseVisualOffset = Vector2.UnitY * MathF.Sin(prog * MathF.PI * 8) * MathF.Sin(prog * MathF.PI) * 20f;
				}

				if (timer > 90 && timer < 300)
				{
					float prog = (timer - 90) / 210f;
					baseVisualOffset = Vector2.UnitY * MathF.Pow((prog + 1), 3);
				}

				if (timer < duration)
				{
					timer++;
				}
				else
				{
					active = false;
						timer = 0;
				}
			}
		}

		public override void ModifyScreenPosition()
		{
			Main.instance.CameraModifiers.Add(new BaseDescendingCameraModifier());
		}

		public override void PostDrawTiles()
		{
			if (active && timer >= 10)
			{
				var tex = BasePlatformModSystem.baseTexture.preview;
				LightingBufferRenderer.DrawWithLighting(tex, BasePlatformModSystem.baseTopLeft.ToVector2() * 16 - Main.screenPosition + baseVisualOffset, Color.White);
			}
		}
	}

	internal class BaseDescendingCameraModifier : ICameraModifier
	{
		public string UniqueIdentity => "ColdWater/BaseDescendingCameraModifier";

		public bool Finished => BaseDescendingAnimation.timer > 0 && BaseDescendingAnimation.timer < BaseDescendingAnimation.duration;

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

			if (timer > 90)
			{
				float prog = (timer - 90) / 210f;
				cameraPosition.CameraPosition = baseCenter + Vector2.UnitY * MathF.Pow((prog + 1), 2);
			}
		}
	}
}

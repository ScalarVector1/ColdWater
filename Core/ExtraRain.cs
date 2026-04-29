using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;

namespace ColdWater.Core
{
	internal class ExtraRain : ModSystem
	{
		public SoundStyle rainStyle = new SoundStyle("ColdWater/Assets/Sound/Rain", SoundType.Ambient)
		{
			IsLooped = true,
			PauseBehavior = PauseBehavior.StopWhenGamePaused
		};

		public SoundStyle waterStyle = new SoundStyle(SoundID.Waterfall.SoundPath, SoundType.Ambient)
		{
			IsLooped = true,
			PauseBehavior = PauseBehavior.StopWhenGamePaused
		};

		public SlotId rainSlot;
		public SlotId waterSlot;

		public override void Load()
		{
			On_Main.DoDraw_WallsTilesNPCs += AddExtraRain;
		}

		public override void PostUpdateEverything()
		{
			var rainy = Main.raining && (!Main.LocalPlayer.behindBackWall && Main.LocalPlayer.Center.Y < Main.worldSurface * 16);

			if (Main.GameUpdateCount % 20 == 0 && rainy)
			{
				if (!SoundEngine.GetActiveSound(rainSlot)?.IsPlaying ?? true)
					rainSlot = SoundEngine.PlaySound(rainStyle.WithPitchOffset(1f), null, RainSoundCallback);

				if (!SoundEngine.GetActiveSound(waterSlot)?.IsPlaying ?? true)
					waterSlot = SoundEngine.PlaySound(waterStyle.WithPitchOffset(2f).WithVolume(Main.cloudAlpha), null, RainSoundCallback);
			}

			if(Main.GameUpdateCount % 10 == 0 && rainy && Main.rand.NextFloat() < Main.cloudAlpha)
			{
				SoundEngine.PlaySound(SoundID.DripSplash.WithPitchOffset(Main.rand.NextFloat(-0.5f, 0.5f)), null, RainSoundCallback);
			}
		}

		private bool RainSoundCallback(ActiveSound soundInstance)
		{
			soundInstance.Volume = Main.cloudAlpha * (
				soundInstance.Style.SoundPath == waterStyle.SoundPath ? 0.2f :
				soundInstance.Style.SoundPath == SoundID.DripSplash.SoundPath? 0.3f :
				1f);

			if (!(Main.raining && (!Main.LocalPlayer.behindBackWall && Main.LocalPlayer.Center.Y < Main.worldSurface * 16)))
				soundInstance.Stop();

			return (Main.raining && (!Main.LocalPlayer.behindBackWall && Main.LocalPlayer.Center.Y < Main.worldSurface * 16));
		}

		private void AddExtraRain(On_Main.orig_DoDraw_WallsTilesNPCs orig, Main self)
		{
			if (Main.raining && Main.LocalPlayer.Center.Y < Main.worldSurface * 16)
			{
				var tex = Assets.LineAlpha.Value;
				Random rand = new(1892639812);

				var rot = Rain.GetRainFallVelocity().ToRotation();
				var dropLen = Main.screenHeight / MathF.Cos(rot - 1.57f);
				var color = Color.Lerp(Main.ColorOfTheSkies, Color.White, 0.2f);
				color.A = 0;

				for (int x = 0; x < Main.screenWidth; x += rand.Next(1, (int)MathF.Max(1, 30 * (0.6f - Main.cloudAlpha))))
				{
					Vector2 pos = new Vector2(rand.Next(Main.screenWidth), 0);

					float len = (Main.GameUpdateCount * (15 + rand.NextSingle() * 25f) + rand.Next(Main.screenHeight)) % Main.screenWidth;

					pos += (Vector2.UnitY * len).RotatedBy(rot - 1.57f);

					float dist = rand.NextSingle();

					pos.X -= Main.screenPosition.X * dist * 0.5f;
					pos.X = (pos.X % Main.screenWidth + Main.screenWidth) % Main.screenWidth;

					Main.spriteBatch.Draw(tex, new Rectangle((int)pos.X, (int)pos.Y, (int)(8 + dist * 4), rand.Next(140, 200)), default, color * (0.1f + dist * 0.2f), rot - 1.57f, tex.Size() / 2f, 0, 0);
				}
			}

			orig(self);
		}
	}
}

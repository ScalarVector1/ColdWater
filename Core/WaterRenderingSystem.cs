using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Peripherals.RGB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ColdWater.Core
{
	internal class WaterRenderingSystem : ModSystem
	{
		public static bool shouldDrawRain;
		public static bool shouldDrawWaterfall;

		public ScreenTarget waterTarget = new(renderWaterTarget, () => true, 0f);

		public override void Load()
		{
			IL_Main.DoDraw += AddWaterShader;
			On_Main.DrawRain += DelayRain;

			On_WaterfallManager.Draw += DelayWaterfall;
		}

		private void DelayRain(On_Main.orig_DrawRain orig, Main self)
		{
			if (shouldDrawRain)
			{
				orig(self);
			}
		}

		private void DelayWaterfall(On_WaterfallManager.orig_Draw orig, WaterfallManager self, SpriteBatch spriteBatch)
		{
			if (shouldDrawWaterfall)
			{
				orig(self, spriteBatch);
			}
		}

		private static void renderWaterTarget(SpriteBatch batch)
		{
			batch.Draw(Main.waterTarget, Main.sceneWaterPos - Main.screenPosition, Color.White);

			shouldDrawRain = true;
			Main.instance.DrawRain();
			shouldDrawRain = false;

			shouldDrawWaterfall = true;
			Main.instance.waterfallManager.Draw(batch);
			shouldDrawWaterfall = false;

			var tex = Assets.Circle.Value;
			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == ModContent.DustType<Content.WaterStyle.ColdWaterDust>())
				{
					float fadeSize = dust.fadeIn < 10f ? dust.fadeIn / 10f : 1f - (dust.fadeIn - 10f) / 50f;
					batch.Draw(tex, dust.position - Main.screenPosition, default, Color.White, 0, tex.Size() / 2f, dust.scale * 0.5f * fadeSize, 0, 0);

					fadeSize = dust.fadeIn < 20f ? dust.fadeIn / 20f : 1f - (dust.fadeIn - 20f) / 40f;
					batch.Draw(tex, dust.position + Vector2.UnitX.RotatedBy(dust.rotation) * 15 - Main.screenPosition, default, Color.White, 0, tex.Size() / 2f, dust.scale * 0.3f * fadeSize, 0, 0);

					fadeSize = dust.fadeIn < 40f ? dust.fadeIn / 40f : 1f - (dust.fadeIn - 40f) / 20f;
					batch.Draw(tex, dust.position + Vector2.UnitX.RotatedBy(dust.rotation * dust.rotation) * 15 - Main.screenPosition, default, Color.White, 0, tex.Size() / 2f, dust.scale * 0.3f * fadeSize, 0, 0);
				}
			}
		}

		private void AddWaterShader(ILContext il)
		{
			var c = new ILCursor(il);

			//back target
			c.TryGotoNext(n => n.MatchLdfld<Main>("backWaterTarget"));

			c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
			c.Index++;
			ILLabel label = il.DefineLabel(c.Next);

			c.TryGotoPrev(n => n.MatchLdfld<Main>("backWaterTarget"));
			c.Index -= 1;
			c.Emit(OpCodes.Pop);
			c.EmitDelegate<Action>(NewDrawBack);
			c.Emit(OpCodes.Br, label);

			//front target
			c.TryGotoNext(n => n.MatchLdsfld<Main>("waterTarget"));

			c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
			c.Index++;
			ILLabel label2 = il.DefineLabel(c.Next);

			c.TryGotoPrev(n => n.MatchLdsfld<Main>("waterTarget"));
			c.Emit(OpCodes.Pop);
			c.EmitDelegate<Action>(NewDraw);
			c.Emit(OpCodes.Br, label2);
		}

		private void NewDrawBack()
		{
			return;
			SpriteBatch sb = Main.spriteBatch;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

			Main.spriteBatch.Draw(Main.instance.backWaterTarget, Main.sceneBackgroundPos - Main.screenPosition, Color.White);

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

		}

		private void NewDraw()
		{
			SpriteBatch sb = Main.spriteBatch;

			sb.End();

			Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTargetSwap);

			Effect effect = ShaderLoader.GetShader("WaterShader").Value;
			LightingBuffer.bufferNeedsPopulated = true;

			if (effect != null)
			{
				effect.Parameters["offset"].SetValue(Main.sceneWaterPos - Main.screenPosition + new Vector2(16, 30));

				if (Main.LocalPlayer.gravDir < 0)
				{
					// Gravitaiton potion is fun :)
					effect.Parameters["offset"].SetValue(new Vector2(Main.sceneWaterPos.X - Main.screenPosition.X + 16, Main.screenPosition.Y - Main.sceneWaterPos.Y - 356));
				}

				effect.Parameters["offset"].SetValue(Vector2.Zero);

				effect.Parameters["sampleTexture2"].SetValue(waterTarget.RenderTarget);
				effect.Parameters["sampleTexture3"].SetValue(LightingBuffer.screenLightingTarget.RenderTarget);
				effect.Parameters["time"].SetValue(Main.GameUpdateCount / 200f);
				effect.Parameters["ratio"].SetValue(Main.screenTarget.Size() / waterTarget.RenderTarget.Size());
				effect.Parameters["screenSize"].SetValue(Main.screenTarget.Size());

				var transform = Matrix.CreateScale(1.0f / Main.GameZoomTarget) * Main.GameViewMatrix.EffectMatrix;
				effect.Parameters["zoom"].SetValue(Main.GameZoomTarget);

				effect.Parameters["transform"].SetValue(transform);

				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, effect, Matrix.Identity);
			}
			else
			{
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			}
			
			Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);

			sb.End();
			Main.graphics.GraphicsDevice.SetRenderTarget(Main.screenTarget);

			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

			Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1, 0, 0);

			//Main.spriteBatch.Draw(Main.waterTarget, Vector2.Zero, Color.White);

		}
	}
}

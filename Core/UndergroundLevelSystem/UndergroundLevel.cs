using ColdWater.Core.BasePlatformSystem;
using ColdWater.Core.Utils;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColdWater.Core.UndergroundLevelSystem;
using Terraria.ModLoader.IO;

namespace ColdWater.Core.UndergroundLevelSystem
{
	internal abstract class UndergroundLevel : ModType
	{
		public Asset<Texture2D> backgroundTexture;
		public Asset<Texture2D> backgroundEdgeTexture;
		public Asset<Texture2D> loopTexture;
		public Asset<Texture2D> edgeTexture;

		public int descentDustType;

		/// <summary>
		/// How long the player has to explore the generated level
		/// </summary>
		public int levelDuration = 6000;

		/// <summary>
		/// How long the descent will last. If the descent can end is also restricted by canFinishDescent.
		/// </summary>
		public int descentDuration = 2000;

		/// <summary>
		/// If the descent will end after the duration does. Can be used to add extra conditions to finishing the descent.
		/// </summary>
		public bool canFinishDescent = true;

		public bool completed;

		public List<UndergroundLevel> possiblePrevious = new();
		public Vector2 mapLocation;

		public string DisplayName => Language.GetTextValue($"Mods.{Mod.Name}.UndergroundLevels.{Name}.DisplayName");
		public string Description => Language.GetTextValue($"Mods.{Mod.Name}.UndergroundLevels.{Name}.Description");

		// Placeholder
		public bool Available => possiblePrevious.Any(n => n.completed) || possiblePrevious.Count == 0;

		public UndergroundLevel()
		{
			SetDefaults();
		}

		public sealed override void Register()
		{
			ModTypeLookup<UndergroundLevel>.Register(this);
		}

		public virtual void SetDefaults() { }

		/// <summary>
		/// Renders the background of the descending area, typically this should scroll to give the illusion of falling
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawDescendingBackground(SpriteBatch spriteBatch) 
		{
			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			var tex = backgroundTexture.Value;
			var edge = backgroundEdgeTexture.Value;
			float parallaxBase = Main.screenPosition.X + Main.screenWidth / 2f;

			var timer = UndergroundLevelModSystem.descendCounter;
			var center = UndergroundLevelModSystem.DescendingBaseWorldCenter;

			var screen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			var source = new Rectangle((int)((center.X - parallaxBase) * -0.95f), (int)((timer * 6) % tex.Height + Main.screenPosition.Y), Main.screenWidth, Main.screenHeight);

			LightingBufferRenderer.DrawWithLighting(tex, screen, source, new Color(160, 160, 160));

			Rectangle edgeSource;

			Vector2 leftPos = new Vector2(center.X - BasePlatformModSystem.baseSize.X * 8, 0);

			edgeSource = new Rectangle(0, (int)(timer * 7 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, GetParallax(leftPos - Vector2.UnitX * 200, parallaxBase, 0.4f, edge.Width), edgeSource, new Color(110, 110, 110), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			edgeSource = new Rectangle(0, (int)(timer * 10 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, GetParallax(leftPos - Vector2.UnitX * 230, parallaxBase, 0.3f, edge.Width), edgeSource, new Color(130, 130, 130), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			edgeSource = new Rectangle(0, (int)(timer * 13 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, GetParallax(leftPos - Vector2.UnitX * 260, parallaxBase, 0.2f, edge.Width), edgeSource, new Color(190, 190, 190), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			Vector2 rightPos = new Vector2(center.X + BasePlatformModSystem.baseSize.X * 8, 0);

			edgeSource = new Rectangle(0, (int)(timer * 7 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, GetParallax(rightPos + Vector2.UnitX * 200, parallaxBase, 0.4f, edge.Width), edgeSource, new Color(110, 110, 110), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			edgeSource = new Rectangle(0, (int)(timer * 10 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, GetParallax(rightPos + Vector2.UnitX * 230, parallaxBase, 0.3f, edge.Width), edgeSource, new Color(130, 130, 130), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			edgeSource = new Rectangle(0, (int)(timer * 13 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, GetParallax(rightPos + Vector2.UnitX * 260, parallaxBase, 0.2f, edge.Width), edgeSource, new Color(190, 190, 190), 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			// Bottom of the dirt
			var tex2 = loopTexture.Value;
			var bottSource = new Rectangle(0, (int)(timer * 16 % tex2.Height), Main.screenWidth, Main.screenHeight / 2);
			LightingBufferRenderer.DrawWithLighting(tex2, new Vector2(0, UndergroundLevelModSystem.DescendingBaseWorldCenter.Y + BasePlatformModSystem.baseSize.Y * 8 + 68 - Main.screenPosition.Y), bottSource, Color.White, 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			var tex3 = edgeTexture.Value;
			var bottEdgeSource = new Rectangle(0, 0, Main.screenWidth, 4);
			LightingBufferRenderer.DrawWithLighting(tex3, new Vector2(0, UndergroundLevelModSystem.DescendingBaseWorldCenter.Y + BasePlatformModSystem.baseSize.Y * 8 + 64 - Main.screenPosition.Y), bottEdgeSource, Color.White, 0, Vector2.UnitX * edge.Width / 2, Vector2.One);
		}

		/// <summary>
		/// Helper to get the target rectangle for a parallax background element
		/// </summary>
		/// <param name="pos">World position of the background element</param>
		/// <param name="baseX">Base X coordinate for the parallax, usually the base center</param>
		/// <param name="factor">Parallax strength</param>
		/// <param name="width">The width of the resulting rectangle</param>
		/// <returns></returns>
		public Rectangle GetParallax(Vector2 pos, float baseX, float factor, int width)
		{
			return new Rectangle((int)(pos.X - (pos.X - baseX) * factor - Main.screenPosition.X), 0, width, Main.screenHeight);
		}

		/// <summary>
		/// Edits the games transform matrix during the descent, by default applies a chaotic periodic rotation
		/// </summary>
		/// <param name="current">The transform matrix before modification</param>
		/// <param name="modified">The modified matrix</param>
		/// <returns></returns>
		public virtual bool ModifyTransformMatrixForDescent(Matrix current, out Matrix modified)
		{
			Matrix rotation =
				Matrix.CreateTranslation(-Main.screenWidth / 2, -Main.screenHeight / 2, 0) *
				Matrix.CreateRotationZ(Noise.Noise1(UndergroundLevelModSystem.descendCounter * 0.1f) * 0.04f) *
				Matrix.CreateTranslation(Main.screenWidth / 2, Main.screenHeight / 2, 0);

			modified = rotation * Main.GameViewMatrix.TransformationMatrix;

			return true;
		}

		/// <summary>
		/// Renders the sides for the descending area, should scroll similar to the background to give the illusion of falling
		/// </summary>
		/// <param name="spriteBatch"></param>
		public virtual void DrawDescendingSides(SpriteBatch spriteBatch) 
		{
			var edge = Assets.Background.DirtSide.Value;
			float parallaxBase = Main.screenPosition.X + Main.screenWidth / 2f;

			var timer = UndergroundLevelModSystem.descendCounter;
			var center = UndergroundLevelModSystem.DescendingBaseWorldCenter;

			Rectangle edgeSource;

			Vector2 leftPos = new Vector2(center.X - BasePlatformModSystem.baseSize.X * 8 - 200 - Main.screenPosition.X, 0);

			edgeSource = new Rectangle(0, (int)(timer * 16 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, leftPos, edgeSource, Color.White, 0, Vector2.UnitX * edge.Width / 2, Vector2.One);

			Vector2 rightPos = new Vector2(center.X + BasePlatformModSystem.baseSize.X * 8 + 200 - Main.screenPosition.X, 0);

			edgeSource = new Rectangle(0, (int)(timer * 16 % edge.Height + Main.screenPosition.Y), edge.Width, Main.screenHeight);
			LightingBufferRenderer.DrawWithLighting(edge, rightPos, edgeSource, Color.White, 0, Vector2.UnitX * edge.Width / 2, Vector2.One);
		}

		/// <summary>
		/// Used to draw the decoration around this levels node on the map
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="center"></param>
		/// <param name="scale"></param>
		public virtual void DrawMapIcon(SpriteBatch spriteBatch, Vector2 center, float scale)
		{

		}

		/// <summary>
		/// World generation for the given underground level. Assume the base will be centered at the region's center.
		/// </summary>
		/// <param name="region"></param>
		public virtual void GenerateLevel(Rectangle region) { }

		/// <summary>
		/// Logic that should occur every frame while in the level
		/// </summary>
		public virtual void UpdateInLevel() { }

		/// <summary>
		/// Logic that should occur every frame while in the descent
		/// </summary>
		public virtual void UpdateInDescent() { }

		public void SaveData(TagCompound tag)
		{
			tag["completed"] = completed;
		}

		public void LoadData(TagCompound tag)
		{
			completed = tag.GetBool("completed");
		}
	}
}

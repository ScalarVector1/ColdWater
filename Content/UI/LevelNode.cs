using ColdWater.Core.UILoading;
using ColdWater.Core.UndergroundLevelSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace ColdWater.Content.UI
{
	internal class LevelNode : SmartUIElement
	{
		public UndergroundLevel level;

		public float Scale => GetDimensions().Width / 34f;

		public LevelNode(UndergroundLevel level) : base()
		{
			Width.Set(34, 0);
			Height.Set(22, 0);

			this.level = level;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var rect = GetDimensions().ToRectangle();

			foreach (var pres in level.possiblePrevious)
			{
				Vector2 prePos = pres.mapLocation;
				Vector2 pos = level.mapLocation;

				float rot = pos.DirectionTo(prePos).ToRotation();
				float len = Scale * Vector2.Distance(pos, prePos);
				var line = Assets.MagicPixel.Value;

				Rectangle target = new Rectangle(rect.Center.X, rect.Center.Y, (int)len, (int)(4 * Scale));

				spriteBatch.Draw(line, target, null, Color.White, rot, new Vector2(0, line.Height / 2f), 0, 0);
			}

			level?.DrawMapIcon(spriteBatch, GetDimensions().Center(), Scale);

			var tex = Assets.UI.LevelNode.Value;
			int frameY = level.Available ? level.completed ? 0 : 24 : 48;
			var frame = new Rectangle(0, frameY, 34, 22);


			if (level.Available && !level.completed)
			{
				var glow = Assets.GradientH.Value;
				Rectangle target = new Rectangle(rect.X + (int)(4 * Scale), rect.Y - (int)(30 * Scale), (int)(rect.Width - 8 * Scale), (int)(38 * Scale));
				Color color = new Color(100, 255, 150, 0) * (0.3f + MathF.Sin(Main.GameUpdateCount * 0.1f) * 0.1f);
				spriteBatch.Draw(glow, target, null, color);

				target = new Rectangle(rect.X + (int)(4 * Scale), rect.Y - (int)(30 * Scale), (int)(2 * Scale), (int)(38 * Scale));
				spriteBatch.Draw(glow, target, null, color);

				target = new Rectangle(rect.X + rect.Width - (int)(6 * Scale), rect.Y - (int)(30 * Scale), (int)(2 * Scale), (int)(38 * Scale));
				spriteBatch.Draw(glow, target, null, color);
			}

			spriteBatch.Draw(tex, rect, frame, Color.White);

			if (IsMouseHovering)
			{
				Tooltip.SetName(level.DisplayName);
				Tooltip.SetTooltip(level.Description);
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{

		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (level.Available)
				level.completed = !level.completed;
		}
	}
}

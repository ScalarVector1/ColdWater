using ColdWater.Core.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColdWater.Content.UI
{
	internal class FuelBar : SmartUIElement
	{
		public FuelBar() : base()
		{
			Width.Set(56, 0);
			Height.Set(262, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var tex = Assets.UI.FuelBar.Value;
			var fill = Assets.UI.FuelBarFill.Value;
			var line = Assets.MagicPixel.Value;

			float fillProg = Main.MouseScreen.X / Main.screenWidth;

			var dims = GetDimensions().ToRectangle();
			Vector2 fillPos = dims.TopLeft() + new Vector2(16, 34);
			var fillSource = new Rectangle(0, (int)(fill.Height * (1f - fillProg)), fill.Width, (int)(fill.Height * fillProg));
			var fillTarget = new Rectangle((int)fillPos.X, (int)fillPos.Y + (int)(fill.Height * (1f - fillProg)), fill.Width, (int)(fill.Height * fillProg));

			var lineTarget = new Rectangle(fillTarget.X, fillTarget.Y, fillTarget.Width, 2);

			spriteBatch.Draw(fill, fillTarget, fillSource, Color.White);
			spriteBatch.Draw(line, lineTarget, null, Color.White * 0.5f);
			spriteBatch.Draw(tex, dims, null, Color.White);
		}
	}
}

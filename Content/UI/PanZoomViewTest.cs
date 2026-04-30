using ColdWater.Core.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace ColdWater.Content.UI
{
	internal class PanZoomViewTest : SmartUIState
	{
		public UIPanel panelOrange;

		public override bool Visible => false;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			UIPanel panel = new UIPanel();
			panel.Left.Set(-400, 0.5f);
			panel.Top.Set(-400, 0.5f);
			panel.Width.Set(800, 0);
			panel.Height.Set(800, 0);
			Append(panel);

			PanZoomView view = new();
			view.Left.Set(-400, 0.5f);
			view.Top.Set(-400, 0.5f);
			view.Width.Set(800, 0);
			view.Height.Set(800, 0);
			Append(view);

			ThermostatBar bar = new ThermostatBar();
			bar.Left.Set(64, 0);
			bar.Top.Set(128, 0);
			view.Append(bar);

			panelOrange = new UIPanel();
			panelOrange.Left.Set(200, 0f);
			panelOrange.Top.Set(400, 0f);
			panelOrange.Width.Set(80, 0);
			panelOrange.Height.Set(80, 0);
			panelOrange.BackgroundColor = Color.Orange;
			view.Append(panelOrange);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if (panelOrange.IsMouseHovering)
				Utils.DrawBorderString(spriteBatch, "I'm hovering the orange box!", Main.MouseScreen + Vector2.One * 16, Main.DiscoColor);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);
		}
	}
}

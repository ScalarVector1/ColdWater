using ColdWater.Content.Levels;
using ColdWater.Core.UILoading;
using ColdWater.Core.UndergroundLevelSystem;
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
		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			UIPanel panel = new UIPanel();
			panel.Left.Set(-200, 0.5f);
			panel.Top.Set(-300, 0.5f);
			panel.Width.Set(400, 0);
			panel.Height.Set(600, 0);
			Append(panel);



			PanZoomView view = new();
			view.Left.Set(-200, 0.5f);
			view.Top.Set(-300, 0.5f);
			view.Width.Set(400, 0);
			view.Height.Set(600, 0);
			Append(view);

			foreach (UndergroundLevel level in ModContent.GetContent<UndergroundLevel>())
			{
				LevelNode node = new(level);
				node.Left.Set(level.mapLocation.X, 0);
				node.Top.Set(level.mapLocation.Y, 0);
				view.Append(node);
			}

			FuelBar bar = new();
			bar.Left.Set(-250, 0.5f);
			bar.Top.Set(-131, 0.5f);
			Append(bar);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Main.LocalPlayer.controlHook)
			{
				RemoveAllChildren();
				OnInitialize();
			}

			base.Draw(spriteBatch);
		}

		public override void SafeUpdate(GameTime gameTime)
		{

			base.SafeUpdate(gameTime);
			Recalculate();
		}
	}
}

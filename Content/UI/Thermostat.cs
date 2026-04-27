using ColdWater.Content.TemperatureSystem;
using ColdWater.Core.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace ColdWater.Content.UI
{
	internal class Thermostat : SmartUIState
	{
		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			ThermostatBar bar = new ThermostatBar();
			bar.Left.Set(64, 0);
			bar.Top.Set(128, 0);
			Append(bar);
		}
	}

	internal class ThermostatBar : UIElement
	{
		public ThermostatBar()
		{
			Width.Set(240, 0);
			Height.Set(38, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Left.Set(-120, 0.5f);
			Top.Set(32, 0);
			Parent.Recalculate();

			Vector2 pos = GetDimensions().ToRectangle().TopLeft();

			var texBack = Assets.UI.ThermostatUnder.Value;

			var texFill0 = Assets.UI.ThermostatFill0.Value;
			var texFill1 = Assets.UI.ThermostatFill1.Value;
			var texFill2 = Assets.UI.ThermostatFill2.Value;
			var texFill3 = Assets.UI.ThermostatFill3.Value;

			var texFront = Assets.UI.ThermostatOver.Value;


			float tempPercent = Main.LocalPlayer.GetModPlayer<TemperaturePlayer>().temperature / (float)TemperaturePlayer.MAX_TEMP;

			int fillWidth = 30 + (int)((texFill0.Width - 36) * tempPercent);
			spriteBatch.Draw(texBack, pos, Color.White);
			spriteBatch.Draw(texFill0, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, texFill0.Height), new Rectangle(0, 0, fillWidth, texFill0.Height), Color.White);

			float blueOpacity = Math.Clamp((tempPercent - 0.2f) / 0.8f, 0, 1);
			spriteBatch.Draw(texFill1, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, texFill0.Height), new Rectangle(0, 0, fillWidth, texFill0.Height), Color.White * blueOpacity);

			float purpleOpacity = Math.Clamp((tempPercent - 0.4f) / 0.6f, 0, 1);
			spriteBatch.Draw(texFill2, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, texFill0.Height), new Rectangle(0, 0, fillWidth, texFill0.Height), Color.White * purpleOpacity);

			float orangeOpacity = Math.Clamp((tempPercent - 0.6f) / 0.4f, 0, 1);
			spriteBatch.Draw(texFill3, new Rectangle((int)pos.X, (int)pos.Y, fillWidth, texFill0.Height), new Rectangle(0, 0, fillWidth, texFill0.Height), Color.White * orangeOpacity);

			spriteBatch.Draw(texFront, pos, Color.White);

			var texNub = Assets.UI.ThermostatNubs.Value;

			int damageNubX = 30 + (int)((texFill0.Width - 36) * Main.LocalPlayer.GetModPlayer<TemperaturePlayer>().damageThresh / (float)TemperaturePlayer.MAX_TEMP) - 8;
			int toolNubX = 30 + (int)((texFill0.Width - 36) * Main.LocalPlayer.GetModPlayer<TemperaturePlayer>().toolThresh / (float)TemperaturePlayer.MAX_TEMP) - 8;

			spriteBatch.Draw(texNub, pos + new Vector2(damageNubX, 4), new Rectangle(16 * 0, 0, 14, texNub.Height), Color.White);
			spriteBatch.Draw(texNub, pos + new Vector2(toolNubX, 4), new Rectangle(16 * 2, 0, 14, texNub.Height), Color.White);
		}
	}
}

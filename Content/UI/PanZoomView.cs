using ColdWater.Core.UILoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameInput;
using Terraria.UI;

namespace ColdWater.Content.UI
{
	internal class PanZoomView : SmartUIElement
	{
		public Vector2 offset = Vector2.Zero;
		public float zoom = 1f;

		public bool panningEnabled;
		public bool zoomingEnabled;

		private Vector2 start;
		private Vector2 root;

		private Vector2 mouseDownAt;
		private bool movingEnabled;
		private bool moved;
		private bool freeze;

		private Dictionary<UIElement, Vector2> basePositions = new();
		private Dictionary<UIElement, Vector2> baseSizes = new();

		private Vector2 CenterPos => GetDimensions().Center() + offset - new Vector2(GetDimensions().Width, GetDimensions().Height) / 2f;

		public override void Draw(SpriteBatch spriteBatch)
		{
			Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;

			var scissor = GetDimensions().ToRectangle();
			scissor.X = (int)(scissor.X * Main.UIScale);
			scissor.Y = (int)(scissor.Y * Main.UIScale);
			scissor.Width = (int)(scissor.Width * Main.UIScale);
			scissor.Height = (int)(scissor.Height * Main.UIScale);

			spriteBatch.GraphicsDevice.ScissorRectangle = scissor;

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			base.Draw(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
			spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
		}

		new public void Append(UIElement element)
		{
			basePositions.Add(element, new(element.Left.Pixels, element.Top.Pixels));
			baseSizes.Add(element, new(element.Width.Pixels, element.Height.Pixels));
			base.Append(element);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				PlayerInput.LockVanillaMouseScroll("PanZoomViewZoom");
			}

			if (!moved && Main.mouseLeft && Children.Any(n => n.IsMouseHovering))
				freeze = true;

			if (Main.mouseLeft && IsMouseHovering && !freeze)
			{
				if (start == Vector2.Zero)
				{
					start = Main.MouseScreen;
					root = offset;
				}

				if (start != Main.MouseScreen)
					moved = true;

				offset = root + Main.MouseScreen - start;
				Recalculate();
			}
			else
			{
				start = Vector2.Zero;
			}

			if (mouseDownAt != default && Vector2.Distance(Main.MouseScreen, mouseDownAt) > 16)
				movingEnabled = true;

			if (!Main.mouseLeft)
			{
				freeze = false;
				movingEnabled = false;
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			mouseDownAt = Main.MouseScreen;
		}

		public override void SafeScrollWheel(UIScrollWheelEvent evt)
		{
			Vector2 scaledMouseDiff = (CenterPos - Main.MouseScreen) * (1f / zoom);

			zoom += evt.ScrollWheelValue / 1200f * zoom;

			if (zoom < 0.5f)
				zoom = 0.5f;

			if (zoom > 2f)
				zoom = 2f;

			Vector2 scaledMouseDiff2 = (CenterPos - Main.MouseScreen) * (1f / zoom);

			offset += (scaledMouseDiff - scaledMouseDiff2) * zoom;
			Recalculate();
		}

		public override void Recalculate()
		{
			foreach (UIElement element in Elements)
			{
				if (basePositions.ContainsKey(element) && baseSizes.ContainsKey(element))
				{
					element.Width.Set(baseSizes[element].X * zoom, 0);
					element.Height.Set(baseSizes[element].Y * zoom, 0);
					element.Left.Set(basePositions[element].X * zoom + offset.X, 0);
					element.Top.Set(basePositions[element].Y * zoom + offset.Y, 0);
				}
			}

			base.Recalculate();
		}
	}
}

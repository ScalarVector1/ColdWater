using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class BaseArrivingAnimation
	{
		public static int duration = 900;

		public static bool active;
		public static int timer;

		public static BaseDescendingCameraModifier cameraMod = new();

		public static Vector2 baseVisualOffset = Vector2.Zero;

		public Dictionary<int, Vector2> initialPlayerOffsets = new();
	}
}

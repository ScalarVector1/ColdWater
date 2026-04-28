using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ColdWater.Core.BasePlatformSystem
{
	internal class DescendingRegionSystem : ModSystem
	{
		public static bool descendingActive;

		public static int DescendingRegionStart => (int)Main.worldSurface + 100;
		public static int DescendingRegionEnd => (int)Main.worldSurface + 300;

		public static Rectangle DescendingRegion => new Rectangle(0, DescendingRegionStart, Main.maxTilesX, DescendingRegionEnd - DescendingRegionStart);
		public static Point16 BasePlacementLocation => new Point16(Main.maxTilesX / 2 - BasePlatformModSystem.miningBase.width / 2, DescendingRegionEnd - BasePlatformModSystem.miningBase.height - 32);

		public override void Load()
		{
			On_Main.DrawBackground += DrawScroll;
		}

		private void DrawScroll(On_Main.orig_DrawBackground orig, Main self)
		{
			orig(self);

			var tex = Assets.Background.DirtScroll.Value;
			Main.spriteBatch.Draw(tex, Vector2.Zero, Color.White);
		}

		public override void PostUpdateEverything()
		{
			base.PostUpdateEverything();
		}

		public static void PlaceBase()
		{
			StructureHelper.API.Generator.GenerateFromData(BasePlatformModSystem.miningBase, BasePlacementLocation);
		}
	}
}

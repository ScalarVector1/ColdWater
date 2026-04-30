using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColdWater.Core.Utils
{
	internal static class Noise
	{
		static float SmoothStep(float t)
		{
			return t * t * (3f - 2f * t);
		}

		static float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		static float Random1(int x)
		{
			x = (x << 13) ^ x;
			return 1.0f - ((x * (x * x * 15731 + 789221) + 1376312589)
						   & 0x7fffffff) / 1073741824f;
		}

		public static float Noise1(float t)
		{
			int t0 = (int)Math.Floor(t);
			int t1 = t0 + 1;

			float localT = t - t0;

			float n0 = Random1(t0);
			float n1 = Random1(t1);

			float smoothT = SmoothStep(localT);

			return Lerp(n0, n1, smoothT);
		}
	}
}

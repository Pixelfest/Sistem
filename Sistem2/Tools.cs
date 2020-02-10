using System;
using System.Collections.Generic;
using System.Text;

namespace Sistem2
{
	public static class Tools
	{
		public const float Factor = 2.54f;
		public const float EyeDistance = 6.5f;

		public static float InchToCM(float value)
		{
			return value / Factor;
		}

		public static float CMToInch(float value)
		{
			return value * Factor;
		}

		public static void Swap<T>(this IList<T> list, int indexA, int indexB)
		{
			var temporary = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = temporary;
		}
	}
}

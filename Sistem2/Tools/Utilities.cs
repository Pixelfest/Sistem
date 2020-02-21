namespace Sistem2.Tools
{
	using System.Collections.Generic;

	public static class Utilities
	{
		public const float Factor = 2.54f;
		public const float EyeDistance = 6.5f;

		public static float InchToCM(float inches)
		{
			return inches * Factor;
		}

		public static float CMToInch(float centimeters)
		{
			return centimeters / Factor;
		}

		public static void Swap<T>(this IList<T> list, int indexA, int indexB)
		{
			var temporary = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = temporary;
		}

		public static bool EqualsWithTolerance(this float value, int compareTo, float tolerance = 0.001f)
		{
			return value + tolerance > compareTo && value - tolerance < compareTo;
		}
	}
}

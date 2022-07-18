namespace OpenStereogramCreator.Tools
{
	using System.Collections.Generic;

	public static class Utilities
	{
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

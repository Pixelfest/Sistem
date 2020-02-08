using System;
using System.Collections.Generic;
using System.Text;

namespace Sistem2
{
	static class Tools
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
	}
}

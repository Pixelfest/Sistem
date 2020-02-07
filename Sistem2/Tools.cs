using System;
using System.Collections.Generic;
using System.Text;

namespace Sistem2
{
	static class Tools
	{
		private const float factor = 2.54f;

		public static float InchToCM(float value)
		{
			return value / factor;
		}

		public static float CMToInch(float value)
		{
			return value * factor;
		}
	}
}

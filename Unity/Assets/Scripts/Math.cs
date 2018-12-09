namespace Jake
{
	public static class Math
	{
		public static float NormaliseAngle(float a)
		{
			while (a < 0)
			{
				a += 360;
			}

			while (a > 360)
			{
				a -= 360;
			}

			return a;
		}
	}
}

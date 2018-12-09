using UnityEngine;

namespace Jake.RL
{
	public class Target : MonoBehaviour
	{
		[Range(20, 40)]
		public float	minRadius,
						maxRadius;

		[Range(0, 360)]
		public float	leftLimit,
						rightLimit;

		[Range(0, 90)]
		public float	lowerLimit,
						upperLimit;

		public void Restart()
		{
			Restart(true);
		}

		public void Restart(bool restart)
		{
			if (restart)
			{
				var left = Math.NormaliseAngle(leftLimit);
				var right = Math.NormaliseAngle(rightLimit);
				if (left > right)
				{
					left -= 360;
				}

				transform.localEulerAngles = new Vector3(
					Random.Range(-lowerLimit, -upperLimit),
					Random.Range(left, right),
					0
				);

				transform.localPosition = Vector3.zero;
				transform.Translate(0, 0, Random.Range(minRadius, maxRadius));
			}
		}
	}
}

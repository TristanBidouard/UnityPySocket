using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Jake.RL
{
	public class RobotController : MonoBehaviour
	{
		[NonSerialized]
		public float	time,
						rot1,
						rot2,
						rot3;

		public Slider	rot1Slider,
						rot2Slider,
						rot3Slider;

		public Cycle cycle;
		
		public void SetRotationsOverTime()
		{
			// stop cycle
			if (time > 0)
			{
				cycle.Wait = true;
			}

			// rotate robot
			StartCoroutine(SetRotationsOverTime_Coroutine());
		}

		IEnumerator SetRotationsOverTime_Coroutine()
		{
			rot1 = Math.NormaliseAngle(rot1);
			
			// initial values
			var rot1Initial = rot1Slider.value;
			var rot2Initial = rot2Slider.value;
			var rot3Initial = rot3Slider.value;

			// change in values
			var rot1Delta = rot1 - rot1Initial;
			var rot2Delta = rot2 - rot2Initial;
			var rot3Delta = rot3 - rot3Initial;

			// go the shortest distance to the target
			if (rot1Delta > 180)
			{
				rot1Delta -= 360;
			}
			else if (rot1Delta < -180)
			{
				rot1Delta += 360;
			}
			
			// rotate
			var timeElapsed = 0f;
			var endTime = time;
			while (endTime > 0 && timeElapsed <= endTime)
			{
				var ratio = timeElapsed / endTime;

				rot1Slider.value = Math.NormaliseAngle(rot1Initial + rot1Delta * ratio);
				rot2Slider.value = rot2Initial + rot2Delta * ratio;
				rot3Slider.value = rot3Initial + rot3Delta * ratio;

				yield return null;
				timeElapsed += Time.deltaTime;
			}

			// make sure they got to their target
			rot1Slider.value = Math.NormaliseAngle(rot1);
			rot2Slider.value = rot2;
			rot3Slider.value = rot3;

			// continue cycle
			if (time > 0)
			{
				cycle.Wait = false;
			}
		}
	}
}

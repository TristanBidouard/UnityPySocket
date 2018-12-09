using System;
using UnityEngine;

namespace Jake
{
	public class Detector : MonoBehaviour
	{
		public float distance;

		public Vector3 DetectPoint()
		{
			return Detect((hit) => hit.point);
		}

		public float DetectDistance()
		{
			return Detect((hit) => hit.distance);
		}

		public T Detect<T>(Func<RaycastHit, T> func)
		{
			var hitInfo = default(RaycastHit);
			if (Physics.Raycast(transform.position, transform.forward, out hitInfo, distance))
			{
				return func(hitInfo);
			}
			else
			{
				return func(default(RaycastHit));
			}
		}

		/*
		 *	Editor 
		 */

		void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay(transform.position, transform.forward * distance);
		}
	}
}

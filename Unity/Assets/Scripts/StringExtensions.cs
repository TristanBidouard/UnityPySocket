using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jake.StringExtensions
{
	public static class StringExtensions
	{
		public static string Endstring(this string that, int count)
		{
			return that.Substring(that.Length - count);
		}

		public static string RemoveFromEnd(this string that, int count)
		{
			return that.Remove(that.Length - count);
		}

		public static string RemoveFromEnd(this string that, string end)
		{
			if (that.Endstring(end.Length) == end)
			{
				return that.RemoveFromEnd(end.Length);
			}
			else
			{
				return that;
			}
		}
	}
}

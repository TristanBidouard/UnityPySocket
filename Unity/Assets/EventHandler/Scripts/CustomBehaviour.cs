using CommonCS.IEnumerables;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using CommonCS.IO;

namespace CommonUnity.Deprecated
{
	public class CustomBehaviour : MonoBehaviour
	{
		protected static void Log(object obj)
		{
			// print in console
			Debug.Log(obj);

			// write to logfile
			LogFile.Write(Application.productName, obj.ToString());
		}
		
		protected static void LogWithName<T>(Expression<Func<T>> f)
		{
			var me = f.Body as MemberExpression;
			
			Log(string.Format("{0} = {1};", me.Member.Name, (me.Member as FieldInfo).GetValue((me.Expression as ConstantExpression).Value)));
		}

		protected static void LogArrayWithName<T>(Expression<Func<T[]>> f)
		{
			var me = f.Body as MemberExpression;
			
			Log(string.Format("{0} = {1};", me.Member.Name, ((me.Member as FieldInfo).GetValue((me.Expression as ConstantExpression).Value) as T[]).ToUsefulString()));
		}

		/// <summary>
		/// Replace old print
		/// </summary>
		public static new void print(object obj)
		{
			Log(obj);
		}
		
		protected void Execute(Action a, float t)
		{
			StartCoroutine(ExecuteInSeconds(a, t));
		}

		private IEnumerator ExecuteInSeconds(Action a, float t)
		{
			yield return new WaitForSeconds(t);

			a();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Jake.Threading
{
	public class Dispatcher : MonoBehaviour
	{
		private static Thread mainThread;
		private static Queue<Job> jobs = new Queue<Job>();
		
		public static bool IsMainThread
		{
			get
			{
				return Thread.CurrentThread.Equals(mainThread);
			}
		}
		
		void Awake()
		{
			mainThread = Thread.CurrentThread;
		}

		void Update()
		{
			while (jobs.Count > 0)
			{
				var job = jobs.Dequeue();

				job.callback();

				if (job.blocker != null)
				{
					job.blocker.Set();
				}
			}
		}
		
		public static void AddJob(Action callback, bool block)
		{
			if (IsMainThread)
			{
				callback();
			}
			else
			{
				var blocker = block ? new AutoResetEvent(false) : null;

				jobs.Enqueue(new Job()
				{
					callback = callback,
					blocker = blocker
				});

				if (blocker != null)
				{
					blocker.WaitOne();
				}
			}
		}
	}
}

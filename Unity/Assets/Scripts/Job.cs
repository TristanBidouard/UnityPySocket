using System;
using System.Threading;

namespace Jake.Threading
{
	public class Job
	{
		public Action callback;
		public AutoResetEvent blocker;
	}
}

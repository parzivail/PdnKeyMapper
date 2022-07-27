namespace PdnKeyMapper;

public static class TaskWaiter
{
	/// <summary>
	/// Blocks until condition is true or timeout occurs.
	/// </summary>
	/// <param name="condition">The break condition.</param>
	/// <param name="checkPeriod">The frequency at which the condition will be checked, in milliseconds.</param>
	/// <param name="timeout">The timeout in milliseconds.</param>
	/// <returns></returns>
	public static async Task WaitUntil(Func<bool> condition, int checkPeriod = 25, int timeout = -1)
	{
		var waitTask = Task.Run(async () =>
		{
			while (!condition()) await Task.Delay(checkPeriod);
		});

		if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout))) 
			throw new TimeoutException();
	}
}
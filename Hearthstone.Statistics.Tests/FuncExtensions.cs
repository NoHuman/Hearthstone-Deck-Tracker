namespace Hearthstone.Statistics.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class FuncExtensions
	{
		private static readonly Random random = new Random();

		public static void Times(this Action source, int times)
		{
			for (int i = 0; i < times; i++)
			{
				source();
			}
		}

		public static T1 Random<T1>(this IEnumerable<T1> source)
		{
			var maxValue = source.Count();
			return source.Skip(random.Next(0, maxValue)).FirstOrDefault();
		}
	}
}
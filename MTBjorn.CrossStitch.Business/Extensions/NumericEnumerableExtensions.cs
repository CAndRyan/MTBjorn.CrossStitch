using System;
using System.Collections.Generic;
using System.Linq;

namespace MTBjorn.CrossStitch.Business.Extensions
{
	public static class NumericEnumerableExtensions
	{
		public static int GetAverage(this IEnumerable<int> values) => (int)Math.Round((double)values.Sum() / values.Count());
	}
}

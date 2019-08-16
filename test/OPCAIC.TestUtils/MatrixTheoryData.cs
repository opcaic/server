using System.Collections.Generic;
using Xunit;

namespace OPCAIC.TestUtils
{
	public class MatrixTheoryData<T1, T2> : TheoryData<T1, T2>
	{
		public MatrixTheoryData(IEnumerable<T1> rows, IEnumerable<T2> columns)
		{
			foreach (var row in rows)
			{
				foreach (var col in columns)
				{
					Add(row, col);
				}
			}
		}
	}
}
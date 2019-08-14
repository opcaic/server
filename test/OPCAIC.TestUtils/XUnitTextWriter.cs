using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public class XUnitTextWriter : TextWriter
	{
		private readonly ITestOutputHelper output;

		public XUnitTextWriter(ITestOutputHelper output)
		{
			this.output = output;
		}

		/// <inheritdoc />
		public override Encoding Encoding { get; }

		/// <inheritdoc />
		public override void WriteLine()
		{
			output.WriteLine("");
		}

		/// <inheritdoc />
		public override void WriteLine(string s)
		{
			if (s != null)
			{
				output.WriteLine(s);
			}
		}
	}
}
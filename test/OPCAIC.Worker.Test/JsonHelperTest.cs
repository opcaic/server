using System.Collections.Generic;
using Newtonsoft.Json;
using OPCAIC.Worker.Services;
using Xunit;

namespace OPCAIC.Worker.Test
{
	public class JsonHelperTest
	{
		private const string Valid = @"{
	""A"": ""value""
}";

		private const string Missing = @"{
}";

		private const string Extra = @"{
	""A"": ""value"",
	""B"": ""value2""
}";

		[Theory]
		[InlineData(Valid, true)]
		[InlineData(Missing, false)]
		[InlineData(Extra, false)]
		public void DeserializeStrictTest(string json, bool shouldSucceed)
		{
			if (shouldSucceed)
			{
				JsonHelper.DeserializeStrict<TestType>(json);
			}
			else
			{
				Assert.ThrowsAny<JsonException>(() => JsonHelper.DeserializeStrict<TestType>(json));
			}
		}

		[Theory]
		[InlineData(Valid, true)]
		[InlineData(Missing, false)]
		[InlineData(Extra, true)]
		public void DeserializeWithExtraTest(string json, bool shouldSucceed)
		{
			if (shouldSucceed)
			{
				JsonHelper.DeserializeWithExtra<TestType>(json);
			}
			else
			{
				Assert.ThrowsAny<JsonException>(()
					=> JsonHelper.DeserializeWithExtra<TestType>(json));
			}
		}

		public class TestType
		{
			public string A { get; set; }
		}

		public class TestTypeWithExtra
		{
			public string A { get; set; }

			[JsonExtensionData]
			public Dictionary<string, object> Extra { get; set; }
		}
	}
}
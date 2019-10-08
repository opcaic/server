using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class GameModuleLoaderTest : ServiceTestBase
	{
		/// <inheritdoc />
		public GameModuleLoaderTest(ITestOutputHelper output) : base(output)
		{
			// hook to logger to know when exception has been logged
			loggerMock = Services.Mock<ILogger<GameModuleLoader>>();
			moduleDir = NewDirectory().CreateSubdirectory(moduleName);
			Services.Configure<Configuration>(cfg => cfg.ModulePath = moduleDir.Parent.FullName);
		}

		private readonly Mock<ILogger<GameModuleLoader>> loggerMock;
		private readonly DirectoryInfo moduleDir;
		private static readonly string moduleName = "GameModule";

		private void SetupConfig(string json)
		{
			File.WriteAllText(Path.Combine(moduleDir.FullName, Constants.FileNames.EntryPointsConfig),
				json);
		}

		[Theory]
		[InlineData(Configs.MissingEntryPoint)]
		[InlineData(Configs.ExtraMembers)]
		[InlineData("Random string which clearly isn't json")]
		public void DetectsMalformedConfig(string json)
		{
			SetupConfig(json);
			var loader = GetService<GameModuleLoader>();

			Should.Throw<JsonException>(() => loader.FindGameModule(moduleName));
		}

		private static class Configs
		{
			public const string Valid = @"{
	""Checker"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Compiler"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Validator"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Executor"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Cleanup"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	}
}";

			public const string MissingEntryPoint = @"{
	""Compiler"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Validator"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Executor"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	}
}";

			public const string ExtraMembers = @"{
	""Checker"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Checker2"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Compiler"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Validator"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Executor"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	},
	""Cleanup"": {
		""Executable"": ""cmd.exe"",
		""Arguments"": [""/C"", ""dir""]
	}
}";
		}

		[Fact]
		public void LoadsCorrectConfig()
		{
			SetupConfig(Configs.Valid);
			var loader = GetService<GameModuleLoader>();

			var module = loader.GetAllModuleNames().ShouldHaveSingleItem();
			module.ShouldBe(moduleName);
			loader.FindGameModule(moduleName).GameName.ShouldBe(module);
		}
	}
}
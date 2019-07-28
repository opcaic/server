using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Services;
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
			loggerMock = Services.MockLogger<GameModuleLoader>();
			Services.Mock<ILoggerFactory>();
			moduleDir = NewDirectory().CreateSubdirectory(moduleName);
			Services.Configure<Configuration>(cfg => cfg.ModulePath = moduleDir.Parent.FullName);
		}

		private readonly Mock<ILogger> loggerMock;
		private readonly DirectoryInfo moduleDir;
		private static readonly string moduleName = "GameModule";

		private void SetupConfig(string json)
			=> File.WriteAllText(Path.Combine(moduleDir.FullName, Constants.FileNames.ModuleConfig),
				json);

		[Theory]
		[InlineData(Configs.MissingEntryPoint)]
		[InlineData(Configs.ExtraMembers)]
		[InlineData("Random string which clearly isn't json")]
		public void DetectsMalformedConfig(string json)
		{
			SetupConfig(json);
			var loader = GetService<GameModuleLoader>();

			// none should be loaded
			Assert.Empty(loader.GetAllModules());
			loggerMock.VerifyLogException<JsonException>(LogLevel.Error);
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
		public void DetectsMissingConfig()
		{
			var loader = GetService<GameModuleLoader>();

			// none should be loaded
			Assert.Empty(loader.GetAllModules());
			loggerMock.VerifyLogException<FileNotFoundException>(LogLevel.Error);
		}

		[Fact]
		public void LoadsCorrectConfig()
		{
			SetupConfig(Configs.Valid);
			var loader = GetService<GameModuleLoader>();

			var module = Assert.Single(loader.GetAllModules());
			Assert.Equal(moduleName, module.GameName);
			Assert.Equal(module, loader.FindGameModule(moduleName));
		}
	}
}
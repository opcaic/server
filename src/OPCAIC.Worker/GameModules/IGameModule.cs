namespace OPCAIC.Worker.GameModules
{
	//TODO: move this to separate project so it can be easily referenced
	public interface IGameModule
	{
		string GameName { get; }
		void Check(string inputDir, string outputDir);
		void Compile(string inputDir, string outputDir);
		void Validate(string inputDir, string outputDir);
		void Execute(string inputDir, string outputDir);
		void Clean();
	}
}

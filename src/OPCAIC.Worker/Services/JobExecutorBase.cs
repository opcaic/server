using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	internal abstract class JobExecutorBase<TRequest, TResult> : IJobExecutor<TRequest, TResult>
		where TRequest : WorkMessageBase where TResult : ReplyMessageBase, new()
	{
		public IExecutionServices Services { get; }
		protected DirectoryInfo WorkingDirectory { get; private set; }
		protected TResult Result { get; }


		protected JobExecutorBase(ILogger logger, IExecutionServices services)
		{
			Services = services;
			this.Logger = logger;
			Result = new TResult();
		}

		protected IGameModule GameModule { get; private set; }

		protected ILogger Logger { get; }

		protected TRequest Request { get; set; }

		/// <inheritdoc />
		public TResult Execute(TRequest request)
		{
			Require.ArgNotNull(request, nameof(request));

			Request = request;
			Result.Id = request.Id;
			GameModule = Services.GetGameModule(request.Game);
			WorkingDirectory = Services.GetWorkingDirectory(request);
			InternalExecute().Wait();

			return Result;
		}

		protected string PathTo(string path) => Path.Combine(WorkingDirectory.FullName, path);

		protected abstract Task InternalExecute();
	}
}

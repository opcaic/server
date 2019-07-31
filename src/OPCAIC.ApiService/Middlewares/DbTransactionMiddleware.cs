using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OPCAIC.Infrastructure.DbContexts;

namespace OPCAIC.ApiService.Middlewares
{
	public sealed class DbTransactionMiddleware
	{
		private readonly RequestDelegate next;

		public DbTransactionMiddleware(RequestDelegate next) => this.next = next;

		public async Task InvokeAsync(HttpContext context, DataContext dataContext)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (dataContext == null)
			{
				throw new ArgumentNullException(nameof(dataContext));
			}

			// temporary skipping transactions - in-memory DB does not support transactions
			await next(context);

			await dataContext.SaveChangesAsync(context.RequestAborted);

			/* uncomment when real database is ready
			var transaction = await dataContext.Database.BeginTransactionAsync();
			try
			{
				await next(context);

				await dataContext.SaveChangesAsync(context.RequestAborted);

				transaction.Commit();
			}
			catch
			{
				transaction.Rollback();
				throw;
			}
			*/
		}
	}
}
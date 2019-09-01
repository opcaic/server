using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GameModuleMock;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Test
{
	public static class ExternalGameModuleHelper
	{
		public static ExternalEntryPointConfiguration CreateEntryPoint(
			Expression<Func<int>> invocation)
		{
			return new ExternalEntryPointConfiguration
			{ // this should be platform independent :)
				Executable = "dotnet",
				Arguments = new[] {$"{nameof(GameModuleMock)}.dll"}
					.Concat(GetCmdLineArguments(invocation)).ToArray()
			};
		}

		public static GameModuleProcessArgs CreateArgs(Expression<Func<int>> invocation)
		{
			return new GameModuleProcessArgs
			{
				WorkingDirectory = Directory.GetCurrentDirectory(),
				EntryPoint = CreateEntryPoint(invocation)
			};
		}

		private static List<string> GetCmdLineArguments(Expression<Func<int>> invocation)
		{
			try
			{
				var body = (MethodCallExpression)invocation.Body;
				var arguments = new List<string> {body.Method.Name};
				var wasNull = false;
				foreach (var arg in body.Arguments)
				{
					object value;
					switch (arg)
					{
						case ConstantExpression ce: // inline constant
							value = ce.Value;
							break;

						default: // we expect a captured variable, nothing else is supported
							var me = (MemberExpression)arg;
							value = ((FieldInfo)me.Member)
								.GetValue(((ConstantExpression)me.Expression).Value);
							break;
					}

					// allow nulls as placeholders for real entry point arguments
					if (value != null)
					{
						if (wasNull)
						{
							throw new InvalidOperationException(
								"Nulls (placeholders) cannot be followed by a non-null value.");
						}

						arguments.Add(value.ToString());
					}
					else
					{
						wasNull = true;
					}
				}

				return arguments;
			}
			catch
			{
				throw new NotSupportedException(
					$"The invocation expression must be in the form of () => {nameof(EntryPoints)}.[Method]([args]), where args are either constants or captured variables. The given values must not be null with the exception of placeholders for the actual entry point arguments inserted by the game module.");
			}
		}
	}
}
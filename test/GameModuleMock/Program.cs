using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GameModuleMock
{
	internal class Program
	{
		private const int ERRNO = -10;

		private static int Main(string[] args)
		{
			if (args.Length > 0)
			{
				var method = typeof(EntryPoints)
					.GetMethods(BindingFlags.Static | BindingFlags.Public)
					.SingleOrDefault(m
						=> m.Name == args[0] && m.GetParameters().Length == args.Length - 1);

				if (method == null)
				{
					Console.WriteLine($"Method '{args[0]}' not found");
					return ERRNO;
				}

				args = args.Skip(1).ToArray();
				var parameters = method.GetParameters();
				if (args.Length != parameters.Length)
				{
					Console.WriteLine($"Invalid number of arguments for method '{method}'");
					return ERRNO;
				}

				var actualPar = new object[parameters.Length];

				for (var i = 0; i < parameters.Length; i++)
				{
					var converter = TypeDescriptor.GetConverter(parameters[i].ParameterType);
					actualPar[i] = converter.ConvertFromString(args[i]);
				}

				Console.WriteLine($"Invoking {method}");
				var exit = (int)method.Invoke(null, actualPar);
				Console.WriteLine($"Exiting with code {exit}");
				return exit;
			}

			foreach (var methodInfo in typeof(EntryPoints).GetMethods(BindingFlags.Static |
				BindingFlags.Public))
			{
				Console.WriteLine(methodInfo);
			}

			return ERRNO;
		}
	}
}
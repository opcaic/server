using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OPCAIC.ApiService.Models.Users;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest.Infrastructure
{
	public class FunctionalTestBase<TSetup> : FunctionalTestBase, IClassFixture<TSetup>
		where TSetup : class
	{
		/// <inheritdoc />
		public FunctionalTestBase(ITestOutputHelper output, FunctionalTestFixture fixture,
			TSetup fixtureSetup) : base(output, fixture)
		{
			FixtureSetup = fixtureSetup;
		}

		protected TSetup FixtureSetup { get; }
	}

	[Collection("ServerContext")]
	public class FunctionalTestBase
	{
		protected readonly HttpClient Client;
		protected readonly FunctionalTestFixture Fixture;
		protected readonly ITestOutputHelper Output;
		private readonly JsonSerializerSettings serializerSettings;
		private AuthenticationHeaderValue authHeader;

		public FunctionalTestBase(ITestOutputHelper output, FunctionalTestFixture fixture)
		{
			this.Fixture = fixture;
			this.Output = output;
			Client = CreateClient();
			// give each client a unique header

			serializerSettings = GetJsonSerializerOptions(fixture);
		}

		protected WebServerFactory ClientFactory => Fixture.ClientFactory;

		protected HttpClient CreateClient()
		{
			var client = Fixture.ClientFactory.CreateClient();

			client.DefaultRequestHeaders.Add(FakeRemoteAddressMiddleware.CustomHeaderName,
				Guid.NewGuid().ToString());

			return client;
		}

		protected void Log(string msg)
		{
			var border = '+' + new string('-', msg.Length + 2) + '+';
			Output.WriteLine(border);
			Output.WriteLine("| " + msg + " |");
			Output.WriteLine(border);
			Output.WriteLine("");
		}

		protected Task LoginAsAdmin()
		{
			return LoginAs("admin@opcaic.com", "Password");
		}

		protected async Task LoginAs(string email, string password)
		{
			var tokens = await PostAsync<UserTokens>("api/users/login",
				new UserCredentialsModel {Email = email, Password = password}, false);

			UseAccessToken(tokens.AccessToken);
		}

		private JsonSerializerSettings GetJsonSerializerOptions(FunctionalTestFixture fixture)
		{
			// HACK: reach deep into running server services and get the instance of serializer
			// settings used
			return fixture.ClientFactory.Server.Host.Services
				.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;
		}

		protected Task<HttpResponseMessage> GetAsync(string url, bool dump = true)
		{
			return SendAsync(HttpMethod.Get, url, null, dump);
		}

		protected async Task<T> GetAsync<T>(string url)
		{
			var response = await GetAsync(url);
			return Deserialize<T>(await response.Content.ReadAsStringAsync());
		}

		protected void UseAccessToken(string token)
		{
			authHeader = new AuthenticationHeaderValue("Bearer", token);
		}

		protected void Logout()
		{
			authHeader = null;
		}

		private string Serialize(object o)
		{
			return JsonConvert.SerializeObject(o, serializerSettings);
		}

		private T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json, serializerSettings);
		}

		protected Task<HttpResponseMessage> PostAsync(string url, object body = null,
			bool dump = true)
		{
			return SendAsync(HttpMethod.Post, url, JsonContent(body), dump);
		}

		protected Task<T> PostAsync<T>(string url, object body = null, bool dump = true)
		{
			return SendAsync<T>(HttpMethod.Post, url,
				body == null
					? null
					: JsonContent(body),
				dump);
		}

		protected Task<HttpResponseMessage> PutAsync(string url, object body, bool dump = true)
		{
			return SendAsync(HttpMethod.Put, url, JsonContent(body), dump);
		}

		protected Task<T> PutAsync<T>(string url, object body, bool dump = true)
		{
			return SendAsync<T>(HttpMethod.Put, url, JsonContent(body), dump);
		}

		protected Task<HttpResponseMessage> DeleteAsync(string url, bool dump = true)
		{
			return SendAsync(HttpMethod.Delete, url, null, dump);
		}

		private HttpRequestMessage CreateRequest(HttpMethod method, string url,
			HttpContent content = null)
		{
			var msg = new HttpRequestMessage(method, url) {Content = content};

			if (authHeader != null)
			{
				msg.Headers.Authorization = authHeader;
			}

			return msg;
		}

		private StringContent JsonContent(object o)
		{
			return new StringContent(Serialize(o), Encoding.Default, "application/json");
		}

		protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url,
			HttpContent content = null, bool dump = true)
		{
			var request = CreateRequest(method, url, content);

			if (dump)
			{
				DumpRequest(request);
			}

			var response = await Client.SendAsync(request);

			if (dump)
			{
				DumpResponse(response);
			}

			return response;
		}

		protected async Task<T> SendAsync<T>(HttpMethod method, string url,
			HttpContent content = null, bool dump = true)
		{
			var response = await SendAsync(method, url, content, dump);
			response.EnsureSuccessStatusCode();
			return Deserialize<T>(await response.Content.ReadAsStringAsync());
		}

		protected void DumpRequest(HttpRequestMessage request)
		{
			Output.WriteLine($"Request: {request.Method} {request.RequestUri}");
			Output.WriteLine($"Headers:\n{request.Headers}");
			Output.WriteLine(
				$"Body: {PrettifyJson(request.Content?.ReadAsStringAsync().GetAwaiter().GetResult())}");
			Output.WriteLine("\n-----------------------------------------------------\n");
		}

		protected void DumpResponse(HttpResponseMessage response)
		{
			Output.WriteLine("Response:");
			Output.WriteLine($"Status code: {response.StatusCode}");
			Output.WriteLine($"Headers:\n{response.Headers}");

			var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

			if (body.Length > 0 && body[0] == '{') // format if received json
			{
				try
				{
					body = PrettifyJson(body);
				}
				catch
				{
					// ignore errors
				}
			}

			Output.WriteLine($"Body: {body}");
			Output.WriteLine("\n#####################################################\n");
		}

		private string PrettifyJson(string json)
		{
			if (json == null) return string.Empty;

			using var reader = new StringReader(json);
			using var writer = new StringWriter();
			using var jsonReader = new JsonTextReader(reader);
			using var jsonWriter = new JsonTextWriter(writer);

			jsonWriter.Formatting = Formatting.Indented;
			jsonWriter.WriteToken(jsonReader);
			return writer.ToString();
		}
	}
}
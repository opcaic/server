using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
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
		protected readonly Faker Faker = new Faker();
		protected UserIdentityModel CurrentIdentity { get; private set; }
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
			var tokens = await PostAsync<UserIdentityModel>("api/users/login",
				new UserCredentialsModel {Email = email, Password = password}, false);

			UseAccessToken(tokens.AccessToken);
			CurrentIdentity = tokens;
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

		protected Task<T> GetAsync<T>(string url, bool dump = true)
		{
			return SendAsync<T>(HttpMethod.Get, url, null, dump);
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

		protected Task<HttpResponseMessage> PostAsync(string url, HttpContent content,
			bool dump = true)
		{
			return SendAsync(HttpMethod.Post, url, content, dump);
		}

		protected Task<T> PostAsync<T>(string url, HttpContent content,
			bool dump = true)
		{
			return SendAsync<T>(HttpMethod.Post, url, content, dump);
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
				$"Body: {PrettifyBody(request.Content?.ReadAsStringAsync().GetAwaiter().GetResult())}");
			Output.WriteLine("\n-----------------------------------------------------\n");
		}

		protected void DumpResponse(HttpResponseMessage response)
		{
			Output.WriteLine("Response:");
			Output.WriteLine($"Status code: {response.StatusCode}");
			Output.WriteLine($"Headers:\n{response.Headers}");
			Output.WriteLine($"Body: {PrettifyBody(response.Content.ReadAsStringAsync().GetAwaiter().GetResult())}");
			Output.WriteLine("\n#####################################################\n");
		}

		private string PrettifyBody(string body)
		{
			if (!string.IsNullOrEmpty(body) && body[0] == '{') // assume json
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

			return body;
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
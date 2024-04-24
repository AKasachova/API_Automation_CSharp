using System.Net.Http.Headers;
using NUnit.Framework;

namespace APIAutomation
{
    public class ClientForReadScope
    {
        private static ClientForReadScope _instance;
        private static readonly object _lock = new object();
        private readonly HttpClient _client;

        private ClientForReadScope(HttpClient client)
        {
            _client = client;
        }

        public static ClientForReadScope GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var baseUrl = TestContext.Parameters["BaseUrl"];
                        var clientUsername = TestContext.Parameters["ClientUsername"];
                        var clientPassword = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForReadScope(CreateHttpClient(baseUrl, clientUsername, clientPassword));

                    }
                }
            }
            return _instance;
        }

        public HttpClient GetHttpClient()
        {
            return _client;
        }

        private static HttpClient CreateHttpClient(string baseUrl, string clientUsername, string clientPassword)
        {
            var authenticator = new BasicAuthenticator(baseUrl, clientUsername, clientPassword, Scope.read);
            var token = authenticator.GetToken().Result;

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 
            return client;
        }
    }

    public class ClientForWriteScope
    {
        private static ClientForWriteScope _instance;
        private static readonly object _lock = new object();
        private readonly HttpClient _client;

        private ClientForWriteScope(HttpClient client)
        {
            _client = client;
        }

        public static ClientForWriteScope GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        var baseUrl = TestContext.Parameters["BaseUrl"];
                        var clientUsername = TestContext.Parameters["ClientUsername"];
                        var clientPassword = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForWriteScope(CreateHttpClient(baseUrl, clientUsername, clientPassword));
                    }
                }
            }
            return _instance;
        }


        public HttpClient GetHttpClient()
        {
            return _client;
        }

        private static HttpClient CreateHttpClient(string baseUrl, string clientUsername, string clientPassword)
        {
            var authenticator = new BasicAuthenticator(baseUrl, clientUsername, clientPassword, Scope.write);
            var token = authenticator.GetToken().Result;

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 

            return client;
        }
    }
}

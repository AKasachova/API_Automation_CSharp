using System.Net.Http.Headers;
using NUnit.Framework;
using System;

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
                        var baseUrlParam = TestContext.Parameters["BaseUrl"];
                        var clientUsernameParam = TestContext.Parameters["ClientUsername"];
                        var clientPasswordParam = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForReadScope(CreateHttpClient(baseUrlParam, clientUsernameParam, clientPasswordParam, Scope.read));
                    }
                }
            }
            return _instance;
        }

        public HttpClient GetHttpClient()
        {
            return _client;
        }

        private static HttpClient CreateHttpClient(string baseUrl, string clientUsername, string clientPassword, Scope scope)
        {
            var authenticator = new BasicAuthenticator(baseUrl, clientUsername, clientPassword, scope);
            var token = authenticator.GetToken().Result;

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token); 
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
                        var baseUrlParam = TestContext.Parameters["BaseUrl"];
                        var clientUsernameParam = TestContext.Parameters["ClientUsername"];
                        var clientPasswordParam = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForWriteScope(CreateHttpClient(baseUrlParam, clientUsernameParam, clientPasswordParam, Scope.write));
                    }
                }
            }
            return _instance;
        }


        public HttpClient GetHttpClient()
        {
            return _client;
        }

        private static HttpClient CreateHttpClient(string baseUrl, string clientUsername, string clientPassword, Scope scope)
        {
            var authenticator = new BasicAuthenticator(baseUrl, clientUsername, clientPassword, scope);
            var token = authenticator.GetToken().Result;

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token); 

            return client;
        }
    }
}
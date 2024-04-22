using System.Net.Http.Headers;
using NUnit.Framework;

namespace APIAutomation
{
    public class ClientForWriteScope
    {
        private static ClientForWriteScope _instance;
        private static readonly object _lock = new object();
        private readonly HttpClient _client;

        private ClientForWriteScope(string baseURL, string clientUsername, string clientPassword)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(baseURL);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientUsername}:{clientPassword}"))
            );
        }

        public static ClientForWriteScope GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Retrieve parameters from configuration
                        string baseURL = TestContext.Parameters["BaseUrl"];
                        string clientUsername = TestContext.Parameters["ClientUsername"];
                        string clientPassword = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForWriteScope(baseURL, clientUsername, clientPassword);
                    }
                }
            }
            return _instance;
        }

        public HttpClient GetHttpClient()
        {
            return _client;
        }
    }

    public class ClientForReadScope
    {
        private static ClientForReadScope _instance;
        private static readonly object _lock = new object();
        private readonly HttpClient _client;

        private ClientForReadScope(string baseURL, string clientUsername, string clientPassword)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(baseURL);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientUsername}:{clientPassword}"))
            );
        }

        public static ClientForReadScope GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Retrieve parameters from configuration
                        string baseURL = TestContext.Parameters["BaseUrl"];
                        string clientUsername = TestContext.Parameters["ClientUsername"];
                        string clientPassword = TestContext.Parameters["ClientPassword"];

                        _instance = new ClientForReadScope(baseURL, clientUsername, clientPassword);
                    }
                }
            }
            return _instance;
        }

        public HttpClient GetHttpClient()
        {
            return _client;
        }
    }
}

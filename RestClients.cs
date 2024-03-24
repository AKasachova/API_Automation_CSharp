using APIAutomation;
using NUnit.Framework;
using RestSharp;

namespace APIAutomation
{
    public class ClientForWriteScope
    {
        private static ClientForWriteScope _instance;
        private static readonly object _lock = new object();
        private readonly RestClient _client;

        private ClientForWriteScope(string baseURL, string clientUsername, string clientPassword)
        {
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new BasicAuthenticator(baseURL, clientUsername, clientPassword, "write")
            };

            _client = new RestClient(options);
        }

        public static ClientForWriteScope GetInstance(string baseURL, string clientUsername, string clientPassword)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ClientForWriteScope(baseURL, clientUsername, clientPassword);
                    }
                }
            }
            return _instance;
        }

        public RestClient GetRestClient()
        {
            return _client;
        }

    }

    public class ClientForReadScope
    {
        private static ClientForReadScope _instance;
        private static readonly object _lock = new object();
        private readonly RestClient _client;

        private ClientForReadScope(string baseURL, string clientUsername, string clientPassword)
        {
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new BasicAuthenticator(baseURL, clientUsername, clientPassword, "read")
            };

            _client = new RestClient(options);
        }

        public static ClientForReadScope GetInstance(string baseURL, string clientUsername, string clientPassword)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ClientForReadScope(baseURL, clientUsername, clientPassword);
                    }
                }
            }
            return _instance;
        }

        public RestClient GetRestClient()
        {
            return _client;
        }
    }
}



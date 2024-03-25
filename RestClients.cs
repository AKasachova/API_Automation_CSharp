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
                Authenticator = new BasicAuthenticator(baseURL, clientUsername, clientPassword, Scope.write)
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
                Authenticator = new BasicAuthenticator(baseURL, clientUsername, clientPassword, Scope.read)
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

//Use for Debugging. To delete after tests implementation
public static class Test_For_Auth_Check
{
    [Test]
    public static void Test1()
    {
        var client = ClientForWriteScope.GetInstance("http://localhost:50990", "0oa157tvtugfFXEhU4x7", "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq");
        var restClient = client.GetRestClient();
    }
}

using RestSharp;
using RestSharp.Authenticators;

namespace APIAutomation
{
    public class BasicAuthenticatorForWriteScope : AuthenticatorBase
    {
        private static BasicAuthenticatorForWriteScope _instance;
        private static readonly object _lock = new object();

        readonly string _baseUrl;
        readonly string _clientUsername;
        readonly string _clientPassword;
        readonly string _method;

        private BasicAuthenticatorForWriteScope(string baseUrl, string clientUsername, string clientPassword, string method) : base("")
        {
            _baseUrl = baseUrl;
            _clientUsername = clientUsername;
            _clientPassword = clientPassword;
            _method = method;
        }

        public static BasicAuthenticatorForWriteScope GetInstance(string baseUrl, string clientUsername, string clientPassword, string method)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new BasicAuthenticatorForWriteScope(baseUrl, clientUsername, clientPassword, method);
                    }
                }
            }
            return _instance;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        public async Task<string> GetToken()
        {
            var scope = "";
            if (_method == "POST" || _method == "PUT" || _method == "PATCH" || _method == "DELETE")
            {
                scope = "write";
            }
            else 
            {
                throw new ArgumentException("Invalid method", _method);
            }
            var options = new RestClientOptions(_baseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(_clientUsername, _clientPassword),
            };
            using var client = new RestClient(options);

            var request = new RestRequest("oauth/token").AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", scope);
            var response = await client.PostAsync<TokenResponse>(request);
            return $"{response!.TokenType} {response!.AccessToken}";
        }


    }

    public class BasicAuthenticatorForReadScope : AuthenticatorBase
    {
        private static BasicAuthenticatorForReadScope _instance;
        private static readonly object _lock = new object();

        readonly string _baseUrl;
        readonly string _clientUsername;
        readonly string _clientPassword;
        readonly string _method;

        private BasicAuthenticatorForReadScope(string baseUrl, string clientUsername, string clientPassword, string method) : base("")
        {
            _baseUrl = baseUrl;
            _clientUsername = clientUsername;
            _clientPassword = clientPassword;
            _method = method;
        }

        public static BasicAuthenticatorForReadScope GetInstance(string baseUrl, string clientUsername, string clientPassword, string method)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new BasicAuthenticatorForReadScope(baseUrl, clientUsername, clientPassword, method);
                    }
                }
            }
            return _instance;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            return new HeaderParameter(KnownHeaders.Authorization, Token);
        }

        public async Task<string> GetToken()
        {
            var scope = "";
            if (_method == "GET")
            {
                scope = "read";
            }
            else
            {
                throw new ArgumentException("Invalid method", nameof(_method));
            }
            var options = new RestClientOptions(_baseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(_clientUsername, _clientPassword),
            };
            using var client = new RestClient(options);

            var request = new RestRequest("oauth/token").AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", scope);
            var response = await client.PostAsync<TokenResponse>(request);
            return $"{response!.TokenType} {response!.AccessToken}";
        }


    }

}

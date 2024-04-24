
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIAutomation
{
    public enum Scope
    {
        read,
        write
    }

    public class BasicAuthenticator
    {
        readonly string _baseUrl;
        readonly string _clientUsername;
        readonly string _clientPassword;
        readonly Scope _scope;

        public BasicAuthenticator(string baseUrl, string clientUsername, string clientPassword, Scope scope)
        {
            _baseUrl = baseUrl;
            _clientUsername = clientUsername;
            _clientPassword = clientPassword;
            _scope = scope;
        }

        public async Task<string> GetToken()
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);

            var authenticationHeaderValue = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{_clientUsername}:{_clientPassword}")
            );

            var url = $"oauth/token?grant_type=client_credentials&scope={_scope}";

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authenticationHeaderValue);

            var response = await client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            return $"{tokenResponse.TokenType} {tokenResponse.AccessToken}";
        }
    }
}

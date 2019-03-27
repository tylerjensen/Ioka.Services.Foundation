using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ioka.Services.Client
{
    public enum ClientProtol
    {
        Json,
        Protobuf
    }

    public class Client
    {
        protected const string DefaultSecret = "LemonDropsAndTulipsGrowHereNow";
        protected const string ProtobufMediaType = "application/x-protobuf";
        protected const string JsonMediaType = "application/json";

        protected readonly string _baseUrl;
        protected readonly Func<HttpClient> _httpClientFactory;
        protected readonly string _identityServerUrl;
        protected readonly string _username;
        protected readonly string _password;
        protected readonly string _secret;
        protected readonly string _clientId;
        protected readonly string _scope;

        protected TokenResponse _tokenResponse = null;
        protected DateTime _tokenExpiresTime = DateTime.UtcNow.AddDays(-1);
        protected Exception _loginException = null;
        protected Task _backgroundLogin = null;
        protected HttpClient _httpClient = null;

        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 30);
        public TokenResponse TokenResponse { get { return _tokenResponse; } }
        public Exception LoginException { get { return _loginException; } }

        /// <summary>
        /// Create a new client proxy for accessing Ioka Foundation based microservices.
        /// </summary>
        /// <param name="baseUrl">The base URL for all calls made by this client. Example, https://my.server.com/ </param>
        /// <param name="httpClientFactory">The function used to get an HttpClient object on each call to avoid creating a new client on each call.</param>
        /// <param name="identityServerUrl">The identity server URL. Leave null for strictly anonymous user access. Be sure to specify port using :{port} if required.</param>
        /// <param name="username">username</param>
        /// <param name="password">password</param>
        /// <param name="clientId">Client specification identifier.</param>
        /// <param name="secret">Client secret. Leave null for common shared secret.</param>
        /// <param name="scope">Scopes are space delimited. Leave null for default "openid offline_access".</param>
        public Client(string baseUrl, Func<HttpClient> httpClientFactory, string identityServerUrl = null, string username = null, string password = null,
            string clientId = null, string secret = null, string scope = "openid offline_access")
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));
            _baseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _identityServerUrl = identityServerUrl?.ToLower().TrimEnd('/');
            _username = username;
            _password = password;
            _secret = secret ?? DefaultSecret;
            _clientId = clientId;
            _scope = scope;
        }

        private async Task CheckLogin()
        {
            if (null != _loginException) throw _loginException; //prior login exception
            //if logging in on background, then just wait for that login
            if (null != _backgroundLogin && _backgroundLogin.Status == TaskStatus.Running)
            {
                await _backgroundLogin;
                _backgroundLogin.Dispose();
                _backgroundLogin = null;
            }
            else
            {
                //if not expired but almost expired, then login using background
                if (null != _tokenResponse
                    && DateTime.UtcNow < _tokenExpiresTime
                    && (_tokenExpiresTime - DateTime.UtcNow).TotalSeconds < 5.0)
                {
                    _backgroundLogin = Login();
                }
                else
                {
                    await Login();
                }
            }
            if (null != _loginException) throw _loginException;
        }

        private async Task Login()
        {
            _loginException = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password))
                {
                    var httpClient = GetHttpClient();
                    var disco = await httpClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest { Address = _identityServerUrl });
                    _tokenResponse = await httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = _clientId,
                        ClientSecret  = _secret,
                        UserName = _username,
                        Password = _password,
                        Scope = _scope
                    });
                    if (_tokenResponse.IsError)
                    {
                        _loginException = new UnauthorizedAccessException($"{_tokenResponse.Error} : {_tokenResponse.ErrorDescription}");
                        _tokenResponse = null;
                    }
                    else
                    {
                        _tokenExpiresTime = DateTime.UtcNow.AddSeconds(_tokenResponse.ExpiresIn);
                    }
                }
            }
            catch (Exception ex)
            {
                _loginException = ex;
            }
        }

        private async Task AddAuthorizationBearerToken(HttpRequestMessage request)
        {
            if (string.IsNullOrWhiteSpace(_identityServerUrl)) return; //no login, all anonymous
            await CheckLogin();
            request.SetBearerToken(_tokenResponse.AccessToken);
        }

        private HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                _httpClient = _httpClientFactory();
                _httpClient.Timeout = this.Timeout;
            }
            return _httpClient;
        }


        /// <summary>
        /// Execute a GET on a specified URL and get a strongly typed response using the specified client protocol.
        /// </summary>
        /// <typeparam name="ResponseType">The type returned by the endpoint.</typeparam>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <returns></returns>
        public async Task<ResponseType> GetAsync<ResponseType>(string query, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + query))
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                        .Accept
                        .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                if (clientProtol == ClientProtol.Protobuf)
                {
                    var data = await response.Content.ReadAsStreamAsync();
                    var protoResult = ProtoBuf.Serializer.Deserialize<ResponseType>(data);
                    return protoResult;
                }
                else
                {
                    var rt = typeof(ResponseType);
                    var text = await response.Content.ReadAsStringAsync();
                    var jsonResult = JsonConvert.DeserializeObject<ResponseType>(text);
                    return jsonResult;
                };
            }
        }

        /// <summary>
        /// Execute a POST on specified URL with data of RequestType to get a strongly typed response using the specified client protocol.
        /// </summary>
        /// <typeparam name="RequestType">The type of the data being sent via the request to the endpoint.</typeparam>
        /// <typeparam name="ResponseType">The type returned by the endpoint.</typeparam>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <param name="data">The data being sent via POST to the endpoint.</param>
        /// <returns></returns>
        public async Task<ResponseType> PostAsync<RequestType, ResponseType>(string query, RequestType data, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + query))
            using (var ms = new MemoryStream())
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                ProtoBuf.Serializer.Serialize<RequestType>(ms, data);
                request.Content = new ByteArrayContent(ms.ToArray());
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = clientProtol == ClientProtol.Protobuf
                    ? ProtoBuf.Serializer.Deserialize<ResponseType>(await response.Content.ReadAsStreamAsync())
                    : JsonConvert.DeserializeObject<ResponseType>(await response.Content.ReadAsStringAsync());
                return result;
            }
        }

        /// <summary>
        /// Execute a POST on specified URL with data of RequestType to get a strongly typed response using the specified client protocol.
        /// No response object. This endpoint has a controller that returns void or Task.
        /// </summary>
        /// <typeparam name="RequestType">The type of the data being sent via the request to the endpoint.</typeparam>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <param name="data">The data being sent via POST to the endpoint.</param>
        /// <returns></returns>
        public async Task PostAsync<RequestType>(string query, RequestType data, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + query))
            using (var ms = new MemoryStream())
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                ProtoBuf.Serializer.Serialize<RequestType>(ms, data);
                request.Content = new ByteArrayContent(ms.ToArray());
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Execute a PUT on specified URL with data of RequestType to get a strongly typed response using the specified client protocol.
        /// </summary>
        /// <typeparam name="RequestType">The type of the data being sent via the request to the endpoint.</typeparam>
        /// <typeparam name="ResponseType">The type returned by the endpoint.</typeparam>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <param name="data">The data being sent via PUT to the endpoint.</param>
        /// <returns></returns>
        public async Task<ResponseType> PutAsync<RequestType, ResponseType>(string query, RequestType data, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Put, _baseUrl + query))
            using (var ms = new MemoryStream())
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                client.Timeout = this.Timeout;
                ProtoBuf.Serializer.Serialize<RequestType>(ms, data);
                request.Content = new ByteArrayContent(ms.ToArray());
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = clientProtol == ClientProtol.Protobuf
                    ? ProtoBuf.Serializer.Deserialize<ResponseType>(await response.Content.ReadAsStreamAsync())
                    : JsonConvert.DeserializeObject<ResponseType>(await response.Content.ReadAsStringAsync());
                return result;
            }
        }

        /// <summary>
        /// Execute a PUT on specified URL with data of RequestType to get a strongly typed response using the specified client protocol.
        /// No response object. This endpoint has a controller that returns void or Task.
        /// </summary>
        /// <typeparam name="RequestType">The type of the data being sent via the request to the endpoint.</typeparam>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <param name="data">The data being sent via PUT to the endpoint.</param>
        /// <returns></returns>
        public async Task PutAsync<RequestType>(string query, RequestType data, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Put, _baseUrl + query))
            using (var ms = new MemoryStream())
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                ProtoBuf.Serializer.Serialize<RequestType>(ms, data);
                request.Content = new ByteArrayContent(ms.ToArray());
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        }


        /// <summary>
        /// Execute a DELETE on a specified URL and get a strongly typed response using the specified client protocol.
        /// </summary>
        /// <typeparam name="ResponseType">The type returned by the endpoint.</typeparam>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <returns></returns>
        public async Task<ResponseType> DeleteAsync<ResponseType>(string query, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Delete, _baseUrl + query))
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = clientProtol == ClientProtol.Protobuf
                    ? ProtoBuf.Serializer.Deserialize<ResponseType>(await response.Content.ReadAsStreamAsync())
                    : JsonConvert.DeserializeObject<ResponseType>(await response.Content.ReadAsStringAsync());
                return result;
            }
        }

        /// <summary>
        /// Execute a DELETE on a specified URL and get a strongly typed response using the specified client protocol.
        /// No response object. This endpoint has a controller that returns void or Task.
        /// </summary>
        /// <param name="query">The query path appended to the base URL, e.g. "api/tables");</param>
        /// <returns></returns>
        public async Task DeleteAsync(string query, ClientProtol clientProtol = ClientProtol.Protobuf)
        {
            if (query.StartsWith("/")) query = query.TrimStart('/');
            var mediaType = clientProtol == ClientProtol.Protobuf ? ProtobufMediaType : JsonMediaType;
            var client = GetHttpClient();
            using (var request = new HttpRequestMessage(HttpMethod.Delete, _baseUrl + query))
            {
                await AddAuthorizationBearerToken(request);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue(mediaType));
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}

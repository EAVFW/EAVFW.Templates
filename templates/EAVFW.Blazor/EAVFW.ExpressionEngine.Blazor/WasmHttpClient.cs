using System;
using System.Net.Http;
using System.Threading.Tasks;
using EAVFWpressionEngine.Auxiliary;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace EAVFW.ExpressionEngine.Blazor
{
    public class WasmHttpClient : IWasmHttpClient
    {
        private readonly HttpClient _httpClient;

        public WasmHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri("https://localhost:44363/api/");
        }

        public object BaseAddress => _httpClient.BaseAddress;

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return await _httpClient.SendAsync(httpRequestMessage);
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return await _httpClient.SendAsync(request);
        }
    }
}
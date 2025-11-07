using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FacturasSRI.Web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public ApiClient(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task<HttpClient> GetHttpClientAsync()
        {
            string token = null;
            try
            {
                token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            }
            catch (System.InvalidOperationException) 
            { 
            }

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            return _httpClient;
        }
    }
}
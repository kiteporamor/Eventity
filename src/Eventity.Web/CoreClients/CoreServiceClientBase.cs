using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Eventity.Web.CoreClients;

public abstract class CoreServiceClientBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    protected CoreServiceClientBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected HttpClient Client => _httpClient;

    protected static async Task<T?> ReadOrDefaultAsync<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
}

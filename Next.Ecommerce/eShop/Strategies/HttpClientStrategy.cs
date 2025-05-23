namespace eShop.Strategies;
public class HttpClientStrategy : IHttpStrategy
{
    private readonly HttpClient _httpClient;

    public HttpClientStrategy(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task PostAsync<T>(string url, T data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TResult?> PostAsync<TRequest, TResult>(string url, TRequest data)
    {
        var response = await _httpClient.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResult>();
    }

    public async Task DeleteAsync(string url)
    {
        var response = await _httpClient.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }
}

